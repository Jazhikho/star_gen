namespace StarGen.Concepts.ReligionGenerator
{
    /// <summary>
    /// Input parameters for religion generation. Values match the reference JS dropdown keys.
    /// </summary>
    public sealed class ReligionParams
    {
        public string Subsistence { get; set; } = "agricultural";
        public string SocialOrg { get; set; } = "chiefdom";
        public string Settlement { get; set; } = "permanent_village";
        public string Environment { get; set; } = "temperate_fertile";
        public string ExternalThreat { get; set; } = "moderate";
        public string Isolation { get; set; } = "trade_contact";
        public string PoliticalPower { get; set; } = "intertwined";
        public string WritingSystem { get; set; } = "none";
        public string PriorTraditions { get; set; } = "indigenous_only";
        public string GenderSystem { get; set; } = "patrilineal";
        public string KinshipStructure { get; set; } = "extended_clan";
        public int Seed { get; set; }
    }
}
