# Pyralis Player Experience Framework Design

Date: 2026-05-26

## Product Intent

Pyralis should treat hub interactions and player-facing HUD/UI as one professional experience layer, not two unrelated feature piles. A hub gives the player things to do: talk to NPCs, accept quests, use vendors, train skills, change loadout, enter portals, launch minigames, and return from missions. The HUD and panels make those actions legible: interaction prompts, dialogue UI, quest tracker, inventory/equipment views, skill tree access, rewards, notifications, and scene transition feedback.

This milestone is named `Player Experience Framework` so the hub spine is designed with HUD consumption from day one. Implementation should still start with the hub interaction spine, because HUD panels need stable runtime concepts to display and operate.

## Current Foundations

Already present:

- RPG owner state: `RpgOwnerKey`, progression, inventory, equipment, skill trees, quests, NPC profiles, and dialogue graphs.
- Narrative runtime and authoring: `DialogueService`, `DialogueGraphDefinition`, and the first `RPG Narrative Editor`.
- Scene flow: `ISceneNavigator`, `SceneLoader`, `SceneFader`, loading screen, main menu, level registry, and setup guidance.
- HUD fragments: arcade `UIManager`, `ParticipantHealthPanel`, `ParticipantHealthHudBinder`, `ParticipantFeedbackHudPresenter`, `ParticipantTimedTextPanel`, settings screens, world health bars, TextMeshPro feedback, and Canvas setup docs.
- Validation style: runtime validation providers, custom guided inspectors, EditMode tests, PlayMode tests, and the pre-scene validation gate.

## Architecture Decision

Design Hub and HUD together. Implement Hub first.

The hub spine should live in core/data surfaces that do not depend on a specific UI package. HUD/UI should subscribe to view-model-like events and panel requests rather than reaching directly into ScriptableObjects or scene objects. This keeps the same interaction definition usable by:

- a side-scrolling brawler hub
- a fantasy RPG village
- a tabletop campaign room
- a survival base
- a minigame arcade lobby
- a dream-world transition space

## Library And UI Stack Decision

Use the existing Unity UI/uGUI + TextMeshPro path for first runtime surfaces because the package already ships Canvas-based HUD, settings, feedback, and menu components. Keep contracts UI-stack-neutral so a UI Toolkit implementation can be added later.

Unity 6 supports runtime UI Toolkit and Unity is investing in it for scalable UI, but the current package has working Canvas/uGUI conventions, editor guidance, and TextMeshPro-based HUD pieces. A forced migration now would slow the hub/HUD milestone and risk breaking existing beginner setup paths. The framework should expose stable presenters and panel routes first; individual UI implementations can be uGUI, UI Toolkit, project custom UI, or external UI packages later.

## Milestone Shape

### Phase 7A: Hub Interaction Spine

Goal: define what a hub is and what a player can interact with before building full screens.

Core concepts:

- `HubDefinition`: stable hub id, display name, scene id, default return point, optional tags.
- `HubInteractableDefinition`: stable interactable id, display name, prompt text, icon id, interaction kind, linked target ids, conditions, effects, and panel route.
- `HubInteractionKind`: NPCDialogue, QuestBoard, Vendor, Trainer, Portal, LoadoutStation, LoreReader, MinigameEntrance, Custom.
- `HubInteractionCondition`: quest status, item count, skill unlock, dialogue flag, project flag, faction, always, custom.
- `HubInteractionEffect`: start dialogue, open panel route, start quest, report objective, grant reward, set flag, navigate scene, custom event.
- `HubInteractionService`: evaluates available interactions for an RPG owner and resolves selected interactions into results.
- `HubInteractionResult`: status, issue, prompt payload, panel route, dialogue graph id, scene navigation request, rewards, and feedback messages.

HUD-ready data must be first-class:

- display name
- short prompt text
- locked prompt text
- icon id
- panel route
- priority/order
- notification text
- optional cooldown or repeat policy fields reserved for future use

### Phase 7B: Player Experience Event And Panel Contracts

Goal: make hub interactions request UI without knowing the UI implementation.

Core concepts:

- `PlayerExperienceEvent`: neutral event record for prompt shown, prompt hidden, panel requested, notification requested, reward shown, scene launch requested, dialogue requested.
- `IPlayerExperienceEventSink`: dispatch point for project UI, stock uGUI presenters, or future UI Toolkit presenters.
- `PlayerPanelRoute`: Dialogue, QuestLog, QuestBoard, Inventory, Equipment, SkillTree, Vendor, Trainer, Loadout, Portal, Rewards, Settings, Custom.
- `PlayerPromptPayload`: text, icon id, owner, interactable id, locked/unlocked state.
- `PlayerNotificationPayload`: title, body, icon id, severity, duration.

The first implementation can be in-memory and synchronous. It does not need a full event bus package yet. If event traffic grows, the interface can later be backed by a stronger messaging system.

### Phase 7C: Stock HUD And Panel Presenters

Goal: provide a professional starter surface that projects can use or replace.

Initial stock uGUI presenters:

- interaction prompt presenter
- notification/toast presenter
- dialogue panel presenter that consumes `DialogueService`
- quest tracker presenter
- reward summary presenter
- panel router that opens one stock panel at a time

Later presenters:

- inventory panel
- equipment/loadout panel
- skill tree panel
- vendor/trainer panel
- portal/minigame launcher panel
- codex/lore reader panel

These should be modular components, not one giant HUD manager. Existing health and feedback HUD components remain valid and can be composed into the same Canvas.

### Phase 7D: Hub Authoring And Validation

Goal: make hub setup creator-friendly.

Authoring assets:

- `HubDefinition`
- `HubInteractableDefinition`
- optional `HubExperienceProfile` for stock UI panel mapping, prompt style, notification style, and default panel routes

Guidance:

- custom inspectors explain hub versus menu versus open world
- create-asset menu coverage
- validation issues for missing ids, duplicate interactables, missing prompt text, invalid target ids, missing scene names, missing dialogue graphs, missing panel routes, and impossible condition/effect combinations

### Phase 7E: Proof Route

Goal: prove the framework through one narrow but real hub route.

Proof scenario:

- one hub definition
- one NPC dialogue interaction
- one quest board interaction
- one portal/minigame entrance
- one loadout or skill trainer interaction
- one interaction prompt
- one notification/reward display
- one scene navigation request through existing `ISceneNavigator`

The proof can begin as runtime and editor tests, then graduate to a sample scene when the hub and HUD contracts are stable.

## Data Flow

1. Player enters or focuses an interactable.
2. Scene adapter sends owner and interactable id to `HubInteractionService`.
3. Service evaluates conditions against existing RPG services and returns a `HubInteractionResult`.
4. Prompt presenter receives prompt payload through `IPlayerExperienceEventSink`.
5. Player confirms interaction.
6. Service applies effects or emits panel/dialogue/navigation requests.
7. UI presenter opens the requested panel or forwards scene navigation to `ISceneNavigator`.
8. Rewards, quest updates, or state changes publish notification payloads.

## Error Handling And Validation

Runtime service methods should return explicit result statuses instead of throwing for normal authoring mistakes. Validation should catch:

- empty hub/interactable ids
- duplicate ids
- missing display or prompt text
- missing scene id for portal/minigame interactions
- missing dialogue graph for NPCDialogue interactions
- missing panel route for panel-only interactions
- condition/effect target ids required by their kind
- references to unavailable quest/item/skill/dialogue ids where the relevant catalog is assigned

Locked interactions should be representable, not only hidden. Designers need both modes: show a locked portal with a hint, or hide a secret trainer until conditions are met.

## Testing Strategy

Phase 7A should use TDD:

- runtime tests for available/locked/hidden interactions
- runtime tests for scene navigation request results without loading scenes
- runtime tests for dialogue and panel route requests
- editor tests for hub and interactable definition validation
- source contract tests for guided inspectors and create-asset menu coverage if needed

Later HUD phases should add:

- presenter validation tests
- PlayMode tests for prompt show/hide and panel route changes
- sample-scene validation once stock prefabs exist

The full project gate remains:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

## Scope Boundaries

In scope for the next implementation plan:

- Phase 7A hub runtime/data contracts
- hub interactable authoring definitions
- validation and guided inspectors
- tests and docs
- HUD-ready payload fields and result types

Out of scope for the first implementation slice:

- full stock inventory/equipment/skill tree UI panels
- final polished hub scene
- save/load persistence
- localization pipeline
- multiplayer replication of hub interactions
- drag-and-drop visual hub editor

These are not rejected; they are later subphases on the same Player Experience Framework.

## AAA Quality Bar

This framework should feel like studio infrastructure, not a sample script:

- stable ids and deterministic results
- participant-owned, actor-agnostic runtime state
- UI-stack-neutral contracts
- guided authoring assets
- validation before Play Mode
- clean tests for every behavior slice
- docs that explain creator workflow and platform boundaries
- room for custom game-specific UI without forking core runtime

## Recommended Next Step

Write the Phase 7A implementation plan for the Hub Interaction Spine. Build the core/data/editor/test foundation first, with HUD-ready payloads included from the start.
