## Derived habitability category from the 0-10 habitability score.
## Used for display and narrative purposes.
class_name HabitabilityCategory
extends RefCounted


## Habitability categories derived from score.
enum Category {
	IMPOSSIBLE, ## Score 0: Cannot support any human life
	HOSTILE, ## Score 1-2: Requires full life support
	HARSH, ## Score 3-4: Significant infrastructure needed
	MARGINAL, ## Score 5-6: Difficult but sustainable
	CHALLENGING, ## Score 7: Requires adaptation/technology
	COMFORTABLE, ## Score 8-9: Minor challenges only
	IDEAL, ## Score 10: Earth-equivalent or better
}


## Converts a habitability score (0-10) to a category.
## @param score: The habitability score (0-10).
## @return: The derived category.
static func from_score(score: int) -> Category:
	score = clampi(score, 0, 10)
	match score:
		0:
			return Category.IMPOSSIBLE
		1, 2:
			return Category.HOSTILE
		3, 4:
			return Category.HARSH
		5, 6:
			return Category.MARGINAL
		7:
			return Category.CHALLENGING
		8, 9:
			return Category.COMFORTABLE
		10:
			return Category.IDEAL
		_:
			return Category.IMPOSSIBLE


## Converts a category to a display string.
## @param category: The category enum value.
## @return: Human-readable string.
static func to_string_name(category: Category) -> String:
	match category:
		Category.IMPOSSIBLE:
			return "Impossible"
		Category.HOSTILE:
			return "Hostile"
		Category.HARSH:
			return "Harsh"
		Category.MARGINAL:
			return "Marginal"
		Category.CHALLENGING:
			return "Challenging"
		Category.COMFORTABLE:
			return "Comfortable"
		Category.IDEAL:
			return "Ideal"
		_:
			return "Unknown"


## Converts a string to a category.
## @param name: The string name (case-insensitive).
## @return: The category, or IMPOSSIBLE if not found.
static func from_string(name: String) -> Category:
	match name.to_lower():
		"impossible":
			return Category.IMPOSSIBLE
		"hostile":
			return Category.HOSTILE
		"harsh":
			return Category.HARSH
		"marginal":
			return Category.MARGINAL
		"challenging":
			return Category.CHALLENGING
		"comfortable":
			return Category.COMFORTABLE
		"ideal":
			return Category.IDEAL
		_:
			return Category.IMPOSSIBLE


## Returns a short description of what the category means.
## @param category: The category to describe.
## @return: A brief description string.
static func get_description(category: Category) -> String:
	match category:
		Category.IMPOSSIBLE:
			return "Cannot support human life under any circumstances"
		Category.HOSTILE:
			return "Requires full life support systems at all times"
		Category.HARSH:
			return "Significant infrastructure and protection needed"
		Category.MARGINAL:
			return "Difficult conditions but long-term habitation possible"
		Category.CHALLENGING:
			return "Requires technological adaptation but livable"
		Category.COMFORTABLE:
			return "Minor environmental challenges only"
		Category.IDEAL:
			return "Earth-equivalent or superior conditions"
		_:
			return "Unknown habitability"


## Returns whether the category allows unassisted human survival.
## @param category: The category to check.
## @return: True if humans can survive briefly without full life support.
static func allows_unassisted_survival(category: Category) -> bool:
	match category:
		Category.MARGINAL, Category.CHALLENGING, Category.COMFORTABLE, Category.IDEAL:
			return true
		_:
			return false


## Returns the number of categories.
## @return: Count of category enum values.
static func count() -> int:
	return 7
