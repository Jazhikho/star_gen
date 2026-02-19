/** Layout for Tech Tree and Regime Chart in Integration (shared data). */
(function () {
  "use strict";

  var TT_COL = 152;
  var TT_ROW = 48;
  var TT_NW = 130;
  var TT_NH = 34;
  var TT_PAD = 14;
  var TT_HDR = 40;

  function buildTechLayout() {
    var LEVELS = window.LEVELS;
    var TECHS = window.TECHS;
    var lvlCols = {};
    var i;
    for (i = 0; i < LEVELS.length; i++) lvlCols[LEVELS[i].id] = new Set();
    for (i = 0; i < TECHS.length; i++) lvlCols[TECHS[i].level].add(TECHS[i].col);
    var startX = {};
    var cx = TT_PAD;
    for (i = 0; i < LEVELS.length; i++) {
      var lid = LEVELS[i].id;
      startX[lid] = cx;
      cx += [...lvlCols[lid]].length * TT_COL + 6;
    }
    var pos = {};
    for (i = 0; i < TECHS.length; i++) {
      var t = TECHS[i];
      var cols = [...lvlCols[t.level]].sort(function (a, b) { return a - b; });
      pos[t.id] = { x: startX[t.level] + cols.indexOf(t.col) * TT_COL, y: TT_PAD + TT_HDR + (t.row - 1) * TT_ROW };
    }
    var maxRow = 0;
    for (i = 0; i < TECHS.length; i++) if (TECHS[i].row > maxRow) maxRow = TECHS[i].row;
    return { startX: startX, lvlCols: lvlCols, pos: pos, totalW: cx + TT_PAD, totalH: TT_PAD + TT_HDR + maxRow * TT_ROW + TT_PAD };
  }

  var RC = 216;
  var RR = 88;
  var RNW = 194;
  var RNH = 74;
  var RPAD = 20;

  function buildRegimeLayout() {
    var R_BRACKETS = window.R_BRACKETS;
    var pos = {};
    var b, id, ri;
    for (b = 0; b < R_BRACKETS.length; b++) {
      for (ri = 0; ri < R_BRACKETS[b].ids.length; ri++) {
        id = R_BRACKETS[b].ids[ri];
        pos[id] = { col: R_BRACKETS[b].col, row: ri };
      }
    }
    var totalW = RPAD * 2 + R_BRACKETS.length * (RC + 14);
    var totalH = RPAD * 2 + 52 + 4 * (RR + 8) + 20;
    return { pos: pos, totalW: totalW, totalH: totalH };
  }

  window.TT_COL = TT_COL;
  window.TT_ROW = TT_ROW;
  window.TT_NW = TT_NW;
  window.TT_NH = TT_NH;
  window.TT_PAD = TT_PAD;
  window.TT_HDR = TT_HDR;
  window.RC = RC;
  window.RR = RR;
  window.RNW = RNW;
  window.RNH = RNH;
  window.RPAD = RPAD;
  window.buildTechLayout = buildTechLayout;
  window.buildRegimeLayout = buildRegimeLayout;
})();
