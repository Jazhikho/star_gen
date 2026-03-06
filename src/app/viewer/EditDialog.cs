using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Editing;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.App.Rendering;
using StarGen.Services.Persistence;

namespace StarGen.App.Viewer;

/// <summary>
/// Dialog for editing celestial body properties.
/// Uses PropertyConstraintSolver to keep slider bounds physically consistent.
/// Locking a property re-runs the solver and narrows coupled properties.
/// The Traveller panel adds an optional UWP size code constraint layer.
/// </summary>
public partial class EditDialog : Window
{
	[Signal]
	public delegate void EditsConfirmedEventHandler(CelestialBody body);

	[Signal]
	public delegate void EditsCancelledEventHandler();

	[Signal]
	public delegate void BodyRegeneratedEventHandler(CelestialBody newBody);

	/// <summary>ParentContext to use when regenerating. Set by the caller (ObjectViewer) if the body was generated in a known context; null = use type default.</summary>
	public ParentContext? RegenerationContext { get; set; }

	private const float SectionColorR = 0.9f;
	private const float SectionColorG = 0.9f;
	private const float SectionColorB = 0.9f;
	private const float LabelColorR = 0.6f;
	private const float LabelColorG = 0.6f;
	private const float LabelColorB = 0.6f;
	private const float DerivedColorR = 0.5f;
	private const float DerivedColorG = 0.6f;
	private const float DerivedColorB = 0.7f;
	private const float WarningColorR = 0.9f;
	private const float WarningColorG = 0.6f;
	private const float WarningColorB = 0.2f;
	private const float LockedColorR = 0.9f;
	private const float LockedColorG = 0.7f;
	private const float LockedColorB = 0.3f;
	private const float LabelMinWidth = 120.0f;
	private const float DefaultDisplayStep = 0.001f;

	private static readonly System.Collections.Generic.Dictionary<string, double> DisplayFactors = new()
	{
		["physical.mass_kg"] = 0.0,
		["physical.radius_m"] = 0.0,
		["physical.rotation_period_s"] = 1.0 / 3600.0,
		["physical.axial_tilt_deg"] = 1.0,
		["physical.oblateness"] = 1.0,
		["stellar.temperature_k"] = 1.0,
		["stellar.luminosity_watts"] = 1.0 / 3.828e26,
		["stellar.age_years"] = 1e-9,
		["stellar.metallicity"] = 1.0,
		["orbital.semi_major_axis_m"] = 1.0 / 1.496e11,
		["orbital.eccentricity"] = 1.0,
		["orbital.inclination_deg"] = 1.0,
		["atmosphere.surface_pressure_pa"] = 1.0 / 101325.0,
		["atmosphere.scale_height_m"] = 1e-3,
		["atmosphere.greenhouse_factor"] = 1.0,
		["surface.temperature_k"] = 1.0,
		["surface.albedo"] = 1.0,
		["surface.volcanism_level"] = 1.0,
	};

	private static readonly System.Collections.Generic.Dictionary<string, string> Suffixes = new()
	{
		["physical.rotation_period_s"] = " hrs",
		["physical.axial_tilt_deg"] = "\u00B0",
		["physical.oblateness"] = "",
		["stellar.temperature_k"] = " K",
		["stellar.luminosity_watts"] = " L\u2609",
		["stellar.age_years"] = " Gyr",
		["stellar.metallicity"] = "",
		["orbital.semi_major_axis_m"] = " AU",
		["orbital.eccentricity"] = "",
		["orbital.inclination_deg"] = "\u00B0",
		["atmosphere.surface_pressure_pa"] = " atm",
		["atmosphere.scale_height_m"] = " km",
		["atmosphere.greenhouse_factor"] = "x",
		["surface.temperature_k"] = " K",
		["surface.albedo"] = "",
		["surface.volcanism_level"] = "",
	};

	private static readonly System.Collections.Generic.Dictionary<string, double> Steps = new()
	{
		["physical.mass_kg"] = 0.0001,
		["physical.radius_m"] = 0.001,
		["physical.rotation_period_s"] = 0.1,
		["physical.axial_tilt_deg"] = 0.1,
		["physical.oblateness"] = 0.001,
		["stellar.temperature_k"] = 10.0,
		["stellar.luminosity_watts"] = 0.0001,
		["stellar.age_years"] = 0.001,
		["stellar.metallicity"] = 0.001,
		["orbital.semi_major_axis_m"] = 0.001,
		["orbital.eccentricity"] = 0.001,
		["orbital.inclination_deg"] = 0.1,
		["atmosphere.surface_pressure_pa"] = 0.001,
		["atmosphere.scale_height_m"] = 0.1,
		["atmosphere.greenhouse_factor"] = 0.01,
		["surface.temperature_k"] = 1.0,
		["surface.albedo"] = 0.001,
		["surface.volcanism_level"] = 0.01,
	};

	private CelestialBody? _body;
	private Dictionary _originalValues = new();
	private Dictionary _workingValues = new();
	private List<string> _lockedPaths = new();
	private Variant _travellerCode;
	private ConstraintSet? _constraints;
	private Dictionary _propertyEditors = new();
	private Dictionary _derivedLabels = new();
	private OptionButton? _travellerOption;
	private Button? _travellerApply;
	private Button? _travellerClear;
	private FileDialog? _saveDialog;
	private Button? _regenerateBtn;
	private Button? _saveBtn;

	private VBoxContainer? _content;
	private Camera3D? _previewCamera;
	private BodyRenderer? _previewBodyRenderer;
	private DirectionalLight3D? _previewLight;
	private Button? _revertButton;
	private Button? _confirmButton;
	private Button? _cancelButton;
	private VBoxContainer? _currentSectionContent;

	public override void _Ready()
	{
		CloseRequested += OnCancelPressed;
		_content = GetNodeOrNull<VBoxContainer>("MarginContainer/VBoxContainer/ContentSplit/ScrollContainer/ContentMargin/ContentContainer");
		_previewCamera = GetNodeOrNull<Camera3D>("MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/Camera3D");
		_previewBodyRenderer = GetNodeOrNull<BodyRenderer>("MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/BodyRenderer");
		Node3D? previewEnv = GetNodeOrNull<Node3D>("MarginContainer/VBoxContainer/ContentSplit/PreviewContainer/SubViewportContainer/SubViewport/PreviewEnvironment");
		_previewLight = previewEnv?.GetNodeOrNull<DirectionalLight3D>("DirectionalLight3D");
		_revertButton = GetNodeOrNull<Button>("MarginContainer/VBoxContainer/ButtonContainer/RevertButton");
		_confirmButton = GetNodeOrNull<Button>("MarginContainer/VBoxContainer/ButtonContainer/ConfirmButton");
		_cancelButton = GetNodeOrNull<Button>("MarginContainer/VBoxContainer/ButtonContainer/CancelButton");

		if (_revertButton != null)
			_revertButton.Pressed += OnRevertPressed;
		if (_confirmButton != null)
			_confirmButton.Pressed += OnConfirmPressed;
		if (_cancelButton != null)
			_cancelButton.Pressed += OnCancelPressed;
		SetupExtraButtons();
	}

	/// <summary>Opens the dialog for editing the given body.</summary>
	public void OpenForBody(CelestialBody body)
	{
		if (body == null)
			return;
		_body = body;
		string displayName;
		if (string.IsNullOrEmpty(body.Name))
		{
			displayName = body.Id;
		}
		else
		{
			displayName = body.Name;
		}

		Title = "Edit: " + displayName;
		_lockedPaths.Clear();
		_travellerCode = default;
		ExtractValuesFromBody();
		_originalValues = (Dictionary)_workingValues.Duplicate(true);
		Resolve();
		BuildEditorUi();
		UpdatePreview();
		PopupCentered();
	}

	private void Resolve()
	{
		if (_body == null)
			return;
		Dictionary extra = new();
		if (_travellerCode.VariantType != Variant.Type.Nil)
			extra = TravellerConstraintBuilder.BuildConstraintsForSize(_travellerCode);
		_constraints = PropertyConstraintSolver.SolveWithExtraConstraints(
			_body.Type, _workingValues, _lockedPaths, extra);
	}

	private void ExtractValuesFromBody()
	{
		_workingValues.Clear();
		if (_body == null)
			return;
		if (string.IsNullOrEmpty(_body.Name))
		{
			_workingValues["name"] = _body.Id;
		}
		else
		{
			_workingValues["name"] = _body.Name;
		}
		PhysicalProps p = _body.Physical;
		_workingValues["physical.mass_kg"] = p.MassKg;
		_workingValues["physical.radius_m"] = p.RadiusM;
		_workingValues["physical.rotation_period_s"] = Math.Abs(p.RotationPeriodS);
		_workingValues["physical.axial_tilt_deg"] = p.AxialTiltDeg;
		_workingValues["physical.oblateness"] = p.Oblateness;
		if (_body.HasStellar() && _body.Stellar != null)
		{
			StellarProps s = _body.Stellar;
			_workingValues["stellar.temperature_k"] = s.EffectiveTemperatureK;
			_workingValues["stellar.luminosity_watts"] = s.LuminosityWatts;
			_workingValues["stellar.age_years"] = s.AgeYears;
			_workingValues["stellar.metallicity"] = s.Metallicity;
		}
		if (_body.HasOrbital() && _body.Orbital != null)
		{
			OrbitalProps o = _body.Orbital;
			_workingValues["orbital.semi_major_axis_m"] = o.SemiMajorAxisM;
			_workingValues["orbital.eccentricity"] = o.Eccentricity;
			_workingValues["orbital.inclination_deg"] = o.InclinationDeg;
		}
		if (_body.HasAtmosphere() && _body.Atmosphere != null)
		{
			AtmosphereProps a = _body.Atmosphere;
			_workingValues["atmosphere.surface_pressure_pa"] = a.SurfacePressurePa;
			_workingValues["atmosphere.scale_height_m"] = a.ScaleHeightM;
			_workingValues["atmosphere.greenhouse_factor"] = a.GreenhouseFactor;
		}
		if (_body.HasSurface() && _body.Surface != null)
		{
			SurfaceProps sf = _body.Surface;
			_workingValues["surface.temperature_k"] = sf.TemperatureK;
			_workingValues["surface.albedo"] = sf.Albedo;
			_workingValues["surface.volcanism_level"] = sf.VolcanismLevel;
		}
	}

	private void ApplyValuesToBody()
	{
		if (_body == null)
			return;
		_body.Name = DictGetString(_workingValues, "name", _body.Name);
		PhysicalProps p = _body.Physical;
		p.MassKg = Wv("physical.mass_kg", p.MassKg);
		p.RadiusM = Wv("physical.radius_m", p.RadiusM);
		double retro = 1.0;
		if (p.RotationPeriodS < 0.0)
			retro = -1.0;
		p.RotationPeriodS = Wv("physical.rotation_period_s", Math.Abs(p.RotationPeriodS)) * retro;
		p.AxialTiltDeg = Wv("physical.axial_tilt_deg", p.AxialTiltDeg);
		p.Oblateness = Wv("physical.oblateness", p.Oblateness);
		if (_body.HasStellar() && _body.Stellar != null)
		{
			StellarProps s = _body.Stellar;
			s.EffectiveTemperatureK = Wv("stellar.temperature_k", s.EffectiveTemperatureK);
			s.LuminosityWatts = Wv("stellar.luminosity_watts", s.LuminosityWatts);
			s.AgeYears = Wv("stellar.age_years", s.AgeYears);
			s.Metallicity = Wv("stellar.metallicity", s.Metallicity);
		}
		if (_body.HasOrbital() && _body.Orbital != null)
		{
			OrbitalProps o = _body.Orbital;
			o.SemiMajorAxisM = Wv("orbital.semi_major_axis_m", o.SemiMajorAxisM);
			o.Eccentricity = Wv("orbital.eccentricity", o.Eccentricity);
			o.InclinationDeg = Wv("orbital.inclination_deg", o.InclinationDeg);
		}
		if (_body.HasAtmosphere() && _body.Atmosphere != null)
		{
			AtmosphereProps a = _body.Atmosphere;
			a.SurfacePressurePa = Wv("atmosphere.surface_pressure_pa", a.SurfacePressurePa);
			a.ScaleHeightM = Wv("atmosphere.scale_height_m", a.ScaleHeightM);
			a.GreenhouseFactor = Wv("atmosphere.greenhouse_factor", a.GreenhouseFactor);
		}
		if (_body.HasSurface() && _body.Surface != null)
		{
			SurfaceProps sf = _body.Surface;
			sf.TemperatureK = Wv("surface.temperature_k", sf.TemperatureK);
			sf.Albedo = Wv("surface.albedo", sf.Albedo);
			sf.VolcanismLevel = Wv("surface.volcanism_level", sf.VolcanismLevel);
		}
	}

	private double Wv(string path, double fallback)
	{
		if (_workingValues.ContainsKey(path))
			return _workingValues[path].AsDouble();
		return fallback;
	}

	private static string DictGetString(Dictionary d, string key, string fallback)
	{
		if (d.TryGetValue(key, out Variant v))
			return v.AsString();
		return fallback;
	}

	private void ClearContent()
	{
		_propertyEditors.Clear();
		_derivedLabels.Clear();
		if (_content == null)
			return;
		foreach (Node child in _content.GetChildren())
			child.QueueFree();
		_currentSectionContent = null;
		_travellerOption = null;
		_travellerApply = null;
		_travellerClear = null;
	}

	private void BuildEditorUi()
	{
		ClearContent();
		if (_body == null || _constraints == null)
			return;
		AddSection("Basic Info");
		AddNameEditor();
		AddDerivedRow("Type", _body.GetTypeString());
		AddDerivedRow("ID", _body.Id);
		if (_body.Type == CelestialType.Type.Planet || _body.Type == CelestialType.Type.Moon)
		{
			AddSection("Traveller UWP");
			AddTravellerPanel();
		}
		AddSection("Physical Properties");
		AddNumericEditor("physical.mass_kg", "Mass");
		AddNumericEditor("physical.radius_m", "Radius");
		AddDerivedRow("Density", FmtDensity(), "Density");
		AddDerivedRow("Surface Gravity", FmtGravity(), "SurfaceGravity");
		AddDerivedRow("Escape Velocity", FmtEscape(), "EscapeVel");
		AddNumericEditor("physical.rotation_period_s", "Rotation Period");
		AddNumericEditor("physical.axial_tilt_deg", "Axial Tilt");
		AddNumericEditor("physical.oblateness", "Oblateness");
		if (_body.HasStellar())
		{
			AddSection("Stellar Properties");
			AddNumericEditor("stellar.temperature_k", "Temperature");
			AddNumericEditor("stellar.luminosity_watts", "Luminosity");
			AddNumericEditor("stellar.age_years", "Age");
			AddNumericEditor("stellar.metallicity", "Metallicity");
		}
		if (_body.HasOrbital() && _body.Orbital != null)
		{
			AddSection("Orbital Properties");
			AddNumericEditor("orbital.semi_major_axis_m", "Semi-major Axis");
			AddNumericEditor("orbital.eccentricity", "Eccentricity");
			AddNumericEditor("orbital.inclination_deg", "Inclination");
			AddDerivedRow("Periapsis", FmtPeriapsis(), "Periapsis");
			AddDerivedRow("Apoapsis", FmtApoapsis(), "Apoapsis");
		}
		if (_body.HasAtmosphere())
		{
			AddSection("Atmosphere");
			AddNumericEditor("atmosphere.surface_pressure_pa", "Surface Pressure");
			AddNumericEditor("atmosphere.scale_height_m", "Scale Height");
			AddNumericEditor("atmosphere.greenhouse_factor", "Greenhouse Factor");
		}
		if (_body.HasSurface())
		{
			AddSection("Surface");
			AddNumericEditor("surface.temperature_k", "Temperature");
			AddNumericEditor("surface.albedo", "Albedo");
			AddNumericEditor("surface.volcanism_level", "Volcanism Level");
		}
	}

	private void AddSection(string titleText)
	{
		if (_content == null)
			return;
		VBoxContainer section = new VBoxContainer();
		section.AddThemeConstantOverride("separation", 5);
		Label header = new Label { Text = titleText };
		header.AddThemeFontSizeOverride("font_size", 16);
		header.AddThemeColorOverride("font_color", new Color(SectionColorR, SectionColorG, SectionColorB));
		section.AddChild(header);
		section.AddChild(new HSeparator());
		VBoxContainer content = new VBoxContainer();
		content.AddThemeConstantOverride("separation", 12);
		section.AddChild(content);
		_content.AddChild(section);
		_currentSectionContent = content;
	}

	private void AddNameEditor()
	{
		if (_currentSectionContent == null)
		{
			return;
		}

		HBoxContainer row = new HBoxContainer();
		Label lbl = new Label { Text = "Name:" };
		lbl.CustomMinimumSize = new Vector2(LabelMinWidth, 0);
		lbl.AddThemeColorOverride("font_color", new Color(LabelColorR, LabelColorG, LabelColorB));
		row.AddChild(lbl);
		LineEdit edit = new LineEdit();
		edit.Text = DictGetString(_workingValues, "name", "");
		edit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		edit.TextChanged += t =>
		{
			_workingValues["name"] = t;
			ApplyValuesToBody();
		};
		row.AddChild(edit);
		_currentSectionContent.AddChild(row);
	}

	private void AddNumericEditor(string propertyPath, string labelText)
	{
		if (_currentSectionContent == null || _constraints == null)
			return;
		if (!_constraints.HasConstraint(propertyPath))
			return;
		VBoxContainer container = new VBoxContainer();
		container.AddThemeConstantOverride("separation", 4);
		Label lbl = new Label { Text = labelText };
		lbl.AddThemeColorOverride("font_color", new Color(LabelColorR, LabelColorG, LabelColorB));
		lbl.AddThemeFontSizeOverride("font_size", 13);
		container.AddChild(lbl);
		HBoxContainer controls = new HBoxContainer();
		controls.AddThemeConstantOverride("separation", 8);
		CheckButton lockBtn = new CheckButton { Text = "Lock", TooltipText = "Lock this property (constrains dependent properties)" };
		lockBtn.CustomMinimumSize = new Vector2(50, 0);
		lockBtn.ButtonPressed = _lockedPaths.Contains(propertyPath);
		string pathCapture = propertyPath;
		lockBtn.Toggled += pressed => OnLockToggled(pressed, pathCapture);
		controls.AddChild(lockBtn);
		Vector2 rangeBase = _constraints.GetRange(propertyPath, Vector2.Zero);
		double factor = DisplayFactorFor(propertyPath);
		float dispMin = (float)(rangeBase.X * factor);
		float dispMax = (float)(rangeBase.Y * factor);
		float dispVal = (float)(Wv(propertyPath, 0.0) * factor);
		float step;
		if (Steps.TryGetValue(propertyPath, out double st))
		{
			step = (float)st;
		}
		else
		{
			step = (float)DefaultDisplayStep;
		}
		string suffix = SuffixFor(propertyPath);
		HSlider slider = new HSlider { MinValue = dispMin, MaxValue = dispMax, Step = step, Value = dispVal };
		slider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		slider.CustomMinimumSize = new Vector2(100, 0);
		controls.AddChild(slider);
		SpinBox spin = new SpinBox { MinValue = dispMin, MaxValue = dispMax, Step = step, Value = dispVal, Suffix = suffix };
		spin.CustomMinimumSize = new Vector2(140, 0);
		spin.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		controls.AddChild(spin);
		container.AddChild(controls);
		Label rangeLabel = new Label();
		rangeLabel.AddThemeFontSizeOverride("font_size", 10);
		rangeLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
		rangeLabel.Text = FmtRangeLabel(propertyPath, rangeBase);
		container.AddChild(rangeLabel);
		_currentSectionContent.AddChild(container);
		slider.ValueChanged += v => OnSliderChanged(v, propertyPath, spin);
		spin.ValueChanged += v => OnSpinChanged(v, propertyPath, slider);
		var edDict = new Dictionary();
		edDict["slider"] = slider;
		edDict["spinbox"] = spin;
		edDict["lock_btn"] = lockBtn;
		edDict["range_label"] = rangeLabel;
		_propertyEditors[propertyPath] = edDict;
	}

	private void AddDerivedRow(string labelText, string valueText, string trackKey = "")
	{
		if (_currentSectionContent == null)
		{
			return;
		}

		HBoxContainer row = new HBoxContainer();
		Label lbl = new Label { Text = labelText + ":" };
		lbl.CustomMinimumSize = new Vector2(LabelMinWidth, 0);
		lbl.AddThemeColorOverride("font_color", new Color(LabelColorR, LabelColorG, LabelColorB));
		lbl.AddThemeFontSizeOverride("font_size", 12);
		row.AddChild(lbl);
		Label val = new Label { Text = valueText };
		val.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		val.AddThemeColorOverride("font_color", new Color(DerivedColorR, DerivedColorG, DerivedColorB));
		val.AddThemeFontSizeOverride("font_size", 12);
		row.AddChild(val);
		Label tag = new Label { Text = "(derived)" };
		tag.AddThemeFontSizeOverride("font_size", 10);
		tag.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.5f));
		row.AddChild(tag);
		_currentSectionContent.AddChild(row);
		if (!string.IsNullOrEmpty(trackKey))
			_derivedLabels[trackKey] = val;
	}

	private void AddTravellerPanel()
	{
		if (_currentSectionContent == null)
			return;
		HBoxContainer row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 8);
		Label lbl = new Label { Text = "Size Code:" };
		lbl.CustomMinimumSize = new Vector2(LabelMinWidth, 0);
		lbl.AddThemeColorOverride("font_color", new Color(LabelColorR, LabelColorG, LabelColorB));
		row.AddChild(lbl);
		_travellerOption = new OptionButton();
		_travellerOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		Godot.Collections.Array codes = TravellerConstraintBuilder.AllCodes();
		Variant currentCode = TravellerConstraintBuilder.CodeForRadius(Wv("physical.radius_m", 6.371e6));
		int selIdx = 0;
		for (int i = 0; i < codes.Count; i++)
		{
			Variant code = codes[i];
			_travellerOption.AddItem(TravellerConstraintBuilder.DescribeCode(code), i);
			_travellerOption.SetItemMetadata(i, code);
			if (code.ToString() == currentCode.ToString())
				selIdx = i;
		}
		_travellerOption.Selected = selIdx;
		row.AddChild(_travellerOption);
		_travellerApply = new Button { Text = "Apply", TooltipText = "Constrain radius and mass to this Traveller size code" };
		_travellerApply.Pressed += OnTravellerApply;
		row.AddChild(_travellerApply);
		_travellerClear = new Button { Text = "Clear", TooltipText = "Remove Traveller constraint" };
		_travellerClear.Disabled = _travellerCode.VariantType == Variant.Type.Nil;
		_travellerClear.Pressed += OnTravellerClear;
		row.AddChild(_travellerClear);
		_currentSectionContent.AddChild(row);
		Label status = new Label();
		status.AddThemeFontSizeOverride("font_size", 10);
		status.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
		status.Text = TravellerStatusText();
		_currentSectionContent.AddChild(status);
		_derivedLabels["TravellerStatus"] = status;
	}

	private void OnLockToggled(bool pressed, string propertyPath)
	{
		if (pressed)
		{
			if (!_lockedPaths.Contains(propertyPath))
				_lockedPaths.Add(propertyPath);
		}
		else
			_lockedPaths.Remove(propertyPath);
		Resolve();
		if (_constraints != null)
		{
			List<string> clamped = _constraints.ClampUnlocked();
			foreach (string path in clamped)
			{
				PropertyConstraint? c = _constraints.GetConstraint(path);
				if (c != null)
					_workingValues[path] = c.CurrentValue;
			}
		}
		RefreshAllEditorBounds();
		ApplyValuesToBody();
		UpdatePreview();
		UpdateDerivedLabels();
	}

	private void OnSliderChanged(double dispValue, string propertyPath, SpinBox spin)
	{
		spin.SetValueNoSignal(dispValue);
		CommitDisplayValue(propertyPath, dispValue);
	}

	private void OnSpinChanged(double dispValue, string propertyPath, HSlider slider)
	{
		slider.SetValueNoSignal(dispValue);
		CommitDisplayValue(propertyPath, dispValue);
	}

	private void CommitDisplayValue(string propertyPath, double dispValue)
	{
		double factor = DisplayFactorFor(propertyPath);
		_workingValues[propertyPath] = dispValue / factor;
		Resolve();
		RefreshAllEditorBounds();
		ApplyValuesToBody();
		UpdatePreview();
		UpdateDerivedLabels();
	}

	private void OnTravellerApply()
	{
		if (_travellerOption == null)
			return;
		int idx = _travellerOption.Selected;
		if (idx < 0)
			return;
		_travellerCode = _travellerOption.GetItemMetadata(idx);
		Resolve();
		if (_constraints != null)
		{
			List<string> clamped = _constraints.ClampUnlocked();
			foreach (string path in clamped)
			{
				PropertyConstraint? c = _constraints.GetConstraint(path);
				if (c != null)
					_workingValues[path] = c.CurrentValue;
			}
		}
		RefreshAllEditorBounds();
		ApplyValuesToBody();
		UpdatePreview();
		UpdateDerivedLabels();
		if (_travellerClear != null)
			_travellerClear.Disabled = false;
	}

	private void OnTravellerClear()
	{
		_travellerCode = default;
		Resolve();
		RefreshAllEditorBounds();
		UpdateDerivedLabels();
		if (_travellerClear != null)
			_travellerClear.Disabled = true;
	}

	private void RefreshAllEditorBounds()
	{
		if (_constraints == null)
			return;
		foreach (Variant key in _propertyEditors.Keys)
		{
			string pathStr = key.AsString();
			Variant edVal = _propertyEditors[key];
			Dictionary ed = edVal.As<Dictionary>();
			PropertyConstraint? c = _constraints.GetConstraint(pathStr);
			if (c == null)
				continue;
			double factor = DisplayFactorFor(pathStr);
			if (!ed.ContainsKey("slider") || !ed.ContainsKey("spinbox") || !ed.ContainsKey("lock_btn") || !ed.ContainsKey("range_label"))
				continue;
			HSlider? slider = ed["slider"].As<HSlider>();
			SpinBox? spin = ed["spinbox"].As<SpinBox>();
			Label? rangeLabel = ed["range_label"].As<Label>();
			CheckButton? lockBtn = ed["lock_btn"].As<CheckButton>();
			if (slider == null || spin == null || rangeLabel == null || lockBtn == null)
				continue;
			float dispMin = (float)(c.MinValue * factor);
			float dispMax = (float)(c.MaxValue * factor);
			float dispVal = (float)(Wv(pathStr, c.CurrentValue) * factor);
			slider.MinValue = dispMin;
			slider.MaxValue = dispMax;
			slider.SetValueNoSignal(dispVal);
			spin.MinValue = dispMin;
			spin.MaxValue = dispMax;
			spin.SetValueNoSignal(dispVal);
			slider.Editable = !c.IsLocked;
			spin.Editable = !c.IsLocked;
			lockBtn.SetPressedNoSignal(c.IsLocked);
			if (c.IsLocked)
				lockBtn.AddThemeColorOverride("font_color", new Color(LockedColorR, LockedColorG, LockedColorB));
			else
				lockBtn.RemoveThemeColorOverride("font_color");
			rangeLabel.Text = FmtRangeLabel(pathStr, new Vector2((float)c.MinValue, (float)c.MaxValue));
			if (c.IsValueInRange())
			{
				rangeLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			}
			else
			{
				rangeLabel.AddThemeColorOverride("font_color", new Color(WarningColorR, WarningColorG, WarningColorB));
			}
		}
	}

	private void UpdateDerivedLabels()
	{
		if (_body == null)
			return;
		if (_derivedLabels.ContainsKey("Density"))
			(_derivedLabels["Density"].As<Label>())!.Text = FmtDensity();
		if (_derivedLabels.ContainsKey("SurfaceGravity"))
			(_derivedLabels["SurfaceGravity"].As<Label>())!.Text = FmtGravity();
		if (_derivedLabels.ContainsKey("EscapeVel"))
			(_derivedLabels["EscapeVel"].As<Label>())!.Text = FmtEscape();
		if (_body.HasOrbital())
		{
			if (_derivedLabels.ContainsKey("Periapsis"))
				(_derivedLabels["Periapsis"].As<Label>())!.Text = FmtPeriapsis();
			if (_derivedLabels.ContainsKey("Apoapsis"))
				(_derivedLabels["Apoapsis"].As<Label>())!.Text = FmtApoapsis();
		}
		if (_derivedLabels.ContainsKey("TravellerStatus"))
			(_derivedLabels["TravellerStatus"].As<Label>())!.Text = TravellerStatusText();
	}

	private void UpdatePreview()
	{
		if (_body == null || _previewBodyRenderer == null)
			return;
		float scaleFactor = CalculateDisplayScale();
		_previewBodyRenderer.RenderBody(_body, scaleFactor);
		if (_previewCamera != null)
		{
			float camDist = Math.Clamp(scaleFactor * 3.5f, 2.0f, 30.0f);
			_previewCamera.Position = new Vector3(0, 0, camDist);
		}
		AdjustPreviewLighting();
	}

	private float CalculateDisplayScale()
	{
		if (_body == null)
			return 1.0f;
		double r = _body.Physical.RadiusM;
		switch (_body.Type)
		{
			case CelestialType.Type.Star:
				return (float)Math.Clamp(r / Units.SolarRadiusMeters, 0.5, 3.0);
			case CelestialType.Type.Planet:
				return (float)Math.Clamp(r / Units.EarthRadiusMeters, 0.2, 2.5);
			case CelestialType.Type.Moon:
				return (float)Math.Clamp(r / Units.EarthRadiusMeters * 2.0, 0.2, 2.0);
			case CelestialType.Type.Asteroid:
				double km = r / 1000.0;
				return (float)Math.Clamp(0.5 + km / 100.0, 0.5, 1.5);
			default:
				return 1.0f;
		}
	}

	private void AdjustPreviewLighting()
	{
		if (_previewLight == null || _body == null)
			return;
		switch (_body.Type)
		{
			case CelestialType.Type.Star:
				_previewLight.LightEnergy = 0.1f;
				break;
			case CelestialType.Type.Asteroid:
				_previewLight.LightEnergy = 1.2f;
				break;
			default:
				_previewLight.LightEnergy = 0.8f;
				break;
		}
	}

	private void OnRevertPressed()
	{
		_workingValues = (Dictionary)_originalValues.Duplicate(true);
		_lockedPaths.Clear();
		_travellerCode = default;
		ApplyValuesToBody();
		Resolve();
		BuildEditorUi();
		UpdatePreview();
	}

	private void OnConfirmPressed()
	{
		if (_body != null)
			EmitSignal(SignalName.EditsConfirmed, _body);
		Hide();
	}

	private void OnCancelPressed()
	{
		_workingValues = (Dictionary)_originalValues.Duplicate(true);
		ApplyValuesToBody();
		EmitSignal(SignalName.EditsCancelled);
		Hide();
	}

	private void SetupExtraButtons()
	{
		HBoxContainer? btnBox = GetNodeOrNull<HBoxContainer>("MarginContainer/VBoxContainer/ButtonContainer");
		if (btnBox == null)
			return;
		_regenerateBtn = new Button
		{
			Text = "Regenerate Unlocked",
			TooltipText = "Re-roll unlocked properties; locked ones stay fixed",
			CustomMinimumSize = new Vector2(160, 35),
		};
		_regenerateBtn.Pressed += OnRegeneratePressed;
		btnBox.AddChild(_regenerateBtn);
		btnBox.MoveChild(_regenerateBtn, 0);
		_saveBtn = new Button
		{
			Text = "Save As\u2026",
			TooltipText = "Save this edited body to a file",
			CustomMinimumSize = new Vector2(100, 35),
		};
		_saveBtn.Pressed += OnSavePressed;
		btnBox.AddChild(_saveBtn);
		btnBox.MoveChild(_saveBtn, 1);
	}

	private void OnRegeneratePressed()
	{
		if (_body == null || _constraints == null)
			return;
		foreach (string path in _lockedPaths)
		{
			if (_workingValues.ContainsKey(path))
				_constraints.SetValue(path, _workingValues[path].AsDouble());
		}
		int seedVal = (int)GD.Randi();
		RegenerateResult result = EditRegenerator.Regenerate(_body.Type, _constraints, seedVal, RegenerationContext);
		if (!result.Success)
		{
			Title = "Regeneration failed: " + result.ErrorMessage;
			return;
		}
		string preservedName = DictGetString(_workingValues, "name", "");
		if (!string.IsNullOrEmpty(preservedName))
			result.Body!.Name = preservedName;
		List<string> keepLocks = new List<string>(_lockedPaths);
		Variant keepTraveller = _travellerCode;
		_body = result.Body;
		ExtractValuesFromBody();
		_originalValues = (Dictionary)_workingValues.Duplicate(true);
		_lockedPaths = keepLocks;
		_travellerCode = keepTraveller;
		Resolve();
		BuildEditorUi();
		UpdatePreview();
		string regenDisplayName;
		if (string.IsNullOrEmpty(_body!.Name))
		{
			regenDisplayName = _body.Id;
		}
		else
		{
			regenDisplayName = _body.Name;
		}

		Title = "Edit: " + regenDisplayName + " (regenerated)";
		EmitSignal(SignalName.BodyRegenerated, _body);
	}

	private void OnSavePressed()
	{
		if (_body == null)
			return;
		if (_saveDialog == null)
		{
			_saveDialog = new FileDialog
			{
				Title = "Save Edited Body",
				FileMode = FileDialog.FileModeEnum.SaveFile,
				Access = FileDialog.AccessEnum.Filesystem,
			};
			_saveDialog.Filters = new string[] { "*.sgb ; StarGen Binary", "*.json ; JSON" };
			_saveDialog.CurrentDir = OS.GetUserDataDir();
			_saveDialog.FileSelected += OnSavePathSelected;
			AddChild(_saveDialog);
		}
		string rawName;
		if (string.IsNullOrEmpty(_body.Name))
		{
			rawName = "edited_body";
		}
		else
		{
			rawName = _body.Name;
		}

		string defaultName = rawName.Replace(" ", "_").ToLowerInvariant();
		_saveDialog.CurrentFile = defaultName + ".sgb";
		_saveDialog.PopupCentered(new Vector2I(600, 400));
	}

	private void OnSavePathSelected(string path)
	{
		ApplyValuesToBody();
		bool compress = path.EndsWith(".sgb");
		Error err = SaveData.SaveEditedBody(_body, path, compress);
		if (err != Error.Ok)
			Title = "Save failed: " + err.ToString();
		else
			Title = "Saved: " + System.IO.Path.GetFileName(path);
	}

	private double DisplayFactorFor(string propertyPath)
	{
		if (DisplayFactors.TryGetValue(propertyPath, out double f) && f != 0.0)
			return f;
		if (propertyPath == "physical.mass_kg" && _body != null)
		{
			switch (_body.Type)
			{
				case CelestialType.Type.Star: return 1.0 / Units.SolarMassKg;
				case CelestialType.Type.Planet:
				case CelestialType.Type.Moon: return 1.0 / Units.EarthMassKg;
				default: return 1e-15;
			}
		}
		if (propertyPath == "physical.radius_m" && _body != null)
		{
			switch (_body.Type)
			{
				case CelestialType.Type.Star: return 1.0 / Units.SolarRadiusMeters;
				case CelestialType.Type.Planet:
				case CelestialType.Type.Moon: return 1.0 / Units.EarthRadiusMeters;
				default: return 1e-3;
			}
		}
		return 1.0;
	}

	private string SuffixFor(string propertyPath)
	{
		if (Suffixes.TryGetValue(propertyPath, out string? s))
			return s;
		if (propertyPath == "physical.mass_kg" && _body != null)
		{
			switch (_body.Type)
			{
				case CelestialType.Type.Star: return " M\u2609";
				case CelestialType.Type.Planet:
				case CelestialType.Type.Moon: return " M\u2295";
				default: return " x10^15 kg";
			}
		}
		if (propertyPath == "physical.radius_m" && _body != null)
		{
			switch (_body.Type)
			{
				case CelestialType.Type.Star: return " R\u2609";
				case CelestialType.Type.Planet:
				case CelestialType.Type.Moon: return " R\u2295";
				default: return " km";
			}
		}
		return "";
	}

	private string FmtRangeLabel(string propertyPath, Vector2 baseRange)
	{
		double factor = DisplayFactorFor(propertyPath);
		string suffix = SuffixFor(propertyPath);
		PropertyConstraint? c = _constraints?.GetConstraint(propertyPath);
		string reason = "";
		if (c != null && !string.IsNullOrEmpty(c.ConstraintReason))
		{
			reason = " [" + c.ConstraintReason + "]";
		}
		return string.Format(System.Globalization.CultureInfo.InvariantCulture,
			"Valid: {0} - {1}{2}{3}",
			(baseRange.X * factor).ToString(),
			(baseRange.Y * factor).ToString(),
			suffix,
			reason);
	}

	private string TravellerStatusText()
	{
		if (_travellerCode.VariantType == Variant.Type.Nil)
			return "No Traveller constraint active.";
		string uwp = TravellerSizeCode.ToStringUwp(_travellerCode);
		return "Active: size code " + uwp + " (locks radius and mass windows)";
	}

	private string FmtDensity() => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.1} kg/m^3", _body!.Physical.GetDensityKgM3());
	private string FmtGravity() => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} m/s^2", _body!.Physical.GetSurfaceGravityMS2());
	private string FmtEscape() => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} km/s", _body!.Physical.GetEscapeVelocityMS() / 1000.0);
	private string FmtPeriapsis() => PropertyFormatter.FormatDistance(_body!.Orbital!.GetPeriapsisM());
	private string FmtApoapsis() => PropertyFormatter.FormatDistance(_body!.Orbital!.GetApoapsisM());
}
