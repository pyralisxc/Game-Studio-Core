using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisRouteCoverageFacts
    {
        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            return new[]
            {
                CreateRouteFact(
                    "route.pawn-actor",
                    "Pawn Actor Route",
                    "Participant-backed player, NPC, or simulated actor routes that spawn or control a pawn body.",
                    "This is the first route family to receive deep Authoring 2.0 coverage.",
                    new[] { "Movement", "Combat", "NpcsEnemies" },
                    new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                    new[] { RuntimeCapabilityLaneTag.TabletopBoard },
                    new[] { "SessionDefinition", "ParticipantDefinition", "PawnDefinition", "RuntimePatternDefinition" },
                    new[] { "InputProfile", "PawnMovementProfile", "PawnPresentationProfile" },
                    new[] { "GameplaySessionBootstrap", "PyralisGameplayLifetimeScope", "Spawn Point Transform" },
                    new[] { "PawnRoot", "Motor2D or Motor3D", "lane movement/presentation/input components" },
                    new[] { "SessionDefinition.defaultParticipants", "ParticipantDefinition.defaultPawn", "PawnDefinition.pawnPrefab" },
                    new[] { "Choose pawn art, collider fit, movement feel, presentation mode, and player/NPC ownership." },
                    "Enter Play Mode and confirm one authored pawn spawns, receives the expected control source, and visibly moves or acts.",
                    new[] { "capability.2d-pawn-movement", "capability.3d-pawn-movement", "proof.1p-pawn-movement", "setup.assign-participant-pawn", "setup.assign-spawn-points" }),

                CreateRouteFact(
                    "route.npc-enemy-actor",
                    "NPC And Enemy Actor Route",
                    "Pawn or actor routes driven by AI, encounter, ambient, dialogue, vendor, quest, or enemy combat definitions.",
                    "The package has NPC/enemy definitions and feature modules, but Authoring 2.0 still needs fuller guided setup and proof coverage.",
                    new[] { "NpcsEnemies", "Combat", "Interaction" },
                    new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                    new[] { RuntimeCapabilityLaneTag.UiMenuOnly },
                    new[] { "NpcDefinition", "ParticipantDefinition or enemy actor definition", "FeatureModuleDefinition" },
                    new[] { "EnemyFeatureProfile", "EnemyCombatProfile", "EnemyReactionProfile", "EnemyAmbientFeatureProfile" },
                    new[] { "Spawner, encounter zone, dialogue/vendor/quest presenter, or authored actor root" },
                    new[] { "EnemyAI or lane actor runtime", "HealthComponent when combat is expected" },
                    new[] { "FeatureModuleDefinition.profileAsset", "actor prefab -> enemy/NPC runtime components" },
                    new[] { "Choose AI role, faction/team, patrol/encounter surface, dialogue/vendor/quest content, and combat reaction style." },
                    "Enter Play Mode and confirm one NPC/enemy appears, can be detected or interacted with, and performs one authored behavior.",
                    new[] { "capability.npc-enemy-setup", "capability.combat-projectile-proof", "capability.interaction-action-selection" }),

                CreateRouteFact(
                    "route.custom-object-feature",
                    "Custom Object Or Feature Route",
                    "Scene objects, feature modules, hazards, pickups, triggers, turrets, traps, or custom systems that are not the primary pawn.",
                    "This route protects custom game objects from being treated as second-class setup after pawn paths work.",
                    new[] { "Interaction", "Combat", "Scoring" },
                    new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                    System.Array.Empty<RuntimeCapabilityLaneTag>(),
                    new[] { "FeatureModuleDefinition", "ActionDefinition or feature-specific definition when commands are involved" },
                    new[] { "PickupFeatureProfile", "HazardFeedbackProfile", "InteractionFeatureProfile, or feature-specific profile" },
                    new[] { "authored trigger, pickup, hazard, actor feature host, or feature runtime object" },
                    new[] { "feature runtime component for the selected lane" },
                    new[] { "FeatureModuleDefinition.runtimePrefab", "feature profile -> runtime component" },
                    new[] { "Choose whether the object is scenery, trigger, pickup, hazard, turret, trap, service, or custom action source." },
                    "Enter Play Mode and confirm the object produces one visible accepted/rejected/completed gameplay effect.",
                    new[] { "capability.interaction-action-selection", "capability.combat-projectile-proof", "capability.ui-scoring-feedback" }),

                CreateRouteFact(
                    "route.ui-hud-menu",
                    "UI HUD Or Menu Route",
                    "Canvas, HUD, menu, prompt, health, score, feedback, inventory, dialogue, card hand, or route-selection surfaces.",
                    "UI routes need authoring coverage because many games start from menus or non-pawn interaction surfaces.",
                    new[] { "UiHud", "Scoring", "Interaction" },
                    new[] { RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                    System.Array.Empty<RuntimeCapabilityLaneTag>(),
                    new[] { "ActionDefinition when UI triggers gameplay commands" },
                    new[] { "SettingsProfile when settings or rebinding are part of the route" },
                    new[] { "Canvas", "EventSystem", "HUD/menu presenter or feedback panel" },
                    System.Array.Empty<string>(),
                    new[] { "HUD presenter -> gameplay service", "UI button/menu/card -> action resolver or command surface" },
                    new[] { "Choose layout, labels, navigation order, feedback timing, accessibility, and whether UI is screen-space or world-space." },
                    "Enter Play Mode and confirm one UI event changes visible state or sends one command to a resolver.",
                    new[] { "capability.ui-scoring-feedback", "capability.interaction-action-selection" }),

                CreateRouteFact(
                    "route.world-camera",
                    "World Camera And Scene Surface Route",
                    "Camera, bounds, scene service, world trigger, board view, cursor view, or environmental route surfaces.",
                    "Camera/world coverage keeps setup honest when the first proof depends on visibility, framing, bounds, or scene context.",
                    new[] { "Camera", "Movement", "Interaction" },
                    new[] { RuntimeCapabilityLaneTag.CameraCursor, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                    System.Array.Empty<RuntimeCapabilityLaneTag>(),
                    System.Array.Empty<string>(),
                    new[] { "CameraRigProfile", "PlayfieldProfile" },
                    new[] { "Camera Root", "CinemachineCameraRigController", "physical Camera", "camera bounds source or world surface" },
                    System.Array.Empty<string>(),
                    new[] { "GameplaySessionBootstrap.cameraRigController", "CinemachineCameraRigController.cameraRigProfile", "camera bounds source" },
                    new[] { "Choose orthographic/perspective framing, follow target, bounds, split-screen behavior, and scene-service ownership." },
                    "Enter Play Mode and confirm the camera sees the route surface and bounds/framing behave as authored.",
                    new[] { "capability.camera-follow-bounds", "setup.assign-camera-bounds-service" }),

                CreateRouteFact(
                    "route.tabletop-card",
                    "Tabletop Board Card Route",
                    "Board seats, board pieces, card hands, faction surfaces, turn order, phases, terminal conditions, and board action selection.",
                    "This route is explicitly non-pawn capable and should not inherit pawn requirements by accident.",
                    new[] { "Tabletop", "Interaction", "Scoring" },
                    new[] { RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.CameraCursor },
                    new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                    new[] { "BoardDefinition", "BoardPieceDefinition", "BoardMovePolicyDefinition", "TurnOrderDefinition", "PhaseDefinition" },
                    System.Array.Empty<string>(),
                    new[] { "TabletopBoardGridPresenter or board/card selection surface" },
                    System.Array.Empty<string>(),
                    new[] { "GameModeDefinition.boardDefinition", "GameModeDefinition.turnOrderDefinition", "selected surface -> action resolver" },
                    new[] { "Choose board layout, legal moves, turn/phase order, card/seat ownership, and how pieces or cards are selected." },
                    "Enter Play Mode and confirm one board/card/menu selection reaches a resolver and updates board, turn, score, or UI state.",
                    new[] { "capability.interaction-action-selection", "capability.ui-scoring-feedback" }),

                CreateRouteFact(
                    "route.networking",
                    "Networking Authority Route",
                    "Host/client/server, participant ownership, authority, network spawn, replicated state, and network-ready route proof.",
                    "Networking must stay explicit so local-first setup does not silently pretend it has network authority coverage.",
                    new[] { "Networking", "Movement", "Combat" },
                    new[] { RuntimeCapabilityLaneTag.Mixed, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                    new[] { RuntimeCapabilityLaneTag.TabletopBoard },
                    new[] { "SessionDefinition" },
                    System.Array.Empty<string>(),
                    new[] { "GameplaySessionBootstrap", "PyralisGameplayLifetimeScope", "network session services when network mode is selected" },
                    new[] { "network-aware pawn or participant runtime components when the selected lane supports them" },
                    new[] { "SessionDefinition.networkMode", "SessionDefinition.localFirst", "networked ownership/authority service configuration" },
                    new[] { "Choose local-only, host, client, or server authority and decide which participant actions replicate." },
                    "Enter Play Mode or a network test and confirm ownership, spawn, input authority, and one replicated state change behave as authored.",
                    new[] { "proof.network-ownership", "capability.2d-pawn-movement", "capability.3d-pawn-movement", "capability.combat-projectile-proof" })
            };
        }

        private static PyralisAuthoringFact CreateRouteFact(
            string stableId,
            string displayName,
            string summary,
            string routeRelevance,
            string[] goalTags,
            RuntimeCapabilityLaneTag[] laneTags,
            RuntimeCapabilityLaneTag[] unsupportedLaneTags,
            string[] requiredDefinitions,
            string[] requiredProfiles,
            string[] requiredSceneComponents,
            string[] requiredPrefabComponents,
            string[] assignmentFields,
            string[] customizationMoments,
            string firstProof,
            string[] relatedStableIds)
        {
            return new PyralisAuthoringFact(
                stableId,
                displayName,
                PyralisAuthoringFactKind.RouteFamily,
                PyralisAuthoringFactSourceKind.HandAuthoredGuideCard,
                PyralisAuthoringConfidence.Explicit,
                summary,
                routeRelevance,
                firstProof,
                goalTags: goalTags,
                laneTags: ToStrings(laneTags),
                unsupportedLaneTags: ToStrings(unsupportedLaneTags),
                requiredDefinitions: requiredDefinitions,
                requiredProfiles: requiredProfiles,
                requiredSceneComponents: requiredSceneComponents,
                requiredPrefabComponents: requiredPrefabComponents,
                assignmentFields: assignmentFields,
                customizationMoments: customizationMoments,
                canWait: new[] { "route-specific polish", "secondary modes", "advanced validation", "shipping automation" },
                nativeActions: new[]
                {
                    new PyralisAuthoringNativeAction(
                        "Choose Route",
                        PyralisAuthoringActionSurface.AuthoringWindow,
                        displayName,
                        "Runtime patterns and setup facts",
                        "the selected setup has one named route family to prove first"),
                    new PyralisAuthoringNativeAction(
                        "Test",
                        PyralisAuthoringActionSurface.PlayMode,
                        displayName,
                        firstProof,
                        "one route-family proof is observed before expanding scope")
                },
                workIntent: "RouteCoverage",
                relatedStableIds: relatedStableIds);
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
