/** Layout constants and builders for tech tree and regime chart. */

const COL_W = 160;
const ROW_H = 50;
const PAD_X = 16;
const PAD_Y = 28;
const NODE_W = 138;
const NODE_H = 36;
const HDR_H = 42;

const R_COL_W = 200;
const R_ROW_H = 90;
const R_NODE_W = 178;
const R_NODE_H = 72;
const R_PAD = 24;

function buildLayout(techs, levels) {
  const levelCols = {};
  levels.forEach(function (l) { levelCols[l.id] = new Set(); });
  techs.forEach(function (t) { levelCols[t.level].add(t.col); });

  const levelStartX = {};
  let curX = PAD_X;
  levels.forEach(function (l) {
    levelStartX[l.id] = curX;
    curX += [...levelCols[l.id]].length * COL_W + 8;
  });

  const totalW = curX + PAD_X;
  const maxRow = Math.max(...techs.map(function (t) { return t.row; }));
  const totalH = PAD_Y + HDR_H + maxRow * ROW_H + PAD_Y;

  const nodePos = {};
  techs.forEach(function (t) {
    const cols = [...levelCols[t.level]].sort(function (a, b) { return a - b; });
    const colIdx = cols.indexOf(t.col);
    const x = levelStartX[t.level] + colIdx * COL_W;
    const y = PAD_Y + HDR_H + (t.row - 1) * ROW_H;
    nodePos[t.id] = { x: x, y: y };
  });

  return { levelStartX: levelStartX, levelCols: levelCols, totalW: totalW, totalH: totalH, nodePos: nodePos };
}

function buildRegimeLayout() {
  const brackets = [
    { ids: ["band", "tribal"], col: 0 },
    { ids: ["chiefdom", "theocracy"], col: 1 },
    { ids: ["citystate", "feudal", "patrimonial"], col: 2 },
    { ids: ["republic", "empire"], col: 3 },
    { ids: ["absolutist", "constitutional"], col: 4 },
    { ids: ["democracy", "oneParty", "junta", "dictator"], col: 5 },
    { ids: ["technocracy", "corporatist", "failed"], col: 6 },
    { ids: ["directdem", "hive", "interstellarfed"], col: 7 },
  ];
  const pos = {};
  brackets.forEach(function (br) {
    br.ids.forEach(function (id, ri) { pos[id] = { col: br.col, row: ri }; });
  });
  const maxCol = 7;
  const maxRow = 3;
  return {
    pos: pos,
    totalW: R_PAD * 2 + (maxCol + 1) * (R_COL_W + 16),
    totalH: R_PAD * 2 + (maxRow + 1) * (R_ROW_H + 8) + 60
  };
}

window.COL_W = COL_W;
window.ROW_H = ROW_H;
window.PAD_X = PAD_X;
window.PAD_Y = PAD_Y;
window.NODE_W = NODE_W;
window.NODE_H = NODE_H;
window.HDR_H = HDR_H;
window.R_COL_W = R_COL_W;
window.R_ROW_H = R_ROW_H;
window.R_NODE_W = R_NODE_W;
window.R_NODE_H = R_NODE_H;
window.R_PAD = R_PAD;
window.buildLayout = buildLayout;
window.buildRegimeLayout = buildRegimeLayout;
