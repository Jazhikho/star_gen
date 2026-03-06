using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// GDScript-bridge state accessors for GalaxyViewer.
/// These expose internal fields needed by GDScript helpers (SaveLoad, test framework).
/// </summary>
public partial class GalaxyViewer
{
	/// <summary>Returns the current zoom-state machine.</summary>
	public ZoomStateMachine? GetZoomMachine() => _zoomMachine;

	/// <summary>Returns the current zoom-state machine.</summary>
	public ZoomStateMachine? get_zoom_machine() => GetZoomMachine();

	/// <summary>Returns the quadrant selector.</summary>
	public QuadrantSelector? GetQuadrantSelector() => _quadrantSelector;

	/// <summary>Returns the quadrant selector.</summary>
	public QuadrantSelector? get_quadrant_selector() => GetQuadrantSelector();

	/// <summary>Returns the quadrant cursor.</summary>
	public GridCursor? GetQuadrantCursor() => _quadrantCursor;

	/// <summary>Returns the quadrant cursor.</summary>
	public GridCursor? get_quadrant_cursor() => GetQuadrantCursor();

	/// <summary>Returns the sector cursor.</summary>
	public GridCursor? GetSectorCursor() => _sectorCursor;

	/// <summary>Returns the sector cursor.</summary>
	public GridCursor? get_sector_cursor() => GetSectorCursor();

	/// <summary>Returns the star camera if present.</summary>
	public StarViewCamera? GetStarCamera() => _starCamera;

	/// <summary>Returns the star camera if present.</summary>
	public StarViewCamera? get_star_camera() => GetStarCamera();

	/// <summary>Returns the selection indicator if present.</summary>
	public SelectionIndicator? GetSelectionIndicator() => _selectionIndicator;

	/// <summary>Returns the selection indicator if present.</summary>
	public SelectionIndicator? get_selection_indicator() => GetSelectionIndicator();

	/// <summary>Returns the active inspector panel when it has the expected type.</summary>
	public GalaxyInspectorPanel? GetInspectorPanel() => _inspectorPanel as GalaxyInspectorPanel;

	/// <summary>Returns the current sector renderer node.</summary>
	public SectorRenderer? GetSectorRenderer() => _sectorRenderer as SectorRenderer;

	/// <summary>Returns the current sector renderer node.</summary>
	public Node? get_sector_renderer() => _sectorRenderer;

	/// <summary>Returns the current neighborhood renderer node.</summary>
	public NeighborhoodRenderer? GetNeighborhoodRenderer() => _neighborhoodRenderer as NeighborhoodRenderer;

	/// <summary>Returns the current neighborhood renderer node.</summary>
	public Node? get_neighborhood_renderer() => _neighborhoodRenderer;

	/// <summary>Returns the currently selected sector, if any.</summary>
	public Vector3I? GetSelectedSector()
	{
		if (_selectedSector.VariantType == Variant.Type.Vector3I)
		{
			return (Vector3I)_selectedSector;
		}

		return null;
	}

	/// <summary>Sets the currently selected sector.</summary>
	public void SetSelectedSector(Vector3I? value)
	{
		if (value.HasValue)
		{
			_selectedSector = Variant.CreateFrom(value.Value);
			return;
		}

		_selectedSector = default;
	}

	/// <summary>Returns the selected sector payload.</summary>
	public Variant get_selected_sector_internal() => _selectedSector;

	/// <summary>Sets the selected sector payload.</summary>
	public void set_selected_sector_internal(Variant value) => _selectedSector = value;

	/// <summary>Returns the selected star seed.</summary>
	public int GetSelectedStarSeed() => _selectedStarSeed;

	/// <summary>Returns the selected star seed.</summary>
	public int get_selected_star_seed_internal() => GetSelectedStarSeed();

	/// <summary>Returns the selected star position.</summary>
	public Vector3 GetSelectedStarPosition() => _selectedStarPosition;

	/// <summary>Returns the selected star position.</summary>
	public Vector3 get_selected_star_position_internal() => GetSelectedStarPosition();

	/// <summary>Sets the selected star state without updating UI.</summary>
	public void SetSelectedStarState(int starSeed, Vector3 worldPosition)
	{
		_selectedStarSeed = starSeed;
		_selectedStarPosition = worldPosition;
	}

	/// <summary>Clears the selected star state without updating UI.</summary>
	public void ClearSelectedStarState()
	{
		_selectedStarSeed = 0;
		_selectedStarPosition = Vector3.Zero;
		_starPreview = null;
	}

	/// <summary>Returns the current star preview.</summary>
	public StarSystemPreviewData? GetStarPreview() => _starPreview;

	/// <summary>Returns the current star preview.</summary>
	public StarSystemPreviewData? get_star_preview() => GetStarPreview();

	/// <summary>Returns the galaxy config.</summary>
	public GalaxyConfig? GetGalaxyConfig() => _galaxyConfig;

	/// <summary>Returns the galaxy config.</summary>
	public GalaxyConfig? get_galaxy_config() => GetGalaxyConfig();

	/// <summary>Returns the galaxy model.</summary>
	public Domain.Galaxy.Galaxy? GetGalaxy() => _galaxy;

	/// <summary>Returns the galaxy model.</summary>
	public Domain.Galaxy.Galaxy? get_galaxy() => GetGalaxy();

	/// <summary>Returns the active density model.</summary>
	public DensityModelInterface? GetDensityModel() => _galaxy?.DensityModel;

	/// <summary>Returns the active density model.</summary>
	public DensityModelInterface? get_density_model() => GetDensityModel();

	/// <summary>Returns the reference density.</summary>
	public float GetReferenceDensity()
	{
		if (_galaxy == null)
		{
			return 0.0f;
		}

		return _galaxy.ReferenceDensity;
	}

	/// <summary>Returns the reference density.</summary>
	public float get_reference_density() => GetReferenceDensity();

	/// <summary>Returns the current jump-lane region.</summary>
	public JumpLaneRegion? GetJumpLaneRegion() => _jumpLaneRegion;

	/// <summary>Returns the current jump-lane region.</summary>
	public JumpLaneRegion? get_jump_lane_region() => GetJumpLaneRegion();

	/// <summary>Sets the current jump-lane region.</summary>
	public void SetJumpLaneRegion(JumpLaneRegion? region) => _jumpLaneRegion = region;

	/// <summary>Returns the cached jump-lane result.</summary>
	public JumpLaneResult? GetJumpLaneResult() => _jumpLaneResult;

	/// <summary>Returns the cached jump-lane result.</summary>
	public JumpLaneResult? get_jump_lane_result() => GetJumpLaneResult();

	/// <summary>Sets the cached jump-lane result.</summary>
	public void SetJumpLaneResult(JumpLaneResult? result) => _jumpLaneResult = result;

	/// <summary>Sets the cached jump-lane result.</summary>
	public void set_jump_lane_result(JumpLaneResult? result) => SetJumpLaneResult(result);

	/// <summary>Refreshes jump-route renderer and control state.</summary>
	public void RefreshJumpRoutePresentationState() => UpdateJumpRoutePresentation();

	/// <summary>Triggers jump-route calculation for the current subsector.</summary>
	public void CalculateJumpRoutesForCurrentSubsector()
	{
		CalculateJumpRoutesSynchronously();
	}

	/// <summary>Applies home initialization.</summary>
	public void InitializeAtHomeState() => InitializeAtHome();

	/// <summary>Applies home initialization.</summary>
	public void call_initialize_at_home() => InitializeAtHomeState();

	/// <summary>Applies a zoom level.</summary>
	public void ApplyZoomLevelState(int level) => ApplyZoomLevel(level);

	/// <summary>Applies a zoom level.</summary>
	public void call_apply_zoom_level(int level) => ApplyZoomLevelState(level);

	/// <summary>Applies a star selection.</summary>
	public void ApplyStarSelectionState(Vector3 worldPosition, int starSeed) => ApplyStarSelection(worldPosition, starSeed);

	/// <summary>Applies a star selection.</summary>
	public void call_apply_star_selection(Vector3 worldPosition, int starSeed) => ApplyStarSelectionState(worldPosition, starSeed);

	/// <summary>Refreshes the inspector panel.</summary>
	public void UpdateInspectorState() => UpdateInspector();

	/// <summary>Refreshes the inspector panel.</summary>
	public void call_update_inspector() => UpdateInspectorState();

	/// <summary>Returns the saved zoom level.</summary>
	public int GetSavedZoomLevel() => _savedZoomLevel;

	/// <summary>Returns the saved zoom level.</summary>
	public int get_saved_zoom_level() => GetSavedZoomLevel();

	/// <summary>Sets the saved zoom level.</summary>
	public void SetSavedZoomLevel(int level) => _savedZoomLevel = level;

	/// <summary>Sets the saved zoom level.</summary>
	public void set_saved_zoom_level(int level) => SetSavedZoomLevel(level);

	/// <summary>Returns the saved quadrant, if any.</summary>
	public Vector3I? GetSavedQuadrant()
	{
		if (_savedQuadrant.VariantType == Variant.Type.Vector3I)
		{
			return (Vector3I)_savedQuadrant;
		}

		return null;
	}

	/// <summary>Sets the saved quadrant.</summary>
	public void SetSavedQuadrant(Vector3I? value)
	{
		if (value.HasValue)
		{
			_savedQuadrant = Variant.CreateFrom(value.Value);
			return;
		}

		_savedQuadrant = default;
	}

	/// <summary>Returns the saved quadrant payload.</summary>
	public Variant get_saved_quadrant() => _savedQuadrant;

	/// <summary>Sets the saved quadrant payload.</summary>
	public void set_saved_quadrant(Variant value) => _savedQuadrant = value;

	/// <summary>Returns the saved sector, if any.</summary>
	public Vector3I? GetSavedSector()
	{
		if (_savedSector.VariantType == Variant.Type.Vector3I)
		{
			return (Vector3I)_savedSector;
		}

		return null;
	}

	/// <summary>Sets the saved sector.</summary>
	public void SetSavedSector(Vector3I? value)
	{
		if (value.HasValue)
		{
			_savedSector = Variant.CreateFrom(value.Value);
			return;
		}

		_savedSector = default;
	}

	/// <summary>Returns the saved sector payload.</summary>
	public Variant get_saved_sector() => _savedSector;

	/// <summary>Sets the saved sector payload.</summary>
	public void set_saved_sector(Variant value) => _savedSector = value;

	/// <summary>Returns the saved star-camera position.</summary>
	public Vector3 GetSavedStarCameraPosition() => _savedStarCameraPosition;

	/// <summary>Returns the saved star-camera position.</summary>
	public Vector3 get_saved_star_camera_position() => GetSavedStarCameraPosition();

	/// <summary>Sets the saved star-camera position.</summary>
	public void SetSavedStarCameraPosition(Vector3 value) => _savedStarCameraPosition = value;

	/// <summary>Sets the saved star-camera position.</summary>
	public void set_saved_star_camera_position(Vector3 value) => SetSavedStarCameraPosition(value);

	/// <summary>Returns the saved star-camera rotation.</summary>
	public Vector3 GetSavedStarCameraRotation() => _savedStarCameraRotation;

	/// <summary>Returns the saved star-camera rotation.</summary>
	public Vector3 get_saved_star_camera_rotation() => GetSavedStarCameraRotation();

	/// <summary>Sets the saved star-camera rotation.</summary>
	public void SetSavedStarCameraRotation(Vector3 value) => _savedStarCameraRotation = value;

	/// <summary>Sets the saved star-camera rotation.</summary>
	public void set_saved_star_camera_rotation(Vector3 value) => SetSavedStarCameraRotation(value);

	/// <summary>Returns the saved star seed.</summary>
	public int GetSavedStarSeed() => _savedStarSeed;

	/// <summary>Returns the saved star seed.</summary>
	public int get_saved_star_seed() => GetSavedStarSeed();

	/// <summary>Sets the saved star seed.</summary>
	public void SetSavedStarSeed(int value) => _savedStarSeed = value;

	/// <summary>Sets the saved star seed.</summary>
	public void set_saved_star_seed(int value) => SetSavedStarSeed(value);

	/// <summary>Returns the saved star position.</summary>
	public Vector3 GetSavedStarPosition() => _savedStarPosition;

	/// <summary>Returns the saved star position.</summary>
	public Vector3 get_saved_star_position() => GetSavedStarPosition();

	/// <summary>Sets the saved star position.</summary>
	public void SetSavedStarPosition(Vector3 value) => _savedStarPosition = value;

	/// <summary>Sets the saved star position.</summary>
	public void set_saved_star_position(Vector3 value) => SetSavedStarPosition(value);
}
