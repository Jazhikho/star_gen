using System.Collections.Generic;

namespace StarGen.Domain.Concepts.Religion;

/// <summary>
/// Folded-in religion summary used by the concept atlas.
/// </summary>
public sealed class ReligionConceptSnapshot
{
    /// <summary>
    /// Primary deity framing.
    /// </summary>
    public string Deity { get; set; } = string.Empty;

    /// <summary>
    /// Cosmology summary.
    /// </summary>
    public string Cosmology { get; set; } = string.Empty;

    /// <summary>
    /// Authority summary.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Specialist summary.
    /// </summary>
    public string Specialist { get; set; } = string.Empty;

    /// <summary>
    /// Ritual sample.
    /// </summary>
    public List<string> Rituals { get; set; } = new List<string>();

    /// <summary>
    /// Ethical emphases.
    /// </summary>
    public List<string> Ethics { get; set; } = new List<string>();

    /// <summary>
    /// Religious landscape notes.
    /// </summary>
    public List<string> Landscape { get; set; } = new List<string>();
}
