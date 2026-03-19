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

		if (_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel)
		{
			float density = _galaxy?.DensityModel.GetDensity(GalaxyCoordinates.QuadrantToParsecCenter(coords)) ?? 0.0f;
			typedInspectorPanel.DisplaySelectedQuadrant(coords, density);
		}

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

		if (
			_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel &&
			_quadrantSelector != null &&
			_quadrantSelector.HasSelection() &&
			_quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
		{
			Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
			Vector3 sectorCenter = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, coords)
				+ (Vector3.One * ((float)GalaxyCoordinates.SectorSizePc * 0.5f));
			float density = _galaxy?.DensityModel.GetDensity(sectorCenter) ?? 0.0f;
			typedInspectorPanel.DisplaySelectedSector(quadrantCoords, coords, density);
		}

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
			_starPreview = StarSystemPreview.Generate(starSeed, worldPosition, _spec, _galaxyConfig?.UseCaseSettings);
		}
		_selectionIndicator?.ShowAt(worldPosition);
		if (_inspectorPanel is GalaxyInspectorPanel typedInspectorPanel)
		{
			typedInspectorPanel.DisplaySelectedStar(worldPosition, starSeed);
			typedInspectorPanel.DisplaySystemPreview(_starPreview);
		}
		else
		{
			_inspectorPanel?.Call("display_selected_star", worldPosition, starSeed);
			Variant previewVariant;
			if (_starPreview == null)
			{
				previewVariant = default;
			}
			else
			{
				previewVariant = Variant.CreateFrom(_starPreview);
			}
			_inspectorPanel?.Call("display_system_preview", previewVariant);
		}
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
			typedInspectorPanel.DisplayGalaxy(_spec, _zoomMachine.GetCurrentLevel());

			if (_selectedStarSeed != 0)
			{
				typedInspectorPanel.DisplaySelectedStar(_selectedStarPosition, _selectedStarSeed);
				if (_starPreview != null)
				{
					typedInspectorPanel.DisplaySystemPreview(_starPreview);
				}
				return;
			}

			if (
				(_zoomMachine.GetCurrentLevel() == (int)GalaxyCoordinates.ZoomLevel.Sector
				|| _zoomMachine.GetCurrentLevel() == (int)GalaxyCoordinates.ZoomLevel.Subsector) &&
				_selectedSector.VariantType == Variant.Type.Vector3I &&
				_quadrantSelector != null &&
				_quadrantSelector.HasSelection() &&
				_quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
			{
				Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
				Vector3I sectorCoords = (Vector3I)_selectedSector;
				Vector3 sectorCenter = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, sectorCoords)
					+ (Vector3.One * ((float)GalaxyCoordinates.SectorSizePc * 0.5f));
				float density = _galaxy?.DensityModel.GetDensity(sectorCenter) ?? 0.0f;
				typedInspectorPanel.DisplaySelectedSector(quadrantCoords, sectorCoords, density);
				return;
			}

			if (
				_zoomMachine.GetCurrentLevel() == (int)GalaxyCoordinates.ZoomLevel.Quadrant &&
				_quadrantSelector != null &&
				_quadrantSelector.HasSelection() &&
				_quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
			{
				Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
				float density = _galaxy?.DensityModel.GetDensity(GalaxyCoordinates.QuadrantToParsecCenter(quadrantCoords)) ?? 0.0f;
				typedInspectorPanel.DisplaySelectedQuadrant(quadrantCoords, density);
				return;
			}

			typedInspectorPanel.ClearSelection();
			return;
		}

		_inspectorPanel.Call("display_galaxy", _spec, _zoomMachine.GetCurrentLevel());
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
	/// Forwards the inspector's concept-atlas request through the viewer signal.
	/// </summary>
	private void OnInspectorOpenConceptAtlasRequested(int starSeed, Vector3 worldPosition)
	{
		if (starSeed == 0)
		{
			return;
		}

		_selectedStarSeed = starSeed;
		_selectedStarPosition = worldPosition;
		EmitSignal(SignalName.OpenConceptAtlasRequested, _selectedStarSeed, _selectedStarPosition);
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
