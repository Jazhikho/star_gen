using Godot;

namespace StarGen.App.Components;

/// <summary>
/// A collapsible UI section with a header button that toggles content visibility.
/// Used for organizing settings into expandable groups.
/// </summary>
public partial class CollapsibleSection : VBoxContainer
{
    /// <summary>Emitted when the section is expanded or collapsed.</summary>
    [Signal]
    public delegate void ToggledEventHandler(bool isExpanded);

    private const string ArrowCollapsed = "▶";
    private const string ArrowExpanded = "▼";

    private Button? _headerButton;
    private VBoxContainer? _contentContainer;
    private bool _isExpanded = true;

    /// <summary>The title text displayed in the header.</summary>
    [Export]
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            UpdateHeaderText();
        }
    }

    private string _title = "Section";

    /// <summary>Whether the section starts expanded.</summary>
    [Export]
    public bool StartExpanded { get; set; } = true;

    public override void _Ready()
    {
        _isExpanded = StartExpanded;
        SetupStructure();
        UpdateHeaderText();
        UpdateContentVisibility();
    }

    /// <summary>Returns whether the section is currently expanded.</summary>
    public bool IsExpanded()
    {
        return _isExpanded;
    }

    /// <summary>Expands the section to show content.</summary>
    public void Expand()
    {
        if (!_isExpanded)
        {
            _isExpanded = true;
            UpdateHeaderText();
            UpdateContentVisibility();
            EmitSignal(SignalName.Toggled, true);
        }
    }

    /// <summary>Collapses the section to hide content.</summary>
    public void Collapse()
    {
        if (_isExpanded)
        {
            _isExpanded = false;
            UpdateHeaderText();
            UpdateContentVisibility();
            EmitSignal(SignalName.Toggled, false);
        }
    }

    /// <summary>Sets the expanded state.</summary>
    public void SetExpanded(bool expanded)
    {
        if (expanded)
        {
            Expand();
        }
        else
        {
            Collapse();
        }
    }

    /// <summary>Returns the content container for adding child controls.</summary>
    public VBoxContainer? GetContentContainer()
    {
        return _contentContainer;
    }

    /// <summary>Adds a control to the content area.</summary>
    public void AddContent(Control control)
    {
        if (_contentContainer != null)
        {
            _contentContainer.AddChild(control);
        }
    }

    private void SetupStructure()
    {
        _headerButton = new Button
        {
            Name = "HeaderButton",
            Flat = false,
            Alignment = HorizontalAlignment.Left,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        _headerButton.Pressed += OnHeaderPressed;
        AddChild(_headerButton);
        MoveChild(_headerButton, 0);

        _contentContainer = new VBoxContainer
        {
            Name = "ContentContainer",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };

        MarginContainer margin = new MarginContainer
        {
            Name = "ContentMargin",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        margin.AddThemeConstantOverride("margin_left", 16);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_bottom", 4);
        margin.AddChild(_contentContainer);
        AddChild(margin);

        MigrateExistingChildren();
    }

    private void MigrateExistingChildren()
    {
        System.Collections.Generic.List<Node> toMove = new System.Collections.Generic.List<Node>();
        foreach (Node child in GetChildren())
        {
            if (child != _headerButton && child.Name != "ContentMargin")
            {
                toMove.Add(child);
            }
        }

        foreach (Node child in toMove)
        {
            RemoveChild(child);
            if (_contentContainer != null)
            {
                _contentContainer.AddChild(child);
            }
        }
    }

    private void UpdateHeaderText()
    {
        if (_headerButton == null)
        {
            return;
        }

        string arrow;
        if (_isExpanded)
        {
            arrow = ArrowExpanded;
        }
        else
        {
            arrow = ArrowCollapsed;
        }
        _headerButton.Text = $"{arrow}  {Title}";
    }

    private void UpdateContentVisibility()
    {
        Node? marginNode = GetNodeOrNull("ContentMargin");
        if (marginNode is Control margin)
        {
            margin.Visible = _isExpanded;
        }
    }

    private void OnHeaderPressed()
    {
        SetExpanded(!_isExpanded);
    }
}
