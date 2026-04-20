using Godot;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Top-menu setup and handlers for the galaxy viewer.
/// </summary>
public partial class GalaxyViewer
{
	private const int FileMenuNewGalaxyId = 1;
	private const int FileMenuReturnToMainMenuId = 2;
	private const int ToolsMenuBuildLocalSpaceId = 20;
	private const int ToolsMenuCalculateRoutesId = 21;
	private const int HelpMenuControlsId = 40;

	private void SetupTopMenu()
	{
		HBoxContainer? menuRow = GetNodeOrNull<HBoxContainer>("UI/UIRoot/TopBar/MarginContainer/TopBarVBox/MenuRow");
		if (menuRow == null || menuRow.GetChildCount() > 0)
		{
			return;
		}

		ConfigureFileMenu(CreateMenuButton(menuRow, "File"));
		ConfigureToolsMenu(CreateMenuButton(menuRow, "Tools"));
		ConfigureOptionsButton(CreateActionButton(menuRow, "Options"));
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

	private static Button CreateActionButton(HBoxContainer menuRow, string title)
	{
		Button button = new()
		{
			Text = title,
			FocusMode = Control.FocusModeEnum.None,
			Flat = true,
		};
		menuRow.AddChild(button);
		return button;
	}

	private void ConfigureFileMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.IdPressed += OnFileMenuIdPressed;
		popup.AddItem("New Galaxy...", FileMenuNewGalaxyId);
		popup.AddItem("Return to Main Menu", FileMenuReturnToMainMenuId);
	}

	private void ConfigureToolsMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.IdPressed += OnToolsMenuIdPressed;
		popup.AboutToPopup += () => RebuildToolsMenu(popup);
		RebuildToolsMenu(popup);
	}

	private void ConfigureOptionsButton(Button button)
	{
		button.Pressed += OpenOptionsDialog;
	}

	private void ConfigureHelpMenu(MenuButton menuButton)
	{
		PopupMenu popup = menuButton.GetPopup();
		popup.AddItem("Controls Summary", HelpMenuControlsId);
		popup.IdPressed += OnHelpMenuIdPressed;
	}

	private void RebuildToolsMenu(PopupMenu popup)
	{
		popup.Clear();
		popup.AddItem("Build Local Space...", ToolsMenuBuildLocalSpaceId);
		if (!IsSubsectorActive())
		{
			popup.SetItemDisabled(popup.ItemCount - 1, true);
		}

		popup.AddSeparator();
		popup.AddItem("Calculate Jump Routes", ToolsMenuCalculateRoutesId);
		if (GetActiveLocalSpaceCache() == null)
		{
			popup.SetItemDisabled(popup.ItemCount - 1, true);
		}
	}

	private void OnFileMenuIdPressed(long id)
	{
		if (id == FileMenuNewGalaxyId)
		{
			EmitSignal(SignalName.NewGalaxyRequested);
			return;
		}

		if (id == FileMenuReturnToMainMenuId)
		{
			EmitSignal(SignalName.MainMenuRequested);
		}
	}

	private void OnToolsMenuIdPressed(long id)
	{
		if (id == ToolsMenuBuildLocalSpaceId)
		{
			OpenBuildLocalSpaceDialog();
			return;
		}

		if (id == ToolsMenuCalculateRoutesId)
		{
			CalculateJumpRoutesForCurrentSubsector();
		}
	}

	private void OnHelpMenuIdPressed(long id)
	{
		if (id == HelpMenuControlsId)
		{
			SetStatus("Controls: mouse orbit, scroll zoom, click to select, Enter to open a system");
		}
	}
}
