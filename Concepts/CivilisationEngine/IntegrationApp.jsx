/**
 * Civilisation Engine (Integration A–style): one shared data model for Tech Tree, Regime Chart, and History sim.
 * Regimes are constrained by tech level; sim uses validRegimesForLevel / pickRegimeForLevel.
 */
var LEVELS = window.LEVELS;
var TECHS = window.TECHS;
var REGIMES = window.REGIMES;
var TRANSITIONS = window.TRANSITIONS;
var REGIME_TRANSITIONS_LIST = window.REGIME_TRANSITIONS_LIST;
var R_BRACKETS = window.R_BRACKETS;
var CLIMATES = window.CLIMATES;
var TERRAINS = window.TERRAINS;
var buildTechLayout = window.buildTechLayout;
var buildRegimeLayout = window.buildRegimeLayout;
var TT_NW = window.TT_NW;
var TT_NH = window.TT_NH;
var TT_PAD = window.TT_PAD;
var TT_HDR = window.TT_HDR;
var RC = window.RC;
var RR = window.RR;
var RNW = window.RNW;
var RNH = window.RNH;
var RPAD = window.RPAD;
var generateCulture = window.generateCulture;
var buildRelations = window.buildRelations;
var simulateTurn = window.simulateTurn;
var techsForLevel = window.techsForLevel;
var pick = window.pick;
var rand = window.rand;

function TechTreeTab() {
  var layout = React.useMemo(buildTechLayout, []);
  var startX = layout.startX;
  var lvlCols = layout.lvlCols;
  var pos = layout.pos;
  var totalW = layout.totalW;
  var totalH = layout.totalH;
  var _s1 = React.useState(null);
  var hovered = _s1[0];
  var setHovered = _s1[1];
  var _s2 = React.useState(new Set());
  var unlocked = _s2[0];
  var setUnlocked = _s2[1];

  var canUnlock = function (t) { return !unlocked.has(t.id) && t.req.every(function (r) { return unlocked.has(r); }); };
  var toggle = function (t) {
    if (unlocked.has(t.id)) {
      var rem = new Set([t.id]);
      var ch = true;
      while (ch) {
        ch = false;
        TECHS.forEach(function (x) {
          if (!rem.has(x.id) && x.req.some(function (r) { return rem.has(r); })) { rem.add(x.id); ch = true; }
        });
      }
      setUnlocked(function (p) { var n = new Set(p); rem.forEach(function (id) { n.delete(id); }); return n; });
    } else if (canUnlock(t)) {
      setUnlocked(function (p) { return new Set([...p, t.id]); });
    }
  };

  var edges = React.useMemo(function () {
    var e = [];
    TECHS.forEach(function (t) {
      t.req.forEach(function (rid) {
        var fp = pos[rid];
        var tp = pos[t.id];
        if (!fp || !tp) return;
        e.push({
          from: rid, to: t.id,
          x1: fp.x + TT_NW, y1: fp.y + TT_NH / 2, x2: tp.x, y2: tp.y + TT_NH / 2,
          mx: (fp.x + TT_NW + tp.x) / 2,
          active: unlocked.has(rid) && unlocked.has(t.id),
          partial: unlocked.has(rid) && !unlocked.has(t.id),
        });
      });
    });
    return e;
  }, [pos, unlocked]);

  var hov = hovered ? TECHS.find(function (t) { return t.id === hovered; }) : null;

  return (
    <div style={{ background: "#0f0f1a", height: "100%", overflow: "hidden", fontFamily: "sans-serif", position: "relative" }}>
      <div style={{ padding: "6px 10px", color: "#9ca3af", fontSize: 11, borderBottom: "1px solid #1f2937", display: "flex", gap: 12, alignItems: "center" }}>
        <span>Unlocked: {unlocked.size}/{TECHS.length}</span>
        {unlocked.size > 0 && <button type="button" onClick={function () { setUnlocked(new Set()); }} style={{ background: "#374151", color: "#f87171", border: "none", borderRadius: 4, padding: "2px 8px", cursor: "pointer", fontSize: 11 }}>Reset</button>}
        <span style={{ color: "#4b5563" }}>Click blue nodes to unlock · click green to re-lock</span>
      </div>
      <div style={{ overflowX: "auto", overflowY: "auto", height: "calc(100% - 32px)" }}>
        <svg width={totalW} height={totalH}>
          {LEVELS.map(function (lvl) {
            var nc = [...lvlCols[lvl.id]].length;
            var x = startX[lvl.id];
            var w = nc * window.TT_COL - 6;
            return (
              <g key={lvl.id}>
                <rect x={x} y={TT_PAD} width={w} height={totalH - TT_PAD * 2} fill={lvl.color} fillOpacity={0.07} rx={3} />
                <rect x={x} y={TT_PAD} width={w} height={TT_HDR - 4} fill={lvl.color} fillOpacity={0.55} rx={3} />
                <text x={x + w / 2} y={TT_PAD + 13} textAnchor="middle" fill="#f3f4f6" fontSize={9} fontWeight="bold">{lvl.label}</text>
                <text x={x + w / 2} y={TT_PAD + 25} textAnchor="middle" fill="#d1d5db" fontSize={8}>{lvl.sublabel}</text>
                <line x1={x + w + 3} y1={TT_PAD} x2={x + w + 3} y2={totalH - TT_PAD} stroke={lvl.color} strokeOpacity={0.35} strokeWidth={1.5} strokeDasharray="4,3" />
                {nc > 1 && [...Array(nc - 1)].map(function (_, ci) {
                  return <line key={ci} x1={x + (ci + 1) * window.TT_COL} y1={TT_PAD + TT_HDR} x2={x + (ci + 1) * window.TT_COL} y2={totalH - TT_PAD} stroke={lvl.color} strokeOpacity={0.2} strokeWidth={1} strokeDasharray="2,4" />;
                })}
              </g>
            );
          })}
          {edges.map(function (e, i) {
            var hl = hovered === e.from || hovered === e.to;
            var stroke = e.active ? "#22c55e" : e.partial ? "#fbbf24" : "#374151";
            return <path key={i} d={"M" + e.x1 + "," + e.y1 + " C" + e.mx + "," + e.y1 + " " + e.mx + "," + e.y2 + " " + e.x2 + "," + e.y2} fill="none" stroke={hl ? "#f59e0b" : stroke} strokeWidth={hl ? 2 : 1.5} opacity={hl ? 1 : hovered ? 0.2 : 0.75} strokeDasharray={e.active || e.partial ? "" : "4,3"} />;
          })}
          {TECHS.map(function (t) {
            var p = pos[t.id];
            if (!p) return null;
            var isU = unlocked.has(t.id);
            var canU = canUnlock(t);
            var isH = hovered === t.id;
            return (
              <g key={t.id} onClick={function () { toggle(t); }} onMouseEnter={function () { setHovered(t.id); }} onMouseLeave={function () { setHovered(null); }} style={{ cursor: isU || canU ? "pointer" : "default" }}>
                <rect x={p.x + 1} y={p.y + 2} width={TT_NW} height={TT_NH} rx={3} fill={isU ? "#14532d" : canU ? "#1e3a5f" : "#1f2937"} stroke={isU ? "#22c55e" : canU ? "#3b82f6" : isH ? "#6b7280" : "#374151"} strokeWidth={isH ? 2 : 1.5} opacity={!isU && !canU && hovered && !isH ? 0.3 : 1} />
                <text x={p.x + 7} y={p.y + 13} fill={isU ? "#86efac" : canU ? "#93c5fd" : "#9ca3af"} fontSize={8.5} fontWeight="bold">{t.name}</text>
                {isU && <text x={p.x + TT_NW - 4} y={p.y + 13} textAnchor="end" fill="#22c55e" fontSize={9}>✓</text>}
              </g>
            );
          })}
        </svg>
      </div>
      {hov && (
        <div style={{ position: "fixed", bottom: 12, right: 12, background: "#1f2937", border: "1px solid #374151", borderRadius: 8, padding: "10px 14px", maxWidth: 260, zIndex: 200, boxShadow: "0 4px 20px rgba(0,0,0,0.7)" }}>
          <div style={{ color: "#fbbf24", fontWeight: "bold", fontSize: 12, marginBottom: 2 }}>{hov.name}</div>
          <div style={{ color: "#9ca3af", fontSize: 10, marginBottom: 4 }}>Level {hov.level} — {LEVELS[hov.level - 1] ? LEVELS[hov.level - 1].sublabel : ""}</div>
          {hov.req.length > 0 && <div style={{ fontSize: 10, color: "#6b7280" }}>Requires: {hov.req.map(function (r) { var tt = TECHS.find(function (x) { return x.id === r; }); return <span key={r} style={{ color: unlocked.has(r) ? "#22c55e" : "#f87171", marginRight: 4 }}>{tt ? tt.name : r}</span>; })}</div>}
          <div style={{ fontSize: 10, color: "#6b7280", marginTop: 4 }}>{unlocked.has(hov.id) ? "✅ Unlocked" : canUnlock(hov) ? "🔵 Available" : "🔒 Locked"}</div>
        </div>
      )}
    </div>
  );
}

function RegimeChartTab() {
  var layout = React.useMemo(buildRegimeLayout, []);
  var pos = layout.pos;
  var totalW = layout.totalW;
  var totalH = layout.totalH;
  var _s1 = React.useState(null);
  var hov = _s1[0];
  var setHov = _s1[1];
  var _s2 = React.useState(null);
  var sel = _s2[0];
  var setSel = _s2[1];
  var active = hov || sel;

  var nodeX = function (col) { return RPAD + col * (RC + 14); };
  var nodeY = function (row) { return RPAD + 52 + row * (RR + 8); };

  var arrows = REGIME_TRANSITIONS_LIST.map(function (t, i) {
    var fp = pos[t.from];
    var tp = pos[t.to];
    if (!fp || !tp) return null;
    var x1 = nodeX(fp.col) + RNW;
    var y1 = nodeY(fp.row) + RNH / 2;
    var x2 = nodeX(tp.col);
    var y2 = nodeY(tp.row) + RNH / 2;
    var sc = fp.col === tp.col;
    var isHL = active && (t.from === active || t.to === active);
    return { from: t.from, to: t.to, label: t.label, x1: x1, y1: y1, x2: x2, y2: y2, mx: sc ? x1 + 55 : (x1 + x2) / 2, sc: sc, isHL: isHL, isFrom: active && t.from === active, idx: i };
  }).filter(Boolean);

  var rdata = function (id) { return REGIMES[id] || { name: id, icon: "?", coercion: [1, 1], capacity: [1, 1], inclusiveness: [1, 1], minLvl: 1, maxLvl: 15 }; };

  return (
    <div style={{ background: "#0f0f1a", height: "100%", overflow: "hidden", fontFamily: "sans-serif", position: "relative" }}>
      <div style={{ padding: "6px 10px", color: "#9ca3af", fontSize: 11, borderBottom: "1px solid #1f2937" }}>Hover or click a regime to see transitions. Yellow = outgoing, blue = incoming.</div>
      <div style={{ overflowX: "auto", overflowY: "auto", height: "calc(100% - 32px)" }}>
        <svg width={totalW} height={totalH}>
          <defs>
            <marker id="arrN" markerWidth="6" markerHeight="6" refX="5" refY="3" orient="auto"><path d="M0,0 L6,3 L0,6 Z" fill="#4b5563" /></marker>
            <marker id="arrH" markerWidth="6" markerHeight="6" refX="5" refY="3" orient="auto"><path d="M0,0 L6,3 L0,6 Z" fill="#fbbf24" /></marker>
            <marker id="arrHI" markerWidth="6" markerHeight="6" refX="5" refY="3" orient="auto"><path d="M0,0 L6,3 L0,6 Z" fill="#60a5fa" /></marker>
          </defs>
          {R_BRACKETS.map(function (br) {
            return (
              <g key={br.col}>
                <rect x={nodeX(br.col) - 4} y={RPAD} width={RC + 8} height={44} fill={br.color} fillOpacity={0.6} rx={4} />
                <text x={nodeX(br.col) + RC / 2} y={RPAD + 15} textAnchor="middle" fill="#f3f4f6" fontSize={9} fontWeight="bold">{br.era}</text>
                <text x={nodeX(br.col) + RC / 2} y={RPAD + 28} textAnchor="middle" fill="#d1d5db" fontSize={8}>{br.lvl}</text>
                <rect x={nodeX(br.col) - 4} y={RPAD + 44} width={RC + 8} height={totalH - RPAD - 44 - RPAD} fill={br.color} fillOpacity={0.05} rx={4} />
              </g>
            );
          })}
          {arrows.map(function (a) {
            var fade = active && !a.isHL;
            var mId = a.isHL ? (a.isFrom ? "arrH" : "arrHI") : "arrN";
            var stroke = a.isHL ? (a.isFrom ? "#fbbf24" : "#60a5fa") : "#4b5563";
            var d = a.sc ? "M" + a.x1 + "," + a.y1 + " C" + a.mx + "," + a.y1 + " " + a.mx + "," + a.y2 + " " + a.x1 + "," + a.y2 : "M" + a.x1 + "," + a.y1 + " C" + a.mx + "," + a.y1 + " " + a.mx + "," + a.y2 + " " + a.x2 + "," + a.y2;
            return (
              <g key={a.idx} opacity={fade ? 0.1 : 0.85}>
                <path d={d} fill="none" stroke={stroke} strokeWidth={a.isHL ? 2 : 1} strokeDasharray={a.isHL ? "" : "3,3"} markerEnd={"url(#" + mId + ")"} />
                {a.isHL && <text x={(a.x1 + a.x2) / 2} y={Math.min(a.y1, a.y2) - 3} textAnchor="middle" fill="#fbbf24" fontSize={7}>{a.label}</text>}
              </g>
            );
          })}
          {(function () {
            var regimeIds = [];
            R_BRACKETS.forEach(function (br) { br.ids.forEach(function (id) { regimeIds.push(id); }); });
            return regimeIds.map(function (id) {
              var r = rdata(id);
              var p = pos[id];
              if (!p) return null;
              var x = nodeX(p.col);
              var y = nodeY(p.row);
              var isH = hov === id;
              var isSel = sel === id;
              var isDim = active && active !== id && !arrows.some(function (a) { return a.isHL && (a.from === id || a.to === id); });
              return (
                <g key={id} onClick={function () { setSel(function (s) { return s === id ? null : id; }); }} onMouseEnter={function () { setHov(id); }} onMouseLeave={function () { setHov(null); }} style={{ cursor: "pointer" }} opacity={isDim ? 0.25 : 1}>
                  <rect x={x} y={y} width={RNW} height={RNH} rx={5} fill={isSel ? "#1e3a5f" : isH ? "#1f2d40" : "#1f2937"} stroke={isSel ? "#3b82f6" : isH ? "#6b7280" : "#374151"} strokeWidth={isH || isSel ? 2 : 1} />
                  <text x={x + 8} y={y + 16} fill="#f3f4f6" fontSize={11} fontWeight="bold">{r.icon} {r.name}</text>
                  <text x={x + 8} y={y + 28} fill="#6b7280" fontSize={8}>Lvl {r.minLvl}–{r.maxLvl}</text>
                  <text x={x + 8} y={y + 40} fill="#9ca3af" fontSize={8} style={{ fontFamily: "monospace" }}>{"C:" + "█".repeat(r.coercion[0]) + "░".repeat(5 - r.coercion[0]) + " Cap:" + "█".repeat(r.capacity[0]) + "░".repeat(5 - r.capacity[0]) + " I:" + "█".repeat(r.inclusiveness[0]) + "░".repeat(5 - r.inclusiveness[0])}</text>
                </g>
              );
            });
          })()}
        </svg>
      </div>
      {(hov || sel) && (function () {
        var r = rdata(hov || sel);
        var out = REGIME_TRANSITIONS_LIST.filter(function (t) { return t.from === (hov || sel); });
        var inc = REGIME_TRANSITIONS_LIST.filter(function (t) { return t.to === (hov || sel); });
        return (
          <div style={{ position: "fixed", bottom: 12, right: 12, background: "#1f2937", border: "1px solid #374151", borderRadius: 8, padding: "10px 14px", maxWidth: 320, zIndex: 200, boxShadow: "0 4px 24px rgba(0,0,0,0.8)" }}>
            <div style={{ color: "#fbbf24", fontWeight: "bold", fontSize: 13, marginBottom: 3 }}>{r.icon} {r.name}</div>
            <div style={{ color: "#9ca3af", fontSize: 10, marginBottom: 5 }}>Level {r.minLvl}–{r.maxLvl} | C:{r.coercion[0]}–{r.coercion[1]} Cap:{r.capacity[0]}–{r.capacity[1]} I:{r.inclusiveness[0]}–{r.inclusiveness[1]}</div>
            {out.length > 0 && <div style={{ marginBottom: 4 }}><span style={{ color: "#fbbf24", fontSize: 10, fontWeight: "bold" }}>→ To: </span>{out.map(function (t) { var rr = REGIMES[t.to]; return <span key={t.to} style={{ color: "#93c5fd", fontSize: 10, marginRight: 6 }}>{rr ? rr.icon : ""}{rr ? rr.name : t.to}</span>; })} </div>}
            {inc.length > 0 && <div><span style={{ color: "#86efac", fontSize: 10, fontWeight: "bold" }}>← From: </span>{inc.map(function (t) { var rr = REGIMES[t.from]; return <span key={t.from} style={{ color: "#86efac", fontSize: 10, marginRight: 6 }}>{rr ? rr.icon : ""}{rr ? rr.name : t.from}</span>; })} </div>}
          </div>
        );
      })()}
    </div>
  );
}

function HistoryTab() {
  var _cfg = React.useState({ scale: "regional", climates: ["Temperate"], terrains: ["Plains", "Riverine"], techLevel: 3 });
  var config = _cfg[0];
  var setConfig = _cfg[1];
  var _cultures = React.useState([]);
  var cultures = _cultures[0];
  var setCultures = _cultures[1];
  var _relations = React.useState([]);
  var relations = _relations[0];
  var setRelations = _relations[1];
  var _history = React.useState([]);
  var history = _history[0];
  var setHistory = _history[1];
  var _year = React.useState(0);
  var year = _year[0];
  var setYear = _year[1];
  var _selected = React.useState(null);
  var selected = _selected[0];
  var setSelected = _selected[1];
  var _dt = React.useState(25);
  var dt = _dt[0];
  var setDt = _dt[1];

  var generate = React.useCallback(function () {
    var count = { local: rand(2, 4), regional: rand(5, 9), continental: rand(10, 16), planetary: rand(16, 24) }[config.scale];
    var cs = [];
    for (var i = 0; i < count; i++) cs.push(generateCulture(i, pick(config.climates), pick(config.terrains), config.techLevel));
    setCultures(cs);
    setRelations(buildRelations(cs));
    setHistory([{ year: 0, dt: 0, events: [{ type: "start", desc: "History begins..." }] }]);
    setYear(0);
    setSelected(null);
  }, [config]);

  var advance = function () {
    var ny = year + dt;
    var result = simulateTurn(cultures, relations, ny, dt);
    setCultures(result.cultures);
    setRelations(buildRelations(result.cultures));
    setHistory(function (h) { return h.concat([{ year: ny, dt: dt, events: result.events }]); });
    setYear(ny);
  };

  var sel = cultures.find(function (c) { return c.id === selected; });
  var era = LEVELS[Math.min(14, Math.floor(year / 200))] ? LEVELS[Math.min(14, Math.floor(year / 200))].sublabel : "Ancient";

  var eventColor = function (type) {
    if (type === "regime") return "#c084fc";
    if (type === "conquest") return "#f87171";
    if (type === "crisis") return "#fb923c";
    if (type === "cultural") return "#60a5fa";
    if (type === "tech") return "#22d3ee";
    if (type === "growth") return "#86efac";
    if (type === "ruler") return "#fde047";
    if (type === "battle") return "#fca5a5";
    if (type === "independence") return "#6ee7b7";
    return "#9ca3af";
  };

  var leftPanel = { width: 240, flexShrink: 0, display: "flex", flexDirection: "column", gap: 8, overflowY: "auto", padding: 8, background: "#111827", fontSize: 12 };
  var box = { background: "#1f2937", borderRadius: 8, padding: 8 };
  var btnAmber = { width: "100%", background: "#b45309", color: "#fff", border: "none", borderRadius: 4, padding: "6px 8px", cursor: "pointer", fontWeight: 600 };
  var btnGreen = { width: "100%", background: "#15803d", color: "#fff", border: "none", borderRadius: 4, padding: "6px 8px", cursor: "pointer", fontWeight: 600 };

  return (
    <div style={{ display: "flex", gap: 8, height: "100%", background: "#111827", color: "#f3f4f6", fontFamily: "sans-serif", overflow: "hidden" }}>
      <div style={leftPanel}>
        <div style={box}>
          <div style={{ color: "#f59e0b", fontWeight: "bold", marginBottom: 8 }}>⚙️ Configuration</div>
          <label style={{ display: "block", marginBottom: 4 }}>
            Scale
            <select value={config.scale} onChange={function (e) { setConfig(function (c) { return { scale: e.target.value, climates: c.climates, terrains: c.terrains, techLevel: c.techLevel }; }); }} style={{ width: "100%", marginTop: 4, background: "#374151", color: "#f3f4f6", border: "none", borderRadius: 4, padding: "4px 6px", fontSize: 12 }}>
              <option value="local">local</option>
              <option value="regional">regional</option>
              <option value="continental">continental</option>
              <option value="planetary">planetary</option>
            </select>
          </label>
          <label style={{ display: "block", marginBottom: 4 }}>
            Starting Tech Level: {config.techLevel} ({LEVELS[config.techLevel - 1] ? LEVELS[config.techLevel - 1].sublabel : ""})
            <input type="range" min="1" max="12" value={config.techLevel} onChange={function (e) { setConfig(function (c) { return { scale: c.scale, climates: c.climates, terrains: c.terrains, techLevel: +e.target.value }; }); }} style={{ width: "100%" }} />
          </label>
          <div style={{ marginBottom: 4 }}>Climates</div>
          <div style={{ display: "flex", flexWrap: "wrap", gap: 4, marginTop: 4 }}>
            {CLIMATES.map(function (cl) {
              var on = config.climates.indexOf(cl) >= 0;
              return <button key={cl} type="button" onClick={function () { setConfig(function (c) { var list = c.climates.indexOf(cl) >= 0 ? c.climates.filter(function (x) { return x !== cl; }) : c.climates.concat([cl]); return { scale: c.scale, climates: list, terrains: c.terrains, techLevel: c.techLevel }; }); }} style={{ padding: "2px 6px", borderRadius: 4, fontSize: 11, background: on ? "#b45309" : "#374151", border: "none", color: "#f3f4f6", cursor: "pointer" }}>{cl}</button>;
            })}
          </div>
          <div style={{ marginBottom: 8 }}>Terrains</div>
          <div style={{ display: "flex", flexWrap: "wrap", gap: 4, marginTop: 4 }}>
            {TERRAINS.map(function (t) {
              var on = config.terrains.indexOf(t) >= 0;
              return <button key={t} type="button" onClick={function () { setConfig(function (c) { var list = c.terrains.indexOf(t) >= 0 ? c.terrains.filter(function (x) { return x !== t; }) : c.terrains.concat([t]); return { scale: c.scale, climates: c.climates, terrains: list, techLevel: c.techLevel }; }); }} style={{ padding: "2px 6px", borderRadius: 4, fontSize: 11, background: on ? "#b45309" : "#374151", border: "none", color: "#f3f4f6", cursor: "pointer" }}>{t}</button>;
            })}
          </div>
          <button type="button" onClick={generate} style={btnAmber}>🌍 Generate World</button>
        </div>

        {cultures.length > 0 && (
          <>
            <div style={box}>
              <div style={{ color: "#f59e0b", fontWeight: "bold", marginBottom: 8 }}>⏳ Year {year} · {era}</div>
              <label style={{ display: "block", marginBottom: 8 }}>
                Advance: {dt} years
                <input type="range" min="5" max="200" step="5" value={dt} onChange={function (e) { setDt(+e.target.value); }} style={{ width: "100%" }} />
              </label>
              <button type="button" onClick={advance} style={btnGreen}>⏩ Advance {dt} Years</button>
            </div>
            <div style={{ ...box, flex: 1, overflow: "hidden", display: "flex", flexDirection: "column" }}>
              <div style={{ color: "#f59e0b", fontWeight: "bold", marginBottom: 8 }}>📜 Timeline</div>
              <div style={{ flex: 1, overflowY: "auto" }}>
                {history.slice().reverse().map(function (h, i) {
                  return (
                    <div key={i} style={{ background: "#374151", borderRadius: 4, padding: 6, marginBottom: 4 }}>
                      <div style={{ color: "#f59e0b", fontWeight: 600 }}>Year {h.year}{h.dt > 0 ? " (+" + h.dt + "y)" : ""}</div>
                      {h.events.slice(0, 6).map(function (e, j) {
                        return <div key={j} style={{ color: eventColor(e.type) }}>{e.desc}</div>;
                      })}
                      {h.events.length > 6 && <div style={{ color: "#6b7280" }}>+{h.events.length - 6} more</div>}
                    </div>
                  );
                })}
              </div>
            </div>
          </>
        )}
      </div>

      <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: 8, overflow: "hidden", padding: 8 }}>
        <div style={box}>
          <div style={{ color: "#f59e0b", fontWeight: "bold", marginBottom: 8 }}>🗺️ World Map</div>
          <svg viewBox="0 0 600 400" style={{ width: "100%", maxHeight: 320, background: "#030712", borderRadius: 4, border: "1px solid #374151" }}>
            {relations.map(function (r, i) {
              var a = cultures.find(function (c) { return c.id === r.a; });
              var b = cultures.find(function (c) { return c.id === r.b; });
              if (!a || !b) return null;
              var color = r.status === "War" ? "#ef4444" : r.status === "Alliance" ? "#22c55e" : r.status === "Trade Partners" ? "#3b82f6" : r.status === "Vassal" ? "#a855f7" : "#374151";
              var mx = (a.x + b.x) / 2;
              var my = (a.y + b.y) / 2;
              return <g key={i}><line x1={a.x} y1={a.y} x2={b.x} y2={b.y} stroke={color} strokeWidth={r.status === "War" ? 2 : 1} strokeDasharray={r.status === "Hostile" ? "4" : ""} /><text x={mx} y={my} textAnchor="middle" fontSize="10">{r.icon}</text></g>;
            })}
            {cultures.map(function (c) {
              var r = REGIMES[c.regime] || REGIMES.failed;
              var fill = c.stability > 65 ? "#166534" : c.stability > 40 ? "#854d0e" : "#7f1d1d";
              var rad = 10 + c.sliders.capacity * 2 + c.vassals.length * 2;
              return (
                <g key={c.id} onClick={function () { setSelected(c.id); }} style={{ cursor: "pointer" }}>
                  <circle cx={c.x} cy={c.y} r={rad} fill={fill} stroke={selected === c.id ? "#fbbf24" : c.vassalOf !== null ? "#a855f7" : "#6b7280"} strokeWidth={selected === c.id ? 3 : c.vassalOf !== null ? 2 : 1} />
                  <text x={c.x} y={c.y + 4} textAnchor="middle" fontSize="12">{r.icon}</text>
                  <text x={c.x} y={c.y + rad + 10} textAnchor="middle" fontSize="8" fill="#d1d5db">{c.name}</text>
                  <text x={c.x + rad - 2} y={c.y - rad + 6} textAnchor="middle" fontSize="8" fill="#60a5fa">L{c.techLevel}</text>
                </g>
              );
            })}
          </svg>
          <div style={{ display: "flex", flexWrap: "wrap", gap: 12, marginTop: 8, justifyContent: "center", fontSize: 11, color: "#9ca3af" }}>
            <span><span style={{ display: "inline-block", width: 8, height: 8, borderRadius: "50%", background: "#166534", marginRight: 4 }} />Stable</span>
            <span><span style={{ display: "inline-block", width: 8, height: 8, borderRadius: "50%", background: "#854d0e", marginRight: 4 }} />Unstable</span>
            <span><span style={{ display: "inline-block", width: 8, height: 8, borderRadius: "50%", background: "#7f1d1d", marginRight: 4 }} />Crisis</span>
            <span style={{ color: "#ef4444" }}>— War</span>
            <span style={{ color: "#22c55e" }}>— Alliance</span>
            <span style={{ color: "#a855f7" }}>— Vassal</span>
          </div>
        </div>

        {sel && (function () {
          var r = REGIMES[sel.regime] || REGIMES.failed;
          var techsUnlocked = techsForLevel(sel.techLevel);
          var unlockCount = techsUnlocked.size;
          return (
            <div style={{ ...box, overflowY: "auto", flex: 1 }}>
              <div style={{ color: "#f59e0b", fontWeight: "bold", marginBottom: 8 }}>{r.icon} {sel.name} — {r.name}</div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 12, fontSize: 12 }}>
                <div>
                  <div style={{ color: "#f59e0b", fontWeight: 600, marginBottom: 4 }}>🗺 Geography</div>
                  <div>{sel.climate} · {sel.terrain}</div>
                  <div style={{ color: "#9ca3af" }}>{sel.resources.join(", ")}</div>
                  <div style={{ color: "#f59e0b", fontWeight: 600, marginTop: 8, marginBottom: 4 }}>👥 Society</div>
                  <div>Pop: {sel.population >= 1e9 ? (sel.population / 1e9).toFixed(1) + "B" : (sel.population / 1e6).toFixed(1) + "M"}</div>
                  <div>Religion: {sel.religion}</div>
                  <div>Language: {sel.language}</div>
                  <div>Influence: {sel.culturalInfluence}</div>
                </div>
                <div>
                  <div style={{ color: "#f59e0b", fontWeight: 600, marginBottom: 4 }}>👑 Political</div>
                  <div>{r.name}</div>
                  <div style={{ color: "#9ca3af" }}>{sel.ruler} · {sel.dynasty}</div>
                  <div>Stability: <span style={{ color: sel.stability > 65 ? "#86efac" : sel.stability > 40 ? "#fde047" : "#f87171" }}>{Math.round(sel.stability)}%</span></div>
                  <div>Legitimacy: {sel.drivers.legitimacy}</div>
                  <div style={{ fontFamily: "monospace", marginTop: 4, color: "#d1d5db" }}>
                    <div>C: {"█".repeat(sel.sliders.coercion)}{"░".repeat(5 - sel.sliders.coercion)}</div>
                    <div>A: {"█".repeat(sel.sliders.capacity)}{"░".repeat(5 - sel.sliders.capacity)}</div>
                    <div>I: {"█".repeat(sel.sliders.inclusiveness)}{"░".repeat(5 - sel.sliders.inclusiveness)}</div>
                  </div>
                  {sel.vassalOf !== null && <div style={{ color: "#c084fc", marginTop: 4 }}>Vassal of {cultures.find(function (c) { return c.id === sel.vassalOf; }) ? cultures.find(function (c) { return c.id === sel.vassalOf; }).name : ""}</div>}
                  {sel.vassals.length > 0 && <div style={{ color: "#d8b4fe" }}>Vassals: {sel.vassals.map(function (v) { return cultures.find(function (c) { return c.id === v; }) ? cultures.find(function (c) { return c.id === v; }).name : v; }).join(", ")}</div>}
                </div>
                <div>
                  <div style={{ color: "#f59e0b", fontWeight: 600, marginBottom: 4 }}>🔬 Technology</div>
                  <div style={{ color: "#22d3ee", fontWeight: "bold" }}>Level {sel.techLevel}: {LEVELS[sel.techLevel - 1] ? LEVELS[sel.techLevel - 1].sublabel : ""}</div>
                  <div style={{ color: "#9ca3af" }}>{unlockCount} techs available</div>
                  <div style={{ color: "#f59e0b", fontWeight: 600, marginTop: 8, marginBottom: 4 }}>💰 Economy</div>
                  <div>{sel.economy.join(", ")}</div>
                  <div>GDP: {(sel.gdp / 1e9).toFixed(1)}B</div>
                  <div>Trade: {(sel.tradeIncome / 1e6).toFixed(0)}M</div>
                  <div style={{ color: "#f59e0b", fontWeight: 600, marginTop: 8, marginBottom: 4 }}>⚔️ Military</div>
                  <div>{(sel.military / 1000).toFixed(0)}K troops</div>
                  <div style={{ color: "#f59e0b", fontWeight: 600, marginTop: 8, marginBottom: 4 }}>🤝 Relations</div>
                  {relations.filter(function (rel) { return rel.a === sel.id || rel.b === sel.id; }).map(function (rel, i) {
                    var other = cultures.find(function (c) { return c.id === (rel.a === sel.id ? rel.b : rel.a); });
                    var col = rel.status === "War" ? "#f87171" : rel.status === "Hostile" ? "#fb923c" : rel.status === "Alliance" ? "#86efac" : rel.status === "Trade Partners" ? "#60a5fa" : rel.status === "Vassal" ? "#c084fc" : "#9ca3af";
                    return <div key={i} style={{ color: col }}>{other ? other.name : ""}: {rel.status} {rel.icon}</div>;
                  })}
                </div>
              </div>
              <div style={{ marginTop: 12, paddingTop: 8, borderTop: "1px solid #374151" }}>
                <div style={{ color: "#f59e0b", fontWeight: 600, marginBottom: 4 }}>📖 Chronicle of {sel.name}</div>
                <div style={{ maxHeight: 112, overflowY: "auto" }}>
                  {sel.history.slice().reverse().map(function (h, i) {
                    return <div key={i} style={{ color: "#d1d5db" }}><span style={{ color: "#6b7280" }}>Y{h.year}:</span> {h.event}</div>;
                  })}
                </div>
              </div>
            </div>
          );
        })()}
      </div>
    </div>
  );
}

function App() {
  var _tab = React.useState("history");
  var tab = _tab[0];
  var setTab = _tab[1];
  var tabs = [["history", "🌍 History"], ["tech", "🔬 Tech Tree"], ["regime", "👑 Regimes"]];

  return (
    <div style={{ background: "#0f0f1a", height: "100vh", display: "flex", flexDirection: "column", fontFamily: "sans-serif" }}>
      <div style={{ display: "flex", alignItems: "center", borderBottom: "2px solid #1f2937", padding: "0 12px", flexShrink: 0 }}>
        <span style={{ color: "#fbbf24", fontWeight: "bold", fontSize: 15, marginRight: 16, padding: "8px 0" }}>⚔️ Civilisation Engine</span>
        {tabs.map(function (item) {
          var id = item[0];
          var label = item[1];
          var isActive = tab === id;
          return (
            <button key={id} type="button" onClick={function () { setTab(id); }} style={{ padding: "8px 16px", border: "none", borderBottom: isActive ? "3px solid #fbbf24" : "3px solid transparent", background: "transparent", color: isActive ? "#fbbf24" : "#9ca3af", fontWeight: isActive ? "bold" : "normal", cursor: "pointer", fontSize: 13 }}>
              {label}
            </button>
          );
        })}
      </div>
      <div style={{ flex: 1, overflow: "hidden" }}>
        {tab === "history" && <HistoryTab />}
        {tab === "tech" && <TechTreeTab />}
        {tab === "regime" && <RegimeChartTab />}
      </div>
    </div>
  );
}

if (typeof document !== "undefined" && document.getElementById("root")) {
  var root = ReactDOM.createRoot(document.getElementById("root"));
  root.render(React.createElement(App));
}
