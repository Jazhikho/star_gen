using Godot;
using Godot.Collections;
using System.Threading.Tasks;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Colonization;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;

namespace StarGen.App;

/// <summary>
/// Navigation event-handlers, system generation helpers, and body-coercion utilities for MainApp.
/// </summary>
public partial class MainApp
{
    /// <summary>
    /// Handles splash-screen completion.
    /// </summary>
    private async void OnSplashFinished()
    {
        if (_startupTransitionRunning)
        {
            return;
        }

        if (!IsInsideTree() || _startupTransitionRect == null)
        {
            ShowMainMenu();
            return;
        }

        _startupTransitionRunning = true;
        await TweenStartupFadeToAlpha(1.0f);
        ShowMainMenu();
        await TweenStartupFadeToAlpha(0.0f);
        _startupTransitionRunning = false;
    }

	/// <summary>
	/// Opens the galaxy-generation screen from the main menu.
	/// </summary>
	private void OnMainMenuGalaxyGenerationRequested()
	{
		_galaxyGenerationScreen?.SetNavigationVisibility(showBackButton: true, showQuitButton: false);
		ShowGalaxyGenerationScreen();
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
	/// Handles galaxy-studio start requests.
	/// </summary>
	private void OnGalaxyGenerationStarted(GalaxyConfig config, int seedValue)
	{
		CreateGalaxyViewer(seedValue, config);
		ShowGalaxyViewer();
	}

	/// <summary>
	/// Returns from the galaxy-generation studio to the main menu.
	/// </summary>
	private void OnGalaxyGenerationBackRequested()
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
	/// Handles app quit requests from the menu-driven screens.
	/// </summary>
	private void OnGalaxyGenerationQuitRequested()
	{
		GetTree().Quit();
	}

	/// <summary>
	/// Returns to the galaxy-generation studio for a new galaxy.
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
		ShowGalaxyGenerationScreen();
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
    /// Returns directly to the main menu from a viewer-level file menu action.
    /// </summary>
    private void OnViewerMainMenuRequested()
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
        _objectViewer?.LaunchStandaloneGeneration(request);
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
            ApplyColonizationSimulationToSystem(system, starSeed);
            _systemCache.PutSystem(starSeed, system);
        }
        else
        {
            system = _systemCache.GetSystem(starSeed);
            if (system == null)
            {
                GenerationUseCaseSettings? useCaseSettings = _galaxyViewer?.GetGalaxyConfig()?.UseCaseSettings;
                system = GenerateSystemFromSeed(starSeed, useCaseSettings, worldPosition, _galaxyViewer?.GetGalaxy());
                if (system != null)
                {
                    ApplyOverridesToSystem(system, starSeed);
                    ApplyColonizationSimulationToSystem(system, starSeed);
                    _systemCache.PutSystem(starSeed, system);
                }
            }
            else
            {
                ApplyOverridesToSystem(system, starSeed);
                ApplyColonizationSimulationToSystem(system, starSeed);
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
    private static SolarSystem? GenerateSystemFromSeed(
        int starSeed,
        GenerationUseCaseSettings? useCaseSettings = null,
        Vector3? worldPosition = null,
        Galaxy? galaxy = null)
    {
        if (galaxy != null && worldPosition.HasValue)
        {
            GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(worldPosition.Value, starSeed, galaxy.Spec);
            SolarSystem? galaxySystem = GalaxySystemGenerator.GenerateSystem(
                star,
                includeAsteroids: true,
                enablePopulation: true,
                overrides: null,
                useCaseSettings: useCaseSettings,
                galaxy: galaxy);
            if (galaxySystem != null)
            {
                ColonizationSimulationOverlay.ApplyToSystem(galaxySystem, starSeed, galaxy);
                return galaxySystem;
            }
        }

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
        spec.GeneratePopulation = true;
        if (useCaseSettings != null)
        {
            spec.UseCaseSettings = useCaseSettings.Clone();
        }

        SolarSystem? system = SystemFixtureGenerator.GenerateSystem(spec);
        if (system != null)
        {
            ColonizationSimulationOverlay.ApplyToSystem(system, starSeed, galaxy);
        }
        return system;
    }

    /// <summary>
    /// Applies authoritative colonization simulation state to an opened system.
    /// </summary>
    private void ApplyColonizationSimulationToSystem(SolarSystem system, int starSeed)
    {
        ColonizationSimulationOverlay.ApplyToSystem(system, starSeed, _galaxyViewer?.GetGalaxy());
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
        if (body == null)
        {
            return;
        }

        if (_objectOrigin == NavigationOrigin.System && _systemViewer != null)
        {
            SolarSystem? system = _systemViewer.GetCurrentSystem();
            if (system != null && system.GetBody(body.Id) != null)
            {
                system.AddBody(body);
                _systemViewer.DisplaySystem(system);
            }
        }

        if (starSeed == 0)
        {
            return;
        }

        _bodyOverrides.SetOverride(starSeed, body);
        _systemCache.Evict(starSeed);
    }

    /// <summary>
    /// Starts a new galaxy with default config.
    /// </summary>
    public void start_galaxy_with_defaults()
    {
        CreateGalaxyViewer(GenerateRandomSeed(), GalaxyConfig.CreateDefault());
        ShowGalaxyViewer();
    }

    /// <summary>
    /// Tweens the startup fade overlay to the requested alpha.
    /// </summary>
    private async Task TweenStartupFadeToAlpha(float targetAlpha)
    {
        if (_startupTransitionRect == null || !IsInsideTree())
        {
            return;
        }

        Tween tween = CreateTween();
        tween.TweenProperty(_startupTransitionRect, "color:a", targetAlpha, StartupScreenFadeDurationSeconds);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
