namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// Ecological role/strategy within a trophic level.
    /// </summary>
    public enum NicheType
    {
        // Producers
        Photosynthesizer,
        Chemosynthesizer,
        Thermosynthesizer,

        // Consumers
        Grazer,
        Browser,
        FilterFeeder,
        ActiveHunter,
        AmbushPredator,
        Scavenger,
        Parasite,
        Omnivore,

        // Decomposers
        Detritivore,
        Saprophyte,
        Reducer
    }
}
