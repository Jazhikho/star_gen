using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;

namespace StarGen.App.Viewer;

/// <summary>
/// C# inspector panel for the external object-viewer path.
/// </summary>
public partial class InspectorPanel : VBoxContainer
{
	/// <summary>
	/// Emitted when the user selects a moon or clears moon focus.
	/// </summary>
	[Signal]
	public delegate void MoonSelectedEventHandler(Variant moon);

	private VBoxContainer? _inspectorContainer;

	/// <summary>
	/// Caches the dynamic content container.
	/// </summary>
	public override void _Ready()
	{
		_inspectorContainer = GetNodeOrNull<VBoxContainer>("InspectorContainer");
	}

	/// <summary>
	/// GDScript-compatible clear wrapper.
	/// </summary>
	public void clear()
	{
		Clear();
	}

	/// <summary>
	/// Clears dynamic inspector content.
	/// </summary>
	public void Clear()
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		foreach (Node child in _inspectorContainer.GetChildren())
		{
			child.QueueFree();
		}
	}

	/// <summary>
	/// GDScript-compatible body display wrapper.
	/// </summary>
	public void display_body_with_moons(Variant bodyVariant, Godot.Collections.Array moons)
	{
		CelestialBody? body = ConvertVariantToCelestialBody(bodyVariant);
		Godot.Collections.Array<CelestialBody> typedMoons = ConvertVariantArrayToBodies(moons);
		DisplayBodyWithMoons(body, typedMoons, moons);
	}

	/// <summary>
	/// Displays a body and optional moon list.
	/// </summary>
	public void DisplayBodyWithMoons(
		CelestialBody? body,
		Godot.Collections.Array<CelestialBody> moons,
		Godot.Collections.Array? originalMoonVariants = null)
	{
		Clear();
		if (_inspectorContainer == null)
		{
			return;
		}

		if (body == null)
		{
			AddInfoLabel("No object loaded");
			return;
		}

		if (moons.Count > 0)
		{
			AddMoonListSection(moons, originalMoonVariants ?? BuildVariantArray(moons), null);
		}

		AddBodySummarySection(body);
	}

	/// <summary>
	/// GDScript-compatible focused-moon display wrapper.
	/// </summary>
	public void display_focused_moon(Variant moonVariant, Variant planetVariant, Godot.Collections.Array allMoons)
	{
		CelestialBody? moon = ConvertVariantToCelestialBody(moonVariant);
		CelestialBody? planet = ConvertVariantToCelestialBody(planetVariant);
		Godot.Collections.Array<CelestialBody> typedMoons = ConvertVariantArrayToBodies(allMoons);
		DisplayFocusedMoon(moon, planet, typedMoons, allMoons);
	}

	/// <summary>
	/// Displays a focused moon view with a back button.
	/// </summary>
	public void DisplayFocusedMoon(
		CelestialBody? moon,
		CelestialBody? planet,
		Godot.Collections.Array<CelestialBody> allMoons,
		Godot.Collections.Array? originalMoonVariants = null)
	{
		Clear();
		if (_inspectorContainer == null || moon == null)
		{
			return;
		}

		AddBackToPlanetButton(planet);
		AddBodySummarySection(moon, $"Moon: {moon.Name}");

		if (allMoons.Count > 1)
		{
			AddMoonListSection(allMoons, originalMoonVariants ?? BuildVariantArray(allMoons), moon);
		}

		if (planet != null)
		{
			AddBodySummarySection(planet, $"Parent: {planet.Name}");
		}
	}

	private void AddBodySummarySection(CelestialBody body, string? headerOverride = null)
	{
		AddSectionHeader(headerOverride ?? "Body");
		string nameValue;
		if (string.IsNullOrEmpty(body.Name))
		{
			nameValue = body.Id;
		}
		else
		{
			nameValue = body.Name;
		}

		AddProperty("Name", nameValue);
		AddProperty("Type", body.GetTypeString());
		AddProperty("ID", body.Id);
		AddPhysicalSummary(body.Physical);

		if (body.HasStellar() && body.Stellar != null)
		{
			AddProperty("Spectral Class", body.Stellar.SpectralClass);
			AddProperty("Temperature", $"{body.Stellar.EffectiveTemperatureK:0} K");
		}

		if (body.HasOrbital() && body.Orbital != null)
		{
			AddProperty("Semi-major Axis", FormatDistance(body.Orbital.SemiMajorAxisM));
			AddProperty("Eccentricity", $"{body.Orbital.Eccentricity:0.0000}");
		}

		if (body.HasAtmosphere() && body.Atmosphere != null)
		{
			AddProperty("Surface Pressure", $"{body.Atmosphere.SurfacePressurePa / 101325.0:0.###} atm");
		}
	}

	private void AddPhysicalSummary(PhysicalProps physical)
	{
		AddProperty("Mass", $"{physical.MassKg:0.###e0} kg");
		AddProperty("Radius", FormatDistance(physical.RadiusM));
		AddProperty("Gravity", $"{physical.GetSurfaceGravityMS2():0.00} m/s^2");
	}

	private void AddMoonListSection(
		Godot.Collections.Array<CelestialBody> moons,
		Godot.Collections.Array originalMoonVariants,
		CelestialBody? focusedMoon)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		VBoxContainer section = new();
		section.AddThemeConstantOverride("separation", 2);

		Label title = new();
		title.Text = $"Moons ({moons.Count})";
		title.AddThemeFontSizeOverride("font_size", 14);
		section.AddChild(title);

		for (int index = 0; index < moons.Count; index++)
		{
			CelestialBody moon = moons[index];
			Button button = new();
			if (focusedMoon != null && moon.Id == focusedMoon.Id)
			{
				button.Text = $"* {moon.Name}";
			}
			else
			{
				button.Text = moon.Name;
			}
			button.Flat = true;
			button.Alignment = HorizontalAlignment.Left;
			Variant emitValue;
			if (index < originalMoonVariants.Count)
			{
				emitValue = (Variant)originalMoonVariants[index];
			}
			else
			{
				emitValue = Variant.From((GodotObject?)null);
			}
			button.Pressed += () => EmitSignal(SignalName.MoonSelected, emitValue);
			section.AddChild(button);
		}

		_inspectorContainer.AddChild(section);
	}

	private void AddBackToPlanetButton(CelestialBody? planet)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		Button button = new();
		if (planet == null)
		{
			button.Text = "Back to Planet";
		}
		else
		{
			button.Text = $"Back to {planet.Name}";
		}
		button.Pressed += () => EmitSignal(SignalName.MoonSelected, new Variant());
		_inspectorContainer.AddChild(button);
	}

	private void AddSectionHeader(string title)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		Label label = new();
		label.Text = title;
		label.AddThemeFontSizeOverride("font_size", 14);
		_inspectorContainer.AddChild(label);
	}

	private void AddProperty(string labelText, string valueText)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		HBoxContainer row = new();

		Label label = new();
		label.Text = $"{labelText}:";
		label.CustomMinimumSize = new Vector2(120.0f, 0.0f);
		row.AddChild(label);

		Label value = new();
		value.Text = valueText;
		value.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		row.AddChild(value);

		_inspectorContainer.AddChild(row);
	}

	private void AddInfoLabel(string text)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		Label label = new();
		label.Text = text;
		_inspectorContainer.AddChild(label);
	}

	private static string FormatDistance(double meters)
	{
		if (meters >= 1.0e9)
		{
			return $"{meters / 1.0e9:0.###} Gm";
		}

		if (meters >= 1.0e6)
		{
			return $"{meters / 1.0e6:0.###} Mm";
		}

		if (meters >= 1000.0)
		{
			return $"{meters / 1000.0:0.###} km";
		}

		return $"{meters:0.###} m";
	}

	private static CelestialBody? ConvertVariantToCelestialBody(Variant value)
	{
		if (value.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = value.AsGodotObject();
		if (godotObject is CelestialBody typedBody)
		{
			return typedBody;
		}

		if (godotObject != null && godotObject.HasMethod("to_dict"))
		{
			Variant dataVariant = godotObject.Call("to_dict");
			if (dataVariant.VariantType == Variant.Type.Dictionary)
			{
				return CelestialSerializer.FromDictionary((Godot.Collections.Dictionary)dataVariant);
			}
		}

		return null;
	}

	private static Godot.Collections.Array<CelestialBody> ConvertVariantArrayToBodies(Godot.Collections.Array values)
	{
		Godot.Collections.Array<CelestialBody> bodies = [];
		foreach (Variant value in values)
		{
			CelestialBody? body = ConvertVariantToCelestialBody(value);
			if (body != null)
			{
				bodies.Add(body);
			}
		}

		return bodies;
	}

	private static Godot.Collections.Array BuildVariantArray(Godot.Collections.Array<CelestialBody> bodies)
	{
		Godot.Collections.Array values = new();
		foreach (CelestialBody body in bodies)
		{
			values.Add(body);
		}

		return values;
	}
}
