#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.App.SystemViewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for the system inspector's small-body reservoir readouts.
/// </summary>
public static class TestSystemInspectorPanel
{
    /// <summary>
    /// Tests the system overview lists first-class small-body reservoir families.
    /// </summary>
    public static void TestSystemOverviewShowsSmallBodyReservoirFamilies()
    {
        SystemInspectorPanel? panel = null;
        try
        {
            panel = CreatePanel();
            SolarSystem system = CreateReservoirSystem();

            panel.DisplaySystem(system);

            if (!ContainsLabelText(panel, "Small-Body Reservoirs"))
            {
                throw new InvalidOperationException("System overview should include a small-body reservoir section.");
            }

            if (!ContainsLabelText(panel, "Hot Classical 38%"))
            {
                throw new InvalidOperationException("System overview should show weighted TNO reservoir families.");
            }
        }
        finally
        {
            CleanupPanel(panel);
        }
    }

    /// <summary>
    /// Tests selected belts summarize their attached reservoir families.
    /// </summary>
    public static void TestSelectedBeltShowsReservoirFamilySummary()
    {
        SystemInspectorPanel? panel = null;
        try
        {
            panel = CreatePanel();
            SolarSystem system = CreateReservoirSystem();

            panel.DisplaySelectedBelt(system.AsteroidBelts[0], system);

            if (!ContainsLabelText(panel, "Reservoir Families:"))
            {
                throw new InvalidOperationException("Selected belt should include a reservoir-family row.");
            }

            if (!ContainsLabelText(panel, "Resonant 26%"))
            {
                throw new InvalidOperationException("Selected belt should summarize linked reservoir-family weights.");
            }
        }
        finally
        {
            CleanupPanel(panel);
        }
    }

    /// <summary>
    /// Tests the dedicated reservoir panel shows source, span, status, and population-surface caveats.
    /// </summary>
    public static void TestReservoirPanelShowsDedicatedFamilyDetails()
    {
        SystemInspectorPanel? panel = null;
        try
        {
            panel = CreatePanel();
            SolarSystem system = CreateReservoirSystem();

            panel.DisplaySystem(system);

            if (!ContainsLabelText(panel, "Records:"))
            {
                throw new InvalidOperationException("Reservoir panel should show a dedicated record count.");
            }

            if (!ContainsLabelText(panel, "35.0-48.0 AU"))
            {
                throw new InvalidOperationException("Reservoir panel should show the reservoir radial span.");
            }

            if (!ContainsLabelText(panel, "KavelaarsEtAl2023, BernardinelliEtAl2022"))
            {
                throw new InvalidOperationException("Reservoir panel should show source IDs for human audit.");
            }

            if (!ContainsLabelText(panel, "Diagnostic Proxy"))
            {
                throw new InvalidOperationException("Reservoir panel should label current reservoir implementation status.");
            }

            if (!ContainsLabelText(panel, "No native life; stations/habitats follow-up"))
            {
                throw new InvalidOperationException("Reservoir panel should avoid implying native-life or colony generation for belts.");
            }
        }
        finally
        {
            CleanupPanel(panel);
        }
    }

    /// <summary>
    /// Tests selected belts expose their tracked large objects as focusable subentries.
    /// </summary>
    public static void TestSelectedBeltShowsLargeObjectSubentries()
    {
        SystemInspectorPanel? panel = null;
        try
        {
            panel = CreatePanel();
            SolarSystem system = CreateReservoirSystem();

            panel.DisplaySelectedBelt(system.AsteroidBelts[0], system);

            if (!ContainsLabelText(panel, "Large Objects"))
            {
                throw new InvalidOperationException("Selected belt should include a large-object section.");
            }

            if (!ContainsLabelText(panel, "900 km dia"))
            {
                throw new InvalidOperationException("Selected belt should show major-object diameter semantics.");
            }

            if (!ContainsLabelText(panel, "Habitation Model:"))
            {
                throw new InvalidOperationException("Selected belt should note that belt habitation is station/habitat follow-up, not native life.");
            }
        }
        finally
        {
            CleanupPanel(panel);
        }
    }

    private static SystemInspectorPanel CreatePanel()
    {
        SystemInspectorPanel panel = new();

        VBoxContainer overviewSection = new()
        {
            Name = "OverviewSection",
        };
        overviewSection.AddChild(new VBoxContainer
        {
            Name = "Content",
        });
        panel.AddChild(overviewSection);

        VBoxContainer selectedSection = new()
        {
            Name = "SelectedBodySection",
        };
        selectedSection.AddChild(new VBoxContainer
        {
            Name = "Content",
        });
        selectedSection.AddChild(new Button
        {
            Name = "OpenViewerButton",
        });
        panel.AddChild(selectedSection);

        VBoxContainer reservoirSection = new()
        {
            Name = "ReservoirSection",
        };
        reservoirSection.AddChild(new VBoxContainer
        {
            Name = "Content",
        });
        panel.AddChild(reservoirSection);

        SceneTree? tree = Engine.GetMainLoop() as SceneTree;
        if (tree == null)
        {
            throw new InvalidOperationException("System inspector tests require an active Godot scene tree.");
        }

        tree.Root.AddChild(panel);
        return panel;
    }

    private static void CleanupPanel(SystemInspectorPanel? panel)
    {
        if (panel == null || !GodotObject.IsInstanceValid(panel))
        {
            return;
        }

        panel.QueueFree();
    }

    private static SolarSystem CreateReservoirSystem()
    {
        SolarSystem system = new("sys_reservoir", "Reservoir Test");
        CelestialBody star = new("star_1", "Sol", CelestialType.Type.Star)
        {
            Physical = new PhysicalProps(Units.SolarMassKg, Units.SolarRadiusMeters, 2.16e6),
            Stellar = new StellarProps(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9),
        };
        system.AddBody(star);

        AsteroidBelt belt = new("belt_outer", "Outer Asteroid Belt")
        {
            OrbitHostId = "star_1",
            InnerRadiusM = 35.0 * Units.AuMeters,
            OuterRadiusM = 48.0 * Units.AuMeters,
            PrimaryComposition = AsteroidBelt.Composition.Icy,
            ReservoirKind = "trans_neptunian_reservoir",
            ReservoirSubfamily = "hot_classical_tno_proxy",
        };
        system.AddAsteroidBelt(belt);

        CelestialBody asteroid = new("asteroid_belt_outer_0", "1 Test TNO", CelestialType.Type.Asteroid)
        {
            Physical = new PhysicalProps(9.0e20, 450.0e3, 2.0e4),
            Orbital = new OrbitalProps(42.0 * Units.AuMeters, 0.04, 2.0, 0.0, 0.0, 45.0, star.Id),
        };
        system.AddBody(asteroid);
        belt.MajorAsteroidIds.Add(asteroid.Id);

        AddReservoir(system, belt, "hot_classical", 0.38);
        AddReservoir(system, belt, "resonant", 0.26);
        AddReservoir(system, belt, "cold_classical", 0.14);
        AddReservoir(system, belt, "scattered", 0.12);
        AddReservoir(system, belt, "comet_feeding", 0.06);
        AddReservoir(system, belt, "centaur", 0.04);
        return system;
    }

    private static void AddReservoir(SolarSystem system, AsteroidBelt belt, string family, double weight)
    {
        system.AddSmallBodyReservoir(new SmallBodyReservoir($"reservoir_{family}", family)
        {
            AnchorBeltId = belt.Id,
            OrbitHostId = belt.OrbitHostId,
            ReservoirKind = belt.ReservoirKind,
            ReservoirFamily = family,
            RelativeWeight = weight,
            InnerRadiusM = belt.InnerRadiusM,
            OuterRadiusM = belt.OuterRadiusM,
            SourceIds = "KavelaarsEtAl2023;BernardinelliEtAl2022",
        });
    }

    private static bool ContainsLabelText(Node node, string text)
    {
        if (node is Label label && label.Text.Contains(text, StringComparison.Ordinal))
        {
            return true;
        }

        if (node is Button button && button.Text.Contains(text, StringComparison.Ordinal))
        {
            return true;
        }

        foreach (Node child in node.GetChildren())
        {
            if (ContainsLabelText(child, text))
            {
                return true;
            }
        }

        return false;
    }
}
