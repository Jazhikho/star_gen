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

    private ItemList _moduleList = null!;
    private SpinBox _seedInput = null!;
    private LineEdit _bodyNameInput = null!;
    private SpinBox _populationInput = null!;
    private HSlider _habitabilityInput = null!;
    private OptionButton _biomeOption = null!;
    private Label _contextLabel = null!;
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private RichTextLabel _summaryText = null!;
    private VBoxContainer _metricsContainer = null!;
    private VBoxContainer _sectionsContainer = null!;
    private Label _provenanceLabel = null!;
    private ConceptContextSnapshot _contextSnapshot = new ConceptContextSnapshot();
    private ConceptContextSnapshot _sourceSnapshot = new ConceptContextSnapshot();

    /// <summary>
    /// Initializes the atlas UI.
    /// </summary>
    public override void _Ready()
    {
        CacheNodeReferences();
        ConnectSignals();
        PopulateBiomeOptions();
        PopulateModuleList();
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

    private void CacheNodeReferences()
    {
        _moduleList = FindRequiredNode<ItemList>("MarginContainer/RootVBox/BodySplit/LeftPanel/LeftMargin/LeftScroll/LeftVBox/ModuleList", "ModuleList");
        _seedInput = FindRequiredNode<SpinBox>("MarginContainer/RootVBox/BodySplit/LeftPanel/LeftMargin/LeftScroll/LeftVBox/SeedBox/SeedInput", "SeedInput");
        _bodyNameInput = FindRequiredNode<LineEdit>("MarginContainer/RootVBox/BodySplit/LeftPanel/LeftMargin/LeftScroll/LeftVBox/WorldNameBox/WorldNameInput", "WorldNameInput");
        _populationInput = FindRequiredNode<SpinBox>("MarginContainer/RootVBox/BodySplit/LeftPanel/LeftMargin/LeftScroll/LeftVBox/PopulationBox/PopulationInput", "PopulationInput");
        _habitabilityInput = FindRequiredNode<HSlider>("MarginContainer/RootVBox/BodySplit/LeftPanel/LeftMargin/LeftScroll/LeftVBox/HabitabilityBox/HabitabilityInput", "HabitabilityInput");
        _biomeOption = FindRequiredNode<OptionButton>("MarginContainer/RootVBox/BodySplit/LeftPanel/LeftMargin/LeftScroll/LeftVBox/BiomeBox/BiomeOption", "BiomeOption");
        _contextLabel = FindRequiredNode<Label>("MarginContainer/RootVBox/HeaderRow/HeaderText/ContextLabel", "ContextLabel");
        _titleLabel = FindRequiredNode<Label>("MarginContainer/RootVBox/BodySplit/RightPanel/RightMargin/RightScroll/RightVBox/TitleLabel", "TitleLabel");
        _subtitleLabel = FindRequiredNode<Label>("MarginContainer/RootVBox/BodySplit/RightPanel/RightMargin/RightScroll/RightVBox/SubtitleLabel", "SubtitleLabel");
        _summaryText = FindRequiredNode<RichTextLabel>("MarginContainer/RootVBox/BodySplit/RightPanel/RightMargin/RightScroll/RightVBox/SummaryText", "SummaryText");
        _metricsContainer = FindRequiredNode<VBoxContainer>("MarginContainer/RootVBox/BodySplit/RightPanel/RightMargin/RightScroll/RightVBox/MetricsContainer", "MetricsContainer");
        _sectionsContainer = FindRequiredNode<VBoxContainer>("MarginContainer/RootVBox/BodySplit/RightPanel/RightMargin/RightScroll/RightVBox/SectionsContainer", "SectionsContainer");
        _provenanceLabel = FindRequiredNode<Label>("MarginContainer/RootVBox/BodySplit/RightPanel/RightMargin/RightScroll/RightVBox/ProvenanceLabel", "ProvenanceLabel");
    }

    private void ConnectSignals()
    {
        Button backButton = FindRequiredNode<Button>("MarginContainer/RootVBox/HeaderRow/BackButton", "BackButton");
        backButton.Pressed += OnBackPressed;

        _moduleList!.ItemSelected += OnModuleSelected;
        _seedInput!.ValueChanged += OnManualInputChanged;
        _bodyNameInput!.TextChanged += OnManualTextChanged;
        _populationInput!.ValueChanged += OnManualInputChanged;
        _habitabilityInput!.ValueChanged += OnManualInputChanged;
        _biomeOption!.ItemSelected += OnBiomeSelected;
    }

    private void PopulateBiomeOptions()
    {
        if (_biomeOption.ItemCount > 0)
        {
            return;
        }

        string[] biomeOptions = new string[] { "Barren", "Temperate", "Forest", "Grassland", "Desert", "Tundra", "Oceanic" };
        foreach (string biome in biomeOptions)
        {
            _biomeOption.AddItem(biome);
        }
    }

    private void PopulateModuleList()
    {
        _moduleList.Clear();
        foreach (ConceptModuleDescriptor descriptor in ConceptAtlasModuleRegistry.GetDescriptors())
        {
            _moduleList.AddItem(descriptor.DisplayName);
            _moduleList.SetItemMetadata(_moduleList.ItemCount - 1, (int)descriptor.Kind);
        }
    }

    private void RefreshDisplay()
    {
        if (_moduleList.GetSelectedItems().Length == 0)
        {
            return;
        }

        ConceptKind kind = GetSelectedKind();
        ConceptContextSnapshot snapshot = BuildSnapshotFromControls();
        ConceptRunResult result = ResolveDisplayResult(kind, snapshot);

        _contextLabel.Text = "Context: " + snapshot.SourceLabel;
        _titleLabel.Text = result.Title;
        _subtitleLabel.Text = result.Subtitle;
        _summaryText.Text = result.Summary;

        RenderMetrics(result.Metrics);
        RenderSections(result.Sections);

        _provenanceLabel.Text =
            $"Seed {result.Provenance.Seed} | {result.Provenance.GeneratorVersion} | {result.Provenance.SourceContext}";
    }

    private void RenderMetrics(List<ConceptMetric> metrics)
    {
        foreach (Node child in _metricsContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (ConceptMetric metric in metrics)
        {
            VBoxContainer row = new VBoxContainer();
            row.AddThemeConstantOverride("separation", 4);

            Label label = new Label();
            string displayText = metric.DisplayText;
            if (string.IsNullOrEmpty(displayText))
            {
                displayText = metric.Value.ToString("0.##");
            }

            label.Text = metric.Label + ": " + displayText;
            row.AddChild(label);

            ProgressBar bar = new ProgressBar();
            bar.MinValue = 0.0;
            bar.MaxValue = 1.0;
            if (metric.MaxValue > 0.0)
            {
                bar.MaxValue = metric.MaxValue;
            }

            bar.Value = metric.Value;
            bar.ShowPercentage = false;
            row.AddChild(bar);

            _metricsContainer.AddChild(row);
        }
    }

    private void RenderSections(List<ConceptSection> sections)
    {
        foreach (Node child in _sectionsContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (ConceptSection section in sections)
        {
            VBoxContainer sectionBox = new VBoxContainer();
            sectionBox.AddThemeConstantOverride("separation", 6);

            Label title = new Label();
            title.Text = section.Title;
            title.AddThemeFontSizeOverride("font_size", 16);
            sectionBox.AddChild(title);

            foreach (string item in section.Items)
            {
                Label line = new Label();
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
        _seedInput.Value = _contextSnapshot.Seed;

        if (!string.IsNullOrEmpty(_contextSnapshot.BodyName))
        {
            _bodyNameInput.Text = _contextSnapshot.BodyName;
        }
        else
        {
            _bodyNameInput.Text = _contextSnapshot.SourceLabel;
        }

        _populationInput.Value = _contextSnapshot.Population;
        _habitabilityInput.Value = _contextSnapshot.HabitabilityScore;

        int biomeIndex = FindBiomeIndex(_contextSnapshot.DominantBiome);
        if (biomeIndex >= 0)
        {
            _biomeOption.Select(biomeIndex);
        }
        else
        {
            _biomeOption.Select(0);
        }
    }

    private void SelectKind(ConceptKind kind)
    {
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
        if (_moduleList.GetSelectedItems().Length == 0)
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
        snapshot.Seed = (int)_seedInput.Value;

        snapshot.BodyName = _bodyNameInput.Text;
        if (!string.IsNullOrEmpty(_bodyNameInput.Text))
        {
            snapshot.SourceLabel = _bodyNameInput.Text;
        }

        snapshot.Population = (int)_populationInput.Value;
        snapshot.HabitabilityScore = (int)_habitabilityInput.Value;
        if (_biomeOption.Selected >= 0)
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

    private int FindBiomeIndex(string biomeName)
    {
        for (int index = 0; index < _biomeOption.ItemCount; index += 1)
        {
            if (_biomeOption.GetItemText(index).Equals(biomeName))
            {
                return index;
            }
        }

        return -1;
    }

    private void OnBackPressed()
    {
        EmitSignal(SignalName.BackRequested);
    }

    private void OnModuleSelected(long _index)
    {
        RefreshDisplay();
    }

    private void OnManualInputChanged(double _value)
    {
        RefreshDisplay();
    }

    private void OnManualTextChanged(string _value)
    {
        RefreshDisplay();
    }

    private void OnBiomeSelected(long _index)
    {
        RefreshDisplay();
    }

    private T FindRequiredNode<T>(string path, string nodeName) where T : Node
    {
        T typedNode = GetNodeOrNull<T>(path);
        if (typedNode != null)
        {
            return typedNode;
        }

        Node discoveredNode = FindChild(nodeName, recursive: true, owned: false);
        if (discoveredNode is T discoveredTypedNode)
        {
            return discoveredTypedNode;
        }

        throw new System.InvalidOperationException($"ConceptAtlasScreen scene is missing {nodeName}.");
    }
}
