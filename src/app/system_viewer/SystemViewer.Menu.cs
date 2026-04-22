using Godot;
using StarGen.App.Shared;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Top-menu setup and handlers for the system viewer.
/// </summary>
public partial class SystemViewer
{
	private const int FileMenuNewSystemId = 1;
	private const int FileMenuSaveSystemId = 2;
	private const int FileMenuLoadSystemId = 3;
	private const int FileMenuMainMenuId = 4;
	private const int FileMenuReturnId = 5;
	private const int ToolsMenuShowOrbitsId = 20;
	private const int ToolsMenuFocusOriginId = 22;
    private const int HelpMenuControlsId = 40;

    private void SetupTopMenu()
    {
        MenuButton? fileMenuButton = GetNodeOrNull<MenuButton>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow/FileMenuButton");
        MenuButton? toolsMenuButton = GetNodeOrNull<MenuButton>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow/ToolsMenuButton");
        Button? optionsButton = GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow/OptionsButton");
        MenuButton? helpMenuButton = GetNodeOrNull<MenuButton>("UI/TopBar/MarginContainer/TopBarVBox/MenuRow/HelpMenuButton");
        if (fileMenuButton == null || toolsMenuButton == null || optionsButton == null || helpMenuButton == null)
        {
            return;
        }

        ConfigureFileMenu(fileMenuButton);
        ConfigureToolsMenu(toolsMenuButton);
        ConfigureOptionsButton(optionsButton);
        ConfigureHelpMenu(helpMenuButton);
    }

    private void ConfigureFileMenu(MenuButton menuButton)
    {
        PopupMenu popup = menuButton.GetPopup();
        if (popup.ItemCount > 0)
        {
            return;
        }

        popup.IdPressed += OnFileMenuIdPressed;
        popup.AboutToPopup += () => RebuildFileMenu(popup);
        RebuildFileMenu(popup);
    }

    private void ConfigureToolsMenu(MenuButton menuButton)
    {
        PopupMenu popup = menuButton.GetPopup();
        if (popup.ItemCount > 0)
        {
            return;
        }

        popup.IdPressed += OnToolsMenuIdPressed;
        popup.AboutToPopup += () => RebuildToolsMenu(popup);
        RebuildToolsMenu(popup);
    }

    private void ConfigureOptionsButton(Button button)
    {
        if (button.IsConnected(BaseButton.SignalName.Pressed, Callable.From(OpenOptionsDialog)))
        {
            return;
        }

        button.Pressed += OpenOptionsDialog;
    }

    private void ConfigureHelpMenu(MenuButton menuButton)
    {
        PopupMenu popup = menuButton.GetPopup();
        if (popup.ItemCount > 0)
        {
            return;
        }

        popup.AddItem("Controls Summary", HelpMenuControlsId);
        popup.IdPressed += OnHelpMenuIdPressed;
    }

    private void RebuildFileMenu(PopupMenu popup)
    {
        popup.Clear();
        popup.AddItem("New System...", FileMenuNewSystemId);
        if (ReleaseEditionService.CanUseSaveLoad())
        {
            popup.AddSeparator();
            popup.AddItem("Save Current System...", FileMenuSaveSystemId);
            popup.SetItemDisabled(popup.ItemCount - 1, _currentSystem == null);
            popup.AddItem("Load System...", FileMenuLoadSystemId);
        }

        popup.AddSeparator();
        popup.AddItem("Return to Main Menu", FileMenuMainMenuId);
        if (_backNavigationVisible)
        {
            popup.AddSeparator();
            popup.AddItem(_backNavigationText, FileMenuReturnId);
        }
    }

    private void RebuildToolsMenu(PopupMenu popup)
    {
		popup.Clear();
		popup.AddCheckItem("Show Orbits", ToolsMenuShowOrbitsId);
		popup.SetItemChecked(popup.ItemCount - 1, _showOrbitsVisible);
		popup.AddSeparator();
		popup.AddItem("Focus Origin", ToolsMenuFocusOriginId);
    }

    private void OnFileMenuIdPressed(long id)
    {
        if (id == FileMenuNewSystemId)
        {
            EmitSignal(SignalName.NewSystemRequested);
            return;
        }

        if (id == FileMenuMainMenuId)
        {
            EmitSignal(SignalName.MainMenuRequested);
            return;
        }

        if (id == FileMenuSaveSystemId)
        {
            _saveLoad.OnSavePressed(this);
            return;
        }

        if (id == FileMenuLoadSystemId)
        {
            _saveLoad.OnLoadPressed(this);
            return;
        }

        if (id == FileMenuReturnId)
        {
            OnBackPressed();
        }
    }

    private void OnToolsMenuIdPressed(long id)
    {
        if (id == ToolsMenuShowOrbitsId)
        {
            bool enabled = !_showOrbitsVisible;
            OnShowOrbitsToggled(enabled);
            return;
        }

		if (id == ToolsMenuFocusOriginId)
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

    private void OnHelpMenuIdPressed(long id)
    {
        if (id == HelpMenuControlsId)
        {
            SetStatus("Controls: left mouse orbit, right mouse pan, scroll zoom, F focus, T toggle angle");
        }
    }
}
