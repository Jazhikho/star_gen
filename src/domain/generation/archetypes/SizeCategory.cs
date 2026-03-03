namespace StarGen.Domain.Generation.Archetypes;

/// <summary>
/// Size categories for planets and moons.
/// </summary>
public static class SizeCategory
{
    /// <summary>
    /// Size category enumeration from smallest to largest.
    /// </summary>
    public enum Category
    {
        Dwarf,
        SubTerrestrial,
        Terrestrial,
        SuperEarth,
        MiniNeptune,
        NeptuneClass,
        GasGiant,
    }

    /// <summary>
    /// Returns a human-readable category name.
    /// </summary>
    public static string ToStringName(Category category)
    {
        return category switch
        {
            Category.Dwarf => "Dwarf",
            Category.SubTerrestrial => "Sub-Terrestrial",
            Category.Terrestrial => "Terrestrial",
            Category.SuperEarth => "Super-Earth",
            Category.MiniNeptune => "Mini-Neptune",
            Category.NeptuneClass => "Neptune-Class",
            Category.GasGiant => "Gas Giant",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a token into a size category.
    /// </summary>
    public static bool TryParse(string categoryName, out Category category)
    {
        string normalized = categoryName
            .ToLowerInvariant()
            .Replace("-", "_")
            .Replace(" ", "_");

        switch (normalized)
        {
            case "dwarf":
                category = Category.Dwarf;
                return true;
            case "sub_terrestrial":
                category = Category.SubTerrestrial;
                return true;
            case "terrestrial":
                category = Category.Terrestrial;
                return true;
            case "super_earth":
                category = Category.SuperEarth;
                return true;
            case "mini_neptune":
                category = Category.MiniNeptune;
                return true;
            case "neptune_class":
                category = Category.NeptuneClass;
                return true;
            case "gas_giant":
                category = Category.GasGiant;
                return true;
            default:
                category = default;
                return false;
        }
    }

    /// <summary>
    /// Returns whether the category implies a predominantly rocky body.
    /// </summary>
    public static bool IsRocky(Category category)
    {
        return category is Category.Dwarf
            or Category.SubTerrestrial
            or Category.Terrestrial
            or Category.SuperEarth;
    }

    /// <summary>
    /// Returns whether the category implies a predominantly gaseous body.
    /// </summary>
    public static bool IsGaseous(Category category)
    {
        return category is Category.MiniNeptune
            or Category.NeptuneClass
            or Category.GasGiant;
    }

    /// <summary>
    /// Returns the number of defined size categories.
    /// </summary>
    public static int Count() => 7;
}
