/**
 * Shared data for Integration: LEVELS, TECHS, REGIMES, TRANSITIONS.
 * Single source of truth so Tech Tree, Regime Chart, and History sim use the same model.
 */
(function () {
  "use strict";

  var LEVELS = [
    { id: 1,  label: "Level 1",  sublabel: "Stone Age",        color: "#78350f" },
    { id: 2,  label: "Level 2",  sublabel: "Early Neolithic",  color: "#92400e" },
    { id: 3,  label: "Level 3",  sublabel: "Late Neolithic",   color: "#b45309" },
    { id: 4,  label: "Level 4",  sublabel: "Chalcolithic",     color: "#a16207" },
    { id: 5,  label: "Level 5",  sublabel: "Bronze Age",       color: "#b08d57" },
    { id: 6,  label: "Level 6",  sublabel: "Iron Age",         color: "#6b7280" },
    { id: 7,  label: "Level 7",  sublabel: "Classical",        color: "#1d4ed8" },
    { id: 8,  label: "Level 8",  sublabel: "Medieval",         color: "#5b21b6" },
    { id: 9,  label: "Level 9",  sublabel: "Renaissance",      color: "#065f46" },
    { id: 10, label: "Level 10", sublabel: "Early Modern",     color: "#0e7490" },
    { id: 11, label: "Level 11", sublabel: "Industrial",       color: "#1e3a5f" },
    { id: 12, label: "Level 12", sublabel: "Modern",           color: "#1a1a2e" },
    { id: 13, label: "Level 13", sublabel: "Early Space Age",  color: "#312e81" },
    { id: 14, label: "Level 14", sublabel: "Interplanetary",  color: "#1e1b4b" },
    { id: 15, label: "Level 15", sublabel: "Interstellar",     color: "#0c0a1e" },
  ];

  var TECHS = [
    { id: "fire", name: "Fire Control", level: 1, col: 1, row: 1, req: [] },
    { id: "flint", name: "Flint Knapping", level: 1, col: 1, row: 3, req: [] },
    { id: "foraging", name: "Systematic Foraging", level: 1, col: 1, row: 5, req: [] },
    { id: "speech", name: "Language", level: 1, col: 1, row: 7, req: [] },
    { id: "shelter", name: "Shelter Building", level: 1, col: 1, row: 9, req: [] },
    { id: "migration", name: "Migration & Pathfinding", level: 1, col: 1, row: 11, req: [] },
    { id: "hunting", name: "Hunting & Trapping", level: 2, col: 1, row: 1, req: ["fire", "flint"] },
    { id: "cooking", name: "Cooking", level: 2, col: 1, row: 3, req: ["fire", "foraging"] },
    { id: "pottery", name: "Pottery", level: 2, col: 1, row: 5, req: ["foraging"] },
    { id: "ritual", name: "Ritual & Religion", level: 2, col: 1, row: 7, req: ["speech"] },
    { id: "tracking", name: "Tracking", level: 2, col: 1, row: 9, req: ["speech", "foraging"] },
    { id: "rope", name: "Cordage & Rope", level: 2, col: 1, row: 11, req: ["flint", "shelter"] },
    { id: "fishhook", name: "Fishing & Nets", level: 2, col: 2, row: 2, req: ["hunting", "rope"] },
    { id: "pigment", name: "Pigment & Cave Art", level: 2, col: 2, row: 8, req: ["ritual"] },
    { id: "farming", name: "Crop Cultivation", level: 3, col: 1, row: 2, req: ["cooking", "pottery"] },
    { id: "domestication", name: "Animal Domestication", level: 3, col: 1, row: 4, req: ["hunting", "tracking"] },
    { id: "weaving", name: "Weaving & Textiles", level: 3, col: 1, row: 6, req: ["rope", "ritual"] },
    { id: "village", name: "Permanent Settlement", level: 3, col: 1, row: 8, req: ["farming", "shelter"] },
    { id: "boats", name: "Watercraft", level: 3, col: 1, row: 10, req: ["rope", "fishhook"] },
    { id: "storage", name: "Food Storage", level: 3, col: 2, row: 3, req: ["farming", "pottery"] },
    { id: "chiefcraft", name: "Gift Exchange & Prestige", level: 3, col: 2, row: 7, req: ["ritual", "village"] },
    { id: "irrigation", name: "Irrigation", level: 4, col: 1, row: 1, req: ["farming"] },
    { id: "plough", name: "Ard & Plough", level: 4, col: 1, row: 3, req: ["farming", "domestication"] },
    { id: "trade", name: "Long-Distance Trade", level: 4, col: 1, row: 5, req: ["boats", "village"] },
    { id: "writing", name: "Proto-Writing", level: 4, col: 1, row: 7, req: ["ritual", "trade"] },
    { id: "copper", name: "Copper Working", level: 4, col: 1, row: 9, req: ["pottery"] },
    { id: "math1", name: "Basic Mathematics", level: 4, col: 2, row: 6, req: ["writing", "trade"] },
    { id: "admin1", name: "Early Administration", level: 4, col: 2, row: 8, req: ["writing", "chiefcraft"] },
    { id: "textile2", name: "Advanced Textiles", level: 4, col: 2, row: 11, req: ["weaving", "trade"] },
    { id: "bronze", name: "Bronze Metallurgy", level: 5, col: 1, row: 1, req: ["copper", "trade"] },
    { id: "wheel", name: "Wheel & Axle", level: 5, col: 1, row: 3, req: ["plough", "copper"] },
    { id: "writing2", name: "Phonetic Writing", level: 5, col: 1, row: 5, req: ["writing", "math1"] },
    { id: "calendar", name: "Calendar", level: 5, col: 1, row: 7, req: ["math1", "ritual"] },
    { id: "sailing", name: "Sailing", level: 5, col: 1, row: 9, req: ["boats", "trade"] },
    { id: "masonry", name: "Masonry", level: 5, col: 1, row: 11, req: ["irrigation", "village"] },
    { id: "draft", name: "Draft Animals & Harness", level: 5, col: 2, row: 4, req: ["wheel", "domestication"] },
    { id: "bronze2", name: "Bronze Weapons & Armour", level: 5, col: 2, row: 2, req: ["bronze"] },
    { id: "iron", name: "Iron Smelting", level: 6, col: 1, row: 1, req: ["bronze"] },
    { id: "roads", name: "Road Networks", level: 6, col: 1, row: 3, req: ["wheel", "masonry"] },
    { id: "coinage", name: "Coinage", level: 6, col: 1, row: 5, req: ["bronze", "writing2"] },
    { id: "law", name: "Codified Law", level: 6, col: 1, row: 7, req: ["writing2", "calendar"] },
    { id: "astronomy", name: "Astronomy", level: 6, col: 1, row: 9, req: ["calendar", "sailing"] },
    { id: "aqueduct", name: "Aqueducts & Sanitation", level: 6, col: 1, row: 11, req: ["masonry", "irrigation"] },
    { id: "census", name: "Census & Taxation", level: 6, col: 2, row: 6, req: ["law", "admin1"] },
    { id: "ironweap", name: "Iron Weapons & Tools", level: 6, col: 2, row: 2, req: ["iron"] },
    { id: "steel1", name: "Early Steel", level: 7, col: 1, row: 1, req: ["iron"] },
    { id: "philosophy", name: "Philosophy & Logic", level: 7, col: 1, row: 3, req: ["law", "writing2"] },
    { id: "medicine1", name: "Early Medicine", level: 7, col: 1, row: 5, req: ["law", "astronomy"] },
    { id: "engineering", name: "Mechanical Engineering", level: 7, col: 1, row: 7, req: ["math1", "masonry", "roads"] },
    { id: "glasswork", name: "Glassmaking", level: 7, col: 1, row: 9, req: ["coinage", "iron"] },
    { id: "navy", name: "Naval Warfare", level: 7, col: 1, row: 11, req: ["sailing", "iron"] },
    { id: "rhetoric", name: "Rhetoric & Governance", level: 7, col: 2, row: 4, req: ["philosophy", "census"] },
    { id: "cartography1", name: "Early Cartography", level: 7, col: 2, row: 10, req: ["astronomy", "navy"] },
    { id: "stirrup", name: "Stirrup & Cavalry", level: 8, col: 1, row: 1, req: ["steel1", "roads"] },
    { id: "windmill", name: "Wind & Water Mills", level: 8, col: 1, row: 3, req: ["engineering"] },
    { id: "printing1", name: "Block Printing", level: 8, col: 1, row: 5, req: ["philosophy", "glasswork"] },
    { id: "algebra", name: "Algebra & Trigonometry", level: 8, col: 1, row: 7, req: ["philosophy", "medicine1"] },
    { id: "compass", name: "Magnetic Compass", level: 8, col: 1, row: 9, req: ["astronomy", "navy"] },
    { id: "siege", name: "Siege Engineering", level: 8, col: 1, row: 11, req: ["engineering", "steel1"] },
    { id: "feudallaw", name: "Feudal & Canon Law", level: 8, col: 2, row: 4, req: ["rhetoric", "printing1"] },
    { id: "finance1", name: "Bills of Exchange", level: 8, col: 2, row: 6, req: ["algebra", "coinage"] },
    { id: "horsecollar", name: "Horse Collar & Crop Rotation", level: 8, col: 2, row: 2, req: ["stirrup", "windmill"] },
    { id: "gunpowder", name: "Gunpowder", level: 9, col: 1, row: 1, req: ["siege", "algebra"] },
    { id: "printing2", name: "Movable Type Press", level: 9, col: 1, row: 3, req: ["printing1", "algebra"] },
    { id: "optics", name: "Optics & Lenses", level: 9, col: 1, row: 5, req: ["glasswork", "algebra"] },
    { id: "anatomy", name: "Anatomy & Surgery", level: 9, col: 1, row: 7, req: ["medicine1", "printing1"] },
    { id: "oceannav", name: "Ocean Navigation", level: 9, col: 1, row: 9, req: ["compass", "algebra"] },
    { id: "blast", name: "Blast Furnace", level: 9, col: 1, row: 11, req: ["windmill", "steel1"] },
    { id: "perspective", name: "Technical Drawing", level: 9, col: 2, row: 4, req: ["optics", "printing2"] },
    { id: "accounting", name: "Double-Entry Accounting", level: 9, col: 2, row: 6, req: ["finance1", "printing2"] },
    { id: "firearms", name: "Firearms & Artillery", level: 10, col: 1, row: 1, req: ["gunpowder", "blast"] },
    { id: "scimethod", name: "Scientific Method", level: 10, col: 1, row: 3, req: ["printing2", "optics", "anatomy"] },
    { id: "banking", name: "Banking & Joint-Stock", level: 10, col: 1, row: 5, req: ["accounting", "oceannav"] },
    { id: "chemistry1", name: "Early Chemistry", level: 10, col: 1, row: 7, req: ["scimethod", "blast"] },
    { id: "mapmaking", name: "Cartography & Surveying", level: 10, col: 1, row: 9, req: ["oceannav", "printing2"] },
    { id: "clockwork", name: "Clockwork & Precision", level: 10, col: 1, row: 11, req: ["optics", "algebra"] },
    { id: "colonialism", name: "Colonial Administration", level: 10, col: 2, row: 6, req: ["banking", "mapmaking"] },
    { id: "printing3", name: "Newspapers & Public Sphere", level: 10, col: 2, row: 4, req: ["printing2", "banking"] },
    { id: "shipbuilding", name: "Advanced Shipbuilding", level: 10, col: 2, row: 10, req: ["mapmaking", "firearms"] },
    { id: "steam", name: "Steam Engine", level: 11, col: 1, row: 1, req: ["firearms", "clockwork"] },
    { id: "elec1", name: "Electrostatics", level: 11, col: 1, row: 3, req: ["scimethod", "chemistry1"] },
    { id: "germ", name: "Germ Theory & Vaccines", level: 11, col: 1, row: 5, req: ["chemistry1", "anatomy"] },
    { id: "steel2", name: "Industrial Steel", level: 11, col: 1, row: 7, req: ["steam", "blast"] },
    { id: "telegraph", name: "Telegraph", level: 11, col: 1, row: 9, req: ["elec1", "clockwork"] },
    { id: "chemind", name: "Industrial Chemistry", level: 11, col: 1, row: 11, req: ["chemistry1", "steam"] },
    { id: "railroad", name: "Railways & Locomotives", level: 11, col: 2, row: 2, req: ["steam", "steel2"] },
    { id: "massprint", name: "Mass Literacy & Education", level: 11, col: 2, row: 4, req: ["printing3", "railroad"] },
    { id: "census2", name: "Statistics & Public Admin", level: 11, col: 2, row: 6, req: ["massprint", "census"] },
    { id: "rights", name: "Rights Theory & Constitutionalism", level: 11, col: 2, row: 8, req: ["massprint", "rhetoric"] },
    { id: "combustion", name: "Internal Combustion", level: 12, col: 1, row: 1, req: ["steam", "chemind"] },
    { id: "elec2", name: "Electrical Grid", level: 12, col: 1, row: 3, req: ["elec1", "steel2"] },
    { id: "radio", name: "Radio & Wireless", level: 12, col: 1, row: 5, req: ["telegraph", "elec2"] },
    { id: "pharma", name: "Pharmaceuticals", level: 12, col: 1, row: 7, req: ["germ", "chemind"] },
    { id: "flight", name: "Powered Flight", level: 12, col: 1, row: 9, req: ["combustion", "steel2"] },
    { id: "nuclear", name: "Nuclear Physics", level: 12, col: 1, row: 11, req: ["elec2", "chemind"] },
    { id: "massdem", name: "Mass Democracy & Suffrage", level: 12, col: 2, row: 4, req: ["rights", "radio"] },
    { id: "propaganda", name: "Mass Media & Propaganda", level: 12, col: 2, row: 6, req: ["radio", "massprint"] },
    { id: "automation1", name: "Assembly Line", level: 12, col: 2, row: 2, req: ["combustion", "railroad"] },
    { id: "computing1", name: "Early Computing", level: 12, col: 2, row: 10, req: ["elec2", "nuclear"] },
    { id: "rocketry", name: "Rocketry", level: 13, col: 1, row: 1, req: ["nuclear", "combustion"] },
    { id: "computing2", name: "Microelectronics", level: 13, col: 1, row: 3, req: ["computing1", "elec2"] },
    { id: "telecom2", name: "Satellite Comms", level: 13, col: 1, row: 5, req: ["radio", "rocketry"] },
    { id: "biotech1", name: "Genetics & Biotech", level: 13, col: 1, row: 7, req: ["pharma", "computing2"] },
    { id: "materials1", name: "Advanced Materials", level: 13, col: 1, row: 9, req: ["nuclear", "chemind"] },
    { id: "internet", name: "Global Internet", level: 13, col: 2, row: 4, req: ["computing2", "telecom2"] },
    { id: "renewable", name: "Renewable Energy", level: 13, col: 2, row: 2, req: ["computing2", "elec2"] },
    { id: "AI1", name: "Artificial Intelligence", level: 13, col: 2, row: 6, req: ["internet", "computing2"] },
    { id: "lifesupport", name: "Life Support Systems", level: 13, col: 2, row: 8, req: ["biotech1", "materials1"] },
    { id: "launcher", name: "Heavy Launch Vehicles", level: 13, col: 2, row: 10, req: ["rocketry", "materials1"] },
    { id: "fusion", name: "Fusion Power", level: 14, col: 1, row: 1, req: ["nuclear", "materials1"] },
    { id: "AI2", name: "General AI", level: 14, col: 1, row: 3, req: ["AI1", "internet"] },
    { id: "biotech2", name: "Synthetic Biology", level: 14, col: 1, row: 5, req: ["biotech1", "AI1"] },
    { id: "interplan", name: "Interplanetary Propulsion", level: 14, col: 1, row: 7, req: ["fusion", "launcher"] },
    { id: "terraforming", name: "Terraforming", level: 14, col: 2, row: 2, req: ["lifesupport", "biotech2"] },
    { id: "spacemining", name: "Asteroid Mining", level: 14, col: 2, row: 4, req: ["launcher", "AI2"] },
    { id: "quantum", name: "Quantum Computing", level: 14, col: 2, row: 6, req: ["AI2", "materials1"] },
    { id: "nanotech", name: "Nanotechnology", level: 14, col: 2, row: 8, req: ["biotech2", "quantum"] },
    { id: "planetgov", name: "Planetary Governance", level: 14, col: 2, row: 10, req: ["AI2", "internet"] },
    { id: "ftlcomm", name: "FTL Communication", level: 15, col: 1, row: 2, req: ["quantum", "interplan"] },
    { id: "warp", name: "Warp Drive", level: 15, col: 1, row: 4, req: ["fusion", "quantum"] },
    { id: "dyson", name: "Dyson Swarm", level: 15, col: 1, row: 6, req: ["spacemining", "nanotech"] },
    { id: "postscarcity", name: "Post-Scarcity Economy", level: 15, col: 1, row: 8, req: ["nanotech", "AI2"] },
    { id: "uploaded", name: "Mind Uploading", level: 15, col: 2, row: 3, req: ["AI2", "biotech2"] },
    { id: "xenobio", name: "Xenobiology", level: 15, col: 2, row: 5, req: ["biotech2", "ftlcomm", "terraforming"] },
    { id: "starcolony", name: "Interstellar Colonisation", level: 15, col: 2, row: 7, req: ["warp", "lifesupport", "planetgov"] },
    { id: "civilnet", name: "Interstellar Network", level: 15, col: 2, row: 9, req: ["starcolony", "ftlcomm", "dyson"] },
  ];

  var REGIMES = {
    band: { name: "Band Society", icon: "👥", minLvl: 1, maxLvl: 2, coercion: [1, 2], capacity: [1, 2], inclusiveness: [3, 5] },
    tribal: { name: "Tribal/Chiefless", icon: "🏕️", minLvl: 1, maxLvl: 3, coercion: [1, 3], capacity: [1, 2], inclusiveness: [3, 5] },
    chiefdom: { name: "Chiefdom", icon: "👑", minLvl: 2, maxLvl: 5, coercion: [2, 4], capacity: [1, 3], inclusiveness: [2, 4] },
    theocracy: { name: "Theocracy", icon: "⛪", minLvl: 3, maxLvl: 9, coercion: [3, 5], capacity: [2, 4], inclusiveness: [1, 3] },
    citystate: { name: "City-State", icon: "🏛️", minLvl: 4, maxLvl: 8, coercion: [2, 4], capacity: [2, 4], inclusiveness: [2, 4] },
    feudal: { name: "Feudal Network", icon: "⚔️", minLvl: 4, maxLvl: 8, coercion: [2, 4], capacity: [1, 3], inclusiveness: [1, 2] },
    patrimonial: { name: "Patrimonial Kingdom", icon: "🤴", minLvl: 4, maxLvl: 10, coercion: [3, 4], capacity: [2, 4], inclusiveness: [1, 2] },
    empire: { name: "Bureaucratic Empire", icon: "🦅", minLvl: 5, maxLvl: 11, coercion: [4, 5], capacity: [4, 5], inclusiveness: [1, 2] },
    republic: { name: "Aristocratic Republic", icon: "🎩", minLvl: 5, maxLvl: 12, coercion: [2, 4], capacity: [3, 5], inclusiveness: [2, 3] },
    absolutist: { name: "Absolutist Monarchy", icon: "👸", minLvl: 6, maxLvl: 11, coercion: [4, 5], capacity: [3, 5], inclusiveness: [1, 2] },
    constitutional: { name: "Constitutional Monarchy", icon: "📜", minLvl: 8, maxLvl: 15, coercion: [2, 4], capacity: [3, 5], inclusiveness: [3, 4] },
    democracy: { name: "Liberal Democracy", icon: "🗳️", minLvl: 11, maxLvl: 15, coercion: [2, 3], capacity: [4, 5], inclusiveness: [4, 5] },
    oneParty: { name: "One-Party State", icon: "⭐", minLvl: 11, maxLvl: 15, coercion: [4, 5], capacity: [3, 5], inclusiveness: [1, 2] },
    junta: { name: "Military Junta", icon: "🎖️", minLvl: 7, maxLvl: 15, coercion: [4, 5], capacity: [2, 4], inclusiveness: [1, 2] },
    dictator: { name: "Personalist Dictatorship", icon: "🦹", minLvl: 7, maxLvl: 15, coercion: [4, 5], capacity: [2, 4], inclusiveness: [1, 2] },
    technocracy: { name: "Technocracy", icon: "🔬", minLvl: 11, maxLvl: 15, coercion: [3, 4], capacity: [5, 5], inclusiveness: [2, 3] },
    corporatist: { name: "Corporate State", icon: "🏢", minLvl: 12, maxLvl: 15, coercion: [3, 5], capacity: [4, 5], inclusiveness: [2, 3] },
    directdem: { name: "Direct Democracy", icon: "📡", minLvl: 13, maxLvl: 15, coercion: [1, 2], capacity: [4, 5], inclusiveness: [5, 5] },
    hive: { name: "Hive Mind", icon: "🧠", minLvl: 14, maxLvl: 15, coercion: [5, 5], capacity: [5, 5], inclusiveness: [5, 5] },
    interstellarfed: { name: "Interstellar Federation", icon: "🌌", minLvl: 15, maxLvl: 15, coercion: [2, 3], capacity: [5, 5], inclusiveness: [4, 5] },
    failed: { name: "Failed State", icon: "💀", minLvl: 1, maxLvl: 15, coercion: [1, 3], capacity: [1, 2], inclusiveness: [1, 3] },
  };

  var TRANSITIONS = {
    band: ["tribal", "chiefdom"],
    tribal: ["chiefdom", "theocracy"],
    chiefdom: ["theocracy", "citystate", "feudal", "patrimonial"],
    theocracy: ["patrimonial", "empire", "failed"],
    citystate: ["republic", "patrimonial", "empire"],
    feudal: ["absolutist", "patrimonial", "failed"],
    patrimonial: ["absolutist", "empire", "republic", "failed"],
    empire: ["absolutist", "failed", "republic"],
    republic: ["constitutional", "democracy", "empire", "failed"],
    absolutist: ["empire", "constitutional", "failed"],
    constitutional: ["republic", "democracy", "absolutist"],
    democracy: ["junta", "dictator", "technocracy", "directdem", "corporatist", "failed"],
    oneParty: ["constitutional", "dictator", "technocracy", "failed"],
    junta: ["oneParty", "absolutist", "patrimonial", "dictator", "failed"],
    dictator: ["absolutist", "junta", "failed"],
    technocracy: ["democracy", "corporatist", "oneParty"],
    corporatist: ["dictator", "democracy", "technocracy"],
    directdem: ["hive", "democracy"],
    hive: ["interstellarfed", "directdem"],
    interstellarfed: ["hive"],
    failed: ["junta", "patrimonial", "tribal", "band"],
  };

  function validRegimesForLevel(lvl) {
    var out = [];
    for (var id in REGIMES) {
      if (REGIMES.hasOwnProperty(id)) {
        var r = REGIMES[id];
        if (lvl >= r.minLvl && lvl <= r.maxLvl) out.push(id);
      }
    }
    return out;
  }

  function pickRegimeForLevel(lvl, terrain) {
    var valid = validRegimesForLevel(lvl);
    if (!valid.length) return "failed";
    var weights = { band: 0, tribal: 0, chiefdom: 0, theocracy: 0, citystate: 0, feudal: 0, patrimonial: 0, empire: 0, republic: 0, absolutist: 0, constitutional: 0, democracy: 0, oneParty: 0, junta: 0, dictator: 0, technocracy: 0, corporatist: 0, directdem: 0, hive: 0, interstellarfed: 0, failed: 0 };
    if (lvl <= 2) { weights.band = 5; weights.tribal = 3; }
    if (lvl === 3) { weights.tribal = 4; weights.chiefdom = 4; weights.theocracy = 2; }
    if (lvl === 4) { weights.chiefdom = 3; weights.citystate = terrain === "Coastal" ? 4 : 1; weights.feudal = 3; weights.patrimonial = 2; }
    if (lvl === 5) { weights.patrimonial = 3; weights.empire = 2; weights.republic = 2; weights.citystate = 2; }
    if (lvl === 6) { weights.absolutist = 4; weights.empire = 3; weights.republic = 2; weights.feudal = 1; }
    if (lvl === 7) { weights.absolutist = 3; weights.empire = 3; weights.republic = 3; weights.theocracy = 1; }
    if (lvl === 8) { weights.absolutist = 3; weights.empire = 2; weights.constitutional = 2; weights.republic = 2; weights.junta = 1; }
    if (lvl === 9) { weights.absolutist = 2; weights.constitutional = 3; weights.republic = 3; weights.empire = 1; }
    if (lvl === 10) { weights.constitutional = 3; weights.republic = 3; weights.absolutist = 2; weights.junta = 1; }
    if (lvl === 11) { weights.democracy = 3; weights.constitutional = 3; weights.oneParty = 2; weights.technocracy = 1; weights.junta = 1; }
    if (lvl === 12) { weights.democracy = 4; weights.oneParty = 2; weights.corporatist = 2; weights.technocracy = 1; weights.junta = 1; }
    if (lvl === 13) { weights.democracy = 3; weights.directdem = 2; weights.technocracy = 2; weights.corporatist = 2; weights.oneParty = 1; }
    if (lvl >= 14) { weights.directdem = 3; weights.technocracy = 3; weights.hive = 2; weights.interstellarfed = 2; }
    var pool = [];
    for (var i = 0; i < valid.length; i++) {
      if (weights[valid[i]] > 0) pool.push(valid[i]);
    }
    if (!pool.length) return valid[Math.floor(Math.random() * valid.length)];
    var total = 0;
    for (var j = 0; j < pool.length; j++) total += weights[pool[j]];
    var rnd = Math.random() * total;
    for (var k = 0; k < pool.length; k++) {
      rnd -= weights[pool[k]];
      if (rnd <= 0) return pool[k];
    }
    return pool[pool.length - 1];
  }

  function techsForLevel(lvl) {
    var set = new Set();
    for (var i = 0; i < TECHS.length; i++) {
      if (TECHS[i].level <= lvl) set.add(TECHS[i].id);
    }
    return set;
  }

  var RELIGIONS = ["Solar Cult", "Ancestor Worship", "Nature Spirits", "Sky Father", "Earth Mother", "Dualism", "Monotheism", "Philosophy", "Mystery Cult", "Secular Humanism", "AI Ethics Code"];
  var LANGUAGES = ["Proto-Northern", "Old Southern", "Eastern Tongue", "Western Speech", "Highland Dialect", "Coastal Creole", "River Language", "Plains Common", "Trade Pidgin"];
  var RESOURCES = ["Grain", "Livestock", "Timber", "Metals", "Spices", "Fish", "Stone", "Salt", "Gold", "Gems", "Coal", "Oil", "Uranium", "Rare Earths"];
  var CLIMATES = ["Tropical", "Arid", "Temperate", "Continental", "Polar", "Mediterranean"];
  var TERRAINS = ["Coastal", "Riverine", "Mountain", "Plains", "Forest", "Desert", "Island"];
  var TERRAIN_BARRIERS = {
    Mountain: { icon: "⛰️", moveCost: 3, tradeCost: 2 },
    Coastal: { icon: "🌊", moveCost: 1.5, tradeCost: 0.5 },
    Riverine: { icon: "🏞️", moveCost: 1.2, tradeCost: 0.5 },
    Forest: { icon: "🌲", moveCost: 2, tradeCost: 1.5 },
    Desert: { icon: "🏜️", moveCost: 2.5, tradeCost: 2 },
    Plains: { icon: "🌾", moveCost: 1, tradeCost: 1 },
    Island: { icon: "🏝️", moveCost: 2, tradeCost: 0.8 },
  };

  var rand = function (a, b) { return Math.floor(Math.random() * (b - a + 1)) + a; };
  var pick = function (arr) { return arr[rand(0, arr.length - 1)]; };
  var pickN = function (arr, n) {
    var copy = arr.slice();
    copy.sort(function () { return Math.random() - 0.5; });
    return copy.slice(0, n);
  };
  var clamp = function (v, a, b) { return Math.max(a, Math.min(b, v)); };
  var pfx = ["Al", "Kar", "Vel", "Nor", "Sul", "Zan", "Mor", "Tel", "Ash", "Bor", "Dra", "Fen", "Gal", "Hel", "Ith", "Khor", "Lum", "Myr", "Nyx", "Oth"];
  var sfx = ["ia", "and", "or", "um", "heim", "stan", "land", "ria", "via", "nia", "mark", "gard", "oth", "ur", "ax"];
  var rpfx = ["Aric", "Bran", "Cael", "Dorn", "Elric", "Finn", "Gorm", "Hald", "Ivar", "Jarl", "Kael", "Leif", "Morn", "Nial", "Oric"];
  var rsfx = ["us", "or", "an", "ius", "ax", "on", "ar", "ek", "im", "os"];
  var genName = function () { return pick(pfx) + pick(["a", "e", "i", "o", ""]) + pick(sfx); };
  var genRuler = function () { return pick(rpfx) + pick(rsfx) + " " + pick(["I", "II", "III", "the Great", "the Wise", "the Bold", "the Cruel", "the Young"]); };

  window.LEVELS = LEVELS;
  window.TECHS = TECHS;
  window.REGIMES = REGIMES;
  window.TRANSITIONS = TRANSITIONS;
  window.validRegimesForLevel = validRegimesForLevel;
  window.pickRegimeForLevel = pickRegimeForLevel;
  window.techsForLevel = techsForLevel;
  window.RELIGIONS = RELIGIONS;
  window.LANGUAGES = LANGUAGES;
  window.RESOURCES = RESOURCES;
  window.CLIMATES = CLIMATES;
  window.TERRAINS = TERRAINS;
  window.TERRAIN_BARRIERS = TERRAIN_BARRIERS;
  window.rand = rand;
  window.pick = pick;
  window.pickN = pickN;
  window.clamp = clamp;
  window.genName = genName;
  window.genRuler = genRuler;

  var REGIME_TRANSITIONS_LIST = [
    { from: "band", to: "tribal", label: "pop grows" },
    { from: "tribal", to: "chiefdom", label: "surplus+prestige" },
    { from: "tribal", to: "theocracy", label: "priestly class" },
    { from: "chiefdom", to: "theocracy", label: "religious legitimation" },
    { from: "chiefdom", to: "citystate", label: "urban trade" },
    { from: "chiefdom", to: "feudal", label: "territory expands" },
    { from: "chiefdom", to: "patrimonial", label: "personal authority" },
    { from: "theocracy", to: "patrimonial", label: "secular king" },
    { from: "theocracy", to: "empire", label: "holy empire" },
    { from: "citystate", to: "republic", label: "institutionalisation" },
    { from: "citystate", to: "patrimonial", label: "strongman capture" },
    { from: "feudal", to: "absolutist", label: "consolidation" },
    { from: "feudal", to: "patrimonial", label: "partial consolidation" },
    { from: "patrimonial", to: "absolutist", label: "tax capacity" },
    { from: "patrimonial", to: "empire", label: "conquest+bureaucracy" },
    { from: "empire", to: "absolutist", label: "exec dominates" },
    { from: "absolutist", to: "empire", label: "bureaucracy grows" },
    { from: "absolutist", to: "constitutional", label: "revenue bargain" },
    { from: "absolutist", to: "failed", label: "fiscal collapse" },
    { from: "republic", to: "constitutional", label: "institutions mature" },
    { from: "constitutional", to: "republic", label: "crown weakens" },
    { from: "constitutional", to: "democracy", label: "franchise expands" },
    { from: "republic", to: "democracy", label: "suffrage expands" },
    { from: "democracy", to: "junta", label: "polarisation+crisis" },
    { from: "democracy", to: "dictator", label: "norm erosion" },
    { from: "democracy", to: "technocracy", label: "complexity" },
    { from: "democracy", to: "directdem", label: "digital participation" },
    { from: "democracy", to: "corporatist", label: "regulatory capture" },
    { from: "junta", to: "oneParty", label: "institutionalises" },
    { from: "junta", to: "absolutist", label: "to executive" },
    { from: "dictator", to: "absolutist", label: "builds apparatus" },
    { from: "dictator", to: "junta", label: "military coup" },
    { from: "dictator", to: "failed", label: "collapse" },
    { from: "oneParty", to: "constitutional", label: "negotiated opening" },
    { from: "oneParty", to: "technocracy", label: "meritocratic reform" },
    { from: "technocracy", to: "democracy", label: "accountability" },
    { from: "technocracy", to: "corporatist", label: "market capture" },
    { from: "corporatist", to: "dictator", label: "authoritarian drift" },
    { from: "directdem", to: "hive", label: "AI integration" },
    { from: "hive", to: "interstellarfed", label: "expansion" },
    { from: "failed", to: "junta", label: "army stabilises" },
    { from: "failed", to: "patrimonial", label: "warlord consolidates" },
    { from: "failed", to: "tribal", label: "regression" },
    { from: "democracy", to: "interstellarfed", label: "multi-system" },
    { from: "technocracy", to: "interstellarfed", label: "coordinated expansion" },
  ];

  var R_BRACKETS = [
    { ids: ["band", "tribal"], col: 0, era: "Pre-State", lvl: "Lvl 1–3", color: "#78350f" },
    { ids: ["chiefdom", "theocracy"], col: 1, era: "Early State", lvl: "Lvl 2–5", color: "#b45309" },
    { ids: ["citystate", "feudal", "patrimonial"], col: 2, era: "Ancient", lvl: "Lvl 4–8", color: "#b08d57" },
    { ids: ["republic", "empire"], col: 3, era: "Classical", lvl: "Lvl 5–9", color: "#1d4ed8" },
    { ids: ["absolutist", "constitutional"], col: 4, era: "Medieval–Modern", lvl: "Lvl 6–11", color: "#5b21b6" },
    { ids: ["democracy", "oneParty", "junta", "dictator"], col: 5, era: "Modern", lvl: "Lvl 8–12", color: "#065f46" },
    { ids: ["technocracy", "corporatist", "failed"], col: 6, era: "Contemporary", lvl: "Lvl 11–13", color: "#0e7490" },
    { ids: ["directdem", "hive", "interstellarfed"], col: 7, era: "Advanced/Space", lvl: "Lvl 13–15", color: "#1e1b4b" },
  ];

  window.REGIME_TRANSITIONS_LIST = REGIME_TRANSITIONS_LIST;
  window.R_BRACKETS = R_BRACKETS;
})();
