namespace Ehsms.Modules.HealthSafety.Infrastructure.Persistence;

/// <summary>Provides the database schema names used by the HealthSafety module (ppe, health, chemical, environment, sustainability).</summary>
public interface IHealthSafetyDbSchema
{
    /// <summary>The PostgreSQL schema hosting the PPE tables, e.g. <c>ppe</c>.</summary>
    string PpeSchema { get; }

    /// <summary>The PostgreSQL schema hosting the health tables, e.g. <c>health</c>.</summary>
    string HealthSchema { get; }

    /// <summary>The PostgreSQL schema hosting the chemical tables, e.g. <c>chemical</c>.</summary>
    string ChemicalSchema { get; }

    /// <summary>The PostgreSQL schema hosting the environment tables, e.g. <c>environment</c>.</summary>
    string EnvironmentSchema { get; }

    /// <summary>The PostgreSQL schema hosting the sustainability tables, e.g. <c>sustainability</c>.</summary>
    string SustainabilitySchema { get; }
}