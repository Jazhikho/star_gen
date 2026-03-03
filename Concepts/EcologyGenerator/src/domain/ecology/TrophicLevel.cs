namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// Position in the food chain energy hierarchy.
    /// </summary>
    public enum TrophicLevel
    {
        Producer = 0,
        PrimaryConsumer = 1,
        SecondaryConsumer = 2,
        TertiaryConsumer = 3,
        ApexPredator = 4,
        Decomposer = 5
    }
}
