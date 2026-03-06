using System.IO;
using System.IO.Compression;
using System.Text;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;

namespace StarGen.Services.Persistence;

/// <summary>
/// File I/O service for galaxy save payloads.
/// </summary>
public static class GalaxyPersistence
{
    /// <summary>
    /// Default file extension for binary galaxy saves.
    /// </summary>
    public const string BinaryExtension = "sgg";

    /// <summary>
    /// Default file extension for JSON galaxy saves.
    /// </summary>
    public const string JsonExtension = "json";

    private static readonly byte[] BinaryMagic = Encoding.ASCII.GetBytes("SGG1");

    /// <summary>
    /// Saves galaxy data to a JSON file.
    /// </summary>
    public static string SaveJson(string path, GalaxySaveData? data)
    {
        if (data == null)
        {
            return "No data to save";
        }

        if (!data.IsValid())
        {
            return "Invalid galaxy save data";
        }

        string globalPath = ProjectSettings.GlobalizePath(path);
        PersistenceUtils.EnsureDirectoryExists(globalPath);
        string json = Json.Stringify(data.ToDictionary(), "  ");
        File.WriteAllText(globalPath, json, Encoding.UTF8);
        return string.Empty;
    }

    /// <summary>
    /// Loads galaxy data from a JSON file.
    /// Returns null if the file is missing, unreadable, or JSON is invalid.
    /// </summary>
    public static GalaxySaveData? LoadJson(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        if (!File.Exists(globalPath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(globalPath, Encoding.UTF8);
            Json parser = new();
            if (parser.Parse(json) != Error.Ok || parser.Data.VariantType != Variant.Type.Dictionary)
            {
                return null;
            }

            return GalaxySaveData.FromDictionary((Dictionary)parser.Data);
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Preserves the current contract for binary saves without silently changing the format.
    /// </summary>
    public static string SaveBinary(string path, GalaxySaveData? data)
    {
        if (data == null)
        {
            return "No data to save";
        }

        if (!data.IsValid())
        {
            return "Invalid galaxy save data";
        }

        try
        {
            string savePath;
            if (Path.GetExtension(path).Equals(".sgg", System.StringComparison.OrdinalIgnoreCase))
            {
                savePath = path;
            }
            else
            {
                savePath = $"{Path.ChangeExtension(path, null)}.sgg";
            }

            string globalPath = ProjectSettings.GlobalizePath(savePath);
            PersistenceUtils.EnsureDirectoryExists(globalPath);

            string json = Json.Stringify(data.ToDictionary());
            byte[] payload = Encoding.UTF8.GetBytes(json);

            using FileStream stream = File.Create(globalPath);
            stream.Write(BinaryMagic, 0, BinaryMagic.Length);
            using GZipStream gzip = new(stream, CompressionLevel.Optimal, leaveOpen: false);
            gzip.Write(payload, 0, payload.Length);
            return string.Empty;
        }
        catch (System.Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// Preserves the current contract for binary loads without silently changing the format.
    /// </summary>
    public static GalaxySaveData? LoadBinary(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        if (!File.Exists(globalPath))
        {
            return null;
        }

        try
        {
            using FileStream stream = File.OpenRead(globalPath);
            byte[] header = new byte[BinaryMagic.Length];
            int read = stream.Read(header, 0, header.Length);
            if (read != BinaryMagic.Length)
            {
                return null;
            }

            for (int index = 0; index < BinaryMagic.Length; index += 1)
            {
                if (header[index] != BinaryMagic[index])
                {
                    return null;
                }
            }

            using GZipStream gzip = new(stream, CompressionMode.Decompress, leaveOpen: false);
            using MemoryStream output = new();
            gzip.CopyTo(output);

            string json = Encoding.UTF8.GetString(output.ToArray());
            Json parser = new();
            if (parser.Parse(json) != Error.Ok || parser.Data.VariantType != Variant.Type.Dictionary)
            {
                return null;
            }

            return GalaxySaveData.FromDictionary((Dictionary)parser.Data);
        }
        catch (InvalidDataException)
        {
            return null;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Loads galaxy data by file extension.
    /// </summary>
    public static GalaxySaveData? LoadAuto(string path)
    {
        string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        if (extension == JsonExtension)
        {
            return LoadJson(path);
        }

        return LoadBinary(path);
    }

    /// <summary>
    /// Returns the recommended file-dialog filter.
    /// </summary>
    public static string GetFileFilter()
    {
        return "*.sgg ; StarGen Galaxy, *.json ; JSON Debug";
    }

}
