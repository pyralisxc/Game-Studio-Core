# Project Overview
- Game Title: Game Studio Core
- High-Level Concept: A highly modular, reflective game development engine built within Unity. It uses "Pawn Patterns" and "Capabilities" to author games without permanent scenes or hardcoded presets.
- Players: Single player / Multi-participant (Local/Networked)
- Tone / Art Direction: Tool-first, clean, highly organized.
- Target Platform: PC (StandaloneWindows64)
- Screen Orientation / Resolution: Landscape (1920x1080)
- Render Pipeline: Built-in (with flexibility for URP/HDRP)

# Game Mechanics (Authoring Evolution)
## Core Gameplay Loop (Authoring)
The primary "gameplay" of the Studio Core is the authoring process:
1. **Define Intent**: Set granular World Axioms (Gravity, Dimensions, Time-scale).
2. **Reflective Discovery**: The system scans the project for any code matching that intent.
3. **Route Construction**: The user follows "Guide" cards to wire ScriptableObject definitions and profiles.
4. **Proof Validation**: The system verifies the setup is ready for Play Mode "Proof."

## Controls and Input Methods
- **Authoring Window**: The primary interface for project-wide guidance.
- **Reflective Tags**: Developers tag C# classes with attributes to make them "discoverable."
- **Axiom Toggles**: Replaces genre-based enums with physical property toggles.

# UI
## Intent Tab Refactor
- **Physical Rules Group**: Toggles for Gravity (None, Vertical, Radial), Dimensions (2D, 3D), and Space (Bounded, Infinite, Wrapped).
- **Interaction Rules Group**: Toggles for Control Type (Direct Pawn, Cursor, Card Surface) and Turn Mode (Realtime, Turn-based, Hybrid).
- **Presentation Rules Group**: Toggles for Visual Style (Sprite, Billboard, Rigged Mesh).

## Guide Tab Refactor
- **Weighted Recommendations**: Guide cards now rank themselves based on how many selected Axioms they satisfy.
- **Contract Feedback**: If a script is missing required setup (e.g., a 2D Motor without a Floor), the Guide surfaces this as a "Logic Bug."

# Key Asset & Context
## AuthoringContractAttribute.cs
New metadata attribute used to tag classes for reflective discovery.
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class AuthoringContractAttribute : Attribute 
{
    public string Goal { get; set; }           // "Combat", "Movement", etc.
    public string Lane { get; set; }           // "Sprite2D", "Rigged3D", etc.
    public string Relevance { get; set; }      // Description for beginners.
    public string AxiomKeywords { get; set; } // Comma-separated world properties.
}
```

## PyralisAuthoringAxioms.cs
A new Flags enum or struct to store the granular project shape.
```csharp
[Flags]
public enum AuthoringWorldAxiom 
{
    None = 0,
    Dimensions2D = 1 << 0,
    Dimensions3D = 1 << 1,
    GravityVertical = 1 << 2,
    GravityNone = 1 << 3,
    Realtime = 1 << 4,
    TurnBased = 1 << 5
}
```

# Implementation Steps
## Phase 1: Research & Definition
1. **Define Axiom Vocabulary**: Map the current `WorldIntent` enums into granular `AuthoringWorldAxiom` flags. (Assigned: developer)
2. **Create Metadata Attributes**: Implement `AuthoringContractAttribute` in the Core assembly. (Assigned: developer)

## Phase 2: Reflective Scanner
1. **Implement Fact Scanner**: Create `PyralisReflectiveFactScanner` in the Editor assembly using `TypeCache`. (Assigned: developer)
2. **Bridge Legacy Facts**: Update `PyralisAuthoringFactRegistry` to combine hand-authored facts with scanned reflective facts. (Assigned: developer)

## Phase 3: UI Evolution
1. **Refactor Intent Tab**: Replace EnumPopups with Axiom toggle groups in `PyralisAuthoringWindow`. (Assigned: developer)
2. **Update Advisor Logic**: Refactor `PyralisAuthoringIntentAdvisor` to use weighted scoring against Axiom flags instead of enum comparisons. (Assigned: developer)

## Phase 4: Expansion (Dark Matter Discovery)
1. **Tag Core Interfaces**: Apply `[AuthoringContract]` to `IActionResolver`, `ITurnOrderService`, and other discovered interfaces. (Assigned: developer)
2. **Guide Card Generation**: Automatically generate Guide Cards for any tagged class that shows up in the "Facts" explorer. (Assigned: developer)

## Phase 5: Verification & Hardening
1. **Logic-as-Bug Policy**: Update `PyralisAuthoringValidationIssue` to clearly distinguish between "Setup Missing" (user error) and "Contract Violation" (code bug). (Assigned: developer)
2. **Automated Scanner Test**: Add a test in `ArchitectureContractTests.cs` that ensures every `IFeatureModuleRuntime` is tagged with an `AuthoringContract` attribute. (Assigned: developer)

# Verification & Testing
- **Fact Coverage Test**: Verify that the "Facts" tab in the Authoring Window shows at least 100+ items after the scanner is active.
- **Granular Intent Test**: Select "2D" + "No Gravity" and verify that `Motor2D` is ranked lower than a hypothetical `TopDownMotor`.
- **Contract Enforcement**: Intentionally remove an attribute and verify the `ArchitectureContractTests` fail.
