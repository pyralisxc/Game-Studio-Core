# Pyralis Proof Scene Policy

Proof scenes are temporary, deletable route-verification artifacts. They answer one question: can a creator understand and verify a route through the native Unity workflow?

## What Proof Scenes Are

- disposable scenes for checking route logic and authoring clarity
- evidence that a specific route can be assembled and understood
- useful for Game Shell, pawn-backed lanes, tabletop, networking, and other route gates
- allowed to be plain, small, and unpolished

## What Proof Scenes Are Not

- package samples
- hidden setup generators
- final presets or starter packs
- evidence that bypasses the Authoring Window, Project window Create menu, Hierarchy, Inspector Add Component, object picker, drag/drop wiring, or Play Mode verification

## Promotion Path

1. Prove the route manually through native Unity authoring.
2. Use a temporary proof scene to verify the smallest visible or inspectable loop.
3. Record friction in route docs, validators, inspectors, or authoring facts.
4. Delete, archive, or intentionally promote the proof scene.
5. Create polished samples only after the manual route is understood.

## Storage

Temporary route proofs should live under a clearly named proof area such as `Assets/PyralisProofs/Temporary/` or a project-local review area. Do not place proof scenes in package samples unless they have been intentionally promoted.

## Success Standard

A proof scene succeeds when the route is clear enough that a beginner-to-adaptable Unity user can follow the guidance, wire the required assets/components, press Play, and observe the smallest route-specific state change.
