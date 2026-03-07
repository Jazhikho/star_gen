# 0.4.0 MVP Scope

This document records the implementation scope selected for the `0.4.0` review cycle. It is a working scope note, not the final release commit or release note.

## Goals

1. Viewers use a panel-aware split layout so the visible 3D area is framed correctly even with the persistent inspector open.
2. Galaxy, system, and object generation expose editable parameters, assumptions, and validation/warning feedback instead of relying on opaque random rolls.

## Included

- Object, system, and galaxy viewers compute framing against the visible render rectangle instead of the full window.
- Object generation exposes presets in the side panel and keeps the detailed lock/regenerate flow in the existing `EditDialog`.
- Object inspector shows effective generation targets from provenance plus post-generation realism warnings/errors.
- Object edit/regenerate now feeds those warnings back live inside `EditDialog`.
- System generation uses a `SolarSystemSpec` editor in the persistent side panel.
- Galaxy generation uses the same `GalaxyConfig` shape in both the welcome screen and the in-viewer inspector.
- Shared generation-parameter metadata and validation warnings/errors back the system and galaxy editors.
- Validation is split into:
  - pre-generation blocking errors and advisory warnings for system and galaxy inputs
  - post-generation body validation for object realism feedback

## Explicitly Excluded

- Freeform structural editing for systems or galaxies
- Manual orbit dragging
- Region editing and direct galaxy placement tools
- A separate realism-profile UX for this release

## Acceptance

- Selected bodies and systems appear visually centered in the usable viewport, not behind the inspector.
- Welcome-screen galaxy settings and in-viewer galaxy settings round-trip through the same `GalaxyConfig`.
- System regeneration is deterministic from edited `SolarSystemSpec` values.
- Object presets expose their assumptions, and object edits show realism warnings without hiding hard validation failures.
- Save/load and cross-view navigation continue to preserve edited state.
