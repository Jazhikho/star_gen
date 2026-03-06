using System.IO;

namespace StarGen.Services.Persistence;

/// <summary>
/// Shared file-system helpers used by the persistence services.
/// </summary>
public static class PersistenceUtils
{
    /// <summary>
    /// Creates all directories in the path of the target file if they do not already exist.
    /// </summary>
    /// <param name="globalFilePath">Absolute file path whose parent directory should be guaranteed.</param>
    public static void EnsureDirectoryExists(string globalFilePath)
    {
        string? directoryPath = Path.GetDirectoryName(globalFilePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// Formats a byte count as a human-readable size string (B, KB, MB, GB).
    /// </summary>
    /// <param name="bytes">Number of bytes.</param>
    /// <returns>Human-readable size string.</returns>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024L)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024L * 1024L)
        {
            return $"{bytes / 1024.0:F1} KB";
        }

        if (bytes < 1024L * 1024L * 1024L)
        {
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }

        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}
