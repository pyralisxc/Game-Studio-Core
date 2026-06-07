using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Enums;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PawnMovementProfile))]
    public class PawnMovementProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PawnMovementProfile profile = (PawnMovementProfile)target;

            EditorGUILayout.LabelField("Movement Starting Points", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Apply an editable starting point to this PawnMovementProfile, then tune every field for the game you are making. These helpers do not choose a setup, lock a route, or replace the pawn prefab's runtime components.", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply Top-Down 2D Start"))
                    ApplyTopDown2DDefaults(profile);

                if (GUILayout.Button("Apply Side-View 2D Start"))
                    ApplySideView2DDefaults(profile);
            }

            DrawCurrentRouteHint(profile);

            EditorGUILayout.Space(6f);
            DrawDefaultInspector();

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Pawn Movement Profile",
                "A pawn movement profile stores reusable movement tuning for a PawnDefinition and its movement components.",
                whenToUse: new[]
                {
                    "Use this for pawn-backed characters, enemies, controllable units, or actors that move in 2D, 2.5D, or 3D.",
                    "Skip it for non-pawn card, board, menu, or camera-only participants."
                },
                createBefore: new[]
                {
                    "PawnDefinition that will reference this profile.",
                    "Pawn prefab with the matching movement stack: Motor2D/Pawn2DMovementComponent or Motor3D/Pawn3DMovementComponent."
                },
                assignFirst: new[]
                {
                    "Choose Movement Mode.",
                    "Set walk, sprint, crouch, acceleration, and deceleration.",
                    "Choose whether this pawn uses CharacterController, top-down/free 2D movement, or side-view 2D jump physics."
                },
                safeToCustomize: new[]
                {
                    "Depth movement is useful for 2.5D lanes and brawler arenas.",
                    "Screen wrap is mostly for arcade loops.",
                    "Depth speed multiplier controls how fast depth-axis movement feels compared with horizontal movement.",
                    "For top-down/free 2D movement, use Movement Mode = TwoD, enable Use 2D Physics, and leave Allow 2D Jump off. Move input drives X/Y movement; Jump can be handled by a feature module such as top-down hop.",
                    "For 2D side-view/platformer movement, enable Allow 2D Jump, tune Jump Velocity 2D and Gravity Scale 2D, and keep Dash Action separate unless the game intentionally has a dodge/dash ability. Move input drives X only; Jump Action drives vertical motion.",
                    "Treat dash, interact, guard, attacks, spells, tools, and other optional actions as abilities or feature modules when they are not part of baseline locomotion."
                },
                validation: new[]
                {
                    "PawnDefinition references this profile.",
                    "Pawn prefab has movement components that match the profile's 2D/3D intent.",
                    "Top-down/free 2D pawns have Allow 2D Jump off before testing vertical map movement.",
                    "Side-view 2D pawns have walk speed, jump velocity, gravity, Rigidbody2D, Collider2D, and ground layer setup."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Pawn movement profile is ready for PawnDefinition assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawCurrentRouteHint(PawnMovementProfile profile)
        {
            if (profile == null || profile.movementMode != MovementMode.TwoD)
                return;

            if (profile.allow2DJump)
            {
                EditorGUILayout.HelpBox(
                    "Current 2D route: Side-view/platformer. Move input drives horizontal motion and the Jump action applies vertical velocity only while grounded. Make sure the pawn has a walkable Collider2D on the Ground Layer and that Ground Check Offset sits at the feet.",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(
                "Current 2D route: Top-down/free movement. Move input drives X/Y movement. Use a feature module such as top-down hop when Space/Button South should lift the visual, or apply the Side-View 2D starting point when Jump should drive Rigidbody2D vertical motion.",
                MessageType.Info);
        }

        private static List<string> GetValidationIssues(PawnMovementProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile == null)
                return issues;

            if (profile.walkSpeed <= 0f && profile.sprintSpeed <= 0f)
                issues.Add("Walk Speed or Sprint Speed should be greater than zero for moving pawns.");

            if (profile.useCharacterController && profile.use2DPhysics)
                issues.Add("Choose CharacterController or 2D physics as the primary movement backend, not both, unless a custom adapter expects it.");

            if (profile.movementMode == MovementMode.TwoD && profile.useCharacterController)
                issues.Add("2D movement profiles should usually leave Use Character Controller off; use Rigidbody2D through the 2D pawn stack unless a custom adapter expects otherwise.");

            if (profile.movementMode == MovementMode.TwoD && !profile.use2DPhysics)
                issues.Add("2D movement profiles should usually enable Use 2D Physics so Pawn2DMovementComponent can apply the profile cleanly.");

            if (profile.allow2DJump)
            {
                if (!profile.use2DPhysics)
                    issues.Add("Allow 2D Jump needs Use 2D Physics because the side-view route uses Rigidbody2D gravity.");

                if (profile.jumpVelocity2D <= 0f)
                    issues.Add("Jump Velocity 2D should be greater than zero when Allow 2D Jump is enabled.");

                if (profile.gravityScale2D <= 0f)
                    issues.Add("Gravity Scale 2D should be greater than zero when Allow 2D Jump is enabled.");
            }

            return issues;
        }

        private static void ApplyTopDown2DDefaults(PawnMovementProfile profile)
        {
            if (profile == null)
                return;

            Undo.RecordObject(profile, "Apply Top-Down 2D Pawn Movement Start");
            profile.movementMode = MovementMode.TwoD;
            profile.walkSpeed = Mathf.Max(profile.walkSpeed, 5f);
            profile.sprintSpeed = Mathf.Max(profile.sprintSpeed, profile.walkSpeed);
            profile.crouchSpeed = Mathf.Min(Mathf.Max(profile.crouchSpeed, 2.5f), profile.walkSpeed);
            profile.acceleration = Mathf.Max(profile.acceleration, 20f);
            profile.deceleration = Mathf.Max(profile.deceleration, 25f);
            profile.useCharacterController = false;
            profile.use2DPhysics = true;
            profile.allowDepthMovement = false;
            profile.allowScreenWrap = false;
            profile.allow2DJump = false;
            profile.gravityScale2D = 0f;
            profile.Sanitize();
            EditorUtility.SetDirty(profile);
        }

        private static void ApplySideView2DDefaults(PawnMovementProfile profile)
        {
            if (profile == null)
                return;

            Undo.RecordObject(profile, "Apply Side-View 2D Pawn Movement Start");
            profile.movementMode = MovementMode.TwoD;
            profile.walkSpeed = Mathf.Max(profile.walkSpeed, 5f);
            profile.sprintSpeed = Mathf.Max(profile.sprintSpeed, profile.walkSpeed);
            profile.crouchSpeed = Mathf.Min(Mathf.Max(profile.crouchSpeed, 2.5f), profile.walkSpeed);
            profile.acceleration = Mathf.Max(profile.acceleration, 20f);
            profile.deceleration = Mathf.Max(profile.deceleration, 25f);
            profile.useCharacterController = false;
            profile.use2DPhysics = true;
            profile.allowDepthMovement = false;
            profile.allowScreenWrap = false;
            profile.allow2DJump = true;
            profile.jumpVelocity2D = Mathf.Max(profile.jumpVelocity2D, 8f);
            profile.gravityScale2D = Mathf.Max(profile.gravityScale2D, 3f);
            profile.Sanitize();
            EditorUtility.SetDirty(profile);
        }
    }
}
