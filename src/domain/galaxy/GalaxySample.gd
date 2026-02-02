## Container for the result of galaxy density sampling.
## Separates bulge and disk populations so the renderer can color them differently.
class_name GalaxySample
extends RefCounted


## Positions of bulge-population stars in parsecs.
var bulge_points: PackedVector3Array = PackedVector3Array()

## Positions of disk-population stars in parsecs.
var disk_points: PackedVector3Array = PackedVector3Array()


## Total number of sampled points.
## @return: Combined count.
func get_total_count() -> int:
	return bulge_points.size() + disk_points.size()
