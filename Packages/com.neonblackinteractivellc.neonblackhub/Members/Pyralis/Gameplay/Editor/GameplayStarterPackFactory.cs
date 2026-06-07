using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Traversal;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public static class GameplayStarterPackFactory
    {
        private const string DefaultStarterPackFolder = "Assets/NeonBlack/StarterPacks";
        private const string PackageSamplesFolderSegment = "/Samples~";

        public static void CreatePawnStarterPack()
        {
            string rootFolder = CreateStarterPackRootFolder("PawnStarterPack");

            string profilesFolder = CreateChildFolder(rootFolder, "Profiles");
            string definitionsFolder = CreateChildFolder(rootFolder, "Definitions");
            string prefabsFolder = CreateChildFolder(rootFolder, "Prefabs");
            string artFolder = CreateChildFolder(rootFolder, "Art");
            Sprite starterPawnSprite = CreateStarterPawnSprite(artFolder);

            InputProfile inputProfile = CreateAsset<InputProfile>(profilesFolder, "SharedInputProfile");
            InputActionAsset defaultInputActions = LoadDefaultInputActions();
            inputProfile.actions = defaultInputActions;
            inputProfile.primaryActionMap = "Player";
            inputProfile.supportsKeyboardMouse = true;
            inputProfile.supportsGamepad = true;
            inputProfile.touchFriendly = false;
            PawnMovementProfile movementProfile = CreateAsset<PawnMovementProfile>(profilesFolder, "BrawlerMovementProfile");
            PawnCombatProfile combatProfile = CreateAsset<PawnCombatProfile>(profilesFolder, "CombatProfile");
            PawnTraversalProfile traversalProfile = CreateAsset<PawnTraversalProfile>(profilesFolder, "TraversalProfile");
            PawnPresentationProfile sprite2DPresentationProfile = CreatePresentationProfile(profilesFolder, "Sprite2DPresentationProfile", ActorPresentationMode.Sprite2D);
            PawnPresentationProfile billboard25DPresentationProfile = CreatePresentationProfile(profilesFolder, "Billboard25DPresentationProfile", ActorPresentationMode.Billboard2_5D);
            PawnPresentationProfile rigged3DPresentationProfile = CreatePresentationProfile(profilesFolder, "Rigged3DPresentationProfile", ActorPresentationMode.Rigged3D);
            PawnAnimationProfile animationProfile = CreateAsset<PawnAnimationProfile>(profilesFolder, "AnimationProfile");
            PlayfieldProfile playfieldProfile = CreateAsset<PlayfieldProfile>(profilesFolder, "ArenaPlayfieldProfile");
            CameraRigProfile cameraRigProfile = CreateAsset<CameraRigProfile>(profilesFolder, "SharedCameraRigProfile");
            cameraRigProfile.orthographic = true;
            cameraRigProfile.orthographicSize = 5f;
            SettingsProfile settingsProfile = CreateAsset<SettingsProfile>(profilesFolder, "DefaultSettingsProfile");
            ActorAnimationDefinition animationDefinition = CreateAsset<ActorAnimationDefinition>(definitionsFolder, "DefaultActorAnimationDefinition");
            FireModeDefinition sampleFireMode = CreateAsset<FireModeDefinition>(definitionsFolder, "SampleHitscanFireMode");
            ProjectileImpactDefinition sampleImpact = CreateAsset<ProjectileImpactDefinition>(definitionsFolder, "SampleProjectileImpact");
            ProjectileDefinition sampleProjectile = CreateAsset<ProjectileDefinition>(definitionsFolder, "SampleHitscanProjectile");
            RuntimePatternDefinition patternRealtimeCharacter = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternRealtimeCharacter");
            RuntimePatternDefinition patternCombat = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternCombat");
            RuntimePatternDefinition patternProjectileCombat = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternProjectileCombat");
            RuntimePatternDefinition patternTurnMenuAction = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternTurnMenuAction");
            RuntimePatternDefinition patternBoardCardTabletop = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternBoardCardTabletop");
            RuntimePatternDefinition patternCameraCursorControl = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternCameraCursorControl");
            GameSetupProfile setupBrawlerWithProjectiles = CreateAsset<GameSetupProfile>(profilesFolder, "SetupBrawlerWithProjectiles");

            animationDefinition.supportedSignals = new[]
            {
                ActorAnimationSignal.Idle,
                ActorAnimationSignal.Move,
                ActorAnimationSignal.Dash,
                ActorAnimationSignal.AttackPrimary,
                ActorAnimationSignal.AttackSecondary,
                ActorAnimationSignal.AttackAerial,
                ActorAnimationSignal.BlockLoop,
                ActorAnimationSignal.Death,
                ActorAnimationSignal.Hurt,
                ActorAnimationSignal.Interact,
                ActorAnimationSignal.Jump,
                ActorAnimationSignal.Fall,
                ActorAnimationSignal.Land,
                ActorAnimationSignal.Hang,
                ActorAnimationSignal.Shimmy
            };

            animationProfile.animationDefinition = animationDefinition;

            sampleFireMode.fireModeId = "fire.sample.single";
            sampleFireMode.displayName = "Sample Single Shot";
            sampleFireMode.cooldown = 0.2f;
            sampleFireMode.clipSize = 8;
            sampleFireMode.ammoPerShot = 1;
            sampleFireMode.reloadDuration = 1.1f;
            sampleFireMode.burstCount = 1;
            sampleFireMode.projectilesPerShot = 1;

            sampleImpact.impactId = "impact.sample.projectile";
            sampleImpact.displayName = "Sample Projectile Impact";
            sampleImpact.effectLifetime = 1.5f;
            sampleImpact.applyHitPause = true;
            sampleImpact.hitPauseDuration = 0.04f;
            sampleImpact.applyCameraShake = true;
            sampleImpact.cameraShakeIntensity = 0.08f;
            sampleImpact.cameraShakeDuration = 0.08f;

            sampleProjectile.projectileId = "projectile.sample.hitscan";
            sampleProjectile.displayName = "Sample Hitscan Projectile";
            sampleProjectile.deliveryMode = ProjectileDeliveryMode.Hitscan;
            sampleProjectile.damage = 12f;
            sampleProjectile.knockback = 3f;
            sampleProjectile.maxDistance = 35f;
            sampleProjectile.lifetime = 0.1f;
            sampleProjectile.impactDefinition = sampleImpact;

            patternRealtimeCharacter.patternId = "pattern.realtime-character";
            patternRealtimeCharacter.displayName = "Realtime Character";
            patternRealtimeCharacter.description = "Pawn-backed realtime movement, combat, traversal, presentation, and input setup.";
            patternRealtimeCharacter.capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay;
            patternRealtimeCharacter.participantEmbodiment = ParticipantEmbodimentRequirement.RequiredPawn;
            patternRealtimeCharacter.supportedControlSurfaces = new[] { RuntimeControlSurface.Pawn };
            patternRealtimeCharacter.requiredRuntimeSystems = new[] { "ParticipantRosterService", "ParticipantSpawnService", "PawnRoot" };
            patternRealtimeCharacter.optionalRuntimeSystems = new[] { "ActorAnimationDriver", "CinemachineCameraRigController", "ParticipantScoreService" };
            patternRealtimeCharacter.presentationLanes = new[] { RuntimePatternPresentationLane.Sprite2D, RuntimePatternPresentationLane.Billboard2_5D, RuntimePatternPresentationLane.Rigged3D };
            patternRealtimeCharacter.firstProofRequirements = RuntimePatternFirstProofRequirement.SpawnPoints | RuntimePatternFirstProofRequirement.CameraRig;
            patternRealtimeCharacter.setupNotes = "Create a ParticipantDefinition, create a PawnDefinition, assign a pawn prefab with PawnRoot and movement/presentation components, add one or more Spawn Points to GameplaySessionBootstrap, then run Play Mode and verify one pawn spawns and responds to input.";

            patternCombat.patternId = "pattern.combat";
            patternCombat.displayName = "Combat";
            patternCombat.description = "Use this when gameplay needs attacks, hitboxes, hurtboxes, health, reactions, damage, brawler actions, fighter moves, or combat sequences.";
            patternCombat.capabilityFamily = RuntimeCapabilityFamily.Combat;
            patternCombat.participantEmbodiment = ParticipantEmbodimentRequirement.OptionalPawn;
            patternCombat.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.Pawn,
                RuntimeControlSurface.MenuSelection,
                RuntimeControlSurface.BoardPiece,
                RuntimeControlSurface.SystemAI
            };
            patternCombat.requiredRuntimeSystems = new[] { "CombatActionDefinition", "CombatSequenceDefinition", "HealthComponent and HitBox as needed" };
            patternCombat.optionalRuntimeSystems = new[] { "PawnCombatProfile", "ActorCombatReactionProfile", "WorldHealthBar", "CameraShake" };
            patternCombat.presentationLanes = new[] { RuntimePatternPresentationLane.Sprite2D, RuntimePatternPresentationLane.Billboard2_5D, RuntimePatternPresentationLane.Rigged3D, RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.UiMenu, RuntimePatternPresentationLane.CameraCursor };
            patternCombat.firstProofRequirements = RuntimePatternFirstProofRequirement.ProjectileOrHitboxSource;
            patternCombat.setupNotes = "Create CombatActionDefinition assets, group them in a CombatSequenceDefinition, assign the sequence through a PawnCombatProfile or action system, add health and hitbox components, then test one hit in Play Mode.";

            patternProjectileCombat.patternId = "pattern.projectile-combat";
            patternProjectileCombat.displayName = "Projectile Combat";
            patternProjectileCombat.description = "Reusable projectile, hitscan, ammo, fire-mode, and impact setup usable by pawns, cameras, cards, board pieces, traps, or AI.";
            patternProjectileCombat.capabilityFamily = RuntimeCapabilityFamily.GunsProjectiles;
            patternProjectileCombat.participantEmbodiment = ParticipantEmbodimentRequirement.OptionalPawn;
            patternProjectileCombat.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.Pawn,
                RuntimeControlSurface.Camera,
                RuntimeControlSurface.Cursor,
                RuntimeControlSurface.BoardPiece,
                RuntimeControlSurface.CardHand,
                RuntimeControlSurface.SystemAI
            };
            patternProjectileCombat.requiredRuntimeSystems = new[] { "ProjectileFirePlanner", "ProjectileLauncher2D or ProjectileLauncher3D" };
            patternProjectileCombat.optionalRuntimeSystems = new[] { "ProjectileMagazineState", "ProjectilePoolHandle", "ProjectileImpactEffectPlayer" };
            patternProjectileCombat.presentationLanes = new[] { RuntimePatternPresentationLane.Sprite2D, RuntimePatternPresentationLane.Billboard2_5D, RuntimePatternPresentationLane.Rigged3D, RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.CameraCursor };
            patternProjectileCombat.firstProofRequirements = RuntimePatternFirstProofRequirement.ProjectileOrHitboxSource;
            patternProjectileCombat.setupNotes = "Create a ProjectileDefinition, ProjectileImpactDefinition, and FireModeDefinition. Decide whether the shot comes from a pawn, camera, card, trap, board piece, or AI, then add the matching launcher/runtime component.";

            patternTurnMenuAction.patternId = "pattern.turn-menu-action";
            patternTurnMenuAction.displayName = "Turn/Menu Action";
            patternTurnMenuAction.description = "Command, menu, tactics, card, and action-queue style selection setup.";
            patternTurnMenuAction.capabilityFamily = RuntimeCapabilityFamily.ActionTargeting;
            patternTurnMenuAction.participantEmbodiment = ParticipantEmbodimentRequirement.OptionalPawn;
            patternTurnMenuAction.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.MenuSelection,
                RuntimeControlSurface.Cursor,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand,
                RuntimeControlSurface.Pawn
            };
            patternTurnMenuAction.requiredRuntimeSystems = new[] { "ActionDefinition", "ActionTargetRule" };
            patternTurnMenuAction.optionalRuntimeSystems = new[] { "TurnOrderService", "ActionQueue" };
            patternTurnMenuAction.presentationLanes = new[] { RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.UiMenu, RuntimePatternPresentationLane.CameraCursor, RuntimePatternPresentationLane.Sprite2D, RuntimePatternPresentationLane.Rigged3D };
            patternTurnMenuAction.firstProofRequirements = RuntimePatternFirstProofRequirement.SelectionSurface;
            patternTurnMenuAction.setupNotes = "Create ActionDefinition assets first. Decide the selection surface: menu, cursor, board space, card hand, or pawn. Add UI, turn, or action queue systems only after the basic action can be selected.";

            patternBoardCardTabletop.patternId = "pattern.board-card-tabletop";
            patternBoardCardTabletop.displayName = "Board/Card/Tabletop";
            patternBoardCardTabletop.description = "Seat, board, piece, hand, deck, zone, legal move, and tabletop-style setup.";
            patternBoardCardTabletop.capabilityFamily = RuntimeCapabilityFamily.BoardCardTabletop;
            patternBoardCardTabletop.participantEmbodiment = ParticipantEmbodimentRequirement.NonPawnSurfaceRequired;
            patternBoardCardTabletop.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.BoardPiece,
                RuntimeControlSurface.CardHand,
                RuntimeControlSurface.Cursor
            };
            patternBoardCardTabletop.requiredRuntimeSystems = new[] { "ParticipantRosterService", "ActionDefinition" };
            patternBoardCardTabletop.optionalRuntimeSystems = new[] { "TurnOrderService", "BoardStateService", "CardZoneService" };
            patternBoardCardTabletop.presentationLanes = new[] { RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.UiMenu, RuntimePatternPresentationLane.CameraCursor };
            patternBoardCardTabletop.firstProofRequirements = RuntimePatternFirstProofRequirement.TabletopRuntimeContract | RuntimePatternFirstProofRequirement.SelectionSurface;
            patternBoardCardTabletop.setupNotes = "Start without a PawnDefinition. Create participants as seats or players, add camera/cursor or UI control, create board/card zones, then add actions for legal moves or card choices.";

            patternCameraCursorControl.patternId = "pattern.camera-cursor-control";
            patternCameraCursorControl.displayName = "Camera/Cursor Control";
            patternCameraCursorControl.description = "Participant control through a camera, cursor, menu selector, faction, or commander-style surface.";
            patternCameraCursorControl.capabilityFamily = RuntimeCapabilityFamily.CameraInput;
            patternCameraCursorControl.participantEmbodiment = ParticipantEmbodimentRequirement.NonPawnSurfaceRequired;
            patternCameraCursorControl.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.Camera,
                RuntimeControlSurface.Cursor,
                RuntimeControlSurface.MenuSelection,
                RuntimeControlSurface.Faction
            };
            patternCameraCursorControl.requiredRuntimeSystems = new[] { "ParticipantInputRouter" };
            patternCameraCursorControl.optionalRuntimeSystems = new[] { "CinemachineCameraRigController", "ActionDefinition" };
            patternCameraCursorControl.presentationLanes = new[] { RuntimePatternPresentationLane.CameraCursor, RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.UiMenu };
            patternCameraCursorControl.firstProofRequirements = RuntimePatternFirstProofRequirement.CameraRig | RuntimePatternFirstProofRequirement.SelectionSurface;
            patternCameraCursorControl.setupNotes = "Create a camera or cursor control surface, assign an InputProfile, add UI or raycast selection if needed, and only add a PawnDefinition if the camera controls an actor body too.";

            patternRealtimeCharacter.recommendedCompanionPatterns = new[] { patternCombat, patternProjectileCombat };
            patternCombat.recommendedCompanionPatterns = new[] { patternRealtimeCharacter, patternProjectileCombat, patternTurnMenuAction };
            patternProjectileCombat.recommendedCompanionPatterns = new[] { patternRealtimeCharacter, patternCombat, patternTurnMenuAction, patternBoardCardTabletop, patternCameraCursorControl };
            patternTurnMenuAction.recommendedCompanionPatterns = new[] { patternCombat, patternProjectileCombat, patternBoardCardTabletop, patternCameraCursorControl };
            patternBoardCardTabletop.recommendedCompanionPatterns = new[] { patternTurnMenuAction, patternCameraCursorControl };
            patternCameraCursorControl.recommendedCompanionPatterns = new[] { patternBoardCardTabletop, patternProjectileCombat };

            setupBrawlerWithProjectiles.setupName = "Brawler With Projectiles";
            setupBrawlerWithProjectiles.summary = "A pawn-backed realtime character game that also uses the reusable projectile combat layer.";
            setupBrawlerWithProjectiles.runtimePatterns = new[] { patternRealtimeCharacter, patternCombat, patternProjectileCombat };
            setupBrawlerWithProjectiles.setupNotes = "Use this profile as a starting point for brawlers, side-scrollers, or arcade action games that need guns, spells, traps, or other projectile delivery.";

            GameModeDefinition gameModeDefinition = CreateAsset<GameModeDefinition>(definitionsFolder, "SharedModeDefinition");
            gameModeDefinition.setupProfile = setupBrawlerWithProjectiles;
            gameModeDefinition.playfieldProfile = playfieldProfile;
            gameModeDefinition.cameraRigProfile = cameraRigProfile;
            gameModeDefinition.enableCombat = true;
            gameModeDefinition.enablePickups = true;
            gameModeDefinition.enableHazards = true;

            PawnDefinition sprite2DPawnDefinition = CreatePawnDefinition(
                definitionsFolder,
                "Sprite2DPawnDefinition",
                inputProfile,
                movementProfile,
                combatProfile,
                traversalProfile,
                sprite2DPresentationProfile,
                animationProfile,
                CreateStarterPawnPrefab2D(prefabsFolder, "Sprite2DPawnPrefab", starterPawnSprite, defaultInputActions));

            PawnDefinition billboard25DPawnDefinition = CreatePawnDefinition(
                definitionsFolder,
                "Billboard25DPawnDefinition",
                inputProfile,
                movementProfile,
                combatProfile,
                traversalProfile,
                billboard25DPresentationProfile,
                animationProfile,
                CreateStarterPawnPrefab3D(prefabsFolder, "Billboard25DPawnPrefab", defaultInputActions));

            PawnDefinition rigged3DPawnDefinition = CreatePawnDefinition(
                definitionsFolder,
                "Rigged3DPawnDefinition",
                inputProfile,
                movementProfile,
                combatProfile,
                traversalProfile,
                rigged3DPresentationProfile,
                animationProfile,
                CreateStarterPawnPrefab3D(prefabsFolder, "Rigged3DPawnPrefab", defaultInputActions));

            ParticipantDefinition playerOne = CreateAsset<ParticipantDefinition>(definitionsFolder, "PlayerOneDefinition");
            playerOne.displayName = "Player One";
            playerOne.preferredSeatIndex = 0;
            playerOne.tint = Color.white;
            playerOne.defaultPawn = sprite2DPawnDefinition;
            playerOne.inputProfile = inputProfile;

            ParticipantDefinition playerTwo = CreateAsset<ParticipantDefinition>(definitionsFolder, "PlayerTwoDefinition");
            playerTwo.displayName = "Player Two";
            playerTwo.preferredSeatIndex = 1;
            playerTwo.tint = new Color(0.45f, 0.5f, 0.62f, 1f);
            playerTwo.defaultPawn = sprite2DPawnDefinition;
            playerTwo.inputProfile = inputProfile;

            SessionDefinition sessionDefinition = CreateAsset<SessionDefinition>(definitionsFolder, "SharedSessionDefinition");
            sessionDefinition.sessionName = "Local Shared Core Session";
            sessionDefinition.defaultGameMode = gameModeDefinition;
            sessionDefinition.defaultInputProfile = inputProfile;
            sessionDefinition.settingsProfile = settingsProfile;
            sessionDefinition.defaultParticipants = new[] { playerOne };
            sessionDefinition.maxParticipants = 1;

            EditorUtility.SetDirty(gameModeDefinition);
            EditorUtility.SetDirty(sprite2DPawnDefinition);
            EditorUtility.SetDirty(billboard25DPawnDefinition);
            EditorUtility.SetDirty(rigged3DPawnDefinition);
            EditorUtility.SetDirty(playerOne);
            EditorUtility.SetDirty(playerTwo);
            EditorUtility.SetDirty(sessionDefinition);
            EditorUtility.SetDirty(sampleFireMode);
            EditorUtility.SetDirty(sampleImpact);
            EditorUtility.SetDirty(sampleProjectile);
            EditorUtility.SetDirty(patternRealtimeCharacter);
            EditorUtility.SetDirty(patternCombat);
            EditorUtility.SetDirty(patternProjectileCombat);
            EditorUtility.SetDirty(patternTurnMenuAction);
            EditorUtility.SetDirty(patternBoardCardTabletop);
            EditorUtility.SetDirty(patternCameraCursorControl);
            EditorUtility.SetDirty(setupBrawlerWithProjectiles);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = sessionDefinition;
        }

        public static void CreateTabletopStarterPack()
        {
            string rootFolder = CreateStarterPackRootFolder("TabletopStarterPack");

            string profilesFolder = CreateChildFolder(rootFolder, "Profiles");
            string definitionsFolder = CreateChildFolder(rootFolder, "Definitions");
            string rulesFolder = CreateChildFolder(definitionsFolder, "Rules");
            string actionsFolder = CreateChildFolder(definitionsFolder, "Actions");

            InputProfile inputProfile = CreateAsset<InputProfile>(profilesFolder, "TabletopInputProfile");
            CameraRigProfile cameraRigProfile = CreateAsset<CameraRigProfile>(profilesFolder, "TabletopCameraRigProfile");
            SettingsProfile settingsProfile = CreateAsset<SettingsProfile>(profilesFolder, "TabletopSettingsProfile");
            BoardPieceDefinition seatOnePiece = CreateAsset<BoardPieceDefinition>(rulesFolder, "SeatOnePiece");
            BoardPieceDefinition seatTwoPiece = CreateAsset<BoardPieceDefinition>(rulesFolder, "SeatTwoPiece");
            BoardDefinition boardDefinition = CreateAsset<BoardDefinition>(rulesFolder, "StarterBoardDefinition");
            BoardMovePolicyDefinition movePolicy = CreateAsset<BoardMovePolicyDefinition>(rulesFolder, "AdjacentCaptureMovePolicy");
            BoardTerminalConditionDefinition seatOneWins = CreateAsset<BoardTerminalConditionDefinition>(rulesFolder, "SeatOneWinsWhenSeatTwoEliminated");
            BoardTerminalConditionDefinition seatTwoWins = CreateAsset<BoardTerminalConditionDefinition>(rulesFolder, "SeatTwoWinsWhenSeatOneEliminated");
            PhaseDefinition actionPhase = CreateAsset<PhaseDefinition>(rulesFolder, "ActionPhase");
            TurnOrderDefinition turnOrder = CreateAsset<TurnOrderDefinition>(rulesFolder, "TwoSeatTurnOrder");
            ActionDefinition moveAction = CreateAsset<ActionDefinition>(actionsFolder, "BoardMoveAction");

            seatOnePiece.pieceId = "piece.seatOne.starter";
            seatOnePiece.displayName = "Seat One Piece";
            seatOnePiece.pieceFamily = "Starter Tabletop";
            seatOnePiece.tags = new[] { "starter", "seat-one", "tabletop" };

            seatTwoPiece.pieceId = "piece.seatTwo.starter";
            seatTwoPiece.displayName = "Seat Two Piece";
            seatTwoPiece.pieceFamily = "Starter Tabletop";
            seatTwoPiece.tags = new[] { "starter", "seat-two", "tabletop" };

            boardDefinition.boardId = "board.tabletop.starter";
            boardDefinition.displayName = "Starter 4x4 Board";
            boardDefinition.width = 4;
            boardDefinition.height = 4;
            boardDefinition.startingPieces = new[]
            {
                new BoardStartingPiece("seat-one-piece", seatOnePiece, 0, new BoardCoordinate(0, 0)),
                new BoardStartingPiece("seat-two-piece", seatTwoPiece, 1, new BoardCoordinate(3, 3))
            };

            movePolicy.policyId = "policy.tabletop.adjacentCapture";
            movePolicy.displayName = "Adjacent Move With Capture";
            movePolicy.shape = BoardMoveShape.Adjacent;
            movePolicy.maxDistance = 1;
            movePolicy.allowCapture = true;

            seatOneWins.conditionId = "condition.seatOneWins.eliminateSeatTwo";
            seatOneWins.displayName = "Seat One Wins When Seat Two Is Eliminated";
            seatOneWins.kind = BoardTerminalConditionKind.SideEliminated;
            seatOneWins.observedSeat = 1;
            seatOneWins.winningSeat = 0;

            seatTwoWins.conditionId = "condition.seatTwoWins.eliminateSeatOne";
            seatTwoWins.displayName = "Seat Two Wins When Seat One Is Eliminated";
            seatTwoWins.kind = BoardTerminalConditionKind.SideEliminated;
            seatTwoWins.observedSeat = 0;
            seatTwoWins.winningSeat = 1;

            actionPhase.phaseId = "phase.action";
            actionPhase.displayName = "Action";
            actionPhase.allowsActionSelection = true;
            actionPhase.endsTurnWhenComplete = true;

            turnOrder.turnOrderId = "turn.twoSeat.starter";
            turnOrder.displayName = "Two Seat Turn Order";
            turnOrder.participantSeats = new[] { 0, 1 };
            turnOrder.phases = new[] { actionPhase };

            moveAction.actionId = "action.board.move";
            moveAction.displayName = "Move Board Piece";
            moveAction.actionFamily = "Tabletop";
            moveAction.executionTiming = ActionExecutionTiming.Queued;
            moveAction.targetRule = ActionTargetRule.Single(ActionTargetKind.BoardSpace);
            moveAction.notes = "Starter tabletop action. Queue this with BoardMoveActionPayload and validate it through the included adjacent-capture move policy.";

            RuntimePatternDefinition patternBoardCardTabletop = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternBoardCardTabletop");
            patternBoardCardTabletop.patternId = "pattern.board-card-tabletop";
            patternBoardCardTabletop.displayName = "Board/Card/Tabletop";
            patternBoardCardTabletop.description = "Seat, board, piece, legal move, and tabletop-style setup without requiring character-controller pawns.";
            patternBoardCardTabletop.capabilityFamily = RuntimeCapabilityFamily.BoardCardTabletop;
            patternBoardCardTabletop.participantEmbodiment = ParticipantEmbodimentRequirement.NonPawnSurfaceRequired;
            patternBoardCardTabletop.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.BoardPiece,
                RuntimeControlSurface.Cursor,
                RuntimeControlSurface.MenuSelection
            };
            patternBoardCardTabletop.requiredRuntimeSystems = new[] { "ParticipantRosterService", "BoardDefinition", "BoardMovePolicyDefinition", "TurnOrderDefinition", "BoardMoveActionResolver" };
            patternBoardCardTabletop.optionalRuntimeSystems = new[] { "ParticipantScoreService", "camera/cursor selection", "BoardTerminalConditionDefinition" };
            patternBoardCardTabletop.presentationLanes = new[] { RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.UiMenu, RuntimePatternPresentationLane.CameraCursor };
            patternBoardCardTabletop.firstProofRequirements = RuntimePatternFirstProofRequirement.TabletopRuntimeContract | RuntimePatternFirstProofRequirement.SelectionSurface;
            patternBoardCardTabletop.setupNotes = "Use the included board, pieces, adjacent-capture move policy, turn order, terminal conditions, and board-move action as the first playable rules loop. Leave Default Pawn empty for seat participants.";

            RuntimePatternDefinition patternTurnMenuAction = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternTurnMenuAction");
            patternTurnMenuAction.patternId = "pattern.turn-menu-action";
            patternTurnMenuAction.displayName = "Turn/Menu Action";
            patternTurnMenuAction.description = "Command, menu, tactics, card, and turn/action selection setup that can run with or without pawn bodies.";
            patternTurnMenuAction.capabilityFamily = RuntimeCapabilityFamily.ActionTargeting;
            patternTurnMenuAction.participantEmbodiment = ParticipantEmbodimentRequirement.OptionalPawn;
            patternTurnMenuAction.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.MenuSelection,
                RuntimeControlSurface.Cursor,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand,
                RuntimeControlSurface.Pawn
            };
            patternTurnMenuAction.requiredRuntimeSystems = new[] { "ActionDefinition", "ActionQueueService", "TurnOrderDefinition" };
            patternTurnMenuAction.optionalRuntimeSystems = new[] { "menu or cursor selection UI" };
            patternTurnMenuAction.presentationLanes = new[] { RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.UiMenu, RuntimePatternPresentationLane.CameraCursor };
            patternTurnMenuAction.firstProofRequirements = RuntimePatternFirstProofRequirement.SelectionSurface;
            patternTurnMenuAction.setupNotes = "Start with the included Move Board Piece action. Decide whether the player selects through a menu, cursor, or direct board-space click, then route that selection into the action queue.";

            RuntimePatternDefinition patternCameraCursorControl = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternCameraCursorControl");
            patternCameraCursorControl.patternId = "pattern.camera-cursor-control";
            patternCameraCursorControl.displayName = "Camera/Cursor Control";
            patternCameraCursorControl.description = "Participant control through a camera, cursor, menu selector, faction, or commander-style surface.";
            patternCameraCursorControl.capabilityFamily = RuntimeCapabilityFamily.CameraInput;
            patternCameraCursorControl.participantEmbodiment = ParticipantEmbodimentRequirement.NonPawnSurfaceRequired;
            patternCameraCursorControl.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.Camera,
                RuntimeControlSurface.Cursor,
                RuntimeControlSurface.MenuSelection,
                RuntimeControlSurface.Faction
            };
            patternCameraCursorControl.requiredRuntimeSystems = new[] { "ParticipantInputRouter or project-owned UI/input bridge" };
            patternCameraCursorControl.optionalRuntimeSystems = new[] { "CinemachineCameraRigController", "raycast selection", "UI selection" };
            patternCameraCursorControl.presentationLanes = new[] { RuntimePatternPresentationLane.CameraCursor, RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.UiMenu };
            patternCameraCursorControl.firstProofRequirements = RuntimePatternFirstProofRequirement.CameraRig | RuntimePatternFirstProofRequirement.SelectionSurface;
            patternCameraCursorControl.setupNotes = "Create a camera, cursor, or UI selector as the player control surface. Assign input only to the participant or surface that actually receives player controls.";

            RuntimePatternDefinition patternScoringObjectives = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternScoringObjectives");
            patternScoringObjectives.patternId = "pattern.scoring-objectives";
            patternScoringObjectives.displayName = "Scoring/Objectives";
            patternScoringObjectives.description = "Score, resources, victory points, timers, round results, objectives, and win/loss setup.";
            patternScoringObjectives.capabilityFamily = RuntimeCapabilityFamily.ScoringObjectives;
            patternScoringObjectives.participantEmbodiment = ParticipantEmbodimentRequirement.NoneRequired;
            patternScoringObjectives.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand,
                RuntimeControlSurface.MenuSelection,
                RuntimeControlSurface.SystemAI
            };
            patternScoringObjectives.requiredRuntimeSystems = new[] { "ParticipantScoreService or project-owned scoring rules" };
            patternScoringObjectives.optionalRuntimeSystems = new[] { "score HUD", "round result UI", "objective trackers" };
            patternScoringObjectives.presentationLanes = new[] { RuntimePatternPresentationLane.TabletopNoPawn, RuntimePatternPresentationLane.UiMenu, RuntimePatternPresentationLane.Sprite2D, RuntimePatternPresentationLane.Billboard2_5D, RuntimePatternPresentationLane.Rigged3D };
            patternScoringObjectives.firstProofRequirements = RuntimePatternFirstProofRequirement.ScoreService;
            patternScoringObjectives.setupNotes = "Decide which actions create score/resource changes, wire ParticipantScoreService or custom scoring rules, then add UI only after score changes correctly in Play Mode.";

            patternBoardCardTabletop.recommendedCompanionPatterns = new[] { patternTurnMenuAction, patternCameraCursorControl, patternScoringObjectives };
            patternTurnMenuAction.recommendedCompanionPatterns = new[] { patternBoardCardTabletop, patternCameraCursorControl, patternScoringObjectives };
            patternCameraCursorControl.recommendedCompanionPatterns = new[] { patternBoardCardTabletop, patternTurnMenuAction };
            patternScoringObjectives.recommendedCompanionPatterns = new[] { patternBoardCardTabletop, patternTurnMenuAction };

            GameSetupProfile setupTabletop = CreateAsset<GameSetupProfile>(profilesFolder, "SetupBoardCardTabletop");
            setupTabletop.setupName = "Board/Card/Tabletop";
            setupTabletop.summary = "A no-pawn starter setup for a small board game driven by seats, board pieces, turn order, a board-move action, and terminal conditions.";
            setupTabletop.runtimePatterns = new[] { patternBoardCardTabletop, patternTurnMenuAction, patternCameraCursorControl, patternScoringObjectives };
            setupTabletop.setupNotes = "Start with the included 4x4 board and two seat participants. Wire board-space selection to the Move Board Piece action, then process it through the adjacent-capture move policy and turn order.";

            GameModeDefinition gameModeDefinition = CreateAsset<GameModeDefinition>(definitionsFolder, "TabletopModeDefinition");
            gameModeDefinition.setupProfile = setupTabletop;
            gameModeDefinition.cameraRigProfile = cameraRigProfile;
            gameModeDefinition.enableScore = true;
            gameModeDefinition.enableCombat = false;
            gameModeDefinition.enablePickups = false;
            gameModeDefinition.enableHazards = false;
            gameModeDefinition.enableRespawn = false;
            gameModeDefinition.boardDefinition = boardDefinition;
            gameModeDefinition.turnOrderDefinition = turnOrder;
            gameModeDefinition.boardTerminalConditions = new[] { seatOneWins, seatTwoWins };

            ParticipantDefinition playerOne = CreateAsset<ParticipantDefinition>(definitionsFolder, "SeatOneParticipant");
            playerOne.displayName = "Seat One";
            playerOne.preferredSeatIndex = 0;
            playerOne.inputProfile = inputProfile;

            ParticipantDefinition playerTwo = CreateAsset<ParticipantDefinition>(definitionsFolder, "SeatTwoParticipant");
            playerTwo.displayName = "Seat Two";
            playerTwo.preferredSeatIndex = 1;
            playerTwo.inputProfile = inputProfile;

            SessionDefinition sessionDefinition = CreateAsset<SessionDefinition>(definitionsFolder, "TabletopSessionDefinition");
            sessionDefinition.sessionName = "Local Tabletop Session";
            sessionDefinition.defaultGameMode = gameModeDefinition;
            sessionDefinition.settingsProfile = settingsProfile;
            sessionDefinition.defaultInputProfile = inputProfile;
            sessionDefinition.defaultParticipants = new[] { playerOne, playerTwo };
            sessionDefinition.maxParticipants = 2;

            EditorUtility.SetDirty(patternBoardCardTabletop);
            EditorUtility.SetDirty(patternTurnMenuAction);
            EditorUtility.SetDirty(patternCameraCursorControl);
            EditorUtility.SetDirty(patternScoringObjectives);
            EditorUtility.SetDirty(setupTabletop);
            EditorUtility.SetDirty(seatOnePiece);
            EditorUtility.SetDirty(seatTwoPiece);
            EditorUtility.SetDirty(boardDefinition);
            EditorUtility.SetDirty(movePolicy);
            EditorUtility.SetDirty(seatOneWins);
            EditorUtility.SetDirty(seatTwoWins);
            EditorUtility.SetDirty(actionPhase);
            EditorUtility.SetDirty(turnOrder);
            EditorUtility.SetDirty(moveAction);
            EditorUtility.SetDirty(gameModeDefinition);
            EditorUtility.SetDirty(playerOne);
            EditorUtility.SetDirty(playerTwo);
            EditorUtility.SetDirty(sessionDefinition);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = sessionDefinition;
        }

        private static string CreateStarterPackRootFolder(string folderName)
        {
            string baseFolder = NormalizeStarterPackFolder(GetSelectedFolder());
            EnsureFolderPath(baseFolder);

            string rootFolder = AssetDatabase.GenerateUniqueAssetPath($"{baseFolder}/{folderName}");
            EnsureFolderPath(rootFolder);
            return rootFolder;
        }

        private static string GetSelectedFolder()
        {
            Object activeObject = Selection.activeObject;
            string selectedPath = activeObject != null ? AssetDatabase.GetAssetPath(activeObject) : "Assets";
            if (string.IsNullOrWhiteSpace(selectedPath))
                return "Assets";

            return AssetDatabase.IsValidFolder(selectedPath)
                ? selectedPath
                : System.IO.Path.GetDirectoryName(selectedPath)?.Replace("\\", "/") ?? "Assets";
        }

        private static string NormalizeStarterPackFolder(string folder)
        {
            string normalized = folder.Replace("\\", "/").TrimEnd('/');
            if (normalized.IndexOf(PackageSamplesFolderSegment, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Debug.LogWarning(
                    $"Pyralis starter packs use Unity's AssetDatabase and cannot be generated inside package sample source folders like '{normalized}'. " +
                    $"Creating the starter pack under '{DefaultStarterPackFolder}' instead.");
                return DefaultStarterPackFolder;
            }

            return normalized;
        }

        private static void EnsureFolderPath(string folder)
        {
            string normalized = folder.Replace("\\", "/").TrimEnd('/');
            if (string.IsNullOrWhiteSpace(normalized) || AssetDatabase.IsValidFolder(normalized))
                return;

            string[] parts = normalized.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string CreateChildFolder(string parent, string name)
        {
            string childPath = $"{parent}/{name}";
            EnsureFolderPath(childPath);
            return childPath;
        }

        private static PawnPresentationProfile CreatePresentationProfile(string folder, string assetName, ActorPresentationMode mode)
        {
            PawnPresentationProfile profile = CreateAsset<PawnPresentationProfile>(folder, assetName);
            profile.presentationMode = mode;
            profile.useSharedCamera = mode != ActorPresentationMode.Rigged3D;
            profile.rigType = mode == ActorPresentationMode.Rigged3D
                ? RiggedAnimationRigType.Humanoid
                : RiggedAnimationRigType.Generic;
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static PawnDefinition CreatePawnDefinition(
            string folder,
            string assetName,
            InputProfile inputProfile,
            PawnMovementProfile movementProfile,
            PawnCombatProfile combatProfile,
            PawnTraversalProfile traversalProfile,
            PawnPresentationProfile presentationProfile,
            PawnAnimationProfile animationProfile,
            GameObject pawnPrefab)
        {
            PawnDefinition pawnDefinition = CreateAsset<PawnDefinition>(folder, assetName);
            pawnDefinition.pawnPrefab = pawnPrefab;
            pawnDefinition.defaultInputProfile = inputProfile;
            pawnDefinition.movementProfile = movementProfile;
            pawnDefinition.combatProfile = combatProfile;
            pawnDefinition.traversalProfile = traversalProfile;
            pawnDefinition.presentationProfile = presentationProfile;
            pawnDefinition.animationProfile = animationProfile;
            EditorUtility.SetDirty(pawnDefinition);
            return pawnDefinition;
        }

        private static GameObject CreateStarterPawnPrefab2D(string folder, string prefabName, Sprite starterSprite, InputActionAsset inputActions)
        {
            GameObject root = new GameObject(prefabName);
            try
            {
                root.AddComponent<PawnRoot>();
                Rigidbody2D rigidbody2D = EnsureComponent<Rigidbody2D>(root);
                rigidbody2D.gravityScale = 0f;
                rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
                EnsureComponent<Pawn2DMovementComponent>(root);
                ActorAnimationDriver animationDriver = EnsureComponent<ActorAnimationDriver>(root);
                Pawn2DPresentationComponent presentation = EnsureComponent<Pawn2DPresentationComponent>(root);
                EnsureComponent<Motor2D>(root);
                Motor2DInputAdapter inputAdapter = EnsureComponent<Motor2DInputAdapter>(root);
                root.AddComponent<HealthComponent>();

                GameObject visual = new GameObject("SpriteVisual");
                visual.transform.SetParent(root.transform, false);
                Animator animator = visual.AddComponent<Animator>();
                SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = starterSprite;
                spriteRenderer.sortingOrder = 10;

                SetObjectReference(presentation, "spriteRenderer", spriteRenderer);
                SetObjectReference(presentation, "animator", animator);
                SetObjectReference(animationDriver, "animator", animator);
                SetObjectReference(animationDriver, "spriteRenderer", spriteRenderer);
                SetObjectReference(animationDriver, "visualRoot", visual.transform);
                SetObjectReference(inputAdapter, "_inputActions", inputActions);

                string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{prefabName}.prefab");
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateStarterPawnPrefab3D(string folder, string prefabName, InputActionAsset inputActions)
        {
            GameObject root = new GameObject(prefabName);
            try
            {
                root.AddComponent<PawnRoot>();
                root.AddComponent<Motor3D>();
                EnsureComponent<Pawn3DMovementComponent>(root);
                EnsureComponent<Pawn3DPresentationComponent>(root);
                Pawn3DInputModule inputModule = EnsureComponent<Pawn3DInputModule>(root);
                EnsureComponent<Pawn3DTraversalComponent>(root);
                root.AddComponent<ActorAnimationDriver>();
                root.AddComponent<PawnCombatBehaviour>();
                SetObjectReference(inputModule, "inputActions", inputActions);

                GameObject visual = new GameObject("RigOrBillboardVisual");
                visual.transform.SetParent(root.transform, false);
                visual.AddComponent<Animator>();

                string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{prefabName}.prefab");
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Sprite CreateStarterPawnSprite(string folder)
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "StarterPawnSpriteTexture"
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color body = new Color(0.1f, 0.58f, 0.95f, 1f);
            Color highlight = new Color(0.92f, 0.98f, 1f, 1f);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.36f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    float distance = Vector2.Distance(point, center);
                    if (distance > radius)
                    {
                        texture.SetPixel(x, y, clear);
                        continue;
                    }

                    bool edge = distance > radius - 2f;
                    bool eye = x > 18 && x < 23 && y > 18 && y < 23;
                    texture.SetPixel(x, y, edge || eye ? highlight : body);
                }
            }

            texture.Apply();

            string texturePath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/StarterPawnSprite.png");
            System.IO.File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        }

        private static InputActionAsset LoadDefaultInputActions()
        {
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (actions == null)
            {
                Debug.LogWarning(
                    "Pyralis starter pack could not find Assets/InputSystem_Actions.inputactions. " +
                    "The generated input profile remains editable, but movement needs a Player action map before Play Mode.");
            }

            return actions;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static T CreateAsset<T>(string folder, string assetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, $"{folder}/{assetName}.asset");
            return asset;
        }

        private static T EnsureComponent<T>(GameObject root) where T : Component
        {
            T component = root.GetComponent<T>();
            return component != null ? component : root.AddComponent<T>();
        }
    }
}
