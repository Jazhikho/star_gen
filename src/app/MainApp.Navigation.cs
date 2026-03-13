using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;
using StarGen.Services.Persistence;

namespace StarGen.App;

/// <summary>
/// Navigation event-handlers, system generation helpers, and body-coercion utilities for MainApp.
/// </summary>
public partial class MainApp
{
    /// <summary>
    /// Handles splash-screen completion.
    /// </summary>
    private void OnSplashFinished()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// Opens the galaxy-generation screen from the main menu.
    /// </summary>
    private void OnMainMenuGalaxyGenerationRequested()
    {
        _welcomeScreen?.SetNavigationVisibility(showBackButton: true, showQuitButton: false);
        ShowWelcomeScreen();
    }

    /// <summary>
    /// Opens the standalone system generator from the main menu.
    /// </summary>
    private void OnMainMenuSystemGenerationRequested()
    {
        _systemOrigin = NavigationOrigin.Menu;
        _objectOrigin = NavigationOrigin.None;
        _currentStarSeed = 0;
        _currentStarPosition = Godot.Vector3.Zero;
        _systemGenerationScreen?.SetInitialSeed(GenerateRandomSeed());
        ShowSystemGenerationScreen();
    }

    /// <summary>
    /// Opens the standalone object generator from the main menu.
    /// </summary>
    private void OnMainMenuObjectGenerationRequested()
    {
        _systemOrigin = NavigationOrigin.None;
        _objectOrigin = NavigationOrigin.Menu;
        _objectGenerationScreen?.SetInitialSeed(GenerateRandomSeed());
        ShowObjectGenerationScreen();
    }

    /// <summary>
    /// Handles startup-screen start requests.
    /// </summary>
    private void OnWelcomeStartNewGalaxy(GalaxyConfig config, int seedValue)
    {
        CreateGalaxyViewer(seedValue, config);
        ShowGalaxyViewer();
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

        ShowGalaxyViewer();
        _galaxyViewer?.ApplySaveData(data);
    }

    /// <summary>
    /// Returns from the galaxy-generation screen to the main menu.
    /// </summary>
    private void OnWelcomeBackRequested()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// Returns from the system-generation studio to the main menu.
    /// </summary>
    private void OnSystemGenerationBackRequested()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// Returns from the object-generation studio to the main menu.
    /// </summary>
    private void OnObjectGenerationBackRequested()
    {
        ShowMainMenu();
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
        _activeViewer = ViewerType.None;
        _currentStarSeed = 0;
        _currentStarPosition = Godot.Vector3.Zero;
        _systemOrigin = NavigationOrigin.None;
        _objectOrigin = NavigationOrigin.None;
        ShowWelcomeScreen();
    }

    /// <summary>
    /// Returns from the galaxy viewer directly to the main menu.
    /// </summary>
    private void OnGalaxyViewerMainMenuRequested()
    {
        _systemOrigin = NavigationOrigin.None;
        _objectOrigin = NavigationOrigin.None;
        ShowMainMenu();
    }

    /// <summary>
    /// Launches the standalone system viewer from the studio.
    /// </summary>
    private void OnSystemGenerationStarted(SolarSystemSpec spec)
    {
        _systemOrigin = NavigationOrigin.Menu;
        _objectOrigin = NavigationOrigin.None;
        _currentStarSeed = 0;
        _currentStarPosition = Godot.Vector3.Zero;
        ShowSystemViewer();
        _systemViewer?.SetGenerationSectionVisible(false);
        _systemViewer?.GenerateSystem(spec);
    }

    /// <summary>
    /// Launches the standalone object viewer from the studio.
    /// </summary>
    private void OnObjectGenerationStarted(ObjectGenerationRequest request)
    {
        _systemOrigin = NavigationOrigin.None;
        _objectOrigin = NavigationOrigin.Menu;
        ShowObjectViewer();
        _objectViewer?.SetBackNavigationVisibility(true, "Return to Main Menu", "Return to the main menu", true);
        _objectViewer?.LaunchStandaloneGeneration(request);
    }

    /// <summary>
    /// Opens a load dialog for standalone system files from the studio.
    /// </summary>
    private void OnSystemGenerationLoadRequested()
    {
        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = new string[] { "*.sgs ; StarGen System", "*.json ; JSON Debug" },
        };
        dialog.FileSelected += path =>
        {
            OnSystemGenerationLoadFileSelected(path);
            dialog.QueueFree();
        };
        dialog.Canceled += dialog.QueueFree;
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Opens a load dialog for standalone body files from the studio.
    /// </summary>
    private void OnObjectGenerationLoadRequested()
    {
        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = SaveData.GetFileFilters(includeLegacy: true),
        };
        dialog.FileSelected += path =>
        {
            OnObjectGenerationLoadFileSelected(path);
            dialog.QueueFree();
        };
        dialog.Canceled += dialog.QueueFree;
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Loads a system selected in the standalone system studio.
    /// </summary>
    private void OnSystemGenerationLoadFileSelected(string path)
    {
        SystemPersistenceLoadResult result = SystemPersistence.Load(path);
        if (!result.Success || result.System == null)
        {
            GD.PushError($"MainApp: failed to load system save '{path}': {result.ErrorMessage}");
            return;
        }

        _systemOrigin = NavigationOrigin.Menu;
        _objectOrigin = NavigationOrigin.None;
        ShowSystemViewer();
        _systemViewer?.SetGenerationSectionVisible(false);
        _systemViewer?.DisplaySystem(result.System);
        if (result.System.Provenance != null)
        {
            _systemViewer?.UpdateSeedDisplay((int)result.System.Provenance.GenerationSeed);
        }
    }

    /// <summary>
    /// Loads an object selected in the standalone object studio.
    /// </summary>
    private void OnObjectGenerationLoadFileSelected(string path)
    {
        _systemOrigin = NavigationOrigin.None;
        _objectOrigin = NavigationOrigin.Menu;
        ShowObjectViewer();
        _objectViewer?.SetBackNavigationVisibility(true, "Return to Main Menu", "Return to the main menu", true);
        _objectViewer?.SetGenerationSectionVisible(false);
        SaveDataLoadResult result = _objectViewer?.LoadBodyFromPath(path) ?? new SaveDataLoadResult();
        if (!result.Success)
        {
            GD.PushError($"MainApp: failed to load body save '{path}': {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Opens a selected system from the galaxy viewer.
    /// </summary>
    private void OnOpenSystemRequested(int starSeed, Godot.Vector3 worldPosition)
    {
        if (starSeed == 0)
        {
            return;
        }

        _currentStarSeed = starSeed;
        _currentStarPosition = worldPosition;
        _systemOrigin = NavigationOrigin.Galaxy;
        _objectOrigin = NavigationOrigin.None;
        _galaxyViewer?.SaveState();

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
                GenerationUseCaseSettings? useCaseSettings = _galaxyViewer?.GetGalaxyConfig()?.UseCaseSettings;
                system = GenerateSystemFromSeed(starSeed, useCaseSettings);
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
    private static SolarSystem? GenerateSystemFromSeed(int starSeed, GenerationUseCaseSettings? useCaseSettings = null)
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
        if (useCaseSettings != null)
        {
            spec.UseCaseSettings = useCaseSettings.Clone();
            if (useCaseSettings.IsTravellerMode())
            {
                spec.GeneratePopulation = true;
            }
        }

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

        _objectOrigin = NavigationOrigin.System;
        ShowObjectViewer();
        if (_objectViewer == null)
        {
            GD.PushError("MainApp: object viewer could not be created");
            return;
        }

        _objectViewer.SetBackNavigationVisibility(true, "Return to System Viewer", "Return to the system viewer");
        _objectViewer.SetGenerationSectionVisible(false);
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
                return CelestialSerializer.FromDictionary((Godot.Collections.Dictionary)data);
            }
        }

        return null;
    }

    /// <summary>
    /// Returns from the object viewer to the system viewer.
    /// </summary>
    private void OnBackToSystem()
    {
        if (_objectOrigin == NavigationOrigin.Menu)
        {
            _objectOrigin = NavigationOrigin.None;
            ShowMainMenu();
            return;
        }

        ShowSystemViewer();
    }

    /// <summary>
    /// Returns from the system viewer to the galaxy viewer.
    /// </summary>
    private void OnBackToGalaxy()
    {
        if (_systemOrigin == NavigationOrigin.Menu)
        {
            _systemOrigin = NavigationOrigin.None;
            ShowMainMenu();
            return;
        }

        ShowGalaxyViewer();
        if (_galaxyViewer != null && _galaxyViewer.HasSavedState())
        {
            _galaxyViewer.RestoreState();
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
    /// Starts a new galaxy with default config.
    /// </summary>
    public void start_galaxy_with_defaults()
    {
        CreateGalaxyViewer(GenerateRandomSeed(), GalaxyConfig.CreateDefault());
        ShowGalaxyViewer();
    }
}
