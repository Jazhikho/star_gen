using Godot;

namespace StarGen.App.Shared;

/// <summary>
/// Applies responsive layout rules for the pre-launch studio screens.
/// </summary>
public static class StudioScreenLayoutHelper
{
    /// <summary>
    /// Width below which the studio columns should stack vertically.
    /// </summary>
    public const float CompactBreakpoint = 1280.0f;

    /// <summary>
    /// Applies responsive orientation and panel minimum sizes for a studio layout.
    /// </summary>
    public static void ApplyResponsiveStudioLayout(
        Control? owner,
        BoxContainer? studioRow,
        Control? settingsPanel,
        Control? summaryPanel)
    {
        if (owner == null || studioRow == null)
        {
            return;
        }

        Vector2 viewportSize = ResolveAvailableSize(owner);
        bool stackPanels = viewportSize.X < CompactBreakpoint;
        studioRow.Vertical = stackPanels;

        if (settingsPanel != null)
        {
            settingsPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            settingsPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            settingsPanel.CustomMinimumSize = Vector2.Zero;
        }

        if (summaryPanel != null)
        {
            summaryPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            summaryPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            if (stackPanels)
            {
                summaryPanel.CustomMinimumSize = new Vector2(0.0f, 260.0f);
            }
            else
            {
                summaryPanel.CustomMinimumSize = new Vector2(280.0f, 0.0f);
            }
        }
    }

    private static Vector2 ResolveAvailableSize(Control owner)
    {
        if (owner.IsInsideTree())
        {
            Rect2 viewportRect = owner.GetViewportRect();
            if (viewportRect.Size.X > 0.0f && viewportRect.Size.Y > 0.0f)
            {
                return viewportRect.Size;
            }
        }

        if (owner.Size.X > 0.0f && owner.Size.Y > 0.0f)
        {
            return owner.Size;
        }

        if (owner.GetParent() is Control parent && parent.Size.X > 0.0f && parent.Size.Y > 0.0f)
        {
            return parent.Size;
        }

        return new Vector2(1440.0f, 900.0f);
    }
}
