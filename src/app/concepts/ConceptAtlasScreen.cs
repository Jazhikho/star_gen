using System.Collections.Generic;
using Godot;
using StarGen.Domain.Concepts;
using StarGen.Services.Concepts;

namespace StarGen.App.Concepts;

/// <summary>
/// Cross-cutting entry point for concept tools and showcase surfaces.
/// </summary>
public partial class ConceptAtlasScreen : Control
{
    [Signal]
    public delegate void BackRequestedEventHandler();

    private ItemList? _moduleList;
    private SpinBox? _seedInput;
    private LineEdit? _bodyNameInput;
    private SpinBox? _populationInput;
    private HSlider? _habitabilityInput;
    private OptionButton? _biomeOption;
    private Label? _contextLabel;
    private Label? _titleLabel;
    private Label? _subtitleLabel;
    private RichTextLabel? _summaryText;
    private VBoxContainer? _metricsContainer;
    private VBoxContainer? _sectionsContainer;
    private Label? _provenanceLabel;
    private ConceptContextSnapshot _contextSnapshot = new ConceptContextSnapshot();
    private ConceptContextSnapshot _sourceSnapshot = new ConceptContextSnapshot();

    /// <summary>
    /// Initializes the atlas UI.
    /// </summary>
    public override void _Ready()
    {
        BuildUi();
        SetContext(ConceptContextBuilder.CreateDefault(424242), ConceptKind.Ecology);
    }

    /// <summary>
    /// Applies context and optionally selects a concept.
    /// </summary>
    public void SetContext(ConceptContextSnapshot snapshot, ConceptKind initialKind)
    {
        _contextSnapshot = snapshot.Clone();
        _sourceSnapshot = snapshot.Clone();
        SyncControlsFromSnapshot();
        SelectKind(initialKind);
        RefreshDisplay();
    }

    /// <summary>
    /// Returns the most recently applied context snapshot.
    /// </summary>
    public ConceptContextSnapshot GetContextSnapshot()
    {
        return _contextSnapshot.Clone();
    }

    private void BuildUi()
    {
        AnchorRight = 1.0f;
        AnchorBottom = 1.0f;
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;

        ColorRect background = new();
        background.AnchorRight = 1.0f;
        background.AnchorBottom = 1.0f;
        background.Color = new Color(0.02f, 0.05f, 0.08f, 1.0f);
        AddChild(background);

        MarginContainer margin = new();
        margin.AnchorRight = 1.0f;
        margin.AnchorBottom = 1.0f;
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_top", 18);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_bottom", 18);
        AddChild(margin);

        VBoxContainer root = new();
        root.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        root.SizeFlagsVertical = SizeFlags.ExpandFill;
        root.AddThemeConstantOverride("separation", 14);
        margin.AddChild(root);

        HBoxContainer header = new();
        header.AddThemeConstantOverride("separation", 12);
        root.AddChild(header);

        Button backButton = new();
        backButton.Text = "Return";
        backButton.CustomMinimumSize = new Vector2(120.0f, 42.0f);
        backButton.Pressed += () => EmitSignal(SignalName.BackRequested);
        header.AddChild(backButton);

        VBoxContainer headerText = new();
        headerText.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        header.AddChild(headerText);

        Label heading = new();
        heading.Text = "Concept Atlas";
        heading.AddThemeFontSizeOverride("font_size", 28);
        headerText.AddChild(heading);

        Label kicker = new();
        kicker.Text = "Standalone tool in development for realistic, user-adjustable worldbuilding models. Use the atlas to explore deterministic concept runs while deeper simulation integration is still being refined.";
        kicker.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        kicker.CustomMinimumSize = new Vector2(520.0f, 0.0f);
        headerText.AddChild(kicker);

        _contextLabel = new Label();
        _contextLabel.Modulate = new Color(0.75f, 0.82f, 0.9f, 1.0f);
        _contextLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        headerText.AddChild(_contextLabel);

        HSplitContainer split = new();
        split.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        split.SizeFlagsVertical = SizeFlags.ExpandFill;
        split.SplitOffsets = new int[] { 280 };
        root.AddChild(split);

        PanelContainer leftPanel = new();
        leftPanel.CustomMinimumSize = new Vector2(280.0f, 0.0f);
        split.AddChild(leftPanel);

        MarginContainer leftMargin = new();
        leftMargin.AddThemeConstantOverride("margin_left", 14);
        leftMargin.AddThemeConstantOverride("margin_top", 14);
        leftMargin.AddThemeConstantOverride("margin_right", 14);
        leftMargin.AddThemeConstantOverride("margin_bottom", 14);
        leftPanel.AddChild(leftMargin);

        VBoxContainer leftBox = new();
        leftBox.SizeFlagsVertical = SizeFlags.ExpandFill;
        leftBox.AddThemeConstantOverride("separation", 12);

        ScrollContainer leftScroll = new();
        leftScroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        leftScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        leftMargin.AddChild(leftScroll);
        leftScroll.AddChild(leftBox);

        Label modulesTitle = new();
        modulesTitle.Text = "Modules";
        modulesTitle.AddThemeFontSizeOverride("font_size", 18);
        leftBox.AddChild(modulesTitle);

        _moduleList = new ItemList();
        _moduleList.Name = "ModuleList";
        _moduleList.SizeFlagsVertical = SizeFlags.ExpandFill;
        _moduleList.CustomMinimumSize = new Vector2(220.0f, 220.0f);
        _moduleList.ItemSelected += _ => RefreshDisplay();
        leftBox.AddChild(_moduleList);

        Label inputTitle = new();
        inputTitle.Text = "Manual Inputs";
        inputTitle.AddThemeFontSizeOverride("font_size", 18);
        leftBox.AddChild(inputTitle);

        _seedInput = CreateSpinBox(1, 999999, 1, 424242);
        AddLabeledControl(leftBox, "Seed", _seedInput);
        _seedInput.ValueChanged += _ => RefreshDisplay();

        _bodyNameInput = new LineEdit();
        _bodyNameInput.PlaceholderText = "Manual world name";
        _bodyNameInput.TextChanged += _ => RefreshDisplay();
        AddLabeledControl(leftBox, "World / Polity", _bodyNameInput);

        _populationInput = CreateSpinBox(0, 5000000000, 1000, 2500000);
        AddLabeledControl(leftBox, "Population", _populationInput);
        _populationInput.ValueChanged += _ => RefreshDisplay();

        _habitabilityInput = new HSlider();
        _habitabilityInput.MinValue = 0;
        _habitabilityInput.MaxValue = 10;
        _habitabilityInput.Step = 1;
        _habitabilityInput.Value = 5;
        _habitabilityInput.ValueChanged += _ => RefreshDisplay();
        AddLabeledControl(leftBox, "Habitability", _habitabilityInput);

        _biomeOption = new OptionButton();
        string[] biomeOptions = new string[] { "Barren", "Temperate", "Forest", "Grassland", "Desert", "Tundra", "Oceanic" };
        foreach (string biome in biomeOptions)
        {
            _biomeOption.AddItem(biome);
        }

        _biomeOption.ItemSelected += _ => RefreshDisplay();
        AddLabeledControl(leftBox, "Biome", _biomeOption);

        PanelContainer rightPanel = new();
        split.AddChild(rightPanel);

        MarginContainer rightMargin = new();
        rightMargin.AddThemeConstantOverride("margin_left", 18);
        rightMargin.AddThemeConstantOverride("margin_top", 18);
        rightMargin.AddThemeConstantOverride("margin_right", 18);
        rightMargin.AddThemeConstantOverride("margin_bottom", 18);
        rightPanel.AddChild(rightMargin);

        ScrollContainer scroll = new();
        scroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        rightMargin.AddChild(scroll);

        VBoxContainer content = new();
        content.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        content.AddThemeConstantOverride("separation", 12);
        scroll.AddChild(content);

        _titleLabel = new Label();
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        content.AddChild(_titleLabel);

        _subtitleLabel = new Label();
        _subtitleLabel.Modulate = new Color(0.85f, 0.86f, 0.8f, 1.0f);
        _subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        content.AddChild(_subtitleLabel);

        _summaryText = new RichTextLabel();
        _summaryText.Name = "SummaryText";
        _summaryText.FitContent = true;
        _summaryText.BbcodeEnabled = false;
        _summaryText.ScrollActive = false;
        _summaryText.CustomMinimumSize = new Vector2(620.0f, 100.0f);
        content.AddChild(_summaryText);

        Label metricsTitle = new();
        metricsTitle.Text = "Metrics";
        metricsTitle.AddThemeFontSizeOverride("font_size", 18);
        content.AddChild(metricsTitle);

        _metricsContainer = new VBoxContainer();
        _metricsContainer.Name = "MetricsContainer";
        _metricsContainer.AddThemeConstantOverride("separation", 8);
        content.AddChild(_metricsContainer);

        Label sectionsTitle = new();
        sectionsTitle.Text = "Details";
        sectionsTitle.AddThemeFontSizeOverride("font_size", 18);
        content.AddChild(sectionsTitle);

        _sectionsContainer = new VBoxContainer();
        _sectionsContainer.Name = "SectionsContainer";
        _sectionsContainer.AddThemeConstantOverride("separation", 10);
        content.AddChild(_sectionsContainer);

        _provenanceLabel = new Label();
        _provenanceLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _provenanceLabel.Modulate = new Color(0.68f, 0.75f, 0.82f, 1.0f);
        content.AddChild(_provenanceLabel);

        PopulateModuleList();
    }

    private void PopulateModuleList()
    {
        if (_moduleList == null)
        {
            return;
        }

        _moduleList.Clear();
        foreach (ConceptModuleDescriptor descriptor in ConceptAtlasModuleRegistry.GetDescriptors())
        {
            _moduleList.AddItem(descriptor.DisplayName);
            _moduleList.SetItemMetadata(_moduleList.ItemCount - 1, (int)descriptor.Kind);
        }
    }

    private void RefreshDisplay()
    {
        if (_moduleList == null || _moduleList.GetSelectedItems().Length == 0)
        {
            return;
        }

        ConceptKind kind = GetSelectedKind();
        ConceptContextSnapshot snapshot = BuildSnapshotFromControls();
        ConceptRunResult result = ResolveDisplayResult(kind, snapshot);

        if (_contextLabel != null)
        {
            _contextLabel.Text = "Context: " + snapshot.SourceLabel;
        }

        if (_titleLabel != null)
        {
            _titleLabel.Text = result.Title;
        }

        if (_subtitleLabel != null)
        {
            _subtitleLabel.Text = result.Subtitle;
        }

        if (_summaryText != null)
        {
            _summaryText.Text = result.Summary;
        }

        RenderMetrics(result.Metrics);
        RenderSections(result.Sections);

        if (_provenanceLabel != null)
        {
            _provenanceLabel.Text =
                $"Seed {result.Provenance.Seed} | {result.Provenance.GeneratorVersion} | {result.Provenance.SourceContext}";
        }
    }

    private void RenderMetrics(List<ConceptMetric> metrics)
    {
        if (_metricsContainer == null)
        {
            return;
        }

        foreach (Node child in _metricsContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (ConceptMetric metric in metrics)
        {
            VBoxContainer row = new();
            row.AddThemeConstantOverride("separation", 4);

            Label label = new();
            string displayText = string.IsNullOrEmpty(metric.DisplayText)
                ? metric.Value.ToString("0.##")
                : metric.DisplayText;
            label.Text = metric.Label + ": " + displayText;
            row.AddChild(label);

            ProgressBar bar = new();
            bar.MinValue = 0.0;
            bar.MaxValue = metric.MaxValue > 0.0 ? metric.MaxValue : 1.0;
            bar.Value = metric.Value;
            bar.ShowPercentage = false;
            row.AddChild(bar);

            _metricsContainer.AddChild(row);
        }
    }

    private void RenderSections(List<ConceptSection> sections)
    {
        if (_sectionsContainer == null)
        {
            return;
        }

        foreach (Node child in _sectionsContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (ConceptSection section in sections)
        {
            VBoxContainer sectionBox = new();
            sectionBox.AddThemeConstantOverride("separation", 6);

            Label title = new();
            title.Text = section.Title;
            title.AddThemeFontSizeOverride("font_size", 16);
            sectionBox.AddChild(title);

            foreach (string item in section.Items)
            {
                Label line = new();
                line.Text = "- " + item;
                line.AutowrapMode = TextServer.AutowrapMode.WordSmart;
                line.CustomMinimumSize = new Vector2(560.0f, 0.0f);
                sectionBox.AddChild(line);
            }

            _sectionsContainer.AddChild(sectionBox);
        }
    }

    private void SyncControlsFromSnapshot()
    {
        if (_seedInput != null)
        {
            _seedInput.Value = _contextSnapshot.Seed;
        }

        if (_bodyNameInput != null)
        {
            _bodyNameInput.Text = !string.IsNullOrEmpty(_contextSnapshot.BodyName)
                ? _contextSnapshot.BodyName
                : _contextSnapshot.SourceLabel;
        }

        if (_populationInput != null)
        {
            _populationInput.Value = _contextSnapshot.Population;
        }

        if (_habitabilityInput != null)
        {
            _habitabilityInput.Value = _contextSnapshot.HabitabilityScore;
        }

        if (_biomeOption != null)
        {
            int biomeIndex = FindBiomeIndex(_contextSnapshot.DominantBiome);
            _biomeOption.Select(biomeIndex >= 0 ? biomeIndex : 0);
        }
    }

    private void SelectKind(ConceptKind kind)
    {
        if (_moduleList == null)
        {
            return;
        }

        for (int index = 0; index < _moduleList.ItemCount; index += 1)
        {
            Variant metadata = _moduleList.GetItemMetadata(index);
            if (metadata.VariantType == Variant.Type.Int && (int)metadata == (int)kind)
            {
                _moduleList.Select(index);
                return;
            }
        }

        if (_moduleList.ItemCount > 0)
        {
            _moduleList.Select(0);
        }
    }

    private ConceptKind GetSelectedKind()
    {
        if (_moduleList == null || _moduleList.GetSelectedItems().Length == 0)
        {
            return ConceptKind.Ecology;
        }

        int selectedIndex = _moduleList.GetSelectedItems()[0];
        Variant metadata = _moduleList.GetItemMetadata(selectedIndex);
        if (metadata.VariantType == Variant.Type.Int)
        {
            return (ConceptKind)(int)metadata;
        }

        return ConceptKind.Ecology;
    }

    private ConceptContextSnapshot BuildSnapshotFromControls()
    {
        ConceptContextSnapshot snapshot = _sourceSnapshot.Clone();
        if (_seedInput != null)
        {
            snapshot.Seed = (int)_seedInput.Value;
        }

        if (_bodyNameInput != null)
        {
            snapshot.BodyName = _bodyNameInput.Text;
            snapshot.SourceLabel = string.IsNullOrEmpty(_bodyNameInput.Text) ? snapshot.SourceLabel : _bodyNameInput.Text;
        }

        if (_populationInput != null)
        {
            snapshot.Population = (int)_populationInput.Value;
        }

        if (_habitabilityInput != null)
        {
            snapshot.HabitabilityScore = (int)_habitabilityInput.Value;
        }

        if (_biomeOption != null && _biomeOption.Selected >= 0)
        {
            snapshot.DominantBiome = _biomeOption.GetItemText(_biomeOption.Selected);
        }

        return snapshot;
    }

    private ConceptRunResult ResolveDisplayResult(ConceptKind kind, ConceptContextSnapshot snapshot)
    {
        if (!HasManualOverrides(snapshot))
        {
            ConceptRunResult? persisted = snapshot.PersistedResults.Get(kind);
            if (persisted != null)
            {
                return persisted;
            }
        }

        return ConceptAtlasModuleRegistry.Run(new ConceptRunRequest
        {
            Kind = kind,
            Context = snapshot,
        });
    }

    private bool HasManualOverrides(ConceptContextSnapshot snapshot)
    {
        if (snapshot.Seed != _sourceSnapshot.Seed)
        {
            return true;
        }

        if (!string.Equals(snapshot.BodyName, GetControlBaselineBodyName(_sourceSnapshot), System.StringComparison.Ordinal))
        {
            return true;
        }

        if (snapshot.Population != _sourceSnapshot.Population)
        {
            return true;
        }

        if (snapshot.HabitabilityScore != _sourceSnapshot.HabitabilityScore)
        {
            return true;
        }

        if (!string.Equals(snapshot.DominantBiome, _sourceSnapshot.DominantBiome, System.StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static string GetControlBaselineBodyName(ConceptContextSnapshot snapshot)
    {
        if (!string.IsNullOrEmpty(snapshot.BodyName))
        {
            return snapshot.BodyName;
        }

        return snapshot.SourceLabel;
    }

    private static SpinBox CreateSpinBox(double minValue, double maxValue, double step, double value)
    {
        SpinBox spinBox = new();
        spinBox.MinValue = minValue;
        spinBox.MaxValue = maxValue;
        spinBox.Step = step;
        spinBox.Value = value;
        return spinBox;
    }

    private static void AddLabeledControl(Container parent, string labelText, Control control)
    {
        VBoxContainer box = new();
        box.AddThemeConstantOverride("separation", 4);

        Label label = new();
        label.Text = labelText;
        box.AddChild(label);
        box.AddChild(control);

        parent.AddChild(box);
    }

    private int FindBiomeIndex(string biomeName)
    {
        if (_biomeOption == null)
        {
            return -1;
        }

        for (int index = 0; index < _biomeOption.ItemCount; index += 1)
        {
            if (_biomeOption.GetItemText(index).Equals(biomeName))
            {
                return index;
            }
        }

        return -1;
    }
}
