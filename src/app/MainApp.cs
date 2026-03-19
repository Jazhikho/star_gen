using Godot;
using StarGen.App.Concepts;
using StarGen.Domain.Concepts;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Services.Concepts;
using StarGen.Services.Persistence;

namespace StarGen.App;

/// <summary>
/// Root application controller for navigating between welcome, galaxy, system, and object viewers.
/// Navigation callbacks and system-generation helpers in MainApp.Navigation.cs.
/// </summary>
public partial class MainApp : Node
{
	private const string SplashScreenScenePath = "res://src/app/SplashScreen.tscn";
	private const string MainMenuScreenScenePath = "res://src/app/MainMenuScreen.tscn";
	private const string WelcomeScreenScenePath = "res://src/app/WelcomeScreen.tscn";
	private const string SystemGenerationScreenScenePath = "res://src/app/SystemGenerationScreen.tscn";
	private const string ObjectGenerationScreenScenePath = "res://src/app/ObjectGenerationScreen.tscn";
	private const string StationStudioScreenScenePath = "res://src/app/StationStudioScreen.tscn";
	private const string ConceptAtlasScreenScenePath = "res://src/app/concepts/ConceptAtlasScreen.tscn";
	private const string GalaxyViewerScenePath = "res://src/app/galaxy_viewer/GalaxyViewerCSharp.tscn";
	private const string SystemViewerScenePath = "res://src/app/system_viewer/SystemViewer.tscn";
	private const string ObjectViewerScenePath = "res://src/app/viewer/ObjectViewer.tscn";

	private enum ViewerType { None, Splash, Menu, GalaxyStudio, SystemStudio, ObjectStudio, StationStudio, ConceptAtlas, Galaxy, System, Object }
	private enum NavigationOrigin { None, Menu, Galaxy, System }

	private ViewerType _activeViewer = ViewerType.None;
	private Node? _viewerContainer;
	private SplashScreen? _splashScreen;
	private MainMenuScreen? _mainMenuScreen;
	private WelcomeScreen? _welcomeScreen;
	private SystemGenerationScreen? _systemGenerationScreen;
	private ObjectGenerationScreen? _objectGenerationScreen;
	private StationStudioScreen? _stationStudioScreen;
	private ConceptAtlasScreen? _conceptAtlasScreen;
	private StarGen.App.GalaxyViewer.GalaxyViewer? _galaxyViewer;
	private StarGen.App.SystemViewer.SystemViewer? _systemViewer;
	private StarGen.App.Viewer.ObjectViewer? _objectViewer;
	private ViewerType _conceptAtlasReturnViewer = ViewerType.Menu;
	private NavigationOrigin _systemOrigin = NavigationOrigin.None;
	private NavigationOrigin _objectOrigin = NavigationOrigin.None;
	private readonly SystemCache _systemCache = new();
	private SeededRng? _startupRng;
	private int _galaxySeed;
	private int _currentStarSeed;
	private Godot.Vector3 _currentStarPosition = Godot.Vector3.Zero;
	private GalaxyBodyOverrides _bodyOverrides = new();

	/// <summary>
	/// Initializes the root app state.
	/// </summary>
	public override void _Ready()
	{
		_viewerContainer = GetNodeOrNull<Node>("ViewerContainer");
		WindowSettingsService.ApplySavedOrCurrent();
		_startupRng = CreateStartupRng();
		CreateSplashScreen();
		CreateMainMenuScreen();
		CreateWelcomeScreen();
		CreateSystemGenerationScreen();
		CreateObjectGenerationScreen();
		CreateStationStudioScreen();
		CreateConceptAtlasScreen();
		ShowSplashScreen();
	}

	/// <summary>
	/// Frees detached startup/viewer nodes that are not owned by the tree at shutdown.
	/// </summary>
	public override void _ExitTree()
	{
		QueueDetachedNodeForCleanup(_splashScreen);
		QueueDetachedNodeForCleanup(_mainMenuScreen);
		QueueDetachedNodeForCleanup(_welcomeScreen);
		QueueDetachedNodeForCleanup(_systemGenerationScreen);
		QueueDetachedNodeForCleanup(_objectGenerationScreen);
		QueueDetachedNodeForCleanup(_stationStudioScreen);
		QueueDetachedNodeForCleanup(_conceptAtlasScreen);
		QueueDetachedNodeForCleanup(_galaxyViewer);
		QueueDetachedNodeForCleanup(_systemViewer);
		QueueDetachedNodeForCleanup(_objectViewer);
	}

	/// <summary>
	/// Updates the current galaxy seed.
	/// </summary>
	public void SetGalaxySeed(int newSeed)
	{
		_galaxySeed = newSeed;
	}

	/// <summary>GDScript-compatible seed setter.</summary>
	public void set_galaxy_seed(int newSeed) => SetGalaxySeed(newSeed);

	/// <summary>GDScript-compatible galaxy seed accessor.</summary>
	public int get_galaxy_seed() => _galaxySeed;

	/// <summary>GDScript-compatible system cache accessor.</summary>
	public SystemCache get_system_cache() => _systemCache;

	/// <summary>Returns the current body-override collection.</summary>
	public GalaxyBodyOverrides GetBodyOverrides() => _bodyOverrides;

	/// <summary>GDScript-compatible body overrides accessor.</summary>
	public GalaxyBodyOverrides get_body_overrides() => GetBodyOverrides();

	/// <summary>Returns true if any body-level edits have been made this session.</summary>
	public bool has_unsaved_edits() => !_bodyOverrides.IsEmpty();

	/// <summary>Returns the identifier of the currently active viewer as a lowercase string for GDScript callers.</summary>
	public string get_active_viewer() => _activeViewer.ToString().ToLowerInvariant();

	/// <summary>Returns the star seed for the currently open system.</summary>
	public int get_current_star_seed() => _currentStarSeed;

	/// <summary>Returns the active galaxy viewer.</summary>
	public StarGen.App.GalaxyViewer.GalaxyViewer? GetGalaxyViewer() => _galaxyViewer;

	/// <summary>GDScript-compatible galaxy viewer accessor.</summary>
	public StarGen.App.GalaxyViewer.GalaxyViewer? get_galaxy_viewer() => GetGalaxyViewer();

	/// <summary>Returns the active system viewer.</summary>
	public StarGen.App.SystemViewer.SystemViewer? GetSystemViewer() => _systemViewer;

	/// <summary>GDScript-compatible system viewer accessor.</summary>
	public StarGen.App.SystemViewer.SystemViewer? get_system_viewer() => GetSystemViewer();

	/// <summary>
	/// Creates a startup RNG seeded from the system clock.
	/// </summary>
	private static SeededRng CreateStartupRng()
	{
		long timeUsec = unchecked((long)Time.GetTicksUsec());
		long unixTimeUsec = (long)(Time.GetUnixTimeFromSystem() * 1000000.0);
		return new SeededRng(unixTimeUsec ^ timeUsec);
	}

	/// <summary>
	/// Generates a random galaxy seed.
	/// </summary>
	private int GenerateRandomSeed()
	{
		_startupRng ??= CreateStartupRng();
		return _startupRng.RandiRange(1, 999999);
	}

	/// <summary>
	/// Creates and wires the welcome screen.
	/// </summary>
	private void CreateWelcomeScreen()
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(WelcomeScreenScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load welcome screen scene");
			return;
		}

		_welcomeScreen = scene.Instantiate() as WelcomeScreen;
		if (_welcomeScreen == null)
		{
			GD.PushError("MainApp: failed to instantiate welcome screen");
			return;
		}

		_welcomeScreen.Name = "WelcomeScreen";
		_welcomeScreen.SetSeededRng(_startupRng);
		_welcomeScreen.Connect("start_new_galaxy", Callable.From<GalaxyConfig, int>(OnWelcomeStartNewGalaxy));
		_welcomeScreen.Connect("load_galaxy_requested", Callable.From(OnWelcomeLoadGalaxyRequested));
		_welcomeScreen.Connect("back_requested", Callable.From(OnWelcomeBackRequested));
		_welcomeScreen.Connect("quit_requested", Callable.From(OnWelcomeQuitRequested));
		_welcomeScreen.SetNavigationVisibility(showBackButton: true, showQuitButton: false);
	}

	/// <summary>
	/// Creates the standalone system-generation studio.
	/// </summary>
	private void CreateSystemGenerationScreen()
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(SystemGenerationScreenScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load system generation screen scene");
			return;
		}

		_systemGenerationScreen = scene.Instantiate() as SystemGenerationScreen;
		if (_systemGenerationScreen == null)
		{
			GD.PushError("MainApp: failed to instantiate system generation screen");
			return;
		}

		_systemGenerationScreen.Name = "SystemGenerationScreen";
		_systemGenerationScreen.Connect("start_system_generation", Callable.From<SolarSystemSpec>(OnSystemGenerationStarted));
		_systemGenerationScreen.Connect("load_system_requested", Callable.From(OnSystemGenerationLoadRequested));
		_systemGenerationScreen.Connect("back_requested", Callable.From(OnSystemGenerationBackRequested));
	}

	/// <summary>
	/// Creates the standalone object-generation studio.
	/// </summary>
	private void CreateObjectGenerationScreen()
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(ObjectGenerationScreenScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load object generation screen scene");
			return;
		}

		_objectGenerationScreen = scene.Instantiate() as ObjectGenerationScreen;
		if (_objectGenerationScreen == null)
		{
			GD.PushError("MainApp: failed to instantiate object generation screen");
			return;
		}

		_objectGenerationScreen.Name = "ObjectGenerationScreen";
		_objectGenerationScreen.Connect("start_object_generation", Callable.From<ObjectGenerationRequest>(OnObjectGenerationStarted));
		_objectGenerationScreen.Connect("load_object_requested", Callable.From(OnObjectGenerationLoadRequested));
		_objectGenerationScreen.Connect("back_requested", Callable.From(OnObjectGenerationBackRequested));
	}

	/// <summary>
	/// Creates the station-studio placeholder screen.
	/// </summary>
	private void CreateStationStudioScreen()
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(StationStudioScreenScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load station studio scene");
			return;
		}

		_stationStudioScreen = scene.Instantiate() as StationStudioScreen;
		if (_stationStudioScreen == null)
		{
			GD.PushError("MainApp: failed to instantiate station studio scene");
			return;
		}

		_stationStudioScreen.Name = "StationStudioScreen";
		_stationStudioScreen.Connect("back_requested", Callable.From(ShowMainMenu));
	}

	/// <summary>
	/// Creates the concept atlas screen.
	/// </summary>
	private void CreateConceptAtlasScreen()
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(ConceptAtlasScreenScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load concept atlas scene");
			return;
		}

		_conceptAtlasScreen = scene.Instantiate() as ConceptAtlasScreen;
		if (_conceptAtlasScreen == null)
		{
			GD.PushError("MainApp: failed to instantiate concept atlas");
			return;
		}

		_conceptAtlasScreen.Name = "ConceptAtlasScreen";
		_conceptAtlasScreen.BackRequested += OnConceptAtlasBackRequested;
	}

	/// <summary>
	/// Creates the splash screen shown at startup.
	/// </summary>
	private void CreateSplashScreen()
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(SplashScreenScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load splash screen scene");
			return;
		}

		_splashScreen = scene.Instantiate() as SplashScreen;
		if (_splashScreen == null)
		{
			GD.PushError("MainApp: failed to instantiate splash screen");
			return;
		}

		_splashScreen.Name = "SplashScreen";
		_splashScreen.Connect("splash_finished", Callable.From(OnSplashFinished));
	}

	/// <summary>
	/// Creates the primary main menu screen.
	/// </summary>
	private void CreateMainMenuScreen()
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(MainMenuScreenScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load main menu scene");
			return;
		}

		_mainMenuScreen = scene.Instantiate() as MainMenuScreen;
		if (_mainMenuScreen == null)
		{
			GD.PushError("MainApp: failed to instantiate main menu");
			return;
		}

		_mainMenuScreen.Name = "MainMenuScreen";
		_mainMenuScreen.Connect("galaxy_generation_requested", Callable.From(OnMainMenuGalaxyGenerationRequested));
		_mainMenuScreen.Connect("system_generation_requested", Callable.From(OnMainMenuSystemGenerationRequested));
		_mainMenuScreen.Connect("object_generation_requested", Callable.From(OnMainMenuObjectGenerationRequested));
		_mainMenuScreen.Connect("station_generation_requested", Callable.From(OnMainMenuStationGenerationRequested));
		_mainMenuScreen.Connect("concept_atlas_requested", Callable.From(OnMainMenuConceptAtlasRequested));
		_mainMenuScreen.Connect("quit_requested", Callable.From(OnWelcomeQuitRequested));
	}

	/// <summary>
	/// Displays the splash screen.
	/// </summary>
	private void ShowSplashScreen()
	{
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_conceptAtlasScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_systemViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_splashScreen);
		_activeViewer = ViewerType.Splash;
	}

	/// <summary>
	/// Displays the main menu.
	/// </summary>
	private void ShowMainMenu()
	{
		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_conceptAtlasScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_systemViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_mainMenuScreen);
		_mainMenuScreen?.RefreshOptionsState();
		_activeViewer = ViewerType.Menu;
	}

	/// <summary>
	/// Displays the welcome screen.
	/// </summary>
	private void ShowWelcomeScreen()
	{
		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_conceptAtlasScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_systemViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_welcomeScreen);
		_welcomeScreen?.SetNavigationVisibility(showBackButton: true, showQuitButton: false);
		_welcomeScreen?.ApplySeedVisibilityPreference(rerollHiddenSeed: true);
		_activeViewer = ViewerType.GalaxyStudio;
	}

	/// <summary>
	/// Displays the system-generation studio.
	/// </summary>
	private void ShowSystemGenerationScreen()
	{
		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_conceptAtlasScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_systemViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_systemGenerationScreen);
		_systemGenerationScreen?.ApplySeedVisibilityPreference(rerollHiddenSeed: true);
		_activeViewer = ViewerType.SystemStudio;
	}

	/// <summary>
	/// Displays the object-generation studio.
	/// </summary>
	private void ShowObjectGenerationScreen()
	{
		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_systemViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_objectGenerationScreen);
		_objectGenerationScreen?.ApplySeedVisibilityPreference(rerollHiddenSeed: true);
		_activeViewer = ViewerType.ObjectStudio;
	}

	/// <summary>
	/// Displays the station-studio placeholder.
	/// </summary>
	private void ShowStationStudioScreen()
	{
		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_conceptAtlasScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_systemViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_stationStudioScreen);
		_activeViewer = ViewerType.StationStudio;
	}

	/// <summary>
	/// Displays the concept atlas.
	/// </summary>
	private void ShowConceptAtlasScreen()
	{
		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_systemViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_conceptAtlasScreen);
		_activeViewer = ViewerType.ConceptAtlas;
	}

	/// <summary>
	/// Opens the concept atlas with the provided context and remembers the return target.
	/// </summary>
	private void OpenConceptAtlas(ConceptContextSnapshot snapshot, Domain.Concepts.ConceptKind initialKind, ViewerType returnViewer)
	{
		if (_conceptAtlasScreen == null)
		{
			return;
		}

		_conceptAtlasReturnViewer = returnViewer;
		_conceptAtlasScreen.SetContext(snapshot, initialKind);
		ShowConceptAtlasScreen();
	}

	/// <summary>
	/// Returns from the concept atlas to the viewer it was opened from.
	/// </summary>
	private void OnConceptAtlasBackRequested()
	{
		switch (_conceptAtlasReturnViewer)
		{
			case ViewerType.Galaxy:
				ShowGalaxyViewer();
				return;
			case ViewerType.System:
				ShowSystemViewer();
				return;
			case ViewerType.Object:
				ShowObjectViewer();
				return;
			default:
				ShowMainMenu();
				return;
		}
	}

	/// <summary>
	/// Opens the station-studio placeholder from the main menu.
	/// </summary>
	private void OnMainMenuStationGenerationRequested()
	{
		ShowStationStudioScreen();
	}

	/// <summary>
	/// Opens the concept atlas from the main menu.
	/// </summary>
	private void OnMainMenuConceptAtlasRequested()
	{
		OpenConceptAtlas(
			ConceptContextBuilder.CreateDefault(GenerateRandomSeed()),
			Domain.Concepts.ConceptKind.Ecology,
			ViewerType.Menu);
	}

	/// <summary>
	/// Creates the galaxy viewer for the current session.
	/// </summary>
	private void CreateGalaxyViewer(int seedValue, GalaxyConfig? config = null)
	{
		_galaxySeed = seedValue;
		_bodyOverrides = new GalaxyBodyOverrides();

		PackedScene? scene = ResourceLoader.Load<PackedScene>(GalaxyViewerScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load galaxy viewer scene");
			return;
		}

		_galaxyViewer = scene.Instantiate() as StarGen.App.GalaxyViewer.GalaxyViewer;
		if (_galaxyViewer == null)
		{
			GD.PushError("MainApp: failed to instantiate galaxy viewer");
			return;
		}

		_galaxyViewer.Name = "GalaxyViewer";
		_galaxyViewer.GalaxySeed = seedValue;
		if (config != null)
		{
			_galaxyViewer.set_galaxy_config(config);
		}

		_galaxyViewer.OpenSystemRequested += OnOpenSystemRequested;
		_galaxyViewer.OpenConceptAtlasRequested += OnGalaxyConceptAtlasRequested;
		_galaxyViewer.GalaxySeedChanged += SetGalaxySeed;
		_galaxyViewer.NewGalaxyRequested += OnNewGalaxyRequested;
		_galaxyViewer.MainMenuRequested += OnGalaxyViewerMainMenuRequested;
	}

	/// <summary>
	/// Creates the system viewer on demand.
	/// </summary>
	private void CreateSystemViewer()
	{
		if (_systemViewer != null)
		{
			return;
		}

		PackedScene? scene = ResourceLoader.Load<PackedScene>(SystemViewerScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load system viewer scene");
			return;
		}

		_systemViewer = scene.Instantiate() as StarGen.App.SystemViewer.SystemViewer;
		if (_systemViewer == null)
		{
			GD.PushError("MainApp: failed to instantiate system viewer");
			return;
		}

		_systemViewer.Name = "SystemViewer";
		_systemViewer.OpenBodyInViewer += OnOpenInObjectViewer;
		_systemViewer.OpenConceptAtlasRequested += OnSystemConceptAtlasRequested;
		_systemViewer.BackToGalaxyRequested += OnBackToGalaxy;
	}

	/// <summary>
	/// Creates the object viewer on demand.
	/// </summary>
	private void CreateObjectViewer()
	{
		if (_objectViewer != null)
		{
			return;
		}

		PackedScene? scene = ResourceLoader.Load<PackedScene>(ObjectViewerScenePath);
		if (scene == null)
		{
			GD.PushError("MainApp: failed to load object viewer scene");
			return;
		}

		_objectViewer = scene.Instantiate() as StarGen.App.Viewer.ObjectViewer;
		if (_objectViewer == null)
		{
			GD.PushError("MainApp: failed to instantiate object viewer");
			return;
		}

		_objectViewer.Name = "ObjectViewer";
		_objectViewer.BackToSystemRequested += OnBackToSystem;
		_objectViewer.BackToMainMenuRequested += ShowMainMenu;
		_objectViewer.OpenConceptAtlasRequested += OnObjectConceptAtlasRequested;
		_objectViewer.BodyEdited += OnBodyEdited;
	}

	/// <summary>
	/// Shows the galaxy viewer.
	/// </summary>
	private void ShowGalaxyViewer()
	{
		if (_activeViewer == ViewerType.Galaxy)
		{
			return;
		}

		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_conceptAtlasScreen);
		RemoveFromViewerContainer(_systemViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_galaxyViewer);
		_activeViewer = ViewerType.Galaxy;
	}

	/// <summary>
	/// Shows the system viewer.
	/// </summary>
	private void ShowSystemViewer()
	{
		if (_activeViewer == ViewerType.System)
		{
			return;
		}

		CreateSystemViewer();
		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_conceptAtlasScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_objectViewer);
		AddToViewerContainer(_systemViewer);
		if (_systemOrigin == NavigationOrigin.Menu)
		{
			_systemViewer?.ConfigureBackNavigation("Return to Main Menu", "Return to the main menu (Esc)");
		}
		else
		{
			_systemViewer?.ConfigureBackNavigation("Return to Galaxy Viewer", "Return to the galaxy viewer (Esc)");
		}

		_activeViewer = ViewerType.System;
	}

	/// <summary>
	/// Shows the object viewer.
	/// </summary>
	private void ShowObjectViewer()
	{
		if (_activeViewer == ViewerType.Object)
		{
			return;
		}

		CreateObjectViewer();
		RemoveFromViewerContainer(_splashScreen);
		RemoveFromViewerContainer(_mainMenuScreen);
		RemoveFromViewerContainer(_welcomeScreen);
		RemoveFromViewerContainer(_systemGenerationScreen);
		RemoveFromViewerContainer(_objectGenerationScreen);
		RemoveFromViewerContainer(_stationStudioScreen);
		RemoveFromViewerContainer(_conceptAtlasScreen);
		RemoveFromViewerContainer(_galaxyViewer);
		RemoveFromViewerContainer(_systemViewer);
		AddToViewerContainer(_objectViewer);
		if (_objectOrigin == NavigationOrigin.Menu)
		{
			_objectViewer?.SetBackNavigationVisibility(true, "Return to Main Menu", "Return to the main menu", true);
		}

		_activeViewer = ViewerType.Object;
	}

	/// <summary>
	/// Removes a child from the viewer container.
	/// </summary>
	private void RemoveFromViewerContainer(Node? node)
	{
		if (_viewerContainer != null && node != null && node.GetParent() == _viewerContainer)
		{
			_viewerContainer.RemoveChild(node);
		}
	}

	/// <summary>
	/// Adds a child to the viewer container.
	/// </summary>
	private void AddToViewerContainer(Node? node)
	{
		if (_viewerContainer != null && node != null && node.GetParent() != _viewerContainer)
		{
			_viewerContainer.AddChild(node);
		}
	}

	/// <summary>
	/// Queues detached nodes so tests and shutdown paths do not leak pre-instantiated screens.
	/// </summary>
	private static void QueueDetachedNodeForCleanup(Node? node)
	{
		if (node != null && GodotObject.IsInstanceValid(node) && node.GetParent() == null)
		{
			node.QueueFree();
		}
	}
}
