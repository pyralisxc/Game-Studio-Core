using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Editor
{
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
            return PyralisProofFamilyVocabulary.GetAuthoringFacts();
        }

        public static System.Collections.Generic.IReadOnlyList<PyralisAuthoringFact> GetFallbackAuthoringFacts()
        {
            return PyralisProofFamilyVocabulary.GetDefaultProofTemplates();
        }

        public static PyralisAuthoringFact FindProofFact(string stableId)
        {
            return PyralisProofFamilyVocabulary.FindProofFact(stableId);
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
            switch (capability)
            {
                case PyralisAuthoringRouteCapability.PawnAction:
                    return new PyralisAuthoringProofStep(capability, "Local pawn movement", "One participant spawns one pawn and movement input visibly moves it.");
                case PyralisAuthoringRouteCapability.CameraCursor:
                    return new PyralisAuthoringProofStep(capability, "Camera/cursor response", "One input or target changes camera, cursor, selection, framing, or bounds.");
                case PyralisAuthoringRouteCapability.ActionSelection:
                    return new PyralisAuthoringProofStep(capability, "Action resolver", "One selected command reaches its resolver and reports accepted, rejected, completed, or failed.");
                case PyralisAuthoringRouteCapability.Combat:
                    return new PyralisAuthoringProofStep(capability, "Combat reaction", "One attack produces one hit, block, damage, or reaction outcome.");
                case PyralisAuthoringRouteCapability.Scoring:
                    return new PyralisAuthoringProofStep(capability, "Score/objective change", "One gameplay event changes score, objective, timer, resource, or result state.");
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
