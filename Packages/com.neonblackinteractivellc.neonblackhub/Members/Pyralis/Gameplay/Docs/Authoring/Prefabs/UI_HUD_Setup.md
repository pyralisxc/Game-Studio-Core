# UI and HUD Setup - Step-by-Step

Covers `UIManager`, the HUD panel, and the game-over panel. Used in 2D arcade-style scenes.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Scoring/Objectives for score, timer, round result, and victory display
- Realtime Character when HUD state follows pawn health, combat, pickups, hazards, or respawn
- Board/Card/Tabletop or Turn/Menu Action when UI represents cursor selection, card zones, action menus, or round phases without a pawn controller

Resolve setup-profile validation before wiring HUD labels, score services, game-over panels, settings screens, or scene navigation buttons.

---

## Concepts

- **UIManager** - the HUD coordinator for live score, time, and game-over presentation. It subscribes to `ParticipantScoreService` during startup. Assign a gameplay session source when the UI owns game-over, restart, main-menu, settings, or time-flow behavior; a score-only HUD can leave the gameplay session source empty.
- The Canvas must be **Screen Space - Overlay** mode.

---

## Step 1 - Create the Canvas

1. In the Hierarchy, right-click → **UI → Canvas**.
2. Set **Render Mode** to `Screen Space - Overlay`.
3. Ensure the Canvas has an `EventSystem` in the scene (Unity creates one automatically with the first Canvas).

---

## Step 2 - Build the HUD panel

1. Inside the Canvas, right-click → **Create Empty**. Rename it `HUDPanel`.
2. Add the following children:
   - A **TextMeshPro - Text (UI)** object named `ScoreLabel`. Position it top-left or top-center.
   - A **TextMeshPro - Text (UI)** object named `TimeLabel`. Position it top-right or below the score.
   - A **Button** named `SettingsButton` (optional) - gear icon for opening settings.

---

## Step 3 - Build the game-over panel

1. Inside the Canvas, right-click → **Create Empty**. Rename it `GameOverPanel`.
2. Add the following children:
   - A **TextMeshPro - Text (UI)** named `FinalScoreLabel` - shows the player's final score.
   - A **TextMeshPro - Text (UI)** named `HighScoreLabel` - shows the all-time high score.
   - A **Button** named `RestartButton`.
   - A **Button** named `MainMenuButton`.
3. Set `GameOverPanel` to **inactive** by default (uncheck the tick next to its name in the Inspector). UIManager shows it when game over fires.

---

## Step 4 - Add UIManager to the Canvas

1. Select your Canvas GameObject.
2. Add Component → `UIManager`.
3. Wire all fields in the Inspector:

**Panels**
- **HUD Panel** - drag `HUDPanel`.
- **Game Over Panel** - drag `GameOverPanel`.

**HUD Labels**
- **Score Label** - drag `ScoreLabel` TMP object.
- **Time Label** - drag `TimeLabel` TMP object.

**Game Over Labels**
- **Final Score Label** - drag `FinalScoreLabel` TMP object.
- **High Score Label** - drag `HighScoreLabel` TMP object.

**Game Over Buttons**
- **Restart Button** - drag `RestartButton`.
- **Main Menu Button** - drag `MainMenuButton`.

**Settings** (optional)
- **Settings Button** - drag `SettingsButton`.
- **Settings Screen** - drag the `SettingsScreen` component (see [Settings_Setup.md](Settings_Setup.md)).

**Runtime Services**
- **Gameplay Session Source** - drag the session-flow component for this scene when the HUD includes game-over, restart, main-menu, settings, or time-flow behavior. The 2D `GameManager` implements `IGameplaySessionFlow`. Score-only HUDs can leave this empty.
- **Score Service Source** - drag `ParticipantScoreService`, or another component implementing `ISessionScoreService`.

**HUD Format**
- **Time Prefix** - text before the time readout (e.g. `Time: `).
- **Score Prefix** - text before the score readout (e.g. `Points: `).

---

## Step 5 - Add EventSystem and Physics Raycaster (if needed)

If buttons are not responding to clicks:

1. Make sure the scene has an `EventSystem` object (usually created automatically).
2. If using `Camera.main` rendering, ensure the Camera has a **Physics Raycaster** component.
3. For Screen Space Overlay canvases this is not required - the Canvas handles its own raycasting via the default **Graphic Raycaster** component on the Canvas.

---

## Step 6 - How UIManager subscribes to events

At `Start`, `UIManager` resolves the assigned `ISessionScoreService` and pushes the current score into the score label. When a gameplay session source is assigned, it also subscribes to `IGameplaySessionFlow` state changes and updates HUD/game-over panels and time labels.

For the stock 2D arcade path, assign the scene `GameManager` as Gameplay Session Source and `ParticipantScoreService` as Score Service Source. For a first pickup/score proof, assign only `ParticipantScoreService` and a score label. For custom game modes, assign project-owned components that implement those contracts.

---

## Optional - Add RPG hub prompts

Use this when a hub, town, camp, menu-room, or open-world safe area exposes NPC dialogue, quest boards, vendors, loadout stations, portals, or minigame entrances.

1. Inside the Canvas, create a prompt group such as `HubPromptPanel`.
2. Add a TextMeshProUGUI label for the current prompt.
3. Add optional TextMeshProUGUI labels for prompt count, route text, issue text, notification title, and notification body.
4. Add optional Select, Next, and Previous buttons.
5. Add Component -> `HubInteractionHudPresenter`.
6. Assign Prompt Label, Select Button, Next Button, Previous Button, notification labels, route label, and issue label as needed.

`HubInteractionHudPresenter` does not evaluate hub rules by itself. Add `HubInteractionSceneController` to bridge the scene to `HubInteractionService`.

For a trigger-driven hub:

1. Add `HubInteractionSceneController` to the hub trigger object.
2. Assign the `HubDefinition` asset.
3. Assign the `HubInteractionHudPresenter`.
4. Set Owner Kind to `Participant` and Owner Stable Id to the owner state key, such as `seat-1`.
5. Add a Collider or Collider2D and set **Is Trigger = true**.
6. Keep Refresh On Trigger Enter enabled and Clear On Trigger Exit enabled.

For an input-driven hub or NPC:

1. Put `HubInteractionSceneController` beside an actor interaction feature or call `RefreshAvailableInteractions()` from custom input.
2. Let the HUD Select Button call through the presenter's selected prompt, or call `ConfirmInteractable(interactableId)` directly.
3. Read `LastResult`, `RequestedSceneId`, `RequestedDialogueGraphId`, `RequestedNpcId`, and `ActivePanelRoute` from the controller/presenter result state to open panels, dialogue, or scene-flow bridges.

To open stock RPG panels:

1. Create a panel GameObject for each route you want to support, such as `DialoguePanel`, `QuestBoardPanel`, `VendorPanel`, `LoadoutPanel`, and `SkillTreePanel`.
2. Add `RpgPanelRoutePresenter` to each panel.
3. Set Route to the matching `PlayerPanelRoute`.
4. Assign Panel Root, or place the presenter directly on the panel root.
5. Assign optional Title, Body, and Context labels.
6. Set each panel inactive by default.
7. Add `RpgHubPanelRouter` near the HUD presenter.
8. Assign Hub Interaction HUD Presenter and add each route presenter to Route Presenters.

The router listens for selected hub results and opens the matching route panel. It now supports rich dialogue bodies through `RpgDialoguePanelPresenter`, quest board bodies through `RpgQuestBoardPanelPresenter`, inventory-backed shop bodies through `RpgVendorPanelPresenter`, equipment editing bodies through `RpgLoadoutPanelPresenter`, and skill unlock bodies through `RpgSkillTreePanelPresenter`.

To make the Dialogue panel playable:

1. Put `RpgDialoguePanelPresenter` inside the `DialoguePanel` route root.
2. Assign the `RpgPanelRoutePresenter` whose Route is `Dialogue`, or keep the dialogue presenter under that route object for discovery.
3. Assign one or more `DialogueGraphDefinition` assets.
4. Assign matching `NpcDefinition` assets when you want friendly speaker display names and validation.
5. Add TextMeshProUGUI labels for speaker, line text, choices, and issues as desired.
6. Add a Continue button for line nodes that use `Next Node Id`.
7. Add enough Choice buttons for the largest choice hub you expect in the conversation.

When a hub interaction selects a Dialogue route with `Dialogue Graph Id` and `NPC Id`, the panel presenter starts a native `DialogueService` session, renders the active node, fills choice buttons from available choices, and advances through `Continue()` or `SelectChoice(...)`.

To make the Quest Board panel playable:

1. Put `RpgQuestBoardPanelPresenter` inside the `QuestBoardPanel` route root.
2. Assign the `RpgPanelRoutePresenter` whose Route is `QuestBoard`, or keep the quest board presenter under that route object for discovery.
3. Assign one or more `QuestDefinition` assets.
4. Add TextMeshProUGUI labels for the board list, selected quest, selected status, and issues as desired.
5. Add an Accept button that calls `StartSelectedQuest()`.
6. Add Next and Previous buttons when the board offers several quests.

When a hub interaction selects a QuestBoard route, the panel presenter lists the board quests, shows the RPG owner's current quest status, and starts the selected quest through `QuestService`.

To make the Vendor panel playable:

1. Create `VendorDefinition` assets for each shopkeeper or shop station.
2. Add offers with Offer Id, Item Id, Currency Item Id, Buy Price, Sell Price, and buy/sell toggles.
3. Put `RpgVendorPanelPresenter` inside the `VendorPanel` route root.
4. Assign the `RpgPanelRoutePresenter` whose Route is `Vendor`, or keep the vendor presenter under that route object for discovery.
5. Assign the Vendor Definition assets the panel can open.
6. Add TextMeshProUGUI labels for vendor name, offer list, selected offer, and issues as desired.
7. Add Buy, Sell, Next, and Previous buttons.
8. Make the hub Vendor interaction's NPC Id match the `VendorDefinition.VendorId`, such as `vendor.apothecary`.

Vendor prices use inventory item ids as currency for this first production-safe pass. For example, a potion offer can cost `3 item.gold`, where `item.gold` is a normal `ItemDefinition` stored in `InventoryService`.

To make the Loadout panel playable:

1. Create `EquipmentSlotDefinition` assets for each visible slot, such as `slot.weapon`, `slot.armor`, `slot.cape`, or `slot.charm`.
2. Create `EquippableItemDefinition` assets and set Allowed Slot Ids to the slots they can occupy.
3. Put `RpgLoadoutPanelPresenter` inside the `LoadoutPanel` route root.
4. Assign the `RpgPanelRoutePresenter` whose Route is `Loadout`, or keep the loadout presenter under that route object for discovery.
5. Assign the slot definitions and equippable item definitions this loadout can present.
6. Add TextMeshProUGUI labels for loadout list, selected item, equipped slots, and issues as desired.
7. Add Equip, Unequip, Next, and Previous buttons.

The first loadout body writes directly to `EquipmentService`. It treats the assigned equippable definitions as the visible gear catalog, shows compatible slot display names, equips the selected item into the first compatible configured slot, and refreshes equipped/available state after each change.

To make the Skill Tree or Trainer panel playable:

1. Create `SkillTreeDefinition` assets with stable node ids, display names, costs, prerequisites, repeatable flags, and optional stat modifiers.
2. Put `RpgSkillTreePanelPresenter` inside a `SkillTreePanel` or `TrainerPanel` route root.
3. Assign the `RpgPanelRoutePresenter` whose Route is `SkillTree` or `Trainer`, or keep the presenter under that route object for discovery.
4. Assign the Skill Tree Definition assets this panel can present.
5. Add TextMeshProUGUI labels for tree name, skill points, node list, selected node, and issues as desired.
6. Add Unlock, Next Node, Previous Node, Next Tree, and Previous Tree buttons.

The first skill tree body writes directly to `SkillTreeService` and reads available points from `ProgressionService`. A Trainer hub interaction can select a specific tree when its NPC Id or Interactable Id matches a `SkillTreeDefinition.treeId`; otherwise the panel opens the first assigned tree.

To reproduce the RPG proof route:

1. Create a `HubDefinition` asset with stable ids for dialogue, quest board, vendor, loadout, trainer, and portal prompts.
2. Wire `HubInteractionSceneController`, `HubInteractionHudPresenter`, `RpgHubPanelRouter`, and route presenters for Dialogue, QuestBoard, Vendor, Loadout, Trainer, and Portal flow.
3. Configure panel bodies with matching data:
   - Dialogue graph id: `dialogue.rpg-proof.elder`
   - Quest id: `quest.rpg-proof.herbs`
   - Vendor id: `vendor.rpg-proof.apothecary`
   - Weapon slot id: `slot.rpg-proof.weapon`
   - Skill tree id: `tree.rpg-proof.hero`
4. Seed the owner inventory with `item.rpg-proof.gold` and the owner progression with at least one skill point.
5. Select prompts in this order to verify the full route: Elder dialogue, Quest Board, Apothecary, Loadout Station, Hero Trainer, Arena Portal.

The package PlayMode proof `RpgHubProofRouteRuntimeTests` builds this route from a test fixture and verifies prompt refresh, dialogue playback, quest start, vendor purchase, loadout equip, skill unlock, and arena scene request without relying on fragile hand-edited scene YAML or production proof factories.

---

## Step 7 - Verify in Play Mode

1. Enter Play Mode. The HUD panel should be visible with `Points: 0` and the timer running.
2. Collect a pickup - score label increments immediately.
3. Trigger game over - HUD panel hides, game-over panel appears with final score and high score.
4. Click Restart - scene reloads and score resets to `0`.
5. Click Main Menu - fades to the menu scene.

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| HUD does not appear on Play | `HUD Panel` reference not wired, panel was accidentally set inactive, or Gameplay Session Source is empty |
| Score never updates | `Score Label` not assigned, Score Service Source is empty, or the assigned score service is not receiving score events |
| Buttons do not respond | No `EventSystem` in scene, or Canvas not set to Screen Space Overlay |
| High score shows `0` always | `SaveHighScore()` not called at session end - check `GameManager` game-over flow |
| Game-over panel shows on Play | Panel was left active - set it inactive in the Inspector before entering Play Mode |
| Hub prompt never appears | Hub trigger/controller is not calling `ShowPrompts()`, or Prompt Label is not assigned |
| Hub select button does nothing | Select Button is empty, no listener is subscribed to `PromptConfirmed`, or the project input bridge is not calling `ConfirmSelectedPrompt()` |
| Hub trigger does not refresh prompts | `HubInteractionSceneController` has no trigger collider, Refresh On Trigger Enter is disabled, or Hub Definition/Owner Stable Id is missing |
| Hub route label updates but no panel opens | `RpgHubPanelRouter` is missing, the selected route has no `RpgPanelRoutePresenter`, or the panel route does not match the hub interaction result |
| Dialogue panel opens but no dialogue appears | `RpgDialoguePanelPresenter` is missing, its Dialogue Graphs array does not include the selected graph id, or the hub interaction has no Dialogue Graph Id |
| Quest board opens but no quests appear | `RpgQuestBoardPanelPresenter` is missing or its Quests array is empty |
| Vendor panel opens but no offers appear | `RpgVendorPanelPresenter` is missing, its Vendors array is empty, or the hub interaction NPC Id does not match a Vendor Id |
| Vendor buy button reports missing currency | The owner inventory does not contain enough of the offer's Currency Item Id, such as `item.gold` |
| Loadout panel opens but no gear appears | `RpgLoadoutPanelPresenter` is missing, its Items array is empty, or no Equippable Item Definition assets were assigned |
| Loadout equip reports no compatible slot | The selected equippable's Allowed Slot Ids do not match any assigned Equipment Slot Definition Slot Id |
| Skill tree panel opens but no nodes appear | `RpgSkillTreePanelPresenter` is missing, its Skill Trees array is empty, or the selected tree has no nodes |
| Skill unlock button is disabled | The selected node is already unlocked, lacks prerequisites, costs more skill points than the owner has, or no ProgressionService is available for paid nodes |
