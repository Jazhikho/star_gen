/** Terrain barriers, cultural options, and name-building constants for the history generator. */
const TERRAIN_BARRIERS = [
  { type: "mountains", icon: "⛰️", moveCost: 3, tradeCost: 2 },
  { type: "sea", icon: "🌊", moveCost: 2, tradeCost: 0.5 },
  { type: "river", icon: "🏞️", moveCost: 1.5, tradeCost: 0.5 },
  { type: "forest", icon: "🌲", moveCost: 2, tradeCost: 1.5 },
  { type: "desert", icon: "🏜️", moveCost: 2.5, tradeCost: 2 },
  { type: "plains", icon: "🌾", moveCost: 1, tradeCost: 1 },
];

const RELIGIONS = ["Solar Cult", "Ancestor Worship", "Nature Spirits", "Sky Father", "Earth Mother", "Dualism", "Monotheism", "Philosophy", "Mystery Cult"];
const LANGUAGES = ["Proto-Northern", "Old Southern", "Eastern Tongue", "Western Speech", "Highland Dialect", "Coastal Creole", "River Language", "Plains Common"];
const RESOURCES = ["Grain", "Livestock", "Timber", "Metals", "Spices", "Fish", "Stone", "Salt", "Gold", "Gems", "Coal", "Oil"];
const CLIMATES = ["Tropical", "Arid", "Temperate", "Continental", "Polar", "Mediterranean"];
const TERRAINS = ["Coastal", "Riverine", "Mountain", "Plains", "Forest", "Desert", "Island"];
const ERAS = ["Ancient", "Classical", "Medieval", "Early Modern", "Modern", "Contemporary"];

const PREFIXES = ["Al", "Kar", "Vel", "Nor", "Sul", "Zan", "Mor", "Tel", "Ash", "Bor", "Dra", "Fen", "Gal", "Hel", "Ith", "Khor", "Lum", "Myr", "Nyx", "Oth"];
const SUFFIXES = ["ia", "and", "or", "um", "heim", "stan", "land", "ria", "via", "nia", "mark", "gard", "oth", "ur", "ax"];
const RULER_PRE = ["Aric", "Bran", "Cael", "Dorn", "Elric", "Finn", "Gorm", "Hald", "Ivar", "Jarl", "Kael", "Leif", "Morn", "Nial", "Oric"];
const RULER_SUF = ["us", "or", "an", "ius", "ax", "on", "ar", "ek", "im", "os"];
const RULER_TITLES = ["I", "II", "III", "IV", "V", "the Great", "the Wise", "the Bold", "the Cruel"];

window.TERRAIN_BARRIERS = TERRAIN_BARRIERS;
window.RELIGIONS = RELIGIONS;
window.LANGUAGES = LANGUAGES;
window.RESOURCES = RESOURCES;
window.CLIMATES = CLIMATES;
window.TERRAINS = TERRAINS;
window.ERAS = ERAS;
window.PREFIXES = PREFIXES;
window.SUFFIXES = SUFFIXES;
window.RULER_PRE = RULER_PRE;
window.RULER_SUF = RULER_SUF;
window.RULER_TITLES = RULER_TITLES;
