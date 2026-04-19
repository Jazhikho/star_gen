using Godot;

namespace StarGen.App.Shared;

/// <summary>
/// Keeps modal help dialogs within the current viewport so close controls stay reachable.
/// </summary>
public static class HelpDialogLayoutHelper
{
    /// <summary>
    /// Sizes and centers a help dialog within the current viewport.
    /// </summary>
    public static void Prepare(Window window, int preferredWidth = 680, int preferredHeight = 500)
    {
        Vector2I viewportSize = ResolveViewportSize(window);

        int width = System.Math.Min(preferredWidth, viewportSize.X - 96);
        int height = System.Math.Min(preferredHeight, viewportSize.Y - 96);

        width = System.Math.Max(width, 420);
        height = System.Math.Max(height, 340);

        window.Size = new Vector2I(width, height);
        window.Position = new Vector2I(
            System.Math.Max((viewportSize.X - width) / 2, 24),
            System.Math.Max((viewportSize.Y - height) / 2, 24));
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
