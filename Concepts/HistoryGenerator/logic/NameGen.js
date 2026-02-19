/** Name and ruler title generation for cultures. */
function genName() {
  var pre = window.pick(window.PREFIXES);
  var mid = window.pick(["a", "e", "i", "o", ""]);
  var suf = window.pick(window.SUFFIXES);
  return pre + mid + suf;
}
function genRuler() {
  var pre = window.pick(window.RULER_PRE);
  var suf = window.pick(window.RULER_SUF);
  var title = window.pick(window.RULER_TITLES);
  return pre + suf + " " + title;
}
window.genName = genName;
window.genRuler = genRuler;
