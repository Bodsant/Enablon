using System.Text.RegularExpressions;
using Ehsms.Modules.AssetReporting.Infrastructure.Persistence;
using Ehsms.Modules.ComplianceContracts.Infrastructure.Persistence;
using Ehsms.Modules.HealthSafety.Infrastructure.Persistence;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.SafetyRisk.Infrastructure.Persistence;
using Ehsms.Modules.Saas.Infrastructure.Persistence;
using Ehsms.Modules.WorkControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Ehsms.DatabaseTests;

public sealed class SchemaCompatibilityTests
{
    [Fact]
    public void All_module_models_match_dbml_metadata_for_all_tables()
    {
        var expected = ReadDbmlTables();
        Assert.Equal(175, expected.Count);

        using var platform = new PlatformDbContext(Options<PlatformDbContext>(), new TestPlatformSchema());
        using var saas = new SaasDbContext(Options<SaasDbContext>());
        using var organisation = new OrganisationDbContext(Options<OrganisationDbContext>());
        using var identity = new EhsmsIdentityDbContext(Options<EhsmsIdentityDbContext>(), new TestIdentitySchema());
        using var safetyRisk = new SafetyRiskDbContext(Options<SafetyRiskDbContext>());
        using var workControl = new WorkControlDbContext(Options<WorkControlDbContext>());
        using var complianceContracts = new ComplianceContractsDbContext(Options<ComplianceContractsDbContext>());
        using var healthSafety = new HealthSafetyDbContext(Options<HealthSafetyDbContext>(), new TestHealthSafetySchema());
        using var assetReporting = new AssetReportingDbContext(Options<AssetReportingDbContext>(), new TestAssetReportingSchema());

        var contexts = new DbContext[] { platform, saas, organisation, identity, safetyRisk, workControl, complianceContracts, healthSafety, assetReporting };
        var entities = contexts.SelectMany(c => c.Model.GetEntityTypes().Where(e => e.GetTableName() is not null)).ToArray();
        var actual = entities.ToDictionary(e => TableKey(e), StringComparer.Ordinal);

        var unexpected = actual.Keys.Except(expected.Keys, StringComparer.Ordinal).OrderBy(k => k).ToArray();
        Assert.True(unexpected.Length == 0, $"Unexpected EF tables: {string.Join(", ", unexpected)}");
        var missing = expected.Keys.Except(actual.Keys, StringComparer.Ordinal).OrderBy(k => k).ToArray();
        Assert.True(missing.Length == 0, $"Missing DBML tables: {string.Join(", ", missing)}");
        Assert.Equal(175, actual.Count);
        Assert.Equal(175, expected.Count);

        foreach (var pair in expected.OrderBy(p => p.Key, StringComparer.Ordinal))
            AssertEntityMatches(actual[pair.Key], pair.Key, pair.Value);
    }

    private static DbContextOptions<T> Options<T>() where T : DbContext
        => new DbContextOptionsBuilder<T>().UseNpgsql("Host=localhost;Database=metadata_only;Username=none;Password=none").Options;

    private static void AssertEntityMatches(IEntityType entity, string tableKey, DbmlTable expected)
    {
        var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
        var properties = entity.GetProperties()
            .Where(p => !p.IsShadowProperty())
            .Select(p => (Property: p, Column: p.GetColumnName(store) ?? throw new InvalidOperationException($"EF table {tableKey} property {p.Name} has no column mapping.")))
            .ToDictionary(x => x.Column, x => x.Property, StringComparer.Ordinal);
        var expectedColumns = expected.Columns.Keys.ToHashSet(StringComparer.Ordinal);
        var actualColumns = properties.Keys.ToHashSet(StringComparer.Ordinal);
        Assert.True(expectedColumns.SetEquals(actualColumns), $"Columns for {tableKey} differ. Expected: {string.Join(",", expectedColumns.OrderBy(x => x))}; actual: {string.Join(",", actualColumns.OrderBy(x => x))}");

        var primaryKey = entity.FindPrimaryKey() ?? throw new Xunit.Sdk.XunitException($"EF table {tableKey} has no primary key.");
        var actualPk = primaryKey.Properties.Select(p => p.GetColumnName(store) ?? p.Name).ToArray();
        Assert.Equal(expected.PrimaryKey, actualPk);

        foreach (var column in expected.Columns)
        {
            var property = properties[column.Key];
            var detail = $"{tableKey}.{column.Key}";
            if (column.Value.Required)
                Assert.False(property.IsNullable, $"Requiredness for {detail} differs. DBML marks it not null but EF allows null.");
            else
                Assert.True(property.IsNullable, $"Requiredness for {detail} differs. DBML allows null but EF requires a value.");
            if (column.Key == "tenant_id")
                Assert.False(property.IsNullable, $"Tenant column {detail} must be required because DBML marks tenant_id as not null.");
            AssertBasicType(detail, property, column.Value.Type);
            if (column.Value.Length is not null)
                Assert.True(column.Value.Length == property.GetMaxLength(), $"Length for {detail} differs. DBML: {column.Value.Length}; EF: {property.GetMaxLength()}.");
            if (column.Value.Precision is not null)
            {
                Assert.Equal(column.Value.Precision, property.GetPrecision());
                Assert.Equal(column.Value.Scale, property.GetScale());
            }
        }
    }

    private static void AssertBasicType(string column, IProperty property, string dbmlType)
    {
        var expectedClr = dbmlType switch
        {
            "uuid" => typeof(Guid), "boolean" => typeof(bool), "smallint" => typeof(short), "int" => typeof(int),
            "bigint" => typeof(long), "decimal" => typeof(decimal), "date" => typeof(DateOnly),
            "timestamptz" => typeof(DateTimeOffset), "text" or "jsonb" => typeof(string), _ when dbmlType.StartsWith("varchar(", StringComparison.Ordinal) => typeof(string),
            _ => throw new Xunit.Sdk.XunitException($"Unsupported DBML type '{dbmlType}' at {column}.")
        };
        Assert.Equal(expectedClr, Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType);
        var storeType = property.GetColumnType()?.ToLowerInvariant() ?? "";
        var expectedStore = dbmlType.StartsWith("varchar(", StringComparison.Ordinal) ? "character varying" : dbmlType switch
        {
            "uuid" => "uuid", "boolean" => "boolean", "smallint" => "smallint", "int" => "integer", "bigint" => "bigint",
            "decimal" => "numeric", "date" => "date", "timestamptz" => "timestamp with time zone", "text" => "text", "jsonb" => "jsonb", _ => ""
        };
        var compatible = storeType == expectedStore
            || (dbmlType.StartsWith("varchar(", StringComparison.Ordinal) && storeType.StartsWith("character varying(", StringComparison.Ordinal))
            || (dbmlType == "decimal" && storeType.StartsWith("numeric", StringComparison.Ordinal));
        Assert.True(compatible, $"Type for {column} differs. DBML: {dbmlType}; EF: {property.ClrType.Name}/{property.GetColumnType()}");
    }

    private static IReadOnlyDictionary<string, DbmlTable> ReadDbmlTables()
    {
        var path = Path.Combine(Directory.GetParent(RepoRoot())!.FullName, "database", "ehsms-erd.dbml");
        var text = File.ReadAllText(path);
        var matches = Regex.Matches(text, @"(?m)^Table\s+(?<table>[A-Za-z_][\w]*\.[A-Za-z_][\w]*)\s*\{(?<body>.*?)^\}", RegexOptions.Singleline);
        if (matches.Count != 175) throw new InvalidOperationException($"DBML parser expected 175 tables but found {matches.Count}.");
        var result = new Dictionary<string, DbmlTable>(StringComparer.Ordinal);
        foreach (Match match in matches)
        {
            var columns = new Dictionary<string, DbmlColumn>(StringComparer.Ordinal);
            var primaryKey = new List<string>();
            foreach (Match line in Regex.Matches(match.Groups["body"].Value, @"(?m)^\s+(?<name>[a-z][a-z0-9_]*)\s+(?<type>[a-z]+(?:\([0-9]+(?:,[0-9]+)?\))?)(?:\s+\[(?<attrs>[^\]]*)\])?\s*$"))
            {
                var name = line.Groups["name"].Value;
                var type = line.Groups["type"].Value;
                var attrs = line.Groups["attrs"].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var isPrimaryKey = attrs.Contains("pk", StringComparer.Ordinal);
                var required = isPrimaryKey || attrs.Contains("not null", StringComparer.Ordinal);
                var length = type.StartsWith("varchar(", StringComparison.Ordinal) ? int.Parse(type[8..^1]) : (int?)null;
                int? precision = null, scale = null;
                if (type.StartsWith("decimal(", StringComparison.Ordinal))
                {
                    var parts = type[8..^1].Split(','); precision = int.Parse(parts[0]); scale = int.Parse(parts[1]); type = "decimal";
                }
                columns.Add(name, new DbmlColumn(type, required, length, precision, scale));
                if (attrs.Contains("pk", StringComparer.Ordinal)) primaryKey.Add(name);
            }
            if (columns.Count == 0) throw new InvalidOperationException($"DBML table {match.Groups["table"].Value} has no columns.");
            if (primaryKey.Count == 0) throw new InvalidOperationException($"DBML table {match.Groups["table"].Value} has no primary key.");
            result.Add(match.Groups["table"].Value, new DbmlTable(columns, primaryKey.ToArray()));
        }
        return result;
    }

    private static string TableKey(IEntityType entity) => $"{entity.GetSchema()}.{entity.GetTableName()}";
    private static string RepoRoot() { var dir = new DirectoryInfo(AppContext.BaseDirectory); while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Ehsms.sln"))) dir = dir.Parent; return dir?.FullName ?? throw new DirectoryNotFoundException("Repository root not found."); }
    private sealed record DbmlTable(IReadOnlyDictionary<string, DbmlColumn> Columns, IReadOnlyList<string> PrimaryKey);
    private sealed record DbmlColumn(string Type, bool Required, int? Length, int? Precision, int? Scale);
    private sealed class TestPlatformSchema : IPlatformDbSchema { public string Schema => "platform"; }
    private sealed class TestIdentitySchema : IDbContextSchema { public string Schema => "iam"; }
    private sealed class TestHealthSafetySchema : IHealthSafetyDbSchema { public string PpeSchema => "ppe"; public string HealthSchema => "health"; public string ChemicalSchema => "chemical"; public string EnvironmentSchema => "environment"; public string SustainabilitySchema => "sustainability"; }
    private sealed class TestAssetReportingSchema : IAssetReportingDbSchema { public string Schema => "asset"; }
}
