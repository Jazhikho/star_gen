using Godot;

namespace StarGen.App.Shared;

/// <summary>
/// Resolves the user-facing version label shown in the application UI.
/// </summary>
public static class UserFacingVersionHelper
{
    private const string InternalVersionSettingPath = "application/config/version";
    private const string UserFacingVersionSettingPath = "application/config/user_facing_version";
    private const string ReleaseChannelSettingPath = "application/config/release_channel";
    private const string DefaultInternalVersion = "0.8.18.2";
    private const string DefaultUserFacingVersion = "0.9";

    /// <summary>
    /// Returns the version string intended for user-facing UI.
    /// </summary>
    public static string GetDisplayVersion()
    {
        string version = DefaultUserFacingVersion;
        Variant userFacingVersionValue = ProjectSettings.GetSetting(UserFacingVersionSettingPath, Variant.From(DefaultUserFacingVersion));
        if (userFacingVersionValue.VariantType == Variant.Type.String)
        {
            string configuredUserFacingVersion = userFacingVersionValue.AsString();
            if (!string.IsNullOrWhiteSpace(configuredUserFacingVersion))
            {
                version = configuredUserFacingVersion;
            }
        }
        else
        {
            Variant internalVersionValue = ProjectSettings.GetSetting(InternalVersionSettingPath, Variant.From(DefaultInternalVersion));
            if (internalVersionValue.VariantType == Variant.Type.String)
            {
                string internalVersion = internalVersionValue.AsString();
                if (!string.IsNullOrWhiteSpace(internalVersion))
                {
                    version = internalVersion;
                }
            }
        }

        string suffix = "d";
        Variant releaseChannelValue = ProjectSettings.GetSetting(ReleaseChannelSettingPath, Variant.From("mainline"));
        if (releaseChannelValue.VariantType == Variant.Type.String)
        {
            string releaseChannel = releaseChannelValue.AsString().Trim().ToLowerInvariant();
            if (releaseChannel == "export")
            {
                suffix = "e";
            }
        }

        return version + suffix;
    }
}
