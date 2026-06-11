# Scoring Setup - Step-by-Step

Covers `ParticipantScoreService`, high-score persistence, and hooking pickups and game events to score.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Scoring/Objectives
- Realtime Character for score loops driven by pawns, pickups, hazards, or combat
- Board/Card/Tabletop for score, victory points, resources, or round results without pawns

Resolve setup-profile validation before adding score services, score UI, pickups, high-score save flow, or end-of-round hooks.

---

## Concepts

- **ParticipantScoreService** - the canonical scoring service. Tracks the current session points, survival time, and high-score records (best points and best time) persisted via `PlayerPrefs`. Use explicit scene references or injected runtime access when another system needs it.
- Points and time are tracked independently. Beating the points record also saves the survival time of that run. Beating the longest survival time is tracked as a separate record (`HighScoreBestTime`).

---

## Step 1 - Add ParticipantScoreService to the scene

`ParticipantScoreService` must be in your gameplay scene. One instance per scene is all you need.

1. On a dedicated scene systems object, or on your 2D `GameManager` when using that flow, Add Component → `ParticipantScoreService`.
2. Wire the Inspector events:
   - **On Points Changed** - fires with the current point total whenever points change. Wire this to a `UIManager` or score display label if you want live updates separate from `UIManager`'s own subscription.
   - **On High Score Beaten** - fires once per session when the new total surpasses the saved best. Wire this to a visual effect, sound, or banner if desired.

No other wiring is required. `ParticipantScoreService` has a `[DefaultExecutionOrder(-30)]` attribute so it initializes before most scene runtime startup.

---

## Step 2 - Adding points from pickups

`CollectibleSpawner2D` integrates with `ParticipantScoreService` through explicit runtime wiring - when the player collects an item the service is incremented through the active score-award route.

To add points manually (e.g. for defeating enemies, events, or bonuses):

```csharp
[Inject] private ISessionScoreAwardSink scoreAwardSink;

scoreAwardSink?.AddPoints(10);
```

`ParticipantScoreService.AddPoints` ignores zero and negative values. Use a custom scoring service when a game needs penalties, resources that can decrease, or multiple score channels.

---

## Step 3 - Start and stop the survival timer

The survival timer runs while the session is active. `GameManager` stops it automatically in the game-over flow. If you manage session end yourself:

```csharp
// Stop timing when the player dies or the round ends
scoreService?.StopTimer();
```

The timer starts automatically when `ParticipantScoreService.Awake` runs (i.e. when the gameplay scene loads).

---

## Step 4 - Save the high score at session end

Call `SaveHighScore()` when the round ends. `GameManager` calls this automatically in its game-over flow. If you trigger session end manually:

```csharp
scoreService?.SaveHighScore();
```

This writes to `PlayerPrefs` and updates the in-memory cache. The next read of `HighScorePoints`, `HighScoreTime`, or `HighScoreBestTime` reflects the new values.

---

## Step 5 - Display scores in UI

`UIManager` subscribes to the assigned `ISessionScoreService` and updates the HUD score label. The game-over panel reads the service for final and high-score values.

To read scores manually (e.g. from a custom UI):

```csharp
if (scoreService != null)
{
    int current = scoreService.PointsCollected;
    int best = scoreService.HighScorePoints;
    float time = scoreService.SurvivalTime;
    float bestTime = scoreService.HighScoreBestTime;

    string formatted = ParticipantScoreService.FormatTime(time);
}
```

---

## Step 6 - Reset for a new round

When starting a fresh round, call:

```csharp
scoreService?.ResetScore();
```

This zeroes the current points and time without touching the high-score record.

---

## Step 7 - Verify in Play Mode

1. Enter Play Mode and start a session.
2. Collect a pickup - score label should increment.
3. Die or end the session - final score and high score should appear on the game-over panel.
4. Start again - score should reset to zero, high score should persist from the previous run.

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| Score never updates in the HUD | `UIManager` not finding `ParticipantScoreService` - check both are in the scene |
| High score resets every launch | `SaveHighScore()` not being called at session end |
| `scoreService` is null | `ParticipantScoreService` is not present in the scene runtime or was not injected into the caller |
| `AddPoints` not found | Update the caller to use explicit or injected `ParticipantScoreService` access |
