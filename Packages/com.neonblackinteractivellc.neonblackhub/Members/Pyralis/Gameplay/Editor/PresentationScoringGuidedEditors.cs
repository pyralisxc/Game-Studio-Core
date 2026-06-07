using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Scoring;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ActorShadowDriver))]
    public sealed class ActorShadowDriverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Actor Shadow Driver",
                new PyralisGuideSection(
                    "What This Is",
                    "ActorShadowDriver applies pawn presentation shadow settings, either as renderer shadow flags or a generated blob-sprite shadow under the actor.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Presentation_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the actor root or visual root that owns the pawn presentation.",
                        "Assign Presentation Profile directly for authored prefabs, or let the pawn presentation stack call ApplyProfile at runtime.",
                        "Assign Shadow Sprite Renderer or Shadow Root when using a hand-authored blob shadow.",
                        "Assign Model Renderers when rigged 3D model shadow casting should be controlled explicitly."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not use this for gameplay collision or hitbox positioning.",
                        "Do not leave blob shadow sorting untuned in 2D scenes with dense foreground art.",
                        "Do not expect generated runtime shadows to become saved prefab children."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PresentationScoringEditorUtility.GetActorShadowMessages(serializedObject, (ActorShadowDriver)target), "ActorShadowDriver is ready for actor presentation shadows.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(BillboardFacing3D))]
    public sealed class BillboardFacing3DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Billboard Facing 3D",
                new PyralisGuideSection(
                    "What This Is",
                    "BillboardFacing3D turns a visual target toward the active camera and optionally mirrors a sprite or visual root for left/right facing.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Presentation_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the 3D visual object that should face the camera.",
                        "Assign Target when a child visual, not this GameObject, should rotate.",
                        "Assign Sprite Renderer for simple sprite flip facing, or Mirrored Visual Root for full-facing visual hierarchy mirroring.",
                        "Assign Camera Override to the gameplay camera that this billboard should face."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put movement or gameplay rotation logic here; this is presentation-only.",
                        "Do not use Full Facing without a mirrored visual root when sprites need left/right flips.",
                        "Do not leave Camera Override empty on split-screen, replay, preview, or custom camera rigs."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PresentationScoringEditorUtility.GetBillboardMessages(serializedObject), "BillboardFacing3D is ready for camera-facing presentation.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(LeaderboardManager))]
    public sealed class LeaderboardManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Leaderboard Manager",
                new PyralisGuideSection(
                    "What This Is",
                    "LeaderboardManager is the compile-safe leaderboard service bridge. Without a backend integration package it logs no-op warnings while keeping menu and gameplay code wired.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Scoring_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place one LeaderboardManager in menus or bootstrap scenes that expose leaderboard UI.",
                        "Set Leaderboard Id to the backend leaderboard key when an online leaderboard integration is installed.",
                        "Set Top Scores Fetch Limit to the number of rows the LeaderboardScreen should request.",
                        "Register or inject ILeaderboardService through the gameplay lifetime scope when replacing the no-op bridge."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not add multiple active LeaderboardManager instances.",
                        "Do not expect online scores until the backend leaderboard package and service implementation are installed.",
                        "Do not depend on LeaderboardManager.Instance; route leaderboard UI and score submission through ILeaderboardService."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PresentationScoringEditorUtility.GetLeaderboardManagerMessages(serializedObject, (LeaderboardManager)target), "LeaderboardManager is ready for leaderboard service wiring.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(LeaderboardScreen))]
    public sealed class LeaderboardScreenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Leaderboard Screen",
                new PyralisGuideSection(
                    "What This Is",
                    "LeaderboardScreen swaps from a main menu page to a leaderboard page, fetches top scores from ILeaderboardService, and instantiates row prefabs into a ScrollView content transform.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Scoring_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Main Menu Page and Leaderboard Page root GameObjects.",
                        "Assign Back Button so Close returns to the main menu page.",
                        "Assign Row Container to the ScrollView Content transform.",
                        "Assign Row Prefab with three TextMeshProUGUI children in rank, name, score order.",
                        "Assign Status Label for loading and empty-state text."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Leaderboard Page active by default unless the menu should open directly to it.",
                        "Do not use a row prefab without three TMP labels.",
                        "Do not expect results without a scene service that implements ILeaderboardService."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PresentationScoringEditorUtility.GetLeaderboardScreenMessages(serializedObject), "LeaderboardScreen is ready for menu leaderboard flow.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    internal static class PresentationScoringEditorUtility
    {
        public static List<PyralisGuideIssue> GetActorShadowMessages(SerializedObject serializedObject, ActorShadowDriver driver)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "presentationProfile"))
                messages.Add(PyralisGuideIssue.Optional("Presentation Profile is empty. This is expected when Pawn2DPresentationComponent or another presentation stack calls ApplyProfile at runtime."));

            if (driver != null && driver.GetComponentInChildren<Renderer>(true) == null)
                messages.Add(PyralisGuideIssue.Recommended("No child Renderer was found. Add a sprite/model renderer or assign generated shadow settings through a profile."));

            bool hasShadowRoot = HasObject(serializedObject, "shadowRoot");
            bool hasShadowRenderer = HasObject(serializedObject, "shadowSpriteRenderer");
            if (hasShadowRoot != hasShadowRenderer)
                messages.Add(PyralisGuideIssue.Optional("Shadow Root and Shadow Sprite Renderer are usually assigned together for hand-authored blob shadows."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetBillboardMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty facingMode = serializedObject.FindProperty("facingMode");

            if (!HasObject(serializedObject, "cameraOverride"))
                messages.Add(PyralisGuideIssue.Recommended("Camera Override is empty. Assign the camera this billboard should face, or call SetCameraOverride at spawn time."));

            if (facingMode != null
                && facingMode.enumDisplayNames.Length > facingMode.enumValueIndex
                && facingMode.enumDisplayNames[facingMode.enumValueIndex] == "Full Facing"
                && !HasObject(serializedObject, "mirroredVisualRoot"))
            {
                messages.Add(PyralisGuideIssue.Optional("Full Facing often needs Mirrored Visual Root assigned so left/right facing can mirror the visual hierarchy."));
            }

            if (!HasObject(serializedObject, "spriteRenderer") && !HasObject(serializedObject, "mirroredVisualRoot"))
                messages.Add(PyralisGuideIssue.Recommended("Assign Sprite Renderer or Mirrored Visual Root when ApplyFacing should visually flip the actor."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetLeaderboardManagerMessages(SerializedObject serializedObject, LeaderboardManager manager)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty leaderboardId = serializedObject.FindProperty("_leaderboardId");
            SerializedProperty fetchLimit = serializedObject.FindProperty("_topScoresFetchLimit");

            if (leaderboardId != null && string.IsNullOrWhiteSpace(leaderboardId.stringValue))
                messages.Add(PyralisGuideIssue.Required("Leaderboard Id should not be blank."));

            if (fetchLimit != null && fetchLimit.intValue < 1)
                messages.Add(PyralisGuideIssue.Required("Top Scores Fetch Limit must be at least 1."));

            if (manager != null && Object.FindObjectsByType<LeaderboardManager>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Multiple LeaderboardManager instances are active. Register one ILeaderboardService for each menu/gameplay flow so score UI does not bind ambiguously."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetLeaderboardScreenMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequireObject(messages, serializedObject, "_mainMenuPage", "Main Menu Page");
            RequireObject(messages, serializedObject, "_leaderboardPage", "Leaderboard Page");
            RequireObject(messages, serializedObject, "_backButton", "Back Button");
            RequireObject(messages, serializedObject, "_rowContainer", "Row Container");
            RequireObject(messages, serializedObject, "_rowPrefab", "Row Prefab");

            SerializedProperty rowPrefab = serializedObject.FindProperty("_rowPrefab");
            if (rowPrefab != null && rowPrefab.objectReferenceValue is GameObject rowPrefabObject)
            {
                if (CountComponentsByTypeName(rowPrefabObject, "TMPro.TextMeshProUGUI") < 3)
                    messages.Add(PyralisGuideIssue.Required("Row Prefab should contain at least three TextMeshProUGUI labels in rank, name, score order."));
            }

            if (!HasObject(serializedObject, "_statusLabel"))
                messages.Add(PyralisGuideIssue.Recommended("Status Label is recommended so loading, empty, and unavailable states are visible."));

            return messages;
        }

        private static void RequireObject(List<PyralisGuideIssue> messages, SerializedObject serializedObject, string propertyName, string displayName)
        {
            if (!HasObject(serializedObject, propertyName))
                messages.Add(PyralisGuideIssue.Required(displayName + " should be assigned."));
        }

        private static bool HasObject(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.objectReferenceValue != null;
        }

        private static int CountComponentsByTypeName(GameObject root, string typeName)
        {
            if (root == null)
                return 0;

            int count = 0;
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null && behaviours[i].GetType().FullName == typeName)
                    count++;
            }

            return count;
        }
    }
}
