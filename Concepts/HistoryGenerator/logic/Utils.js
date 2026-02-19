/** Random and math helpers for the history generator. */
function rand(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}
function pick(arr) {
  return arr[rand(0, arr.length - 1)];
}
function pickN(arr, n) {
  var copy = arr.slice();
  copy.sort(function () { return Math.random() - 0.5; });
  return copy.slice(0, n);
}
function clamp(v, min, max) {
  return Math.max(min, Math.min(max, v));
}
window.rand = rand;
window.pick = pick;
window.pickN = pickN;
window.clamp = clamp;
