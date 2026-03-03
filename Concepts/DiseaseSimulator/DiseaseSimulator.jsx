/**
 * Pathogen Evolution Simulator — Concept 9 prototype.
 * Deterministic SEIRDV + symptoms + comorbidities. Single-file React (Babel), injected RNG.
 */
var useState = React.useState;
var useCallback = React.useCallback;

var RechartsLib = (typeof window !== "undefined" && window.Recharts) ? (window.Recharts.default || window.Recharts) : {};
var ResponsiveContainer = RechartsLib.ResponsiveContainer;
var AreaChart = RechartsLib.AreaChart;
var Area = RechartsLib.Area;
var LineChart = RechartsLib.LineChart;
var Line = RechartsLib.Line;
var XAxis = RechartsLib.XAxis;
var YAxis = RechartsLib.YAxis;
var Tooltip = RechartsLib.Tooltip;
var Legend = RechartsLib.Legend;
var CartesianGrid = RechartsLib.CartesianGrid;
var RadarChart = RechartsLib.RadarChart;
var PolarGrid = RechartsLib.PolarGrid;
var PolarAngleAxis = RechartsLib.PolarAngleAxis;
var PolarRadiusAxis = RechartsLib.PolarRadiusAxis;
var Radar = RechartsLib.Radar;

function createMulberry32(seed) {
  var a = seed >>> 0;
  return function next() {
    a |= 0;
    a = (a + 0x6D2B79F5) | 0;
    var t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

function sampleRange(rng, min, max) {
  return rng() * (max - min) + min;
}

function clamp(value, low, high) {
  if (low === undefined) low = 0;
  if (high === undefined) high = 1;
  return Math.min(high, Math.max(low, value));
}

function lerp(a, b, t) {
  return a + (b - a) * clamp(t, 0, 1);
}

function formatPopulation(n) {
  if (n >= 1e9) return (n / 1e9).toFixed(2) + "B";
  if (n >= 1e6) return (n / 1e6).toFixed(2) + "M";
  if (n >= 1e3) return (n / 1e3).toFixed(1) + "K";
  return Math.round(n).toString();
}

function formatPercent(n) {
  return (n * 100).toFixed(1) + "%";
}

var PLANETS = [
  { id: "terran", name: "Terran", icon: "🌍", tempMod: 0, humMod: 0, radMod: 0 },
  { id: "arid", name: "Arid", icon: "🏜️", tempMod: 0.3, humMod: -0.4, radMod: 0.1 },
  { id: "oceanic", name: "Oceanic", icon: "🌊", tempMod: -0.1, humMod: 0.5, radMod: 0 },
  { id: "frozen", name: "Frozen", icon: "❄️", tempMod: -0.5, humMod: -0.2, radMod: 0 },
  { id: "toxic", name: "Toxic", icon: "☣️", tempMod: 0.2, humMod: 0.1, radMod: 0.4 },
  { id: "jungle", name: "Jungle", icon: "🌿", tempMod: 0.1, humMod: 0.4, radMod: -0.1 }
];

var PATHOGEN_TYPES = [
  { id: "virus", name: "Virus", icon: "🦠" },
  { id: "bacteria", name: "Bacteria", icon: "🔬" },
  { id: "fungal", name: "Fungal Spore", icon: "🍄" },
  { id: "prion", name: "Prion", icon: "🧬" },
  { id: "nanobot", name: "Nanobot Swarm", icon: "⚙️" },
  { id: "parasite", name: "Xenoparasite", icon: "🐛" }
];

var SYMPTOM_POOLS = {
  virus: {
    early: [
      { name: "Fever", icon: "🌡️", sev: 1, traitKey: "severity", threshold: 0.1 },
      { name: "Cough", icon: "😷", sev: 1, traitKey: "airborne", threshold: 0.2 },
      { name: "Headache", icon: "🤕", sev: 1, traitKey: "severity", threshold: 0.15 }
    ],
    mid: [
      { name: "Pneumonia", icon: "🫁", sev: 2, traitKey: "airborne", threshold: 0.4 },
      { name: "High Fever", icon: "🔥", sev: 2, traitKey: "severity", threshold: 0.35 }
    ],
    late: [
      { name: "Organ Failure", icon: "💔", sev: 3, traitKey: "lethality", threshold: 0.3 },
      { name: "Cytokine Storm", icon: "⚡", sev: 3, traitKey: "immuneEvasion", threshold: 0.5 }
    ]
  },
  bacteria: {
    early: [
      { name: "Inflammation", icon: "🔴", sev: 1, traitKey: "severity", threshold: 0.1 },
      { name: "Fever", icon: "🌡️", sev: 1, traitKey: "severity", threshold: 0.15 }
    ],
    mid: [
      { name: "Septic Shock", icon: "⚡", sev: 2, traitKey: "severity", threshold: 0.45 },
      { name: "Meningitis", icon: "🧠", sev: 2, traitKey: "severity", threshold: 0.5 }
    ],
    late: [
      { name: "Gangrene", icon: "🦴", sev: 3, traitKey: "lethality", threshold: 0.35 },
      { name: "Multi-organ Sepsis", icon: "💔", sev: 3, traitKey: "lethality", threshold: 0.5 }
    ]
  },
  fungal: {
    early: [
      { name: "Skin Lesions", icon: "🔴", sev: 1, traitKey: "infectivity", threshold: 0.1 },
      { name: "Cough", icon: "😷", sev: 1, traitKey: "airborne", threshold: 0.15 }
    ],
    mid: [
      { name: "Lung Colonization", icon: "🫁", sev: 2, traitKey: "airborne", threshold: 0.35 },
      { name: "Vision Loss", icon: "👁️", sev: 2, traitKey: "severity", threshold: 0.4 }
    ],
    late: [
      { name: "Brain Fungemia", icon: "🧠", sev: 3, traitKey: "immuneEvasion", threshold: 0.4 },
      { name: "Systemic Failure", icon: "💔", sev: 3, traitKey: "lethality", threshold: 0.4 }
    ]
  },
  prion: {
    early: [
      { name: "Insomnia", icon: "😴", sev: 1, traitKey: "severity", threshold: 0.1 },
      { name: "Memory Gaps", icon: "🧠", sev: 1, traitKey: "severity", threshold: 0.2 }
    ],
    mid: [
      { name: "Dementia", icon: "🧠", sev: 2, traitKey: "severity", threshold: 0.4 },
      { name: "Ataxia", icon: "🚶", sev: 2, traitKey: "severity", threshold: 0.5 }
    ],
    late: [
      { name: "Brain Sponging", icon: "🕳️", sev: 3, traitKey: "lethality", threshold: 0.4 },
      { name: "Cortical Death", icon: "💀", sev: 3, traitKey: "lethality", threshold: 0.6 }
    ]
  },
  nanobot: {
    early: [
      { name: "Metallic Taste", icon: "🔩", sev: 1, traitKey: "infectivity", threshold: 0.1 },
      { name: "Micro-tremors", icon: "🤚", sev: 1, traitKey: "severity", threshold: 0.2 }
    ],
    mid: [
      { name: "Neural Override", icon: "🧠", sev: 2, traitKey: "immuneEvasion", threshold: 0.4 },
      { name: "Tissue Rewrite", icon: "🧬", sev: 2, traitKey: "mutability", threshold: 0.5 }
    ],
    late: [
      { name: "Full Conversion", icon: "🤖", sev: 3, traitKey: "lethality", threshold: 0.4 },
      { name: "System Collapse", icon: "💔", sev: 3, traitKey: "lethality", threshold: 0.5 }
    ]
  },
  parasite: {
    early: [
      { name: "Nausea", icon: "🤢", sev: 1, traitKey: "severity", threshold: 0.15 },
      { name: "Abdominal Pain", icon: "😣", sev: 1, traitKey: "severity", threshold: 0.15 }
    ],
    mid: [
      { name: "Organ Cysts", icon: "🟣", sev: 2, traitKey: "resilience", threshold: 0.4 },
      { name: "Anemia", icon: "🩸", sev: 2, traitKey: "severity", threshold: 0.4 }
    ],
    late: [
      { name: "Brain Parasites", icon: "🧠", sev: 3, traitKey: "immuneEvasion", threshold: 0.5 },
      { name: "Systemic Failure", icon: "💔", sev: 3, traitKey: "lethality", threshold: 0.5 }
    ]
  }
};

var SEVERITY_LABELS = ["", "Mild", "Moderate", "Severe"];
var SEVERITY_COLORS = ["", "#4ade80", "#f59e0b", "#ef4444"];

function selectSymptoms(pathogenType, traits, rng) {
  var pool = SYMPTOM_POOLS[pathogenType] || SYMPTOM_POOLS.virus;
  var selected = [];
  function maybeAdd(symptom, probability, phase) {
    if (traits[symptom.traitKey] >= symptom.threshold && rng() < probability) {
      selected.push({ name: symptom.name, icon: symptom.icon, sev: symptom.sev, phase: phase });
    }
  }
  (pool.early || []).forEach(function (s) { maybeAdd(s, 0.7, "early"); });
  (pool.mid || []).forEach(function (s) { maybeAdd(s, 0.45, "mid"); });
  (pool.late || []).forEach(function (s) { maybeAdd(s, 0.25, "late"); });
  if (selected.length === 0 && pool.early && pool.early[0]) {
    selected.push({ name: pool.early[0].name, icon: pool.early[0].icon, sev: pool.early[0].sev, phase: "early" });
  }
  return selected;
}

function checkNewSymptom(pathogenType, traits, existingSymptoms, rng) {
  var pool = SYMPTOM_POOLS[pathogenType] || SYMPTOM_POOLS.virus;
  var all = []
    .concat((pool.early || []).map(function (s) { return Object.assign({ phase: "early" }, s); }))
    .concat((pool.mid || []).map(function (s) { return Object.assign({ phase: "mid" }, s); }))
    .concat((pool.late || []).map(function (s) { return Object.assign({ phase: "late" }, s); }));
  var existingNames = new Set(existingSymptoms.map(function (s) { return s.name; }));
  var candidates = all.filter(function (s) {
    return !existingNames.has(s.name) && traits[s.traitKey] >= s.threshold;
  });
  if (candidates.length === 0 || rng() >= 0.3) return null;
  return candidates[Math.floor(rng() * candidates.length)];
}

var COMORBIDITIES = [
  { id: "respiratory", name: "Respiratory Disease", icon: "🫁", defaultPrevalence: 0.08, severityMultiplier: 1.4, lethalityMultiplier: 1.6 },
  { id: "cardiovascular", name: "Cardiovascular Disease", icon: "❤️", defaultPrevalence: 0.10, severityMultiplier: 1.3, lethalityMultiplier: 1.8 },
  { id: "immunocompromised", name: "Immunodeficiency", icon: "🛡️", defaultPrevalence: 0.04, severityMultiplier: 1.5, lethalityMultiplier: 2.0 },
  { id: "metabolic", name: "Metabolic Disorder", icon: "⚗️", defaultPrevalence: 0.12, severityMultiplier: 1.2, lethalityMultiplier: 1.4 },
  { id: "neurological", name: "Neurological Condition", icon: "🧠", defaultPrevalence: 0.05, severityMultiplier: 1.1, lethalityMultiplier: 1.3 },
  { id: "augmentation", name: "Cybernetic Rejection", icon: "⚙️", defaultPrevalence: 0.03, severityMultiplier: 1.6, lethalityMultiplier: 1.5 }
];

function createDefaultComorbidities() {
  var result = {};
  COMORBIDITIES.forEach(function (c) { result[c.id] = c.defaultPrevalence; });
  return result;
}

function computeComorbidityMultipliers(comorbidities) {
  var lethalityMultiplier = 1;
  var severityMultiplier = 1;
  COMORBIDITIES.forEach(function (c) {
    var prev = comorbidities[c.id] || 0;
    lethalityMultiplier += prev * (c.lethalityMultiplier - 1);
    severityMultiplier += prev * (c.severityMultiplier - 1);
  });
  return { lethalityMultiplier: lethalityMultiplier, severityMultiplier: severityMultiplier };
}

function generateTraits(env, pathogenConfig, rng) {
  var planet = PLANETS.find(function (p) { return p.id === env.planet; }) || PLANETS[0];
  var temp = clamp(env.temperature + planet.tempMod, 0, 1);
  var humidity = clamp(env.humidity + planet.humMod, 0, 1);
  var radiation = clamp(env.radiation + planet.radMod, 0, 1);
  var generators = {
    virus: function () {
      return {
        infectivity: clamp(sampleRange(rng, 0.3, 0.7) + humidity * 0.2 + env.popDensity * 0.15, 0, 1),
        severity: clamp(sampleRange(rng, 0.1, 0.5) + radiation * 0.2, 0, 1),
        lethality: clamp(sampleRange(rng, 0.01, 0.15) + radiation * 0.15 - env.medTech * 0.1, 0, 1),
        mutability: clamp(sampleRange(rng, 0.2, 0.6) + radiation * 0.2 + pathogenConfig.mutationRate * 0.3, 0, 1),
        resilience: clamp(sampleRange(rng, 0.1, 0.4) + temp * 0.15, 0, 1),
        incubation: clamp(sampleRange(rng, 0.3, 0.7) - temp * 0.1, 0, 1),
        airborne: clamp(sampleRange(rng, 0.2, 0.8) + humidity * 0.1 - temp * 0.05, 0, 1),
        immuneEvasion: clamp(sampleRange(rng, 0.05, 0.3) + radiation * 0.15, 0, 1)
      };
    },
    bacteria: function () {
      return {
        infectivity: clamp(sampleRange(rng, 0.2, 0.6) + humidity * 0.25 + (1 - env.hygiene) * 0.2, 0, 1),
        severity: clamp(sampleRange(rng, 0.2, 0.6) + temp * 0.1, 0, 1),
        lethality: clamp(sampleRange(rng, 0.02, 0.2) - env.medTech * 0.15, 0, 1),
        mutability: clamp(sampleRange(rng, 0.1, 0.4) + radiation * 0.1 + pathogenConfig.mutationRate * 0.2, 0, 1),
        resilience: clamp(sampleRange(rng, 0.3, 0.7) + temp * 0.1 + humidity * 0.1, 0, 1),
        incubation: clamp(sampleRange(rng, 0.2, 0.5), 0, 1),
        airborne: clamp(sampleRange(rng, 0.05, 0.3), 0, 1),
        immuneEvasion: clamp(sampleRange(rng, 0.1, 0.4) + (1 - env.medTech) * 0.1, 0, 1)
      };
    },
    fungal: function () {
      return {
        infectivity: clamp(sampleRange(rng, 0.15, 0.5) + humidity * 0.35, 0, 1),
        severity: clamp(sampleRange(rng, 0.1, 0.4) + humidity * 0.15, 0, 1),
        lethality: clamp(sampleRange(rng, 0.01, 0.1) + temp * 0.05, 0, 1),
        mutability: clamp(sampleRange(rng, 0.05, 0.25) + pathogenConfig.mutationRate * 0.15, 0, 1),
        resilience: clamp(sampleRange(rng, 0.4, 0.8) + humidity * 0.15, 0, 1),
        incubation: clamp(sampleRange(rng, 0.5, 0.9), 0, 1),
        airborne: clamp(sampleRange(rng, 0.1, 0.5) + humidity * 0.2, 0, 1),
        immuneEvasion: clamp(sampleRange(rng, 0.05, 0.2), 0, 1)
      };
    },
    prion: function () {
      return {
        infectivity: clamp(sampleRange(rng, 0.05, 0.25), 0, 1),
        severity: clamp(sampleRange(rng, 0.6, 0.95), 0, 1),
        lethality: clamp(sampleRange(rng, 0.4, 0.8), 0, 1),
        mutability: clamp(sampleRange(rng, 0.01, 0.1), 0, 1),
        resilience: clamp(sampleRange(rng, 0.7, 0.95), 0, 1),
        incubation: clamp(sampleRange(rng, 0.7, 0.99), 0, 1),
        airborne: clamp(sampleRange(rng, 0, 0.05), 0, 1),
        immuneEvasion: clamp(sampleRange(rng, 0.5, 0.9), 0, 1)
      };
    },
    nanobot: function () {
      return {
        infectivity: clamp(sampleRange(rng, 0.3, 0.7) + env.connectivity * 0.2, 0, 1),
        severity: clamp(sampleRange(rng, 0.2, 0.7), 0, 1),
        lethality: clamp(sampleRange(rng, 0.05, 0.3), 0, 1),
        mutability: clamp(sampleRange(rng, 0.3, 0.7) + pathogenConfig.mutationRate * 0.3, 0, 1),
        resilience: clamp(sampleRange(rng, 0.5, 0.9) - humidity * 0.1, 0, 1),
        incubation: clamp(sampleRange(rng, 0.1, 0.3), 0, 1),
        airborne: clamp(sampleRange(rng, 0.1, 0.4), 0, 1),
        immuneEvasion: clamp(sampleRange(rng, 0.3, 0.7) + radiation * 0.1, 0, 1)
      };
    },
    parasite: function () {
      return {
        infectivity: clamp(sampleRange(rng, 0.2, 0.55) + temp * 0.15 + humidity * 0.1, 0, 1),
        severity: clamp(sampleRange(rng, 0.3, 0.7), 0, 1),
        lethality: clamp(sampleRange(rng, 0.05, 0.25), 0, 1),
        mutability: clamp(sampleRange(rng, 0.05, 0.3) + pathogenConfig.mutationRate * 0.1, 0, 1),
        resilience: clamp(sampleRange(rng, 0.3, 0.6) + temp * 0.2, 0, 1),
        incubation: clamp(sampleRange(rng, 0.4, 0.8), 0, 1),
        airborne: clamp(sampleRange(rng, 0, 0.1), 0, 1),
        immuneEvasion: clamp(sampleRange(rng, 0.2, 0.6) + temp * 0.1, 0, 1)
      };
    }
  };
  var generator = generators[pathogenConfig.type] || generators.virus;
  return generator();
}

function generatePathogen(env, pathogenConfig, rng) {
  var traits = generateTraits(env, pathogenConfig, rng);
  var symptoms = selectSymptoms(pathogenConfig.type, traits, rng);
  return { traits: traits, symptoms: symptoms };
}

function runSimulation(env, pathogenConfig, pathogen, comorbidities, days, rng) {
  var traits = pathogen.traits;
  var currentSymptoms = pathogen.symptoms.slice();
  var population = env.popSize;
  var initialInfected = Math.max(1, Math.round(population * 0.0001));
  var susceptible = population - initialInfected;
  var exposed = initialInfected;
  var infected = 0;
  var recovered = 0;
  var dead = 0;
  var vaccinated = 0;
  var mult = computeComorbidityMultipliers(comorbidities);
  var baseTransmission = traits.infectivity * 0.8 * (1 + env.popDensity * 0.6) * (1 + env.connectivity * 0.3);
  var sigma = lerp(0.05, 0.5, 1 - traits.incubation);
  var gammaBase = lerp(0.03, 0.15, 1 - traits.severity) / mult.severityMultiplier;
  var mu = traits.lethality * lerp(0.02, 0.12, traits.severity) * (1 - env.medTech * 0.5) * mult.lethalityMultiplier;
  var immuneWane = lerp(0.0005, 0.005, traits.immuneEvasion);
  var vaccinationRate = env.medTech > 0.5 ? lerp(0, 0.003, (env.medTech - 0.5) * 2) : 0;
  var vaccinationDelayDays = Math.round(lerp(120, 30, env.medTech));
  var birthRate = 0.00003;
  var natDeath = 0.00002;
  var currentTraits = Object.assign({}, traits);
  var data = [];
  var traitHistory = [];
  var mutationEvents = [];
  var totalInfectedFlow = 0;
  var peakInfected = 0;
  var peakDay = 0;

  for (var day = 0; day <= days; day += 1) {
    var livingPopulation = susceptible + exposed + infected + recovered + vaccinated;
    if (livingPopulation <= 0) break;
    data.push({
      day: day,
      S: Math.round(susceptible),
      E: Math.round(exposed),
      I: Math.round(infected),
      R: Math.round(recovered),
      D: Math.round(dead),
      V: Math.round(vaccinated),
      N: Math.round(livingPopulation),
      infectRate: infected / livingPopulation,
      immuneRate: (recovered + vaccinated) / livingPopulation,
      mortalityRate: totalInfectedFlow > 0 ? dead / totalInfectedFlow : 0,
      R0: (baseTransmission * (1 + currentTraits.infectivity * 0.3) * susceptible) / (livingPopulation * (gammaBase + mu))
    });
    traitHistory.push(Object.assign({ day: day }, currentTraits));
    if (infected > peakInfected) { peakInfected = infected; peakDay = day; }

    if (day > 0 && day % 14 === 0 && rng() < currentTraits.mutability * 0.6) {
      var traitKeys = Object.keys(currentTraits);
      var traitKey = traitKeys[Math.floor(rng() * traitKeys.length)];
      var delta = (rng() - 0.5) * 0.16 * (1 + env.radiation * 0.5);
      currentTraits[traitKey] = clamp(currentTraits[traitKey] + delta, 0, 1);
      var newSymptom = checkNewSymptom(pathogenConfig.type, currentTraits, currentSymptoms, rng);
      mutationEvents.push({
        day: day,
        trait: traitKey,
        delta: Number(delta.toFixed(4)),
        newValue: Number(currentTraits[traitKey].toFixed(4)),
        newSymptom: newSymptom ? newSymptom.name : null,
        symptomIcon: newSymptom ? newSymptom.icon : null,
        symptomSeverity: newSymptom ? newSymptom.sev : 0
      });
      if (newSymptom) currentSymptoms.push(newSymptom);
    }

    var betaEffective = baseTransmission * (1 + currentTraits.infectivity * 0.3) * (1 - env.hygiene * 0.3);
    var gammaEffective = gammaBase * (1 + env.medTech * 0.3);
    var newExposed = (betaEffective * susceptible * infected) / livingPopulation;
    var newInfected = sigma * exposed;
    var newRecovered = gammaEffective * infected;
    var newDead = mu * infected;
    var lostImmunity = immuneWane * recovered;
    var newVaccinated = day > vaccinationDelayDays ? vaccinationRate * susceptible : 0;
    susceptible += -newExposed - newVaccinated - natDeath * susceptible + birthRate * livingPopulation + lostImmunity;
    exposed += newExposed - newInfected;
    infected += newInfected - newRecovered - newDead;
    recovered += newRecovered - lostImmunity - natDeath * recovered;
    dead += newDead;
    vaccinated += newVaccinated;
    totalInfectedFlow += newInfected;
    susceptible = Math.max(0, susceptible);
    exposed = Math.max(0, exposed);
    infected = Math.max(0, infected);
    recovered = Math.max(0, recovered);
    vaccinated = Math.max(0, vaccinated);
  }

  return {
    data: data,
    traitHistory: traitHistory,
    mutationEvents: mutationEvents,
    totalInfected: Math.round(totalInfectedFlow),
    peakInfected: Math.round(peakInfected),
    peakDay: peakDay,
    totalDead: Math.round(dead),
    finalPopulation: Math.round(susceptible + exposed + infected + recovered + vaccinated),
    finalSymptoms: currentSymptoms,
    comorbidityLethalityMultiplier: mult.lethalityMultiplier,
    comorbiditySeverityMultiplier: mult.severityMultiplier
  };
}

var SLIDER_META = {
  temperature: { label: "🌡 Temperature", lowLabel: "-50°C", highLabel: "150°C", describe: function (v) { return Math.round(-50 + v * 200) + "°C"; } },
  humidity: { label: "💧 Humidity", lowLabel: "0%", highLabel: "100%", describe: function (v) { return Math.round(v * 100) + "% RH"; } },
  radiation: { label: "☢ Radiation", lowLabel: "0 mSv", highLabel: "500 mSv/yr", describe: function (v) { return Math.round(v * 500) + " mSv/yr"; } },
  popDensity: { label: "🏘 Pop. Density", lowLabel: "Rural", highLabel: "Mega-city", describe: function (v) { return v < 0.2 ? "Sparse" : v < 0.4 ? "Suburban" : v < 0.6 ? "Urban" : v < 0.8 ? "Dense" : "Mega-city"; } },
  medTech: { label: "🔬 Medical Tech", lowLabel: "Primitive", highLabel: "Advanced", describe: function (v) { return v < 0.2 ? "None" : v < 0.4 ? "Basic" : v < 0.6 ? "Modern" : v < 0.8 ? "Gene-tech" : "Nano-med"; } },
  hygiene: { label: "🧹 Hygiene", lowLabel: "None", highLabel: "Sterile", describe: function (v) { return v < 0.2 ? "None" : v < 0.4 ? "Basic" : v < 0.6 ? "Modern" : v < 0.8 ? "High" : "Sterile"; } },
  connectivity: { label: "✈ Connectivity", lowLabel: "Isolated", highLabel: "Hyper-linked", describe: function (v) { return v < 0.2 ? "Isolated" : v < 0.4 ? "Roads" : v < 0.6 ? "Air" : v < 0.8 ? "Orbital" : "Teleport"; } },
  immuneBaseline: { label: "🛡 Immune Baseline", lowLabel: "Weak", highLabel: "Hardened", describe: function (v) { return v < 0.2 ? "Compromised" : v < 0.4 ? "Weak" : v < 0.6 ? "Average" : v < 0.8 ? "Strong" : "Augmented"; } }
};

function Slider(props) {
  var id = props.id;
  var value = props.value;
  var onChange = props.onChange;
  var min = props.min;
  var max = props.max;
  var step = props.step;
  var meta = props.meta;
  if (min === undefined) min = 0;
  if (max === undefined) max = 1;
  if (step === undefined) step = 0.01;
  var metaData = meta || SLIDER_META[id];
  var percentage = ((value - min) / (max - min)) * 100;
  return (
    <div style={{ marginBottom: 10 }}>
      <div style={{ display: "flex", justifyContent: "space-between", fontSize: 11, marginBottom: 3 }}>
        <span style={{ color: "#67e8f9" }}>{metaData.label}</span>
        <span style={{ color: "#34d399", fontFamily: "monospace", fontWeight: 600 }}>{metaData.describe(value)}</span>
      </div>
      <div style={{ position: "relative", height: 20, display: "flex", alignItems: "center" }}>
        <div style={{ position: "absolute", left: 0, right: 0, height: 6, borderRadius: 3, background: "#1e293b" }} />
        <div style={{ position: "absolute", left: 0, width: percentage + "%", height: 6, borderRadius: 3, background: "linear-gradient(90deg, #06b6d4, #8b5cf6)" }} />
        <input type="range" min={min} max={max} step={step} value={value} onChange={function (e) { onChange(Number(e.target.value)); }} style={{ position: "relative", width: "100%", height: 20, opacity: 0, cursor: "pointer", zIndex: 2 }} />
        <div style={{ position: "absolute", left: "calc(" + percentage + "% - 7px)", width: 14, height: 14, borderRadius: "50%", background: "#c4b5fd", border: "2px solid #7c3aed", pointerEvents: "none" }} />
      </div>
      <div style={{ display: "flex", justifyContent: "space-between", fontSize: 9, color: "#64748b", marginTop: 1 }}>
        <span>{metaData.lowLabel}</span><span>{metaData.highLabel}</span>
      </div>
    </div>
  );
}

function TraitBar(props) {
  var name = props.name;
  var value = props.value;
  var color = props.color;
  return (
    <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 5 }}>
      <span style={{ fontSize: 11, width: 100, color: "#cbd5e1" }}>{name}</span>
      <div style={{ flex: 1, height: 12, background: "#1e293b", borderRadius: 6, overflow: "hidden" }}>
        <div style={{ height: "100%", width: (value * 100).toFixed(0) + "%", background: color, borderRadius: 6 }} />
      </div>
      <span style={{ fontSize: 10, width: 36, textAlign: "right", fontFamily: "monospace", color: "#94a3b8" }}>{(value * 100).toFixed(0)}%</span>
    </div>
  );
}

function StatCard(props) {
  var label = props.label;
  var value = props.value;
  var sub = props.sub;
  var color = props.color || "#06b6d4";
  return (
    <div style={{ background: "rgba(30,41,59,.8)", borderRadius: 10, padding: 12, border: "1px solid rgba(51,65,85,.5)" }}>
      <div style={{ fontSize: 10, color: "#94a3b8", marginBottom: 4 }}>{label}</div>
      <div style={{ fontSize: 17, fontWeight: 700, fontFamily: "monospace", color: color }}>{value}</div>
      {sub ? <div style={{ fontSize: 10, color: "#64748b", marginTop: 2 }}>{sub}</div> : null}
    </div>
  );
}

var defaultEnvironment = { planet: "terran", temperature: 0.5, humidity: 0.5, radiation: 0.1, popSize: 1000000, popDensity: 0.5, medTech: 0.3, hygiene: 0.4, connectivity: 0.5, immuneBaseline: 0.3 };
var defaultPathogenConfig = { type: "virus", mutationRate: 0.3 };
var TABS = ["🌍 Environment", "🦠 Pathogen", "📊 Simulation", "📖 Model Docs"];

function DiseaseSimulatorApp() {
  var tabIndexState = useState(0);
  var tabIndex = tabIndexState[0];
  var setTabIndex = tabIndexState[1];
  var environmentState = useState(defaultEnvironment);
  var environment = environmentState[0];
  var setEnvironment = environmentState[1];
  var pathogenConfigState = useState(defaultPathogenConfig);
  var pathogenConfig = pathogenConfigState[0];
  var setPathogenConfig = pathogenConfigState[1];
  var pathogenState = useState(null);
  var pathogen = pathogenState[0];
  var setPathogen = pathogenState[1];
  var simulationState = useState(null);
  var simulation = simulationState[0];
  var setSimulation = simulationState[1];
  var simulationDaysState = useState(365);
  var simulationDays = simulationDaysState[0];
  var setSimulationDays = simulationDaysState[1];
  var seedState = useState(42);
  var seed = seedState[0];
  var setSeed = seedState[1];
  var runningState = useState(false);
  var isRunning = runningState[0];
  var setIsRunning = runningState[1];
  var comorbiditiesState = useState(createDefaultComorbidities());
  var comorbidities = comorbiditiesState[0];
  var setComorbidities = comorbiditiesState[1];

  var setEnvironmentField = useCallback(function (key, value) {
    setEnvironment(function (prev) { var next = Object.assign({}, prev); next[key] = value; return next; });
  }, []);
  var setComorbidityField = useCallback(function (key, value) {
    setComorbidities(function (prev) { var next = Object.assign({}, prev); next[key] = value; return next; });
  }, []);

  var evolvePathogen = useCallback(function () {
    var rng = createMulberry32(seed >>> 0);
    var evolved = generatePathogen(environment, pathogenConfig, rng);
    setPathogen(evolved);
    setSimulation(null);
    setTabIndex(1);
  }, [environment, pathogenConfig, seed]);

  var runEpidemic = useCallback(function () {
    if (!pathogen) return;
    setIsRunning(true);
    var simRng = createMulberry32((seed * 9973 + 1) >>> 0);
    setTimeout(function () {
      var result = runSimulation(environment, pathogenConfig, pathogen, comorbidities, simulationDays, simRng);
      setSimulation(result);
      setIsRunning(false);
      setTabIndex(2);
    }, 40);
  }, [environment, pathogenConfig, pathogen, comorbidities, simulationDays, seed]);

  var traits = pathogen ? pathogen.traits : null;
  var radarData = traits
    ? [
        { trait: "Infectivity", value: traits.infectivity },
        { trait: "Severity", value: traits.severity },
        { trait: "Lethality", value: traits.lethality },
        { trait: "Mutability", value: traits.mutability },
        { trait: "Resilience", value: traits.resilience },
        { trait: "Incubation", value: traits.incubation },
        { trait: "Airborne", value: traits.airborne },
        { trait: "Immune Ev.", value: traits.immuneEvasion }
      ]
    : [];
  var chartColors = { S: "#3b82f6", E: "#f59e0b", I: "#ef4444", R: "#10b981", D: "#6b7280", V: "#8b5cf6" };
  var cardStyle = { background: "rgba(30,41,59,.5)", borderRadius: 14, padding: 16, border: "1px solid rgba(51,65,85,.4)" };
  var cardDense = { background: "rgba(30,41,59,.6)", borderRadius: 14, padding: 14, border: "1px solid rgba(51,65,85,.4)" };

  function renderEnvironmentTab() {
    return (
      <div>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(3,1fr)", gap: 8, marginBottom: 14 }}>
          {PLANETS.map(function (p) {
            var isActive = environment.planet === p.id;
            return (
              <button
                key={p.id}
                onClick={function () { setEnvironmentField("planet", p.id); }}
                style={{ padding: 10, borderRadius: 10, border: "1.5px solid " + (isActive ? "#06b6d4" : "#334155"), background: isActive ? "rgba(6,182,212,.12)" : "rgba(30,41,59,.6)", cursor: "pointer", textAlign: "center" }}
              >
                <div style={{ fontSize: 22 }}>{p.icon}</div>
                <div style={{ fontSize: 11, marginTop: 4, color: isActive ? "#06b6d4" : "#94a3b8" }}>{p.name}</div>
              </button>
            );
          })}
        </div>
        <div style={Object.assign({}, cardStyle, { display: "grid", gridTemplateColumns: "1fr 1fr", gap: "4px 20px", marginBottom: 14 })}>
          {Object.keys(SLIDER_META).map(function (key) {
            return <Slider key={key} id={key} value={environment[key]} onChange={function (v) { setEnvironmentField(key, v); }} />;
          })}
          <div style={{ gridColumn: "1 / -1" }}>
            <Slider id="popSize" value={environment.popSize} onChange={function (v) { setEnvironmentField("popSize", v); }} min={10000} max={1e9} step={10000} meta={{ label: "👥 Population", lowLabel: "10K", highLabel: "1B", describe: formatPopulation }} />
          </div>
        </div>
        <div style={Object.assign({}, cardStyle, { marginBottom: 14 })}>
          <div style={{ fontSize: 13, color: "#67e8f9", marginBottom: 8, fontWeight: 600 }}>🏥 Comorbidities</div>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "4px 20px" }}>
            {COMORBIDITIES.map(function (c) {
              var prev = comorbidities[c.id] || 0;
              var pct = (prev / 0.3) * 100;
              return (
                <div key={c.id} style={{ marginBottom: 8 }}>
                  <div style={{ display: "flex", justifyContent: "space-between", fontSize: 11, marginBottom: 2 }}>
                    <span style={{ color: "#e2e8f0" }}>{c.icon} {c.name}</span>
                    <span style={{ color: "#f59e0b", fontFamily: "monospace", fontWeight: 600 }}>{(prev * 100).toFixed(0)}%</span>
                  </div>
                  <div style={{ position: "relative", height: 20, display: "flex", alignItems: "center" }}>
                    <div style={{ position: "absolute", left: 0, right: 0, height: 6, borderRadius: 3, background: "#1e293b" }} />
                    <div style={{ position: "absolute", left: 0, width: pct + "%", height: 6, borderRadius: 3, background: "linear-gradient(90deg,#f59e0b,#ef4444)" }} />
                    <input type="range" min={0} max={0.3} step={0.005} value={prev} onChange={function (e) { setComorbidityField(c.id, Number(e.target.value)); }} style={{ position: "relative", width: "100%", height: 20, opacity: 0, cursor: "pointer", zIndex: 2 }} />
                  </div>
                </div>
              );
            })}
          </div>
        </div>
        <div style={Object.assign({}, cardStyle, { marginBottom: 14 })}>
          <div style={{ fontSize: 13, color: "#a78bfa", marginBottom: 8, fontWeight: 600 }}>🦠 Pathogen Class</div>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(3,1fr)", gap: 8, marginBottom: 10 }}>
            {PATHOGEN_TYPES.map(function (p) {
              var isActive = pathogenConfig.type === p.id;
              return (
                <button
                  key={p.id}
                  onClick={function () { setPathogenConfig(function (prev) { var n = Object.assign({}, prev); n.type = p.id; return n; }); }}
                  style={{ padding: 10, borderRadius: 10, border: "1.5px solid " + (isActive ? "#8b5cf6" : "#334155"), background: isActive ? "rgba(139,92,246,.12)" : "rgba(30,41,59,.4)", cursor: "pointer", textAlign: "center" }}
                >
                  <span style={{ fontSize: 20 }}>{p.icon}</span>
                  <div style={{ fontSize: 11, marginTop: 4, color: isActive ? "#a78bfa" : "#94a3b8" }}>{p.name}</div>
                </button>
              );
            })}
          </div>
          <Slider id="mutationRate" value={pathogenConfig.mutationRate} onChange={function (v) { setPathogenConfig(function (prev) { var n = Object.assign({}, prev); n.mutationRate = v; return n; }); }} meta={{ label: "⚡ Mutation Pressure", lowLabel: "0%", highLabel: "100%", describe: function (v) { return (v * 100).toFixed(0) + "%"; } }} />
        </div>
        <button onClick={evolvePathogen} style={{ width: "100%", padding: 12, borderRadius: 10, fontSize: 13, fontWeight: 700, border: "none", cursor: "pointer", background: "linear-gradient(135deg,#0e7490,#7c3aed)", color: "#fff" }}>
          🧬 EVOLVE PATHOGEN →
        </button>
      </div>
    );
  }

  function renderPathogenTab() {
    if (!pathogen || !traits) {
      return (
        <div style={cardStyle}>
          <div style={{ fontSize: 12, color: "#64748b" }}>Configure environment and pathogen class, then click EVOLVE PATHOGEN.</div>
        </div>
      );
    }
    var pathType = PATHOGEN_TYPES.find(function (p) { return p.id === pathogenConfig.type; }) || PATHOGEN_TYPES[0];
    var planet = PLANETS.find(function (p) { return p.id === environment.planet; }) || PLANETS[0];
    return (
      <div>
        <div style={Object.assign({}, cardDense, { marginBottom: 14, padding: 18 })}>
          <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 14 }}>
            <span style={{ fontSize: 32 }}>{pathType.icon}</span>
            <div>
              <div style={{ fontSize: 17, fontWeight: 700, color: "#c4b5fd" }}>{pathType.name}</div>
              <div style={{ fontSize: 11, color: "#94a3b8" }}>{planet.name} biome · Seed #{seed}</div>
            </div>
          </div>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "0 24px" }}>
            <div>
              <TraitBar name="Infectivity" value={traits.infectivity} color="#ef4444" />
              <TraitBar name="Severity" value={traits.severity} color="#f97316" />
              <TraitBar name="Lethality" value={traits.lethality} color="#dc2626" />
              <TraitBar name="Mutability" value={traits.mutability} color="#a855f7" />
            </div>
            <div>
              <TraitBar name="Resilience" value={traits.resilience} color="#eab308" />
              <TraitBar name="Incubation" value={traits.incubation} color="#06b6d4" />
              <TraitBar name="Airborne" value={traits.airborne} color="#3b82f6" />
              <TraitBar name="Immune Evasion" value={traits.immuneEvasion} color="#ec4899" />
            </div>
          </div>
        </div>
        <div style={Object.assign({}, cardDense, { marginBottom: 14 })}>
          <div style={{ fontSize: 13, color: "#67e8f9", marginBottom: 8, fontWeight: 600 }}>🩺 Symptoms ({pathogen.symptoms.length})</div>
          <div style={{ display: "flex", flexWrap: "wrap", gap: 6 }}>
            {pathogen.symptoms.map(function (s, i) {
              return (
                <div key={i} style={{ display: "inline-flex", alignItems: "center", gap: 4, background: "rgba(15,23,42,.6)", borderRadius: 6, padding: "4px 8px", border: "1px solid " + SEVERITY_COLORS[s.sev] + "33" }}>
                  <span style={{ fontSize: 14 }}>{s.icon}</span>
                  <span style={{ fontSize: 10, color: "#e2e8f0" }}>{s.name}</span>
                  <span style={{ fontSize: 8, color: SEVERITY_COLORS[s.sev], fontWeight: 600 }}>{SEVERITY_LABELS[s.sev]}</span>
                </div>
              );
            })}
          </div>
        </div>
        {ResponsiveContainer && RadarChart && radarData.length > 0 ? (
          <div style={Object.assign({}, cardDense, { marginBottom: 14, height: 220 })}>
            <div style={{ fontSize: 11, color: "#67e8f9", marginBottom: 4, fontWeight: 600 }}>Trait Radar</div>
            <ResponsiveContainer width="100%" height="85%">
              <RadarChart data={radarData}>
                <PolarGrid stroke="#334155" />
                <PolarAngleAxis dataKey="trait" tick={{ fill: "#94a3b8", fontSize: 9 }} />
                <PolarRadiusAxis domain={[0, 1]} tick={false} axisLine={false} />
                <Radar dataKey="value" stroke="#8b5cf6" fill="#8b5cf6" fillOpacity={0.3} strokeWidth={2} />
              </RadarChart>
            </ResponsiveContainer>
          </div>
        ) : null}
        <div style={Object.assign({}, cardStyle, { marginBottom: 14 })}>
          <Slider id="simDays" value={simulationDays} onChange={setSimulationDays} min={30} max={730} step={1} meta={{ label: "📅 Simulation Length", lowLabel: "30d", highLabel: "730d", describe: function (v) { return v + " days"; } }} />
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <button onClick={function () { setPathogen(null); setSimulation(null); setTabIndex(0); }} style={{ flex: 1, padding: 10, borderRadius: 8, fontSize: 11, fontWeight: 600, background: "transparent", border: "1px solid #475569", color: "#94a3b8", cursor: "pointer" }}>← BACK</button>
          <button onClick={evolvePathogen} style={{ flex: 1, padding: 10, borderRadius: 8, fontSize: 11, fontWeight: 600, background: "transparent", border: "1px solid #7c3aed", color: "#c4b5fd", cursor: "pointer" }}>🔄 RE-EVOLVE</button>
          <button onClick={runEpidemic} disabled={isRunning} style={{ flex: 2, padding: 10, borderRadius: 8, fontSize: 12, fontWeight: 700, border: "none", cursor: isRunning ? "wait" : "pointer", background: isRunning ? "#475569" : "linear-gradient(135deg,#dc2626,#9333ea)", color: "#fff" }}>{isRunning ? "⏳ SIMULATING…" : "▶ RUN EPIDEMIC →"}</button>
        </div>
      </div>
    );
  }

  function renderSimulationTab() {
    if (!simulation) {
      return (
        <div style={cardStyle}>
          <div style={{ fontSize: 12, color: "#64748b" }}>Run an epidemic from the Pathogen tab to see results.</div>
        </div>
      );
    }
    var hasCharts = ResponsiveContainer && AreaChart && LineChart;
    return (
      <div>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(4,1fr)", gap: 8, marginBottom: 12 }}>
          <StatCard label="Total Infected" value={formatPopulation(simulation.totalInfected)} sub={formatPercent(simulation.totalInfected / environment.popSize)} color="#ef4444" />
          <StatCard label="Deaths" value={formatPopulation(simulation.totalDead)} sub={"CFR: " + (simulation.totalInfected > 0 ? formatPercent(simulation.totalDead / simulation.totalInfected) : "0%")} color="#6b7280" />
          <StatCard label="Peak" value={formatPopulation(simulation.peakInfected)} sub={"Day " + simulation.peakDay} color="#f59e0b" />
          <StatCard label="Survivors" value={formatPopulation(simulation.finalPopulation)} sub={formatPercent(simulation.finalPopulation / environment.popSize)} color="#10b981" />
        </div>
        {simulation.comorbidityLethalityMultiplier > 1.01 ? (
          <div style={{ marginBottom: 10, padding: "8px 14px", background: "rgba(245,158,11,.08)", borderRadius: 10, border: "1px solid rgba(245,158,11,.2)", fontSize: 11, color: "#fbbf24" }}>
            🏥 Comorbidities increased lethality ×{simulation.comorbidityLethalityMultiplier.toFixed(3)} and slowed recovery ×{simulation.comorbiditySeverityMultiplier.toFixed(3)}
          </div>
        ) : null}
        {hasCharts && simulation.data && simulation.data.length > 0 ? (
          <div style={Object.assign({}, cardDense, { marginBottom: 10, height: 260 })}>
            <div style={{ fontSize: 11, color: "#67e8f9", marginBottom: 4, fontWeight: 600 }}>Disease Spread (SEIRDV)</div>
            <ResponsiveContainer width="100%" height="88%">
              <AreaChart data={simulation.data} margin={{ top: 5, right: 5, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
                <XAxis dataKey="day" tick={{ fill: "#64748b", fontSize: 9 }} />
                <YAxis tick={{ fill: "#64748b", fontSize: 9 }} tickFormatter={formatPopulation} />
                <Tooltip contentStyle={{ background: "#1e293b", border: "1px solid #334155", borderRadius: 8, fontSize: 10 }} formatter={function (v) { return formatPopulation(v); }} />
                <Area type="monotone" dataKey="S" stackId="1" stroke={chartColors.S} fill={chartColors.S} fillOpacity={0.6} name="Susceptible" />
                <Area type="monotone" dataKey="E" stackId="1" stroke={chartColors.E} fill={chartColors.E} fillOpacity={0.6} name="Exposed" />
                <Area type="monotone" dataKey="I" stackId="1" stroke={chartColors.I} fill={chartColors.I} fillOpacity={0.7} name="Infected" />
                <Area type="monotone" dataKey="R" stackId="1" stroke={chartColors.R} fill={chartColors.R} fillOpacity={0.6} name="Recovered" />
                <Area type="monotone" dataKey="V" stackId="1" stroke={chartColors.V} fill={chartColors.V} fillOpacity={0.5} name="Vaccinated" />
                <Area type="monotone" dataKey="D" stackId="1" stroke={chartColors.D} fill={chartColors.D} fillOpacity={0.5} name="Dead" />
                <Legend wrapperStyle={{ fontSize: 9 }} />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        ) : null}
        {simulation.mutationEvents && simulation.mutationEvents.length > 0 ? (
          <div style={Object.assign({}, cardDense, { marginBottom: 10 })}>
            <div style={{ fontSize: 11, color: "#67e8f9", marginBottom: 6, fontWeight: 600 }}>🧬 Mutations ({simulation.mutationEvents.length})</div>
            <div style={{ maxHeight: 120, overflowY: "auto" }}>
              {simulation.mutationEvents.map(function (m, i) {
                return (
                  <div key={i} style={{ padding: "4px 0", borderBottom: "1px solid rgba(51,65,85,.3)", fontSize: 10 }}>
                    <span style={{ color: "#64748b", marginRight: 8 }}>Day {m.day}</span>
                    <span style={{ color: "#c4b5fd", marginRight: 8 }}>{m.trait}</span>
                    <span style={{ color: m.delta > 0 ? "#f87171" : "#4ade80" }}>{m.delta > 0 ? "▲" : "▼"} {Math.abs(m.delta).toFixed(4)}</span>
                    <span style={{ color: "#94a3b8", marginLeft: 4 }}>→ {(m.newValue * 100).toFixed(1)}%</span>
                    {m.newSymptom ? <div style={{ marginTop: 2, marginLeft: 60, color: "#fbbf24", fontSize: 9 }}>NEW: {m.symptomIcon} {m.newSymptom}</div> : null}
                  </div>
                );
              })}
            </div>
          </div>
        ) : null}
        <div style={{ display: "flex", gap: 8 }}>
          <button onClick={function () { setTabIndex(1); }} style={{ flex: 1, padding: 10, borderRadius: 8, fontSize: 11, fontWeight: 600, background: "transparent", border: "1px solid #475569", color: "#94a3b8", cursor: "pointer" }}>← PATHOGEN</button>
          <button onClick={runEpidemic} style={{ flex: 2, padding: 10, borderRadius: 8, fontSize: 12, fontWeight: 700, border: "none", cursor: "pointer", background: "linear-gradient(135deg,#dc2626,#9333ea)", color: "#fff" }}>🔄 RE-RUN</button>
        </div>
      </div>
    );
  }

  function renderModelDocsTab() {
    var s = { section: { marginBottom: 16 }, h2: { fontSize: 14, fontWeight: 700, color: "#67e8f9", marginBottom: 5, borderBottom: "1px solid #334155", paddingBottom: 3 }, p: { fontSize: 11, color: "#cbd5e1", lineHeight: 1.6, marginBottom: 5 }, code: { background: "#1e293b", color: "#34d399", padding: "1px 4px", borderRadius: 3, fontFamily: "monospace", fontSize: 10 }, ul: { fontSize: 11, color: "#cbd5e1", lineHeight: 1.7, paddingLeft: 16, marginBottom: 6 } };
    return (
      <div style={Object.assign({}, cardStyle, { maxHeight: "70vh", overflowY: "auto" })}>
        <div style={s.section}>
          <h2 style={s.h2}>1. SEIRDV Model</h2>
          <p style={s.p}>Compartments: S (Susceptible), E (Exposed), I (Infected), R (Recovered), D (Dead), V (Vaccinated). RNG is injected; same seed + same inputs → identical results.</p>
        </div>
        <div style={s.section}>
          <h2 style={s.h2}>2. Comorbidities</h2>
          <p style={s.p}>Six conditions with prevalence 0–30%. Each has severity and lethality multipliers. Effective multiplier = 1 + Σ(prevalence × (mult − 1)). Slows recovery (γ) and amplifies death rate (μ).</p>
        </div>
        <div style={s.section}>
          <h2 style={s.h2}>3. Symptoms</h2>
          <p style={s.p}>Per-pathogen-type pools (early/mid/late). Selection by trait threshold + probability (70% / 45% / 25%). New symptoms can emerge on mutation (30% chance).</p>
        </div>
        <div style={s.section}>
          <h2 style={s.h2}>4. Assumptions</h2>
          <ul style={s.ul}>
            <li>Homogeneous mixing; single strain; no age structure</li>
            <li>Perfect vaccination; no behavioral interventions</li>
            <li>Symptoms are presentational only</li>
          </ul>
        </div>
      </div>
    );
  }

  var tabContents = [renderEnvironmentTab(), renderPathogenTab(), renderSimulationTab(), renderModelDocsTab()];

  return (
    <div style={{ background: "linear-gradient(135deg,#0f172a 0%,#1e1b4b 50%,#0f172a 100%)", minHeight: "100vh", color: "#e2e8f0", fontFamily: "'Segoe UI', system-ui, sans-serif" }}>
      <div style={{ maxWidth: 1100, margin: "0 auto", padding: "16px 12px" }}>
        <div style={{ textAlign: "center", marginBottom: 12 }}>
          <h1 style={{ fontSize: 24, fontWeight: 800, background: "linear-gradient(90deg,#06b6d4,#8b5cf6,#ec4899)", WebkitBackgroundClip: "text", WebkitTextFillColor: "transparent", letterSpacing: 1 }}>☣ PATHOGEN EVOLUTION SIMULATOR</h1>
          <p style={{ color: "#94a3b8", fontSize: 11, marginTop: 4 }}>Concept 9 · Deterministic · Symptoms · Comorbidities</p>
        </div>
        <div style={{ display: "flex", gap: 3, marginBottom: 12, background: "rgba(30,41,59,.6)", borderRadius: 10, padding: 3 }}>
          {TABS.map(function (t, i) {
            return (
              <button
                key={t}
                onClick={function () { setTabIndex(i); }}
                style={{ flex: 1, padding: "7px 4px", borderRadius: 8, fontSize: 11, fontWeight: 600, border: "none", cursor: "pointer", background: tabIndex === i ? "linear-gradient(135deg,#0e7490,#6d28d9)" : "transparent", color: tabIndex === i ? "#fff" : "#94a3b8", opacity: ((i === 1 && !pathogen) || (i === 2 && !simulation)) ? 0.35 : 1 }}
              >
                {t}
              </button>
            );
          })}
        </div>
        <div style={Object.assign({}, cardStyle, { display: "flex", alignItems: "center", gap: 8, marginBottom: 12, padding: "8px 14px" })}>
          <span style={{ fontSize: 11, color: "#67e8f9", fontWeight: 600 }}>🎲 Seed:</span>
          <input type="number" value={seed} onChange={function (e) { setSeed(parseInt(e.target.value, 10) || 0); }} style={{ background: "#0f172a", border: "1px solid #334155", borderRadius: 6, padding: "4px 10px", color: "#34d399", fontFamily: "monospace", fontSize: 13, width: 90 }} />
          <span style={{ fontSize: 10, color: "#64748b" }}>Same seed + params = identical results</span>
        </div>
        {tabContents[tabIndex]}
      </div>
    </div>
  );
}

if (typeof document !== "undefined" && document.getElementById("root")) {
  var root = ReactDOM.createRoot(document.getElementById("root"));
  root.render(React.createElement(DiseaseSimulatorApp));
}
