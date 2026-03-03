using Godot;

namespace StarGen.App.Rendering;

internal enum GasGiantArchetype
{
    HotJupiter,
    JupiterClass,
    SaturnClass,
    NeptuneClass,
    UranusClass,
    SuperJupiter,
    MiniNeptune,
}

internal sealed class GasGiantPalette
{
    public readonly Color ZoneEq;
    public readonly Color ZoneMid;
    public readonly Color BeltEq;
    public readonly Color BeltMid;
    public readonly Color BeltPolar;
    public readonly Color Storm;
    public readonly Color Polar;
    public readonly Color Haze;
    public readonly Color Atmosphere;

    public GasGiantPalette(
        Color zoneEq,
        Color zoneMid,
        Color beltEq,
        Color beltMid,
        Color beltPolar,
        Color storm,
        Color polar,
        Color haze,
        Color atmosphere)
    {
        ZoneEq = zoneEq;
        ZoneMid = zoneMid;
        BeltEq = beltEq;
        BeltMid = beltMid;
        BeltPolar = beltPolar;
        Storm = storm;
        Polar = polar;
        Haze = haze;
        Atmosphere = atmosphere;
    }
}

internal sealed class GasGiantArchetypePreset
{
    public readonly float BandCount;
    public readonly float BandContrast;
    public readonly float BandTurbulence;
    public readonly float BandSharpness;
    public readonly float BandWarp;
    public readonly float Chevron;
    public readonly float FlowSpeed;
    public readonly int FlowDetail;
    public readonly float JetStrength;
    public readonly float StormIntensity;
    public readonly float StormScale;
    public readonly float Vortex;
    public readonly int StormCountLo;
    public readonly int StormCountHi;
    public readonly float DarkSpotRatio;
    public readonly float DetailLevel;
    public readonly float HazeDensity;
    public readonly float Streak;
    public readonly float AtmoDensity;
    public readonly float AtmoFalloff;
    public readonly float Scatter;
    public readonly float LimbDark;
    public readonly float TerminatorSharp;
    public readonly float Ambient;
    public readonly GasGiantPalette Palette;

    public GasGiantArchetypePreset(
        float bandCount,
        float bandContrast,
        float bandTurbulence,
        float bandSharpness,
        float bandWarp,
        float chevron,
        float flowSpeed,
        int flowDetail,
        float jetStrength,
        float stormIntensity,
        float stormScale,
        float vortex,
        int stormCountLo,
        int stormCountHi,
        float darkSpotRatio,
        float detailLevel,
        float hazeDensity,
        float streak,
        float atmoDensity,
        float atmoFalloff,
        float scatter,
        float limbDark,
        float terminatorSharp,
        float ambient,
        GasGiantPalette palette)
    {
        BandCount = bandCount;
        BandContrast = bandContrast;
        BandTurbulence = bandTurbulence;
        BandSharpness = bandSharpness;
        BandWarp = bandWarp;
        Chevron = chevron;
        FlowSpeed = flowSpeed;
        FlowDetail = flowDetail;
        JetStrength = jetStrength;
        StormIntensity = stormIntensity;
        StormScale = stormScale;
        Vortex = vortex;
        StormCountLo = stormCountLo;
        StormCountHi = stormCountHi;
        DarkSpotRatio = darkSpotRatio;
        DetailLevel = detailLevel;
        HazeDensity = hazeDensity;
        Streak = streak;
        AtmoDensity = atmoDensity;
        AtmoFalloff = atmoFalloff;
        Scatter = scatter;
        LimbDark = limbDark;
        TerminatorSharp = terminatorSharp;
        Ambient = ambient;
        Palette = palette;
    }
}

internal static class GasGiantShaderParamProfiles
{
    public static GasGiantArchetypePreset GetPreset(GasGiantArchetype archetype)
    {
        return archetype switch
        {
            GasGiantArchetype.HotJupiter => new GasGiantArchetypePreset(
                10.0f, 0.70f, 1.20f, 0.35f, 0.35f, 0.15f,
                0.60f, 6, 1.3f,
                0.80f, 2.50f, 1.20f, 3, 6, 0.15f,
                0.75f, 0.10f, 0.35f,
                1.50f, 1.50f, 0.50f,
                0.70f, 0.08f, 0.08f,
                new GasGiantPalette(
                    new Color(0.85f, 0.50f, 0.20f),
                    new Color(0.75f, 0.40f, 0.18f),
                    new Color(0.55f, 0.18f, 0.08f),
                    new Color(0.40f, 0.12f, 0.05f),
                    new Color(0.30f, 0.15f, 0.12f),
                    new Color(0.95f, 0.70f, 0.20f),
                    new Color(0.45f, 0.25f, 0.18f),
                    new Color(0.60f, 0.35f, 0.18f),
                    new Color(0.80f, 0.40f, 0.15f))),
            GasGiantArchetype.JupiterClass => new GasGiantArchetypePreset(
                14.0f, 0.50f, 0.60f, 0.55f, 0.30f, 0.30f,
                0.30f, 5, 1.2f,
                0.60f, 2.00f, 0.80f, 2, 6, 0.20f,
                0.85f, 0.05f, 0.35f,
                1.20f, 2.00f, 0.70f,
                0.90f, 0.15f, 0.04f,
                new GasGiantPalette(
                    new Color(0.91f, 0.84f, 0.63f),
                    new Color(0.85f, 0.78f, 0.58f),
                    new Color(0.55f, 0.42f, 0.23f),
                    new Color(0.50f, 0.38f, 0.22f),
                    new Color(0.38f, 0.38f, 0.42f),
                    new Color(0.80f, 0.40f, 0.27f),
                    new Color(0.45f, 0.50f, 0.62f),
                    new Color(0.60f, 0.55f, 0.50f),
                    new Color(0.67f, 0.63f, 0.50f))),
            GasGiantArchetype.SaturnClass => new GasGiantArchetypePreset(
                20.0f, 0.30f, 0.40f, 0.25f, 0.45f, 0.10f,
                0.25f, 5, 0.8f,
                0.20f, 1.50f, 0.40f, 0, 3, 0.20f,
                0.60f, 0.18f, 0.25f,
                0.90f, 2.20f, 0.60f,
                0.85f, 0.12f, 0.04f,
                new GasGiantPalette(
                    new Color(0.94f, 0.87f, 0.63f),
                    new Color(0.88f, 0.82f, 0.58f),
                    new Color(0.76f, 0.65f, 0.38f),
                    new Color(0.68f, 0.58f, 0.35f),
                    new Color(0.50f, 0.50f, 0.45f),
                    new Color(0.90f, 0.82f, 0.50f),
                    new Color(0.55f, 0.55f, 0.52f),
                    new Color(0.72f, 0.68f, 0.50f),
                    new Color(0.80f, 0.75f, 0.52f))),
            GasGiantArchetype.NeptuneClass => new GasGiantArchetypePreset(
                8.0f, 0.40f, 0.80f, 0.30f, 0.40f, 0.15f,
                0.35f, 6, 1.1f,
                0.50f, 2.50f, 1.00f, 1, 4, 0.65f,
                0.50f, 0.30f, 0.35f,
                1.00f, 2.50f, 0.90f,
                0.95f, 0.15f, 0.03f,
                new GasGiantPalette(
                    new Color(0.27f, 0.47f, 0.80f),
                    new Color(0.25f, 0.42f, 0.72f),
                    new Color(0.15f, 0.30f, 0.65f),
                    new Color(0.13f, 0.25f, 0.55f),
                    new Color(0.12f, 0.22f, 0.45f),
                    new Color(0.35f, 0.55f, 0.85f),
                    new Color(0.20f, 0.32f, 0.60f),
                    new Color(0.28f, 0.42f, 0.70f),
                    new Color(0.22f, 0.38f, 0.72f))),
            GasGiantArchetype.UranusClass => new GasGiantArchetypePreset(
                6.0f, 0.15f, 0.30f, 0.10f, 0.25f, 0.03f,
                0.15f, 4, 0.6f,
                0.15f, 1.50f, 0.30f, 0, 2, 0.50f,
                0.25f, 0.55f, 0.10f,
                0.80f, 2.50f, 0.85f,
                0.90f, 0.12f, 0.04f,
                new GasGiantPalette(
                    new Color(0.53f, 0.78f, 0.82f),
                    new Color(0.48f, 0.72f, 0.78f),
                    new Color(0.42f, 0.65f, 0.72f),
                    new Color(0.38f, 0.58f, 0.65f),
                    new Color(0.34f, 0.52f, 0.58f),
                    new Color(0.48f, 0.72f, 0.75f),
                    new Color(0.38f, 0.58f, 0.68f),
                    new Color(0.48f, 0.68f, 0.75f),
                    new Color(0.45f, 0.68f, 0.78f))),
            GasGiantArchetype.SuperJupiter => new GasGiantArchetypePreset(
                18.0f, 0.65f, 0.90f, 0.50f, 0.35f, 0.35f,
                0.45f, 6, 1.5f,
                0.70f, 3.00f, 1.50f, 3, 7, 0.25f,
                0.90f, 0.05f, 0.40f,
                1.40f, 1.80f, 0.60f,
                0.80f, 0.10f, 0.05f,
                new GasGiantPalette(
                    new Color(0.87f, 0.73f, 0.53f),
                    new Color(0.78f, 0.62f, 0.45f),
                    new Color(0.40f, 0.27f, 0.13f),
                    new Color(0.32f, 0.20f, 0.10f),
                    new Color(0.28f, 0.22f, 0.22f),
                    new Color(0.93f, 0.53f, 0.27f),
                    new Color(0.33f, 0.27f, 0.40f),
                    new Color(0.50f, 0.42f, 0.32f),
                    new Color(0.53f, 0.47f, 0.33f))),
            _ => new GasGiantArchetypePreset(
                5.0f, 0.12f, 0.25f, 0.05f, 0.20f, 0.02f,
                0.12f, 3, 0.5f,
                0.10f, 1.30f, 0.25f, 0, 1, 0.50f,
                0.15f, 0.65f, 0.05f,
                1.10f, 2.20f, 0.80f,
                0.85f, 0.12f, 0.04f,
                new GasGiantPalette(
                    new Color(0.55f, 0.60f, 0.68f),
                    new Color(0.50f, 0.55f, 0.65f),
                    new Color(0.42f, 0.48f, 0.58f),
                    new Color(0.38f, 0.42f, 0.52f),
                    new Color(0.35f, 0.38f, 0.48f),
                    new Color(0.60f, 0.55f, 0.50f),
                    new Color(0.40f, 0.45f, 0.55f),
                    new Color(0.50f, 0.55f, 0.62f),
                    new Color(0.45f, 0.52f, 0.62f))),
        };
    }

    public static Color ShiftHue(Color color, float delta)
    {
        float shiftedHue = color.H + delta;
        shiftedHue -= Mathf.Floor(shiftedHue);
        return Color.FromHsv(shiftedHue, color.S, color.V, color.A);
    }

    public static Color ClampColor(Color color)
    {
        return new Color(
            Mathf.Clamp(color.R, 0.0f, 1.0f),
            Mathf.Clamp(color.G, 0.0f, 1.0f),
            Mathf.Clamp(color.B, 0.0f, 1.0f),
            color.A);
    }

    public static Color FinalizeColor(Color color, float hueShift, RandomNumberGenerator rng, float jitter)
    {
        Color shifted = ShiftHue(color, hueShift);
        return ClampColor(new Color(
            shifted.R + rng.RandfRange(-jitter, jitter),
            shifted.G + rng.RandfRange(-jitter, jitter),
            shifted.B + rng.RandfRange(-jitter, jitter),
            shifted.A));
    }

    public static Vector3 ColorToVector(Color color)
    {
        return new Vector3(color.R, color.G, color.B);
    }
}
