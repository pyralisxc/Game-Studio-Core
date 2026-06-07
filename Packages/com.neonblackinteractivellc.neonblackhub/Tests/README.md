# Pyralis Test Organization

The package test suite is grouped by the kind of promise each test protects.

- `Editor/DefinitionValidationTests.cs` covers ScriptableObject sanitization and validation behavior.
- `Editor/SetupFlowValidatorTests.cs` covers guided setup route and monitor behavior.
- `Editor/ArchitectureContractTests.cs` covers assembly, folder, and domain ownership contracts.
- `Editor/AuthoringSourceContractTests.cs` covers custom inspector and authoring-window safety contracts.
- `Editor/SetupDocsContractTests.cs` covers setup documentation promises that must stay aligned with authoring tools.
- `Editor/PyralisEditorTestSupport.cs` contains shared editor-test helpers only.
- `Runtime/*Tests.cs` covers runtime systems and should stay focused on play/runtime behavior.

Avoid adding new tests to a catch-all file. If a new test protects a different kind of promise, create a purpose-named test file or extend the closest existing group.

## Verification Gates

Use `dotnet build "Game Studio Core.slnx" --no-restore` as the fast compile gate for C#/asmdef changes. It should finish with 0 project errors before handing off foundation work. Warnings from package cache or vendor code should be reviewed but are not the same as project-code failures.

Use `dotnet test "Game Studio Core.slnx" --no-build` only as a CLI smoke check unless it prints real Unity/NUnit result summaries. Unity package tests still need the Unity Test Runner for authoritative EditMode and PlayMode results.

For foundation-ready Unity work, run the project-owned pre-scene validation gate with the GUI Editor closed:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

That script first checks package portability, including the embedded `com.neonblackinteractivellc.neonblackhub` package name/version, stale `com.studiotools.core` manifest references, and stale legacy `Runtime/Members` package content. It then runs `dotnet restore`, `dotnet build`, Unity EditMode, Unity PlayMode, then a final restore/build because Unity test runs can disturb `Temp/obj`. It intentionally runs Unity Test Runner without `-quit`; Unity Test Framework 1.6 skips command-line tests when `-quit` is supplied.

If the GUI Editor is open, close it before running the full gate. Use the Unity stewardship helper's `-Mode Refresh` path only for compile/log refresh while the Editor is open.
