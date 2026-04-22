using Godot;

namespace StarGen.App.Shared;

/// <summary>
/// Keeps modal help dialogs within the current viewport so close controls stay reachable.
/// </summary>
public static class HelpDialogLayoutHelper
{
    private const double MaxViewportFraction = 0.70;
    private const int EdgeMargin = 24;
    private const int MinimumWidth = 420;
    private const int MinimumHeight = 340;

    /// <summary>
    /// Sizes and centers a help dialog within the current viewport.
    /// </summary>
    public static void Prepare(Window window, int preferredWidth = 680, int preferredHeight = 500)
    {
        Vector2I viewportSize = ResolveViewportSize(window);
        int maxWidth = ResolveMaximumDimension(viewportSize.X, MinimumWidth);
        int maxHeight = ResolveMaximumDimension(viewportSize.Y, MinimumHeight);
        int width = ResolveDialogDimension(preferredWidth, MinimumWidth, maxWidth);
        int height = ResolveDialogDimension(preferredHeight, MinimumHeight, maxHeight);

        window.Size = new Vector2I(width, height);
        window.Position = new Vector2I(
            System.Math.Max((viewportSize.X - width) / 2, EdgeMargin),
            System.Math.Max((viewportSize.Y - height) / 2, EdgeMargin));
    }

    /// <summary>
    /// Sizes, centers, and opens a help dialog within the current viewport.
    /// </summary>
    public static void Open(Window window, int preferredWidth = 640, int preferredHeight = 460)
    {
        Prepare(window, preferredWidth, preferredHeight);
        window.Visible = true;
    }

    private static int ResolveMaximumDimension(int viewportDimension, int minimumDimension)
    {
        int percentageCap = (int)System.Math.Floor(viewportDimension * MaxViewportFraction);
        int marginCap = System.Math.Max(viewportDimension - (EdgeMargin * 2), 1);
        int maxDimension = System.Math.Min(percentageCap, marginCap);
        if (maxDimension <= 0)
        {
            return minimumDimension;
        }

        return System.Math.Max(maxDimension, System.Math.Min(minimumDimension, viewportDimension));
    }

    private static int ResolveDialogDimension(int preferredDimension, int minimumDimension, int maximumDimension)
    {
        int clampedMinimum = System.Math.Min(minimumDimension, maximumDimension);
        int dimension = System.Math.Min(preferredDimension, maximumDimension);
        if (dimension < clampedMinimum)
        {
            return clampedMinimum;
        }

        return dimension;
    }

    private static Vector2I ResolveViewportSize(Window window)
    {
        Viewport? viewport = window.GetViewport();
        if (viewport != null)
        {
            Rect2 visibleRect = viewport.GetVisibleRect();
            if (visibleRect.Size.X > 0.0f && visibleRect.Size.Y > 0.0f)
            {
                return new Vector2I((int)visibleRect.Size.X, (int)visibleRect.Size.Y);
            }
        }

        if (window.IsInsideTree())
        {
            SceneTree? tree = window.GetTree();
            if (tree?.Root != null && tree.Root.Size.X > 0 && tree.Root.Size.Y > 0)
            {
                return tree.Root.Size;
            }
        }

        Vector2I displaySize = DisplayServer.WindowGetSize();
        if (displaySize.X > 0 && displaySize.Y > 0)
        {
            return displaySize;
        }

        return new Vector2I(1280, 720);
    }
}
