using System.Threading.Tasks;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;
using StarGen.Services.Persistence;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Options-dialog and explicit local-space cache tools for GalaxyViewer.
/// </summary>
public partial class GalaxyViewer
{
	private const int DefaultLocalSpaceExtent = 5;
	private const int MaximumLocalSpaceExtent = 10;

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

	private void ConnectLocalSpaceDialogSignals()
	{
		if (_localSpaceExtentXInput != null)
		{
			_localSpaceExtentXInput.ValueChanged += _ => RefreshLocalSpacePreview();
		}

		if (_localSpaceExtentYInput != null)
		{
			_localSpaceExtentYInput.ValueChanged += _ => RefreshLocalSpacePreview();
		}

		if (_localSpaceExtentZInput != null)
		{
			_localSpaceExtentZInput.ValueChanged += _ => RefreshLocalSpacePreview();
		}

		if (_buildLocalSpaceRunButton != null)
		{
			_buildLocalSpaceRunButton.Pressed += OnBuildLocalSpaceRunPressed;
		}

		if (_buildLocalSpaceCloseButton != null)
		{
			_buildLocalSpaceCloseButton.Pressed += HideBuildLocalSpaceDialog;
		}

		if (_buildLocalSpaceDialog != null)
		{
			_buildLocalSpaceDialog.CloseRequested += HideBuildLocalSpaceDialog;
		}

		InitializeLocalSpaceDialogInputs();
	}

	private void InitializeLocalSpaceDialogInputs()
	{
		ConfigureLocalSpaceSpinBox(_localSpaceExtentXInput);
		ConfigureLocalSpaceSpinBox(_localSpaceExtentYInput);
		ConfigureLocalSpaceSpinBox(_localSpaceExtentZInput);
		RefreshLocalSpacePreview();
	}

	private static void ConfigureLocalSpaceSpinBox(SpinBox? spinBox)
	{
		if (spinBox == null)
		{
			return;
		}

		spinBox.MinValue = 0;
		spinBox.MaxValue = MaximumLocalSpaceExtent;
		spinBox.Step = 1;
		spinBox.Value = DefaultLocalSpaceExtent;
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

		WindowSettingsService.WindowSettingsState currentSettings = WindowSettingsService.CaptureCurrent();
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

	private void OnOptionsFullscreenToggled(bool fullscreen)
	{
		if (_resolutionOption != null)
		{
			_resolutionOption.Disabled = fullscreen;
		}
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
		UpdateInspector();
		SetStatus("Viewer options updated");
	}

	private void OpenBuildLocalSpaceDialog()
	{
		if (_buildLocalSpaceDialog == null)
		{
			SetStatus("Build Local Space dialog is unavailable in this scene");
			return;
		}

		if (!IsSubsectorActive())
		{
			SetStatus("Build local space is only available in local subsector view");
			return;
		}

		RefreshLocalSpacePreview();
		ShowViewerDialog(_buildLocalSpaceDialog, new Vector2I(520, 340));
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

	private void HideBuildLocalSpaceDialog()
	{
		if (_buildLocalSpaceDialog != null)
		{
			_buildLocalSpaceDialog.Hide();
		}
	}

	private void RefreshLocalSpacePreview()
	{
		if (_localSpaceAreaLabel == null || _localSpaceStarCountLabel == null || _galaxy == null)
		{
			return;
		}

		if (!IsSubsectorActive())
		{
			_localSpaceAreaLabel.Text = "Coverage: unavailable outside local subsector view";
			_localSpaceStarCountLabel.Text = "Stars in area: unavailable";
			if (_localSpaceStatusLabel != null)
			{
				_localSpaceStatusLabel.Text = "Zoom into a local subsector before building local space.";
			}

			SetLocalSpaceDialogEnabled(false);
			return;
		}

		Vector3I extent = ReadLocalSpaceExtentInputs();
		_localSpacePreviewData = SubSectorNeighborhood.Build(
			GetActiveViewPosition(),
			GalaxySeed,
			_galaxy.DensityModel,
			_galaxy.ReferenceDensity,
			extent);

		int width = (extent.X * 2) + 1;
		int height = (extent.Y * 2) + 1;
		int depth = (extent.Z * 2) + 1;
		int subsectorCount = SubSectorNeighborhood.GetTotalSubsectors(extent);
		_localSpaceAreaLabel.Text = $"Coverage: {width} x {height} x {depth} subsectors ({subsectorCount} total)";
		_localSpaceStarCountLabel.Text = $"Stars in area: {_localSpacePreviewData.GetStarCount()}";
		if (_localSpaceWarningLabel != null)
		{
			_localSpaceWarningLabel.Text = "Larger areas retain more systems but increase cache build time.";
		}

		if (_localSpaceStatusLabel != null)
		{
			if (_localSpaceCache == null)
			{
				_localSpaceStatusLabel.Text = "Build local space to cache system summaries for nearby stars.";
			}
			else if (_localSpaceCache.ContainsPosition(GetActiveViewPosition()))
			{
				_localSpaceStatusLabel.Text = "Build local space again to append any still-uncached nearby systems around this location.";
			}
			else
			{
				_localSpaceStatusLabel.Text = "Build local space here to append this new area to the existing local-space cache.";
			}
		}

		SetLocalSpaceDialogEnabled(true);
	}

	private void SetLocalSpaceDialogEnabled(bool enabled)
	{
		if (_buildLocalSpaceRunButton != null)
		{
			_buildLocalSpaceRunButton.Disabled = !enabled || _isBuildingLocalSpace;
		}
	}

	private Vector3I ReadLocalSpaceExtentInputs()
	{
		int x = ReadExtentValue(_localSpaceExtentXInput);
		int y = ReadExtentValue(_localSpaceExtentYInput);
		int z = ReadExtentValue(_localSpaceExtentZInput);
		return new Vector3I(x, y, z);
	}

	private static int ReadExtentValue(SpinBox? spinBox)
	{
		if (spinBox == null)
		{
			return DefaultLocalSpaceExtent;
		}

		return Mathf.Clamp((int)Mathf.Round((float)spinBox.Value), 0, MaximumLocalSpaceExtent);
	}

	private async void OnBuildLocalSpaceRunPressed()
	{
		if (_galaxy == null)
		{
			SetStatus("Local-space build failed: no galaxy loaded");
			return;
		}

		if (!IsSubsectorActive())
		{
			SetStatus("Build local space is only available in local subsector view");
			return;
		}

		if (_localSpacePreviewData == null)
		{
			RefreshLocalSpacePreview();
		}

		if (_localSpacePreviewData == null)
		{
			SetStatus("Local-space build failed: preview data was unavailable");
			return;
		}

		_isBuildingLocalSpace = true;
		SetLocalSpaceDialogEnabled(false);
		if (_localSpaceStatusLabel != null)
		{
			_localSpaceStatusLabel.Text = "Building local-space cache...";
		}

		try
		{
			Vector3I extent = ReadLocalSpaceExtentInputs();
			string regionId = GalaxyLocalSpaceCache.BuildRegionId(_localSpacePreviewData.CenterOrigin, extent);
			JumpLaneRegion? region = await BuildJumpLaneRegionFromNeighborhoodAsync(_localSpacePreviewData, regionId, "Profiling local space");
			if (region == null)
			{
				SetStatus("Local-space build failed");
				if (_localSpaceStatusLabel != null)
				{
					_localSpaceStatusLabel.Text = "Local-space build failed.";
				}

				return;
			}

			int appendedSystems;
			if (_localSpaceCache == null)
			{
				_localSpaceCache = new GalaxyLocalSpaceCache(
					region,
					_localSpacePreviewData.CenterOrigin,
					extent);
				appendedSystems = _localSpaceCache.Region.GetSystemCount();
			}
			else
			{
				appendedSystems = _localSpaceCache.AppendRegion(
					region,
					_localSpacePreviewData.CenterOrigin,
					extent);
			}

			_jumpLaneRegion = CloneJumpLaneRegion(_localSpaceCache.Region);
			_jumpLaneResult = null;
			_jumpRouteCalculatedRegionIds.Clear();
			UpdateJumpRoutePresentation();

			if (_localSpaceStatusLabel != null)
			{
				_localSpaceStatusLabel.Text = $"Appended {appendedSystems} systems. Cache now covers {_localSpaceCache.StarCount} systems across {_localSpaceCache.CoverageAreaCount} local areas.";
			}

			SetStatus($"Built local space: appended {appendedSystems} systems. Cache now covers {_localSpaceCache.StarCount} systems.");
		}
		finally
		{
			_isBuildingLocalSpace = false;
			SetLocalSpaceDialogEnabled(true);
		}
	}

	private async Task<JumpLaneRegion?> BuildJumpLaneRegionFromNeighborhoodAsync(
		SubSectorNeighborhoodData neighborhoodData,
		string regionId,
		string stageLabel)
	{
		JumpLaneRegion region = new(JumpLaneRegion.RegionScope.Subsector, regionId);
		int starCount = System.Math.Min(neighborhoodData.StarPositions.Length, neighborhoodData.StarSeeds.Length);
		if (_localSpaceStatusLabel != null)
		{
			_localSpaceStatusLabel.Text = $"{stageLabel}: 0/{starCount}";
		}

		for (int index = 0; index < starCount; index += 1)
		{
			long starSeed = neighborhoodData.StarSeeds[index];
			JumpLaneSystem system = CreateJumpLaneSystemForRoutes(neighborhoodData.StarPositions[index], starSeed);
			region.AddSystem(system);

			int completed = index + 1;
			if (_localSpaceStatusLabel != null && (completed == starCount || (completed % JumpRouteProgressBatchSize) == 0))
			{
				_localSpaceStatusLabel.Text = $"{stageLabel}: {completed}/{starCount}";
			}

			if (completed < starCount && (completed % JumpRouteProgressBatchSize) == 0)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}

		return region;
	}

	private GalaxyLocalSpaceCache? GetActiveLocalSpaceCache()
	{
		if (_localSpaceCache == null)
		{
			return null;
		}

		if (!_localSpaceCache.ContainsPosition(GetActiveViewPosition()))
		{
			return null;
		}

		return _localSpaceCache;
	}

	public GalaxyLocalSpaceCache? GetLocalSpaceCache()
	{
		return _localSpaceCache;
	}

	private void ClearLocalSpaceCache()
	{
		_localSpaceCache = null;
		_localSpacePreviewData = null;
	}

	public bool BuildLocalSpaceSynchronouslyForTesting(Vector3I extent)
	{
		if (_galaxy == null)
		{
			return false;
		}

		if (!IsSubsectorActive())
		{
			return false;
		}

		SubSectorNeighborhoodData previewData = SubSectorNeighborhood.Build(
			GetActiveViewPosition(),
			GalaxySeed,
			_galaxy.DensityModel,
			_galaxy.ReferenceDensity,
			extent);
		string regionId = GalaxyLocalSpaceCache.BuildRegionId(previewData.CenterOrigin, extent);
		JumpLaneRegion region = new(JumpLaneRegion.RegionScope.Subsector, regionId);
		int starCount = System.Math.Min(previewData.StarPositions.Length, previewData.StarSeeds.Length);
		for (int index = 0; index < starCount; index += 1)
		{
			region.AddSystem(CreateJumpLaneSystemForRoutes(previewData.StarPositions[index], previewData.StarSeeds[index]));
		}

		if (_localSpaceCache == null)
		{
			_localSpaceCache = new GalaxyLocalSpaceCache(region, previewData.CenterOrigin, extent);
		}
		else
		{
			_localSpaceCache.AppendRegion(region, previewData.CenterOrigin, extent);
		}

		_jumpLaneRegion = CloneJumpLaneRegion(_localSpaceCache.Region);
		_jumpLaneResult = null;
		_jumpRouteCalculatedRegionIds.Clear();
		return true;
	}
}
