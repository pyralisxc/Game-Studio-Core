using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisSceneSurfaceEvidenceFacts
    {
        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            return new[]
            {
                CreateSceneSurfaceFact(
                    "scene-evidence.environment-playfield",
                    PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield,
                    "World, board, arena, backdrop, collider, bounds, zone, spawn, or selectable playfield evidence.",
                    new[] { "Movement", "Tabletop", "Interaction" },
                    new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                    new[] { "environment root", "playfield root", "collision surface", "board root", "zone", "spawn point", "generated output root" },
                    new[] { "route.pawn-actor", "route.tabletop-card", "route.world-camera", "proof.1p-pawn-movement", "proof.board-card-action", "proof.generated-content" }),

                CreateSceneSurfaceFact(
                    "scene-evidence.camera-bounds",
                    PyralisAuthoringSceneSurfaceGuidance.CameraBounds,
                    "Camera root, Cinemachine controller, physical target camera, camera profile, or bounds evidence.",
                    new[] { "Camera", "Movement", "Interaction" },
                    new[] { RuntimeCapabilityLaneTag.CameraCursor, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                    new[] { "camera root", "camera controller", "camera profile", "playfield profile", "physical camera", "camera bounds source" },
                    new[] { "route.world-camera", "capability.camera-follow-bounds", "proof.camera-cursor-world", "inspector.cinemachine-camera-rig-controller.camera-fields" }),

                CreateSceneSurfaceFact(
                    "scene-evidence.ui-hud-menus",
                    PyralisAuthoringSceneSurfaceGuidance.UiHudMenus,
                    "Canvas, EventSystem, HUD presenter, menu presenter, prompt, card hand, action buttons, or score/feedback panel evidence.",
                    new[] { "UiHud", "Scoring", "Interaction" },
                    new[] { RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                    new[] { "canvas", "event system", "HUD presenter", "menu presenter", "board UI", "action buttons", "feedback panel" },
                    new[] { "route.ui-hud-menu", "proof.ui-hud-menu", "capability.ui-scoring-feedback", "reflection.add-component-menu.participant-feedback-hud-presenter" }),

                CreateSceneSurfaceFact(
                    "scene-evidence.scoring-objectives",
                    PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives,
                    "Score, objective, timer, resource, result, win/loss service, or visible output evidence.",
                    new[] { "Scoring", "UiHud" },
                    new[] { RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                    new[] { "score service", "objective service", "timer/resource/result service", "score HUD label" },
                    new[] { "route.ui-hud-menu", "proof.ui-hud-menu", "capability.ui-scoring-feedback" }),

                CreateSceneSurfaceFact(
                    "scene-evidence.board-action-selection",
                    PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection,
                    "Board grid presenter, selection bridge, card hand, action/menu presenter, UI button, cursor bridge, or collider/raycast target evidence.",
                    new[] { "Tabletop", "Interaction" },
                    new[] { RuntimeCapabilityLaneTag.TabletopBoard, RuntimeCapabilityLaneTag.UiMenuOnly, RuntimeCapabilityLaneTag.CameraCursor, RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.ThirdPerson3D },
                    new[] { "board presenter", "selection bridge", "UI button", "cursor bridge", "card hand", "collider or raycast target" },
                    new[] { "route.tabletop-card", "route.custom-object-feature", "proof.board-card-action", "proof.action-selection", "capability.interaction-action-selection" }),

                CreateSceneSurfaceFact(
                    "scene-evidence.pickups-hazards-enemies",
                    PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies,
                    "Pickup, hazard, enemy, encounter, arena, spawner, or custom feature object evidence.",
                    new[] { "NpcsEnemies", "Combat", "Interaction" },
                    new[] { RuntimeCapabilityLaneTag.Sprite2D, RuntimeCapabilityLaneTag.Billboard2_5D, RuntimeCapabilityLaneTag.ThirdPerson3D, RuntimeCapabilityLaneTag.TabletopBoard },
                    new[] { "pickup spawner", "hazard zone", "enemy actor", "enemy spawner", "arena zone", "encounter anchor", "custom feature object" },
                    new[] { "route.npc-enemy-actor", "route.custom-object-feature", "proof.npc-enemy-behavior", "proof.custom-object-effect", "reflection.add-component-menu.enemy-spawner", "reflection.add-component-menu.damage-zone-2d" })
            };
        }

        private static PyralisAuthoringFact CreateSceneSurfaceFact(
            string stableId,
            string surface,
            string summary,
            string[] goalTags,
            RuntimeCapabilityLaneTag[] laneTags,
            string[] sceneComponents,
            string[] relatedStableIds)
        {
            return new PyralisAuthoringFact(
                stableId,
                surface,
                PyralisAuthoringFactKind.SceneComponent,
                PyralisAuthoringFactSourceKind.SceneEvidence,
                PyralisAuthoringConfidence.Inferred,
                summary,
                "scene surface evidence",
                PyralisAuthoringSceneSurfaceGuidance.GetSuccess(surface),
                goalTags: goalTags,
                laneTags: ToStrings(laneTags),
                requiredSceneComponents: sceneComponents,
                nativeActions: new[]
                {
                    new PyralisAuthoringNativeAction(
                        "Inspect",
                        PyralisAuthoringActionSurface.Hierarchy,
                        surface,
                        PyralisAuthoringSceneSurfaceGuidance.GetExpected(surface),
                        PyralisAuthoringSceneSurfaceGuidance.GetSuccess(surface))
                },
                workIntent: "SceneEvidence",
                relatedStableIds: relatedStableIds);
        }

        private static string[] ToStrings(RuntimeCapabilityLaneTag[] tags)
        {
            string[] values = new string[tags.Length];
            for (int i = 0; i < tags.Length; i++)
                values[i] = tags[i].ToString();

            return values;
        }
    }
}
