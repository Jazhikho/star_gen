using System;
using System.Collections.Generic;
using Godot;
using StarGen.App.Viewer;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.App;

public partial class ObjectGenerationScreen
{
    private void OnPresetChanged()
    {
        ApplyPresetToEnhancedControls();
        RefreshEnhancedFieldPresentation();
        RefreshEnhancedParameterVisibility();
        RefreshSummary();
    }

    private ObjectGenerationRequest BuildEnhancedRequest()
    {
        ObjectGenerationRequest request = new ObjectGenerationRequest();
        request.ObjectType = GetSelectedObjectType();
        request.PresetId = _presetOption?.GetSelectedId() ?? 0;
        request.SeedValue = _seedInput != null ? (int)_seedInput.Value : 0;
        request.ShowSeed = _showSeedControls;
        request.UseCaseSettings = BuildEnhancedUseCaseSettingsFromControls();
        request.SpecData = BuildEnhancedSpecData(request);
        request.TravellerWorldProfileData = BuildTravellerWorldProfileData(request);
        return request;
    }

    private Godot.Collections.Dictionary BuildEnhancedSpecData(ObjectGenerationRequest request)
    {
        if (request.ObjectType == ObjectViewer.ObjectType.Planet)
        {
            return CreatePlanetSpec(request).ToDictionary();
        }

        if (request.ObjectType == ObjectViewer.ObjectType.Moon)
        {
            return CreateMoonSpec(request).ToDictionary();
        }

        if (request.ObjectType == ObjectViewer.ObjectType.Star)
        {
            return CreateStarSpec(request).ToDictionary();
        }

        if (request.ObjectType == ObjectViewer.ObjectType.Asteroid)
        {
            return CreateAsteroidSpec(request).ToDictionary();
        }

        return new Godot.Collections.Dictionary();
    }

    private Godot.Collections.Dictionary BuildTravellerWorldProfileData(ObjectGenerationRequest request)
    {
        if (request.ObjectType != ObjectViewer.ObjectType.Planet || !ShouldUseTravellerWorldGeneration())
        {
            return new Godot.Collections.Dictionary();
        }

        int orbitZoneValue = ResolveTravellerOrbitZoneForGeneration(request.SeedValue, request.PresetId);

        SeededRng rng = new SeededRng(request.SeedValue);
        TravellerWorldProfile profile = TravellerWorldGenerator.GeneratePlanetProfile(
            GetTravellerRequestedCode(_travellerSizeCodeOption),
            GetTravellerRequestedCode(_travellerAtmosphereCodeOption),
            GetTravellerRequestedCode(_travellerHydrographicsCodeOption),
            GetTravellerRequestedCode(_travellerPopulationCodeOption),
            (OrbitZone.Zone)orbitZoneValue,
            rng);
        return profile.ToDictionary();
    }

    private PlanetSpec CreatePlanetSpec(ObjectGenerationRequest request)
    {
        PlanetSpec spec = CreatePlanetPresetSpec(request.SeedValue, request.PresetId);
        ApplyCommonSpecFields(spec, request.UseCaseSettings);
        spec.SizeCategory = _planetSizeCategoryOption?.GetSelectedId() ?? spec.SizeCategory;
        spec.OrbitZone = _planetOrbitZoneOption?.GetSelectedId() ?? spec.OrbitZone;
        spec.HasAtmosphere = GetTriStatePreference(_planetAtmosphereOption);
        spec.HasRings = GetTriStatePreference(_planetRingsOption);
        spec.RingComplexity = _planetRingComplexityOption?.GetSelectedId() ?? spec.RingComplexity;

        Godot.Collections.Dictionary travellerData = BuildTravellerWorldProfileData(request);
        if (travellerData.Count > 0)
        {
            if (spec.OrbitZone < 0)
            {
                spec.OrbitZone = ResolveTravellerOrbitZoneForGeneration(request.SeedValue, request.PresetId);
            }

            TravellerWorldProfile profile = TravellerWorldProfile.FromDictionary(travellerData);
            TravellerWorldGenerator.ApplyToPlanetSpec(spec, profile, new SeededRng(request.SeedValue));
        }

        ApplySharedAdvancedOverrides(spec, request.ObjectType);
        ApplySurfaceOverrides(spec);
        return spec;
    }

    private MoonSpec CreateMoonSpec(ObjectGenerationRequest request)
    {
        MoonSpec spec = CreateMoonPresetSpec(request.SeedValue, request.PresetId);
        ApplyCommonSpecFields(spec, request.UseCaseSettings);
        spec.SizeCategory = _moonSizeCategoryOption?.GetSelectedId() ?? spec.SizeCategory;
        if (_moonCapturedCheck != null)
        {
            spec.IsCaptured = _moonCapturedCheck.ButtonPressed;
        }

        spec.HasAtmosphere = GetTriStatePreference(_moonAtmosphereOption);
        spec.HasSubsurfaceOcean = GetTriStatePreference(_moonOceanOption);
        ApplySharedAdvancedOverrides(spec, request.ObjectType);
        ApplySurfaceOverrides(spec);
        return spec;
    }

    private StarSpec CreateStarSpec(ObjectGenerationRequest request)
    {
        StarSpec spec = CreateStarPresetSpec(request.SeedValue, request.PresetId);
        ApplyCommonSpecFields(spec, request.UseCaseSettings);
        spec.SpectralClass = _starSpectralClassOption?.GetSelectedId() ?? spec.SpectralClass;

        if (TryGetOptionalValue("StarSubclass", out double subclass))
        {
            spec.Subclass = (int)subclass;
        }
        else
        {
            spec.Subclass = -1;
        }

        if (TryGetOptionalValue("StarMetallicity", out double metallicity))
        {
            spec.Metallicity = metallicity;
        }
        else
        {
            spec.Metallicity = -1.0;
        }

        if (TryGetOptionalValue("StarAgeGyr", out double ageGyr))
        {
            spec.AgeYears = ageGyr * 1.0e9;
        }
        else
        {
            spec.AgeYears = -1.0;
        }

        ApplySharedAdvancedOverrides(spec, request.ObjectType);
        if (TryGetOptionalValue("TemperatureOverride", out double temperatureK))
        {
            spec.SetOverride("stellar.temperature_k", temperatureK);
        }

        if (TryGetOptionalValue("LuminosityOverride", out double luminositySolar))
        {
            spec.SetOverride("stellar.luminosity_watts", luminositySolar * 3.828e26);
        }

        return spec;
    }

    private AsteroidSpec CreateAsteroidSpec(ObjectGenerationRequest request)
    {
        AsteroidSpec spec = CreateAsteroidPresetSpec(request.SeedValue, request.PresetId);
        ApplyCommonSpecFields(spec, request.UseCaseSettings);
        spec.AsteroidType = _asteroidTypeOption?.GetSelectedId() ?? spec.AsteroidType;
        if (_asteroidLargeCheck != null)
        {
            spec.IsLarge = _asteroidLargeCheck.ButtonPressed;
        }

        ApplySharedAdvancedOverrides(spec, request.ObjectType);
        if (TryGetOptionalValue("AlbedoOverride", out double albedo))
        {
            spec.SetOverride("surface.albedo", albedo);
        }

        return spec;
    }

    private void ApplyCommonSpecFields(BaseSpec spec, GenerationUseCaseSettings useCaseSettings)
    {
        if (_nameInput != null)
        {
            spec.NameHint = _nameInput.Text.Trim();
        }

        spec.UseCaseSettings = useCaseSettings.Clone();
    }

    private void ApplySharedAdvancedOverrides(BaseSpec spec, ObjectViewer.ObjectType objectType)
    {
        if (TryGetOptionalValue("MassOverride", out double massValue))
        {
            spec.SetOverride("physical.mass_kg", ConvertMassToBaseUnits(massValue, objectType));
        }

        if (TryGetOptionalValue("RadiusOverride", out double radiusValue))
        {
            spec.SetOverride("physical.radius_m", ConvertRadiusToBaseUnits(radiusValue, objectType));
        }

        if (TryGetOptionalValue("RotationOverride", out double rotationHours))
        {
            spec.SetOverride("physical.rotation_period_s", rotationHours * 3600.0);
        }

        if (TryGetOptionalValue("AxialTiltOverride", out double axialTilt))
        {
            spec.SetOverride("physical.axial_tilt_deg", axialTilt);
        }

        if (TryGetOptionalValue("SemiMajorAxisOverride", out double semiMajorAxisAu))
        {
            spec.SetOverride("orbital.semi_major_axis_m", semiMajorAxisAu * Units.AuMeters);
        }

        if (TryGetOptionalValue("EccentricityOverride", out double eccentricity))
        {
            spec.SetOverride("orbital.eccentricity", eccentricity);
        }

        if (TryGetOptionalValue("InclinationOverride", out double inclinationDeg))
        {
            spec.SetOverride("orbital.inclination_deg", inclinationDeg);
        }

        if (TryGetOptionalValue("SurfacePressureOverride", out double surfacePressureAtm))
        {
            spec.SetOverride("atmosphere.surface_pressure_pa", surfacePressureAtm * 101325.0);
        }
    }

    private void ApplySurfaceOverrides(BaseSpec spec)
    {
        if (TryGetOptionalValue("AlbedoOverride", out double albedo))
        {
            spec.SetOverride("surface.albedo", albedo);
        }

        if (TryGetOptionalValue("VolcanismOverride", out double volcanism))
        {
            spec.SetOverride("surface.volcanism_level", volcanism);
        }
    }

    private bool ShouldUseTravellerWorldGeneration()
    {
        return GetSelectedObjectType() == ObjectViewer.ObjectType.Planet
            && _rulesetModeOption != null
            && _rulesetModeOption.GetSelectedId() == (int)GenerationUseCaseSettings.RulesetModeType.Traveller
            && _useTravellerWorldProfileCheck != null
            && _useTravellerWorldProfileCheck.ButtonPressed;
    }

    private bool TryGetOptionalValue(string key, out double value)
    {
        value = 0.0;
        if (!_optionalToggles.TryGetValue(key, out CheckBox? toggle) || !toggle.ButtonPressed)
        {
            return false;
        }

        if (!_optionalInputs.TryGetValue(key, out SpinBox? input))
        {
            return false;
        }

        value = input.Value;
        return true;
    }

    private void SetOptionalValue(string key, bool enabled, double value)
    {
        if (_optionalToggles.TryGetValue(key, out CheckBox? toggle))
        {
            toggle.ButtonPressed = enabled;
        }

        if (_optionalInputs.TryGetValue(key, out SpinBox? input))
        {
            input.Value = value;
            input.Editable = enabled;
        }
    }

    private double ConvertMassToBaseUnits(double value, ObjectViewer.ObjectType objectType)
    {
        if (objectType == ObjectViewer.ObjectType.Star)
        {
            return value * Units.SolarMassKg;
        }

        if (objectType == ObjectViewer.ObjectType.Asteroid)
        {
            return value * 1.0e15;
        }

        return value * Units.EarthMassKg;
    }

    private double ConvertRadiusToBaseUnits(double value, ObjectViewer.ObjectType objectType)
    {
        if (objectType == ObjectViewer.ObjectType.Star)
        {
            return value * Units.SolarRadiusMeters;
        }

        if (objectType == ObjectViewer.ObjectType.Asteroid)
        {
            return value * 1000.0;
        }

        return value * Units.EarthRadiusMeters;
    }

    private void RebuildEnhancedPresetOptions()
    {
        if (_presetOption == null)
        {
            return;
        }

        _presetOption.Clear();
        ObjectViewer.ObjectType objectType = GetSelectedObjectType();
        if (objectType == ObjectViewer.ObjectType.Star)
        {
            _presetOption.AddItem("Random", 0);
            _presetOption.AddItem("Sun-like", 1);
            _presetOption.AddItem("Red Dwarf", 2);
            _presetOption.AddItem("Hot Blue", 3);
        }
        else if (objectType == ObjectViewer.ObjectType.Planet)
        {
            _presetOption.AddItem("Random", 0);
            _presetOption.AddItem("Earth-like", 1);
            _presetOption.AddItem("Hot Jupiter", 2);
            _presetOption.AddItem("Cold Giant", 3);
            _presetOption.AddItem("Mars-like", 4);
            _presetOption.AddItem("Dwarf Planet", 5);
            _presetOption.AddItem("Ice Giant", 6);
        }
        else if (objectType == ObjectViewer.ObjectType.Moon)
        {
            _presetOption.AddItem("Random", 0);
            _presetOption.AddItem("Luna-like", 1);
            _presetOption.AddItem("Europa-like", 2);
            _presetOption.AddItem("Titan-like", 3);
            _presetOption.AddItem("Captured", 4);
        }
        else
        {
            _presetOption.AddItem("Random", 0);
            _presetOption.AddItem("Carbonaceous", 1);
            _presetOption.AddItem("Metallic", 2);
            _presetOption.AddItem("Stony", 3);
            _presetOption.AddItem("Ceres-like", 4);
        }

        _presetOption.Select(0);
        ApplyPresetToEnhancedControls();
    }

    private void ApplyPresetToEnhancedControls()
    {
        ResetEnhancedOptionalInputs();
        ObjectViewer.ObjectType objectType = GetSelectedObjectType();
        int seedValue = _seedInput != null ? (int)_seedInput.Value : 0;
        if (_nameInput != null)
        {
            _nameInput.Text = string.Empty;
        }

        if (objectType == ObjectViewer.ObjectType.Planet)
        {
            PlanetSpec spec = CreatePlanetPresetSpec(seedValue, _presetOption?.GetSelectedId() ?? 0);
            SelectOptionById(_planetSizeCategoryOption, spec.HasSizeCategory() ? spec.SizeCategory : -1);
            SelectOptionById(_planetOrbitZoneOption, spec.HasOrbitZone() ? spec.OrbitZone : -1);
            SelectOptionByVariantPreference(_planetAtmosphereOption, spec.HasAtmosphere);
            SelectOptionByVariantPreference(_planetRingsOption, spec.HasRings);
            SelectOptionById(_planetRingComplexityOption, spec.RingComplexity);
            return;
        }

        if (objectType == ObjectViewer.ObjectType.Moon)
        {
            MoonSpec spec = CreateMoonPresetSpec(seedValue, _presetOption?.GetSelectedId() ?? 0);
            SelectOptionById(_moonSizeCategoryOption, spec.HasSizeCategory() ? spec.SizeCategory : -1);
            if (_moonCapturedCheck != null)
            {
                _moonCapturedCheck.ButtonPressed = spec.IsCaptured;
            }

            SelectOptionByVariantPreference(_moonAtmosphereOption, spec.HasAtmosphere);
            SelectOptionByVariantPreference(_moonOceanOption, spec.HasSubsurfaceOcean);
            return;
        }

        if (objectType == ObjectViewer.ObjectType.Star)
        {
            StarSpec spec = CreateStarPresetSpec(seedValue, _presetOption?.GetSelectedId() ?? 0);
            SelectOptionById(_starSpectralClassOption, spec.SpectralClass);
            SetOptionalValue("StarSubclass", spec.HasSubclass(), spec.Subclass);
            SetOptionalValue("StarMetallicity", spec.HasMetallicity(), spec.Metallicity);
            SetOptionalValue("StarAgeGyr", spec.HasAge(), spec.AgeYears / 1.0e9);
            return;
        }

        AsteroidSpec asteroidSpec = CreateAsteroidPresetSpec(seedValue, _presetOption?.GetSelectedId() ?? 0);
        SelectOptionById(_asteroidTypeOption, asteroidSpec.AsteroidType);
        if (_asteroidLargeCheck != null)
        {
            _asteroidLargeCheck.ButtonPressed = asteroidSpec.IsLarge;
        }
    }

    private void SelectOptionById(OptionButton? optionButton, int id)
    {
        if (optionButton == null)
        {
            return;
        }

        for (int index = 0; index < optionButton.ItemCount; index++)
        {
            if (optionButton.GetItemId(index) == id)
            {
                optionButton.Select(index);
                return;
            }
        }

        if (optionButton.ItemCount > 0)
        {
            optionButton.Select(0);
        }
    }

    private void SelectOptionByVariantPreference(OptionButton? optionButton, Variant value)
    {
        if (optionButton == null)
        {
            return;
        }

        if (value.VariantType == Variant.Type.Bool)
        {
            SelectOptionById(optionButton, (bool)value ? 1 : 0);
            return;
        }

        SelectOptionById(optionButton, -1);
    }

    private Variant GetTriStatePreference(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return default;
        }

        int selectedId = optionButton.GetSelectedId();
        if (selectedId < 0)
        {
            return default;
        }

        return selectedId == 1;
    }

    private int GetTravellerRequestedCode(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return -1;
        }

        return optionButton.GetSelectedId();
    }

    private int ResolveTravellerOrbitZoneForGeneration(int seedValue, int presetId)
    {
        int orbitZoneValue = _planetOrbitZoneOption?.GetSelectedId() ?? -1;
        if (orbitZoneValue >= 0)
        {
            return orbitZoneValue;
        }

        PlanetSpec presetSpec = CreatePlanetPresetSpec(seedValue, presetId);
        if (presetSpec.HasOrbitZone())
        {
            return presetSpec.OrbitZone;
        }

        return (int)OrbitZone.Zone.Temperate;
    }

    private PlanetSpec CreatePlanetPresetSpec(int seedValue, int presetId)
    {
        if (presetId == 1)
        {
            return PlanetSpec.EarthLike(seedValue);
        }

        if (presetId == 2)
        {
            return PlanetSpec.HotJupiter(seedValue);
        }

        if (presetId == 3)
        {
            return PlanetSpec.ColdGiant(seedValue);
        }

        if (presetId == 4)
        {
            return PlanetSpec.MarsLike(seedValue);
        }

        if (presetId == 5)
        {
            return PlanetSpec.DwarfPlanet(seedValue);
        }

        if (presetId == 6)
        {
            return PlanetSpec.IceGiant(seedValue);
        }

        return PlanetSpec.Random(seedValue);
    }

    private MoonSpec CreateMoonPresetSpec(int seedValue, int presetId)
    {
        if (presetId == 1)
        {
            return MoonSpec.LunaLike(seedValue);
        }

        if (presetId == 2)
        {
            return MoonSpec.EuropaLike(seedValue);
        }

        if (presetId == 3)
        {
            return MoonSpec.TitanLike(seedValue);
        }

        if (presetId == 4)
        {
            return MoonSpec.Captured(seedValue);
        }

        return MoonSpec.Random(seedValue);
    }

    private StarSpec CreateStarPresetSpec(int seedValue, int presetId)
    {
        if (presetId == 1)
        {
            return StarSpec.SunLike(seedValue);
        }

        if (presetId == 2)
        {
            return StarSpec.RedDwarf(seedValue);
        }

        if (presetId == 3)
        {
            return StarSpec.HotBlue(seedValue);
        }

        return StarSpec.Random(seedValue);
    }

    private AsteroidSpec CreateAsteroidPresetSpec(int seedValue, int presetId)
    {
        if (presetId == 1)
        {
            return AsteroidSpec.Carbonaceous(seedValue);
        }

        if (presetId == 2)
        {
            return AsteroidSpec.Metallic(seedValue);
        }

        if (presetId == 3)
        {
            return AsteroidSpec.Stony(seedValue);
        }

        if (presetId == 4)
        {
            return AsteroidSpec.CeresLike(seedValue);
        }

        return AsteroidSpec.Random(seedValue);
    }
}
