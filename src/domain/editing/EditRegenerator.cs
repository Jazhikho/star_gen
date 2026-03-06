using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Editing;

/// <summary>
/// Regenerates a celestial body with locked properties held fixed.
/// Deterministic: same seed + same locked set -> identical output.
/// </summary>
public static class EditRegenerator
{
    /// <summary>Regenerates a body of the given type with locked properties fixed.</summary>
    public static RegenerateResult Regenerate(
        CelestialType.Type bodyType,
        ConstraintSet constraints,
        int seedValue,
        ParentContext? context = null)
    {
        SeededRng rng = new SeededRng(seedValue);
        if (bodyType == CelestialType.Type.Star)
        {
            return RegenerateStar(constraints, seedValue, rng);
        }

        if (bodyType == CelestialType.Type.Planet)
        {
            return RegeneratePlanet(constraints, seedValue, rng, context);
        }

        if (bodyType == CelestialType.Type.Moon)
        {
            return RegenerateMoon(constraints, seedValue, rng, context);
        }

        if (bodyType == CelestialType.Type.Asteroid)
        {
            return RegenerateAsteroid(constraints, seedValue, rng, context);
        }

        return RegenerateResult.Fail($"Unsupported body type: {bodyType}");
    }

    private static RegenerateResult RegenerateStar(
        ConstraintSet constraints,
        int seedValue,
        SeededRng rng)
    {
        StarSpec spec = new StarSpec(seedValue);
        EditSpecBuilder.ApplyToSpec(spec, CelestialType.Type.Star, constraints);
        CelestialBody body = StarGenerator.Generate(spec, rng);
        return RegenerateResult.Ok(body);
    }

    private static RegenerateResult RegeneratePlanet(
        ConstraintSet constraints,
        int seedValue,
        SeededRng rng,
        ParentContext? context)
    {
        PlanetSpec spec = new PlanetSpec(seedValue);
        EditSpecBuilder.ApplyToSpec(spec, CelestialType.Type.Planet, constraints);
        ParentContext ctx = context ?? ParentContext.SunLike();
        CelestialBody body = PlanetGenerator.Generate(spec, ctx, rng, false, 0);
        return RegenerateResult.Ok(body);
    }

    private static RegenerateResult RegenerateMoon(
        ConstraintSet constraints,
        int seedValue,
        SeededRng rng,
        ParentContext? context)
    {
        MoonSpec spec = new MoonSpec(seedValue);
        EditSpecBuilder.ApplyToSpec(spec, CelestialType.Type.Moon, constraints);
        ParentContext ctx;
        if (context != null)
        {
            ctx = context;
        }
        else
        {
            ctx = ParentContext.ForMoon(
                Units.SolarMassKg,
                3.828e26,
                5778.0,
                4.6e9,
                5.2 * Units.AuMeters,
                1.898e27,
                6.9911e7,
                5.0e8);
        }

        CelestialBody? body = MoonGenerator.Generate(spec, ctx, rng, false, null, 0);
        if (body == null)
        {
            return RegenerateResult.Fail("MoonGenerator returned null");
        }

        return RegenerateResult.Ok(body);
    }

    private static RegenerateResult RegenerateAsteroid(
        ConstraintSet constraints,
        int seedValue,
        SeededRng rng,
        ParentContext? context)
    {
        AsteroidSpec spec = new AsteroidSpec(seedValue);
        EditSpecBuilder.ApplyToSpec(spec, CelestialType.Type.Asteroid, constraints);
        ParentContext ctx = context ?? ParentContext.SunLike(2.7 * Units.AuMeters);
        CelestialBody body = AsteroidGenerator.Generate(spec, ctx, rng);
        return RegenerateResult.Ok(body);
    }
}
