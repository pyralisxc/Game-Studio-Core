# Pyralis Guns Projectiles Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first reusable guns/projectiles foundation on top of Action + Targeting without tying projectile behavior to one character controller.

**Architecture:** Keep authored projectile and fire-mode data in the combat data surface and keep runtime planning in the combat feature assembly. The first slice plans projectile/hitscan spawn commands deterministically; later slices can add pooling, concrete 2D/3D adapters, visual target selection, and scene-prefab generation.

**Tech Stack:** Unity 6, C#, ScriptableObject definitions, existing Combat/Data/Core assemblies, NUnit compile-time validation.

---

## File Structure

- Create `Data/Definitions/Combat/ProjectileDeliveryMode.cs`
- Create `Data/Definitions/Combat/ProjectileDefinition.cs`
- Create `Data/Definitions/Combat/FireModeDefinition.cs`
- Create `Features/Combat/ProjectileFireRequest.cs`
- Create `Features/Combat/ProjectileSpawnCommand.cs`
- Create `Features/Combat/ProjectileFirePlanner.cs`
- Modify runtime/editor tests for sanitation and planner behavior.
- Update feature roadmap/inventory docs.

## Acceptance Criteria

- [x] Authored projectile data supports projectile-prefab and hitscan delivery modes.
- [x] Authored fire-mode data supports cooldown, ammo, reload, burst count, projectile count, and spread angle.
- [x] Runtime planner can produce deterministic spread commands from a source origin/direction.
- [x] Planner uses action context when supplied, so this path can serve non-pawn control surfaces later.
- [x] All four package builds pass.
