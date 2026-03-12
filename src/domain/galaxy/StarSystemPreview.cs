using System.Collections.Generic;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Generates lightweight solar-system previews from galaxy-star context.
/// </summary>
public static class StarSystemPreview
{
    /// <summary>
    /// Generates a preview for the given star seed and world position.
    /// </summary>
    public static StarSystemPreviewData? Generate(int starSeed, Godot.Vector3 worldPosition, GalaxySpec galaxySpec, GenerationUseCaseSettings? useCaseSettings = null)
    {
        if (starSeed == 0 || galaxySpec == null)
        {
            return null;
        }

        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(worldPosition, starSeed, galaxySpec);
        bool enablePopulation = useCaseSettings != null && useCaseSettings.IsTravellerMode();
        StarGen.Domain.Systems.SolarSystem? system = GalaxySystemGenerator.GenerateSystem(star, true, enablePopulation, null, useCaseSettings);
        if (system == null)
        {
            return null;
        }

        List<string> spectralClasses = new();
        List<float> starTemperatures = new();
        foreach (CelestialBody starBody in system.GetStars())
        {
            if (starBody.HasStellar())
            {
                spectralClasses.Add(starBody.Stellar!.SpectralClass);
                starTemperatures.Add((float)starBody.Stellar.EffectiveTemperatureK);
            }
            else
            {
                spectralClasses.Add("?");
                starTemperatures.Add(0.0f);
            }
        }

        return new StarSystemPreviewData
        {
            StarSeed = starSeed,
            WorldPosition = worldPosition,
            StarCount = system.GetStarCount(),
            SpectralClasses = spectralClasses.ToArray(),
            StarTemperatures = starTemperatures.ToArray(),
            PlanetCount = system.GetPlanetCount(),
            MoonCount = system.GetMoonCount(),
            BeltCount = system.AsteroidBelts.Count,
            Metallicity = star.Metallicity,
            TotalPopulation = system.GetTotalPopulation(),
            IsInhabited = system.IsInhabited(),
            System = system,
        };
    }
}
