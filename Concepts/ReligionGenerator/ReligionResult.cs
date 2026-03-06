using System.Collections.Generic;

namespace StarGen.Concepts.ReligionGenerator
{
    /// <summary>
    /// Single option chosen from a weighted set (e.g. deity type, cosmology).
    /// </summary>
    public sealed class ReligionOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
    }

    /// <summary>
    /// Specialist type with access note.
    /// </summary>
    public sealed class SpecialistOption
    {
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string Access { get; set; } = string.Empty;
    }

    /// <summary>
    /// Minority/rival tradition or non-belief form.
    /// </summary>
    public sealed class LandscapeEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
    }

    /// <summary>
    /// Religious landscape: hegemony, rivals, non-belief, dynamics.
    /// </summary>
    public sealed class ReligionLandscape
    {
        public int HegemonyPct { get; set; }
        public string HegemonyDesc { get; set; } = string.Empty;
        public List<LandscapeEntry> Rivals { get; set; } = new List<LandscapeEntry>();
        public int NonBeliefPct { get; set; }
        public List<LandscapeEntry> NonBeliefForms { get; set; } = new List<LandscapeEntry>();
        public List<LandscapeEntry> Dynamics { get; set; } = new List<LandscapeEntry>();
    }

    /// <summary>
    /// Full generated religion output.
    /// </summary>
    public sealed class ReligionResult
    {
        public ReligionOption Deity { get; set; } = new ReligionOption();
        public ReligionOption Cosmology { get; set; } = new ReligionOption();
        public ReligionOption Afterlife { get; set; } = new ReligionOption();
        public SpecialistOption Specialist { get; set; } = new SpecialistOption();
        public ReligionOption GenderRole { get; set; } = new ReligionOption();
        public ReligionOption Misfortune { get; set; } = new ReligionOption();
        public ReligionOption Authority { get; set; } = new ReligionOption();
        public List<string> Rituals { get; set; } = new List<string>();
        public List<string> SacredTimes { get; set; } = new List<string>();
        public List<string> SacredSpaces { get; set; } = new List<string>();
        public List<string> MaterialCulture { get; set; } = new List<string>();
        public List<string> Ethics { get; set; } = new List<string>();
        public List<string> Taboos { get; set; } = new List<string>();
        public List<string> Unique { get; set; } = new List<string>();
        public List<string> SyncNotes { get; set; } = new List<string>();
        public ReligionLandscape Landscape { get; set; } = new ReligionLandscape();
    }
}
