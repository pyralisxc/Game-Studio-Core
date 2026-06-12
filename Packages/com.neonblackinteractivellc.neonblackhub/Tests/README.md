# Pyralis Test Organization

The package test suite is grouped by the kind of promise each test protects.

- `Editor/*DefinitionTests.cs` covers ScriptableObject sanitization, validation, and domain definition behavior.
- `Editor/*EditorTests.cs` covers editor-only mapping, inspector, and authoring behavior that can run without Play Mode.
- `Editor/*ContractTests.cs` covers assembly, folder, authoring, MVP, roadmap, and documentation contracts.
- `Editor/*MvpContractTests.cs` covers present MVP readiness across shell, networking, tabletop/non-pawn, and related package promises.
- `Editor/Rpg*Tests.cs` covers RPG-oriented definition, editor, persistence, quest, dialogue, inventory, progression, and roadmap promises.
- `Editor/SetupFlowValidatorTests.cs` covers guided setup route and monitor behavior.
- `Editor/PyralisEditorTestSupport.cs` contains shared editor-test helpers only.
- `Members/Pyralis/Gameplay/Tests/Runtime/*` contains package-local runtime fixtures and should stay focused on runtime/reflection behavior.

Avoid adding new tests to a catch-all file. If a new test protects a different kind of promise, create a purpose-named test file or extend the closest existing group.

## Verification Gates

Use the smallest phase that protects the change.

```powershell
.\Tools\Validation\Run-PreSceneValidation.ps1 -Phase Smoke
```

Use `Smoke` for ordinary C# and folderbase cleanup. It restores and builds the fast editor test project and its dependencies without Unity, package portability, final rebuild, or residue scan.

```powershell
.\Tools\Validation\Run-PreSceneValidation.ps1 -Phase Authoring
```

Use `Authoring` after authoring-spine, inspector, route, or contract edits that do not need Play Mode. It runs the smoke build plus package portability and residue scan.

```powershell
.\Tools\Validation\Run-PreSceneValidation.ps1 -Phase Checkpoint
```

Use `Checkpoint` before a reviewable Pyralis checkpoint. It runs owned project restore/build, package portability, Unity EditMode, Unity PlayMode, and residue scan, but skips the final post-Unity rebuild because Unity batchmode can transiently desync renamed source files in generated project state.

Use `dotnet test "Game Studio Core.slnx" --no-build` only as a CLI smoke check unless it prints real Unity/NUnit result summaries. Unity package tests still need the Unity Test Runner for authoritative EditMode and PlayMode results.

Use the full gate only when validating a release-style package checkpoint or investigating final build state after Unity:

```powershell
.\Tools\Validation\Run-PreSceneValidation.ps1 -Phase Full
```

`Full` keeps the old expensive path: package portability, owned project restore/build, Unity EditMode, Unity PlayMode, final restore/build, and residue scan. It intentionally runs Unity Test Runner without `-quit`; Unity Test Framework 1.6 skips command-line tests when `-quit` is supplied.

If the GUI Editor is open, close it before running `Checkpoint` or `Full`. Use the Unity stewardship helper's `-Mode Refresh` path only for compile/log refresh while the Editor is open.
