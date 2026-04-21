using Godot;

namespace StarGen.App.Viewer;

/// <summary>
/// Top-menu setup and handlers for the object viewer.
/// </summary>
public partial class ObjectViewer
{
	private const int FileMenuNewObjectId = 1;
	private const int FileMenuMainMenuId = 2;
	private const int FileMenuReturnId = 3;
	private const int ToolsMenuGenerateId = 10;
	private const int ToolsMenuRerollId = 11;
	private const int ToolsMenuEditBodyId = 12;
	private const int ToolsMenuFitId = 20;
	private const int ToolsMenuFocusPrimaryId = 21;
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
		popup.AddItem("New Object...", FileMenuNewObjectId);
		popup.AddItem("Return to Main Menu", FileMenuMainMenuId);
		if (_backNavigationVisible)
		{
			popup.AddSeparator();
			popup.AddItem(GetReturnMenuText(), FileMenuReturnId);
		}
	}

	private void RebuildToolsMenu(PopupMenu popup)
	{
		bool hasBody = GetCurrentTargetBody() != null;
		popup.Clear();
		if (_generationActionsVisible)
		{
			popup.AddItem("Generate", ToolsMenuGenerateId);
			popup.AddItem("Re-roll", ToolsMenuRerollId);
			popup.AddSeparator();
		}

		popup.AddItem("Edit Current Object...", ToolsMenuEditBodyId);
		popup.SetItemDisabled(popup.ItemCount - 1, !hasBody);
		popup.AddSeparator();
		popup.AddItem("Fit View", ToolsMenuFitId);
		popup.SetItemDisabled(popup.ItemCount - 1, !hasBody);
		popup.AddItem("Focus Primary Body", ToolsMenuFocusPrimaryId);
		popup.SetItemDisabled(popup.ItemCount - 1, !hasBody);
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
		if (id == FileMenuNewObjectId)
		{
			EmitSignal(SignalName.NewObjectRequested);
			return;
		}

		if (id == FileMenuMainMenuId)
		{
			EmitSignal(SignalName.BackToMainMenuRequested);
			return;
		}

		if (id == FileMenuReturnId)
		{
			OnBackPressed();
		}
	}

	private void OnToolsMenuIdPressed(long id)
	{
		if (id == ToolsMenuGenerateId)
		{
			if (!_generationActionsVisible)
			{
				return;
			}

			OnGeneratePressed();
			return;
		}

		if (id == ToolsMenuRerollId)
		{
			if (!_generationActionsVisible)
			{
				return;
			}

			OnRerollPressed();
			return;
		}

		if (id == ToolsMenuEditBodyId)
		{
			OnInspectorEditRequested();
			return;
		}

		if (id == ToolsMenuFitId)
		{
			FitCamera();
			return;
		}

		if (id == ToolsMenuFocusPrimaryId)
		{
			FocusOnPlanet();
		}
	}

	private void OnHelpMenuIdPressed(long id)
	{
		if (id == HelpMenuControlsId)
		{
			SetStatus("Controls: left mouse orbit, right mouse pan, scroll zoom, F focus");
		}
	}
}
