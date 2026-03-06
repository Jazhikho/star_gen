using System.IO;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Services.Persistence;

namespace StarGen.App.Viewer;

/// <summary>
/// Generation and persistence handlers for ObjectViewer.
/// </summary>
public partial class ObjectViewer
{
    private void SetupControls()
    {
        SetupTypeOptions();
        SetupPopulationOptions();

        if (_generateButton != null)
        {
            _generateButton.Pressed += OnGeneratePressed;
        }

        if (_rerollButton != null)
        {
            _rerollButton.Pressed += OnRerollPressed;
        }

        if (_saveButton != null)
        {
            _saveButton.Pressed += OnSavePressed;
        }

        if (_loadButton != null)
        {
            _loadButton.Pressed += OnLoadPressed;
        }

        if (_saveFileDialog != null)
        {
            _saveFileDialog.FileSelected += OnSaveFileSelected;
        }

        if (_loadFileDialog != null)
        {
            _loadFileDialog.FileSelected += OnLoadFileSelected;
        }
    }

    private void SetupTypeOptions()
    {
        if (_typeOption == null || _typeOption.ItemCount > 0)
        {
            return;
        }

        _typeOption.AddItem("Star", (int)ObjectType.Star);
        _typeOption.AddItem("Planet", (int)ObjectType.Planet);
        _typeOption.AddItem("Moon", (int)ObjectType.Moon);
        _typeOption.AddItem("Asteroid", (int)ObjectType.Asteroid);
        _typeOption.Selected = (int)ObjectType.Planet;
    }

    private void SetupPopulationOptions()
    {
        if (_populationOption == null || _populationOption.ItemCount > 0)
        {
            return;
        }

        _populationOption.AddItem("Off", 0);
        _populationOption.AddItem("On (later)", 1);
        _populationOption.Selected = 0;
    }

    private void OnGeneratePressed()
    {
        int seedValue = 0;
        if (_seedInput != null)
        {
            seedValue = (int)_seedInput.Value;
        }

        ObjectType objectType = ObjectType.Planet;
        if (_typeOption != null)
        {
            objectType = (ObjectType)_typeOption.GetSelectedId();
        }

        generate_object(objectType, seedValue);
        UpdateFileInfoForCurrentTarget();
    }

    private void OnRerollPressed()
    {
        int seedValue = unchecked((int)GD.Randi());
        if (_seedInput != null)
        {
            _seedInput.Value = seedValue;
        }

        OnGeneratePressed();
    }

    private void OnSavePressed()
    {
        CelestialBody? targetBody = GetCurrentSaveTargetBody();
        if (targetBody == null)
        {
            SetStatus("No object to save");
            return;
        }

        if (_saveFileDialog == null)
        {
            SetStatus("Save dialog is unavailable");
            return;
        }

        _saveFileDialog.Filters = SaveData.GetFileFilters(targetBody.Type, includeLegacy: true);
        _saveFileDialog.CurrentFile = BuildDefaultBodyFileName(targetBody);
        _saveFileDialog.PopupCentered(new Vector2I(600, 400));
    }

    private void OnLoadPressed()
    {
        if (_loadFileDialog == null)
        {
            SetStatus("Load dialog is unavailable");
            return;
        }

        _loadFileDialog.Filters = SaveData.GetFileFilters(includeLegacy: true);
        _loadFileDialog.PopupCentered(new Vector2I(600, 400));
    }

    private void OnSaveFileSelected(string path)
    {
        CelestialBody? targetBody = GetCurrentSaveTargetBody();
        if (targetBody == null)
        {
            SetStatus("No object to save");
            return;
        }

        bool compress = !path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase);
        string savePath = SaveData.ResolveSavePath(targetBody, path, compress);
        SaveData.SaveMode saveMode = GetPreferredSaveMode(targetBody);
        Error error = SaveData.SaveBody(targetBody, savePath, saveMode, compress);
        if (error != Error.Ok)
        {
            SetStatus($"Save failed: {error}");
            return;
        }

        long size = SaveData.GetFileSize(savePath);
        string sizeString = SaveData.FormatFileSize(size);
        SetStatus($"Saved {targetBody.Name} to {Path.GetFileName(savePath)} ({sizeString})");
        UpdateFileInfo(Path.GetFileName(savePath), targetBody);
    }

    private void OnLoadFileSelected(string path)
    {
        SaveDataLoadResult result = SaveData.LoadBody(path);
        if (!result.Success)
        {
            SetStatus($"Load failed: {result.ErrorMessage}");
            return;
        }

        if (result.Body == null)
        {
            SetStatus("Loaded file contains no object data");
            return;
        }

        _currentBody = result.Body;
        _gdCurrentBody = result.Body;
        _currentMoons.Clear();
        _gdCurrentMoons.Clear();
        _gdMoonById.Clear();
        _sourceStarSeed = 0;

        if (_backButton != null && !_navigatedFromSystem)
        {
            _backButton.Visible = false;
        }

        SetGenerationControlsEnabled(true);
        SetFileControlsEnabled(true);
        DisplayBodyWithMoons(result.Body, _currentMoons);
        SetStatus($"Loaded: {Path.GetFileName(path)}");
        UpdateFileInfo(Path.GetFileName(path), result.Body);
    }

    public Error SaveCurrentBodyToPath(string path)
    {
        CelestialBody? targetBody = GetCurrentSaveTargetBody();
        if (targetBody == null)
        {
            return Error.InvalidParameter;
        }

        bool compress = !path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase);
        string savePath = SaveData.ResolveSavePath(targetBody, path, compress);
        return SaveData.SaveBody(targetBody, savePath, GetPreferredSaveMode(targetBody), compress);
    }

    public SaveDataLoadResult LoadBodyFromPath(string path)
    {
        SaveDataLoadResult result = SaveData.LoadBody(path);
        if (result.Success && result.Body != null)
        {
            DisplayExternalBody(result.Body, [], 0);
            _navigatedFromSystem = false;
            if (_backButton != null)
            {
                _backButton.Visible = false;
            }

            SetGenerationControlsEnabled(true);
            UpdateFileInfo(Path.GetFileName(path), result.Body);
        }

        return result;
    }

    private CelestialBody? GetCurrentSaveTargetBody()
    {
        if (_moonSystem != null && _moonSystem.GetFocusedMoon() != null)
        {
            return _moonSystem.GetFocusedMoon();
        }

        return _currentBody;
    }

    private SaveData.SaveMode GetPreferredSaveMode(CelestialBody body)
    {
        if (body.HasMeta("user_modifications"))
        {
            Variant modifications = body.GetMeta("user_modifications");
            if (modifications.VariantType == Variant.Type.Dictionary
                && ((Godot.Collections.Dictionary)modifications).Count > 0)
            {
                return SaveData.SaveMode.Full;
            }
        }

        return SaveData.SaveMode.Compact;
    }

    private string BuildDefaultBodyFileName(CelestialBody body)
    {
        string baseName = body.Name;
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = body.Id;
        }

        if (string.IsNullOrWhiteSpace(baseName) && body.Provenance != null)
        {
            baseName = $"{body.GetTypeString()}_{body.Provenance.GenerationSeed}";
        }

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = body.GetTypeString();
        }

        string safeName = baseName.Replace(" ", "_").ToLowerInvariant();
        return $"{safeName}.{SaveData.GetPreferredBinaryExtension(body.Type)}";
    }

    private void UpdateFileInfoForCurrentTarget()
    {
        CelestialBody? targetBody = GetCurrentSaveTargetBody();
        if (targetBody != null)
        {
            UpdateFileInfo(string.Empty, targetBody);
        }
    }

    private void UpdateFileInfo(string fileName, CelestialBody body)
    {
        if (_fileInfo == null)
        {
            return;
        }

        string seedText = "unknown";
        if (body.Provenance != null)
        {
            seedText = body.Provenance.GenerationSeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        string typeLabel = body.Type switch
        {
            CelestialType.Type.Star => "Star",
            CelestialType.Type.Planet => "Planet",
            CelestialType.Type.Moon => "Moon",
            CelestialType.Type.Asteroid => "Asteroid",
            _ => "Object",
        };

        if (string.IsNullOrEmpty(fileName))
        {
            _fileInfo.Text = $"{typeLabel} save ready. Seed: {seedText}";
            return;
        }

        _fileInfo.Text = $"{typeLabel} file: {fileName} | Seed: {seedText}";
    }
}
