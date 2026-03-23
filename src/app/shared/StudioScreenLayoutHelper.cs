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
    public const float CompactBreakpoint = 1040.0f;

    /// <summary>
    /// Applies responsive orientation and panel minimum sizes for a studio layout.
    /// </summary>
    public static void ApplyResponsiveStudioLayout(
        Control? owner,
        BoxContainer? studioRow,
        Control? settingsPanel,
        Control? summaryPanel,
        float compactBreakpoint = CompactBreakpoint)
    {
        ApplyResponsiveStudioOrientation(owner, studioRow, compactBreakpoint);
    }

    /// <summary>
    /// Applies responsive orientation and panel minimum sizes for a three-column studio layout.
    /// </summary>
    public static void ApplyResponsiveStudioLayout(
        Control? owner,
        BoxContainer? studioRow,
        Control? settingsPanel,
        Control? rulesPanel,
        Control? summaryPanel,
        float compactBreakpoint = CompactBreakpoint)
    {
        ApplyResponsiveStudioOrientation(owner, studioRow, compactBreakpoint);
    }

    private static void ApplyResponsiveStudioOrientation(
        Control? owner,
        BoxContainer? studioRow,
        float compactBreakpoint)
    {
        if (owner == null || studioRow == null)
        {
            return;
        }

        Vector2 viewportSize = ResolveAvailableSize(owner);
        float availableWidth = ResolveAvailableWidth(viewportSize.X);
        studioRow.Vertical = availableWidth < compactBreakpoint;
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

    private static float ResolveAvailableWidth(float viewportWidth)
    {
        return Mathf.Max(viewportWidth - 64.0f, 320.0f);
    }
}
