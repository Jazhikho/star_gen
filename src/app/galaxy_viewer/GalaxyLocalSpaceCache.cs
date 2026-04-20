using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Temporary local-space cache built around the active subsector view.
/// </summary>
public sealed class GalaxyLocalSpaceCache
{
	/// <summary>
	/// Creates a new local-space cache profile.
	/// </summary>
	public GalaxyLocalSpaceCache(
		JumpLaneRegion region,
		Vector3 centerOrigin,
		Vector3I extent,
		int starCount)
	{
		Region = region ?? throw new ArgumentNullException(nameof(region));
		CenterOrigin = centerOrigin;
		Extent = extent;
		StarCount = starCount;
	}

	/// <summary>
	/// Cached local-space region.
	/// </summary>
	public JumpLaneRegion Region { get; }

	/// <summary>
	/// Center subsector origin used for the build.
	/// </summary>
	public Vector3 CenterOrigin { get; }

	/// <summary>
	/// Inclusive subsector extents from the center origin.
	/// </summary>
	public Vector3I Extent { get; }

	/// <summary>
	/// Total star count captured in the cache.
	/// </summary>
	public int StarCount { get; }

	/// <summary>
	/// Returns whether a world position lies inside the cached local-space bounds.
	/// </summary>
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

	/// <summary>
	/// Returns a stable cache region identifier for the supplied center and extents.
	/// </summary>
	public static string BuildRegionId(Vector3 centerOrigin, Vector3I extent)
	{
		return FormattableString.Invariant(
			$"local:{centerOrigin.X:0.###},{centerOrigin.Y:0.###},{centerOrigin.Z:0.###}:{extent.X},{extent.Y},{extent.Z}");
	}

	private static int ToSubsectorOffset(float delta)
	{
		return (int)Math.Round(delta / (float)GalaxyCoordinates.SubsectorSizePc);
	}
}
