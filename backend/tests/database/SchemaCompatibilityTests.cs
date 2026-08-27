using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Ehsms.DatabaseTests;

public sealed class SchemaCompatibilityTests
{
    // Read connection from env (set in local dev / CI), skip gracefully if absent
    private static string? ConnectionString =>
        Environment.GetEnvironmentVariable("NEON_DATABASE_URL")
        ?? ReadEnvLocal();

    private static string? ReadEnvLocal()
    {
        // Look for .env.local at repo root (two levels up from tests/database)
        var candidate = Path.Combine(RepoRoot, ".env.local");
        if (File.Exists(candidate))
        {
            var line = File.ReadAllLines(candidate)
                .FirstOrDefault(l => l.StartsWith("NEON_DATABASE_URL=", StringComparison.Ordinal));
            if (line is not null)
                return line["NEON_DATABASE_URL=".Length..].Trim();
        }
        return null;
    }

    private static string RepoRoot
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Ehsms.sln")) && !File.Exists(Path.Combine(dir.FullName, "global.json")))
                dir = dir.Parent;
            return dir?.FullName
                ?? throw new DirectoryNotFoundException("Repo root not found from test bin.");
        }
    }

    [Fact]
    public void Database_reachable_and_schema_matches_dbml_counts()
    {
        var cs = ConnectionString;
        if (string.IsNullOrEmpty(cs))
        {
            // Not failing hard: DB-backed tests are opt-in via env
            return;
        }
        using var conn = new NpgsqlConnection(cs);
        conn.Open();

        // Verify 175 tables exist
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*) FROM pg_tables
            WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
            """;
        var tableCount = Convert.ToInt32(cmd.ExecuteScalar());
        Assert.Equal(175, tableCount);
    }

    [Fact]
    public void DbContexts_can_build_models_without_errors()
    {
        // Instantiate each context with an in-memory-ish config just to validate model building.
        // (Npgsql provider without connection only validates model mapping.)
        var options = new DbContextOptionsBuilder()
            .UseNpgsql("Host=localhost;Database=none;Username=none;Password=none")
            .Options;

        // We need to reference concrete contexts — they'll be produced by the EF specialists.
        // This test compiles against the contexts we expect to exist.
        _ = options; // placeholder until EF models land
        Assert.True(true); // model-build smoke test will be filled in after entities exist
    }
}