using Godot;
using Godot.Collections;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Serializable data structure for galaxy-viewer save files.
/// </summary>
public partial class GalaxySaveData : RefCounted
{
    /// <summary>
    /// Current save format version.
    /// </summary>
    public const int FormatVersion = 1;

    /// <summary>
    /// File format version.
    /// </summary>
    public int Version = FormatVersion;

    /// <summary>
    /// Galaxy master seed.
    /// </summary>
    public int GalaxySeed = 42;

    /// <summary>
    /// Unix timestamp at save time.
    /// </summary>
    public long SavedAt;

    /// <summary>
    /// Current zoom level.
    /// </summary>
    public int ZoomLevel = (int)GalaxyCoordinates.ZoomLevel.Subsector;

    /// <summary>
    /// Selected quadrant coordinates, if any.
    /// </summary>
    public Vector3I? SelectedQuadrant;

    /// <summary>
    /// Selected sector coordinates, if any.
    /// </summary>
    public Vector3I? SelectedSector;

    /// <summary>
    /// Camera position in subsector view.
    /// </summary>
    public Vector3 CameraPosition = Vector3.Zero;

    /// <summary>
    /// Camera rotation in subsector view.
    /// </summary>
    public Vector3 CameraRotation = Vector3.Zero;

    /// <summary>
    /// Returns whether a star is selected.
    /// </summary>
    public bool HasStarSelection;

    /// <summary>
    /// Selected star seed, if any.
    /// </summary>
    public int SelectedStarSeed;

    /// <summary>
    /// Selected star position, if any.
    /// </summary>
    public Vector3 SelectedStarPosition = Vector3.Zero;

    /// <summary>
    /// Serialized galaxy configuration payload.
    /// </summary>
    public Dictionary GalaxyConfigData = new();

    /// <summary>
    /// Informational cached-system count.
    /// </summary>
    public int CachedSystemCount;

    /// <summary>
    /// Serialized jump-lane region payload.
    /// </summary>
    public Dictionary JumpLaneRegionData = new();

    /// <summary>
    /// Serialized jump-lane result payload.
    /// </summary>
    public Dictionary JumpLaneResultData = new();

    /// <summary>
    /// Serialized edited-body overrides payload.
    /// </summary>
    public Dictionary BodyOverridesData = new();

    /// <summary>
    /// Creates a new save-data payload for the supplied timestamp.
    /// </summary>
    public static GalaxySaveData Create(long timestamp)
    {
        GalaxySaveData data = new();
        data.SavedAt = timestamp;
        return data;
    }

    /// <summary>
    /// Returns whether the save-data payload is structurally valid.
    /// </summary>
    public bool IsValid()
    {
        if (GalaxySeed == 0)
        {
            return false;
        }

        int minZoom = (int)GalaxyCoordinates.ZoomLevel.Galaxy;
        int maxZoom = (int)GalaxyCoordinates.ZoomLevel.Subsector;
        return ZoomLevel >= minZoom && ZoomLevel <= maxZoom;
    }

    /// <summary>
    /// Converts the save data to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new()
        {
            ["version"] = Version,
            ["galaxy_seed"] = GalaxySeed,
            ["saved_at"] = SavedAt,
            ["zoom_level"] = ZoomLevel,
            ["camera_position"] = Vector3ToArray(CameraPosition),
            ["camera_rotation"] = Vector3ToArray(CameraRotation),
            ["has_star_selection"] = HasStarSelection,
            ["selected_star_seed"] = SelectedStarSeed,
            ["selected_star_position"] = Vector3ToArray(SelectedStarPosition),
            ["galaxy_config_data"] = GalaxyConfigData,
            ["cached_system_count"] = CachedSystemCount,
            ["jump_lane_region_data"] = JumpLaneRegionData,
            ["jump_lane_result_data"] = JumpLaneResultData,
            ["body_overrides_data"] = BodyOverridesData,
        };

        if (SelectedQuadrant.HasValue)
        {
            data["selected_quadrant"] = Vector3IToArray(SelectedQuadrant.Value);
        }
        else
        {
            data["selected_quadrant"] = default(Variant);
        }

        if (SelectedSector.HasValue)
        {
            data["selected_sector"] = Vector3IToArray(SelectedSector.Value);
        }
        else
        {
            data["selected_sector"] = default(Variant);
        }
        return data;
    }

    /// <summary>
    /// Rebuilds save data from a dictionary payload.
    /// </summary>
    public static GalaxySaveData? FromDictionary(Dictionary data)
    {
        if (!data.ContainsKey("version") || !data.ContainsKey("galaxy_seed"))
        {
            return null;
        }

        GalaxySaveData saveData = new()
        {
            Version = GetInt(data, "version", FormatVersion),
            GalaxySeed = GetInt(data, "galaxy_seed", 42),
            SavedAt = GetLong(data, "saved_at", 0),
            ZoomLevel = GetInt(data, "zoom_level", (int)GalaxyCoordinates.ZoomLevel.Subsector),
            HasStarSelection = GetBool(data, "has_star_selection", false),
            SelectedStarSeed = GetInt(data, "selected_star_seed", 0),
        };

        if (TryGetArray(data, "camera_position", out Array cameraPosition))
        {
            saveData.CameraPosition = ArrayToVector3(cameraPosition);
        }

        if (TryGetArray(data, "camera_rotation", out Array cameraRotation))
        {
            saveData.CameraRotation = ArrayToVector3(cameraRotation);
        }

        if (TryGetArray(data, "selected_quadrant", out Array selectedQuadrant))
        {
            saveData.SelectedQuadrant = ArrayToVector3I(selectedQuadrant);
        }

        if (TryGetArray(data, "selected_sector", out Array selectedSector))
        {
            saveData.SelectedSector = ArrayToVector3I(selectedSector);
        }

        if (TryGetArray(data, "selected_star_position", out Array selectedStarPosition))
        {
            saveData.SelectedStarPosition = ArrayToVector3(selectedStarPosition);
        }

        saveData.GalaxyConfigData = GetDictionary(data, "galaxy_config_data");
        saveData.CachedSystemCount = GetInt(data, "cached_system_count", 0);
        saveData.JumpLaneRegionData = GetDictionary(data, "jump_lane_region_data");
        saveData.JumpLaneResultData = GetDictionary(data, "jump_lane_result_data");
        saveData.BodyOverridesData = GetDictionary(data, "body_overrides_data");
        return saveData;
    }

    /// <summary>
    /// Returns a human-readable summary string.
    /// </summary>
    public string GetSummary()
    {
        return $"Seed {GalaxySeed}, {GetZoomName(ZoomLevel)} view, saved {SavedAt}";
    }

    /// <summary>
    /// Returns whether a galaxy configuration payload is present.
    /// </summary>
    public bool HasConfig()
    {
        return GalaxyConfigData.Count > 0;
    }

    /// <summary>
    /// Returns the galaxy configuration object, if present.
    /// </summary>
    public GalaxyConfig? GetConfig()
    {
        return GalaxyConfigData.Count == 0 ? null : GalaxyConfig.FromDictionary(GalaxyConfigData);
    }

    /// <summary>
    /// Stores a galaxy configuration into the save payload.
    /// </summary>
    public void SetConfig(GalaxyConfig? config)
    {
        GalaxyConfigData = config != null ? config.ToDictionary() : new Dictionary();
    }

    /// <summary>
    /// Returns whether any body overrides are present.
    /// </summary>
    public bool HasBodyOverrides()
    {
        return BodyOverridesData.Count > 0;
    }

    /// <summary>
    /// Returns the stored body overrides as a typed object.
    /// </summary>
    public GalaxyBodyOverrides GetBodyOverrides()
    {
        return BodyOverridesData.Count == 0 ? new GalaxyBodyOverrides() : GalaxyBodyOverrides.FromDictionary(BodyOverridesData);
    }

    /// <summary>
    /// Stores body overrides into the save payload.
    /// </summary>
    public void SetBodyOverrides(GalaxyBodyOverrides? overrides)
    {
        BodyOverridesData = overrides == null || overrides.IsEmpty() ? new Dictionary() : overrides.ToDictionary();
    }

    /// <summary>
    /// Converts a Vector3 to an array payload for JSON compatibility.
    /// </summary>
    private static Array Vector3ToArray(Vector3 value)
    {
        return new Array { value.X, value.Y, value.Z };
    }

    /// <summary>
    /// Converts a Vector3I to an array payload for JSON compatibility.
    /// </summary>
    private static Array Vector3IToArray(Vector3I value)
    {
        return new Array { value.X, value.Y, value.Z };
    }

    /// <summary>
    /// Converts a numeric array payload to a Vector3.
    /// </summary>
    private static Vector3 ArrayToVector3(Array values)
    {
        if (values.Count < 3)
        {
            return Vector3.Zero;
        }

        return new Vector3(
            (float)GetNumeric(values[0]),
            (float)GetNumeric(values[1]),
            (float)GetNumeric(values[2]));
    }

    /// <summary>
    /// Converts a numeric array payload to a Vector3I.
    /// </summary>
    private static Vector3I ArrayToVector3I(Array values)
    {
        if (values.Count < 3)
        {
            return Vector3I.Zero;
        }

        return new Vector3I(
            (int)GetNumeric(values[0]),
            (int)GetNumeric(values[1]),
            (int)GetNumeric(values[2]));
    }

    /// <summary>
    /// Returns the display name of a zoom level.
    /// </summary>
    private static string GetZoomName(int level)
    {
        return level switch
        {
            (int)GalaxyCoordinates.ZoomLevel.Galaxy => "Galaxy",
            (int)GalaxyCoordinates.ZoomLevel.Quadrant => "Quadrant",
            (int)GalaxyCoordinates.ZoomLevel.Sector => "Sector",
            (int)GalaxyCoordinates.ZoomLevel.Subsector => "Star Field",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Reads an integer from a dictionary payload.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a 64-bit integer from a dictionary payload.
    /// </summary>
    private static long GetLong(Dictionary data, string key, long fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (long)(int)value,
            Variant.Type.Float => (long)(double)value,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a boolean from a dictionary payload.
    /// </summary>
    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.Bool ? (bool)value : fallback;
    }

    /// <summary>
    /// Returns a dictionary payload value or an empty dictionary.
    /// </summary>
    private static Dictionary GetDictionary(Dictionary data, string key)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Dictionary
            ? (Dictionary)data[key]
            : new Dictionary();
    }

    /// <summary>
    /// Returns whether a payload key contains an array.
    /// </summary>
    private static bool TryGetArray(Dictionary data, string key, out Array array)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Array)
        {
            array = (Array)data[key];
            return true;
        }

        array = new Array();
        return false;
    }

    /// <summary>
    /// Reads a numeric value from a variant.
    /// </summary>
    private static double GetNumeric(Variant value)
    {
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (double)value,
            _ => 0.0,
        };
    }
}
