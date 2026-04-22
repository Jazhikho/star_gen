using System.Collections.Generic;
using StarGen.Domain.Colonization;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Systems;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Generates lightweight solar-system previews from galaxy-star context.
/// </summary>
public static class StarSystemPreview
{
    /// <summary>
    /// Generates a preview for the given star seed and world position.
    /// </summary>
    public static StarSystemPreviewData? Generate(
        int starSeed,
        Godot.Vector3 worldPosition,
        GalaxySpec galaxySpec,
        GenerationUseCaseSettings? useCaseSettings = null,
        Galaxy? galaxy = null)
    {
        if (starSeed == 0 || galaxySpec == null)
        {
            return null;
        }

        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(worldPosition, starSeed, galaxySpec);
        SolarSystem? system = null;
        if (galaxy != null)
        {
            SolarSystem? cachedSystem = galaxy.GetCachedSystem(starSeed);
            if (cachedSystem != null)
            {
                system = SystemSerializer.Clone(cachedSystem);
            }
        }

        if (system == null)
        {
            bool enablePopulation = true;
            system = GalaxySystemGenerator.GenerateSystem(star, true, enablePopulation, null, useCaseSettings, galaxy);
            if (system != null)
            {
                ColonizationSimulationOverlay.ApplyToSystem(system, starSeed, galaxy);
                if (galaxy != null)
                {
                    SolarSystem? cachedCopy = SystemSerializer.Clone(system);
                    if (cachedCopy != null)
                    {
                        galaxy.CacheSystem(starSeed, cachedCopy);
                    }
                }
            }
        }

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

        int biosphereWorldCount = 0;
        int sentientWorldCount = 0;
        foreach (CelestialBody body in system.Bodies.Values)
        {
            if (!body.HasPopulationData() || body.PopulationData == null)
            {
                continue;
            }

            if (body.PopulationData.EcologyState != null
                && body.PopulationData.EcologyState.Status == Domain.Concepts.ConceptRunStatus.Generated)
            {
                biosphereWorldCount += 1;
            }

            if (body.PopulationData.SentienceAssessment != null
                && body.PopulationData.SentienceAssessment.HasSentientLife)
            {
                sentientWorldCount += 1;
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
            BiosphereWorldCount = biosphereWorldCount,
            SentientWorldCount = sentientWorldCount,
            IsInhabited = system.IsInhabited(),
            System = system,
        };
    }
}
