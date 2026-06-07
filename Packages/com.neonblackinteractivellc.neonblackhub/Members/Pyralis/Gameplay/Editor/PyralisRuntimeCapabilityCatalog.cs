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
    public enum RuntimeCapabilityGoalTag
    {
        Movement,
        Camera,
        Interaction,
        Combat,
        Projectiles,
        UiHud,
        Scoring,
        Tabletop,
        Networking,
        NpcsEnemies,
        JumpTraversal,
        Input,
        AnimationPresentation
    }

    public enum RuntimeCapabilityLaneTag
    {
        Sprite2D,
        Billboard2_5D,
        Rigged3D,
        TabletopNoPawn,
        UiMenu,
        CameraCursor,
        Networked
    }

    public sealed class RuntimeCapabilityCard
    {
        public RuntimeCapabilityCard(
            string stableId,
            string displayName,
            RuntimeCapabilityFamily capabilityFamily,
            RuntimeCapabilityGoalTag[] goalTags,
            RuntimeCapabilityLaneTag[] laneTags,
            RuntimeCapabilityLaneTag[] cautionLaneTags,
            string whatItAdds,
            string whenToUse,
            string[] requiredDefinitions,
            string[] requiredProfiles,
            string[] requiredSceneComponents,
            string[] requiredPrefabComponents,
            string[] assignmentFields,
            string[] customizationMoments,
            string[] canWait,
            string firstProof,
            string[] commonNextCapabilities,
            string[] relatedStableIds = null)
        {
            StableId = stableId;
            DisplayName = displayName;
            CapabilityFamily = capabilityFamily;
            GoalTags = goalTags ?? System.Array.Empty<RuntimeCapabilityGoalTag>();
            LaneTags = laneTags ?? System.Array.Empty<RuntimeCapabilityLaneTag>();
            CautionLaneTags = cautionLaneTags ?? System.Array.Empty<RuntimeCapabilityLaneTag>();
            WhatItAdds = whatItAdds;
            WhenToUse = whenToUse;
            RequiredDefinitions = requiredDefinitions ?? System.Array.Empty<string>();
            RequiredProfiles = requiredProfiles ?? System.Array.Empty<string>();
            RequiredSceneComponents = requiredSceneComponents ?? System.Array.Empty<string>();
            RequiredPrefabComponents = requiredPrefabComponents ?? System.Array.Empty<string>();
            AssignmentFields = assignmentFields ?? System.Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? System.Array.Empty<string>();
            CanWait = canWait ?? System.Array.Empty<string>();
            FirstProof = firstProof;
            CommonNextCapabilities = commonNextCapabilities ?? System.Array.Empty<string>();
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
                ToStrings(GoalTags),
                ToStrings(LaneTags),
                ToStrings(CautionLaneTags),
                RequiredDefinitions,
                RequiredProfiles,
                RequiredSceneComponents,
                RequiredPrefabComponents,
                AssignmentFields,
                CustomizationMoments,
                CanWait,
                NativeActions,
                relatedStableIds: relatedStableIds);
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public RuntimeCapabilityFamily CapabilityFamily { get; }
        public RuntimeCapabilityGoalTag[] GoalTags { get; }
        public RuntimeCapabilityLaneTag[] LaneTags { get; }
        public RuntimeCapabilityLaneTag[] CautionLaneTags { get; }
        public string WhatItAdds { get; }
        public string WhenToUse { get; }
        public string[] RequiredDefinitions { get; }
        public string[] RequiredProfiles { get; }
        public string[] RequiredSceneComponents { get; }
        public string[] RequiredPrefabComponents { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }
        public string[] CanWait { get; }
        public string FirstProof { get; }
        public string[] CommonNextCapabilities { get; }
        public PyralisAuthoringNativeAction[] NativeActions { get; }
        public PyralisAuthoringFact Fact { get; }

        public bool HasGoal(RuntimeCapabilityGoalTag tag)
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

        private static bool Contains(RuntimeCapabilityGoalTag[] tags, RuntimeCapabilityGoalTag tag)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
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

        private static string[] ToStrings(RuntimeCapabilityGoalTag[] tags)
        {
            string[] values = new string[tags.Length];
            for (int i = 0; i < tags.Length; i++)
                values[i] = tags[i].ToString();

            return values;
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
                new[]
                {
                    RuntimeCapabilityGoalTag.Movement,
                    RuntimeCapabilityGoalTag.JumpTraversal,
                    RuntimeCapabilityGoalTag.Input,
                    RuntimeCapabilityGoalTag.AnimationPresentation
                },
                new[] { RuntimeCapabilityLaneTag.Sprite2D },
                new[] { RuntimeCapabilityLaneTag.Rigged3D, RuntimeCapabilityLaneTag.TabletopNoPawn },
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
                new[]
                {
                    RuntimeCapabilityGoalTag.Movement,
                    RuntimeCapabilityGoalTag.JumpTraversal,
                    RuntimeCapabilityGoalTag.Input,
                    RuntimeCapabilityGoalTag.AnimationPresentation,
                    RuntimeCapabilityGoalTag.Combat,
                    RuntimeCapabilityGoalTag.NpcsEnemies
                },
                new[] { RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.Rigged3D },
                new[] { RuntimeCapabilityLaneTag.TabletopNoPawn },
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
                    "PawnDefinition.presentationProfile -> Billboard2_5D or Rigged3D presentation",
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
                new[] { RuntimeCapabilityGoalTag.Camera },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.Rigged3D, RuntimeCapabilityLaneTag.CameraCursor },
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
                new[] { "2D Pawn Movement", "Combat Attack Proof", "Interaction Or Action Selection", "UI And Scoring Feedback" }),

            new RuntimeCapabilityCard(
                "capability.interaction-action-selection",
                "Interaction Or Action Selection",
                RuntimeCapabilityFamily.ActionTargeting,
                new[] { RuntimeCapabilityGoalTag.Interaction, RuntimeCapabilityGoalTag.Input, RuntimeCapabilityGoalTag.Tabletop },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.Rigged3D, RuntimeCapabilityLaneTag.TabletopNoPawn, RuntimeCapabilityLaneTag.UiMenu, RuntimeCapabilityLaneTag.CameraCursor },
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
                new[] { "UI And Scoring Feedback", "Combat Attack Proof", "Camera Follow And Bounds" }),

            new RuntimeCapabilityCard(
                "capability.combat-projectile-proof",
                "Combat Attack Proof",
                RuntimeCapabilityFamily.Combat,
                new[]
                {
                    RuntimeCapabilityGoalTag.Combat,
                    RuntimeCapabilityGoalTag.Input,
                    RuntimeCapabilityGoalTag.AnimationPresentation,
                    RuntimeCapabilityGoalTag.Projectiles,
                    RuntimeCapabilityGoalTag.NpcsEnemies
                },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.Rigged3D, RuntimeCapabilityLaneTag.CameraCursor },
                new[] { RuntimeCapabilityLaneTag.TabletopNoPawn },
                "A smallest attack, hit, shot, damage, or reaction path for a pawn, NPC/enemy, trap, turret, or command source.",
                "Use this after the source surface exists and the first proof needs cause-and-effect combat or projectile feedback.",
                new[] { "CombatActionDefinition", "CombatSequenceDefinition", "EnemyAttack", "ProjectileDefinition", "ProjectileImpactDefinition", "FireModeDefinition when firing cadence matters" },
                new[] { "PawnCombatProfile", "EnemyCombatProfile", "ActorFeedbackProfile", "ActorCombatReactionProfile" },
                new[] { "HealthComponent or target health surface", "projectile launcher, HitBox, EnemySpawner, or ArenaZone surface" },
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
                new[] { "UI And Scoring Feedback", "Interaction Or Action Selection", "Camera Follow And Bounds" }),

            new RuntimeCapabilityCard(
                "capability.npc-enemy-setup",
                "NPC / Enemy Actor Setup",
                RuntimeCapabilityFamily.Combat,
                new[]
                {
                    RuntimeCapabilityGoalTag.NpcsEnemies,
                    RuntimeCapabilityGoalTag.Combat,
                    RuntimeCapabilityGoalTag.AnimationPresentation,
                    RuntimeCapabilityGoalTag.Movement
                },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.Rigged3D },
                new[] { RuntimeCapabilityLaneTag.UiMenu },
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
                new[] { RuntimeCapabilityGoalTag.UiHud, RuntimeCapabilityGoalTag.Scoring },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.Rigged3D, RuntimeCapabilityLaneTag.TabletopNoPawn, RuntimeCapabilityLaneTag.UiMenu },
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
                new[] { "Interaction Or Action Selection", "Combat Attack Proof", "Camera Follow And Bounds" })
        };

        public static IReadOnlyList<RuntimeCapabilityCard> All => Cards;

        public static List<RuntimeCapabilityCard> GetByGoal(RuntimeCapabilityGoalTag goal)
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
    }

}
