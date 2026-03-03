using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.App.Rendering;

/// <summary>
/// Derives ring-system shader uniforms from ring-band data.
/// </summary>
public static class RingShaderParams
{
    /// <summary>
    /// Returns shader parameters for the spatial ring-system shader.
    /// </summary>
    public static Dictionary GetParams(RingSystemProps? ringSystem, CelestialBody? body = null)
    {
        Dictionary parameters = new();
        if (ringSystem == null || ringSystem.GetBandCount() == 0)
        {
            parameters["u_bandCount"] = 0;
            parameters["u_innerRadius"] = 1.4f;
            parameters["u_outerRadius"] = 2.6f;
            parameters["u_density"] = 0.5f;
            parameters["u_gapSize"] = 0.15f;
            parameters["u_colorInner"] = new Color(0.8f, 0.6f, 0.4f);
            parameters["u_colorMid"] = new Color(0.73f, 0.67f, 0.53f);
            parameters["u_colorOuter"] = new Color(0.53f, 0.6f, 0.67f);
            parameters["u_lightDir"] = new Vector3(0.7f, 0.4f, 0.6f);
            parameters["u_ambient"] = 0.15f;
            parameters["u_seed"] = 0.0f;
            return parameters;
        }

        float seedValue = body?.Provenance == null ? 0.0f : (body.Provenance.GenerationSeed % 1000L) / 10.0f;
        parameters["u_seed"] = seedValue;
        parameters["u_bandCount"] = ringSystem.GetBandCount();

        float innerRadius = float.PositiveInfinity;
        float outerRadius = 0.0f;
        float bodyRadius = 1.0f;
        if (body?.Physical != null && body.Physical.RadiusM > 0.0)
        {
            bodyRadius = (float)body.Physical.RadiusM;
        }

        for (int index = 0; index < ringSystem.GetBandCount(); index += 1)
        {
            RingBand? band = ringSystem.GetBand(index);
            if (band == null)
            {
                continue;
            }

            float bandInner = (float)(band.InnerRadiusM / bodyRadius);
            float bandOuter = (float)(band.OuterRadiusM / bodyRadius);
            innerRadius = Mathf.Min(innerRadius, bandInner);
            outerRadius = Mathf.Max(outerRadius, bandOuter);
        }

        if (float.IsPositiveInfinity(innerRadius))
        {
            innerRadius = 1.4f;
        }

        if (outerRadius < innerRadius)
        {
            outerRadius = innerRadius + 0.5f;
        }

        parameters["u_innerRadius"] = Mathf.Clamp(innerRadius, 1.1f, 2.0f);
        parameters["u_outerRadius"] = Mathf.Clamp(outerRadius, (float)parameters["u_innerRadius"] + 0.2f, 4.0f);
        parameters["u_density"] = CalculateAverageDensity(ringSystem);
        parameters["u_gapSize"] = CalculateGapSize(ringSystem);

        Dictionary colors = CalculateRingColors(ringSystem);
        parameters["u_colorInner"] = colors["inner"];
        parameters["u_colorMid"] = colors["mid"];
        parameters["u_colorOuter"] = colors["outer"];

        parameters["u_lightDir"] = new Vector3(0.7f, 0.4f, 0.6f);
        parameters["u_ambient"] = 0.15f;
        return parameters;
    }

    /// <summary>
    /// Returns shader parameters for a single rendered ring band.
    /// </summary>
    public static Dictionary GetBandParams(RingBand band, CelestialBody? body = null)
    {
        Dictionary parameters = new();
        float seedValue = body?.Provenance == null ? 0.0f : (body.Provenance.GenerationSeed % 1000L) / 10.0f;
        parameters["u_seed"] = seedValue;

        float bodyRadius = 1.0f;
        if (body?.Physical != null && body.Physical.RadiusM > 0.0)
        {
            bodyRadius = (float)body.Physical.RadiusM;
        }

        parameters["u_bandCount"] = 1;
        parameters["u_innerRadius"] = Mathf.Clamp((float)(band.InnerRadiusM / bodyRadius), 1.1f, 3.0f);
        parameters["u_outerRadius"] = Mathf.Clamp((float)(band.OuterRadiusM / bodyRadius), 1.2f, 4.0f);
        parameters["u_density"] = Mathf.Clamp((float)band.OpticalDepth, 0.1f, 1.0f);
        parameters["u_gapSize"] = 0.05f;

        Color ringColor = ColorUtils.RingToColor(band.Composition, (float)band.OpticalDepth);
        parameters["u_colorInner"] = ringColor;
        parameters["u_colorMid"] = ringColor;
        parameters["u_colorOuter"] = ringColor;
        parameters["u_lightDir"] = new Vector3(0.7f, 0.4f, 0.6f);
        parameters["u_ambient"] = 0.15f;
        return parameters;
    }

    /// <summary>
    /// Returns parameters for the legacy inline ring shader path.
    /// </summary>
    public static Dictionary GetRingShaderParams(RingSystemProps? ringSystem, float planetRadiusM)
    {
        Dictionary parameters = new();
        if (ringSystem == null || ringSystem.GetBandCount() == 0)
        {
            parameters["u_ringType"] = 0;
            return parameters;
        }

        int bandCount = ringSystem.GetBandCount();
        parameters["u_ringType"] = bandCount switch
        {
            <= 1 => 1,
            <= 3 => 2,
            _ => 3,
        };

        float innerRadius = float.PositiveInfinity;
        float outerRadius = 0.0f;
        for (int index = 0; index < bandCount; index += 1)
        {
            RingBand? band = ringSystem.GetBand(index);
            if (band == null)
            {
                continue;
            }

            innerRadius = Mathf.Min(innerRadius, (float)band.InnerRadiusM);
            outerRadius = Mathf.Max(outerRadius, (float)band.OuterRadiusM);
        }

        parameters["u_ringInner"] = innerRadius / planetRadiusM;
        parameters["u_ringOuter"] = outerRadius / planetRadiusM;
        parameters["u_ringBands"] = bandCount;

        Dictionary averageComposition = new();
        for (int index = 0; index < bandCount; index += 1)
        {
            RingBand? band = ringSystem.GetBand(index);
            if (band == null)
            {
                continue;
            }

            foreach (Variant keyVariant in band.Composition.Keys)
            {
                string key = (string)keyVariant;
                float existing = GetFloat(averageComposition, key, 0.0f);
                averageComposition[key] = existing + (GetFloat(band.Composition, key, 0.0f) / bandCount);
            }
        }

        Color ringColor = ColorUtils.RingToColor(averageComposition, 0.5f);
        parameters["u_ringColor1"] = new Vector3(ringColor.R * 1.1f, ringColor.G * 1.1f, ringColor.B);
        parameters["u_ringColor2"] = new Vector3(ringColor.R, ringColor.G, ringColor.B);
        parameters["u_ringColor3"] = new Vector3(ringColor.R * 0.9f, ringColor.G * 0.95f, ringColor.B * 1.1f);

        float averageOpticalDepth = 0.0f;
        for (int index = 0; index < bandCount; index += 1)
        {
            RingBand? band = ringSystem.GetBand(index);
            if (band != null)
            {
                averageOpticalDepth += (float)band.OpticalDepth / bandCount;
            }
        }

        parameters["u_ringDensity"] = Mathf.Clamp(averageOpticalDepth * 0.8f, 0.15f, 0.95f);

        float totalRingWidth = outerRadius - innerRadius;
        float bandWidthSum = 0.0f;
        for (int index = 0; index < bandCount; index += 1)
        {
            RingBand? band = ringSystem.GetBand(index);
            if (band != null)
            {
                bandWidthSum += (float)(band.OuterRadiusM - band.InnerRadiusM);
            }
        }

        float gapFraction = 1.0f - (bandWidthSum / Mathf.Max(totalRingWidth, 1.0f));
        parameters["u_ringGap"] = Mathf.Clamp(gapFraction, 0.05f, 0.4f);

        float inclinationRad = Mathf.DegToRad((float)ringSystem.InclinationDeg);
        parameters["u_ringNormal"] = new Vector3(Mathf.Sin(inclinationRad), Mathf.Cos(inclinationRad), 0.12f);
        return parameters;
    }

    /// <summary>
    /// Calculates weighted average optical density across all bands.
    /// </summary>
    private static float CalculateAverageDensity(RingSystemProps ringSystem)
    {
        if (ringSystem.GetBandCount() == 0)
        {
            return 0.5f;
        }

        float totalDepth = 0.0f;
        float totalWidth = 0.0f;
        for (int index = 0; index < ringSystem.GetBandCount(); index += 1)
        {
            RingBand? band = ringSystem.GetBand(index);
            if (band == null)
            {
                continue;
            }

            float width = (float)(band.OuterRadiusM - band.InnerRadiusM);
            totalDepth += (float)band.OpticalDepth * width;
            totalWidth += width;
        }

        if (totalWidth <= 0.0f)
        {
            return 0.5f;
        }

        return Mathf.Clamp(totalDepth / totalWidth, 0.1f, 1.0f);
    }

    /// <summary>
    /// Calculates total gap fraction across the ring system.
    /// </summary>
    private static float CalculateGapSize(RingSystemProps ringSystem)
    {
        if (ringSystem.GetBandCount() < 2)
        {
            return 0.1f;
        }

        float totalSpan = (float)(ringSystem.GetOuterRadiusM() - ringSystem.GetInnerRadiusM());
        float bandCoverage = 0.0f;
        for (int index = 0; index < ringSystem.GetBandCount(); index += 1)
        {
            RingBand? band = ringSystem.GetBand(index);
            if (band != null)
            {
                bandCoverage += (float)(band.OuterRadiusM - band.InnerRadiusM);
            }
        }

        if (totalSpan <= 0.0f)
        {
            return 0.15f;
        }

        return Mathf.Clamp(1.0f - (bandCoverage / totalSpan), 0.05f, 0.5f);
    }

    /// <summary>
    /// Calculates representative inner/mid/outer ring colors.
    /// </summary>
    private static Dictionary CalculateRingColors(RingSystemProps ringSystem)
    {
        Color inner = new(0.8f, 0.6f, 0.4f);
        Color mid = new(0.73f, 0.67f, 0.53f);
        Color outer = new(0.53f, 0.6f, 0.67f);
        if (ringSystem.GetBandCount() == 0)
        {
            return new Dictionary { ["inner"] = inner, ["mid"] = mid, ["outer"] = outer };
        }

        float iceContent = 0.0f;
        float rockContent = 0.0f;
        float ironContent = 0.0f;
        float carbonContent = 0.0f;
        float bandCount = ringSystem.GetBandCount();

        for (int index = 0; index < ringSystem.GetBandCount(); index += 1)
        {
            RingBand? band = ringSystem.GetBand(index);
            if (band == null)
            {
                continue;
            }

            iceContent += GetFloat(band.Composition, "water_ice", 0.0f) + GetFloat(band.Composition, "ice", 0.0f);
            rockContent += GetFloat(band.Composition, "silicates", 0.0f) + GetFloat(band.Composition, "rock", 0.0f);
            ironContent += GetFloat(band.Composition, "iron", 0.0f) + GetFloat(band.Composition, "iron_oxides", 0.0f);
            carbonContent += GetFloat(band.Composition, "carbon", 0.0f) + GetFloat(band.Composition, "carbon_compounds", 0.0f);
        }

        iceContent /= bandCount;
        rockContent /= bandCount;
        ironContent /= bandCount;
        carbonContent /= bandCount;

        if (iceContent > 0.5f)
        {
            inner = new Color(0.85f, 0.8f, 0.75f);
            mid = new Color(0.9f, 0.88f, 0.85f);
            outer = new Color(0.8f, 0.85f, 0.9f);
        }
        else if (rockContent > 0.5f)
        {
            inner = new Color(0.5f, 0.45f, 0.4f);
            mid = new Color(0.55f, 0.5f, 0.45f);
            outer = new Color(0.45f, 0.45f, 0.5f);
        }
        else if (ironContent > 0.2f)
        {
            inner = new Color(0.7f, 0.5f, 0.4f);
            mid = new Color(0.65f, 0.55f, 0.5f);
            outer = new Color(0.55f, 0.5f, 0.55f);
        }
        else if (carbonContent > 0.3f)
        {
            inner = new Color(0.25f, 0.23f, 0.2f);
            mid = new Color(0.3f, 0.28f, 0.25f);
            outer = new Color(0.28f, 0.28f, 0.3f);
        }

        return new Dictionary { ["inner"] = inner, ["mid"] = mid, ["outer"] = outer };
    }

    /// <summary>
    /// Reads a float payload from a dictionary.
    /// </summary>
    private static float GetFloat(Dictionary data, string key, float fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (float)(double)value,
            Variant.Type.Int => (int)value,
            _ => fallback,
        };
    }
}
