using Godot;

namespace StarGen.App.Shared;

/// <summary>
/// Resolves the user-facing version label shown in the application UI.
/// </summary>
public static class UserFacingVersionHelper
{
    private const string PublicVersionSettingPath = "application/config/public_version_label";
    private const string InternalVersionSettingPath = "application/config/version";
    private const string DefaultVersion = "0.8.0.0";

    /// <summary>
    /// Returns the version string intended for user-facing UI.
    /// </summary>
    public static string GetDisplayVersion()
    {
        Variant publicVersionValue = ProjectSettings.GetSetting(PublicVersionSettingPath, Variant.From(""));
        if (publicVersionValue.VariantType == Variant.Type.String)
        {
            string publicVersion = publicVersionValue.AsString();
            if (!string.IsNullOrWhiteSpace(publicVersion))
            {
                return publicVersion;
            }
        }

        Variant internalVersionValue = ProjectSettings.GetSetting(InternalVersionSettingPath, Variant.From(DefaultVersion));
        if (internalVersionValue.VariantType == Variant.Type.String)
        {
            string internalVersion = internalVersionValue.AsString();
            if (!string.IsNullOrWhiteSpace(internalVersion))
            {
                return internalVersion;
            }
        }

        return DefaultVersion;
    }
}
