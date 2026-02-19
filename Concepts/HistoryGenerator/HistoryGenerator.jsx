/** History Generator: config, world map, culture details, timeline. Uses inline styles; no Tailwind/lucide. */
function HistoryGenerator() {
  var REGIMES = window.REGIMES_HG;
  var CLIMATES = window.CLIMATES;
  var TERRAINS = window.TERRAINS;
  var ERAS = window.ERAS;
  var pick = window.pick;
  var generateCulture = window.generateCulture;
  var generateBarriers = window.generateBarriers;
  var generateRelations = window.generateRelations;
  var simulateTurn = window.simulateTurn;

  var _useState = React.useState({ scale: "regional", climates: ["Temperate"], terrains: ["Plains", "Riverine"], techLevel: 2, age: 2 });
  var config = _useState[0];
  var setConfig = _useState[1];
  var _useState2 = React.useState([]);
  var cultures = _useState2[0];
  var setCultures = _useState2[1];
  var _useState3 = React.useState([]);
  var barriers = _useState3[0];
  var setBarriers = _useState3[1];
  var _useState4 = React.useState([]);
  var relations = _useState4[0];
  var setRelations = _useState4[1];
  var _useState5 = React.useState([]);
  var history = _useState5[0];
  var setHistory = _useState5[1];
  var _useState6 = React.useState(0);
  var year = _useState6[0];
  var setYear = _useState6[1];
  var _useState7 = React.useState(null);
  var selected = _useState7[0];
  var setSelected = _useState7[1];
  var _useState8 = React.useState(25);
  var timeStep = _useState8[0];
  var setTimeStep = _useState8[1];
  var _useState9 = React.useState({ config: true, map: true, details: true, history: false });
  var expanded = _useState9[0];
  var setExpanded = _useState9[1];

  var scaleCounts = { local: [2, 4], regional: [5, 9], continental: [10, 16], planetary: [16, 24] };
  var rand = window.rand;

  var generate = function () {
    var range = scaleCounts[config.scale];
    var count = range ? rand(range[0], range[1]) : rand(5, 9);
    var newCultures = [];
    for (var i = 0; i < count; i++) {
      newCultures.push(generateCulture(i, pick(config.climates), pick(config.terrains), config.techLevel, config.age));
    }
    var newBarriers = generateBarriers(newCultures);
    var newRelations = generateRelations(newCultures, newBarriers);
    setCultures(newCultures);
    setBarriers(newBarriers);
    setRelations(newRelations);
    setHistory([{ year: 0, events: [{ type: "start", desc: "History begins..." }] }]);
    setYear(0);
    setSelected(null);
  };

  var advance = function () {
    var newYear = year + timeStep;
    var result = simulateTurn(cultures, relations, newYear, timeStep);
    var newBarriers = generateBarriers(result.cultures);
    var newRelations = generateRelations(result.cultures, newBarriers);
    setCultures(result.cultures);
    setBarriers(newBarriers);
    setRelations(newRelations);
    setHistory(function (h) {
      var next = h.slice();
      next.push({ year: newYear, yearsElapsed: timeStep, events: result.events });
      return next;
    });
    setYear(newYear);
  };

  function Section(props) {
    var title = props.title;
    var id = props.id;
    var children = props.children;
    var isOpen = expanded[id];
    return (
      <div style={{ background: "#1f2937", borderRadius: 8, overflow: "hidden", marginBottom: 8 }}>
        <button
          type="button"
          onClick={function () { setExpanded(function (e) { var o = {}; for (var k in e) o[k] = e[k]; o[id] = !e[id]; return o; }); }}
          style={{ width: "100%", padding: 8, display: "flex", justifyContent: "space-between", alignItems: "center", textAlign: "left", fontWeight: 600, color: "#f3f4f6", fontSize: 14, border: "none", background: "transparent", cursor: "pointer" }}
        >
          {title} {isOpen ? "▲" : "▼"}
        </button>
        {isOpen && <div style={{ padding: "0 8px 8px", borderTop: "1px solid #374151" }}>{children}</div>}
      </div>
    );
  }

  var sel = cultures.find(function (c) { return c.id === selected; });
  var era = ERAS[Math.min(5, Math.floor(year / 200))];

  var base = { minHeight: "100vh", background: "#111827", color: "#f3f4f6", padding: 8, display: "flex", gap: 8, fontSize: 14, fontFamily: "sans-serif" };
  var sidebar = { width: 256, flexShrink: 0 };

  return (
    <div style={base}>
      <div style={sidebar}>
        <h1 style={{ fontSize: 18, fontWeight: "bold", marginBottom: 8, color: "#f59e0b" }}>⚔️ History Generator</h1>

        <Section title="⚙️ Configuration" id="config">
          <label style={{ display: "block", marginBottom: 4 }}>
            Scale
            <select
              value={config.scale}
              onChange={function (e) { setConfig(function (c) { return { scale: e.target.value, climates: c.climates, terrains: c.terrains, techLevel: c.techLevel, age: c.age }; }); }}
              style={{ width: "100%", marginTop: 4, background: "#374151", borderRadius: 4, padding: "6px 8px", fontSize: 12 }}
            >
              <option value="local">local</option>
              <option value="regional">regional</option>
              <option value="continental">continental</option>
              <option value="planetary">planetary</option>
            </select>
          </label>
          <label style={{ display: "block", marginBottom: 4 }}>
            Starting Tech: {config.techLevel}
            <input type="range" min="1" max="5" value={config.techLevel} onChange={function (e) { setConfig(function (c) { return { scale: c.scale, climates: c.climates, terrains: c.terrains, techLevel: +e.target.value, age: c.age }; }); }} style={{ width: "100%" }} />
          </label>
          <label style={{ display: "block", marginBottom: 4 }}>
            Starting Age: {config.age}
            <input type="range" min="1" max="5" value={config.age} onChange={function (e) { setConfig(function (c) { return { scale: c.scale, climates: c.climates, terrains: c.terrains, techLevel: c.techLevel, age: +e.target.value }; }); }} style={{ width: "100%" }} />
          </label>
          <div style={{ marginBottom: 4 }}>Climates</div>
          <div style={{ display: "flex", flexWrap: "wrap", gap: 4, marginTop: 4 }}>
            {CLIMATES.map(function (cl) {
              var active = config.climates.indexOf(cl) >= 0;
              return (
                <button
                  key={cl}
                  type="button"
                  onClick={function () {
                    setConfig(function (c) {
                      var list = c.climates.indexOf(cl) >= 0 ? c.climates.filter(function (x) { return x !== cl; }) : c.climates.concat([cl]);
                      return { scale: c.scale, climates: list, terrains: c.terrains, techLevel: c.techLevel, age: c.age };
                    });
                  }}
                  style={{ padding: "2px 6px", borderRadius: 4, fontSize: 12, background: active ? "#b45309" : "#374151", border: "none", color: "#f3f4f6", cursor: "pointer" }}
                >
                  {cl}
                </button>
              );
            })}
          </div>
          <div style={{ marginBottom: 8 }}>Terrains</div>
          <div style={{ display: "flex", flexWrap: "wrap", gap: 4, marginTop: 4 }}>
            {TERRAINS.map(function (t) {
              var active = config.terrains.indexOf(t) >= 0;
              return (
                <button
                  key={t}
                  type="button"
                  onClick={function () {
                    setConfig(function (c) {
                      var list = c.terrains.indexOf(t) >= 0 ? c.terrains.filter(function (x) { return x !== t; }) : c.terrains.concat([t]);
                      return { scale: c.scale, climates: c.climates, terrains: list, techLevel: c.techLevel, age: c.age };
                    });
                  }}
                  style={{ padding: "2px 6px", borderRadius: 4, fontSize: 12, background: active ? "#b45309" : "#374151", border: "none", color: "#f3f4f6", cursor: "pointer" }}
                >
                  {t}
                </button>
              );
            })}
          </div>
          <button type="button" onClick={generate} style={{ width: "100%", background: "#b45309", color: "#fff", border: "none", borderRadius: 4, padding: 6, display: "flex", alignItems: "center", justifyContent: "center", gap: 4, fontSize: 12, cursor: "pointer" }}>
            🔄 Generate World
          </button>
        </Section>

        {cultures.length > 0 && (
          <>
            <div style={{ background: "#1f2937", borderRadius: 8, padding: 8, marginBottom: 8 }}>
              <div style={{ display: "flex", alignItems: "center", gap: 8, color: "#f59e0b", fontWeight: 600, marginBottom: 8 }}>
                🕐 Year {year} • {era} Era
              </div>
              <label style={{ display: "block", marginBottom: 8, fontSize: 12 }}>
                Years per advance: {timeStep}
                <input type="range" min="5" max="100" step="5" value={timeStep} onChange={function (e) { setTimeStep(+e.target.value); }} style={{ width: "100%" }} />
              </label>
              <button type="button" onClick={advance} style={{ width: "100%", background: "#15803d", color: "#fff", border: "none", borderRadius: 4, padding: 6, fontSize: 12, cursor: "pointer" }}>
                Advance {timeStep} Years →
              </button>
            </div>

            <Section title="📜 Timeline" id="timeline">
              <div style={{ maxHeight: 160, overflowY: "auto", fontSize: 12 }}>
                {history.slice().reverse().map(function (h, i) {
                  return (
                    <div key={i} style={{ background: "#374151", borderRadius: 4, padding: 6, marginBottom: 4 }}>
                      <div style={{ fontWeight: 600, color: "#f59e0b" }}>Year {h.year} {h.yearsElapsed ? "(+" + h.yearsElapsed + "y)" : ""}</div>
                      {h.events.slice(0, 5).map(function (e, j) {
                        var color = e.type === "regime" ? "#c084fc" : e.type === "conquest" ? "#f87171" : e.type === "crisis" ? "#fb923c" : e.type === "cultural" ? "#60a5fa" : e.type === "tech" ? "#22d3ee" : "#d1d5db";
                        return <div key={j} style={{ color: color }}>{e.desc}</div>;
                      })}
                      {h.events.length > 5 && <div style={{ color: "#6b7280" }}>+{h.events.length - 5} more...</div>}
                    </div>
                  );
                })}
              </div>
            </Section>
          </>
        )}
      </div>

      <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: 8 }}>
        <Section title="🗺️ World Map" id="map">
          <svg viewBox="0 0 600 400" style={{ width: "100%", background: "#030712", borderRadius: 4, border: "1px solid #374151" }}>
            {relations.map(function (r, i) {
              var a = cultures.find(function (c) { return c.id === r.a; });
              var b = cultures.find(function (c) { return c.id === r.b; });
              if (!a || !b) return null;
              var color = r.status === "War" ? "#ef4444" : r.status === "Alliance" ? "#22c55e" : r.status === "Trade Partners" ? "#3b82f6" : r.status === "Vassal" ? "#a855f7" : "#4b5563";
              var mx = (a.x + b.x) / 2;
              var my = (a.y + b.y) / 2;
              return (
                <g key={i}>
                  <line x1={a.x} y1={a.y} x2={b.x} y2={b.y} stroke={color} strokeWidth={r.status === "War" ? 2 : 1} strokeDasharray={r.status === "Hostile" ? "4" : ""} />
                  <text x={mx} y={my} textAnchor="middle" fontSize="10">{r.icon}</text>
                </g>
              );
            })}
            {cultures.map(function (c) {
              var isSel = selected === c.id;
              var fill = c.stability > 60 ? "#166534" : c.stability > 40 ? "#ca8a04" : "#991b1b";
              var stroke = isSel ? "#fbbf24" : c.vassalOf !== null ? "#a855f7" : "#6b7280";
              var r = 10 + c.sliders.capacity * 2 + c.vassals.length * 2;
              return (
                <g key={c.id} onClick={function () { setSelected(c.id); }} style={{ cursor: "pointer" }}>
                  <circle cx={c.x} cy={c.y} r={r} fill={fill} stroke={stroke} strokeWidth={isSel ? 3 : c.vassalOf !== null ? 2 : 1} />
                  <text x={c.x} y={c.y + 4} textAnchor="middle" fontSize="11">{REGIMES[c.regime].icon}</text>
                  <text x={c.x} y={c.y + 24} textAnchor="middle" fontSize="8" fill="#d1d5db">{c.name}</text>
                </g>
              );
            })}
          </svg>
          <div style={{ display: "flex", flexWrap: "wrap", gap: 12, fontSize: 12, marginTop: 4, justifyContent: "center" }}>
            <span style={{ display: "flex", alignItems: "center", gap: 4 }}><span style={{ width: 8, height: 8, borderRadius: "50%", background: "#166534" }} /> Stable</span>
            <span style={{ display: "flex", alignItems: "center", gap: 4 }}><span style={{ width: 8, height: 8, borderRadius: "50%", background: "#ca8a04" }} /> Unstable</span>
            <span style={{ display: "flex", alignItems: "center", gap: 4 }}><span style={{ width: 8, height: 8, borderRadius: "50%", background: "#991b1b" }} /> Crisis</span>
            <span style={{ display: "flex", alignItems: "center", gap: 4 }}><span style={{ width: 24, height: 2, background: "#ef4444" }} /> War</span>
            <span style={{ display: "flex", alignItems: "center", gap: 4 }}><span style={{ width: 24, height: 2, background: "#22c55e" }} /> Alliance</span>
            <span style={{ display: "flex", alignItems: "center", gap: 4 }}><span style={{ width: 24, height: 2, background: "#a855f7" }} /> Vassal</span>
          </div>
        </Section>

        {sel && (
          <Section title={REGIMES[sel.regime].icon + " " + sel.name} id="details">
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 12, fontSize: 12 }}>
              <div>
                <h4 style={{ fontWeight: 600, color: "#f59e0b", marginBottom: 4 }}>📍 Geography</h4>
                <p>{sel.climate} • {sel.terrain}</p>
                <p style={{ color: "#9ca3af" }}>{sel.resources.join(", ")}</p>
                <h4 style={{ fontWeight: 600, color: "#f59e0b", marginTop: 8, marginBottom: 4 }}>👥 Society</h4>
                <p>Pop: {(sel.population / 1000000).toFixed(1)}M</p>
                <p>Religion: {sel.religion}</p>
                <p>Language: {sel.language}</p>
                <p>Influence: {sel.culturalInfluence}</p>
              </div>
              <div>
                <h4 style={{ fontWeight: 600, color: "#f59e0b", marginBottom: 4 }}>👑 Political</h4>
                <p>{REGIMES[sel.regime].name}</p>
                <p>Ruler: {sel.ruler}</p>
                <p style={{ color: "#9ca3af" }}>{sel.dynasty}</p>
                <p>Stability: <span style={{ color: sel.stability > 60 ? "#86efac" : sel.stability > 40 ? "#fde047" : "#f87171" }}>{Math.round(sel.stability)}%</span></p>
                <p>Tech Level: {sel.techLevel}/5</p>
                {sel.vassalOf !== null && <p style={{ color: "#c084fc" }}>Vassal of {cultures.find(function (c) { return c.id === sel.vassalOf; }) ? cultures.find(function (c) { return c.id === sel.vassalOf; }).name : ""}</p>}
                {sel.vassals.length > 0 && <p style={{ color: "#d8b4fe" }}>Vassals: {sel.vassals.map(function (v) { return cultures.find(function (c) { return c.id === v; }) ? cultures.find(function (c) { return c.id === v; }).name : v; }).join(", ")}</p>}
                <div style={{ marginTop: 4, fontSize: 11, fontFamily: "monospace" }}>
                  <div>Coercion: {"█".repeat(sel.sliders.coercion)}{"░".repeat(5 - sel.sliders.coercion)}</div>
                  <div>Capacity: {"█".repeat(sel.sliders.capacity)}{"░".repeat(5 - sel.sliders.capacity)}</div>
                  <div>Inclusive: {"█".repeat(sel.sliders.inclusiveness)}{"░".repeat(5 - sel.sliders.inclusiveness)}</div>
                </div>
              </div>
              <div>
                <h4 style={{ fontWeight: 600, color: "#f59e0b", marginBottom: 4 }}>🪙 Economy</h4>
                <p>{sel.economy.join(", ")}</p>
                <p>GDP: {(sel.gdp / 1000000000).toFixed(1)}B</p>
                <p>Trade: {(sel.tradeIncome / 1000000).toFixed(0)}M</p>
                <h4 style={{ fontWeight: 600, color: "#f59e0b", marginTop: 8, marginBottom: 4 }}>⚔️ Military</h4>
                <p>{(sel.military / 1000).toFixed(0)}K troops</p>
                <h4 style={{ fontWeight: 600, color: "#f59e0b", marginTop: 8, marginBottom: 4 }}>🤝 Relations</h4>
                <div>
                  {relations.filter(function (r) { return r.a === sel.id || r.b === sel.id; }).slice(0, 4).map(function (r, i) {
                    var other = cultures.find(function (c) { return c.id === (r.a === sel.id ? r.b : r.a); });
                    var colors = { War: "#f87171", Hostile: "#fb923c", Alliance: "#86efac", "Trade Partners": "#60a5fa", Vassal: "#c084fc", Neutral: "#9ca3af" };
                    return <div key={i} style={{ color: colors[r.status] }}>{other ? other.name : ""}: {r.status} {r.icon}</div>;
                  })}
                </div>
              </div>
            </div>
            <div style={{ marginTop: 8, paddingTop: 8, borderTop: "1px solid #374151" }}>
              <h4 style={{ fontWeight: 600, color: "#f59e0b", marginBottom: 4 }}>📖 History of {sel.name}</h4>
              <div style={{ maxHeight: 96, overflowY: "auto", fontSize: 12 }}>
                {sel.history.slice().reverse().map(function (h, i) {
                  return (
                    <div key={i} style={{ color: "#d1d5db" }}><span style={{ color: "#6b7280" }}>Y{h.year}:</span> {h.event}</div>
                  );
                })}
              </div>
            </div>
          </Section>
        )}
      </div>
    </div>
  );
}
window.HistoryGenerator = HistoryGenerator;

if (!window.INTEGRATION && typeof document !== "undefined" && document.getElementById("root")) {
  var root = ReactDOM.createRoot(document.getElementById("root"));
  root.render(React.createElement(HistoryGenerator));
}
