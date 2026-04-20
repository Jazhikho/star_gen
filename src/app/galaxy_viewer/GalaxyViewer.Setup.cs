using System;
using System.Globalization;
using System.Threading.Tasks;
using Godot;
using StarGen.Domain.Colonization;
using StarGen.Domain.Celestial;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;

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
		_uiRoot = GetNodeOrNull<Control>("UI/UIRoot");
		_topBar = GetNodeOrNull<Control>("UI/UIRoot/TopBar");
		_sidePanel = GetNodeOrNull<Control>("UI/UIRoot/SidePanel");
		_statusLabel = GetNodeOrNull<Label>("UI/UIRoot/TopBar/MarginContainer/TopBarVBox/HeaderRow/StatusLabel");
		_inspectorPanel = GetNodeOrNull<Node>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel");
		_saveLoadSection = GetNodeOrNull<Control>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection");
		_saveButton = GetNodeOrNull<Button>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton");
		_loadButton = GetNodeOrNull<Button>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton");
		_newGalaxyButton = GetNodeOrNull<Button>("UI/UIRoot/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/NewGalaxyButton");
		_optionsDialog = GetNodeOrNull<Window>("OptionsDialog");
		_fullscreenCheck = GetNodeOrNull<CheckButton>("OptionsDialog/MarginContainer/OptionsVBox/FullscreenCheck");
		_showSeedControlsCheck = GetNodeOrNull<CheckButton>("OptionsDialog/MarginContainer/OptionsVBox/ShowSeedControlsCheck");
		_skipIntroCheck = GetNodeOrNull<CheckButton>("OptionsDialog/MarginContainer/OptionsVBox/SkipIntroCheck");
		_resolutionOption = GetNodeOrNull<OptionButton>("OptionsDialog/MarginContainer/OptionsVBox/ResolutionRow/ResolutionOption");
		_applyOptionsButton = GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ApplyOptionsButton");
		_optionsStatusLabel = GetNodeOrNull<Label>("OptionsDialog/MarginContainer/OptionsVBox/OptionsStatusLabel");
		_optionsDialogCloseButton = GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/CloseButton");
		_buildLocalSpaceDialog = GetNodeOrNull<Window>("BuildLocalSpaceDialog");
		_localSpaceExtentXInput = GetNodeOrNull<SpinBox>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/ExtentGrid/XSpinBox");
		_localSpaceExtentYInput = GetNodeOrNull<SpinBox>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/ExtentGrid/YSpinBox");
		_localSpaceExtentZInput = GetNodeOrNull<SpinBox>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/ExtentGrid/ZSpinBox");
		_localSpaceAreaLabel = GetNodeOrNull<Label>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/AreaLabel");
		_localSpaceStarCountLabel = GetNodeOrNull<Label>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/StarCountLabel");
		_localSpaceWarningLabel = GetNodeOrNull<Label>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/WarningLabel");
		_localSpaceStatusLabel = GetNodeOrNull<Label>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/StatusLabel");
		_buildLocalSpaceRunButton = GetNodeOrNull<Button>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/ButtonRow/BuildButton");
		_buildLocalSpaceCloseButton = GetNodeOrNull<Button>("BuildLocalSpaceDialog/MarginContainer/LocalSpaceVBox/ButtonRow/CloseButton");
	}

	/// <summary>
	/// Initializes in-memory state and helper objects.
	/// </summary>
	private void InitializeState()
	{
		_galaxyConfig ??= GalaxyConfig.CreateDefault();
		_colonizationSimulationSettings ??= ColonizationSimulationSettings.CreateDefault();
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
		if (_saveLoadSection != null)
		{
			_saveLoadSection.Visible = false;
		}

		if (_starCamera != null)
		{
			_starCamera.SubsectorChanged += OnSubsectorChanged;
		}

		PopulateResolutionOptions();
		RefreshOptionsState();
		ConnectOptionsSignals();
		ConnectLocalSpaceDialogSignals();

		if (_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel)
		{
			typedInspectorPanel.OpenSystemRequested += OnInspectorOpenSystemRequested;
			typedInspectorPanel.CalculateJumpRoutesRequested += OnCalculateJumpRoutesRequested;
			typedInspectorPanel.JumpRoutesVisibilityToggled += OnJumpRoutesVisibilityToggled;
			typedInspectorPanel.ApplyGalaxyConfigRequested += OnApplyGalaxyConfigRequested;
			typedInspectorPanel.SetColonizationSimulationSettings(_colonizationSimulationSettings);
		}
	}

	/// <summary>
	/// Updates orbit-camera framing to account for the left panel and top bar.
	/// </summary>
	private void UpdatePanelAwareFraming()
	{
		_renderAreaRect = StarGen.App.Shared.ViewerLayoutHelper.ComputeRenderRect(GetViewport(), _topBar, _sidePanel);
		Vector2 framingOffset = StarGen.App.Shared.ViewerLayoutHelper.ComputeNormalizedCenterOffset(GetViewport(), _renderAreaRect);
		if (_orbitCamera != null)
		{
			_orbitCamera.SetFramingOffset(framingOffset);
		}
	}

	/// <summary>
	/// Refreshes the inspector when the live overview position changes.
	/// </summary>
	private void UpdateInspectorForMovement()
	{
		if (_inspectorPanel == null || _spec == null || _zoomMachine == null)
		{
			return;
		}

		Vector3 displayPosition = GetInspectorDisplayPosition();
		int zoomLevel = _zoomMachine.GetCurrentLevel();
		if (_hasInspectorDisplayPosition
			&& displayPosition.IsEqualApprox(_lastInspectorDisplayPosition)
			&& _lastInspectorSelectedStarSeed == _selectedStarSeed
			&& _lastInspectorZoomLevel == zoomLevel)
		{
			return;
		}

		_hasInspectorDisplayPosition = true;
		_lastInspectorDisplayPosition = displayPosition;
		_lastInspectorSelectedStarSeed = _selectedStarSeed;
		_lastInspectorZoomLevel = zoomLevel;
		UpdateInspector();
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
		string? regionId = GetCurrentJumpRouteRegionId();
		if (_localSpaceCache == null || string.IsNullOrEmpty(regionId))
		{
			_jumpLaneRegion = null;
			_jumpLaneResult = null;
		}
		ClearStarSelection();
		UpdateJumpRoutePresentation();
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
		_jumpRouteSystemCache.Clear();
		_jumpRouteCalculatedRegionIds.Clear();
		UpdateJumpRoutePresentation();
	}

	private void UpdateJumpRoutePresentation()
	{
		GalaxyInspectorPanel? inspectorPanel = GetInspectorPanel();
		bool subsectorActive = IsSubsectorActive();
		string? currentRegionId = GetCurrentJumpRouteRegionId();

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

		if (_jumpLaneRegion != null
			&& !string.IsNullOrEmpty(currentRegionId)
			&& !string.IsNullOrEmpty(_jumpLaneRegion.RegionId)
			&& !string.Equals(currentRegionId, _jumpLaneRegion.RegionId, StringComparison.Ordinal))
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
			JumpLaneRegion? region = BuildJumpLaneRegionSynchronously(calculationGeneration);
			if (calculationGeneration != _jumpRouteCalculationGeneration)
			{
				return;
			}

			if (region == null)
			{
				InvalidateJumpRoutes();
				SetStatus("Build local space around the current location before calculating jump routes");
				return;
			}

			if (region.GetSystemCount() == 0)
			{
				InvalidateJumpRoutes();
				SetStatus("No stars available for jump-route calculation");
				return;
			}

			if (inspectorPanel != null)
			{
				inspectorPanel.SetJumpRoutesStage("Running explicit route simulation", 2, JumpRoutePipelineStageCount);
			}

			if (UseTravellerRouteMode())
			{
				JumpRouteBackgroundResult graphResult = CalculateJumpRouteGraphSynchronously(region);
				if (calculationGeneration != _jumpRouteCalculationGeneration)
				{
					return;
				}

				if (inspectorPanel != null)
				{
					inspectorPanel.SetJumpRoutesStage("Rendering jump routes", 3, JumpRoutePipelineStageCount);
				}

				_jumpLaneRegion = BuildJumpLaneRegionFromBackground(region, graphResult);
				_jumpLaneResult = BuildJumpLaneResultFromBackground(graphResult);
				_jumpRouteCalculatedRegionIds.Clear();
				if (!string.IsNullOrEmpty(region.RegionId))
				{
					_jumpRouteCalculatedRegionIds.Add(region.RegionId);
				}
			}
			else
			{
				ColonizationSimulationState simulationState = CalculateColonizationSimulationSynchronously(region);
				if (calculationGeneration != _jumpRouteCalculationGeneration)
				{
					return;
				}

				if (_galaxy != null)
				{
					_galaxy.CacheColonizationSimulationState(simulationState);
				}

				if (inspectorPanel != null)
				{
					inspectorPanel.SetJumpRoutesStage("Rendering simulated routes", 3, JumpRoutePipelineStageCount);
				}

				_jumpLaneRegion = simulationState.ToJumpLaneRegion();
				_jumpLaneResult = simulationState.ToJumpLaneResult();
				_jumpRouteCalculatedRegionIds.Clear();
				if (!string.IsNullOrEmpty(region.RegionId))
				{
					_jumpRouteCalculatedRegionIds.Add(region.RegionId);
				}
			}

			UpdateJumpRoutePresentation();
			SetStatus(
				$"Calculated {_jumpLaneResult.GetTotalConnections()} jump routes across {_jumpLaneRegion.GetSystemCount()} systems ({_jumpLaneResult.GetTotalOrphans()} orphans)");
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
			JumpLaneRegion? region = await BuildJumpLaneRegionAsync(calculationGeneration, yieldBetweenBatches);
			if (calculationGeneration != _jumpRouteCalculationGeneration)
			{
				return;
			}

			if (region == null)
			{
				InvalidateJumpRoutes();
				SetStatus("Build local space around the current location before calculating jump routes");
				return;
			}

			if (region.GetSystemCount() == 0)
			{
				InvalidateJumpRoutes();
				SetStatus("No stars available for jump-route calculation");
				return;
			}

			if (yieldBetweenBatches)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			if (inspectorPanel != null)
			{
				inspectorPanel.SetJumpRoutesStage("Running explicit route simulation", 2, JumpRoutePipelineStageCount);
			}

			if (UseTravellerRouteMode())
			{
				Task<JumpRouteBackgroundResult> graphTask = CalculateJumpRouteGraphAsync(region);
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
					inspectorPanel.SetJumpRoutesStage("Rendering jump routes", 3, JumpRoutePipelineStageCount);
				}

				_jumpLaneRegion = BuildJumpLaneRegionFromBackground(region, graphResult);
				_jumpLaneResult = BuildJumpLaneResultFromBackground(graphResult);
			}
			else
			{
				Task<ColonizationSimulationState> simulationTask = CalculateColonizationSimulationAsync(region);
				if (yieldBetweenBatches)
				{
					while (!simulationTask.IsCompleted)
					{
						if (calculationGeneration != _jumpRouteCalculationGeneration)
						{
							return;
						}

						await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
					}
				}

				ColonizationSimulationState simulationState = await simulationTask;
				if (calculationGeneration != _jumpRouteCalculationGeneration)
				{
					return;
				}

				if (_galaxy != null)
				{
					_galaxy.CacheColonizationSimulationState(simulationState);
				}

				if (inspectorPanel != null)
				{
					inspectorPanel.SetJumpRoutesStage("Rendering simulated routes", 3, JumpRoutePipelineStageCount);
				}

				_jumpLaneRegion = simulationState.ToJumpLaneRegion();
				_jumpLaneResult = simulationState.ToJumpLaneResult();
			}

			if (yieldBetweenBatches)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			_jumpRouteCalculatedRegionIds.Clear();
			if (!string.IsNullOrEmpty(region.RegionId))
			{
				_jumpRouteCalculatedRegionIds.Add(region.RegionId);
			}

			UpdateJumpRoutePresentation();
			SetStatus(
				$"Calculated {_jumpLaneResult.GetTotalConnections()} jump routes across {_jumpLaneRegion.GetSystemCount()} systems ({_jumpLaneResult.GetTotalOrphans()} orphans)");
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
		GalaxyLocalSpaceCache? activeCache = GetActiveLocalSpaceCache();
		if (!IsSubsectorActive() || activeCache == null)
		{
			return null;
		}

		if (calculationGeneration != _jumpRouteCalculationGeneration)
		{
			return null;
		}

		if (yieldBetweenBatches)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		return CloneJumpLaneRegion(activeCache.Region);
	}

	private JumpLaneRegion? BuildJumpLaneRegionSynchronously(int calculationGeneration)
	{
		GalaxyLocalSpaceCache? activeCache = GetActiveLocalSpaceCache();
		if (!IsSubsectorActive() || activeCache == null)
		{
			return null;
		}

		if (calculationGeneration != _jumpRouteCalculationGeneration)
		{
			return null;
		}

		return CloneJumpLaneRegion(activeCache.Region);
	}

	private string? GetCurrentJumpRouteRegionId()
	{
		GalaxyLocalSpaceCache? activeCache = GetActiveLocalSpaceCache();
		if (activeCache == null)
		{
			return null;
		}

		return activeCache.Region.RegionId;
	}

	private JumpLaneSystem CreateJumpLaneSystemForRoutes(Vector3 worldPosition, long starSeed)
	{
		if (_spec == null || starSeed < int.MinValue || starSeed > int.MaxValue)
		{
			return new JumpLaneSystem(
				starSeed.ToString(CultureInfo.InvariantCulture),
				worldPosition,
				0);
		}

		int typedSeed = (int)starSeed;
		if (_jumpRouteSystemCache.TryGetValue(typedSeed, out JumpLaneSystem? cachedSystem))
		{
			return CloneRouteSystem(cachedSystem);
		}

		GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(worldPosition, typedSeed, _spec);
		SolarSystem? system = GenerateJumpRoutePopulationSystem(star);

		JumpLaneSystem routeSystem = new(
			starSeed.ToString(CultureInfo.InvariantCulture),
			worldPosition,
			0);

		if (system != null)
		{
			if (UseTravellerRouteMode() && system.TravellerProfile != null)
			{
				routeSystem.Population = system.TravellerProfile.RouteProfile.EstimatedPopulation;
				routeSystem.TravellerProfile = TravellerSystemProfile.FromDictionary(system.TravellerProfile.ToDictionary());
				routeSystem.RouteTechnologyLevel = system.TravellerProfile.WorldProfile.TechLevelCode;
				routeSystem.GovernmentSummary = $"Gov {TravellerWorldProfile.ToHexDigit(system.TravellerProfile.WorldProfile.GovernmentCode)}";
				routeSystem.TradeCodesSummary = system.TravellerProfile.TradeCodes.ToDisplayString();
			}
			else
			{
				routeSystem.Population = system.GetTotalPopulation();
				ApplyColonizationSummary(routeSystem, system);
			}
		}

		_jumpRouteSystemCache[typedSeed] = CloneRouteSystem(routeSystem);
		return routeSystem;
	}

	private SolarSystem? GenerateJumpRoutePopulationSystem(GalaxyStar star)
	{
		SolarSystemSpec spec = SolarSystemSpec.RandomSmall(star.StarSeed);
		spec.SystemMetallicity = star.Metallicity;
		spec.IncludeAsteroidBelts = false;
		spec.GeneratePopulation = true;
		if (_galaxyConfig != null && _galaxyConfig.UseCaseSettings != null)
		{
			spec.UseCaseSettings = _galaxyConfig.UseCaseSettings.Clone();
		}

		return SystemFixtureGenerator.GenerateSystem(spec, true);
	}

	private static void ApplyColonizationSummary(JumpLaneSystem routeSystem, SolarSystem system)
	{
		double bestExportPressure = 0.0;
		int bestExportTech = -1;
		int bestObservedTech = -1;
		string bestExportBodyId = string.Empty;
		string bestExportCivilizationId = string.Empty;
		string bestExportCivilizationName = string.Empty;
		double bestTargetScore = 0.0;
		int bestTargetCapacity = 0;
		string bestTargetBodyId = string.Empty;
		int bestHabitabilityScore = -1;
		int bestResourceScore = -1;
		int bestSummaryPopulation = -1;
		string bestGovernmentSummary = string.Empty;

		foreach (CelestialBody body in system.Bodies.Values)
		{
			if (body.PopulationData == null)
			{
				continue;
			}

			PlanetPopulationData data = body.PopulationData;
			ColonySuitability? suitability = data.Suitability;
			if (data.Profile != null)
			{
				if (data.Profile.HabitabilityScore > bestHabitabilityScore)
				{
					bestHabitabilityScore = data.Profile.HabitabilityScore;
				}
			}

			if (suitability != null)
			{
				int resourceScore = suitability.GetFactorScore(ColonySuitability.FactorType.Resources);
				if (resourceScore > bestResourceScore)
				{
					bestResourceScore = resourceScore;
				}

				double targetScore = ColonizationRouteCalculator.CalculateColonyTargetScore(suitability);
				int targetCapacity = suitability.CarryingCapacity;
				bool replaceTarget = false;
				if (targetScore > bestTargetScore)
				{
					replaceTarget = true;
				}
				else if (System.Math.Abs(targetScore - bestTargetScore) < 0.0001 && targetCapacity > bestTargetCapacity)
				{
					replaceTarget = true;
				}
				else if (System.Math.Abs(targetScore - bestTargetScore) < 0.0001
					&& targetCapacity == bestTargetCapacity
					&& string.CompareOrdinal(body.Id, bestTargetBodyId) < 0)
				{
					replaceTarget = true;
				}

				if (replaceTarget)
				{
					bestTargetScore = targetScore;
					bestTargetCapacity = targetCapacity;
					bestTargetBodyId = body.Id;
				}
			}

			int bodyPopulation = data.GetTotalPopulation();
			int bodyTech = ResolveHighestExportTechLevel(data);
			if (bodyTech > bestObservedTech)
			{
				bestObservedTech = bodyTech;
			}

			string governmentSummary = ResolveDominantGovernmentSummary(data);
			if (bodyPopulation > bestSummaryPopulation && !string.IsNullOrEmpty(governmentSummary))
			{
				bestSummaryPopulation = bodyPopulation;
				bestGovernmentSummary = governmentSummary;
			}

			if (bodyPopulation <= 0 || suitability == null)
			{
				continue;
			}

			int carryingCapacity = suitability.CarryingCapacity;
			int exportTech = bodyTech;
			if (exportTech < (int)TechnologyLevel.Level.Interstellar)
			{
				continue;
			}

			double exportPressure = ColonizationRouteCalculator.CalculateExportPressure(bodyPopulation, carryingCapacity);
			if (exportPressure <= 0.0)
			{
				continue;
			}

			bool replaceExporter = false;
			if (exportPressure > bestExportPressure)
			{
				replaceExporter = true;
			}
			else if (System.Math.Abs(exportPressure - bestExportPressure) < 0.0001 && exportTech > bestExportTech)
			{
				replaceExporter = true;
			}
			else if (System.Math.Abs(exportPressure - bestExportPressure) < 0.0001
				&& exportTech == bestExportTech
				&& string.CompareOrdinal(body.Id, bestExportBodyId) < 0)
			{
				replaceExporter = true;
			}

			if (replaceExporter)
			{
				bestExportPressure = exportPressure;
				bestExportTech = exportTech;
				bestExportBodyId = body.Id;
				ResolveExportCivilization(data, out bestExportCivilizationId, out bestExportCivilizationName);
			}
		}

		routeSystem.ColonyTargetScore = bestTargetScore;
		routeSystem.ColonyTargetCapacity = bestTargetCapacity;
		routeSystem.ColonyTargetBodyId = bestTargetBodyId;
		routeSystem.HabitabilityScore = bestHabitabilityScore;
		routeSystem.ResourceScore = bestResourceScore;
		routeSystem.GovernmentSummary = bestGovernmentSummary;
		routeSystem.RouteTechnologyLevel = bestObservedTech;
		if (bestExportTech >= (int)TechnologyLevel.Level.Interstellar)
		{
			routeSystem.CanExportColonists = true;
			routeSystem.ExportPressure = bestExportPressure;
			routeSystem.RouteTechnologyLevel = bestExportTech;
			routeSystem.ColonizationRangePc = ColonizationRouteCalculator.DetermineColonizationRange(bestExportTech);
			routeSystem.ExportBodyId = bestExportBodyId;
			routeSystem.ExportCivilizationId = bestExportCivilizationId;
			routeSystem.ExportCivilizationName = bestExportCivilizationName;
		}
	}

	private static void ResolveExportCivilization(
		PlanetPopulationData data,
		out string civilizationId,
		out string civilizationName)
	{
		civilizationId = string.Empty;
		civilizationName = string.Empty;
		int bestTech = -1;
		int bestPopulation = -1;

		foreach (NativePopulation nativePopulation in data.NativePopulations)
		{
			if (!nativePopulation.IsExtant)
			{
				continue;
			}

			int tech = (int)nativePopulation.TechLevel;
			bool replace = false;
			if (tech > bestTech)
			{
				replace = true;
			}
			else if (tech == bestTech && nativePopulation.Population > bestPopulation)
			{
				replace = true;
			}

			if (replace)
			{
				bestTech = tech;
				bestPopulation = nativePopulation.Population;
				civilizationId = nativePopulation.Id;
				civilizationName = nativePopulation.Name;
			}
		}

		foreach (Colony colony in data.Colonies)
		{
			if (!colony.IsActive)
			{
				continue;
			}

			int tech = (int)colony.TechLevel;
			bool replace = false;
			if (tech > bestTech)
			{
				replace = true;
			}
			else if (tech == bestTech && colony.Population > bestPopulation)
			{
				replace = true;
			}

			if (replace)
			{
				bestTech = tech;
				bestPopulation = colony.Population;
				civilizationId = colony.FoundingCivilizationId;
				civilizationName = colony.FoundingCivilizationName;
			}
		}
	}

	private static int ResolveHighestExportTechLevel(PlanetPopulationData data)
	{
		int highestTech = -1;
		foreach (NativePopulation nativePopulation in data.NativePopulations)
		{
			if (!nativePopulation.IsExtant)
			{
				continue;
			}

			int nativeTech = (int)nativePopulation.TechLevel;
			if (nativeTech > highestTech)
			{
				highestTech = nativeTech;
			}
		}

		foreach (Colony colony in data.Colonies)
		{
			if (!colony.IsActive)
			{
				continue;
			}

			int colonyTech = (int)colony.TechLevel;
			if (colonyTech > highestTech)
			{
				highestTech = colonyTech;
			}
		}

		return highestTech;
	}

	private static string ResolveDominantGovernmentSummary(PlanetPopulationData data)
	{
		int bestPopulation = -1;
		string regime = string.Empty;

		foreach (NativePopulation nativePopulation in data.NativePopulations)
		{
			if (!nativePopulation.IsExtant)
			{
				continue;
			}

			if (nativePopulation.Population > bestPopulation)
			{
				bestPopulation = nativePopulation.Population;
				regime = GovernmentType.ToStringName(nativePopulation.GetRegime());
			}
		}

		foreach (Colony colony in data.Colonies)
		{
			if (!colony.IsActive)
			{
				continue;
			}

			if (colony.Population > bestPopulation)
			{
				bestPopulation = colony.Population;
				regime = GovernmentType.ToStringName(colony.GetRegime());
			}
		}

		return regime;
	}

	private bool UseTravellerRouteMode()
	{
		return _galaxyConfig != null
			&& _galaxyConfig.UseCaseSettings != null
			&& _galaxyConfig.UseCaseSettings.IsTravellerMode();
	}

	private static JumpLaneSystem CloneRouteSystem(JumpLaneSystem source)
	{
		JumpLaneSystem clone = new(source.Id, source.Position, source.Population);
		clone.FalsePopulation = source.FalsePopulation;
		clone.IsBridge = source.IsBridge;
		clone.CanExportColonists = source.CanExportColonists;
		clone.ExportPressure = source.ExportPressure;
		clone.ColonyTargetScore = source.ColonyTargetScore;
		clone.ColonyTargetCapacity = source.ColonyTargetCapacity;
		clone.ColonizationRangePc = source.ColonizationRangePc;
		clone.RouteTechnologyLevel = source.RouteTechnologyLevel;
		clone.ExportBodyId = source.ExportBodyId;
		clone.ColonyTargetBodyId = source.ColonyTargetBodyId;
		clone.ExportCivilizationId = source.ExportCivilizationId;
		clone.ExportCivilizationName = source.ExportCivilizationName;
		clone.HabitabilityScore = source.HabitabilityScore;
		clone.ResourceScore = source.ResourceScore;
		clone.GovernmentSummary = source.GovernmentSummary;
		clone.TradeCodesSummary = source.TradeCodesSummary;
		if (source.TravellerProfile != null)
		{
			clone.TravellerProfile = TravellerSystemProfile.FromDictionary(source.TravellerProfile.ToDictionary());
		}

		return clone;
	}

}
