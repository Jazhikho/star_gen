/**
 * Conlang Generator — Concept 18 prototype.
 * Seeded procedural language: phonology, grammar, concept lexicon, inflection, sentence builder.
 */
var useState = React.useState;
var useCallback = React.useCallback;
var useMemo = React.useMemo;

var Icon = function Icon(_ref) {
  var name = _ref.name,
    size = _ref.size;
  var chars = { RefreshCw: '\u21BB', BookOpen: '\uD83D\uDCDA', Volume2: '\uD83D\uDD0A', Scroll: '\uD83D\uDCDC', Settings: '\u2699', Sparkles: '\u2728', AlignLeft: '\u2261', Shuffle: '\u21C4', Lock: '\uD83D\uDD12' };
  return React.createElement('span', { style: { fontSize: size || 16 }, className: 'inline-block', title: name }, chars[name] || '\u2022');
};

// ── Seeded RNG (Mulberry32) ───────────────────────────────────────────────────
function mulberry32(seed) {
  return function() {
    seed |= 0; seed = seed + 0x6D2B79F5 | 0;
    var t = Math.imul(seed ^ seed >>> 15, 1 | seed);
    t = t + Math.imul(t ^ t >>> 7, 61 | t) ^ t;
    return ((t ^ t >>> 14) >>> 0) / 4294967296;
  };
}

// ── Phoneme pools with labels ─────────────────────────────────────────────────
var ALL_CONSONANTS = [
  {p:'m',cat:'nasal',freq:'common'},{p:'n',cat:'nasal',freq:'common'},{p:'ŋ',cat:'nasal',freq:'uncommon'},{p:'ɲ',cat:'nasal',freq:'rare'},
  {p:'p',cat:'stop',freq:'common'},{p:'t',cat:'stop',freq:'common'},{p:'k',cat:'stop',freq:'common'},{p:'b',cat:'stop',freq:'uncommon'},{p:'d',cat:'stop',freq:'uncommon'},{p:'g',cat:'stop',freq:'uncommon'},{p:'q',cat:'stop',freq:'rare'},{p:'ʔ',cat:'stop',freq:'rare'},
  {p:'f',cat:'fricative',freq:'uncommon'},{p:'v',cat:'fricative',freq:'uncommon'},{p:'s',cat:'fricative',freq:'common'},{p:'z',cat:'fricative',freq:'uncommon'},{p:'ʃ',cat:'fricative',freq:'uncommon'},{p:'ʒ',cat:'fricative',freq:'rare'},{p:'x',cat:'fricative',freq:'rare'},{p:'h',cat:'fricative',freq:'uncommon'},{p:'θ',cat:'fricative',freq:'rare'},{p:'ð',cat:'fricative',freq:'rare'},
  {p:'tʃ',cat:'affricate',freq:'uncommon'},{p:'dʒ',cat:'affricate',freq:'uncommon'},{p:'ts',cat:'affricate',freq:'rare'},
  {p:'l',cat:'liquid',freq:'common'},{p:'r',cat:'liquid',freq:'common'},{p:'ɾ',cat:'liquid',freq:'rare'},{p:'ʎ',cat:'liquid',freq:'rare'},
  {p:'w',cat:'glide',freq:'common'},{p:'j',cat:'glide',freq:'common'}
];
var ALL_VOWELS = [
  {p:'a',cat:'low',freq:'common'},{p:'e',cat:'mid',freq:'common'},{p:'i',cat:'high',freq:'common'},{p:'o',cat:'mid',freq:'common'},{p:'u',cat:'high',freq:'common'},
  {p:'ə',cat:'mid',freq:'uncommon'},{p:'ɛ',cat:'mid',freq:'uncommon'},{p:'ɔ',cat:'mid',freq:'uncommon'},{p:'æ',cat:'low',freq:'uncommon'},{p:'ʊ',cat:'high',freq:'uncommon'},
  {p:'y',cat:'high',freq:'rare'},{p:'ø',cat:'mid',freq:'rare'},{p:'ɯ',cat:'high',freq:'rare'},{p:'ɨ',cat:'high',freq:'rare'},{p:'ã',cat:'nasal',freq:'rare'}
];
var ALL_SYL_PATTERNS = ['V','CV','VC','CVC','CCV','VCC','CVCC','CCVC','CVCCC','CCVCC'];
var WORD_ORDERS = ['SOV','SVO','VSO','VOS','OVS','OSV'];
var MORPH_TYPES = ['Agglutinative','Fusional','Isolating','Polysynthetic'];
var TONES = ['high','low','rising','falling','mid'];
var GENDER_OPTIONS = ['animate','inanimate','celestial','earthly','masculine','feminine','neuter','sacred','profane','human','nonhuman'];
var REPAIR_STRATEGIES = [
  'Repeat last word with rising intonation',
  'Insert filler particle before correction',
  'Restart sentence from beginning',
  'Use correction particle after error',
  'Pause + negation + correct word',
];

// ── Concept groups (unchanged, abbreviated for space) ─────────────────────────
var CONCEPT_GROUPS = [
  {cat:'nature', links:[['sun','light','day','bright','warmth'],['moon','night','dark','tide','cycle'],['star','sky','heaven','far','guide'],['water','river','rain','sea','wet'],['fire','heat','burn','ash','smoke'],['earth','soil','ground','mud','clay'],['wind','air','breath','storm','blow'],['tree','wood','forest','root','branch'],['stone','rock','cliff','hard','mountain'],['cloud','mist','fog','gray','soft'],['snow','ice','cold','freeze','winter'],['flower','seed','pollen','spring','bloom'],['grass','plain','meadow','green','low'],['desert','sand','dry','thirst','dune'],['island','shore','coast','wave','salt']]},
  {cat:'body', links:[['head','skull','brain','thought','top'],['eye','see','sight','gaze','watch'],['ear','hear','sound','listen','noise'],['nose','smell','scent','breath','sniff'],['mouth','speak','taste','lip','tongue'],['hand','hold','touch','finger','grip'],['foot','walk','step','path','run'],['heart','feel','emotion','beat','center'],['blood','wound','life','red','flow'],['bone','hard','support','death','white'],['skin','surface','outside','cover','soft'],['hair','head','grow','color','fine'],['back','carry','behind','support','strong'],['belly','hunger','inside','digest','round'],['arm','reach','throw','push','pull'],['leg','stand','kick','long','strong']]},
  {cat:'beings', links:[['person','people','crowd','community','individual'],['man','male','husband','father','son'],['woman','female','wife','mother','daughter'],['child','young','play','grow','innocent'],['elder','old','wisdom','respect','ancestor'],['stranger','foreign','unknown','distant','other'],['enemy','war','hate','oppose','threat'],['friend','trust','bond','help','close'],['leader','power','decide','rule','authority'],['warrior','fight','brave','weapon','defend'],['healer','cure','herb','help','mend'],['merchant','trade','travel','profit','goods'],['animal','beast','wild','nature','instinct'],['bird','fly','feather','song','free'],['fish','swim','river','silent','cold'],['wolf','hunt','pack','howl','fierce'],['horse','ride','fast','strong','loyal'],['spirit','ghost','soul','death','unseen'],['god','divine','worship','power','eternal'],['demon','evil','chaos','fear','dark']]},
  {cat:'actions', links:[['go','move','travel','away','direction'],['come','arrive','return','toward','here'],['see','look','watch','observe','notice'],['hear','listen','sound','perceive','attend'],['speak','say','tell','word','voice'],['ask','question','wonder','seek','request'],['eat','food','hunger','consume','taste'],['drink','water','thirst','swallow','liquid'],['sleep','rest','dream','night','tired'],['wake','rise','alert','morning','begin'],['fight','strike','force','defend','attack'],['flee','escape','fear','run','hide'],['make','create','build','craft','shape'],['destroy','break','ruin','end','force'],['give','offer','share','generous','gift'],['take','receive','seize','grab','own'],['buy','sell','trade','worth','exchange'],['carry','load','heavy','transport','back'],['throw','launch','projectile','aim','release'],['hold','grip','keep','maintain','tight'],['open','reveal','begin','access','unlock'],['close','seal','end','block','shut'],['grow','increase','expand','develop','more'],['shrink','decrease','reduce','less','wither'],['find','discover','search','locate','reveal'],['lose','miss','forget','gone','fail'],['know','understand','wisdom','truth','learn'],['forget','past','lost','memory','fade'],['love','desire','bond','warmth','cherish'],['hate','anger','enemy','oppose','cold'],['fear','danger','flee','small','threat'],['hope','future','wish','dream','forward'],['remember','past','memory','story','keep'],['think','mind','reason','consider','plan'],['decide','choose','will','act','resolve'],['wait','time','still','patient','remain'],['work','effort','labor','purpose','hand'],['rest','stop','peace','recover','still'],['laugh','joy','sound','light','amuse'],['cry','sorrow','tear','pain','grieve']]},
  {cat:'concepts', links:[['good','right','virtue','pure','worthy'],['bad','wrong','vice','corrupt','unworthy'],['true','real','fact','honest','certain'],['false','lie','deceit','illusion','wrong'],['big','large','great','vast','important'],['small','little','minor','weak','insignificant'],['many','count','group','all','more'],['few','rare','scarce','precious','some'],['new','fresh','change','begin','young'],['old','ancient','worn','past','wise'],['fast','quick','rush','urgent','energy'],['slow','careful','patient','still','steady'],['strong','power','force','hard','resist'],['weak','fragile','soft','yield','tired'],['alive','living','breath','motion','warm'],['dead','still','past','cold','gone'],['free','open','will','escape','wild'],['bound','law','duty','hold','closed'],['same','equal','mirror','match','one'],['different','change','other','unique','far'],['near','close','here','short','touch'],['far','distant','other','long','away'],['above','high','sky','over','superior'],['below','low','ground','under','inferior'],['before','past','early','first','front'],['after','future','late','last','behind'],['inside','hidden','contain','within','private'],['outside','open','beyond','exposed','public'],['sacred','holy','divine','pure','ritual'],['profane','taboo','forbidden','corrupt','unclean'],['beautiful','harmony','art','pleasing','whole'],['ugly','discord','broken','displease','flaw'],['rich','wealth','plenty','power','high'],['poor','lack','need','low','struggle'],['hungry','need','empty','eat','weak'],['full','complete','satisfied','eat','enough'],['alone','one','isolated','quiet','self'],['together','group','bond','united','shared'],['lost','wander','unknown','dark','confused'],['found','place','known','safe','clear']]},
  {cat:'society', links:[['name','identity','self','call','known'],['word','speak','meaning','symbol','truth'],['story','past','memory','tell','people'],['law','rule','must','bind','order'],['ritual','sacred','repeat','symbol','community'],['war','enemy','fight','death','power'],['peace','rest','bond','open','end'],['trade','worth','give','take','travel'],['gift','bond','trust','give','sacred'],['debt','owe','bind','take','return'],['oath','promise','bind','truth','witness'],['lie','false','deceive','word','break'],['honor','worth','name','respect','keep'],['shame','disgrace','name','fail','hide'],['family','bond','home','trust','blood'],['clan','group','warrior','protect','territory'],['city','people','wall','trade','center'],['home','shelter','family','safe','warm'],['border','edge','between','territory','guard'],['road','connect','travel','trade','between']]},
  {cat:'time', links:[['now','present','here','moment','exist'],['past','memory','before','gone','old'],['future','hope','change','after','plan'],['day','sun','light','work','cycle'],['night','moon','dark','rest','dream'],['year','cycle','time','change','return'],['season','change','nature','cycle','prepare'],['morning','begin','fresh','light','hope'],['evening','end','rest','dark','reflect'],['moment','fast','short','now','pass'],['long','wait','distance','time','patience'],['always','eternal','never','constant','forever'],['never','not','stop','end','absent']]},
  {cat:'things', links:[['weapon','fight','metal','sharp','hand'],['shield','defend','strong','protect','arm'],['tool','work','make','hand','purpose'],['vessel','hold','liquid','travel','contain'],['cloth','cover','warm','made','soft'],['rope','bind','hold','connect','long'],['door','open','close','between','wall'],['wall','divide','protect','hard','enclose'],['fire','light','warm','cook','center'],['food','eat','give','life','gather'],['coin','trade','worth','metal','small'],['book','word','know','store','past'],['map','place','travel','know','path'],['medicine','heal','herb','pain','help'],['poison','harm','death','liquid','hidden'],['crown','leader','power','symbol','high'],['key','open','secret','hold','small'],['mirror','reflect','true','see','self'],['cage','hold','bound','trap','contain'],['bridge','connect','over','between','path'],['boat','water','travel','wood','move'],['net','catch','fish','trap','weave']]},
  {cat:'space', links:[['place','here','there','where','exist'],['direction','go','toward','away','guide'],['center','middle','balance','important','gather'],['edge','boundary','end','between','limit'],['top','high','above','head','peak'],['bottom','low','below','ground','base'],['left','side','turn','opposite','wrong'],['right','side','turn','correct','dominant'],['front','before','face','toward','first'],['back','behind','past','away','last']]},
];
function buildConceptList() {
  var list = []; var gid = 0;
  CONCEPT_GROUPS.forEach(function(_ref2) {
    var cat = _ref2.cat,
      links = _ref2.links;
    links.forEach(function(cluster, i) {
      var root = cluster[0];
      cluster.forEach(function(gloss, j) {
        list.push({ gloss: gloss, cat: cat, groupId: gid, rootOf: j === 0 ? null : root, isRoot: j === 0 });
      });
      gid++;
    });
  });
  return list;
}
var ALL_CONCEPTS = buildConceptList();
var ADPOSITION_ROLES = ['of','in','at','to','from','with','by','on','under','over','before','after','between','through','against','without','about','because','if','and','or','not','than'];

// ── Utility ───────────────────────────────────────────────────────────────────
var rPick = function rPick(rng, arr) { return arr[Math.floor(rng() * arr.length)]; };
var rPickN = function rPickN(rng, arr, n) { var a = [].concat(arr); for (var i = a.length - 1; i > 0; i--) { var j = Math.floor(rng() * (i + 1)); var _ref3 = [a[j], a[i]]; a[i] = _ref3[0]; a[j] = _ref3[1]; } return a.slice(0, Math.min(n, a.length)); };
var rBool = function rBool(rng, p) { if (p === void 0) p = 0.5; return rng() < p; };
var rInt = function rInt(rng, min, max) { return Math.floor(rng() * (max - min + 1)) + min; };

// ── Default settings factory ──────────────────────────────────────────────────
var defaultSettings = function defaultSettings() {
  return {
    consonants: { mode: 'random', count: 14, selected: [] },
    vowels: { mode: 'random', count: 5, selected: [] },
    syllables: { mode: 'random', selected: [] },
    wordOrder: { mode: 'random', value: 'SVO' },
    morphType: { mode: 'random', value: 'Agglutinative' },
    headDir: { mode: 'random', value: 'head-initial' },
    hasCase: { mode: 'random', value: true },
    hasGender: { mode: 'random', value: true },
    genderClasses: { mode: 'random', count: 2, selected: [] },
    hasTones: { mode: 'random', value: false },
    tones: { mode: 'random', count: 2, selected: [] },
    hasVowelHarmony: { mode: 'random', value: false },
    hasAspect: { mode: 'random', value: true },
    affixPosition: { mode: 'random', value: 'suffix' },
  };
};

// ── Core generator ────────────────────────────────────────────────────────────
function generateLanguage(seed, settings) {
  var rng = mulberry32(seed);
  var s = settings;

  var resolve = function resolve(opt, randomFn) { return opt.mode === 'random' ? randomFn() : opt.value; };
  var resolveList = function resolveList(opt, pool, countFn) {
    if (opt.mode === 'manual' && opt.selected.length > 0) return opt.selected;
    if (opt.mode === 'count') return rPickN(rng, pool, opt.count);
    return rPickN(rng, pool, countFn());
  };

  var consonants = resolveList(s.consonants, ALL_CONSONANTS.map(function(c) { return c.p; }), function() { return rInt(rng, 8, 22); });
  var vowels = resolveList(s.vowels, ALL_VOWELS.map(function(v) { return v.p; }), function() { return rInt(rng, 3, 10); });
  var sylPats = resolveList(s.syllables, ALL_SYL_PATTERNS, function() { return rInt(rng, 3, 6); });

  var syllable = function syllable() { return [].concat(rPick(rng, sylPats)).map(function(c) { return c === 'C' ? rPick(rng, consonants) : rPick(rng, vowels); }).join(''); };
  var word = function word(n) {
    var c = n != null ? n : (rng() < 0.25 ? 1 : rng() < 0.7 ? 2 : 3);
    var w = '';
    for (var i = 0; i < c; i++) w += syllable();
    return w;
  };

  var langName = word(2);
  var capitalName = langName.charAt(0).toUpperCase() + langName.slice(1);

  var wordOrder = resolve(s.wordOrder, function() { return rPick(rng, WORD_ORDERS); });
  var headDir = resolve(s.headDir, function() { return rBool(rng) ? 'head-initial' : 'head-final'; });
  var morphType = resolve(s.morphType, function() { return rPick(rng, MORPH_TYPES); });
  var hasCase = resolve(s.hasCase, function() { return rBool(rng, 0.6); });
  var hasGender = resolve(s.hasGender, function() { return rBool(rng, 0.5); });
  var hasTones = resolve(s.hasTones, function() { return rBool(rng, 0.2); });
  var hasVowelHarmony = resolve(s.hasVowelHarmony, function() { return rBool(rng, 0.25); });
  var hasAspect = resolve(s.hasAspect, function() { return rBool(rng, 0.5); });
  var affixPref = resolve(s.affixPosition, function() { return rPick(rng, ['suffix','prefix','mixed']); });

  var genderClasses = hasGender ? resolveList(s.genderClasses, GENDER_OPTIONS, function() { return rInt(rng, 2, 4); }) : null;
  var tones = hasTones ? resolveList(s.tones, TONES, function() { return rInt(rng, 2, 4); }) : null;

  var affixPos = function affixPos() { return affixPref === 'mixed' ? (rBool(rng) ? 'pre' : 'suf') : (affixPref === 'prefix' ? 'pre' : 'suf'); };
  var affix = function affix(meaning) { return { form: syllable(), position: affixPos(), meaning: meaning }; };

  var tenseAffixes = { past: affix('past'), present: { form: '', position: 'suf', meaning: 'present (unmarked)' }, future: affix('future') };
  if (hasAspect) { tenseAffixes.perfective = affix('perfective'); tenseAffixes.imperfective = affix('imperfective'); }
  var nounAffixes = { plural: affix('plural'), diminutive: affix('diminutive'), augmentative: affix('augmentative') };
  var verbAffixes = { negation: affix('negation'), causative: affix('causative'), passive: affix('passive'), question: affix('question') };
  var caseAffixes = hasCase ? { nominative: { form: '', position: 'suf', meaning: 'nominative' }, accusative: affix('accusative'), genitive: affix('genitive'), dative: affix('dative'), locative: affix('locative'), ablative: affix('ablative') } : null;
  var genderAffixes = genderClasses ? Object.fromEntries(genderClasses.map(function(g) { return [g, affix(g)]; })) : null;

  var repairParticle = syllable();
  var fillerParticle = syllable();
  var repairStrategy = rPick(rng, REPAIR_STRATEGIES);

  var adpositions = {};
  ADPOSITION_ROLES.forEach(function(role) {
    adpositions[role] = { form: word(1), role: role, type: headDir === 'head-initial' ? 'preposition' : 'postposition' };
  });

  var lexicon = {};
  var rootForms = {};
  var maxGroups = Math.max.apply(Math, ALL_CONCEPTS.map(function(c) { return c.groupId; })) + 1;
  for (var g = 0; g < maxGroups; g++) rootForms[g] = word();
  ALL_CONCEPTS.forEach(function(_ref4) {
    var gloss = _ref4.gloss,
      cat = _ref4.cat,
      groupId = _ref4.groupId,
      rootOf = _ref4.rootOf,
      isRoot = _ref4.isRoot;
    var form = isRoot ? rootForms[groupId] : (rBool(rng) ? rootForms[groupId] + syllable() : syllable() + rootForms[groupId]);
    lexicon[gloss] = { form: form, cat: cat, groupId: groupId, derivedFrom: rootOf, isRoot: isRoot };
  });

  var applyAffix = function applyAffix(base, aff) { return (!aff || aff.form === '') ? base : (aff.position === 'pre' ? aff.form + base : base + aff.form); };
  var inflectNoun = function inflectNoun(gloss, opts) {
    if (opts === void 0) opts = {};
    var f = (lexicon[gloss] && lexicon[gloss].form) || gloss;
    if (opts.plural) f = applyAffix(f, nounAffixes.plural);
    if (opts.case && caseAffixes && caseAffixes[opts.case]) f = applyAffix(f, caseAffixes[opts.case]);
    if (opts.gender && genderAffixes && genderAffixes[opts.gender]) f = applyAffix(f, genderAffixes[opts.gender]);
    return f;
  };
  var inflectVerb = function inflectVerb(gloss, opts) {
    if (opts === void 0) opts = {};
    var f = (lexicon[gloss] && lexicon[gloss].form) || gloss;
    if (opts.tense && tenseAffixes[opts.tense] && tenseAffixes[opts.tense].form) f = applyAffix(f, tenseAffixes[opts.tense]);
    if (opts.causative) f = applyAffix(f, verbAffixes.causative);
    if (opts.passive) f = applyAffix(f, verbAffixes.passive);
    if (opts.negated) f = applyAffix(f, verbAffixes.negation);
    if (opts.question) f = applyAffix(f, verbAffixes.question);
    return f;
  };
  var buildSentence = function buildSentence(params) {
    var subj = params.subj,
      verb = params.verb,
      obj = params.obj,
      _params$tense = params.tense,
      tense = _params$tense === void 0 ? 'present' : _params$tense,
      _params$negated = params.negated,
      negated = _params$negated === void 0 ? false : _params$negated,
      _params$question = params.question,
      question = _params$question === void 0 ? false : _params$question,
      withAdp = params.withAdp;
    var sForm = inflectNoun(subj, { case: hasCase ? 'nominative' : null });
    var oForm = inflectNoun(obj, { case: hasCase ? 'accusative' : null });
    var vForm = inflectVerb(verb, { tense: tense, negated: negated, question: question });
    var adpStr = '';
    if (withAdp) {
      var adp = adpositions[withAdp.role];
      var npForm = inflectNoun(withAdp.noun, { case: hasCase ? 'locative' : null });
      adpStr = headDir === 'head-initial' ? ' ' + adp.form + ' ' + npForm : ' ' + npForm + ' ' + adp.form;
    }
    var parts = { S: sForm, O: oForm, V: vForm };
    return [].concat(wordOrder).map(function(c) { return parts[c]; }).join(' ') + adpStr + (question ? '?' : '.');
  };

  return {
    name: capitalName,
    seed: seed,
    phonology: { consonants: consonants, vowels: vowels, sylPats: sylPats },
    grammar: { wordOrder: wordOrder, headDir: headDir, morphType: morphType, hasTones: hasTones, tones: tones, hasCase: hasCase, hasGender: hasGender, hasVowelHarmony: hasVowelHarmony, hasAspect: hasAspect, affixPref: affixPref, repairParticle: repairParticle, repairStrategy: repairStrategy, fillerParticle: fillerParticle },
    affixes: { tense: tenseAffixes, noun: nounAffixes, verb: verbAffixes, case: caseAffixes, gender: genderAffixes },
    adpositions: adpositions,
    lexicon: lexicon,
    genderClasses: genderClasses,
    inflectNoun: inflectNoun,
    inflectVerb: inflectVerb,
    buildSentence: buildSentence,
    word: word,
    syllable: syllable,
  };
}

// ── Demo sentences ────────────────────────────────────────────────────────────
var DEMO_SENTENCES = [
  { subj: 'person', verb: 'see', obj: 'water', tense: 'present', label: 'Simple present' },
  { subj: 'warrior', verb: 'fight', obj: 'enemy', tense: 'past', label: 'Past tense' },
  { subj: 'child', verb: 'eat', obj: 'food', tense: 'future', label: 'Future tense' },
  { subj: 'person', verb: 'find', obj: 'stone', tense: 'present', negated: true, label: 'Negation' },
  { subj: 'elder', verb: 'speak', obj: 'word', tense: 'present', question: true, label: 'Question' },
  { subj: 'healer', verb: 'give', obj: 'medicine', tense: 'past', withAdp: { role: 'to', noun: 'child' }, label: 'With adposition' },
];

// ── UI Components ─────────────────────────────────────────────────────────────
var Tag = function Tag(_ref5) {
  var children = _ref5.children,
    color = _ref5.color,
    onClick = _ref5.onClick,
    active = _ref5.active;
  var cls = { indigo: 'bg-indigo-900 text-indigo-200', purple: 'bg-purple-900 text-purple-200', amber: 'bg-amber-900 text-amber-200', emerald: 'bg-emerald-900 text-emerald-200', rose: 'bg-rose-900 text-rose-200', gray: 'bg-gray-700 text-gray-300' };
  var base = cls[color] || cls.indigo;
  var ring = active ? 'ring-2 ring-white' : '';
  return React.createElement('span', { onClick: onClick, className: 'px-2 py-0.5 rounded text-xs font-mono ' + base + ' ' + ring + (onClick ? ' cursor-pointer hover:opacity-80' : '') }, children);
};
var Card = function Card(_ref6) {
  var title = _ref6.title,
    children = _ref6.children,
    actions = _ref6.actions;
  return React.createElement('div', { className: 'bg-gray-800 rounded-xl p-4 space-y-2' },
    React.createElement('div', { className: 'flex justify-between items-center' },
      React.createElement('h3', { className: 'font-semibold text-indigo-400' }, title),
      actions),
    children);
};
var ModeToggle = function ModeToggle(_ref7) {
  var mode = _ref7.mode,
    setMode = _ref7.setMode,
    _ref7$options = _ref7.options,
    options = _ref7$options === void 0 ? ['random', 'fixed'] : _ref7$options;
  return React.createElement('div', { className: 'flex gap-1' },
    options.map(function(o) {
      return React.createElement('button', {
        key: o,
        onClick: function onClick() { return setMode(o); },
        className: 'px-2 py-0.5 rounded text-xs ' + (mode === o ? 'bg-indigo-600 text-white' : 'bg-gray-700 text-gray-400 hover:bg-gray-600')
      }, o === 'random' ? React.createElement(Icon, { name: 'Shuffle', size: 12 }) : null, o === 'manual' ? React.createElement(Icon, { name: 'Lock', size: 12 }) : null, '\u00A0', o);
    }));
};

var TABS = [['overview', 'BookOpen', 'Overview'], ['phonology', 'Volume2', 'Phonology'], ['grammar', 'Scroll', 'Grammar'], ['lexicon', 'Sparkles', 'Lexicon'], ['sentences', 'AlignLeft', 'Sentences'], ['settings', 'Settings', 'Settings']];

function App() {
  var _useState = useState(42),
    seed = _useState[0],
    setSeed = _useState[1];
  var _useState2 = useState('42'),
    seedInput = _useState2[0],
    setSeedInput = _useState2[1];
  var _useState3 = useState('settings'),
    tab = _useState3[0],
    setTab = _useState3[1];
  var _useState4 = useState(defaultSettings()),
    settings = _useState4[0],
    setSettings = _useState4[1];
  var _useState5 = useState(null),
    lang = _useState5[0],
    setLang = _useState5[1];
  var _useState6 = useState(''),
    lexSearch = _useState6[0],
    setLexSearch = _useState6[1];
  var _useState7 = useState(''),
    wordOf = _useState7[0],
    setWordOf = _useState7[1];
  var _useState8 = useState('warrior'),
    inflectGloss = _useState8[0],
    setInflectGloss = _useState8[1];
  var _useState9 = useState({ plural: false, case: 'nominative', tense: 'present', negated: false }),
    inflectOpts = _useState9[0],
    setInflectOpts = _useState9[1];

  var updateSetting = function updateSetting(path, value) {
    setSettings(function(s) {
      var copy = JSON.parse(JSON.stringify(s));
      var keys = path.split('.');
      var obj = copy;
      for (var i = 0; i < keys.length - 1; i++) obj = obj[keys[i]];
      obj[keys[keys.length - 1]] = value;
      return copy;
    });
  };

  var generate = useCallback(function() {
    var s = parseInt(seedInput, 10) || 42;
    setSeed(s);
    setLang(generateLanguage(s, settings));
    setTab('overview');
  }, [seedInput, settings]);

  var inflected = useMemo(function() {
    if (!lang) return null;
    var g = inflectGloss.trim().toLowerCase();
    return {
      noun: lang.inflectNoun(g, { plural: inflectOpts.plural, case: inflectOpts.case }),
      verb: lang.inflectVerb(g, { tense: inflectOpts.tense, negated: inflectOpts.negated }),
      base: (lang.lexicon[g] && lang.lexicon[g].form) || '(not found)'
    };
  }, [lang, inflectGloss, inflectOpts]);

  var demoSentences = useMemo(function() {
    return lang ? DEMO_SENTENCES.map(function(s) { return Object.assign({}, s, { output: lang.buildSentence(s) }); }) : [];
  }, [lang]);
  var filteredLex = useMemo(function() {
    return lang ? Object.entries(lang.lexicon).filter(function(_ref8) { var g = _ref8[0]; return !lexSearch || g.includes(lexSearch.toLowerCase()); }).sort(function(a, b) { return a[0].localeCompare(b[0]); }) : [];
  }, [lang, lexSearch]);
  var cluster = useMemo(function() {
    if (!lang || !wordOf) return [];
    var entry = lang.lexicon[wordOf.trim().toLowerCase()];
    return entry ? Object.entries(lang.lexicon).filter(function(_ref9) { var v = _ref9[1]; return v.groupId === entry.groupId; }) : [];
  }, [lang, wordOf]);

  var affixRow = function affixRow(label, aff) {
    return aff && React.createElement('div', { key: label, className: 'flex justify-between text-sm bg-gray-700 rounded px-2 py-1' },
      React.createElement('span', { className: 'text-gray-400' }, label),
      React.createElement('span', { className: 'font-mono text-indigo-300' }, aff.form ? (aff.position === 'pre' ? aff.form + '-' : '-' + aff.form) : '(unmarked)'),
      React.createElement('span', { className: 'text-gray-500 text-xs' }, aff.position === 'pre' ? 'prefix' : 'suffix'));
  };

  var SettingsTab = function SettingsTab() {
    var s = settings;
    var togglePhoneme = function togglePhoneme(type, p) {
      var key = type === 'c' ? 'consonants' : 'vowels';
      var sel = s[key].selected;
      var newSel = sel.includes(p) ? sel.filter(function(x) { return x !== p; }) : [].concat(sel, [p]);
      updateSetting(key + '.selected', newSel);
    };
    var toggleSyl = function toggleSyl(p) {
      var sel = s.syllables.selected;
      updateSetting('syllables.selected', sel.includes(p) ? sel.filter(function(x) { return x !== p; }) : [].concat(sel, [p]));
    };
    var toggleGender = function toggleGender(g) {
      var sel = s.genderClasses.selected;
      updateSetting('genderClasses.selected', sel.includes(g) ? sel.filter(function(x) { return x !== g; }) : [].concat(sel, [g]));
    };
    var toggleTone = function toggleTone(t) {
      var sel = s.tones.selected;
      updateSetting('tones.selected', sel.includes(t) ? sel.filter(function(x) { return x !== t; }) : [].concat(sel, [t]));
    };

    return React.createElement('div', { className: 'space-y-4' },
      React.createElement(Card, {
        title: 'Seed',
        actions: React.createElement('button', { onClick: function onClick() { return setSeedInput(String(Math.floor(Math.random() * 99999))); }, className: 'px-2 py-1 bg-gray-700 hover:bg-gray-600 rounded text-xs' }, '\uD83C\uDFB2 Random')
      }, React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Same seed + same settings = same language.'), React.createElement('input', { value: seedInput, onChange: function onChange(e) { return setSeedInput(e.target.value); }, className: 'px-2 py-1 bg-gray-700 rounded text-white font-mono w-full' })),

      React.createElement(Card, {
        title: 'Consonants',
        actions: React.createElement(ModeToggle, { mode: s.consonants.mode, setMode: function setMode(m) { return updateSetting('consonants.mode', m); }, options: ['random', 'count', 'manual'] })
      }, s.consonants.mode === 'count' && React.createElement('div', { className: 'flex items-center gap-2' }, React.createElement('input', { type: 'range', min: 5, max: 25, value: s.consonants.count, onChange: function onChange(e) { return updateSetting('consonants.count', +e.target.value); }, className: 'flex-1' }), React.createElement('span', { className: 'text-white font-mono w-8' }, s.consonants.count)), s.consonants.mode === 'manual' && React.createElement('div', { className: 'space-y-2' }, ['nasal', 'stop', 'fricative', 'affricate', 'liquid', 'glide'].map(function(cat) {
        return React.createElement('div', { key: cat }, React.createElement('p', { className: 'text-gray-500 text-xs mb-1 capitalize' }, cat + 's'), React.createElement('div', { className: 'flex flex-wrap gap-1' }, ALL_CONSONANTS.filter(function(c) { return c.cat === cat; }).map(function(c) {
          return React.createElement(Tag, { key: c.p, color: s.consonants.selected.includes(c.p) ? 'indigo' : 'gray', onClick: function onClick() { return togglePhoneme('c', c.p); }, active: s.consonants.selected.includes(c.p) }, c.p);
        })));
      }), React.createElement('p', { className: 'text-indigo-400 text-xs' }, s.consonants.selected.length + ' selected')), s.consonants.mode === 'random' && React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Will pick 8–22 consonants randomly.')),

      React.createElement(Card, {
        title: 'Vowels',
        actions: React.createElement(ModeToggle, { mode: s.vowels.mode, setMode: function setMode(m) { return updateSetting('vowels.mode', m); }, options: ['random', 'count', 'manual'] })
      }, s.vowels.mode === 'count' && React.createElement('div', { className: 'flex items-center gap-2' }, React.createElement('input', { type: 'range', min: 3, max: 15, value: s.vowels.count, onChange: function onChange(e) { return updateSetting('vowels.count', +e.target.value); }, className: 'flex-1' }), React.createElement('span', { className: 'text-white font-mono w-8' }, s.vowels.count)), s.vowels.mode === 'manual' && React.createElement('div', { className: 'flex flex-wrap gap-1' }, ALL_VOWELS.map(function(v) {
        return React.createElement(Tag, { key: v.p, color: s.vowels.selected.includes(v.p) ? 'indigo' : 'gray', onClick: function onClick() { return togglePhoneme('v', v.p); }, active: s.vowels.selected.includes(v.p) }, v.p);
      })), s.vowels.mode === 'random' && React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Will pick 3–10 vowels randomly.')),

      React.createElement(Card, {
        title: 'Syllable Structures',
        actions: React.createElement(ModeToggle, { mode: s.syllables.mode, setMode: function setMode(m) { return updateSetting('syllables.mode', m); }, options: ['random', 'manual'] })
      }, s.syllables.mode === 'manual' ? React.createElement('div', { className: 'flex flex-wrap gap-1' }, ALL_SYL_PATTERNS.map(function(p) {
        return React.createElement(Tag, { key: p, color: s.syllables.selected.includes(p) ? 'purple' : 'gray', onClick: function onClick() { return toggleSyl(p); }, active: s.syllables.selected.includes(p) }, p);
      })) : React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Will pick 3–6 patterns randomly.')),

      React.createElement(Card, {
        title: 'Word Order',
        actions: React.createElement(ModeToggle, { mode: s.wordOrder.mode, setMode: function setMode(m) { return updateSetting('wordOrder.mode', m); }, options: ['random', 'fixed'] })
      }, s.wordOrder.mode === 'fixed' ? React.createElement('div', { className: 'flex flex-wrap gap-1' }, WORD_ORDERS.map(function(wo) {
        return React.createElement(Tag, { key: wo, color: s.wordOrder.value === wo ? 'indigo' : 'gray', onClick: function onClick() { return updateSetting('wordOrder.value', wo); }, active: s.wordOrder.value === wo }, wo);
      })) : React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Randomly chosen (SOV/SVO most common worldwide).')),

      React.createElement(Card, {
        title: 'Morphological Type',
        actions: React.createElement(ModeToggle, { mode: s.morphType.mode, setMode: function setMode(m) { return updateSetting('morphType.mode', m); }, options: ['random', 'fixed'] })
      }, s.morphType.mode === 'fixed' ? React.createElement('div', { className: 'flex flex-wrap gap-1' }, MORPH_TYPES.map(function(mt) {
        return React.createElement(Tag, { key: mt, color: s.morphType.value === mt ? 'emerald' : 'gray', onClick: function onClick() { return updateSetting('morphType.value', mt); }, active: s.morphType.value === mt }, mt);
      })) : React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Randomly chosen.')),

      React.createElement(Card, {
        title: 'Head Direction (Adpositions)',
        actions: React.createElement(ModeToggle, { mode: s.headDir.mode, setMode: function setMode(m) { return updateSetting('headDir.mode', m); }, options: ['random', 'fixed'] })
      }, s.headDir.mode === 'fixed' ? React.createElement('div', { className: 'flex gap-2' }, ['head-initial', 'head-final'].map(function(h) {
        return React.createElement(Tag, { key: h, color: s.headDir.value === h ? 'purple' : 'gray', onClick: function onClick() { return updateSetting('headDir.value', h); }, active: s.headDir.value === h }, h === 'head-initial' ? 'Prepositions' : 'Postpositions');
      })) : React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Random — determines prep/postpositions.')),

      React.createElement(Card, {
        title: 'Affix Position Preference',
        actions: React.createElement(ModeToggle, { mode: s.affixPosition.mode, setMode: function setMode(m) { return updateSetting('affixPosition.mode', m); }, options: ['random', 'fixed'] })
      }, s.affixPosition.mode === 'fixed' ? React.createElement('div', { className: 'flex gap-2' }, ['suffix', 'prefix', 'mixed'].map(function(a) {
        return React.createElement(Tag, { key: a, color: s.affixPosition.value === a ? 'amber' : 'gray', onClick: function onClick() { return updateSetting('affixPosition.value', a); }, active: s.affixPosition.value === a }, a);
      })) : React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Random — most languages prefer suffixes.')),

      React.createElement(Card, { title: 'Grammar Features' }, React.createElement('div', { className: 'space-y-3' }, [['hasCase', 'Case System'], ['hasGender', 'Grammatical Gender'], ['hasTones', 'Tonal'], ['hasVowelHarmony', 'Vowel Harmony'], ['hasAspect', 'Aspect Marking']].map(function(_ref10) {
        var key = _ref10[0],
          label = _ref10[1];
        return React.createElement('div', { key: key, className: 'flex items-center justify-between' }, React.createElement('span', { className: 'text-sm' }, label), React.createElement('div', { className: 'flex gap-1' }, ['random', 'on', 'off'].map(function(m) {
          return React.createElement('button', {
            key: m,
            onClick: function onClick() {
              updateSetting(key + '.mode', m === 'on' || m === 'off' ? 'fixed' : m);
              if (m === 'on' || m === 'off') updateSetting(key + '.value', m === 'on');
            },
            className: 'px-2 py-0.5 rounded text-xs ' + ((s[key].mode === 'random' && m === 'random') || (s[key].mode === 'fixed' && s[key].value && m === 'on') || (s[key].mode === 'fixed' && !s[key].value && m === 'off') ? 'bg-indigo-600 text-white' : 'bg-gray-700 text-gray-400')
          }, m);
        })));
      }))),

      (s.hasGender.mode === 'random' || (s.hasGender.mode === 'fixed' && s.hasGender.value)) && React.createElement(Card, {
        title: 'Gender Classes',
        actions: React.createElement(ModeToggle, { mode: s.genderClasses.mode, setMode: function setMode(m) { return updateSetting('genderClasses.mode', m); }, options: ['random', 'count', 'manual'] })
      }, s.genderClasses.mode === 'count' && React.createElement('div', { className: 'flex items-center gap-2' }, React.createElement('input', { type: 'range', min: 2, max: 5, value: s.genderClasses.count, onChange: function onChange(e) { return updateSetting('genderClasses.count', +e.target.value); }, className: 'flex-1' }), React.createElement('span', { className: 'text-white font-mono w-8' }, s.genderClasses.count)), s.genderClasses.mode === 'manual' && React.createElement('div', { className: 'flex flex-wrap gap-1' }, GENDER_OPTIONS.map(function(g) {
        return React.createElement(Tag, { key: g, color: s.genderClasses.selected.includes(g) ? 'rose' : 'gray', onClick: function onClick() { return toggleGender(g); }, active: s.genderClasses.selected.includes(g) }, g);
      })), s.genderClasses.mode === 'random' && React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Will pick 2–4 classes randomly.')),

      (s.hasTones.mode === 'random' || (s.hasTones.mode === 'fixed' && s.hasTones.value)) && React.createElement(Card, {
        title: 'Tones',
        actions: React.createElement(ModeToggle, { mode: s.tones.mode, setMode: function setMode(m) { return updateSetting('tones.mode', m); }, options: ['random', 'manual'] })
      }, s.tones.mode === 'manual' ? React.createElement('div', { className: 'flex flex-wrap gap-1' }, TONES.map(function(t) {
        return React.createElement(Tag, { key: t, color: s.tones.selected.includes(t) ? 'amber' : 'gray', onClick: function onClick() { return toggleTone(t); }, active: s.tones.selected.includes(t) }, t);
      })) : React.createElement('p', { className: 'text-gray-500 text-xs' }, 'Will pick 2–4 tones randomly.')),

      React.createElement('button', { onClick: generate, className: 'w-full py-3 bg-indigo-600 hover:bg-indigo-500 rounded-xl text-white font-semibold flex items-center justify-center gap-2' }, React.createElement(Icon, { name: 'RefreshCw', size: 18 }), ' Generate Language'));
  };

  var Overview = function Overview() {
    return lang && React.createElement('div', { className: 'space-y-4' },
      React.createElement('div', { className: 'bg-gradient-to-r from-indigo-900 to-purple-900 rounded-xl p-6' }, React.createElement('div', { className: 'flex justify-between items-start' }, React.createElement('div', null, React.createElement('h2', { className: 'text-3xl font-bold text-white mb-1' }, lang.name), React.createElement('p', { className: 'text-indigo-300 text-sm' }, 'Seed: ' + lang.seed)), React.createElement('div', { className: 'text-right text-sm space-y-1' }, React.createElement('div', null, React.createElement('span', { className: 'text-gray-400' }, 'Order: '), React.createElement('span', { className: 'font-mono text-white' }, lang.grammar.wordOrder)), React.createElement('div', null, React.createElement('span', { className: 'text-gray-400' }, 'Type: '), React.createElement('span', { className: 'text-white' }, lang.grammar.morphType)), React.createElement('div', null, React.createElement('span', { className: 'text-gray-400' }, 'Affixes: '), React.createElement('span', { className: 'text-white' }, lang.grammar.affixPref)))), React.createElement('div', { className: 'grid grid-cols-3 gap-3 mt-4 text-sm' }, [['Consonants', lang.phonology.consonants.length], ['Vowels', lang.phonology.vowels.length], ['Concepts', Object.keys(lang.lexicon).length]].map(function(_ref11) {
        var l = _ref11[0],
          v = _ref11[1];
        return React.createElement('div', { key: l, className: 'bg-black bg-opacity-30 rounded-lg p-2 text-center' }, React.createElement('div', { className: 'text-2xl font-bold text-indigo-300' }, v), React.createElement('div', { className: 'text-gray-400' }, l));
      }))),
      React.createElement(Card, { title: 'Quick Reference' }, React.createElement('div', { className: 'grid grid-cols-2 gap-2 text-sm' }, [['yes', lang.lexicon['true'] && lang.lexicon['true'].form], ['no', lang.inflectVerb('true', { negated: true })], ['I', lang.lexicon['person'] && lang.lexicon['person'].form], ['you', lang.lexicon['stranger'] && lang.lexicon['stranger'].form], ['hello', lang.lexicon['good'] && lang.lexicon['good'].form], ['friend', lang.lexicon['friend'] && lang.lexicon['friend'].form]].map(function(_ref12) {
        var k = _ref12[0],
          v = _ref12[1];
        return React.createElement('div', { key: k, className: 'flex justify-between bg-gray-700 rounded px-2 py-1' }, React.createElement('span', { className: 'text-gray-400' }, k), React.createElement('span', { className: 'font-mono text-white' }, v));
      }))),
      React.createElement(Card, { title: 'Self-Repair Strategy' }, React.createElement('p', { className: 'text-gray-300 text-sm' }, lang.grammar.repairStrategy), React.createElement('div', { className: 'flex gap-2 mt-1 flex-wrap text-sm' }, React.createElement('span', { className: 'text-gray-400' }, 'Correction:'), React.createElement(Tag, null, lang.grammar.repairParticle), React.createElement('span', { className: 'text-gray-400' }, 'Filler:'), React.createElement(Tag, { color: 'purple' }, lang.grammar.fillerParticle))),
      React.createElement(Card, { title: 'Features' }, React.createElement('div', { className: 'flex flex-wrap gap-2' }, lang.grammar.hasVowelHarmony && React.createElement(Tag, { color: 'emerald' }, 'Vowel Harmony'), lang.grammar.hasAspect && React.createElement(Tag, { color: 'emerald' }, 'Aspect'), lang.grammar.hasTones && React.createElement(Tag, { color: 'amber' }, 'Tonal (' + (lang.grammar.tones && lang.grammar.tones.length) + ')'), lang.grammar.hasCase && React.createElement(Tag, { color: 'purple' }, 'Case System'), lang.grammar.hasGender && React.createElement(Tag, { color: 'rose' }, 'Gender (' + (lang.genderClasses && lang.genderClasses.length) + ')'), React.createElement(Tag, null, lang.grammar.headDir === 'head-initial' ? 'Prepositions' : 'Postpositions'))));
  };

  var Phonology = function Phonology() {
    return lang && React.createElement('div', { className: 'space-y-4' }, React.createElement(Card, { title: 'Consonants' }, React.createElement('div', { className: 'flex flex-wrap gap-2' }, lang.phonology.consonants.map(function(c) { return React.createElement('span', { key: c, className: 'px-3 py-1 bg-gray-700 rounded font-mono text-lg' }, c); }))), React.createElement(Card, { title: 'Vowels' }, React.createElement('div', { className: 'flex flex-wrap gap-2' }, lang.phonology.vowels.map(function(v) { return React.createElement('span', { key: v, className: 'px-3 py-1 bg-indigo-900 rounded font-mono text-lg' }, v); }))), React.createElement(Card, { title: 'Syllable Structures' }, React.createElement('div', { className: 'flex flex-wrap gap-2' }, lang.phonology.sylPats.map(function(p) { return React.createElement('span', { key: p, className: 'px-3 py-1 bg-purple-900 rounded font-mono' }, p); })), React.createElement('p', { className: 'text-gray-500 text-xs' }, 'C=consonant V=vowel')), lang.grammar.hasTones && React.createElement(Card, { title: 'Tones' }, React.createElement('div', { className: 'flex flex-wrap gap-2' }, lang.grammar.tones.map(function(t) { return React.createElement('span', { key: t, className: 'px-3 py-1 bg-amber-900 rounded' }, t); }))));
  };

  var Grammar = function Grammar() {
    return lang && React.createElement('div', { className: 'space-y-4' }, React.createElement(Card, { title: 'Word Order: ' + lang.grammar.wordOrder }, React.createElement('p', { className: 'text-gray-400 text-sm' }, { 'SOV': 'Subject-Object-Verb', 'SVO': 'Subject-Verb-Object', 'VSO': 'Verb-Subject-Object', 'VOS': 'Verb-Object-Subject', 'OVS': 'Object-Verb-Subject', 'OSV': 'Object-Subject-Verb' }[lang.grammar.wordOrder])), React.createElement(Card, { title: 'Type: ' + lang.grammar.morphType }, React.createElement('p', { className: 'text-gray-400 text-sm' }, { 'Agglutinative': 'Morphemes stack neatly.', 'Fusional': 'Affixes bundle meanings.', 'Isolating': 'Little inflection.', 'Polysynthetic': 'Word = clause.' }[lang.grammar.morphType])), React.createElement(Card, { title: 'Tense/Aspect' }, Object.entries(lang.affixes.tense).map(function(_ref13) { var k = _ref13[0], v = _ref13[1]; return affixRow(k, v); })), React.createElement(Card, { title: 'Verb Affixes' }, Object.entries(lang.affixes.verb).map(function(_ref14) { var k = _ref14[0], v = _ref14[1]; return affixRow(k, v); })), React.createElement(Card, { title: 'Noun Affixes' }, Object.entries(lang.affixes.noun).map(function(_ref15) { var k = _ref15[0], v = _ref15[1]; return affixRow(k, v); })), lang.affixes.case && React.createElement(Card, { title: 'Cases' }, Object.entries(lang.affixes.case).map(function(_ref16) { var k = _ref16[0], v = _ref16[1]; return affixRow(k, v); })), lang.affixes.gender && React.createElement(Card, { title: 'Genders: ' + lang.genderClasses.join(', ') }, Object.entries(lang.affixes.gender).map(function(_ref17) { var k = _ref17[0], v = _ref17[1]; return affixRow(k, v); })), React.createElement(Card, { title: 'Adpositions' }, React.createElement('div', { className: 'grid grid-cols-2 gap-1 text-sm max-h-48 overflow-y-auto' }, Object.entries(lang.adpositions).map(function(_ref18) { var r = _ref18[0], form = _ref18[1].form; return React.createElement('div', { key: r, className: 'flex justify-between bg-gray-700 rounded px-2 py-1' }, React.createElement('span', { className: 'text-gray-400' }, r), React.createElement('span', { className: 'font-mono text-indigo-300' }, form)); }))), React.createElement(Card, { title: 'Inflection Demo' }, React.createElement('input', { value: inflectGloss, onChange: function onChange(e) { return setInflectGloss(e.target.value); }, placeholder: 'gloss…', className: 'w-full px-2 py-1 bg-gray-700 rounded text-sm text-white mb-2' }), React.createElement('div', { className: 'grid grid-cols-2 gap-2 text-xs mb-2' }, React.createElement('label', { className: 'flex items-center gap-1' }, React.createElement('input', { type: 'checkbox', checked: inflectOpts.plural, onChange: function onChange(e) { return setInflectOpts(function(o) { return Object.assign({}, o, { plural: e.target.checked }); }); } }), 'Plural'), React.createElement('label', { className: 'flex items-center gap-1' }, React.createElement('input', { type: 'checkbox', checked: inflectOpts.negated, onChange: function onChange(e) { return setInflectOpts(function(o) { return Object.assign({}, o, { negated: e.target.checked }); }); } }), 'Negated'), lang.affixes.case && React.createElement('select', { value: inflectOpts.case, onChange: function onChange(e) { return setInflectOpts(function(o) { return Object.assign({}, o, { case: e.target.value }); }); }, className: 'bg-gray-700 rounded px-1' }, Object.keys(lang.affixes.case).map(function(c) { return React.createElement('option', { key: c }, c); })), React.createElement('select', { value: inflectOpts.tense, onChange: function onChange(e) { return setInflectOpts(function(o) { return Object.assign({}, o, { tense: e.target.value }); }); }, className: 'bg-gray-700 rounded px-1' }, Object.keys(lang.affixes.tense).map(function(t) { return React.createElement('option', { key: t }, t); }))), inflected && React.createElement('div', { className: 'space-y-1 text-sm' }, React.createElement('div', null, 'Base: ', React.createElement('span', { className: 'font-mono text-white' }, inflected.base)), React.createElement('div', null, 'Noun: ', React.createElement('span', { className: 'font-mono text-indigo-300' }, inflected.noun)), React.createElement('div', null, 'Verb: ', React.createElement('span', { className: 'font-mono text-purple-300' }, inflected.verb)))));
  };

  var LexiconTab = function LexiconTab() {
    return lang && React.createElement('div', { className: 'space-y-4' }, React.createElement(Card, { title: 'Concept Clusters' }, React.createElement('input', { value: wordOf, onChange: function onChange(e) { return setWordOf(e.target.value); }, placeholder: 'Find cluster for…', className: 'w-full px-2 py-1 bg-gray-700 rounded text-sm text-white mb-2' }), cluster.length > 0 && React.createElement('div', { className: 'grid grid-cols-2 gap-1 text-sm' }, cluster.map(function(_ref19) { var g = _ref19[0], v = _ref19[1]; return React.createElement('div', { key: g, className: (v.isRoot ? 'bg-indigo-900' : 'bg-gray-700') + ' rounded px-2 py-1' }, React.createElement('span', { className: v.isRoot ? 'text-indigo-200' : 'text-gray-400' }, g, v.isRoot ? ' ★' : ''), React.createElement('span', { className: 'font-mono text-white' }, v.form)); }))), React.createElement(Card, { title: 'Lexicon (' + Object.keys(lang.lexicon).length + ')' }, React.createElement('input', { value: lexSearch, onChange: function onChange(e) { return setLexSearch(e.target.value); }, placeholder: 'Filter…', className: 'w-full px-2 py-1 bg-gray-700 rounded text-sm text-white mb-2' }), React.createElement('div', { className: 'grid grid-cols-2 gap-1 text-sm max-h-72 overflow-y-auto' }, filteredLex.map(function(_ref20) { var g = _ref20[0], v = _ref20[1]; return React.createElement('div', { key: g, className: 'flex justify-between bg-gray-700 rounded px-2 py-1 cursor-pointer hover:bg-gray-600', onClick: function onClick() { return setWordOf(g); } }, React.createElement('span', { className: 'text-gray-400' }, g), React.createElement('span', { className: 'font-mono text-white' }, v.form)); })))));
  };

  var SentencesTab = function SentencesTab() {
    var glosses = useMemo(function() { return lang ? Object.keys(lang.lexicon).sort() : []; }, [lang]);
    var _useState10 = useState({ subj: 'warrior', verb: 'see', obj: 'enemy', tense: 'present', negated: false, question: false, useAdp: false, adpRole: 'in', adpNoun: 'water' }),
      cs = _useState10[0],
      setCs = _useState10[1];
    var out = useMemo(function() { return lang ? lang.buildSentence(Object.assign({}, cs, { withAdp: cs.useAdp ? { role: cs.adpRole, noun: cs.adpNoun } : null })) : ''; }, [cs, lang]);
    if (!lang) return null;
    return React.createElement('div', { className: 'space-y-4' }, React.createElement(Card, { title: 'Examples' }, demoSentences.map(function(s) { return React.createElement('div', { key: s.label, className: 'bg-gray-700 rounded p-2 mb-2' }, React.createElement('p', { className: 'text-xs text-gray-500' }, s.label), React.createElement('p', { className: 'font-mono text-white' }, s.output)); })), React.createElement(Card, { title: 'Custom Builder' }, React.createElement('div', { className: 'grid grid-cols-3 gap-2 text-xs mb-2' }, React.createElement('div', null, 'Subj', React.createElement('select', { value: cs.subj, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { subj: e.target.value }); }); }, className: 'w-full bg-gray-700 rounded' }, glosses.map(function(g) { return React.createElement('option', { key: g }, g); }))), React.createElement('div', null, 'Verb', React.createElement('select', { value: cs.verb, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { verb: e.target.value }); }); }, className: 'w-full bg-gray-700 rounded' }, glosses.map(function(g) { return React.createElement('option', { key: g }, g); }))), React.createElement('div', null, 'Obj', React.createElement('select', { value: cs.obj, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { obj: e.target.value }); }); }, className: 'w-full bg-gray-700 rounded' }, glosses.map(function(g) { return React.createElement('option', { key: g }, g); })))), React.createElement('div', { className: 'flex flex-wrap gap-2 text-xs mb-2' }, React.createElement('select', { value: cs.tense, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { tense: e.target.value }); }); }, className: 'bg-gray-700 rounded px-1' }, Object.keys(lang.affixes.tense).map(function(t) { return React.createElement('option', { key: t }, t); })), React.createElement('label', { className: 'flex items-center gap-1' }, React.createElement('input', { type: 'checkbox', checked: cs.negated, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { negated: e.target.checked }); }); } }), 'Neg'), React.createElement('label', { className: 'flex items-center gap-1' }, React.createElement('input', { type: 'checkbox', checked: cs.question, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { question: e.target.checked }); }); } }), '?'), React.createElement('label', { className: 'flex items-center gap-1' }, React.createElement('input', { type: 'checkbox', checked: cs.useAdp, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { useAdp: e.target.checked }); }); } }), 'Adp')), cs.useAdp && React.createElement('div', { className: 'flex gap-2 text-xs mb-2' }, React.createElement('select', { value: cs.adpRole, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { adpRole: e.target.value }); }); }, className: 'bg-gray-700 rounded' }, ADPOSITION_ROLES.map(function(r) { return React.createElement('option', { key: r }, r); })), React.createElement('select', { value: cs.adpNoun, onChange: function onChange(e) { return setCs(function(p) { return Object.assign({}, p, { adpNoun: e.target.value }); }); }, className: 'bg-gray-700 rounded' }, glosses.map(function(g) { return React.createElement('option', { key: g }, g); }))), React.createElement('div', { className: 'bg-gray-900 rounded p-3' }, React.createElement('p', { className: 'font-mono text-xl text-white' }, out))));
  };

  var Empty = function Empty() { return React.createElement('div', { className: 'text-center text-gray-500 py-12' }, 'Generate a language in Settings first.'); };

  return React.createElement('div', { className: 'min-h-screen bg-gray-900 text-white p-4' }, React.createElement('div', { className: 'max-w-2xl mx-auto' }, React.createElement('h1', { className: 'text-2xl font-bold text-center mb-4 bg-gradient-to-r from-indigo-400 to-purple-400 bg-clip-text text-transparent' }, 'Conlang Generator'), React.createElement('div', { className: 'flex gap-1 mb-4 flex-wrap' }, TABS.map(function(_ref21) { var t = _ref21[0], iconName = _ref21[1], label = _ref21[2]; return React.createElement('button', { key: t, onClick: function onClick() { return setTab(t); }, className: 'flex items-center gap-1 px-3 py-2 rounded-lg text-xs font-medium ' + (tab === t ? 'bg-indigo-600 text-white' : 'bg-gray-700 text-gray-300 hover:bg-gray-600') }, React.createElement(Icon, { name: iconName, size: 14 }), label); })), tab === 'overview' && (lang ? React.createElement(Overview, null) : React.createElement(Empty, null)), tab === 'phonology' && (lang ? React.createElement(Phonology, null) : React.createElement(Empty, null)), tab === 'grammar' && (lang ? React.createElement(Grammar, null) : React.createElement(Empty, null)), tab === 'lexicon' && (lang ? React.createElement(LexiconTab, null) : React.createElement(Empty, null)), tab === 'sentences' && (lang ? React.createElement(SentencesTab, null) : React.createElement(Empty, null)), tab === 'settings' && React.createElement(SettingsTab, null)));
}

var root = document.getElementById('root');
if (root) ReactDOM.createRoot(root).render(React.createElement(App, null));
