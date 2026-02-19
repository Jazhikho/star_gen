/** Tech tree tab: interactive unlock/lock of tech nodes with prerequisite graph. */
function TechTreeTab() {
  const LEVELS = window.LEVELS;
  const TECHS = window.TECHS;
  const buildLayout = window.buildLayout;
  const NODE_W = window.NODE_W;
  const NODE_H = window.NODE_H;
  const COL_W = window.COL_W;
  const PAD_Y = window.PAD_Y;
  const HDR_H = window.HDR_H;

  const [hovered, setHovered] = React.useState(null);
  const [unlocked, setUnlocked] = React.useState(new Set());

  const layout = React.useMemo(function () { return buildLayout(TECHS, LEVELS); }, []);
  const levelStartX = layout.levelStartX;
  const levelCols = layout.levelCols;
  const totalW = layout.totalW;
  const totalH = layout.totalH;
  const nodePos = layout.nodePos;

  const canUnlock = function (tech) {
    return !unlocked.has(tech.id) && tech.req.every(function (r) { return unlocked.has(r); });
  };

  const toggleUnlock = function (tech) {
    if (unlocked.has(tech.id)) {
      const toRemove = new Set([tech.id]);
      let changed = true;
      while (changed) {
        changed = false;
        TECHS.forEach(function (t) {
          if (!toRemove.has(t.id) && t.req.some(function (r) { return toRemove.has(r); })) {
            toRemove.add(t.id);
            changed = true;
          }
        });
      }
      setUnlocked(function (prev) {
        const n = new Set(prev);
        toRemove.forEach(function (id) { n.delete(id); });
        return n;
      });
    } else if (canUnlock(tech)) {
      setUnlocked(function (prev) { return new Set([...prev, tech.id]); });
    }
  };

  const connections = React.useMemo(function () {
    const c = [];
    TECHS.forEach(function (tech) {
      tech.req.forEach(function (reqId) {
        const from = TECHS.find(function (t) { return t.id === reqId; });
        if (!from) return;
        const fp = nodePos[from.id];
        const tp = nodePos[tech.id];
        if (!fp || !tp) return;
        const x1 = fp.x + NODE_W;
        const y1 = fp.y + NODE_H / 2;
        const x2 = tp.x;
        const y2 = tp.y + NODE_H / 2;
        const mx = (x1 + x2) / 2;
        const isActive = unlocked.has(reqId) && unlocked.has(tech.id);
        const isPartial = unlocked.has(reqId) && !unlocked.has(tech.id);
        c.push({ from: reqId, to: tech.id, x1: x1, y1: y1, x2: x2, y2: y2, mx: mx, isActive: isActive, isPartial: isPartial });
      });
    });
    return c;
  }, [nodePos, unlocked]);

  const hov = hovered ? TECHS.find(function (t) { return t.id === hovered; }) : null;

  return (
    <div style={{ background: '#0f0f1a', minHeight: '100%', padding: '10px', fontFamily: 'sans-serif', position: 'relative' }}>
      <div style={{ color: '#9ca3af', fontSize: 11, marginBottom: 6 }}>
        Unlocked: {unlocked.size}/{TECHS.length}
        {unlocked.size > 0 && (
          <button onClick={function () { setUnlocked(new Set()); }}
            style={{ marginLeft: 10, background: '#374151', color: '#f87171', border: 'none', borderRadius: 4, padding: '2px 8px', cursor: 'pointer', fontSize: 11 }}>
            Reset
          </button>
        )}
        <span style={{ marginLeft: 16, color: '#6b7280' }}>Click available (blue) nodes to unlock · Click unlocked to re-lock</span>
      </div>
      <div style={{ overflowX: 'auto', overflowY: 'auto', maxHeight: 'calc(100vh - 110px)' }}>
        <svg width={totalW} height={totalH} style={{ display: 'block' }}>
          {LEVELS.map(function (lvl) {
            const cols = [...levelCols[lvl.id]].length;
            const x = levelStartX[lvl.id];
            const w = cols * COL_W - 8;
            return (
              <g key={lvl.id}>
                <rect x={x} y={PAD_Y} width={w} height={totalH - PAD_Y * 2} fill={lvl.color} fillOpacity={0.07} rx={4}/>
                <rect x={x} y={PAD_Y} width={w} height={HDR_H - 4} fill={lvl.color} fillOpacity={0.55} rx={4}/>
                <text x={x + w / 2} y={PAD_Y + 14} textAnchor="middle" fill="#f3f4f6" fontSize={10} fontWeight="bold">{lvl.label}</text>
                <text x={x + w / 2} y={PAD_Y + 27} textAnchor="middle" fill="#d1d5db" fontSize={9}>{lvl.sublabel}</text>
                <line x1={x + w + 4} y1={PAD_Y} x2={x + w + 4} y2={totalH - PAD_Y} stroke={lvl.color} strokeOpacity={0.35} strokeWidth={1.5} strokeDasharray="4,3"/>
                {cols > 1 && [...Array(cols - 1)].map(function (_, ci) {
                  return (
                    <line key={ci} x1={x + (ci + 1) * COL_W} y1={PAD_Y + HDR_H} x2={x + (ci + 1) * COL_W} y2={totalH - PAD_Y}
                      stroke={lvl.color} strokeOpacity={0.2} strokeWidth={1} strokeDasharray="2,4"/>
                  );
                })}
              </g>
            );
          })}
          {connections.map(function (c, i) {
            const hl = hovered === c.from || hovered === c.to;
            const stroke = c.isActive ? '#22c55e' : c.isPartial ? '#fbbf24' : '#374151';
            return (
              <path key={i}
                d={'M' + c.x1 + ',' + c.y1 + ' C' + c.mx + ',' + c.y1 + ' ' + c.mx + ',' + c.y2 + ' ' + c.x2 + ',' + c.y2}
                fill="none" stroke={hl ? '#f59e0b' : stroke}
                strokeWidth={hl ? 2 : 1.5} opacity={hl ? 1 : hovered ? 0.2 : 0.75}
                strokeDasharray={c.isActive || c.isPartial ? '' : '4,3'}/>
            );
          })}
          {TECHS.map(function (tech) {
            const p = nodePos[tech.id];
            if (!p) return null;
            const isU = unlocked.has(tech.id);
            const canU = canUnlock(tech);
            const isH = hovered === tech.id;
            const bg = isU ? '#14532d' : canU ? '#1e3a5f' : '#1f2937';
            const border = isU ? '#22c55e' : canU ? '#3b82f6' : isH ? '#6b7280' : '#374151';
            const tc = isU ? '#86efac' : canU ? '#93c5fd' : '#9ca3af';
            const reqText = tech.req.length === 0 ? 'No prerequisites' : tech.req.map(function (r) { var t = TECHS.find(function (x) { return x.id === r; }); return t ? t.name : r; }).join(', ');
            return (
              <g key={tech.id} onClick={function () { toggleUnlock(tech); }}
                onMouseEnter={function () { setHovered(tech.id); }} onMouseLeave={function () { setHovered(null); }}
                style={{ cursor: (isU || canU) ? 'pointer' : 'default' }}>
                <rect x={p.x + 1} y={p.y + 2} width={NODE_W} height={NODE_H} rx={4}
                  fill={bg} stroke={border} strokeWidth={isH ? 2 : 1.5}
                  opacity={(!isU && !canU && hovered && !isH) ? 0.35 : 1}/>
                <text x={p.x + 8} y={p.y + 14} fill={tc} fontSize={9} fontWeight="bold">{tech.name}</text>
                <text x={p.x + 8} y={p.y + 26} fill="#5b7280" fontSize={7.5}>{reqText}</text>
                {isU && <text x={p.x + NODE_W - 5} y={p.y + 14} textAnchor="end" fill="#22c55e" fontSize={10}>✓</text>}
              </g>
            );
          })}
        </svg>
      </div>
      {hov && (
        <div style={{ position: 'fixed', bottom: 16, right: 16, background: '#1f2937', border: '1px solid #374151',
          borderRadius: 8, padding: '10px 14px', maxWidth: 300, zIndex: 100, boxShadow: '0 4px 20px rgba(0,0,0,0.7)' }}>
          <div style={{ color: '#fbbf24', fontWeight: 'bold', fontSize: 13, marginBottom: 3 }}>{hov.name}</div>
          <div style={{ color: '#9ca3af', fontSize: 10, marginBottom: 5 }}>Level {hov.level} — {LEVELS.find(function (l) { return l.id === hov.level; }) ? LEVELS.find(function (l) { return l.id === hov.level; }).sublabel : ''}</div>
          <div style={{ color: '#d1d5db', fontSize: 11, marginBottom: 6 }}>{hov.desc}</div>
          {hov.req.length > 0 && (
            <div style={{ fontSize: 10, color: '#6b7280' }}>Requires: {hov.req.map(function (r) {
              const t = TECHS.find(function (x) { return x.id === r; });
              return <span key={r} style={{ color: unlocked.has(r) ? '#22c55e' : '#f87171', marginRight: 4 }}>{t ? t.name : r}</span>;
            })}</div>
          )}
          <div style={{ fontSize: 10, color: '#6b7280', marginTop: 4 }}>
            {unlocked.has(hov.id) ? '✅ Unlocked — click to re-lock' : canUnlock(hov) ? '🔵 Available — click to unlock' : '🔒 Prerequisites not met'}
          </div>
        </div>
      )}
      <div style={{ position: 'fixed', top: 48, right: 12, background: '#1f2937', border: '1px solid #374151',
        borderRadius: 6, padding: '8px 12px', fontSize: 10, color: '#9ca3af', zIndex: 50 }}>
        <div style={{ color: '#fbbf24', fontWeight: 'bold', marginBottom: 4 }}>Legend</div>
        {[['#14532d', '#22c55e', 'Unlocked'], ['#1e3a5f', '#3b82f6', 'Available'], ['#1f2937', '#374151', 'Locked']].map(function (item) {
          const bg = item[0], br = item[1], lb = item[2];
          return (
            <div key={lb} style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 3 }}>
              <div style={{ width: 12, height: 12, background: bg, border: '1px solid ' + br, borderRadius: 2 }}/>{lb}
            </div>
          );
        })}
        <div style={{ borderTop: '1px solid #374151', paddingTop: 4, marginTop: 2 }}>
          <div style={{ color: '#22c55e' }}>—— Unlocked path</div>
          <div style={{ color: '#fbbf24' }}>—— Partial path</div>
          <div style={{ color: '#374151' }}>- - Locked</div>
          <div style={{ color: 'rgba(100,120,180,0.5)', marginTop: 2 }}>| Sub-col divider</div>
        </div>
      </div>
    </div>
  );
}
window.TechTreeTab = TechTreeTab;
