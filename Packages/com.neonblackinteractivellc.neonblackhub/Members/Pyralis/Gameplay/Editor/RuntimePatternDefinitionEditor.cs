using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(RuntimePatternDefinition))]
    public class RuntimePatternDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RuntimePatternDefinition pattern = (RuntimePatternDefinition)target;

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Runtime Pattern",
                new PyralisGuideSection(
                    "What This Is",
                    "A RuntimePatternDefinition describes one setup capability, such as realtime character control, projectile combat, board/card/tabletop play, camera/cursor control, scoring, animation, or procedural generation.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("AUTHORING_MODEL.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Fill Pattern Id with a stable lowercase id such as pattern.projectile-combat.",
                        "Fill Display Name with a readable name for designers.",
                        "Choose Capability Family so setup profiles can summarize the path.",
                        "Choose Participant Embodiment to say whether this path requires a pawn, allows one, or needs a non-pawn surface.",
                        "Assign Supported Control Surfaces so the Inspector can explain valid player-control routes.",
                        "Assign Presentation Lanes and First Proof Requirements so Authoring can validate the route without guessing from text."
                    },
                    PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")),
                new PyralisGuideSection(
                    "Route Fit",
                    GetEmbodimentHelp(pattern),
                    new[]
                    {
                        "Use Required Pawn for character-controller-first patterns.",
                        "Use Optional Pawn when the same capability can come from a pawn, enemy, turret, card, trap, board piece, camera, cursor, or AI.",
                        "Use Non-Pawn Surface Required for board, card, menu, faction, camera-only, or cursor-only patterns.",
                        "Use First Proof Requirements for structural needs such as spawn points, camera rig, Sprite2D bounds, local join, HUD, score service, enemy spawner, or tabletop contract.",
                        "Use recommended companion patterns to teach common overlap without making them required."
                    }),
                new PyralisGuideSection(
                    "Unity Wiring",
                    null,
                    new[]
                    {
                        GetSuggestedSetupNotes(pattern),
                        "After this pattern is ready, add it to a GameSetupProfile and assign that setup profile to a GameModeDefinition.",
                        "Let the GameSetupProfile explain the combined setup route when multiple patterns overlap."
                    },
                    PyralisInspectorGuide.SetupManualPath("CANONICAL_SETUP.md")),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not mark pawn as required just because a sample uses a pawn. Only require pawns when this capability cannot run without one.",
                        "Do not leave Description and Setup Notes empty; they are route-facing instructions other inspectors surface later.",
                        "Do not use cautionary companion patterns for normal overlap. Use them only for combinations that need special care."
                    },
                    PyralisInspectorGuide.SetupManualPath("MANUAL.md")));

            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            DrawGuidedSetup(pattern);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Setup Readiness", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Capability Family", pattern.capabilityFamily.ToString());
            EditorGUILayout.LabelField("Participant Embodiment", pattern.participantEmbodiment.ToString());
            EditorGUILayout.LabelField("Supports Pawn", pattern.SupportsControlSurface(RuntimeControlSurface.Pawn) ? "Yes" : "No");
            EditorGUILayout.LabelField("Supports Non-Pawn Surface", SupportsNonPawnSurface(pattern) ? "Yes" : "No");
            EditorGUILayout.LabelField("First Proof Requirements", pattern.firstProofRequirements.ToString());

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Supported Control Surfaces", EditorStyles.boldLabel);
            DrawControlSurfaces(pattern);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Presentation / Runtime Lanes", EditorStyles.boldLabel);
            DrawPresentationLanes(pattern);
            DrawPresentationLaneChecklist(pattern);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("First Proof Evidence", EditorStyles.boldLabel);
            DrawFirstProofRequirementChecklist(pattern);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Companion Patterns", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Recommended", CountAssigned(pattern.recommendedCompanionPatterns).ToString());
            EditorGUILayout.LabelField("Cautionary", CountAssigned(pattern.cautionaryCompanionPatterns).ToString());

            List<string> issues = pattern.GetValidationIssues();
            DrawIssues(issues);

            if (serializedObject.ApplyModifiedProperties())
            {
                pattern.Sanitize();
                EditorUtility.SetDirty(pattern);
            }
        }

        private static void DrawGuidedSetup(RuntimePatternDefinition pattern)
        {
            EditorGUILayout.LabelField("Pattern Authoring Text", EditorStyles.boldLabel);

            string description = !string.IsNullOrWhiteSpace(pattern.description)
                ? pattern.description
                : RuntimePatternAuthoringText.GetSuggestedDescription(pattern);

            string setupNotes = !string.IsNullOrWhiteSpace(pattern.setupNotes)
                ? pattern.setupNotes
                : RuntimePatternAuthoringText.GetSuggestedSetupNotes(pattern);

            EditorGUILayout.HelpBox(description, MessageType.Info);

            if (!string.IsNullOrWhiteSpace(setupNotes))
                EditorGUILayout.HelpBox("Setup notes:\n" + setupNotes, MessageType.None);

            EditorGUILayout.HelpBox(GetEmbodimentHelp(pattern), MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                bool hasMissingText = string.IsNullOrWhiteSpace(pattern.description) || string.IsNullOrWhiteSpace(pattern.setupNotes);
                using (new EditorGUI.DisabledScope(!hasMissingText))
                {
                    if (GUILayout.Button(new GUIContent("Fill Missing Text From Fields", "Fills only empty Description and Setup Notes from the selected capability, embodiment, control surface, and lane fields. It does not choose a route or assign proof requirements.")))
                        FillMissingGuidanceText(pattern);
                }

                if (GUILayout.Button("Copy Guidance"))
                {
                    EditorGUIUtility.systemCopyBuffer = description + "\n\nSetup notes:\n" + setupNotes;
                    Debug.Log($"Copied runtime pattern guidance for '{GetPatternLabel(pattern)}'.", pattern);
                }
            }
        }

        private static void FillMissingGuidanceText(RuntimePatternDefinition pattern)
        {
            Undo.RecordObject(pattern, "Fill Runtime Pattern Guidance Text");

            if (string.IsNullOrWhiteSpace(pattern.description))
                pattern.description = RuntimePatternAuthoringText.GetSuggestedDescription(pattern);

            if (string.IsNullOrWhiteSpace(pattern.setupNotes))
                pattern.setupNotes = RuntimePatternAuthoringText.GetSuggestedSetupNotes(pattern);

            pattern.Sanitize();
            EditorUtility.SetDirty(pattern);
        }

        private static void DrawControlSurfaces(RuntimePatternDefinition pattern)
        {
            if (pattern.supportedControlSurfaces == null || pattern.supportedControlSurfaces.Length == 0)
            {
                EditorGUILayout.HelpBox("No supported control surfaces assigned.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < pattern.supportedControlSurfaces.Length; i++)
                EditorGUILayout.LabelField("-", pattern.supportedControlSurfaces[i].ToString());
        }

        private static void DrawPresentationLanes(RuntimePatternDefinition pattern)
        {
            if (pattern.presentationLanes == null || pattern.presentationLanes.Length == 0)
            {
                EditorGUILayout.HelpBox("No presentation/runtime lanes assigned.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < pattern.presentationLanes.Length; i++)
                EditorGUILayout.LabelField("-", pattern.presentationLanes[i].ToString());
        }

        private static void DrawPresentationLaneChecklist(RuntimePatternDefinition pattern)
        {
            EditorGUILayout.HelpBox("Manually choose the lanes this pattern can honestly support. Use Any only when the route is genuinely lane-agnostic.", MessageType.None);

            RuntimePatternPresentationLane[] lanes =
            {
                RuntimePatternPresentationLane.Any,
                RuntimePatternPresentationLane.Sprite2D,
                RuntimePatternPresentationLane.Billboard2_5D,
                RuntimePatternPresentationLane.Rigged3D,
                RuntimePatternPresentationLane.TabletopNoPawn,
                RuntimePatternPresentationLane.UiMenu,
                RuntimePatternPresentationLane.CameraCursor,
                RuntimePatternPresentationLane.Networked
            };

            EditorGUI.BeginChangeCheck();
            List<RuntimePatternPresentationLane> selected = new List<RuntimePatternPresentationLane>();
            for (int i = 0; i < lanes.Length; i++)
            {
                RuntimePatternPresentationLane lane = lanes[i];
                bool enabled = ContainsLane(pattern.presentationLanes, lane);
                bool nextEnabled = EditorGUILayout.ToggleLeft(GetLaneLabel(lane), enabled);
                if (nextEnabled)
                    selected.Add(lane);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(pattern, "Edit Runtime Pattern Presentation Lanes");
                pattern.presentationLanes = selected.Count == 0
                    ? new[] { RuntimePatternPresentationLane.Any }
                    : selected.ToArray();
                pattern.Sanitize();
                EditorUtility.SetDirty(pattern);
            }
        }

        private static void DrawFirstProofRequirementChecklist(RuntimePatternDefinition pattern)
        {
            EditorGUILayout.HelpBox("Check the concrete scene or prefab evidence the first proof must show. These are validation expectations, not generated content.", MessageType.None);

            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.SpawnPoints, "Spawn points");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.CameraRig, "Camera rig");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.CameraBounds2D, "2D camera bounds");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.PlayerInputManager, "Local join / PlayerInputManager");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.GameplayStateService, "Gameplay state service");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.CameraBoundsService, "Camera bounds service");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.ScoreService, "Score service");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.HudOrMenuSurface, "HUD or menu surface");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.ProjectileOrHitboxSource, "Projectile or hitbox source");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.EnemyOrNpcSpawner, "Enemy or NPC spawner");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.TabletopRuntimeContract, "Tabletop runtime contract");
            DrawProofToggle(pattern, RuntimePatternFirstProofRequirement.SelectionSurface, "Selection surface");
        }

        private static void DrawProofToggle(RuntimePatternDefinition pattern, RuntimePatternFirstProofRequirement requirement, string label)
        {
            bool enabled = pattern.RequiresFirstProof(requirement);
            EditorGUI.BeginChangeCheck();
            bool nextEnabled = EditorGUILayout.ToggleLeft(label, enabled);
            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(pattern, "Edit Runtime Pattern First Proof Requirement");
            if (nextEnabled)
                pattern.firstProofRequirements |= requirement;
            else
                pattern.firstProofRequirements &= ~requirement;
            pattern.Sanitize();
            EditorUtility.SetDirty(pattern);
        }

        private static bool ContainsLane(RuntimePatternPresentationLane[] lanes, RuntimePatternPresentationLane lane)
        {
            if (lanes == null)
                return false;

            for (int i = 0; i < lanes.Length; i++)
            {
                if (lanes[i] == lane)
                    return true;
            }

            return false;
        }

        private static string GetLaneLabel(RuntimePatternPresentationLane lane)
        {
            switch (lane)
            {
                case RuntimePatternPresentationLane.Billboard2_5D:
                    return "Billboard 2.5D";
                case RuntimePatternPresentationLane.Rigged3D:
                    return "Rigged 3D";
                case RuntimePatternPresentationLane.TabletopNoPawn:
                    return "Tabletop / no pawn";
                case RuntimePatternPresentationLane.UiMenu:
                    return "UI / menu";
                case RuntimePatternPresentationLane.CameraCursor:
                    return "Camera / cursor";
                default:
                    return lane.ToString();
            }
        }

        private static void DrawIssues(List<string> issues)
        {
            PyralisInspectorGuide.DrawValidationIssues(issues, "Pattern metadata is ready for setup-profile use.");
        }

        private static bool SupportsNonPawnSurface(RuntimePatternDefinition pattern)
        {
            if (pattern.supportedControlSurfaces == null)
                return false;

            for (int i = 0; i < pattern.supportedControlSurfaces.Length; i++)
            {
                if (pattern.supportedControlSurfaces[i] != RuntimeControlSurface.Pawn)
                    return true;
            }

            return false;
        }

        private static string GetSuggestedDescription(RuntimePatternDefinition pattern)
        {
            return RuntimePatternAuthoringText.GetSuggestedDescription(pattern);
        }

        private static string GetSuggestedSetupNotes(RuntimePatternDefinition pattern)
        {
            return RuntimePatternAuthoringText.GetSuggestedSetupNotes(pattern);
        }

        private static string GetEmbodimentHelp(RuntimePatternDefinition pattern)
        {
            return RuntimePatternAuthoringText.GetEmbodimentHelp(pattern);
        }

        private static string GetPatternLabel(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return "<null>";

            if (!string.IsNullOrWhiteSpace(pattern.displayName))
                return pattern.displayName;

            if (!string.IsNullOrWhiteSpace(pattern.patternId))
                return pattern.patternId;

            return pattern.name;
        }

        private static int CountAssigned(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return 0;

            int count = 0;
            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i] != null)
                    count++;
            }

            return count;
        }
    }
}
