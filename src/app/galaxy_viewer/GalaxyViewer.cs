using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Rng;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Compile-ready C# top-level controller for the galaxy-viewer slice.
/// </summary>
public partial class GalaxyViewer : Node3D
{
	/// <summary>
	/// Emitted when a star is selected.
	/// </summary>
	[Signal]
	public delegate void StarSelectedEventHandler(Vector3 worldPosition, int starSeed);

	/// <summary>
	/// Emitted when the user requests to open a selected system.
	/// </summary>
	[Signal]
	public delegate void OpenSystemRequestedEventHandler(int starSeed, Vector3 worldPosition);

	/// <summary>
	/// Emitted when the galaxy seed changes.
	/// </summary>
	[Signal]
	public delegate void GalaxySeedChangedEventHandler(int newSeed);

	/// <summary>
	/// Emitted when the user requests a new galaxy.
	/// </summary>
	[Signal]
	public delegate void NewGalaxyRequestedEventHandler();

	/// <summary>
	/// Master seed for the galaxy.
	/// </summary>
	[Export]
	public int GalaxySeed = 42;

	/// <summary>
	/// Total sample points at galaxy scale.
	/// </summary>
	[Export]
	public int NumPoints = 100000;

	/// <summary>
	/// World-space star billboard size.
	/// </summary>
	[Export]
	public float StarSize = 80.0f;

	/// <summary>
	/// Whether the viewer starts at the home location.
	/// </summary>
	[Export]
	public bool StartAtHome = true;

	private Label? _statusLabel;
	private SpinBox? _seedInput;
	private CheckBox? _showCompassCheck;
	private Node? _inspectorPanel;
	private Button? _saveButton;
	private Button? _loadButton;
	private Button? _newGalaxyButton;

	private Galaxy? _galaxy;
	private GalaxySpec? _spec;
	private GalaxyConfig? _galaxyConfig;
	private ZoomStateMachine? _zoomMachine;
	private GalaxyRenderer? _galaxyRenderer;
	private QuadrantRenderer? _quadrantRenderer;
	private QuadrantSelector? _quadrantSelector;
	private GridCursor? _quadrantCursor;
	private GridCursor? _sectorCursor;
	private OrbitCamera? _orbitCamera;
	private StarViewCamera? _starCamera;
	private NavigationCompass? _compass;
	private SelectionIndicator? _selectionIndicator;
	private Node? _sectorRenderer;
	private Node? _neighborhoodRenderer;
	private SectorJumpLaneRenderer? _sectorJumpLaneRenderer;

	private Variant _selectedSector = default;
	private Vector3 _selectedStarPosition = Vector3.Zero;
	private int _selectedStarSeed;
	private StarSystemPreviewData? _starPreview;

	private int _savedZoomLevel = -1;
	private Variant _savedQuadrant = default;
	private Variant _savedSector = default;
	private Vector3 _savedStarCameraPosition = Vector3.Zero;
	private Vector3 _savedStarCameraRotation = Vector3.Zero;
	private int _savedStarSeed;
	private Vector3 _savedStarPosition = Vector3.Zero;

	private JumpLaneRegion? _jumpLaneRegion;
	private JumpLaneResult? _jumpLaneResult;
	private readonly GalaxyViewerSaveLoad _saveLoad = new();

	/// <summary>
	/// Initializes controller state and helper objects.
	/// </summary>
	public override void _Ready()
	{
		CacheNodeReferences();
		InitializeState();
		BuildStaticRenderers();
		ConnectUiSignals();
		UpdateSeedDisplay();

		if (StartAtHome)
		{
			InitializeAtHome();
		}
		else
		{
			ApplyZoomLevel((int)GalaxyCoordinates.ZoomLevel.Galaxy);
			UpdateInspector();
			SetStatus("Galaxy viewer ready");
		}
	}

	/// <summary>
	/// Keeps the compass aligned with the orbit camera.
	/// </summary>
	public override void _Process(double delta)
	{
		if (_compass != null && _compass.Visible && _orbitCamera != null)
		{
			_compass.SyncRotation(_orbitCamera.GetYawDeg(), _orbitCamera.GetPitchDeg());
		}
	}

	/// <summary>
	/// Handles minimal key and mouse input for the compile-ready slice.
	/// </summary>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent)
		{
			HandleKeyInput(keyEvent);
			return;
		}

		if (@event is InputEventMouseButton mouseButton)
		{
			HandleMouseClick(mouseButton);
		}
	}

	/// <summary>
	/// Handles the keyboard shortcuts used by the current slice.
	/// </summary>
	private void HandleKeyInput(InputEventKey keyEvent)
	{
		if (!keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		int currentLevel = _zoomMachine?.GetCurrentLevel() ?? (int)GalaxyCoordinates.ZoomLevel.Galaxy;

		switch (keyEvent.Keycode)
		{
			case Key.Bracketright:
				TryZoomIn(currentLevel);
				break;
			case Key.Bracketleft:
				_zoomMachine?.ZoomOut();
				ApplyZoomLevel(_zoomMachine?.GetCurrentLevel() ?? (int)GalaxyCoordinates.ZoomLevel.Galaxy);
				break;
			case Key.Enter:
			case Key.KpEnter:
				TryOpenSelectedSystem();
				break;
			case Key.Escape:
				ClearStarSelection();
				break;
		}
	}

	/// <summary>
	/// Handles left-click selection in the current zoom level.
	/// </summary>
	private void HandleMouseClick(InputEventMouseButton mouseButton)
	{
		if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		int currentLevel = _zoomMachine?.GetCurrentLevel() ?? (int)GalaxyCoordinates.ZoomLevel.Galaxy;
		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Quadrant)
		{
			PickQuadrantAt(mouseButton.Position);
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Sector)
		{
			PickSectorAt(mouseButton.Position);
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector)
		{
			PickStarAt(mouseButton.Position);
		}
	}

	/// <summary>
	/// Updates the status label.
	/// </summary>
	public void set_status(string message)
	{
		SetStatus(message);
	}

	/// <summary>
	/// Updates the status label.
	/// </summary>
	public void SetStatus(string message)
	{
		if (_statusLabel != null)
		{
			_statusLabel.Text = message;
			_statusLabel.Modulate = new Color(0.7f, 0.7f, 0.7f, 1.0f);
		}
	}

	/// <summary>
	/// Saves the current viewer state.
	/// </summary>
	public void save_state()
	{
		_saveLoad.SaveState(this);
	}

	/// <summary>
	/// Restores the previously saved viewer state.
	/// </summary>
	public void restore_state()
	{
		_saveLoad.RestoreState(this);
	}

	/// <summary>
	/// Returns whether saved state exists.
	/// </summary>
	public bool has_saved_state()
	{
		return _saveLoad.HasSavedState(this);
	}

	/// <summary>
	/// Applies loaded save data to core state.
	/// </summary>
	public void apply_save_data(Variant dataVariant)
	{
		GalaxySaveData? saveData = dataVariant.As<GalaxySaveData>();
		if (saveData == null)
		{
			return;
		}

		GalaxySeed = saveData.GalaxySeed;
		_galaxyConfig = saveData.GetConfig() ?? _galaxyConfig ?? GalaxyConfig.CreateDefault();
		_galaxy = new Galaxy(_galaxyConfig, GalaxySeed);
		_spec = _galaxy.Spec;
		_selectedStarSeed = saveData.SelectedStarSeed;
		_selectedStarPosition = saveData.SelectedStarPosition;
		_savedZoomLevel = saveData.ZoomLevel;
		_savedQuadrant = saveData.SelectedQuadrant.HasValue ? Variant.CreateFrom(saveData.SelectedQuadrant.Value) : default;
		_savedSector = saveData.SelectedSector.HasValue ? Variant.CreateFrom(saveData.SelectedSector.Value) : default;
		UpdateSeedDisplay();
		UpdateInspector();
		EmitSignal(SignalName.GalaxySeedChanged, GalaxySeed);
	}

	/// <summary>
	/// Sets a new galaxy configuration.
	/// </summary>
	public void set_galaxy_config(Variant configVariant)
	{
		GalaxyConfig? config = ConvertGalaxyConfig(configVariant);
		if (config == null)
		{
			return;
		}

		_galaxyConfig = config;
		_galaxy = new Galaxy(_galaxyConfig, GalaxySeed);
		_spec = _galaxy.Spec;
		UpdateInspector();
	}

	/// <summary>
	/// Returns the current zoom-state machine.
	/// </summary>
	public ZoomStateMachine? get_zoom_machine() => _zoomMachine;

	/// <summary>
	/// Returns the quadrant selector.
	/// </summary>
	public QuadrantSelector? get_quadrant_selector() => _quadrantSelector;

	/// <summary>
	/// Returns the quadrant cursor.
	/// </summary>
	public GridCursor? get_quadrant_cursor() => _quadrantCursor;

	/// <summary>
	/// Returns the sector cursor.
	/// </summary>
	public GridCursor? get_sector_cursor() => _sectorCursor;

	/// <summary>
	/// Returns the star camera if present.
	/// </summary>
	public StarViewCamera? get_star_camera() => _starCamera;

	/// <summary>
	/// Returns the selection indicator if present.
	/// </summary>
	public SelectionIndicator? get_selection_indicator() => _selectionIndicator;

	/// <summary>
	/// Returns the current sector renderer node.
	/// </summary>
	public Node? get_sector_renderer() => _sectorRenderer;

	/// <summary>
	/// Returns the current neighborhood renderer node.
	/// </summary>
	public Node? get_neighborhood_renderer() => _neighborhoodRenderer;

	/// <summary>
	/// Returns the selected sector payload.
	/// </summary>
	public Variant get_selected_sector_internal() => _selectedSector;

	/// <summary>
	/// Sets the selected sector payload.
	/// </summary>
	public void set_selected_sector_internal(Variant value) => _selectedSector = value;

	/// <summary>
	/// Returns the selected star seed.
	/// </summary>
	public int get_selected_star_seed_internal() => _selectedStarSeed;

	/// <summary>
	/// Returns the selected star position.
	/// </summary>
	public Vector3 get_selected_star_position_internal() => _selectedStarPosition;

	/// <summary>
	/// Returns the current star preview.
	/// </summary>
	public StarSystemPreviewData? get_star_preview() => _starPreview;

	/// <summary>
	/// Returns the galaxy config.
	/// </summary>
	public GalaxyConfig? get_galaxy_config() => _galaxyConfig;

	/// <summary>
	/// Returns the galaxy model.
	/// </summary>
	public Galaxy? get_galaxy() => _galaxy;

	/// <summary>
	/// Returns the active density model.
	/// </summary>
	public DensityModelInterface? get_density_model() => _galaxy?.DensityModel;

	/// <summary>
	/// Returns the reference density.
	/// </summary>
	public float get_reference_density() => _galaxy?.ReferenceDensity ?? 0.0f;

	/// <summary>
	/// Returns the current jump-lane region.
	/// </summary>
	public JumpLaneRegion? get_jump_lane_region() => _jumpLaneRegion;

	/// <summary>
	/// Returns the cached jump-lane result.
	/// </summary>
	public JumpLaneResult? get_jump_lane_result() => _jumpLaneResult;

	/// <summary>
	/// Sets the cached jump-lane result.
	/// </summary>
	public void set_jump_lane_result(JumpLaneResult? result) => _jumpLaneResult = result;

	/// <summary>
	/// Applies home initialization.
	/// </summary>
	public void call_initialize_at_home() => InitializeAtHome();

	/// <summary>
	/// Applies a zoom level.
	/// </summary>
	public void call_apply_zoom_level(int level) => ApplyZoomLevel(level);

	/// <summary>
	/// Applies a star selection.
	/// </summary>
	public void call_apply_star_selection(Vector3 worldPosition, int starSeed) => ApplyStarSelection(worldPosition, starSeed);

	/// <summary>
	/// Refreshes the inspector panel.
	/// </summary>
	public void call_update_inspector() => UpdateInspector();

	/// <summary>
	/// Returns the saved zoom level.
	/// </summary>
	public int get_saved_zoom_level() => _savedZoomLevel;

	/// <summary>
	/// Sets the saved zoom level.
	/// </summary>
	public void set_saved_zoom_level(int level) => _savedZoomLevel = level;

	/// <summary>
	/// Returns the saved quadrant payload.
	/// </summary>
	public Variant get_saved_quadrant() => _savedQuadrant;

	/// <summary>
	/// Sets the saved quadrant payload.
	/// </summary>
	public void set_saved_quadrant(Variant value) => _savedQuadrant = value;

	/// <summary>
	/// Returns the saved sector payload.
	/// </summary>
	public Variant get_saved_sector() => _savedSector;

	/// <summary>
	/// Sets the saved sector payload.
	/// </summary>
	public void set_saved_sector(Variant value) => _savedSector = value;

	/// <summary>
	/// Returns the saved star-camera position.
	/// </summary>
	public Vector3 get_saved_star_camera_position() => _savedStarCameraPosition;

	/// <summary>
	/// Sets the saved star-camera position.
	/// </summary>
	public void set_saved_star_camera_position(Vector3 value) => _savedStarCameraPosition = value;

	/// <summary>
	/// Returns the saved star-camera rotation.
	/// </summary>
	public Vector3 get_saved_star_camera_rotation() => _savedStarCameraRotation;

	/// <summary>
	/// Sets the saved star-camera rotation.
	/// </summary>
	public void set_saved_star_camera_rotation(Vector3 value) => _savedStarCameraRotation = value;

	/// <summary>
	/// Returns the saved star seed.
	/// </summary>
	public int get_saved_star_seed() => _savedStarSeed;

	/// <summary>
	/// Sets the saved star seed.
	/// </summary>
	public void set_saved_star_seed(int value) => _savedStarSeed = value;

	/// <summary>
	/// Returns the saved star position.
	/// </summary>
	public Vector3 get_saved_star_position() => _savedStarPosition;

	/// <summary>
	/// Sets the saved star position.
	/// </summary>
	public void set_saved_star_position(Vector3 value) => _savedStarPosition = value;

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
	/// Initializes the viewer at the home position.
	/// </summary>
	private void InitializeAtHome()
	{
		HierarchyCoords hierarchy = HomePosition.GetHomeHierarchy();
		_quadrantCursor!.Position = hierarchy.QuadrantCoords;
		_quadrantSelector!.SetSelection(Variant.CreateFrom(hierarchy.QuadrantCoords));
		_selectedSector = Variant.CreateFrom(hierarchy.SectorLocalCoords);
		_sectorCursor!.Position = hierarchy.SectorLocalCoords;
		_zoomMachine!.SetLevel((int)GalaxyCoordinates.ZoomLevel.Subsector);
		ApplyZoomLevel((int)GalaxyCoordinates.ZoomLevel.Subsector);
		UpdateInspector();

		Vector3 homePosition = HomePosition.GetDefaultPosition();
		double distKpc = homePosition.Length() / 1000.0;
		SetStatus($"Home sector - {distKpc:0.0} kpc from galactic center");
	}

	/// <summary>
	/// Applies a zoom-level transition.
	/// </summary>
	private void ApplyZoomLevel(int level)
	{
		_zoomMachine?.SetLevel(level);
		bool quadrantSelected = _quadrantSelector?.HasSelection() ?? false;
		bool sectorSelected = _selectedSector.VariantType == Variant.Type.Vector3I;
		bool subsectorActive = level == (int)GalaxyCoordinates.ZoomLevel.Subsector && quadrantSelected && sectorSelected;

		if (_galaxyRenderer != null)
		{
			_galaxyRenderer.Visible = level == (int)GalaxyCoordinates.ZoomLevel.Galaxy
				|| level == (int)GalaxyCoordinates.ZoomLevel.Quadrant
				|| level == (int)GalaxyCoordinates.ZoomLevel.Sector
				|| level == (int)GalaxyCoordinates.ZoomLevel.Subsector;
			_galaxyRenderer.SetOpacity(level switch
			{
				(int)GalaxyCoordinates.ZoomLevel.Galaxy => 1.0f,
				(int)GalaxyCoordinates.ZoomLevel.Subsector => 0.05f,
				_ => 0.15f,
			});
		}

		if (_quadrantRenderer != null)
		{
			_quadrantRenderer.Visible = level == (int)GalaxyCoordinates.ZoomLevel.Quadrant
				|| level == (int)GalaxyCoordinates.ZoomLevel.Sector;
			if (level == (int)GalaxyCoordinates.ZoomLevel.Quadrant)
			{
				if (!quadrantSelected)
				{
					_quadrantRenderer.SetHighlight(default);
				}
				else if (_quadrantSelector != null)
				{
					_quadrantRenderer.SetHighlight(_quadrantSelector.SelectedCoords);
				}
			}
			else
			{
				_quadrantRenderer.SetHighlight(default);
			}
		}

		if (_sectorRenderer is SectorRenderer sectorRenderer)
		{
			if (level == (int)GalaxyCoordinates.ZoomLevel.Sector && quadrantSelected && _quadrantSelector != null)
			{
				Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
				if (_galaxy != null)
				{
					sectorRenderer.BuildForQuadrant(quadrantCoords, _galaxy.DensityModel);
				}

				sectorRenderer.Visible = true;
				sectorRenderer.SetHighlight(sectorSelected ? _selectedSector : default);

				if (_orbitCamera != null)
				{
					_orbitCamera.Current = true;
					_orbitCamera.ReconfigureConstraints(
						50.0f,
						5000.0f,
						GalaxyCoordinates.QuadrantToParsecCenter(quadrantCoords));
				}
			}
			else
			{
				sectorRenderer.Visible = false;
			}
		}

		if (_neighborhoodRenderer is NeighborhoodRenderer neighborhoodRenderer && subsectorActive && _quadrantSelector != null)
		{
			Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
			Vector3I sectorCoords = (Vector3I)_selectedSector;
			Vector3 sectorCenter = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, sectorCoords)
				+ (Vector3.One * ((float)GalaxyCoordinates.SectorSizePc * 0.5f));

			if (_starCamera != null)
			{
				_starCamera.Configure(sectorCenter);
				_starCamera.Current = true;
				_starCamera.SetProcess(true);
			}

			if (_orbitCamera != null)
			{
				_orbitCamera.Current = false;
			}

			if (_galaxy != null)
			{
				neighborhoodRenderer.BuildNeighborhood(
					sectorCenter,
					GalaxySeed,
					_galaxy.DensityModel,
					_galaxy.ReferenceDensity);
			}

			neighborhoodRenderer.Visible = true;
		}
		else if (_neighborhoodRenderer is Node3D neighborhoodNode)
		{
			neighborhoodNode.Visible = false;
		}

		if (!subsectorActive)
		{
			if (_starCamera != null)
			{
				_starCamera.Current = false;
				_starCamera.SetProcess(false);
			}

			if (_orbitCamera != null)
			{
				_orbitCamera.Current = true;
			}
		}

		_sectorJumpLaneRenderer?.Clear();
		if (_sectorJumpLaneRenderer != null)
		{
			_sectorJumpLaneRenderer.Visible = false;
		}

		if (subsectorActive && _selectedStarSeed != 0)
		{
			_selectionIndicator?.ShowAt(_selectedStarPosition);
		}
		else
		{
			_selectionIndicator?.HideIndicator();
		}

		if (_compass != null)
		{
			_compass.Visible = _showCompassCheck?.ButtonPressed == true
				&& (level == (int)GalaxyCoordinates.ZoomLevel.Quadrant
				|| level == (int)GalaxyCoordinates.ZoomLevel.Sector);
		}

		UpdateInspector();
	}

	/// <summary>
	/// Applies the gated zoom-in rules from the GDScript viewer.
	/// </summary>
	private void TryZoomIn(int currentLevel)
	{
		if (_zoomMachine == null)
		{
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Galaxy)
		{
			_zoomMachine.ZoomIn();
			ApplyZoomLevel(_zoomMachine.GetCurrentLevel());
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Quadrant && (_quadrantSelector?.HasSelection() ?? false))
		{
			_zoomMachine.ZoomIn();
			ApplyZoomLevel(_zoomMachine.GetCurrentLevel());
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Sector && _selectedSector.VariantType == Variant.Type.Vector3I)
		{
			_zoomMachine.ZoomIn();
			ApplyZoomLevel(_zoomMachine.GetCurrentLevel());
		}
	}

	/// <summary>
	/// Picks a quadrant from the current orbit-camera ray.
	/// </summary>
	private void PickQuadrantAt(Vector2 screenPosition)
	{
		if (_orbitCamera == null || _quadrantSelector == null || _quadrantRenderer == null)
		{
			return;
		}

		Vector3 rayOrigin = _orbitCamera.ProjectRayOrigin(screenPosition);
		Vector3 rayDirection = _orbitCamera.ProjectRayNormal(screenPosition);
		Variant picked = _quadrantSelector.PickFromRay(rayOrigin, rayDirection, _quadrantRenderer.get_occupied_coords());
		if (picked.VariantType == Variant.Type.Vector3I)
		{
			SelectQuadrant((Vector3I)picked);
		}
	}

	/// <summary>
	/// Stores and displays the current quadrant selection.
	/// </summary>
	private void SelectQuadrant(Vector3I coords)
	{
		if (_quadrantCursor != null)
		{
			_quadrantCursor.Position = coords;
		}

		_quadrantSelector?.SetSelection(Variant.CreateFrom(coords));
		_quadrantRenderer?.SetHighlight(Variant.CreateFrom(coords));
		_selectedSector = default;

		if (_sectorRenderer is SectorRenderer sectorRenderer)
		{
			sectorRenderer.SetHighlight(default);
		}

		if (_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel)
		{
			float density = _galaxy?.DensityModel.GetDensity(GalaxyCoordinates.QuadrantToParsecCenter(coords)) ?? 0.0f;
			typedInspectorPanel.DisplaySelectedQuadrant(coords, density);
		}

		SetStatus($"Selected quadrant ({coords.X}, {coords.Y}, {coords.Z})");
	}

	/// <summary>
	/// Picks a sector from the current orbit-camera ray.
	/// </summary>
	private void PickSectorAt(Vector2 screenPosition)
	{
		if (_orbitCamera == null || _sectorRenderer is not SectorRenderer sectorRenderer)
		{
			return;
		}

		Godot.Collections.Array<Vector3I> occupied = sectorRenderer.get_occupied_coords();
		if (occupied.Count == 0)
		{
			return;
		}

		Vector3 rayOrigin = _orbitCamera.ProjectRayOrigin(screenPosition);
		Vector3 rayDirection = _orbitCamera.ProjectRayNormal(screenPosition);
		Vector3I? bestCoords = null;
		float bestDistance = float.PositiveInfinity;

		foreach (Vector3I coords in occupied)
		{
			Godot.Collections.Array<Vector3> aabb = sectorRenderer.get_sector_world_aabb(coords);
			float hitDistance = RaycastUtils.RayIntersectsAabb(rayOrigin, rayDirection, aabb[0], aabb[1]);
			if (hitDistance >= 0.0f && hitDistance < bestDistance)
			{
				bestDistance = hitDistance;
				bestCoords = coords;
			}
		}

		if (bestCoords.HasValue)
		{
			SelectSector(bestCoords.Value);
		}
	}

	/// <summary>
	/// Stores and displays the current sector selection.
	/// </summary>
	private void SelectSector(Vector3I coords)
	{
		if (_sectorCursor != null)
		{
			_sectorCursor.Position = coords;
		}

		_selectedSector = Variant.CreateFrom(coords);
		if (_sectorRenderer is SectorRenderer sectorRenderer)
		{
			sectorRenderer.SetHighlight(_selectedSector);
		}

		if (
			_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel &&
			_quadrantSelector != null &&
			_quadrantSelector.HasSelection() &&
			_quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
		{
			Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
			Vector3 sectorCenter = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, coords)
				+ (Vector3.One * ((float)GalaxyCoordinates.SectorSizePc * 0.5f));
			float density = _galaxy?.DensityModel.GetDensity(sectorCenter) ?? 0.0f;
			typedInspectorPanel.DisplaySelectedSector(quadrantCoords, coords, density);
		}

		SetStatus($"Selected sector ({coords.X}, {coords.Y}, {coords.Z})");
	}

	/// <summary>
	/// Picks a star from the current star-camera ray.
	/// </summary>
	private void PickStarAt(Vector2 screenPosition)
	{
		if (_starCamera == null || _neighborhoodRenderer is not NeighborhoodRenderer neighborhoodRenderer)
		{
			return;
		}

		Vector3 rayOrigin = _starCamera.ProjectRayOrigin(screenPosition);
		Vector3 rayDirection = _starCamera.ProjectRayNormal(screenPosition);
		Variant picked = neighborhoodRenderer.pick_star(rayOrigin, rayDirection);
		GodotObject? pickedObject = picked.AsGodotObject();
		if (pickedObject is StarPickResult result)
		{
			ApplyStarSelection(result.WorldPosition, (int)result.StarSeed);
			return;
		}

		ClearStarSelection();
	}

	/// <summary>
	/// Applies a star selection and updates the inspector.
	/// </summary>
	private void ApplyStarSelection(Vector3 worldPosition, int starSeed)
	{
		_selectedStarPosition = worldPosition;
		_selectedStarSeed = starSeed;
		_starPreview = _spec == null ? null : StarSystemPreview.Generate(starSeed, worldPosition, _spec);
		_selectionIndicator?.ShowAt(worldPosition);
		if (_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel)
		{
			typedInspectorPanel.DisplaySelectedStar(worldPosition, starSeed);
			typedInspectorPanel.DisplaySystemPreview(_starPreview);
		}
		else
		{
			_inspectorPanel?.Call("display_selected_star", worldPosition, starSeed);
			Variant previewVariant = _starPreview == null ? default : Variant.CreateFrom(_starPreview);
			_inspectorPanel?.Call("display_system_preview", previewVariant);
		}
	}

	/// <summary>
	/// Refreshes the inspector state.
	/// </summary>
	private void UpdateInspector()
	{
		if (_inspectorPanel == null || _spec == null || _zoomMachine == null)
		{
			return;
		}

		if (_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel)
		{
			typedInspectorPanel.DisplayGalaxy(_spec, _zoomMachine.GetCurrentLevel());

			if (_selectedStarSeed != 0)
			{
				typedInspectorPanel.DisplaySelectedStar(_selectedStarPosition, _selectedStarSeed);
				if (_starPreview != null)
				{
					typedInspectorPanel.DisplaySystemPreview(_starPreview);
				}
				return;
			}

			if (
				(_zoomMachine.GetCurrentLevel() == (int)GalaxyCoordinates.ZoomLevel.Sector
				|| _zoomMachine.GetCurrentLevel() == (int)GalaxyCoordinates.ZoomLevel.Subsector) &&
				_selectedSector.VariantType == Variant.Type.Vector3I &&
				_quadrantSelector != null &&
				_quadrantSelector.HasSelection() &&
				_quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
			{
				Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
				Vector3I sectorCoords = (Vector3I)_selectedSector;
				Vector3 sectorCenter = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, sectorCoords)
					+ (Vector3.One * ((float)GalaxyCoordinates.SectorSizePc * 0.5f));
				float density = _galaxy?.DensityModel.GetDensity(sectorCenter) ?? 0.0f;
				typedInspectorPanel.DisplaySelectedSector(quadrantCoords, sectorCoords, density);
				return;
			}

			if (
				_zoomMachine.GetCurrentLevel() == (int)GalaxyCoordinates.ZoomLevel.Quadrant &&
				_quadrantSelector != null &&
				_quadrantSelector.HasSelection() &&
				_quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
			{
				Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
				float density = _galaxy?.DensityModel.GetDensity(GalaxyCoordinates.QuadrantToParsecCenter(quadrantCoords)) ?? 0.0f;
				typedInspectorPanel.DisplaySelectedQuadrant(quadrantCoords, density);
				return;
			}

			typedInspectorPanel.ClearSelection();
			return;
		}

		_inspectorPanel.Call("display_galaxy", _spec, _zoomMachine.GetCurrentLevel());
	}

	/// <summary>
	/// Clears the current star selection.
	/// </summary>
	private void ClearStarSelection()
	{
		_selectedStarSeed = 0;
		_selectedStarPosition = Vector3.Zero;
		_starPreview = null;
		_selectionIndicator?.HideIndicator();
		UpdateInspector();
	}

	/// <summary>
	/// Emits an open-system request when a star is selected.
	/// </summary>
	private void TryOpenSelectedSystem()
	{
		if (_selectedStarSeed != 0)
		{
			EmitSignal(SignalName.OpenSystemRequested, _selectedStarSeed, _selectedStarPosition);
		}
	}

	/// <summary>
	/// Forwards the inspector's open-system request through the viewer signal.
	/// </summary>
	private void OnInspectorOpenSystemRequested(int starSeed, Vector3 worldPosition)
	{
		if (starSeed == 0)
		{
			return;
		}

		_selectedStarSeed = starSeed;
		_selectedStarPosition = worldPosition;
		TryOpenSelectedSystem();
	}

	/// <summary>
	/// Converts a mixed GDScript/C# config payload into a C# galaxy config.
	/// </summary>
	private static GalaxyConfig? ConvertGalaxyConfig(Variant configVariant)
	{
		if (configVariant.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = configVariant.AsGodotObject();
		if (godotObject is GalaxyConfig typedConfig)
		{
			return typedConfig;
		}

		if (godotObject != null && godotObject.HasMethod("to_dict"))
		{
			Variant dictVariant = godotObject.Call("to_dict");
			if (dictVariant.VariantType == Variant.Type.Dictionary)
			{
				return GalaxyConfig.FromDictionary((Godot.Collections.Dictionary)dictVariant);
			}
		}

		return null;
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

		Vector3 cameraPosition = _starCamera?.GlobalPosition ?? newOrigin;
		neighborhoodRenderer.BuildNeighborhood(
			cameraPosition,
			GalaxySeed,
			_galaxy.DensityModel,
			_galaxy.ReferenceDensity);
		ClearStarSelection();
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
}
