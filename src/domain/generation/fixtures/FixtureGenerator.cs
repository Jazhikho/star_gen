using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Fixtures;

/// <summary>
/// Utility for generating golden master fixtures for regression testing.
/// Creates deterministic test cases for all body types.
/// </summary>
public static class FixtureGenerator
{
    /// <summary>Base seed for fixture generation.</summary>
    public const int BaseSeed = 42000;

    /// <summary>
    /// Generates all fixtures and returns them as an array of dictionaries.
    /// Each dictionary contains: name, type, seed, spec, context (if applicable), body.
    /// </summary>
    public static Godot.Collections.Array<Dictionary> GenerateAllFixtures()
    {
        Godot.Collections.Array<Dictionary> fixtures = new();
        foreach (Dictionary d in GenerateStarFixtures())
            fixtures.Add(d);
        foreach (Dictionary d in GeneratePlanetFixtures())
            fixtures.Add(d);
        foreach (Dictionary d in GenerateMoonFixtures())
            fixtures.Add(d);
        foreach (Dictionary d in GenerateAsteroidFixtures())
            fixtures.Add(d);
        return fixtures;
    }

    /// <summary>
    /// Generates star fixtures — one per spectral class (O, B, A, F, G, K, M).
    /// </summary>
    /// <returns>Array of dictionaries, each containing name, type, seed, spec, context, and body.</returns>
    public static Godot.Collections.Array<Dictionary> GenerateStarFixtures()
    {
        Godot.Collections.Array<Dictionary> fixtures = new();
        (StarClass.SpectralClass Class, string Name)[] spectralClasses = new (StarClass.SpectralClass Class, string Name)[]
        {
            (StarClass.SpectralClass.O, "star_o_class"),
            (StarClass.SpectralClass.B, "star_b_class"),
            (StarClass.SpectralClass.A, "star_a_class"),
            (StarClass.SpectralClass.F, "star_f_class"),
            (StarClass.SpectralClass.G, "star_g_class"),
            (StarClass.SpectralClass.K, "star_k_class"),
            (StarClass.SpectralClass.M, "star_m_class"),
        };

        for (int i = 0; i < spectralClasses.Length; i++)
        {
            (StarClass.SpectralClass sc, string name) = spectralClasses[i];
            int seedVal = BaseSeed + i;
            StarSpec spec = new StarSpec(seedVal, (int)sc, -1, -1.0, -1.0, "", null);
            SeededRng rng = new SeededRng(seedVal);
            CelestialBody body = StarGenerator.Generate(spec, rng);
            fixtures.Add(new Dictionary
            {
                ["name"] = name,
                ["type"] = "star",
                ["seed"] = seedVal,
                ["spec"] = spec.ToDictionary(),
                ["context"] = default(Variant),
                ["body"] = CelestialSerializer.ToDictionary(body),
            });
        }

        return fixtures;
    }

    /// <summary>
    /// Generates planet fixtures covering all major size categories and orbit zones.
    /// Includes dwarf, sub-terrestrial, terrestrial, super-Earth, mini-Neptune, Neptune-class, and gas giant.
    /// </summary>
    /// <returns>Array of dictionaries, each containing name, type, seed, spec, context, and body.</returns>
    public static Godot.Collections.Array<Dictionary> GeneratePlanetFixtures()
    {
        Godot.Collections.Array<Dictionary> fixtures = new();
        (SizeCategory.Category Size, OrbitZone.Zone Zone, string Name)[] configs = new (SizeCategory.Category Size, OrbitZone.Zone Zone, string Name)[]
        {
            (SizeCategory.Category.Dwarf, OrbitZone.Zone.Cold, "planet_dwarf_cold"),
            (SizeCategory.Category.SubTerrestrial, OrbitZone.Zone.Temperate, "planet_subterrestrial_temperate"),
            (SizeCategory.Category.Terrestrial, OrbitZone.Zone.Temperate, "planet_terrestrial_temperate"),
            (SizeCategory.Category.SuperEarth, OrbitZone.Zone.Hot, "planet_superearth_hot"),
            (SizeCategory.Category.MiniNeptune, OrbitZone.Zone.Temperate, "planet_minineptune_temperate"),
            (SizeCategory.Category.NeptuneClass, OrbitZone.Zone.Cold, "planet_neptuneclass_cold"),
            (SizeCategory.Category.GasGiant, OrbitZone.Zone.Cold, "planet_gasgiant_cold"),
        };

        ParentContext context = ParentContext.SunLike();

        for (int i = 0; i < configs.Length; i++)
        {
            (SizeCategory.Category size, OrbitZone.Zone zone, string name) = configs[i];
            int seedVal = BaseSeed + 100 + i;
            PlanetSpec spec = new PlanetSpec(seedVal, (int)size, (int)zone, default, default, -1, "", null);
            SeededRng rng = new SeededRng(seedVal);
            CelestialBody body = PlanetGenerator.Generate(spec, context, rng);
            fixtures.Add(new Dictionary
            {
                ["name"] = name,
                ["type"] = "planet",
                ["seed"] = seedVal,
                ["spec"] = spec.ToDictionary(),
                ["context"] = context.ToDictionary(),
                ["body"] = CelestialSerializer.ToDictionary(body),
            });
        }

        return fixtures;
    }

    /// <summary>
    /// Generates moon fixtures covering the major archetypes: Luna-like, Europa-like, Titan-like,
    /// captured, dwarf regular, sub-terrestrial regular, and terrestrial regular.
    /// All moons are generated around a Jupiter-analogue orbiting a Sun-like star at 5.2 AU.
    /// </summary>
    /// <returns>Array of dictionaries, each containing name, type, seed, spec, context, and body.</returns>
    public static Godot.Collections.Array<Dictionary> GenerateMoonFixtures()
    {
        Godot.Collections.Array<Dictionary> fixtures = new();
        double jupiterMassKg = 1.898e27;
        double jupiterRadiusM = 6.9911e7;
        ParentContext context = ParentContext.ForMoon(
            Units.SolarMassKg,
            StellarProps.SolarLuminosityWatts,
            5778.0,
            4.6e9,
            5.2 * Units.AuMeters,
            jupiterMassKg,
            jupiterRadiusM,
            5.0e8);

        (string Type, string Name)[] configs = new (string Type, string Name)[]
        {
            ("luna_like", "moon_luna_like"),
            ("europa_like", "moon_europa_like"),
            ("titan_like", "moon_titan_like"),
            ("captured", "moon_captured"),
            ("dwarf_regular", "moon_dwarf_regular"),
            ("subterrestrial_regular", "moon_subterrestrial_regular"),
            ("terrestrial_regular", "moon_terrestrial_regular"),
        };

        for (int i = 0; i < configs.Length; i++)
        {
            (string type, string name) = configs[i];
            int seedVal = BaseSeed + 200 + i;
            MoonSpec spec;
            switch (type)
            {
                case "luna_like":
                    spec = MoonSpec.LunaLike(seedVal);
                    break;
                case "europa_like":
                    spec = MoonSpec.EuropaLike(seedVal);
                    break;
                case "titan_like":
                    spec = MoonSpec.TitanLike(seedVal);
                    context.OrbitalDistanceFromParentM = 1.2e9;
                    break;
                case "captured":
                    spec = MoonSpec.Captured(seedVal);
                    context.OrbitalDistanceFromParentM = 2.0e10;
                    break;
                case "dwarf_regular":
                    spec = new MoonSpec(seedVal, (int)SizeCategory.Category.Dwarf, false);
                    context.OrbitalDistanceFromParentM = 3.0e8;
                    break;
                case "subterrestrial_regular":
                    spec = new MoonSpec(seedVal, (int)SizeCategory.Category.SubTerrestrial, false);
                    context.OrbitalDistanceFromParentM = 4.0e8;
                    break;
                case "terrestrial_regular":
                    spec = new MoonSpec(seedVal, (int)SizeCategory.Category.Terrestrial, false);
                    context.OrbitalDistanceFromParentM = 6.0e8;
                    break;
                default:
                    spec = MoonSpec.Random(seedVal);
                    break;
            }

            SeededRng rng = new SeededRng(seedVal);
            CelestialBody? body = MoonGenerator.Generate(spec, context, rng);
            if (body == null)
                continue;
            fixtures.Add(new Dictionary
            {
                ["name"] = name,
                ["type"] = "moon",
                ["seed"] = seedVal,
                ["spec"] = spec.ToDictionary(),
                ["context"] = context.ToDictionary(),
                ["body"] = CelestialSerializer.ToDictionary(body),
            });
        }

        return fixtures;
    }

    /// <summary>
    /// Generates asteroid fixtures covering C-type, S-type, M-type, Ceres-like, and three random variants.
    /// All asteroids are placed in the main belt region (2.7 AU from a Sun-like star).
    /// </summary>
    /// <returns>Array of dictionaries, each containing name, type, seed, spec, context, and body.</returns>
    public static Godot.Collections.Array<Dictionary> GenerateAsteroidFixtures()
    {
        Godot.Collections.Array<Dictionary> fixtures = new();
        ParentContext context = ParentContext.SunLike(2.7 * Units.AuMeters);

        (string Type, bool Large, string Name)[] configs = new (string Type, bool Large, string Name)[]
        {
            ("c_type", false, "asteroid_c_type"),
            ("s_type", false, "asteroid_s_type"),
            ("m_type", false, "asteroid_m_type"),
            ("ceres_like", true, "asteroid_ceres_like"),
            ("random_1", false, "asteroid_random_1"),
            ("random_2", false, "asteroid_random_2"),
            ("random_3", false, "asteroid_random_3"),
        };

        for (int i = 0; i < configs.Length; i++)
        {
            (string type, bool large, string name) = configs[i];
            int seedVal = BaseSeed + 300 + i;
            AsteroidSpec spec;
            switch (type)
            {
                case "c_type":
                    spec = AsteroidSpec.Carbonaceous(seedVal);
                    break;
                case "s_type":
                    spec = AsteroidSpec.Stony(seedVal);
                    break;
                case "m_type":
                    spec = AsteroidSpec.Metallic(seedVal);
                    break;
                case "ceres_like":
                    spec = AsteroidSpec.CeresLike(seedVal);
                    break;
                default:
                    spec = AsteroidSpec.Random(seedVal);
                    break;
            }

            SeededRng rng = new SeededRng(seedVal);
            CelestialBody body = AsteroidGenerator.Generate(spec, context, rng);
            fixtures.Add(new Dictionary
            {
                ["name"] = name,
                ["type"] = "asteroid",
                ["seed"] = seedVal,
                ["spec"] = spec.ToDictionary(),
                ["context"] = context.ToDictionary(),
                ["body"] = CelestialSerializer.ToDictionary(body),
            });
        }

        return fixtures;
    }

    /// <summary>
    /// Exports all fixtures to JSON strings.
    /// </summary>
    /// <param name="pretty">Whether to format with indentation.</param>
    /// <returns>Dictionary mapping fixture name to JSON string.</returns>
    public static Dictionary ExportAllToJson(bool pretty = true)
    {
        Godot.Collections.Array<Dictionary> fixtures = GenerateAllFixtures();
        Dictionary result = new Dictionary();
        foreach (Dictionary fixture in fixtures)
        {
            string fixtureName = fixture["name"].AsString();
            string jsonStr;
            if (pretty)
            {
                jsonStr = Json.Stringify(fixture, "\t");
            }
            else
            {
                jsonStr = Json.Stringify(fixture);
            }
            result[fixtureName] = jsonStr;
        }
        return result;
    }
}
