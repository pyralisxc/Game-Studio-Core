# Scene Flow Setup — Step-by-Step

Covers `LevelData`, `LevelRegistry`, `SceneFader`, and scene loading.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Platform Core for shared scene bootstrapping, navigation, and setup validation
- Camera/Cursor Control when the player is the camera, cursor, menu focus, or tabletop selection surface
- Realtime Character, Board/Card/Tabletop, or Turn/Menu Action based on the destination scene's actual control model

Resolve setup-profile validation before assigning level registries, faders, menu destinations, or restart flow.

---

## Concepts

- **LevelData** — a ScriptableObject defining one playable world: its scene name, display name, and preview image.
- **LevelRegistry** — an ordered list of all `LevelData` assets. Drives main menu world selection and random level picking.
- **SceneFader** — the scene-transition coordinator that fades to black before loading a new scene and fades back in after it loads.

---

## Step 1 — Add all scenes to Build Settings

Every scene you want to load at runtime must be in **File → Build Settings**.

1. Open **File → Build Settings**.
2. Drag each scene file from the Project window into the **Scenes In Build** list.
3. Note the exact **scene names** (shown in the list) — you will type these into `LevelData` assets.

---

## Step 2 — Create LevelData assets

Create one asset per playable world.

1. Right-click in the Project window → **Create → NeonBlack → Gameplay → Scene Flow → Level Data**.
2. Name it (e.g. `LevelData_Kitchen`).
3. Fill in the Inspector:
   - **Scene Name** — type the scene name exactly as shown in Build Settings (case-sensitive, no `.unity` extension, e.g. `KitchenScene`).
   - **Display Name** — the friendly world name shown in the main menu selector (e.g. `The Kitchen`).
   - **Preview Image** — drag a `Sprite` asset to use as the background on the main menu while this world is selected.
4. Repeat for every playable world.

---

## Step 3 — Create a LevelRegistry asset

1. Right-click in the Project window → **Create → NeonBlack → Gameplay → Scene Flow → Level Registry**.
2. Name it `LevelRegistry`.
3. In the Inspector, expand the **Levels** array and add one slot per world.
4. Drag each `LevelData` asset into its slot, in the order they should appear in the menu selector.

---

## Step 4 — Assign LevelRegistry to consumers

Two components reference the `LevelRegistry`:

- **MainMenuManager** (your main menu scene) → **Level Registry** field — drives the world selector.
- **GameManager** (your gameplay scene) → **Level Registry** field — used by `DoRestart` to pick a random next level.

Drag your `LevelRegistry` asset into both.

---

## Step 5 — Add SceneFader to your first scene

`SceneFader` is a persistent scene-flow service — add it once, to your main menu or earliest scene.

1. In the menu scene Hierarchy, right-click → **Create Empty**. Rename it `SceneFader`.
2. Add Component → `SceneFader`.
3. Wire the Inspector fields:
   - **Fade Out Duration** — seconds for the screen to go to black (e.g. `0.35`).
   - **Fade In Duration** — seconds for the screen to come back in after the new scene loads (e.g. `0.35`).

`SceneFader` calls `DontDestroyOnLoad` automatically and creates its own black overlay image at runtime — no Canvas setup required.

---

## Step 6 — Load scenes via SceneFader

Anywhere you load a scene, call `SceneFader` first:

```csharp
// By scene name
SceneFader.Instance?.FadeToScene("KitchenScene");

// By build index
SceneFader.Instance?.FadeToScene(2);
```

If `SceneFader` is not present (e.g. in a standalone test scene), `GameManager` falls back to `SceneManager.LoadScene(...)` directly — no null-check boilerplate needed in most callers.

---

## Step 7 — Verify scene transition in Play Mode

1. Press **Play** in the menu scene.
2. Select a world and press Play / Start.
3. Confirm the screen fades to black, the gameplay scene loads, and the screen fades back in.
4. In the gameplay scene, pressing Restart or Return to Menu should fade out and navigate correctly.

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| `Scene 'X' not found` exception at runtime | Scene name in `LevelData.sceneName` does not match the name in Build Settings exactly |
| No fade — loads instantly | `SceneFader` not in the scene, or `FadeToScene` not called (scene loaded directly via `SceneManager`) |
| Fader persists into a second instance | Second `SceneFader` in a later scene — keep one active scene-flow instance in the first/menu scene |
| Main menu world selector shows wrong scenes | `LevelRegistry` not assigned to `MainMenuManager`, or `Levels` array order is wrong |
| Restart loads wrong scene | `LevelRegistry` not assigned to `GameManager`, or `sceneName` typo in `LevelData` |
