## Stable FNV-1a hash that does not depend on Godot's internal hash().
## Used for deterministic seed derivation across versions.
class_name StableHash
extends RefCounted


## FNV-1a 32-bit offset basis.
const FNV_OFFSET: int = 2166136261

## FNV-1a 32-bit prime.
const FNV_PRIME: int = 16777619

## 32-bit mask to keep values in unsigned 32-bit range.
const MASK_32: int = 0xFFFFFFFF


## Hashes an array of integers using FNV-1a over their raw bytes.
## @param values: Integers to hash.
## @return: Deterministic 32-bit hash (positive).
static func hash_integers(values: Array[int]) -> int:
	var h: int = FNV_OFFSET
	for value in values:
		# Feed each of the 8 bytes of the 64-bit int
		for i in range(8):
			var byte_val: int = (value >> (i * 8)) & 0xFF
			h = (h ^ byte_val) & MASK_32
			h = (h * FNV_PRIME) & MASK_32
	return h


## Derives a child seed from a parent seed and 3D grid coordinates.
## @param parent_seed: The parent seed value.
## @param coords: Grid coordinates.
## @return: Deterministic child seed.
static func derive_seed(parent_seed: int, coords: Vector3i) -> int:
	var values: Array[int] = [parent_seed, coords.x, coords.y, coords.z]
	return hash_integers(values)


## Derives a child seed from a parent seed and a scalar index.
## @param parent_seed: The parent seed value.
## @param index: Child index.
## @return: Deterministic child seed.
static func derive_seed_indexed(parent_seed: int, index: int) -> int:
	var values: Array[int] = [parent_seed, index]
	return hash_integers(values)
