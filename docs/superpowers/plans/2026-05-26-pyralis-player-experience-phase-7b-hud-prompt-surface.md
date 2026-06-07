# Pyralis Player Experience Phase 7B HUD Prompt Surface Implementation Plan

**Goal:** Turn the Phase 7A hub interaction spine into a first HUD-facing surface without locking Pyralis to one final UI skin.

**Architecture:** Keep prompt selection and routed result state UI-neutral in Core/Rpg, then add a uGUI/TextMeshPro presenter in the existing gameplay assembly. The presenter should be callable from input bridges, buttons, trigger volumes, NPC interactors, or future hub controllers.

## Completed Slice

- Added tests for hub prompt sorting, prompt navigation, routed interaction results, notifications, and presenter runtime validation.
- Added `HubHudPromptList` for sorted prompt state and selected prompt navigation.
- Added `HubHudPresentationState` for panel, dialogue, scene, issue, and notification state after selection.
- Extended `HubPromptPayload` with priority so HUDs can present designer-authored ordering.
- Added `HubInteractionHudPresenter` for uGUI/TextMeshPro prompt, locked, route, issue, and notification display.
- Added `HubInteractionSceneController` to bridge hub definitions, trigger/input refresh, actor interaction handling, selected prompts, and `HubInteractionService` results into the presenter.
- Added `RpgHubPanelRouter` and `RpgPanelRoutePresenter` so selected hub results open matching RPG panel shell surfaces.
- Added `RpgDialoguePanelPresenter` so the Dialogue route can start native dialogue sessions, render active lines, show choices, and advance through Continue or Choice buttons.
- Added `RpgQuestBoardPanelPresenter` so the QuestBoard route can list authored quests, show owner-specific status, and start selected quests through `QuestService`.
- Added `VendorService`, `VendorDefinition`, and `RpgVendorPanelPresenter` so the Vendor route can list offers and perform inventory-backed buy/sell transactions.
- Added `RpgLoadoutPanelPresenter` so the Loadout route can list authored gear, show compatible equipment slots, and equip/unequip selected items through `EquipmentService`.
- Added `RpgSkillTreePanelPresenter` so SkillTree and Trainer routes can list authored nodes, show skill points, and unlock selected skills through `SkillTreeService`.
- Added `RpgHubProofRouteFactory` and `RpgHubProofRouteIds` as a reusable reference route for dialogue, quest board, vendor, loadout, trainer, and portal interactions.
- Added a PlayMode proof route test that refreshes prompts, selects dialogue, starts a quest, buys from a vendor, equips loadout gear, unlocks a skill, and requests an arena scene.
- Added guided inspector support so scene authors know how to wire the HUD surface.
- Updated generated project includes for immediate compile coverage.

## Remaining Phase 7B Work

- Phase 7B is complete at the package proof-fixture level. Optional follow-up: generate or ship an imported sample scene that mirrors the proof route visually.
