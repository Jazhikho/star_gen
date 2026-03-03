using Godot;
using StarGen.App.Viewer;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// C# inspector panel for galaxy and star-selection details.
/// </summary>
public partial class GalaxyInspectorPanel : VBoxContainer
{
	/// <summary>
	/// Emitted when the user requests jump-route calculation.
	/// </summary>
	[Signal]
	public delegate void CalculateJumpRoutesRequestedEventHandler();

	/// <summary>
	/// Emitted when jump-route visibility is toggled.
	/// </summary>
	[Signal]
	public delegate void JumpRoutesVisibilityToggledEventHandler(bool showRoutes);

	/// <summary>
	/// Emitted when the user requests to open the selected star system.
	/// </summary>
	[Signal]
	public delegate void OpenSystemRequestedEventHandler(int starSeed, Vector3 worldPosition);

	private int _selectedStarSeed;
	private Vector3 _selectedStarPosition = Vector3.Zero;
	private StarSystemPreviewData? _currentPreview;

	private VBoxContainer? _overviewContainer;
	private VBoxContainer? _selectionContainer;
	private VBoxContainer? _previewContainer;
	private Button? _openSystemButton;
	private Button? _calculateRoutesButton;
	private CheckBox? _showRoutesCheck;
	private bool _isCalculating;

	/// <summary>
	/// Builds the inspector UI.
	/// </summary>
	public override void _Ready()
	{
		BuildUi();
	}

	/// <summary>
	/// GDScript-compatible wrapper for overview display.
	/// </summary>
	public void display_galaxy(Variant specVariant, int zoomLevel)
	{
		DisplayGalaxy(ConvertVariantToGalaxySpec(specVariant), zoomLevel);
	}

	/// <summary>
	/// Displays galaxy overview information.
	/// </summary>
	public void DisplayGalaxy(GalaxySpec? spec, int zoomLevel)
	{
		ClearContainer(_overviewContainer);
		if (_overviewContainer == null)
		{
			return;
		}

		if (spec == null)
		{
			AddProperty(_overviewContainer, "Status", "No galaxy loaded");
			return;
		}

		AddProperty(_overviewContainer, "Type", GetGalaxyTypeName(spec.Type));
		AddProperty(_overviewContainer, "Seed", spec.GalaxySeed.ToString());
		AddProperty(_overviewContainer, "Radius", $"{spec.RadiusPc / 1000.0:0.0} kpc");
		AddProperty(_overviewContainer, "Height", $"{spec.HeightPc / 1000.0:0.0} kpc");
		AddProperty(_overviewContainer, "Spiral Arms", spec.NumArms.ToString());
		AddProperty(_overviewContainer, "Arm Pitch", $"{spec.ArmPitchAngleDeg:0.0} deg");
		AddProperty(_overviewContainer, "View", GetZoomLevelName(zoomLevel));
	}

	/// <summary>
	/// GDScript-compatible wrapper for zoom-level updates.
	/// </summary>
	public void update_zoom_level(int zoomLevel)
	{
		UpdateZoomLevel(zoomLevel);
	}

	/// <summary>
	/// Updates the zoom-level display row.
	/// </summary>
	public void UpdateZoomLevel(int zoomLevel)
	{
		if (_overviewContainer == null)
		{
			return;
		}

		foreach (Node child in _overviewContainer.GetChildren())
		{
			if (child is not HBoxContainer row)
			{
				continue;
			}

			Label? keyLabel = row.GetNodeOrNull<Label>("Key");
			Label? valueLabel = row.GetNodeOrNull<Label>("Value");
			if (keyLabel != null && valueLabel != null && keyLabel.Text == "View:")
			{
				valueLabel.Text = GetZoomLevelName(zoomLevel);
				return;
			}
		}

		AddProperty(_overviewContainer, "View", GetZoomLevelName(zoomLevel));
	}

	/// <summary>
	/// Displays selected quadrant information.
	/// </summary>
	public void display_selected_quadrant(Vector3I coords, float density)
	{
		DisplaySelectedQuadrant(coords, density);
	}

	/// <summary>
	/// Displays selected quadrant information.
	/// </summary>
	public void DisplaySelectedQuadrant(Vector3I coords, float density)
	{
		ClearContainer(_selectionContainer);
		ClearStarSelection();
		if (_selectionContainer == null)
		{
			return;
		}

		AddProperty(_selectionContainer, "Type", "Quadrant");
		AddProperty(_selectionContainer, "Coordinates", FormatVector3I(coords));
		AddProperty(_selectionContainer, "Density", density.ToString("0.0000"));

		Vector3 center = GalaxyCoordinates.QuadrantToParsecCenter(coords);
		double distKpc = center.Length() / 1000.0;
		AddProperty(_selectionContainer, "Distance", $"{distKpc:0.00} kpc");
	}

	/// <summary>
	/// Displays selected sector information.
	/// </summary>
	public void display_selected_sector(Vector3I quadrantCoords, Vector3I sectorCoords, float density)
	{
		DisplaySelectedSector(quadrantCoords, sectorCoords, density);
	}

	/// <summary>
	/// Displays selected sector information.
	/// </summary>
	public void DisplaySelectedSector(Vector3I quadrantCoords, Vector3I sectorCoords, float density)
	{
		ClearContainer(_selectionContainer);
		ClearStarSelection();
		if (_selectionContainer == null)
		{
			return;
		}

		AddProperty(_selectionContainer, "Type", "Sector");
		AddProperty(_selectionContainer, "Quadrant", FormatVector3I(quadrantCoords));
		AddProperty(_selectionContainer, "Local", FormatVector3I(sectorCoords));
		AddProperty(_selectionContainer, "Density", density.ToString("0.0000"));
	}

	/// <summary>
	/// Displays selected star information.
	/// </summary>
	public void display_selected_star(Vector3 worldPosition, int starSeed)
	{
		DisplaySelectedStar(worldPosition, starSeed);
	}

	/// <summary>
	/// Displays selected star information.
	/// </summary>
	public void DisplaySelectedStar(Vector3 worldPosition, int starSeed)
	{
		ClearContainer(_selectionContainer);
		_selectedStarSeed = starSeed;
		_currentPreview = null;
		_selectedStarPosition = worldPosition;

		if (_selectionContainer != null)
		{
			AddProperty(_selectionContainer, "Type", "Star System");
			AddProperty(_selectionContainer, "Seed", starSeed.ToString());
			AddProperty(_selectionContainer, "X", $"{worldPosition.X:0.00} pc");
			AddProperty(_selectionContainer, "Y", $"{worldPosition.Y:0.00} pc");
			AddProperty(_selectionContainer, "Z", $"{worldPosition.Z:0.00} pc");

			float distPc = worldPosition.Length();
			AddProperty(
				_selectionContainer,
				"From Center",
				distPc > 1000.0f ? $"{distPc / 1000.0f:0.00} kpc" : $"{distPc:0.0} pc");
		}

		if (_openSystemButton != null)
		{
			_openSystemButton.Visible = true;
		}

		ClearContainer(_previewContainer);
		if (_previewContainer != null)
		{
			AddProperty(_previewContainer, "Status", "Generating preview...");
		}
	}

	/// <summary>
	/// GDScript-compatible preview wrapper.
	/// </summary>
	public void display_system_preview(Variant previewVariant)
	{
		DisplaySystemPreview(ConvertVariantToPreviewData(previewVariant));
	}

	/// <summary>
	/// Displays a generated system preview.
	/// </summary>
	public void DisplaySystemPreview(StarSystemPreviewData? preview)
	{
		_currentPreview = preview;
		ClearContainer(_previewContainer);
		if (_previewContainer == null)
		{
			return;
		}

		if (preview == null)
		{
			AddProperty(_previewContainer, "Status", "Preview unavailable");
			return;
		}

		AddProperty(_previewContainer, "Stars", preview.StarCount.ToString());
		for (int index = 0; index < preview.SpectralClasses.Length; index++)
		{
			string spectral = preview.SpectralClasses[index];
			float temp = index < preview.StarTemperatures.Length ? preview.StarTemperatures[index] : 0.0f;
			string tempText = temp > 0.0f ? $"{(int)temp} K" : "?";
			AddProperty(_previewContainer, $"  Star {index + 1}", $"{spectral}  {tempText}");
		}

		AddProperty(_previewContainer, "Planets", preview.PlanetCount.ToString());
		AddProperty(_previewContainer, "Moons", preview.MoonCount.ToString());
		AddProperty(_previewContainer, "Belts", preview.BeltCount.ToString());
		AddProperty(_previewContainer, "Metallicity", $"{preview.Metallicity:0.00} Zsun");
		AddProperty(_previewContainer, "Inhabited", preview.IsInhabited ? "Yes" : "No");
		if (preview.IsInhabited)
		{
			AddProperty(_previewContainer, "Population", PropertyFormatter.FormatPopulation(preview.TotalPopulation));
		}
	}

	/// <summary>
	/// GDScript-compatible clear wrapper.
	/// </summary>
	public void clear_selection()
	{
		ClearSelection();
	}

	/// <summary>
	/// Clears selection and preview state.
	/// </summary>
	public void ClearSelection()
	{
		ClearContainer(_selectionContainer);
		ClearContainer(_previewContainer);
		ClearStarSelection();
		if (_selectionContainer != null)
		{
			AddProperty(_selectionContainer, "Status", "Nothing selected");
		}
		if (_previewContainer != null)
		{
			AddProperty(_previewContainer, "Status", "Select a star to preview");
		}
	}

	/// <summary>
	/// GDScript-compatible calculating-state wrapper.
	/// </summary>
	public void set_jump_routes_calculating(bool calculating)
	{
		SetJumpRoutesCalculating(calculating);
	}

	/// <summary>
	/// Updates the calculating state for jump-route controls.
	/// </summary>
	public void SetJumpRoutesCalculating(bool calculating)
	{
		_isCalculating = calculating;
		if (_calculateRoutesButton != null)
		{
			_calculateRoutesButton.Disabled = calculating;
			_calculateRoutesButton.Text = calculating ? "Calculating..." : "Recalculate Jump Routes";
		}
	}

	/// <summary>
	/// GDScript-compatible availability wrapper.
	/// </summary>
	public void set_jump_routes_available(bool available)
	{
		SetJumpRoutesAvailable(available);
	}

	/// <summary>
	/// Updates jump-route availability state.
	/// </summary>
	public void SetJumpRoutesAvailable(bool available)
	{
		if (_showRoutesCheck != null)
		{
			_showRoutesCheck.Disabled = !available;
		}
		if (_calculateRoutesButton != null)
		{
			_calculateRoutesButton.Disabled = false;
			_calculateRoutesButton.Text = available ? "Recalculate Jump Routes" : "Calculate Jump Routes";
		}
		_isCalculating = false;
	}

	/// <summary>
	/// GDScript-compatible checkbox-state wrapper.
	/// </summary>
	public bool get_show_routes_checked()
	{
		return GetShowRoutesChecked();
	}

	/// <summary>
	/// Returns whether jump routes are currently toggled on.
	/// </summary>
	public bool GetShowRoutesChecked()
	{
		return _showRoutesCheck?.ButtonPressed ?? true;
	}

	/// <summary>
	/// Returns whether a star is selected.
	/// </summary>
	public bool has_star_selected()
	{
		return _selectedStarSeed != 0;
	}

	/// <summary>
	/// Returns the selected star seed.
	/// </summary>
	public int get_selected_star_seed()
	{
		return _selectedStarSeed;
	}

	/// <summary>
	/// Returns the selected star position.
	/// </summary>
	public Vector3 get_selected_star_position()
	{
		return _selectedStarPosition;
	}

	/// <summary>
	/// Returns the current preview data.
	/// </summary>
	public StarSystemPreviewData? get_current_preview()
	{
		return _currentPreview;
	}

	private void BuildUi()
	{
		AddThemeConstantOverride("separation", 4);

		AddTitle("Galaxy Inspector", 14);
		AddChild(new HSeparator());

		AddSectionLabel("Overview");
		_overviewContainer = CreateSectionContainer("OverviewContainer");
		AddChild(_overviewContainer);

		AddChild(new HSeparator());

		AddSectionLabel("Selection");
		_selectionContainer = CreateSectionContainer("SelectionContainer");
		AddChild(_selectionContainer);

		AddChild(new HSeparator());

		AddSectionLabel("System Preview");
		_previewContainer = CreateSectionContainer("PreviewContainer");
		AddChild(_previewContainer);

		_openSystemButton = new Button
		{
			Name = "OpenSystemButton",
			Text = "Open System",
			Visible = false,
		};
		_openSystemButton.Pressed += OnOpenSystemPressed;
		AddChild(_openSystemButton);

		AddChild(new HSeparator());

		AddSectionLabel("Jump Routes");

		_calculateRoutesButton = new Button
		{
			Name = "CalculateRoutesButton",
			Text = "Calculate Jump Routes",
		};
		_calculateRoutesButton.Pressed += OnCalculateRoutesPressed;
		AddChild(_calculateRoutesButton);

		_showRoutesCheck = new CheckBox
		{
			Name = "ShowRoutesCheck",
			Text = "Show Jump Routes",
			ButtonPressed = true,
			Disabled = true,
		};
		_showRoutesCheck.Toggled += OnShowRoutesToggled;
		AddChild(_showRoutesCheck);

		if (_selectionContainer != null)
		{
			AddProperty(_selectionContainer, "Status", "Nothing selected");
		}
		if (_previewContainer != null)
		{
			AddProperty(_previewContainer, "Status", "Select a star to preview");
		}
	}

	private void ClearStarSelection()
	{
		_selectedStarSeed = 0;
		_selectedStarPosition = Vector3.Zero;
		_currentPreview = null;
		if (_openSystemButton != null)
		{
			_openSystemButton.Visible = false;
		}
	}

	private void OnOpenSystemPressed()
	{
		if (_selectedStarSeed != 0)
		{
			EmitSignal(SignalName.OpenSystemRequested, _selectedStarSeed, _selectedStarPosition);
		}
	}

	private void OnCalculateRoutesPressed()
	{
		if (!_isCalculating)
		{
			EmitSignal(SignalName.CalculateJumpRoutesRequested);
		}
	}

	private void OnShowRoutesToggled(bool enabled)
	{
		EmitSignal(SignalName.JumpRoutesVisibilityToggled, enabled);
	}

	private static VBoxContainer CreateSectionContainer(string name)
	{
		VBoxContainer container = new()
		{
			Name = name,
		};
		container.AddThemeConstantOverride("separation", 2);
		return container;
	}

	private void AddTitle(string text, int fontSize)
	{
		Label label = new()
		{
			Text = text,
		};
		label.AddThemeFontSizeOverride("font_size", fontSize);
		AddChild(label);
	}

	private void AddSectionLabel(string text)
	{
		Label label = new()
		{
			Text = text,
			Modulate = new Color(0.8f, 0.8f, 0.8f),
		};
		label.AddThemeFontSizeOverride("font_size", 12);
		AddChild(label);
	}

	private static void ClearContainer(VBoxContainer? container)
	{
		if (container == null)
		{
			return;
		}

		foreach (Node child in container.GetChildren())
		{
			child.QueueFree();
		}
	}

	private static void AddProperty(VBoxContainer container, string key, string value)
	{
		HBoxContainer row = new();

		Label keyLabel = new()
		{
			Name = "Key",
			Text = $"{key}:",
			Modulate = new Color(0.7f, 0.7f, 0.7f),
			CustomMinimumSize = new Vector2(100.0f, 0.0f),
		};
		keyLabel.AddThemeFontSizeOverride("font_size", 11);
		row.AddChild(keyLabel);

		Label valueLabel = new()
		{
			Name = "Value",
			Text = value,
		};
		valueLabel.AddThemeFontSizeOverride("font_size", 11);
		row.AddChild(valueLabel);

		container.AddChild(row);
	}

	private static string GetGalaxyTypeName(GalaxySpec.GalaxyType galaxyType)
	{
		return galaxyType switch
		{
			GalaxySpec.GalaxyType.Spiral => "Spiral",
			GalaxySpec.GalaxyType.Elliptical => "Elliptical",
			GalaxySpec.GalaxyType.Irregular => "Irregular",
			_ => "Unknown",
		};
	}

	private static string GetZoomLevelName(int zoomLevel)
	{
		return zoomLevel switch
		{
			(int)GalaxyCoordinates.ZoomLevel.Galaxy => "Galaxy",
			(int)GalaxyCoordinates.ZoomLevel.Quadrant => "Quadrant",
			(int)GalaxyCoordinates.ZoomLevel.Sector => "Sector",
			(int)GalaxyCoordinates.ZoomLevel.Subsector => "Star Field",
			_ => "Unknown",
		};
	}

	private static string FormatVector3I(Vector3I value)
	{
		return $"({value.X}, {value.Y}, {value.Z})";
	}

	private static GalaxySpec? ConvertVariantToGalaxySpec(Variant specVariant)
	{
		if (specVariant.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = specVariant.AsGodotObject();
		if (godotObject is GalaxySpec typedSpec)
		{
			return typedSpec;
		}

		if (godotObject != null && godotObject.HasMethod("to_dict"))
		{
			Variant dictVariant = godotObject.Call("to_dict");
			if (dictVariant.VariantType == Variant.Type.Dictionary)
			{
				return GalaxySpec.FromDictionary((Godot.Collections.Dictionary)dictVariant);
			}
		}

		return null;
	}

	private static StarSystemPreviewData? ConvertVariantToPreviewData(Variant previewVariant)
	{
		if (previewVariant.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = previewVariant.AsGodotObject();
		if (godotObject is StarSystemPreviewData typedPreview)
		{
			return typedPreview;
		}

		if (godotObject == null)
		{
			return null;
		}

		StarSystemPreviewData preview = new();
		preview.StarSeed = GetIntProperty(godotObject, "star_seed", 0);
		preview.WorldPosition = GetVector3Property(godotObject, "world_position", Vector3.Zero);
		preview.StarCount = GetIntProperty(godotObject, "star_count", 0);
		preview.SpectralClasses = GetStringArrayProperty(godotObject, "spectral_classes");
		preview.StarTemperatures = GetFloatArrayProperty(godotObject, "star_temperatures");
		preview.PlanetCount = GetIntProperty(godotObject, "planet_count", 0);
		preview.MoonCount = GetIntProperty(godotObject, "moon_count", 0);
		preview.BeltCount = GetIntProperty(godotObject, "belt_count", 0);
		preview.Metallicity = GetDoubleProperty(godotObject, "metallicity", 1.0);
		preview.TotalPopulation = GetIntProperty(godotObject, "total_population", 0);
		preview.IsInhabited = GetBoolProperty(godotObject, "is_inhabited", false);
		return preview;
	}

	private static int GetIntProperty(GodotObject source, string propertyName, int fallback)
	{
		Variant value = source.Get(propertyName);
		return value.VariantType switch
		{
			Variant.Type.Int => (int)value,
			Variant.Type.Float => (int)(double)value,
			_ => fallback,
		};
	}

	private static double GetDoubleProperty(GodotObject source, string propertyName, double fallback)
	{
		Variant value = source.Get(propertyName);
		return value.VariantType switch
		{
			Variant.Type.Float => (double)value,
			Variant.Type.Int => (int)value,
			_ => fallback,
		};
	}

	private static bool GetBoolProperty(GodotObject source, string propertyName, bool fallback)
	{
		Variant value = source.Get(propertyName);
		return value.VariantType == Variant.Type.Bool ? (bool)value : fallback;
	}

	private static Vector3 GetVector3Property(GodotObject source, string propertyName, Vector3 fallback)
	{
		Variant value = source.Get(propertyName);
		return value.VariantType == Variant.Type.Vector3 ? (Vector3)value : fallback;
	}

	private static string[] GetStringArrayProperty(GodotObject source, string propertyName)
	{
		Variant value = source.Get(propertyName);
		if (value.VariantType != Variant.Type.Array)
		{
			return global::System.Array.Empty<string>();
		}

		Godot.Collections.Array array = (Godot.Collections.Array)value;
		string[] result = new string[array.Count];
		for (int index = 0; index < array.Count; index++)
		{
			Variant item = array[index];
			result[index] = item.VariantType == Variant.Type.String ? (string)item : "?";
		}

		return result;
	}

	private static float[] GetFloatArrayProperty(GodotObject source, string propertyName)
	{
		Variant value = source.Get(propertyName);
		if (value.VariantType != Variant.Type.Array)
		{
			return global::System.Array.Empty<float>();
		}

		Godot.Collections.Array array = (Godot.Collections.Array)value;
		float[] result = new float[array.Count];
		for (int index = 0; index < array.Count; index++)
		{
			Variant item = array[index];
			result[index] = item.VariantType switch
			{
				Variant.Type.Float => (float)(double)item,
				Variant.Type.Int => (int)item,
				_ => 0.0f,
			};
		}

		return result;
	}
}
