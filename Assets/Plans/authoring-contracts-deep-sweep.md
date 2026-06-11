# Authoring Contracts Deep Sweep Plan

## Phase 1: Core & Shell (Setup, Session, Networking, Input)
- Update **InputConfig**, **LocalParticipantAuthorityService**, **LocalSessionOwnershipService**, **SceneLoader**, **SceneNavigator**, **TimeManager**, **GameplaySessionBootstrap**, **SessionStateService**, **ParticipantSpawnService**, **ParticipantRosterService**, **ParticipantInputRouter**.
- Focus: Standardizing bootstrap logic and session authority guidance.

## Phase 2: Actor Physics & Movement (KineticMotor, Steering, Traversal)
- Update **PawnRoot**, **IMovementModule**, **ICharacterMotorState**, **Pawn3DInputModule**, **IActorTraversalFeature**, **PawnTraversalFeatureRuntime3D**, **Pawn3DTraversalComponent**, **TopDownHopFeatureRuntime**.
- Focus: Fine-tuning physical coordination and 2D/3D motor separation.

## Phase 3: Combat & RPG (CombatState, Sensors, Flow, RPG Services)
- Update **IActorHealthState**, **HitBox/2D**, **ProjectileLauncherBase/2D/3D**, **ProjectileFirePlanner**, **ActorCombatReactionFeatureRuntime**, **ActorStatusEffectFeatureRuntime**, **BattleManager**, **DialogueService**, **InventoryService**, **QuestService**, **ProgressionService**, **SkillTreeService**, **VendorService**, **EquipmentService**.
- Focus: Filling out the 'Action' layer with hit-registration and narrative-flow guidance.

## Phase 4: Presentation & Data (Animation, VFX, UI, Profiles)
- Update **IActorAnimationController**, **ActorAnimationDefinition**, **ActorFeedbackProfile**, **HazardFeedbackProfile**, **SpriteFlasher**, **CameraShake**, **ActorShadowDriver**, **UIOrientationHandler**, **LoadingScreenController**, **MainMenuManager**, **SettingsManager**.
- Focus: Visual feedback loops and data-driven configuration pro-tips.

## Phase 5: World & Tabletop (Environment, Tabletop, Grid)
- Update **LevelData**, **LevelRegistry**, **LevelSession**, **TabletopBoardGridPresenter**, **TabletopTurnStatusPresenter**, **BoardRuntimeState**, **TabletopBoardSelectionBridge**, **ITurnOrderService**, **TurnRuntimeState**.
- Focus: Strategic and world-navigation logic.
