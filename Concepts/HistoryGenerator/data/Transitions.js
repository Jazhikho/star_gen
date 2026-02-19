/** Allowed regime transitions for the history generator simulation. */
const TRANSITIONS = {
  tribal: ["chiefdom"],
  chiefdom: ["cityState", "feudal", "patrimonial"],
  cityState: ["eliteRepublic", "patrimonial"],
  feudal: ["absolutist", "patrimonial"],
  patrimonial: ["absolutist", "empire"],
  empire: ["absolutist", "failed"],
  absolutist: ["constitutional", "empire", "failed"],
  constitutional: ["eliteRepublic", "absolutist"],
  eliteRepublic: ["democracy", "constitutional"],
  democracy: ["junta", "dictator", "eliteRepublic"],
  oneParty: ["constitutional", "dictator"],
  junta: ["oneParty", "absolutist", "patrimonial"],
  dictator: ["absolutist", "junta", "failed"],
  failed: ["junta", "patrimonial", "tribal"],
};
window.TRANSITIONS_HG = TRANSITIONS;
