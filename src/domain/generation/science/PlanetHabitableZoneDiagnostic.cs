using Godot.Collections;

namespace StarGen.Domain.Generation.Science;

/// <summary>
/// Diagnostic-only planet-mass-corrected habitable-zone record.
/// </summary>
public sealed class PlanetHabitableZoneDiagnostic
{
    /// <summary>
    /// Engine that produced this diagnostic.
    /// </summary>
    public string EngineId { get; set; } = "kopparapu_2014_mass_corrected_hz_diagnostic";

    /// <summary>
    /// Semicolon-delimited source IDs used by this diagnostic.
    /// </summary>
    public string SourceIds { get; set; } = "Kopparapu2013;Kopparapu2014";

    /// <summary>
    /// Planet mass used by the diagnostic, in Earth masses.
    /// </summary>
    public double PlanetMassEarth { get; set; }

    /// <summary>
    /// Inner mass-corrected HZ edge in AU.
    /// </summary>
    public double InnerAu { get; set; }

    /// <summary>
    /// Outer mass-corrected HZ edge in AU.
    /// </summary>
    public double OuterAu { get; set; }

    /// <summary>
    /// Orbit distance used for alignment, in AU.
    /// </summary>
    public double OrbitAu { get; set; }

    /// <summary>
    /// Alignment score in the range [0, 1].
    /// </summary>
    public double Alignment { get; set; }

    /// <summary>
    /// Inner-edge correction applied to the 1 Earth-mass Kopparapu band.
    /// </summary>
    public double InnerFluxCorrectionScalar { get; set; }

    /// <summary>
    /// Model status note for audit and UI readouts.
    /// </summary>
    public string Status { get; set; } = "diagnostic_only_star_gen_mass_correction_proxy";

    /// <summary>
    /// Converts the diagnostic to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["engine_id"] = EngineId,
            ["source_ids"] = SourceIds,
            ["planet_mass_earth"] = PlanetMassEarth,
            ["inner_au"] = InnerAu,
            ["outer_au"] = OuterAu,
            ["orbit_au"] = OrbitAu,
            ["alignment"] = Alignment,
            ["inner_flux_correction_scalar"] = InnerFluxCorrectionScalar,
            ["status"] = Status,
        };
    }
}
