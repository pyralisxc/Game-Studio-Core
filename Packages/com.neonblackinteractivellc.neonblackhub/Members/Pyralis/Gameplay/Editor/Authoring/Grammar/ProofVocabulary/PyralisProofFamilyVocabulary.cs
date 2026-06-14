using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisProofFamilyVocabulary
    {
        public static System.Collections.Generic.IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            return PyralisContractProofFactProjector.EnrichProofTemplateFacts(GetDefaultProofTemplates());
        }

        public static System.Collections.Generic.IReadOnlyList<PyralisAuthoringFact> GetDefaultProofTemplates()
        {
            System.Collections.Generic.List<PyralisAuthoringFact> facts = new System.Collections.Generic.List<PyralisAuthoringFact>();
            facts.Add(CreateFallbackProofFact(
                "proof.1p-pawn-movement",
                "1P Pawn Movement Proof",
                "Run one local pawn-backed movement proof before adding combat, HUD, enemies, scoring, or networking.",
                "Generic pawn-backed route proof.",
                "One participant spawns one pawn, the selected InputProfile reaches a pawn input module, movement is visibly responsive, and the Game view follows the runtime shared camera focus.",
                new[] { "Movement" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "combat", "projectiles", "HUD", "scoring", "pickups", "hazards", "networking", "local join" },
                "the active 2D pawn route",
                "press Play after the selected intent's Do Now setup is clear; move the pawn through the mapped input path and watch the Game view",
                "one pawn spawns at the assigned spawn point, visibly moves, and the Game view follows the runtime GameplaySharedCameraFocus driven by that pawn",
                "The 1P pawn proof is the foundation of the player experience. Ensure movement feels responsive before adding more complexity.",
                string.Empty,
                new[]
                {
                    "route.pawn-actor",
                    "capability.2d-pawn-movement",
                    "setup.assign-session-definition",
                    "setup.assign-default-game-mode",
                    "setup.resolve-route-capabilities",
                    "setup.assign-default-participants",
                    "setup.assign-participant-pawn",
                    "setup.assign-input-profile",
                    "setup.assign-spawn-points",
                    "setup.assign-camera-rig",
                    "setup.tune-pawn-visuals-and-collision",
                    "setup.tune-movement-and-input-feel"
                }));

            facts.Add(CreateFallbackProofFact(
                "proof.board-card-action",
                "Board Card Action Proof",
                "Run one rules-backed tabletop selection before adding card UX, AI turns, campaign flow, or networking.",
                "Generic no-pawn tabletop route proof.",
                "One board space, card, seat command, or turn action is selected; Pyralis accepts or rejects it through rules; and board/card/turn state visibly or inspectably changes.",
                new[] { "Tabletop", "Interaction" },
                new[] { RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.CameraCursor },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                new[] { "pawn actors", "final board art", "full card UX", "AI turns", "shops", "deckbuilding", "networking", "campaign flow" },
                "the active tabletop route",
                "press Play after one board/card/seat command surface is wired; choose one legal or illegal action",
                "one selection reaches rules and visibly or inspectably changes board, card, turn, score, or UI state",
                "Tabletop proofs verify that your rules engine is correctly processing player choices. Use deterministic board state for testing.",
                string.Empty,
                new[] { "route.tabletop-card", "capability.interaction-action-selection", "capability.ui-scoring-feedback" }));

            facts.Add(CreateFallbackProofFact(
                "proof.action-selection",
                "Action Selection Proof",
                "Run one selected command before expanding menus, cards, ability lists, animation polish, or AI.",
                "Generic action, menu, or cursor route proof.",
                "One command reaches its resolver and reports accepted, rejected, completed, or failed.",
                new[] { "Interaction", "Tabletop" },
                new[] { RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.CameraCursor, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "large menus", "card rewards", "ability trees", "animation polish", "AI", "scoring", "networking" },
                "the active action route",
                "press Play and select one command from its authored surface",
                "the resolver receives the command and reports a clear accepted, rejected, completed, or failed result",
                "Action selection is the core of player agency. Ensure that the player always knows why an action was accepted or rejected.",
                string.Empty,
                new[] { "route.custom-object-feature", "route.ui-hud-menu", "route.tabletop-card", "capability.interaction-action-selection" }));

            facts.Add(CreateFallbackProofFact(
                "proof.npc-enemy-behavior",
                "NPC Enemy Behavior Proof",
                "Run one NPC or enemy behavior proof before building encounter waves, boss phases, vendors, or broad AI systems.",
                "Generic NPC or enemy actor route proof.",
                "One NPC or enemy appears, is detected or interacted with, and performs one authored behavior or combat reaction.",
                new[] { "NpcsEnemies", "Combat", "Interaction" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                new[] { RuntimeCapabilityLaneTag.UiMenuOnly },
                new[] { "waves", "boss phases", "shops", "quest chains", "loot tables", "network replication", "full AI debugging" },
                "the active NPC/enemy route",
                "press Play after one NPC/enemy surface is wired; trigger one interaction, detection, or attack",
                "one authored NPC/enemy behavior is visible or inspectable without requiring a complete encounter loop",
                "NPC behavior proofs should focus on the individual actor's logic. Group behaviors like wave spawning should be tested later.",
                string.Empty,
                new[] { "route.npc-enemy-actor", "capability.combat-projectile-proof", "capability.interaction-action-selection" }));

            facts.Add(CreateFallbackProofFact(
                "proof.custom-object-effect",
                "Custom Object Effect Proof",
                "Run one custom object, feature, trigger, pickup, hazard, turret, trap, or service effect before treating it as a full system.",
                "Generic custom object or feature route proof.",
                "One authored scene object or feature produces a visible accepted, rejected, completed, damaged, collected, triggered, or scored effect.",
                new[] { "Interaction", "Combat", "Scoring" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "secondary variants", "spawn tables", "economy", "large VFX", "network replication", "shipping automation" },
                "the active custom object route",
                "press Play and trigger one authored object or feature",
                "one object produces a visible or inspectable gameplay effect",
                "Custom objects are often specific to one level or mechanic. Use Proof scenes to iterate on their visual and logical feel.",
                string.Empty,
                new[] { "route.custom-object-feature", "capability.interaction-action-selection", "capability.combat-projectile-proof", "capability.ui-scoring-feedback" }));

            facts.Add(CreateFallbackProofFact(
                "proof.ui-hud-menu",
                "UI HUD Menu Proof",
                "Run one UI, HUD, prompt, score, health, feedback, or menu event before building full navigation or result screens.",
                "Generic UI, HUD, menu, or feedback route proof.",
                "One UI event changes visible state or sends one command to a resolver.",
                new[] { "UiHud", "Scoring", "Interaction" },
                new[] { RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "full menu navigation", "save slots", "results screens", "leaderboards", "achievements", "localization polish" },
                "the active UI/HUD/menu route",
                "press Play and trigger one UI, HUD, prompt, score, health, feedback, or menu event",
                "one visible label, panel, score value, prompt, health display, feedback message, or command result changes",
                "UI proofs ensure that the data-flow between the engine and the Canvas is healthy. Use Placeholder art during this stage.",
                string.Empty,
                new[] { "route.ui-hud-menu", "capability.ui-scoring-feedback", "capability.interaction-action-selection" }));

            facts.Add(CreateFallbackProofFact(
                "proof.camera-cursor-world",
                "Camera Cursor World Proof",
                "Run one camera, cursor, bounds, or world-surface proof before adding multi-target framing or cinematic polish.",
                "Generic world, camera, cursor, or bounds route proof.",
                "One input or selected target changes camera, cursor, framing, bounds, highlighted surface, or scene visibility as authored.",
                new[] { "Camera", "Movement", "Interaction" },
                new[] { RuntimeCapabilityLaneTag.CameraCursor, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "multi-target cameras", "split-screen polish", "camera shake", "cinematic transitions", "full action targeting" },
                "the active camera/cursor/world route",
                "press Play and move/select the authored camera, cursor, target, bounds, or world surface",
                "visibility, framing, bounds, cursor, highlight, or scene response changes as authored",
                "Camera and cursor proofs are vital for navigation-heavy routes. Ensure your raycast masks are correctly configured.",
                string.Empty,
                new[] { "route.world-camera", "capability.camera-follow-bounds", "setup.assign-camera-rig", "setup.assign-camera-bounds-service" }));

            facts.Add(CreateFallbackProofFact(
                "proof.generated-content",
                "Generated Content Proof",
                "Generate one inspectable output before making generated content required for progression.",
                "Generic procedural or generated-content route proof.",
                "One generated output is deterministic enough to inspect in the scene or logs and does not block the route if generation is disabled.",
                new[] { "Interaction", "Scoring" },
                new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.CameraCursor },
                System.Array.Empty<RuntimeCapabilityLaneTag>(),
                new[] { "full biomes", "unreachable-room validation", "rewards", "difficulty curves", "runtime dependency on generation" },
                "the active generated-content route",
                "press Play or run the route-owned generation action and inspect the output root or log",
                "one generated result is visible, deterministic enough to inspect, or logged clearly",
                "Generated content should always be 'debuggable'. Ensure you can manually set a Seed to reproduce specific issues.",
                string.Empty,
                new[] { "route.custom-object-feature", "route.world-camera" }));

            facts.Add(CreateFallbackProofFact(
                "proof.network-ownership",
                "Network Ownership Proof",
                "Confirm the local proof first, then prove one host/client ownership path before expanding replication.",
                "Generic network ownership or authority route proof.",
                "Host/client can connect, the owned participant controls the expected surface, and one replicated state change is visible without breaking the local proof.",
                new[] { "Networking", "Movement", "Combat" },
                new[] { RuntimeCapabilityLaneTag.Mixed, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                new[] { RuntimeCapabilityLaneTag.TabletopBoard },
                new[] { "prediction", "rollback", "matchmaking", "backend persistence", "broad replication", "Steam lobby integration" },
                "the active networking route",
                "after the local proof passes, start host/client and exercise the owned participant surface",
                "ownership, spawn, input authority, and one replicated state change behave as authored",
                "Networking is easiest to debug when local-first. Always ensure the local proof is rock-solid before testing over transport.",
                string.Empty,
                new[] { "route.networking", "capability.2d-pawn-movement", "capability.combat-projectile-proof" }));

            return facts;
        }

        public static string GetFallbackProofTargetId(RuntimeCapabilityFamily[] families, bool requiresPawn)
        {
            if (requiresPawn)
                return "proof.1p-pawn-movement";

            if (ContainsFamily(families, RuntimeCapabilityFamily.BoardCardTabletop))
                return "proof.board-card-action";
            if (ContainsFamily(families, RuntimeCapabilityFamily.ActionTargeting))
                return "proof.action-selection";
            if (ContainsFamily(families, RuntimeCapabilityFamily.GunsProjectiles))
                return "proof.custom-object-effect";
            if (ContainsFamily(families, RuntimeCapabilityFamily.Combat))
                return "proof.npc-enemy-behavior";
            if (ContainsFamily(families, RuntimeCapabilityFamily.ScoringObjectives))
                return "proof.ui-hud-menu";
            if (ContainsFamily(families, RuntimeCapabilityFamily.CameraInput))
                return "proof.camera-cursor-world";
            if (ContainsFamily(families, RuntimeCapabilityFamily.ProceduralGeneration))
                return "proof.generated-content";
            if (ContainsFamily(families, RuntimeCapabilityFamily.Networking))
                return "proof.network-ownership";

            return string.Empty;
        }

        private static bool ContainsFamily(RuntimeCapabilityFamily[] families, RuntimeCapabilityFamily family)
        {
            if (families == null)
                return false;

            for (int i = 0; i < families.Length; i++)
            {
                if (families[i] == family)
                    return true;
            }

            return false;
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

        private static PyralisAuthoringFact CreateFallbackProofFact(
            string stableId,
            string displayName,
            string summary,
            string routeRelevance,
            string firstProof,
            string[] goalTags,
            RuntimeCapabilityLaneTag[] laneTags,
            RuntimeCapabilityLaneTag[] unsupportedLaneTags,
            string[] canWait,
            string nativeActionTarget,
            string nativeActionInstructions,
            string nativeActionSuccess,
            string expertAdvice,
            string documentationURL,
            string[] relatedStableIds)
        {
            return CreateProofFact(
                stableId,
                displayName,
                summary,
                routeRelevance,
                firstProof,
                goalTags,
                laneTags,
                unsupportedLaneTags,
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                canWait,
                nativeActionTarget,
                nativeActionInstructions,
                nativeActionSuccess,
                expertAdvice,
                documentationURL,
                relatedStableIds);
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

    }
}
