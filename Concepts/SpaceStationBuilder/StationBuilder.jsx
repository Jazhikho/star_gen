/**
 * Traveller Station Builder — concept.
 * Extended ship design rules, auto-presets, tonnage budget scaling, classification system.
 * Single-file React (Babel) concept; no build step.
 */
var useState = React.useState;
var useMemo = React.useMemo;
var useCallback = React.useCallback;
var useRef = React.useRef;

// ═══════════════ GAME DATA ═══════════════
const TYPES = {
  highport_a: { n: "Class A Highport", min: 80000, max: 500000, d: "Full shipyard, refined fuel, excellent facilities" },
  highport_b: { n: "Class B Highport", min: 30000, max: 200000, d: "Spacecraft construction, refined fuel, good facilities" },
  highport_c: { n: "Class C Highport", min: 5000, max: 50000, d: "Limited repair, unrefined fuel, routine quality" },
  highport_d: { n: "Class D Highport", min: 1000, max: 10000, d: "Minimal facilities, unrefined fuel" },
  naval: { n: "Naval Base", min: 50000, max: 1000000, d: "Military operations & fleet support" },
  scout: { n: "Scout Base", min: 5000, max: 50000, d: "IISS operations & comms relay" },
  research: { n: "Research Station", min: 2000, max: 20000, d: "Scientific research facility" },
  mining: { n: "Mining Platform", min: 5000, max: 100000, d: "Resource extraction & processing" },
  trade: { n: "Trade Station", min: 10000, max: 100000, d: "Commercial hub & marketplace" },
  waystation: { n: "Waystation", min: 1000, max: 10000, d: "Deep space refueling point" },
  defense: { n: "Defense Platform", min: 2000, max: 50000, d: "System defense monitor" },
  freeport: { n: "Freeport", min: 10000, max: 200000, d: "Independent station, mixed use" },
  custom: { n: "Custom Station", min: 100, max: 2000000, d: "Design from scratch" },
};

const CONFIGS = {
  sphere: { n: "Sphere", cost: 1.0, arm: 1.0, d: "Most efficient volume-to-surface ratio" },
  cylinder: { n: "Cylinder", cost: 0.9, arm: 0.9, d: "Spinnable for centrifugal gravity" },
  ring: { n: "Ring/Torus", cost: 1.2, arm: 0.8, d: "Spin gravity, spacious interior" },
  modular: { n: "Modular", cost: 1.1, arm: 0.7, d: "Connected pods, easily expandable" },
  asteroid: { n: "Asteroid", cost: 0.6, arm: 1.5, d: "Hollowed rock, natural armor bonus" },
  platform: { n: "Open Platform", cost: 0.7, arm: 0.5, d: "Minimal hull, maximum docking access" },
};

const ARMOR = {
  none: { n: "None", tpph: 0, cpp: 0 },
  titanium: { n: "Titanium Steel", tpph: 0.02, cpp: 50000 },
  crystaliron: { n: "Crystaliron", tpph: 0.0125, cpp: 150000 },
  superdense: { n: "Bonded Superdense", tpph: 0.008, cpp: 250000 },
};

const PP = {
  fusion: { n: "Fusion Reactor", tpp: 1.0, cpp: 2000000, fpm: 0.1 },
  fission: { n: "Fission Pile", tpp: 1.5, cpp: 1000000, fpm: 0 },
  solar: { n: "Solar Array", tpp: 5.0, cpp: 500000, fpm: 0 },
};

const SENSORS = {
  basic: { n: "Basic (-4 DM)", t: 1, c: 25000, p: 0, dm: -4 },
  civilian: { n: "Civilian (-2 DM)", t: 3, c: 100000, p: 1, dm: -2 },
  military: { n: "Military (+0 DM)", t: 5, c: 1000000, p: 2, dm: 0 },
  improved: { n: "Improved (+1 DM)", t: 7, c: 4300000, p: 4, dm: 1 },
  advanced: { n: "Advanced (+2 DM)", t: 10, c: 8600000, p: 6, dm: 2 },
};

const COMPUTERS = {
  5: { n: "Computer/5", c: 30000 },
  10: { n: "Computer/10", c: 160000 },
  15: { n: "Computer/15", c: 2000000 },
  20: { n: "Computer/20", c: 5000000 },
  25: { n: "Computer/25", c: 10000000 },
  30: { n: "Computer/30", c: 20000000 },
  35: { n: "Computer/35", c: 30000000 },
};

const TURRETS = {
  single: { n: "Single Turret", t: 1, c: 200000, p: 1, hp: 1 },
  double: { n: "Double Turret", t: 1, c: 500000, p: 1, hp: 1 },
  triple: { n: "Triple Turret", t: 1, c: 1000000, p: 1, hp: 1 },
  barbette: { n: "Barbette", t: 5, c: 3000000, p: 3, hp: 1 },
};

const BAYS = {
  missile50: { n: "Missile Bay (50t)", t: 50, c: 12000000, p: 5 },
  laser50: { n: "Beam Laser Bay (50t)", t: 50, c: 9000000, p: 8 },
  particle50: { n: "Particle Bay (50t)", t: 50, c: 20000000, p: 10 },
  meson50: { n: "Meson Gun Bay (50t)", t: 50, c: 50000000, p: 15 },
  missile100: { n: "Missile Bay (100t)", t: 100, c: 24000000, p: 10 },
  laser100: { n: "Beam Laser Bay (100t)", t: 100, c: 18000000, p: 16 },
  particle100: { n: "Particle Bay (100t)", t: 100, c: 40000000, p: 20 },
  meson100: { n: "Meson Gun Bay (100t)", t: 100, c: 100000000, p: 30 },
};

const SCREENS_D = {
  damper: { n: "Nuclear Damper", t: 50, c: 50000000, p: 10 },
  meson: { n: "Meson Screen", t: 50, c: 60000000, p: 15 },
};

const DOCKING = {
  small_craft: { n: "Small Craft Bay", t: 30, c: 150000, d: "1 craft up to 100t" },
  standard: { n: "Standard Docking Berth", t: 150, c: 500000, d: "1 vessel up to 1,000t" },
  large: { n: "Large Docking Berth", t: 1500, c: 3000000, d: "1 vessel up to 5,000t" },
  capital: { n: "Capital Ship Berth", t: 5000, c: 10000000, d: "1 vessel up to 50,000t" },
  hangar_s: { n: "Enclosed Hangar (Small)", t: 200, c: 1000000, d: "1 craft <100t, full repair" },
  hangar_m: { n: "Enclosed Hangar (Medium)", t: 2000, c: 5000000, d: "1 vessel <1,000t, full repair" },
};

const ACCOM = {
  stateroom: { n: "Stateroom", t: 4, c: 50000, occ: 1, d: "Standard crew/passenger quarters" },
  high: { n: "High Stateroom", t: 6, c: 100000, occ: 1, d: "Superior passenger quarters" },
  luxury: { n: "Luxury Suite", t: 10, c: 250000, occ: 1, d: "VIP accommodations" },
  low: { n: "Low Berth", t: 0.5, c: 50000, occ: 0, d: "Cryogenic suspension pod" },
  barracks: { n: "Barracks (4-person)", t: 8, c: 25000, occ: 4, d: "Shared military quarters" },
  emergency: { n: "Emergency Low (×10)", t: 5, c: 100000, occ: 0, d: "Emergency cryogenic pods" },
};

const FACILITIES = {
  shipyard_s: { n: "Small Craft Yard (100t)", t: 400, c: 30000000, p: 10, d: "Build & repair craft ≤100t" },
  shipyard_m: { n: "Shipyard (1,000t)", t: 3000, c: 500000000, p: 50, d: "Build & repair ships ≤1,000t" },
  shipyard_l: { n: "Shipyard (5,000t)", t: 15000, c: 2000000000, p: 150, d: "Build & repair ships ≤5,000t" },
  repair: { n: "Repair Bay (100t)", t: 200, c: 10000000, p: 5, d: "Repairs only, ≤100t" },
  fuel_refinery: { n: "Fuel Refinery (500t/day)", t: 50, c: 5000000, p: 5, d: "Refines unprocessed fuel" },
  fuel_depot: { n: "Fuel Depot (1,000t)", t: 1000, c: 1000000, p: 0, d: "Bulk fuel storage" },
  commercial: { n: "Commercial District", t: 100, c: 2000000, p: 2, d: "Shops, markets, services" },
  medical: { n: "Medical Bay (10 beds)", t: 40, c: 5000000, p: 3, d: "Hospital facilities" },
  lab: { n: "Laboratory", t: 50, c: 4000000, p: 3, d: "Scientific research lab" },
  manufacturing: { n: "Manufacturing Plant", t: 100, c: 8000000, p: 8, d: "Production facility" },
  hydroponics: { n: "Hydroponics (feeds 50)", t: 100, c: 1000000, p: 3, d: "Food & air recycling" },
  recreation: { n: "Recreation Facility", t: 50, c: 1000000, p: 1, d: "Parks, gym, entertainment" },
  armory: { n: "Armory", t: 20, c: 2000000, p: 0, d: "Weapons storage & range" },
  warehouse: { n: "Warehouse (1,000t)", t: 1000, c: 500000, p: 0, d: "Organized cargo storage" },
  ore_process: { n: "Ore Processing", t: 200, c: 15000000, p: 10, d: "Mineral refining & smelting" },
  comms: { n: "Comms Array", t: 20, c: 10000000, p: 5, d: "Enhanced long-range comms" },
  cic: { n: "Combat Info Center", t: 30, c: 20000000, p: 5, d: "Military C&C" },
  prison: { n: "Brig / Detention", t: 50, c: 3000000, p: 1, d: "Secure detention" },
  customs: { n: "Customs & Immigration", t: 30, c: 1000000, p: 1, d: "Entry processing" },
  training: { n: "Training Facility", t: 60, c: 4000000, p: 2, d: "Crew training & simulation" },
};

const SOFTWARE = {
  maneuver0: { n: "Maneuver/0 (Station-Keeping)", c: 0 },
  library: { n: "Library", c: 0 },
  intellect: { n: "Intellect", c: 1000000 },
  fire1: { n: "Fire Control/1", c: 2000000 },
  fire2: { n: "Fire Control/2", c: 4000000 },
  fire3: { n: "Fire Control/3", c: 6000000 },
  autorepair: { n: "Auto-Repair/1", c: 5000000 },
  antihijack: { n: "Anti-Hijack/2", c: 8000000 },
  evade1: { n: "Evade/1", c: 1000000 },
  battlenet: { n: "Battle Network/1", c: 5000000 },
};

// ═══════════════ CLASSIFICATION REQUIREMENTS ═══════════════
const CLASSES = {
  starport_a: {
    name: "Class A Starport",
    icon: "🅰️",
    reqs: [
      ["Shipyard ≥1,000t capacity", (c) => (c.fac.shipyard_m || 0) + (c.fac.shipyard_l || 0) >= 1],
      ["Fuel refinery ≥1", (c) => (c.fac.fuel_refinery || 0) >= 1],
      ["Fuel depot ≥1", (c) => (c.fac.fuel_depot || 0) >= 1],
      ["Large or Capital berth ≥1", (c) => (c.dock.large || 0) + (c.dock.capital || 0) >= 1],
      ["Standard berths ≥4", (c) => (c.dock.standard || 0) >= 4],
      ["Small craft bays ≥2", (c) => (c.dock.small_craft || 0) + (c.dock.hangar_s || 0) >= 2],
      ["Commercial district ≥2", (c) => (c.fac.commercial || 0) >= 2],
      ["Medical bay ≥1", (c) => (c.fac.medical || 0) >= 1],
      ["Customs ≥1", (c) => (c.fac.customs || 0) >= 1],
      ["Warehouse ≥1", (c) => (c.fac.warehouse || 0) >= 1],
    ],
  },
  starport_b: {
    name: "Class B Starport",
    icon: "🅱️",
    reqs: [
      ["Craft yard or Shipyard ≥1", (c) => (c.fac.shipyard_s || 0) + (c.fac.shipyard_m || 0) + (c.fac.shipyard_l || 0) >= 1],
      ["Fuel refinery ≥1", (c) => (c.fac.fuel_refinery || 0) >= 1],
      ["Standard+ berths ≥3", (c) => (c.dock.standard || 0) + (c.dock.large || 0) + (c.dock.capital || 0) >= 3],
      ["Small craft bays ≥2", (c) => (c.dock.small_craft || 0) + (c.dock.hangar_s || 0) >= 2],
      ["Commercial ≥1", (c) => (c.fac.commercial || 0) >= 1],
      ["Medical bay ≥1", (c) => (c.fac.medical || 0) >= 1],
    ],
  },
  starport_c: {
    name: "Class C Starport",
    icon: "🇨",
    reqs: [
      ["Repair bay or yard ≥1", (c) => (c.fac.repair || 0) + (c.fac.shipyard_s || 0) + (c.fac.shipyard_m || 0) + (c.fac.shipyard_l || 0) >= 1],
      ["Docking berths ≥2", (c) => (c.dock.standard || 0) + (c.dock.large || 0) + (c.dock.capital || 0) >= 2],
      ["Fuel available", (c) => (c.fac.fuel_depot || 0) + (c.fac.fuel_refinery || 0) >= 1],
    ],
  },
  starport_d: {
    name: "Class D Starport",
    icon: "🇩",
    reqs: [
      ["Any docking ≥1", (c) => Object.values(c.dock).reduce(function (a, b) { return a + b; }, 0) >= 1],
      ["Fuel depot ≥1", (c) => (c.fac.fuel_depot || 0) >= 1],
    ],
  },
  naval_base: {
    name: "Naval Base",
    icon: "⚓",
    reqs: [
      ["Combat Information Center", (c) => (c.fac.cic || 0) >= 1],
      ["Armory ≥1", (c) => (c.fac.armory || 0) >= 1],
      ["Military command center", (c) => c.cmdType === "military"],
      ["Military+ sensors", (c) => ["military", "improved", "advanced"].indexOf(c.sensors) >= 0],
      ["Weapons ≥20% hardpoints", (c) => c.hp > 0 && c.wpnHP / c.hp >= 0.2],
      ["Docking berths ≥6", (c) => Object.values(c.dock).reduce(function (a, b) { return a + b; }, 0) >= 6],
      ["Training facility", (c) => (c.fac.training || 0) >= 1],
    ],
  },
  scout_base: {
    name: "Scout Base",
    icon: "🔭",
    reqs: [
      ["Comms array ≥1", (c) => (c.fac.comms || 0) >= 1],
      ["Laboratory ≥1", (c) => (c.fac.lab || 0) >= 1],
      ["Improved+ sensors", (c) => ["improved", "advanced"].indexOf(c.sensors) >= 0],
      ["Small craft bays ≥2", (c) => (c.dock.small_craft || 0) + (c.dock.hangar_s || 0) >= 2],
    ],
  },
  research_stn: {
    name: "Research Station",
    icon: "🔬",
    reqs: [
      ["Laboratories ≥2", (c) => (c.fac.lab || 0) >= 2],
      ["Computer/15+", (c) => c.computer >= 15],
      ["Comms array ≥1", (c) => (c.fac.comms || 0) >= 1],
    ],
  },
  mining_platform: {
    name: "Mining Platform",
    icon: "⛏️",
    reqs: [
      ["Ore processing ≥1", (c) => (c.fac.ore_process || 0) >= 1],
      ["Warehouse ≥1", (c) => (c.fac.warehouse || 0) >= 1],
      ["Small craft bay ≥1", (c) => (c.dock.small_craft || 0) + (c.dock.hangar_s || 0) >= 1],
    ],
  },
  trade_stn: {
    name: "Trade Station",
    icon: "💱",
    reqs: [
      ["Commercial ≥3", (c) => (c.fac.commercial || 0) >= 3],
      ["Warehouse ≥2", (c) => (c.fac.warehouse || 0) >= 2],
      ["Customs ≥1", (c) => (c.fac.customs || 0) >= 1],
      ["Standard+ berths ≥5", (c) => (c.dock.standard || 0) + (c.dock.large || 0) + (c.dock.capital || 0) >= 5],
    ],
  },
  defense_platform: {
    name: "Defense Platform",
    icon: "🛡️",
    reqs: [
      ["CIC", (c) => (c.fac.cic || 0) >= 1],
      ["Advanced sensors", (c) => c.sensors === "advanced"],
      ["Weapons ≥30% hardpoints", (c) => c.hp > 0 && c.wpnHP / c.hp >= 0.3],
      ["Screens ≥1", (c) => Object.values(c.screens).reduce(function (a, b) { return a + b; }, 0) >= 1],
      ["Military command", (c) => c.cmdType === "military"],
    ],
  },
  waystation: {
    name: "Waystation",
    icon: "⛽",
    reqs: [
      ["Fuel depot ≥2", (c) => (c.fac.fuel_depot || 0) >= 2],
      ["Docking ≥2", (c) => Object.values(c.dock).reduce(function (a, b) { return a + b; }, 0) >= 2],
    ],
  },
};

// ═══════════════ AUTO PRESETS ═══════════════
function Sc(h, div, mn) {
  if (mn === undefined) mn = 0;
  return div <= 0 ? mn : Math.max(mn, Math.floor(h / div));
}
var AP = {
  highport_a: {
    cmd: "military",
    comp: 35,
    sen: "advanced",
    ppT: "fusion",
    ppM: 1.25,
    fm: 12,
    aT: "crystaliron",
    aR: [30000, 3, 12],
    sw: ["library", "maneuver0", "intellect", "fire2", "autorepair", "antihijack", "battlenet"],
    f: { shipyard_l: [400000, 0], shipyard_m: [60000, 1], fuel_refinery: [15000, 2], fuel_depot: [12000, 2], commercial: [8000, 3], medical: [25000, 1], warehouse: [12000, 2], customs: [40000, 1], recreation: [10000, 2], comms: [0, 1], lab: [100000, 0], hydroponics: [15000, 1] },
    dk: { small_craft: [6000, 3], standard: [4000, 6], large: [25000, 1], capital: [500000, 0], hangar_s: [12000, 2], hangar_m: [80000, 0] },
    wP: 0.2,
    wM: { triple: 0.5, barbette: 0.3, double: 0.2 },
    bP: 0.01,
    bM: { missile50: 0.4, laser50: 0.3, particle50: 0.3 },
    sc: { damper: [120000, 0], meson: [250000, 0] },
    oR: 0.2,
  },
  highport_b: {
    cmd: "standard",
    comp: 25,
    sen: "improved",
    ppT: "fusion",
    ppM: 1.2,
    fm: 12,
    aT: "crystaliron",
    aR: [25000, 2, 8],
    sw: ["library", "maneuver0", "intellect", "fire1", "autorepair", "antihijack"],
    f: { shipyard_m: [100000, 0], shipyard_s: [30000, 1], fuel_refinery: [18000, 1], fuel_depot: [15000, 1], commercial: [10000, 2], medical: [35000, 1], warehouse: [15000, 1], customs: [50000, 1], recreation: [15000, 1], comms: [0, 1] },
    dk: { small_craft: [7000, 2], standard: [4000, 5], large: [25000, 1], hangar_s: [15000, 1], hangar_m: [80000, 0] },
    wP: 0.12,
    wM: { triple: 0.4, double: 0.4, barbette: 0.2 },
    bP: 0.004,
    bM: { missile50: 0.5, laser50: 0.5 },
    sc: { damper: [200000, 0] },
    oR: 0.2,
  },
  highport_c: {
    cmd: "standard",
    comp: 15,
    sen: "civilian",
    ppT: "fusion",
    ppM: 1.15,
    fm: 6,
    aT: "titanium",
    aR: [12000, 1, 4],
    sw: ["library", "maneuver0", "fire1"],
    f: { repair: [12000, 1], fuel_refinery: [25000, 0], fuel_depot: [10000, 1], commercial: [15000, 1], medical: [40000, 0], warehouse: [20000, 0], customs: [50000, 0] },
    dk: { small_craft: [8000, 1], standard: [5000, 2], large: [40000, 0] },
    wP: 0.06,
    wM: { double: 0.6, single: 0.4 },
    bP: 0,
    bM: {},
    sc: {},
    oR: 0.15,
  },
  highport_d: {
    cmd: "standard",
    comp: 10,
    sen: "basic",
    ppT: "fission",
    ppM: 1.1,
    fm: 0,
    aT: "titanium",
    aR: [6000, 1, 2],
    sw: ["library", "maneuver0"],
    f: { repair: [20000, 0], fuel_depot: [5000, 1] },
    dk: { small_craft: [5000, 1], standard: [3000, 1] },
    wP: 0.04,
    wM: { single: 0.7, double: 0.3 },
    bP: 0,
    bM: {},
    sc: {},
    oR: 0.1,
  },
  naval: {
    cmd: "military",
    comp: 35,
    sen: "advanced",
    ppT: "fusion",
    ppM: 1.3,
    fm: 18,
    aT: "superdense",
    aR: [35000, 4, 15],
    sw: ["library", "maneuver0", "intellect", "fire3", "autorepair", "antihijack", "battlenet", "evade1"],
    f: { shipyard_l: [350000, 0], shipyard_m: [100000, 1], fuel_refinery: [12000, 2], fuel_depot: [10000, 2], medical: [20000, 1], armory: [25000, 1], cic: [0, 1], training: [35000, 1], warehouse: [15000, 1], recreation: [18000, 1], hydroponics: [15000, 1], prison: [150000, 0] },
    dk: { small_craft: [4000, 4], standard: [3000, 8], large: [15000, 2], capital: [100000, 1], hangar_s: [8000, 3], hangar_m: [50000, 1] },
    wP: 0.35,
    wM: { triple: 0.35, barbette: 0.45, double: 0.2 },
    bP: 0.02,
    bM: { missile50: 0.3, laser50: 0.2, particle50: 0.3, meson50: 0.2 },
    sc: { damper: [100000, 1], meson: [150000, 0] },
    oR: 0.25,
  },
  scout: {
    cmd: "standard",
    comp: 20,
    sen: "improved",
    ppT: "fusion",
    ppM: 1.2,
    fm: 12,
    aT: "titanium",
    aR: [18000, 1, 6],
    sw: ["library", "maneuver0", "intellect", "fire1", "autorepair"],
    f: { shipyard_s: [20000, 0], repair: [12000, 1], fuel_refinery: [18000, 1], fuel_depot: [15000, 1], lab: [8000, 1], comms: [0, 1], medical: [30000, 0], recreation: [25000, 0], hydroponics: [25000, 0] },
    dk: { small_craft: [5000, 2], standard: [6000, 2], large: [30000, 0], hangar_s: [10000, 1] },
    wP: 0.08,
    wM: { double: 0.5, triple: 0.5 },
    bP: 0,
    bM: {},
    sc: {},
    oR: 0.2,
  },
  research: {
    cmd: "standard",
    comp: 20,
    sen: "improved",
    ppT: "fusion",
    ppM: 1.2,
    fm: 12,
    aT: "titanium",
    aR: [12000, 1, 4],
    sw: ["library", "maneuver0", "intellect", "autorepair"],
    f: { lab: [3000, 2], medical: [10000, 1], comms: [0, 1], hydroponics: [10000, 1], recreation: [10000, 0] },
    dk: { small_craft: [5000, 1], standard: [8000, 1], hangar_s: [12000, 1] },
    wP: 0.04,
    wM: { double: 0.6, single: 0.4 },
    bP: 0,
    bM: {},
    sc: {},
    oR: 0.25,
  },
  mining: {
    cmd: "standard",
    comp: 15,
    sen: "civilian",
    ppT: "fission",
    ppM: 1.2,
    fm: 0,
    aT: "titanium",
    aR: [18000, 1, 6],
    sw: ["library", "maneuver0", "fire1"],
    f: { ore_process: [10000, 1], manufacturing: [20000, 0], repair: [15000, 1], fuel_depot: [18000, 1], warehouse: [8000, 1], medical: [30000, 0], recreation: [25000, 0], hydroponics: [20000, 0] },
    dk: { small_craft: [6000, 2], standard: [8000, 2], large: [35000, 0], hangar_s: [12000, 1] },
    wP: 0.06,
    wM: { single: 0.4, double: 0.6 },
    bP: 0,
    bM: {},
    sc: {},
    oR: 0.15,
  },
  trade: {
    cmd: "standard",
    comp: 20,
    sen: "civilian",
    ppT: "fusion",
    ppM: 1.2,
    fm: 12,
    aT: "crystaliron",
    aR: [25000, 2, 6],
    sw: ["library", "maneuver0", "intellect", "fire1", "antihijack"],
    f: { commercial: [5000, 3], warehouse: [6000, 2], customs: [20000, 1], fuel_refinery: [18000, 1], fuel_depot: [10000, 1], medical: [25000, 1], recreation: [8000, 2], hydroponics: [15000, 1], comms: [0, 1] },
    dk: { small_craft: [6000, 2], standard: [3000, 5], large: [15000, 2], capital: [120000, 0], hangar_s: [12000, 1], hangar_m: [60000, 0] },
    wP: 0.08,
    wM: { double: 0.5, triple: 0.5 },
    bP: 0.004,
    bM: { missile50: 0.6, laser50: 0.4 },
    sc: { damper: [200000, 0] },
    oR: 0.2,
  },
  waystation: {
    cmd: "standard",
    comp: 10,
    sen: "civilian",
    ppT: "solar",
    ppM: 1.15,
    fm: 0,
    aT: "titanium",
    aR: [6000, 1, 2],
    sw: ["library", "maneuver0"],
    f: { fuel_depot: [2000, 2], fuel_refinery: [10000, 0], repair: [20000, 0], medical: [25000, 0] },
    dk: { small_craft: [5000, 1], standard: [2500, 2] },
    wP: 0.04,
    wM: { single: 0.6, double: 0.4 },
    bP: 0,
    bM: {},
    sc: {},
    oR: 0.1,
  },
  defense: {
    cmd: "military",
    comp: 30,
    sen: "advanced",
    ppT: "fusion",
    ppM: 1.3,
    fm: 18,
    aT: "superdense",
    aR: [12000, 4, 15],
    sw: ["library", "maneuver0", "intellect", "fire3", "autorepair", "antihijack", "battlenet", "evade1"],
    f: { cic: [0, 1], armory: [12000, 1], medical: [18000, 1], training: [25000, 0], fuel_depot: [12000, 1] },
    dk: { small_craft: [6000, 1], standard: [10000, 1], hangar_s: [10000, 1] },
    wP: 0.45,
    wM: { triple: 0.3, barbette: 0.5, double: 0.2 },
    bP: 0.035,
    bM: { missile50: 0.25, laser50: 0.25, particle50: 0.25, meson50: 0.25 },
    sc: { damper: [40000, 1], meson: [60000, 1] },
    oR: 0.25,
  },
  freeport: {
    cmd: "standard",
    comp: 20,
    sen: "military",
    ppT: "fusion",
    ppM: 1.2,
    fm: 12,
    aT: "crystaliron",
    aR: [25000, 2, 8],
    sw: ["library", "maneuver0", "intellect", "fire1", "autorepair", "antihijack"],
    f: { repair: [18000, 1], fuel_refinery: [15000, 1], fuel_depot: [10000, 1], commercial: [5000, 3], warehouse: [8000, 2], medical: [25000, 1], recreation: [8000, 2], customs: [35000, 1], hydroponics: [15000, 1], comms: [0, 1] },
    dk: { small_craft: [5000, 3], standard: [3000, 5], large: [15000, 1], hangar_s: [10000, 1], hangar_m: [60000, 0] },
    wP: 0.12,
    wM: { triple: 0.5, barbette: 0.2, double: 0.3 },
    bP: 0.006,
    bM: { missile50: 0.5, laser50: 0.5 },
    sc: { damper: [150000, 0] },
    oR: 0.2,
  },
  custom: {
    cmd: "standard",
    comp: 10,
    sen: "basic",
    ppT: "fusion",
    ppM: 1.15,
    fm: 6,
    aT: "none",
    aR: [1, 0, 0],
    sw: ["library", "maneuver0"],
    f: {},
    dk: {},
    wP: 0,
    wM: {},
    bP: 0,
    bM: {},
    sc: {},
    oR: 0.15,
  },
};

// ═══════════════ HELPERS ═══════════════
function fmt(n) {
  if (n >= 1e9) return "Cr " + (n / 1e9).toFixed(2) + "B";
  if (n >= 1e6) return "Cr " + (n / 1e6).toFixed(2) + "M";
  if (n >= 1e3) return "Cr " + (n / 1e3).toFixed(1) + "K";
  return "Cr " + n.toFixed(0);
}
function fmtT(n) {
  return n.toLocaleString() + "t";
}
function hullCPT(t) {
  return t <= 2000 ? 50000 : t <= 10000 ? 40000 : t <= 100000 ? 30000 : 25000;
}
function clamp(v, lo, hi) {
  return Math.max(lo, Math.min(hi, v));
}
function initC(obj) {
  return Object.fromEntries(Object.keys(obj).map(function (k) { return [k, 0]; }));
}
function sumO(c, d, f) {
  return Object.entries(c).reduce(function (s, kv) {
    return s + (d[kv[0]] ? d[kv[0]][f] : 0) * kv[1];
  }, 0);
}
function scaleO(c, f) {
  var r = {};
  Object.entries(c).forEach(function (kv) {
    r[kv[0]] = Math.max(0, Math.floor(kv[1] * f));
  });
  return r;
}
function applyScl(tmpl, h) {
  var r = {};
  Object.entries(tmpl).forEach(function (kv) {
    r[kv[0]] = Sc(h, kv[1][0], kv[1][1]);
  });
  return r;
}
function autoFac(type, h) {
  var a = AP[type];
  var base = initC(FACILITIES);
  if (a && a.f) Object.assign(base, applyScl(a.f, h));
  return base;
}
function autoDock(type, h) {
  var a = AP[type];
  var base = initC(DOCKING);
  if (a && a.dk) Object.assign(base, applyScl(a.dk, h));
  return base;
}
function autoScr(type, h) {
  var a = AP[type];
  var base = initC(SCREENS_D);
  if (a && a.sc) Object.assign(base, applyScl(a.sc, h));
  return base;
}
function autoWeapons(type, h) {
  var a = AP[type] || AP.custom;
  var hp = Math.floor(h / 100);
  var tN = Math.floor(hp * (a.wP || 0));
  var base = initC(TURRETS);
  var rem = tN;
  var sorted = Object.entries(a.wM || {}).sort(function (x, y) { return y[1] - x[1]; });
  sorted.forEach(function (kv, i) {
    var n = i === sorted.length - 1 ? rem : Math.floor(tN * kv[1]);
    base[kv[0]] = (base[kv[0]] || 0) + n;
    rem -= n;
  });
  var bB = initC(BAYS);
  var bT = Math.floor(h * (a.bP || 0));
  var bR = bT;
  Object.entries(a.bM || {}).sort(function (x, y) { return y[1] - x[1]; }).forEach(function (kv, i, arr) {
    var bt = BAYS[kv[0]].t;
    var al = i === arr.length - 1 ? bR : Math.floor(bT * kv[1]);
    bB[kv[0]] = Math.floor(al / bt);
    bR -= bB[kv[0]] * bt;
  });
  return { turrets: base, bays: bB };
}
function autoArmorPts(type, h) {
  var a = AP[type];
  var aR = (a && a.aR) ? a.aR : [1, 0, 0];
  return Math.min(aR[2], Math.max(aR[1], Math.floor(h / aR[0])));
}
function autoAccom(crew, r) {
  var b = initC(ACCOM);
  var off = Math.max(2, Math.ceil(crew * r));
  var enl = Math.max(0, crew - off);
  b.stateroom = off;
  b.barracks = Math.ceil(enl / 4);
  b.emergency = Math.max(1, Math.ceil(crew / 50));
  return b;
}

var INIT = {
  type: "highport_b",
  name: "Unnamed Station",
  hullTons: 50000,
  config: "sphere",
  armorType: "crystaliron",
  armorPts: 3,
  ppType: "fusion",
  ppRating: 60,
  fuelMonths: 12,
  cmdType: "standard",
  computer: 10,
  sensors: "civilian",
  sw: ["library", "maneuver0"],
  turrets: initC(TURRETS),
  bays: initC(BAYS),
  screens: initC(SCREENS_D),
  docking: initC(DOCKING),
  accom: initC(ACCOM),
  fac: initC(FACILITIES),
  auto: { engineering: true, command: true, defenses: true, docking: true, quarters: true, facilities: true },
  officerRatio: 0.2,
};

// ═══════════════ UI COMPONENTS ═══════════════
function Sel(props) {
  var label = props.label, value = props.value, onChange = props.onChange, options = props.options, desc = props.desc, disabled = props.disabled;
  return (
    <div className="mb-3">
      <label className="block text-xs text-gray-400 mb-1">{label}</label>
      <select
        value={value}
        onChange={function (e) { onChange(e.target.value); }}
        disabled={disabled}
        className={"w-full bg-gray-800 border border-gray-600 text-gray-100 rounded px-2 py-1.5 text-sm focus:border-cyan-500 focus:outline-none" + (disabled ? " opacity-60" : "")}
      >
        {options.map(function (kv) { return <option key={kv[0]} value={kv[0]}>{kv[1]}</option>; })}
      </select>
      {desc ? <p className="text-xs text-gray-500 mt-0.5">{desc}</p> : null}
    </div>
  );
}
function Num(props) {
  var label = props.label, value = props.value, onChange = props.onChange, min = props.min, max = props.max, step = props.step, desc = props.desc, unit = props.unit, disabled = props.disabled;
  if (min === undefined) min = 0;
  if (max === undefined) max = 999999;
  if (step === undefined) step = 1;
  if (unit === undefined) unit = "";
  return (
    <div className="mb-3">
      <label className="block text-xs text-gray-400 mb-1">{label}</label>
      <div className="flex items-center gap-2">
        <input
          type="number"
          value={value}
          min={min}
          max={max}
          step={step}
          disabled={disabled}
          onChange={function (e) { onChange(Number(e.target.value)); }}
          className={"w-full bg-gray-800 border border-gray-600 text-gray-100 rounded px-2 py-1.5 text-sm focus:border-cyan-500 focus:outline-none" + (disabled ? " opacity-60" : "")}
        />
        {unit ? <span className="text-xs text-gray-400 whitespace-nowrap">{unit}</span> : null}
      </div>
      {desc ? <p className="text-xs text-gray-500 mt-0.5">{desc}</p> : null}
    </div>
  );
}
function Counter(props) {
  var label = props.label, count = props.count, onChange = props.onChange, desc = props.desc, extra = props.extra, disabled = props.disabled;
  return (
    <div className={"flex items-center justify-between py-1.5 border-b border-gray-800" + (disabled ? " opacity-50" : "")}>
      <div className="flex-1 min-w-0 pr-2">
        <div className="text-sm text-gray-200 truncate">{label}</div>
        {desc ? <div className="text-xs text-gray-500 truncate">{desc}</div> : null}
        {extra ? <div className="text-xs text-cyan-600">{extra}</div> : null}
      </div>
      <div className="flex items-center gap-1 shrink-0">
        <button
          type="button"
          onClick={function () { if (!disabled) onChange(Math.max(0, count - 1)); }}
          disabled={disabled}
          className="w-7 h-7 bg-gray-700 hover:bg-gray-600 text-gray-200 rounded text-sm flex items-center justify-center"
        >−</button>
        <span className="w-10 text-center text-sm text-gray-100">{count}</span>
        <button
          type="button"
          onClick={function () { if (!disabled) onChange(count + 1); }}
          disabled={disabled}
          className="w-7 h-7 bg-gray-700 hover:bg-gray-600 text-gray-200 rounded text-sm flex items-center justify-center"
        >+</button>
      </div>
    </div>
  );
}
function Badge(props) {
  var label = props.label, value = props.value, color = props.color;
  if (color === undefined) color = "cyan";
  return (
    <div className="text-center px-1">
      <div className="text-xs text-gray-500">{label}</div>
      <div className={"text-xs font-bold text-" + color + "-400"}>{value}</div>
    </div>
  );
}
function AutoBanner(props) {
  var auto = props.auto, onToggle = props.onToggle, label = props.label;
  return (
    <div className="flex items-center justify-between bg-gray-800 border border-gray-700 rounded-lg px-3 py-2 mb-4">
      <div className="flex items-center gap-2">
        <span className={"w-2 h-2 rounded-full " + (auto ? "bg-cyan-400" : "bg-gray-600")} />
        <span className="text-xs text-gray-300">{label || "Auto-populate from station class"}</span>
      </div>
      <button
        type="button"
        onClick={onToggle}
        className={"px-3 py-1 rounded text-xs font-medium " + (auto ? "bg-cyan-700 text-cyan-100 hover:bg-cyan-600" : "bg-gray-700 text-gray-400 hover:bg-gray-600")}
      >
        {auto ? "AUTO" : "MANUAL"}
      </button>
    </div>
  );
}

// ═══════════════ MAIN ═══════════════
function StationBuilder() {
  var _s = useState(INIT);
  var d = _s[0];
  var setD = _s[1];
  var _tab = useState(0);
  var tab = _tab[0];
  var setTab = _tab[1];
  var _showExport = useState(false);
  var showExport = _showExport[0];
  var setShowExport = _showExport[1];
  var _expandedClass = useState(null);
  var expandedClass = _expandedClass[0];
  var setExpandedClass = _expandedClass[1];
  var exportRef = useRef(null);

  var upd = useCallback(function (k, v) {
    setD(function (p) { var next = {}; for (var key in p) next[key] = p[key]; next[k] = v; return next; });
  }, []);
  var updN = useCallback(function (cat, k, v) {
    setD(function (p) {
      var next = {}; for (var key in p) next[key] = p[key];
      next[cat] = {}; for (var key in p[cat]) next[cat][key] = p[cat][key];
      next[cat][k] = v;
      return next;
    });
  }, []);
  var toggleSW = useCallback(function (k) {
    setD(function (p) {
      var sw = p.sw.indexOf(k) >= 0 ? p.sw.filter(function (s) { return s !== k; }) : p.sw.concat([k]);
      var next = {}; for (var key in p) next[key] = p[key];
      next.sw = sw;
      return next;
    });
  }, []);
  var toggleAuto = useCallback(function (section, calc) {
    setD(function (p) {
      var was = p.auto[section];
      var next = {}; for (var key in p) next[key] = p[key];
      next.auto = {}; for (var key in p.auto) next.auto[key] = p.auto[key];
      next.auto[section] = !was;
      if (was && calc) {
        if (section === "engineering") { next.ppType = calc.ePPType; next.ppRating = calc.ePPRating; next.fuelMonths = calc.eFuelMonths; }
        if (section === "command") { next.cmdType = calc.eCmdType; next.computer = calc.eComputer; next.sensors = calc.eSensors; next.sw = calc.eSW.slice(); }
        if (section === "defenses") { next.armorType = calc.eArmorType; next.armorPts = calc.eArmorPts; next.turrets = Object.assign({}, calc.eTurrets); next.bays = Object.assign({}, calc.eBays); next.screens = Object.assign({}, calc.eScreens); }
        if (section === "docking") next.docking = Object.assign({}, calc.eDocking);
        if (section === "quarters") next.accom = Object.assign({}, calc.eAccom);
        if (section === "facilities") next.fac = Object.assign({}, calc.eFac);
      }
      return next;
    });
  }, []);
  var applyType = useCallback(function (typeKey) {
    var t = TYPES[typeKey];
    var tons = clamp(d.hullTons, t.min, t.max);
    setD(function (p) { var next = {}; for (var key in p) next[key] = p[key]; next.type = typeKey; next.hullTons = tons; return next; });
  }, [d.hullTons]);

  var calc = useMemo(function () {
    var h = d.hullTons;
    var cfg = CONFIGS[d.config];
    var a = AP[d.type] || AP.custom;
    var hullCost = h * hullCPT(h) * cfg.cost;
    var structHP = Math.ceil(h / 50);
    var hardpoints = Math.floor(h / 100);

    var eFac = d.auto.facilities ? autoFac(d.type, h) : Object.assign({}, d.fac);
    var eDocking = d.auto.docking ? autoDock(d.type, h) : Object.assign({}, d.docking);
    var eCmdType = d.auto.command ? a.cmd : d.cmdType;
    var eComputer = d.auto.command ? a.comp : d.computer;
    var eSensors = d.auto.command ? a.sen : d.sensors;
    var eSW = d.auto.command ? (a.sw ? a.sw.slice() : []) : d.sw.slice();
    var eArmorType = d.auto.defenses ? a.aT : d.armorType;
    var eArmorPts = d.auto.defenses ? autoArmorPts(d.type, h) : d.armorPts;
    var autoWpn = autoWeapons(d.type, h);
    var eTurrets = d.auto.defenses ? autoWpn.turrets : Object.assign({}, d.turrets);
    var eBays = d.auto.defenses ? autoWpn.bays : Object.assign({}, d.bays);
    var eScreens = d.auto.defenses ? autoScr(d.type, h) : Object.assign({}, d.screens);
    var ePPType = d.auto.engineering ? a.ppT : d.ppType;
    var eFuelMonths = d.auto.engineering ? a.fm : d.fuelMonths;

    var arm = ARMOR[eArmorType];
    var armorTons = eArmorType === "none" ? 0 : Math.ceil(h * arm.tpph * eArmorPts / cfg.arm);
    var cmdPct = eCmdType === "military" ? 0.025 : 0.02;
    var cmdTons = Math.max(20, Math.ceil(h * cmdPct));
    var sen = SENSORS[eSensors];
    var fixedTons = armorTons + cmdTons + sen.t;
    var cmdPower = Math.ceil(cmdTons * 0.2);

    var mFac = sumO(eFac, FACILITIES, "t");
    var mDock = sumO(eDocking, DOCKING, "t");
    var mWpn = sumO(eTurrets, TURRETS, "t") + sumO(eBays, BAYS, "t") + sumO(eScreens, SCREENS_D, "t");
    var moduleTons = mFac + mDock + mWpn;
    var estPow = cmdPower + sen.p + sumO(eFac, FACILITIES, "p") + sumO(eTurrets, TURRETS, "p") + sumO(eBays, BAYS, "p") + sumO(eScreens, SCREENS_D, "p");
    var estPPR = d.auto.engineering ? Math.max(10, Math.ceil(estPow * (a.ppM || 1.2))) : d.ppRating;
    var pp = PP[ePPType];
    var estPPT = Math.ceil(estPPR * pp.tpp);
    var estFuel = Math.ceil(estPPR * pp.fpm * eFuelMonths);
    var estCrewT = Math.ceil(h / 350) * 3;
    var totalEst = fixedTons + moduleTons + estPPT + estFuel + estCrewT;
    var budget = h * 0.88;

    if (totalEst > budget && moduleTons > 0) {
      var target = Math.max(moduleTons * 0.3, budget - fixedTons - estPPT - estFuel - estCrewT);
      var scale = Math.max(0.25, Math.min(1, target / moduleTons));
      if (d.auto.facilities) eFac = scaleO(eFac, scale);
      if (d.auto.docking) eDocking = scaleO(eDocking, scale);
      if (d.auto.defenses) { eTurrets = scaleO(eTurrets, scale); eBays = scaleO(eBays, scale); eScreens = scaleO(eScreens, scale); }
    }

    var armorCost = armorTons * arm.cpp;
    var effectiveArmor = eArmorType === "none" ? 0 : Math.floor(eArmorPts * cfg.arm);
    var cmdCostPer = eCmdType === "military" ? 750000 : 500000;
    var cmdCost = cmdTons * cmdCostPer;
    var compCost = (COMPUTERS[eComputer] && COMPUTERS[eComputer].c) ? COMPUTERS[eComputer].c : 0;

    var wpnTons = 0, wpnCost = 0, wpnPower = 0, wpnHP = 0;
    Object.entries(eTurrets).forEach(function (kv) {
      var w = TURRETS[kv[0]];
      wpnTons += w.t * kv[1]; wpnCost += w.c * kv[1]; wpnPower += w.p * kv[1]; wpnHP += (w.hp || 0) * kv[1];
    });
    Object.entries(eBays).forEach(function (kv) {
      var w = BAYS[kv[0]];
      wpnTons += w.t * kv[1]; wpnCost += w.c * kv[1]; wpnPower += w.p * kv[1];
    });
    var scrTons = 0, scrCost = 0, scrPower = 0;
    Object.entries(eScreens).forEach(function (kv) {
      var s = SCREENS_D[kv[0]];
      scrTons += s.t * kv[1]; scrCost += s.c * kv[1]; scrPower += s.p * kv[1];
    });
    var facTons = 0, facCost = 0, facPower = 0;
    Object.entries(eFac).forEach(function (kv) {
      var f = FACILITIES[kv[0]];
      if (f) { facTons += f.t * kv[1]; facCost += f.c * kv[1]; facPower += (f.p || 0) * kv[1]; }
    });
    var dockTons = 0, dockCost = 0;
    Object.entries(eDocking).forEach(function (kv) {
      var dk = DOCKING[kv[0]];
      dockTons += dk.t * kv[1]; dockCost += dk.c * kv[1];
    });

    var totalPowerNeeded = cmdPower + sen.p + wpnPower + scrPower + facPower;
    var ePPRating = d.auto.engineering ? Math.max(10, Math.ceil(totalPowerNeeded * (a.ppM || 1.2))) : d.ppRating;
    pp = PP[ePPType];
    var ppTons = Math.ceil(ePPRating * pp.tpp);
    var ppCost = ppTons * pp.cpp;
    var fuelTons = Math.ceil(ePPRating * pp.fpm * eFuelMonths);
    var powerSurplus = ePPRating - totalPowerNeeded;

    var crewCmd = Math.max(2, Math.ceil(h / 5000));
    var crewEng = Math.max(1, Math.ceil(ppTons / 1000)) + Math.max(0, Math.ceil(fuelTons / 5000));
    var crewGun = Object.values(eTurrets).reduce(function (s, n) { return s + n; }, 0) + Object.values(eBays).reduce(function (s, n) { return s + n * 4; }, 0);
    var crewDock = Object.entries(eDocking).reduce(function (s, kv) {
      return s + ((kv[0] === "small_craft" || kv[0] === "hangar_s") ? kv[1] : kv[1] * 2);
    }, 0);
    var crewMaint = Math.max(1, Math.ceil(h / 1000));
    var crewMed = (eFac.medical || 0) * 2;
    var crewSec = Math.max(1, Math.ceil(h / 2000));
    var crewFac = Object.entries(eFac).reduce(function (s, kv) {
      var k = kv[0], n = kv[1];
      if (["shipyard_s", "shipyard_m", "shipyard_l"].indexOf(k) >= 0) return s + n * 10;
      if (k === "manufacturing" || k === "ore_process") return s + n * 5;
      if (k === "lab") return s + n * 3;
      if (k === "fuel_refinery" || k === "training") return s + n * 2;
      if (k === "commercial") return s + n * 3;
      return s;
    }, 0);
    var crewBase = crewCmd + crewEng + crewGun + crewDock + crewMaint + crewMed + crewSec + crewFac;
    var crewAdmin = Math.max(1, Math.ceil(crewBase / 20));
    var totalCrew = crewBase + crewAdmin;

    var eAccom = d.auto.quarters ? autoAccom(totalCrew, d.officerRatio) : Object.assign({}, d.accom);
    var accTons = 0, accCost = 0, berths = 0;
    Object.entries(eAccom).forEach(function (kv) {
      var ac = ACCOM[kv[0]];
      if (ac) { accTons += ac.t * kv[1]; accCost += ac.c * kv[1]; berths += (ac.occ || 0) * kv[1]; }
    });
    var swCost = 0;
    eSW.forEach(function (k) { swCost += (SOFTWARE[k] && SOFTWARE[k].c) ? SOFTWARE[k].c : 0; });

    var usedTons = armorTons + cmdTons + sen.t + ppTons + fuelTons + wpnTons + scrTons + dockTons + accTons + facTons;
    var cargoTons = Math.max(0, h - usedTons);
    var totalCost = hullCost + armorCost + cmdCost + compCost + sen.c + ppCost + wpnCost + scrCost + dockCost + accCost + facCost + swCost;

    var classCtx = { fac: eFac, dock: eDocking, turrets: eTurrets, bays: eBays, screens: eScreens, sensors: eSensors, cmdType: eCmdType, computer: eComputer, hp: hardpoints, wpnHP: wpnHP, sw: eSW };
    var classifications = {};
    Object.entries(CLASSES).forEach(function (kv) {
      var k = kv[0], cl = kv[1];
      var results = cl.reqs.map(function (r) { return { label: r[0], met: r[1](classCtx) }; });
      var metCount = results.filter(function (r) { return r.met; }).length;
      classifications[k] = { name: cl.name, icon: cl.icon, reqs: cl.reqs, results: results, earned: metCount === cl.reqs.length, metCount: metCount, total: cl.reqs.length };
    });
    var earnedClasses = Object.entries(classifications).filter(function (kv) { return kv[1].earned; }).map(function (kv) { return kv[0]; });

    return {
      hullCost: hullCost, structHP: structHP, hardpoints: hardpoints,
      armorTons: armorTons, armorCost: armorCost, effectiveArmor: effectiveArmor,
      cmdTons: cmdTons, cmdCost: cmdCost, cmdPower: cmdPower, compCost: compCost,
      senTons: sen.t, senCost: sen.c, senPower: sen.p,
      ppTons: ppTons, ppCost: ppCost, fuelTons: fuelTons,
      wpnTons: wpnTons, wpnCost: wpnCost, wpnPower: wpnPower, wpnHP: wpnHP,
      scrTons: scrTons, scrCost: scrCost, scrPower: scrPower,
      dockTons: dockTons, dockCost: dockCost,
      accTons: accTons, accCost: accCost, berths: berths,
      facTons: facTons, facCost: facCost, facPower: facPower, swCost: swCost,
      totalPowerNeeded: totalPowerNeeded, powerSurplus: powerSurplus,
      usedTons: usedTons, cargoTons: cargoTons, totalCost: totalCost,
      crew: { cmd: crewCmd, eng: crewEng, gun: crewGun, dock: crewDock, maint: crewMaint, med: crewMed, sec: crewSec, fac: crewFac, admin: crewAdmin, total: totalCrew },
      eFac: eFac, eDocking: eDocking, eCmdType: eCmdType, eComputer: eComputer, eSensors: eSensors, eSW: eSW,
      eArmorType: eArmorType, eArmorPts: eArmorPts, eTurrets: eTurrets, eBays: eBays, eScreens: eScreens,
      ePPType: ePPType, ePPRating: ePPRating, eFuelMonths: eFuelMonths, eAccom: eAccom,
      classifications: classifications, earnedClasses: earnedClasses,
    };
  }, [d]);

  var pctUsed = Math.min(100, (calc.usedTons / d.hullTons) * 100);
  var overT = calc.usedTons > d.hullTons;
  var overP = calc.powerSurplus < 0;

  function ClassPanel(props) {
    var detailed = props.detailed;
    var earned = calc.earnedClasses;
    return (
      <div className="bg-gray-800 rounded-lg p-3 mb-4 border border-gray-700">
        <div className="text-xs text-gray-400 mb-2 font-bold">CLASSIFICATIONS EARNED</div>
        {earned.length === 0 ? <div className="text-xs text-gray-500 italic">No classifications met — add facilities &amp; docking</div> : null}
        <div className="flex flex-wrap gap-1 mb-2">
          {earned.map(function (k) {
            var cl = calc.classifications[k];
            return <span key={k} className="px-2 py-0.5 bg-emerald-800 text-emerald-200 rounded text-xs font-medium">{cl.icon} {cl.name}</span>;
          })}
        </div>
        {detailed ? (
          <div className="mt-3 space-y-2">
            {Object.entries(calc.classifications).map(function (kv) {
              var k = kv[0], cl = kv[1];
              return (
                <div key={k} className="border border-gray-700 rounded">
                  <button
                    type="button"
                    onClick={function () { setExpandedClass(expandedClass === k ? null : k); }}
                    className="w-full flex items-center justify-between px-2 py-1.5 text-left hover:bg-gray-700 rounded"
                  >
                    <span className="text-xs font-medium text-gray-200">{cl.icon} {cl.name}</span>
                    <span className={"text-xs font-bold " + (cl.earned ? "text-emerald-400" : "text-gray-500")}>
                      {cl.earned ? "✓ EARNED" : cl.metCount + "/" + cl.total}
                    </span>
                  </button>
                  {expandedClass === k ? (
                    <div className="px-2 pb-2 space-y-0.5">
                      {cl.results.map(function (r, i) {
                        return (
                          <div key={i} className="flex items-center gap-2 text-xs">
                            <span className={r.met ? "text-emerald-400" : "text-red-400"}>{r.met ? "✓" : "✗"}</span>
                            <span className={r.met ? "text-gray-300" : "text-gray-500"}>{r.label}</span>
                          </div>
                        );
                      })}
                    </div>
                  ) : null}
                </div>
              );
            })}
          </div>
        ) : null}
      </div>
    );
  }

  function genExport() {
    var ln = function (s) { return s + "\n"; };
    var hr = "─".repeat(50) + "\n";
    var eq = "═".repeat(50) + "\n";
    var itm = function (data, counts, label) {
      var lines = Object.entries(counts).filter(function (kv) { return kv[1] > 0; }).map(function (kv) { return "  " + data[kv[0]].n + ": " + kv[1]; });
      return lines.length ? ln(label) + lines.join("\n") + "\n" : "";
    };
    var o = eq + ln("STATION: " + d.name) + eq;
    o += ln("Design Template: " + TYPES[d.type].n);
    if (calc.earnedClasses.length > 0) o += ln("Classifications: " + calc.earnedClasses.map(function (k) { return calc.classifications[k].name; }).join(", "));
    o += ln("Configuration: " + CONFIGS[d.config].n);
    o += ln("Hull: " + fmtT(d.hullTons) + " | Structure HP: " + calc.structHP + " | Hardpoints: " + calc.hardpoints);
    o += ln("Armor: " + ARMOR[calc.eArmorType].n + ", " + calc.effectiveArmor + " effective points");
    o += hr + ln("ENGINEERING") + ln("  Power Plant: " + PP[calc.ePPType].n + ", Rating " + calc.ePPRating + " (" + fmtT(calc.ppTons) + ")");
    o += ln("  Fuel: " + fmtT(calc.fuelTons) + " (" + calc.eFuelMonths + " months)");
    o += ln("  Power: " + calc.totalPowerNeeded + "/" + calc.ePPRating + " PP (" + (calc.powerSurplus >= 0 ? "+" : "") + calc.powerSurplus + ")");
    o += hr + ln("COMMAND & CONTROL") + ln("  Command: " + calc.eCmdType + " (" + fmtT(calc.cmdTons) + ")");
    o += ln("  Computer: " + (COMPUTERS[calc.eComputer] ? COMPUTERS[calc.eComputer].n : ""));
    o += ln("  Sensors: " + SENSORS[calc.eSensors].n);
    o += ln("  Software: " + calc.eSW.map(function (k) { return SOFTWARE[k] ? SOFTWARE[k].n : k; }).join(", "));
    o += hr + itm(TURRETS, calc.eTurrets, "TURRETS & BARBETTES") + itm(BAYS, calc.eBays, "BAY WEAPONS") + itm(SCREENS_D, calc.eScreens, "SCREENS");
    o += hr + itm(DOCKING, calc.eDocking, "DOCKING FACILITIES");
    o += hr + itm(ACCOM, calc.eAccom, "ACCOMMODATIONS") + ln("  Berths: " + calc.berths);
    o += hr + itm(FACILITIES, calc.eFac, "FACILITIES");
    o += hr + ln("TONNAGE BREAKDOWN");
    [["Armor", calc.armorTons], ["Command", calc.cmdTons], ["Sensors", calc.senTons], ["Power Plant", calc.ppTons], ["Fuel", calc.fuelTons], ["Weapons", calc.wpnTons], ["Screens", calc.scrTons], ["Docking", calc.dockTons], ["Quarters", calc.accTons], ["Facilities", calc.facTons]].forEach(function (row) {
      if (row[1] > 0) o += "  " + row[0] + ": " + fmtT(row[1]) + "\n";
    });
    o += ln("  Cargo: " + fmtT(calc.cargoTons)) + ln("  USED: " + fmtT(calc.usedTons) + " / " + fmtT(d.hullTons));
    o += hr + ln("CREW: " + calc.crew.total);
    o += "  Cmd:" + calc.crew.cmd + " Eng:" + calc.crew.eng + " Gun:" + calc.crew.gun + " Dock:" + calc.crew.dock + " Mnt:" + calc.crew.maint + " Med:" + calc.crew.med + " Sec:" + calc.crew.sec + " Fac:" + calc.crew.fac + " Adm:" + calc.crew.admin + "\n";
    o += hr + ln("TOTAL COST: " + fmt(calc.totalCost)) + eq;
    return o;
  }
  function copyExport() {
    try { navigator.clipboard.writeText(genExport()); } catch (e) {}
    if (exportRef.current) exportRef.current.select();
  }

  var TABS = ["Hull", "Power", "Command", "Defenses", "Docking", "Quarters", "Facilities", "Summary"];
  var autoKeys = ["", "engineering", "command", "defenses", "docking", "quarters", "facilities"];

  function renderHull() {
    return (
      <div>
        <h3 className="text-lg font-bold text-cyan-400 mb-3">Station Type &amp; Hull</h3>
        <Sel label="Design Template" value={d.type} onChange={function (v) { applyType(v); }} options={Object.entries(TYPES).map(function (kv) { return [kv[0], kv[1].n]; })} desc={TYPES[d.type].d} />
        <div className="mb-3">
          <label className="block text-xs text-gray-400 mb-1">Station Name</label>
          <input type="text" value={d.name} onChange={function (e) { upd("name", e.target.value); }} className="w-full bg-gray-800 border border-gray-600 text-gray-100 rounded px-2 py-1.5 text-sm focus:border-cyan-500 focus:outline-none" />
        </div>
        <Num label="Hull Displacement" value={d.hullTons} unit="dtons" onChange={function (v) { upd("hullTons", clamp(v, TYPES[d.type].min, TYPES[d.type].max)); }} min={TYPES[d.type].min} max={TYPES[d.type].max} step={100} desc={"Range: " + TYPES[d.type].min.toLocaleString() + "–" + TYPES[d.type].max.toLocaleString()} />
        <Sel label="Configuration" value={d.config} onChange={function (v) { upd("config", v); }} options={Object.entries(CONFIGS).map(function (kv) { return [kv[0], kv[1].n + " (×" + kv[1].cost + " cost)"]; })} desc={CONFIGS[d.config].d} />
        <div className="bg-gray-800 rounded p-3 mt-3 grid grid-cols-2 gap-2 text-sm">
          <div>Hull Cost: <span className="text-cyan-400">{fmt(calc.hullCost)}</span></div>
          <div>Structure HP: <span className="text-cyan-400">{calc.structHP}</span></div>
          <div>Hardpoints: <span className="text-cyan-400">{calc.hardpoints}</span></div>
          <div>Armor Mod: <span className="text-cyan-400">×{CONFIGS[d.config].arm}</span></div>
        </div>
        <div className="mt-4"><ClassPanel detailed={false} /></div>
        <div className="bg-gray-800 rounded p-3">
          <div className="text-xs text-gray-400 mb-2">Auto-Populate Toggles</div>
          <div className="space-y-1">
            {["engineering", "command", "defenses", "docking", "quarters", "facilities"].map(function (s) {
              return (
                <div key={s} className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-300 capitalize">{s}</span>
                  <button type="button" onClick={function () { toggleAuto(s, calc); }} className={"px-2 py-0.5 rounded text-xs " + (d.auto[s] ? "bg-cyan-700 text-cyan-100" : "bg-gray-700 text-gray-400")}>{d.auto[s] ? "AUTO" : "MANUAL"}</button>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    );
  }

  function renderEng() {
    var isA = d.auto.engineering;
    return (
      <div>
        <h3 className="text-lg font-bold text-cyan-400 mb-3">Power &amp; Fuel</h3>
        <AutoBanner auto={isA} onToggle={function () { toggleAuto("engineering", calc); }} />
        <Sel label="Power Plant Type" value={calc.ePPType} disabled={isA} onChange={function (v) { upd("ppType", v); }} options={Object.entries(PP).map(function (kv) { return [kv[0], kv[1].n + " (" + kv[1].tpp + "t/PP)"]; })} desc={PP[calc.ePPType].fpm > 0 ? "Fuel: " + PP[calc.ePPType].fpm + "t/PP/month" : "No fuel required"} />
        <Num label="Power Plant Rating" value={calc.ePPRating} disabled={isA} onChange={function (v) { upd("ppRating", Math.max(1, v)); }} min={1} max={9999} unit="PP" desc={isA ? "Auto: " + calc.totalPowerNeeded + "PP × " + Math.round((AP[d.type] && AP[d.type].ppM ? AP[d.type].ppM : 1.2) * 100) + "% margin" : ""} />
        <Num label="Fuel Reserve" value={calc.eFuelMonths} disabled={isA} onChange={function (v) { upd("fuelMonths", Math.max(0, v)); }} min={0} max={120} unit="months" desc={fmtT(calc.fuelTons) + " fuel storage"} />
        <div className="bg-gray-800 rounded p-3 mt-3 grid grid-cols-2 gap-2 text-sm">
          <div>Plant: <span className="text-cyan-400">{fmtT(calc.ppTons)}</span></div>
          <div>Cost: <span className="text-cyan-400">{fmt(calc.ppCost)}</span></div>
          <div>Fuel: <span className="text-cyan-400">{fmtT(calc.fuelTons)}</span></div>
          <div>Output: <span className="text-cyan-400">{calc.ePPRating} PP</span></div>
          <div>Demand: <span className={overP ? "text-red-400" : "text-cyan-400"}>{calc.totalPowerNeeded} PP</span></div>
          <div>Surplus: <span className={overP ? "text-red-400" : "text-emerald-400"}>{(calc.powerSurplus >= 0 ? "+" : "") + calc.powerSurplus} PP</span></div>
        </div>
      </div>
    );
  }

  function renderCmd() {
    var isA = d.auto.command;
    return (
      <div>
        <h3 className="text-lg font-bold text-cyan-400 mb-3">Command, Sensors &amp; Software</h3>
        <AutoBanner auto={isA} onToggle={function () { toggleAuto("command", calc); }} />
        <Sel label="Command Center" value={calc.eCmdType} disabled={isA} onChange={function (v) { upd("cmdType", v); }} options={[["standard", "Standard (2% hull)"], ["military", "Military (2.5% hull)"]]} desc={fmtT(calc.cmdTons) + " — " + fmt(calc.cmdCost) + " — " + calc.cmdPower + "PP"} />
        <Sel label="Computer" value={calc.eComputer} disabled={isA} onChange={function (v) { upd("computer", Number(v)); }} options={Object.entries(COMPUTERS).map(function (kv) { return [kv[0], kv[1].n + " — " + fmt(kv[1].c)]; })} />
        <Sel label="Sensors" value={calc.eSensors} disabled={isA} onChange={function (v) { upd("sensors", v); }} options={Object.entries(SENSORS).map(function (kv) { return [kv[0], kv[1].n + " — " + kv[1].t + "t, " + kv[1].p + "PP"]; })} />
        <div className="mt-4 mb-2 text-xs text-gray-400">Software</div>
        <div className="space-y-1">
          {Object.entries(SOFTWARE).map(function (kv) {
            return (
              <label key={kv[0]} className={"flex items-center gap-2 py-1 rounded px-1 " + (isA ? "opacity-60" : "cursor-pointer hover:bg-gray-800")}>
                <input type="checkbox" checked={calc.eSW.indexOf(kv[0]) >= 0} disabled={isA} onChange={function () { toggleSW(kv[0]); }} className="accent-cyan-500" />
                <span className="text-sm text-gray-200">{kv[1].n}</span>
                {kv[1].c > 0 ? <span className="text-xs text-gray-500 ml-auto">{fmt(kv[1].c)}</span> : null}
              </label>
            );
          })}
        </div>
      </div>
    );
  }

  function renderDef() {
    var isA = d.auto.defenses;
    return (
      <div>
        <h3 className="text-lg font-bold text-cyan-400 mb-3">Armor &amp; Weapons</h3>
        <AutoBanner auto={isA} onToggle={function () { toggleAuto("defenses", calc); }} />
        <Sel label="Armor Type" value={calc.eArmorType} disabled={isA} onChange={function (v) { upd("armorType", v); if (v === "none") upd("armorPts", 0); }} options={Object.entries(ARMOR).map(function (kv) { return [kv[0], kv[1].n]; })} />
        {calc.eArmorType !== "none" ? <Num label="Armor Points" value={calc.eArmorPts} disabled={isA} onChange={function (v) { upd("armorPts", clamp(v, 0, 20)); }} min={0} max={20} desc={fmtT(calc.armorTons) + " — Effective: " + calc.effectiveArmor + " pts"} /> : null}
        <div className="mt-4 mb-2 text-xs text-gray-400 flex justify-between">
          <span>Turrets &amp; Barbettes</span>
          <span className={calc.wpnHP > calc.hardpoints ? "text-red-400" : "text-gray-500"}>HP: {calc.wpnHP}/{calc.hardpoints}</span>
        </div>
        {Object.entries(TURRETS).map(function (kv) { return <Counter key={kv[0]} label={kv[1].n} count={calc.eTurrets[kv[0]] || 0} disabled={isA} onChange={function (n) { updN("turrets", kv[0], n); }} extra={kv[1].t + "t, " + fmt(kv[1].c) + ", " + kv[1].p + "PP"} />; })}
        <div className="mt-4 mb-2 text-xs text-gray-400">Bay Weapons</div>
        {Object.entries(BAYS).map(function (kv) { return <Counter key={kv[0]} label={kv[1].n} count={calc.eBays[kv[0]] || 0} disabled={isA} onChange={function (n) { updN("bays", kv[0], n); }} extra={kv[1].t + "t, " + fmt(kv[1].c) + ", " + kv[1].p + "PP"} />; })}
        <div className="mt-4 mb-2 text-xs text-gray-400">Screens</div>
        {Object.entries(SCREENS_D).map(function (kv) { return <Counter key={kv[0]} label={kv[1].n} count={calc.eScreens[kv[0]] || 0} disabled={isA} onChange={function (n) { updN("screens", kv[0], n); }} extra={kv[1].t + "t, " + fmt(kv[1].c) + ", " + kv[1].p + "PP"} />; })}
      </div>
    );
  }

  function renderDock() {
    var isA = d.auto.docking;
    return (
      <div>
        <h3 className="text-lg font-bold text-cyan-400 mb-3">Docking Facilities</h3>
        <AutoBanner auto={isA} onToggle={function () { toggleAuto("docking", calc); }} />
        {Object.entries(DOCKING).map(function (kv) { return <Counter key={kv[0]} label={kv[1].n} count={calc.eDocking[kv[0]] || 0} disabled={isA} onChange={function (n) { updN("docking", kv[0], n); }} desc={kv[1].d} extra={fmtT(kv[1].t) + ", " + fmt(kv[1].c)} />; })}
        <div className="bg-gray-800 rounded p-3 mt-4 text-sm">Tonnage: <span className="text-cyan-400">{fmtT(calc.dockTons)}</span> — Cost: <span className="text-cyan-400">{fmt(calc.dockCost)}</span></div>
      </div>
    );
  }

  function renderAccom() {
    var isA = d.auto.quarters;
    return (
      <div>
        <h3 className="text-lg font-bold text-cyan-400 mb-3">Accommodations</h3>
        <AutoBanner auto={isA} onToggle={function () { toggleAuto("quarters", calc); }} label="Auto-allocate by officer/enlisted ratio" />
        {isA ? <Num label="Officer Ratio" value={d.officerRatio} onChange={function (v) { upd("officerRatio", clamp(v, 0.05, 0.5)); }} min={0.05} max={0.5} step={0.05} desc={Math.ceil(calc.crew.total * d.officerRatio) + " officers (staterooms), " + (calc.crew.total - Math.ceil(calc.crew.total * d.officerRatio)) + " enlisted (barracks)"} /> : null}
        <p className="text-xs text-gray-500 mb-3">Crew: {calc.crew.total} | Berths: {calc.berths}</p>
        {Object.entries(ACCOM).map(function (kv) { return <Counter key={kv[0]} label={kv[1].n} count={calc.eAccom[kv[0]] || 0} disabled={isA} onChange={function (n) { updN("accom", kv[0], n); }} desc={kv[1].d} extra={kv[1].t + "t, " + fmt(kv[1].c) + (kv[1].occ ? ", ×" + kv[1].occ : "")} />; })}
        <div className="bg-gray-800 rounded p-3 mt-4 text-sm">Tonnage: <span className="text-cyan-400">{fmtT(calc.accTons)}</span> — Berths: <span className={calc.berths < calc.crew.total ? "text-amber-400" : "text-emerald-400"}>{calc.berths}/{calc.crew.total}</span></div>
      </div>
    );
  }

  function renderFac() {
    var isA = d.auto.facilities;
    return (
      <div>
        <h3 className="text-lg font-bold text-cyan-400 mb-3">Station Facilities</h3>
        <AutoBanner auto={isA} onToggle={function () { toggleAuto("facilities", calc); }} />
        {Object.entries(FACILITIES).map(function (kv) { return <Counter key={kv[0]} label={kv[1].n} count={calc.eFac[kv[0]] || 0} disabled={isA} onChange={function (n) { updN("fac", kv[0], n); }} desc={kv[1].d} extra={fmtT(kv[1].t) + ", " + fmt(kv[1].c) + ", " + kv[1].p + "PP"} />; })}
      </div>
    );
  }

  function renderSummary() {
    var rows = [
      ["Hull", fmtT(d.hullTons), "—", fmt(calc.hullCost)],
      ["Armor", fmtT(calc.armorTons), calc.effectiveArmor + "pts", fmt(calc.armorCost)],
      ["Command", fmtT(calc.cmdTons), calc.cmdPower + "PP", fmt(calc.cmdCost)],
      ["Computer", "—", (COMPUTERS[calc.eComputer] && COMPUTERS[calc.eComputer].n) || "", fmt(calc.compCost)],
      ["Sensors", fmtT(calc.senTons), calc.senPower + "PP", fmt(calc.senCost)],
      ["Power Plant", fmtT(calc.ppTons), calc.ePPRating + "PP", fmt(calc.ppCost)],
      ["Fuel", fmtT(calc.fuelTons), calc.eFuelMonths + "mo", "—"],
      ["Weapons", fmtT(calc.wpnTons), calc.wpnPower + "PP", fmt(calc.wpnCost)],
      ["Screens", fmtT(calc.scrTons), calc.scrPower + "PP", fmt(calc.scrCost)],
      ["Docking", fmtT(calc.dockTons), "—", fmt(calc.dockCost)],
      ["Quarters", fmtT(calc.accTons), calc.berths + "berths", fmt(calc.accCost)],
      ["Facilities", fmtT(calc.facTons), calc.facPower + "PP", fmt(calc.facCost)],
      ["Software", "—", "—", fmt(calc.swCost)],
    ];
    return (
      <div>
        <h3 className="text-lg font-bold text-cyan-400 mb-1">Station Summary</h3>
        <div className="text-xl font-bold text-gray-100">{d.name}</div>
        <div className="text-sm text-gray-400 mb-3">{TYPES[d.type].n} • {CONFIGS[d.config].n} • {fmtT(d.hullTons)}</div>
        <ClassPanel detailed={true} />
        {(overT || overP) ? (
          <div className="bg-red-900 bg-opacity-40 border border-red-700 rounded p-2 mb-3 text-sm text-red-300">
            {overT ? <div>⚠ Over tonnage by {fmtT(calc.usedTons - d.hullTons)}!</div> : null}
            {overP ? <div>⚠ Power deficit: {Math.abs(calc.powerSurplus)} PP short!</div> : null}
          </div>
        ) : null}
        {calc.berths < calc.crew.total ? <div className="bg-amber-900 bg-opacity-40 border border-amber-700 rounded p-2 mb-3 text-sm text-amber-300">⚠ Need {calc.crew.total - calc.berths} more berths</div> : null}
        <div className="overflow-x-auto mb-4">
          <table className="w-full text-sm">
            <thead><tr className="text-xs text-gray-400 border-b border-gray-700"><th className="text-left py-1">System</th><th className="text-right px-2">Tons</th><th className="text-right px-2">Detail</th><th className="text-right">Cost</th></tr></thead>
            <tbody>
              {rows.map(function (row, i) {
                return <tr key={i} className="border-b border-gray-800 text-gray-200"><td className="py-1">{row[0]}</td><td className="text-right px-2 text-cyan-400">{row[1]}</td><td className="text-right px-2 text-gray-400">{row[2]}</td><td className="text-right text-emerald-400">{row[3]}</td></tr>;
              })}
              <tr className="border-t-2 border-gray-600 font-bold text-gray-100"><td className="py-2">TOTAL</td><td className="text-right px-2 text-cyan-300">{fmtT(calc.usedTons)}</td><td></td><td className="text-right text-emerald-300">{fmt(calc.totalCost)}</td></tr>
              <tr><td className="py-1 text-gray-300">Cargo</td><td className={"text-right px-2 " + (overT ? "text-red-400" : "text-amber-400")}>{fmtT(calc.cargoTons)}</td><td></td><td></td></tr>
            </tbody>
          </table>
        </div>
        <h4 className="text-sm font-bold text-cyan-400 mb-2">Crew: {calc.crew.total}</h4>
        <div className="grid grid-cols-3 gap-x-3 gap-y-1 text-xs text-gray-300 mb-4">
          <div>Cmd: {calc.crew.cmd}</div><div>Eng: {calc.crew.eng}</div><div>Gun: {calc.crew.gun}</div>
          <div>Dock: {calc.crew.dock}</div><div>Maint: {calc.crew.maint}</div><div>Med: {calc.crew.med}</div>
          <div>Sec: {calc.crew.sec}</div><div>Fac: {calc.crew.fac}</div><div>Admin: {calc.crew.admin}</div>
        </div>
        <h4 className="text-sm font-bold text-cyan-400 mb-2">Combat</h4>
        <div className="grid grid-cols-2 gap-x-4 text-xs text-gray-300 mb-4">
          <div>HP: {calc.structHP}</div><div>Armor: {calc.effectiveArmor}</div>
          <div>Hardpoints: {calc.wpnHP}/{calc.hardpoints}</div>
          <div>Sensors: {(SENSORS[calc.eSensors].dm >= 0 ? "+" : "") + SENSORS[calc.eSensors].dm} DM</div>
        </div>
        <button type="button" onClick={function () { setShowExport(!showExport); }} className="w-full py-2 bg-cyan-700 hover:bg-cyan-600 text-white rounded font-medium text-sm mb-3">{showExport ? "Hide" : "📋 Export Stat Block"}</button>
        {showExport ? (
          <div className="mb-4">
            <textarea ref={exportRef} readOnly value={genExport()} rows={28} className="w-full bg-gray-900 border border-gray-600 text-green-400 rounded p-3 text-xs font-mono focus:outline-none" style={{ resize: "vertical" }} />
            <button type="button" onClick={copyExport} className="mt-2 px-4 py-1.5 bg-emerald-700 hover:bg-emerald-600 text-white rounded text-sm">Copy to Clipboard</button>
          </div>
        ) : null}
      </div>
    );
  }

  var tabFns = [renderHull, renderEng, renderCmd, renderDef, renderDock, renderAccom, renderFac, renderSummary];

  return (
    <div className="min-h-screen bg-gray-950 text-gray-100" style={{ fontFamily: "'Segoe UI', system-ui, sans-serif" }}>
      <div className="bg-gray-900 border-b border-gray-700 px-4 py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3"><div className="text-2xl">🛸</div><div><h1 className="text-lg font-bold text-cyan-400">Traveller Station Builder</h1><p className="text-xs text-gray-500">Extended Ship Rules — Classification System</p></div></div>
          {calc.earnedClasses.length > 0 ? (
            <div className="flex flex-wrap gap-1 justify-end">
              {calc.earnedClasses.slice(0, 3).map(function (k) { return <span key={k} className="px-1.5 py-0.5 bg-emerald-900 text-emerald-300 rounded text-xs">{calc.classifications[k].icon}</span>; })}
              {calc.earnedClasses.length > 3 ? <span className="text-xs text-emerald-400">+{calc.earnedClasses.length - 3}</span> : null}
            </div>
          ) : null}
        </div>
      </div>
      <div className="bg-gray-900 border-b border-gray-800 px-3 py-2">
        <div className="flex flex-wrap justify-between items-center gap-1">
          <div className="flex gap-3"><Badge label="Hull" value={fmtT(d.hullTons)} /><Badge label="Used" value={fmtT(calc.usedTons)} color={overT ? "red" : "cyan"} /><Badge label="Free" value={fmtT(calc.cargoTons)} color={overT ? "red" : "amber"} /></div>
          <div className="flex gap-3"><Badge label="Power" value={(calc.powerSurplus >= 0 ? "+" : "") + calc.powerSurplus} color={overP ? "red" : "emerald"} /><Badge label="Cost" value={fmt(calc.totalCost)} color="emerald" /><Badge label="Crew" value={calc.crew.total} color={calc.berths < calc.crew.total ? "amber" : "cyan"} /></div>
        </div>
        <div className="mt-1.5 h-1.5 bg-gray-800 rounded-full overflow-hidden">
          <div className={"h-full rounded-full transition-all " + (overT ? "bg-red-500" : "bg-cyan-500")} style={{ width: Math.min(100, pctUsed) + "%" }} />
        </div>
        <div className="text-xs text-gray-600 text-right">{pctUsed.toFixed(1)}%</div>
      </div>
      <div className="bg-gray-900 border-b border-gray-800 px-1 flex overflow-x-auto">
        {TABS.map(function (t, i) {
          return (
            <button key={i} type="button" onClick={function () { setTab(i); }} className={"px-3 py-2 text-xs font-medium whitespace-nowrap border-b-2 " + (tab === i ? "border-cyan-400 text-cyan-400" : "border-transparent text-gray-500 hover:text-gray-300")}>
              {t}
              {i > 0 && i < 7 && d.auto[autoKeys[i]] ? <span className="ml-1 text-cyan-600">●</span> : null}
            </button>
          );
        })}
      </div>
      <div className="p-4 max-w-2xl mx-auto pb-20">{tabFns[tab]()}</div>
    </div>
  );
}

if (typeof document !== "undefined" && document.getElementById("root")) {
  var root = ReactDOM.createRoot(document.getElementById("root"));
  root.render(React.createElement(StationBuilder));
}
