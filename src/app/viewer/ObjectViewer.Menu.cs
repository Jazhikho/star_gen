using System.Collections.Generic;
using Godot;
using StarGen.Services.Persistence;

namespace StarGen.App.Viewer;

/// <summary>
/// Top-menu setup and handlers for the object viewer.
/// </summary>
public partial class ObjectViewer
{
	private const int FileMenuSaveId = 1;
	private const int FileMenuLoadId = 2;
	private const int FileMenuReturnId = 3;
	private const int EditMenuGenerateId = 10;
	private const int EditMenuRerollId = 11;
	private const int EditMenuEditBodyId = 12;
	private const int ViewMenuFitId = 20;
	private const int ViewMenuFocusPrimaryId = 21;
	private const int WindowMenuFullscreenId = 30;
	private const int WindowMenuResolutionBaseId = 100;
	private const int HelpMenuControlsId = 40;

	private void SetupTopMenu()
	{
		HBoxContainer? menuRow = GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow");
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
		popup.AboutToPopup += () => RebuildFileMenu(popup);
		RebuildFileMenu(popup);
	}

	private void ConfigureEditMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.IdPressed += OnEditMenuIdPressed;
		popup.AboutToPopup += () => RebuildEditMenu(popup);
		RebuildEditMenu(popup);
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

	private void RebuildFileMenu(PopupMenu popup)
	{
		popup.Clear();
		popup.AddItem("Save...", FileMenuSaveId);
		popup.SetItemDisabled(popup.ItemCount - 1, GetCurrentSaveTargetBody() == null);
		popup.AddItem("Load...", FileMenuLoadId);
		popup.AddSeparator();
		popup.AddItem(GetReturnMenuText(), FileMenuReturnId);
		popup.SetItemDisabled(popup.ItemCount - 1, !_backNavigationVisible);
	}

	private void RebuildEditMenu(PopupMenu popup)
	{
		bool hasBody = GetCurrentTargetBody() != null;
		popup.Clear();
		popup.AddItem("Generate", EditMenuGenerateId);
		popup.AddItem("Re-roll", EditMenuRerollId);
		popup.AddSeparator();
		popup.AddItem("Edit Current Object...", EditMenuEditBodyId);
		popup.SetItemDisabled(popup.ItemCount - 1, !hasBody);
	}

	private void RebuildViewMenu(PopupMenu popup)
	{
		bool hasBody = _currentBody != null;
		popup.Clear();
		popup.AddItem("Fit View", ViewMenuFitId);
		popup.SetItemDisabled(popup.ItemCount - 1, !hasBody);
		popup.AddItem("Focus Primary Body", ViewMenuFocusPrimaryId);
		popup.SetItemDisabled(popup.ItemCount - 1, !hasBody);
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

	private string GetReturnMenuText()
	{
		if (_backNavigationVisible && !string.IsNullOrWhiteSpace(_backNavigationText))
		{
			return _backNavigationText;
		}

		return "Return";
	}

	private void OnFileMenuIdPressed(long id)
	{
		if (id == FileMenuSaveId)
		{
			OnSavePressed();
			return;
		}

		if (id == FileMenuLoadId)
		{
			OnLoadPressed();
			return;
		}

		if (id == FileMenuReturnId)
		{
			OnBackPressed();
		}
	}

	private void OnEditMenuIdPressed(long id)
	{
		if (id == EditMenuGenerateId)
		{
			OnGeneratePressed();
			return;
		}

		if (id == EditMenuRerollId)
		{
			OnRerollPressed();
			return;
		}

		if (id == EditMenuEditBodyId)
		{
			OnInspectorEditRequested();
		}
	}

	private void OnViewMenuIdPressed(long id)
	{
		if (id == ViewMenuFitId)
		{
			FitCamera();
			return;
		}

		if (id == ViewMenuFocusPrimaryId)
		{
			FocusOnPlanet();
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
			SetStatus("Controls: left mouse orbit, right mouse pan, scroll zoom, F focus");
		}
	}
}
