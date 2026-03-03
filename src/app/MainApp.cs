using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;
using StarGen.Services.Persistence;

namespace StarGen.App;

/// <summary>
/// Root application controller for navigating between welcome, galaxy, system, and object viewers.
/// </summary>
public partial class MainApp : Node
{
    private const string WelcomeScreenScenePath = "res://src/app/WelcomeScreen.tscn";
    private const string GalaxyViewerScenePath = "res://src/app/galaxy_viewer/GalaxyViewerCSharp.tscn";
    private const string SystemViewerScenePath = "res://src/app/system_viewer/SystemViewerCSharp.tscn";
    private const string ObjectViewerScenePath = "res://src/app/viewer/ObjectViewerCSharp.tscn";

    private string _activeViewer = string.Empty;
    private Node? _viewerContainer;
    private WelcomeScreen? _welcomeScreen;
    private StarGen.App.GalaxyViewer.GalaxyViewer? _galaxyViewer;
    private StarGen.App.SystemViewer.SystemViewer? _systemViewer;
    private StarGen.App.Viewer.ObjectViewer? _objectViewer;
    private readonly SystemCache _systemCache = new();
    private SeededRng? _startupRng;
    private int _galaxySeed;
    private int _currentStarSeed;
    private Vector3 _currentStarPosition = Vector3.Zero;
    private GalaxyBodyOverrides _bodyOverrides = new();

    /// <summary>
    /// Initializes the root app state.
    /// </summary>
    public override void _Ready()
    {
        _viewerContainer = GetNodeOrNull<Node>("ViewerContainer");
        _startupRng = CreateStartupRng();
        CreateWelcomeScreen();
        ShowWelcomeScreen();
    }

    /// <summary>
    /// Creates the startup RNG.
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
        _welcomeScreen.Connect("quit_requested", Callable.From(OnWelcomeQuitRequested));
    }

    /// <summary>
    /// Displays the welcome screen.
    /// </summary>
    private void ShowWelcomeScreen()
    {
        RemoveFromViewerContainer(_galaxyViewer);
        RemoveFromViewerContainer(_systemViewer);
        RemoveFromViewerContainer(_objectViewer);
        AddToViewerContainer(_welcomeScreen);
        _welcomeScreen?.RefreshRandomSeedDisplay();
        _activeViewer = string.Empty;
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
        _galaxyViewer.GalaxySeedChanged += SetGalaxySeed;
        _galaxyViewer.NewGalaxyRequested += OnNewGalaxyRequested;
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
        _objectViewer.BodyEdited += OnBodyEdited;
    }

    /// <summary>
    /// Shows the galaxy viewer.
    /// </summary>
    private void ShowGalaxyViewer()
    {
        if (_activeViewer == "galaxy")
        {
            return;
        }

        RemoveFromViewerContainer(_systemViewer);
        RemoveFromViewerContainer(_objectViewer);
        AddToViewerContainer(_galaxyViewer);
        _activeViewer = "galaxy";
    }

    /// <summary>
    /// Shows the system viewer.
    /// </summary>
    private void ShowSystemViewer()
    {
        if (_activeViewer == "system")
        {
            return;
        }

        CreateSystemViewer();
        RemoveFromViewerContainer(_galaxyViewer);
        RemoveFromViewerContainer(_objectViewer);
        AddToViewerContainer(_systemViewer);
        _activeViewer = "system";
    }

    /// <summary>
    /// Shows the object viewer.
    /// </summary>
    private void ShowObjectViewer()
    {
        if (_activeViewer == "object")
        {
            return;
        }

        CreateObjectViewer();
        RemoveFromViewerContainer(_galaxyViewer);
        RemoveFromViewerContainer(_systemViewer);
        AddToViewerContainer(_objectViewer);
        _activeViewer = "object";
    }

    /// <summary>
    /// Handles startup-screen start requests.
    /// </summary>
    private void OnWelcomeStartNewGalaxy(GalaxyConfig config, int seedValue)
    {
        CreateGalaxyViewer(seedValue, config);
        RemoveFromViewerContainer(_welcomeScreen);
        AddToViewerContainer(_galaxyViewer);
        _activeViewer = "galaxy";
    }

    /// <summary>
    /// Handles startup-screen load requests.
    /// </summary>
    private void OnWelcomeLoadGalaxyRequested()
    {
        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = new string[] { "*.sgg ; StarGen Galaxy", "*.json ; JSON Debug" },
        };
        dialog.FileSelected += path =>
        {
            OnWelcomeLoadFileSelected(path);
            dialog.QueueFree();
        };
        dialog.Canceled += dialog.QueueFree;
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Handles file selection from the load dialog.
    /// </summary>
    private void OnWelcomeLoadFileSelected(string path)
    {
        GalaxySaveData? data = GalaxyPersistence.LoadAuto(path);
        if (data == null || !data.IsValid())
        {
            GD.PushError($"MainApp: invalid or missing save file: {path}");
            return;
        }

        _galaxySeed = data.GalaxySeed;
        CreateGalaxyViewer(_galaxySeed, data.GetConfig());
        _bodyOverrides = data.GetBodyOverrides();
        _galaxyViewer?.apply_save_data(data);

        RemoveFromViewerContainer(_welcomeScreen);
        AddToViewerContainer(_galaxyViewer);
        _activeViewer = "galaxy";
    }

    /// <summary>
    /// Handles startup-screen quit requests.
    /// </summary>
    private void OnWelcomeQuitRequested()
    {
        GetTree().Quit();
    }

    /// <summary>
    /// Returns to the startup screen for a new galaxy.
    /// </summary>
    private void OnNewGalaxyRequested()
    {
        RemoveFromViewerContainer(_galaxyViewer);
        if (_galaxyViewer != null)
        {
            _galaxyViewer.QueueFree();
            _galaxyViewer = null;
        }

        _systemCache.Clear();
        _activeViewer = string.Empty;
        _currentStarSeed = 0;
        _currentStarPosition = Vector3.Zero;
        ShowWelcomeScreen();
    }

    /// <summary>
    /// Opens a selected system from the galaxy viewer.
    /// </summary>
    private void OnOpenSystemRequested(int starSeed, Vector3 worldPosition)
    {
        if (starSeed == 0)
        {
            return;
        }

        _currentStarSeed = starSeed;
        _currentStarPosition = worldPosition;
        _galaxyViewer?.save_state();

        SolarSystem? system = null;
        StarSystemPreviewData? preview = _galaxyViewer?.get_star_preview();
        if (preview != null && preview.StarSeed == starSeed && preview.System != null)
        {
            system = preview.System;
            ApplyOverridesToSystem(system, starSeed);
            _systemCache.PutSystem(starSeed, system);
        }
        else
        {
            system = _systemCache.GetSystem(starSeed);
            if (system == null)
            {
                system = GenerateSystemFromSeed(starSeed);
                if (system != null)
                {
                    ApplyOverridesToSystem(system, starSeed);
                    _systemCache.PutSystem(starSeed, system);
                }
            }
            else
            {
                ApplyOverridesToSystem(system, starSeed);
            }
        }

        if (system == null)
        {
            GD.PushError($"MainApp: failed to generate system for star seed {starSeed}");
            return;
        }

        ShowSystemViewer();
        _systemViewer?.SetSourceStarSeed(starSeed);
        _systemViewer?.DisplaySystem(system);
        _systemViewer?.SetStatus($"System from star seed {starSeed}");
    }

    /// <summary>
    /// Generates a system from a star seed.
    /// </summary>
    private static SolarSystem? GenerateSystemFromSeed(int starSeed)
    {
        RandomNumberGenerator rng = new()
        {
            Seed = unchecked((ulong)starSeed),
        };

        float starRoll = rng.Randf();
        int starCount = 1;
        if (starRoll > 0.85f)
        {
            starCount = 3;
        }
        else if (starRoll > 0.55f)
        {
            starCount = 2;
        }

        SolarSystemSpec spec = new(starSeed, starCount, starCount);
        return SystemFixtureGenerator.GenerateSystem(spec);
    }

    /// <summary>
    /// Applies edited-body overrides to a system.
    /// </summary>
    private void ApplyOverridesToSystem(SolarSystem system, int starSeed)
    {
        if (!_bodyOverrides.HasAnyFor(starSeed))
        {
            return;
        }

        Array<CelestialBody> allBodies = new();
        AppendBodies(allBodies, system.GetStars());
        AppendBodies(allBodies, system.GetPlanets());
        AppendBodies(allBodies, system.GetMoons());
        AppendBodies(allBodies, system.GetAsteroids());
        int replaced = _bodyOverrides.ApplyToBodies(starSeed, allBodies);
        if (replaced <= 0)
        {
            return;
        }

        foreach (CelestialBody body in allBodies)
        {
            if (body == null)
            {
                continue;
            }

            if (_bodyOverrides.GetOverrideDict(starSeed, body.Id).Count == 0)
            {
                continue;
            }

            system.AddBody(body);
        }
    }

    /// <summary>
    /// Appends bodies into a combined array.
    /// </summary>
    private static void AppendBodies(Array<CelestialBody> destination, Array<CelestialBody> source)
    {
        foreach (CelestialBody body in source)
        {
            destination.Add(body);
        }
    }

    /// <summary>
    /// Handles the object-viewer open request from the system viewer.
    /// </summary>
    private void OnOpenInObjectViewer(GodotObject bodyObject, Array moons, int starSeed)
    {
        CelestialBody? typedBody = CoerceToCelestialBody(bodyObject);
        if (typedBody == null)
        {
            GD.PushError("MainApp: unable to convert body payload for object viewer");
            return;
        }

        Array moonPayload = new();
        foreach (Variant moonVariant in moons)
        {
            GodotObject? moonObject = moonVariant.AsGodotObject();
            CelestialBody? typedMoon = CoerceToCelestialBody(moonObject);
            if (typedMoon != null)
            {
                moonPayload.Add(typedMoon);
            }
        }

        ShowObjectViewer();
        if (_objectViewer == null)
        {
            GD.PushError("MainApp: object viewer could not be created");
            return;
        }

        _objectViewer.DisplayExternalBody(typedBody, moonPayload, starSeed);
    }

    /// <summary>
    /// Converts a runtime payload into a C# celestial body.
    /// </summary>
    private static CelestialBody? CoerceToCelestialBody(GodotObject? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is CelestialBody typedBody)
        {
            return typedBody;
        }

        if (value.HasMethod("to_dict"))
        {
            Variant data = value.Call("to_dict");
            if (data.VariantType == Variant.Type.Dictionary)
            {
                return CelestialSerializer.FromDictionary((Dictionary)data);
            }
        }

        return null;
    }

    /// <summary>
    /// Returns from the object viewer to the system viewer.
    /// </summary>
    private void OnBackToSystem()
    {
        ShowSystemViewer();
    }

    /// <summary>
    /// Returns from the system viewer to the galaxy viewer.
    /// </summary>
    private void OnBackToGalaxy()
    {
        ShowGalaxyViewer();
        if (_galaxyViewer != null && _galaxyViewer.has_saved_state())
        {
            _galaxyViewer.restore_state();
        }
    }

    /// <summary>
    /// Handles edited-body notifications from the object viewer.
    /// </summary>
    private void OnBodyEdited(GodotObject bodyObject, int starSeed)
    {
        CelestialBody? body = CoerceToCelestialBody(bodyObject);
        if (body == null || starSeed == 0)
        {
            return;
        }

        _bodyOverrides.SetOverride(starSeed, body);
        _systemCache.Evict(starSeed);

        if (_systemViewer != null && _currentStarSeed == starSeed)
        {
            SolarSystem? system = _systemViewer.GetCurrentSystem();
            if (system != null)
            {
                system.AddBody(body);
                _systemViewer.DisplaySystem(system);
            }
        }
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
    /// Updates the current galaxy seed.
    /// </summary>
    public void SetGalaxySeed(int newSeed)
    {
        _galaxySeed = newSeed;
    }

    public void set_galaxy_seed(int newSeed) => SetGalaxySeed(newSeed);
    public int get_galaxy_seed() => _galaxySeed;
    public SystemCache get_system_cache() => _systemCache;
    public GalaxyBodyOverrides get_body_overrides() => _bodyOverrides;
    public bool has_unsaved_edits() => !_bodyOverrides.IsEmpty();
    public string get_active_viewer() => _activeViewer;
    public int get_current_star_seed() => _currentStarSeed;

    /// <summary>
    /// Starts a new galaxy with default config.
    /// </summary>
    public void start_galaxy_with_defaults()
    {
        CreateGalaxyViewer(GenerateRandomSeed(), GalaxyConfig.CreateDefault());
        RemoveFromViewerContainer(_welcomeScreen);
        AddToViewerContainer(_galaxyViewer);
        _activeViewer = "galaxy";
    }

    public StarGen.App.GalaxyViewer.GalaxyViewer? get_galaxy_viewer() => _galaxyViewer;
    public StarGen.App.SystemViewer.SystemViewer? get_system_viewer() => _systemViewer;
}
