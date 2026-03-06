using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

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
    public int Version { get; set; } = FormatVersion;

    /// <summary>
    /// Galaxy master seed.
    /// </summary>
    public int GalaxySeed { get; set; } = 42;

    /// <summary>
    /// Unix timestamp at save time.
    /// </summary>
    public long SavedAt { get; set; }

    /// <summary>
    /// Current zoom level.
    /// </summary>
    public GalaxyCoordinates.ZoomLevel ZoomLevel { get; set; } = GalaxyCoordinates.ZoomLevel.Subsector;

    /// <summary>
    /// Selected quadrant coordinates, if any.
    /// </summary>
    public Vector3I? SelectedQuadrant { get; set; }

    /// <summary>
    /// Selected sector coordinates, if any.
    /// </summary>
    public Vector3I? SelectedSector { get; set; }

    /// <summary>
    /// Camera position in subsector view.
    /// </summary>
    public Vector3 CameraPosition { get; set; } = Vector3.Zero;

    /// <summary>
    /// Camera rotation in subsector view.
    /// </summary>
    public Vector3 CameraRotation { get; set; } = Vector3.Zero;

    /// <summary>
    /// Whether a star is currently selected.
    /// </summary>
    public bool HasStarSelection { get; set; }

    /// <summary>
    /// Selected star seed, if any.
    /// </summary>
    public int SelectedStarSeed { get; set; }

    /// <summary>
    /// Selected star position, if any.
    /// </summary>
    public Vector3 SelectedStarPosition { get; set; } = Vector3.Zero;

    /// <summary>
    /// Serialized galaxy configuration payload.
    /// </summary>
    public Dictionary GalaxyConfigData { get; set; } = new();

    /// <summary>
    /// Informational cached-system count.
    /// </summary>
    public int CachedSystemCount { get; set; }

    /// <summary>
    /// Serialized jump-lane region payload.
    /// </summary>
    public Dictionary JumpLaneRegionData { get; set; } = new();

    /// <summary>
    /// Serialized jump-lane result payload.
    /// </summary>
    public Dictionary JumpLaneResultData { get; set; } = new();

    /// <summary>
    /// Serialized edited-body overrides payload.
    /// </summary>
    public Dictionary BodyOverridesData { get; set; } = new();

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
        int zoom = (int)ZoomLevel;
        return zoom >= minZoom && zoom <= maxZoom;
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
            ["zoom_level"] = (int)ZoomLevel,
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
            Version = DomainDictionaryUtils.GetInt(data, "version", FormatVersion),
            GalaxySeed = DomainDictionaryUtils.GetInt(data, "galaxy_seed", 42),
            SavedAt = DomainDictionaryUtils.GetLong(data, "saved_at", 0),
            ZoomLevel = (GalaxyCoordinates.ZoomLevel)DomainDictionaryUtils.GetInt(data, "zoom_level", (int)GalaxyCoordinates.ZoomLevel.Subsector),
            HasStarSelection = DomainDictionaryUtils.GetBool(data, "has_star_selection", false),
            SelectedStarSeed = DomainDictionaryUtils.GetInt(data, "selected_star_seed", 0),
        };

        if (DomainDictionaryUtils.TryGetArray(data, "camera_position", out Array cameraPosition))
        {
            saveData.CameraPosition = ArrayToVector3(cameraPosition);
        }

        if (DomainDictionaryUtils.TryGetArray(data, "camera_rotation", out Array cameraRotation))
        {
            saveData.CameraRotation = ArrayToVector3(cameraRotation);
        }

        if (DomainDictionaryUtils.TryGetArray(data, "selected_quadrant", out Array selectedQuadrant))
        {
            saveData.SelectedQuadrant = ArrayToVector3I(selectedQuadrant);
        }

        if (DomainDictionaryUtils.TryGetArray(data, "selected_sector", out Array selectedSector))
        {
            saveData.SelectedSector = ArrayToVector3I(selectedSector);
        }

        if (DomainDictionaryUtils.TryGetArray(data, "selected_star_position", out Array selectedStarPosition))
        {
            saveData.SelectedStarPosition = ArrayToVector3(selectedStarPosition);
        }

        saveData.GalaxyConfigData = DomainDictionaryUtils.GetDictionary(data, "galaxy_config_data");
        saveData.CachedSystemCount = DomainDictionaryUtils.GetInt(data, "cached_system_count", 0);
        saveData.JumpLaneRegionData = DomainDictionaryUtils.GetDictionary(data, "jump_lane_region_data");
        saveData.JumpLaneResultData = DomainDictionaryUtils.GetDictionary(data, "jump_lane_result_data");
        saveData.BodyOverridesData = DomainDictionaryUtils.GetDictionary(data, "body_overrides_data");
        return saveData;
    }

    /// <summary>
    /// Returns a human-readable summary string.
    /// </summary>
    public string GetSummary()
    {
        return $"Seed {GalaxySeed}, {GetZoomName((int)ZoomLevel)} view, saved {SavedAt}";
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
        if (GalaxyConfigData.Count == 0)
        {
            return null;
        }

        return GalaxyConfig.FromDictionary(GalaxyConfigData);
    }

    /// <summary>
    /// Stores a galaxy configuration into the save payload.
    /// </summary>
    public void SetConfig(GalaxyConfig? config)
    {
        if (config != null)
        {
            GalaxyConfigData = config.ToDictionary();
        }
        else
        {
            GalaxyConfigData = new Dictionary();
        }
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
        if (BodyOverridesData.Count == 0)
        {
            return new GalaxyBodyOverrides();
        }

        return GalaxyBodyOverrides.FromDictionary(BodyOverridesData);
    }

    /// <summary>
    /// Stores body overrides into the save payload.
    /// </summary>
    public void SetBodyOverrides(GalaxyBodyOverrides? overrides)
    {
        if (overrides == null || overrides.IsEmpty())
        {
            BodyOverridesData = new Dictionary();
        }
        else
        {
            BodyOverridesData = overrides.ToDictionary();
        }
    }

    /// <summary>
    /// Converts a Vector3 to an array payload for JSON compatibility.
    /// </summary>
    public static Array Vector3ToArray(Vector3 value)
    {
        return new Array { value.X, value.Y, value.Z };
    }

    /// <summary>
    /// Converts a Vector3I to an array payload for JSON compatibility.
    /// </summary>
    public static Array Vector3IToArray(Vector3I value)
    {
        return new Array { value.X, value.Y, value.Z };
    }

    /// <summary>
    /// Converts a numeric array payload to a Vector3.
    /// </summary>
    public static Vector3 ArrayToVector3(Array values)
    {
        if (values.Count < 3)
        {
            return Vector3.Zero;
        }

        return new Vector3(
            (float)DomainDictionaryUtils.GetNumeric(values[0]),
            (float)DomainDictionaryUtils.GetNumeric(values[1]),
            (float)DomainDictionaryUtils.GetNumeric(values[2]));
    }

    /// <summary>
    /// Converts a numeric array payload to a Vector3I.
    /// </summary>
    public static Vector3I ArrayToVector3I(Array values)
    {
        if (values.Count < 3)
        {
            return Vector3I.Zero;
        }

        return new Vector3I(
            (int)DomainDictionaryUtils.GetNumeric(values[0]),
            (int)DomainDictionaryUtils.GetNumeric(values[1]),
            (int)DomainDictionaryUtils.GetNumeric(values[2]));
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
    /// Legacy casing alias.
    /// </summary>
    public static Array Vector3iToArray(Vector3I value) => Vector3IToArray(value);

    /// <summary>
    /// Legacy casing alias.
    /// </summary>
    public static Vector3I ArrayToVector3i(Array values) => ArrayToVector3I(values);

}
