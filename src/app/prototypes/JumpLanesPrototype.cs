using System;
using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.Prototypes;

/// <summary>
/// Prototype scene for visualizing jump lane calculations.
/// </summary>
public partial class JumpLanesPrototype : Node3D
{
    public enum RegionType
    {
        Simple = 0,
        Random = 1,
        Clustered = 2,
    }

    private OptionButton? _regionOption;
    private SpinBox? _seedInput;
    private Button? _generateButton;
    private Label? _statsLabel;
    private Label? _legendLabel;
    private JumpLaneRenderer? _renderer;
    private Node3D? _cameraPivot;
    private Camera3D? _camera;

    private JumpLaneCalculator? _calculator;
    private JumpLaneRegion? _currentRegion;
    private JumpLaneResult? _currentResult;

    private float _cameraDistance = 40.0f;
    private Vector2 _cameraRotation = new Vector2(-0.5f, 0.0f);
    private bool _isDragging;
    private Vector2 _lastMousePos = Vector2.Zero;

    public override void _Ready()
    {
        _calculator = new JumpLaneCalculator();
        _regionOption = GetNodeOrNull<OptionButton>("UI/Panel/VBox/RegionOption");
        _seedInput = GetNodeOrNull<SpinBox>("UI/Panel/VBox/SeedContainer/SeedInput");
        _generateButton = GetNodeOrNull<Button>("UI/Panel/VBox/GenerateButton");
        _statsLabel = GetNodeOrNull<Label>("UI/Panel/VBox/StatsLabel");
        _legendLabel = GetNodeOrNull<Label>("UI/Panel/VBox/LegendLabel");
        _renderer = GetNodeOrNull<JumpLaneRenderer>("JumpLaneRenderer");
        _cameraPivot = GetNodeOrNull<Node3D>("CameraPivot");
        _camera = GetNodeOrNull<Camera3D>("CameraPivot/Camera3D");

        SetupUi();
        SetupLegend();
        UpdateCamera();
        GenerateAndDisplay();
    }

    private void SetupUi()
    {
        if (_regionOption == null)
            return;
        _regionOption.AddItem("Simple (Hand-crafted)", (int)RegionType.Simple);
        _regionOption.AddItem("Random", (int)RegionType.Random);
        _regionOption.AddItem("Clustered", (int)RegionType.Clustered);
        _regionOption.Selected = 0;

        if (_seedInput != null)
        {
            _seedInput.Value = 12345;
            _seedInput.MinValue = 0;
            _seedInput.MaxValue = 999999;
        }

        if (_generateButton != null)
            _generateButton.Pressed += OnGeneratePressed;
    }

    private void SetupLegend()
    {
        if (_legendLabel == null)
            return;
        _legendLabel.Text = "Legend:\n";
        _legendLabel.Text += "● Blue: Populated system\n";
        _legendLabel.Text += "● Cyan: Bridge system\n";
        _legendLabel.Text += "● Red: Orphan (no connections)\n";
        _legendLabel.Text += "● Gray: Unpopulated\n";
        _legendLabel.Text += "— Green: Direct (≤5 pc)\n";
        _legendLabel.Text += "— Yellow: Bridged\n";
        _legendLabel.Text += "— Orange: Direct (7 pc)";
    }

    private void OnGeneratePressed()
    {
        GenerateAndDisplay();
    }

    private void GenerateAndDisplay()
    {
        int regionType = 0;
        if (_regionOption != null)
        {
            regionType = _regionOption.GetSelectedId();
        }

        int seedValue = 12345;
        if (_seedInput != null)
        {
            seedValue = (int)_seedInput.Value;
        }

        _currentRegion = CreateRegion((RegionType)regionType, seedValue);
        if (_calculator == null)
        {
            UpdateStats();
            return;
        }
        _currentResult = _calculator.Calculate(_currentRegion);

        if (_renderer != null && _currentRegion != null && _currentResult != null)
            _renderer.Render(_currentRegion, _currentResult);
        UpdateStats();
    }

    private JumpLaneRegion CreateRegion(RegionType regionType, int seedValue)
    {
        switch (regionType)
        {
            case RegionType.Simple:
                return MockRegionGenerator.CreateSimpleRegion(seedValue);
            case RegionType.Random:
                return MockRegionGenerator.CreateRandomRegion(seedValue);
            case RegionType.Clustered:
                return MockRegionGenerator.CreateClusteredRegion(seedValue);
            default:
                return MockRegionGenerator.CreateSimpleRegion(seedValue);
        }
    }

    private void UpdateStats()
    {
        if (_statsLabel == null)
            return;
        if (_currentResult == null || _currentRegion == null)
        {
            _statsLabel.Text = "No data";
            return;
        }

        Godot.Collections.Dictionary counts = _currentResult.GetConnectionCounts();
        _statsLabel.Text = $"Systems: {_currentRegion.GetSystemCount()}\n";
        _statsLabel.Text += $"Populated: {_currentRegion.GetPopulatedCount()}\n";
        _statsLabel.Text += $"Connections: {_currentResult.GetTotalConnections()}\n";
        int green = 0;
        if (counts.TryGetValue((int)JumpLaneConnection.ConnectionType.Green, out Variant g))
        {
            green = g.AsInt32();
        }

        int yellow = 0;
        if (counts.TryGetValue((int)JumpLaneConnection.ConnectionType.Yellow, out Variant y))
        {
            yellow = y.AsInt32();
        }

        int orange = 0;
        if (counts.TryGetValue((int)JumpLaneConnection.ConnectionType.Orange, out Variant o))
        {
            orange = o.AsInt32();
        }
        _statsLabel.Text += $"  Green: {green}\n";
        _statsLabel.Text += $"  Yellow: {yellow}\n";
        _statsLabel.Text += $"  Orange: {orange}\n";
        _statsLabel.Text += $"Orphans: {_currentResult.GetTotalOrphans()}";
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
            HandleMouseButton(mouseButton);
        else if (@event is InputEventMouseMotion mouseMotion)
            HandleMouseMotion(mouseMotion);
    }

    private void HandleMouseButton(InputEventMouseButton ev)
    {
        if (ev.ButtonIndex == MouseButton.Right)
        {
            _isDragging = ev.Pressed;
            _lastMousePos = ev.Position;
        }
        else if (ev.ButtonIndex == MouseButton.WheelUp)
        {
            _cameraDistance = Mathf.Max(_cameraDistance - 3.0f, 10.0f);
            UpdateCamera();
        }
        else if (ev.ButtonIndex == MouseButton.WheelDown)
        {
            _cameraDistance = Mathf.Min(_cameraDistance + 3.0f, 100.0f);
            UpdateCamera();
        }
    }

    private void HandleMouseMotion(InputEventMouseMotion ev)
    {
        if (!_isDragging)
            return;
        Vector2 delta = ev.Position - _lastMousePos;
        _lastMousePos = ev.Position;
        _cameraRotation.X -= delta.Y * 0.01f;
        _cameraRotation.Y -= delta.X * 0.01f;
        _cameraRotation.X = Math.Clamp(_cameraRotation.X, (float)(-Math.PI / 2 + 0.1), (float)(Math.PI / 2 - 0.1));
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        if (_cameraPivot != null)
            _cameraPivot.Rotation = new Vector3(_cameraRotation.X, _cameraRotation.Y, 0);
        if (_camera != null)
            _camera.Position = new Vector3(0, 0, _cameraDistance);
    }
}
