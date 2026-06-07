using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal.Editor
{
    [CustomEditor(typeof(Pawn3DTraversalComponent))]
    public sealed class Pawn3DTraversalComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameObject root = ((Pawn3DTraversalComponent)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "3D Pawn Stack Field Guide: Traversal Component",
                defaultOpen: false,
                sections: new[]
                {
                new PyralisGuideSection(
                    "What This Is",
                    "Pawn3DTraversalComponent handles ledge probes, ClimbZone interactions, hanging, dropping, shimmy movement, and climb animation signals for a Motor3D pawn.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Required Context",
                    null,
                    new[]
                    {
                        "Keep this on the same root GameObject as Motor3D, Pawn3DMovementComponent, and CharacterController.",
                        "Use ClimbZone objects in the scene when this pawn can climb, hang, mantle, or use ledge assists.",
                        "Use a GrabDetector child only when you want trigger-based ledge detection instead of probe-only traversal.",
                        "ActorAnimationDriver is recommended when climb, hang, shimmy, drop, or interact animations matter."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not add traversal to board/card/menu/camera-only participants that do not have pawn bodies.",
                        "Do not expect traversal to move without Pawn3DMovementComponent; traversal mutates that movement state.",
                        "Do not forget ClimbZone scene objects when testing climb or hang behavior.",
                        "Do not force traversal into games where movement is driven by board rules or menu actions."
                    })
                });

            DrawDefaultInspector();

            PyralisInspectorGuide.DrawValidationMessages(
                GetTraversalMessages(root),
                "Pawn3DTraversalComponent is ready for 3D pawn traversal.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetTraversalMessages(GameObject root)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            RequireComponent<Motor3D>(messages, root, "Motor3D is missing. It calls traversal checks and hang handling each frame.");
            RequireComponent<Pawn3DMovementComponent>(messages, root, "Pawn3DMovementComponent is missing. Traversal updates movement and climb state through it.");
            RequireComponent<CharacterController>(messages, root, "CharacterController is missing. Traversal uses it while hanging and climbing.");

            if (root.GetComponent<ActorAnimationDriver>() == null)
                messages.Add(PyralisGuideIssue.Recommended("ActorAnimationDriver is missing. Add it when traversal should fire climb, hang, shimmy, drop, or interact animation signals."));

            return messages;
        }

        private static void RequireComponent<T>(List<PyralisGuideIssue> messages, GameObject root, string message)
            where T : Component
        {
            if (root != null && root.GetComponent<T>() == null)
                messages.Add(PyralisGuideIssue.Required(message));
        }
    }
}
