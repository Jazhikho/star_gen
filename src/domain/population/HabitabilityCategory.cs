namespace StarGen.Domain.Population;

/// <summary>
/// Derived habitability categories for the 0-10 habitability score.
/// </summary>
public static class HabitabilityCategory
{
    /// <summary>
    /// Habitability categories derived from score.
    /// </summary>
    public enum Category
    {
        Impossible,
        Hostile,
        Harsh,
        Marginal,
        Challenging,
        Comfortable,
        Ideal,
    }

    /// <summary>
    /// Converts a habitability score into a category.
    /// </summary>
    public static Category FromScore(int score)
    {
        int clampedScore = System.Math.Clamp(score, 0, 10);
        return clampedScore switch
        {
            0 => Category.Impossible,
            1 or 2 => Category.Hostile,
            3 or 4 => Category.Harsh,
            5 or 6 => Category.Marginal,
            7 => Category.Challenging,
            8 or 9 => Category.Comfortable,
            10 => Category.Ideal,
            _ => Category.Impossible,
        };
    }

    /// <summary>
    /// Converts a category to a display string.
    /// </summary>
    public static string ToStringName(Category category)
    {
        return category switch
        {
            Category.Impossible => "Impossible",
            Category.Hostile => "Hostile",
            Category.Harsh => "Harsh",
            Category.Marginal => "Marginal",
            Category.Challenging => "Challenging",
            Category.Comfortable => "Comfortable",
            Category.Ideal => "Ideal",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses a category from a string.
    /// </summary>
    public static Category FromString(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "impossible" => Category.Impossible,
            "hostile" => Category.Hostile,
            "harsh" => Category.Harsh,
            "marginal" => Category.Marginal,
            "challenging" => Category.Challenging,
            "comfortable" => Category.Comfortable,
            "ideal" => Category.Ideal,
            _ => Category.Impossible,
        };
    }

    /// <summary>
    /// Returns a short description of the category.
    /// </summary>
    public static string GetDescription(Category category)
    {
        return category switch
        {
            Category.Impossible => "Cannot support human life under any circumstances",
            Category.Hostile => "Requires full life support systems at all times",
            Category.Harsh => "Significant infrastructure and protection needed",
            Category.Marginal => "Difficult conditions but long-term habitation possible",
            Category.Challenging => "Requires technological adaptation but livable",
            Category.Comfortable => "Minor environmental challenges only",
            Category.Ideal => "Earth-equivalent or superior conditions",
            _ => "Unknown habitability",
        };
    }

    /// <summary>
    /// Returns whether the category allows unassisted human survival.
    /// </summary>
    public static bool AllowsUnassistedSurvival(Category category)
    {
        return category is Category.Marginal
            or Category.Challenging
            or Category.Comfortable
            or Category.Ideal;
    }

    /// <summary>
    /// Returns the number of habitability categories.
    /// </summary>
    public static int Count() => 7;
}
