using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum ResolvedAuthoringContractProofState
    {
        ProofTargetMissing,
        ProofBlockedBySetup,
        ProofNotRunInPlayMode
    }

    public sealed class ResolvedAuthoringContractProofGuidanceRow
    {
        public ResolvedAuthoringContractProofGuidanceRow(
            ResolvedAuthoringContract contract,
            FeatureModuleDefinition module,
            PyralisAuthoringFact proofFact,
            ResolvedAuthoringContractProofState state,
            ActorPresentationMode? activeLane)
        {
            Contract = contract;
            Module = module;
            ProofFact = proofFact;
            State = state;
            ActiveLane = activeLane;
        }

        public ResolvedAuthoringContract Contract { get; }
        public FeatureModuleDefinition Module { get; }
        public PyralisAuthoringFact ProofFact { get; }
        public ResolvedAuthoringContractProofState State { get; }
        public ActorPresentationMode? ActiveLane { get; }
        public bool ProofTargetExists => ProofFact != null;
        public bool PlayModeProofRequired => ProofTargetExists && State == ResolvedAuthoringContractProofState.ProofNotRunInPlayMode;
        public bool BlocksProof => State == ResolvedAuthoringContractProofState.ProofTargetMissing || State == ResolvedAuthoringContractProofState.ProofBlockedBySetup;
        public bool HasUnsupportedLaneCaution => ActiveLane.HasValue && Contract != null && Contract.IsExplicitlyUnsupported(ActiveLane.Value);
    }

    public static class ResolvedAuthoringContractProofGuidance
    {
        private sealed class ActiveModuleContext
        {
            public ActiveModuleContext(FeatureModuleDefinition module, ActorPresentationMode? lane)
            {
                Module = module;
                Lane = lane;
            }

            public FeatureModuleDefinition Module { get; }
            public ActorPresentationMode? Lane { get; }
        }

        public static IReadOnlyList<ResolvedAuthoringContractProofGuidanceRow> Build(Object activeSetup, PyralisAuthoringRouteReport routeReport)
        {
            List<ResolvedAuthoringContractProofGuidanceRow> rows = new List<ResolvedAuthoringContractProofGuidanceRow>();
            List<ActiveModuleContext> moduleContexts = CollectActiveModuleContexts(activeSetup);
            bool setupBlocked = routeReport != null && routeReport.ValidationIssues.Count > 0;

            for (int i = 0; i < moduleContexts.Count; i++)
            {
                ActiveModuleContext context = moduleContexts[i];
                if (context.Module == null || string.IsNullOrWhiteSpace(context.Module.moduleId))
                    continue;

                ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId(context.Module.moduleId);
                if (contract == null)
                    continue;

                PyralisAuthoringFact proofFact = PyralisAuthoringRouteProof.FindProofFact(contract.FirstProofTargetId);
                ResolvedAuthoringContractProofState state = GetState(proofFact, setupBlocked);
                rows.Add(new ResolvedAuthoringContractProofGuidanceRow(contract, context.Module, proofFact, state, context.Lane));
            }

            return rows;
        }

        private static ResolvedAuthoringContractProofState GetState(PyralisAuthoringFact proofFact, bool setupBlocked)
        {
            if (proofFact == null)
                return ResolvedAuthoringContractProofState.ProofTargetMissing;

            return setupBlocked
                ? ResolvedAuthoringContractProofState.ProofBlockedBySetup
                : ResolvedAuthoringContractProofState.ProofNotRunInPlayMode;
        }

        private static List<ActiveModuleContext> CollectActiveModuleContexts(Object activeSetup)
        {
            List<ActiveModuleContext> modules = new List<ActiveModuleContext>();

            if (activeSetup is FeatureModuleDefinition featureModule)
            {
                AddModule(modules, featureModule, null);
                return modules;
            }

            if (activeSetup is PawnDefinition pawn)
            {
                AddPawnModules(modules, pawn);
                return modules;
            }

            if (activeSetup is ParticipantDefinition participant)
            {
                AddPawnModules(modules, participant.defaultPawn);
                return modules;
            }

            if (activeSetup is GameModeDefinition mode)
            {
                AddGameModeModules(modules, mode);
                return modules;
            }

            if (activeSetup is SessionDefinition session)
            {
                AddSessionModules(modules, session);
                return modules;
            }

            if (activeSetup is GameplaySessionBootstrap bootstrap)
            {
                SessionDefinition bootstrapSession = PyralisAuthoringSetupContextResolver.GetSelectedSession(bootstrap, bootstrap);
                AddSessionModules(modules, bootstrapSession);
            }

            return modules;
        }

        private static void AddSessionModules(List<ActiveModuleContext> modules, SessionDefinition session)
        {
            if (session == null)
                return;

            AddGameModeModules(modules, session.defaultGameMode);

            if (session.defaultParticipants == null)
                return;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null)
                    AddPawnModules(modules, participant.defaultPawn);
            }
        }

        private static void AddGameModeModules(List<ActiveModuleContext> modules, GameModeDefinition mode)
        {
            if (mode == null || mode.requiredFeatureModules == null)
                return;

            for (int i = 0; i < mode.requiredFeatureModules.Length; i++)
                AddModule(modules, mode.requiredFeatureModules[i], null);
        }

        private static void AddPawnModules(List<ActiveModuleContext> modules, PawnDefinition pawn)
        {
            if (pawn == null || pawn.featureModules == null)
                return;

            ActorPresentationMode? lane = pawn.presentationProfile != null ? pawn.presentationProfile.presentationMode : null;
            for (int i = 0; i < pawn.featureModules.Length; i++)
                AddModule(modules, pawn.featureModules[i], lane);
        }

        private static void AddModule(List<ActiveModuleContext> modules, FeatureModuleDefinition module, ActorPresentationMode? lane)
        {
            if (module == null)
                return;

            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i].Module == module)
                    return;
            }

            modules.Add(new ActiveModuleContext(module, lane));
        }
    }

    public sealed class PyralisAuthoringProofStep
    {
        public PyralisAuthoringProofStep(PyralisAuthoringRouteCapability capability, string label, string successCriteria)
        {
            Capability = capability;
            Label = label ?? string.Empty;
            SuccessCriteria = successCriteria ?? string.Empty;
        }

        public PyralisAuthoringRouteCapability Capability { get; }
        public string Label { get; }
        public string SuccessCriteria { get; }
    }

    public sealed class PyralisAuthoringRouteProof
    {
        private PyralisAuthoringRouteProof(
            string stableId,
            string label,
            string guidance,
            string setupSurface,
            string successCriteria,
            string deferUntilAfter,
            string firstUnityFocus,
            PyralisAuthoringProofStep[] proofChain = null)
        {
            StableId = stableId ?? string.Empty;
            Label = label;
            Guidance = guidance;
            SetupSurface = setupSurface;
            SuccessCriteria = successCriteria;
            DeferUntilAfter = deferUntilAfter;
            FirstUnityFocus = firstUnityFocus;
            ProofChain = proofChain ?? System.Array.Empty<PyralisAuthoringProofStep>();
        }

        public string StableId { get; }
        public string Label { get; }
        public string Guidance { get; }
        public string SetupSurface { get; }
        public string SuccessCriteria { get; }
        public string DeferUntilAfter { get; }
        public string FirstUnityFocus { get; }
        public PyralisAuthoringProofStep[] ProofChain { get; }
        public string ProofChainSummary => BuildProofChainSummary(ProofChain);

        public static System.Collections.Generic.IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            System.Collections.Generic.List<PyralisAuthoringFact> facts = new System.Collections.Generic.List<PyralisAuthoringFact>();
            facts.Add(CreateProofFact(
                "proof.1p-pawn-movement",
                "1P Pawn Movement Proof",
                "Run one local pawn-backed movement proof before adding combat, HUD, enemies, scoring, or networking.",
                "2D pawn movement route",
                "One participant spawns one pawn, the selected InputProfile reaches a pawn input module, movement is visibly responsive, and the Game view follows the runtime shared camera focus.",
                new[] { "Movement" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "SessionDefinition", "ParticipantDefinition", "PawnDefinition" },
                new[] { "InputProfile", "PawnMovementProfile", "PawnPresentationProfile" },
                new[] { "GameplaySessionBootstrap", "PyralisGameplayLifetimeScope", "Spawn Point Transform" },
                new[] { "PawnRoot", "IPawnMotor", "IPawnInputModule", "IPawnPresentationModule" },
                new[]
                {
                    "SessionDefinition.defaultParticipants -> ParticipantDefinition",
                    "ParticipantDefinition.defaultPawn -> PawnDefinition",
                    "PawnDefinition.pawnPrefab -> pawn prefab",
                    "Pawn prefab input module -> mapped InputProfile Move action",
                    "GameplaySessionBootstrap.spawnPoints -> scene spawn Transform"
                },
                new[]
                {
                    "Tune pawn visuals, collision, pivot, and lane presentation.",
                    "Tune PawnMovementProfile speed/acceleration/jump/dash feel.",
                    "Map InputProfile action names to the project's input action asset."
                },
                new[] { "combat", "projectiles", "HUD", "scoring", "pickups", "hazards", "networking", "local join" },
                "the active 2D pawn route",
                "press Play after the selected intent's Do Now setup is clear; move the pawn through the mapped input path and watch the Game view",
                "one pawn spawns at the assigned spawn point, visibly moves, and the Game view follows the runtime GameplaySharedCameraFocus driven by that pawn",
                "The 1P pawn proof is the foundation of the player experience. Ensure movement feels responsive before adding more complexity.",
                "https://docs.neonblack.com/pyralis/movement",
                new[]
                {
                    "route.pawn-actor",
                    "capability.2d-pawn-movement",
                    "setup.assign-session-definition",
                    "setup.assign-default-game-mode",
                    "setup.assign-setup-profile",
                    "setup.add-runtime-patterns",
                    "setup.assign-default-participants",
                    "setup.assign-participant-pawn",
                    "setup.assign-input-profile",
                    "setup.assign-spawn-points",
                    "setup.assign-camera-rig",
                    "setup.tune-pawn-visuals-and-collision",
                    "setup.tune-movement-and-input-feel"
                }));

            facts.Add(CreateProofFact(
                "proof.board-card-action",
                "Board Card Action Proof",
                "Run one rules-backed tabletop selection before adding card UX, AI turns, campaign flow, or networking.",
                "tabletop board/card route",
                "One board space, card, seat command, or turn action is selected; Pyralis accepts or rejects it through rules; and board/card/turn state visibly or inspectably changes.",
                new[] { "Tabletop", "Interaction" },
                new[] { RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.CameraCursor },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                new[] { "BoardDefinition", "BoardMovePolicyDefinition", "TurnOrderDefinition", "ActionDefinition" },
                System.Array.Empty<string>(),
                new[] { "TabletopBoardGridPresenter or equivalent board/card selection surface", "Canvas or cursor bridge when selection is UI-driven" },
                System.Array.Empty<string>(),
                new[] { "GameModeDefinition.boardDefinition", "GameModeDefinition.turnOrderDefinition", "selection surface -> action resolver" },
                new[] { "Choose board layout, legal moves, turn/phase order, card/seat ownership, and selection feedback." },
                new[] { "pawn actors", "final board art", "full card UX", "AI turns", "shops", "deckbuilding", "networking", "campaign flow" },
                "the active tabletop route",
                "press Play after one board/card/seat command surface is wired; choose one legal or illegal action",
                "one selection reaches rules and visibly or inspectably changes board, card, turn, score, or UI state",
                "Tabletop proofs verify that your rules engine is correctly processing player choices. Use deterministic board state for testing.",
                "https://docs.neonblack.com/pyralis/tabletop",
                new[] { "route.tabletop-card", "capability.interaction-action-selection", "capability.ui-scoring-feedback" }));

            facts.Add(CreateProofFact(
                "proof.action-selection",
                "Action Selection Proof",
                "Run one selected command before expanding menus, cards, ability lists, animation polish, or AI.",
                "action/menu/cursor route",
                "One command reaches its resolver and reports accepted, rejected, completed, or failed.",
                new[] { "Interaction", "Tabletop" },
                new[] { RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.CameraCursor, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "ActionDefinition" },
                new[] { "InputProfile when input drives the command", "InteractionFeatureProfile when actor interaction is used" },
                new[] { "UI button, cursor target, card, board space, interaction trigger, or pawn action surface" },
                new[] { "lane-specific action trigger when the source is a pawn" },
                new[] { "Action presenter or resolver -> ActionDefinition", "selected surface -> action resolver or bridge" },
                new[] { "Choose command source, target filtering, costs, cooldowns, prompts, and accepted/rejected feedback." },
                new[] { "large menus", "card rewards", "ability trees", "animation polish", "AI", "scoring", "networking" },
                "the active action route",
                "press Play and select one command from its authored surface",
                "the resolver receives the command and reports a clear accepted, rejected, completed, or failed result",
                "Action selection is the core of player agency. Ensure that the player always knows why an action was accepted or rejected.",
                "https://docs.neonblack.com/pyralis/interaction",
                new[] { "route.custom-object-feature", "route.ui-hud-menu", "route.tabletop-card", "capability.interaction-action-selection" }));

            facts.Add(CreateProofFact(
                "proof.npc-enemy-behavior",
                "NPC Enemy Behavior Proof",
                "Run one NPC or enemy behavior proof before building encounter waves, boss phases, vendors, or broad AI systems.",
                "NPC/enemy actor route",
                "One NPC or enemy appears, is detected or interacted with, and performs one authored behavior or combat reaction.",
                new[] { "NpcsEnemies", "Combat", "Interaction" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                new[] { RuntimeCapabilityLaneTag.UiMenuOnly },
                new[] { "NpcDefinition", "ParticipantDefinition or enemy actor definition", "FeatureModuleDefinition" },
                new[] { "EnemyFeatureProfile", "EnemyCombatProfile", "EnemyReactionProfile", "EnemyAmbientFeatureProfile" },
                new[] { "Spawner, encounter zone, dialogue/vendor/quest presenter, or authored actor root" },
                new[] { "enemy/NPC runtime components", "HealthComponent when combat is expected" },
                new[] { "FeatureModuleDefinition.profileAsset", "actor prefab -> enemy/NPC runtime components" },
                new[] { "Choose AI role, faction/team, patrol/encounter surface, dialogue/vendor/quest content, and combat reaction style." },
                new[] { "waves", "boss phases", "shops", "quest chains", "loot tables", "network replication", "full AI debugging" },
                "the active NPC/enemy route",
                "press Play after one NPC/enemy surface is wired; trigger one interaction, detection, or attack",
                "one authored NPC/enemy behavior is visible or inspectable without requiring a complete encounter loop",
                "NPC behavior proofs should focus on the individual actor's logic. Group behaviors like wave spawning should be tested later.",
                "https://docs.neonblack.com/pyralis/enemies",
                new[] { "route.npc-enemy-actor", "capability.combat-projectile-proof", "capability.interaction-action-selection" }));

            facts.Add(CreateProofFact(
                "proof.custom-object-effect",
                "Custom Object Effect Proof",
                "Run one custom object, feature, trigger, pickup, hazard, turret, trap, or service effect before treating it as a full system.",
                "custom object/feature route",
                "One authored scene object or feature produces a visible accepted, rejected, completed, damaged, collected, triggered, or scored effect.",
                new[] { "Interaction", "Combat", "Scoring" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "FeatureModuleDefinition", "ActionDefinition or feature-specific definition when commands are involved" },
                new[] { "PickupFeatureProfile", "HazardFeedbackProfile", "InteractionFeatureProfile, or feature-specific profile" },
                new[] { "authored trigger, pickup, hazard, actor feature host, or feature runtime object" },
                new[] { "feature runtime component for the selected lane" },
                new[] { "FeatureModuleDefinition.runtimePrefab", "feature profile -> runtime component" },
                new[] { "Choose whether the object is scenery, trigger, pickup, hazard, turret, trap, service, or custom action source." },
                new[] { "secondary variants", "spawn tables", "economy", "large VFX", "network replication", "shipping automation" },
                "the active custom object route",
                "press Play and trigger one authored object or feature",
                "one object produces a visible or inspectable gameplay effect",
                "Custom objects are often specific to one level or mechanic. Use Proof scenes to iterate on their visual and logical feel.",
                "https://docs.neonblack.com/pyralis/features",
                new[] { "route.custom-object-feature", "capability.interaction-action-selection", "capability.combat-projectile-proof", "capability.ui-scoring-feedback" }));

            facts.Add(CreateProofFact(
                "proof.ui-hud-menu",
                "UI HUD Menu Proof",
                "Run one UI, HUD, prompt, score, health, feedback, or menu event before building full navigation or result screens.",
                "UI/HUD/menu route",
                "One UI event changes visible state or sends one command to a resolver.",
                new[] { "UiHud", "Scoring", "Interaction" },
                new[] { RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "ActionDefinition when UI triggers gameplay commands" },
                new[] { "SettingsProfile when settings or rebinding are part of the route" },
                new[] { "Canvas", "EventSystem", "HUD/menu presenter or feedback panel" },
                System.Array.Empty<string>(),
                new[] { "HUD presenter -> gameplay service", "UI button/menu/card -> action resolver or command surface" },
                new[] { "Choose layout, labels, navigation order, feedback timing, accessibility, and whether UI is screen-space or world-space." },
                new[] { "full menu navigation", "save slots", "results screens", "leaderboards", "achievements", "localization polish" },
                "the active UI/HUD/menu route",
                "press Play and trigger one UI, HUD, prompt, score, health, feedback, or menu event",
                "one visible label, panel, score value, prompt, health display, feedback message, or command result changes",
                "UI proofs ensure that the data-flow between the engine and the Canvas is healthy. Use Placeholder art during this stage.",
                "https://docs.neonblack.com/pyralis/ui",
                new[] { "route.ui-hud-menu", "capability.ui-scoring-feedback", "capability.interaction-action-selection" }));

            facts.Add(CreateProofFact(
                "proof.camera-cursor-world",
                "Camera Cursor World Proof",
                "Run one camera, cursor, bounds, or world-surface proof before adding multi-target framing or cinematic polish.",
                "world/camera route",
                "One input or selected target changes camera, cursor, framing, bounds, highlighted surface, or scene visibility as authored.",
                new[] { "Camera", "Movement", "Interaction" },
                new[] { RuntimeCapabilityLaneTag.CameraCursor, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                System.Array.Empty<string>(),
                new[] { "CameraRigProfile", "PlayfieldProfile" },
                new[] { "Camera Root", "CinemachineCameraRigController", "physical Camera", "camera bounds source or world surface" },
                System.Array.Empty<string>(),
                new[] { "GameplaySessionBootstrap.cameraRigController", "CinemachineCameraRigController.cameraRigProfile", "camera bounds source" },
                new[] { "Choose orthographic/perspective framing, follow target, bounds, split-screen behavior, and scene-service ownership." },
                new[] { "multi-target cameras", "split-screen polish", "camera shake", "cinematic transitions", "full action targeting" },
                "the active camera/cursor/world route",
                "press Play and move/select the authored camera, cursor, target, bounds, or world surface",
                "visibility, framing, bounds, cursor, highlight, or scene response changes as authored",
                "Camera and cursor proofs are vital for navigation-heavy routes. Ensure your raycast masks are correctly configured.",
                "https://docs.neonblack.com/pyralis/camera",
                new[] { "route.world-camera", "capability.camera-follow-bounds", "setup.assign-camera-rig", "setup.assign-camera-bounds-service" }));

            facts.Add(CreateProofFact(
                "proof.generated-content",
                "Generated Content Proof",
                "Generate one inspectable output before making generated content required for progression.",
                "procedural/generated-content route",
                "One generated output is deterministic enough to inspect in the scene or logs and does not block the route if generation is disabled.",
                new[] { "Interaction", "Scoring" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.CameraCursor },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "segment, chunk, board layout, spawn table, or feature-specific generation definition" },
                new[] { "generator profile, spawn budget, socket rule, or board layout profile" },
                new[] { "generator output root or logged deterministic output" },
                System.Array.Empty<string>(),
                new[] { "generator profile -> authored chunks/sockets/seeds/spawn budgets" },
                new[] { "Choose chunks, sockets, seeds, spawn budgets, board layouts, and validation surfaces before hiding generation behind game flow." },
                new[] { "full biomes", "unreachable-room validation", "rewards", "difficulty curves", "runtime dependency on generation" },
                "the active generated-content route",
                "press Play or run the route-owned generation action and inspect the output root or log",
                "one generated result is visible, deterministic enough to inspect, or logged clearly",
                "Generated content should always be 'debuggable'. Ensure you can manually set a Seed to reproduce specific issues.",
                "https://docs.neonblack.com/pyralis/generation",
                new[] { "route.custom-object-feature", "route.world-camera" }));

            facts.Add(CreateProofFact(
                "proof.network-ownership",
                "Network Ownership Proof",
                "Confirm the local proof first, then prove one host/client ownership path before expanding replication.",
                "networking authority route",
                "Host/client can connect, the owned participant controls the expected surface, and one replicated state change is visible without breaking the local proof.",
                new[] { "Networking", "Movement", "Combat" },
                new[] { RuntimeCapabilityLaneTag.Mixed, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                new[] { RuntimeCapabilityLaneTag.TabletopBoard },
                new[] { "SessionDefinition" },
                System.Array.Empty<string>(),
                new[] { "GameplaySessionBootstrap", "PyralisGameplayLifetimeScope", "NetworkManager", "UnityTransport" },
                new[] { "NetworkObject", "network-aware pawn or participant runtime components when the selected lane supports them" },
                new[] { "SessionDefinition.networkMode", "SessionDefinition.localFirst", "NetworkManager.NetworkPrefabs", "networked ownership/authority service configuration" },
                new[] { "Choose local-only, host, client, or server authority and decide which participant actions replicate." },
                new[] { "prediction", "rollback", "matchmaking", "backend persistence", "broad replication", "Steam lobby integration" },
                "the active networking route",
                "after the local proof passes, start host/client and exercise the owned participant surface",
                "ownership, spawn, input authority, and one replicated state change behave as authored",
                "Networking is easiest to debug when local-first. Always ensure the local proof is rock-solid before testing over transport.",
                "https://docs.neonblack.com/pyralis/networking",
                new[] { "route.networking", "capability.2d-pawn-movement", "capability.combat-projectile-proof" }));

            return PyralisContractProofFactProjector.EnrichRouteProofFacts(facts);
        }

        public static PyralisAuthoringFact FindProofFact(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                return null;

            System.Collections.Generic.IReadOnlyList<PyralisAuthoringFact> facts = GetAuthoringFacts();
            System.Collections.Generic.HashSet<string> existingProofIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i] == null)
                    continue;

                existingProofIds.Add(facts[i].StableId);
                if (facts[i].MatchesStableId(stableId))
                    return facts[i];
            }

            return PyralisContractProofFactProjector.FindProofFact(stableId, existingProofIds);
        }

        private static PyralisAuthoringFact CreateProofFact(
            string stableId,
            string displayName,
            string summary,
            string routeRelevance,
            string firstProof,
            string[] goalTags,
            RuntimeCapabilityLaneTag[] laneTags,
            RuntimeCapabilityLaneTag[] unsupportedLaneTags,
            string[] requiredDefinitions,
            string[] requiredProfiles,
            string[] requiredSceneComponents,
            string[] requiredUnitySurfaces,
            string[] assignmentFields,
            string[] customizationMoments,
            string[] canWait,
            string nativeActionTarget,
            string nativeActionInstructions,
            string nativeActionSuccess,
            string expertAdvice,
            string documentationURL,
            string[] relatedStableIds)
        {
            return new PyralisAuthoringFact(
                stableId,
                displayName,
                PyralisAuthoringFactKind.Proof,
                PyralisAuthoringFactSourceKind.SetupFlow,
                PyralisAuthoringConfidence.Explicit,
                summary,
                routeRelevance,
                firstProof,
                goalTags,
                laneTags: ToStrings(laneTags),
                unsupportedLaneTags: ToStrings(unsupportedLaneTags),
                requiredDefinitions: requiredDefinitions,
                requiredProfiles: requiredProfiles,
                requiredSceneComponents: requiredSceneComponents,
                requiredUnitySurfaces: requiredUnitySurfaces,
                assignmentFields: assignmentFields,
                customizationMoments: customizationMoments,
                canWait: canWait,
                nativeActions: new[]
                {
                    new PyralisAuthoringNativeAction(
                        "Test",
                        PyralisAuthoringActionSurface.PlayMode,
                        nativeActionTarget,
                        nativeActionInstructions,
                        nativeActionSuccess)
                },
                workIntent: "FirstProof",
                relatedStableIds: relatedStableIds,
                expertAdvice: expertAdvice,
                documentationURL: documentationURL);
        }

        

        private static string[] ToStrings(RuntimeCapabilityLaneTag[] tags)
        {
            string[] values = new string[tags.Length];
            for (int i = 0; i < tags.Length; i++)
                values[i] = tags[i] == RuntimeCapabilityLaneTag.Mixed ? "Networked" : tags[i].ToString();

            return values;
        }

        public static PyralisAuthoringRouteProof Build(PyralisAuthoringRouteDescriptor route)
        {
            if (route == null || !route.HasSelectedCapabilities)
            {
                return new PyralisAuthoringRouteProof(
                    string.Empty,
                    "Choose Capability Ingredients",
                    "Use Intent or the GameSetupProfile Runtime Capabilities section to describe the route before wiring scene objects.",
                    "Choose capability families before deciding which scene or prefab surface matters.",
                    "The setup profile contains at least one selected capability family.",
                    "Defer scene wiring, scene building, optional route metadata, and optional systems until the setup intent is clear.",
                    "First Unity focus: choose the capability ingredients that describe the game surface before wiring scene roots.");
            }

            PyralisAuthoringProofStep[] proofChain = BuildProofChain(route);

            if (route.HasPawn)
                return BuildPawnProof(route, proofChain);

            if (route.HasTabletop)
                return BuildTabletopProof(route, proofChain);

            if (route.HasActions)
                return BuildActionProof(route, proofChain);

            if (route.HasProjectiles)
                return BuildProjectileProof(route, proofChain);

            if (route.HasCombat)
                return BuildCombatProof(route, proofChain);

            if (route.HasScoring)
                return BuildScoringProof(route, proofChain);

            if (route.HasCamera)
                return BuildCameraProof(route, proofChain);

            if (route.HasProcedural)
                return BuildProceduralProof(route, proofChain);

            if (route.HasNetworking)
                return BuildNetworkProof(route, proofChain);

            return new PyralisAuthoringRouteProof(
                string.Empty,
                "Smallest Playable Proof",
                "Run one route-specific interaction in Play mode, confirm the result is visible or inspectable, then expand optional setup.",
                "One small route-owned Unity scene surface that can show a result in Play mode.",
                "One small route-specific interaction produces an inspectable result.",
                "Defer optional systems until the first route interaction works.",
                "First Unity focus: choose the control surface that receives input or drives rules, then wire one proof before adding more systems.",
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildPawnProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            string later = route.HasCombat || route.HasProjectiles
                ? "Defer hitboxes, projectiles, enemies, scoring, HUD, networking, respawn complexity, and advanced camera polish until one 1P pawn movement proof is reliable."
                : "Defer scoring, HUD, pickups, combat, networking, enemy setup, respawn complexity, and advanced camera polish until one 1P pawn movement proof is reliable.";

            return new PyralisAuthoringRouteProof(
                "proof.1p-pawn-movement",
                "1P Pawn Movement Proof",
                "Run a minimal Play-mode proof: wire one participant input profile to one pawn-backed participant, spawn the pawn, then confirm one visible movement input path before adding combat, HUD, enemies, score rules, or network authority.",
                "One participant, one PawnDefinition with prefab, one scene Transform in Spawn Points, and one Input Profile path on that participant or SessionDefinition.defaultInputProfile. Add camera bounds only if framing is part of this proof.",
                "One participant spawns one pawn, input reaches the pawn through the selected InputProfile, and movement is visibly responsive. Core session services are present or auto-created by GameplaySessionBootstrap.",
                later,
                "First Unity focus: create or inspect the pawn prefab stack, then link ParticipantDefinition, PawnDefinition, InputProfile, and Spawn Points. For a 2D proof, the prefab root should carry PawnRoot, Motor2D, Motor2DInputAdapter, SpriteRenderer, and Animator before Play Mode.",
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildTabletopProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            string focus = route.HasActions
                ? "First Unity focus: create board/card/seat state plus one action selection surface. Keep pawn fields empty while the route is seat, hand, faction, board, or menu driven."
                : "First Unity focus: create board/card/seat state and one selection surface. Keep pawn fields empty while the route is seat, hand, faction, board, or menu driven.";

            return new PyralisAuthoringRouteProof(
                "proof.board-card-action",
                "Board/Card Action Proof",
                "Run one rules-backed selection in Play mode: choose one board space, card, seat command, or turn action, then confirm the route accepts, rejects, or resolves it through a visible selection surface.",
                "One board, card, seat, or command selection surface. Use a TabletopBoardGridPresenter, TabletopBoardSelectionBridge, card-hand presenter, UI button, cursor bridge, collider/raycast target, or project-owned equivalent.",
                "The developer can choose one legal board space, card, seat command, or turn action; Pyralis accepts or rejects it through rules; and board/card/turn state visibly or inspectably changes.",
                "Defer pawn actors, final board art, full card UX, AI turns, complex scoring, shops, deckbuilding, networking, and campaign flow until one rules-backed selection works.",
                focus,
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildActionProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            return new PyralisAuthoringRouteProof(
                "proof.action-selection",
                "Action Selection Proof",
                "Run one command in Play mode and confirm it reaches its resolver with a clear accepted, rejected, completed, or failed result before expanding menus, cards, or ability lists.",
                "One selectable command surface, such as a button, card, board space, cursor target, or action presenter connected to one ActionDefinition.",
                "The developer can choose one command and see it reach the intended resolver with a clear accepted, rejected, completed, or failed result.",
                "Defer large menus, card rewards, ability trees, animation polish, AI, scoring, and networking until one command resolves.",
                "First Unity focus: create one ActionDefinition and one selection surface such as a Canvas button, cursor, card, board space, or pawn action trigger.",
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildProjectileProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            return new PyralisAuthoringRouteProof(
                "proof.custom-object-effect",
                "Projectile Proof",
                "Run one shot in Play mode from the chosen source, verify it spawns or traces and resolves impact, miss, or expiry, then add more projectile routes.",
                "One firing source plus one ProjectileLauncher2D, ProjectileLauncher3D, or project-owned launcher adapter connected to a ProjectileDefinition, ProjectileImpactDefinition, and FireModeDefinition.",
                "One shot spawns or traces from the expected source, travels or resolves immediately, then produces an impact, expiry, or miss result.",
                "Defer ammo economies, weapon inventories, bullet patterns, upgrades, score rewards, and networking until one shot resolves.",
                "First Unity focus: decide the firing source, then wire the matching projectile launcher and projectile/fire-mode definitions.",
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildCombatProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            return new PyralisAuthoringRouteProof(
                "proof.npc-enemy-behavior",
                "Combat Proof",
                "Run one attack in Play mode and confirm one visible hit, block, damage, or reaction path before building a larger combat loop.",
                "One attacker, one target, one hitbox/hurtbox path, one CombatActionDefinition or CombatSequenceDefinition, and one visible reaction.",
                "One authored attack causes one target to take damage, react, block, or otherwise report the expected combat outcome.",
                "Defer combo lists, enemy waves, status stacks, VFX polish, balance, scoring, and networking until one hit path works.",
                "First Unity focus: create one combat action and one runtime surface that can execute it.",
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildScoringProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            return new PyralisAuthoringRouteProof(
                "proof.ui-hud-menu",
                "Scoring Proof",
                "Run one score/objective event in Play mode, confirm the service value changes, then connect only the first HUD label that proves the route output.",
                "One score or objective service plus one visible or inspectable output that changes when the proof action happens.",
                "One gameplay event changes score, objective, timer, resource, or result state and the developer can see or inspect the change.",
                "Defer leaderboards, results screens, persistence, achievements, economy, and broad HUD polish until one score/objective change works.",
                "First Unity focus: add the score service, prove score changes, then connect HUD labels.",
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildCameraProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            return new PyralisAuthoringRouteProof(
                "proof.camera-cursor-world",
                "Camera/Cursor Proof",
                "Run one cursor, selector, or follow-target interaction in Play mode, verify camera/cursor selection responds, then add broader selection logic.",
                "One Cinemachine-backed camera, cursor, bounds, or selection-control surface that visibly responds to route input. Use CinemachineCameraRigController with CameraRigProfile for shared/split camera and 2D visible bounds.",
                "One input or selected target changes camera, cursor, framing, bounds, or highlighted surface as expected.",
                "Defer multi-target cameras, split-screen polish, shake polish, cinematic transitions, and full action targeting until one camera/cursor control proof works.",
                "First Unity focus: create Camera Root or cursor/select surface and connect it to camera/input profiles.",
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildProceduralProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            return new PyralisAuthoringRouteProof(
                "proof.generated-content",
                "Generated Content Proof",
                "Generate one output and inspect it in the scene or logs before making generated content required for progression.",
                "One generator output that can be inspected in the scene or logged as a deterministic result, with authored chunks, sockets, seeds, spawn budgets, or board layouts visible enough to debug.",
                "One generated output is deterministic enough to inspect and does not block the route if generation is disabled.",
                "Defer full biome sets, unreachable-room validation, rewards, difficulty curves, and runtime dependency on generation until one generated output is inspectable.",
                "First Unity focus: author chunks, sockets, seeds, spawn budgets, and validation first. Keep generated content inspectable before it becomes required game flow.",
                proofChain);
        }

        private static PyralisAuthoringRouteProof BuildNetworkProof(PyralisAuthoringRouteDescriptor route, PyralisAuthoringProofStep[] proofChain)
        {
            return new PyralisAuthoringRouteProof(
                "proof.network-ownership",
                "Network Ownership Proof",
                "Confirm the local route first, then start a host/client scene and verify the participant owns the expected pawn or control surface.",
                "A locally working route plus NetworkManager, UnityTransport, network prefab registration, NetworkObject setup, and authority metadata for the objects that must replicate.",
                "Host/client can connect, the owned participant controls the expected surface, and replicated state changes are visible without breaking the local proof.",
                "Defer prediction, rollback, matchmaking, backend persistence, and broad replication until one ownership path works.",
                "First Unity focus: prove the local route, then add explicit ownership, authority, NetworkObject, transport, and Network Prefabs setup.",
                proofChain);
        }

        private static PyralisAuthoringProofStep[] BuildProofChain(PyralisAuthoringRouteDescriptor route)
        {
            if (route == null || route.RouteFacts.Length == 0)
                return System.Array.Empty<PyralisAuthoringProofStep>();

            System.Collections.Generic.List<PyralisAuthoringProofStep> steps = new System.Collections.Generic.List<PyralisAuthoringProofStep>();
            for (int i = 0; i < route.RouteFacts.Length; i++)
            {
                PyralisAuthoringRouteFact fact = route.RouteFacts[i];
                if (fact == null || !fact.PrimaryProofCandidate)
                    continue;

                steps.Add(CreateProofStep(fact.Capability));
            }

            return steps.ToArray();
        }

        private static PyralisAuthoringProofStep CreateProofStep(PyralisAuthoringRouteCapability capability)
        {
            RuntimeCapabilityCard card = FindProofStepCard(capability);
            if (card != null)
                return new PyralisAuthoringProofStep(capability, card.ProofStepLabel, card.ProofStepSuccessCriteria);

            switch (capability)
            {
                case PyralisAuthoringRouteCapability.Tabletop:
                    return new PyralisAuthoringProofStep(capability, "Board/card action", "One board, card, or seat command resolves through rules.");
                case PyralisAuthoringRouteCapability.Projectile:
                    return new PyralisAuthoringProofStep(capability, "Projectile resolution", "One shot spawns or traces and resolves impact, miss, or expiry.");
                case PyralisAuthoringRouteCapability.Procedural:
                    return new PyralisAuthoringProofStep(capability, "Generated output", "One generated result is visible, deterministic enough to inspect, or logged clearly.");
                case PyralisAuthoringRouteCapability.Networking:
                    return new PyralisAuthoringProofStep(capability, "Network ownership", "After local proof, host/client ownership controls the expected participant surface.");
                default:
                    return new PyralisAuthoringProofStep(capability, "Route support", "The selected capability has inspectable setup evidence.");
            }
        }

        private static RuntimeCapabilityCard FindProofStepCard(PyralisAuthoringRouteCapability capability)
        {
            IReadOnlyList<RuntimeCapabilityCard> cards = PyralisRuntimeCapabilityCatalog.All;
            for (int i = 0; i < cards.Count; i++)
            {
                RuntimeCapabilityCard card = cards[i];
                if (card.RouteCapability == capability && card.PrimaryProofCandidate)
                    return card;
            }

            return null;
        }

        private static string BuildProofChainSummary(PyralisAuthoringProofStep[] proofChain)
        {
            if (proofChain == null || proofChain.Length == 0)
                return "No proof chain until the route has valid capability facts.";

            string[] labels = new string[proofChain.Length];
            for (int i = 0; i < proofChain.Length; i++)
                labels[i] = proofChain[i].Label;

            return string.Join(" -> ", labels);
        }
    }
}
