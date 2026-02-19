/** Root app: tab bar and Tech Tree / Regime Chart tabs. */
function App() {
  const [tab, setTab] = React.useState('tech');
  const TechTreeTab = window.TechTreeTab;
  const RegimeChartTab = window.RegimeChartTab;
  const tabs = [
    ['tech', '🔬 Tech Tree'],
    ['regime', '👑 Regime Chart'],
  ];
  return (
    <div style={{ background: '#0f0f1a', minHeight: '100vh', fontFamily: 'sans-serif' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 0, borderBottom: '2px solid #1f2937', padding: '0 12px' }}>
        <span style={{ color: '#fbbf24', fontWeight: 'bold', fontSize: 15, marginRight: 16, padding: '10px 0' }}>🌍 Civilisation Reference</span>
        {tabs.map(function (item) {
          const id = item[0];
          const label = item[1];
          const isActive = tab === id;
          return (
            <button key={id} onClick={function () { setTab(id); }} style={{
              padding: '10px 18px',
              border: 'none',
              borderBottom: isActive ? '3px solid #fbbf24' : '3px solid transparent',
              background: 'transparent',
              color: isActive ? '#fbbf24' : '#9ca3af',
              fontWeight: isActive ? 'bold' : 'normal',
              cursor: 'pointer',
              fontSize: 13
            }}>{label}</button>
          );
        })}
      </div>
      <div style={{ height: 'calc(100vh - 46px)', overflow: 'hidden' }}>
        {tab === 'tech' && <TechTreeTab/>}
        {tab === 'regime' && <RegimeChartTab/>}
      </div>
    </div>
  );
}
window.App = App;

if (typeof document !== 'undefined' && document.getElementById('root')) {
  var root = ReactDOM.createRoot(document.getElementById('root'));
  root.render(React.createElement(App));
}
