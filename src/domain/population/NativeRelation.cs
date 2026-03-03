using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Relationship state between a colony and a native population.
/// </summary>
public partial class NativeRelation : RefCounted
{
    /// <summary>
    /// Relationship status categories.
    /// </summary>
    public enum Status
    {
        Unknown,
        FirstContact,
        Peaceful,
        Trading,
        Tense,
        Hostile,
        Subjugated,
        Integrated,
        Extinct,
    }

    /// <summary>
    /// Native population identifier.
    /// </summary>
    public string NativePopulationId = string.Empty;

    /// <summary>
    /// Current relationship status.
    /// </summary>
    public Status CurrentStatus = Status.Unknown;

    /// <summary>
    /// Year of first contact.
    /// </summary>
    public int FirstContactYear;

    /// <summary>
    /// Relation score in the range [-100, 100].
    /// </summary>
    public int RelationScore;

    /// <summary>
    /// Whether a treaty exists.
    /// </summary>
    public bool HasTreaty;

    /// <summary>
    /// Year of the current treaty.
    /// </summary>
    public int TreatyYear;

    /// <summary>
    /// Trade volume in the range [0, 1].
    /// </summary>
    public double TradeLevel;

    /// <summary>
    /// Cultural exchange in the range [0, 1].
    /// </summary>
    public double CulturalExchange;

    /// <summary>
    /// Conflict intensity in the range [0, 1].
    /// </summary>
    public double ConflictIntensity;

    /// <summary>
    /// Fraction of territory taken in the range [0, 1].
    /// </summary>
    public double TerritoryTaken;

    /// <summary>
    /// Relationship events.
    /// </summary>
    public Array<string> RelationshipEvents = new();

    /// <summary>
    /// Creates a first-contact relation.
    /// </summary>
    public static NativeRelation CreateFirstContact(string nativeId, int year, int initialDisposition = 0)
    {
        NativeRelation relation = new();
        relation.NativePopulationId = nativeId;
        relation.CurrentStatus = Status.FirstContact;
        relation.FirstContactYear = year;
        relation.RelationScore = System.Math.Clamp(initialDisposition, -100, 100);
        relation.RelationshipEvents.Add($"First contact in year {year}");
        return relation;
    }

    /// <summary>
    /// Updates the current status from the current scores and modifiers.
    /// </summary>
    public void UpdateStatus()
    {
        if (CurrentStatus == Status.Extinct || CurrentStatus == Status.Unknown)
        {
            return;
        }

        if (TerritoryTaken > 0.8)
        {
            CurrentStatus = Status.Subjugated;
        }
        else if (CulturalExchange > 0.7 && RelationScore > 50)
        {
            CurrentStatus = Status.Integrated;
        }
        else if (ConflictIntensity > 0.5)
        {
            CurrentStatus = Status.Hostile;
        }
        else if (RelationScore < -50)
        {
            CurrentStatus = Status.Tense;
        }
        else if (TradeLevel > 0.3 && RelationScore > 20)
        {
            CurrentStatus = Status.Trading;
        }
        else if (RelationScore > 0)
        {
            CurrentStatus = Status.Peaceful;
        }
        else if (RelationScore < -20)
        {
            CurrentStatus = Status.Tense;
        }
        else if (FirstContactYear > 0)
        {
            CurrentStatus = Status.Peaceful;
        }
    }

    /// <summary>
    /// Records extinction of the related native population.
    /// </summary>
    public void RecordExtinction(int year, string cause)
    {
        CurrentStatus = Status.Extinct;
        RelationshipEvents.Add($"Native population extinct in year {year}: {cause}");
    }

    /// <summary>
    /// Records a treaty event.
    /// </summary>
    public void RecordTreaty(int year, string description)
    {
        HasTreaty = true;
        TreatyYear = year;
        RelationScore = System.Math.Min(RelationScore + 20, 100);
        RelationshipEvents.Add($"Treaty signed in year {year}: {description}");
        UpdateStatus();
    }

    /// <summary>
    /// Records a conflict event.
    /// </summary>
    public void RecordConflict(int year, string description, double intensity)
    {
        ConflictIntensity = System.Math.Max(ConflictIntensity, intensity);
        RelationScore = System.Math.Max(RelationScore - (int)System.Math.Round(intensity * 30.0), -100);
        HasTreaty = false;
        RelationshipEvents.Add($"Conflict in year {year}: {description}");
        UpdateStatus();
    }

    /// <summary>
    /// Returns whether the relationship is positive.
    /// </summary>
    public bool IsPositive()
    {
        return RelationScore > 0;
    }

    /// <summary>
    /// Returns whether the relationship is hostile.
    /// </summary>
    public bool IsHostile()
    {
        return CurrentStatus == Status.Hostile || (CurrentStatus == Status.Tense && RelationScore < -30);
    }

    /// <summary>
    /// Converts this relation to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<string> events = new();
        foreach (string relationshipEvent in RelationshipEvents)
        {
            events.Add(relationshipEvent);
        }

        return new Dictionary
        {
            ["native_population_id"] = NativePopulationId,
            ["status"] = (int)CurrentStatus,
            ["first_contact_year"] = FirstContactYear,
            ["relation_score"] = RelationScore,
            ["has_treaty"] = HasTreaty,
            ["treaty_year"] = TreatyYear,
            ["trade_level"] = TradeLevel,
            ["cultural_exchange"] = CulturalExchange,
            ["conflict_intensity"] = ConflictIntensity,
            ["territory_taken"] = TerritoryTaken,
            ["relationship_events"] = events,
        };
    }

    /// <summary>
    /// Creates a native relation from a dictionary payload.
    /// </summary>
    public static NativeRelation FromDictionary(Dictionary data)
    {
        NativeRelation relation = new()
        {
            NativePopulationId = GetString(data, "native_population_id", string.Empty),
            CurrentStatus = (Status)GetInt(data, "status", 0),
            FirstContactYear = GetInt(data, "first_contact_year", 0),
            RelationScore = System.Math.Clamp(GetInt(data, "relation_score", 0), -100, 100),
            HasTreaty = GetBool(data, "has_treaty", false),
            TreatyYear = GetInt(data, "treaty_year", 0),
            TradeLevel = Clamp01(GetDouble(data, "trade_level", 0.0)),
            CulturalExchange = Clamp01(GetDouble(data, "cultural_exchange", 0.0)),
            ConflictIntensity = Clamp01(GetDouble(data, "conflict_intensity", 0.0)),
            TerritoryTaken = Clamp01(GetDouble(data, "territory_taken", 0.0)),
        };

        if (data.ContainsKey("relationship_events") && data["relationship_events"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["relationship_events"])
            {
                if (value.VariantType == Variant.Type.String)
                {
                    relation.RelationshipEvents.Add((string)value);
                }
            }
        }

        return relation;
    }

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            Variant.Type.String => int.TryParse((string)value, out int parsed) ? parsed : fallback,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a floating-point value from a dictionary.
    /// </summary>
    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            Variant.Type.String => double.TryParse((string)value, out double parsed) ? parsed : fallback,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    /// <summary>
    /// Reads a boolean value from a dictionary.
    /// </summary>
    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool ? (bool)data[key] : fallback;
    }

    /// <summary>
    /// Clamps a value to the unit interval.
    /// </summary>
    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }
}
