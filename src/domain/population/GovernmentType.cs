using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Government and regime classifications shared by native populations and colonies.
/// </summary>
public static class GovernmentType
{
    /// <summary>
    /// Regime forms.
    /// </summary>
    public enum Regime
    {
        Tribal,
        Chiefdom,
        CityState,
        Feudal,
        PatrimonialKingdom,
        BureaucraticEmpire,
        AbsoluteMonarchy,
        Constitutional,
        Oligarchic,
        EliteRepublic,
        MassDemocracy,
        OnePartyState,
        MilitaryJunta,
        PersonalistDict,
        FailedState,
        Corporate,
        Theocracy,
        Technocracy,
    }

    /// <summary>
    /// Converts a regime to a display string.
    /// </summary>
    public static string ToStringName(Regime regime)
    {
        return regime switch
        {
            Regime.Tribal => "Tribal",
            Regime.Chiefdom => "Chiefdom",
            Regime.CityState => "City-State",
            Regime.Feudal => "Feudal",
            Regime.PatrimonialKingdom => "Patrimonial Kingdom",
            Regime.BureaucraticEmpire => "Bureaucratic Empire",
            Regime.AbsoluteMonarchy => "Absolute Monarchy",
            Regime.Constitutional => "Constitutional Government",
            Regime.Oligarchic => "Oligarchy",
            Regime.EliteRepublic => "Elite Republic",
            Regime.MassDemocracy => "Mass Democracy",
            Regime.OnePartyState => "One-Party State",
            Regime.MilitaryJunta => "Military Junta",
            Regime.PersonalistDict => "Personalist Dictatorship",
            Regime.FailedState => "Failed State",
            Regime.Corporate => "Corporate Governance",
            Regime.Theocracy => "Theocracy",
            Regime.Technocracy => "Technocracy",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a regime from a string.
    /// </summary>
    public static Regime FromString(string name)
    {
        string normalized = name.ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
        return normalized switch
        {
            "tribal" => Regime.Tribal,
            "chiefdom" => Regime.Chiefdom,
            "city_state" => Regime.CityState,
            "feudal" => Regime.Feudal,
            "patrimonial_kingdom" => Regime.PatrimonialKingdom,
            "bureaucratic_empire" => Regime.BureaucraticEmpire,
            "absolute_monarchy" => Regime.AbsoluteMonarchy,
            "constitutional" => Regime.Constitutional,
            "constitutional_government" => Regime.Constitutional,
            "oligarchic" => Regime.Oligarchic,
            "oligarchy" => Regime.Oligarchic,
            "elite_republic" => Regime.EliteRepublic,
            "mass_democracy" => Regime.MassDemocracy,
            "one_party_state" => Regime.OnePartyState,
            "military_junta" => Regime.MilitaryJunta,
            "personalist_dict" => Regime.PersonalistDict,
            "personalist_dictatorship" => Regime.PersonalistDict,
            "failed_state" => Regime.FailedState,
            "corporate" => Regime.Corporate,
            "corporate_governance" => Regime.Corporate,
            "theocracy" => Regime.Theocracy,
            "technocracy" => Regime.Technocracy,
            _ => Regime.Tribal,
        };
    }

    /// <summary>
    /// Returns typical starting regimes for native populations.
    /// </summary>
    public static Array<Regime> NativeStartingRegimes()
    {
        return new Array<Regime> { Regime.Tribal, Regime.Chiefdom };
    }

    /// <summary>
    /// Returns typical starting regimes for colonies.
    /// </summary>
    public static Array<Regime> ColonyStartingRegimes()
    {
        return new Array<Regime>
        {
            Regime.Corporate,
            Regime.MilitaryJunta,
            Regime.Constitutional,
            Regime.OnePartyState,
            Regime.Technocracy,
        };
    }

    /// <summary>
    /// Returns whether the regime is authoritarian.
    /// </summary>
    public static bool IsAuthoritarian(Regime regime)
    {
        return regime == Regime.AbsoluteMonarchy
            || regime == Regime.OnePartyState
            || regime == Regime.MilitaryJunta
            || regime == Regime.PersonalistDict
            || regime == Regime.Theocracy
            || regime == Regime.Oligarchic;
    }

    /// <summary>
    /// Returns whether the regime is participatory.
    /// </summary>
    public static bool IsParticipatory(Regime regime)
    {
        return regime == Regime.Tribal
            || regime == Regime.CityState
            || regime == Regime.Constitutional
            || regime == Regime.EliteRepublic
            || regime == Regime.MassDemocracy;
    }

    /// <summary>
    /// Returns whether the regime is unstable.
    /// </summary>
    public static bool IsUnstable(Regime regime)
    {
        return regime == Regime.FailedState || regime == Regime.MilitaryJunta;
    }

    /// <summary>
    /// Returns the number of defined regimes.
    /// </summary>
    public static int Count()
    {
        return 18;
    }
}
