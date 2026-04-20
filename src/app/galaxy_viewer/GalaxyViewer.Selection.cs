using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Quadrant, sector, and star pick-and-select logic, inspector updates, and config conversion for GalaxyViewer.
/// </summary>
public partial class GalaxyViewer
{
	/// <summary>
	/// Picks a quadrant from the current orbit-camera ray.
	/// </summary>
	private void PickQuadrantAt(Vector2 screenPosition)
	{
		if (_orbitCamera == null || _quadrantSelector == null || _quadrantRenderer == null)
		{
			return;
		}

		Vector3 rayOrigin = _orbitCamera.ProjectRayOrigin(screenPosition);
		Vector3 rayDirection = _orbitCamera.ProjectRayNormal(screenPosition);
		Variant picked = _quadrantSelector.PickFromRay(rayOrigin, rayDirection, _quadrantRenderer.get_occupied_coords());
		if (picked.VariantType == Variant.Type.Vector3I)
		{
			SelectQuadrant((Vector3I)picked);
		}
	}

	/// <summary>
	/// Stores and displays the current quadrant selection.
	/// </summary>
	private void SelectQuadrant(Vector3I coords)
	{
		if (_quadrantCursor != null)
		{
			_quadrantCursor.Position = coords;
		}

		_quadrantSelector?.SetSelection(Variant.CreateFrom(coords));
		_quadrantRenderer?.SetHighlight(Variant.CreateFrom(coords));
		_selectedSector = default;

		if (_sectorRenderer is SectorRenderer sectorRenderer)
		{
			sectorRenderer.SetHighlight(default);
		}

		UpdateInspector();
		SetStatus($"Selected quadrant ({coords.X}, {coords.Y}, {coords.Z})");
	}

	/// <summary>
	/// Picks a sector from the current orbit-camera ray.
	/// </summary>
	private void PickSectorAt(Vector2 screenPosition)
	{
		if (_orbitCamera == null || _sectorRenderer is not SectorRenderer sectorRenderer)
		{
			return;
		}

		Godot.Collections.Array<Vector3I> occupied = sectorRenderer.get_occupied_coords();
		if (occupied.Count == 0)
		{
			return;
		}

		Vector3 rayOrigin = _orbitCamera.ProjectRayOrigin(screenPosition);
		Vector3 rayDirection = _orbitCamera.ProjectRayNormal(screenPosition);
		Vector3I? bestCoords = null;
		float bestDistance = float.PositiveInfinity;

		foreach (Vector3I coords in occupied)
		{
			Godot.Collections.Array<Vector3> aabb = sectorRenderer.get_sector_world_aabb(coords);
			float hitDistance = RaycastUtils.RayIntersectsAabb(rayOrigin, rayDirection, aabb[0], aabb[1]);
			if (hitDistance >= 0.0f && hitDistance < bestDistance)
			{
				bestDistance = hitDistance;
				bestCoords = coords;
			}
		}

		if (bestCoords.HasValue)
		{
			SelectSector(bestCoords.Value);
		}
	}

	/// <summary>
	/// Stores and displays the current sector selection.
	/// </summary>
	private void SelectSector(Vector3I coords)
	{
		if (_sectorCursor != null)
		{
			_sectorCursor.Position = coords;
		}

		_selectedSector = Variant.CreateFrom(coords);
		if (_sectorRenderer is SectorRenderer sectorRenderer)
		{
			sectorRenderer.SetHighlight(_selectedSector);
		}

		UpdateInspector();
		SetStatus($"Selected sector ({coords.X}, {coords.Y}, {coords.Z})");
	}

	/// <summary>
	/// Picks a star from the current star-camera ray.
	/// </summary>
	private void PickStarAt(Vector2 screenPosition)
	{
		if (_starCamera == null || _neighborhoodRenderer is not NeighborhoodRenderer neighborhoodRenderer)
		{
			return;
		}

		Vector3 rayOrigin = _starCamera.ProjectRayOrigin(screenPosition);
		Vector3 rayDirection = _starCamera.ProjectRayNormal(screenPosition);
		Variant picked = neighborhoodRenderer.pick_star(rayOrigin, rayDirection);
		GodotObject? pickedObject = picked.AsGodotObject();
		if (pickedObject is StarPickResult result)
		{
			ApplyStarSelection(result.WorldPosition, (int)result.StarSeed);
			return;
		}

		ClearStarSelection();
	}

	/// <summary>
	/// Applies a star selection and updates the inspector.
	/// </summary>
	private void ApplyStarSelection(Vector3 worldPosition, int starSeed)
	{
		_selectedStarPosition = worldPosition;
		_selectedStarSeed = starSeed;
		if (_spec == null)
		{
			_starPreview = null;
		}
		else
		{
			_starPreview = StarSystemPreview.Generate(starSeed, worldPosition, _spec, _galaxyConfig?.UseCaseSettings, _galaxy);
		}
		_selectionIndicator?.ShowAt(worldPosition);
		UpdateInspector();
	}

	/// <summary>
	/// Refreshes the inspector state.
	/// </summary>
	private void UpdateInspector()
	{
		if (_inspectorPanel == null || _spec == null || _zoomMachine == null)
		{
			return;
		}

		if (_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel)
		{
			typedInspectorPanel.SetEditableConfig(_galaxyConfig);
			Vector3 displayPosition = GetInspectorDisplayPosition();
			float density = _galaxy?.DensityModel.GetDensity(displayPosition) ?? 0.0f;
			typedInspectorPanel.DisplayOverview(_spec, displayPosition, density);

			if (_selectedStarSeed != 0)
			{
				typedInspectorPanel.DisplaySelectedStar(_selectedStarPosition, _selectedStarSeed);
				typedInspectorPanel.DisplaySystemPreview(_starPreview);
				return;
			}

			typedInspectorPanel.ClearSelection();
			return;
		}

		_inspectorPanel.Call("display_galaxy", _spec, _zoomMachine.GetCurrentLevel());
	}

	/// <summary>
	/// Returns the position that defines the live inspector overview.
	/// </summary>
	private Vector3 GetInspectorDisplayPosition()
	{
		if (_selectedStarSeed != 0)
		{
			return _selectedStarPosition;
		}

		if (_starCamera != null && IsSubsectorActive())
		{
			return _starCamera.GetCurrentPosition();
		}

		if (_orbitCamera != null)
		{
			return _orbitCamera.GetTarget();
		}

		if (
			_selectedSector.VariantType == Variant.Type.Vector3I &&
			_quadrantSelector != null &&
			_quadrantSelector.HasSelection() &&
			_quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
		{
			Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
			Vector3I sectorCoords = (Vector3I)_selectedSector;
			return GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, sectorCoords)
				+ (Vector3.One * ((float)GalaxyCoordinates.SectorSizePc * 0.5f));
		}

		if (_quadrantSelector != null && _quadrantSelector.HasSelection() && _quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
		{
			return GalaxyCoordinates.QuadrantToParsecCenter((Vector3I)_quadrantSelector.SelectedCoords);
		}

		return HomePosition.GetDefaultPosition();
	}

	/// <summary>
	/// Returns to the galaxy-generation studio, which owns galaxy parameter editing.
	/// </summary>
	private void OnApplyGalaxyConfigRequested()
	{
		SetStatus("Returning to Galaxy Generation Studio");
		EmitSignal(SignalName.NewGalaxyRequested);
	}

	/// <summary>
	/// Clears the current star selection.
	/// </summary>
	private void ClearStarSelection()
	{
		_selectedStarSeed = 0;
		_selectedStarPosition = Vector3.Zero;
		_starPreview = null;
		_selectionIndicator?.HideIndicator();
		UpdateInspector();
	}

	/// <summary>
	/// Emits an open-system request when a star is selected.
	/// </summary>
	private void TryOpenSelectedSystem()
	{
		if (_selectedStarSeed != 0)
		{
			EmitSignal(SignalName.OpenSystemRequested, _selectedStarSeed, _selectedStarPosition);
		}
	}

	/// <summary>
	/// Forwards the inspector's open-system request through the viewer signal.
	/// </summary>
	private void OnInspectorOpenSystemRequested(int starSeed, Vector3 worldPosition)
	{
		if (starSeed == 0)
		{
			return;
		}

		_selectedStarSeed = starSeed;
		_selectedStarPosition = worldPosition;
		TryOpenSelectedSystem();
	}

	/// <summary>
	/// Converts a mixed GDScript/C# config payload into a C# galaxy config.
	/// </summary>
	private static GalaxyConfig? ConvertGalaxyConfig(Variant configVariant)
	{
		if (configVariant.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = configVariant.AsGodotObject();
		if (godotObject is GalaxyConfig typedConfig)
		{
			return typedConfig;
		}

		if (godotObject != null && godotObject.HasMethod("to_dict"))
		{
			Variant dictVariant = godotObject.Call("to_dict");
			if (dictVariant.VariantType == Variant.Type.Dictionary)
			{
				return GalaxyConfig.FromDictionary((Godot.Collections.Dictionary)dictVariant);
			}
		}

		return null;
	}
}
