# Health and Combat Setup — Step-by-Step

Covers `HealthComponent`, `HitBox`, `WeaponData`, `KnockbackReceiver`, and `DamageNumber`.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Combat
- Realtime Character for direct pawn/enemy fighting
- Projectile Combat for guns, spells, hitscan, thrown objects, or ranged hazards
- Turn/Menu Action for tactics, card combat, or menu-selected attacks

Resolve setup-profile validation before attaching health, hitboxes, weapons, damage numbers, or knockback receivers.

---

## Concepts

- **HealthComponent** — any GameObject that can be damaged or killed. Attach to the player, enemies, and destructible props.
- **HitBox** — a query volume that fires and detects what it hits. Attach to attack limbs or weapon points.
- **WeaponData** — a ScriptableObject that holds damage, knockback, range, and timing for one weapon.
- **KnockbackReceiver** — receives knockback impulses and exposes the velocity for the character controller to consume.

---

## Step 1 — Add HealthComponent to a character

1. Select the root GameObject of your player or enemy prefab.
2. Add Component → search `HealthComponent` → click it.
3. In the Inspector, set the following fields:

**Stats**
- **Max Health** — hit points this character starts with (e.g. `100`).
- **Destroy On Death** — enable if this object should be destroyed when health reaches zero. Leave off for the player (use Respawn instead).
- **Death Destroy Delay** — seconds to wait after death before destroying. Gives death animations time to finish.

**Faction**
- **Faction** — set to `Player` for the player, `Enemy` for enemies, `Neutral` for destructible props. Characters with the same faction cannot damage each other.

**Invincibility Frames**
- **IFrame Duration** — seconds the character is immune after being hit (default `0.15`). Prevents instant multi-hit.

**Health Recovery** (optional)
- **Regen Enabled** — enable to restore HP automatically over time.
- **Regen Rate** — HP per second while regenerating.
- **Regen Delay** — seconds after being hit before regen resumes.

**Events**
- **On Damaged** — drag a component here and choose a method to call when damage is taken.
- **On Death** — drag a component here and choose a method to call on death (e.g. trigger a game-over or respawn).

---

## Step 2 — Add KnockbackReceiver (3D characters only)

`KnockbackReceiver` requires a `CharacterController` on the same object.

1. On the same root GameObject as `HealthComponent`, add `KnockbackReceiver`.
2. Set the fields:
   - **Knockback Resistance** — `1` = full knockback, `0` = immune. Use `0.5` for heavy characters.
   - **Decay Rate** — how fast knockback fades per second (default `10`). Higher = snappier recovery.

`Motor3D` and `EnemyAI` both call `KnockbackReceiver.Tick()` and fold `Velocity` into their movement — no additional code needed.

---

## Step 3 — Create a WeaponData asset

`WeaponData` holds the stats for one weapon or attack type.

1. Right-click in the Project window → **Create → NeonBlack → Gameplay → Combat → Weapon Data**.
2. Name it (e.g. `WeaponData_Punch`).
3. Fill in the Inspector:

**Identity**
- **Weapon Name** — display name.

**Damage**
- **Damage** — base damage per hit (e.g. `20`).
- **Knockback Force** — force applied on hit in world units per second (e.g. `6`).

**Timing**
- **Attack Cooldown** — minimum seconds between swings (e.g. `0.45`).
- **Hit Delay** — seconds from button press to hitbox activating. Set to `0` if you use Animation Events.
- **Hit Duration** — seconds the hitbox stays active (e.g. `0.15`).

**Range**
- **Attack Range** — leave `0` to auto-measure from the HitBox collider, or set manually.

**Hit Zone**
- **Hit Box Zone** — type the name that matches the `HitBox` child you create in Step 4 (e.g. `Punch`). Must match exactly.

---

## Step 4 — Add a HitBox to the attack limb

A `HitBox` lives on a child GameObject inside your character prefab. It defines the volume of an attack.

1. Open your character prefab in Prefab Mode.
2. Right-click the root → **Create Empty** child. Rename it `HitBox_Punch` (or whatever matches your `WeaponData.hitBoxZone`).
3. With `HitBox_Punch` selected, add a **BoxCollider** (or SphereCollider). Do **not** check **Is Trigger** — the `HitBox` script uses an overlap query, not a trigger.
4. Position and size the collider to cover the attack reach.
5. Add Component → `HitBox`.
6. Fill in the Inspector:

**Owner**
- **Owner** — drag the root GameObject of the attacker here. Used for faction checking (so the player cannot hit themselves).

**Hit FX** (optional)
- **Hit FX Prefab** — drag a particle effect prefab to spawn at the hit point.
- **Hit SFX** — drag an AudioClip to play on hit.

**Hit Pause** (optional)
- **Freeze Frame Duration** — `0.06`–`0.1` for a light punch feel. `0` to disable.

**Camera Shake** (optional)
- **Camera Shake Intensity** — `0.15` for a punch, `0.3` for a heavy hit. `0` to disable.

7. In `PawnCombatBehaviour`, assign this HitBox to the appropriate weapon slot so the controller activates it during attacks. If using `WeaponData`, the zone name must match `HitBox_Punch` exactly.

---

## Step 5 — Assign weapons to PawnCombatBehaviour

1. Open your player prefab. Select the `PawnCombatBehaviour` component.
2. Scroll to the weapon fields:
   - **Attack Weapon** — drag your `WeaponData_Punch` asset here.
   - **Kick Weapon** — drag a `WeaponData_Kick` asset here if you have one.
   - **Aerial Weapon** — drag an aerial attack weapon asset here if needed.

---

## Step 6 — Add DamageNumber spawner (optional)

`DamageNumber` floats rising numbers over a hit target.

1. Find `DamageNumberSpawner` in the Project window (under `Features/Combat`).
2. Add it to any persistent GameObject in the scene (e.g. your systems root).
3. Assign the `DamageNumber` prefab to the **Prefab** field.
4. `HitBox` can find an available damage-number spawner at runtime, but explicit scene references are preferred when you need deterministic setup.

---

## How damage flows at runtime

```
Motor3D dispatches Attack → PawnCombatBehaviour.HandleAttack()
  → activates HitBox.Fire(damage, knockback)
  → HitBox runs Physics.OverlapBox
  → finds HealthComponent on hit targets (different faction only)
  → calls target.TakeDamage(damage, hitPoint, attacker)
  → HealthComponent fires OnDamaged event
  → KnockbackReceiver.ApplyKnockback called with direction * force
  → health reaches 0 → OnDeath fires
```

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| Attacks do nothing | `HitBox.Owner` not assigned, or factions match (player hitting player) |
| Hit Box Zone not found | `WeaponData.hitBoxZone` does not exactly match the child GameObject name |
| Knockback not applied | `KnockbackReceiver` missing, or the controller is not reading `Velocity` |
| Enemy damages player but player cannot damage enemy | Factions both set to `Neutral` — set player to `Player` and enemy to `Enemy` |
| Character destroys immediately on death | `destroyOnDeath` enabled on the player — use `PlayerSpawner` for respawn instead |
