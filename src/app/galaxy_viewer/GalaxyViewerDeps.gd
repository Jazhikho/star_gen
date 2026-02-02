## Loads galaxy save/load types so class_name is registered before GalaxyViewer parses.
## Preload this from MainApp and from Phase1Deps so GalaxySaveData/GalaxyPersistence are in scope.
extends RefCounted

const _galaxy_save_data := preload("res://src/domain/galaxy/GalaxySaveData.gd")
const _galaxy_persistence := preload("res://src/services/persistence/GalaxyPersistence.gd")
