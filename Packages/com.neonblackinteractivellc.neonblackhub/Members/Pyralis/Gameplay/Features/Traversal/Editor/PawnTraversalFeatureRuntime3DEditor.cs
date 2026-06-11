using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Characters;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal.Editor
{
    [CustomEditor(typeof(PawnTraversalFeatureRuntime3D))]
    public sealed class PawnTraversalFeatureRuntime3DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Pawn Traversal Feature Runtime 3D",
                new PyralisGuideSection(
                    "What This Is",
                    "PawnTraversalFeatureRuntime3D installs ledge probe, hang, shimmy, climb-up, and traversal interaction behavior as an actor feature runtime.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("RUNTIME_PATTERN_COOKBOOK.md")),
                new PyralisGuideSection(
                    "Feature Module Fields",
                    null,
                    new[]
                    {
                        PyralisAuthoringContractGuideText.FeatureModuleSetup((PawnTraversalFeatureRuntime3D)target),
                        "Set Runtime Prefab to a prefab containing this runtime component and Pawn3DTraversalComponent.",
                        "Keep Supported Presentation Modes limited to 3D pawn setups.",
                        "Install this through ActorFeatureHost or another actor feature bootstrap."
                    }),
                new PyralisGuideSection(
                    "Actor Fields",
                    null,
                    new[]
                    {
                        "The actor root needs Motor3D, Pawn3DMovementComponent, CharacterController, and Pawn3DTraversalComponent.",
                        "ActorInteractionInputBridge2D is for 2D actors; 3D input should call the traversal feature through the 3D interaction/input route.",
                        "Use ClimbZone scene objects when the pawn can hang, climb, mantle, or use ledge assists."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not place this runtime component directly on a scene pawn unless a custom bootstrap initializes it.",
                        "Do not use this on 2D or board/menu participants.",
                        "Do not expect traversal to work without Pawn3DTraversalComponent on the same GameObject."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(serializedObject, (PawnTraversalFeatureRuntime3D)target), "PawnTraversalFeatureRuntime3D is ready for 3D actor traversal feature wiring.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetMessages(SerializedObject serializedObject, PawnTraversalFeatureRuntime3D runtime)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty profile = serializedObject.FindProperty("traversalProfile");
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Traversal Profile is empty. This is expected when FeatureModuleDefinition provides the PawnTraversalProfile at runtime."));

            GameObject root = runtime != null ? runtime.gameObject : null;
            if (root != null && root.GetComponent<Pawn3DTraversalComponent>() == null)
                messages.Add(PyralisGuideIssue.Required("Pawn3DTraversalComponent is required on the same GameObject."));

            if (root != null && root.GetComponent<Motor3D>() == null)
                messages.Add(PyralisGuideIssue.Required("Motor3D is missing from this actor root."));

            if (root != null && root.GetComponent<Pawn3DMovementComponent>() == null)
                messages.Add(PyralisGuideIssue.Required("Pawn3DMovementComponent is missing from this actor root."));

            return messages;
        }
    }
}
