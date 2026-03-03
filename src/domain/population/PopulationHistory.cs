using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Ordered timeline of historical events for a population.
/// </summary>
public partial class PopulationHistory : RefCounted
{
    private readonly Array<HistoryEvent> _events = new();
    private bool _needsSort;

    /// <summary>
    /// Adds an event.
    /// </summary>
    public void AddEvent(HistoryEvent historyEvent)
    {
        _events.Add(historyEvent);
        _needsSort = true;
    }

    /// <summary>
    /// Creates and adds a new event.
    /// </summary>
    public HistoryEvent AddNewEvent(
        HistoryEvent.EventType type,
        int year,
        string title,
        string description = "",
        double magnitude = 0.0)
    {
        HistoryEvent historyEvent = new(type, year, title, description, magnitude);
        AddEvent(historyEvent);
        return historyEvent;
    }

    /// <summary>
    /// Returns all events sorted by year.
    /// </summary>
    public Array<HistoryEvent> GetAllEvents()
    {
        EnsureSorted();
        Array<HistoryEvent> copy = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            copy.Add(historyEvent);
        }

        return copy;
    }

    /// <summary>
    /// Converts this history to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        EnsureSorted();
        Array<Dictionary> events = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            events.Add(historyEvent.ToDictionary());
        }

        return new Dictionary
        {
            ["events"] = events,
        };
    }

    /// <summary>
    /// Creates a history from a dictionary payload.
    /// </summary>
    public static PopulationHistory FromDictionary(Dictionary data)
    {
        PopulationHistory history = new();
        if (!data.ContainsKey("events") || data["events"].VariantType != Variant.Type.Array)
        {
            return history;
        }

        foreach (Variant value in (Array)data["events"])
        {
            if (value.VariantType == Variant.Type.Dictionary)
            {
                history.AddEvent(HistoryEvent.FromDictionary((Dictionary)value));
            }
        }

        return history;
    }

    /// <summary>
    /// Sorts events when pending mutations changed the order.
    /// </summary>
    private void EnsureSorted()
    {
        if (!_needsSort)
        {
            return;
        }

        List<HistoryEvent> sorted = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            sorted.Add(historyEvent);
        }

        sorted.Sort((left, right) => left.Year.CompareTo(right.Year));
        _events.Clear();
        foreach (HistoryEvent historyEvent in sorted)
        {
            _events.Add(historyEvent);
        }

        _needsSort = false;
    }
}
