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
        float renderLeft = visibleRect.Position.X;
        float renderTop = visibleRect.Position.Y;
        if (topBar != null)
        {
            Rect2 topRect = topBar.GetGlobalRect();
            renderTop = Mathf.Max(renderTop, topRect.Position.Y + topRect.Size.Y);
        }

        if (sidePanel != null)
        {
            Rect2 sideRect = sidePanel.GetGlobalRect();
            renderLeft = Mathf.Max(renderLeft, sideRect.Position.X + sideRect.Size.X);
        }

        float renderRight = visibleRect.Position.X + visibleRect.Size.X;
        float renderBottom = visibleRect.Position.Y + visibleRect.Size.Y;
        float width = Mathf.Max(1.0f, renderRight - renderLeft);
        float height = Mathf.Max(1.0f, renderBottom - renderTop);
        return new Rect2(new Vector2(renderLeft, renderTop), new Vector2(width, height));
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
