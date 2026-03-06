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
    /// Returns the number of events.
    /// </summary>
    public int Size()
    {
        return _events.Count;
    }

    /// <summary>
    /// Returns whether history is empty.
    /// </summary>
    public bool IsEmpty()
    {
        return _events.Count == 0;
    }

    /// <summary>
    /// Returns an event at index or null.
    /// </summary>
    public HistoryEvent? GetEvent(int index)
    {
        EnsureSorted();
        if (index < 0 || index >= _events.Count)
        {
            return null;
        }

        return _events[index];
    }

    /// <summary>
    /// Returns first event or null.
    /// </summary>
    public HistoryEvent? GetFirstEvent()
    {
        EnsureSorted();
        if (_events.Count == 0)
        {
            return null;
        }

        return _events[0];
    }

    /// <summary>
    /// Returns last event or null.
    /// </summary>
    public HistoryEvent? GetLastEvent()
    {
        EnsureSorted();
        if (_events.Count == 0)
        {
            return null;
        }

        return _events[_events.Count - 1];
    }

    /// <summary>
    /// Returns founding event or null.
    /// </summary>
    public HistoryEvent? GetFoundingEvent()
    {
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.Type == HistoryEvent.EventType.Founding)
            {
                return historyEvent;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns events matching type.
    /// </summary>
    public Array<HistoryEvent> GetEventsByType(HistoryEvent.EventType type)
    {
        EnsureSorted();
        Array<HistoryEvent> result = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.Type == type)
            {
                result.Add(historyEvent);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns events within inclusive year range.
    /// </summary>
    public Array<HistoryEvent> GetEventsInRange(int startYear, int endYear)
    {
        EnsureSorted();
        Array<HistoryEvent> result = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.Year >= startYear && historyEvent.Year <= endYear)
            {
                result.Add(historyEvent);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns events before the given year.
    /// </summary>
    public Array<HistoryEvent> GetEventsBefore(int year)
    {
        EnsureSorted();
        Array<HistoryEvent> result = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.Year < year)
            {
                result.Add(historyEvent);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns events after the given year.
    /// </summary>
    public Array<HistoryEvent> GetEventsAfter(int year)
    {
        EnsureSorted();
        Array<HistoryEvent> result = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.Year > year)
            {
                result.Add(historyEvent);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns harmful events.
    /// </summary>
    public Array<HistoryEvent> GetHarmfulEvents()
    {
        EnsureSorted();
        Array<HistoryEvent> result = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.IsHarmful())
            {
                result.Add(historyEvent);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns beneficial events.
    /// </summary>
    public Array<HistoryEvent> GetBeneficialEvents()
    {
        EnsureSorted();
        Array<HistoryEvent> result = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.IsBeneficial())
            {
                result.Add(historyEvent);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns events involving a related population id.
    /// </summary>
    public Array<HistoryEvent> GetEventsInvolving(string populationId)
    {
        EnsureSorted();
        Array<HistoryEvent> result = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.RelatedPopulationId == populationId)
            {
                result.Add(historyEvent);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns total population delta.
    /// </summary>
    public int GetTotalPopulationDelta()
    {
        int total = 0;
        foreach (HistoryEvent historyEvent in _events)
        {
            total += historyEvent.PopulationDelta;
        }

        return total;
    }

    /// <summary>
    /// Returns total population delta in inclusive year range.
    /// </summary>
    public int GetPopulationDeltaInRange(int startYear, int endYear)
    {
        int total = 0;
        foreach (HistoryEvent historyEvent in _events)
        {
            if (historyEvent.Year >= startYear && historyEvent.Year <= endYear)
            {
                total += historyEvent.PopulationDelta;
            }
        }

        return total;
    }

    /// <summary>
    /// Returns start/end years as dictionary, or empty dictionary when no events exist.
    /// </summary>
    public Dictionary GetYearSpan()
    {
        if (_events.Count == 0)
        {
            return new Dictionary();
        }

        EnsureSorted();
        return new Dictionary
        {
            ["start"] = _events[0].Year,
            ["end"] = _events[_events.Count - 1].Year,
        };
    }

    /// <summary>
    /// Returns duration in years between first and last events.
    /// </summary>
    public int GetDurationYears()
    {
        if (_events.Count < 2)
        {
            return 0;
        }

        EnsureSorted();
        return _events[_events.Count - 1].Year - _events[0].Year;
    }

    /// <summary>
    /// Returns counts by event type key.
    /// </summary>
    public Dictionary GetEventTypeCounts()
    {
        Dictionary counts = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            int key = (int)historyEvent.Type;
            int current = 0;
            if (counts.ContainsKey(key))
            {
                current = (int)counts[key];
            }

            counts[key] = current + 1;
        }

        return counts;
    }

    /// <summary>
    /// Returns most common event type, or Founding for empty history.
    /// </summary>
    public HistoryEvent.EventType GetMostCommonEventType()
    {
        Dictionary counts = GetEventTypeCounts();
        int maxCount = 0;
        HistoryEvent.EventType mostCommon = HistoryEvent.EventType.Founding;
        foreach (Variant typeKey in counts.Keys)
        {
            int count = (int)counts[typeKey];
            if (count > maxCount)
            {
                maxCount = count;
                mostCommon = (HistoryEvent.EventType)(int)typeKey;
            }
        }

        return mostCommon;
    }

    /// <summary>
    /// Clears all events.
    /// </summary>
    public void Clear()
    {
        _events.Clear();
        _needsSort = false;
    }

    /// <summary>
    /// Removes event instance if present.
    /// </summary>
    public bool RemoveEvent(HistoryEvent historyEvent)
    {
        int index = _events.IndexOf(historyEvent);
        if (index < 0)
        {
            return false;
        }

        _events.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Creates a deep copy of history events.
    /// </summary>
    public PopulationHistory Duplicate()
    {
        PopulationHistory copy = new();
        foreach (HistoryEvent historyEvent in _events)
        {
            copy.AddEvent(HistoryEvent.FromDictionary(historyEvent.ToDictionary()));
        }

        return copy;
    }

    /// <summary>
    /// Merges another history into this one.
    /// </summary>
    public void Merge(PopulationHistory other)
    {
        foreach (HistoryEvent historyEvent in other._events)
        {
            _events.Add(historyEvent);
        }

        _needsSort = true;
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
    /// Compatibility alias for legacy API naming.
    /// </summary>
    public Dictionary ToDict()
    {
        return ToDictionary();
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
    /// Compatibility alias for legacy API naming.
    /// </summary>
    public static PopulationHistory FromDict(Dictionary data)
    {
        return FromDictionary(data);
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
