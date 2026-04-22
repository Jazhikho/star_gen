# Viewer Scene Ownership Audit

Date: 2026-04-21

Purpose: track the remaining work needed to keep the active viewer stack aligned with the repo's `.tscn`-first, engine-first UI standard.

## Current State

### Galaxy Viewer
- Scene-owned already:
  - top bar layout
  - side panel shell
  - inspector section shells
  - options dialog shell
  - build-local-space dialog shell
  - compact controls panel shell
- Converted in this patch:
  - top-level menu buttons are now scene-owned instead of being created at runtime
  - controls panel now expands upward from a scene-owned header/content stack
- Still code-owned by design:
  - menu popup contents
  - inspector row content
  - local-space preview text
  - route/cache state presentation

### System Viewer
- Scene-owned already:
  - top bar layout
  - side panel shell
  - options dialog shell
  - compact controls panel shell
  - inspector section shells
- Converted in this patch:
  - top-level menu buttons are now scene-owned instead of being created at runtime
  - controls panel now expands upward from a scene-owned header/content stack
  - the old embedded generation/editor stack was removed from the active `SystemViewer.tscn`
  - the old embedded save/load block was removed from the active `SystemViewer.tscn`
  - viewer scripts now treat the spec as studio-owned input instead of building a second parameter editor
- Still violating engine-first expectations:
  - overview and selection rows are still created in code rather than using reusable scene fragments
  - orbit preview entries are still fully controller-built instead of using scene-owned row templates

### Object Viewer
- Scene-owned already:
  - top bar layout
  - side panel shell
  - options dialog shell
  - compact controls panel shell
  - file section shell
  - dedicated `InspectorPanel` node shell
- Converted in this patch:
  - top-level menu buttons are now scene-owned instead of being created at runtime
  - controls panel now expands upward from a scene-owned header/content stack
  - the old embedded generation/editor stack was removed from the active `ObjectViewer.tscn`
  - the duplicate inspector-script attachment on the outer side-panel container was removed, leaving the dedicated `InspectorPanel` node as the active inspector controller
  - inspector navigation buttons, file block, and section shells now live directly and visibly in the active `ObjectViewer.tscn`, with the script reduced to clearing editor placeholders, toggling visibility, and populating dynamic moon buttons and property rows
- Still violating engine-first expectations:
  - inspector property rows, validation messages, and moon-list entries are still created in code because their counts and content are data-driven at runtime
  - file-operation status text is still controller-populated rather than bound through a richer scene-owned template set

## Priority Gaps

### High Priority
1. Remove viewer-side generation editors from System Viewer.
   - Done for the active `SystemViewer.tscn`.
   - Follow-up is limited to inspector row templating, not more generator stripping.
2. Standardize shared viewer menu scenes.
   - Current button shells are scene-owned, but popup structure still lives in code.
   - Next step should be to decide whether popup contents remain controller-owned or move into reusable scene/menu templates.

### Medium Priority
1. Move more inspector row templates into `.tscn`.
   - Current inspectors use scene-owned sections but still assemble row content mostly in code.
   - This is acceptable for dynamic data, but reusable row templates should be scene-owned where possible.
2. Standardize viewer dialog scenes.
   - Galaxy, System, and Object viewers all have near-duplicate options dialog shells.
   - A shared scene or reusable dialog shell would reduce drift.
3. Standardize compact controls panels.
   - The shell is aligned now, but text/content still differs ad hoc by viewer.

### Low Priority
1. Review whether save/load buttons in viewers should stay visible in version `0.9`.
2. Review whether remaining dynamic menu enable/disable logic should be wrapped in a shared helper.

## Recommended Next Refactor Order

1. Shared viewer menu/dialog components: factor repeated scene shells into reusable assets.
2. Inspector row template pass: migrate repeated dynamic rows to reusable `.tscn` fragments where it reduces code without freezing dynamic behavior.
3. System Viewer: convert reusable inspector row/button templates from controller-built helpers into `.tscn` fragments where it reduces runtime mutation.
4. Object Viewer: reduce the remaining controller-owned file-status and dynamic-row glue where a clearer scene-owned binding pattern exists without freezing data-driven sections.
