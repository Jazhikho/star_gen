/** UI components and root App for Evo Tech Tree. Depends on globals: NODES, NM, CAT, ENVS, STARTING, evolveStep, genSpecies, getUnlockable, canUnlock. */

function SpeciesDetail({species,onClose,onSave,saveLabel="💾 Save to Dictionary"}) {
  const S=({title,children})=><div style={{marginBottom:12}}><div style={{color:"#94a3b8",fontSize:10,fontWeight:700,letterSpacing:1.5,textTransform:"uppercase",marginBottom:3}}>{title}</div>{children}</div>;
  const Tag=({c="#1e293b",children})=><span style={{background:c,borderRadius:4,padding:"2px 8px",fontSize:11,color:"#e2e8f0",marginRight:4,marginBottom:4,display:"inline-block"}}>{children}</span>;
  return (
    <div style={{position:"fixed",inset:0,background:"#000000bb",zIndex:300,display:"flex",alignItems:"center",justifyContent:"center",padding:16}} onClick={onClose}>
      <div onClick={e=>e.stopPropagation()} style={{background:"#0f172a",border:"2px solid #334155",borderRadius:16,padding:24,maxWidth:780,width:"100%",maxHeight:"92vh",overflowY:"auto",color:"#f1f5f9"}}>
        <div style={{display:"flex",justifyContent:"space-between",alignItems:"flex-start",marginBottom:16}}>
          <div>
            <div style={{fontSize:10,color:"#64748b",letterSpacing:2,marginBottom:2}}>SPECIES RECORD · {species.environment} · {species.totalMya?.toFixed?.(1)||species.totalMya} Mya evolved</div>
            <div style={{fontSize:26,fontWeight:800,fontStyle:"italic",color:"#e2e8f0"}}>{species.name}</div>
            <div style={{fontSize:12,color:"#64748b"}}>{species.traitCount} traits</div>
          </div>
          <div style={{display:"flex",gap:8,alignItems:"center"}}>
            {onSave&&<button onClick={onSave} style={{background:"#16a34a",border:"none",color:"white",borderRadius:7,padding:"7px 14px",fontWeight:700,cursor:"pointer",fontSize:12}}>{saveLabel}</button>}
            <button onClick={onClose} style={{background:"none",border:"1px solid #334155",color:"#94a3b8",borderRadius:6,padding:"4px 12px",cursor:"pointer",fontSize:18}}>✕</button>
          </div>
        </div>
        <div style={{display:"grid",gridTemplateColumns:"1fr 1fr",gap:14}}>
          <div>
            <S title="Overview"><Tag c="#1e3a5f">Size: {species.size}</Tag><Tag c="#1e3a5f">Habitat: {species.habitat}</Tag><Tag c="#1e3a5f">Symmetry: {species.symmetry}</Tag><Tag c="#1e3a5f">Skeleton: {species.skeleton}</Tag></S>
            <S title="Integument"><div style={{fontSize:12,color:"#cbd5e1"}}>{species.integument} — {species.color}</div></S>
            <S title="Locomotion">{species.locomotion.map(l=><Tag key={l} c="#1a2e1a">{l}</Tag>)}</S>
            <S title="Diet"><Tag c="#2d1b0e">{species.diet}</Tag></S>
            <S title="Senses">{species.senses.map(s=><Tag key={s} c="#1e1b3a">{s}</Tag>)}</S>
            <S title="Reproduction"><div style={{fontSize:12,color:"#cbd5e1",marginBottom:3}}>{species.reproduction}</div><div style={{fontSize:11,color:"#94a3b8"}}>{species.parentalCare}</div></S>
            <S title="Lifecycle"><div style={{fontSize:12,color:"#cbd5e1"}}>{species.lifecycle} · Lifespan: {species.longevity}</div></S>
          </div>
          <div>
            <S title="Defense">{species.defense.map(d=><Tag key={d} c="#2d1b1b">{d}</Tag>)}</S>
            <S title="Metabolism"><Tag c="#2d2000">{species.metabolism}</Tag>{(species.metabolicExtras||[]).map(m=><Tag key={m} c="#1a1a2e">{m}</Tag>)}</S>
            <S title="Cognition"><div style={{fontSize:12,color:"#cbd5e1"}}>{species.cognition}</div></S>
            <S title="Social Structure"><Tag c="#0f2d1a">{species.social}</Tag></S>
            <S title="Communication">{species.communication.map(c=><Tag key={c} c="#1a1a2e">{c}</Tag>)}</S>
            {species.symbiosis?.length>0&&<S title="Symbiosis">{species.symbiosis.map(s=><Tag key={s} c="#0f2222">{s}</Tag>)}</S>}
            {species.extremophile?.length>0&&<S title="Extremophile">{species.extremophile.map(e=><Tag key={e} c="#2d0f0f">{e}</Tag>)}</S>}
          </div>
        </div>
        <div style={{marginTop:12,padding:12,background:"#1e293b",borderRadius:8,fontSize:12,color:"#64748b",lineHeight:1.6}}>
          <b style={{color:"#94a3b8"}}>Field Notes: </b>
          <i>{species.name}</i> is a {species.size} {species.diet} inhabiting {species.habitat}, moving via {species.locomotion.slice(0,2).join(" and ")}.
          It perceives its world through {species.senses.slice(0,2).join(" and ")}, and defends itself with {species.defense.slice(0,2).join(" and ")}.
          Reproductively, it employs {species.reproduction}. {species.parentalCare}
          {species.extremophile?.length>0?` Remarkably, it has evolved ${species.extremophile[0]}.`:""} Cognitively it exhibits {species.cognition}.
        </div>
      </div>
    </div>
  );
}

function DictModal({saved,onClose,onLoadTraits,onExport,onClear}) {
  const [sel,setSel]=React.useState(null);
  const [lineageFilter,setLineageFilter]=React.useState(null);
  const [confirmClear,setConfirmClear]=React.useState(false);
  const nameSet=new Set(saved.map(s=>s.name));
  const getRoot=(sp)=>{
    let cur=sp; const visited=new Set();
    while(cur.ancestorName && nameSet.has(cur.ancestorName) && !visited.has(cur.ancestorName)){
      visited.add(cur.ancestorName);
      cur=saved.find(s=>s.name===cur.ancestorName)||cur;
    }
    return cur.name;
  };
  const lineageRoots=[...new Set(saved.map(sp=>getRoot(sp)))];
  const standaloneRoots=lineageRoots.filter(r=>saved.filter(sp=>getRoot(sp)===r).length===1);
  const familyRoots=lineageRoots.filter(r=>saved.filter(sp=>getRoot(sp)===r).length>1);

  return (
    <div style={{position:"fixed",inset:0,background:"#000000cc",zIndex:200,display:"flex",alignItems:"center",justifyContent:"center",padding:16}} onClick={()=>{if(!sel)onClose();}}>
      <div onClick={e=>e.stopPropagation()} style={{background:"#0f172a",border:"2px solid #334155",borderRadius:16,padding:0,maxWidth:920,width:"100%",maxHeight:"92vh",display:"flex",flexDirection:"column",color:"#f1f5f9",overflow:"hidden"}}>
        <div style={{padding:"14px 20px",borderBottom:"1px solid #1e293b",display:"flex",alignItems:"center",gap:10}}>
          <div style={{fontSize:17,fontWeight:800,color:"#4ade80"}}>📚 Species Dictionary ({saved.length})</div>
          <div style={{marginLeft:"auto",display:"flex",gap:7}}>
            <button onClick={onExport} style={{background:"#1e293b",border:"1px solid #334155",color:"#94a3b8",borderRadius:6,padding:"5px 11px",cursor:"pointer",fontSize:10}}>⬇ Export JSON</button>
            {confirmClear
              ? <><button onClick={()=>{onClear();setConfirmClear(false);}} style={{background:"#7f1d1d",border:"1px solid #ef4444",color:"#fca5a5",borderRadius:6,padding:"5px 11px",cursor:"pointer",fontSize:10}}>Confirm Clear</button>
                <button onClick={()=>setConfirmClear(false)} style={{background:"#1e293b",border:"1px solid #334155",color:"#94a3b8",borderRadius:6,padding:"5px 11px",cursor:"pointer",fontSize:10}}>Cancel</button></>
              : <button onClick={()=>setConfirmClear(true)} style={{background:"#1e293b",border:"1px solid #7f1d1d",color:"#f87171",borderRadius:6,padding:"5px 11px",cursor:"pointer",fontSize:10}}>🗑 Clear All</button>}
            <button onClick={onClose} style={{background:"none",border:"1px solid #334155",color:"#94a3b8",borderRadius:6,padding:"4px 12px",cursor:"pointer"}}>✕</button>
          </div>
        </div>
        <div style={{padding:"7px 20px",borderBottom:"1px solid #1e293b",display:"flex",gap:5,overflowX:"auto",alignItems:"center"}}>
          <span style={{fontSize:9,color:"#475569",whiteSpace:"nowrap",fontWeight:700,marginRight:2}}>LINEAGE</span>
          <button onClick={()=>setLineageFilter(null)} style={{background:lineageFilter===null?"#1e3a5f":"transparent",border:`1px solid ${lineageFilter===null?"#60a5fa":"#334155"}`,color:lineageFilter===null?"#60a5fa":"#64748b",borderRadius:12,padding:"2px 9px",cursor:"pointer",fontSize:9,whiteSpace:"nowrap"}}>All</button>
          {familyRoots.map(r=>(
            <button key={r} onClick={()=>setLineageFilter(r)} style={{background:lineageFilter===r?"#1e3a5f":"transparent",border:`1px solid ${lineageFilter===r?"#60a5fa":"#334155"}`,color:lineageFilter===r?"#60a5fa":"#64748b",borderRadius:12,padding:"2px 9px",cursor:"pointer",fontSize:9,whiteSpace:"nowrap"}}>🌳 <i>{r.split(" ")[0]}</i></button>
          ))}
          {standaloneRoots.length>0&&(
            <button onClick={()=>setLineageFilter("__standalone__")} style={{background:lineageFilter==="__standalone__"?"#1e3a5f":"transparent",border:`1px solid ${lineageFilter==="__standalone__"?"#60a5fa":"#334155"}`,color:lineageFilter==="__standalone__"?"#60a5fa":"#64748b",borderRadius:12,padding:"2px 9px",cursor:"pointer",fontSize:9,whiteSpace:"nowrap"}}>🌿 Standalone ({standaloneRoots.length})</button>
          )}
        </div>
        <div style={{overflowY:"auto",padding:"12px 20px",flex:1}}>
          {saved.length===0&&<div style={{color:"#475569",fontSize:13}}>No species saved yet. Generate and save species to build your dictionary!</div>}
          {saved.map((sp,i)=>{
            const root=getRoot(sp);
            const isStandalone=standaloneRoots.includes(root);
            if(lineageFilter===null) {}
            else if(lineageFilter==="__standalone__"&&!isStandalone) return null;
            else if(lineageFilter!=="__standalone__"&&root!==lineageFilter) return null;
            const depth = (()=>{let d=0,c=sp;const v=new Set();while(c.ancestorName&&nameSet.has(c.ancestorName)&&!v.has(c.ancestorName)){v.add(c.ancestorName);c=saved.find(s=>s.name===c.ancestorName)||c;d++;}return d;})();
            return (
              <div key={i} style={{background:"#1e293b",borderRadius:9,padding:"12px 14px",marginBottom:9,border:`1px solid ${sp.ancestorName&&nameSet.has(sp.ancestorName)?"#1e3a5f":"#334155"}`,cursor:"pointer",marginLeft:Math.min(depth,4)*12}} onClick={()=>setSel(i)}>
                <div style={{display:"flex",justifyContent:"space-between",alignItems:"flex-start",gap:8}}>
                  <div style={{minWidth:0}}>
                    {sp.ancestorName&&nameSet.has(sp.ancestorName)&&<div style={{fontSize:9,color:"#475569",marginBottom:2}}>↳ descendant of <i style={{color:"#60a5fa"}}>{sp.ancestorName}</i></div>}
                    {!sp.ancestorName&&<div style={{fontSize:9,color:"#334155",marginBottom:2}}>🌿 standalone origin</div>}
                    <div style={{fontSize:14,fontWeight:700,fontStyle:"italic",color:"#e2e8f0"}}>{sp.name}</div>
                    <div style={{fontSize:10,color:"#64748b"}}>{sp.habitat} · {sp.size} · {sp.traitCount} traits · {(sp.totalMya||0).toFixed?.(1)||(sp.totalMya||0)} Mya</div>
                  </div>
                  <div style={{display:"flex",gap:5,flexShrink:0}}>
                    {sp.unlockedSnapshot&&<button onClick={e=>{e.stopPropagation();onLoadTraits(sp,i);}} style={{background:"#0f2d1a",border:"1px solid #16a34a",color:"#4ade80",borderRadius:6,padding:"3px 9px",fontSize:9,cursor:"pointer"}}>🔁 Branch From</button>}
                    <button onClick={e=>{e.stopPropagation();setSel(i);}} style={{background:"#1e3a5f",border:"1px solid #3b82f6",color:"#60a5fa",borderRadius:6,padding:"3px 9px",fontSize:9,cursor:"pointer"}}>Details</button>
                  </div>
                </div>
                <div style={{marginTop:7,display:"flex",flexWrap:"wrap",gap:3}}>
                  <span style={{background:"#2d1b0e",borderRadius:3,padding:"1px 6px",fontSize:9,color:"#facc15"}}>{sp.diet}</span>
                  <span style={{background:"#1e1b3a",borderRadius:3,padding:"1px 6px",fontSize:9,color:"#c084fc"}}>{sp.cognition}</span>
                  <span style={{background:"#0f2d1a",borderRadius:3,padding:"1px 6px",fontSize:9,color:"#34d399"}}>{sp.social}</span>
                  {sp.defense?.slice(0,2).map(d=><span key={d} style={{background:"#2d1b1b",borderRadius:3,padding:"1px 6px",fontSize:9,color:"#f87171"}}>{d}</span>)}
                </div>
              </div>
            );
          })}
        </div>
      </div>
      {sel!==null&&<SpeciesDetail species={saved[sel]} onClose={()=>setSel(null)}/>}
    </div>
  );
}

function NodeCard({node,state,onClick,highlight}) {
  const c=CAT[node.cat];
  const isU=state==="unlocked",isA=state==="unlockable";
  return (
    <div onClick={()=>onClick(node.id)} title={node.desc} style={{
      background:isU?`${c.color}20`:isA?"#1e293b":"#0a0f1e",
      border:`2px solid ${isU?c.color:isA?"#475569":"#1e293b"}`,
      borderRadius:6,padding:"5px 9px",cursor:state==="locked"?"default":"pointer",
      opacity:state==="locked"?0.28:1,transition:"all 0.15s",
      minWidth:136,maxWidth:156,userSelect:"none",position:"relative",
      boxShadow:highlight==="focus"?`0 0 0 2px #facc15,0 0 12px #facc1566`:isU?`0 0 6px ${c.color}44`:"none",
    }}>
      <div style={{fontSize:9,color:c.color,fontWeight:700}}>{c.icon} {c.label}</div>
      <div style={{fontSize:11,color:isU?"#f1f5f9":isA?"#cbd5e1":"#3f4f6a",fontWeight:600,lineHeight:1.3,marginTop:1}}>{node.label}</div>
      {isA&&<div style={{fontSize:9,color:"#4ade80",marginTop:1}}>▶ Unlock</div>}
      {isU&&<div style={{position:"absolute",top:3,right:5,fontSize:9,color:c.color}}>✓</div>}
    </div>
  );
}

function EvoTimeline({steps,currentIdx,onRevert}) {
  if(!steps.length)return null;
  return (
    <div style={{marginBottom:16}}>
      <div style={{fontSize:11,color:"#64748b",fontWeight:700,marginBottom:8,letterSpacing:1}}>🌳 LINEAGE TIMELINE — click any node to branch from that ancestor</div>
      <div style={{overflowX:"auto",paddingBottom:8}}>
        <div style={{display:"flex",alignItems:"center",minWidth:"max-content",gap:0}}>
          <div style={{display:"flex",flexDirection:"column",alignItems:"center",gap:3}}>
            <div style={{width:10,height:10,borderRadius:"50%",background:"#4ade80",border:"2px solid #166534"}}/>
            <div style={{fontSize:9,color:"#4ade80",fontWeight:700}}>ORIGIN</div>
          </div>
          {steps.map((step,i)=>{
            const isCurrent=i===steps.length-1;
            const isReverted=i<steps.length-1;
            return (
              <div key={i} style={{display:"flex",alignItems:"center"}}>
                <div style={{width:32,height:2,background:isCurrent?"#4ade80":isReverted?"#334155":"#4ade8066"}}/>
                <div style={{display:"flex",flexDirection:"column",alignItems:"center",gap:3}}>
                  <div onClick={()=>onRevert(i)} title={`Revert to ${step.totalMya.toFixed(1)} Mya — ${ENVS[step.envKey]?.label}`} style={{width:isCurrent?14:10,height:isCurrent?14:10,borderRadius:"50%",background:isCurrent?"#4ade80":"#334155",border:`2px solid ${isCurrent?"#16a34a":"#475569"}`,cursor:"pointer",transition:"all 0.15s",boxShadow:isCurrent?"0 0 8px #4ade8066":"none"}}/>
                  <div style={{fontSize:9,color:isCurrent?"#4ade80":"#475569",textAlign:"center",lineHeight:1.2,cursor:"pointer",whiteSpace:"nowrap"}} onClick={()=>onRevert(i)}>{ENVS[step.envKey]?.icon} {step.totalMya.toFixed(1)}Ma<div style={{fontSize:8,color:"#334155"}}>+{step.gained.length}t</div></div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

function EvoMode({initUnlocked,onApply,onClose}) {
  const [envKey,setEnvKey]=React.useState("ocean_shallow");
  const [steps,setSteps]=React.useState([]);
  const [cur,setCur]=React.useState(new Set(initUnlocked||STARTING));
  const [timeMode,setTimeMode]=React.useState("fixed");
  const [fixedMya,setFixedMya]=React.useState(2);
  const [totalMya,setTotalMya]=React.useState(0);
  const [autoShift,setAutoShift]=React.useState(true);

  const doStep=(mya)=>{
    const newEnv=autoShift&&Math.random()<0.3?pick(ENV_TRANSITIONS[envKey]||[envKey]):envKey;
    const newU=evolveStep(cur,ENVS[newEnv],mya);
    const gained=[...newU].filter(id=>!cur.has(id));
    const newTotal=totalMya+mya;
    const step={envKey:newEnv,mya:+mya.toFixed(2),totalMya:+newTotal.toFixed(2),unlocked:new Set(newU),gained};
    setSteps(s=>[...s,step]);
    setCur(newU);
    setEnvKey(newEnv);
    setTotalMya(newTotal);
  };

  const revertTo=(idx)=>{
    const s=steps[idx];
    setSteps(prev=>prev.slice(0,idx+1));
    setCur(new Set(s.unlocked));
    setEnvKey(s.envKey);
    setTotalMya(s.totalMya);
  };

  const advance=()=>{const mya=timeMode==="random"?+(0.5+Math.random()*2.5).toFixed(2):fixedMya;doStep(mya);};
  const env=ENVS[envKey];

  return (
    <div style={{position:"fixed",inset:0,background:"#000000cc",zIndex:150,display:"flex",alignItems:"center",justifyContent:"center",padding:12}} onClick={onClose}>
      <div onClick={e=>e.stopPropagation()} style={{background:"#0f172a",border:"2px solid #334155",borderRadius:16,padding:22,maxWidth:820,width:"100%",maxHeight:"94vh",overflowY:"auto",color:"#f1f5f9"}}>
        <div style={{display:"flex",justifyContent:"space-between",alignItems:"center",marginBottom:14}}>
          <div>
            <div style={{fontSize:18,fontWeight:800,color:"#4ade80"}}>🌍 Natural Selection Simulator</div>
            <div style={{fontSize:11,color:"#64748b"}}>Traits evolve at ~{MYA_PER_TRAIT} Mya per adaptation on average. Use longer steps for macro-evolution.</div>
          </div>
          <button onClick={onClose} style={{background:"none",border:"1px solid #334155",color:"#94a3b8",borderRadius:6,padding:"4px 10px",cursor:"pointer",fontSize:16}}>✕</button>
        </div>
        <div style={{marginBottom:12}}>
          <div style={{fontSize:10,color:"#64748b",fontWeight:700,marginBottom:6}}>ENVIRONMENT — {env.icon} {env.label} <span style={{color:"#475569",fontWeight:400}}>· {env.desc}</span></div>
          <div style={{display:"flex",flexWrap:"wrap",gap:5,marginBottom:8}}>
            {Object.entries(ENVS).map(([k,v])=>(<button key={k} onClick={()=>setEnvKey(k)} style={{background:envKey===k?"#1e40af22":"transparent",border:`1px solid ${envKey===k?"#60a5fa":"#334155"}`,color:envKey===k?"#60a5fa":"#64748b",borderRadius:14,padding:"3px 9px",fontSize:10,cursor:"pointer"}}>{v.icon} {v.label}</button>))}
          </div>
          <div style={{display:"grid",gridTemplateColumns:"repeat(4,1fr)",gap:6}}>
            {Object.entries(env.pressures).map(([k,v])=>(<div key={k}><div style={{fontSize:9,color:"#64748b",marginBottom:2,textTransform:"capitalize"}}>{k} {v}/5</div><div style={{height:5,background:"#1e293b",borderRadius:3}}><div style={{height:5,width:`${v*20}%`,background:`hsl(${120-v*20},70%,50%)`,borderRadius:3,transition:"width 0.3s"}}/></div></div>))}
          </div>
        </div>
        <div style={{display:"flex",gap:10,marginBottom:12,alignItems:"center",flexWrap:"wrap",background:"#1e293b",padding:"10px 14px",borderRadius:8}}>
          <label style={{fontSize:11,color:"#94a3b8"}}>Time per step:</label>
          <button onClick={()=>setTimeMode("fixed")} style={{background:timeMode==="fixed"?"#1e3a5f":"transparent",border:`1px solid ${timeMode==="fixed"?"#60a5fa":"#334155"}`,color:timeMode==="fixed"?"#60a5fa":"#64748b",borderRadius:6,padding:"3px 9px",fontSize:10,cursor:"pointer"}}>Fixed</button>
          <button onClick={()=>setTimeMode("random")} style={{background:timeMode==="random"?"#1e3a5f":"transparent",border:`1px solid ${timeMode==="random"?"#60a5fa":"#334155"}`,color:timeMode==="random"?"#60a5fa":"#64748b",borderRadius:6,padding:"3px 9px",fontSize:10,cursor:"pointer"}}>Random</button>
          {timeMode==="fixed"&&<><input type="range" min={0.5} max={10} step={0.5} value={fixedMya} onChange={e=>setFixedMya(+e.target.value)} style={{width:100}}/><span style={{fontSize:11,color:"#60a5fa",minWidth:40}}>{fixedMya} Mya</span></>}
          <label style={{fontSize:11,color:"#94a3b8",display:"flex",alignItems:"center",gap:5,marginLeft:8}}><input type="checkbox" checked={autoShift} onChange={e=>setAutoShift(e.target.checked)}/>Auto-shift env.</label>
          <div style={{marginLeft:"auto",fontSize:12,color:"#64748b"}}><span style={{color:"#4ade80",fontWeight:700}}>{totalMya.toFixed(1)}</span> Mya total · <span style={{color:"#60a5fa"}}>{cur.size}</span> traits</div>
        </div>
        <div style={{display:"flex",gap:8,marginBottom:16}}>
          <button onClick={advance} style={{background:"#16a34a",border:"none",color:"white",borderRadius:8,padding:"8px 18px",fontWeight:700,cursor:"pointer",fontSize:12}}>⏩ Advance {timeMode==="random"?"(0.5–3 Mya)":fixedMya+" Mya"}</button>
          <button onClick={()=>{for(let i=0;i<5;i++)setTimeout(advance,i*80);}} style={{background:"#1e3a5f",border:"1px solid #334155",color:"#60a5fa",borderRadius:8,padding:"8px 12px",cursor:"pointer",fontSize:11}}>⏭ ×5 Steps</button>
          <button onClick={()=>{ setCur(new Set(STARTING)); setSteps([]); setTotalMya(0); }} style={{background:"#2d1b1b",border:"1px solid #7f1d1d",color:"#f87171",borderRadius:8,padding:"8px 12px",cursor:"pointer",fontSize:11}}>↩ Reset</button>
        </div>
        <EvoTimeline steps={steps} currentIdx={steps.length-1} onRevert={revertTo}/>
        {steps.length>0&&(
          <div style={{marginBottom:14,background:"#1e293b",borderRadius:8,padding:"10px 14px"}}>
            <div style={{fontSize:10,color:"#64748b",fontWeight:700,marginBottom:6}}>LAST STEP: +{steps[steps.length-1].gained.length} TRAITS ({ENVS[steps[steps.length-1].envKey]?.icon} {ENVS[steps[steps.length-1].envKey]?.label}, {steps[steps.length-1].mya} Mya)</div>
            <div style={{display:"flex",flexWrap:"wrap",gap:4}}>
              {steps[steps.length-1].gained.length===0?<span style={{color:"#475569",fontSize:11}}>No new traits this step — increase time or change environment.</span>:steps[steps.length-1].gained.map(id=>{const n=NM[id];const c=CAT[n.cat];return <span key={id} style={{background:`${c.color}22`,border:`1px solid ${c.color}66`,borderRadius:4,padding:"2px 8px",fontSize:10,color:c.color}}>{c.icon} {n.label}</span>;})}
            </div>
          </div>
        )}
        <div style={{display:"flex",gap:8}}>
          <button onClick={()=>onApply(cur,envKey,totalMya,null)} style={{background:"#4ade80",color:"#020617",border:"none",borderRadius:8,padding:"8px 18px",fontWeight:700,cursor:"pointer",fontSize:12}}>✅ Apply to Tree</button>
          <button onClick={()=>onApply(cur,envKey,totalMya,"generate")} style={{background:"#7c3aed",color:"white",border:"none",borderRadius:8,padding:"8px 18px",fontWeight:700,cursor:"pointer",fontSize:12}}>🔬 Apply & Generate Species</button>
          <button onClick={onClose} style={{background:"#334155",border:"none",color:"#f1f5f9",borderRadius:8,padding:"8px 12px",cursor:"pointer",fontSize:11}}>Cancel</button>
        </div>
      </div>
    </div>
  );
}

function ConnCanvas({rects,unlocked,unlockable,hovered}) {
  const ref=React.useRef(null);
  React.useEffect(()=>{
    const cv=ref.current; if(!cv)return;
    const ctx=cv.getContext("2d");
    ctx.clearRect(0,0,cv.width,cv.height);
    for(const node of NODES){
      for(const rid of node.req){
        const fr=rects[rid],tr=rects[node.id];
        if(!fr||!tr)continue;
        const isHov=hovered===node.id||hovered===rid;
        ctx.beginPath();
        ctx.setLineDash(isHov?[4,3]:[4,5]);
        ctx.lineWidth=isHov?1.5:1;
        ctx.strokeStyle=isHov?"#facc15":(unlocked.has(rid)&&(unlocked.has(node.id)||unlockable.has(node.id)))?"#4ade8055":"#1e293b";
        const x1=fr.x+fr.w, y1=fr.y+fr.h/2;
        const x2=tr.x, y2=tr.y+tr.h/2;
        const cpx=(x1+x2)/2;
        ctx.moveTo(x1,y1);
        ctx.bezierCurveTo(cpx,y1,cpx,y2,x2,y2);
        ctx.stroke();
      }
    }
  },[rects,unlocked,unlockable,hovered]);
  return <canvas ref={ref} width={5000} height={4000} style={{position:"absolute",top:0,left:0,pointerEvents:"none",zIndex:0}}/>;
}

function App() {
  const [unlocked,setUnlocked]=React.useState(new Set(STARTING));
  const [selCat,setSelCat]=React.useState("ALL");
  const [search,setSearch]=React.useState("");
  const [selNode,setSelNode]=React.useState(null);
  const [hovered,setHovered]=React.useState(null);
  const [species,setSpecies]=React.useState(null);
  const [saved,setSaved]=React.useState([]);
  const [showDict,setShowDict]=React.useState(false);
  const [showEvo,setShowEvo]=React.useState(false);
  const [curEnv,setCurEnv]=React.useState("ocean_shallow");
  const [totalMya,setTotalMya]=React.useState(0);
  const [rects,setRects]=React.useState({});
  const [ancestor,setAncestor]=React.useState(null);
  const treeRef=React.useRef(null);
  const cardRefs=React.useRef({});

  const unlockable=React.useMemo(()=>getUnlockable(unlocked),[unlocked]);
  const filteredNodes=React.useMemo(()=>NODES.filter(n=>{const cOk=selCat==="ALL"||n.cat===selCat;const sOk=!search||n.label.toLowerCase().includes(search.toLowerCase())||n.desc.toLowerCase().includes(search.toLowerCase());return cOk&&sOk;}),[selCat,search]);
  const maxTier=React.useMemo(()=>Math.max(...NODES.map(n=>n.tier)),[]);
  const byTier=React.useMemo(()=>{
    const bt={};
    for(let t=0;t<=maxTier;t++){
      const nodes=filteredNodes.filter(n=>n.tier===t);
      const sorted=[...nodes];
      sorted.sort((a,b)=>{const aReqInTier=a.req.some(r=>NM[r]&&NM[r].tier===t);const bReqInTier=b.req.some(r=>NM[r]&&NM[r].tier===t);if(aReqInTier&&!bReqInTier)return 1;if(!aReqInTier&&bReqInTier)return -1;if(a.cat!==b.cat)return a.cat.localeCompare(b.cat);return 0;});
      if(sorted.length)bt[t]=sorted;
    }
    return bt;
  },[filteredNodes,maxTier]);

  React.useEffect(()=>{
    const r={};
    for(const id of Object.keys(cardRefs.current)){const el=cardRefs.current[id];if(!el||!treeRef.current)continue;const tr2=treeRef.current.getBoundingClientRect();const er=el.getBoundingClientRect();r[id]={x:er.left-tr2.left,y:er.top-tr2.top,w:er.width,h:er.height};}
    setRects(r);
  },[unlocked,selCat,search,byTier]);

  const handleClick=React.useCallback(id=>{if(unlocked.has(id)){setSelNode(n=>n===id?null:id);return;}if(unlockable.has(id))setUnlocked(prev=>new Set([...prev,id]));},[unlocked,unlockable]);
  const handleEvoApply=(newU,envKey,mya,mode)=>{setUnlocked(newU);setCurEnv(envKey);setTotalMya(mya);setShowEvo(false);if(mode==="generate")setSpecies(genSpecies(newU,envKey,mya));};

  const saveSpecies=()=>{
    if(!species)return;
    const sp={...species,ancestorName:ancestor?ancestor.name:null,savedAt:Date.now()};
    setSaved(s=>{const next=[...s,sp];setAncestor({name:sp.name,savedIdx:next.length-1});return next;});
    setSpecies(null);
  };

  const exportDict=()=>{const blob=new Blob([JSON.stringify(saved,null,2)],{type:"application/json"});const a=document.createElement("a");a.href=URL.createObjectURL(blob);a.download="species_dictionary.json";a.click();};

  const loadTraits=(sp,idx)=>{if(sp.unlockedSnapshot){setUnlocked(new Set(sp.unlockedSnapshot));setCurEnv(Object.keys(ENVS).find(k=>ENVS[k].label===sp.environment)||"ocean_shallow");setTotalMya(sp.totalMya||0);setAncestor({name:sp.name,savedIdx:idx});}setShowDict(false);};

  const selNodeData=selNode?NM[selNode]:null;
  const getHighlight=(id)=>{if(!hovered)return null;if(id===hovered)return "focus";const hn=NM[hovered];if(hn&&hn.req.includes(id))return "prereq";const idn=NM[id];if(idn&&idn.req.includes(hovered))return "unlocks";return null;};

  return (
    <div style={{background:"#020617",minHeight:"100vh",color:"#f1f5f9",fontFamily:"'Segoe UI',system-ui,sans-serif",display:"flex",flexDirection:"column",height:"100vh"}}>
      <div style={{background:"#0f172a",borderBottom:"1px solid #1e293b",padding:"10px 18px",display:"flex",alignItems:"center",gap:12,flexWrap:"wrap",flexShrink:0}}>
        <div><div style={{fontSize:18,fontWeight:800,color:"#4ade80"}}>🧬 Evo Tech Tree</div><div style={{fontSize:9,color:"#475569"}}>Biology modeled as an evolutionary technology tree</div></div>
        <input value={search} onChange={e=>setSearch(e.target.value)} placeholder="Search traits…" style={{background:"#1e293b",border:"1px solid #334155",borderRadius:6,padding:"5px 10px",color:"#f1f5f9",fontSize:11,width:160}}/>
        <div style={{fontSize:10,color:"#64748b"}}><span style={{color:"#4ade80",fontWeight:700}}>{unlocked.size}</span>/{NODES.length} &nbsp;·&nbsp;<span style={{color:"#60a5fa"}}>{unlockable.size}</span> available{totalMya>0&&<>&nbsp;·&nbsp;<span style={{color:"#facc15"}}>{totalMya.toFixed(1)} Mya</span></>}{ancestor&&<>&nbsp;·&nbsp;<span style={{color:"#e879f9"}}>Branching from: <i>{ancestor.name}</i></span></>}</div>
        <div style={{marginLeft:"auto",display:"flex",gap:6,flexWrap:"wrap"}}>
          <button onClick={()=>setShowEvo(true)} style={{background:"#7c3aed",border:"none",color:"white",borderRadius:7,padding:"6px 12px",fontWeight:700,cursor:"pointer",fontSize:11}}>🌍 Evo Mode</button>
          <button onClick={()=>setSpecies(genSpecies(unlocked,curEnv,totalMya))} style={{background:"#4ade80",color:"#020617",border:"none",borderRadius:7,padding:"6px 12px",fontWeight:700,cursor:"pointer",fontSize:11}}>🔬 Generate</button>
          <button onClick={()=>setShowDict(true)} style={{background:"#334155",color:"#f1f5f9",border:"none",borderRadius:7,padding:"6px 10px",cursor:"pointer",fontSize:11}}>📚 Dict ({saved.length})</button>
          <button onClick={()=>{setUnlocked(new Set(NODES.map(n=>n.id)));}} style={{background:"#334155",color:"#f1f5f9",border:"none",borderRadius:7,padding:"6px 10px",cursor:"pointer",fontSize:10}}>Unlock All</button>
          <button onClick={()=>{setUnlocked(new Set(STARTING));setSelNode(null);setSpecies(null);setTotalMya(0);setAncestor(null);setCurEnv("ocean_shallow");}} style={{background:"#334155",color:"#f1f5f9",border:"none",borderRadius:7,padding:"6px 10px",cursor:"pointer",fontSize:10}}>Reset Tree</button>
        </div>
      </div>
      <div style={{background:"#0f172a",borderBottom:"1px solid #1e293b",padding:"5px 18px",display:"flex",gap:5,overflowX:"auto",alignItems:"center",flexShrink:0}}>
        <span style={{fontSize:9,color:"#475569",whiteSpace:"nowrap",fontWeight:700}}>PRESETS</span>
        {Object.keys(PRESETS).map(p=>(<button key={p} onClick={()=>{setUnlocked(new Set(PRESETS[p]));setSelNode(null);}} style={{background:"#1e293b",border:"1px solid #334155",color:"#94a3b8",borderRadius:12,padding:"2px 9px",cursor:"pointer",fontSize:9,whiteSpace:"nowrap"}}>{p}</button>))}
        <div style={{width:1,height:16,background:"#334155",margin:"0 4px"}}/>
        {[{key:"ALL",label:"All",color:"#64748b",icon:"🌐"},...Object.entries(CAT).map(([k,v])=>({key:k,...v}))].map(c=>(<button key={c.key} onClick={()=>setSelCat(c.key)} style={{background:selCat===c.key?`${c.color}33`:"transparent",border:`1px solid ${selCat===c.key?c.color:"#334155"}`,color:selCat===c.key?c.color:"#64748b",borderRadius:12,padding:"2px 8px",cursor:"pointer",fontSize:9,whiteSpace:"nowrap",fontWeight:selCat===c.key?700:400}}>{c.icon} {c.label}</button>))}
      </div>
      <div style={{padding:"4px 18px",display:"flex",gap:12,fontSize:9,color:"#475569",borderBottom:"1px solid #1e293b",flexShrink:0}}><span>🟢 Unlocked (inspect)</span><span>⬜ Unlockable (click)</span><span>⬛ Locked</span><span>— — Lines: prerequisites (hover to highlight)</span><span style={{marginLeft:"auto"}}>Tiers increase →</span></div>
      <div style={{flex:1,overflow:"auto",position:"relative"}}>
        <div ref={treeRef} style={{position:"relative",padding:"16px 18px",display:"inline-flex",gap:14,alignItems:"stretch",minWidth:"max-content",minHeight:"100%"}}>
          <ConnCanvas rects={rects} unlocked={unlocked} unlockable={unlockable} hovered={hovered}/>
          {Object.entries(byTier).map(([tier,nodes])=>(
            <div key={tier} style={{display:"flex",flexDirection:"column",gap:7,alignItems:"stretch",position:"relative",zIndex:1}}>
              <div style={{fontSize:8,color:"#1e3a5f",letterSpacing:1,fontWeight:700,textAlign:"center",paddingBottom:2}}>T{tier}</div>
              {nodes.map(node=>{const state=unlocked.has(node.id)?"unlocked":unlockable.has(node.id)?"unlockable":"locked";const hl=getHighlight(node.id);return (<div key={node.id} ref={el=>{if(el)cardRefs.current[node.id]=el;}} onMouseEnter={()=>setHovered(node.id)} onMouseLeave={()=>setHovered(null)}><NodeCard node={node} state={state} onClick={handleClick} highlight={hl}/></div>);})}
            </div>
          ))}
        </div>
      </div>
      {selNodeData&&(
        <div style={{background:"#0c1524",borderTop:"1px solid #1e293b",padding:"10px 18px",display:"flex",gap:18,alignItems:"flex-start",flexShrink:0}}>
        <div style={{flex:1,minWidth:0}}><div style={{fontSize:9,color:CAT[selNodeData.cat].color,fontWeight:700,letterSpacing:1.5,marginBottom:1}}>{CAT[selNodeData.cat].icon} {CAT[selNodeData.cat].label} — TIER {selNodeData.tier}</div><div style={{fontSize:14,fontWeight:700,marginBottom:1}}>{selNodeData.label}</div><div style={{fontSize:11,color:"#94a3b8"}}>{selNodeData.desc}</div></div>
        <div style={{fontSize:10,color:"#64748b",minWidth:160}}><div style={{color:"#475569",fontWeight:700,fontSize:9,marginBottom:3}}>PREREQUISITES</div>{selNodeData.req.length===0?<span style={{color:"#4ade80"}}>None — base trait</span>:selNodeData.req.map(r=>(<div key={r} style={{display:"flex",alignItems:"center",gap:4,marginBottom:2}}><span style={{color:unlocked.has(r)?"#4ade80":"#f87171",fontSize:10}}>{unlocked.has(r)?"✓":"✗"}</span><span style={{color:unlocked.has(r)?"#64748b":"#94a3b8"}}>{NM[r]?.label}</span></div>))}</div>
        <div style={{fontSize:10,color:"#64748b",minWidth:140}}><div style={{color:"#475569",fontWeight:700,fontSize:9,marginBottom:3}}>UNLOCKS</div>{NODES.filter(n=>n.req.includes(selNode)).map(n=>(<div key={n.id} style={{color:unlocked.has(n.id)?"#4ade80":unlockable.has(n.id)?"#60a5fa":"#475569",marginBottom:2}}>→ {n.label}</div>))}{NODES.filter(n=>n.req.includes(selNode)).length===0&&<span style={{color:"#1e293b"}}>Terminal node</span>}</div>
        <button onClick={()=>setSelNode(null)} style={{background:"none",border:"1px solid #1e293b",color:"#475569",borderRadius:5,padding:"3px 8px",cursor:"pointer",fontSize:12}}>✕</button>
      </div>
      )}
      {showEvo&&<EvoMode initUnlocked={unlocked} onApply={handleEvoApply} onClose={()=>setShowEvo(false)}/>}
      {species&&<SpeciesDetail species={species} onClose={()=>setSpecies(null)} onSave={saveSpecies}/>}
      {showDict&&<DictModal saved={saved} onClose={()=>setShowDict(false)} onLoadTraits={loadTraits} onExport={exportDict} onClear={()=>{setSaved([]);setAncestor(null);}}/>}
    </div>
  );
}

window.App = App;
if (typeof document !== "undefined" && document.getElementById("root")) {
  var root = ReactDOM.createRoot(document.getElementById("root"));
  root.render(React.createElement(App));
}
