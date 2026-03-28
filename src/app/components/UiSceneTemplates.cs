using Godot;

namespace StarGen.App.Components;

/// <summary>
/// Centralized access to reusable scene-authored UI fragments.
/// </summary>
public static class UiSceneTemplates
{
	private const string UiSectionScenePath = "res://src/app/components/UiSection.tscn";
	private const string UiPropertyRowScenePath = "res://src/app/components/UiPropertyRow.tscn";
	private const string UiMessageLabelScenePath = "res://src/app/components/UiMessageLabel.tscn";
	private const string UiActionButtonScenePath = "res://src/app/components/UiActionButton.tscn";
	private const string UiLabeledInputRowScenePath = "res://src/app/components/UiLabeledInputRow.tscn";
	private const string EditNumericEditorRowScenePath = "res://src/app/components/EditNumericEditorRow.tscn";
	private const string EditTravellerConstraintRowScenePath = "res://src/app/components/EditTravellerConstraintRow.tscn";
	private const string EditSectionScenePath = "res://src/app/components/EditSection.tscn";
	private const string EditDerivedPropertyRowScenePath = "res://src/app/components/EditDerivedPropertyRow.tscn";
	private const string EditValidationMessageLabelScenePath = "res://src/app/components/EditValidationMessageLabel.tscn";
	private const string UiSubheaderLabelScenePath = "res://src/app/components/UiSubheaderLabel.tscn";
	private const string UiDividerScenePath = "res://src/app/components/UiDivider.tscn";

	private static readonly PackedScene UiSectionScene = LoadScene(UiSectionScenePath);
	private static readonly PackedScene UiPropertyRowScene = LoadScene(UiPropertyRowScenePath);
	private static readonly PackedScene UiMessageLabelScene = LoadScene(UiMessageLabelScenePath);
	private static readonly PackedScene UiActionButtonScene = LoadScene(UiActionButtonScenePath);
	private static readonly PackedScene UiLabeledInputRowScene = LoadScene(UiLabeledInputRowScenePath);
	private static readonly PackedScene EditNumericEditorRowScene = LoadScene(EditNumericEditorRowScenePath);
	private static readonly PackedScene EditTravellerConstraintRowScene = LoadScene(EditTravellerConstraintRowScenePath);
	private static readonly PackedScene EditSectionScene = LoadScene(EditSectionScenePath);
	private static readonly PackedScene EditDerivedPropertyRowScene = LoadScene(EditDerivedPropertyRowScenePath);
	private static readonly PackedScene EditValidationMessageLabelScene = LoadScene(EditValidationMessageLabelScenePath);
	private static readonly PackedScene UiSubheaderLabelScene = LoadScene(UiSubheaderLabelScenePath);
	private static readonly PackedScene UiDividerScene = LoadScene(UiDividerScenePath);

	public static VBoxContainer InstantiateSection()
	{
		return InstantiateRequired<VBoxContainer>(UiSectionScene, UiSectionScenePath);
	}

	public static HBoxContainer InstantiatePropertyRow()
	{
		return InstantiateRequired<HBoxContainer>(UiPropertyRowScene, UiPropertyRowScenePath);
	}

	public static Label InstantiateMessageLabel()
	{
		return InstantiateRequired<Label>(UiMessageLabelScene, UiMessageLabelScenePath);
	}

	public static Button InstantiateActionButton()
	{
		return InstantiateRequired<Button>(UiActionButtonScene, UiActionButtonScenePath);
	}

	public static HBoxContainer InstantiateLabeledInputRow()
	{
		return InstantiateRequired<HBoxContainer>(UiLabeledInputRowScene, UiLabeledInputRowScenePath);
	}

	public static VBoxContainer InstantiateNumericEditorRow()
	{
		return InstantiateRequired<VBoxContainer>(EditNumericEditorRowScene, EditNumericEditorRowScenePath);
	}

	public static VBoxContainer InstantiateTravellerConstraintRow()
	{
		return InstantiateRequired<VBoxContainer>(EditTravellerConstraintRowScene, EditTravellerConstraintRowScenePath);
	}

	public static VBoxContainer InstantiateEditSection()
	{
		return InstantiateRequired<VBoxContainer>(EditSectionScene, EditSectionScenePath);
	}

	public static HBoxContainer InstantiateEditDerivedPropertyRow()
	{
		return InstantiateRequired<HBoxContainer>(EditDerivedPropertyRowScene, EditDerivedPropertyRowScenePath);
	}

	public static Label InstantiateEditValidationMessageLabel()
	{
		return InstantiateRequired<Label>(EditValidationMessageLabelScene, EditValidationMessageLabelScenePath);
	}

	public static Label InstantiateSubheaderLabel()
	{
		return InstantiateRequired<Label>(UiSubheaderLabelScene, UiSubheaderLabelScenePath);
	}

	public static HSeparator InstantiateDivider()
	{
		return InstantiateRequired<HSeparator>(UiDividerScene, UiDividerScenePath);
	}

	public static T GetRequiredChild<T>(Node parent, string childName) where T : Node
	{
		T? child = parent.GetNodeOrNull<T>(childName);
		if (child == null)
		{
			throw new System.InvalidOperationException($"UI template node '{parent.Name}' is missing child '{childName}'.");
		}

		return child;
	}

	private static PackedScene LoadScene(string path)
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(path);
		if (scene == null)
		{
			throw new System.InvalidOperationException($"Failed to load UI template scene '{path}'.");
		}

		return scene;
	}

	private static T InstantiateRequired<T>(PackedScene scene, string path) where T : Node
	{
		T? instance = scene.Instantiate() as T;
		if (instance == null)
		{
			throw new System.InvalidOperationException($"UI template scene '{path}' did not instantiate as {typeof(T).Name}.");
		}

		return instance;
	}
}
