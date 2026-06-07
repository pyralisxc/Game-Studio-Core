# Computer Use With Unity

This note is for Codex agents working in Game Studio Core when the Computer Use plugin is available. It records the project-specific Unity layout and the safe operating pattern for using the real Unity Editor UI.

## Codex Unity Layout

Use the Unity layout named:

```text
Codex Unity Automation
```

The layout was created through Unity on 2026-05-29 and is stored by Unity at:

```text
%APPDATA%\Unity\Editor-5.x\Preferences\Layouts\default\Codex Unity Automation.wlt
```

Before interacting with Unity through Computer Use:

1. Activate the Game Studio Core Unity window.
2. Switch the top-right Unity layout dropdown to `Codex Unity Automation`.
3. Make any automation-driven panel, tab, or sizing changes only while that layout is active.
4. Leave Cameron's personal layouts alone.

The layout is intended to keep these surfaces visible or nearby:

- `Pyralis Authoring` on the left for route/setup guidance.
- `Scene` and `Game` views for visual/runtime checks.
- `Inspector` for selected object and asset details.
- `Console` for compile/runtime messages.
- `Project`, `Hierarchy`, and `Unity Version Control` as secondary tabs.

For first-pass test runs, prefer native Unity workflow: create scene objects from the `Hierarchy`, create assets from the selected `Project` folder, add scripts through Inspector `Add Component`, and assign references through Inspector fields. Use the Authoring Window as the route guide and validation surface, not as the place that creates or wires assets.

If the layout is missing, recreate it from a practical Unity workspace with the same surfaces and save it under the same name. Do not overwrite an unrelated layout to do that.

## What Computer Use Is Good For Here

Use Computer Use as the Unity human-path verifier. It is useful for checking whether the authored workflow makes sense in the actual Editor, especially:

- opening and navigating `NeonBlack/Gameplay/Pyralis Authoring Window`
- checking `Overview`, `Guide`, `Map`, and `Validate` modes
- verifying `GameplaySessionBootstrap` Setup Flow rows and readiness summaries
- inspecting custom Inspector guidance and validation messages
- reviewing Console errors/warnings after a refresh or Play Mode attempt
- checking Scene/Game view state when visual evidence matters
- testing Project-window starter-pack creation and Authoring Window guidance carefully

Prefer code search, C# inspection, docs, tests, and project validation for structural work. Computer Use complements those checks; it does not replace them.

## Safe Operating Loop

1. Use `list_apps` and select the Unity window whose title contains `Game Studio Core`.
2. Use `get_window_state` before coordinate input and after each meaningful UI change.
3. Prefer menus, keyboard shortcuts, and visible stable coordinates over blind repeated clicking.
4. Keep actions small: open a menu, select one tab, inspect the result, then continue.
5. Treat modal dialogs, package import progress, compilation, and Play Mode transitions as long-running states to monitor.
6. Do not automate sign-in, licensing, account, security, or privacy prompts.
7. Do not use Computer Use to run terminal commands or bypass Windows/Unity safety prompts.

If Computer Use reports recent user input or interruption, stop input actions and take a fresh window state before continuing.

## Unity Project Reality Checks

Computer Use can see the Editor, but Unity's UI is a large custom surface. Accessibility labels often expose major panes and menus but not every field or docked control. When labels are incomplete, use screenshots and stable coordinates carefully, then verify visually.

Trust hierarchy:

1. Unity Test Runner XML summaries under `Logs\Codex`.
2. Project validation gate output from `Tools\Validation\Run-PreSceneValidation.ps1`.
3. Editor log evidence and Console state.
4. Computer Use screenshots for visual/UI workflow proof.

When the GUI Editor is open, do not claim full pre-scene validation passed unless the project gate or Unity Test Runner XML proves it. For full validation, close the GUI Editor and run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

## Current Known Editor Context

On 2026-05-29, Computer Use successfully targeted the Unity window:

```text
Game Studio Core - Untitled - Windows, Mac, Linux - Unity 6.4 (6000.4.0f1) <DX11>
```

It could capture the Editor with `Pyralis Authoring`, `Scene`, `Inspector`, `Console`, `Project`, and `Unity Version Control` visible. This confirms that Computer Use is viable for Unity UI verification in this project when the desktop is unlocked and the tool is not interrupted by active user input.
