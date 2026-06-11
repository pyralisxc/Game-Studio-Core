# Project Overview
- Game Title: Pyralis / Neon Black (Package)
- High-Level Concept: An authoring tool and engine core focused on reflective discovery of game capabilities and intents.
- Players: Developers / Content Authors
- Inspiration / Reference Games: N/A (Tooling)
- Tone / Art Direction: Neon / Cyberpunk UI
- Target Platform: Unity Editor
- Render Pipeline: Built-in (based on project settings)

# Goals: Maintenance, Consolidation, and Doc Truth
This plan targets the refactoring of the Pyralis Authoring system to improve long-term maintenance, ensure a singular source of truth for metadata (Doc Truth), and modernize the UI.

## Current Pain Points
1. **The Monolith**: `PyralisAuthoringWindow.cs` is a ~4000 line IMGUI script that is difficult to navigate and maintain.
2. **Consolidated UI**: The "Capabilities" scroll view is too dense and lacks visual hierarchy, making it hard to "author intent" clearly.
3. **Doc Truth Drift**: Documentation and metadata are scattered across attributes, registries, and advisor logic.

# Phased Implementation Plan

## Phase 1: Consolidation of Truth (Metadata Architecture)
- **Description**: Ensure all "Doc Truth" (documentation, tooltips, categorization) is defined in attributes or central registries.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No
- **Action Items**:
    - Update `AuthoringContractAttribute` to include a `DocumentationURL` and `ExpertAdvice` field.
    - Move all "Reasoning" strings from `PyralisAuthoringIntentAdvisor` into the data-providing attributes or registries.
    - Ensure `AuthoringCapabilityRegistry` is the sole provider of capability metadata.

## Phase 2: UI Toolkit Infrastructure
- **Description**: Transition the authoring window from IMGUI to UI Toolkit.
- **Assigned role**: developer
- **Dependencies**: Phase 1
- **Parallelizable**: Yes
- **Action Items**:
    - Create `PyralisAuthoringWindow.uxml` and `PyralisAuthoringWindow.uss`.
    - Implement a "Semantic Layout":
        - **DNA Section**: World Axioms (Dimensions, Gravity, etc.)
        - **Spine Section**: Engine Capabilities (Combat, Movement, etc.)
        - **Lane Section**: Presentation Modes (2D, 3D, etc.)
    - Use "Cards" for the Intent Advisor recommendations to improve readability.

## Phase 3: Enhanced Capabilities Scroll View
- **Description**: Refactor the consolidated scroll view into a categorized, hierarchical interface.
- **Assigned role**: developer
- **Dependencies**: Phase 2
- **Parallelizable**: Yes
- **Action Items**:
    - Create a `CapabilitiesCategoryElement` that groups toggles by their "Spine" category.
    - Implement search/filtering for capabilities.
    - Add "Quick-Select" presets based on common Route Intents (e.g., "Top-Down Brawler").

## Phase 4: Reflective Hygiene & Automated Docs
- **Description**: Use the "Doc Truth" to drive validation and external documentation.
- **Assigned role**: explorer / developer
- **Dependencies**: Phase 1
- **Parallelizable**: Yes
- **Action Items**:
    - Implement a `PyralisDocGenerator` utility that outputs a README.md based on the discovered `AuthoringContracts`.
    - Enhance `PyralisAuthoringIntentAdvisor.ValidateHygiene` to flag contracts missing "Relevance" or "Documentation" metadata.

# Key Asset & Context
- `PyralisAuthoringWindow.cs`: Target for refactoring.
- `AuthoringContractAttribute.cs`: The "Source of Truth" attribute.
- `PyralisAuthoringIntentAdvisor.cs`: Logic to be decoupled from the UI.
- `PyralisAuthoringFactRegistry.cs`: Discovery hub.

# Verification & Testing
- **Unit Tests**: Verify that `PyralisReflectiveFactScanner` correctly picks up new metadata fields in Phase 1.
- **Layout Check**: Manual verification of the UI Toolkit layout across different window sizes.
- **Truth Check**: Verify that changing a "Relevance" string in an attribute immediately reflects in the UI Tooltip and the Hygiene report.
