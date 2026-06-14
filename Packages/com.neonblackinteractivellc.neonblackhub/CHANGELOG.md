# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.8] - 2026-06-14

### Changed
- Pyralis authoring cleanup now distinguishes feature-owned authoring contracts from runtime contract interfaces.
- Runtime gameplay interface seams moved from `Core/Contracts` to `Core/ContractInterfaces` to reduce authoring-contract naming ambiguity.
- Package validation, marker scripts, and handoff documentation now agree on the `0.2.8` package metadata.

## [0.2.0] - 2026-06-11

### Changed
- Pyralis authoring infrastructure is organized around the central `Editor/Authoring` spine and feature-local `Editor/Authoring` guided inspectors.
- Package validation, marker scripts, and handoff documentation now agree on the `0.2.0` package metadata.

## [0.1.2] - 2026-06-02

### Fixed
- Package portability for downstream Unity projects: the current source layout is `Members/Pyralis/Gameplay`; stale legacy runtime/member script copies are not part of shipped package content.
- Authoring and Inspector updates are now protected by package source contracts so older runtime layouts cannot silently reappear in a clean checkout/import.

## [0.1.1] - 2026-05-19

### Changed
- `PlayerActions` (1,199 lines) fully removed and replaced by five focused components: `Motor3D` (coordinator), `Pawn3DInputModule`, `Pawn3DMovementComponent`, `Pawn3DTraversalComponent`, and `Pawn3DPresentationComponent`.
- `FrameInput` struct introduced as the per-frame input snapshot produced by `Pawn3DInputModule` and consumed by all modules.
- `PlayerActionsEditor` (403 lines) removed; each module component exposes its own domain-specific Inspector fields.
- `GrabDetector`, `ClimbZone`, `GameplaySessionBootstrap`, and `PlayerSpawner` updated to reference `Motor3D` directly.
- All doc comments across `Combat/`, `Movement/`, and `Characters/` updated to reflect the new component owners.
- All architecture and setup documentation updated to reflect the current module-based 3D pawn model.

## [0.1.0] - 2026-05-11

### This is the first release of *\<Neon Black Hub\>*.

*Short description of this release*
