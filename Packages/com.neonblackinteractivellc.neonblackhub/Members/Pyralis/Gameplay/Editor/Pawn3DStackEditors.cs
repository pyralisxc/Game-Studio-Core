using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Traversal;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static NeonBlack.Gameplay.Editor.Inspectors.Pawn3DStackEditorUtility;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(Motor3D))]
    public sealed class Motor3DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameObject root = ((Motor3D)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "3D Pawn Stack Field Guide: Motor 3D",
                defaultOpen: false,
                sections: new[]
                {
                new PyralisGuideSection(
                    "What This Is",
                    "Motor3D is the coordinator for a 2.5D or rigged 3D pawn stack. It does not define the pawn asset, spawn the pawn, or own every gameplay setting; it sequences the sibling input, movement, traversal, presentation, combat, and feedback modules each frame.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Where This Fits",
                    "Use this on the root GameObject of a pawn prefab that is referenced by a PawnDefinition.",
                    new[]
                    {
                        "SessionDefinition lists participants.",
                        "ParticipantDefinition points to a PawnDefinition when that participant needs a body.",
                        "PawnDefinition points to the prefab.",
                        "The prefab root should contain PawnRoot and this 3D pawn stack.",
                        "GameplaySessionBootstrap spawns the participant pawn and injects runtime context."
                    }),
                new PyralisGuideSection(
                    "Required Siblings",
                    null,
                    new[]
                    {
                        "PawnRoot on the same prefab root so the authored PawnDefinition can apply profiles.",
                        "CharacterController for collision and movement.",
                        "HealthComponent and KnockbackReceiver for damage/reaction support.",
                        "Pawn3DInputModule, Pawn3DMovementComponent, Pawn3DTraversalComponent, and Pawn3DPresentationComponent.",
                        "ActorAnimationDriver when presentation or Animator signals are used."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not add Motor3D to board/card/menu/camera-only objects; those are non-pawn control surfaces.",
                        "Do not expect Motor3D to move by itself without Pawn3DInputModule and Pawn3DMovementComponent.",
                        "Do not assign PawnDefinition here; assign it on PawnRoot or through ParticipantDefinition > Default Pawn.",
                        "Do not skip CharacterController just because the model has a collider."
                    })
                });

            DrawDefaultInspector();

            PyralisInspectorGuide.DrawValidationMessages(
                GetMotorMessages(root),
                "Motor3D has the required 3D pawn stack siblings.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetMotorMessages(GameObject root)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            RequireComponent<PawnRoot>(messages, root, "PawnRoot is missing. Add it to the prefab root so PawnDefinition profiles can apply.");
            RequireComponent<CharacterController>(messages, root, "CharacterController is missing. Motor3D movement uses it for collision and motion.");
            RequireComponent<HealthComponent>(messages, root, "HealthComponent is missing. Motor3D expects it for damage and reaction wiring.");
            RequireComponent<KnockbackReceiver>(messages, root, "KnockbackReceiver is missing. Motor3D and combat reactions use it for hit movement.");
            RequireComponent<Pawn3DInputModule>(messages, root, "Pawn3DInputModule is missing. Motor3D needs it to collect FrameInput.");
            RequireComponent<Pawn3DMovementComponent>(messages, root, "Pawn3DMovementComponent is missing. Motor3D delegates movement to it.");
            RequireComponent<Pawn3DTraversalComponent>(messages, root, "Pawn3DTraversalComponent is missing. Add it for ledge, climb, and traversal support.");
            RequireComponent<Pawn3DPresentationComponent>(messages, root, "Pawn3DPresentationComponent is missing. Motor3D delegates animation and facing to it.");

            if (root.GetComponent<ActorAnimationDriver>() == null)
                messages.Add(PyralisGuideIssue.Recommended("ActorAnimationDriver is missing. Add it when this pawn uses Animator signals, billboard facing, rigged visuals, or animation profiles."));

            if (root.GetComponent<PawnCombatBehaviour>() == null)
                messages.Add(PyralisGuideIssue.Optional("PawnCombatBehaviour is empty. This is fine for non-combat pawns; add it when attacks, blocking, or weapon cycling are needed."));

            return messages;
        }
    }

    [CustomEditor(typeof(Pawn3DInputModule))]
    public sealed class Pawn3DInputModuleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameObject root = ((Pawn3DInputModule)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "3D Pawn Stack Field Guide: Input Module",
                defaultOpen: false,
                sections: new[]
                {
                new PyralisGuideSection(
                    "What This Is",
                    "Pawn3DInputModule translates Unity Input System actions into a FrameInput snapshot. Motor3D reads that snapshot once per frame and passes it to movement, traversal, combat, and presentation.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Required Context",
                    null,
                    new[]
                    {
                        "Keep this on the same root GameObject as Motor3D.",
                        "Assign Input Actions directly, assign an InputConfig, use PlayerInput.actions, or let the session provide default input actions at runtime.",
                        "The action asset should expose a Player action map with Move, Jump, Attack, Kick, Interact, Sprint, and Crouch actions.",
                        "Optional actions include Look, Previous, Next, Roll, Block, and LookAround."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put input here for AI-only pawns unless a system intentionally feeds player-style input.",
                        "Do not expect InputProfile to create scene controls; it only configures input once this module or PlayerInput exists.",
                        "Do not place this on a camera, board, or menu surface unless that surface is also acting as a pawn."
                    })
                });

            DrawDefaultInspector();

            PyralisInspectorGuide.DrawValidationMessages(
                GetInputMessages(root, serializedObject),
                "Pawn3DInputModule is ready for Motor3D input collection.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetInputMessages(GameObject root, SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            RequireComponent<Motor3D>(messages, root, "Motor3D is missing. Pawn3DInputModule is normally read by Motor3D.");
            RequireComponent<PawnRoot>(messages, root, "PawnRoot is missing. Add it on pawn prefabs that should receive PawnDefinition profiles.");

            bool hasInputActions = serializedObject.FindProperty("inputActions")?.objectReferenceValue != null;
            bool hasInputConfig = serializedObject.FindProperty("inputConfig")?.objectReferenceValue != null;
            bool hasPlayerInput = root.GetComponent<PlayerInput>() != null;

            if (!hasInputActions && !hasInputConfig && !hasPlayerInput)
                messages.Add(PyralisGuideIssue.Recommended("No Input Actions, InputConfig, or PlayerInput component is assigned. This can still work if GameplayRuntimeContext provides default input actions at runtime."));

            return messages;
        }
    }

    [CustomEditor(typeof(Pawn3DMovementComponent))]
    public sealed class Pawn3DMovementComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameObject root = ((Pawn3DMovementComponent)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "3D Pawn Stack Field Guide: Movement Component",
                defaultOpen: false,
                sections: new[]
                {
                new PyralisGuideSection(
                    "What This Is",
                    "Pawn3DMovementComponent owns the 3D movement model, CharacterController movement, ground checks, crouch capsule resizing, dodges, jumps, slides, and profile-applied movement tuning.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Required Context",
                    null,
                    new[]
                    {
                        "Keep this on the same root GameObject as Motor3D and CharacterController.",
                        "Tune movement values directly here or through PawnDefinition > Movement Profile.",
                        "Tune jump, gravity, dodge, and traversal values directly here or through PawnDefinition > Traversal Profile.",
                        "Set Ground Layer to the terrain/platform layers the CharacterController should treat as walkable ground.",
                        "Assign Movement Camera when this pawn should move relative to a gameplay camera instead of world axes."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not use this as a standalone mover; Motor3D calls its per-frame Tick and ApplyMovement methods.",
                        "Do not leave Ground Layer mismatched with the level geometry.",
                        "Do not expect Rigidbody physics here; this stack is CharacterController-based.",
                        "Do not rely on Camera.main for camera-relative movement; assign Movement Camera for split-screen, replay, or custom camera rigs."
                    })
                });

            DrawDefaultInspector();

            PyralisInspectorGuide.DrawValidationMessages(
                GetMovementMessages(root, serializedObject),
                "Pawn3DMovementComponent is ready for Motor3D movement.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetMovementMessages(GameObject root, SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            RequireComponent<Motor3D>(messages, root, "Motor3D is missing. It coordinates this movement component each frame.");
            RequireComponent<CharacterController>(messages, root, "CharacterController is missing. This movement component applies motion through it.");

            if (root.GetComponent<KnockbackReceiver>() == null)
                messages.Add(PyralisGuideIssue.Recommended("KnockbackReceiver is missing. Non-combat pawns can still move, but combat or damage reactions will not push this pawn."));

            SerializedProperty movementMode = serializedObject.FindProperty("movementMode");
            SerializedProperty movementCamera = serializedObject.FindProperty("movementCamera");
            if (movementMode != null
                && movementCamera != null
                && movementCamera.objectReferenceValue == null
                && movementMode.enumValueIndex != (int)MovementMode.TwoD)
            {
                messages.Add(PyralisGuideIssue.Recommended("Movement Camera is empty. 3D/top-down input will use world axes instead of camera-relative axes."));
            }

            return messages;
        }
    }

    [CustomEditor(typeof(Pawn3DPresentationComponent))]
    public sealed class Pawn3DPresentationComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameObject root = ((Pawn3DPresentationComponent)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "3D Pawn Stack Field Guide: Presentation Component",
                defaultOpen: false,
                sections: new[]
                {
                new PyralisGuideSection(
                    "What This Is",
                    "Pawn3DPresentationComponent reads movement, combat, traversal, and look-around state, then forwards shared ActorAnimationSignal values into ActorAnimationDriver.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Required Context",
                    null,
                    new[]
                    {
                        "Keep this on the same root GameObject as Motor3D and Pawn3DMovementComponent.",
                        "Add ActorAnimationDriver when this pawn has an Animator, billboard sprite, rigged visual, or animation profile.",
                        "Assign animation through PawnDefinition > Animation Profile and ActorAnimationDefinition instead of hardcoding raw Animator parameter names.",
                        "Presentation Profile chooses 2.5D billboard or rigged 3D behavior."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not treat this as the Animator controller itself; ActorAnimationDriver owns the mapping.",
                        "Do not skip Pawn3DMovementComponent, because presentation reads movement state from it.",
                        "Do not force billboard behavior on rigged 3D presentation."
                    })
                });

            DrawDefaultInspector();

            PyralisInspectorGuide.DrawValidationMessages(
                GetPresentationMessages(root),
                "Pawn3DPresentationComponent is ready for 3D pawn presentation.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetPresentationMessages(GameObject root)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            RequireComponent<Motor3D>(messages, root, "Motor3D is missing. It calls this presentation component each frame.");
            RequireComponent<Pawn3DMovementComponent>(messages, root, "Pawn3DMovementComponent is missing. Presentation reads movement state from it.");
            RequireComponent<ActorAnimationDriver>(messages, root, "ActorAnimationDriver is missing. This component forwards animation signals through it.");

            if (root.GetComponent<Animator>() == null && root.GetComponentInChildren<Animator>() == null)
                messages.Add(PyralisGuideIssue.Optional("No Animator found on this object or its children. This is fine while blocking out movement, but animation signals need an Animator-backed visual later."));

            return messages;
        }
    }

    internal static class Pawn3DStackEditorUtility
    {
        public static void RequireComponent<T>(List<PyralisGuideIssue> messages, GameObject root, string message)
            where T : Component
        {
            if (root != null && root.GetComponent<T>() == null)
                messages.Add(PyralisGuideIssue.Required(message));
        }
    }
}
