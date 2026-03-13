# StarGen AI Use Statement

## Purpose

This document defines how AI may be used in the StarGen repository and related project materials.

StarGen is a deterministic procedural generator with scientific-realism goals, provenance requirements, and higher-risk concept areas involving population, civilisation, religion, language, and culture-adjacent modeling. Because of that, AI use in this repo must stay bounded, reviewable, and explicitly human-directed.

The goal is not to ban AI assistance. The goal is to keep authorship, technical judgment, source evaluation, validation, and release approval in human hands.

## Core Principle

StarGen permits AI-assisted work only inside a human-led workflow.

AI may assist with:

- exploration
- drafting
- transformation
- testing
- critique
- formatting
- structured ideation
- refactoring suggestions
- documentation cleanup

AI may not be treated as the final authority for:

- design decisions
- scientific claims
- realism claims
- culture modeling
- civilisation, religion, language, or species framing
- legal or licensing judgments
- release approval
- publication or repository policy decisions

Human judgment remains mandatory at every decisive stage.

## Repository Standard

AI-assisted work may be kept in StarGen only when all of the following are true:

1. A human defined the task, constraints, and success criteria.
2. A human critically reviewed the output rather than accepting it automatically.
3. A human revised, integrated, tested, or otherwise substantively transformed the output.
4. A human accepted responsibility for the final result.
5. AI use is disclosed where disclosure is relevant, appropriate, or reasonably expected.

No AI-assisted output may be shipped, published, or merged on the assumption that the model is authoritative.

## Allowed Uses

### Generally Allowed

AI may be used for:

- code scaffolding
- test drafts
- bug-hunting suggestions
- refactoring suggestions
- formatting and cleanup
- documentation drafts and rewrites
- schema comparison
- summary generation from human-authored notes
- structured brainstorming
- UI copy drafts
- export-format drafting

### Conditionally Allowed

AI may be used with increased human review for:

- realism-profile tuning proposals
- scientific benchmark summaries
- Traveller or other ruleset mapping drafts
- population detail ideas
- station-design alternatives
- narrative examples
- naming alternatives
- example outputs for demos

These uses require substantive human review and revision before they are treated as project material.

## Restricted Uses

AI must not be treated as the primary authority or autonomous producer for:

- deterministic generation philosophy
- scientific grounding or claims of realism
- claims about astrophysics, exoplanet demographics, or benchmark validity without human verification against sources
- legal or licensing conclusions
- final research analysis
- final release notes or product claims without human review
- culture, civilisation, religion, language, or species modeling without explicit human audit
- citations or bibliographic claims that have not been verified by a human
- content that imitates protected settings, distinctive proprietary material, or copyrighted styles

## Human Review Standard

All meaningful AI-assisted outputs in StarGen must pass through this chain:

Human task definition -> AI assistance -> human review -> human revision or integration -> validation -> human approval

For code, validation usually means tests, determinism checks, manual verification, or source review.

For scientific or realism-related material, validation also includes checking the underlying source material in `Sources/` or another human-reviewed reference set.

## Substantive Transformation Rule

AI-assisted output may be incorporated only if it has undergone at least one meaningful human contribution, such as:

- substantial rewriting
- structural reorganization
- integration into a larger human-authored system
- explicit testing and correction against defined criteria
- source verification against cited or referenced materials
- selection among alternatives using human rationale

Minor wording cleanup alone is not sufficient when the AI contribution is substantial.

## Provenance and Recordkeeping

For each significant AI-assisted artifact, maintain a provenance entry in [AI-Provenance-Log.md](./AI-Provenance-Log.md) with:

- date
- tool or model used
- task purpose
- input materials used
- summary of AI contribution
- what the human accepted
- what the human rejected
- what the human changed
- validation method used
- final approver

The record may be brief, but it must be specific enough to demonstrate human direction and review.

## StarGen-Specific Guidance

### Deterministic Generators and Technical Systems

AI may be used aggressively for bounded support tasks in deterministic technical areas such as:

- implementation scaffolds
- helper methods
- serialization plumbing
- refactors
- test infrastructure
- documentation support

Human oversight remains mandatory for:

- RNG flow and determinism
- provenance data shape
- save/load compatibility
- benchmark interpretation
- export correctness
- user-facing claims about realism or scientific grounding

### Scientific Realism and Source Use

StarGen defaults to scientific realism. Any AI-assisted claim about astrophysics, distributions, benchmarks, or realism must be checked by a human against the source-review workflow and project references under `Sources/` and the benchmark/test materials in `Tests/`.

AI may help summarize sources. AI may not replace source reading, source selection, or evidentiary judgment.

### Population, Civilisation, Religion, Language, and Culture-Adjacent Work

StarGen includes or contemplates systems touching civilisation, regime, religion, conlang, and other culture-adjacent concepts. These areas require tighter control.

AI may assist with:

- alternative framings
- edge-case brainstorming
- structured critique
- stress-testing assumptions
- candidate examples for review

Humans must retain direct control over:

- ontology design
- claims of realism or plausibility
- bias review
- analogy boundaries with real-world cultures or religions
- publication and release decisions

No such material should be merged or shipped without explicit human audit.

## Bias, Slop, and Release Review

Before release, AI-assisted outputs should be checked for:

- hallucinated facts
- unverifiable citations
- shallow or generic prose
- contradictions with deterministic or scientific goals
- cultural flattening or stereotype drift
- biological determinism presented as realism
- accidental imitation of protected settings or styles
- polished but unusable code or documentation

Outputs that fail these checks must be revised or discarded.

## Disclosure Standard

StarGen does not treat AI assistance as equivalent to authorship.

Disclosure should be proportional to context. At minimum, disclosure is expected when AI played a meaningful role in:

- public-facing product materials
- research or academic outputs
- contributor-facing repository work
- generated content offered as part of a product
- significant design or documentation artifacts

### Short Repo Disclosure

Use when a concise repository note is appropriate:

> AI-assisted development note: AI tools may be used for bounded support tasks such as code scaffolding, refactoring suggestions, test drafting, documentation support, and structured ideation. Deterministic logic, source verification, scientific claims, integration, validation, final edits, and release approval remain human-led. No AI-assisted output should be merged or shipped without human review and substantive revision, testing, or source verification.

## Contributor Rules

Contributors using AI in StarGen must:

- disclose meaningful AI assistance in contribution notes when relevant
- verify code behavior, determinism, and text accuracy
- verify scientific or source-based claims against human-reviewed materials
- avoid submitting unverifiable citations or licensing claims
- avoid using AI to imitate proprietary settings or copyrighted styles
- add or update provenance entries for significant AI-assisted artifacts

Contributors may not use AI assistance as an excuse for:

- broken code
- fabricated documentation
- shallow or generic writing
- unverified scientific claims
- culturally irresponsible outputs
- licensing contamination

## Review Checklist

Before accepting meaningful AI-assisted material, ask:

- Did a human define the task clearly?
- Was the output reviewed critically rather than accepted by default?
- Was it substantively revised, integrated, tested, or source-checked?
- Does it conflict with StarGen's deterministic, scientific, or documentation standards?
- Does it create licensing, attribution, or provenance gaps?
- Does it introduce bias, stereotype drift, or unsupported realism claims?
- Is final responsibility clearly human?

If any of the final four answers is problematic, do not ship it.

## Internal Summary

AI may assist StarGen, but it may not decide StarGen. Human contributors define goals, verify sources, validate behavior, review outputs, approve releases, and accept responsibility for the final work.
