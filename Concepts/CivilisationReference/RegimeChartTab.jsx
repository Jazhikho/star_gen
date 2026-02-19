/** Regime chart tab: regime nodes and transition arrows with hover/select detail. */
function RegimeChartTab() {
  const REGIMES = window.REGIMES;
  const REGIME_TRANSITIONS = window.REGIME_TRANSITIONS;
  const buildRegimeLayout = window.buildRegimeLayout;
  const R_COL_W = window.R_COL_W;
  const R_ROW_H = window.R_ROW_H;
  const R_NODE_W = window.R_NODE_W;
  const R_NODE_H = window.R_NODE_H;
  const R_PAD = window.R_PAD;

  const [hovered, setHovered] = React.useState(null);
  const [selected, setSelected] = React.useState(null);
  const layout = React.useMemo(buildRegimeLayout, []);
  const pos = layout.pos;
  const totalW = layout.totalW;
  const totalH = layout.totalH;

  const nodeX = function (col) { return R_PAD + col * (R_COL_W + 16); };
  const nodeY = function (row) { return R_PAD + 56 + row * (R_ROW_H + 10); };

  const hov = hovered ? REGIMES.find(function (r) { return r.id === hovered; }) : null;
  const sel = selected ? REGIMES.find(function (r) { return r.id === selected; }) : null;
  const activeId = hovered || selected;

  const arrows = REGIME_TRANSITIONS.map(function (t, i) {
    const fp = pos[t.from];
    const tp = pos[t.to];
    if (!fp || !tp) return null;
    const x1 = nodeX(fp.col) + R_NODE_W;
    const y1 = nodeY(fp.row) + R_NODE_H / 2;
    const x2 = nodeX(tp.col);
    const y2 = nodeY(tp.row) + R_NODE_H / 2;
    const sameCol = fp.col === tp.col;
    const mx = sameCol ? x1 + 60 : (x1 + x2) / 2;
    const isHighlighted = activeId && (t.from === activeId || t.to === activeId);
    const isFrom = activeId && t.from === activeId;
    return { from: t.from, to: t.to, label: t.label, x1: x1, y1: y1, x2: x2, y2: y2, mx: mx, sameCol: sameCol, isHighlighted: isHighlighted, isFrom: isFrom, idx: i };
  }).filter(Boolean);

  const eraBands = [
    { col: 0, label: 'Pre-State', color: '#78350f' },
    { col: 1, label: 'Early State', color: '#b45309' },
    { col: 2, label: 'Ancient', color: '#b08d57' },
    { col: 3, label: 'Classical', color: '#1d4ed8' },
    { col: 4, label: 'Medieval–Early Modern', color: '#5b21b6' },
    { col: 5, label: 'Modern', color: '#065f46' },
    { col: 6, label: 'Contemporary', color: '#0e7490' },
    { col: 7, label: 'Advanced / Space', color: '#1e1b4b' },
  ];
  const eraLvl = ['Lvl 1–3', 'Lvl 2–5', 'Lvl 4–8', 'Lvl 5–9', 'Lvl 6–11', 'Lvl 8–12', 'Lvl 11–13', 'Lvl 13–15'];

  return (
    <div style={{ background: '#0f0f1a', minHeight: '100%', padding: '10px', fontFamily: 'sans-serif', position: 'relative' }}>
      <div style={{ color: '#9ca3af', fontSize: 11, marginBottom: 6 }}>
        Hover or click a regime to highlight transitions. Arrows show typical paths of regime change.
      </div>
      <div style={{ overflowX: 'auto', overflowY: 'auto', maxHeight: 'calc(100vh - 110px)' }}>
        <svg width={totalW} height={totalH} style={{ display: 'block' }}>
          {eraBands.map(function (b) {
            return (
              <g key={b.col}>
                <rect x={nodeX(b.col) - 4} y={R_PAD} width={R_COL_W + 8} height={44} fill={b.color} fillOpacity={0.55} rx={4}/>
                <text x={nodeX(b.col) + R_COL_W / 2} y={R_PAD + 16} textAnchor="middle" fill="#f3f4f6" fontSize={9} fontWeight="bold">{b.label}</text>
                <text x={nodeX(b.col) + R_COL_W / 2} y={R_PAD + 30} textAnchor="middle" fill="#d1d5db" fontSize={8}>{eraLvl[b.col]}</text>
                <rect x={nodeX(b.col) - 4} y={R_PAD + 44} width={R_COL_W + 8} height={totalH - R_PAD - 44 - R_PAD} fill={b.color} fillOpacity={0.05} rx={4}/>
              </g>
            );
          })}
          <defs>
            <marker id="arrN" markerWidth="6" markerHeight="6" refX="5" refY="3" orient="auto">
              <path d="M0,0 L6,3 L0,6 Z" fill="#4b5563"/>
            </marker>
            <marker id="arrH" markerWidth="6" markerHeight="6" refX="5" refY="3" orient="auto">
              <path d="M0,0 L6,3 L0,6 Z" fill="#fbbf24"/>
            </marker>
          </defs>
          {arrows.map(function (a) {
            const fade = activeId && !a.isHighlighted;
            const color = a.isHighlighted ? (a.isFrom ? '#fbbf24' : '#60a5fa') : '#4b5563';
            const sw = a.isHighlighted ? 2 : 1;
            const d = a.sameCol
              ? 'M' + a.x1 + ',' + a.y1 + ' C' + a.mx + ',' + a.y1 + ' ' + a.mx + ',' + a.y2 + ' ' + a.x1 + ',' + a.y2
              : 'M' + a.x1 + ',' + a.y1 + ' C' + a.mx + ',' + a.y1 + ' ' + a.mx + ',' + a.y2 + ' ' + a.x2 + ',' + a.y2;
            const markerEnd = 'url(#arr' + (a.isHighlighted ? 'H' : 'N') + ')';
            return (
              <g key={a.idx} opacity={fade ? 0.12 : 0.85}>
                <path d={d} fill="none" stroke={color} strokeWidth={sw} strokeDasharray={a.isHighlighted ? '' : '3,3'} markerEnd={markerEnd}/>
                {a.isHighlighted && (
                  <text x={(a.x1 + a.x2) / 2} y={Math.min(a.y1, a.y2) - 4} textAnchor="middle" fill="#fbbf24" fontSize={8}>{a.label}</text>
                )}
              </g>
            );
          })}
          {REGIMES.map(function (reg) {
            const p = pos[reg.id];
            if (!p) return null;
            const x = nodeX(p.col);
            const y = nodeY(p.row);
            const isH = hovered === reg.id;
            const isSel = selected === reg.id;
            const isDim = activeId && activeId !== reg.id && !arrows.some(function (a) { return a.isHighlighted && (a.from === reg.id || a.to === reg.id); });
            const bg = isSel ? '#1e3a5f' : isH ? '#1f2d40' : '#1f2937';
            const border = isSel ? '#3b82f6' : isH ? '#6b7280' : '#374151';
            const cBar = '█'.repeat(reg.coercion[0]) + '░'.repeat(5 - reg.coercion[0]);
            const capBar = '█'.repeat(reg.capacity[0]) + '░'.repeat(5 - reg.capacity[0]);
            const iBar = '█'.repeat(reg.inclusiveness[0]) + '░'.repeat(5 - reg.inclusiveness[0]);
            const descShort = reg.desc.length > 72 ? reg.desc.slice(0, 72) + '…' : reg.desc;
            return (
              <g key={reg.id}
                onClick={function () { setSelected(function (s) { return s === reg.id ? null : reg.id; }); }}
                onMouseEnter={function () { setHovered(reg.id); }}
                onMouseLeave={function () { setHovered(null); }}
                style={{ cursor: 'pointer' }}
                opacity={isDim ? 0.3 : 1}>
                <rect x={x} y={y} width={R_NODE_W} height={R_NODE_H} rx={5} fill={bg} stroke={border} strokeWidth={isH || isSel ? 2 : 1}/>
                <text x={x + 8} y={y + 16} fill="#f3f4f6" fontSize={11} fontWeight="bold">{reg.icon} {reg.name}</text>
                <text x={x + 8} y={y + 28} fill="#6b7280" fontSize={8}>Lvl {reg.minLvl}–{reg.maxLvl}</text>
                <text x={x + 8} y={y + 40} fill="#9ca3af" fontSize={7.5} style={{ whiteSpace: 'pre-wrap' }}>{'C:' + cBar + ' Cap:' + capBar + ' I:' + iBar}</text>
                <text x={x + 8} y={y + 52} fill="#6b7280" fontSize={7} style={{ fontStyle: 'italic' }}>{descShort}</text>
              </g>
            );
          })}
        </svg>
      </div>
      {(hov || sel) && (function () {
        const r = hov || sel;
        const outgoing = REGIME_TRANSITIONS.filter(function (t) { return t.from === r.id; });
        const incoming = REGIME_TRANSITIONS.filter(function (t) { return t.to === r.id; });
        return (
          <div style={{ position: 'fixed', bottom: 16, right: 16, background: '#1f2937', border: '1px solid #374151',
            borderRadius: 8, padding: '12px 16px', maxWidth: 340, zIndex: 100, boxShadow: '0 4px 24px rgba(0,0,0,0.8)' }}>
            <div style={{ color: '#fbbf24', fontWeight: 'bold', fontSize: 14, marginBottom: 3 }}>{r.icon} {r.name}</div>
            <div style={{ color: '#9ca3af', fontSize: 10, marginBottom: 6 }}>
              Tech Level {r.minLvl}–{r.maxLvl} &nbsp;|&nbsp; Coercion {r.coercion[0]}–{r.coercion[1]} &nbsp;|&nbsp; Capacity {r.capacity[0]}–{r.capacity[1]} &nbsp;|&nbsp; Inclusiveness {r.inclusiveness[0]}–{r.inclusiveness[1]}
            </div>
            <div style={{ color: '#d1d5db', fontSize: 11, marginBottom: 8 }}>{r.desc}</div>
            {outgoing.length > 0 && (
              <div style={{ marginBottom: 6 }}>
                <span style={{ color: '#fbbf24', fontSize: 10, fontWeight: 'bold' }}>→ Can transition to: </span>
                {outgoing.map(function (t) {
                  const tr = REGIMES.find(function (x) { return x.id === t.to; });
                  return <span key={t.to} style={{ color: '#93c5fd', fontSize: 10, marginRight: 8 }}>{tr ? tr.icon : ''}{tr ? tr.name : t.to} <span style={{ color: '#6b7280' }}>({t.label})</span></span>;
                })}
              </div>
            )}
            {incoming.length > 0 && (
              <div>
                <span style={{ color: '#86efac', fontSize: 10, fontWeight: 'bold' }}>← Reached from: </span>
                {incoming.map(function (t) {
                  const fr = REGIMES.find(function (x) { return x.id === t.from; });
                  return <span key={t.from} style={{ color: '#86efac', fontSize: 10, marginRight: 8 }}>{fr ? fr.icon : ''}{fr ? fr.name : t.from}</span>;
                })}
              </div>
            )}
          </div>
        );
      })()}
    </div>
  );
}
window.RegimeChartTab = RegimeChartTab;
