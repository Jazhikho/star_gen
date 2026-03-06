using Godot;
using System.Collections.Generic;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Rng;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Compile-ready C# top-level controller for the galaxy-viewer slice.
/// State accessors in GalaxyViewer.Accessors.cs. Navigation in GalaxyViewer.Navigation.cs.
/// Setup and rendering in GalaxyViewer.Setup.cs. Selection in GalaxyViewer.Selection.cs.
/// </summary>
public partial class GalaxyViewer : Node3D, IGalaxyViewerSavedStateHost
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

	internal Label? _statusLabel;
	internal SpinBox? _seedInput;
	internal CheckBox? _showCompassCheck;
	internal Node? _inspectorPanel;
	internal Button? _saveButton;
	internal Button? _loadButton;
	internal Button? _newGalaxyButton;

	internal Galaxy? _galaxy;
	internal GalaxySpec? _spec;
	internal GalaxyConfig? _galaxyConfig;
	internal ZoomStateMachine? _zoomMachine;
	internal GalaxyRenderer? _galaxyRenderer;
	internal QuadrantRenderer? _quadrantRenderer;
	internal QuadrantSelector? _quadrantSelector;
	internal GridCursor? _quadrantCursor;
	internal GridCursor? _sectorCursor;
	internal OrbitCamera? _orbitCamera;
	internal StarViewCamera? _starCamera;
	internal NavigationCompass? _compass;
	internal SelectionIndicator? _selectionIndicator;
	internal Node? _sectorRenderer;
	internal Node? _neighborhoodRenderer;
	internal SectorJumpLaneRenderer? _sectorJumpLaneRenderer;

	internal Variant _selectedSector = default;
	internal Vector3 _selectedStarPosition = Vector3.Zero;
	internal int _selectedStarSeed;
	internal StarSystemPreviewData? _starPreview;

	internal int _savedZoomLevel = -1;
	internal Variant _savedQuadrant = default;
	internal Variant _savedSector = default;
	internal Vector3 _savedStarCameraPosition = Vector3.Zero;
	internal Vector3 _savedStarCameraRotation = Vector3.Zero;
	internal int _savedStarSeed;
	internal Vector3 _savedStarPosition = Vector3.Zero;

	internal JumpLaneRegion? _jumpLaneRegion;
	internal JumpLaneResult? _jumpLaneResult;
	internal readonly Dictionary<int, int> _jumpRoutePopulationCache = new();
	internal readonly HashSet<string> _jumpRouteCalculatedRegionIds = new();
	internal int _jumpRouteCalculationGeneration;
	internal readonly GalaxyViewerSaveLoad _saveLoad = new();

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
	/// GDScript-compatible status setter.
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
	public void SaveState()
	{
		_saveLoad.SaveState(this);
	}

	/// <summary>
	/// Restores the previously saved viewer state.
	/// </summary>
	public void RestoreState()
	{
		_saveLoad.RestoreState(this);
	}

	/// <summary>
	/// Returns whether saved state exists.
	/// </summary>
	public bool HasSavedState()
	{
		return _saveLoad.HasSavedState(this);
	}

	/// <summary>
	/// Applies loaded save data to core state.
	/// </summary>
	public void ApplySaveData(GalaxySaveData saveData)
	{
		_saveLoad.ApplySaveData(this, saveData);
	}

	/// <summary>
	/// Saves the current viewer state.
	/// </summary>
	public void save_state()
	{
		SaveState();
	}

	/// <summary>
	/// Restores the previously saved viewer state.
	/// </summary>
	public void restore_state()
	{
		RestoreState();
	}

	/// <summary>
	/// Returns whether saved state exists.
	/// </summary>
	public bool has_saved_state()
	{
		return HasSavedState();
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

		ApplySaveData(saveData);
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

		SetGalaxyConfig(config);
	}
}
