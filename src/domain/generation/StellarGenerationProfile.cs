using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Generation;

/// <summary>
/// Supported initial-mass-function families for stellar generation.
/// </summary>
public enum StellarImfForm
{
    Kroupa = 0,
    Chabrier = 1,
}

/// <summary>
/// Supported IMF variation modes.
/// </summary>
public enum StellarImfVariationMode
{
    Canonical = 0,
    MetallicityAgeModulated = 1,
}

/// <summary>
/// Supported stellar-evolution lookup families.
/// </summary>
public enum StellarIsochroneModel
{
    Mist = 0,
    Parsec = 1,
}

/// <summary>
/// Shared stellar-generation priors used by galaxy and system generation.
/// </summary>
public partial class StellarGenerationProfile : RefCounted
{
    /// <summary>
    /// Initial-mass-function family used for mass sampling.
    /// </summary>
    public StellarImfForm ImfForm { get; set; } = StellarImfForm.Kroupa;

    /// <summary>
    /// Controls whether the IMF stays canonical or shifts with metallicity and age context.
    /// </summary>
    public StellarImfVariationMode ImfVariationMode { get; set; } = StellarImfVariationMode.Canonical;

    /// <summary>
    /// Isochrone family used to resolve luminosity, radius, and temperature.
    /// </summary>
    public StellarIsochroneModel IsochroneModel { get; set; } = StellarIsochroneModel.Mist;

    /// <summary>
    /// Scales how often systems produce companions and higher-order multiplicity.
    /// </summary>
    public double MultiplicityScale { get; set; } = 1.0;

    /// <summary>
    /// Creates the default stellar-generation profile.
    /// </summary>
    public static StellarGenerationProfile CreateDefault()
    {
        return new StellarGenerationProfile();
    }

    /// <summary>
    /// Creates a detached copy of the profile.
    /// </summary>
    public StellarGenerationProfile Clone()
    {
        return new StellarGenerationProfile
        {
            ImfForm = ImfForm,
            ImfVariationMode = ImfVariationMode,
            IsochroneModel = IsochroneModel,
            MultiplicityScale = MultiplicityScale,
        };
    }

    /// <summary>
    /// Returns whether the profile values are inside the supported ranges.
    /// </summary>
    public bool IsValid()
    {
        if (!System.Enum.IsDefined(typeof(StellarImfForm), (int)ImfForm))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(StellarImfVariationMode), (int)ImfVariationMode))
        {
            return false;
        }

        if (!System.Enum.IsDefined(typeof(StellarIsochroneModel), (int)IsochroneModel))
        {
            return false;
        }

        if (MultiplicityScale < 0.35 || MultiplicityScale > 2.0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Converts the profile to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["imf_form"] = (int)ImfForm,
            ["imf_variation_mode"] = (int)ImfVariationMode,
            ["isochrone_model"] = (int)IsochroneModel,
            ["multiplicity_scale"] = MultiplicityScale,
        };
    }

    /// <summary>
    /// Rebuilds a profile from a dictionary payload.
    /// </summary>
    public static StellarGenerationProfile FromDictionary(Dictionary data)
    {
        StellarGenerationProfile profile = new StellarGenerationProfile();

        int imfFormValue = DomainDictionaryUtils.GetInt(data, "imf_form", (int)StellarImfForm.Kroupa);
        if (System.Enum.IsDefined(typeof(StellarImfForm), imfFormValue))
        {
            profile.ImfForm = (StellarImfForm)imfFormValue;
        }

        int imfVariationValue = DomainDictionaryUtils.GetInt(data, "imf_variation_mode", (int)StellarImfVariationMode.Canonical);
        if (System.Enum.IsDefined(typeof(StellarImfVariationMode), imfVariationValue))
        {
            profile.ImfVariationMode = (StellarImfVariationMode)imfVariationValue;
        }

        int isochroneValue = DomainDictionaryUtils.GetInt(data, "isochrone_model", (int)StellarIsochroneModel.Mist);
        if (System.Enum.IsDefined(typeof(StellarIsochroneModel), isochroneValue))
        {
            profile.IsochroneModel = (StellarIsochroneModel)isochroneValue;
        }

        profile.MultiplicityScale = DomainDictionaryUtils.GetDouble(data, "multiplicity_scale", 1.0);
        return profile;
    }
}
