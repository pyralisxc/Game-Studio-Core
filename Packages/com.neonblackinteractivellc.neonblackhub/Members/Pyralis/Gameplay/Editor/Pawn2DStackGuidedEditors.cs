using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(Motor2D))]
    public sealed class Motor2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GameObject root = ((Motor2D)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "2D Pawn Stack Field Guide: Motor 2D",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "Motor2D is the shared 2D pawn motor surface. Movement, presentation, input, and reactions live in focused sibling components so the pawn stays inspectable and profile-driven.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                    new PyralisGuideSection(
                        "Required Siblings",
                        null,
                        new[]
                        {
                            "Pawn2DMovementComponent for Rigidbody2D movement, bounds, dash, and reaction locks.",
                            "Pawn2DPresentationComponent for sprite tint, flip, tilt, squash/stretch, audio, and animation signals.",
                            "Rigidbody2D and PolygonCollider2D from the movement component requirements.",
                            "ActorAnimationDriver when this pawn has Animator-driven presentation.",
                            "HealthComponent and combat/feedback components when the pawn can take damage or attack."
                        }),
                    new PyralisGuideSection(
                        "Route Fit",
                        null,
                        new[]
                        {
                            "Player pawn path: pair with Motor2DInputAdapter, which uses the current input profile route.",
                            "AI pawn path: drive MoveDirection, TryDash, and combat receivers from AI instead of player input.",
                            "Board/card/menu path: skip this stack unless the object is an embodied 2D pawn."
                        })
                });

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(root), "Motor2D has the expected 2D pawn stack siblings.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetMessages(GameObject root)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            Pawn3DStackEditorUtility.RequireComponent<Pawn2DMovementComponent>(messages, root, "Pawn2DMovementComponent is missing. Motor2D delegates movement to it.");
            Pawn3DStackEditorUtility.RequireComponent<Pawn2DPresentationComponent>(messages, root, "Pawn2DPresentationComponent is missing. Motor2D delegates presentation feedback to it.");
            Pawn3DStackEditorUtility.RequireComponent<Rigidbody2D>(messages, root, "Rigidbody2D is missing. The 2D movement component requires it.");
            Pawn3DStackEditorUtility.RequireComponent<PolygonCollider2D>(messages, root, "PolygonCollider2D is missing. The 2D movement component requires it for pawn collision.");

            if (root.GetComponent<ActorAnimationDriver>() == null)
                messages.Add(PyralisGuideIssue.Recommended("ActorAnimationDriver is missing. Add it when this pawn uses animation profiles, Animator signals, or sprite/visual facing."));

            if (root.GetComponent<Motor2DInputAdapter>() == null && root.GetComponent<PlayerInputHandler>() == null)
                messages.Add(PyralisGuideIssue.Optional("No 2D input adapter found. This is fine for AI or scripted pawns; player pawns usually need Motor2DInputAdapter or PlayerInputHandler."));

            return messages;
        }
    }

    [CustomEditor(typeof(Pawn2DMovementComponent))]
    public sealed class Pawn2DMovementComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GameObject root = ((Pawn2DMovementComponent)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "2D Pawn Stack Field Guide: Movement Component",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "Pawn2DMovementComponent owns Rigidbody2D movement, camera bounds, dash timing, screen wrap, dead zones, and movement locks for a 2D pawn. With Jump Enabled off it is a top-down/free-2D mover; with Jump Enabled on it is a side-view/platformer mover.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                    new PyralisGuideSection(
                        "Required Context",
                        null,
                        new[]
                        {
                            "Keep this on the same root GameObject as Motor2D.",
                            "Choose the movement route before tuning: top-down/free 2D leaves Jump Enabled off so Move input drives X/Y; side-view/platformer enables Jump Enabled so Move X drives horizontal movement and the Jump action supplies vertical velocity.",
                            "Use CinemachineCameraRigController as the Camera Bounds Source when assigning directly, or assign an explicit Target Camera. On spawned prefabs these fields can stay empty when the scene session supplies camera bounds at runtime.",
                            "Let GameManager configure Gameplay State Source at runtime, or assign another IGameplayStateReader directly for standalone prefab tests or custom scene services.",
                            "For side-view/platformer 2D, set Ground Layer to your walkable Collider2D layer and move Ground Check Offset to the pawn feet.",
                            "Assign Input Zone Set only when parts of the screen should block pawn movement.",
                            "Tune sprite radius and radius offset after the visual sprite/collider is in place."
                        }),
                    new PyralisGuideSection(
                        "Watch For",
                        null,
                        new[]
                        {
                            "Do not use this as a standalone mover; Motor2D and input/AI systems feed it.",
                            "If vertical input does nothing while Jump Enabled is on, that is expected side-view behavior. Disable Jump Enabled / Allow 2D Jump for top-down map movement.",
                            "If jump does nothing in side-view mode, check InputProfile > Jump Action, the Input Actions asset, Ground Layer, Ground Check Offset, and whether the Game view has focus.",
                            "Do not put visual background sprites on the ground layer unless they also have Collider2D and are meant to be walkable.",
                            "Do not leave Move Speed, Dash Speed, or Dash Cooldown at placeholder values after applying a movement profile.",
                            "Do not enable Screen Wrap for arena or room-based games unless wrapping is intentional."
                        })
                });

            DrawDefaultInspector();
            DrawRuntimeDiagnostics((Pawn2DMovementComponent)target);
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(root, serializedObject), "Pawn2DMovementComponent is ready for 2D pawn movement.");
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawRuntimeDiagnostics(Pawn2DMovementComponent movement)
        {
            if (movement == null || !Application.isPlaying)
                return;

            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Play Mode Movement Diagnostics", EditorStyles.miniBoldLabel);
                bool hasGameplayState = movement.TryGetRuntimeGameplayActive(out bool isGameplayActive);
                bool hasBounds = movement.TryGetRuntimeCameraBounds(out CameraBounds2D bounds);
                Object stateSource = movement.RuntimeGameplayStateSource;
                Object boundsSource = movement.RuntimeCameraBoundsSource;

                EditorGUILayout.LabelField("Gameplay State", hasGameplayState ? (isGameplayActive ? "Active" : "Resolved but not active") : "Missing");
                EditorGUILayout.LabelField("Gameplay Source", stateSource != null ? stateSource.name : "None");
                EditorGUILayout.LabelField("Camera Bounds", hasBounds ? $"Resolved ({bounds.HalfWidth:0.##} x {bounds.HalfHeight:0.##})" : "Missing or not orthographic");
                EditorGUILayout.LabelField("Bounds Source", boundsSource != null ? boundsSource.name : "None");
                EditorGUILayout.LabelField("Movement Enabled", movement.MovementEnabled ? "Yes" : "No");
                EditorGUILayout.LabelField("Jump Enabled", movement.JumpEnabled ? "Yes" : "No - top-down/free 2D");
                EditorGUILayout.LabelField("Grounded", movement.JumpEnabled ? (movement.RuntimeGrounded ? "Yes" : "No") : "Not used");
                EditorGUILayout.LabelField("Jump Queued", movement.JumpEnabled ? (movement.RuntimeJumpQueued ? "Yes" : "No") : "Not used");
                EditorGUILayout.LabelField("Move Direction", movement.MoveDirection.ToString("F2"));
                EditorGUILayout.LabelField("Current Velocity", movement.CurrentVelocity.ToString("F2"));
                EditorGUILayout.LabelField("Action Locked", movement.IsActionLocked ? "Yes" : "No");
            }
        }

        private static List<PyralisGuideIssue> GetMessages(GameObject root, SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty moveSpeed = serializedObject.FindProperty("moveSpeed");
            SerializedProperty dashEnabled = serializedObject.FindProperty("dashEnabled");
            SerializedProperty dashSpeed = serializedObject.FindProperty("dashSpeed");
            SerializedProperty dashCooldown = serializedObject.FindProperty("dashCooldown");
            SerializedProperty spriteRadius = serializedObject.FindProperty("spriteRadius");
            SerializedProperty jumpEnabled = serializedObject.FindProperty("jumpEnabled");
            SerializedProperty jumpVelocity = serializedObject.FindProperty("jumpVelocity");
            SerializedProperty gravityScale = serializedObject.FindProperty("gravityScale");
            SerializedProperty maxFallSpeed = serializedObject.FindProperty("maxFallSpeed");
            SerializedProperty groundCheckRadius = serializedObject.FindProperty("groundCheckRadius");
            SerializedProperty cameraBoundsSource = serializedObject.FindProperty("cameraBoundsSource");
            SerializedProperty targetCamera = serializedObject.FindProperty("targetCamera");
            SerializedProperty gameplayStateSource = serializedObject.FindProperty("gameplayStateSource");

            Pawn3DStackEditorUtility.RequireComponent<Motor2D>(messages, root, "Motor2D is missing. It is the shared motor surface most 2D pawn systems talk to.");
            Pawn3DStackEditorUtility.RequireComponent<Rigidbody2D>(messages, root, "Rigidbody2D is missing. Movement applies through a kinematic Rigidbody2D.");
            Pawn3DStackEditorUtility.RequireComponent<PolygonCollider2D>(messages, root, "PolygonCollider2D is missing. Add one before testing collision or pickup overlap behavior.");

            if (moveSpeed != null && moveSpeed.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Move Speed must be greater than zero."));

            if ((cameraBoundsSource == null || cameraBoundsSource.objectReferenceValue == null)
                && (targetCamera == null || targetCamera.objectReferenceValue == null))
                messages.Add(PyralisGuideIssue.Optional("Camera Bounds Source and Target Camera are empty. The scene session can provide camera bounds through Bootstrap > Camera Rig Controller; assign here only for prefab-local or custom service tests."));

            if (gameplayStateSource != null && gameplayStateSource.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Gameplay State Source is empty. GameManager can provide it at runtime; assign an IGameplayStateReader only for standalone prefab or custom service tests."));

            if (jumpEnabled != null && !jumpEnabled.boolValue)
                messages.Add(PyralisGuideIssue.Optional("Jump Enabled is off, so this movement component will not perform side-view Rigidbody2D jump. Space/Button South can still be handled by an installed feature module such as TopDownHopFeatureRuntime."));

            if (dashEnabled != null && dashEnabled.boolValue)
            {
                if (dashSpeed != null && dashSpeed.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("Dash Speed must be greater than zero when dash is enabled."));

                if (dashCooldown != null && dashCooldown.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("Dash Cooldown must be greater than zero when dash is enabled."));
            }

            if (spriteRadius != null && spriteRadius.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Recommended("Sprite Radius should be greater than zero so camera bounds match the pawn visual."));

            if (jumpEnabled != null && jumpEnabled.boolValue)
            {
                messages.Add(PyralisGuideIssue.Optional("Active 2D route: side-view/platformer. Move input uses X for horizontal movement; vertical motion comes from the Jump action and Rigidbody2D gravity."));

                if (jumpVelocity != null && jumpVelocity.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("Jump Velocity must be greater than zero when side-view jump is enabled."));

                if (gravityScale != null && gravityScale.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("Gravity Scale must be greater than zero when side-view jump is enabled."));

                if (maxFallSpeed != null && maxFallSpeed.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("Max Fall Speed must be greater than zero when side-view jump is enabled."));

                if (groundCheckRadius != null && groundCheckRadius.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("Ground Check Radius must be greater than zero when side-view jump is enabled."));
            }
            else
            {
                messages.Add(PyralisGuideIssue.Optional("Active 2D route: top-down/free 2D. Move input drives X/Y movement; the movement component does not perform side-view Rigidbody2D jump, but Jump can still trigger a feature module such as TopDownHopFeatureRuntime."));
            }

            return messages;
        }
    }

    [CustomEditor(typeof(Pawn2DPresentationComponent))]
    public sealed class Pawn2DPresentationComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GameObject root = ((Pawn2DPresentationComponent)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "2D Pawn Stack Field Guide: Presentation Component",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "Pawn2DPresentationComponent turns movement and dash/death state into sprite tinting, facing, squash/stretch, tilt, audio, and animation-driver signals.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                    new PyralisGuideSection(
                        "Required Context",
                        null,
                        new[]
                        {
                            "Keep this on the same root GameObject as Motor2D and Pawn2DMovementComponent.",
                            "Assign Sprite Renderer when the pawn visual is not on this root object.",
                            "For SpriteRenderer > Sprite, assign a texture imported as Sprite (2D and UI) or a visible Sprite subasset. If an Aseprite file's frames do not appear in the picker, export/select a static PNG frame or use the generated Aseprite prefab only when you are ready for Animator-driven presentation.",
                            "Use ActorAnimationDriver for Animator parameter mapping instead of hardcoding Animator fields here.",
                            "Assign Dash Clip and Death Clip only after the pawn behavior feels correct."
                        }),
                    new PyralisGuideSection(
                        "Watch For",
                        null,
                        new[]
                        {
                            "Do not use this without Pawn2DMovementComponent; presentation reads movement state from it.",
                            "Do not leave both moving and idle tint invisible.",
                            "Do not enable tilt or squash/stretch until the visual pivot is correct."
                        })
                });

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(root, serializedObject), "Pawn2DPresentationComponent is ready for 2D pawn visual feedback.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetMessages(GameObject root, SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty spriteRenderer = serializedObject.FindProperty("spriteRenderer");
            SerializedProperty stretchAmount = serializedObject.FindProperty("stretchAmount");
            SerializedProperty squashSnapSpeed = serializedObject.FindProperty("squashSnapSpeed");
            SerializedProperty tiltSpeed = serializedObject.FindProperty("tiltSpeed");

            Pawn3DStackEditorUtility.RequireComponent<Pawn2DMovementComponent>(messages, root, "Pawn2DMovementComponent is missing. Presentation needs movement state.");
            Pawn3DStackEditorUtility.RequireComponent<ActorAnimationDriver>(messages, root, "ActorAnimationDriver is missing. This component forwards animation signals through it.");

            if (spriteRenderer != null && spriteRenderer.objectReferenceValue == null
                && root.GetComponentInChildren<SpriteRenderer>(true) == null)
            {
                messages.Add(PyralisGuideIssue.Required("Sprite Renderer is empty and no child SpriteRenderer was found."));
            }

            if (stretchAmount != null && stretchAmount.floatValue < 1f)
                messages.Add(PyralisGuideIssue.Required("Stretch Amount should be at least 1."));

            if (squashSnapSpeed != null && squashSnapSpeed.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Recommended("Squash Snap Speed should be greater than zero when squash/stretch is enabled."));

            if (tiltSpeed != null && tiltSpeed.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Recommended("Tilt Speed should be greater than zero when tilt is enabled."));

            return messages;
        }
    }

    [CustomEditor(typeof(PawnCombatBehaviour2D))]
    public sealed class PawnCombatBehaviour2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GameObject root = ((PawnCombatBehaviour2D)target).gameObject;

            Pawn2DStackEditorUtility.Draw2DCombatFieldGuide("2D Pawn Stack Field Guide: Combat Behaviour 2D");
            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(Pawn2DStackEditorUtility.Get2DCombatMessages(root, serializedObject), "PawnCombatBehaviour2D is ready for 2D attack input.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(PawnCombatBehaviour))]
    public sealed class PawnCombatBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GameObject root = ((PawnCombatBehaviour)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "Pawn Stack Field Guide: Combat Behaviour",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "PawnCombatBehaviour is the shared pawn combat module for grounded, aerial, blocking, hitbox, weapon, combo, and projectile behavior.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")),
                    new PyralisGuideSection(
                        "Required Fields",
                        null,
                        new[]
                        {
                            "Use this on pawns with a motor that implements ICharacterMotorState.",
                            "Assign HitBox zones for melee attacks, or WeaponData with ProjectileDefinition and ProjectileLauncher3D for ranged attacks.",
                            "Assign CombatSequenceDefinition assets when combos should be authored as data.",
                            "Use ActorAnimationDriver when attacks, block, aerial, and combo confirmation should trigger Animator parameters."
                        }),
                    new PyralisGuideSection(
                        "Watch For",
                        null,
                        new[]
                        {
                            "For 2D-only combat, prefer PawnCombatBehaviour2D unless this pawn intentionally uses the shared combat module.",
                            "Do not leave hitbox zone names mismatched with WeaponData or CombatActionDefinition fallback zones.",
                            "Do not enable blocking here if an Actor Guard feature owns guard behavior on the same pawn."
                        })
                });

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(Pawn2DStackEditorUtility.GetSharedCombatMessages(root, serializedObject), "PawnCombatBehaviour is ready for shared pawn combat.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(Motor2DInputAdapter))]
    public sealed class Motor2DInputAdapterEditor : PlayerInputHandlerEditor
    {
    }

    [CustomEditor(typeof(ActorGuardInputBridge2D))]
    public sealed class ActorGuardInputBridge2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GameObject root = ((ActorGuardInputBridge2D)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "2D Pawn Stack Field Guide: Guard Input Bridge",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "ActorGuardInputBridge2D forwards 2D guard input into an installed Actor Guard feature on ActorFeatureHost.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")),
                    new PyralisGuideSection(
                        "Required Fields",
                        null,
                        new[]
                        {
                            "Add ActorFeatureHost to the same GameObject.",
                            "Install or configure a feature module that provides IActorGuardFeature.",
                            "Route input from PlayerInputHandler, Motor2DInputAdapter, or another control surface into this bridge."
                        }),
                    new PyralisGuideSection(
                        "Watch For",
                        null,
                        new[]
                        {
                            "Do not add this bridge without an ActorFeatureHost.",
                            "Do not expect this to block damage by itself; it only forwards guard input to the feature.",
                            "Do not also rely on PawnCombatBehaviour blocking unless that is an intentional shared-combat path."
                        })
                });

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(Pawn2DStackEditorUtility.GetFeatureBridgeMessages(root, "IActorGuardFeature"), "Guard input bridge is ready to forward guard input.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ActorInteractionInputBridge2D))]
    public sealed class ActorInteractionInputBridge2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GameObject root = ((ActorInteractionInputBridge2D)target).gameObject;

            PyralisInspectorGuide.DrawFieldGuide(
                title: "2D Pawn Stack Field Guide: Interaction Input Bridge",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "ActorInteractionInputBridge2D forwards interact input into an installed Actor Interaction feature on ActorFeatureHost.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")),
                    new PyralisGuideSection(
                        "Required Fields",
                        null,
                        new[]
                        {
                            "Add ActorFeatureHost to the same GameObject.",
                            "Install or configure a feature module that provides IActorInteractionFeature.",
                            "Route interaction input from the pawn input surface into this bridge."
                        }),
                    new PyralisGuideSection(
                        "Watch For",
                        null,
                        new[]
                        {
                            "Do not add this bridge without an ActorFeatureHost.",
                            "Do not expect this to find interactables unless the interaction feature is installed.",
                            "Do not wire menu button clicks here unless the menu is intentionally controlling a pawn interaction feature."
                        })
                });

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(Pawn2DStackEditorUtility.GetFeatureBridgeMessages(root, "IActorInteractionFeature"), "Interaction input bridge is ready to forward interact input.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ActorAnimationDriver))]
    public sealed class ActorAnimationDriverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                title: "Pawn Presentation Field Guide: Actor Animation Driver",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "ActorAnimationDriver maps shared gameplay signals to the Animator Controller your pawn visual equips, plus sprite facing, billboard facing, and shadow/profile defaults.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                    new PyralisGuideSection(
                        "Required Fields",
                        null,
                        new[]
                        {
                            "Assign Animator or place one on a child visual.",
                            "Assign Presentation Profile for sprite, billboard, or rigged 3D presentation defaults.",
                            "Assign Animation Profile with bindings from ActorAnimationSignal to the parameter names in that Animator Controller.",
                            "Assign Visual Root or Billboard Target when the rendered object is not the pawn root.",
                            "Assign Camera Override when using Billboard2_5D presentation, or call SetCameraOverride from your spawn/bootstrap code."
                        }),
                    new PyralisGuideSection(
                        "Watch For",
                        null,
                        new[]
                        {
                            "Do not put raw Animator parameter names into movement or combat components; bind them in the animation profile.",
                            "Do not rename an imported controller just to satisfy Pyralis; update PawnAnimationProfile bindings instead.",
                            "Do not use billboard facing for rigged 3D presentation.",
                            "Do not leave Animator empty once animation feedback is expected."
                        })
                });

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(Pawn2DStackEditorUtility.GetAnimationMessages(serializedObject, ((ActorAnimationDriver)target).gameObject), "ActorAnimationDriver is ready for presentation signals.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    internal static class Pawn2DStackEditorUtility
    {
        public static void Draw2DCombatFieldGuide(string title)
        {
            PyralisInspectorGuide.DrawFieldGuide(
                title: title,
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "2D pawn combat receives attack input, resolves combo definitions or authored attacks, activates HitBox2D zones, and fires WeaponData projectiles.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")),
                    new PyralisGuideSection(
                        "Required Fields",
                        null,
                        new[]
                        {
                            "Keep this on the same root GameObject as Motor2D.",
                            "Assign HitBox2D zones for melee attacks, or WeaponData with ProjectileDefinition and ProjectileLauncher2D for ranged attacks.",
                            "Assign Primary and Secondary CombatSequenceDefinition assets when combos should be authored as data.",
                            "Assign Projectile Launcher and Projectile Spawn Point when ranged weapons should fire from a hand, muzzle, or socket."
                        }),
                    new PyralisGuideSection(
                        "Watch For",
                        null,
                        new[]
                        {
                            "Do not leave hitbox zone names mismatched with WeaponData or CombatActionDefinition fallback zones.",
                            "Do not expect this component to read input directly; an input handler or AI must call it.",
                            "Do not skip HealthComponent when projectile ownership, factions, or damage feedback matter."
                        })
                });
        }

        public static List<PyralisGuideIssue> Get2DCombatMessages(GameObject root, SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty hitBoxZones = serializedObject.FindProperty("hitBoxZones");
            SerializedProperty equippedWeapons = serializedObject.FindProperty("equippedWeapons");
            SerializedProperty startingWeaponIndex = serializedObject.FindProperty("startingWeaponIndex");
            SerializedProperty attackCooldown = serializedObject.FindProperty("attackCooldown");
            SerializedProperty kickCooldown = serializedObject.FindProperty("kickCooldown");
            SerializedProperty projectileLauncher = serializedObject.FindProperty("projectileLauncher");

            Pawn3DStackEditorUtility.RequireComponent<Motor2D>(messages, root, "Motor2D is missing. PawnCombatBehaviour2D locks and reads the 2D motor.");

            if (root.GetComponent<ActorAnimationDriver>() == null)
                messages.Add(PyralisGuideIssue.Recommended("ActorAnimationDriver is missing. Add it when attacks should trigger Animator signals."));

            if (root.GetComponent<HealthComponent>() == null)
                messages.Add(PyralisGuideIssue.Optional("HealthComponent is empty. Add it when factions, owner damage rules, or death reactions matter."));

            if (hitBoxZones != null && hitBoxZones.arraySize == 0)
                messages.Add(PyralisGuideIssue.Recommended("Hit Box Zones is empty. This is fine for projectile-only combat; melee attacks need HitBox2D zones."));

            if (projectileLauncher != null && projectileLauncher.objectReferenceValue == null && root.GetComponentInParent<ProjectileLauncher2D>() == null)
                messages.Add(PyralisGuideIssue.Recommended("Projectile Launcher is empty. Ranged/thrown weapons need ProjectileLauncher2D to use the authored projectile path."));

            if (equippedWeapons != null && startingWeaponIndex != null
                && equippedWeapons.arraySize > 0 && startingWeaponIndex.intValue >= equippedWeapons.arraySize)
            {
                messages.Add(PyralisGuideIssue.Required("Starting Weapon Index is outside the Equipped Weapons array."));
            }

            if (attackCooldown != null && attackCooldown.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Attack Cooldown cannot be negative."));

            if (kickCooldown != null && kickCooldown.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Kick Cooldown cannot be negative."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetSharedCombatMessages(GameObject root, SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty hitBoxZones = serializedObject.FindProperty("hitBoxZones");
            SerializedProperty equippedWeapons = serializedObject.FindProperty("equippedWeapons");
            SerializedProperty startingWeaponIndex = serializedObject.FindProperty("startingWeaponIndex");
            SerializedProperty maxAerialAttacks = serializedObject.FindProperty("maxAerialAttacks");
            SerializedProperty blockDamageReduction = serializedObject.FindProperty("blockDamageReduction");
            SerializedProperty projectileLauncher = serializedObject.FindProperty("projectileLauncher");

            if (root.GetComponent<ActorAnimationDriver>() == null)
                messages.Add(PyralisGuideIssue.Recommended("ActorAnimationDriver is missing. Add it when attacks, block, aerial, or combo signals should animate."));

            if (root.GetComponent<HealthComponent>() == null)
                messages.Add(PyralisGuideIssue.Optional("HealthComponent is empty. Add it when factions, owner damage rules, or death reactions matter."));

            if (hitBoxZones != null && hitBoxZones.arraySize == 0)
                messages.Add(PyralisGuideIssue.Recommended("Hit Box Zones is empty. This is fine for projectile-only combat; melee attacks need HitBox zones."));

            if (projectileLauncher != null && projectileLauncher.objectReferenceValue == null && root.GetComponentInParent<ProjectileLauncher3D>() == null)
                messages.Add(PyralisGuideIssue.Recommended("Projectile Launcher is empty. Ranged/thrown weapons need ProjectileLauncher3D to use the authored projectile path."));

            if (equippedWeapons != null && startingWeaponIndex != null
                && equippedWeapons.arraySize > 0 && startingWeaponIndex.intValue >= equippedWeapons.arraySize)
            {
                messages.Add(PyralisGuideIssue.Required("Starting Weapon Index is outside the Equipped Weapons array."));
            }

            if (maxAerialAttacks != null && maxAerialAttacks.intValue < 0)
                messages.Add(PyralisGuideIssue.Required("Max Aerial Attacks cannot be negative."));

            if (blockDamageReduction != null && Mathf.Approximately(blockDamageReduction.floatValue, 1f))
                messages.Add(PyralisGuideIssue.Recommended("Block Damage Reduction is 1, so blocking will not reduce damage."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetFeatureBridgeMessages(GameObject root, string expectedFeature)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (root.GetComponent("ActorFeatureHost") == null)
                messages.Add(PyralisGuideIssue.Required("ActorFeatureHost is missing. Feature input bridges need it to find installed actor features."));

            messages.Add(PyralisGuideIssue.Optional("Confirm the assigned FeatureModuleDefinition installs " + expectedFeature + " at runtime."));
            return messages;
        }

        public static List<PyralisGuideIssue> GetAnimationMessages(SerializedObject serializedObject, GameObject root)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty animator = serializedObject.FindProperty("animator");
            SerializedProperty presentationProfile = serializedObject.FindProperty("presentationProfile");
            SerializedProperty animationProfile = serializedObject.FindProperty("animationProfile");
            SerializedProperty cameraOverride = serializedObject.FindProperty("cameraOverride");

            if (animator != null && animator.objectReferenceValue == null
                && root.GetComponentInChildren<Animator>(true) == null)
            {
                messages.Add(PyralisGuideIssue.Recommended("Animator is empty and no child Animator was found. Animation signals will be ignored until one exists."));
            }

            if (presentationProfile != null && presentationProfile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Presentation Profile is empty. Add one for sprite, billboard, rigged, tint, and facing defaults."));

            if (animationProfile != null && animationProfile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Animation Profile is empty. Add one when gameplay signals should drive Animator parameters."));

            if (presentationProfile != null
                && presentationProfile.objectReferenceValue is PawnPresentationProfile profile
                && profile.presentationMode == ActorPresentationMode.Billboard2_5D
                && cameraOverride != null
                && cameraOverride.objectReferenceValue == null)
            {
                messages.Add(PyralisGuideIssue.Recommended("Camera Override is empty for Billboard2_5D presentation. Assign the gameplay camera or call SetCameraOverride at spawn time."));
            }

            return messages;
        }
    }
}
