# Pyralis Projectile Launchers And Ammo Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Connect projectile spawn commands to reusable 2D/3D launcher adapters and add the first runtime fire-mode ammo state.

**Architecture:** Keep authored projectile/fire-mode data unchanged and add runtime execution in the Combat feature assembly. Launchers consume `ProjectileSpawnCommand` output from `ProjectileFirePlanner`, execute prefab or hitscan delivery, and optionally reuse prefab instances through a small launcher-owned pool. Ammo state remains a plain C# object so pawns, traps, turrets, cards, and turn/menu actions can use it without a MonoBehaviour.

**Tech Stack:** Unity 6, C#, NUnit, Unity physics/physics2D, existing Combat/Core/Data assemblies.

---

## File Structure

- Create `Features/Combat/ProjectileSpawnStatus.cs`
- Create `Features/Combat/ProjectileSpawnResult.cs`
- Create `Features/Combat/ProjectileMagazineState.cs`
- Create `Features/Combat/ProjectilePoolHandle.cs`
- Create `Features/Combat/ProjectileLauncherBase.cs`
- Create `Features/Combat/ProjectileLauncher3D.cs`
- Create `Features/Combat/ProjectileLauncher2D.cs`
- Modify `Tests/Runtime/PlatformKernelTests.cs`
- Modify `Docs/FEATURE_DEVELOPMENT_ROADMAP.md`
- Modify `Docs/FEATURE_INVENTORY.md`
- Modify `Docs/FEATURE_DEVELOPMENT_SCOPE.md`

## Acceptance Criteria

- [x] Magazine state supports unlimited-fire modes, clipped modes, reload, reserve ammo, and invalid consume rejection.
- [x] 3D launcher can execute hitscan commands and apply damage through `HealthComponent`.
- [x] 2D launcher can execute hitscan commands and apply damage through `HealthComponent`.
- [x] Launchers can execute projectile-prefab commands with optional pooling.
- [x] Full package build matrix passes.
- [x] Unity refresh shows no fresh C# compiler errors from this slice.
