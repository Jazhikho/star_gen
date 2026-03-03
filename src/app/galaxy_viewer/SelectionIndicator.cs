using Godot;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Visual indicator rendered at a selected star's position.
/// </summary>
public partial class SelectionIndicator : MeshInstance3D
{
    private const float IndicatorSize = 4.0f;

    private ShaderMaterial? _material;

    /// <summary>
    /// Builds the selection mesh on ready.
    /// </summary>
    public override void _Ready()
    {
        BuildMesh();
        Visible = false;
    }

    /// <summary>
    /// Shows the indicator at a world-space position.
    /// </summary>
    public void ShowAt(Vector3 worldPosition)
    {
        Position = worldPosition;
        Visible = true;
    }

    /// <summary>
    /// GDScript-compatible wrapper for mixed-runtime call paths.
    /// </summary>
    public void show_at(Vector3 worldPosition)
    {
        ShowAt(worldPosition);
    }

    /// <summary>
    /// Hides the indicator.
    /// </summary>
    public void HideIndicator()
    {
        Visible = false;
    }

    /// <summary>
    /// GDScript-compatible wrapper for mixed-runtime call paths.
    /// </summary>
    public void hide_indicator()
    {
        HideIndicator();
    }

    /// <summary>
    /// Returns whether the indicator is currently shown.
    /// </summary>
    public bool IsShown()
    {
        return Visible;
    }

    /// <summary>
    /// GDScript-compatible wrapper for mixed-runtime call paths.
    /// </summary>
    public bool is_shown()
    {
        return IsShown();
    }

    /// <summary>
    /// Builds the billboard quad mesh with the selection-ring shader.
    /// </summary>
    private void BuildMesh()
    {
        QuadMesh quad = new()
        {
            Size = new Vector2(1.0f, 1.0f),
        };

        _material = new ShaderMaterial
        {
            Shader = ResourceLoader.Load<Shader>("res://src/app/galaxy_viewer/shaders/selection_ring.gdshader"),
        };
        quad.Material = _material;
        Mesh = quad;

        Basis scaledBasis = Basis.Identity.Scaled(new Vector3(IndicatorSize, IndicatorSize, IndicatorSize));
        Transform = new Transform3D(scaledBasis, Vector3.Zero);
    }
}
