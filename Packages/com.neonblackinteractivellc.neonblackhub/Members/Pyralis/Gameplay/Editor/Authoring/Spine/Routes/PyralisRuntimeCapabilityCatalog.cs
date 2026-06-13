using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using System.Reflection;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Spawning;
using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Features.Zones;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum RuntimeCapabilityLaneTag
    {
        /// <summary> Flat 2D rendering using SpriteRenderers (Side-scroller, Top-down). </summary>
        Sprite2D,
        
        /// <summary> 2D sprites moving through 3D depth lanes or environments. </summary>
        Billboard2_5D,
        
        /// <summary> 3D rigged models with trailing or isometric target cameras. </summary>
        ThirdPerson3D,
        
        /// <summary> 3D rigged models with cameras pinned inside character eyes. </summary>
        FirstPerson3D,
        
        /// <summary> Overhead board layout, grid spaces, or card playing fields. </summary>
        TabletopBoard,
        
        /// <summary> Screen-space overlay/Canvas with no active world camera. </summary>
        UiMenuOnly,

        /// <summary> Camera control surface focusing on cursor selection or raycasting. </summary>
        CameraCursor,

        /// <summary> Dynamically switches between multiple camera profiles or visual styles. </summary>
        Mixed
    }

    public static class RuntimeCapabilityLaneRegistry
    {
        private static readonly Dictionary<RuntimeCapabilityLaneTag, string> _tooltips = new Dictionary<RuntimeCapabilityLaneTag, string>
        {
            { RuntimeCapabilityLaneTag.Sprite2D, "Side-scroller, flat top-down sprite rendering." },
            { RuntimeCapabilityLaneTag.Billboard2_5D, "Classic retro 2.5D (2D sprites in 3D depth lanes)." },
            { RuntimeCapabilityLaneTag.ThirdPerson3D, "3D rigged models, trailing or isometric target camera." },
            { RuntimeCapabilityLaneTag.FirstPerson3D, "3D rigged models, camera pinned inside character eyes." },
            { RuntimeCapabilityLaneTag.TabletopBoard, "Overhead board layout, grid spaces, or card playing fields." },
            { RuntimeCapabilityLaneTag.UiMenuOnly, "Screen space overlay Canvas, menu/text-driven (no world camera)." },
            { RuntimeCapabilityLaneTag.CameraCursor, "Camera control surface focusing on cursor selection or raycasting." },
            { RuntimeCapabilityLaneTag.Mixed, "Set up multiple camera profiles in game that can be switched between." }
        };

        public static string GetTooltip(RuntimeCapabilityLaneTag lane) => _tooltips.TryGetValue(lane, out var t) ? t : "A presentation lane perspective.";
    }

    public sealed class RuntimeCapabilityCard
    {
        public RuntimeCapabilityCard(
            string stableId,
            string displayName,
            RuntimeCapabilityFamily capabilityFamily,
            PyralisAuthoringRouteCapability routeCapability,
            string routeLabel,
            bool primaryProofCandidate,
            string proofStepLabel,
            string proofStepSuccessCriteria,
            string[] goalTags,
            RuntimeCapabilityLaneTag[] laneTags,
            RuntimeCapabilityLaneTag[] cautionLaneTags,
            string whatItAdds,
            string whenToUse,
            string[] requiredDefinitions,
            string[] requiredProfiles,
            string[] requiredSceneComponents,
            string[] requiredUnitySurfaces,
            string[] assignmentFields,
            string[] customizationMoments,
            string[] canWait,
            string firstProof,
            string[] commonNextCapabilities,
            string expertAdvice,
            string documentationURL,
            string[] relatedStableIds = null)
        {
            StableId = stableId;
            DisplayName = displayName;
            CapabilityFamily = capabilityFamily;
            RouteCapability = routeCapability;
            RouteLabel = routeLabel ?? string.Empty;
            PrimaryProofCandidate = primaryProofCandidate;
            ProofStepLabel = proofStepLabel ?? string.Empty;
            ProofStepSuccessCriteria = proofStepSuccessCriteria ?? string.Empty;
            GoalTags = goalTags ?? System.Array.Empty<string>();
            LaneTags = laneTags ?? System.Array.Empty<RuntimeCapabilityLaneTag>();
            CautionLaneTags = cautionLaneTags ?? System.Array.Empty<RuntimeCapabilityLaneTag>();
            WhatItAdds = whatItAdds;
            WhenToUse = whenToUse;
            RequiredDefinitions = requiredDefinitions ?? System.Array.Empty<string>();
            RequiredProfiles = requiredProfiles ?? System.Array.Empty<string>();
            RequiredSceneComponents = requiredSceneComponents ?? System.Array.Empty<string>();
            RequiredUnitySurfaces = requiredUnitySurfaces ?? System.Array.Empty<string>();
            AssignmentFields = assignmentFields ?? System.Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? System.Array.Empty<string>();
            CanWait = canWait ?? System.Array.Empty<string>();
            FirstProof = firstProof;
            CommonNextCapabilities = commonNextCapabilities ?? System.Array.Empty<string>();
            ExpertAdvice = expertAdvice;
            DocumentationURL = documentationURL;
            NativeActions = BuildNativeActions(assignmentFields, customizationMoments, firstProof);
            Fact = new PyralisAuthoringFact(
                stableId,
                displayName,
                PyralisAuthoringFactKind.RuntimeCapability,
                PyralisAuthoringFactSourceKind.HandAuthoredGuideCard,
                PyralisAuthoringConfidence.Explicit,
                whatItAdds,
                whenToUse,
                firstProof,
                GoalTags,
                ToStrings(LaneTags),
                ToStrings(CautionLaneTags),
                RequiredDefinitions,
                RequiredProfiles,
                RequiredSceneComponents,
                RequiredUnitySurfaces,
                AssignmentFields,
                CustomizationMoments,
                CanWait,
                NativeActions,
                relatedStableIds: relatedStableIds,
                expertAdvice: expertAdvice,
                documentationURL: documentationURL);
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public RuntimeCapabilityFamily CapabilityFamily { get; }
        public PyralisAuthoringRouteCapability RouteCapability { get; }
        public string RouteLabel { get; }
        public bool PrimaryProofCandidate { get; }
        public string ProofStepLabel { get; }
        public string ProofStepSuccessCriteria { get; }
        public string[] GoalTags { get; }
        public RuntimeCapabilityLaneTag[] LaneTags { get; }
        public RuntimeCapabilityLaneTag[] CautionLaneTags { get; }
        public string WhatItAdds { get; }
        public string WhenToUse { get; }
        public string[] RequiredDefinitions { get; }
        public string[] RequiredProfiles { get; }
        public string[] RequiredSceneComponents { get; }
        public string[] RequiredUnitySurfaces { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }
        public string[] CanWait { get; }
        public string FirstProof { get; }
        public string[] CommonNextCapabilities { get; }
        public string ExpertAdvice { get; }
        public string DocumentationURL { get; }
        public PyralisAuthoringNativeAction[] NativeActions { get; }
        public PyralisAuthoringFact Fact { get; }

        public bool HasGoal(string tag)
        {
            return Contains(GoalTags, tag);
        }

        public bool HasLane(RuntimeCapabilityLaneTag tag)
        {
            return Contains(LaneTags, tag);
        }

        public bool HasCautionLane(RuntimeCapabilityLaneTag tag)
        {
            return Contains(CautionLaneTags, tag);
        }

        private static bool Contains(string[] tags, string tag)
        {
            if (tags == null || string.IsNullOrEmpty(tag)) return false;
            for (int i = 0; i < tags.Length; i++)
            {
                string t = tags[i];
                if (string.Equals(t, tag, System.StringComparison.OrdinalIgnoreCase))
                    return true;

                // Hierarchical match: 
                // - A tag like 'Combat/Reaction' matches a search for 'Combat'
                // - A tag like 'Combat' matches a search for 'Combat/Reaction' (as a parent category)
                if (t != null && (t.StartsWith(tag + "/", System.StringComparison.OrdinalIgnoreCase) ||
                                 tag.StartsWith(t + "/", System.StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            return false;
        }

        private static bool Contains(RuntimeCapabilityLaneTag[] tags, RuntimeCapabilityLaneTag tag)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                    return true;
            }

            return false;
        }

        private static string[] ToStrings(RuntimeCapabilityLaneTag[] tags)
        {
            string[] values = new string[tags.Length];
            for (int i = 0; i < tags.Length; i++)
                values[i] = tags[i].ToString();

            return values;
        }

        private static PyralisAuthoringNativeAction[] BuildNativeActions(string[] assignmentFields, string[] customizationMoments, string firstProof)
        {
            List<PyralisAuthoringNativeAction> actions = new List<PyralisAuthoringNativeAction>();
            if (assignmentFields != null && assignmentFields.Length > 0)
            {
                actions.Add(new PyralisAuthoringNativeAction(
                    "Assign",
                    PyralisAuthoringActionSurface.Inspector,
                    "the relevant setup asset, scene object, or prefab",
                    string.Join("; ", assignmentFields),
                    "the capability card's required assignments are linked"));
            }

            if (customizationMoments != null && customizationMoments.Length > 0)
            {
                actions.Add(new PyralisAuthoringNativeAction(
                    "Customize",
                    PyralisAuthoringActionSurface.Inspector,
                    "the project-owned definition, profile, scene object, or prefab",
                    string.Join("; ", customizationMoments),
                    "the route reflects the user's design choice"));
            }

            if (!string.IsNullOrWhiteSpace(firstProof))
            {
                actions.Add(new PyralisAuthoringNativeAction(
                    "Test",
                    PyralisAuthoringActionSurface.PlayMode,
                    "the active route",
                    firstProof,
                    "the first proof is observed by the author"));
            }

            return actions.ToArray();
        }
    }

    public static class PyralisRuntimeCapabilityCatalog
    {
        private static readonly RuntimeCapabilityCard[] Cards =
        {
            new RuntimeCapabilityCard(
                "capability.2d-pawn-movement",
                "2D Pawn Movement",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                PyralisAuthoringRouteCapability.PawnAction,
                "Pawn Action",
                true,
                "Local pawn movement",
                "One participant spawns one pawn and movement input visibly moves it.",
                new[]
                {
                    "Movement",
                    "JumpTraversal",
                    "Input",
                    "AnimationPresentation"
                },
                new[] { RuntimeCapabilityLaneTag.Sprite2D },
                new[] { RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                "A participant-owned Sprite2D pawn that can spawn into the scene and move through input.",
                "Use this when the first proof is a visible 2D player, NPC, or AI actor body moving in the world.",
                new[] { "ParticipantDefinition", "PawnDefinition" },
                new[] { "InputProfile", "PawnMovementProfile", "PawnPresentationProfile" },
                new[] { "GameplaySessionBootstrap", "PyralisGameplayLifetimeScope", "one Spawn Point Transform" },
                new[] { "PawnRoot", "Motor2D", "Motor2DInputAdapter", "Pawn2DMovementComponent", "Pawn2DPresentationComponent" },
                new[]
                {
                    "SessionDefinition.defaultParticipants -> ParticipantDefinition",
                    "ParticipantDefinition.defaultPawn -> PawnDefinition",
                    "PawnDefinition.pawnPrefab -> pawn prefab",
                    "GameplaySessionBootstrap.spawnPoints -> scene spawn Transform"
                },
                new[]
                {
                    "Choose the player or NPC art/prefab.",
                    "Choose Sprite2D presentation and collider fit.",
                    "Tune speed, acceleration, braking, and jump feel.",
                    "Map project Input Action names through InputProfile.",
                    "Place the spawn point where the first proof should begin."
                },
                new[] { "combat", "HUD", "scoring", "pickups", "hazards", "networking", "local join" },
                "Enter Play Mode and confirm one pawn spawns at the assigned spawn point, receives input, and visibly moves.",
                new[] { "Combat Attack Proof", "Camera Follow And Bounds", "UI And Scoring Feedback" },
                "The 2D Pawn is the most fundamental proof. Ensure your SpriteRenderer pivot is at the 'Feet' for consistent ground snapping.",
                "https://docs.neonblack.com/pyralis/movement",
                new[]
                {
                    "proof.1p-pawn-movement",
                    "setup.assign-participant-pawn",
                    "setup.assign-input-profile",
                    "setup.assign-spawn-points",
                    "setup.tune-pawn-visuals-and-collision",
                    "setup.tune-movement-and-input-feel"
                }),

            new RuntimeCapabilityCard(
                "capability.3d-pawn-movement",
                "3D / 2.5D Pawn Movement",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                PyralisAuthoringRouteCapability.PawnAction,
                "Pawn Action",
                true,
                "Local pawn movement",
                "One participant spawns one pawn and movement input visibly moves it.",
                new[]
                {
                    "Movement",
                    "JumpTraversal",
                    "Input",
                    "AnimationPresentation",
                    "Combat",
                    "NpcsEnemies"
                },
                new[] { RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                new[] { RuntimeCapabilityLaneTag.TabletopBoard },
                "A participant-owned 3D CharacterController pawn that can move on X/Z, present as a billboard sprite or rigged model, and receive gameplay input.",
                "Use this when the first proof is a 3D brawler, arena actor, action RPG body, local co-op pawn, or billboard character moving through lane depth.",
                new[] { "ParticipantDefinition", "PawnDefinition" },
                new[] { "InputProfile", "PawnMovementProfile", "PawnTraversalProfile", "PawnPresentationProfile", "PawnAnimationProfile" },
                new[] { "GameplaySessionBootstrap", "PyralisGameplayLifetimeScope", "one Spawn Point Transform per starting participant", "PlayerInputManager when local join is part of the proof" },
                new[] { "PawnRoot", "CharacterController", "Motor3D", "Pawn3DInputModule", "Pawn3DMovementComponent", "Pawn3DTraversalComponent", "Pawn3DPresentationComponent", "ActorAnimationDriver", "HealthComponent", "KnockbackReceiver" },
                new[]
                {
                    "SessionDefinition.defaultParticipants -> ParticipantDefinition rows for each player seat",
                    "ParticipantDefinition.defaultPawn -> PawnDefinition",
                    "PawnDefinition.pawnPrefab -> 3D pawn prefab",
                    "PawnDefinition.presentationProfile -> Billboard2_5D or ThirdPerson3D presentation",
                    "PawnDefinition.animationProfile -> PawnAnimationProfile",
                    "GameplaySessionBootstrap.spawnPoints -> one scene spawn Transform per starting seat",
                    "GameplaySessionBootstrap.playerInputManager -> PlayerInputManager only for local join"
                },
                new[]
                {
                    "Choose billboard sprite art or a rigged model before tuning animation bindings.",
                    "Tune CharacterController height, radius, center, ground layer, speed, lane-depth multiplier, jump, dodge, crouch, and camera-relative movement.",
                    "Map InputProfile roles for Move, Attack, Kick, Block, Jump, Roll, Crouch, Sprint, and Interact.",
                    "For local co-op, decide which participants auto-join and which seats wait for PlayerInputManager join."
                },
                new[] { "full combo trees", "split screen", "networking", "HUD polish", "export/build menus" },
                "Enter Play Mode and confirm each authored or joined pawn spawns at a unique spawn point, receives its input owner, faces correctly, and moves through the 3D lane.",
                new[] { "Combat Attack Proof", "NPC / Enemy Actor Setup", "Camera Follow And Bounds", "UI And Scoring Feedback" },
                "CharacterController movement is highly sensitive to step offset and slope limit. Ensure these match your environment's geometry.",
                "https://docs.neonblack.com/pyralis/movement",
                new[]
                {
                    "intent.2_5d-lane-arena",
                    "intent.3d-space-action",
                    "intent.pawn-brawler",
                    "setup.assign-player-input-manager",
                    "setup.assign-participant-pawn",
                    "setup.assign-spawn-points",
                    "setup.tune-pawn-visuals-and-collision",
                    "setup.tune-movement-and-input-feel"
                }),

            new RuntimeCapabilityCard(
                "capability.camera-follow-bounds",
                "Camera Follow And Bounds",
                RuntimeCapabilityFamily.CameraInput,
                PyralisAuthoringRouteCapability.CameraCursor,
                "Camera / Cursor",
                true,
                "Camera/cursor response",
                "One input or target changes camera, cursor, selection, framing, or bounds.",
                new[] { "Camera" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.CameraCursor },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                "A camera or cursor control surface that makes the current route visible and keeps 2D proofs framed.",
                "Use this when a pawn, board, cursor, projectile, or UI route needs visible framing, bounds, or selection.",
                System.Array.Empty<string>(),
                new[] { "CameraRigProfile", "PlayfieldProfile when authored bounds matter" },
                new[] { "Camera Root with CinemachineCameraRigController", "physical Target Camera with Cinemachine Brain" },
                System.Array.Empty<string>(),
                new[]
                {
                    "GameplaySessionBootstrap.cameraRigController -> Camera Root",
                    "CinemachineCameraRigController.cameraRigProfile -> CameraRigProfile",
                    "CinemachineCameraRigController.targetCamera -> physical Camera"
                },
                new[]
                {
                    "Choose orthographic framing for 2D routes.",
                    "Tune camera size, follow target, margins, and bounds.",
                    "Decide whether camera input is player-controlled, pawn-follow, board-view, or cursor-driven."
                },
                new[] { "camera shake polish", "split screen", "cinematic transitions", "multi-target framing" },
                "Enter Play Mode and confirm the camera shows the proof surface and respects assigned follow or bounds behavior.",
                new[] { "2D Pawn Movement", "Combat Attack Proof", "Interaction Or Action Selection", "UI And Scoring Feedback" },
                "The Cinemachine Rig is the most flexible camera solution. Use 'Camera Bounds' to prevent the player from seeing 'off-map' areas.",
                "https://docs.neonblack.com/pyralis/camera"),

            new RuntimeCapabilityCard(
                "capability.interaction-action-selection",
                "Interaction Or Action Selection",
                RuntimeCapabilityFamily.ActionTargeting,
                PyralisAuthoringRouteCapability.ActionSelection,
                "Action Selection",
                true,
                "Action resolver",
                "One selected command reaches its resolver and reports accepted, rejected, completed, or failed.",
                new[] { "Interaction", "Input", "Tabletop" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.CameraCursor },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                "One selectable command surface such as an interact prompt, menu command, board space, card, cursor target, or pawn action.",
                "Use this when the player chooses what to do before the runtime resolves a command.",
                new[] { "ActionDefinition" },
                new[] { "InteractionFeatureProfile when using actor interaction" },
                new[] { "UI button, board presenter, cursor bridge, raycast target, or interaction trigger" },
                new[] { "Interaction runtime component or lane-specific action trigger when the source is a pawn" },
                new[]
                {
                    "Action presenter or resolver -> ActionDefinition",
                    "ParticipantDefinition.inputProfile -> InputProfile when input drives the action",
                    "Selected surface -> action resolver or bridge"
                },
                new[]
                {
                    "Choose whether selection is UI, cursor, collider/raycast, board space, card hand, or pawn input.",
                    "Tune prompts, target filters, action costs, cooldowns, and accepted/rejected feedback."
                },
                new[] { "large menus", "AI turns", "full card UX", "animation polish", "campaign flow" },
                "Enter Play Mode and confirm one selected command reaches a resolver and reports accepted, rejected, completed, or failed.",
                new[] { "UI And Scoring Feedback", "Combat Attack Proof", "Camera Follow And Bounds" },
                "Interaction is the bridge between input and logic. Always provide a clear visual prompt when the player is in range of an interactable object.",
                "https://docs.neonblack.com/pyralis/interaction"),

            new RuntimeCapabilityCard(
                "capability.combat-projectile-proof",
                "Combat Attack Proof",
                RuntimeCapabilityFamily.Combat,
                PyralisAuthoringRouteCapability.Combat,
                "Combat",
                true,
                "Combat reaction",
                "One attack produces one hit, block, damage, or reaction outcome.",
                new[]
                {
                    "Combat",
                    "Input",
                    "AnimationPresentation",
                    "Projectiles",
                    "NpcsEnemies"
                },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.CameraCursor },
                new[] { RuntimeCapabilityLaneTag.TabletopBoard },
                "A smallest attack, hit, shot, damage, or reaction path for a pawn, NPC/enemy, trap, turret, or command source.",
                "Use this after the source surface exists and the first proof needs cause-and-effect combat or projectile feedback.",
                new[] { "CombatActionDefinition", "CombatSequenceDefinition", "EnemyAttack", "ProjectileDefinition", "ProjectileImpactDefinition", "FireModeDefinition when firing cadence matters" },
                new[] { "PawnCombatProfile", "EnemyCombatProfile", "ActorFeedbackProfile", "ActorCombatReactionProfile" },
                new[] { "HealthComponent or target health health surface", "projectile launcher, HitBox, EnemySpawner, or ArenaZone surface" },
                new[] { "PawnCombatBehaviour, PawnCombatBehaviour2D, EnemyAI, ProjectileLauncher2D, ProjectileLauncher3D, or lane-specific combat runtime" },
                new[]
                {
                    "PawnDefinition.combatProfile -> PawnCombatProfile",
                    "PawnCombatProfile primary/secondary sequence -> CombatSequenceDefinition",
                    "EnemyAI.attackSequence or EnemyCombatProfile.attackSequence -> EnemyAttack assets",
                    "PawnCombatBehaviour.hitBoxZones and EnemyAI.hitBoxZones -> named HitBox children",
                    "Projectile launcher -> ProjectileDefinition and FireModeDefinition",
                    "feedback/reaction runtime -> feedback or reaction profile"
                },
                new[]
                {
                    "Choose attacker and target art.",
                    "Tune damage, knockback, hit timing, projectile cadence, and reaction readability.",
                    "Decide whether the source is player pawn, NPC/enemy, trap, turret, card, board piece, or camera command."
                },
                new[] { "combo trees", "enemy waves", "ammo economy", "score rewards", "network replication", "VFX polish" },
                "Enter Play Mode and confirm one attack or shot produces one visible hit, miss, impact, damage, block, or reaction outcome.",
                new[] { "UI And Scoring Feedback", "Interaction Or Action Selection", "Camera Follow And Bounds" },
                "Melee combat relies on 'HitBox' zones. Ensure your attack sequences trigger these zones during the active frames of the animation.",
                "https://docs.neonblack.com/pyralis/combat"),

            new RuntimeCapabilityCard(
                "capability.npc-enemy-setup",
                "NPC / Enemy Actor Setup",
                RuntimeCapabilityFamily.Combat,
                PyralisAuthoringRouteCapability.Combat,
                "Combat",
                true,
                "Combat reaction",
                "One attack produces one hit, block, damage, or reaction outcome.",
                new[]
                {
                    "NpcsEnemies",
                    "Combat",
                    "AnimationPresentation",
                    "Movement"
                },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                new[] { RuntimeCapabilityLaneTag.UiMenuOnly },
                "An authored enemy or NPC actor that can appear in the scene, detect or patrol, take damage, attack, and participate in an encounter.",
                "Use this after the player pawn and one attack path exist, when the proof needs ragers, opponents, NPCs, hazards, or encounter pressure.",
                new[] { "EnemyAttack or project-owned NPC definition" },
                new[] { "EnemyFeatureProfile", "EnemyCombatProfile", "EnemyReactionProfile", "PawnPresentationProfile or actor presentation profile" },
                new[] { "EnemySpawner", "ArenaZone when encounter gating matters", "spawn point Transforms", "optional exit blockers" },
                new[] { "EnemyAI", "CharacterController", "HealthComponent", "KnockbackReceiver", "ActorAnimationDriver", "Animator", "HitBox child" },
                new[]
                {
                    "EnemySpawner.enemyPrefabs -> enemy prefab",
                    "EnemySpawner.spawnPoints -> spawn anchors",
                    "ArenaZone.enemySpawners -> spawners activated by the zone",
                    "EnemyAI.attackSequence or combatProfile -> EnemyAttack assets",
                    "EnemyAI.hitBoxZones -> named HitBox child components",
                    "EnemyAI.targetOverride -> optional explicit player target for tiny proofs"
                },
                new[]
                {
                    "Choose enemy sprite/model, faction, collider, patrol/chase ranges, leash, attack range, and spawn cadence.",
                    "Tune EnemyAttack damage, knockback, hit timing, animation signal, and hitbox zone name.",
                    "Decide whether enemies are always present, spawned continuously, spawned in waves, or activated by an ArenaZone."
                },
                new[] { "boss AI", "dialogue/vendor/quest content", "advanced navigation", "loot tables", "network replication" },
                "Enter Play Mode and confirm one enemy spawns or activates, finds a player target, attacks through a named hitbox, and can take damage.",
                new[] { "Combat Attack Proof", "3D / 2.5D Pawn Movement", "UI And Scoring Feedback" },
                "Enemy AI uses a simple range-based detection system. Adjust 'Aggro Range' and 'Leash Range' to fit the size of your combat arenas.",
                "https://docs.neonblack.com/pyralis/enemies",
                new[]
                {
                    "route.npc-enemy-actor",
                    "proof.npc-enemy-behavior",
                    "scene-evidence.pickups-hazards-enemies",
                    "reflection.add-component-menu.enemy-spawner"
                }),

            new RuntimeCapabilityCard(
                "capability.ui-scoring-feedback",
                "UI And Scoring Feedback",
                RuntimeCapabilityFamily.ScoringObjectives,
                PyralisAuthoringRouteCapability.Scoring,
                "Scoring",
                true,
                "Score/objective change",
                "One gameplay event changes score, objective, timer, resource, or result state.",
                new[] { "UiHud", "Scoring" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.UiMenuOnly },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                "Visible route state such as score, health, prompt text, feedback, objective state, or menu/action labels.",
                "Use this after one runtime event works and the player needs to see the result without opening debug tools.",
                System.Array.Empty<string>(),
                new[] { "SettingsProfile when settings UI is part of the proof" },
                new[] { "UI Root with Canvas and EventSystem", "ParticipantScoreService when score changes are part of the route" },
                System.Array.Empty<string>(),
                new[]
                {
                    "HUD presenter -> relevant Pyralis service",
                    "ParticipantHealthHudBinder -> participant health panels",
                    "ParticipantFeedbackHudPresenter -> feedback stream",
                    "GameModeDefinition scoring fields -> scoring setup when objectives are route-owned"
                },
                new[]
                {
                    "Choose HUD layout, labels, prompts, text style, and when feedback appears.",
                    "Decide which state belongs in HUD, world-space feedback, pause/settings, or result screens."
                },
                new[] { "leaderboards", "results screens", "save persistence", "achievements", "full menu navigation" },
                "Enter Play Mode and confirm one gameplay event changes a visible label, panel, score value, prompt, health display, or feedback message.",
                new[] { "Interaction Or Action Selection", "Combat Attack Proof", "Camera Follow And Bounds" },
                "UI feedback should be responsive. Use the 'Feedback Hud Presenter' to queue messages so they don't overlap when many events happen at once.",
                "https://docs.neonblack.com/pyralis/ui")
};

        public static IReadOnlyList<RuntimeCapabilityCard> All => Cards;

        public static List<RuntimeCapabilityCard> GetByGoal(string goal)
{
            List<RuntimeCapabilityCard> matches = new List<RuntimeCapabilityCard>();
            for (int i = 0; i < Cards.Length; i++)
            {
                if (Cards[i].HasGoal(goal))
                    matches.Add(Cards[i]);
            }

            return matches;
        }

        public static List<RuntimeCapabilityCard> GetByLane(RuntimeCapabilityLaneTag lane)
        {
            List<RuntimeCapabilityCard> matches = new List<RuntimeCapabilityCard>();
            for (int i = 0; i < Cards.Length; i++)
            {
                if (Cards[i].HasLane(lane) || Cards[i].HasCautionLane(lane))
                    matches.Add(Cards[i]);
            }

            return matches;
        }

        public static RuntimeCapabilityCard FindPrimaryByFamily(RuntimeCapabilityFamily family)
        {
            for (int i = 0; i < Cards.Length; i++)
            {
                RuntimeCapabilityCard card = Cards[i];
                if (card.CapabilityFamily == family)
                    return card;
            }

            return null;
        }
    }

}
