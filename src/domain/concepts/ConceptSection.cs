using System.Collections.Generic;

namespace StarGen.Domain.Concepts;

/// <summary>
/// Named section of concept-atlas content.
/// </summary>
public sealed class ConceptSection
{
    /// <summary>
    /// Section title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Section items.
    /// </summary>
    public List<string> Items { get; set; } = new List<string>();
}
