using System.IO;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Services.Persistence;

namespace StarGen.App.Viewer;

/// <summary>
/// Generation and persistence handlers for ObjectViewer.
/// </summary>
public partial class ObjectViewer
{
    private void SetupControls()
    {
        SetupTypeOptions();
        SetupPresetOptions();
        SetupPopulationOptions();
        SetupUseCaseControls();

        if (_generateButton != null)
        {
            _generateButton.Pressed += OnGeneratePressed;
        }

        if (_rerollButton != null)
        {
            _rerollButton.Pressed += OnRerollPressed;
        }

        if (_saveButton != null)
        {
            _saveButton.Pressed += OnSavePressed;
        }

        if (_loadButton != null)
        {
            _loadButton.Pressed += OnLoadPressed;
        }

        if (_saveFileDialog != null)
        {
            _saveFileDialog.FileSelected += OnSaveFileSelected;
        }

        if (_loadFileDialog != null)
        {
            _loadFileDialog.FileSelected += OnLoadFileSelected;
        }
    }

    private void SetupTypeOptions()
    {
        if (_typeOption == null || _typeOption.ItemCount > 0)
        {
            return;
        }

        _typeOption.AddItem("Star", (int)ObjectType.Star);
        _typeOption.AddItem("Planet", (int)ObjectType.Planet);
        _typeOption.AddItem("Asteroid", (int)ObjectType.Asteroid);
        _typeOption.AddItem("Comet", (int)ObjectType.Comet);
        _typeOption.Selected = (int)ObjectType.Planet;
    }

    private void SetupPresetOptions()
    {
        if (_typeOption == null)
        {
            return;
        }

        if (_presetOption == null)
        {
            throw new System.InvalidOperationException("ObjectViewer scene is missing PresetOption.");
        }

        if (_presetAssumptionsLabel == null)
        {
            throw new System.InvalidOperationException("ObjectViewer scene is missing PresetAssumptionsLabel.");
        }

        if (_typeOption != null)
        {
            _typeOption.ItemSelected += OnObjectTypeChanged;
        }

        if (_presetOption != null)
        {
            _presetOption.ItemSelected += OnPresetChanged;
        }

        RebuildPresetOptions();
    }

    private void SetupPopulationOptions()
    {
        if (_populationOption == null || _populationOption.ItemCount > 0)
        {
            return;
        }

        _populationOption.AddItem("Off", 0);
        _populationOption.AddItem("On (later)", 1);
        _populationOption.Selected = 0;
    }

    private void OnGeneratePressed()
    {
        if (!_generationActionsVisible)
        {
            return;
        }

        int seedValue = 0;
        if (_seedInput != null)
        {
            seedValue = (int)_seedInput.Value;
        }

        ObjectType objectType = ObjectType.Planet;
        if (_typeOption != null)
        {
            objectType = (ObjectType)_typeOption.GetSelectedId();
        }

        GenerateObjectFromPreset(objectType, seedValue);
        UpdateFileInfoForCurrentTarget();
    }

    private void OnRerollPressed()
    {
        if (!_generationActionsVisible)
        {
            return;
        }

        int seedValue = unchecked((int)GD.Randi());
        if (_seedInput != null)
        {
            _seedInput.Value = seedValue;
        }

        OnGeneratePressed();
    }

    private void OnSavePressed()
    {
        CelestialBody? targetBody = GetCurrentSaveTargetBody();
        if (targetBody == null)
        {
            SetStatus("No object to save");
            return;
        }

        if (_saveFileDialog == null)
        {
            SetStatus("Save dialog is unavailable");
            return;
        }

        _saveFileDialog.Filters = SaveData.GetFileFilters(targetBody.Type, includeLegacy: true);
        _saveFileDialog.CurrentFile = BuildDefaultBodyFileName(targetBody);
        _saveFileDialog.PopupCentered(new Vector2I(600, 400));
    }

    private void OnLoadPressed()
    {
        if (_loadFileDialog == null)
        {
            SetStatus("Load dialog is unavailable");
            return;
        }

        _loadFileDialog.Filters = SaveData.GetFileFilters(includeLegacy: true);
        _loadFileDialog.PopupCentered(new Vector2I(600, 400));
    }

    private void OnSaveFileSelected(string path)
    {
        CelestialBody? targetBody = GetCurrentSaveTargetBody();
        if (targetBody == null)
        {
            SetStatus("No object to save");
            return;
        }

        bool compress = !path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase);
        string savePath = SaveData.ResolveSavePath(targetBody, path, compress);
        SaveData.SaveMode saveMode = GetPreferredSaveMode(targetBody);
        Error error = SaveData.SaveBody(targetBody, savePath, saveMode, compress);
        if (error != Error.Ok)
        {
            SetStatus($"Save failed: {error}");
            return;
        }

        long size = SaveData.GetFileSize(savePath);
        string sizeString = SaveData.FormatFileSize(size);
        SetStatus($"Saved {targetBody.Name} to {Path.GetFileName(savePath)} ({sizeString})");
        UpdateFileInfo(Path.GetFileName(savePath), targetBody);
    }

    private void OnLoadFileSelected(string path)
    {
        SaveDataLoadResult result = SaveData.LoadBody(path);
        if (!result.Success)
        {
            SetStatus($"Load failed: {result.ErrorMessage}");
            return;
        }

        if (result.Body == null)
        {
            SetStatus("Loaded file contains no object data");
            return;
        }

        _currentBody = result.Body;
        _gdCurrentBody = result.Body;
        _currentMoons.Clear();
        _gdCurrentMoons.Clear();
        _gdMoonById.Clear();
        _sourceStarSeed = 0;

        SetGenerationControlsEnabled(_generationActionsVisible);
        _startupState = ViewerStartupState.ViewingExistingContent;
        TryApplyUseCaseSettingsFromBody(result.Body);
        ApplyUseCaseSettingsToControls(_activeUseCaseSettings);
        SetFileControlState(true, true);
        DisplayBodyWithMoons(result.Body, _currentMoons);
        SetStatus($"Loaded: {Path.GetFileName(path)}");
        UpdateFileInfo(Path.GetFileName(path), result.Body);
    }

    public Error SaveCurrentBodyToPath(string path)
    {
        CelestialBody? targetBody = GetCurrentSaveTargetBody();
        if (targetBody == null)
        {
            return Error.InvalidParameter;
        }

        bool compress = !path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase);
        string savePath = SaveData.ResolveSavePath(targetBody, path, compress);
        return SaveData.SaveBody(targetBody, savePath, GetPreferredSaveMode(targetBody), compress);
    }

    public SaveDataLoadResult LoadBodyFromPath(string path)
    {
        SaveDataLoadResult result = SaveData.LoadBody(path);
        if (result.Success && result.Body != null)
        {
            DisplayExternalBody(result.Body, [], 0);
            _navigatedFromSystem = false;
            SetGenerationControlsEnabled(_generationActionsVisible);
            SetFileControlState(true, true);
            UpdateFileInfo(Path.GetFileName(path), result.Body);
        }

        return result;
    }

    private CelestialBody? GetCurrentSaveTargetBody()
    {
        if (_moonSystem != null && _moonSystem.GetFocusedMoon() != null)
        {
            return _moonSystem.GetFocusedMoon();
        }

        return _currentBody;
    }

    private SaveData.SaveMode GetPreferredSaveMode(CelestialBody body)
    {
        if (body.HasPopulationData() || body.HasConceptResults())
        {
            return SaveData.SaveMode.Full;
        }

        if (body.HasMeta("user_modifications"))
        {
            Variant modifications = body.GetMeta("user_modifications");
            if (modifications.VariantType == Variant.Type.Dictionary
                && ((Godot.Collections.Dictionary)modifications).Count > 0)
            {
                return SaveData.SaveMode.Full;
            }
        }

        return SaveData.SaveMode.Compact;
    }

    private string BuildDefaultBodyFileName(CelestialBody body)
    {
        string baseName = body.Name;
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = body.Id;
        }

        if (string.IsNullOrWhiteSpace(baseName) && body.Provenance != null)
        {
            baseName = $"{body.GetTypeString()}_{body.Provenance.GenerationSeed}";
        }

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = body.GetTypeString();
        }

        string safeName = baseName.Replace(" ", "_").ToLowerInvariant();
        return $"{safeName}.{SaveData.GetPreferredBinaryExtension(body.Type)}";
    }

    private void UpdateFileInfoForCurrentTarget()
    {
        CelestialBody? targetBody = GetCurrentSaveTargetBody();
        if (targetBody != null)
        {
            UpdateFileInfo(string.Empty, targetBody);
        }
    }

    private void UpdateFileInfo(string fileName, CelestialBody body)
    {
        if (_fileInfo == null)
        {
            return;
        }

        string seedText = "unknown";
        if (body.Provenance != null)
        {
            seedText = body.Provenance.GenerationSeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        string typeLabel = body.Type switch
        {
            CelestialType.Type.Star => "Star",
            CelestialType.Type.Planet => "Planet",
            CelestialType.Type.Moon => "Moon",
            CelestialType.Type.Asteroid => "Asteroid",
            CelestialType.Type.Comet => "Comet",
            _ => "Object",
        };

        if (string.IsNullOrEmpty(fileName))
        {
            _fileInfo.Text = $"{typeLabel} save ready. Seed: {seedText}";
            return;
        }

        _fileInfo.Text = $"{typeLabel} file: {fileName} | Seed: {seedText}";
    }

    private void OnObjectTypeChanged(long _index)
    {
        RebuildPresetOptions();
    }

    private void OnPresetChanged(long _index)
    {
        UpdatePresetAssumptions();
    }

    private void RebuildPresetOptions()
    {
        if (_presetOption == null)
        {
            return;
        }

        _presetOption.Clear();
        ObjectType objectType = ObjectType.Planet;
        if (_typeOption != null)
        {
            objectType = (ObjectType)_typeOption.GetSelectedId();
        }

        if (objectType == ObjectType.Star)
        {
            _presetOption.AddItem("Random", 0);
            _presetOption.AddItem("Sun-like", 1);
            _presetOption.AddItem("Red Dwarf", 2);
            _presetOption.AddItem("Hot Blue", 3);
        }
        else if (objectType == ObjectType.Planet)
        {
            _presetOption.AddItem("Random", 0);
            _presetOption.AddItem("Earth-like", 1);
            _presetOption.AddItem("Hot Jupiter", 2);
            _presetOption.AddItem("Cold Giant", 3);
            _presetOption.AddItem("Mars-like", 4);
            _presetOption.AddItem("Dwarf Planet", 5);
            _presetOption.AddItem("Ice Giant", 6);
        }
        else if (objectType == ObjectType.Asteroid)
        {
            _presetOption.AddItem("Random", 0);
            _presetOption.AddItem("Carbonaceous", 1);
            _presetOption.AddItem("Metallic", 2);
            _presetOption.AddItem("Stony", 3);
            _presetOption.AddItem("Dark Red", 4);
            _presetOption.AddItem("Basaltic", 5);
            _presetOption.AddItem("Ceres-like", 6);
        }
        else
        {
            _presetOption.AddItem("Random", 0);
            _presetOption.AddItem("Jupiter-family", 1);
            _presetOption.AddItem("Long-period", 2);
            _presetOption.AddItem("Dormant", 3);
            _presetOption.AddItem("Extinct", 4);
        }

        _presetOption.Selected = 0;
        UpdatePresetAssumptions();
    }

    private void GenerateObjectFromPreset(ObjectType objectType, int seedValue)
    {
        if (objectType == ObjectType.Star)
        {
            GenerateStarFromPreset(seedValue);
            return;
        }

        if (objectType == ObjectType.Planet)
        {
            GeneratePlanetFromPreset(seedValue);
            return;
        }

        if (objectType == ObjectType.Asteroid)
        {
            GenerateAsteroidFromPreset(seedValue);
            return;
        }

        if (objectType == ObjectType.Comet)
        {
            GenerateCometFromPreset(seedValue);
            return;
        }

        if (objectType == ObjectType.Moon)
        {
            GenerateMoonFromPreset(seedValue);
            return;
        }

        generate_object(objectType, seedValue);
    }

    private void GenerateObjectFromRequest(ObjectGenerationRequest request)
    {
        if (request.SpecData.Count == 0)
        {
            GenerateObjectFromPreset(request.ObjectType, request.SeedValue);
            return;
        }

        if (request.ObjectType == ObjectType.Planet)
        {
            PlanetSpec spec = PlanetSpec.FromDictionary(request.SpecData);
            GeneratePlanetFromSpec(spec, request.PresetId, request.TravellerWorldProfileData);
            return;
        }

        if (request.ObjectType == ObjectType.Star)
        {
            StarSpec spec = StarSpec.FromDictionary(request.SpecData);
            GenerateStarFromSpec(spec, request.PresetId);
            return;
        }

        if (request.ObjectType == ObjectType.Asteroid)
        {
            AsteroidSpec spec = AsteroidSpec.FromDictionary(request.SpecData);
            GenerateAsteroidFromSpec(spec, request.PresetId);
            return;
        }

        if (request.ObjectType == ObjectType.Comet)
        {
            CometSpec spec = CometSpec.FromDictionary(request.SpecData);
            GenerateCometFromSpec(spec, request.PresetId);
            return;
        }

        if (request.ObjectType == ObjectType.Moon)
        {
            MoonSpec spec = MoonSpec.FromDictionary(request.SpecData);
            GenerateMoonFromSpec(spec, request.PresetId);
            return;
        }

        GenerateObjectFromPreset(request.ObjectType, request.SeedValue);
    }

    private void GenerateStarFromPreset(int seedValue)
    {
        int presetId = _presetOption?.GetSelectedId() ?? 0;
        StarSpec spec;
        if (presetId == 1)
        {
            spec = StarSpec.SunLike(seedValue);
        }
        else if (presetId == 2)
        {
            spec = StarSpec.RedDwarf(seedValue);
        }
        else if (presetId == 3)
        {
            spec = StarSpec.HotBlue(seedValue);
        }
        else
        {
            spec = StarSpec.Random(seedValue);
        }

        spec.UseCaseSettings = _activeUseCaseSettings.Clone();
        GenerateStarFromSpec(spec, presetId);
    }

    private void GenerateStarFromSpec(StarSpec spec, int presetId)
    {
        SeededRng rng = new SeededRng(spec.GenerationSeed);
        CelestialBody body = StarGenerator.Generate(spec, rng);
        DisplayGeneratedBody(body, ObjectType.Star, presetId);
    }

    private void GeneratePlanetFromPreset(int seedValue)
    {
        int presetId = _presetOption?.GetSelectedId() ?? 0;
        PlanetSpec spec;
        if (presetId == 1)
        {
            spec = PlanetSpec.EarthLike(seedValue);
        }
        else if (presetId == 2)
        {
            spec = PlanetSpec.HotJupiter(seedValue);
        }
        else if (presetId == 3)
        {
            spec = PlanetSpec.ColdGiant(seedValue);
        }
        else if (presetId == 4)
        {
            spec = PlanetSpec.MarsLike(seedValue);
        }
        else if (presetId == 5)
        {
            spec = PlanetSpec.DwarfPlanet(seedValue);
        }
        else if (presetId == 6)
        {
            spec = PlanetSpec.IceGiant(seedValue);
        }
        else
        {
            spec = PlanetSpec.Random(seedValue);
        }

        spec.UseCaseSettings = _activeUseCaseSettings.Clone();
        GeneratePlanetFromSpec(spec, presetId, new Godot.Collections.Dictionary());
    }

    private void GeneratePlanetFromSpec(PlanetSpec spec, int presetId, Godot.Collections.Dictionary travellerWorldProfileData)
    {
        SeededRng rng = new SeededRng(spec.GenerationSeed);
        CelestialBody body = PlanetGenerator.Generate(spec, ParentContext.SunLike(), rng);
        Godot.Collections.Array<CelestialBody> moons = [];
        if (travellerWorldProfileData.Count > 0 && body.Provenance != null)
        {
            body.Provenance.SpecSnapshot["traveller_world_profile"] = travellerWorldProfileData.Duplicate(true);
            TravellerWorldProfile travellerProfile = TravellerWorldProfile.FromDictionary(travellerWorldProfileData);
            body.Name = ApplyTravellerDisplayName(body.Name, travellerProfile);
        }

        if (spec.GetOverride("studio.generate_moon", false).VariantType == Variant.Type.Bool
            && (bool)spec.GetOverride("studio.generate_moon", false))
        {
            MoonSpec moonSpec = BuildMoonSpecForPlanet(spec, body);
            SeededRng moonRng = new SeededRng(spec.GenerationSeed + 1);
            ParentContext moonContext = BuildMoonContextForPlanet(body);
            CelestialBody? moon = MoonGenerator.Generate(moonSpec, moonContext, moonRng);
            if (moon != null)
            {
                if (moon.Orbital != null)
                {
                    moon.Orbital.ParentId = body.Id;
                }

                moons.Add(moon);
            }
        }

        DisplayGeneratedBody(body, ObjectType.Planet, presetId, moons);
    }

    private void GenerateMoonFromPreset(int seedValue)
    {
        int presetId = _presetOption?.GetSelectedId() ?? 0;
        MoonSpec spec;
        if (presetId == 1)
        {
            spec = MoonSpec.LunaLike(seedValue);
        }
        else if (presetId == 2)
        {
            spec = MoonSpec.EuropaLike(seedValue);
        }
        else if (presetId == 3)
        {
            spec = MoonSpec.TitanLike(seedValue);
        }
        else if (presetId == 4)
        {
            spec = MoonSpec.Captured(seedValue);
        }
        else
        {
            spec = MoonSpec.Random(seedValue);
        }

        spec.UseCaseSettings = _activeUseCaseSettings.Clone();
        GenerateMoonFromSpec(spec, presetId);
    }

    private void GenerateMoonFromSpec(MoonSpec spec, int presetId)
    {
        SeededRng rng = new SeededRng(spec.GenerationSeed);
        ParentContext moonContext = ParentContext.ForMoon(
            Units.SolarMassKg,
            3.828e26,
            5778.0,
            4.6e9,
            5.2 * Units.AuMeters,
            Units.JupiterMassKg,
            Units.JupiterRadiusMeters,
            5.0e8);
        CelestialBody? body = MoonGenerator.Generate(spec, moonContext, rng);
        if (body == null)
        {
            return;
        }

        DisplayGeneratedBody(body, ObjectType.Moon, presetId);
    }

    private void GenerateAsteroidFromPreset(int seedValue)
    {
        int presetId = _presetOption?.GetSelectedId() ?? 0;
        AsteroidSpec spec;
        if (presetId == 1)
        {
            spec = AsteroidSpec.Carbonaceous(seedValue);
        }
        else if (presetId == 2)
        {
            spec = AsteroidSpec.Metallic(seedValue);
        }
        else if (presetId == 3)
        {
            spec = AsteroidSpec.Stony(seedValue);
        }
        else if (presetId == 4)
        {
            spec = AsteroidSpec.DarkRedPrimitive(seedValue);
        }
        else if (presetId == 5)
        {
            spec = AsteroidSpec.Basaltic(seedValue);
        }
        else if (presetId == 6)
        {
            spec = AsteroidSpec.CeresLike(seedValue);
        }
        else
        {
            spec = AsteroidSpec.Random(seedValue);
        }

        spec.UseCaseSettings = _activeUseCaseSettings.Clone();
        GenerateAsteroidFromSpec(spec, presetId);
    }

    private void GenerateAsteroidFromSpec(AsteroidSpec spec, int presetId)
    {
        SeededRng rng = new SeededRng(spec.GenerationSeed);
        CelestialBody body = AsteroidGenerator.Generate(spec, ParentContext.SunLike(), rng);
        DisplayGeneratedBody(body, ObjectType.Asteroid, presetId);
    }

    private void GenerateCometFromPreset(int seedValue)
    {
        int presetId = _presetOption?.GetSelectedId() ?? 0;
        CometSpec spec;
        if (presetId == 1)
        {
            spec = CometSpec.JupiterFamily(seedValue);
        }
        else if (presetId == 2)
        {
            spec = CometSpec.LongPeriod(seedValue);
        }
        else if (presetId == 3)
        {
            spec = new CometSpec(seedValue, -1, (int)CometGenerator.ActivityType.Dormant);
        }
        else if (presetId == 4)
        {
            spec = new CometSpec(seedValue, -1, (int)CometGenerator.ActivityType.Extinct);
        }
        else
        {
            spec = CometSpec.Random(seedValue);
        }

        spec.UseCaseSettings = _activeUseCaseSettings.Clone();
        GenerateCometFromSpec(spec, presetId);
    }

    private void GenerateCometFromSpec(CometSpec spec, int presetId)
    {
        SeededRng rng = new SeededRng(spec.GenerationSeed);
        CelestialBody body = CometGenerator.Generate(spec, ParentContext.SunLike(5.0 * Units.AuMeters), rng);
        DisplayGeneratedBody(body, ObjectType.Comet, presetId);
    }

    private void UpdatePresetAssumptions()
    {
        if (_presetAssumptionsLabel == null)
        {
            return;
        }

        ObjectType objectType = ObjectType.Planet;
        if (_typeOption != null)
        {
            objectType = (ObjectType)_typeOption.GetSelectedId();
        }

        int presetId = _presetOption?.GetSelectedId() ?? 0;
        _presetAssumptionsLabel.Text = GetPresetAssumptionText(objectType, presetId);
    }

    private string GetPresetAssumptionText(ObjectType objectType, int presetId)
    {
        if (objectType == ObjectType.Star)
        {
            if (presetId == 1)
            {
                return "Sun-like targets a G-class star with near-solar metallicity; the generator still fills in the rest deterministically.";
            }

            if (presetId == 2)
            {
                return "Red Dwarf locks the spectral target to M-class while leaving age and metallicity generator-driven.";
            }

            if (presetId == 3)
            {
                return "Hot Blue biases toward a luminous B-class star; this is realistic but intentionally rare.";
            }

            return "Random leaves all star-class targets open and rolls the full stellar parameter set from the seed.";
        }

        if (objectType == ObjectType.Planet)
        {
            if (presetId == 1)
            {
                return "Earth-like locks a terrestrial size target in the temperate zone, with atmosphere present and rings disabled.";
            }

            if (presetId == 2)
            {
                return "Hot Jupiter locks a gas-giant size target in the hot zone; downstream chemistry and visuals stay seed-driven.";
            }

            if (presetId == 3)
            {
                return "Cold Giant targets a gas giant in the cold zone and leaves ring complexity open.";
            }

            if (presetId == 4)
            {
                return "Mars-like biases toward a smaller terrestrial world in the cold zone with a thin-atmosphere style target.";
            }

            if (presetId == 5)
            {
                return "Dwarf Planet locks a dwarf-size target with no atmosphere or rings.";
            }

            if (presetId == 6)
            {
                return "Ice Giant locks a Neptune-class body in the cold zone and prefers trace rings.";
            }

            return "Random leaves size, orbit zone, atmosphere, and ring targets open for a fully seed-driven planet roll.";
        }

        if (objectType == ObjectType.Asteroid)
        {
            if (presetId == 1)
            {
                return "Carbonaceous biases toward dark, carbon-rich rock and a typical main-belt orbit.";
            }

            if (presetId == 2)
            {
                return "Metallic biases toward dense, iron-rich material with a brighter reflective surface.";
            }

            if (presetId == 3)
            {
                return "Stony biases toward silicate-rich rock with a balanced reflectivity profile.";
            }

            if (presetId == 4)
            {
                return "Dark Red biases toward primitive outer-system material with low reflectivity.";
            }

            if (presetId == 5)
            {
                return "Basaltic biases toward differentiated rock, denser material, and a brighter surface.";
            }

            if (presetId == 6)
            {
                return "Ceres-like biases toward a larger carbonaceous body with more dwarf-like proportions.";
            }

            return "Random asteroid generation leaves type, orbit band, density, and brightness open for the seed to resolve.";
        }

        if (presetId == 1)
        {
            return "Jupiter-family biases toward a shorter-period comet shaped by the giant-planet region.";
        }

        if (presetId == 2)
        {
            return "Long-period biases toward a distant icy body on a much more stretched orbit.";
        }

        if (presetId == 3)
        {
            return "Dormant keeps comet structure but biases away from a bright, active state.";
        }

        if (presetId == 4)
        {
            return "Extinct biases toward a spent comet with darker, dustier surface behavior.";
        }

        return "Random comet generation leaves family, activity, and nucleus size open for the seed to resolve.";
    }

    private void DisplayGeneratedBody(CelestialBody body, ObjectType objectType, int presetId, Godot.Collections.Array<CelestialBody>? moons = null)
    {
        Godot.Collections.Array moonPayload = [];
        if (moons != null)
        {
            foreach (CelestialBody moon in moons)
            {
                moonPayload.Add(moon);
            }
        }

        DisplayExternalBody(body, moonPayload, 0);
        _navigatedFromSystem = false;
        _startupState = ViewerStartupState.ViewingExistingContent;
        SetGenerationControlsEnabled(_generationActionsVisible);
        SetFileControlState(true, true);
        string presetLabel = "Random";
        if (_presetOption != null && _presetOption.ItemCount > 0)
        {
            presetLabel = _presetOption.GetItemText(Mathf.Clamp(presetId, 0, _presetOption.ItemCount - 1));
        }

        SetStatus($"Generated {objectType}: {presetLabel}");
    }

    private static MoonSpec BuildMoonSpecForPlanet(PlanetSpec planetSpec, CelestialBody planetBody)
    {
        bool captured = planetSpec.GetOverride("studio.moon_captured", false).VariantType == Variant.Type.Bool
            && (bool)planetSpec.GetOverride("studio.moon_captured", false);
        MoonSpec moonSpec;
        if (captured)
        {
            moonSpec = MoonSpec.Captured(planetSpec.GenerationSeed + 1);
        }
        else
        {
            moonSpec = MoonSpec.Random(planetSpec.GenerationSeed + 1);
        }

        moonSpec.UseCaseSettings = planetSpec.UseCaseSettings.Clone();
        return moonSpec;
    }

    private static ParentContext BuildMoonContextForPlanet(CelestialBody planetBody)
    {
        double planetOrbitalDistanceM = Units.AuMeters;
        if (planetBody.Orbital != null && planetBody.Orbital.SemiMajorAxisM > 0.0)
        {
            planetOrbitalDistanceM = planetBody.Orbital.SemiMajorAxisM;
        }

        double moonOrbitDistanceM = System.Math.Max(planetBody.Physical.RadiusM * 12.0, 2.0e8);
        return ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            planetOrbitalDistanceM,
            planetBody.Physical.MassKg,
            planetBody.Physical.RadiusM,
            moonOrbitDistanceM);
    }

    private static string ApplyTravellerDisplayName(string currentName, TravellerWorldProfile profile)
    {
        if (!string.IsNullOrWhiteSpace(currentName))
        {
            return currentName;
        }

        return $"World {profile.ToUwpString()}";
    }
}
