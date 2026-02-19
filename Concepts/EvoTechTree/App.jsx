/** Evo Tech Tree — biology as evolutionary technology tree. Version A. */
const { useState, useCallback, useRef, useEffect, useMemo } = React;

// ─── CATEGORIES ───────────────────────────────────────────────────────────────
const CAT = {
  CELLULAR:      { label:"Cellular",       color:"#4ade80", icon:"🦠" },
  BODYPLAN:      { label:"Body Plan",      color:"#60a5fa", icon:"🧬" },
  LOCOMOTION:    { label:"Locomotion",     color:"#f97316", icon:"🦿" },
  DIET:          { label:"Diet",           color:"#facc15", icon:"🍖" },
  SENSES:        { label:"Senses",         color:"#c084fc", icon:"👁️" },
  REPRODUCTION:  { label:"Reproduction",   color:"#f472b6", icon:"🥚" },
  DEFENSE:       { label:"Defense",        color:"#94a3b8", icon:"🛡️" },
  COGNITION:     { label:"Cognition",      color:"#38bdf8", icon:"🧠" },
  METABOLISM:    { label:"Metabolism",     color:"#fb923c", icon:"⚡" },
  INTEGUMENT:    { label:"Integument",     color:"#a78bfa", icon:"🐚" },
  SOCIAL:        { label:"Social",         color:"#34d399", icon:"🐝" },
  COMMUNICATION: { label:"Communication",  color:"#fbbf24", icon:"📡" },
  LIFECYCLE:     { label:"Lifecycle",      color:"#e879f9", icon:"♻️" },
  SYMBIOSIS:     { label:"Symbiosis",      color:"#2dd4bf", icon:"🤝" },
  EXTREMOPHILE:  { label:"Extremophile",   color:"#f87171", icon:"🌋" },
};

const NODES = [
  {id:"prokaryote",cat:"CELLULAR",tier:0,req:[],label:"Prokaryotic Cell",desc:"Single-celled organism lacking a nucleus."},
  {id:"eukaryote",cat:"CELLULAR",tier:1,req:["prokaryote"],label:"Eukaryotic Cell",desc:"Cell with a true nucleus and organelles."},
  {id:"multicellular",cat:"CELLULAR",tier:2,req:["eukaryote"],label:"Multicellularity",desc:"Cells cooperate and specialize into tissues."},
  {id:"cell_diff",cat:"CELLULAR",tier:3,req:["multicellular"],label:"Cell Differentiation",desc:"Cells adopt specialized roles: muscle, nerve, etc."},
  {id:"stem_cells",cat:"CELLULAR",tier:4,req:["cell_diff"],label:"Stem Cells",desc:"Undifferentiated cells enabling regeneration."},
  {id:"radial_sym",cat:"BODYPLAN",tier:2,req:["multicellular"],label:"Radial Symmetry",desc:"Body around a central axis."},
  {id:"bilateral_sym",cat:"BODYPLAN",tier:2,req:["multicellular"],label:"Bilateral Symmetry",desc:"Left-right symmetry enabling directional movement."},
  {id:"asymmetry",cat:"BODYPLAN",tier:2,req:["multicellular"],label:"Asymmetry",desc:"No symmetry axis. Common in sponges."},
  {id:"coelom",cat:"BODYPLAN",tier:3,req:["bilateral_sym"],label:"True Coelom",desc:"Fluid-filled body cavity enabling organ complexity."},
  {id:"segmentation",cat:"BODYPLAN",tier:3,req:["bilateral_sym"],label:"Segmentation",desc:"Body divided into repeating units."},
  {id:"hydrostatic_skel",cat:"BODYPLAN",tier:3,req:["coelom"],label:"Hydrostatic Skeleton",desc:"Fluid pressure for structural support."},
  {id:"endoskeleton",cat:"BODYPLAN",tier:4,req:["coelom"],label:"Endoskeleton",desc:"Internal skeleton for support."},
  {id:"exoskeleton",cat:"BODYPLAN",tier:4,req:["coelom"],label:"Exoskeleton",desc:"External hard cuticle."},
  {id:"tentacles",cat:"BODYPLAN",tier:4,req:["hydrostatic_skel","radial_sym"],label:"Tentacles",desc:"Flexible appendages for prey capture."},
  {id:"notochord",cat:"BODYPLAN",tier:4,req:["endoskeleton"],label:"Notochord",desc:"Stiff rod; precursor to vertebral column."},
  {id:"vertebral_col",cat:"BODYPLAN",tier:5,req:["notochord"],label:"Vertebral Column",desc:"Jointed backbone protecting spinal cord."},
  {id:"limbs",cat:"BODYPLAN",tier:6,req:["vertebral_col","segmentation"],label:"Limbs / Appendages",desc:"Extensions for locomotion or manipulation."},
  {id:"wings_morph",cat:"BODYPLAN",tier:7,req:["limbs"],label:"Wing Morphology",desc:"Modified limbs adapted for flight."},
  {id:"flagella",cat:"LOCOMOTION",tier:1,req:["prokaryote"],label:"Flagella / Cilia",desc:"Whip-like appendages for fluid propulsion."},
  {id:"amoeboid",cat:"LOCOMOTION",tier:2,req:["eukaryote"],label:"Amoeboid Movement",desc:"Pseudopod extension and cytoplasm flow."},
  {id:"peristalsis",cat:"LOCOMOTION",tier:3,req:["bilateral_sym"],label:"Peristalsis / Crawling",desc:"Muscular wave contraction for crawling."},
  {id:"jet_propulsion",cat:"LOCOMOTION",tier:4,req:["hydrostatic_skel"],label:"Jet Propulsion",desc:"Forceful fluid expulsion for rapid movement."},
  {id:"undulation",cat:"LOCOMOTION",tier:5,req:["bilateral_sym","endoskeleton"],label:"Undulation / Swimming",desc:"Body wave propagation for aquatic locomotion."},
  {id:"walking",cat:"LOCOMOTION",tier:6,req:["limbs"],label:"Walking / Running",desc:"Legged terrestrial locomotion."},
  {id:"burrowing",cat:"LOCOMOTION",tier:6,req:["limbs","exoskeleton"],label:"Burrowing",desc:"Digging into substrates."},
  {id:"climbing",cat:"LOCOMOTION",tier:7,req:["walking"],label:"Climbing / Arboreal",desc:"Locomotion through vertical environments."},
  {id:"gliding",cat:"LOCOMOTION",tier:7,req:["limbs"],label:"Gliding",desc:"Passive aerial descent via membranes."},
  {id:"powered_flight",cat:"LOCOMOTION",tier:8,req:["gliding","wings_morph"],label:"Powered Flight",desc:"Active generation of lift and thrust."},
  {id:"chemosynthesis",cat:"DIET",tier:0,req:[],label:"Chemosynthesis",desc:"Energy from inorganic chemical reactions."},
  {id:"photosynthesis",cat:"DIET",tier:1,req:["prokaryote"],label:"Photosynthesis",desc:"Converting sunlight to chemical energy."},
  {id:"filter_feeding",cat:"DIET",tier:3,req:["multicellular"],label:"Filter Feeding",desc:"Straining microorganisms from water."},
  {id:"herbivory",cat:"DIET",tier:4,req:["cell_diff"],label:"Herbivory",desc:"Consuming plant matter."},
  {id:"carnivory",cat:"DIET",tier:4,req:["cell_diff"],label:"Carnivory",desc:"Consuming animal prey."},
  {id:"detritivory",cat:"DIET",tier:4,req:["cell_diff"],label:"Detritivory",desc:"Consuming decaying organic matter."},
  {id:"omnivory",cat:"DIET",tier:5,req:["herbivory","carnivory"],label:"Omnivory",desc:"Flexible diet of plant and animal matter."},
  {id:"parasitism_diet",cat:"DIET",tier:5,req:["carnivory"],label:"Parasitic Feeding",desc:"Feeding on host without killing it."},
  {id:"fluid_feeding",cat:"DIET",tier:5,req:["carnivory","herbivory"],label:"Fluid Feeding",desc:"Consuming blood, nectar, or sap."},
  {id:"external_dig",cat:"DIET",tier:5,req:["carnivory"],label:"External Digestion",desc:"Secreting enzymes onto prey before ingestion."},
  {id:"venom_diet",cat:"DIET",tier:6,req:["carnivory","venom_synthesis"],label:"Venom (Prey Capture)",desc:"Injecting toxins to subdue prey."},
  {id:"chemoreception",cat:"SENSES",tier:1,req:["prokaryote"],label:"Chemoreception",desc:"Detection of chemical gradients."},
  {id:"mechanoreception",cat:"SENSES",tier:2,req:["eukaryote"],label:"Mechanoreception",desc:"Sensing vibration, pressure, touch."},
  {id:"photoreception",cat:"SENSES",tier:2,req:["eukaryote"],label:"Photoreception",desc:"Detecting light intensity."},
  {id:"thermoreception",cat:"SENSES",tier:3,req:["mechanoreception"],label:"Thermoreception",desc:"Detecting temperature gradients."},
  {id:"simple_eye",cat:"SENSES",tier:4,req:["photoreception","cell_diff"],label:"Simple Eye (Ocellus)",desc:"Light/dark sensing pit."},
  {id:"electroreception",cat:"SENSES",tier:4,req:["mechanoreception"],label:"Electroreception",desc:"Detecting electric fields."},
  {id:"compound_eye",cat:"SENSES",tier:5,req:["simple_eye","exoskeleton"],label:"Compound Eye",desc:"Mosaic vision from thousands of ommatidia."},
  {id:"camera_eye",cat:"SENSES",tier:5,req:["simple_eye","endoskeleton"],label:"Camera Eye",desc:"Single-lens high-resolution image formation."},
  {id:"lateral_line",cat:"SENSES",tier:5,req:["mechanoreception","undulation"],label:"Lateral Line System",desc:"Aquatic pressure-wave detection."},
  {id:"magnetoreception",cat:"SENSES",tier:5,req:["electroreception","chemoreception"],label:"Magnetoreception",desc:"Sensing Earth's magnetic field."},
  {id:"color_vision",cat:"SENSES",tier:6,req:["camera_eye","compound_eye"],label:"Color Vision",desc:"Multiple photoreceptor types."},
  {id:"echolocation",cat:"SENSES",tier:6,req:["mechanoreception"],label:"Echolocation",desc:"Active sonar using self-generated sound."},
  {id:"uv_vision",cat:"SENSES",tier:7,req:["color_vision"],label:"UV / Infrared Vision",desc:"Seeing beyond human-visible spectrum."},
  {id:"binary_fission",cat:"REPRODUCTION",tier:0,req:[],label:"Binary Fission",desc:"Asexual splitting into two daughter cells."},
  {id:"budding",cat:"REPRODUCTION",tier:2,req:["multicellular"],label:"Budding / Fragmentation",desc:"Asexual reproduction via outgrowths."},
  {id:"sexual_repro",cat:"REPRODUCTION",tier:2,req:["eukaryote"],label:"Sexual Reproduction",desc:"Genetic mixing via gametes."},
  {id:"external_fert",cat:"REPRODUCTION",tier:3,req:["sexual_repro"],label:"External Fertilization",desc:"Gametes released into environment."},
  {id:"internal_fert",cat:"REPRODUCTION",tier:4,req:["sexual_repro"],label:"Internal Fertilization",desc:"Fertilization inside the body."},
  {id:"parthenogenesis",cat:"REPRODUCTION",tier:4,req:["sexual_repro"],label:"Parthenogenesis",desc:"Offspring from unfertilized eggs."},
  {id:"oviparity",cat:"REPRODUCTION",tier:5,req:["internal_fert","external_fert"],label:"Oviparity (Egg-laying)",desc:"Offspring develop in external eggs."},
  {id:"ovoviviparity",cat:"REPRODUCTION",tier:5,req:["internal_fert"],label:"Ovoviviparity",desc:"Eggs retained internally until hatching."},
  {id:"viviparity",cat:"REPRODUCTION",tier:5,req:["internal_fert"],label:"Viviparity (Live Birth)",desc:"Offspring develop internally."},
  {id:"parental_care",cat:"REPRODUCTION",tier:6,req:["oviparity","viviparity"],label:"Parental Care",desc:"Active investment in offspring survival."},
  {id:"placenta",cat:"REPRODUCTION",tier:6,req:["viviparity"],label:"Placenta",desc:"Organ for nutrient exchange mother↔fetus."},
  {id:"eusociality_repro",cat:"REPRODUCTION",tier:7,req:["parental_care"],label:"Repro. Division of Labor",desc:"Only some individuals reproduce."},
  {id:"spore_formation",cat:"LIFECYCLE",tier:2,req:["eukaryote"],label:"Spore / Cyst Formation",desc:"Dormant stages for dispersal."},
  {id:"senescence",cat:"LIFECYCLE",tier:3,req:["sexual_repro"],label:"Senescence (Aging)",desc:"Programmed decline."},
  {id:"alternation_gen",cat:"LIFECYCLE",tier:4,req:["sexual_repro","spore_formation"],label:"Alternation of Generations",desc:"Alternating sexual and asexual phases."},
  {id:"colonial_organism",cat:"LIFECYCLE",tier:4,req:["budding","cell_diff"],label:"Colonial Organism",desc:"Physically connected individuals."},
  {id:"metamorphosis",cat:"LIFECYCLE",tier:5,req:["oviparity","cell_diff"],label:"Metamorphosis",desc:"Transformation through larval stages."},
  {id:"holometabolism",cat:"LIFECYCLE",tier:6,req:["metamorphosis"],label:"Complete Metamorphosis",desc:"Egg→Larva→Pupa→Adult."},
  {id:"negligible_sen",cat:"LIFECYCLE",tier:7,req:["regeneration","stem_cells"],label:"Negligible Senescence",desc:"No measurable mortality increase with age."},
  {id:"camouflage",cat:"DEFENSE",tier:3,req:["cell_diff"],label:"Camouflage",desc:"Blending into background via color or pattern."},
  {id:"aposematism",cat:"DEFENSE",tier:4,req:["camouflage"],label:"Aposematism",desc:"Warning coloration advertising toxicity."},
  {id:"armor_plates",cat:"DEFENSE",tier:4,req:["exoskeleton","endoskeleton"],label:"Armor / Plates",desc:"Bony plates for physical protection."},
  {id:"ink_ejection",cat:"DEFENSE",tier:4,req:["jet_propulsion","chemoreception"],label:"Ink / Chemical Ejection",desc:"Releasing ink or chemicals to confuse threats."},
  {id:"spines_quills",cat:"DEFENSE",tier:4,req:["exoskeleton","integument_scales"],label:"Spines / Quills",desc:"Sharp projections deterring attack."},
  {id:"bioluminescence",cat:"DEFENSE",tier:5,req:["cell_diff","aerobic_metab"],label:"Bioluminescence",desc:"Light production for startling predators or luring prey."},
  {id:"venom_defense",cat:"DEFENSE",tier:5,req:["venom_synthesis"],label:"Venom (Defense)",desc:"Toxin delivery for predator deterrence."},
  {id:"autotomy",cat:"DEFENSE",tier:5,req:["limbs","stem_cells"],label:"Autotomy",desc:"Voluntary detachment of body parts."},
  {id:"regeneration",cat:"DEFENSE",tier:5,req:["stem_cells"],label:"Regeneration",desc:"Re-growing lost limbs or organs."},
  {id:"mimicry",cat:"DEFENSE",tier:6,req:["camouflage","color_vision"],label:"Mimicry",desc:"Resembling dangerous species for protection."},
  {id:"anaerobic_metab",cat:"METABOLISM",tier:0,req:[],label:"Anaerobic Metabolism",desc:"Energy production without oxygen."},
  {id:"aerobic_metab",cat:"METABOLISM",tier:1,req:["prokaryote"],label:"Aerobic Metabolism",desc:"Oxygen-based respiration. More efficient."},
  {id:"nitrogen_fix",cat:"METABOLISM",tier:1,req:["prokaryote"],label:"Nitrogen Fixation",desc:"Converting atmospheric N₂ to ammonia."},
  {id:"ectothermy",cat:"METABOLISM",tier:3,req:["aerobic_metab"],label:"Ectothermy (Cold-blooded)",desc:"Body temp regulated by environment."},
  {id:"desiccation_res",cat:"METABOLISM",tier:4,req:["aerobic_metab"],label:"Desiccation Resistance",desc:"Surviving extreme water loss."},
  {id:"venom_synthesis",cat:"METABOLISM",tier:5,req:["aerobic_metab","cell_diff"],label:"Venom Synthesis",desc:"Specialized glands producing toxic compounds."},
  {id:"endothermy",cat:"METABOLISM",tier:6,req:["aerobic_metab","vertebral_col"],label:"Endothermy (Warm-blooded)",desc:"Internal heat generation."},
  {id:"electric_organ",cat:"METABOLISM",tier:6,req:["electroreception","ectothermy"],label:"Electric Organ",desc:"Modified muscle generating electric fields."},
  {id:"hibernation",cat:"METABOLISM",tier:7,req:["endothermy","ectothermy"],label:"Torpor / Hibernation",desc:"Metabolic suppression during harsh periods."},
  {id:"cell_membrane",cat:"INTEGUMENT",tier:0,req:[],label:"Cell Membrane",desc:"Phospholipid bilayer separating cell from environment."},
  {id:"cell_wall",cat:"INTEGUMENT",tier:1,req:["prokaryote"],label:"Cell Wall",desc:"Rigid outer layer of chitin or peptidoglycan."},
  {id:"integument_mucus",cat:"INTEGUMENT",tier:2,req:["multicellular"],label:"Mucus / Slime Layer",desc:"Secreted gel for protection and signaling."},
  {id:"integument_skin",cat:"INTEGUMENT",tier:3,req:["cell_diff"],label:"Moist Permeable Skin",desc:"Gas-exchange capable skin."},
  {id:"integument_scales",cat:"INTEGUMENT",tier:5,req:["cell_diff","ectothermy"],label:"Scales",desc:"Overlapping keratinized plates."},
  {id:"chromatophores",cat:"INTEGUMENT",tier:6,req:["cell_diff","camera_eye"],label:"Chromatophores",desc:"Pigment cells enabling rapid color change."},
  {id:"integument_feathers",cat:"INTEGUMENT",tier:7,req:["integument_scales","endothermy"],label:"Feathers",desc:"Complex structures for insulation and flight."},
  {id:"integument_fur",cat:"INTEGUMENT",tier:7,req:["endothermy","integument_scales"],label:"Fur / Hair",desc:"Keratinous filaments for insulation."},
  {id:"neural_net",cat:"COGNITION",tier:3,req:["cell_diff"],label:"Nerve Net",desc:"Diffuse neural network without centralization."},
  {id:"ganglion",cat:"COGNITION",tier:4,req:["neural_net","bilateral_sym"],label:"Ganglia",desc:"Nerve cell clusters for regional processing."},
  {id:"assoc_learning",cat:"COGNITION",tier:5,req:["ganglion"],label:"Associative Learning",desc:"Linking stimuli and outcomes."},
  {id:"brain",cat:"COGNITION",tier:6,req:["ganglion","vertebral_col"],label:"Centralized Brain",desc:"Highly centralized neural processing."},
  {id:"cerebral_cortex",cat:"COGNITION",tier:7,req:["brain"],label:"Cerebral Cortex",desc:"Layered structure enabling complex thought."},
  {id:"episodic_memory",cat:"COGNITION",tier:7,req:["cerebral_cortex"],label:"Episodic Memory",desc:"Recalling specific past events."},
  {id:"tool_use",cat:"COGNITION",tier:8,req:["cerebral_cortex","limbs","assoc_learning"],label:"Tool Use",desc:"Using external objects to accomplish tasks."},
  {id:"theory_of_mind",cat:"COGNITION",tier:9,req:["tool_use","social_living"],label:"Theory of Mind",desc:"Modeling mental states of others."},
  {id:"abstract_reason",cat:"COGNITION",tier:9,req:["theory_of_mind","cerebral_cortex"],label:"Abstract Reasoning",desc:"Thinking beyond immediate senses."},
  {id:"solitary",cat:"SOCIAL",tier:2,req:["multicellular"],label:"Solitary Lifestyle",desc:"Living and foraging alone."},
  {id:"aggregation",cat:"SOCIAL",tier:3,req:["chemoreception","solitary"],label:"Aggregation",desc:"Passive grouping near resources."},
  {id:"social_living",cat:"SOCIAL",tier:5,req:["aggregation","parental_care"],label:"Social Groups",desc:"Active cooperation in groups."},
  {id:"dominance_hier",cat:"SOCIAL",tier:6,req:["social_living"],label:"Dominance Hierarchy",desc:"Ranked social order for resource access."},
  {id:"cooperative_hunt",cat:"SOCIAL",tier:7,req:["social_living","carnivory"],label:"Cooperative Hunting",desc:"Coordinated group prey capture."},
  {id:"altruism",cat:"SOCIAL",tier:7,req:["social_living","assoc_learning"],label:"Altruism / Kin Selection",desc:"Self-sacrifice benefiting genetic relatives."},
  {id:"eusociality",cat:"SOCIAL",tier:8,req:["dominance_hier","eusociality_repro"],label:"Eusociality",desc:"Superorganism colonies with sterile workers."},
  {id:"culture",cat:"SOCIAL",tier:9,req:["tool_use","altruism","comm_complex"],label:"Cultural Transmission",desc:"Non-genetic learning passed between generations."},
  {id:"pheromones",cat:"COMMUNICATION",tier:2,req:["chemoreception"],label:"Pheromone Signaling",desc:"Chemical messages for mating and alarm."},
  {id:"substrate_vib",cat:"COMMUNICATION",tier:3,req:["mechanoreception"],label:"Substrate Vibration",desc:"Communicating through solid surfaces."},
  {id:"acoustic_sig",cat:"COMMUNICATION",tier:5,req:["mechanoreception","aggregation"],label:"Acoustic Signals",desc:"Sound production for territory and mating."},
  {id:"visual_sig",cat:"COMMUNICATION",tier:5,req:["simple_eye","aggregation"],label:"Visual Signals",desc:"Color displays and postures."},
  {id:"electric_comm",cat:"COMMUNICATION",tier:7,req:["electric_organ","electroreception"],label:"Electric Communication",desc:"Electric pulses for species recognition."},
  {id:"comm_complex",cat:"COMMUNICATION",tier:7,req:["acoustic_sig","visual_sig","brain"],label:"Complex Communication",desc:"Multi-modal signals with learned components."},
  {id:"language",cat:"COMMUNICATION",tier:9,req:["comm_complex","abstract_reason"],label:"Symbolic Language",desc:"Grammar-based abstract communication."},
  {id:"endosymbiosis",cat:"SYMBIOSIS",tier:1,req:["prokaryote"],label:"Endosymbiosis",desc:"Internalizing another organism for mutual benefit."},
  {id:"commensalism",cat:"SYMBIOSIS",tier:4,req:["aggregation"],label:"Commensalism",desc:"One benefits; the other is unaffected."},
  {id:"mutualism",cat:"SYMBIOSIS",tier:4,req:["pheromones","aggregation"],label:"Mutualism",desc:"Both species benefit from the interaction."},
  {id:"parasitism_sym",cat:"SYMBIOSIS",tier:5,req:["aggregation","parasitism_diet"],label:"Parasitism (Ecological)",desc:"One benefits at the expense of another."},
  {id:"mycelial_net",cat:"SYMBIOSIS",tier:6,req:["mutualism","nitrogen_fix"],label:"Mycelial / Root Network",desc:"Underground nutrient-sharing networks."},
  {id:"cleaner_sym",cat:"SYMBIOSIS",tier:7,req:["mutualism","social_living"],label:"Cleaner Symbiosis",desc:"Specialized removal of parasites from host."},
  {id:"thermophile",cat:"EXTREMOPHILE",tier:1,req:["prokaryote"],label:"Thermophily",desc:"Thriving in high-temperature environments."},
  {id:"psychrophile",cat:"EXTREMOPHILE",tier:2,req:["prokaryote"],label:"Psychrophily",desc:"Optimal growth near or below freezing."},
  {id:"halophile",cat:"EXTREMOPHILE",tier:2,req:["prokaryote"],label:"Halophily",desc:"Thriving in high-salt environments."},
  {id:"acidophile",cat:"EXTREMOPHILE",tier:2,req:["prokaryote"],label:"Acidophily",desc:"Growth in highly acidic conditions."},
  {id:"radioresistance",cat:"EXTREMOPHILE",tier:3,req:["prokaryote","desiccation_res"],label:"Radiation Resistance",desc:"Surviving high-dose ionizing radiation."},
  {id:"barophile",cat:"EXTREMOPHILE",tier:3,req:["anaerobic_metab","chemosynthesis"],label:"Barophily (Deep-sea)",desc:"Adapted to extreme pressure."},
  {id:"cryptobiosis",cat:"EXTREMOPHILE",tier:6,req:["desiccation_res","spore_formation"],label:"Cryptobiosis",desc:"Suspending all metabolic activity to survive extremes."},
];

const NM = Object.fromEntries(NODES.map(n=>[n.id,n]));
const STARTING = new Set(["prokaryote","binary_fission","anaerobic_metab","chemosynthesis","cell_membrane","chemoreception"]);

const PRESETS = {
  "Primordial Microbe": ["prokaryote","binary_fission","anaerobic_metab","chemosynthesis","cell_membrane","chemoreception","flagella","thermophile"],
  "Deep Sea Invertebrate": ["prokaryote","binary_fission","anaerobic_metab","chemosynthesis","cell_membrane","chemoreception","eukaryote","multicellular","cell_diff","bilateral_sym","coelom","hydrostatic_skel","jet_propulsion","peristalsis","carnivory","mechanoreception","photoreception","simple_eye","sexual_repro","external_fert","camouflage","aerobic_metab","ectothermy","integument_mucus","neural_net","pheromones","barophile","ink_ejection"],
  "Arthropod Predator": ["prokaryote","binary_fission","anaerobic_metab","chemosynthesis","cell_membrane","chemoreception","eukaryote","multicellular","cell_diff","bilateral_sym","coelom","segmentation","exoskeleton","limbs","walking","burrowing","carnivory","venom_diet","venom_synthesis","mechanoreception","photoreception","simple_eye","compound_eye","sexual_repro","internal_fert","oviparity","metamorphosis","aposematism","spines_quills","aerobic_metab","ectothermy","integument_scales","neural_net","ganglion","pheromones","acoustic_sig","aggregation"],
  "Avian Omnivore": ["prokaryote","binary_fission","anaerobic_metab","chemosynthesis","cell_membrane","chemoreception","eukaryote","multicellular","cell_diff","bilateral_sym","coelom","endoskeleton","notochord","vertebral_col","limbs","wings_morph","gliding","powered_flight","herbivory","carnivory","omnivory","mechanoreception","photoreception","simple_eye","camera_eye","color_vision","sexual_repro","internal_fert","oviparity","parental_care","camouflage","aerobic_metab","endothermy","integument_scales","integument_feathers","neural_net","ganglion","brain","assoc_learning","aggregation","social_living","acoustic_sig","visual_sig"],
  "Sapient Mammal": ["prokaryote","binary_fission","anaerobic_metab","chemosynthesis","cell_membrane","chemoreception","eukaryote","multicellular","cell_diff","stem_cells","bilateral_sym","coelom","segmentation","endoskeleton","notochord","vertebral_col","limbs","walking","climbing","herbivory","carnivory","omnivory","mechanoreception","photoreception","simple_eye","camera_eye","color_vision","electroreception","magnetoreception","thermoreception","sexual_repro","internal_fert","viviparity","placenta","parental_care","camouflage","venom_defense","venom_synthesis","regeneration","aerobic_metab","endothermy","desiccation_res","integument_scales","integument_fur","neural_net","ganglion","brain","cerebral_cortex","assoc_learning","episodic_memory","tool_use","theory_of_mind","abstract_reason","solitary","aggregation","social_living","dominance_hier","altruism","cooperative_hunt","pheromones","visual_sig","acoustic_sig","comm_complex","language","endosymbiosis","mutualism"],
};

const ENVS = {
  ocean_deep:    {label:"Deep Ocean",icon:"🌊",desc:"Cold, dark, high pressure",pressures:{predation:3,cold:4,dark:5,pressure:5,drought:0,heat:0,competition:2}},
  ocean_shallow: {label:"Shallow Ocean",icon:"🐠",desc:"Warm, abundant light, high competition",pressures:{predation:4,cold:1,dark:0,pressure:1,drought:0,heat:2,competition:5}},
  freshwater:    {label:"Freshwater",icon:"💧",desc:"Variable, seasonal, moderate predation",pressures:{predation:3,cold:2,dark:1,pressure:0,drought:2,heat:2,competition:3}},
  forest:        {label:"Dense Forest",icon:"🌳",desc:"Abundant food, high vertical complexity",pressures:{predation:4,cold:1,dark:2,pressure:0,drought:0,heat:1,competition:5}},
  grassland:     {label:"Open Grassland",icon:"🌾",desc:"Exposed, fast predators, seasonal drought",pressures:{predation:5,cold:2,dark:0,pressure:0,drought:3,heat:3,competition:3}},
  desert:        {label:"Desert",icon:"🏜️",desc:"Extreme heat, drought, scarce resources",pressures:{predation:2,cold:2,dark:0,pressure:0,drought:5,heat:5,competition:2}},
  arctic:        {label:"Arctic Tundra",icon:"❄️",desc:"Extreme cold, seasonal darkness",pressures:{predation:2,cold:5,dark:3,pressure:0,drought:2,heat:0,competition:2}},
  cave:          {label:"Cave System",icon:"🕳️",desc:"Permanent dark, stable temp, scarce food",pressures:{predation:2,cold:1,dark:5,pressure:1,drought:0,heat:0,competition:2}},
  volcanic:      {label:"Volcanic Vent",icon:"🌋",desc:"Extreme heat, toxic chemicals",pressures:{predation:1,cold:0,dark:5,pressure:3,drought:2,heat:5,competition:1}},
  canopy:        {label:"Forest Canopy",icon:"🐒",desc:"High aerial, seasonal fruit",pressures:{predation:4,cold:1,dark:1,pressure:0,drought:1,heat:2,competition:4}},
  intertidal:    {label:"Intertidal Zone",icon:"🦀",desc:"Periodic exposure, salinity swings",pressures:{predation:4,cold:1,dark:1,pressure:1,drought:3,heat:3,competition:4}},
};

const PRESSURE_TRAITS = {
  predation:   ["camouflage","armor_plates","venom_defense","bioluminescence","autotomy","spines_quills","mimicry","aposematism","ink_ejection","cooperative_hunt"],
  cold:        ["endothermy","hibernation","integument_fur","integument_feathers","psychrophile","desiccation_res"],
  dark:        ["electroreception","echolocation","thermoreception","bioluminescence","mechanoreception","lateral_line","chemoreception"],
  pressure:    ["barophile","endoskeleton","exoskeleton","hydrostatic_skel"],
  drought:     ["desiccation_res","cryptobiosis","burrowing","integument_scales","ectothermy"],
  heat:        ["ectothermy","desiccation_res","thermophile","integument_scales","integument_mucus"],
  competition: ["carnivory","venom_diet","social_living","cooperative_hunt","comm_complex","tool_use","brain","dominance_hier"],
};

const ENV_TRANSITIONS = {
  ocean_deep:["ocean_shallow","volcanic"],ocean_shallow:["freshwater","intertidal","ocean_deep"],
  freshwater:["ocean_shallow","forest","grassland"],forest:["grassland","canopy","freshwater"],
  grassland:["forest","desert","arctic"],desert:["grassland","volcanic"],
  arctic:["grassland","ocean_shallow"],cave:["forest","ocean_deep"],
  volcanic:["ocean_deep","desert"],canopy:["forest","grassland"],intertidal:["ocean_shallow","freshwater"],
};

function canUnlock(id,u){return NM[id].req.every(r=>u.has(r));}
function getUnlockable(u){return new Set(NODES.filter(n=>!u.has(n.id)&&canUnlock(n.id,u)).map(n=>n.id));}
function pick(a){return a[Math.floor(Math.random()*a.length)];}

const MYA_PER_TRAIT = 3.5;

function evolveStep(unlocked, env, mya) {
  const pressures = env.pressures;
  const unlockable = getUnlockable(unlocked);
  const scored = {};
  for (const id of unlockable) {
    let s = Math.random() * 0.15;
    for (const [pt, list] of Object.entries(PRESSURE_TRAITS))
      if (list.includes(id)) s += (pressures[pt]||0) * (0.2 + Math.random()*0.3);
    scored[id] = s;
  }
  const expected = mya / MYA_PER_TRAIT;
  const numTraits = Math.max(0, Math.round(expected + (Math.random()-0.5)*Math.sqrt(expected)*2));
  const sorted = Object.entries(scored).sort((a,b)=>b[1]-a[1]);
  const gained = new Set(unlocked);
  for (let i=0; i<numTraits && i<sorted.length; i++) {
    const id = sorted[i][0];
    if (canUnlock(id, gained)) gained.add(id);
  }
  let chg=true;
  while(chg){chg=false;for(const n of NODES){if(!gained.has(n.id)&&canUnlock(n.id,gained)&&(scored[n.id]||0)>1.2){gained.add(n.id);chg=true;}}}
  return gained;
}

function genSpecies(unlocked, envKey, totalMya) {
  const has = id=>unlocked.has(id);
  const env = ENVS[envKey]||ENVS.ocean_shallow;
  const symmetry = has("bilateral_sym")?"bilateral":has("radial_sym")?"radial":"asymmetrical";
  const skeleton = has("vertebral_col")?"vertebrate":has("endoskeleton")?"endoskeleton":has("exoskeleton")?"exoskeleton":has("hydrostatic_skel")?"hydrostatic":"none";
  const size = pick(["microscopic","tiny","small","medium","large","massive","enormous"]);
  const loco=[];
  if(has("powered_flight"))loco.push("powered flight");
  else if(has("gliding"))loco.push("gliding");
  if(has("climbing"))loco.push("arboreal climbing");
  if(has("walking"))loco.push(pick(["walking","running","leaping"]));
  if(has("undulation"))loco.push("aquatic undulation");
  if(has("jet_propulsion"))loco.push("jet propulsion");
  if(has("peristalsis"))loco.push("peristaltic crawling");
  if(has("burrowing"))loco.push("burrowing");
  if(has("flagella"))loco.push("flagellar swimming");
  if(!loco.length)loco.push("sessile");
  const diet=has("venom_diet")?"venomous predator":has("external_dig")?"external-digestion predator":has("fluid_feeding")?pick(["blood-feeder","nectar-feeder"]):has("omnivory")?"omnivore":has("carnivory")?"carnivore":has("herbivory")?"herbivore":has("filter_feeding")?"filter feeder":has("detritivory")?"detritivore":has("parasitism_diet")?"parasite":has("photosynthesis")?"photosynthesizer":has("chemosynthesis")?"chemosynthesizer":"absorptive feeder";
  const senses=[];
  if(has("uv_vision"))senses.push("UV/IR vision");
  else if(has("color_vision"))senses.push("color vision");
  else if(has("camera_eye"))senses.push("camera eyes");
  else if(has("compound_eye"))senses.push("compound eyes");
  else if(has("simple_eye"))senses.push("simple eyes");
  if(has("echolocation"))senses.push("echolocation");
  if(has("electroreception"))senses.push("electroreception");
  if(has("magnetoreception"))senses.push("magnetoreception");
  if(has("lateral_line"))senses.push("lateral line");
  if(has("thermoreception"))senses.push("heat-sensing pits");
  if(!senses.length)senses.push("basic chemoreception");
  const repro=has("eusociality_repro")?"eusocial division of labor":has("placenta")?"placental viviparity":has("viviparity")?"live birth":has("oviparity")?"egg-laying":has("parthenogenesis")?"parthenogenesis":has("external_fert")?"external fertilization":has("budding")?"budding":"binary fission";
  const care=has("parental_care")?"Significant parental care provided.":"No parental care; offspring independent.";
  const defense=[];
  if(has("venom_defense"))defense.push("venom");
  if(has("mimicry"))defense.push("mimicry");
  else if(has("aposematism"))defense.push("warning coloration");
  else if(has("camouflage"))defense.push("camouflage");
  if(has("armor_plates"))defense.push("armor");
  if(has("spines_quills"))defense.push("spines/quills");
  if(has("autotomy"))defense.push("autotomy");
  if(has("regeneration"))defense.push("regeneration");
  if(has("bioluminescence"))defense.push("bioluminescence");
  if(has("ink_ejection"))defense.push("ink ejection");
  if(!defense.length)defense.push("cryptic behavior");
  const metab=has("endothermy")?"endothermic":has("ectothermy")?"ectothermic":"poikilothermic";
  const extras=[];
  if(has("hibernation"))extras.push("torpor/hibernation");
  if(has("electric_organ"))extras.push("electric organ");
  if(has("cryptobiosis"))extras.push("cryptobiosis");
  if(has("negligible_sen"))extras.push("negligible senescence");
  const integ=has("integument_feathers")?"feathered":has("integument_fur")?"fur-covered":has("integument_scales")?"scaled":has("exoskeleton")?"chitinous exoskeleton":has("integument_skin")?"moist permeable skin":has("integument_mucus")?"mucus-coated":"bare membrane";
  const color=has("chromatophores")?"Dynamic chromatophores enable active color change.":pick(["Cryptically colored to match habitat.","Vividly aposematic.","Iridescent under light.","Countershaded for camouflage."]);
  const cog=has("abstract_reason")?"abstract reasoning & sapient":has("theory_of_mind")?"theory of mind":has("tool_use")?"tool-using":has("episodic_memory")?"episodic memory":has("assoc_learning")?"associative learning":has("brain")?"centralized brain":has("ganglion")?"ganglionic":"diffuse nerve net";
  const social=has("eusociality")?"eusocial colony":has("culture")?"culture-bearing groups":has("cooperative_hunt")?"cooperative hunting packs":has("dominance_hier")?"hierarchical groups":has("social_living")?"loosely social":has("aggregation")?"aggregative":"solitary";
  const comm=[];
  if(has("language"))comm.push("symbolic language");
  if(has("comm_complex"))comm.push("complex multi-modal");
  if(has("electric_comm"))comm.push("electric pulses");
  if(has("acoustic_sig"))comm.push("acoustic");
  if(has("visual_sig"))comm.push("visual displays");
  if(has("substrate_vib"))comm.push("substrate vibration");
  if(has("pheromones"))comm.push("pheromones");
  if(!comm.length)comm.push("none");
  const lifecycle=has("holometabolism")?"holometabolous":has("metamorphosis")?"hemimetabolous":has("alternation_gen")?"alternation of generations":has("colonial_organism")?"colonial":"direct development";
  const longevity=has("negligible_sen")?"potentially indefinite":has("stem_cells")&&has("regeneration")?"extremely long-lived":has("endothermy")?pick(["1–5 yrs","10–30 yrs","50–100 yrs"]):pick(["days–weeks","1–3 yrs","5–20 yrs"]);
  const sym=[];
  if(has("mycelial_net"))sym.push("mycelial network");if(has("cleaner_sym"))sym.push("cleaner symbiosis");
  if(has("mutualism"))sym.push("mutualism");if(has("parasitism_sym"))sym.push("parasitic");
  const extremo=[];
  if(has("cryptobiosis"))extremo.push("cryptobiosis");if(has("thermophile"))extremo.push("thermophily");
  if(has("psychrophile"))extremo.push("psychrophily");if(has("halophile"))extremo.push("halophily");
  if(has("radioresistance"))extremo.push("radiation resistance");if(has("barophile"))extremo.push("barophily");
  const P=["Alio","Brachi","Cryo","Dendro","Echino","Ferox","Hydro","Icthyo","Krypto","Lumino","Myco","Nervo","Osteo","Proto","Rhizo","Spectro","Thero","Umbro","Velo","Xeno"];
  const R=["morphon","brachis","dactyl","ptera","saurus","thrix","cephalon","gnathus","soma","vorax","tactis","phagus","noctis","ferrus","ventor"];
  const sfx=pick(["us","is","ax","on","ia","um"]);
  const genus=pick(P)+pick(R).replace(/us|is|ax|on|ia|um$/,"")+sfx;
  const sp=pick(["magnificus","horridus","mirabilis","elegans","vorax","obscurus","luminescens","crypticus","giganteus","parvulus","rapax","velox","tardus"]);
  return {name:`${genus} ${sp}`,habitat:env.label,size,symmetry,skeleton,locomotion:loco,diet,senses,reproduction:repro,parentalCare:care,defense,metabolism:metab,metabolicExtras:extras,integument:integ,color,cognition:cog,social,communication:comm,lifecycle,longevity,symbiosis:sym,extremophile:extremo,traitCount:unlocked.size,environment:env.label,totalMya:totalMya||0,unlockedSnapshot:[...unlocked]};
}
