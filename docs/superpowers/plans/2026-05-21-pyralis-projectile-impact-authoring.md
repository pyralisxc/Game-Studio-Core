# Pyralis Projectile Impact Authoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add authored projectile impact/effects policy and make the example authoring pack include a usable projectile setup.

**Architecture:** Keep visual/audio impact settings in a `ProjectileImpactDefinition` ScriptableObject referenced by `ProjectileDefinition`. Runtime launchers execute commands as before, then pass hit/miss results through a shared impact effect player that can spawn effect prefabs and trigger existing hit-pause/camera-shake services without coupling launchers to one controller.

**Tech Stack:** Unity 6, C#, ScriptableObject definitions, existing Combat/Data/Editor assemblies, NUnit compile-time validation.

---

## Acceptance Criteria

- [x] Projectile impact authoring data sanitizes labels and clamps effect timings.
- [x] Projectile definitions can reference an impact definition.
- [x] Projectile fire planning carries the impact definition into spawn commands.
- [x] 2D and 3D launchers apply hit/miss impact effects and expose the spawned effect in `ProjectileSpawnResult`.
- [x] Example authoring pack creates a sample hitscan projectile, fire mode, and impact definition.
- [x] Current package build matrix passes and Unity refresh shows no fresh C# compiler errors.
