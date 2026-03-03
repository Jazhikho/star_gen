using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Governance and ownership types for small stations.
/// </summary>
public static class OutpostAuthority
{
    /// <summary>
    /// Authority types for outposts.
    /// </summary>
    public enum Type
    {
        Corporate,
        Military,
        Independent,
        Franchise,
        Cooperative,
        Automated,
        Government,
        Religious,
    }

    /// <summary>
    /// Converts an authority type to a display string.
    /// </summary>
    public static string ToStringName(Type authority)
    {
        return authority switch
        {
            Type.Corporate => "Corporate",
            Type.Military => "Military",
            Type.Independent => "Independent",
            Type.Franchise => "Franchise",
            Type.Cooperative => "Cooperative",
            Type.Automated => "Automated",
            Type.Government => "Government",
            Type.Religious => "Religious",
            _ => "Unknown",
        };
    }

    /// <summary>
    /// Parses an authority type from a string.
    /// </summary>
    public static Type FromString(string name)
    {
        return name.ToLowerInvariant().Trim() switch
        {
            "corporate" => Type.Corporate,
            "military" => Type.Military,
            "independent" => Type.Independent,
            "franchise" => Type.Franchise,
            "cooperative" => Type.Cooperative,
            "automated" => Type.Automated,
            "government" => Type.Government,
            "religious" => Type.Religious,
            _ => Type.Independent,
        };
    }

    /// <summary>
    /// Returns the typical commander title for an authority type.
    /// </summary>
    public static string TypicalCommanderTitle(Type authority)
    {
        return authority switch
        {
            Type.Corporate => "Station Manager",
            Type.Military => "Base Commander",
            Type.Independent => "Station Chief",
            Type.Franchise => "Franchise Manager",
            Type.Cooperative => "Station Coordinator",
            Type.Automated => "System Administrator",
            Type.Government => "Station Director",
            Type.Religious => "Station Prior",
            _ => "Station Chief",
        };
    }

    /// <summary>
    /// Returns whether the authority type typically has a parent organization.
    /// </summary>
    public static bool HasParentOrganization(Type authority)
    {
        return authority == Type.Corporate
            || authority == Type.Military
            || authority == Type.Franchise
            || authority == Type.Government
            || authority == Type.Religious;
    }

    /// <summary>
    /// Returns common authority types for utility stations.
    /// </summary>
    public static Array<Type> TypicalForUtility()
    {
        return new Array<Type> { Type.Corporate, Type.Franchise, Type.Independent };
    }

    /// <summary>
    /// Returns common authority types for outposts.
    /// </summary>
    public static Array<Type> TypicalForOutpost()
    {
        return new Array<Type> { Type.Corporate, Type.Military, Type.Government, Type.Independent };
    }

    /// <summary>
    /// Returns the number of authority types.
    /// </summary>
    public static int Count()
    {
        return 8;
    }
}
