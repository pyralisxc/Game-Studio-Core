# Markdown Audit - 2026-05-20

Status: historical snapshot. Use `README.md`, `CURRENT_STATE_AUDIT.md`, and `Authoring/START_HERE.md` for current documentation status.

This audit records the documentation cleanup completed after the gameplay platform realignment and deferred-implementation cleanup pass.

## Audit Goals

- identify stale setup steps
- identify deleted or renamed types still documented as active
- identify outdated architecture descriptions
- identify setup docs that still described singleton or legacy flows as the preferred path
- establish one canonical setup entrypoint

## Main Findings

### Setup docs had multiple competing sources of truth

Before this pass, setup guidance was split across several partially overlapping docs. The package needed one clearly named canonical setup document.

### Deferred cleanup changed important user-facing type names

Docs still referenced:

- `ParticipantHudFeedbackReceiver`
- old pickup examples such as `CollectibleSpawner2D.Active`
- crumb-era naming as if it were still a primary setup path

The codebase now uses:

- `ParticipantFeedbackHudPresenter`
- `ParticipantHealthHudBinder`
- `CollectibleSpawner2D.Instance`
- historical thin `PointPickup*` aliases rather than a separate pickup architecture

### Architecture docs lagged behind the cleanup seams

The package now has real seams for:

- typed participant feedback streams
- leaderboard service access
- gameplay-state reads
- pickup collect-vs-remove behavior

Those seams needed to appear in the docs because they change how engineers should extend the package.

## Files Updated In This Pass

- `README.md`
- `Docs/Setup/README.md`
- `Docs/Setup/CANONICAL_SETUP.md`
- `Docs/Setup/Prefabs/Pickups_Setup.md`
- `Docs/Setup/Prefabs/Feature_Module_Framework_Setup.md`
- `Docs/FEATURE_INVENTORY.md`
- `Docs/Setup/SCENE_SETUP_GUIDE.md`
- `Docs/Setup/Systems/Architecture_Overview.md`
- `Docs/CURRENT_STATE_AUDIT.md`
- `Docs/REFACTOR_WORKSPACE.md`

## Recommended Ongoing Rules

- every setup doc should assume `GameplaySessionBootstrap` is the composition root
- feature aliases should be documented as aliases, not as peer primary paths
- subsystem docs should link back to `Docs/Setup/CANONICAL_SETUP.md`
- when a runtime seam changes, update both the feature setup doc and the architecture overview in the same pass

## Remaining Watchlist

These are not blockers, but they are the next places likely to drift:

- example setup docs under `Docs/Setup/Prefabs/*Example*.md`
- `CURRENT_STATE_AUDIT.md` whenever platform priorities change again
- any docs that still mention old arcade terminology for historical context
