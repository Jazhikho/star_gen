using Godot;
using StarGen.Services.Persistence;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Shared viewer-local options dialog behavior for the system viewer.
/// </summary>
public partial class SystemViewer
{
    private void SetupOptionsUi()
    {
        PopulateResolutionOptions();
        RefreshOptionsState();
        ConnectOptionsSignals();
    }

    private void PopulateResolutionOptions()
    {
        if (_resolutionOption == null)
        {
            return;
        }

        _resolutionOption.Clear();
        foreach (Vector2I resolution in WindowSettingsService.GetCommonResolutions())
        {
            _resolutionOption.AddItem(WindowSettingsService.FormatResolutionLabel(resolution));
            _resolutionOption.SetItemMetadata(
                _resolutionOption.ItemCount - 1,
                WindowSettingsService.FormatResolutionKey(resolution));
        }
    }

	private void RefreshOptionsState()
	{
		if (_fullscreenCheck == null || _resolutionOption == null)
		{
			return;
		}

		WindowSettingsService.WindowSettingsState currentSettings = WindowSettingsService.LoadOrCaptureCurrent();
        _fullscreenCheck.ButtonPressed = currentSettings.Fullscreen;
        int index = WindowSettingsService.FindResolutionIndex(currentSettings.Resolution);
        if (index >= 0)
        {
            _resolutionOption.Select(index);
        }

        _resolutionOption.Disabled = currentSettings.Fullscreen;
        StudioUiPreferencesService.StudioUiPreferences preferences = StudioUiPreferencesService.LoadOrDefault();
        if (_showSeedControlsCheck != null)
        {
            _showSeedControlsCheck.ButtonPressed = preferences.ShowSeedControls;
        }

        if (_skipIntroCheck != null)
        {
            _skipIntroCheck.ButtonPressed = preferences.SkipIntro;
        }

        if (_optionsStatusLabel != null)
        {
            string modeSummary;
            if (currentSettings.Fullscreen)
            {
                modeSummary = "Fullscreen active";
            }
            else
            {
                modeSummary = $"Windowed at {currentSettings.Resolution.X} x {currentSettings.Resolution.Y}";
            }

            string seedSummary;
            if (preferences.ShowSeedControls)
            {
                seedSummary = "All studio seeds are visible.";
            }
            else
            {
                seedSummary = "All studio seeds are hidden.";
            }

            string introSummary;
            if (preferences.SkipIntro)
            {
                introSummary = "Intro is skipped on startup.";
            }
            else
            {
                introSummary = "Intro plays on startup.";
            }

            _optionsStatusLabel.Text = $"{modeSummary}. {seedSummary} {introSummary}";
        }
    }

    private void ConnectOptionsSignals()
    {
        if (_applyOptionsButton != null)
        {
            _applyOptionsButton.Pressed += ApplyOptionsSettings;
        }

        if (_optionsDialogCloseButton != null)
        {
            _optionsDialogCloseButton.Pressed += HideOptionsDialog;
        }

        if (_optionsDialog != null)
        {
            _optionsDialog.CloseRequested += HideOptionsDialog;
        }

        if (_fullscreenCheck != null)
        {
            _fullscreenCheck.Toggled += OnOptionsFullscreenToggled;
        }
    }

    private void OnOptionsFullscreenToggled(bool fullscreen)
    {
        if (_resolutionOption != null)
        {
            _resolutionOption.Disabled = fullscreen;
        }
    }

    private void OpenOptionsDialog()
    {
        if (_optionsDialog == null)
        {
            SetStatus("Options dialog is unavailable in this scene");
            return;
        }

        RefreshOptionsState();
        ShowViewerDialog(_optionsDialog, new Vector2I(460, 280));
    }

    private void HideOptionsDialog()
    {
        if (_optionsDialog != null)
        {
            _optionsDialog.Hide();
        }
    }

    private static void ShowViewerDialog(Window dialog, Vector2I size)
    {
        if (dialog.IsInsideTree())
        {
            dialog.PopupCentered(size);
            return;
        }

        dialog.Size = size;
        dialog.Visible = true;
    }

    private void ApplyOptionsSettings()
    {
        if (_fullscreenCheck == null || _resolutionOption == null)
        {
            return;
        }

        Vector2I resolution = WindowSettingsService.CaptureCurrent().Resolution;
        if (!_fullscreenCheck.ButtonPressed)
        {
            Variant metadata = _resolutionOption.GetSelectedMetadata();
            if (metadata.VariantType == Variant.Type.String)
            {
                string key = metadata.AsString();
                if (WindowSettingsService.TryParseResolutionKey(key, out Vector2I parsedResolution))
                {
                    resolution = parsedResolution;
                }
            }
        }

		WindowSettingsService.ApplyAndSave(new WindowSettingsService.WindowSettingsState(_fullscreenCheck.ButtonPressed, resolution));
		bool showSeeds = _showSeedControlsCheck != null && _showSeedControlsCheck.ButtonPressed;
		bool skipIntro = _skipIntroCheck != null && _skipIntroCheck.ButtonPressed;
		StudioUiPreferencesService.Save(new StudioUiPreferencesService.StudioUiPreferences(showSeeds, skipIntro));
		RefreshOptionsState();
		SetStatus("Viewer options updated");
		HideOptionsDialog();
	}

	private void ToggleCameraPanel()
	{
		bool collapsed = _cameraPanelContent == null || _cameraPanelContent.Visible;
		SetCameraPanelCollapsed(collapsed);
	}

	private void SetCameraPanelCollapsed(bool collapsed)
	{
		if (_cameraPanelContent != null)
		{
			_cameraPanelContent.Visible = !collapsed;
		}

		if (_cameraPanelHeaderButton != null)
		{
			_cameraPanelHeaderButton.Text = collapsed ? "> Camera" : "v Camera";
		}
	}
}
