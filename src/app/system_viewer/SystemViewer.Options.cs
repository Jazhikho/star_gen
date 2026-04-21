using Godot;
using StarGen.Services.Persistence;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Shared viewer-local options dialog behavior for the system viewer.
/// </summary>
public partial class SystemViewer
{
    private const float CameraPanelAnimationDurationSeconds = 0.18f;
    private const float CameraPanelCornerMarginPixels = 16.0f;
    private const float CameraPanelCollapsedWidthPaddingPixels = 18.0f;
    private const float CameraPanelCollapsedHeightPaddingPixels = 10.0f;

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
		SetCameraPanelCollapsed(!_cameraPanelCollapsed, true);
	}

	private void SetCameraPanelCollapsed(bool collapsed)
	{
		SetCameraPanelCollapsed(collapsed, false);
	}

	private void SetCameraPanelCollapsed(bool collapsed, bool animate)
	{
		_cameraPanelCollapsed = collapsed;
		if (_cameraPanel == null || _cameraPanelHeaderButton == null)
		{
			return;
		}

		if (_cameraPanelExpandedSize == Vector2.Zero)
		{
			_cameraPanelExpandedSize = MeasureCurrentPanelSize();
		}

		if (_cameraPanelTween != null && GodotObject.IsInstanceValid(_cameraPanelTween))
		{
			_cameraPanelTween.Kill();
			_cameraPanelTween = null;
		}

		_cameraPanel.ClipContents = true;
		Vector2 targetSize;
		if (collapsed)
		{
			targetSize = MeasureCollapsedPanelSize();
		}
		else
		{
			targetSize = _cameraPanelExpandedSize;
			if (_cameraPanelContent != null)
			{
				_cameraPanelContent.Visible = true;
			}
		}

		_cameraPanelHeaderButton.Text = collapsed ? "> Controls" : "^ Controls";
		if (!animate || !IsInsideTree())
		{
			ApplyCameraPanelSize(targetSize);
			if (_cameraPanelContent != null)
			{
				_cameraPanelContent.Visible = !collapsed;
			}

			return;
		}

		_cameraPanelTween = CreateTween();
		_cameraPanelTween.SetTrans(Tween.TransitionType.Cubic);
		_cameraPanelTween.SetEase(Tween.EaseType.Out);
		Vector2 targetOffsets = ComputeCameraPanelTopLeft(targetSize);
		_cameraPanelTween.TweenProperty(_cameraPanel, "offset_left", targetOffsets.X, CameraPanelAnimationDurationSeconds);
		_cameraPanelTween.Parallel().TweenProperty(_cameraPanel, "offset_top", targetOffsets.Y, CameraPanelAnimationDurationSeconds);
		_cameraPanelTween.TweenCallback(Callable.From(() => FinalizeCameraPanelAnimation(collapsed)));
	}

	private void FinalizeCameraPanelAnimation(bool collapsed)
	{
		if (_cameraPanelContent != null)
		{
			_cameraPanelContent.Visible = !collapsed;
		}

		_cameraPanelTween = null;
	}

	private Vector2 MeasureCurrentPanelSize()
	{
		if (_cameraPanel == null)
		{
			return Vector2.Zero;
		}

		float width = _cameraPanel.OffsetRight - _cameraPanel.OffsetLeft;
		float height = _cameraPanel.OffsetBottom - _cameraPanel.OffsetTop;
		return new Vector2(width, height);
	}

	private Vector2 MeasureCollapsedPanelSize()
	{
		if (_cameraPanelHeaderButton == null)
		{
			return new Vector2(110.0f, 34.0f);
		}

		Vector2 headerSize = _cameraPanelHeaderButton.GetCombinedMinimumSize();
		float width = Mathf.Ceil(headerSize.X + CameraPanelCollapsedWidthPaddingPixels);
		float height = Mathf.Ceil(headerSize.Y + CameraPanelCollapsedHeightPaddingPixels);
		return new Vector2(width, height);
	}

	private void ApplyCameraPanelSize(Vector2 size)
	{
		if (_cameraPanel == null)
		{
			return;
		}

		Vector2 topLeft = ComputeCameraPanelTopLeft(size);
		_cameraPanel.OffsetLeft = topLeft.X;
		_cameraPanel.OffsetTop = topLeft.Y;
		_cameraPanel.OffsetRight = -CameraPanelCornerMarginPixels;
		_cameraPanel.OffsetBottom = -CameraPanelCornerMarginPixels;
	}

	private static Vector2 ComputeCameraPanelTopLeft(Vector2 size)
	{
		float left = -CameraPanelCornerMarginPixels - size.X;
		float top = -CameraPanelCornerMarginPixels - size.Y;
		return new Vector2(left, top);
	}

	private void InitializeCameraPanelLayout()
	{
		if (_cameraPanel == null)
		{
			return;
		}

		_cameraPanelExpandedSize = MeasureCurrentPanelSize();
		SetCameraPanelCollapsed(true, false);
	}
}
