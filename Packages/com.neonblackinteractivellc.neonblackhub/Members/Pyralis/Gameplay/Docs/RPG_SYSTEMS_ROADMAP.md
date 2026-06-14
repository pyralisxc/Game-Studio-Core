# Pyralis RPG Systems Platform Roadmap

Date: 2026-05-26

This roadmap makes RPG capability a first-class Pyralis platform program. It is not a promise to build one fantasy RPG template. It is a plan for reusable progression, state, inventory, quest, hub, and unlock systems that can serve side-scrolling brawlers, tabletop tactics, survival loops, hub-launched minigames, action RPGs, open-zone prototypes, and hybrid games.

## Product Promise

Pyralis RPG systems should let a creator add persistent progression and authored adventure structure without replacing the existing participant, pawn, action, rules, scoring, setup, and scene-flow architecture.

The RPG layer must stay participant-owned and actor-agnostic. A participant may own RPG state while their current embodiment is a pawn, board seat, faction, cursor, menu selection, camera, NPC ally, or project-owned control surface.

## Completion Bar

Every RPG phase must satisfy the same Pyralis five-part bar before it can be called ready:

- `Runtime`: the service or runtime state executes real behavior.
- `Authoring`: ScriptableObject definitions, profiles, components, or setup assets exist for Unity creators.
- `Guidance`: inspectors and docs explain what to create, assign, and leave empty.
- `Validation`: common wrong wiring produces actionable warnings before or during first Play Mode.
- `Proof`: EditMode, PlayMode, validation gates, or proof scenes verify the route.

## Phase 1: RPG Identity, Stats, And Progression

Status: foundational code added.

Goal: create the reusable identity and numeric progression spine that inventory, equipment, skill trees, quests, and hub unlocks can share.

Required platform capabilities:

- RPG owner keys that can point at a participant, actor, pawn, board piece, NPC, faction, or custom owner id.
- `StatDefinition` authoring for base stats such as strength, speed, wisdom, health, defense, charisma, or project-owned values.
- runtime stat sheets with base values, modifiers, and derived value lookup.
- `ProgressionCurveDefinition` authoring for XP thresholds and skill point grants.
- `ProgressionService` runtime state for XP, levels, and skill points.
- clear contracts for future save/load ownership.

Ready proof:

- runtime tests for stat lookup, modifier application, XP threshold leveling, and skill point grants.
- editor tests for invalid stat and progression definitions.
- docs and guided authoring path for assigning progression state to participants or actors.

Current limitations:

- no inventory or equipment ownership yet.
- no skill tree graph authoring yet.
- no setup-flow inspector guidance yet.
- no persistence adapter yet.

## Phase 2: Inventory And Item Catalog

Status: foundational code and first native editor window added.

Goal: give all game types a shared item ownership model.

Required platform capabilities:

- `ItemDefinition` assets with stable ids, display names, categories, tags, stack rules, and rarity.
- `ItemCatalogDefinition` validation for duplicate ids and missing required authoring fields.
- per-owner `InventoryService` for add, remove, count, contains, and query operations.
- stack handling that is deterministic and easy to test.
- optional capacity policies that can be added after the basic inventory path is proven.

Ready proof:

- runtime tests for add, remove, stack, overflow rejection, and owner separation.
- editor tests for catalog and item validation.
- setup docs for brawler loot, tabletop relics, survival resources, hub keys, and quest items.

Current limitations:

- no equipment slots or item effects yet.
- no UI inventory view yet.
- no quest reward integration yet.
- no persistence adapter yet.

## Phase 3: Equipment And Effects

Status: foundational code added.

Goal: let items change gameplay through explicit, reusable effects.

Required platform capabilities:

- equipment slot definitions such as weapon, armor, cape, charm, tool, key item, or project-owned slots.
- equippable item definitions with allowed slots and stat modifiers.
- loadout state per RPG owner through `EquipmentService`.
- stat modifier effect application contracts that avoid hard dependencies on one pawn controller.
- future effect definitions that modify actions, projectile/combat/traversal behavior, unlock abilities, or set flags.

Ready proof:

- runtime tests for equip, unequip, slot replacement, stat modifier application, and rejected slot mismatches.
- editor validation for invalid equipment/effect combinations.
- documentation for equipment in brawlers, action RPGs, tabletop heroes, and hub loadout stations.

Current limitations:

- only stat modifier effects are implemented.
- no equipment inventory-consumption policy yet.
- loadout UI now exists through `RpgLoadoutPanelPresenter`; hub loadout station proof content is still needed.
- no ability/action unlock effects yet.

## Phase 4: Skill Trees

Goal: make authored unlock graphs reusable across genres.

Status: foundational code added.

Required platform capabilities:

- `SkillTreeDefinition` and skill node authoring.
- node costs, prerequisites, repeatability, visibility, and refund policy.
- unlock state per RPG owner.
- effects that grant stats, actions, abilities, equipment slots, dialogue flags, hub access, world flags, traversal powers, or project-owned rewards.
- validation for missing node ids, duplicate node ids, broken prerequisites, impossible costs, and cycles.

A skill tree node should be able to unlock a brawler combo, gun upgrade, board-piece ability, RPG spell, survival perk, hub portal, gliding move, or dialogue flag such as a mind-reading condition.

Current foundation:

- `ISkillTree`, `SkillNode`, and `SkillTreeService` provide owner-scoped unlock state without coupling Core to authoring assets.
- `SkillNodeDefinition` and `SkillTreeDefinition` provide ScriptableObject authoring with guided inspector support.
- supported node behavior includes point costs, prerequisites, repeatable unlocks, and stat modifier effects.
- current limitations: no ability/action, hub, traversal, dialogue, visibility, cycle, refund, or setup-flow routing yet.

Ready proof:

- runtime tests for prerequisite enforcement, point spending, unlock effects, and repeatable nodes.
- editor tests for malformed graph authoring.
- docs and setup guidance for beginner skill trees.

## Phase 5: Quests And Objectives

Goal: provide authored objective tracking that listens to gameplay events and grants rewards.

Status: foundational code added.

Required platform capabilities:

- quest definitions with stable ids, states, objective lists, and reward bundles.
- objective types for collect item, defeat actor, reach zone, talk to NPC, use action, complete board move, earn score, or project-owned event.
- quest progress service per RPG owner or session.
- reward grants for XP, skill points, items, equipment, hub access, world flags, and dialogue flags.

Current foundation:

- `IQuestDefinition`, `QuestObjective`, `QuestReward`, `QuestProgressState`, and `QuestService` provide owner-scoped quest progress without coupling Core to Unity authoring assets.
- `QuestDefinition`, `QuestObjectiveDefinition`, `QuestRewardDefinition`, and `QuestItemRewardDefinition` provide ScriptableObject authoring with guided inspector support.
- supported objective behavior includes collect item, defeat actor, reach zone, talk to NPC, use action, complete board move, earn score, and project-owned event payloads.
- supported reward behavior includes XP, skill points, and item rewards through existing progression and inventory services.
- current limitations: no equipment reward, hub/world/dialogue flag routing, quest UI, setup-flow validation, persistence, or event-bus adapters yet.

Ready proof:

- runtime tests for objective progress, completion, reward grants, and repeatability rules.
- editor validation for missing rewards, invalid objective payloads, and duplicate quest ids.
- docs for a one-NPC, one-quest beginner route.

## Phase 6: NPC And Dialogue Hooks

Goal: make NPC interaction a platform hook instead of a one-off dialogue implementation.

Status: foundational code added.

Required platform capabilities:

- NPC identity definitions and optional participant/actor links.
- interaction hooks that can check quests, inventory, skill unlocks, flags, faction, or project-owned conditions.
- dialogue condition and effect contracts without forcing one dialogue UI package.
- vendor, quest-giver, trainer, portal, and lore-reader use cases.

Current foundation:

- `NpcProfile`, `DialogueGraph`, `DialogueNode`, `DialogueChoice`, `DialogueCondition`, `DialogueEffect`, and `DialogueSessionState` provide the Core runtime model without coupling Core to Unity authoring assets.
- `DialogueService` provides owner-scoped sessions, dialogue flags, line-node continuation through `NextNodeId`, available-choice filtering, choice selection, and effect dispatch.
- supported conditions include item count, quest status, dialogue flags, always-true conditions, and custom condition hooks. Skill, faction, and project flags route through the custom hook until their platform services exist.
- supported effects include setting dialogue flags, starting quests, reporting quest objective progress, granting items, granting XP, granting skill points, and custom/vendor/trainer/portal hook dispatch.
- `NpcDefinition`, `DialogueGraphDefinition`, `DialogueNodeDefinition`, `DialogueChoiceDefinition`, `DialogueConditionDefinition`, and `DialogueEffectDefinition` provide guided ScriptableObject authoring.
- `DialogueGraphEditorModel` and `DialogueGraphEditorWindow` provide the first native narrative editor at `NeonBlack/Gameplay/RPG Narrative Editor`, with graph asset selection, node map, node/choice editing, validation display, and `DialogueService` preview.
- the editor intentionally uses stable IMGUI for the first production-safe pass instead of taking a dependency on Unity graph tooling; the runtime data model stays independent so a later graph-canvas UI can edit the same assets.
- current limitations: no drag-and-drop graph canvas yet, no localization/voice/lip-sync pipeline, no dedicated trainer/portal services yet, no external Yarn/Ink/Pixel Crushers adapters yet, and no persistence integration yet.

Ready proof:

- runtime tests for condition checks and effect dispatch.
- editor validation for broken NPC condition references plus editor model tests for node creation, choice creation, link cleanup, and preview readiness.
- docs for dialogue hooks, vendor hooks, and mind-read style conditions.

## Phase 7: Hub Framework

Goal: define a reusable hub setup pattern for games that launch or connect multiple gameplay modes.

Status: foundational code added.

Required platform capabilities:

- hub runtime pattern definition.
- hub entrances, portals, quest boards, vendors, loadout stations, skill trainers, and minigame launch points.
- scene-flow integration through existing shell/loading systems.
- setup validation for missing destination scenes, missing conditions, and unresolved reward targets.

Current foundation:

- `HubDefinitionModel`, `HubInteractable`, `HubInteractionCondition`, `HubInteractionEffect`, `HubPromptPayload`, `HubNotificationPayload`, and `HubInteractionResult` provide the Core runtime model without coupling the hub spine to one UI framework.
- `HubInteractionService` evaluates owner-scoped availability, visible locked prompts, hidden locked interactions, selected interactions, panel routes, dialogue ids, scene navigation requests, notifications, and basic item/quest/dialogue-flag effects.
- `HubHudPromptList` and `HubHudPresentationState` provide a UI-neutral HUD state layer for sorted prompts, selected prompt navigation, panel requests, dialogue requests, scene requests, issues, and notifications.
- `HubInteractionHudPresenter` provides the first stock uGUI/TextMeshPro prompt surface with guided inspector support, button-driven prompt selection, locked prompt display, route/issue labels, and notification labels.
- `HubInteractionSceneController` bridges authored hub definitions, scene trigger/input events, actor interaction handlers, `HubInteractionService`, and the HUD prompt presenter.
- `RpgHubPanelRouter` and `RpgPanelRoutePresenter` provide the first stock routed panel surfaces for Dialogue, QuestBoard, Vendor, Loadout, SkillTree, Trainer, and other `PlayerPanelRoute` values.
- `RpgDialoguePanelPresenter` provides the first rich routed panel body: it starts native `DialogueService` sessions from selected hub results, renders the current node, exposes available choices, and advances through Continue or Choice buttons.
- `RpgQuestBoardPanelPresenter` provides the first rich quest board body: it lists authored quests, reflects owner-specific quest status, and starts selected quests through `QuestService`.
- `VendorOffer`, `IVendorDefinition`, `VendorService`, `VendorDefinition`, and `RpgVendorPanelPresenter` provide the first rich vendor body: offers exchange inventory-owned currency items for shop items through real buy/sell transactions.
- `RpgLoadoutPanelPresenter` provides the first rich loadout body: it lists authored equippable items, shows compatible equipment slots, and equips or unequips selected gear through `EquipmentService`.
- `RpgSkillTreePanelPresenter` provides the first rich skill tree/trainer body: it lists authored skill nodes, shows skill points, and unlocks selected nodes through `SkillTreeService`.
- runtime tests provide an in-memory proof route for dialogue, quest board, vendor, loadout, trainer, and portal interactions without shipping proof fixture data in the production RPG feature.
- `HubDefinition`, `HubInteractableDefinition`, `HubConditionDefinition`, and `HubEffectDefinition` provide ScriptableObject authoring with guided inspector support.
- hub results are HUD-ready from day one: display prompts, locked prompts, icon ids, designer-authored priority, panel routes, scene ids, dialogue graph ids, NPC ids, and notification payloads are part of the contract.
- current limitations: imported visual sample scene, persistence of hub flags, localization, multiplayer replication, and visual hub editor are not implemented yet. QuestBoard can start quests but does not yet provide map pins, staged turn-ins, or objective event subscriptions. Vendor uses inventory item ids as currency until a richer economy service exists. Loadout uses an assigned gear catalog and does not yet enforce inventory ownership. SkillTree/Trainer uses authored node lists and does not yet provide refunds, respecs, ability/action unlock adapters, or visual graph placement in runtime UI.

Ready proof:

- PlayMode proof route covers entering a hub, selecting dialogue, accepting a quest, buying from a vendor, changing loadout, unlocking a skill, and requesting a gameplay scene. Persistence and return-state durability move to Phase 8.
- docs that explain how the hub differs from a menu, level select, or open world.

## Phase 8: Persistence

Goal: keep RPG state durable across sessions without tying the platform to one save backend.

Required platform capabilities:

- save contracts for RPG owner state.
- serializable state for stats, XP, skill unlocks, inventory, equipment, quests, flags, hub unlocks, and world flags.
- adapter seam for local JSON, PlayerPrefs bridge, cloud save, or project-owned persistence.
- migration/version fields for future schema changes.

Ready proof:

- Runtime proof now captures and reapplies `RpgOwnerSaveData` across fresh RPG services, covering progression, inventory, equipment, quests, skill unlocks, dialogue flags/session state, and hub return metadata.
- Missing/unknown-data tolerance is covered for unknown inventory items, quests, skill unlocks, dialogue flags, and missing equipment definitions.
- Pyralis owns schema-versioned save contracts and service capture/restore hooks. Project save backends own storage policy, profile slots, file/cloud transport, encryption/compression, conflict handling, and item-definition resolution.

## Phase 9: Open-Zone Readiness

Goal: support open-zone and open-world-shaped prototypes by adding state contracts before large streaming technology.

Required platform capabilities:

- zone definitions and durable zone ids.
- entrances, exits, and hub/world travel flags.
- encounter, resource, pickup, NPC, and quest-state persistence per zone.
- reset policies for roguelike runs, survival worlds, campaign hubs, and authored story zones.

Ready proof:

- Runtime proof now covers zone travel state, zone flags, encounter/resource/pickup/NPC snapshots, reset-on-run policy, and `RpgOwnerSaveData` integration.
- `RpgOpenZoneService` provides open-zone readiness only. Projects still own terrain streaming, additive scene loading, spawn transforms, resource timers, encounter spawning, and storage transport.
- Docs frame this as durable state readiness, not a full terrain streaming system.

## Phase 10: Golden RPG Sample

Goal: prove the platform through one small, complete RPG route.

Required sample:

- a hub scene
- one NPC
- one quest
- one item reward
- one skill tree node
- one equipment effect
- one gameplay entrance into a brawler, survival, tabletop, or minigame slice
- save/load proof for the RPG state touched by the sample

The sample should be small enough for a beginner to inspect and broad enough to prove that RPG systems are not tied to one genre.

Ready proof:

- Runtime proof now drives `RpgGoldenSampleRuntime` through guide dialogue, quest acceptance, vendor purchase, meadow entry, herb collection, quest completion, cape reward equipment, skill unlock, portal scene request, save capture, and restore.
- `RpgGoldenSampleFactory` provides code-backed definitions and services so the Unity visual pass can focus on scene placement, UI skin, camera, and feel.
- Imported sample scenes/prefabs are still optional visual content work on top of the verified route.

## Current Recommended Build Order

1. RPG Identity, Stats, And Progression
2. Inventory And Item Catalog
3. Equipment And Effects
4. Skill Trees
5. Quests And Objectives
6. NPC And Dialogue Hooks
7. Hub Framework
8. Persistence
9. Open-Zone Readiness
10. Golden RPG Sample - code-backed route added

Do not start Phase 2 runtime implementation until Phase 1 has tests and docs. Later phases may receive design notes earlier, but implementation should keep the runtime spine coherent and verified.
