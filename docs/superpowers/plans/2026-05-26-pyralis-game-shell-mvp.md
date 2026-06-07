# Pyralis Game Shell MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Game Shell route beginner-prototype ready through guided Unity setup: boot/loading/menu/settings/credits/gameplay transition can be wired from the current package without private studio-only scenes.

**Architecture:** Keep the existing Unity UGUI and explicit `ISceneNavigator` pattern. Extend `MainMenuManager` with a first-class optional credits panel and button/back wiring, then align editor guidance and setup docs so beginners can assemble the shell from scratch. Use source-contract tests to prevent the route from drifting back into doc-only readiness.

**Tech Stack:** Unity, C#, UGUI `Button`/`GameObject` panels, `ISceneNavigator`, NUnit EditMode source-contract tests, Pyralis guided inspectors.

---

## File Structure

- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Navigation/UI/MainMenuManager.cs`
  - Add `creditsPanel`, `creditsButton`, and `creditsBackButton`.
  - Route the Credits button to `ShowPanel(creditsPanel)`.
  - Include the credits panel in the panel hide/show group.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/MenuNavigationGuidedEditors.cs`
  - Teach the `MainMenuManager` inspector that Credits is part of the shell route.
  - Validate missing credits references as optional until a sample scene or prefab template makes them required.
  - Keep `Scene Navigator Source` validation required for play/load/exit.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/Prefabs/Scene_Flow_Setup.md`
  - Replace stale `LevelRegistry`/world-selector guidance for `MainMenuManager` with current `gameSceneName` shell wiring.
  - Add one beginner path that covers boot scene, loading scene, main menu, settings, credits, and gameplay scene transition.
  - Document `SceneFader.FadeToSceneViaLoader` as the loading-scene route.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
  - Mark Game Shell as closer to `Ready` once credits runtime, guidance, and validation exist.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`
  - Update the Game Shell checkpoint status with this slice's coverage and remaining proof-scene work.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/GameShellMvpContractTests.cs`
  - Verify credits runtime fields/listeners exist.
  - Verify editor guidance mentions Credits and scene navigator requirements.
  - Verify scene-flow docs contain the complete shell route and no stale `MainMenuManager` `Level Registry` claim.

## Task 1: Add Failing Game Shell Contract Tests

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/GameShellMvpContractTests.cs`

- [ ] **Step 1: Add source-contract tests**

```csharp
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class GameShellMvpContractTests
    {
        private static readonly string PackageRoot = Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub");

        private static readonly string GameplayRoot = Path.Combine(
            PackageRoot,
            "Members",
            "Pyralis",
            "Gameplay");

        [Test]
        public void MainMenuManager_Source_ExposesCreditsPanelFlow()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Core", "Navigation", "UI", "MainMenuManager.cs"));

            StringAssert.Contains("creditsPanel", source);
            StringAssert.Contains("creditsButton", source);
            StringAssert.Contains("creditsBackButton", source);
            StringAssert.Contains("OnCredits", source);
            StringAssert.Contains("creditsButton.onClick.AddListener(OnCredits)", source);
            StringAssert.Contains("creditsBackButton.onClick.AddListener(OnBackToMain)", source);
            StringAssert.Contains("creditsPanel.SetActive(creditsPanel == target)", source);
        }

        [Test]
        public void MainMenuManagerEditor_GuidesCreditsAndRequiredNavigation()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Editor", "MenuNavigationGuidedEditors.cs"));

            StringAssert.Contains("Credits", source);
            StringAssert.Contains("creditsPanel", source);
            StringAssert.Contains("creditsButton", source);
            StringAssert.Contains("creditsBackButton", source);
            StringAssert.Contains("Scene Navigator Source is required for play/load/exit buttons.", source);
        }

        [Test]
        public void SceneFlowSetup_DocumentsCompleteGameShellRoute()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Scene_Flow_Setup.md"));

            StringAssert.Contains("Game Shell MVP route", docs);
            StringAssert.Contains("boot scene", docs);
            StringAssert.Contains("loading scene", docs);
            StringAssert.Contains("main menu", docs);
            StringAssert.Contains("settings", docs);
            StringAssert.Contains("credits", docs);
            StringAssert.Contains("gameplay scene transition", docs);
            StringAssert.Contains("FadeToSceneViaLoader", docs);
            Assert.That(docs.Contains("MainMenuManager` (your main menu scene) -> **Level Registry**"), Is.False);
        }
    }
}
```

- [ ] **Step 2: Run the focused test and confirm it fails before implementation**

Run:

```powershell
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: compile succeeds after the test file is added. The Unity EditMode test would fail until `MainMenuManager`, editor guidance, and docs expose credits.

## Task 2: Add Credits Runtime Flow To MainMenuManager

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Navigation/UI/MainMenuManager.cs`

- [ ] **Step 1: Add serialized credits fields**

Add `creditsPanel` beside the existing panel references:

```csharp
[SerializeField] private GameObject creditsPanel;
```

Add `creditsButton` beside the existing main menu buttons:

```csharp
[SerializeField] private Button creditsButton;
```

Add a dedicated credits back button section:

```csharp
[Header("Credits Panel")]
[SerializeField] private Button creditsBackButton;
```

- [ ] **Step 2: Wire listeners**

In `Start`, add:

```csharp
if (creditsButton != null) creditsButton.onClick.AddListener(OnCredits);
if (creditsBackButton != null) creditsBackButton.onClick.AddListener(OnBackToMain);
```

- [ ] **Step 3: Add the panel action**

Add:

```csharp
private void OnCredits()
{
    ShowPanel(creditsPanel);
}
```

- [ ] **Step 4: Include credits in panel visibility**

Update `ShowPanel`:

```csharp
if (creditsPanel != null) creditsPanel.SetActive(creditsPanel == target);
```

## Task 3: Update Guided Inspector For Credits

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/MenuNavigationGuidedEditors.cs`

- [ ] **Step 1: Update MainMenuManager manual text**

Change the summary to include credits:

```csharp
"MainMenuManager controls the panel-driven main menu, opens settings, credits, and co-op panels, and sends play/load/exit buttons through an explicit scene navigation service."
```

Update required setup bullets:

```csharp
"Assign Main Panel and every main menu button that should be clickable.",
"Assign Settings, Credits, and Co-op panels only for pages exposed by this menu.",
"Assign Game Scene Name to the scene loaded by New Game, Load Game, and Host Co-op.",
"Assign Scene Navigator Source to SceneFader, SceneLoader, or another ISceneNavigator.",
"Assign each panel Back button so settings, credits, and co-op pages can return to Main Panel."
```

Update common mistakes:

```csharp
"Do not leave New Game, Load Game, Settings, Credits, Co-op, or Exit buttons empty unless that action is intentionally disabled.",
"Do not wire Credits as a separate scene unless that is a deliberate presentation choice; the shell route expects a simple panel.",
"Do not use blank Game Scene Name; navigation services cannot load an unnamed scene.",
"Do not forget the Scene Navigator Source in the bootstrap/menu scene."
```

- [ ] **Step 2: Add optional validation messages**

In `GetMainMenuMessages`, require the credits button at the same level as other main buttons:

```csharp
RequireObject(serializedObject, messages, "creditsButton", "Credits Button");
```

Add optional panel/back diagnostics:

```csharp
if (!HasObject(serializedObject, "creditsPanel"))
    messages.Add(PyralisGuideIssue.Optional("Credits Panel is empty. The Credits button listener will still run but ShowPanel has no target panel."));

if (HasObject(serializedObject, "creditsPanel") && !HasObject(serializedObject, "creditsBackButton"))
    messages.Add(PyralisGuideIssue.Optional("Credits Back Button is empty. Credits can open, but beginners need a button path back to Main Panel."));
```

## Task 4: Align Setup Docs With Current Shell Route

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/Prefabs/Scene_Flow_Setup.md`

- [ ] **Step 1: Update the overview**

Make the opening sentence:

```markdown
Covers the Game Shell MVP route: boot scene, loading scene, main menu, settings, credits, and gameplay scene transition through `SceneFader`, `LoadingScreenController`, `MainMenuManager`, and `ISceneNavigator`.
```

- [ ] **Step 2: Add shell route concepts**

Add concept bullets:

```markdown
- **Game Shell MVP route** - the beginner setup path that proves a project can boot, show loading progress, open a main menu, open settings, open credits, and transition into gameplay.
- **LoadingScreenController** - the optional loading-scene component that reads `SceneFader.PendingScene`, updates progress UI, and activates the target scene.
- **MainMenuManager** - the panel-driven menu component for New Game, Load Game, Settings, Credits, Co-op, and Exit buttons.
```

- [ ] **Step 3: Replace stale LevelRegistry consumer text**

Replace the `MainMenuManager` `Level Registry` assignment claim with current scene-name wiring:

```markdown
`MainMenuManager` loads the scene named in **Game Scene Name**. Type the gameplay scene name exactly as it appears in Build Settings.
```

- [ ] **Step 4: Add menu, settings, credits, and loading wiring steps**

Add a beginner step that says:

```markdown
Create panels named `MainPanel`, `SettingsPanel`, `CreditsPanel`, and optionally `CoopPanel`. Put your credits text, studio name, tools, asset acknowledgements, and helper names inside `CreditsPanel`. Start `MainPanel` active and the other panels inactive.
```

Add:

```markdown
When you want a loading scene, call `SceneFader.FadeToSceneViaLoader(gameSceneName)` from custom code or a small menu adapter. `SceneFader` stores the target scene in `PendingScene`, loads `SceneNames.LoadingScreen`, and `LoadingScreenController` activates the final gameplay scene.
```

## Task 5: Update Readiness Docs

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`

- [ ] **Step 1: Update Game Shell audit**

Record that credits now have runtime/editor/docs coverage and that the remaining work is shell proof-scene validation, not missing first-class support.

- [ ] **Step 2: Keep status honest**

Keep Game Shell at `Guided Needs Proof` until an importable proof scene or explicit verification fixture exists.

## Task 6: Verify The Slice

**Files:**
- All files touched above.

- [ ] **Step 1: Run source scans**

Run:

```powershell
rg -n "MainMenuManager` \\(your main menu scene\\).*Level Registry|TBD|TODO|\\?\\?|placeholder" Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/Prefabs/Scene_Flow_Setup.md Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md
```

Expected: no output.

- [ ] **Step 2: Run dotnet build**

Run:

```powershell
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: build succeeds with no new errors.

- [ ] **Step 3: Run the Unity pre-scene gate**

Run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected: EditMode and PlayMode test suites pass. Existing third-party package warnings are acceptable only if there are zero errors.

## Self-Review

- Spec coverage: This plan covers the Game Shell MVP route's runtime, authoring, guidance, validation, and honest proof status.
- Placeholder scan: The plan contains no unresolved implementation placeholders.
- Type consistency: All planned fields and methods match the existing `MainMenuManager` serialized-field pattern and editor source-contract style.
