# UI and HUD Setup — Step-by-Step

Covers `UIManager`, the HUD panel, and the game-over panel. Used in 2D arcade-style scenes.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Scoring/Objectives for score, timer, round result, and victory display
- Realtime Character when HUD state follows pawn health, combat, pickups, hazards, or respawn
- Board/Card/Tabletop or Turn/Menu Action when UI represents cursor selection, card zones, action menus, or round phases without a pawn controller

Resolve setup-profile validation before wiring HUD labels, score services, game-over panels, settings screens, or scene navigation buttons.

---

## Concepts

- **UIManager** — the HUD coordinator for live score, time, and game-over presentation. It subscribes to the active scene runtime and `ParticipantScoreService` during startup.
- The Canvas must be **Screen Space - Overlay** mode.

---

## Step 1 — Create the Canvas

1. In the Hierarchy, right-click → **UI → Canvas**.
2. Set **Render Mode** to `Screen Space - Overlay`.
3. Ensure the Canvas has an `EventSystem` in the scene (Unity creates one automatically with the first Canvas).

---

## Step 2 — Build the HUD panel

1. Inside the Canvas, right-click → **Create Empty**. Rename it `HUDPanel`.
2. Add the following children:
   - A **TextMeshPro - Text (UI)** object named `ScoreLabel`. Position it top-left or top-center.
   - A **TextMeshPro - Text (UI)** object named `TimeLabel`. Position it top-right or below the score.
   - A **Button** named `SettingsButton` (optional) — gear icon for opening settings.

---

## Step 3 — Build the game-over panel

1. Inside the Canvas, right-click → **Create Empty**. Rename it `GameOverPanel`.
2. Add the following children:
   - A **TextMeshPro - Text (UI)** named `FinalScoreLabel` — shows the player's final score.
   - A **TextMeshPro - Text (UI)** named `HighScoreLabel` — shows the all-time high score.
   - A **Button** named `RestartButton`.
   - A **Button** named `MainMenuButton`.
3. Set `GameOverPanel` to **inactive** by default (uncheck the tick next to its name in the Inspector). UIManager shows it when game over fires.

---

## Step 4 — Add UIManager to the Canvas

1. Select your Canvas GameObject.
2. Add Component → `UIManager`.
3. Wire all fields in the Inspector:

**Panels**
- **HUD Panel** — drag `HUDPanel`.
- **Game Over Panel** — drag `GameOverPanel`.

**HUD Labels**
- **Score Label** — drag `ScoreLabel` TMP object.
- **Time Label** — drag `TimeLabel` TMP object.

**Game Over Labels**
- **Final Score Label** — drag `FinalScoreLabel` TMP object.
- **High Score Label** — drag `HighScoreLabel` TMP object.

**Game Over Buttons**
- **Restart Button** — drag `RestartButton`.
- **Main Menu Button** — drag `MainMenuButton`.

**Settings** (optional)
- **Settings Button** — drag `SettingsButton`.
- **Settings Screen** — drag the `SettingsScreen` component (see [Settings_Setup.md](Settings_Setup.md)).

**HUD Format**
- **Time Prefix** — text before the time readout (e.g. `Time: `).
- **Score Prefix** — text before the score readout (e.g. `Points: `).

---

## Step 5 — Add EventSystem and Physics Raycaster (if needed)

If buttons are not responding to clicks:

1. Make sure the scene has an `EventSystem` object (usually created automatically).
2. If using `Camera.main` rendering, ensure the Camera has a **Physics Raycaster** component.
3. For Screen Space Overlay canvases this is not required — the Canvas handles its own raycasting via the default **Graphic Raycaster** component on the Canvas.

---

## Step 6 - How UIManager subscribes to events

At `Start`, `UIManager` finds `GameManager` and subscribes to `OnGameStateChanged`. Scoring is provided by the active scene runtime and the current session composition path.

No code changes needed - as long as `ParticipantScoreService` is present in the scene runtime, the wiring is automatic.

---

## Step 7 — Verify in Play Mode

1. Enter Play Mode. The HUD panel should be visible with `Points: 0` and the timer running.
2. Collect a pickup — score label increments immediately.
3. Trigger game over — HUD panel hides, game-over panel appears with final score and high score.
4. Click Restart — scene reloads and score resets to `0`.
5. Click Main Menu — fades to the menu scene.

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| HUD does not appear on Play | `HUD Panel` reference not wired, or panel was accidentally set inactive and `UIManager` receives no GameState event |
| Score never updates | `Score Label` not assigned, or `ParticipantScoreService` missing from scene |
| Buttons do not respond | No `EventSystem` in scene, or Canvas not set to Screen Space Overlay |
| High score shows `0` always | `SaveHighScore()` not called at session end — check `GameManager` game-over flow |
| Game-over panel shows on Play | Panel was left active — set it inactive in the Inspector before entering Play Mode |
