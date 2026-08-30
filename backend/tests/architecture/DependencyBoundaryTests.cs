using System.Xml.Linq;
using Xunit;

namespace Ehsms.ArchitectureTests;

public sealed class DependencyBoundaryTests
{
    private static readonly string BackendRoot = FindBackendRoot();
    private static readonly string[] ProjectFiles = Directory.GetFiles(
        Path.Combine(BackendRoot, "src"), "*.csproj", SearchOption.AllDirectories);

    [Fact]
    public void Domain_projects_do_not_depend_on_application_infrastructure_or_api() =>
        AssertNoForbiddenReferences(".Domain", ".Application", ".Infrastructure", "Ehsms.Api");

    [Fact]
    public void Application_projects_do_not_depend_on_infrastructure_or_api() =>
        AssertNoForbiddenReferences(".Application", ".Infrastructure", "Ehsms.Api");

    [Fact]
    public void Contracts_projects_do_not_depend_on_domain_application_infrastructure_or_api() =>
        AssertNoForbiddenReferences(".Contracts", ".Domain", ".Application", ".Infrastructure", "Ehsms.Api");

    [Fact]
    public void Infrastructure_projects_do_not_depend_on_api() =>
        AssertNoForbiddenReferences(".Infrastructure", "Ehsms.Api");

    [Fact]
    public void BuildingBlocks_does_not_depend_on_modules() =>
        AssertNoForbiddenReferences("Ehsms.BuildingBlocks", "Ehsms.Modules.");

    [Fact]
    public void Source_does_not_contain_tenant_filter_bypass()
    {
        var sourceFiles = Directory.GetFiles(Path.Combine(BackendRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(IsNotBuildArtifact);
        var violations = sourceFiles.Where(path => File.ReadAllText(path).Contains("|| true", StringComparison.Ordinal)).ToArray();
        Assert.True(violations.Length == 0, "Tenant filtering must never be bypassed with '|| true':\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Cross_module_references_use_contracts_only()
    {
        var violations = new List<string>();
        foreach (var project in ProjectFiles.Where(IsModuleProject))
        {
            var owner = ModuleName(project);
            foreach (var reference in References(project))
            {
                if (!IsModuleProject(reference) || ModuleName(reference) == owner)
                    continue;
                if (!ProjectName(reference).EndsWith(".Contracts", StringComparison.Ordinal))
                    violations.Add($"{ProjectName(project)} -> {ProjectName(reference)}");
            }
        }
        Assert.True(violations.Count == 0, "Cross-module references must target Contracts only:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Project_reference_graph_has_no_cycles()
    {
        var byName = ProjectFiles.ToDictionary(ProjectName, p => p, StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var inStack = new HashSet<string>(StringComparer.Ordinal);
        var cycles = new List<string>();

        void Visit(string project)
        {
            var name = ProjectName(project);
            if (inStack.Contains(name))
            {
                cycles.Add($"Circular project reference detected involving '{name}'.");
                return;
            }
            if (!visited.Add(name))
                return;

            inStack.Add(name);
            foreach (var reference in References(project))
            {
                if (byName.TryGetValue(ProjectName(reference), out var target))
                    Visit(target);
            }
            inStack.Remove(name);
        }

        foreach (var project in ProjectFiles)
            Visit(project);

        Assert.True(cycles.Count == 0, string.Join("\n", cycles));
    }

    [Fact]
    public void Application_projects_do_not_use_persistence_or_ef_api()
    {
        var applicationSources = Directory.GetFiles(Path.Combine(BackendRoot, "src", "Modules"), "*.cs", SearchOption.AllDirectories)
            .Where(path => path.Replace('\\', '/').Contains("/Application/", StringComparison.OrdinalIgnoreCase))
            .Where(IsNotBuildArtifact);

        var forbiddenTokens = new[] { "DbContext", "DbSet", "SaveChanges", "Npgsql", "SqlConnection" };
        var violations = new List<string>();
        foreach (var file in applicationSources)
        {
            var text = File.ReadAllText(file);
            foreach (var token in forbiddenTokens)
            {
                if (text.Contains(token, StringComparison.Ordinal))
                    violations.Add($"{Path.GetFileName(file)} uses '{token}'");
            }
        }
        Assert.True(violations.Count == 0, "Application layer must stay persistence-agnostic:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Production_code_does_not_write_to_console()
    {
        var sourceFiles = Directory.GetFiles(Path.Combine(BackendRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(IsNotBuildArtifact);
        var violations = sourceFiles
            .Where(path => File.ReadAllText(path).Contains("Console.", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToArray();
        Assert.True(violations.Length == 0, "Production code must log via ILogger, not Console:\n" + string.Join("\n", violations));
    }

    private static bool IsNotBuildArtifact(string path)
    {
        var normalized = path.Replace('\\', '/');
        return !normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            && !normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoForbiddenReferences(string projectMarker, params string[] forbidden)
    {
        var violations = ProjectFiles
            .Where(path => ProjectName(path).Contains(projectMarker, StringComparison.Ordinal))
            .SelectMany(project => References(project).Select(reference => (project, reference)))
            .Where(edge => forbidden.Any(marker => ProjectName(edge.reference).Contains(marker, StringComparison.Ordinal)))
            .Select(edge => $"{ProjectName(edge.project)} -> {ProjectName(edge.reference)}")
            .ToArray();
        Assert.True(violations.Length == 0, "Forbidden references:\n" + string.Join("\n", violations));
    }

    private static IEnumerable<string> References(string project)
    {
        var document = XDocument.Load(project);
        return document.Descendants("ProjectReference")
            .Select(node => node.Attribute("Include")?.Value)
            .Where(value => value is not null)
            .Select(value => Path.GetFullPath(value!, Path.GetDirectoryName(project)!));
    }

    private static bool IsModuleProject(string path) =>
        path.Replace('\\', '/').Contains("/src/Modules/", StringComparison.OrdinalIgnoreCase);

    private static string ModuleName(string path)
    {
        var parts = path.Replace('\\', '/').Split('/');
        return parts[Array.FindIndex(parts, part => part.Equals("Modules", StringComparison.OrdinalIgnoreCase)) + 1];
    }

    private static string ProjectName(string path) => Path.GetFileNameWithoutExtension(path);

    private static string FindBackendRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Ehsms.sln")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Backend root containing Ehsms.sln was not found.");
    }
}