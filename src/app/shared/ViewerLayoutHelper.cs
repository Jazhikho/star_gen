using Godot;

namespace StarGen.App.Shared;

/// <summary>
/// Computes the visible 3D render area for viewers that reserve UI chrome at the top and left.
/// </summary>
public static class ViewerLayoutHelper
{
    /// <summary>
    /// Returns the visible render rect after subtracting the top bar and side panel.
    /// </summary>
    public static Rect2 ComputeRenderRect(Viewport? viewport, Control? topBar, Control? sidePanel)
    {
        if (viewport == null)
        {
            return new Rect2();
        }

        Rect2 visibleRect = viewport.GetVisibleRect();
        float topHeight = 0.0f;
        float sideWidth = 0.0f;
        if (topBar != null)
        {
            topHeight = topBar.GetGlobalRect().Size.Y;
        }

        if (sidePanel != null)
        {
            sideWidth = sidePanel.GetGlobalRect().Size.X;
        }

        float width = Mathf.Max(1.0f, visibleRect.Size.X - sideWidth);
        float height = Mathf.Max(1.0f, visibleRect.Size.Y - topHeight);
        return new Rect2(new Vector2(sideWidth, topHeight), new Vector2(width, height));
    }

    /// <summary>
    /// Returns the render-rect center offset relative to the full viewport center in normalized units.
    /// </summary>
    public static Vector2 ComputeNormalizedCenterOffset(Viewport? viewport, Rect2 renderRect)
    {
        if (viewport == null)
        {
            return Vector2.Zero;
        }

        Vector2 viewportSize = viewport.GetVisibleRect().Size;
        if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f)
        {
            return Vector2.Zero;
        }

        Vector2 renderCenter = renderRect.Position + (renderRect.Size * 0.5f);
        Vector2 normalizedCenter = new Vector2(renderCenter.X / viewportSize.X, renderCenter.Y / viewportSize.Y);
        Vector2 viewportCenter = new Vector2(0.5f, 0.5f);
        return (normalizedCenter - viewportCenter) * 2.0f;
    }
}
