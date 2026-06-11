# Project Overview
- **Game Title:** NeonBlack (Pyralis Member Package)
- **High-Level Concept:** A multi-assembly gameplay framework focusing on authoring readiness and reflective discovery for Unity creators.
- **Target Platform:** PC / Console (Built-in Pipeline)
- **Input System:** New Input System
- **Core Architecture:** VContainer, Participant/Pawn Model, Reflective Authoring Contracts.

# Game Mechanics (Spine Architecture)
## The Capability Spine
Instead of loose strings, we define a formal `AuthoringCapability` registry.
- **Nature:** A central enum or static registry defining "Legal Goals" (Combat, Movement, Narrative, Puzzle, etc.).
- **Metadata:** Each entry in the Spine includes a formal Tooltip, a "First Proof" description, and a "Recommended Provider" logic.

## Compositional Intent
- **Multi-Tagging:** The `[AuthoringContract]` will support multiple Capability tags.
- **Intersection Discovery:** If a user selects both 'Combat' and 'Puzzle', the system reflectively finds scripts tagged with both, prioritizing them as "Bridge" capabilities.

# UI (Intent Tab Refresh)
- **Registry-First Layout:** The "Intent" tab headers are drawn from the Spine Registry (ensuring clean naming and tooltips).
- **Reflective Slotting:** Scripts are "slotted" into these headers based on their contract tags.
- **Conflict Badging:** If multiple scripts claim the same Goal/Lane combo, a "Conflict" badge appears to help identify code duplication.

# Key Asset & Context
- **`PyralisAuthoringSpine.cs`**: (New) Central registry for all capability definitions and tooltips.
- **`AuthoringContractAttribute.cs`**: (Modify) Update `Goal` to use typed Spine identifiers and support multiple tags.
- **`PyralisAuthoringContractRegistry.cs`**: (Modify) Update the harvester to group contracts by Spine identifiers.
- **`PyralisAuthoringWindow.cs`**: (Modify) Refresh the Intent tab to draw from the Spine rather than raw string discovery.

# Implementation Steps
## Phase 1: The Spine Registry
1. **Define `PyralisAuthoringCapability`**: Create a central enum or static class in `Gameplay.Core.Contracts`.
   - **Role**: Developer
   - **Dependencies**: None
2. **Implement `AuthoringCapabilityRegistry`**: A mapping system that associates each capability with a human-readable name, tooltip, and "Axiom Affinity".
   - **Role**: Developer
   - **Dependencies**: Step 1

## Phase 2: Attribute & Harvester Update
3. **Refactor `AuthoringContractAttribute`**: 
   - Change `string Goal` to `AuthoringCapability Goal` (or support an array for multiple tags).
   - Add a `Priority` field (int) to help the UI decide which script is the "Platform Default" when duplication exists.
   - **Role**: Developer
   - **Dependencies**: Step 1
4. **Update `PyralisAuthoringContractRegistry`**:
   - Refactor the `TypeCache` harvesting to use the new typed goals.
   - **Role**: Developer
   - **Dependencies**: Step 3

## Phase 3: UI & Validation
5. **Refresh `PyralisAuthoringWindow`**:
   - Update `DrawIntentGoalToggles` to iterate through the Spine.
   - Update `DrawIntentRows` to show formal tooltips from the Registry.
   - **Role**: Developer
   - **Dependencies**: Step 4
6. **Implement Duplication Validator**:
   - Add a "Hygiene" check to the Authoring Window that flags if multiple `Required` contracts are missing a single "Primary Provider."
   - **Role**: Developer
   - **Dependencies**: Step 5

# Verification & Testing
- **Manual Check:** Verify that hovering over the "Combat" goal in the Intent tab shows the definition from `PyralisAuthoringSpine`.
- **Edge Case:** Tag one script with both `Combat` and `Puzzle`. Select both in the UI and verify it appears as an "Intersection" or "High-Relevance" row.
- **Hygiene Test:** Create a temporary duplicate script for "Health" and verify the Authoring Window shows a "Conflict" issue card.
