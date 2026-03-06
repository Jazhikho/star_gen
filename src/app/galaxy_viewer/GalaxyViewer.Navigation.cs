using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Keyboard and mouse input routing, zoom transitions, and home-position initialization for GalaxyViewer.
/// </summary>
public partial class GalaxyViewer
{
	/// <summary>
	/// Handles the keyboard shortcuts used by the current slice.
	/// </summary>
	private void HandleKeyInput(InputEventKey keyEvent)
	{
		if (!keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		int currentLevel = _zoomMachine?.GetCurrentLevel() ?? (int)GalaxyCoordinates.ZoomLevel.Galaxy;

		switch (keyEvent.Keycode)
		{
			case Key.Bracketright:
				TryZoomIn(currentLevel);
				break;
			case Key.Bracketleft:
				_zoomMachine?.ZoomOut();
				ApplyZoomLevel(_zoomMachine?.GetCurrentLevel() ?? (int)GalaxyCoordinates.ZoomLevel.Galaxy);
				break;
			case Key.Enter:
			case Key.KpEnter:
				TryOpenSelectedSystem();
				break;
			case Key.Escape:
				ClearStarSelection();
				break;
		}
	}

	/// <summary>
	/// Handles left-click selection in the current zoom level.
	/// </summary>
	private void HandleMouseClick(InputEventMouseButton mouseButton)
	{
		if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		int currentLevel = _zoomMachine?.GetCurrentLevel() ?? (int)GalaxyCoordinates.ZoomLevel.Galaxy;
		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Quadrant)
		{
			PickQuadrantAt(mouseButton.Position);
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Sector)
		{
			PickSectorAt(mouseButton.Position);
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector)
		{
			PickStarAt(mouseButton.Position);
		}
	}

	/// <summary>
	/// Initializes the viewer at the home position.
	/// </summary>
	private void InitializeAtHome()
	{
		HierarchyCoords hierarchy = HomePosition.GetHomeHierarchy();
		_quadrantCursor!.Position = hierarchy.QuadrantCoords;
		_quadrantSelector!.SetSelection(Variant.CreateFrom(hierarchy.QuadrantCoords));
		_selectedSector = Variant.CreateFrom(hierarchy.SectorLocalCoords);
		_sectorCursor!.Position = hierarchy.SectorLocalCoords;
		_zoomMachine!.SetLevel((int)GalaxyCoordinates.ZoomLevel.Subsector);
		ApplyZoomLevel((int)GalaxyCoordinates.ZoomLevel.Subsector);
		UpdateInspector();

		Vector3 homePosition = HomePosition.GetDefaultPosition();
		double distKpc = homePosition.Length() / 1000.0;
		SetStatus($"Home sector - {distKpc:0.0} kpc from galactic center");
	}

	/// <summary>
	/// Applies the gated zoom-in rules from the GDScript viewer.
	/// </summary>
	private void TryZoomIn(int currentLevel)
	{
		if (_zoomMachine == null)
		{
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Galaxy)
		{
			_zoomMachine.ZoomIn();
			ApplyZoomLevel(_zoomMachine.GetCurrentLevel());
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Quadrant && (_quadrantSelector?.HasSelection() ?? false))
		{
			_zoomMachine.ZoomIn();
			ApplyZoomLevel(_zoomMachine.GetCurrentLevel());
			return;
		}

		if (currentLevel == (int)GalaxyCoordinates.ZoomLevel.Sector && _selectedSector.VariantType == Variant.Type.Vector3I)
		{
			_zoomMachine.ZoomIn();
			ApplyZoomLevel(_zoomMachine.GetCurrentLevel());
		}
	}

	/// <summary>
	/// Applies a zoom-level transition.
	/// </summary>
	private void ApplyZoomLevel(int level)
	{
		_zoomMachine?.SetLevel(level);
		bool quadrantSelected = _quadrantSelector?.HasSelection() ?? false;
		bool sectorSelected = _selectedSector.VariantType == Variant.Type.Vector3I;
		bool subsectorActive = level == (int)GalaxyCoordinates.ZoomLevel.Subsector && quadrantSelected && sectorSelected;

		if (_galaxyRenderer != null)
		{
			_galaxyRenderer.Visible = level == (int)GalaxyCoordinates.ZoomLevel.Galaxy
				|| level == (int)GalaxyCoordinates.ZoomLevel.Quadrant
				|| level == (int)GalaxyCoordinates.ZoomLevel.Sector
				|| level == (int)GalaxyCoordinates.ZoomLevel.Subsector;
			_galaxyRenderer.SetOpacity(level switch
			{
				(int)GalaxyCoordinates.ZoomLevel.Galaxy => 1.0f,
				(int)GalaxyCoordinates.ZoomLevel.Subsector => 0.05f,
				_ => 0.15f,
			});
		}

		if (_quadrantRenderer != null)
		{
			_quadrantRenderer.Visible = level == (int)GalaxyCoordinates.ZoomLevel.Quadrant
				|| level == (int)GalaxyCoordinates.ZoomLevel.Sector;
			if (level == (int)GalaxyCoordinates.ZoomLevel.Quadrant)
			{
				if (!quadrantSelected)
				{
					_quadrantRenderer.SetHighlight(default);
				}
				else if (_quadrantSelector != null)
				{
					_quadrantRenderer.SetHighlight(_quadrantSelector.SelectedCoords);
				}
			}
			else
			{
				_quadrantRenderer.SetHighlight(default(Vector3I));
			}
		}

		if (_sectorRenderer is SectorRenderer sectorRenderer)
		{
			if (level == (int)GalaxyCoordinates.ZoomLevel.Sector && quadrantSelected && _quadrantSelector != null)
			{
				Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
				if (_galaxy != null)
				{
					sectorRenderer.BuildForQuadrant(quadrantCoords, _galaxy.DensityModel);
				}

				sectorRenderer.Visible = true;
				Vector3I highlight;
				if (sectorSelected)
				{
					highlight = (Vector3I)_selectedSector;
				}
				else
				{
					highlight = default(Vector3I);
				}

				sectorRenderer.SetHighlight(highlight);

				if (_orbitCamera != null)
				{
					_orbitCamera.Current = true;
					_orbitCamera.ReconfigureConstraints(
						50.0f,
						5000.0f,
						GalaxyCoordinates.QuadrantToParsecCenter(quadrantCoords));
				}
			}
			else
			{
				sectorRenderer.Visible = false;
			}
		}

		if (_neighborhoodRenderer is NeighborhoodRenderer neighborhoodRenderer && subsectorActive && _quadrantSelector != null)
		{
			Vector3I quadrantCoords = (Vector3I)_quadrantSelector.SelectedCoords;
			Vector3I sectorCoords = (Vector3I)_selectedSector;
			Vector3 sectorCenter = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, sectorCoords)
				+ (Vector3.One * ((float)GalaxyCoordinates.SectorSizePc * 0.5f));

			if (_starCamera != null)
			{
				_starCamera.Configure(sectorCenter);
				_starCamera.Current = true;
				_starCamera.SetProcess(true);
			}

			if (_orbitCamera != null)
			{
				_orbitCamera.Current = false;
			}

			if (_galaxy != null)
			{
				neighborhoodRenderer.BuildNeighborhood(
					sectorCenter,
					GalaxySeed,
					_galaxy.DensityModel,
					_galaxy.ReferenceDensity);
			}

			neighborhoodRenderer.Visible = true;
		}
		else if (_neighborhoodRenderer is Node3D neighborhoodNode)
		{
			neighborhoodNode.Visible = false;
		}

		if (!subsectorActive)
		{
			if (_starCamera != null)
			{
				_starCamera.Current = false;
				_starCamera.SetProcess(false);
			}

			if (_orbitCamera != null)
			{
				_orbitCamera.Current = true;
			}
		}

		UpdateJumpRoutePresentation();

		if (subsectorActive && _selectedStarSeed != 0)
		{
			_selectionIndicator?.ShowAt(_selectedStarPosition);
		}
		else
		{
			_selectionIndicator?.HideIndicator();
		}

		if (_compass != null)
		{
			_compass.Visible = _showCompassCheck?.ButtonPressed == true
				&& (level == (int)GalaxyCoordinates.ZoomLevel.Quadrant
				|| level == (int)GalaxyCoordinates.ZoomLevel.Sector);
		}

		UpdateInspector();
	}
}
