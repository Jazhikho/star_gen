using Godot;
using StarGen.Domain.Systems;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Lightweight summary of a generated star system for inspector and cache use.
/// </summary>
public partial class StarSystemPreviewData : RefCounted
{
    /// <summary>
    /// Deterministic star seed used for the preview.
    /// </summary>
    public int StarSeed;

    /// <summary>
    /// World position of the star in parsecs.
    /// </summary>
    public Vector3 WorldPosition = Vector3.Zero;

    /// <summary>
    /// Number of stars in the generated system.
    /// </summary>
    public int StarCount;

    /// <summary>
    /// Spectral class labels for system stars.
    /// </summary>
    public string[] SpectralClasses = global::System.Array.Empty<string>();

    /// <summary>
    /// Effective temperatures for system stars.
    /// </summary>
    public float[] StarTemperatures = global::System.Array.Empty<float>();

    /// <summary>
    /// Number of planets in the system.
    /// </summary>
    public int PlanetCount;

    /// <summary>
    /// Number of moons in the system.
    /// </summary>
    public int MoonCount;

    /// <summary>
    /// Number of asteroid belts in the system.
    /// </summary>
    public int BeltCount;

    /// <summary>
    /// Metallicity relative to solar.
    /// </summary>
    public double Metallicity = 1.0;

    /// <summary>
    /// Total system population.
    /// </summary>
    public int TotalPopulation;

    /// <summary>
    /// Returns whether the previewed system is inhabited.
    /// </summary>
    public bool IsInhabited;

    /// <summary>
    /// Cached full solar system for reuse.
    /// </summary>
    public SolarSystem? System;
}
