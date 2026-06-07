# Pyralis Core Package Readiness Checkpoints Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn Pyralis core-package work into product-shaped readiness checkpoints before scene building continues.

**Architecture:** Durable package docs define the product gates. Existing roadmap, inventory, and runtime parity docs point back to those gates so implementation work can be judged by creator-ready outcomes instead of isolated runtime classes.

**Tech Stack:** Unity package documentation, Markdown, existing Pyralis gameplay docs.

---

## File Structure

- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`
  - Owns the product-level readiness gates for tabletop, side-scrolling shooter, FPS, Unity authoring UX, and runtime parity.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`
  - Reframes current priority around readiness checkpoints.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
  - Adds checkpoint alignment so parity rows connect to product outcomes.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`
  - Points near-term feature planning at checkpoint-based readiness.

### Task 1: Create Readiness Checkpoints Doc

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`

- [x] **Step 1: Write the checkpoint document**

Create a document with:

- product promise
- shared definition of done
- Rules-Driven Tabletop MVP
- Local Two-Player Side-Scrolling Shooter MVP
- FPS And 3D Projectile MVP
- Unity-Only Authoring UX
- Runtime Parity Hardening
- active development gate

- [x] **Step 2: Check for missing placeholders**

Run:

```powershell
rg "TBD|TODO|fill in|implement later" Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md
```

Expected: no output.

### Task 2: Reframe Roadmap Current Priority

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`

- [x] **Step 1: Add checkpoint rule**

Add a short section after `Roadmap Rule`:

```markdown
## Readiness Checkpoint Rule

Near-term work is governed by `CORE_PACKAGE_READINESS_CHECKPOINTS.md`. A slice should advance a named checkpoint toward Unity-authorable playable proof. If a slice only adds isolated infrastructure without moving a checkpoint gate, defer it.
```

- [x] **Step 2: Replace current priority**

Replace the existing `Current Priority` paragraph with a checkpoint-ordered priority list:

```markdown
## Current Priority

Use `CORE_PACKAGE_READINESS_CHECKPOINTS.md` as the near-term product gate.

1. Finish Rules-Driven Tabletop MVP: legal move policies, baseline capture/occupancy policies, terminal conditions, starter assets, and beginner setup docs.
2. Prove Local Two-Player Side-Scrolling Shooter MVP: local participant setup, 2D projectile launcher starter path, scene readiness validation, and a small playable sample path.
3. Keep Unity-Only Authoring UX current: guided inspectors, Create Asset menu coverage, starter packs, and setup validation for every new creator-facing asset.
4. Preserve Runtime Parity Hardening: update matrix, inventory, tests, and docs whenever a lane changes status.

Do not start deeper scene building until the relevant checkpoint has a runtime path, authoring path, validation path, and proof path.
```

### Task 3: Connect Runtime Parity To Checkpoints

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`

- [x] **Step 1: Add checkpoint alignment section**

Add after `Parity Levels`:

```markdown
## Checkpoint Alignment

The product-level readiness gates live in `CORE_PACKAGE_READINESS_CHECKPOINTS.md`. Matrix rows should stay honest about whether a capability is only foundational, playable, or production-ready for one of those checkpoints.
```

- [x] **Step 2: Replace next targets with checkpoint-oriented targets**

Update `Next Parity Targets` so it lists tabletop MVP, side-scrolling shooter MVP, Unity-only authoring UX, and FPS/3D projectile MVP in that order.

### Task 4: Update Feature Inventory Planning Pointer

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`

- [x] **Step 1: Update Intended Expansion Surface**

Change the paragraph under `Intended Expansion Surface` so it references both `FEATURE_DEVELOPMENT_SCOPE.md` and `CORE_PACKAGE_READINESS_CHECKPOINTS.md`.

- [x] **Step 2: Replace near-term priorities**

Replace the near-term feature planning list with:

```markdown
1. Rules-driven tabletop MVP
2. Local two-player side-scrolling shooter MVP
3. Unity-only authoring UX for new creator-facing assets
4. FPS and 3D projectile MVP
5. Runtime parity hardening across all advertised lanes
```

### Task 5: Verify Documentation Checkpoint

**Files:**
- Read: updated docs

- [x] **Step 1: Search for churn language and placeholders**

Run:

```powershell
rg "TBD|TODO|fill in|implement later" Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs docs/superpowers/plans/2026-05-25-pyralis-core-package-readiness-checkpoints.md
```

Expected: no placeholder text introduced by this plan.

Observed: no placeholder text introduced by the checkpoint docs. The search also found command examples inside this plan and older `fill in` wording in existing setup docs.

- [x] **Step 2: Confirm the new doc is linked**

Run:

```powershell
rg "CORE_PACKAGE_READINESS_CHECKPOINTS" Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs
```

Expected: roadmap, runtime parity matrix, feature inventory, and the checkpoint doc reference the checkpoint gate.
