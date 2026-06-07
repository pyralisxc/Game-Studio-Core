# Enemy Setup - Step-by-Step

This guide walks through setting up an enemy using `EnemyAI` and optionally spawning it with `EnemySpawner`.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Combat
- Realtime Character when enemies fight pawn-controlled actors
- Projectile Combat when enemies fire projectiles, hitscan, spells, or thrown attacks
- Animation/Presentation when enemies use Animator-driven states

Resolve setup-profile validation before building enemy prefabs, attack assets, spawners, or encounter links.

---

## What you need

- An enemy prefab (or a new empty GameObject you will turn into one)
- At least one `EnemyAttack` ScriptableObject asset (for combat-capable enemies)
- An Animator Controller with the required parameters (see Step 3)

---

## Step 1 - Build the enemy prefab

1. In the **Hierarchy**, right-click → **Create Empty**. Rename it `Enemy` (or your enemy's name).
2. With the enemy object selected, add the following components via **Add Component**:

| Component | Notes |
|---|---|
| `CharacterController` | Required. Set Center Y to half your capsule height so it sits on the ground. |
| `HealthComponent` | Required. Set **Faction** to `Enemy`. Set **Max Health** as needed. |
| `KnockbackReceiver` | Required for hit reactions and death launch. |
| `Animator` | Required. Assign your Animator Controller (see Step 3). |
| `EnemyAI` | The main behavior script. |

3. Right-click the enemy root → **Create Empty** child. Rename it `HitBox_Attack`.
   - Add a **Collider** (BoxCollider or CapsuleCollider) to this child and size it to your attack reach.
   - Add a `HitBox` component to the same child.
   - Set the collider to **Is Trigger = true**.

4. Save as a prefab: drag the enemy from the Hierarchy into your Project window (e.g. `Assets/Game/Prefabs/Enemies`). Then double-click the prefab to keep editing it in Prefab Mode.

---

## Step 2 - Create an EnemyAttack asset

`EnemyAttack` is a ScriptableObject that defines one attack move (damage, range, cooldown, animation trigger).

1. Right-click in the Project window → **Create → NeonBlack → Gameplay → Combat → Enemy Attack**.
2. Name it (e.g. `Attack_Punch`).
3. In the Inspector on the asset, fill in:
   - **Hit Box Zone** - type the name that matches the HitBox child you created (e.g. `HitBox_Attack`).
   - **Damage** - how much damage this attack deals.
   - **Knockback** - force applied on hit.
   - **Attack Range** - leave at `0` to auto-measure from the collider, or set manually.
   - **Animation Trigger** - the Animator trigger name this attack fires (e.g. `Attack`).
   - **Attack Cooldown** - seconds between uses of this attack.

---

## Step 3 - Set up the Animator

Your Animator Controller needs the following parameters for `EnemyAI` to drive it:

| Parameter | Type | Used for |
|---|---|---|
| `IsMoving` | Bool | Walking/idle blend |
| `IsGrounded` | Bool | Air vs ground state |
| `Attack` | Trigger | Attack animation (or named per EnemyAttack asset) |
| `Death` | Trigger | Death animation |

1. Open your Animator Controller (double-click it in the Project window).
2. In the **Parameters** panel (top-left of the Animator window), click `+` to add each parameter above.
3. Wire your animation states to these parameters as needed.

---

## Step 4 - Wire EnemyAI in the Inspector

1. Open your enemy prefab (double-click in the Project window).
2. Select the root GameObject and find the `EnemyAI` component.
3. Fill in the fields:

**Detection**
- **Aggro Range** - how close the player must be to trigger chase (default `8`).
- **Leash Range** - how far the player can run before the enemy gives up (default `16`).
- **Require Line Of Sight** - enable if you want the enemy to check for obstacles before aggroing.
- **Target Override** - assign the target directly for simple scenes. Leave empty when `GameplaySessionBootstrap` or the gameplay lifetime scope provides the active player through participant infrastructure.

**Movement**
- **Movement Mode** - `ThreeD` for a 2.5D brawler moving on X/Z. `TwoD` for a side-scroller on X only.
- **Move Speed** - walking speed in units/second.

**Visuals**
- **Visual Root** - drag the child Transform that holds the sprite (the one whose `localScale.x` you want to flip when the enemy turns). If left empty, the AI falls back to `SpriteRenderer.flipX`.
- **Sprite Default Faces Right** - enable if your sprite art faces right; disable if it faces left.

**Patrol Points** (optional)
- Leave empty for random left-right patrol around spawn position.
- Add child Transform objects in the scene and drag them here for fixed patrol routes.

**Combat**
- **Hit Box Zones** - click `+` and drag your `HitBox_Attack` child here. Set **Zone Name** to match the name used in your `EnemyAttack` asset (e.g. `HitBox_Attack`).
- **Attack Sequence** - click `+` and drag your `EnemyAttack` asset(s) here.
- **Attack Mode** - `Sequential` cycles through attacks in order. `Random` picks by weight.

**Ground Check**
- **Ground Layer** - set to your ground layer mask so the enemy knows when it is standing on something.

4. Save the prefab (Ctrl+S or click **Save** in the top-left of Prefab Mode, then click the back arrow to return to the scene).

---

## Step 5 - Add EnemySpawner to the scene

`EnemySpawner` manages spawning and respawning enemies at runtime. You do not need it if you are placing enemies directly in the scene.

1. In the **Hierarchy**, right-click → **Create Empty**. Rename it `EnemySpawner`.
2. Add the `EnemySpawner` component.
3. In the Inspector:

**Enemy Prefabs**
- Click `+` on the **Enemy Prefabs** list and drag your enemy prefab here. Add more for random variety.

**Spawn Points** (optional)
- Leave empty to spawn at the spawner's own position.
- Or create child empty GameObjects under `EnemySpawner` (right-click → Create Empty), position them, and drag them into **Spawn Points**.
- **Spawn Radius** - scatter enemies randomly within this radius around each point.

**Spawn Mode**
- **Continuous** - keeps a set number of enemies alive. When one dies it respawns after a delay.
  - **Max Alive** - how many enemies to keep alive at once.
  - **Respawn Delay** - seconds before a dead enemy is replaced.
- **Waves** - spawns a fixed number per wave, waits for all to die, then starts the next wave.
  - **Enemies Per Wave**, **Total Waves** (0 = endless), **Wave Cooldown**.

4. Position the `EnemySpawner` object in the scene where you want enemies to appear.

---

## Step 6 - Verify in Play Mode

1. Press **Play**.
2. Walk your player into the enemy's `Aggro Range`.
3. The enemy should transition from patrol → chase → attack.
4. When the enemy's health reaches zero, the `Death` trigger fires and the GameObject is destroyed.
5. If `EnemySpawner` is set to **Continuous**, a replacement should appear after `Respawn Delay` seconds.

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| Enemy does not move | CharacterController not added, or ground layer not set so it thinks it is always in the air |
| Enemy does not attack | Hit Box Zones list is empty, or Zone Name does not match the name on the EnemyAttack asset |
| Enemy flickers or faces wrong direction | Sprite Default Faces Right is set incorrectly for your art |
| Attack animation does not play | Animation trigger name in EnemyAttack asset does not match the parameter name in the Animator |
| Enemy does not take damage | HealthComponent Faction is not set to `Enemy`, or there is no HitBox on the player's attack |
| Nothing spawns from EnemySpawner | Enemy Prefabs list is empty, or the prefab is missing a CharacterController |
