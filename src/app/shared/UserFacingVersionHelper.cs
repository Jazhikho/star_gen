using Godot;

namespace StarGen.App.Shared;

/// <summary>
/// Resolves the user-facing version label shown in the application UI.
/// </summary>
public static class UserFacingVersionHelper
{
    private const string InternalVersionSettingPath = "application/config/version";
    private const string UserFacingVersionSettingPath = "application/config/user_facing_version";
    private const string DefaultInternalVersion = "0.10.12.0";
    private const string DefaultUserFacingVersion = "0.11";

    /// <summary>
    /// Returns the version string intended for user-facing UI.
    /// </summary>
    public static string GetDisplayVersion()
    {
        string baseVersion = DefaultInternalVersion;
        Variant userFacingVersionValue = ProjectSettings.GetSetting(UserFacingVersionSettingPath, Variant.From(DefaultUserFacingVersion));
        if (userFacingVersionValue.VariantType == Variant.Type.String)
        {
            string configuredUserFacingVersion = userFacingVersionValue.AsString();
            if (!string.IsNullOrWhiteSpace(configuredUserFacingVersion))
            {
                return ReleaseEditionService.FormatDisplayVersion(
                    configuredUserFacingVersion,
                    ReleaseEditionService.GetCurrentEdition());
            }
        }

        Variant internalVersionValue = ProjectSettings.GetSetting(InternalVersionSettingPath, Variant.From(DefaultInternalVersion));
        if (internalVersionValue.VariantType == Variant.Type.String)
        {
            string internalVersion = internalVersionValue.AsString();
            if (!string.IsNullOrWhiteSpace(internalVersion))
            {
                baseVersion = internalVersion;
            }
        }

        return ReleaseEditionService.FormatDisplayVersion(
            baseVersion,
            ReleaseEditionService.GetCurrentEdition());
    }
}
