using System;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Flags controlling which station design sections are auto-populated
/// from presets versus manually overridden.
/// </summary>
[Flags]
public enum AutoPopulateFlags
{
    None = 0,
    Engineering = 1 << 0,
    Command = 1 << 1,
    Defenses = 1 << 2,
    Docking = 1 << 3,
    Quarters = 1 << 4,
    Facilities = 1 << 5,
    AllAuto = Engineering | Command | Defenses | Docking | Quarters | Facilities,
}
