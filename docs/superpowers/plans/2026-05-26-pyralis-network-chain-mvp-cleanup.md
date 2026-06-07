# Pyralis Network Chain MVP Cleanup Plan

## Goal

Strengthen the networking lane so Pyralis can support local multiplayer, host/client prototype scenes, and later online game work without promising a custom networking engine.

## Decision

Use Unity Netcode for GameObjects and Unity Transport for the MVP backend. Write the Pyralis-specific layer ourselves: authoring semantics, setup guidance, validation, ownership, authority, roster/session services, spawn adapters, and game-rule integration seams.

## Slice

1. Document the build-or-buy boundary in the networking domain.
2. Split local multiplayer and NGO setup expectations in beginner docs.
3. Promote Network Chain MVP into readiness checkpoints and runtime parity tracking.
4. Add source-contract tests so future edits preserve the boundary.
5. Run focused Unity/package validation.

## Definition Of Done

- Docs clearly say local `PlayerInputManager` multiplayer is not NGO networking.
- Docs clearly say Pyralis uses NGO and Unity Transport rather than custom low-level networking.
- Readiness docs list Network Chain MVP as a first-class guided setup checkpoint.
- Tests protect the networking boundary, setup quick path, and readiness checkpoint language.
