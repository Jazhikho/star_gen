using System.Collections.Generic;
using System.Text;
using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Population.StationDesign.Classification;

namespace StarGen.Services.Export;

/// <summary>
/// Exports a station design as a Traveller-style stat block.
/// </summary>
public static class StationStatBlockExporter
{
	/// <summary>
	/// Produces a formatted plain-text stat block.
	/// </summary>
	public static string Export(string stationName, DesignResult design, ClassificationReport? report)
	{
		StringBuilder builder = new();
		string separator = new('=', 50);
		string subSeparator = new('-', 50);

		builder.AppendLine(separator);
		builder.AppendLine($"STATION: {stationName}");
		builder.AppendLine(separator);

		(string templateName, int _, int _, string _) = ComponentCatalog.Templates[design.Spec.Template];
		builder.AppendLine($"Design Template: {templateName}");

		if (report != null && report.Earned.Count > 0)
		{
			List<string> names = new();
			foreach (ClassificationId id in report.Earned)
			{
				names.Add(report.Results[id].DisplayName);
			}

			builder.AppendLine($"Classifications: {string.Join(", ", names)}");
		}

		builder.AppendLine($"Configuration: {ComponentCatalog.HullConfigurations[design.Spec.Configuration].DisplayName}");
		builder.AppendLine($"Hull: {design.Spec.HullTons:N0}t | Structure HP: {design.StructureHitPoints} | Hardpoints: {design.Hardpoints}");
		builder.AppendLine($"Armor: {ComponentCatalog.ArmorMaterials[design.Selection.ArmorMaterial].DisplayName}, {design.EffectiveArmorPoints} effective points");

		builder.AppendLine(subSeparator);
		builder.AppendLine("ENGINEERING");
		builder.AppendLine($"  Power Plant: {ComponentCatalog.PowerPlants[design.Selection.PowerPlant].DisplayName}, Rating {design.Selection.PowerRating} ({design.Tonnage.PowerPlant:N0}t)");
		builder.AppendLine($"  Fuel: {design.Tonnage.Fuel:N0}t ({design.Selection.FuelMonths} months)");
		string surplusSign = string.Empty;
		if (design.Power.Surplus >= 0)
		{
			surplusSign = "+";
		}

		builder.AppendLine($"  Power: {design.Power.Demand}/{design.Power.Output} PP ({surplusSign}{design.Power.Surplus})");

		builder.AppendLine(subSeparator);
		builder.AppendLine("COMMAND & CONTROL");
		builder.AppendLine($"  Command: {design.Selection.CommandCenter} ({design.Tonnage.Command:N0}t)");
		if (ComponentCatalog.ComputerNames.TryGetValue(design.Selection.ComputerRating, out string? computerName))
		{
			builder.AppendLine($"  Computer: {computerName}");
		}

		builder.AppendLine($"  Sensors: {ComponentCatalog.Sensors[design.Selection.Sensors].DisplayName}");

		List<string> softwareNames = new();
		foreach (SoftwarePackage software in design.Selection.Software)
		{
			softwareNames.Add(ComponentCatalog.Software[software].DisplayName);
		}

		builder.AppendLine($"  Software: {string.Join(", ", softwareNames)}");

		builder.AppendLine(subSeparator);
		AppendCounts(builder, "TURRETS & BARBETTES", design.Selection.Turrets, key => ComponentCatalog.Turrets[key].DisplayName);
		AppendCounts(builder, "BAY WEAPONS", design.Selection.Bays, key => ComponentCatalog.Bays[key].DisplayName);
		AppendCounts(builder, "SCREENS", design.Selection.Screens, key => ComponentCatalog.Screens[key].DisplayName);

		builder.AppendLine(subSeparator);
		AppendDocking(builder, design.Selection.Docking);

		builder.AppendLine(subSeparator);
		AppendAccommodations(builder, design.Selection.Accommodations, design.BerthsAvailable);

		builder.AppendLine(subSeparator);
		AppendFacilities(builder, design.Selection.Facilities);

		builder.AppendLine(subSeparator);
		builder.AppendLine("TONNAGE BREAKDOWN");
		AppendTonnageLine(builder, "Armor", design.Tonnage.Armor);
		AppendTonnageLine(builder, "Command", design.Tonnage.Command);
		AppendTonnageLine(builder, "Sensors", design.Tonnage.Sensors);
		AppendTonnageLine(builder, "Power Plant", design.Tonnage.PowerPlant);
		AppendTonnageLine(builder, "Fuel", design.Tonnage.Fuel);
		AppendTonnageLine(builder, "Weapons", design.Tonnage.Weapons);
		AppendTonnageLine(builder, "Screens", design.Tonnage.Screens);
		AppendTonnageLine(builder, "Docking", design.Tonnage.Docking);
		AppendTonnageLine(builder, "Quarters", design.Tonnage.Quarters);
		AppendTonnageLine(builder, "Facilities", design.Tonnage.Facilities);
		builder.AppendLine($"  Cargo: {design.Tonnage.Cargo:N0}t");
		builder.AppendLine($"  USED: {design.Tonnage.Used:N0}t / {design.Spec.HullTons:N0}t");

		builder.AppendLine(subSeparator);
		CrewBreakdown crew = design.Crew;
		builder.AppendLine($"CREW: {crew.Total}");
		builder.AppendLine($"  Cmd:{crew.Command} Eng:{crew.Engineering} Gun:{crew.Gunnery} Dock:{crew.Docking} Mnt:{crew.Maintenance} Med:{crew.Medical} Sec:{crew.Security} Fac:{crew.Facilities} Adm:{crew.Admin}");

		builder.AppendLine(subSeparator);
		builder.AppendLine($"TOTAL COST: {FormatCredits(design.Cost.Total)}");
		builder.AppendLine(separator);

		return builder.ToString();
	}

	private static void AppendCounts<TKey>(
		StringBuilder builder,
		string header,
		IReadOnlyDictionary<TKey, int> counts,
		System.Func<TKey, string> getDisplayName)
		where TKey : notnull
	{
		bool any = false;
		foreach (KeyValuePair<TKey, int> entry in counts)
		{
			if (entry.Value > 0)
			{
				if (!any)
				{
					builder.AppendLine(header);
					any = true;
				}

				builder.AppendLine($"  {getDisplayName(entry.Key)}: {entry.Value}");
			}
		}
	}

	private static void AppendDocking(StringBuilder builder, IReadOnlyDictionary<DockingBerthKind, int> docking)
	{
		bool any = false;
		foreach (KeyValuePair<DockingBerthKind, int> entry in docking)
		{
			if (entry.Value > 0)
			{
				if (!any)
				{
					builder.AppendLine("DOCKING FACILITIES");
					any = true;
				}

				builder.AppendLine($"  {ComponentCatalog.Docking[entry.Key].DisplayName}: {entry.Value}");
			}
		}
	}

	private static void AppendAccommodations(
		StringBuilder builder,
		IReadOnlyDictionary<AccommodationKind, int> accommodations,
		int berths)
	{
		bool any = false;
		foreach (KeyValuePair<AccommodationKind, int> entry in accommodations)
		{
			if (entry.Value > 0)
			{
				if (!any)
				{
					builder.AppendLine("ACCOMMODATIONS");
					any = true;
				}

				builder.AppendLine($"  {ComponentCatalog.Accommodations[entry.Key].DisplayName}: {entry.Value}");
			}
		}

		builder.AppendLine($"  Berths: {berths}");
	}

	private static void AppendFacilities(StringBuilder builder, IReadOnlyDictionary<FacilityKind, int> facilities)
	{
		bool any = false;
		foreach (KeyValuePair<FacilityKind, int> entry in facilities)
		{
			if (entry.Value > 0)
			{
				if (!any)
				{
					builder.AppendLine("FACILITIES");
					any = true;
				}

				builder.AppendLine($"  {ComponentCatalog.Facilities[entry.Key].DisplayName}: {entry.Value}");
			}
		}
	}

	private static void AppendTonnageLine(StringBuilder builder, string label, int tons)
	{
		if (tons > 0)
		{
			builder.AppendLine($"  {label}: {tons:N0}t");
		}
	}

	private static string FormatCredits(long credits)
	{
		if (credits >= 1_000_000_000)
		{
			return $"Cr {credits / 1_000_000_000.0:F2}B";
		}

		if (credits >= 1_000_000)
		{
			return $"Cr {credits / 1_000_000.0:F2}M";
		}

		if (credits >= 1_000)
		{
			return $"Cr {credits / 1_000.0:F1}K";
		}

		return $"Cr {credits}";
	}
}
