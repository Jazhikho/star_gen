using System.Collections.Generic;
using Godot;
using StarGen.Services.Persistence;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Top-menu setup and handlers for the system viewer.
/// </summary>
public partial class SystemViewer
{
    private const int FileMenuSaveId = 1;
    private const int FileMenuLoadId = 2;
    private const int FileMenuReturnId = 3;
    private const int EditMenuGenerateId = 10;
    private const int EditMenuRerollId = 11;
    private const int ViewMenuShowOrbitsId = 20;
    private const int ViewMenuShowZonesId = 21;
    private const int ViewMenuFocusOriginId = 22;
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
        popup.AddItem("Generate", EditMenuGenerateId);
        popup.AddItem("Re-roll", EditMenuRerollId);
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
        popup.SetItemDisabled(popup.ItemCount - 1, _currentSystem == null);
        popup.AddItem("Load...", FileMenuLoadId);
        popup.AddSeparator();
        popup.AddItem(_backNavigationText, FileMenuReturnId);
    }

    private void RebuildViewMenu(PopupMenu popup)
    {
        popup.Clear();
        popup.AddCheckItem("Show Orbits", ViewMenuShowOrbitsId);
        popup.SetItemChecked(popup.ItemCount - 1, _showOrbitsCheck != null && _showOrbitsCheck.ButtonPressed);
        popup.AddCheckItem("Show Zones", ViewMenuShowZonesId);
        popup.SetItemChecked(popup.ItemCount - 1, _showZonesCheck != null && _showZonesCheck.ButtonPressed);
        popup.AddSeparator();
        popup.AddItem("Focus Origin", ViewMenuFocusOriginId);
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
        }
    }

    private void OnViewMenuIdPressed(long id)
    {
        if (id == ViewMenuShowOrbitsId && _showOrbitsCheck != null)
        {
            bool enabled = !_showOrbitsCheck.ButtonPressed;
            _showOrbitsCheck.ButtonPressed = enabled;
            OnShowOrbitsToggled(enabled);
            return;
        }

        if (id == ViewMenuShowZonesId && _showZonesCheck != null)
        {
            bool enabled = !_showZonesCheck.ButtonPressed;
            _showZonesCheck.ButtonPressed = enabled;
            OnShowZonesToggled(enabled);
            return;
        }

        if (id == ViewMenuFocusOriginId)
        {
            if (_cameraController is SystemCameraController typedCameraController)
            {
                typedCameraController.FocusOnOrigin();
            }
            else
            {
                _cameraController?.Call("focus_on_origin");
            }
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
            SetStatus("Controls: left mouse orbit, right mouse pan, scroll zoom, F focus, T toggle angle");
        }
    }
}
