using System;
using System.Collections.Generic;
using System.Linq;

namespace StarGen.Concepts.ReligionGenerator
{
    /// <summary>
    /// Procedural religion generator. Multi-factor anthropological modeling; deterministic given params and seed.
    /// Base logic from F with E's framing; fixes: Fisher-Yates, weighted sampling, sigmoid-bounded percentages, no dead code.
    /// </summary>
    public static class ReligionGenerator
    {
        public const string GeneratorVersion = "1.0.0";

        /// <summary>
        /// Maps a raw score to a percentage in [0, 100] via sigmoid. No arbitrary clamps; bounded by construction.
        /// </summary>
        public static int SigmoidPct(double score, double scale = 10.0)
        {
            double x = 100.0 / (1.0 + Math.Exp(-score / scale));
            int pct = (int)Math.Round(x);
            if (pct < 0)
            {
                return 0;
            }
            if (pct > 100)
            {
                return 100;
            }
            return pct;
        }

        /// <summary>
        /// Generates a full religion from the given parameters and seed.
        /// </summary>
        public static ReligionResult Generate(ReligionParams p)
        {
            ReligionRng rng = new ReligionRng(p.Seed);

            bool complexSociety = p.SocialOrg == "state" || p.SocialOrg == "empire";
            bool simpleSociety = p.SocialOrg == "band" || p.SocialOrg == "tribe";
            bool mobile = p.Settlement == "nomadic" || p.Settlement == "semi_nomadic";
            bool settled = p.Settlement == "permanent_village" || p.Settlement == "urban_centers";
            bool urban = p.Settlement == "urban_centers";
            bool harshEnv = p.Environment == "arid_scarce" || p.Environment == "harsh_extreme" || p.Environment == "unpredictable";
            bool highThreat = p.ExternalThreat == "high" || p.ExternalThreat == "existential";
            bool literate = p.WritingSystem == "limited" || p.WritingSystem == "widespread";
            bool strongState = complexSociety && (p.PoliticalPower == "intertwined" || p.PoliticalPower == "theocratic");
            bool connected = p.Isolation == "cultural_exchange" || p.Isolation == "cosmopolitan";

            ReligionOption deity = ChooseDeity(p, rng, complexSociety, simpleSociety, highThreat, connected, strongState);
            ReligionOption cosmology = ChooseCosmology(p, rng, deity, harshEnv, highThreat, literate, complexSociety);
            ReligionOption afterlife = ChooseAfterlife(p, rng, complexSociety, simpleSociety, strongState, highThreat, deity, connected, literate);
            SpecialistOption specialist = ChooseSpecialist(p, rng, simpleSociety, complexSociety, literate, harshEnv, highThreat);
            ReligionOption genderRole = ChooseGenderRole(p, rng, specialist, strongState);
            ReligionOption misfortune = ChooseMisfortune(p, rng, deity, simpleSociety, complexSociety, highThreat, harshEnv, connected);
            ReligionOption authority = ChooseAuthority(p, rng, simpleSociety, complexSociety, specialist);

            List<string> rituals = ChooseRituals(p, rng, settled, urban, literate, specialist, deity, highThreat, harshEnv, misfortune);
            List<string> sacredTimes = ChooseSacredTimes(p, rng, deity, settled, strongState, complexSociety, literate, urban, connected);
            List<string> sacredSpaces = ChooseSacredSpaces(p, rng, mobile, settled, urban, strongState, complexSociety, deity);
            List<string> materialCulture = ChooseMaterialCulture(p, rng, settled, complexSociety, literate, mobile, strongState, harshEnv, deity);
            List<string> ethics = ChooseEthics(p, rng, simpleSociety, complexSociety, highThreat, deity, strongState, connected, misfortune, mobile);
            List<string> taboos = ChooseTaboos(p, rng, complexSociety, literate, harshEnv, deity, misfortune);
            List<string> unique = ChooseUnique(p, rng, strongState, specialist, simpleSociety, complexSociety, literate, highThreat, connected, deity, urban);
            List<string> syncNotes = BuildSyncNotes(p);
            ReligionLandscape landscape = BuildLandscape(p, rng, complexSociety, simpleSociety, urban, connected, strongState, literate, harshEnv, deity, highThreat);

            return new ReligionResult
            {
                Deity = deity,
                Cosmology = cosmology,
                Afterlife = afterlife,
                Specialist = specialist,
                GenderRole = genderRole,
                Misfortune = misfortune,
                Authority = authority,
                Rituals = rituals,
                SacredTimes = sacredTimes,
                SacredSpaces = sacredSpaces,
                MaterialCulture = materialCulture,
                Ethics = ethics,
                Taboos = taboos,
                Unique = unique,
                SyncNotes = syncNotes,
                Landscape = landscape,
            };
        }

        private static ReligionOption ChooseDeity(
            ReligionParams p,
            ReligionRng rng,
            bool complexSociety,
            bool simpleSociety,
            bool highThreat,
            bool connected,
            bool strongState)
        {
            var options = new List<ReligionOption>
            {
                new ReligionOption { Id = "animistic", Name = "Animistic", Desc = "Spirits inhabit all things—rocks, rivers, animals, plants" },
                new ReligionOption { Id = "polytheistic_nature", Name = "Nature Polytheism", Desc = "Gods embody natural forces (sun, storm, earth, sea)" },
                new ReligionOption { Id = "polytheistic_specialized", Name = "Specialized Pantheon", Desc = "Gods of domains: war, craft, wisdom, fertility, etc." },
                new ReligionOption { Id = "henotheistic", Name = "Henotheistic", Desc = "Many gods exist; one supreme deity is primary focus" },
                new ReligionOption { Id = "monolatrist", Name = "Monolatrist", Desc = "One god for this people; others have their own gods" },
                new ReligionOption { Id = "monotheistic", Name = "Monotheistic", Desc = "One supreme deity; others subordinate or denied" },
                new ReligionOption { Id = "pantheistic", Name = "Pantheistic", Desc = "Divine is identical with cosmos; all reality is sacred" },
                new ReligionOption { Id = "ancestor_focused", Name = "Ancestor-Focused", Desc = "Ancestors are primary spirits; gods distant or absent" },
            };

            double[] w = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
            if (simpleSociety) { w[0] += 4; w[7] += 3; }
            if (complexSociety) { w[2] += 4; w[3] += 3; w[5] += 2; }
            if (p.KinshipStructure == "lineage_corporate") { w[7] += 4; }
            if (p.KinshipStructure == "extended_clan") { w[7] += 2; w[0] += 2; }
            if (p.Subsistence == "pastoral") { w[4] += 3; }
            if (highThreat && complexSociety) { w[3] += 2; w[5] += 2; }
            if (connected) { w[2] += 2; w[3] += 2; }
            if (p.Isolation == "isolated") { w[0] += 2; w[7] += 2; }
            if (strongState) { w[5] += 3; w[3] += 2; }

            return ReligionRng.WeightedChoice(rng, options, w.ToList());
        }

        private static ReligionOption ChooseCosmology(
            ReligionParams p,
            ReligionRng rng,
            ReligionOption deity,
            bool harshEnv,
            bool highThreat,
            bool literate,
            bool complexSociety)
        {
            var options = new List<ReligionOption>
            {
                new ReligionOption { Id = "primordial_waters", Desc = "World emerged from primordial waters or formless chaos" },
                new ReligionOption { Id = "divine_craft", Desc = "Divine being(s) intentionally crafted the world" },
                new ReligionOption { Id = "cosmic_sacrifice", Desc = "World born from sacrifice or dismemberment of primordial being" },
                new ReligionOption { Id = "cyclical", Desc = "Reality is cyclical—endless creation and destruction" },
                new ReligionOption { Id = "cosmic_egg", Desc = "World hatched from cosmic egg or split from original unity" },
                new ReligionOption { Id = "divine_speech", Desc = "World spoken or thought into existence by divine mind" },
                new ReligionOption { Id = "cosmic_conflict", Desc = "World emerged from conflict between primal forces" },
            };

            double[] w = new double[] { 1, 1, 1, 1, 1, 1, 1 };
            if (p.Environment == "coastal_riverine") { w[0] += 5; }
            if (p.Environment == "arid_scarce") { w[0] += 3; w[1] += 2; }
            if (p.Subsistence == "agricultural") { w[3] += 3; w[2] += 2; }
            if (harshEnv || highThreat) { w[6] += 4; w[2] += 2; }
            if (p.Environment == "unpredictable") { w[3] += 3; w[6] += 2; }
            if (deity.Id == "monotheistic") { w[1] += 4; w[5] += 4; }
            if (deity.Id == "henotheistic") { w[6] += 3; }
            if (literate && complexSociety) { w[5] += 3; }

            return ReligionRng.WeightedChoice(rng, options, w.ToList());
        }

        private static ReligionOption ChooseAfterlife(
            ReligionParams p,
            ReligionRng rng,
            bool complexSociety,
            bool simpleSociety,
            bool strongState,
            bool highThreat,
            ReligionOption deity,
            bool connected,
            bool literate)
        {
            var options = new List<ReligionOption>
            {
                new ReligionOption { Id = "cyclical", Name = "Cyclical Rebirth", Desc = "Souls return in new bodies based on conduct or cosmic cycles" },
                new ReligionOption { Id = "moral_judgment", Name = "Moral Judgment", Desc = "Souls judged; paradise for righteous, punishment for wicked" },
                new ReligionOption { Id = "ancestor_realm", Name = "Ancestor Realm", Desc = "Dead join ancestors, remain connected to living" },
                new ReligionOption { Id = "dissolution", Name = "Dissolution", Desc = "Individual soul merges into cosmic whole or ceases" },
                new ReligionOption { Id = "shadow", Name = "Shadow Existence", Desc = "All souls go to same grey underworld regardless of conduct" },
                new ReligionOption { Id = "conditional", Name = "Conditional Immortality", Desc = "Only heroes, initiates, or elites achieve true afterlife" },
                new ReligionOption { Id = "legacy", Name = "Legacy Focus", Desc = "No afterlife; one lives through descendants and memory" },
            };

            double[] w = new double[] { 1, 1, 1, 1, 1, 1, 1 };
            if (complexSociety) { w[1] += 4; w[5] += 3; }
            if (simpleSociety) { w[2] += 3; w[4] += 2; w[6] += 3; }
            if (p.KinshipStructure == "lineage_corporate") { w[2] += 4; w[6] += 2; }
            if (strongState) { w[1] += 3; }
            if (highThreat) { w[5] += 3; w[1] += 2; }
            if (p.ExternalThreat == "existential") { w[1] += 2; }
            if (deity.Id == "ancestor_focused") { w[2] += 5; }
            if (deity.Id == "pantheistic") { w[3] += 4; w[0] += 2; }
            if (connected && literate) { w[0] += 3; }

            return ReligionRng.WeightedChoice(rng, options, w.ToList());
        }

        private static SpecialistOption ChooseSpecialist(
            ReligionParams p,
            ReligionRng rng,
            bool simpleSociety,
            bool complexSociety,
            bool literate,
            bool harshEnv,
            bool highThreat)
        {
            var options = new List<SpecialistOption>
            {
                new SpecialistOption { Name = "Shamans/Spirit-Workers", Desc = "Enter altered states to contact spirits; called by visions", Access = "Individual calling" },
                new SpecialistOption { Name = "Hereditary Priests", Desc = "Born into priestly lineage; trained in ritual traditions", Access = "Birth into priestly clan" },
                new SpecialistOption { Name = "Temple Priesthood", Desc = "Professional clergy maintaining institutions", Access = "Training and appointment" },
                new SpecialistOption { Name = "Prophet/Charismatic", Desc = "Claim direct divine revelation; often challenge establishment", Access = "Divine calling from margins" },
                new SpecialistOption { Name = "Monastic Orders", Desc = "Ascetic communities withdrawn for spiritual practice", Access = "Voluntary renunciation" },
                new SpecialistOption { Name = "Elder/Household Heads", Desc = "No specialists; family leaders conduct rites", Access = "Age and kinship position" },
                new SpecialistOption { Name = "Secret Societies", Desc = "Initiation-based groups with esoteric knowledge", Access = "Initiation by gender/age" },
                new SpecialistOption { Name = "Divine Ruler/Sacred King", Desc = "Political leader is primary religious figure", Access = "Royal succession" },
            };

            double[] w = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
            if (simpleSociety) { w[0] += 4; w[5] += 4; }
            if (p.SocialOrg == "tribe") { w[6] += 4; }
            if (complexSociety) { w[2] += 4; w[1] += 3; }
            if (p.PoliticalPower == "theocratic") { w[7] += 5; w[2] += 2; }
            if (p.PoliticalPower == "separate") { w[2] += 2; w[3] += 2; }
            if (literate) { w[2] += 3; w[4] += 3; }
            if (p.KinshipStructure == "lineage_corporate") { w[1] += 4; w[5] += 2; }
            if (p.KinshipStructure == "fictive_kin") { w[6] += 3; w[4] += 2; }
            if (highThreat) { w[3] += 2; }
            if (p.PriorTraditions == "reformation") { w[3] += 4; }
            if (harshEnv && simpleSociety) { w[0] += 3; }

            return ReligionRng.WeightedChoice(rng, options, w.ToList());
        }

        private static ReligionOption ChooseGenderRole(
            ReligionParams p,
            ReligionRng rng,
            SpecialistOption specialist,
            bool strongState)
        {
            var options = new List<ReligionOption>
            {
                new ReligionOption { Name = "Male-Dominated Clergy", Desc = "Religious authority held almost exclusively by men" },
                new ReligionOption { Name = "Female-Dominated Clergy", Desc = "Women hold primary religious authority" },
                new ReligionOption { Name = "Gender-Parallel Traditions", Desc = "Separate male and female religious spheres, both valued" },
                new ReligionOption { Name = "Gender-Balanced Access", Desc = "Both genders can access religious roles relatively equally" },
                new ReligionOption { Name = "Third-Gender/Liminal Specialists", Desc = "Gender-liminal individuals hold special religious status" },
                new ReligionOption { Name = "Domestic Female/Public Male Split", Desc = "Women lead household religion; men lead public rites" },
            };

            double[] w = new double[] { 1, 1, 1, 1, 1, 1 };
            if (p.GenderSystem == "patrilineal") { w[0] += 4; w[5] += 3; }
            if (p.GenderSystem == "matrilineal") { w[1] += 3; w[2] += 3; w[3] += 2; }
            if (p.GenderSystem == "bilateral") { w[3] += 4; }
            if (p.GenderSystem == "dualistic") { w[2] += 5; w[5] += 2; }
            if (specialist.Name.Contains("Shaman")) { w[4] += 3; w[3] += 2; }
            if ((specialist.Name.Contains("Temple") || specialist.Name.Contains("Hereditary")) && p.GenderSystem == "patrilineal") { w[0] += 3; }
            if (p.Subsistence == "horticultural") { w[1] += 2; w[2] += 2; }
            if (strongState) { w[0] += 2; }
            if (p.PoliticalPower == "distributed") { w[3] += 2; w[2] += 2; }

            return ReligionRng.WeightedChoice(rng, options, w.ToList());
        }

        private static ReligionOption ChooseMisfortune(
            ReligionParams p,
            ReligionRng rng,
            ReligionOption deity,
            bool simpleSociety,
            bool complexSociety,
            bool highThreat,
            bool harshEnv,
            bool connected)
        {
            var options = new List<ReligionOption>
            {
                new ReligionOption { Id = "spirit_offense", Name = "Spirit Offense", Desc = "Offending spirits through taboo violation or neglect" },
                new ReligionOption { Id = "sorcery", Name = "Sorcery/Witchcraft", Desc = "Malevolent magic from enemies or envious neighbors" },
                new ReligionOption { Id = "ancestral", Name = "Ancestral Displeasure", Desc = "Ancestors punish neglect or improper conduct" },
                new ReligionOption { Id = "divine_punishment", Name = "Divine Punishment", Desc = "God(s) punish sin, impiety, or moral transgression" },
                new ReligionOption { Id = "cosmic_imbalance", Name = "Cosmic Imbalance", Desc = "Disrupting natural/cosmic harmony causes backlash" },
                new ReligionOption { Id = "fate", Name = "Fate/Destiny", Desc = "Misfortune predetermined; must be accepted or navigated" },
                new ReligionOption { Id = "karma", Name = "Moral Causation", Desc = "Present suffering from past actions (this life or previous)" },
                new ReligionOption { Id = "enemy_gods", Name = "Enemy Gods/Demons", Desc = "Malevolent supernatural beings actively cause harm" },
            };

            double[] w = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
            if (deity.Id == "animistic") { w[0] += 5; w[4] += 2; }
            if (deity.Id == "ancestor_focused") { w[2] += 5; }
            if (deity.Id == "monotheistic" || deity.Id == "henotheistic") { w[3] += 5; w[7] += 2; }
            if (deity.Id == "pantheistic") { w[4] += 4; w[6] += 3; }
            if (simpleSociety) { w[1] += 3; w[0] += 2; }
            if (complexSociety) { w[3] += 2; w[5] += 2; w[6] += 2; }
            if (p.KinshipStructure == "lineage_corporate") { w[2] += 3; }
            if (highThreat) { w[7] += 3; w[1] += 2; }
            if (p.Isolation == "isolated") { w[1] += 2; }
            if (connected) { w[6] += 2; w[5] += 2; }
            if (harshEnv) { w[0] += 2; w[4] += 2; }

            return ReligionRng.WeightedChoice(rng, options, w.ToList());
        }

        private static ReligionOption ChooseAuthority(
            ReligionParams p,
            ReligionRng rng,
            bool simpleSociety,
            bool complexSociety,
            SpecialistOption specialist)
        {
            var options = new List<ReligionOption>
            {
                new ReligionOption { Name = "Charismatic/Personal", Desc = "Authority from personal spiritual power, visions, efficacy" },
                new ReligionOption { Name = "Traditional/Hereditary", Desc = "Authority inherited through lineage" },
                new ReligionOption { Name = "Institutional/Bureaucratic", Desc = "Authority from position in hierarchy" },
                new ReligionOption { Name = "Textual/Scriptural", Desc = "Authority from mastery of sacred texts" },
                new ReligionOption { Name = "Orthopraxy-Based", Desc = "Authority from correct ritual performance" },
                new ReligionOption { Name = "Consensus/Elder Council", Desc = "Collective religious decision-making" },
            };

            double[] w = new double[] { 1, 1, 1, 1, 1, 1 };
            if (simpleSociety) { w[0] += 3; w[5] += 3; }
            if (complexSociety) { w[2] += 4; w[4] += 2; }
            if (p.WritingSystem == "widespread") { w[3] += 5; }
            if (p.WritingSystem == "limited") { w[3] += 3; w[2] += 2; }
            if (p.WritingSystem == "none") { w[1] += 3; w[0] += 2; }
            if (p.PoliticalPower == "theocratic") { w[2] += 3; }
            if (p.PoliticalPower == "distributed") { w[0] += 2; w[5] += 3; }
            if (specialist.Name.Contains("Shaman")) { w[0] += 4; }
            if (specialist.Name.Contains("Hereditary")) { w[1] += 4; }
            if (specialist.Name.Contains("Temple")) { w[2] += 3; w[4] += 2; }
            if (p.KinshipStructure == "lineage_corporate") { w[1] += 3; }
            if (p.PriorTraditions == "reformation") { w[3] += 2; w[0] += 2; }

            return ReligionRng.WeightedChoice(rng, options, w.ToList());
        }

        private static List<string> ChooseRituals(
            ReligionParams p,
            ReligionRng rng,
            bool settled,
            bool urban,
            bool literate,
            SpecialistOption specialist,
            ReligionOption deity,
            bool highThreat,
            bool harshEnv,
            ReligionOption misfortune)
        {
            var items = new List<(string name, double weight)>
            {
                ("Animal Sacrifice", 1),
                ("Offerings (food, drink, valuables)", 2),
                ("Communal Feasting", 1),
                ("Ecstatic Dance/Music", 1),
                ("Meditation/Contemplation", 1),
                ("Pilgrimage", 1),
                ("Water Purification", 1),
                ("Fire Rituals", 1),
                ("Divination/Oracle Consultation", 2),
                ("Rites of Passage", 2),
                ("Calendrical Festivals", 1),
                ("Sacred Narrative Recitation", 1),
                ("Ancestor Veneration Rites", 1),
                ("Fasting/Abstinence", 1),
                ("Ritual Combat/Contests", 1),
                ("Possession/Trance States", 1),
                ("Processions/Public Display", 1),
            };

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                double score = it.weight;
                if (it.name == "Animal Sacrifice" && (p.Subsistence == "agricultural" || p.Subsistence == "pastoral" || p.SocialOrg == "chiefdom" || p.SocialOrg == "state")) score += 3;
                if (it.name == "Communal Feasting" && (p.SocialOrg == "tribe" || p.SocialOrg == "chiefdom" || p.Subsistence == "agricultural")) score += 3;
                if (it.name == "Ecstatic Dance/Music" && (p.Subsistence == "foraging" || p.SocialOrg == "tribe")) score += 3;
                if (it.name == "Meditation/Contemplation" && literate) score += 3;
                if (urban && it.name == "Meditation/Contemplation") score += 2;
                if (specialist.Name.Contains("Shaman") && (it.name.Contains("Dance") || it.name.Contains("Possession"))) score += 4;
                if (specialist.Name.Contains("Monastic") && (it.name.Contains("Meditation") || it.name.Contains("Fasting"))) score += 4;
                if (specialist.Name.Contains("Temple") && it.name.Contains("Processions")) score += 3;
                if (deity.Id == "ancestor_focused" && it.name.Contains("Ancestor")) score += 4;
                if (p.KinshipStructure == "lineage_corporate" && it.name.Contains("Ancestor")) score += 3;
                if (highThreat && it.name.Contains("Combat")) score += 3;
                if ((misfortune.Id == "spirit_offense" || misfortune.Id == "cosmic_imbalance") && it.name.Contains("Purification")) score += 2;
                if (harshEnv && (it.name.Contains("Fire") || it.name.Contains("Fasting"))) score += 2;
                if (settled && it.name == "Pilgrimage") score += 2;
                if (it.name == "Calendrical Festivals" && (p.Subsistence == "agricultural" || p.SocialOrg == "state")) score += 3;
                if (it.name == "Fire Rituals" && (p.Subsistence == "pastoral" || p.Environment == "arid_scarce" || p.Environment == "harsh_extreme")) score += 3;
                items[i] = (it.name, score);
            }

            var pool = items.Where(x => x.weight > 0).Select(x => x.name).ToList();
            var weights = items.Where(x => x.weight > 0).Select(x => x.weight).ToList();
            int k = 4 + rng.NextInt(0, 2);
            var result = new List<string>();
            for (int n = 0; n < k && pool.Count > 0; n++)
            {
                double total = 0;
                foreach (double w in weights) total += w;
                if (total <= 0) break;
                double r = rng.NextDouble() * total;
                for (int i = 0; i < pool.Count; i++)
                {
                    if (r < weights[i])
                    {
                        result.Add(pool[i]);
                        pool.RemoveAt(i);
                        weights.RemoveAt(i);
                        break;
                    }
                    r -= weights[i];
                }
            }
            return result;
        }

        private static List<string> ChooseSacredTimes(
            ReligionParams p,
            ReligionRng rng,
            ReligionOption deity,
            bool settled,
            bool strongState,
            bool complexSociety,
            bool literate,
            bool urban,
            bool connected)
        {
            var list = new List<string>();
            if (p.Subsistence == "agricultural" || p.Subsistence == "horticultural")
                list.Add("Agricultural cycle (planting, harvest, first fruits)");
            if (p.Subsistence == "pastoral" || p.Subsistence == "foraging")
                list.Add("Seasonal resource cycles (migrations, hunts, blooms)");
            if (p.Subsistence == "maritime")
                list.Add("Maritime cycles (fishing seasons, safe sailing)");
            if (p.Environment == "coastal_riverine")
                list.Add("Flood/water cycles");
            if (p.Environment == "unpredictable")
                list.Add("Disaster anniversaries and appeasement");

            var extra = new List<(string name, double weight)>
            {
                ("Solar events (solstices, equinoxes)", settled ? 4 : 2),
                ("Lunar phases", 3),
                ("Ancestor commemoration days", deity.Id == "ancestor_focused" ? 5 : (p.KinshipStructure == "lineage_corporate" ? 4 : 1)),
                ("Royal/state ceremonies", strongState ? 5 : (complexSociety ? 3 : 1)),
                ("Mythical/historical anniversaries", literate ? 4 : 2),
                ("Market-linked festivals", (urban || connected) ? 4 : 1),
            };
            foreach (var t in extra)
            {
                if (list.Count >= 5) break;
                if (rng.NextDouble() * 6 < t.weight)
                    list.Add(t.name);
            }
            if (list.Count < 2)
                list.Add("Lunar phases and monthly observances");
            return list;
        }

        private static List<string> ChooseSacredSpaces(
            ReligionParams p,
            ReligionRng rng,
            bool mobile,
            bool settled,
            bool urban,
            bool strongState,
            bool complexSociety,
            ReligionOption deity)
        {
            var options = new List<(string name, double weight)>
            {
                ("Natural features (mountains, groves, springs, caves)", 2),
                ("Domestic shrines in homes", 2),
                ("Portable sacred objects", 1),
                ("Village shrine or sacred grove", 1),
                ("Temple complexes", 1),
                ("Pilgrimage sites", 1),
                ("Burial grounds/ancestor sites", 2),
                ("Urban ceremonial centers", 1),
                ("Palace-temple complexes", 1),
                ("Boundary markers and crossroads", 1),
            };
            if (mobile) { options[2] = (options[2].name, 5); options[1] = (options[1].name, 4); }
            if (p.Settlement == "permanent_village") { var o = options[3]; options[3] = (o.name, 5); }
            if (complexSociety && settled) { var o4 = options[4]; var o5 = options[5]; options[4] = (o4.name, 5); options[5] = (o5.name, 4); }
            if (urban) { var o7 = options[7]; options[7] = (o7.name, 5); }
            if (deity.Id == "ancestor_focused" || p.KinshipStructure == "lineage_corporate") { var o6 = options[6]; options[6] = (o6.name, 5); }
            if (strongState || p.PoliticalPower == "theocratic") { var o8 = options[8]; options[8] = (o8.name, 5); }
            if (p.Subsistence == "pastoral" || mobile) { var o9 = options[9]; options[9] = (o9.name, 3); }
            if (p.Environment == "coastal_riverine" || p.Environment == "arid_scarce") { var o0 = options[0]; options[0] = (o0.name, 4); }

            var filtered = options.Where(x => x.weight > 0).ToList();
            int k = 2 + rng.NextInt(0, 2);
            return WeightedSampleStrings(rng, filtered, k);
        }

        private static List<string> WeightedSampleStrings(ReligionRng rng, List<(string name, double weight)> options, int k)
        {
            var pool = new List<string>();
            var weights = new List<double>();
            foreach (var o in options)
            {
                pool.Add(o.name);
                weights.Add(o.weight);
            }
            var result = new List<string>();
            for (int n = 0; n < k && pool.Count > 0; n++)
            {
                double total = 0;
                foreach (double w in weights) total += w;
                if (total <= 0) break;
                double r = rng.NextDouble() * total;
                for (int i = 0; i < pool.Count; i++)
                {
                    if (r < weights[i])
                    {
                        result.Add(pool[i]);
                        pool.RemoveAt(i);
                        weights.RemoveAt(i);
                        break;
                    }
                    r -= weights[i];
                }
            }
            return result;
        }

        private static List<string> ChooseMaterialCulture(
            ReligionParams p,
            ReligionRng rng,
            bool settled,
            bool complexSociety,
            bool literate,
            bool mobile,
            bool strongState,
            bool harshEnv,
            ReligionOption deity)
        {
            var options = new List<(string name, double weight)>
            {
                ("Sacred animals as symbols", 2),
                ("Astronomical symbols", 2),
                ("Plant/tree imagery", 2),
                ("Geometric sacred patterns", 2),
                ("Ancestor masks or figures", 1),
                ("Sacred textiles and regalia", 2),
                ("Monumental architecture", 1),
                ("Sacred texts/scrolls", 1),
                ("Ritual vessels", 2),
                ("Body modification (tattoos, scarification)", 1),
                ("Musical instruments", 2),
                ("Statuary and iconography", 1),
            };
            if (p.Subsistence == "pastoral") options[0] = (options[0].name, 6);
            if (p.Subsistence == "agricultural") { options[2] = (options[2].name, 5); options[1] = (options[1].name, 4); }
            if (p.Subsistence == "foraging") { options[0] = (options[0].name, 5); options[9] = (options[9].name, 4); }
            if (settled) options[1] = (options[1].name, 4);
            if (complexSociety && settled) { options[6] = (options[6].name, 5); options[11] = (options[11].name, 4); }
            if (literate) options[7] = (options[7].name, 6);
            if (p.KinshipStructure == "lineage_corporate" || deity.Id == "ancestor_focused") options[4] = (options[4].name, 5);
            if (mobile) { options[5] = (options[5].name, 5); options[9] = (options[9].name, 3); }
            if (p.SocialOrg == "tribe" || p.KinshipStructure == "fictive_kin") options[9] = (options[9].name, options[9].weight + 3);
            if (strongState) { options[5] = (options[5].name, options[5].weight + 2); options[6] = (options[6].name, options[6].weight + 2); }
            if (harshEnv) options[9] = (options[9].name, options[9].weight + 2);

            var filtered = options.Where(x => x.weight > 0).ToList();
            int k = 3 + rng.NextInt(0, 2);
            return WeightedSampleStrings(rng, filtered, k);
        }

        private static List<string> ChooseEthics(
            ReligionParams p,
            ReligionRng rng,
            bool simpleSociety,
            bool complexSociety,
            bool highThreat,
            ReligionOption deity,
            bool strongState,
            bool connected,
            ReligionOption misfortune,
            bool mobile)
        {
            var options = new List<(string name, double weight)>
            {
                ("Reciprocity—balanced exchanges", 2),
                ("Purity—avoid pollution", 2),
                ("Honor/Shame—reputation paramount", 2),
                ("Cosmic order—human acts affect world", 2),
                ("Virtue cultivation", 2),
                ("Divine obedience", 2),
                ("Karmic consequence", 1),
                ("Group solidarity", 2),
                ("Hierarchical duty", 2),
                ("Hospitality to strangers", 2),
            };
            if (simpleSociety) { options[0] = (options[0].name, 5); options[7] = (options[7].name, 5); }
            if (complexSociety) { options[8] = (options[8].name, 6); options[4] = (options[4].name, 4); }
            if (p.KinshipStructure == "lineage_corporate") { options[8] = (options[8].name, options[8].weight + 2); options[7] = (options[7].name, options[7].weight + 2); }
            if (highThreat) { options[2] = (options[2].name, 5); options[7] = (options[7].name, 4); }
            if (p.Subsistence == "pastoral" || mobile) { options[9] = (options[9].name, 6); options[2] = (options[2].name, 4); }
            if (deity.Id == "monotheistic" || deity.Id == "henotheistic") options[5] = (options[5].name, 6);
            if (deity.Id == "pantheistic") { options[3] = (options[3].name, 5); options[6] = (options[6].name, 4); }
            if (misfortune.Id == "spirit_offense" || misfortune.Id == "cosmic_imbalance") { options[1] = (options[1].name, 5); options[3] = (options[3].name, 4); }
            if (strongState) { options[8] = (options[8].name, options[8].weight + 2); options[5] = (options[5].name, options[5].weight + 2); }
            if (connected) options[9] = (options[9].name, options[9].weight + 2);

            var filtered = options.Where(x => x.weight > 0).ToList();
            return WeightedSampleStrings(rng, filtered, 3);
        }

        private static List<string> ChooseTaboos(
            ReligionParams p,
            ReligionRng rng,
            bool complexSociety,
            bool literate,
            bool harshEnv,
            ReligionOption deity,
            ReligionOption misfortune)
        {
            var options = new List<(string name, double weight)>
            {
                ("Dietary restrictions", 2),
                ("Sexual/marriage regulations", 2),
                ("Death pollution rules", 2),
                ("Sacred space access restrictions", 2),
                ("Name/speech taboos", 2),
                ("Caste/outsider contact rules", 1),
                ("Menstrual/bodily taboos", 2),
                ("Resource/hunting restrictions", 2),
                ("Temporal taboos (unlucky days)", 2),
            };
            if (p.Subsistence == "pastoral") options[0] = (options[0].name, 5);
            if (p.Subsistence == "foraging") { options[7] = (options[7].name, 6); options[0] = (options[0].name, 4); }
            if (p.KinshipStructure == "lineage_corporate") { options[1] = (options[1].name, 5); options[2] = (options[2].name, 4); }
            if (p.KinshipStructure == "extended_clan") options[1] = (options[1].name, 4);
            if (complexSociety) { options[5] = (options[5].name, 4); options[3] = (options[3].name, 4); }
            if (p.GenderSystem == "dualistic") { options[6] = (options[6].name, 5); options[1] = (options[1].name, 4); }
            if (p.GenderSystem == "patrilineal") options[6] = (options[6].name, 4);
            if (deity.Id == "ancestor_focused") { options[2] = (options[2].name, 5); options[4] = (options[4].name, 4); }
            if (misfortune.Id == "spirit_offense" || misfortune.Id == "cosmic_imbalance") { options[2] = (options[2].name, options[2].weight + 2); options[6] = (options[6].name, options[6].weight + 2); }
            if (literate && complexSociety) options[8] = (options[8].name, 5);
            if (harshEnv) options[7] = (options[7].name, options[7].weight + 2);

            var filtered = options.Where(x => x.weight > 0).ToList();
            int k = 2 + rng.NextInt(0, 2);
            return WeightedSampleStrings(rng, filtered, k);
        }

        private static List<string> ChooseUnique(
            ReligionParams p,
            ReligionRng rng,
            bool strongState,
            SpecialistOption specialist,
            bool simpleSociety,
            bool complexSociety,
            bool literate,
            bool highThreat,
            bool connected,
            ReligionOption deity,
            bool urban)
        {
            bool harshEnv = p.Environment == "arid_scarce" || p.Environment == "harsh_extreme" || p.Environment == "unpredictable";
            var options = new List<(string name, bool cond)>
            {
                ("Sacred kingship", strongState || p.PoliticalPower == "theocratic"),
                ("Regular spirit possession", specialist.Name.Contains("Shaman") || deity.Id == "animistic"),
                ("Vision quests expected", simpleSociety || specialist.Name.Contains("Shaman")),
                ("Complex sacred calendar", literate && (complexSociety || p.Subsistence == "agricultural")),
                ("Ritual language", literate || complexSociety),
                ("Totem/clan animal system", p.KinshipStructure == "extended_clan" || p.KinshipStructure == "lineage_corporate"),
                ("Elaborate mortuary practices", deity.Id == "ancestor_focused" || complexSociety),
                ("Mystery initiations", specialist.Name.Contains("Secret") || p.KinshipStructure == "fictive_kin"),
                ("Prophetic/millennial expectations", highThreat || p.PriorTraditions == "reformation" || p.ExternalThreat == "existential"),
                ("Ascetic virtuoso tradition", specialist.Name.Contains("Monastic") || (literate && connected)),
                ("Sacred entheogens", specialist.Name.Contains("Shaman") || p.Environment == "tropical_abundant"),
                ("Oracular institutions", complexSociety || harshEnv || p.Environment == "unpredictable"),
                ("Religious law code", literate && (strongState || p.PoliticalPower == "theocratic")),
                ("Warrior cult/religious warfare", highThreat && p.SocialOrg != "band"),
                ("Sacred craft guilds", urban || p.Subsistence == "urban_trade"),
                ("Healing cult central", specialist.Name.Contains("Shaman") || harshEnv),
                ("Elite vs folk divide", complexSociety && literate),
            };
            var applicable = options.Where(x => x.cond).Select(x => x.name).ToList();
            rng.ShuffleInPlace(applicable);
            int k = Math.Min(3 + rng.NextInt(0, 2), applicable.Count);
            return applicable.Take(k).ToList();
        }

        private static List<string> BuildSyncNotes(ReligionParams p)
        {
            var list = new List<string>();
            if (p.PriorTraditions == "syncretic") list.Add("Visible layers of older traditions beneath current practices");
            if (p.PriorTraditions == "reformation") list.Add("Explicit rejection or reform of older religious elements");
            if (p.PriorTraditions == "imposed") list.Add("Indigenous practices persist beneath or alongside imposed tradition");
            if (p.Isolation == "cosmopolitan") list.Add("Heavy borrowing from multiple surrounding cultures");
            if (p.ExternalThreat == "existential") list.Add("Religion shaped by trauma, displacement, or resistance");
            return list;
        }

        private static ReligionLandscape BuildLandscape(
            ReligionParams p,
            ReligionRng rng,
            bool complexSociety,
            bool simpleSociety,
            bool urban,
            bool connected,
            bool strongState,
            bool literate,
            bool harshEnv,
            ReligionOption deity,
            bool highThreat)
        {
            double hegemonyScore = 50.0;
            if (simpleSociety) hegemonyScore += 25;
            if (p.SocialOrg == "band") hegemonyScore += 10;
            if (p.Isolation == "isolated") hegemonyScore += 15;
            if (p.PoliticalPower == "theocratic") hegemonyScore += 10;
            if (strongState) hegemonyScore += 5;
            if (connected) hegemonyScore -= 15;
            if (p.Isolation == "cosmopolitan") hegemonyScore -= 10;
            if (urban) hegemonyScore -= 10;
            if (p.PriorTraditions == "imposed") hegemonyScore -= 15;
            if (p.PriorTraditions == "syncretic") hegemonyScore -= 5;
            if (literate) hegemonyScore -= 5;
            if (p.WritingSystem == "widespread") hegemonyScore -= 5;
            hegemonyScore += (rng.NextDouble() - 0.5) * 10;
            int hegemonyPct = SigmoidPct(hegemonyScore);

            string hegemonyDesc;
            if (hegemonyPct >= 85) hegemonyDesc = "Near-universal adherence; alternatives barely visible";
            else if (hegemonyPct >= 70) hegemonyDesc = "Strong majority; minorities exist but marginal";
            else if (hegemonyPct >= 55) hegemonyDesc = "Clear majority but significant minorities";
            else if (hegemonyPct >= 40) hegemonyDesc = "Plurality; substantial religious diversity";
            else hegemonyDesc = "Fragmented landscape; no clear majority";

            var rivalCandidates = new List<(LandscapeEntry e, double weight)>
            {
                (new LandscapeEntry { Name = "Folk/local traditions", Desc = "Village practices, household cults, local spirits" }, (complexSociety ? 3 : 0) + (p.PriorTraditions == "imposed" ? 3 : 0)),
                (new LandscapeEntry { Name = "Foreign merchant cults", Desc = "Religions brought by traders in market quarters" }, (connected ? 3 : 0) + (urban ? 2 : 0) + (p.Subsistence == "urban_trade" ? 2 : 0)),
                (new LandscapeEntry { Name = "Immigrant community religions", Desc = "Ethnic enclaves maintain distinct traditions" }, (urban ? 3 : 0) + (p.Isolation == "cosmopolitan" ? 3 : 0)),
                (new LandscapeEntry { Name = "Elite philosophical schools", Desc = "Educated rationalized alternatives" }, (literate && complexSociety ? 3 : 0)),
                (new LandscapeEntry { Name = "Mystery cults", Desc = "Initiation groups offering personal salvation" }, (complexSociety ? 2 : 0) + (urban ? 1 : 0) + (connected ? 1 : 0)),
                (new LandscapeEntry { Name = "Prophetic movements", Desc = "Charismatic leaders promising renewal" }, (highThreat ? 3 : 0) + (p.ExternalThreat == "existential" ? 2 : 0) + (p.PriorTraditions == "imposed" ? 2 : 0)),
                (new LandscapeEntry { Name = "Reform sects", Desc = "Claim purer/original form of dominant tradition" }, (literate ? 2 : 0) + (p.PriorTraditions == "reformation" ? 3 : 0)),
                (new LandscapeEntry { Name = "Conquered peoples' religions", Desc = "Subjugated groups' traditions" }, (p.SocialOrg == "empire" ? 4 : 0) + (p.PriorTraditions == "imposed" ? 2 : 0)),
                (new LandscapeEntry { Name = "Artisan/guild traditions", Desc = "Craft-specific practices and patron deities" }, (urban ? 3 : 0) + (p.Subsistence == "urban_trade" ? 2 : 0)),
                (new LandscapeEntry { Name = "Heretical movements", Desc = "Deviant interpretations deemed dangerous" }, (literate && (strongState || p.PoliticalPower == "theocratic") ? 3 : 0)),
                (new LandscapeEntry { Name = "Old religion survivals", Desc = "Pre-existing traditions in rural areas" }, (p.PriorTraditions != "indigenous_only" ? 3 : 0)),
                (new LandscapeEntry { Name = "Royal/court cult", Desc = "Distinct ruler-focused practices" }, (strongState ? 3 : 0) + (p.PoliticalPower == "theocratic" ? 2 : 0)),
            };

            int rivalCount = 0;
            if (p.SocialOrg == "tribe") rivalCount = 1;
            if (p.SocialOrg == "chiefdom") rivalCount = 2;
            if (p.SocialOrg == "state") rivalCount = 3;
            if (p.SocialOrg == "empire") rivalCount = 4;
            if (connected) rivalCount += 1;
            if (urban) rivalCount += 1;
            if (p.Isolation == "isolated") rivalCount = Math.Max(0, rivalCount - 2);

            var rivals = ReligionRng.WeightedSampleWithoutReplacement(rng, rivalCandidates.Where(x => x.weight > 0).Select(x => x.e).ToList(), e =>
            {
                var pair = rivalCandidates.FirstOrDefault(c => c.e.Name == e.Name);
                return pair.weight;
            }, rivalCount);

            var nonBeliefCandidates = new List<(LandscapeEntry e, double weight)>
            {
                (new LandscapeEntry { Name = "Practical indifference", Desc = "Too busy surviving to engage deeply; minimally observant" }, 2),
                (new LandscapeEntry { Name = "Ritual-only participation", Desc = "Social conformity without inner belief" }, 2),
                (new LandscapeEntry { Name = "Supernatural agnosticism", Desc = "Uncertainty: \"who can really know?\"" }, 2),
                (new LandscapeEntry { Name = "Philosophical skepticism", Desc = "Educated questioning of claims" }, literate ? 3 : 0),
                (new LandscapeEntry { Name = "Materialist philosophy", Desc = "Explicit denial of supernatural" }, (literate && complexSociety) ? 2 : 0),
                (new LandscapeEntry { Name = "Cynical view", Desc = "\"Priests invented it to control people\"" }, complexSociety ? 2 : 0),
                (new LandscapeEntry { Name = "Private doubt", Desc = "Hidden disbelief, public conformity" }, (strongState || p.PoliticalPower == "theocratic") ? 3 : 1),
                (new LandscapeEntry { Name = "Practical causation focus", Desc = "Prefer technical explanations" }, 1),
            };
            if (literate && connected) { nonBeliefCandidates[3] = (nonBeliefCandidates[3].e, nonBeliefCandidates[3].weight + 2); if (nonBeliefCandidates[4].weight > 0) nonBeliefCandidates[4] = (nonBeliefCandidates[4].e, nonBeliefCandidates[4].weight + 2); }
            if (urban && nonBeliefCandidates[5].weight > 0) nonBeliefCandidates[5] = (nonBeliefCandidates[5].e, nonBeliefCandidates[5].weight + 2);
            if (harshEnv) { nonBeliefCandidates[0] = (nonBeliefCandidates[0].e, nonBeliefCandidates[0].weight + 2); nonBeliefCandidates[7] = (nonBeliefCandidates[7].e, nonBeliefCandidates[7].weight + 2); }

            int nonBeliefCount = simpleSociety ? 2 : (p.SocialOrg == "chiefdom" ? 3 : 4);
            var nonBeliefForms = ReligionRng.WeightedSampleWithoutReplacement(rng, nonBeliefCandidates.Where(x => x.weight > 0).Select(x => x.e).ToList(), e =>
            {
                var pair = nonBeliefCandidates.FirstOrDefault(c => c.e.Name == e.Name);
                return pair.weight;
            }, nonBeliefCount);

            double nonBeliefScore = 5;
            if (urban) nonBeliefScore += 5;
            if (literate) nonBeliefScore += 5;
            if (p.WritingSystem == "widespread") nonBeliefScore += 3;
            if (connected) nonBeliefScore += 3;
            if (complexSociety) nonBeliefScore += 5;
            if (p.PoliticalPower == "distributed") nonBeliefScore += 3;
            if (p.PoliticalPower == "theocratic") nonBeliefScore -= 3;
            if (harshEnv) nonBeliefScore += 2;
            nonBeliefScore += (rng.NextDouble() - 0.5) * 8;
            int nonBeliefPct = SigmoidPct(nonBeliefScore);

            var dynamicsCandidates = new List<(LandscapeEntry e, double weight)>
            {
                (new LandscapeEntry { Name = "Coercive uniformity", Desc = "Authorities suppress alternatives; penalties for deviance" }, (strongState ? 3 : 0) + (p.PoliticalPower == "theocratic" ? 3 : 0)),
                (new LandscapeEntry { Name = "Hierarchical incorporation", Desc = "Minorities tolerated in subordinate position" }, complexSociety ? 3 : 0),
                (new LandscapeEntry { Name = "Segmented coexistence", Desc = "Different groups practice separately" }, 2),
                (new LandscapeEntry { Name = "Competitive pluralism", Desc = "Traditions actively compete for adherents" }, (urban ? 3 : 0) + (connected ? 2 : 0)),
                (new LandscapeEntry { Name = "Syncretic blending", Desc = "Boundaries blurry; people combine elements" }, (p.PriorTraditions == "syncretic" ? 4 : 0) + (p.Isolation == "cosmopolitan" ? 3 : 0)),
                (new LandscapeEntry { Name = "Elite/folk divide", Desc = "Educated and common practice different religions" }, (complexSociety && literate ? 3 : 0)),
                (new LandscapeEntry { Name = "Ethnic-religious mapping", Desc = "Religious identity tied to ethnic/tribal identity" }, (p.KinshipStructure == "extended_clan" ? 2 : 0) + (p.KinshipStructure == "lineage_corporate" ? 3 : 0)),
                (new LandscapeEntry { Name = "Patron-client cults", Desc = "People use multiple specialists/traditions as needed" }, (deity.Id.Contains("polytheistic") || deity.Id == "animistic") ? 3 : 0),
            };

            var dynamics = ReligionRng.WeightedSampleWithoutReplacement(rng, dynamicsCandidates.Where(x => x.weight > 0).Select(x => x.e).ToList(), e =>
            {
                var pair = dynamicsCandidates.FirstOrDefault(c => c.e.Name == e.Name);
                return pair.weight;
            }, 1 + rng.NextInt(0, 1));

            return new ReligionLandscape
            {
                HegemonyPct = hegemonyPct,
                HegemonyDesc = hegemonyDesc,
                Rivals = rivals,
                NonBeliefPct = nonBeliefPct,
                NonBeliefForms = nonBeliefForms,
                Dynamics = dynamics,
            };
        }
    }
}
