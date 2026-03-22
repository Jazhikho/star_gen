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
        if (owner == null || studioRow == null)
        {
            return;
        }

        Vector2 viewportSize = ResolveAvailableSize(owner);
        float availableWidth = ResolveAvailableWidth(viewportSize.X);
        bool useVerticalLayout = availableWidth < compactBreakpoint;
        studioRow.Vertical = useVerticalLayout;

        if (useVerticalLayout)
        {
            ApplyStackedSizing(settingsPanel, 1.4f);
            ApplyStackedSizing(summaryPanel, 1.0f);
            return;
        }

        ApplyAdaptiveHorizontalSizing(settingsPanel, availableWidth, 0.62f, 320.0f, 420.0f, 1.6f);
        ApplyAdaptiveHorizontalSizing(summaryPanel, availableWidth, 0.38f, 260.0f, 320.0f, 1.0f);
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
        if (owner == null || studioRow == null)
        {
            return;
        }

        Vector2 viewportSize = ResolveAvailableSize(owner);
        float availableWidth = ResolveAvailableWidth(viewportSize.X);
        bool useVerticalLayout = availableWidth < compactBreakpoint;
        studioRow.Vertical = useVerticalLayout;

        if (useVerticalLayout)
        {
            ApplyStackedSizing(settingsPanel, 1.5f);
            ApplyStackedSizing(rulesPanel, 1.15f);
            ApplyStackedSizing(summaryPanel, 1.0f);
            return;
        }

        ApplyAdaptiveHorizontalSizing(settingsPanel, availableWidth, 0.38f, 320.0f, 420.0f, 1.25f);
        ApplyAdaptiveHorizontalSizing(rulesPanel, availableWidth, 0.31f, 280.0f, 360.0f, 1.0f);
        ApplyAdaptiveHorizontalSizing(summaryPanel, availableWidth, 0.31f, 280.0f, 360.0f, 1.0f);
    }

    private static void ApplyAdaptiveHorizontalSizing(
        Control? panel,
        float availableWidth,
        float widthShare,
        float minimumWidth,
        float preferredWidth,
        float stretchRatio)
    {
        if (panel == null)
        {
            return;
        }

        panel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        panel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        panel.SizeFlagsStretchRatio = stretchRatio;
        float adaptiveWidth = availableWidth * widthShare;
        float resolvedWidth = Mathf.Clamp(adaptiveWidth, minimumWidth, preferredWidth);
        panel.CustomMinimumSize = new Vector2(resolvedWidth, 0.0f);
    }

    private static void ApplyStackedSizing(Control? panel, float stretchRatio)
    {
        if (panel == null)
        {
            return;
        }

        panel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        panel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        panel.SizeFlagsStretchRatio = stretchRatio;
        panel.CustomMinimumSize = Vector2.Zero;
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
