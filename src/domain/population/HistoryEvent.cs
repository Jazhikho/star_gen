using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Single historical event for a population timeline.
/// </summary>
public partial class HistoryEvent : RefCounted
{
    /// <summary>
    /// Event type categories.
    /// </summary>
    public enum EventType
    {
        Founding,
        NaturalDisaster,
        Plague,
        Famine,
        War,
        CivilWar,
        TechAdvancement,
        Expansion,
        PoliticalChange,
        Migration,
        Collapse,
        GoldenAge,
        CulturalShift,
        Contact,
        Treaty,
        Independence,
        Annexation,
        Discovery,
        Construction,
        Leader,
    }

    /// <summary>
    /// Event type.
    /// </summary>
    public EventType Type = EventType.Founding;

    /// <summary>
    /// Event year.
    /// </summary>
    public int Year;

    /// <summary>
    /// Event title.
    /// </summary>
    public string Title = string.Empty;

    /// <summary>
    /// Event description.
    /// </summary>
    public string Description = string.Empty;

    /// <summary>
    /// Event magnitude in the range [-1, 1].
    /// </summary>
    public double Magnitude;

    /// <summary>
    /// Population delta caused by the event.
    /// </summary>
    public int PopulationDelta;

    /// <summary>
    /// Related population identifier.
    /// </summary>
    public string RelatedPopulationId = string.Empty;

    /// <summary>
    /// Event-specific metadata.
    /// </summary>
    public Dictionary Metadata = new();

    /// <summary>
    /// Creates a new history event.
    /// </summary>
    public HistoryEvent(
        EventType type = EventType.Founding,
        int year = 0,
        string title = "",
        string description = "",
        double magnitude = 0.0)
    {
        Type = type;
        Year = year;
        Title = title;
        Description = description;
        Magnitude = System.Math.Clamp(magnitude, -1.0, 1.0);
    }

    /// <summary>
    /// Returns whether the event is harmful.
    /// </summary>
    public bool IsHarmful()
    {
        return Magnitude < 0.0;
    }

    /// <summary>
    /// Returns whether the event is beneficial.
    /// </summary>
    public bool IsBeneficial()
    {
        return Magnitude > 0.0;
    }

    /// <summary>
    /// Returns whether an event type is typically harmful.
    /// </summary>
    public static bool IsTypicallyHarmful(EventType type)
    {
        return type == EventType.NaturalDisaster
            || type == EventType.Plague
            || type == EventType.Famine
            || type == EventType.War
            || type == EventType.CivilWar
            || type == EventType.Collapse
            || type == EventType.Annexation;
    }

    /// <summary>
    /// Returns whether an event type is typically beneficial.
    /// </summary>
    public static bool IsTypicallyBeneficial(EventType type)
    {
        return type == EventType.TechAdvancement
            || type == EventType.Expansion
            || type == EventType.GoldenAge
            || type == EventType.Treaty
            || type == EventType.Independence
            || type == EventType.Discovery
            || type == EventType.Construction;
    }

    /// <summary>
    /// Converts this event to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["event_type"] = (int)Type,
            ["year"] = Year,
            ["title"] = Title,
            ["description"] = Description,
            ["magnitude"] = Magnitude,
            ["population_delta"] = PopulationDelta,
            ["related_population_id"] = RelatedPopulationId,
            ["metadata"] = CloneDictionary(Metadata),
        };
    }

    /// <summary>
    /// Creates a history event from a dictionary payload.
    /// </summary>
    public static HistoryEvent FromDictionary(Dictionary data)
    {
        EventType type = (EventType)GetInt(data, "event_type", (int)EventType.Founding);
        HistoryEvent historyEvent = new(
            type,
            GetInt(data, "year", 0),
            GetString(data, "title", string.Empty),
            GetString(data, "description", string.Empty),
            GetDouble(data, "magnitude", 0.0));

        historyEvent.PopulationDelta = GetInt(data, "population_delta", 0);
        historyEvent.RelatedPopulationId = GetString(data, "related_population_id", string.Empty);
        historyEvent.Metadata = GetDictionary(data, "metadata");
        return historyEvent;
    }

    /// <summary>
    /// Clones a metadata dictionary.
    /// </summary>
    private static Dictionary CloneDictionary(Dictionary source)
    {
        Dictionary clone = new();
        foreach (Variant key in source.Keys)
        {
            clone[key] = source[key];
        }

        return clone;
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
    /// Reads and clones a nested dictionary value.
    /// </summary>
    private static Dictionary GetDictionary(Dictionary data, string key)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Dictionary
            ? CloneDictionary((Dictionary)data[key])
            : new Dictionary();
    }
}
