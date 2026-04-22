using Godot;

namespace StarGen.App.Shared;

/// <summary>
/// Supported release editions for the shipped StarGen builds.
/// </summary>
public enum ReleaseEdition
{
    Demo = 0,
    Export = 1,
}

/// <summary>
/// Resolves the active StarGen release edition and its gated capabilities.
/// </summary>
public static class ReleaseEditionService
{
    private const string ReleaseChannelSettingPath = "application/config/release_channel";
    private const string DefaultReleaseChannel = "demo";
    private const string LegacyReleaseChannel = "release";
    private const string DemoReleaseChannel = "demo";
    private const string ExportReleaseChannel = "export";
    private const string DemoFeatureName = "stargen_demo";
    private const string ExportFeatureName = "stargen_export";

    /// <summary>
    /// Returns the currently resolved release edition for the running build.
    /// </summary>
    public static ReleaseEdition GetCurrentEdition()
    {
        bool hasDemoFeature = OS.HasFeature(DemoFeatureName);
        bool hasExportFeature = OS.HasFeature(ExportFeatureName);
        string configuredChannel = GetConfiguredReleaseChannel();
        return ResolveEdition(configuredChannel, hasDemoFeature, hasExportFeature);
    }

    /// <summary>
    /// Resolves an edition from build metadata and optional feature tags.
    /// </summary>
    public static ReleaseEdition ResolveEdition(
        string? releaseChannel,
        bool hasDemoFeature = false,
        bool hasExportFeature = false)
    {
        if (hasExportFeature)
        {
            return ReleaseEdition.Export;
        }

        if (hasDemoFeature)
        {
            return ReleaseEdition.Demo;
        }

        string normalizedChannel = NormalizeReleaseChannel(releaseChannel);
        if (normalizedChannel == ExportReleaseChannel)
        {
            return ReleaseEdition.Export;
        }

        return ReleaseEdition.Demo;
    }

    /// <summary>
    /// Returns the normalized release-channel token used by project settings.
    /// </summary>
    public static string NormalizeReleaseChannel(string? releaseChannel)
    {
        if (string.IsNullOrWhiteSpace(releaseChannel))
        {
            return DemoReleaseChannel;
        }

        string normalizedChannel = releaseChannel.Trim().ToLowerInvariant();
        if (normalizedChannel == ExportReleaseChannel)
        {
            return ExportReleaseChannel;
        }

        if (normalizedChannel == DemoReleaseChannel || normalizedChannel == LegacyReleaseChannel)
        {
            return DemoReleaseChannel;
        }

        return DemoReleaseChannel;
    }

    /// <summary>
    /// Returns the version suffix used by the supplied edition.
    /// </summary>
    public static string GetDisplaySuffix(ReleaseEdition edition)
    {
        if (edition == ReleaseEdition.Export)
        {
            return "e";
        }

        return "d";
    }

    /// <summary>
    /// Returns the version suffix for the active build edition.
    /// </summary>
    public static string GetCurrentDisplaySuffix()
    {
        return GetDisplaySuffix(GetCurrentEdition());
    }

    /// <summary>
    /// Formats a user-facing version label with the correct edition suffix.
    /// </summary>
    public static string FormatDisplayVersion(string baseVersion, ReleaseEdition edition)
    {
        string suffix = GetDisplaySuffix(edition);
        if (string.IsNullOrWhiteSpace(baseVersion))
        {
            return suffix;
        }

        string trimmedVersion = baseVersion.Trim();
        if (trimmedVersion.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
        {
            return trimmedVersion;
        }

        if (trimmedVersion.EndsWith("d", System.StringComparison.OrdinalIgnoreCase)
            || trimmedVersion.EndsWith("e", System.StringComparison.OrdinalIgnoreCase))
        {
            return trimmedVersion.Substring(0, trimmedVersion.Length - 1) + suffix;
        }

        return trimmedVersion + suffix;
    }

    /// <summary>
    /// Returns whether the active build may expose save and load flows.
    /// </summary>
    public static bool CanUseSaveLoad()
    {
        return GetCurrentEdition() == ReleaseEdition.Export;
    }

    /// <summary>
    /// Returns the standard user-facing message for disabled persistence flows.
    /// </summary>
    public static string GetPersistenceDisabledMessage()
    {
        return "Save and load are only available in the 0.9e build.";
    }

    /// <summary>
    /// Reads and normalizes the configured release-channel setting from the project.
    /// </summary>
    private static string GetConfiguredReleaseChannel()
    {
        Variant configuredValue = ProjectSettings.GetSetting(
            ReleaseChannelSettingPath,
            Variant.From(DefaultReleaseChannel));
        if (configuredValue.VariantType == Variant.Type.String)
        {
            return NormalizeReleaseChannel(configuredValue.AsString());
        }

        return DemoReleaseChannel;
    }
}
