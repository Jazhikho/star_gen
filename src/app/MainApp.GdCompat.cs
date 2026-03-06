using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Systems;

namespace StarGen.App;

/// <summary>
/// GDScript-compatible wrappers for MainApp navigation and generation hooks.
/// </summary>
public partial class MainApp
{
	/// <summary>
	/// GDScript-compatible system-open wrapper.
	/// </summary>
	public void _on_open_system_requested(int starSeed, Vector3 worldPosition)
	{
		OnOpenSystemRequested(starSeed, worldPosition);
	}

	/// <summary>
	/// GDScript-compatible object-viewer open wrapper.
	/// </summary>
	public void _on_open_in_object_viewer(Variant bodyVariant)
	{
		GodotObject? bodyObject = bodyVariant.AsGodotObject();
		if (bodyObject == null)
		{
			return;
		}

		Array moons = [];
		OnOpenInObjectViewer(bodyObject, moons, _currentStarSeed);
	}

	/// <summary>
	/// GDScript-compatible back-to-system wrapper.
	/// </summary>
	public void _on_back_to_system()
	{
		OnBackToSystem();
	}

	/// <summary>
	/// GDScript-compatible back-to-galaxy wrapper.
	/// </summary>
	public void _on_back_to_galaxy()
	{
		OnBackToGalaxy();
	}

	/// <summary>
	/// GDScript-compatible system-generation wrapper.
	/// </summary>
	public SolarSystem? _generate_system_from_seed(int starSeed)
	{
		return GenerateSystemFromSeed(starSeed);
	}

	/// <summary>
	/// Returns the current object viewer instance for test compatibility.
	/// </summary>
	public StarGen.App.Viewer.ObjectViewer? get_object_viewer()
	{
		return _objectViewer;
	}
}
