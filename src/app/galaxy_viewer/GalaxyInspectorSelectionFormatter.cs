using System;
using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Formats and derives galaxy-inspector selection details from world-space positions.
/// </summary>
internal static class GalaxyInspectorSelectionFormatter
{
	internal readonly struct SelectionLocationSummary
	{
		public SelectionLocationSummary(
			Vector3I currentQuadrant,
			Vector3I localGrid,
			double azimuthDegrees,
			double inclinationDegrees,
			double distanceFromCorePc)
		{
			CurrentQuadrant = currentQuadrant;
			LocalGrid = localGrid;
			AzimuthDegrees = azimuthDegrees;
			InclinationDegrees = inclinationDegrees;
			DistanceFromCorePc = distanceFromCorePc;
		}

		public Vector3I CurrentQuadrant { get; }

		public Vector3I LocalGrid { get; }

		public double AzimuthDegrees { get; }

		public double InclinationDegrees { get; }

		public double DistanceFromCorePc { get; }
	}

	/// <summary>
	/// Builds the selection summary from the active view position and the selected star-system position.
	/// </summary>
	public static SelectionLocationSummary Build(Vector3 worldPosition, Vector3 referencePosition)
	{
		Vector3I currentQuadrant = GalaxyCoordinates.ParsecToQuadrant(referencePosition);
		Vector3 subsectorOrigin = GalaxyCoordinates.GetSubsectorWorldOrigin(worldPosition);
		Vector3 localOffset = worldPosition - subsectorOrigin;
		Vector3I localGrid = new(
			ClampGridCoordinate(localOffset.X),
			ClampGridCoordinate(localOffset.Y),
			ClampGridCoordinate(localOffset.Z));

		double planarDistance = Math.Sqrt((worldPosition.X * worldPosition.X) + (worldPosition.Z * worldPosition.Z));
		double inclinationDegrees;
		if (planarDistance <= 1.0e-6)
		{
			inclinationDegrees = 0.0;
		}
		else
		{
			inclinationDegrees = Mathf.RadToDeg((float)Math.Atan2(worldPosition.Y, planarDistance));
		}

		return new SelectionLocationSummary(
			currentQuadrant,
			localGrid,
			ComputeAzimuthDegreesFromHome(worldPosition),
			inclinationDegrees,
			planarDistance);
	}

	/// <summary>
	/// Formats a distance-from-core value for display.
	/// </summary>
	public static string FormatDistanceFromCore(double distanceFromCorePc)
	{
		if (distanceFromCorePc >= 1000.0)
		{
			return $"{distanceFromCorePc / 1000.0:0.00} kpc";
		}

		return $"{distanceFromCorePc:0.0} pc";
	}

	private static double ComputeAzimuthDegreesFromHome(Vector3 worldPosition)
	{
		Vector3 home = HomePosition.GetDefaultPosition();
		double homeAngle = Math.Atan2(home.Z, home.X);
		double targetAngle = Math.Atan2(worldPosition.Z, worldPosition.X);
		double angleDegrees = Mathf.RadToDeg((float)(targetAngle - homeAngle));

		while (angleDegrees < 0.0)
		{
			angleDegrees += 360.0;
		}

		while (angleDegrees >= 360.0)
		{
			angleDegrees -= 360.0;
		}

		return angleDegrees;
	}

	private static int ClampGridCoordinate(float value)
	{
		return Math.Clamp((int)Math.Floor(value), 0, 9);
	}
}
