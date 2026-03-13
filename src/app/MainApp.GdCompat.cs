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
	/// GDScript-compatible main-menu galaxy-generation wrapper.
	/// </summary>
	public void _on_main_menu_galaxy_generation_requested()
	{
		OnMainMenuGalaxyGenerationRequested();
	}

	/// <summary>
	/// GDScript-compatible return-to-main-menu wrapper from the galaxy viewer.
	/// </summary>
	public void _on_galaxy_viewer_main_menu_requested()
	{
		OnGalaxyViewerMainMenuRequested();
	}

	/// <summary>
	/// GDScript-compatible main-menu system-generation wrapper.
	/// </summary>
	public void _on_main_menu_system_generation_requested()
	{
		OnMainMenuSystemGenerationRequested();
	}

	/// <summary>
	/// GDScript-compatible main-menu object-generation wrapper.
	/// </summary>
	public void _on_main_menu_object_generation_requested()
	{
		OnMainMenuObjectGenerationRequested();
	}

	/// <summary>
	/// GDScript-compatible main-menu station-generation wrapper.
	/// </summary>
	public void _on_main_menu_station_generation_requested()
	{
		OnMainMenuStationGenerationRequested();
	}

	/// <summary>
	/// Returns the current object viewer instance for test compatibility.
	/// </summary>
	public StarGen.App.Viewer.ObjectViewer? get_object_viewer()
	{
		return _objectViewer;
	}

	/// <summary>
	/// Returns the current galaxy-generation screen for test compatibility.
	/// </summary>
	public WelcomeScreen? get_galaxy_generation_screen()
	{
		return _welcomeScreen;
	}

	/// <summary>
	/// Returns the current system-generation screen for test compatibility.
	/// </summary>
	public SystemGenerationScreen? get_system_generation_screen()
	{
		return _systemGenerationScreen;
	}

	/// <summary>
	/// Returns the current object-generation screen for test compatibility.
	/// </summary>
	public ObjectGenerationScreen? get_object_generation_screen()
	{
		return _objectGenerationScreen;
	}

	/// <summary>
	/// Returns the current station-studio screen for test compatibility.
	/// </summary>
	public StationStudioScreen? get_station_studio_screen()
	{
		return _stationStudioScreen;
	}
}
