using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.App.Viewer;

/// <summary>
/// Legacy GDScript-compatible API wrappers for ObjectViewer.
/// </summary>
public partial class ObjectViewer
{
	/// <summary>
	/// Legacy object type selector for generate_object.
	/// </summary>
	public enum ObjectType
	{
		Star = 0,
		Planet = 1,
		Moon = 2,
		Asteroid = 3,
	}

	/// <summary>
	/// Legacy ready-state accessor.
	/// </summary>
	public bool is_ready => _statusLabel != null;

	/// <summary>
	/// Legacy current-body accessor.
	/// </summary>
	public CelestialBody? current_body => _currentBody;

	/// <summary>
	/// Legacy camera accessor.
	/// </summary>
	public CameraController? camera => _camera;

	/// <summary>
	/// Legacy status-label accessor.
	/// </summary>
	public Label? status_label => _statusLabel;

	/// <summary>
	/// Legacy side-panel accessor.
	/// </summary>
	public Control? side_panel => GetNodeOrNull<Control>("UI/SidePanel");

	/// <summary>
	/// Legacy panel-container accessor.
	/// </summary>
	public Control? panel_container => GetNodeOrNull<Control>("UI/SidePanel/MarginContainer");

	/// <summary>
	/// Legacy inspector-panel accessor.
	/// </summary>
	public Node? inspector_panel => _inspectorPanel;

	/// <summary>
	/// Legacy wrapper to display a primary body with no moons.
	/// </summary>
	public void display_body(Variant bodyVariant)
	{
		DisplayExternalBody(bodyVariant, [], _sourceStarSeed);
	}

	/// <summary>
	/// Legacy error-status wrapper.
	/// </summary>
	public void set_error(string message, bool _showPopup = false)
	{
		if (_statusLabel != null)
		{
			_statusLabel.Text = "Error: " + message;
			_statusLabel.Modulate = new Color(1.0f, 0.3f, 0.3f, 1.0f);
		}
	}

	/// <summary>
	/// Legacy wrapper for generated object creation by enum value.
	/// </summary>
	public void generate_object(int typeValue, int seedValue)
	{
		generate_object((ObjectType)typeValue, seedValue);
	}

	/// <summary>
	/// Legacy wrapper for generated object creation by enum.
	/// </summary>
	public void generate_object(ObjectType objectType, int seedValue)
	{
		SeededRng rng = new(seedValue);
		CelestialBody? body;

		if (objectType == ObjectType.Star)
		{
			StarSpec spec = StarSpec.Random(seedValue);
			body = StarGenerator.Generate(spec, rng);
		}
		else if (objectType == ObjectType.Planet)
		{
			PlanetSpec spec = PlanetSpec.Random(seedValue);
			body = PlanetGenerator.Generate(spec, ParentContext.SunLike(), rng);
		}
		else if (objectType == ObjectType.Moon)
		{
			MoonSpec spec = MoonSpec.Random(seedValue);
			ParentContext moonContext = ParentContext.ForMoon(
				Units.SolarMassKg,
				3.828e26,
				5778.0,
				4.6e9,
				5.2 * Units.AuMeters,
				Units.JupiterMassKg,
				Units.JupiterRadiusMeters,
				5.0e8);
			body = MoonGenerator.Generate(spec, moonContext, rng);
		}
		else
		{
			AsteroidSpec spec = AsteroidSpec.Random(seedValue);
			body = AsteroidGenerator.Generate(spec, ParentContext.SunLike(2.7 * Units.AuMeters), rng);
		}

		if (body == null)
		{
			throw new System.InvalidOperationException($"Failed to generate body for object type {objectType}.");
		}

		DisplayExternalBody(body, [], 0);
		_navigatedFromSystem = false;
		SetGenerationControlsEnabled(true);
		SetFileControlsEnabled(true);
	}
}
