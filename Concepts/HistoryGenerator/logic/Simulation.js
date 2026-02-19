/** Culture generation, barriers, relations, and turn simulation. */
function generateCulture(id, climate, terrain, techLevel, age) {
  var REGIMES = window.REGIMES_HG;
  var pickN = window.pickN;
  var pick = window.pick;
  var rand = window.rand;
  var clamp = window.clamp;
  var genName = window.genName;
  var genRuler = window.genRuler;
  var RESOURCES = window.RESOURCES;
  var RELIGIONS = window.RELIGIONS;
  var LANGUAGES = window.LANGUAGES;

  var resources = pickN(RESOURCES, rand(2, 4));
  var scale = clamp(rand(1, techLevel + 1) + (terrain === "Riverine" ? 1 : 0), 1, 5);
  var infoTech = clamp(techLevel, 1, 5);
  var revBase = resources.some(function (r) { return ["Gold", "Metals", "Oil"].indexOf(r) >= 0; }) ? rand(2, 5) : rand(1, 4);
  var threat = rand(1, 5);
  var legitimacy = pick(["Religious", "Traditional", "Ideological", "National", "Charismatic"]);
  var coercion = clamp(Math.round((scale + threat) / 2), 1, 5);
  var capacity = clamp(Math.round((infoTech + revBase + scale) / 3), 1, 5);
  var inclusiveness = clamp(rand(1, 3) + (terrain === "Coastal" ? 1 : 0), 1, 5);

  var regime;
  if (capacity <= 2 && age < 3) {
    regime = "tribal";
  } else if (capacity <= 2) {
    regime = pick(["chiefdom", "feudal"]);
  } else if (terrain === "Coastal" && inclusiveness >= 3) {
    regime = pick(["cityState", "eliteRepublic"]);
  } else if (coercion >= 4 && capacity >= 4) {
    regime = pick(["empire", "absolutist", "oneParty"]);
  } else if (inclusiveness >= 4 && capacity >= 3) {
    regime = pick(["democracy", "eliteRepublic", "constitutional"]);
  } else {
    regime = pick(["patrimonial", "absolutist", "feudal"]);
  }

  var economies = [];
  if (resources.some(function (r) { return ["Grain", "Livestock"].indexOf(r) >= 0; })) economies.push("Agricultural");
  if (terrain === "Coastal" || terrain === "Riverine") economies.push("Trade");
  if (resources.some(function (r) { return ["Metals", "Gold", "Stone", "Coal"].indexOf(r) >= 0; })) economies.push("Extractive");
  if (techLevel >= 3) economies.push("Manufacturing");
  if (economies.length === 0) economies.push("Pastoral");

  var pop = scale * rand(100, 500) * 1000;

  return {
    id: id,
    name: genName(),
    climate: climate,
    terrain: terrain,
    resources: resources,
    regime: regime,
    religion: pick(RELIGIONS),
    language: pick(LANGUAGES),
    ruler: genRuler(),
    dynasty: genName() + " Dynasty",
    drivers: { scale: scale, infoTech: infoTech, revBase: revBase, threat: threat, legitimacy: legitimacy },
    sliders: { coercion: coercion, capacity: capacity, inclusiveness: inclusiveness },
    economy: economies,
    gdp: pop * rand(50, 200),
    tradeIncome: 0,
    population: pop,
    military: coercion * rand(5, 15) * 1000,
    stability: rand(40, 85),
    techLevel: techLevel,
    age: age,
    culturalInfluence: rand(10, 50),
    history: [{ year: 0, event: genName() + " founded as a " + REGIMES[regime].name }],
    x: rand(60, 540),
    y: rand(60, 340),
    vassalOf: null,
    vassals: [],
  };
}

function generateBarriers(cultures) {
  var TERRAIN_BARRIERS = window.TERRAIN_BARRIERS;
  var barriers = [];
  for (var i = 0; i < cultures.length; i++) {
    for (var j = i + 1; j < cultures.length; j++) {
      var a = cultures[i];
      var b = cultures[j];
      var dist = Math.sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
      if (dist > 350) continue;

      var barrier;
      if (a.terrain === "Mountain" || b.terrain === "Mountain") barrier = TERRAIN_BARRIERS[0];
      else if (a.terrain === "Coastal" && b.terrain === "Coastal") barrier = TERRAIN_BARRIERS[1];
      else if (a.terrain === "Riverine" || b.terrain === "Riverine") barrier = TERRAIN_BARRIERS[2];
      else if (a.terrain === "Forest" || b.terrain === "Forest") barrier = TERRAIN_BARRIERS[3];
      else if (a.terrain === "Desert" || b.terrain === "Desert") barrier = TERRAIN_BARRIERS[4];
      else barrier = TERRAIN_BARRIERS[5];

      barriers.push({ a: a.id, b: b.id, type: barrier.type, icon: barrier.icon, moveCost: barrier.moveCost, tradeCost: barrier.tradeCost, dist: dist });
    }
  }
  return barriers;
}

function generateRelations(cultures, barriers) {
  var rand = window.rand;
  return barriers
    .map(function (bar) {
      var a = cultures.find(function (c) { return c.id === bar.a; });
      var b = cultures.find(function (c) { return c.id === bar.b; });
      if (!a || !b) return null;

      var tension = Math.abs(a.sliders.coercion - b.sliders.coercion) + a.drivers.threat / 2 + rand(-2, 2);
      var sameReligion = a.religion === b.religion;
      var sameLang = a.language === b.language;
      var tradeBonus = a.economy.indexOf("Trade") >= 0 || b.economy.indexOf("Trade") >= 0 ? 2 : 0;
      var tradePot = Math.max(0, 5 - bar.tradeCost + tradeBonus + (sameReligion ? 1 : 0) + (sameLang ? 1 : 0));

      var status;
      if (a.vassalOf === b.id || b.vassalOf === a.id) status = "Vassal";
      else if (tension > 7) status = "War";
      else if (tension > 5) status = "Hostile";
      else if (tradePot > 5) status = "Alliance";
      else if (tradePot > 2) status = "Trade Partners";
      else status = "Neutral";

      return {
        a: bar.a,
        b: bar.b,
        icon: bar.icon,
        dist: bar.dist,
        status: status,
        tension: tension,
        tradePotential: tradePot,
        sameReligion: sameReligion,
        sameLang: sameLang,
      };
    })
    .filter(Boolean);
}

function simulateTurn(cultures, relations, year, yearsToAdvance) {
  var REGIMES = window.REGIMES_HG;
  var TRANSITIONS = window.TRANSITIONS_HG;
  var RELIGIONS = window.RELIGIONS;
  var rand = window.rand;
  var pick = window.pick;
  var clamp = window.clamp;
  var genName = window.genName;
  var genRuler = window.genRuler;

  var events = [];
  var updated = cultures.map(function (c) {
    var copy = {};
    for (var key in c) if (c.hasOwnProperty(key)) copy[key] = c[key];
    copy.drivers = { scale: c.drivers.scale, infoTech: c.drivers.infoTech, revBase: c.drivers.revBase, threat: c.drivers.threat, legitimacy: c.drivers.legitimacy };
    copy.sliders = { coercion: c.sliders.coercion, capacity: c.sliders.capacity, inclusiveness: c.sliders.inclusiveness };
    copy.history = c.history.slice();
    copy.vassals = c.vassals.slice();
    return copy;
  });

  updated.forEach(function (c) {
    var r = Math.random();
    var rels = relations.filter(function (rel) { return rel.a === c.id || rel.b === c.id; });
    var wars = rels.filter(function (rel) { return rel.status === "War"; });
    var trades = rels.filter(function (rel) { return rel.status === "Trade Partners" || rel.status === "Alliance"; });

    c.tradeIncome = trades.reduce(function (sum, t) { return sum + t.tradePotential * 1000000; }, 0);
    c.gdp = c.population * (100 + c.techLevel * 20) + c.tradeIncome;

    if (r < 0.1 + trades.length * 0.02 + (c.stability > 60 ? 0.05 : 0)) {
      if (c.techLevel < 5) {
        c.techLevel = clamp(c.techLevel + 1, 1, 5);
        c.sliders.capacity = clamp(c.sliders.capacity + 1, 1, 5);
        var ev = c.name + " advances in technology";
        events.push({ type: "tech", cultureId: c.id, desc: ev });
        c.history.push({ year: year, event: ev });
      }
    }

    wars.forEach(function (war) {
      var enemy = updated.find(function (x) { return x.id === (war.a === c.id ? war.b : war.a); });
      if (!enemy || enemy.vassalOf === c.id) return;

      var cPower = c.military * (c.sliders.capacity / 3) * (c.stability / 50);
      var ePower = enemy.military * (enemy.sliders.capacity / 3) * (enemy.stability / 50);

      if (Math.random() < 0.3) {
        if (cPower > ePower * 1.5 && Math.random() < 0.4) {
          enemy.vassalOf = c.id;
          c.vassals.push(enemy.id);
          var ev = c.name + " conquers " + enemy.name + ", making them a vassal";
          events.push({ type: "conquest", cultureId: c.id, desc: ev });
          c.history.push({ year: year, event: "Conquered " + enemy.name });
          enemy.history.push({ year: year, event: "Subjugated by " + c.name });
          c.culturalInfluence += 15;
          enemy.stability -= 20;
        } else {
          var cLoss = rand(5, 15) * 1000;
          var eLoss = rand(5, 15) * 1000;
          c.military = Math.max(1000, c.military - cLoss);
          enemy.military = Math.max(1000, enemy.military - eLoss);
          c.stability -= rand(3, 8);
          enemy.stability -= rand(3, 8);
          var ev = "Battle between " + c.name + " and " + enemy.name;
          events.push({ type: "battle", desc: ev });
        }
      }
    });

    if (c.stability < 35 && r < 0.35) {
      var transitions = TRANSITIONS[c.regime] || ["failed"];
      var newRegime = pick(transitions);
      var ev = c.name + ": " + REGIMES[c.regime].name + " → " + REGIMES[newRegime].name;
      events.push({ type: "regime", cultureId: c.id, from: c.regime, to: newRegime, desc: ev });
      c.history.push({ year: year, event: "Regime change to " + REGIMES[newRegime].name });
      c.regime = newRegime;
      c.ruler = genRuler();
      c.dynasty = Math.random() < 0.5 ? genName() + " Dynasty" : c.dynasty;
      c.stability = rand(45, 65);
    }

    if (c.culturalInfluence > 40) {
      var neighbors = rels.filter(function (rel) { return rel.dist < 200; });
      neighbors.forEach(function (n) {
        var neighbor = updated.find(function (x) { return x.id === (n.a === c.id ? n.b : n.a); });
        if (!neighbor) return;
        if (Math.random() < 0.08 * (c.culturalInfluence / 100)) {
          if (!n.sameReligion && Math.random() < 0.5) {
            var ev = neighbor.name + " adopts " + c.religion + " from " + c.name;
            events.push({ type: "cultural", desc: ev });
            neighbor.history.push({ year: year, event: "Adopted " + c.religion });
            neighbor.religion = c.religion;
          } else if (!n.sameLang) {
            var ev = c.language + " spreads to " + neighbor.name;
            events.push({ type: "cultural", desc: ev });
            neighbor.language = c.language;
          }
        }
      });
    }

    if (r < 0.12) {
      var crisis = pick(["Famine", "Plague", "Revolt", "Succession Crisis", "Religious Schism", "Economic Collapse"]);
      var ev = c.name + ": " + crisis;
      events.push({ type: "crisis", cultureId: c.id, desc: ev });
      c.history.push({ year: year, event: crisis });
      c.stability -= rand(10, 25);
      if (crisis === "Succession Crisis") c.ruler = genRuler();
      if (crisis === "Religious Schism") c.religion = pick(RELIGIONS);
    } else if (r < 0.22) {
      var growth = pick(["Golden Age", "Population Boom", "Trade Expansion", "Military Victory", "Cultural Renaissance", "Reform Movement"]);
      var ev = c.name + ": " + growth;
      events.push({ type: "growth", cultureId: c.id, desc: ev });
      c.history.push({ year: year, event: growth });
      c.stability += rand(5, 12);
      c.culturalInfluence += rand(3, 10);
      if (growth === "Population Boom") c.population *= 1.1;
      if (growth === "Military Victory") c.military *= 1.15;
    }

    if (Math.random() < 0.05) {
      var oldRuler = c.ruler;
      c.ruler = genRuler();
      var ev = c.name + ": " + oldRuler + " dies, " + c.ruler + " takes power";
      events.push({ type: "ruler", cultureId: c.id, desc: ev });
      c.history.push({ year: year, event: c.ruler + " ascends" });
      if (Math.random() < 0.3) c.stability -= rand(5, 15);
    }

    if (c.vassalOf !== null && c.stability > 70 && Math.random() < 0.15) {
      var overlord = updated.find(function (x) { return x.id === c.vassalOf; });
      if (overlord) {
        overlord.vassals = overlord.vassals.filter(function (v) { return v !== c.id; });
        var ev = c.name + " declares independence from " + overlord.name;
        events.push({ type: "independence", desc: ev });
        c.history.push({ year: year, event: "Independence from " + overlord.name });
        c.vassalOf = null;
      }
    }

    c.stability = clamp(c.stability + rand(-2, 3), 10, 95);
    c.population = Math.round(c.population * (1 + (c.stability > 50 ? 0.01 : -0.005) * (yearsToAdvance / 10)));
    c.military = Math.round(c.military * (1 + (c.stability > 50 ? 0.005 : -0.01) * (yearsToAdvance / 10)));
    c.age = clamp(c.age + 1, 1, 6);
  });

  return {
    cultures: updated,
    events: events.length ? events : [{ type: "quiet", desc: "A quiet era passes." }],
  };
}

window.generateCulture = generateCulture;
window.generateBarriers = generateBarriers;
window.generateRelations = generateRelations;
window.simulateTurn = simulateTurn;
