using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Zones;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Presentation.Visuals;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(CinemachineCameraRigController))]
    public class CinemachineCameraRigControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Cinemachine Camera Rig Controller",
                defaultOpen: false,
                new PyralisGuideSection(
                    "What This Is",
                    "CinemachineCameraRigController is the Pyralis scene camera runtime. Use this Inspector for assigned references and tuning values; use the Pyralis Authoring Window for setup order, route diagnosis, and first-proof guidance.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Camera_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Camera Rig Profile chooses shared/split camera behavior, zoom, orthographic mode, and playfield locking.",
                        "Shared Camera Behaviour references the CinemachineCamera that composes the view.",
                        "Target Camera references the physical Unity Camera that renders the view, usually the scene Main Camera with Cinemachine Brain.",
                        "For shared camera mode, the runtime follow target is not hand-assigned to Rico in Edit Mode. GameplaySessionBootstrap spawns participants, the rig creates GameplaySharedCameraFocus, then the Cinemachine Camera follows that focus.",
                        "A normal Cinemachine route keeps or creates one physical Unity Camera, usually the default Main Camera, then adds separate Cinemachine Camera GameObjects that control it.",
                        "A normal shared-camera proof should have one enabled physical render Camera plus one Cinemachine Camera. Disable or remove accidental extra physical Camera objects only when they were created by mistake; keep intentional split screen, overlay, minimap, or render-texture cameras.",
                        "For 2D movement or bounded views, the Target Camera Projection or assigned CameraRigProfile must be Orthographic.",
                        "Leave the Cinemachine Camera's Tracking Target empty in Edit Mode for the shared runtime route. This rig creates a shared focus target at runtime after the participant roster has spawned pawns.",
                        "For no-lag shared follow, set Camera Rig Profile > Follow Damping to 0. Use Profile Transform then applies Follow Offset and View Euler Angles to the shared Cinemachine camera at runtime.",
                        "2D Bounds Framing values define the minimum visible world area when this rig is used as an ICameraBoundsProvider."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Choose what the camera follows through Presentation Mode: Shared follows the participant group, SplitScreen follows each participant with separate Cinemachine cameras.",
                        "Tune the shot shape in three places: CameraRigProfile for shared behavior, no-lag or smooth follow, offset, pitch/yaw/roll, zoom, and damping; Camera Root for 2D bounds and scroll zoom; Cinemachine Camera / physical Target Camera transforms for hand-authored composition when Use Profile Transform is disabled.",
                        "For a 2D top-down proof, use CameraRigProfile > Use 2D Orthographic Defaults, then adjust Orthographic Size and Camera Root > 2D Bounds Framing. In Edit Mode and Play Mode, the rig previews the profile size, then Enforce Minimum Visible Area 2D can raise the effective orthographic size above the profile value.",
                        "For an angled 3D or 2.5D proof, use Follow Offset plus View Euler Angles, or disable Use Profile Transform and rotate/position the Cinemachine Camera directly.",
                        "If Target Camera is missing Cinemachine Brain, fix that on the camera Inspector, then return here to keep the reference assigned.",
                        "If the camera does not follow in Play Mode, inspect this rig first: Camera Rig Profile, Participant Roster, Shared Camera Behaviour, and Target Camera should all resolve before tuning composition."
                    }));

            DrawDefaultInspector();
            DrawRuntimeDiagnostics((CinemachineCameraRigController)target);
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(), "Cinemachine camera rig is ready for participant framing.");
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawRuntimeDiagnostics(CinemachineCameraRigController rig)
        {
            if (rig == null || !Application.isPlaying)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Play Mode Camera Diagnostics", EditorStyles.miniBoldLabel);
                Object sharedBehaviour = rig.RuntimeSharedCameraBehaviour;
                Camera targetCamera = rig.RuntimeTargetCamera as Camera;
                Transform focusTarget = rig.RuntimeSharedFocusTarget;

                EditorGUILayout.LabelField("Participant Count", rig.RuntimeParticipantCount.ToString());
                EditorGUILayout.LabelField("Runtime Follow Target", focusTarget != null ? focusTarget.name : "None yet");
                EditorGUILayout.LabelField("Follow Position", focusTarget != null ? focusTarget.position.ToString("F2") : "None");
                EditorGUILayout.LabelField("Follow Damping", rig.RuntimeFollowDamping <= 0f ? "0 (snap / no lag)" : rig.RuntimeFollowDamping.ToString("0.##"));
                EditorGUILayout.LabelField("Profile Transform", rig.RuntimeUsingProfileTransform ? "On" : "Off (scene transform authored)");
                EditorGUILayout.LabelField("Profile Offset", rig.RuntimeFollowOffset.ToString("F2"));
                EditorGUILayout.LabelField("Profile Pitch/Yaw/Roll", rig.RuntimeViewEulerAngles.ToString("F1"));
                EditorGUILayout.LabelField("Shared Cinemachine Camera", sharedBehaviour != null ? sharedBehaviour.name : "None");
                EditorGUILayout.LabelField("Physical Target Camera", targetCamera != null ? targetCamera.name : "None");
                EditorGUILayout.LabelField("Profile Orthographic Size", rig.RuntimeProfileOrthographicSize > 0f ? rig.RuntimeProfileOrthographicSize.ToString("0.##") : "No profile");
                if (rig.RuntimeSharedCinemachineOrthographicSize > 0f)
                    EditorGUILayout.LabelField("Cinemachine Lens Size", rig.RuntimeSharedCinemachineOrthographicSize.ToString("0.##"));
                if (targetCamera != null)
                {
                    EditorGUILayout.LabelField("Projection", targetCamera.orthographic ? "Orthographic" : "Perspective");
                    EditorGUILayout.LabelField("Physical Camera Size", targetCamera.orthographicSize.ToString("0.##"));
                    if (rig.RuntimeEnforceMinimumVisibleArea2D)
                    {
                        EditorGUILayout.LabelField("2D Min Visible Size", rig.RuntimeMinimumOrthographicSize2D.ToString("0.##"));
                        EditorGUILayout.LabelField("2D Size Clamp", rig.RuntimeOrthographicSizeClampedBy2DVisibleArea ? "Active" : "Not active");
                    }
                    EditorGUILayout.LabelField("Camera Transform", $"{targetCamera.transform.position.ToString("F2")} / {targetCamera.transform.rotation.eulerAngles.ToString("F1")}");
                }

                if (sharedBehaviour is CinemachineVirtualCameraBase sharedVcam)
                {
                    EditorGUILayout.LabelField("Cinemachine Follow", sharedVcam.Follow != null ? sharedVcam.Follow.name : "None");
                    EditorGUILayout.LabelField("Cinemachine Look At", sharedVcam.LookAt != null ? sharedVcam.LookAt.name : "None");
                }
            }
        }

        private List<PyralisGuideIssue> GetMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty profile = serializedObject.FindProperty("cameraRigProfile");
            SerializedProperty sharedCamera = serializedObject.FindProperty("sharedCameraBehaviour");
            SerializedProperty targetCamera = serializedObject.FindProperty("targetCamera");
            SerializedProperty participantRoster = serializedObject.FindProperty("participantRoster");
            SerializedProperty minWidth2D = serializedObject.FindProperty("minWorldWidth2D");
            SerializedProperty minHeight2D = serializedObject.FindProperty("minWorldHeight2D");
            SerializedProperty enforceMinimumVisibleArea2D = serializedObject.FindProperty("enforceMinimumVisibleArea2D");
            SerializedProperty portraitMinWidth2D = serializedObject.FindProperty("portraitMinWorldWidth2D");
            SerializedProperty portraitMinHeight2D = serializedObject.FindProperty("portraitMinWorldHeight2D");
            SerializedProperty letterbox2D = serializedObject.FindProperty("letterbox2D");
            SerializedProperty allowScrollZoom = serializedObject.FindProperty("allowScrollZoom");
            SerializedProperty scrollZoomSpeed = serializedObject.FindProperty("scrollZoomSpeed");

            CameraRigProfile assignedProfile = profile != null ? profile.objectReferenceValue as CameraRigProfile : null;
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Required("Camera Rig Profile is required for a manually authored camera rig."));
            else if (assignedProfile != null)
            {
                Camera cameraFor2D = targetCamera != null ? targetCamera.objectReferenceValue as Camera : null;
                if (!assignedProfile.orthographic && cameraFor2D != null && !cameraFor2D.orthographic)
                    messages.Add(PyralisGuideIssue.Recommended("2D pawn movement needs orthographic camera bounds. For a 2D proof, make this Camera Rig Profile orthographic or select the physical Target Camera and set Camera > Projection to Orthographic."));
                else if (!assignedProfile.orthographic && cameraFor2D != null && cameraFor2D.orthographic)
                    messages.Add(PyralisGuideIssue.Recommended("Camera Rig Profile is Perspective while the Target Camera is Orthographic. At runtime this profile can switch the Target Camera back to perspective, so make the profile Orthographic for a 2D movement proof."));
            }

            if (sharedCamera != null && sharedCamera.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Shared Camera Behaviour is empty. Assign the Cinemachine camera for shared-camera modes."));
            else if (sharedCamera.objectReferenceValue is MonoBehaviour sharedBehaviour)
            {
                if (sharedBehaviour is not CinemachineVirtualCameraBase)
                    messages.Add(PyralisGuideIssue.Required("Shared Camera Behaviour should be the Cinemachine Camera / virtual camera component, not the physical Unity Camera or Cinemachine Brain."));

                if (sharedBehaviour.CompareTag("MainCamera"))
                    messages.Add(PyralisGuideIssue.Recommended("The Cinemachine Camera should usually stay untagged. Keep the MainCamera tag on the physical Target Camera that has Cinemachine Brain."));

                if (sharedBehaviour is CinemachineVirtualCameraBase sharedVcam
                    && sharedVcam.Follow == null
                    && sharedVcam.LookAt == null)
                {
                    messages.Add(PyralisGuideIssue.Optional("The Cinemachine Camera has no Tracking Target in Edit Mode. This is expected for the Pyralis shared-camera route; the rig assigns a runtime focus target from the participant roster after Play starts."));
                }
            }

            if (participantRoster != null && participantRoster.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Participant Roster is empty. GameplaySessionBootstrap / PyralisGameplayLifetimeScope can inject it at runtime; if the camera still does not follow in Play Mode, assign or inspect the scene ParticipantRosterService."));

            if (targetCamera != null && targetCamera.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Target Camera is empty. Assign the physical Unity Camera that renders the view, or place that Camera under this rig object."));
            else if (targetCamera != null && targetCamera.objectReferenceValue is Camera camera
                && camera.GetComponent<CinemachineBrain>() == null)
            {
                messages.Add(PyralisGuideIssue.Required("Target Camera is missing Cinemachine Brain. Select the physical camera, use Inspector > Add Component, add Cinemachine Brain, then keep this Target Camera assignment."));
            }
            else if (targetCamera != null && targetCamera.objectReferenceValue is Camera taggedCamera
                && !taggedCamera.CompareTag("MainCamera"))
            {
                messages.Add(PyralisGuideIssue.Recommended("Target Camera is not tagged MainCamera. Pyralis uses explicit references, but Unity camera/audio/editor conventions are clearer when the physical render camera keeps the MainCamera tag."));
            }

            bool sharedPresentation = assignedProfile == null
                || assignedProfile.presentationMode == CameraRigProfile.CameraPresentationMode.Shared;
            if (sharedPresentation && CountEnabledSceneCameras() > 1)
            {
                messages.Add(PyralisGuideIssue.Optional("Multiple enabled physical Camera components are present. For a shared-camera proof, keep one render camera (usually Main Camera with Cinemachine Brain) plus one Cinemachine Camera for composition; disable or remove accidental extra physical Camera objects only when they were created by mistake; keep intentional split-screen, overlay, minimap, or render-texture cameras."));
            }

            if (enforceMinimumVisibleArea2D != null && enforceMinimumVisibleArea2D.boolValue)
            {
                if (minWidth2D != null && minWidth2D.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("2D Min World Width must be greater than zero."));

                if (minHeight2D != null && minHeight2D.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("2D Min World Height must be greater than zero."));

                if (assignedProfile != null && assignedProfile.orthographic)
                {
                    float requiredSize = EstimateMinimumOrthographicSize2D(
                        minWidth2D,
                        minHeight2D,
                        portraitMinWidth2D,
                        portraitMinHeight2D,
                        letterbox2D);
                    if (requiredSize > assignedProfile.orthographicSize + 0.01f)
                    {
                        messages.Add(PyralisGuideIssue.Recommended($"2D Bounds Framing will raise Orthographic Size to at least {requiredSize:0.##} in this Game view. Lower Min World Width/Height or turn off Enforce Minimum Visible Area 2D if you want CameraRigProfile > Orthographic Size ({assignedProfile.orthographicSize:0.##}) to zoom closer."));
                    }
                }
            }

            if (allowScrollZoom != null && allowScrollZoom.boolValue
                && scrollZoomSpeed != null && scrollZoomSpeed.floatValue <= 0f)
            {
                messages.Add(PyralisGuideIssue.Required("Scroll Zoom Speed must be greater than zero when scroll zoom is enabled."));
            }

            return messages;
        }

        private static int CountEnabledSceneCameras()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
            int count = 0;
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera != null && camera.enabled && camera.gameObject.activeInHierarchy)
                    count++;
            }

            return count;
        }

        private static float EstimateMinimumOrthographicSize2D(
            SerializedProperty minWidth2D,
            SerializedProperty minHeight2D,
            SerializedProperty portraitMinWidth2D,
            SerializedProperty portraitMinHeight2D,
            SerializedProperty letterbox2D)
        {
            float minWidth = minWidth2D != null ? minWidth2D.floatValue : 0f;
            float minHeight = minHeight2D != null ? minHeight2D.floatValue : 0f;
            bool isPortrait = Screen.height >= Screen.width;
            if (isPortrait
                && portraitMinWidth2D != null
                && portraitMinHeight2D != null
                && portraitMinWidth2D.floatValue > 0f
                && portraitMinHeight2D.floatValue > 0f)
            {
                minWidth = portraitMinWidth2D.floatValue;
                minHeight = portraitMinHeight2D.floatValue;
            }

            if (letterbox2D != null && letterbox2D.boolValue)
                return Mathf.Max(0f, minHeight * 0.5f);

            float screenAspect = Mathf.Max(0.01f, (float)Screen.width / Mathf.Max(1, Screen.height));
            return Mathf.Max(Mathf.Max(0f, minHeight) * 0.5f, (Mathf.Max(0f, minWidth) * 0.5f) / screenAspect);
        }
    }

    [CustomEditor(typeof(CameraOcclusionFader))]
    public class CameraOcclusionFaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Camera Occlusion Fader",
                defaultOpen: false,
                new PyralisGuideSection(
                    "What This Is",
                    "CameraOcclusionFader fades renderers that block the line of sight between the camera and a tracked target. Use this Inspector for target, mask, fade, and material-tuning fields.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Camera_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Target may be assigned directly, or set by runtime code after the player spawns.",
                        "Limit Occlusion Mask to world geometry layers.",
                        "Tune Fade Alpha and Fade Distance after the camera distance is final."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Keep the player layer out of Occlusion Mask.",
                        "Use this for 3D line-of-sight fading; 2D sprite visibility is usually sorting layers or renderer order."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(), "Camera occlusion fader is ready for 3D line-of-sight fading.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty targetTransform = serializedObject.FindProperty("target");
            SerializedProperty fadeDistance = serializedObject.FindProperty("fadeDistance");
            SerializedProperty occlusionMask = serializedObject.FindProperty("occlusionMask");

            if (targetTransform != null && targetTransform.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Target is empty. Assign it here or call SetTarget when the player spawns."));

            if (fadeDistance != null && fadeDistance.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Fade Distance must be greater than zero."));

            if (occlusionMask != null && occlusionMask.intValue == 0)
                messages.Add(PyralisGuideIssue.Recommended("Occlusion Mask is empty, so no geometry will be faded."));

            return messages;
        }
    }

    [CustomEditor(typeof(CameraShake))]
    public class CameraShakeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Camera Shake",
                defaultOpen: false,
                new PyralisGuideSection(
                    "What This Is",
                    "CameraShake is the shared gameplay impact feedback service. Use this Inspector for the target transform, shake axis, and intensity fields.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Camera_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Target Transform may reference the camera rig root, physical Camera transform, or another object that should receive shake.",
                        "Leaving Target Transform empty shakes this component's own Transform.",
                        "Position Influence and Rotation Influence decide whether shake is visible."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "2D path: use Planar2D and mostly position influence.",
                        "3D path: use Spatial3D or PositionAndRotation with lower intensity.",
                        "UI/camera-only path: use RotationOnly when world displacement feels too disruptive."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(), "CameraShake is ready for impact feedback calls.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty targetTransform = serializedObject.FindProperty("targetTransform");
            SerializedProperty positionInfluence = serializedObject.FindProperty("positionInfluence");
            SerializedProperty rotationInfluence = serializedObject.FindProperty("rotationInfluence");

            if (targetTransform != null && targetTransform.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Target Transform is empty. Runtime will shake this component's own Transform."));

            if (positionInfluence != null && rotationInfluence != null
                && positionInfluence.floatValue <= 0f && rotationInfluence.floatValue <= 0f)
            {
                messages.Add(PyralisGuideIssue.Recommended("Position Influence and Rotation Influence are both zero, so shake calls will not be visible."));
            }

            return messages;
        }
    }

    [CustomEditor(typeof(CameraZone))]
    public class CameraZoneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Camera Zone",
                defaultOpen: false,
                new PyralisGuideSection(
                    "What This Is",
                    "CameraZone is a 3D trigger volume that switches CameraRigProfile when the player enters. Use this Inspector for zone profile references and trigger tuning.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Camera_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign On Enter Profile to the camera framing this zone should activate.",
                        "Assign Camera Rig Controller manually, or let dependency injection provide it.",
                        "Set Player Tag to the tag used by entering pawn objects."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Combat arena path: enter a tighter profile and exit back to the default profile.",
                        "Cutscene path: enable One Shot and leave On Exit Profile empty.",
                        "Exploration path: use wider profiles for overlooks, rooms, or large platforming spaces."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(), "CameraZone is ready for trigger-based camera profile switching.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty cameraRig = serializedObject.FindProperty("cameraRigController");
            SerializedProperty enterProfile = serializedObject.FindProperty("onEnterProfile");
            SerializedProperty transitionDuration = serializedObject.FindProperty("transitionDuration");
            SerializedProperty playerTag = serializedObject.FindProperty("_playerTag");
            BoxCollider box = ((Component)target).GetComponent<BoxCollider>();

            if (box == null)
                messages.Add(PyralisGuideIssue.Required("A BoxCollider is required for the trigger volume."));
            else if (!box.isTrigger)
                messages.Add(PyralisGuideIssue.Recommended("BoxCollider should be set to Is Trigger for camera-zone entry detection."));

            if (enterProfile != null && enterProfile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Required("On Enter Profile is required before the zone can switch camera framing."));

            if (cameraRig != null && cameraRig.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Camera Rig Controller is empty. Dependency injection can fill this at runtime if the scene has the camera rig service."));

            if (transitionDuration != null && transitionDuration.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Transition Duration cannot be negative."));

            if (playerTag != null && string.IsNullOrWhiteSpace(playerTag.stringValue))
                messages.Add(PyralisGuideIssue.Required("Player Tag is required so the zone knows which trigger entries count."));

            return messages;
        }
    }
}
