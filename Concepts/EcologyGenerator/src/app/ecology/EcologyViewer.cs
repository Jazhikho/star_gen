using Godot;
using System;
using StarGen.Domain.Ecology;

namespace StarGen.App.Ecology
{
	/// <summary>
	/// Main controller for the ecology viewer scene.
	/// Handles UI input and coordinates generation/rendering.
	/// </summary>
	public partial class EcologyViewer : Control
	{
		private LineEdit _seedInput = null!;
		private OptionButton _biomeOption = null!;
		private Button _generateButton = null!;

		private HSlider _tempMinSlider = null!;
		private HSlider _tempMaxSlider = null!;
		private HSlider _waterSlider = null!;
		private HSlider _lightSlider = null!;
		private HSlider _nutrientSlider = null!;
		private HSlider _gravitySlider = null!;
		private HSlider _radiationSlider = null!;
		private HSlider _oxygenSlider = null!;

		private Label _tempMinValue = null!;
		private Label _tempMaxValue = null!;
		private Label _waterValue = null!;
		private Label _lightValue = null!;
		private Label _nutrientValue = null!;
		private Label _gravityValue = null!;
		private Label _radiationValue = null!;
		private Label _oxygenValue = null!;

		private RichTextLabel _statsText = null!;
		private Label _statusLabel = null!;
		private EcologyWebRenderer _webRenderer = null!;

		private EcologyWeb? _currentWeb;

		/// <summary>
		/// Scene entry point. Caches node references and wires signals.
		/// </summary>
		public override void _Ready()
		{
			CacheNodeReferences();
			SetupBiomeOptions();
			ConnectSignals();
			RefreshAllValueLabels();
			UpdateStatus("Ready - adjust parameters and click Generate");
		}

		/// <summary>
		/// Retrieves and caches all required UI nodes from the scene tree.
		/// </summary>
		private void CacheNodeReferences()
		{
			_seedInput = GetNode<LineEdit>("VBoxContainer/TopPanel/SeedInput");
			_biomeOption = GetNode<OptionButton>("VBoxContainer/TopPanel/BiomeOption");
			_generateButton = GetNode<Button>("VBoxContainer/TopPanel/GenerateButton");

			string paramPath = "VBoxContainer/HSplitContainer/LeftPanel/LeftVBox/ParamScroll/ParamVBox";

			_tempMinSlider = GetNode<HSlider>(paramPath + "/TempMinRow/TempMinSlider");
			_tempMaxSlider = GetNode<HSlider>(paramPath + "/TempMaxRow/TempMaxSlider");
			_waterSlider = GetNode<HSlider>(paramPath + "/WaterRow/WaterSlider");
			_lightSlider = GetNode<HSlider>(paramPath + "/LightRow/LightSlider");
			_nutrientSlider = GetNode<HSlider>(paramPath + "/NutrientRow/NutrientSlider");
			_gravitySlider = GetNode<HSlider>(paramPath + "/GravityRow/GravitySlider");
			_radiationSlider = GetNode<HSlider>(paramPath + "/RadiationRow/RadiationSlider");
			_oxygenSlider = GetNode<HSlider>(paramPath + "/OxygenRow/OxygenSlider");

			_tempMinValue = GetNode<Label>(paramPath + "/TempMinRow/TempMinValue");
			_tempMaxValue = GetNode<Label>(paramPath + "/TempMaxRow/TempMaxValue");
			_waterValue = GetNode<Label>(paramPath + "/WaterRow/WaterValue");
			_lightValue = GetNode<Label>(paramPath + "/LightRow/LightValue");
			_nutrientValue = GetNode<Label>(paramPath + "/NutrientRow/NutrientValue");
			_gravityValue = GetNode<Label>(paramPath + "/GravityRow/GravityValue");
			_radiationValue = GetNode<Label>(paramPath + "/RadiationRow/RadiationValue");
			_oxygenValue = GetNode<Label>(paramPath + "/OxygenRow/OxygenValue");

			_statsText = GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/LeftPanel/LeftVBox/StatsText");
			_statusLabel = GetNode<Label>("VBoxContainer/BottomPanel/StatusLabel");
			_webRenderer = GetNode<EcologyWebRenderer>("VBoxContainer/HSplitContainer/WebRenderer");
		}

		/// <summary>
		/// Populates the biome option list from the BiomeType enum.
		/// </summary>
		private void SetupBiomeOptions()
		{
			_biomeOption.Clear();
			foreach (BiomeType biome in Enum.GetValues(typeof(BiomeType)))
			{
				_biomeOption.AddItem(biome.ToString());
			}

			_biomeOption.Selected = (int)BiomeType.Grassland;
		}

		/// <summary>
		/// Connects button and slider signals to their handlers.
		/// </summary>
		private void ConnectSignals()
		{
			_generateButton.Pressed += OnGeneratePressed;

			_tempMinSlider.ValueChanged += OnTempMinChanged;
			_tempMaxSlider.ValueChanged += OnTempMaxChanged;

			BindSliderLabel(_waterSlider, _waterValue, "F2");
			BindSliderLabel(_lightSlider, _lightValue, "F2");
			BindSliderLabel(_nutrientSlider, _nutrientValue, "F2");
			BindSliderLabel(_gravitySlider, _gravityValue, "F1");
			BindSliderLabel(_radiationSlider, _radiationValue, "F2");
			BindSliderLabel(_oxygenSlider, _oxygenValue, "F2");
		}

		/// <summary>
		/// Wires a slider's ValueChanged event to update a label with a formatted value.
		/// </summary>
		/// <param name="slider">The slider to observe.</param>
		/// <param name="label">The label to update.</param>
		/// <param name="format">Standard numeric format string.</param>
		private void BindSliderLabel(HSlider slider, Label label, string format)
		{
			Label target = label;
			string fmt = format;
			slider.ValueChanged += (double value) =>
			{
				target.Text = value.ToString(fmt);
			};
		}

		/// <summary>
		/// Keeps minimum temperature at or below maximum and updates its label.
		/// </summary>
		/// <param name="value">New minimum temperature value.</param>
		private void OnTempMinChanged(double value)
		{
			if (value > _tempMaxSlider.Value)
			{
				_tempMaxSlider.Value = value;
			}

			_tempMinValue.Text = value.ToString("F0");
		}

		/// <summary>
		/// Keeps maximum temperature at or above minimum and updates its label.
		/// </summary>
		/// <param name="value">New maximum temperature value.</param>
		private void OnTempMaxChanged(double value)
		{
			if (value < _tempMinSlider.Value)
			{
				_tempMinSlider.Value = value;
			}

			_tempMaxValue.Text = value.ToString("F0");
		}

		/// <summary>
		/// Synchronizes all value labels with their corresponding sliders.
		/// </summary>
		private void RefreshAllValueLabels()
		{
			_tempMinValue.Text = _tempMinSlider.Value.ToString("F0");
			_tempMaxValue.Text = _tempMaxSlider.Value.ToString("F0");
			_waterValue.Text = _waterSlider.Value.ToString("F2");
			_lightValue.Text = _lightSlider.Value.ToString("F2");
			_nutrientValue.Text = _nutrientSlider.Value.ToString("F2");
			_gravityValue.Text = _gravitySlider.Value.ToString("F1");
			_radiationValue.Text = _radiationSlider.Value.ToString("F2");
			_oxygenValue.Text = _oxygenSlider.Value.ToString("F2");
		}

		/// <summary>
		/// Handles the Generate button press and drives ecology generation and rendering.
		/// </summary>
		private void OnGeneratePressed()
		{
			try
			{
				EnvironmentSpec spec = BuildSpecFromUI();
				EcologyRng rng = new EcologyRng(spec.Seed);

				UpdateStatus("Generating ecology...");
				_currentWeb = EcologyGenerator.Generate(spec, rng);

				System.Collections.Generic.List<string> errors = EcologyConstraints.ValidateEcologyWeb(_currentWeb);
				if (errors.Count > 0)
				{
					GD.PrintErr("Validation warnings: " + string.Join("; ", errors));
				}

				UpdateStatsDisplay();
				_webRenderer.RenderWeb(_currentWeb);

				UpdateStatus("Generated ecology with " + _currentWeb.Slots.Count + " slots, " + _currentWeb.Connections.Count + " connections");
			}
			catch (Exception ex)
			{
				UpdateStatus("Error: " + ex.Message);
				GD.PrintErr(ex);
			}
		}

		/// <summary>
		/// Builds an EnvironmentSpec instance from the current UI slider state.
		/// </summary>
		/// <returns>Populated environment spec.</returns>
		private EnvironmentSpec BuildSpecFromUI()
		{
			ulong seed;
			if (!ulong.TryParse(_seedInput.Text, out seed))
			{
				seed = (ulong)GD.Randi();
				_seedInput.Text = seed.ToString();
			}

			EnvironmentSpec spec = new EnvironmentSpec
			{
				Seed = seed,
				Biome = (BiomeType)_biomeOption.Selected,
				TemperatureMin = (float)_tempMinSlider.Value,
				TemperatureMax = (float)_tempMaxSlider.Value,
				WaterAvailability = (float)_waterSlider.Value,
				LightLevel = (float)_lightSlider.Value,
				NutrientLevel = (float)_nutrientSlider.Value,
				Gravity = (float)_gravitySlider.Value,
				RadiationLevel = (float)_radiationSlider.Value,
				OxygenLevel = (float)_oxygenSlider.Value
			};

			return spec;
		}

		/// <summary>
		/// Updates the statistics panel with summary data for the current web.
		/// </summary>
		private void UpdateStatsDisplay()
		{
			if (_currentWeb == null)
			{
				_statsText.Text = "No ecology generated.";
				return;
			}

			string text = "[b]Ecology Statistics[/b]\n\n";
			text += "Total Productivity: " + _currentWeb.TotalProductivity.ToString("F2") + "\n";
			text += "Total Biomass: " + _currentWeb.GetTotalBiomass().ToString("F1") + "\n";
			text += "Complexity: " + _currentWeb.ComplexityScore.ToString("P0") + "\n";
			text += "Stability: " + _currentWeb.StabilityScore.ToString("P0") + "\n";
			text += "Max Chain Length: " + _currentWeb.GetMaxChainLength() + "\n\n";

			text += "[b]Slots by Level[/b]\n";
			foreach (TrophicLevel level in Enum.GetValues(typeof(TrophicLevel)))
			{
				System.Collections.Generic.List<TrophicSlot> slots = _currentWeb.GetSlotsByLevel(level);
				if (slots.Count > 0)
				{
					text += "  " + level + ": " + slots.Count + "\n";
				}
			}

			text += "\n[b]Connections[/b]: " + _currentWeb.Connections.Count + "\n";
			text += "Connection Density: " + _currentWeb.GetConnectionDensity().ToString("F2") + "\n";

			_statsText.Text = text;
		}

		/// <summary>
		/// Updates the bottom status label with the given message.
		/// </summary>
		/// <param name="message">Status message to show.</param>
		private void UpdateStatus(string message)
		{
			_statusLabel.Text = message;
		}
	}
}
