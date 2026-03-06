using System;
using System.Globalization;
using System.Threading.Tasks;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Node-caching, state initialization, renderer construction, and signal-wiring for GalaxyViewer.
/// </summary>
public partial class GalaxyViewer
{
	private const int JumpRouteProgressBatchSize = 16;
	private const int JumpRoutePipelineStageCount = 4;

	/// <summary>
	/// Caches UI node references.
	/// </summary>
	private void CacheNodeReferences()
	{
		_statusLabel = GetNodeOrNull<Label>("UI/UIRoot/TopBar/MarginContainer/HBoxContainer/StatusLabel");
		_seedInput = GetNodeOrNull<SpinBox>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput");
		_showCompassCheck = GetNodeOrNull<CheckBox>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection/ShowCompassCheck");
		_inspectorPanel = GetNodeOrNull<Node>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel");
		_saveButton = GetNodeOrNull<Button>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton");
		_loadButton = GetNodeOrNull<Button>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton");
		_newGalaxyButton = GetNodeOrNull<Button>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/NewGalaxyButton");
	}

	/// <summary>
	/// Initializes in-memory state and helper objects.
	/// </summary>
	private void InitializeState()
	{
		_galaxyConfig ??= GalaxyConfig.CreateDefault();
		_galaxy = new Galaxy(_galaxyConfig, GalaxySeed);
		_spec = _galaxy.Spec;
		_zoomMachine = new ZoomStateMachine();
		_galaxyRenderer = GetNodeOrNull<GalaxyRenderer>("GalaxyRenderer");
		_quadrantRenderer = GetNodeOrNull<QuadrantRenderer>("QuadrantRenderer");
		_quadrantSelector = new QuadrantSelector();
		_quadrantCursor = new GridCursor();
		_sectorCursor = new GridCursor();
		_orbitCamera = GetNodeOrNull<OrbitCamera>("OrbitCamera");
		_starCamera = GetNodeOrNull<StarViewCamera>("StarCamera");
		_compass = GetNodeOrNull<NavigationCompass>("UI/Compass");
		_selectionIndicator = GetNodeOrNull<SelectionIndicator>("SelectionIndicator");
		_sectorRenderer = GetNodeOrNull<Node>("SectorRenderer");
		_neighborhoodRenderer = GetNodeOrNull<Node>("NeighborhoodRenderer");
		_sectorJumpLaneRenderer = GetNodeOrNull<SectorJumpLaneRenderer>("SectorJumpLaneRenderer");

		if (_spec != null)
		{
			float cameraFar = (float)(_spec.RadiusPc * 10.0);
			if (_orbitCamera != null)
			{
				_orbitCamera.Far = cameraFar;
			}

			if (_starCamera != null)
			{
				_starCamera.Far = cameraFar;
			}
		}
	}

	/// <summary>
	/// Builds the initial galaxy and quadrant renderers for the C# scene path.
	/// </summary>
	private void BuildStaticRenderers()
	{
		if (_galaxy == null || _spec == null)
		{
			return;
		}

		if (_galaxyRenderer != null)
		{
			SeededRng rng = new(_spec.GalaxySeed);
			GalaxySample sample = DensitySampler.SampleGalaxy(_spec, NumPoints, rng);
			_galaxyRenderer.BuildFromSample(sample, StarSize, (int)_spec.Type);
		}

		_quadrantRenderer?.BuildFromDensity(_spec, _galaxy.DensityModel);
	}

	/// <summary>
	/// Connects UI signals used by the controller.
	/// </summary>
	private void ConnectUiSignals()
	{
		if (_showCompassCheck != null)
		{
			_showCompassCheck.Toggled += OnShowCompassToggled;
		}

		if (_newGalaxyButton != null)
		{
			_newGalaxyButton.Pressed += () => EmitSignal(SignalName.NewGalaxyRequested);
		}

		if (_starCamera != null)
		{
			_starCamera.SubsectorChanged += OnSubsectorChanged;
		}

		if (_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel)
		{
			typedInspectorPanel.OpenSystemRequested += OnInspectorOpenSystemRequested;
			typedInspectorPanel.CalculateJumpRoutesRequested += OnCalculateJumpRoutesRequested;
			typedInspectorPanel.JumpRoutesVisibilityToggled += OnJumpRoutesVisibilityToggled;
		}
	}

	/// <summary>
	/// Updates the seed display in the sidebar.
	/// </summary>
	private void UpdateSeedDisplay()
	{
		if (_seedInput != null)
		{
			_seedInput.Value = GalaxySeed;
		}
	}

	/// <summary>
	/// Rebuilds the visible neighborhood when the star camera crosses a subsector boundary.
	/// </summary>
	private void OnSubsectorChanged(Vector3 newOrigin)
	{
		if (
			_zoomMachine == null ||
			_zoomMachine.GetCurrentLevel() != (int)GalaxyCoordinates.ZoomLevel.Subsector ||
			_galaxy == null ||
			_neighborhoodRenderer is not NeighborhoodRenderer neighborhoodRenderer)
		{
			return;
		}

		Vector3 cameraPosition = newOrigin;
		if (_starCamera != null)
		{
			cameraPosition = _starCamera.GetCurrentPosition();
		}

		neighborhoodRenderer.BuildNeighborhood(
			cameraPosition,
			GalaxySeed,
			_galaxy.DensityModel,
			_galaxy.ReferenceDensity);
		ClearStarSelection();
		UpdateJumpRoutePresentation();
	}

	/// <summary>
	/// Toggles compass visibility.
	/// </summary>
	private void OnShowCompassToggled(bool visible)
	{
		if (_compass != null)
		{
			_compass.Visible = visible;
		}
	}

	private bool IsSubsectorActive()
	{
		if (_zoomMachine == null)
		{
			return false;
		}

		if (_zoomMachine.GetCurrentLevel() != (int)GalaxyCoordinates.ZoomLevel.Subsector)
		{
			return false;
		}

		if (_quadrantSelector == null || !_quadrantSelector.HasSelection())
		{
			return false;
		}

		if (_selectedSector.VariantType != Variant.Type.Vector3I)
		{
			return false;
		}

		return true;
	}

	private void InvalidateJumpRoutes()
	{
		_jumpRouteCalculationGeneration += 1;
		_jumpLaneRegion = null;
		_jumpLaneResult = null;
		_jumpRouteCalculatedRegionIds.Clear();
		UpdateJumpRoutePresentation();
	}

	private void UpdateJumpRoutePresentation()
	{
		GalaxyInspectorPanel? inspectorPanel = GetInspectorPanel();
		bool subsectorActive = IsSubsectorActive();

		if (!subsectorActive)
		{
			if (_sectorJumpLaneRenderer != null)
			{
				_sectorJumpLaneRenderer.Clear();
				_sectorJumpLaneRenderer.Visible = false;
			}

			if (inspectorPanel != null)
			{
				inspectorPanel.SetJumpRoutesAvailable(false);
			}

			return;
		}

		if (_jumpLaneResult == null)
		{
			if (_sectorJumpLaneRenderer != null)
			{
				_sectorJumpLaneRenderer.Clear();
				_sectorJumpLaneRenderer.Visible = false;
			}

			if (inspectorPanel != null)
			{
				inspectorPanel.SetJumpRoutesAvailable(false);
			}

			return;
		}

		if (inspectorPanel != null)
		{
			inspectorPanel.SetJumpRoutesAvailable(true);
		}

		bool showRoutes = true;
		if (inspectorPanel != null)
		{
			showRoutes = inspectorPanel.GetShowRoutesChecked();
		}

		if (_sectorJumpLaneRenderer == null)
		{
			return;
		}

		if (!showRoutes)
		{
			_sectorJumpLaneRenderer.Visible = false;
			return;
		}

		_sectorJumpLaneRenderer.Render(_jumpLaneResult);
		_sectorJumpLaneRenderer.Visible = true;
	}

	private async void OnCalculateJumpRoutesRequested()
	{
		await CalculateJumpRoutesAsync(true);
	}

	private void CalculateJumpRoutesSynchronously()
	{
		if (!IsSubsectorActive())
		{
			SetStatus("Jump routes are only available in subsector view");
			UpdateJumpRoutePresentation();
			return;
		}

		GalaxyInspectorPanel? inspectorPanel = GetInspectorPanel();
		int calculationGeneration = _jumpRouteCalculationGeneration + 1;
		_jumpRouteCalculationGeneration = calculationGeneration;
		if (inspectorPanel != null)
		{
			inspectorPanel.SetJumpRoutesCalculating(true);
		}

		try
		{
			string? currentRegionId = GetCurrentJumpRouteRegionId();
			if (_jumpLaneRegion != null && !string.IsNullOrEmpty(currentRegionId) && _jumpRouteCalculatedRegionIds.Contains(currentRegionId))
			{
				UpdateJumpRoutePresentation();
				SetStatus("Jump routes already calculated for visible subsectors");
				return;
			}

			JumpLaneRegion? region = BuildJumpLaneRegionSynchronously(calculationGeneration);
			if (calculationGeneration != _jumpRouteCalculationGeneration)
			{
				return;
			}

			if (region == null)
			{
				InvalidateJumpRoutes();
				SetStatus("Jump-route data unavailable for the current subsector");
				return;
			}

			if (region.GetSystemCount() == 0)
			{
				InvalidateJumpRoutes();
				SetStatus("No stars available for jump-route calculation");
				return;
			}

			JumpLaneRegion combinedRegion;
			int newSystemCount;
			if (_jumpLaneRegion != null)
			{
				if (inspectorPanel != null)
				{
					inspectorPanel.SetJumpRoutesStage("Merging visible subsectors", 2, JumpRoutePipelineStageCount);
				}

				combinedRegion = CloneJumpLaneRegion(_jumpLaneRegion);
				newSystemCount = MergeJumpLaneRegion(combinedRegion, region);
			}
			else
			{
				if (inspectorPanel != null)
				{
					inspectorPanel.SetJumpRoutesStage("Creating route region", 2, JumpRoutePipelineStageCount);
				}

				combinedRegion = region;
				newSystemCount = region.GetSystemCount();
			}

			if (newSystemCount == 0)
			{
				UpdateJumpRoutePresentation();
				SetStatus("Jump routes already calculated for visible subsectors");
				return;
			}

			if (inspectorPanel != null)
			{
				inspectorPanel.SetJumpRoutesStage("Calculating route graph", 3, JumpRoutePipelineStageCount);
			}

			JumpRouteBackgroundResult graphResult = CalculateJumpRouteGraphSynchronously(combinedRegion);
			if (calculationGeneration != _jumpRouteCalculationGeneration)
			{
				return;
			}

			if (inspectorPanel != null)
			{
				inspectorPanel.SetJumpRoutesStage("Rendering jump routes", 4, JumpRoutePipelineStageCount);
			}

			_jumpLaneRegion = BuildJumpLaneRegionFromBackground(combinedRegion, graphResult);
			_jumpLaneResult = BuildJumpLaneResultFromBackground(graphResult);
			if (!string.IsNullOrEmpty(region.RegionId))
			{
				_jumpRouteCalculatedRegionIds.Add(region.RegionId);
			}
			UpdateJumpRoutePresentation();

			SetStatus(
				$"Calculated {_jumpLaneResult.GetTotalConnections()} jump routes across {_jumpLaneRegion.GetSystemCount()} systems ({newSystemCount} new, {_jumpLaneResult.GetTotalOrphans()} orphans)");
		}
		finally
		{
			if (inspectorPanel != null && calculationGeneration == _jumpRouteCalculationGeneration)
			{
				inspectorPanel.SetJumpRoutesCalculating(false);
			}
		}
	}

	private async Task CalculateJumpRoutesAsync(bool yieldBetweenBatches)
	{
		if (!IsSubsectorActive())
		{
			SetStatus("Jump routes are only available in subsector view");
			UpdateJumpRoutePresentation();
			return;
		}

		GalaxyInspectorPanel? inspectorPanel = GetInspectorPanel();
		int calculationGeneration = _jumpRouteCalculationGeneration + 1;
		_jumpRouteCalculationGeneration = calculationGeneration;
		if (inspectorPanel != null)
		{
			inspectorPanel.SetJumpRoutesCalculating(true);
		}

		try
		{
			string? currentRegionId = GetCurrentJumpRouteRegionId();
			if (_jumpLaneRegion != null && !string.IsNullOrEmpty(currentRegionId) && _jumpRouteCalculatedRegionIds.Contains(currentRegionId))
			{
				UpdateJumpRoutePresentation();
				SetStatus("Jump routes already calculated for visible subsectors");
				return;
			}

			JumpLaneRegion? region = await BuildJumpLaneRegionAsync(calculationGeneration, yieldBetweenBatches);
			if (calculationGeneration != _jumpRouteCalculationGeneration)
			{
				return;
			}

			if (region == null)
			{
				InvalidateJumpRoutes();
				SetStatus("Jump-route data unavailable for the current subsector");
				return;
			}

			if (region.GetSystemCount() == 0)
			{
				InvalidateJumpRoutes();
				SetStatus("No stars available for jump-route calculation");
				return;
			}

			JumpLaneRegion combinedRegion;
			int newSystemCount;
			if (_jumpLaneRegion != null)
			{
				if (inspectorPanel != null)
				{
					inspectorPanel.SetJumpRoutesStage("Merging visible subsectors", 2, JumpRoutePipelineStageCount);
				}

				combinedRegion = CloneJumpLaneRegion(_jumpLaneRegion);
				newSystemCount = MergeJumpLaneRegion(combinedRegion, region);
			}
			else
			{
				if (inspectorPanel != null)
				{
					inspectorPanel.SetJumpRoutesStage("Creating route region", 2, JumpRoutePipelineStageCount);
				}

				combinedRegion = region;
				newSystemCount = region.GetSystemCount();
			}

			if (newSystemCount == 0)
			{
				UpdateJumpRoutePresentation();
				SetStatus("Jump routes already calculated for visible subsectors");
				return;
			}

			if (yieldBetweenBatches)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			if (inspectorPanel != null)
			{
				inspectorPanel.SetJumpRoutesStage("Calculating route graph", 3, JumpRoutePipelineStageCount);
			}

			Task<JumpRouteBackgroundResult> graphTask = CalculateJumpRouteGraphAsync(combinedRegion);
			if (yieldBetweenBatches)
			{
				while (!graphTask.IsCompleted)
				{
					if (calculationGeneration != _jumpRouteCalculationGeneration)
					{
						return;
					}

					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}
			}

			JumpRouteBackgroundResult graphResult = await graphTask;
			if (calculationGeneration != _jumpRouteCalculationGeneration)
			{
				return;
			}

			if (inspectorPanel != null)
			{
				inspectorPanel.SetJumpRoutesStage("Rendering jump routes", 4, JumpRoutePipelineStageCount);
			}

			if (yieldBetweenBatches)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			_jumpLaneRegion = BuildJumpLaneRegionFromBackground(combinedRegion, graphResult);
			_jumpLaneResult = BuildJumpLaneResultFromBackground(graphResult);
			if (!string.IsNullOrEmpty(region.RegionId))
			{
				_jumpRouteCalculatedRegionIds.Add(region.RegionId);
			}
			UpdateJumpRoutePresentation();

			SetStatus(
				$"Calculated {_jumpLaneResult.GetTotalConnections()} jump routes across {_jumpLaneRegion.GetSystemCount()} systems ({newSystemCount} new, {_jumpLaneResult.GetTotalOrphans()} orphans)");
		}
		finally
		{
			if (inspectorPanel != null && calculationGeneration == _jumpRouteCalculationGeneration)
			{
				inspectorPanel.SetJumpRoutesCalculating(false);
			}
		}
	}

	private void OnJumpRoutesVisibilityToggled(bool showRoutes)
	{
		if (!showRoutes && _sectorJumpLaneRenderer != null)
		{
			_sectorJumpLaneRenderer.Visible = false;
		}

		UpdateJumpRoutePresentation();
	}

	private async Task<JumpLaneRegion?> BuildJumpLaneRegionAsync(int calculationGeneration, bool yieldBetweenBatches)
	{
		if (!IsSubsectorActive())
		{
			return null;
		}

		if (_spec == null || _neighborhoodRenderer is not NeighborhoodRenderer neighborhoodRenderer)
		{
			return null;
		}

		SubSectorNeighborhoodData? neighborhoodData = neighborhoodRenderer.get_neighborhood_data();
		if (neighborhoodData == null)
		{
			return null;
		}

		string regionId = FormattableString.Invariant(
			$"{neighborhoodData.CenterOrigin.X:0.###},{neighborhoodData.CenterOrigin.Y:0.###},{neighborhoodData.CenterOrigin.Z:0.###}");
		JumpLaneRegion region = new(JumpLaneRegion.RegionScope.Subsector, regionId);
		int starCount = Math.Min(neighborhoodData.StarPositions.Length, neighborhoodData.StarSeeds.Length);
		GalaxyInspectorPanel? inspectorPanel = GetInspectorPanel();

		if (inspectorPanel != null)
		{
			inspectorPanel.SetJumpRoutesProgress("Scanning subsector stars", 0, starCount);
		}

		for (int index = 0; index < starCount; index++)
		{
			if (calculationGeneration != _jumpRouteCalculationGeneration)
			{
				return null;
			}

			long starSeed = neighborhoodData.StarSeeds[index];
			int population = EstimateJumpRoutePopulation(neighborhoodData.StarPositions[index], starSeed);

			JumpLaneSystem system = new(
				starSeed.ToString(CultureInfo.InvariantCulture),
				neighborhoodData.StarPositions[index],
				population);
			region.AddSystem(system);

			int completed = index + 1;
			if (inspectorPanel != null && (completed == starCount || (completed % JumpRouteProgressBatchSize) == 0))
			{
				inspectorPanel.SetJumpRoutesProgress("Scanning subsector stars", completed, starCount);
			}

			if (yieldBetweenBatches && completed < starCount && (completed % JumpRouteProgressBatchSize) == 0)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}

		return region;
	}

	private JumpLaneRegion? BuildJumpLaneRegionSynchronously(int calculationGeneration)
	{
		if (!IsSubsectorActive())
		{
			return null;
		}

		if (_spec == null || _neighborhoodRenderer is not NeighborhoodRenderer neighborhoodRenderer)
		{
			return null;
		}

		SubSectorNeighborhoodData? neighborhoodData = neighborhoodRenderer.get_neighborhood_data();
		if (neighborhoodData == null)
		{
			return null;
		}

		string regionId = FormattableString.Invariant(
			$"{neighborhoodData.CenterOrigin.X:0.###},{neighborhoodData.CenterOrigin.Y:0.###},{neighborhoodData.CenterOrigin.Z:0.###}");
		JumpLaneRegion region = new(JumpLaneRegion.RegionScope.Subsector, regionId);
		int starCount = Math.Min(neighborhoodData.StarPositions.Length, neighborhoodData.StarSeeds.Length);
		GalaxyInspectorPanel? inspectorPanel = GetInspectorPanel();

		if (inspectorPanel != null)
		{
			inspectorPanel.SetJumpRoutesProgress("Scanning subsector stars", 0, starCount);
		}

		for (int index = 0; index < starCount; index += 1)
		{
			if (calculationGeneration != _jumpRouteCalculationGeneration)
			{
				return null;
			}

			long starSeed = neighborhoodData.StarSeeds[index];
			int population = EstimateJumpRoutePopulation(neighborhoodData.StarPositions[index], starSeed);
			JumpLaneSystem system = new(
				starSeed.ToString(CultureInfo.InvariantCulture),
				neighborhoodData.StarPositions[index],
				population);
			region.AddSystem(system);

			int completed = index + 1;
			if (inspectorPanel != null && (completed == starCount || (completed % JumpRouteProgressBatchSize) == 0))
			{
				inspectorPanel.SetJumpRoutesProgress("Scanning subsector stars", completed, starCount);
			}
		}

		return region;
	}

	private string? GetCurrentJumpRouteRegionId()
	{
		if (_neighborhoodRenderer is not NeighborhoodRenderer neighborhoodRenderer)
		{
			return null;
		}

		SubSectorNeighborhoodData? neighborhoodData = neighborhoodRenderer.get_neighborhood_data();
		if (neighborhoodData == null)
		{
			return null;
		}

		return FormattableString.Invariant(
			$"{neighborhoodData.CenterOrigin.X:0.###},{neighborhoodData.CenterOrigin.Y:0.###},{neighborhoodData.CenterOrigin.Z:0.###}");
	}

	private int EstimateJumpRoutePopulation(Vector3 worldPosition, long starSeed)
	{
		if (_spec == null)
		{
			return 0;
		}

		if (starSeed < int.MinValue || starSeed > int.MaxValue)
		{
			return 0;
		}

		int typedSeed = (int)starSeed;
		if (_jumpRoutePopulationCache.TryGetValue(typedSeed, out int cachedPopulation))
		{
			return cachedPopulation;
		}

		GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(worldPosition, typedSeed, _spec);
		int population = EstimateJumpRoutePopulationHeuristically(star);

		_jumpRoutePopulationCache[typedSeed] = population;
		return population;
	}

	private int EstimateJumpRoutePopulationHeuristically(GalaxyStar star)
	{
		if (_spec == null)
		{
			return 0;
		}

		SeededRng rng = new(unchecked(star.StarSeed ^ GalaxySeed ^ 0x4A50524F));
		double radialDistancePc = star.GetRadialDistance();
		double ghzCenterPc = 8000.0;
		double ghzSigmaPc = 3500.0;
		double radialOffset = radialDistancePc - ghzCenterPc;
		double radialFactor = Math.Exp(-(radialOffset * radialOffset) / (2.0 * ghzSigmaPc * ghzSigmaPc));

		double scaleHeightPc;
		if (_spec.DiskScaleHeightPc > 0.0)
		{
			scaleHeightPc = _spec.DiskScaleHeightPc;
		}
		else
		{
			scaleHeightPc = 300.0;
		}

		double heightFactor = Math.Exp(-Math.Abs(star.GetHeight()) / scaleHeightPc);
		double metallicityFactor = Math.Clamp((star.Metallicity - 0.25) / 1.35, 0.0, 1.0);
		double ageBiasDistance = Math.Abs(star.AgeBias - 1.0);
		double ageFactor = Math.Clamp(1.0 - (ageBiasDistance * 0.6), 0.2, 1.0);

		double habitabilityScore =
			(radialFactor * 0.45) +
			(heightFactor * 0.20) +
			(metallicityFactor * 0.25) +
			(ageFactor * 0.10);
		habitabilityScore = Math.Clamp(habitabilityScore, 0.0, 1.0);

		double inhabitedChance = Math.Clamp((habitabilityScore * 0.70) - 0.10, 0.0, 0.65);
		if (rng.Randf() >= inhabitedChance)
		{
			return 0;
		}

		double populationTier = Math.Clamp(
			(habitabilityScore * 0.75) +
			(metallicityFactor * 0.15) +
			((1.0 - ageBiasDistance) * 0.10),
			0.05,
			1.0);
		double jitter = rng.RandfRange(-0.35f, 0.35f);
		double logPopulation = 3.5 + (populationTier * 6.0) + jitter;
		double populationValue = Math.Pow(10.0, logPopulation);
		if (populationValue > int.MaxValue)
		{
			return int.MaxValue;
		}

		return Math.Max(1, (int)populationValue);
	}
}
