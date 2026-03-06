#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for PopulationHistory timeline management.
/// </summary>
public static class TestPopulationHistory
{
    /// <summary>
    /// Creates a sample history for testing.
    /// </summary>
    private static PopulationHistory CreateSampleHistory()
    {
        PopulationHistory history = new();

        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.Founding, -1000, "The Founding", "", 0.5));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.Expansion, -800, "First Expansion", "", 0.4));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.War, -500, "The Great War", "", -0.7));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.GoldenAge, -200, "The Golden Age", "", 0.8));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.Plague, 0, "The Plague", "", -0.6));

        Array<HistoryEvent> events = history.GetAllEvents();
        events[0].PopulationDelta = 1000;
        events[1].PopulationDelta = 5000;
        events[2].PopulationDelta = -10000;
        events[3].PopulationDelta = 20000;
        events[4].PopulationDelta = -15000;

        return history;
    }

    /// <summary>
    /// Tests empty history.
    /// </summary>
    public static void TestEmptyHistory()
    {
        PopulationHistory history = new();
        DotNetNativeTestSuite.AssertEqual(0, history.Size(), "Empty history size should be 0");
        DotNetNativeTestSuite.AssertTrue(history.IsEmpty(), "Empty history should return true");
        DotNetNativeTestSuite.AssertEqual(null, history.GetFirstEvent(), "GetFirstEvent should return null");
        DotNetNativeTestSuite.AssertEqual(null, history.GetLastEvent(), "GetLastEvent should return null");
    }

    /// <summary>
    /// Tests adding events.
    /// </summary>
    public static void TestAddEvent()
    {
        PopulationHistory history = new();
        HistoryEvent historyEvent = new(HistoryEvent.EventType.Founding, -1000, "Test", "", 0.5);

        history.AddEvent(historyEvent);

        DotNetNativeTestSuite.AssertEqual(1, history.Size(), "Size should be 1");
        DotNetNativeTestSuite.AssertFalse(history.IsEmpty(), "Should not be empty");
    }

    /// <summary>
    /// Tests add_new_event convenience method.
    /// </summary>
    public static void TestAddNewEvent()
    {
        PopulationHistory history = new();

        HistoryEvent historyEvent = history.AddNewEvent(
            HistoryEvent.EventType.War,
            -500,
            "The War",
            "A great conflict.",
            -0.5
        );

        DotNetNativeTestSuite.AssertEqual(1, history.Size(), "Size should be 1");
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.War, historyEvent.Type, "Type should be War");
        DotNetNativeTestSuite.AssertEqual(-500, historyEvent.Year, "Year should be -500");
    }

    /// <summary>
    /// Tests events are sorted by year.
    /// </summary>
    public static void TestEventsSorted()
    {
        PopulationHistory history = new();

        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.War, 0, "Middle", "", 0.0));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.Founding, -1000, "First", "", 0.0));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.Expansion, 500, "Last", "", 0.0));

        Array<HistoryEvent> events = history.GetAllEvents();

        DotNetNativeTestSuite.AssertEqual(-1000, events[0].Year, "First event should be year -1000");
        DotNetNativeTestSuite.AssertEqual(0, events[1].Year, "Second event should be year 0");
        DotNetNativeTestSuite.AssertEqual(500, events[2].Year, "Third event should be year 500");
    }

    /// <summary>
    /// Tests get_event by index.
    /// </summary>
    public static void TestGetEvent()
    {
        PopulationHistory history = CreateSampleHistory();

        HistoryEvent first = history.GetEvent(0);
        DotNetNativeTestSuite.AssertNotNull(first, "First event should not be null");
        DotNetNativeTestSuite.AssertEqual(-1000, first.Year, "First event year should be -1000");

        HistoryEvent outOfBounds = history.GetEvent(100);
        DotNetNativeTestSuite.AssertEqual(null, outOfBounds, "Out of bounds should return null");

        HistoryEvent negative = history.GetEvent(-1);
        DotNetNativeTestSuite.AssertEqual(null, negative, "Negative index should return null");
    }

    /// <summary>
    /// Tests get_first_event.
    /// </summary>
    public static void TestGetFirstEvent()
    {
        PopulationHistory history = CreateSampleHistory();
        HistoryEvent first = history.GetFirstEvent();

        DotNetNativeTestSuite.AssertNotNull(first, "First event should not be null");
        DotNetNativeTestSuite.AssertEqual(-1000, first.Year, "First event year should be -1000");
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.Founding, first.Type, "First event type should be Founding");
    }

    /// <summary>
    /// Tests get_last_event.
    /// </summary>
    public static void TestGetLastEvent()
    {
        PopulationHistory history = CreateSampleHistory();
        HistoryEvent last = history.GetLastEvent();

        DotNetNativeTestSuite.AssertNotNull(last, "Last event should not be null");
        DotNetNativeTestSuite.AssertEqual(0, last.Year, "Last event year should be 0");
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.Plague, last.Type, "Last event type should be Plague");
    }

    /// <summary>
    /// Tests get_founding_event.
    /// </summary>
    public static void TestGetFoundingEvent()
    {
        PopulationHistory history = CreateSampleHistory();
        HistoryEvent founding = history.GetFoundingEvent();

        DotNetNativeTestSuite.AssertNotNull(founding, "Founding event should not be null");
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.Founding, founding.Type, "Founding event type should be Founding");
    }

    /// <summary>
    /// Tests get_founding_event when none exists.
    /// </summary>
    public static void TestGetFoundingEventNone()
    {
        PopulationHistory history = new();
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.War, 0, "War", "", 0.0));

        HistoryEvent founding = history.GetFoundingEvent();
        DotNetNativeTestSuite.AssertEqual(null, founding, "Should return null when no founding event");
    }

    /// <summary>
    /// Tests get_events_by_type.
    /// </summary>
    public static void TestGetEventsByType()
    {
        PopulationHistory history = CreateSampleHistory();

        Array<HistoryEvent> wars = history.GetEventsByType(HistoryEvent.EventType.War);
        DotNetNativeTestSuite.AssertEqual(1, wars.Count, "Should have 1 war");
        DotNetNativeTestSuite.AssertEqual("The Great War", wars[0].Title, "War title should match");

        Array<HistoryEvent> expansions = history.GetEventsByType(HistoryEvent.EventType.Expansion);
        DotNetNativeTestSuite.AssertEqual(1, expansions.Count, "Should have 1 expansion");

        Array<HistoryEvent> migrations = history.GetEventsByType(HistoryEvent.EventType.Migration);
        DotNetNativeTestSuite.AssertEqual(0, migrations.Count, "Should have 0 migrations");
    }

    /// <summary>
    /// Tests get_events_in_range.
    /// </summary>
    public static void TestGetEventsInRange()
    {
        PopulationHistory history = CreateSampleHistory();

        Array<HistoryEvent> events = history.GetEventsInRange(-600, -100);
        DotNetNativeTestSuite.AssertEqual(2, events.Count, "Should have 2 events in range");
    }

    /// <summary>
    /// Tests get_events_before.
    /// </summary>
    public static void TestGetEventsBefore()
    {
        PopulationHistory history = CreateSampleHistory();

        Array<HistoryEvent> events = history.GetEventsBefore(-500);
        DotNetNativeTestSuite.AssertEqual(2, events.Count, "Should have 2 events before -500");
    }

    /// <summary>
    /// Tests get_events_after.
    /// </summary>
    public static void TestGetEventsAfter()
    {
        PopulationHistory history = CreateSampleHistory();

        Array<HistoryEvent> events = history.GetEventsAfter(-500);
        DotNetNativeTestSuite.AssertEqual(2, events.Count, "Should have 2 events after -500");
    }

    /// <summary>
    /// Tests get_harmful_events.
    /// </summary>
    public static void TestGetHarmfulEvents()
    {
        PopulationHistory history = CreateSampleHistory();

        Array<HistoryEvent> harmful = history.GetHarmfulEvents();
        DotNetNativeTestSuite.AssertEqual(2, harmful.Count, "Should have 2 harmful events");
    }

    /// <summary>
    /// Tests get_beneficial_events.
    /// </summary>
    public static void TestGetBeneficialEvents()
    {
        PopulationHistory history = CreateSampleHistory();

        Array<HistoryEvent> beneficial = history.GetBeneficialEvents();
        DotNetNativeTestSuite.AssertEqual(3, beneficial.Count, "Should have 3 beneficial events");
    }

    /// <summary>
    /// Tests get_events_involving.
    /// </summary>
    public static void TestGetEventsInvolving()
    {
        PopulationHistory history = new();

        HistoryEvent event1 = new(HistoryEvent.EventType.War, -500, "War", "", -0.5);
        event1.RelatedPopulationId = "enemy_001";
        history.AddEvent(event1);

        HistoryEvent event2 = new(HistoryEvent.EventType.Treaty, -400, "Peace", "", 0.3);
        event2.RelatedPopulationId = "enemy_001";
        history.AddEvent(event2);

        HistoryEvent event3 = new(HistoryEvent.EventType.Expansion, -300, "Expand", "", 0.4);
        history.AddEvent(event3);

        Array<HistoryEvent> related = history.GetEventsInvolving("enemy_001");
        DotNetNativeTestSuite.AssertEqual(2, related.Count, "Should have 2 events involving enemy_001");
    }

    /// <summary>
    /// Tests get_total_population_delta.
    /// </summary>
    public static void TestGetTotalPopulationDelta()
    {
        PopulationHistory history = CreateSampleHistory();

        int total = history.GetTotalPopulationDelta();
        DotNetNativeTestSuite.AssertEqual(1000, total, "Total population delta should be 1000");
    }

    /// <summary>
    /// Tests get_population_delta_in_range.
    /// </summary>
    public static void TestGetPopulationDeltaInRange()
    {
        PopulationHistory history = CreateSampleHistory();

        int delta = history.GetPopulationDeltaInRange(-600, -100);
        DotNetNativeTestSuite.AssertEqual(10000, delta, "Population delta in range should be 10000");
    }

    /// <summary>
    /// Tests get_year_span.
    /// </summary>
    public static void TestGetYearSpan()
    {
        PopulationHistory history = CreateSampleHistory();

        Godot.Collections.Dictionary span = history.GetYearSpan();
        DotNetNativeTestSuite.AssertEqual(-1000, span["start"].AsInt32(), "Start year should be -1000");
        DotNetNativeTestSuite.AssertEqual(0, span["end"].AsInt32(), "End year should be 0");
    }

    /// <summary>
    /// Tests get_year_span empty.
    /// </summary>
    public static void TestGetYearSpanEmpty()
    {
        PopulationHistory history = new();

        Godot.Collections.Dictionary span = history.GetYearSpan();
        DotNetNativeTestSuite.AssertTrue(span.Count == 0, "Empty history should return empty span");
    }

    /// <summary>
    /// Tests get_duration_years.
    /// </summary>
    public static void TestGetDurationYears()
    {
        PopulationHistory history = CreateSampleHistory();

        int duration = history.GetDurationYears();
        DotNetNativeTestSuite.AssertEqual(1000, duration, "Duration should be 1000 years");
    }

    /// <summary>
    /// Tests get_duration_years with single event.
    /// </summary>
    public static void TestGetDurationYearsSingle()
    {
        PopulationHistory history = new();
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.Founding, 0, "Start", "", 0.0));

        int duration = history.GetDurationYears();
        DotNetNativeTestSuite.AssertEqual(0, duration, "Single event should have 0 duration");
    }

    /// <summary>
    /// Tests get_event_type_counts.
    /// </summary>
    public static void TestGetEventTypeCounts()
    {
        PopulationHistory history = CreateSampleHistory();

        Godot.Collections.Dictionary counts = history.GetEventTypeCounts();
        DotNetNativeTestSuite.AssertEqual(1, counts[(int)HistoryEvent.EventType.Founding].AsInt32(), "Should have 1 Founding event");
        DotNetNativeTestSuite.AssertEqual(1, counts[(int)HistoryEvent.EventType.War].AsInt32(), "Should have 1 War event");
        DotNetNativeTestSuite.AssertEqual(1, counts[(int)HistoryEvent.EventType.GoldenAge].AsInt32(), "Should have 1 GoldenAge event");
    }

    /// <summary>
    /// Tests get_most_common_event_type.
    /// </summary>
    public static void TestGetMostCommonEventType()
    {
        PopulationHistory history = new();
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.War, -500, "War 1", "", 0.0));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.War, -400, "War 2", "", 0.0));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.War, -300, "War 3", "", 0.0));
        history.AddEvent(new HistoryEvent(HistoryEvent.EventType.Plague, -200, "Plague", "", 0.0));

        HistoryEvent.EventType mostCommon = history.GetMostCommonEventType();
        DotNetNativeTestSuite.AssertEqual(HistoryEvent.EventType.War, mostCommon, "Most common should be War");
    }

    /// <summary>
    /// Tests clear.
    /// </summary>
    public static void TestClear()
    {
        PopulationHistory history = CreateSampleHistory();
        DotNetNativeTestSuite.AssertFalse(history.IsEmpty(), "Should not be empty");

        history.Clear();
        DotNetNativeTestSuite.AssertTrue(history.IsEmpty(), "Should be empty after clear");
    }

    /// <summary>
    /// Tests remove_event.
    /// </summary>
    public static void TestRemoveEvent()
    {
        PopulationHistory history = new();
        HistoryEvent event1 = new(HistoryEvent.EventType.Founding, -1000, "First", "", 0.0);
        HistoryEvent event2 = new(HistoryEvent.EventType.War, -500, "Second", "", 0.0);

        history.AddEvent(event1);
        history.AddEvent(event2);
        DotNetNativeTestSuite.AssertEqual(2, history.Size(), "Size should be 2");

        bool removed = history.RemoveEvent(event1);
        DotNetNativeTestSuite.AssertTrue(removed, "Remove should return true");
        DotNetNativeTestSuite.AssertEqual(1, history.Size(), "Size should be 1");

        bool notFound = history.RemoveEvent(event1);
        DotNetNativeTestSuite.AssertFalse(notFound, "Remove should return false for not found");
    }

    /// <summary>
    /// Tests duplicate.
    /// </summary>
    public static void TestDuplicate()
    {
        PopulationHistory original = CreateSampleHistory();
        PopulationHistory copy = original.Duplicate();

        DotNetNativeTestSuite.AssertEqual(original.Size(), copy.Size(), "Copy size should match");

        original.Clear();
        DotNetNativeTestSuite.AssertEqual(5, copy.Size(), "Copy should be independent");
    }

    /// <summary>
    /// Tests merge.
    /// </summary>
    public static void TestMerge()
    {
        PopulationHistory history1 = new();
        history1.AddEvent(new HistoryEvent(HistoryEvent.EventType.Founding, -1000, "First", "", 0.0));

        PopulationHistory history2 = new();
        history2.AddEvent(new HistoryEvent(HistoryEvent.EventType.War, -500, "Second", "", 0.0));

        history1.Merge(history2);

        DotNetNativeTestSuite.AssertEqual(2, history1.Size(), "Merged size should be 2");

        Array<HistoryEvent> events = history1.GetAllEvents();
        DotNetNativeTestSuite.AssertEqual(-1000, events[0].Year, "First event should be -1000");
        DotNetNativeTestSuite.AssertEqual(-500, events[1].Year, "Second event should be -500");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        PopulationHistory original = CreateSampleHistory();

        Godot.Collections.Dictionary data = original.ToDictionary();
        PopulationHistory restored = PopulationHistory.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Size(), restored.Size(), "Size should match");

        Array<HistoryEvent> originalEvents = original.GetAllEvents();
        Array<HistoryEvent> restoredEvents = restored.GetAllEvents();

        for (int i = 0; i < originalEvents.Count; i += 1)
        {
            DotNetNativeTestSuite.AssertEqual(originalEvents[i].Year, restoredEvents[i].Year, $"Event {i} year should match");
            DotNetNativeTestSuite.AssertEqual(originalEvents[i].Type, restoredEvents[i].Type, $"Event {i} type should match");
            DotNetNativeTestSuite.AssertEqual(originalEvents[i].Title, restoredEvents[i].Title, $"Event {i} title should match");
        }
    }

    /// <summary>
    /// Tests empty history serialization.
    /// </summary>
    public static void TestEmptySerialization()
    {
        PopulationHistory original = new();

        Godot.Collections.Dictionary data = original.ToDictionary();
        PopulationHistory restored = PopulationHistory.FromDictionary(data);

        DotNetNativeTestSuite.AssertTrue(restored.IsEmpty(), "Restored should be empty");
    }
}
