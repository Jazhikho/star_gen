/**
 * Simulation for Integration: generateCulture, buildRelations, simulateTurn.
 * Uses shared REGIMES/TRANSITIONS and validRegimesForLevel / pickRegimeForLevel.
 */
(function () {
  "use strict";

  function barrierBetween(a, b) {
    var TB = window.TERRAIN_BARRIERS;
    var t = (a.terrain === "Mountain" || b.terrain === "Mountain") ? "Mountain"
      : (a.terrain === "Coastal" && b.terrain === "Coastal") ? "Coastal"
      : (a.terrain === "Riverine" || b.terrain === "Riverine") ? "Riverine"
      : (a.terrain === "Forest" || b.terrain === "Forest") ? "Forest"
      : (a.terrain === "Desert" || b.terrain === "Desert") ? "Desert"
      : (a.terrain === "Island" || b.terrain === "Island") ? "Island"
      : "Plains";
    var bar = TB[t] || TB.Plains;
    return { type: t, icon: bar.icon, moveCost: bar.moveCost, tradeCost: bar.tradeCost };
  }

  function generateCulture(id, climate, terrain, techLevel) {
    var REGIMES = window.REGIMES;
    var pickN = window.pickN;
    var pick = window.pick;
    var rand = window.rand;
    var clamp = window.clamp;
    var genName = window.genName;
    var genRuler = window.genRuler;
    var pickRegimeForLevel = window.pickRegimeForLevel;
    var RESOURCES = window.RESOURCES;
    var RELIGIONS = window.RELIGIONS;
    var LANGUAGES = window.LANGUAGES;

    var resources = pickN(RESOURCES, rand(2, 4));
    var revBase = resources.some(function (r) { return ["Gold", "Metals", "Oil", "Uranium"].indexOf(r) >= 0; }) ? rand(2, 5) : rand(1, 4);
    var threat = rand(1, 5);
    var scale = clamp(rand(1, techLevel) + (terrain === "Riverine" ? 1 : 0), 1, 5);
    var coercion = clamp(Math.round((scale + threat) / 2), 1, 5);
    var capacity = clamp(Math.round((techLevel / 3 + revBase + scale) / 3), 1, 5);
    var inclusiveness = clamp(rand(1, 3) + (terrain === "Coastal" ? 1 : 0), 1, 5);
    var regime = pickRegimeForLevel(techLevel, terrain);
    var pop = scale * rand(100, 500) * 1000 * Math.pow(1.5, techLevel / 3);

    var economies = [];
    if (resources.some(function (r) { return ["Grain", "Livestock"].indexOf(r) >= 0; })) economies.push("Agricultural");
    if (terrain === "Coastal" || terrain === "Riverine") economies.push("Trade");
    if (resources.some(function (r) { return ["Metals", "Gold", "Stone", "Coal", "Oil", "Uranium"].indexOf(r) >= 0; })) economies.push("Extractive");
    if (techLevel >= 5) economies.push("Manufacturing");
    if (techLevel >= 11) economies.push("Industrial");
    if (techLevel >= 13) economies.push("Digital");
    if (!economies.length) economies.push("Pastoral");

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
      sliders: { coercion: coercion, capacity: capacity, inclusiveness: inclusiveness },
      drivers: { scale: scale, revBase: revBase, threat: threat, legitimacy: pick(["Religious", "Traditional", "Ideological", "National", "Charismatic", "Technocratic"]) },
      economy: economies,
      techLevel: techLevel,
      gdp: pop * (50 + techLevel * 30),
      tradeIncome: 0,
      population: Math.round(pop),
      military: coercion * rand(5, 15) * 1000 * (1 + techLevel / 5),
      stability: rand(45, 85),
      culturalInfluence: rand(10, 50),
      history: [{ year: 0, event: "Founded as a " + REGIMES[regime].name }],
      x: rand(60, 540),
      y: rand(60, 340),
      vassalOf: null,
      vassals: [],
    };
  }

  function buildRelations(cultures) {
    var rand = window.rand;
    var rels = [];
    for (var i = 0; i < cultures.length; i++) {
      for (var j = i + 1; j < cultures.length; j++) {
        var a = cultures[i];
        var b = cultures[j];
        var dist = Math.sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        if (dist > 320) continue;
        var bar = barrierBetween(a, b);
        var sameRel = a.religion === b.religion;
        var sameLang = a.language === b.language;
        var tradeBonus = (a.economy.indexOf("Trade") >= 0 || b.economy.indexOf("Trade") >= 0) ? 2 : 0;
        var tradePot = Math.max(0, 5 - bar.tradeCost + tradeBonus + (sameRel ? 1 : 0) + (sameLang ? 1 : 0));
        var tension = Math.abs(a.sliders.coercion - b.sliders.coercion) + a.drivers.threat / 2 + rand(-2, 2);
        var status;
        if (a.vassalOf === b.id || b.vassalOf === a.id) status = "Vassal";
        else if (tension > 7) status = "War";
        else if (tension > 5) status = "Hostile";
        else if (tradePot > 5) status = "Alliance";
        else if (tradePot > 2) status = "Trade Partners";
        else status = "Neutral";
        rels.push({ a: a.id, b: b.id, icon: bar.icon, dist: Math.round(dist), status: status, tension: tension, tradePot: tradePot, sameRel: sameRel, sameLang: sameLang });
      }
    }
    return rels;
  }

  var CRISES = ["Famine", "Plague", "Revolt", "Succession Crisis", "Religious Schism", "Economic Collapse", "Civil War", "Drought", "Corruption Scandal", "Military Defeat"];
  var GROWTHS = ["Golden Age", "Population Boom", "Trade Expansion", "Military Victory", "Cultural Renaissance", "Reform Movement", "Technological Breakthrough", "Diplomatic Alliance"];

  function simulateTurn(cultures, relations, year, dt) {
    var REGIMES = window.REGIMES;
    var TRANSITIONS = window.TRANSITIONS;
    var LEVELS = window.LEVELS;
    var validRegimesForLevel = window.validRegimesForLevel;
    var pickRegimeForLevel = window.pickRegimeForLevel;
    var RELIGIONS = window.RELIGIONS;
    var pick = window.pick;
    var rand = window.rand;
    var clamp = window.clamp;
    var genRuler = window.genRuler;
    var genName = window.genName;

    var events = [];
    var updated = cultures.map(function (c) {
      var copy = {};
      for (var key in c) if (c.hasOwnProperty(key)) copy[key] = c[key];
      copy.drivers = { scale: c.drivers.scale, revBase: c.drivers.revBase, threat: c.drivers.threat, legitimacy: c.drivers.legitimacy };
      copy.sliders = { coercion: c.sliders.coercion, capacity: c.sliders.capacity, inclusiveness: c.sliders.inclusiveness };
      copy.history = c.history.slice();
      copy.vassals = c.vassals.slice();
      return copy;
    });

    updated.forEach(function (c) {
      var r = Math.random();
      var myRels = relations.filter(function (rel) { return rel.a === c.id || rel.b === c.id; });
      var wars = myRels.filter(function (rel) { return rel.status === "War"; });
      var trades = myRels.filter(function (rel) { return rel.status === "Trade Partners" || rel.status === "Alliance"; });

      c.tradeIncome = trades.reduce(function (s, t) { return s + t.tradePot * 500000 * c.techLevel; }, 0);
      c.gdp = c.population * (50 + c.techLevel * 30) + c.tradeIncome;

      var techChance = 0.06 + trades.length * 0.025 + (c.stability > 65 ? 0.04 : 0) + (c.vassals.length > 0 ? 0.02 : 0);
      if (r < techChance && c.techLevel < 15) {
        c.techLevel = clamp(c.techLevel + 1, 1, 15);
        c.sliders.capacity = clamp(c.sliders.capacity + 1, 1, 5);
        var validR = validRegimesForLevel(c.techLevel);
        if (validR.indexOf(c.regime) < 0) {
          var newR = pickRegimeForLevel(c.techLevel, c.terrain);
          var ev = c.name + " advances to " + (LEVELS[c.techLevel - 1] ? LEVELS[c.techLevel - 1].sublabel : "?") + "; government transitions to " + REGIMES[newR].name;
          events.push({ type: "tech", desc: ev });
          c.history.push({ year: year, event: "Tech " + c.techLevel + ": " + REGIMES[newR].name });
          c.regime = newR;
        } else {
          var ev2 = c.name + " reaches " + (LEVELS[c.techLevel - 1] ? LEVELS[c.techLevel - 1].sublabel : "?") + " (Level " + c.techLevel + ")";
          events.push({ type: "tech", desc: ev2 });
          c.history.push({ year: year, event: "Tech advance → Level " + c.techLevel });
        }
      }

      var validR = validRegimesForLevel(c.techLevel);
      if (validR.indexOf(c.regime) < 0 && Math.random() < 0.6) {
        var newR = pickRegimeForLevel(c.techLevel, c.terrain);
        var ev3 = c.name + ": " + REGIMES[c.regime].name + " → " + REGIMES[newR].name + " (tech pressure)";
        events.push({ type: "regime", desc: ev3 });
        c.history.push({ year: year, event: "Regime change: " + REGIMES[newR].name });
        c.regime = newR;
        c.ruler = genRuler();
      }

      if (c.stability < 35 && Math.random() < 0.35) {
        var options = (TRANSITIONS[c.regime] || ["failed"]).filter(function (id) { return validRegimesForLevel(c.techLevel).indexOf(id) >= 0; });
        if (options.length) {
          var newR = pick(options);
          var ev4 = c.name + ": " + REGIMES[c.regime].name + " → " + REGIMES[newR].name + " (crisis)";
          events.push({ type: "regime", desc: ev4 });
          c.history.push({ year: year, event: REGIMES[newR].name + " after crisis" });
          c.regime = newR;
          c.ruler = genRuler();
          c.dynasty = Math.random() < 0.5 ? genName() + " Dynasty" : c.dynasty;
          c.stability = rand(40, 60);
        }
      }

      wars.forEach(function (war) {
        var enemy = updated.find(function (x) { return x.id === (war.a === c.id ? war.b : war.a); });
        if (!enemy || enemy.vassalOf === c.id) return;
        if (Math.random() < 0.25) {
          var cPow = c.military * (c.sliders.capacity / 3) * (c.stability / 50) * (1 + c.techLevel / 15);
          var ePow = enemy.military * (enemy.sliders.capacity / 3) * (enemy.stability / 50) * (1 + enemy.techLevel / 15);
          if (cPow > ePow * 1.5 && Math.random() < 0.35) {
            enemy.vassalOf = c.id;
            c.vassals.push(enemy.id);
            var ev5 = c.name + " conquers " + enemy.name;
            events.push({ type: "conquest", desc: ev5 });
            c.history.push({ year: year, event: "Conquered " + enemy.name });
            enemy.history.push({ year: year, event: "Subjugated by " + c.name });
            c.culturalInfluence += 15;
            enemy.stability -= 20;
          } else {
            c.military = Math.max(1000, c.military - rand(5, 15) * 1000);
            enemy.military = Math.max(1000, enemy.military - rand(5, 15) * 1000);
            c.stability -= rand(3, 8);
            enemy.stability -= rand(3, 8);
            events.push({ type: "battle", desc: "Battle: " + c.name + " vs " + enemy.name });
          }
        }
      });

      if (c.culturalInfluence > 40) {
        myRels.filter(function (rel) { return rel.dist < 200; }).forEach(function (n) {
          var nb = updated.find(function (x) { return x.id === (n.a === c.id ? n.b : n.a); });
          if (!nb || Math.random() > 0.07 * (c.culturalInfluence / 100)) return;
          if (!n.sameRel && Math.random() < 0.5) {
            events.push({ type: "cultural", desc: nb.name + " adopts " + c.religion });
            nb.history.push({ year: year, event: "Adopted " + c.religion });
            nb.religion = c.religion;
          } else if (!n.sameLang) {
            events.push({ type: "cultural", desc: c.language + " spreads to " + nb.name });
            nb.language = c.language;
          }
        });
      }

      if (r < 0.12) {
        var crisis = pick(CRISES);
        events.push({ type: "crisis", desc: c.name + ": " + crisis });
        c.history.push({ year: year, event: crisis });
        c.stability -= rand(10, 25);
        if (crisis === "Succession Crisis") c.ruler = genRuler();
        if (crisis === "Religious Schism") c.religion = pick(RELIGIONS);
      } else if (r < 0.22) {
        var growth = pick(GROWTHS);
        events.push({ type: "growth", desc: c.name + ": " + growth });
        c.history.push({ year: year, event: growth });
        c.stability += rand(5, 12);
        c.culturalInfluence += rand(3, 8);
        if (growth === "Population Boom") c.population = Math.round(c.population * 1.12);
        if (growth === "Military Victory") c.military = Math.round(c.military * 1.15);
      }

      if (Math.random() < 0.05) {
        var oldR = c.ruler;
        c.ruler = genRuler();
        events.push({ type: "ruler", desc: c.name + ": " + oldR + " → " + c.ruler });
        c.history.push({ year: year, event: c.ruler + " ascends" });
        if (Math.random() < 0.3) c.stability -= rand(5, 12);
      }

      if (c.vassalOf !== null && c.stability > 72 && Math.random() < 0.15) {
        var ol = updated.find(function (x) { return x.id === c.vassalOf; });
        if (ol) {
          ol.vassals = ol.vassals.filter(function (v) { return v !== c.id; });
          events.push({ type: "independence", desc: c.name + " breaks free from " + ol.name });
          c.history.push({ year: year, event: "Independence from " + ol.name });
          c.vassalOf = null;
        }
      }

      c.stability = clamp(c.stability + rand(-2, 3), 10, 95);
      var growthRate = (c.stability > 50 ? 0.008 : -0.003) * (dt / 10);
      c.population = Math.max(1000, Math.round(c.population * (1 + growthRate)));
      c.military = Math.max(500, Math.round(c.military * (1 + (c.stability > 50 ? 0.003 : -0.008) * (dt / 10))));
    });

    return { cultures: updated, events: events.length ? events : [{ type: "quiet", desc: "A quiet era passes." }] };
  }

  window.generateCulture = generateCulture;
  window.buildRelations = buildRelations;
  window.simulateTurn = simulateTurn;
})();
