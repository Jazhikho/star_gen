using System.Collections.Generic;
using Godot;
using StarGen.Services.Persistence;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Top-menu setup and handlers for the galaxy viewer.
/// </summary>
public partial class GalaxyViewer
{
	private const int FileMenuSaveId = 1;
	private const int FileMenuLoadId = 2;
	private const int FileMenuNewGalaxyId = 3;
	private const int EditMenuApplyConfigId = 10;
	private const int ViewMenuShowCompassId = 20;
	private const int ViewMenuCalculateRoutesId = 21;
	private const int WindowMenuFullscreenId = 30;
	private const int WindowMenuResolutionBaseId = 100;
	private const int HelpMenuControlsId = 40;

	private void SetupTopMenu()
	{
		HBoxContainer? menuRow = GetNodeOrNull<HBoxContainer>("UI/UIRoot/TopBar/MarginContainer/TopBarVBox/MenuRow");
		if (menuRow == null || menuRow.GetChildCount() > 0)
		{
			return;
		}

		ConfigureFileMenu(CreateMenuButton(menuRow, "File"));
		ConfigureEditMenu(CreateMenuButton(menuRow, "Edit"));
		ConfigureViewMenu(CreateMenuButton(menuRow, "View"));
		ConfigureWindowMenu(CreateMenuButton(menuRow, "Window"));
		ConfigureHelpMenu(CreateMenuButton(menuRow, "Help"));
	}

	private static MenuButton CreateMenuButton(HBoxContainer menuRow, string title)
	{
		MenuButton button = new()
		{
			Text = title,
			FocusMode = Control.FocusModeEnum.None,
		};
		menuRow.AddChild(button);
		return button;
	}

	private void ConfigureFileMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.IdPressed += OnFileMenuIdPressed;
		popup.AddItem("Save...", FileMenuSaveId);
		popup.AddItem("Load...", FileMenuLoadId);
		popup.AddSeparator();
		popup.AddItem("New Galaxy...", FileMenuNewGalaxyId);
	}

	private void ConfigureEditMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.IdPressed += OnEditMenuIdPressed;
		popup.AddItem("Apply Current Parameters", EditMenuApplyConfigId);
	}

	private void ConfigureViewMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.IdPressed += OnViewMenuIdPressed;
		popup.AboutToPopup += () => RebuildViewMenu(popup);
		RebuildViewMenu(popup);
	}

	private void ConfigureWindowMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.IdPressed += OnWindowMenuIdPressed;
		popup.AboutToPopup += () => RebuildWindowMenu(popup);
		RebuildWindowMenu(popup);
	}

	private void ConfigureHelpMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.AddItem("Controls Summary", HelpMenuControlsId);
		popup.IdPressed += OnHelpMenuIdPressed;
	}

	private void RebuildViewMenu(PopupMenu popup)
	{
		popup.Clear();
		popup.AddCheckItem("Show Compass", ViewMenuShowCompassId);
		popup.SetItemChecked(popup.ItemCount - 1, _showCompassCheck != null && _showCompassCheck.ButtonPressed);
		popup.AddSeparator();
		popup.AddItem("Calculate Jump Routes", ViewMenuCalculateRoutesId);
	}

	private void RebuildWindowMenu(PopupMenu popup)
	{
		popup.Clear();
		WindowSettingsService.WindowSettingsState currentSettings = WindowSettingsService.CaptureCurrent();
		popup.AddCheckItem("Fullscreen", WindowMenuFullscreenId);
		popup.SetItemChecked(popup.ItemCount - 1, currentSettings.Fullscreen);
		popup.AddSeparator();

		IReadOnlyList<Vector2I> resolutions = WindowSettingsService.GetCommonResolutions();
		for (int index = 0; index < resolutions.Count; index += 1)
		{
			Vector2I resolution = resolutions[index];
			popup.AddCheckItem(
				WindowSettingsService.FormatResolutionLabel(resolution),
				WindowMenuResolutionBaseId + index);
			bool isChecked = !currentSettings.Fullscreen && resolution == currentSettings.Resolution;
			popup.SetItemChecked(popup.ItemCount - 1, isChecked);
			popup.SetItemDisabled(popup.ItemCount - 1, currentSettings.Fullscreen);
		}
	}

	private void OnFileMenuIdPressed(long id)
	{
		if (id == FileMenuSaveId)
		{
			_saveLoad.OnSavePressed(this);
			return;
		}

		if (id == FileMenuLoadId)
		{
			_saveLoad.OnLoadPressed(this);
			return;
		}

		if (id == FileMenuNewGalaxyId)
		{
			EmitSignal(SignalName.NewGalaxyRequested);
		}
	}

	private void OnEditMenuIdPressed(long id)
	{
		if (id == EditMenuApplyConfigId)
		{
			OnApplyGalaxyConfigRequested();
		}
	}

	private void OnViewMenuIdPressed(long id)
	{
		if (id == ViewMenuShowCompassId && _showCompassCheck != null)
		{
			bool visible = !_showCompassCheck.ButtonPressed;
			_showCompassCheck.ButtonPressed = visible;
			OnShowCompassToggled(visible);
			return;
		}

		if (id == ViewMenuCalculateRoutesId)
		{
			CalculateJumpRoutesForCurrentSubsector();
		}
	}

	private void OnWindowMenuIdPressed(long id)
	{
		if (id == WindowMenuFullscreenId)
		{
			WindowSettingsService.WindowSettingsState currentSettings = WindowSettingsService.CaptureCurrent();
			WindowSettingsService.WindowSettingsState updatedSettings =
				new WindowSettingsService.WindowSettingsState(!currentSettings.Fullscreen, currentSettings.Resolution);
			WindowSettingsService.ApplyAndSave(updatedSettings);
			if (updatedSettings.Fullscreen)
			{
				SetStatus("Window menu applied fullscreen mode");
			}
			else
			{
				SetStatus($"Window menu applied {updatedSettings.Resolution.X} x {updatedSettings.Resolution.Y}");
			}

			return;
		}

		if (id < WindowMenuResolutionBaseId)
		{
			return;
		}

		int resolutionIndex = (int)(id - WindowMenuResolutionBaseId);
		IReadOnlyList<Vector2I> resolutions = WindowSettingsService.GetCommonResolutions();
		if (resolutionIndex < 0 || resolutionIndex >= resolutions.Count)
		{
			return;
		}

		Vector2I resolution = resolutions[resolutionIndex];
		WindowSettingsService.ApplyAndSave(new WindowSettingsService.WindowSettingsState(false, resolution));
		SetStatus($"Window menu applied {resolution.X} x {resolution.Y}");
	}

	private void OnHelpMenuIdPressed(long id)
	{
		if (id == HelpMenuControlsId)
		{
			SetStatus("Controls: mouse orbit, scroll zoom, click to select, Enter to open a system");
		}
	}
}
