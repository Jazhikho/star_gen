using System;
using System.Collections.Generic;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Temporary local-space cache built around the active subsector view.
/// </summary>
public sealed class GalaxyLocalSpaceCache
{
	private readonly List<CoverageArea> _coverageAreas = new();

	/// <summary>
	/// Creates a new local-space cache profile.
	/// </summary>
	public GalaxyLocalSpaceCache(
		JumpLaneRegion region,
		Vector3 centerOrigin,
		Vector3I extent)
	{
		Region = region ?? throw new ArgumentNullException(nameof(region));
		CenterOrigin = centerOrigin;
		Extent = extent;
		AppendCoverageArea(centerOrigin, extent);
		RefreshRegionId();
	}

	/// <summary>
	/// Cached local-space region.
	/// </summary>
	public JumpLaneRegion Region { get; }

	/// <summary>
	/// Center subsector origin used for the build.
	/// </summary>
	public Vector3 CenterOrigin { get; private set; }

	/// <summary>
	/// Inclusive subsector extents from the center origin.
	/// </summary>
	public Vector3I Extent { get; private set; }

	/// <summary>
	/// Total star count captured in the cache.
	/// </summary>
	public int StarCount => Region.GetSystemCount();

	/// <summary>
	/// Number of distinct cached local-space coverage areas.
	/// </summary>
	public int CoverageAreaCount => _coverageAreas.Count;

	/// <summary>
	/// Returns whether a world position lies inside the cached local-space bounds.
	/// </summary>
	public bool ContainsPosition(Vector3 worldPosition)
	{
		foreach (CoverageArea coverageArea in _coverageAreas)
		{
			if (coverageArea.ContainsPosition(worldPosition))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Appends any uncached systems from another built local-space region and records its coverage.
	/// </summary>
	public int AppendRegion(JumpLaneRegion region, Vector3 centerOrigin, Vector3I extent)
	{
		if (region == null)
		{
			throw new ArgumentNullException(nameof(region));
		}

		int appendedSystems = 0;
		foreach (JumpLaneSystem system in region.Systems)
		{
			if (Region.GetSystem(system.Id) != null)
			{
				continue;
			}

			Region.AddSystem(JumpLaneSystem.FromDictionary(system.ToDictionary()));
			appendedSystems += 1;
		}

		CenterOrigin = centerOrigin;
		Extent = extent;
		AppendCoverageArea(centerOrigin, extent);
		RefreshRegionId();
		return appendedSystems;
	}

	/// <summary>
	/// Returns a stable cache region identifier for the supplied center and extents.
	/// </summary>
	public static string BuildRegionId(Vector3 centerOrigin, Vector3I extent)
	{
		return FormattableString.Invariant(
			$"local:{centerOrigin.X:0.###},{centerOrigin.Y:0.###},{centerOrigin.Z:0.###}:{extent.X},{extent.Y},{extent.Z}");
	}

	private void AppendCoverageArea(Vector3 centerOrigin, Vector3I extent)
	{
		foreach (CoverageArea coverageArea in _coverageAreas)
		{
			if (coverageArea.CenterOrigin.IsEqualApprox(centerOrigin) && coverageArea.Extent == extent)
			{
				return;
			}
		}

		_coverageAreas.Add(new CoverageArea(centerOrigin, extent));
	}

	private void RefreshRegionId()
	{
		Region.RegionId = FormattableString.Invariant(
			$"local-cache:{CoverageAreaCount}:{Region.GetSystemCount()}");
	}

	private static int ToSubsectorOffset(float delta)
	{
		return (int)Math.Round(delta / (float)GalaxyCoordinates.SubsectorSizePc);
	}

	private readonly struct CoverageArea
	{
		public CoverageArea(Vector3 centerOrigin, Vector3I extent)
		{
			CenterOrigin = centerOrigin;
			Extent = extent;
		}

		public Vector3 CenterOrigin { get; }

		public Vector3I Extent { get; }

		public bool ContainsPosition(Vector3 worldPosition)
		{
			Vector3 subsectorOrigin = GalaxyCoordinates.GetSubsectorWorldOrigin(worldPosition);
			Vector3 delta = subsectorOrigin - CenterOrigin;
			int dx = ToSubsectorOffset(delta.X);
			int dy = ToSubsectorOffset(delta.Y);
			int dz = ToSubsectorOffset(delta.Z);
			return Math.Abs(dx) <= Extent.X
				&& Math.Abs(dy) <= Extent.Y
				&& Math.Abs(dz) <= Extent.Z;
		}
	}
}
