using System.Text.RegularExpressions;
using Ehsms.Modules.Identity.Infrastructure.Persistence;
using Ehsms.Modules.Organisation.Infrastructure.Persistence;
using Ehsms.Modules.Platform.Infrastructure.Persistence;
using Ehsms.Modules.Saas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Ehsms.DatabaseTests;

public sealed class SchemaCompatibilityTests
{
    [Fact]
    public void All_module_models_use_dbml_tables_schemas_and_columns()
    {
        var expected = ReadDbmlTables();
        using var platform = new PlatformDbContext(Options<PlatformDbContext>(), new TestPlatformSchema());
        using var saas = new SaasDbContext(Options<SaasDbContext>());
        using var organisation = new OrganisationDbContext(Options<OrganisationDbContext>());
        using var identity = new EhsmsIdentityDbContext(Options<EhsmsIdentityDbContext>(), new TestIdentitySchema());

        AssertModelMatches(platform, expected);
        AssertModelMatches(saas, expected);
        AssertModelMatches(organisation, expected);
        AssertModelMatches(identity, expected);
    }

    private static DbContextOptions<T> Options<T>() where T : DbContext
        => new DbContextOptionsBuilder<T>()
            .UseNpgsql("Host=localhost;Database=metadata_only;Username=none;Password=none")
            .Options;

    private static void AssertModelMatches(DbContext context, IReadOnlyDictionary<string, HashSet<string>> expected)
    {
        foreach (var entity in context.Model.GetEntityTypes().Where(e => e.GetTableName() is not null))
        {
            var key = $"{entity.GetSchema()}.{entity.GetTableName()}";
            Assert.True(expected.ContainsKey(key), $"EF entity {entity.Name} maps to unexpected table {key}.");

            var actualColumns = entity.GetProperties()
                .Select(p => p.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema())))
                .Where(name => name is not null)
                .Select(name => name!)
                .ToHashSet(StringComparer.Ordinal);
            Assert.True(expected[key].SetEquals(actualColumns),
                $"Columns for {key} differ. Expected: {string.Join(",", expected[key])}; actual: {string.Join(",", actualColumns)}");
        }
    }

    private static IReadOnlyDictionary<string, HashSet<string>> ReadDbmlTables()
    {
        var path = Path.Combine(Directory.GetParent(RepoRoot())!.FullName, "database", "ehsms-erd.dbml");
        var text = File.ReadAllText(path);
        return Regex.Matches(text, @"Table\s+(?<table>[\w]+\.[\w]+)\s*\{(?<body>.*?)\n\}", RegexOptions.Singleline)
            .ToDictionary(
                m => m.Groups["table"].Value,
                m => Regex.Matches(m.Groups["body"].Value, @"^\s+(?<column>[a-z][a-z0-9_]*)\s", RegexOptions.Multiline)
                    .Select(c => c.Groups["column"].Value).ToHashSet(StringComparer.Ordinal),
                StringComparer.Ordinal);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Ehsms.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }

    private sealed class TestPlatformSchema : IPlatformDbSchema
    {
        public string Schema => "platform";
    }

    private sealed class TestIdentitySchema : IDbContextSchema
    {
        public string Schema => "iam";
    }
}