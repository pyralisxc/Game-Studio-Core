using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public static class GameplayExampleAssetFactory
    {
        [MenuItem("Assets/Create/NeonBlack/Gameplay/Example Authoring Pack")]
        public static void CreateExampleAuthoringPack()
        {
            string baseFolder = GetSelectedFolder();
            string rootFolder = AssetDatabase.GenerateUniqueAssetPath($"{baseFolder}/GameplayExamplePack");
            AssetDatabase.CreateFolder(baseFolder, System.IO.Path.GetFileName(rootFolder));

            string profilesFolder = CreateChildFolder(rootFolder, "Profiles");
            string definitionsFolder = CreateChildFolder(rootFolder, "Definitions");

            InputProfile inputProfile = CreateAsset<InputProfile>(profilesFolder, "SharedInputProfile");
            PawnMovementProfile movementProfile = CreateAsset<PawnMovementProfile>(profilesFolder, "BrawlerMovementProfile");
            PawnCombatProfile combatProfile = CreateAsset<PawnCombatProfile>(profilesFolder, "CombatProfile");
            PawnTraversalProfile traversalProfile = CreateAsset<PawnTraversalProfile>(profilesFolder, "TraversalProfile");
            PawnPresentationProfile presentationProfile = CreateAsset<PawnPresentationProfile>(profilesFolder, "PresentationProfile");
            PawnAnimationProfile animationProfile = CreateAsset<PawnAnimationProfile>(profilesFolder, "AnimationProfile");
            PlayfieldProfile playfieldProfile = CreateAsset<PlayfieldProfile>(profilesFolder, "ArenaPlayfieldProfile");
            CameraRigProfile cameraRigProfile = CreateAsset<CameraRigProfile>(profilesFolder, "SharedCameraRigProfile");
            SettingsProfile settingsProfile = CreateAsset<SettingsProfile>(profilesFolder, "DefaultSettingsProfile");
            ActorAnimationDefinition animationDefinition = CreateAsset<ActorAnimationDefinition>(definitionsFolder, "DefaultActorAnimationDefinition");
            FireModeDefinition sampleFireMode = CreateAsset<FireModeDefinition>(definitionsFolder, "SampleHitscanFireMode");
            ProjectileImpactDefinition sampleImpact = CreateAsset<ProjectileImpactDefinition>(definitionsFolder, "SampleProjectileImpact");
            ProjectileDefinition sampleProjectile = CreateAsset<ProjectileDefinition>(definitionsFolder, "SampleHitscanProjectile");
            RuntimePatternDefinition patternRealtimeCharacter = CreateAsset<RuntimePatternDefinition>(definitionsFolder, "PatternRealtimeCharacter");
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

            patternRealtimeCharacter.recommendedCompanionPatterns = new[] { patternProjectileCombat };
            patternProjectileCombat.recommendedCompanionPatterns = new[] { patternRealtimeCharacter, patternTurnMenuAction, patternBoardCardTabletop, patternCameraCursorControl };
            patternTurnMenuAction.recommendedCompanionPatterns = new[] { patternProjectileCombat, patternBoardCardTabletop, patternCameraCursorControl };
            patternBoardCardTabletop.recommendedCompanionPatterns = new[] { patternTurnMenuAction, patternCameraCursorControl };
            patternCameraCursorControl.recommendedCompanionPatterns = new[] { patternBoardCardTabletop, patternProjectileCombat };

            setupBrawlerWithProjectiles.setupName = "Brawler With Projectiles";
            setupBrawlerWithProjectiles.summary = "A pawn-backed realtime character game that also uses the reusable projectile combat layer.";
            setupBrawlerWithProjectiles.runtimePatterns = new[] { patternRealtimeCharacter, patternProjectileCombat };
            setupBrawlerWithProjectiles.setupNotes = "Use this profile as a starting point for brawlers, side-scrollers, or arcade action games that need guns, spells, traps, or other projectile delivery.";

            GameModeDefinition gameModeDefinition = CreateAsset<GameModeDefinition>(definitionsFolder, "SharedModeDefinition");
            gameModeDefinition.setupProfile = setupBrawlerWithProjectiles;
            gameModeDefinition.playfieldProfile = playfieldProfile;
            gameModeDefinition.cameraRigProfile = cameraRigProfile;
            gameModeDefinition.enableCombat = true;
            gameModeDefinition.enablePickups = true;
            gameModeDefinition.enableHazards = true;

            PawnDefinition pawnDefinition = CreateAsset<PawnDefinition>(definitionsFolder, "SharedPawnDefinition");
            pawnDefinition.defaultInputProfile = inputProfile;
            pawnDefinition.movementProfile = movementProfile;
            pawnDefinition.combatProfile = combatProfile;
            pawnDefinition.traversalProfile = traversalProfile;
            pawnDefinition.presentationProfile = presentationProfile;
            pawnDefinition.animationProfile = animationProfile;

            ParticipantDefinition playerOne = CreateAsset<ParticipantDefinition>(definitionsFolder, "PlayerOneDefinition");
            playerOne.displayName = "Player One";
            playerOne.preferredSeatIndex = 0;
            playerOne.defaultPawn = pawnDefinition;
            playerOne.inputProfile = inputProfile;

            ParticipantDefinition playerTwo = CreateAsset<ParticipantDefinition>(definitionsFolder, "PlayerTwoDefinition");
            playerTwo.displayName = "Player Two";
            playerTwo.preferredSeatIndex = 1;
            playerTwo.defaultPawn = pawnDefinition;
            playerTwo.inputProfile = inputProfile;

            SessionDefinition sessionDefinition = CreateAsset<SessionDefinition>(definitionsFolder, "SharedSessionDefinition");
            sessionDefinition.sessionName = "Local Shared Core Session";
            sessionDefinition.defaultGameMode = gameModeDefinition;
            sessionDefinition.settingsProfile = settingsProfile;
            sessionDefinition.defaultParticipants = new[] { playerOne, playerTwo };
            sessionDefinition.maxParticipants = 4;

            EditorUtility.SetDirty(gameModeDefinition);
            EditorUtility.SetDirty(pawnDefinition);
            EditorUtility.SetDirty(playerOne);
            EditorUtility.SetDirty(playerTwo);
            EditorUtility.SetDirty(sessionDefinition);
            EditorUtility.SetDirty(sampleFireMode);
            EditorUtility.SetDirty(sampleImpact);
            EditorUtility.SetDirty(sampleProjectile);
            EditorUtility.SetDirty(patternRealtimeCharacter);
            EditorUtility.SetDirty(patternProjectileCombat);
            EditorUtility.SetDirty(patternTurnMenuAction);
            EditorUtility.SetDirty(patternBoardCardTabletop);
            EditorUtility.SetDirty(patternCameraCursorControl);
            EditorUtility.SetDirty(setupBrawlerWithProjectiles);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = sessionDefinition;
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

        private static string CreateChildFolder(string parent, string name)
        {
            string childPath = $"{parent}/{name}";
            AssetDatabase.CreateFolder(parent, name);
            return childPath;
        }

        private static T CreateAsset<T>(string folder, string assetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, $"{folder}/{assetName}.asset");
            return asset;
        }
    }
}
