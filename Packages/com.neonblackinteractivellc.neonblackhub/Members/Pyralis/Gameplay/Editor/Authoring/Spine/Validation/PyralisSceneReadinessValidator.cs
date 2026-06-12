using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Combat;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public enum PyralisSceneReadinessSeverity
    {
        RequiredBeforePlay,
        RecommendedBeforePlay,
        ProofEnhancer
    }

    public enum PyralisSceneReadinessCategory
    {
        SceneRoot,
        CameraAudio,
        Input,
        UserInterface,
        Presentation,
        Physics,
        PrefabContract,
        Networking,
        Other
    }

    public sealed class PyralisSceneReadinessIssue
    {
        public PyralisSceneReadinessIssue(
            string message,
            PyralisSceneReadinessSeverity severity,
            PyralisSceneReadinessCategory category,
            string nativeAction = "")
        {
            Message = message ?? string.Empty;
            Severity = severity;
            Category = category;
            NativeAction = nativeAction ?? string.Empty;
        }

        public string Message { get; }
        public PyralisSceneReadinessSeverity Severity { get; }
        public PyralisSceneReadinessCategory Category { get; }
        public string NativeAction { get; }
    }

    public sealed class PyralisSceneReadinessReport
    {
        private readonly List<string> _requiredIssues;
        private readonly List<string> _recommendedIssues;
        private readonly List<PyralisSceneReadinessIssue> _issues;

        public PyralisSceneReadinessReport(IEnumerable<string> requiredIssues, IEnumerable<string> recommendedIssues)
        {
            _requiredIssues = new List<string>(requiredIssues ?? System.Array.Empty<string>());
            _recommendedIssues = new List<string>(recommendedIssues ?? System.Array.Empty<string>());
            _issues = BuildIssues(_requiredIssues, _recommendedIssues);
        }

        public IReadOnlyList<string> RequiredIssues => _requiredIssues;
        public IReadOnlyList<string> RecommendedIssues => _recommendedIssues;
        public IReadOnlyList<PyralisSceneReadinessIssue> Issues => _issues;
        public bool IsReady => _requiredIssues.Count == 0;
        public bool HasRecommendations => _recommendedIssues.Count > 0;
        public string RequiredSummary => BuildSummary(_requiredIssues);
        public string RecommendedSummary => BuildSummary(_recommendedIssues);
        public string RequiredBeforePlaySummary => BuildSummary(GetMessages(PyralisSceneReadinessSeverity.RequiredBeforePlay));
        public string RecommendedBeforePlaySummary => BuildSummary(GetMessages(PyralisSceneReadinessSeverity.RecommendedBeforePlay));
        public string ProofEnhancerSummary => BuildSummary(GetMessages(PyralisSceneReadinessSeverity.ProofEnhancer));

        public IReadOnlyList<PyralisSceneReadinessIssue> GetIssues(PyralisSceneReadinessSeverity severity)
        {
            List<PyralisSceneReadinessIssue> issues = new List<PyralisSceneReadinessIssue>();
            for (int i = 0; i < _issues.Count; i++)
            {
                if (_issues[i].Severity == severity)
                    issues.Add(_issues[i]);
            }

            return issues;
        }

        private static string BuildSummary(IReadOnlyList<string> issues)
        {
            if (issues == null || issues.Count == 0)
                return string.Empty;

            int maxVisibleIssues = Mathf.Min(issues.Count, 5);
            List<string> visibleIssues = new List<string>(maxVisibleIssues);
            for (int i = 0; i < maxVisibleIssues; i++)
                visibleIssues.Add(issues[i]);

            string summary = string.Join("; ", visibleIssues);
            if (issues.Count > maxVisibleIssues)
                summary += " +" + (issues.Count - maxVisibleIssues) + " more";

            return summary;
        }

        private IReadOnlyList<string> GetMessages(PyralisSceneReadinessSeverity severity)
        {
            List<string> messages = new List<string>();
            for (int i = 0; i < _issues.Count; i++)
            {
                PyralisSceneReadinessIssue issue = _issues[i];
                if (issue.Severity == severity)
                    messages.Add(issue.Message);
            }

            return messages;
        }

        private static List<PyralisSceneReadinessIssue> BuildIssues(
            IReadOnlyList<string> requiredIssues,
            IReadOnlyList<string> recommendedIssues)
        {
            List<PyralisSceneReadinessIssue> issues = new List<PyralisSceneReadinessIssue>();
            AppendIssues(issues, requiredIssues, PyralisSceneReadinessSeverity.RequiredBeforePlay);
            AppendIssues(issues, recommendedIssues, PyralisSceneReadinessSeverity.RecommendedBeforePlay);
            return issues;
        }

        private static void AppendIssues(
            List<PyralisSceneReadinessIssue> output,
            IReadOnlyList<string> messages,
            PyralisSceneReadinessSeverity defaultSeverity)
        {
            if (messages == null)
                return;

            for (int i = 0; i < messages.Count; i++)
            {
                string message = messages[i];
                if (string.IsNullOrWhiteSpace(message))
                    continue;

                PyralisSceneReadinessSeverity severity = defaultSeverity == PyralisSceneReadinessSeverity.RecommendedBeforePlay && IsProofEnhancer(message)
                    ? PyralisSceneReadinessSeverity.ProofEnhancer
                    : defaultSeverity;

                output.Add(new PyralisSceneReadinessIssue(
                    message,
                    severity,
                    InferCategory(message),
                    InferNativeAction(message)));
            }
        }

        private static bool IsProofEnhancer(string message)
        {
            string lower = message.ToLowerInvariant();
            return lower.Contains("should have")
                || lower.Contains("should be registered")
                || lower.Contains("preferred seat")
                || lower.Contains("mixes 2d and 3d physics")
                || lower.Contains("has visible renderers but no collider");
        }

        private static PyralisSceneReadinessCategory InferCategory(string message)
        {
            string lower = (message ?? string.Empty).ToLowerInvariant();
            if (lower.Contains("audio") || lower.Contains("camera"))
                return PyralisSceneReadinessCategory.CameraAudio;

            if (lower.Contains("inputprofile") || lower.Contains("input system") || lower.Contains("inputmodule") || lower.Contains("move row") || lower.Contains("action map"))
                return PyralisSceneReadinessCategory.Input;

            if (lower.Contains("eventsystem") || lower.Contains("ui") || lower.Contains("canvas") || lower.Contains("hud"))
                return PyralisSceneReadinessCategory.UserInterface;

            if (lower.Contains("physics") || lower.Contains("collider") || lower.Contains("rigidbody"))
                return PyralisSceneReadinessCategory.Physics;

            if (lower.Contains("sprite") || lower.Contains("renderer") || lower.Contains("presentation"))
                return PyralisSceneReadinessCategory.Presentation;

            if (lower.Contains("network"))
                return PyralisSceneReadinessCategory.Networking;

            if (lower.Contains("prefab") || lower.Contains("pawnroot") || lower.Contains("ipawn") || lower.Contains("missing script"))
                return PyralisSceneReadinessCategory.PrefabContract;

            if (lower.Contains("gameplay root") || lower.Contains("scene"))
                return PyralisSceneReadinessCategory.SceneRoot;

            return PyralisSceneReadinessCategory.Other;
        }

        private static string InferNativeAction(string message)
        {
            string lower = (message ?? string.Empty).ToLowerInvariant();
            if (lower.Contains("standaloneinputmodule"))
                return "Select the EventSystem in the Hierarchy, then use the Inspector warning or Add Component path to replace StandaloneInputModule with InputSystemUIInputModule.";

            if (lower.Contains("eventsystem"))
                return "Create or select one EventSystem in the Hierarchy, then inspect its input module in the Inspector.";

            if (lower.Contains("audiolistener"))
                return "Select Main Camera in the Hierarchy and add or enable AudioListener in the Inspector.";

            if (lower.Contains("camera"))
                return "Create or select the physical Main Camera or Camera Root, then inspect framing and target camera fields.";

            if (lower.Contains("sprite"))
                return "Select the named scene object or prefab child and assign a Sprite on its SpriteRenderer.";

            if (lower.Contains("inputprofile") || lower.Contains("move row") || lower.Contains("action map"))
                return "Open the effective InputProfile and verify Actions, Primary Action Map, Move row, action name, and supported device toggles.";

            if (lower.Contains("collider") || lower.Contains("physics"))
                return "Inspect the prefab root and child colliders/Rigidbodies; keep one 2D or 3D physics lane for the proof.";

            if (lower.Contains("prefab") || lower.Contains("pawnroot") || lower.Contains("ipawn") || lower.Contains("missing script"))
                return "Inspect the prefab root in Prefab Mode or the Project window, then add or repair the named runtime component through the Inspector.";

            return string.Empty;
        }
    }

    public static class PyralisSceneReadinessValidator
    {
        private const string NetworkManagerTypeName = "Unity.Netcode.NetworkManager";
        private const string NetworkObjectTypeName = "Unity.Netcode.NetworkObject";
        private const string UnityTransportTypeName = "Unity.Netcode.Transports.UTP.UnityTransport";

        public static PyralisSceneReadinessReport BuildReport(GameplaySessionBootstrap bootstrap)
        {
            List<string> requiredIssues = new List<string>();
            List<string> recommendedIssues = new List<string>();

            if (bootstrap == null)
            {
                requiredIssues.Add("Select a GameplaySessionBootstrap before checking scene and prefab readiness.");
                return new PyralisSceneReadinessReport(requiredIssues, recommendedIssues);
            }

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SessionDefinition session = serializedBootstrap.FindProperty("sessionDefinition")?.objectReferenceValue as SessionDefinition;
            if (session == null)
                return new PyralisSceneReadinessReport(requiredIssues, recommendedIssues);

            AppendSceneRootIssues(bootstrap, serializedBootstrap, requiredIssues, recommendedIssues);
            AppendParticipantSeatIssues(session, requiredIssues, recommendedIssues);
            AppendParticipantInputIssues(session, requiredIssues);
            AppendParticipantPawnIssues(session, requiredIssues, recommendedIssues);
            AppendNetworkReadinessIssues(bootstrap, session, requiredIssues, recommendedIssues);

            return new PyralisSceneReadinessReport(requiredIssues, recommendedIssues);
        }

        private static void AppendSceneRootIssues(
            GameplaySessionBootstrap bootstrap,
            SerializedObject serializedBootstrap,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            HashSet<GameObject> inspectedRoots = new HashSet<GameObject>();
            AppendReferencedHierarchyIssue(bootstrap.gameObject, "Gameplay root", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "cameraRigController"), "Camera rig", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "playerInputManager"), "Player input manager", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "sessionStateService"), "Session state service", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "participantRosterService"), "Participant roster service", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "participantSpawnService"), "Participant spawn service", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "participantInputRouter"), "Participant input router", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "sceneLoader"), "Scene loader", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "timeManager"), "Time manager", inspectedRoots, requiredIssues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "cameraShake"), "Camera shake", inspectedRoots, requiredIssues);
            AppendArrayReferenceIssues(serializedBootstrap, "spawnPoints", "Spawn point", inspectedRoots, requiredIssues);

            UnityEngine.SceneManagement.Scene scene = bootstrap.gameObject.scene;
            AppendCameraAndAudioIssues(scene, inspectedRoots, requiredIssues, recommendedIssues);
            AppendUiEventSystemIssues(scene, inspectedRoots, requiredIssues);
            AppendSceneComponentIssues<ProjectileLauncherBase>(scene, "Projectile launcher", inspectedRoots, requiredIssues);
            AppendSceneServiceIssues<ISessionScoreService>(scene, "Score service", inspectedRoots, requiredIssues);
            AppendSceneSpriteRendererIssues(scene, inspectedRoots, requiredIssues);
        }

        private static void AppendCameraAndAudioIssues(
            UnityEngine.SceneManagement.Scene scene,
            HashSet<GameObject> inspectedRoots,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
            bool hasSceneCamera = false;
            bool hasSceneAudioListener = false;

            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera == null || camera.gameObject.scene != scene)
                    continue;

                hasSceneCamera = true;
                AppendReferencedHierarchyIssue(camera, "Camera", inspectedRoots, requiredIssues);

                AudioListener listener = camera.GetComponent<AudioListener>();
                if (listener != null && listener.enabled)
                    hasSceneAudioListener = true;
            }

            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude);
            int sceneListenerCount = 0;
            for (int i = 0; i < listeners.Length; i++)
            {
                AudioListener listener = listeners[i];
                if (listener == null || listener.gameObject.scene != scene)
                    continue;

                sceneListenerCount++;
                hasSceneAudioListener = true;
                AppendReferencedHierarchyIssue(listener, "Audio listener", inspectedRoots, requiredIssues);
            }

            if (!hasSceneCamera)
                recommendedIssues.Add("Scene should have at least one enabled Camera before Play Mode can show a visual proof.");

            if (!hasSceneAudioListener)
                recommendedIssues.Add("Scene should have one enabled AudioListener, usually on Main Camera, before Play Mode to avoid Unity audio errors.");
            else if (sceneListenerCount > 1)
                requiredIssues.Add($"Scene has {sceneListenerCount} enabled AudioListener components. Keep exactly one active listener before Play Mode.");
        }

        private static bool HasSceneComponent<T>(UnityEngine.SceneManagement.Scene scene) where T : Component
        {
            T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component != null && component.gameObject.scene == scene)
                    return true;
            }

            return false;
        }

        private static bool HasSceneBehaviourName(UnityEngine.SceneManagement.Scene scene, string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return false;

            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null
                    && behaviour.gameObject.scene == scene
                    && string.Equals(behaviour.GetType().Name, typeName, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendUiEventSystemIssues(
            UnityEngine.SceneManagement.Scene scene,
            HashSet<GameObject> inspectedRoots,
            List<string> requiredIssues)
        {
            bool hasSceneUi = HasSceneComponent<Canvas>(scene)
                || HasSceneComponent<Selectable>(scene)
                || HasSceneBehaviourName(scene, "UIManager")
                || HasSceneBehaviourName(scene, "ParticipantHealthHudBinder");

            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude);
            int sceneEventSystemCount = 0;
            bool hasInputSystemModule = false;
            bool hasStandaloneModule = false;

            for (int i = 0; i < eventSystems.Length; i++)
            {
                EventSystem eventSystem = eventSystems[i];
                if (eventSystem == null || eventSystem.gameObject.scene != scene)
                    continue;

                sceneEventSystemCount++;
                AppendReferencedHierarchyIssue(eventSystem, "EventSystem", inspectedRoots, requiredIssues);

                if (eventSystem.GetComponent<InputSystemUIInputModule>() != null)
                    hasInputSystemModule = true;

                if (eventSystem.GetComponent<StandaloneInputModule>() != null)
                    hasStandaloneModule = true;
            }

            if (hasSceneUi && sceneEventSystemCount == 0)
                requiredIssues.Add("Scene UI needs one EventSystem before Play Mode so buttons, menus, HUD selection, and pointer input can work.");

            if (sceneEventSystemCount > 1)
                requiredIssues.Add($"Scene has {sceneEventSystemCount} active EventSystem objects. Keep one active EventSystem before Play Mode.");

            if (sceneEventSystemCount > 0 && hasStandaloneModule && !hasInputSystemModule)
                requiredIssues.Add("EventSystem uses StandaloneInputModule. Replace it with InputSystemUIInputModule for this Input System project before Play Mode.");
        }

        private static void AppendArrayReferenceIssues(
            SerializedObject serializedObject,
            string propertyName,
            string label,
            HashSet<GameObject> inspectedRoots,
            List<string> requiredIssues)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
                return;

            for (int i = 0; i < property.arraySize; i++)
            {
                Object value = property.GetArrayElementAtIndex(i).objectReferenceValue;
                AppendReferencedHierarchyIssue(value, $"{label} {i}", inspectedRoots, requiredIssues);
            }
        }

        private static void AppendSceneComponentIssues<T>(
            UnityEngine.SceneManagement.Scene scene,
            string label,
            HashSet<GameObject> inspectedRoots,
            List<string> requiredIssues) where T : Component
        {
            T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component != null && component.gameObject.scene == scene)
                    AppendReferencedHierarchyIssue(component, label, inspectedRoots, requiredIssues);
            }
        }

        private static void AppendSceneServiceIssues<T>(
            UnityEngine.SceneManagement.Scene scene,
            string label,
            HashSet<GameObject> inspectedRoots,
            List<string> requiredIssues) where T : class
        {
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.gameObject.scene == scene && behaviour is T)
                    AppendReferencedHierarchyIssue(behaviour, label, inspectedRoots, requiredIssues);
            }
        }

        private static void AppendReferencedHierarchyIssue(
            Object reference,
            string label,
            HashSet<GameObject> inspectedRoots,
            List<string> requiredIssues)
        {
            GameObject root = GetReferenceGameObject(reference);
            if (root == null || !inspectedRoots.Add(root))
                return;

            int missingScripts = GetMissingScriptCountInHierarchy(root);
            if (missingScripts > 0)
                requiredIssues.Add($"{label} `{root.name}` has {missingScripts} missing script reference(s) in its hierarchy.");
        }

        private static GameObject GetReferenceGameObject(Object reference)
        {
            if (reference is GameObject gameObject)
                return gameObject;

            if (reference is Component component)
                return component.gameObject;

            return null;
        }

        private static void AppendParticipantSeatIssues(
            SessionDefinition session,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            if (session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return;

            int effectiveMaxParticipants = session.GetEffectiveMaxParticipants();
            if (session.defaultParticipants.Length > effectiveMaxParticipants)
                requiredIssues.Add($"Session has {session.defaultParticipants.Length} default participants but only supports {effectiveMaxParticipants} participants.");

            HashSet<int> preferredSeats = new HashSet<int>();
            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                {
                    requiredIssues.Add($"Default participant slot {i} is empty.");
                    continue;
                }

                if (participant.preferredSeatIndex < 0)
                    continue;

                if (participant.preferredSeatIndex >= effectiveMaxParticipants)
                {
                    requiredIssues.Add($"Participant `{participant.displayName}` prefers seat {participant.preferredSeatIndex}, outside max participant count {effectiveMaxParticipants}.");
                    continue;
                }

                if (!preferredSeats.Add(participant.preferredSeatIndex))
                    recommendedIssues.Add($"Preferred seat {participant.preferredSeatIndex} is assigned more than once; runtime can reassign duplicates, but prefabs/scenes should author seats clearly.");
            }
        }

        private static void AppendParticipantPawnIssues(
            SessionDefinition session,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            if (session.defaultParticipants == null)
                return;

            HashSet<Object> inspectedObjects = new HashSet<Object>();
            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                PawnDefinition pawn = participant != null ? participant.defaultPawn : null;
                if (pawn == null)
                    continue;

                AppendPawnDefinitionIssues(i, pawn, inspectedObjects, requiredIssues, recommendedIssues);
            }
        }

        private static void AppendParticipantInputIssues(SessionDefinition session, List<string> requiredIssues)
        {
            if (session == null || session.defaultParticipants == null)
                return;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null || participant.defaultPawn == null)
                    continue;

                InputProfile profile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(
                    participant,
                    participant.defaultPawn,
                    session.defaultInputProfile);

                if (profile == null)
                {
                    requiredIssues.Add($"Participant slot {i} pawn input needs an InputProfile on the participant, PawnDefinition, or SessionDefinition default.");
                    continue;
                }

                string issue = GetInputProfileReadinessIssue(profile);
                if (!string.IsNullOrWhiteSpace(issue))
                    requiredIssues.Add($"Participant slot {i} effective InputProfile `{profile.name}`: {issue}");
            }
        }

        private static string GetInputProfileReadinessIssue(InputProfile profile)
        {
            if (profile == null)
                return "InputProfile is not assigned.";

            profile.Sanitize();

            if (profile.actions == null)
                return "Actions must reference the stock Assets/InputSystem_Actions.inputactions asset or another Unity Input Action Asset before movement can be proven.";

            InputActionMap map = ParticipantInputProfileUtility.FindGameplayActionMap(profile.actions, profile);
            if (map == null)
            {
                string mapName = !string.IsNullOrWhiteSpace(profile.primaryActionMap)
                    ? profile.primaryActionMap
                    : "Player";
                return $"Primary Action Map `{mapName}` was not found in Actions.";
            }

            GameplayInputActionBinding moveBinding = profile.FindBinding(GameplayInputActionRole.Move);
            if (moveBinding == null)
                return "Gameplay Actions must include a required Move row.";

            if (string.IsNullOrWhiteSpace(moveBinding.actionName))
                return "Move row must name the Unity action that drives movement.";

            string moveMapName = moveBinding.GetActionMap(profile.primaryActionMap);
            InputActionMap moveMap = string.Equals(moveMapName, map.name, System.StringComparison.OrdinalIgnoreCase)
                ? map
                : profile.actions.FindActionMap(moveMapName, throwIfNotFound: false);
            if (moveMap == null)
                return $"Move row Action Map `{moveMapName}` was not found in Actions.";

            if (ParticipantInputProfileUtility.FindAction(moveMap, moveBinding.actionName) == null)
                return $"Move row Unity Action Name `{moveBinding.actionName}` was not found in Action Map `{moveMap.name}`.";

            if (!profile.supportsGamepad && !profile.supportsKeyboardMouse && !profile.touchFriendly)
                return "At least one input surface should be enabled for a player-owned pawn.";

            return string.Empty;
        }

        private static void AppendPawnDefinitionIssues(
            int participantSlot,
            PawnDefinition pawn,
            HashSet<Object> inspectedObjects,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            if (pawn.pawnPrefab == null)
                return;

            GameObject prefab = pawn.pawnPrefab;
            if (inspectedObjects.Add(prefab))
            {
                AppendMissingScriptIssue(prefab, $"Pawn prefab `{prefab.name}`", requiredIssues);

                if (prefab.GetComponent<PawnRoot>() == null)
                    requiredIssues.Add($"Participant slot {participantSlot} pawn prefab `{prefab.name}` is missing PawnRoot on its root GameObject.");

                if (!HasComponentImplementing<IPawnMotor>(prefab))
                    requiredIssues.Add($"Participant slot {participantSlot} pawn prefab `{prefab.name}` is missing a lane motor component implementing IPawnMotor.");

                if (!HasComponentImplementing<IPawnInputModule>(prefab))
                    requiredIssues.Add($"Participant slot {participantSlot} pawn prefab `{prefab.name}` is missing an input adapter component implementing IPawnInputModule so the selected InputProfile can reach movement.");

                if (!HasComponentImplementing<IPawnPresentationModule>(prefab))
                    requiredIssues.Add($"Participant slot {participantSlot} pawn prefab `{prefab.name}` needs a component implementing IPawnPresentationModule.");

                AppendPrefabSpriteRendererIssues(participantSlot, prefab, requiredIssues);
                AppendPawnPrefabPresentationPhysicsIssues(participantSlot, prefab, recommendedIssues);
            }

            AppendFeatureModuleIssues(pawn, inspectedObjects, requiredIssues);
            AppendCombatProjectileIssues(pawn, inspectedObjects, requiredIssues, recommendedIssues);
        }

        private static void AppendPrefabSpriteRendererIssues(int participantSlot, GameObject prefab, List<string> requiredIssues)
        {
            if (prefab == null)
                return;

            SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers == null || renderers.Length == 0)
                return;

            bool hasEnabledRenderer = false;
            bool hasAssignedSprite = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                    continue;

                hasEnabledRenderer = true;
                if (renderer.sprite != null)
                    hasAssignedSprite = true;
            }

            if (hasEnabledRenderer && !hasAssignedSprite)
                requiredIssues.Add($"Participant slot {participantSlot} pawn prefab `{prefab.name}` has enabled SpriteRenderer components but no assigned Sprite. Assign a visual sprite or use a presentation route that supplies one before Play Mode.");
        }

        private static void AppendPawnPrefabPresentationPhysicsIssues(int participantSlot, GameObject prefab, List<string> recommendedIssues)
        {
            if (prefab == null)
                return;

            bool hasVisibleRenderer = HasEnabledVisibleRenderer(prefab);
            bool has2DPhysics = prefab.GetComponentInChildren<Rigidbody2D>(true) != null
                || prefab.GetComponentInChildren<Collider2D>(true) != null;
            bool has3DPhysics = prefab.GetComponentInChildren<Rigidbody>(true) != null
                || prefab.GetComponentInChildren<Collider>(true) != null
                || prefab.GetComponentInChildren<CharacterController>(true) != null;

            if (hasVisibleRenderer && !has2DPhysics && !has3DPhysics)
                recommendedIssues.Add($"Participant slot {participantSlot} pawn prefab `{prefab.name}` has visible renderers but no Collider, Rigidbody, or CharacterController. Add the route-appropriate collision surface before judging movement feel.");

            if (has2DPhysics && has3DPhysics)
                recommendedIssues.Add($"Participant slot {participantSlot} pawn prefab `{prefab.name}` mixes 2D and 3D physics components. Keep one physics lane per pawn prefab for clean movement proof behavior.");
        }

        private static bool HasEnabledVisibleRenderer(GameObject prefab)
        {
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer.enabled)
                    return true;
            }

            return false;
        }

        private static void AppendSceneSpriteRendererIssues(
            UnityEngine.SceneManagement.Scene scene,
            HashSet<GameObject> inspectedRoots,
            List<string> requiredIssues)
        {
            SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null
                    || renderer.sprite != null
                    || renderer.gameObject.scene != scene
                    || !inspectedRoots.Add(renderer.gameObject))
                {
                    continue;
                }

                requiredIssues.Add($"Scene SpriteRenderer `{renderer.gameObject.name}` has no Sprite assigned. Assign a sprite before using Play Mode as a visual proof.");
            }
        }

        private static void AppendFeatureModuleIssues(
            PawnDefinition pawn,
            HashSet<Object> inspectedObjects,
            List<string> requiredIssues)
        {
            if (pawn.featureModules == null)
                return;

            for (int i = 0; i < pawn.featureModules.Length; i++)
            {
                FeatureModuleDefinition module = pawn.featureModules[i];
                if (module == null || !module.enabledByDefault)
                    continue;

                if (inspectedObjects.Add(module))
                {
                    List<string> moduleIssues = module.GetValidationIssues();
                    for (int issueIndex = 0; issueIndex < moduleIssues.Count; issueIndex++)
                        requiredIssues.Add($"Feature module `{module.moduleId}`: {moduleIssues[issueIndex]}");
                }

                if (module.runtimePrefab != null && inspectedObjects.Add(module.runtimePrefab))
                    AppendMissingScriptIssue(module.runtimePrefab, $"Feature runtime prefab `{module.runtimePrefab.name}`", requiredIssues);
            }
        }

        private static void AppendCombatProjectileIssues(
            PawnDefinition pawn,
            HashSet<Object> inspectedObjects,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            PawnCombatProfile combatProfile = pawn.combatProfile;
            if (combatProfile == null || !combatProfile.enableCombat)
                return;

            AppendWeaponProjectileIssues(combatProfile.attackWeapon, inspectedObjects, requiredIssues, recommendedIssues);
            AppendWeaponProjectileIssues(combatProfile.kickWeapon, inspectedObjects, requiredIssues, recommendedIssues);
            AppendWeaponProjectileIssues(combatProfile.aerialWeapon, inspectedObjects, requiredIssues, recommendedIssues);
            AppendSequenceProjectileIssues(combatProfile.primarySequence, inspectedObjects, requiredIssues, recommendedIssues);
            AppendSequenceProjectileIssues(combatProfile.secondarySequence, inspectedObjects, requiredIssues, recommendedIssues);
            AppendSequenceProjectileIssues(combatProfile.aerialSequence, inspectedObjects, requiredIssues, recommendedIssues);
        }

        private static void AppendSequenceProjectileIssues(
            CombatSequenceDefinition sequence,
            HashSet<Object> inspectedObjects,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            if (sequence == null || sequence.actions == null)
                return;

            for (int i = 0; i < sequence.actions.Length; i++)
            {
                CombatActionDefinition action = sequence.actions[i];
                if (action != null)
                    AppendWeaponProjectileIssues(action.weapon, inspectedObjects, requiredIssues, recommendedIssues);
            }
        }

        private static void AppendWeaponProjectileIssues(
            WeaponData weapon,
            HashSet<Object> inspectedObjects,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            if (weapon == null || weapon.projectileDefinition == null)
                return;

            ProjectileDefinition projectile = weapon.projectileDefinition;
            if (!inspectedObjects.Add(projectile))
                return;

            List<string> projectileIssues = projectile.GetValidationIssues();
            for (int i = 0; i < projectileIssues.Count; i++)
                requiredIssues.Add($"Projectile `{projectile.displayName}`: {projectileIssues[i]}");

            if (projectile.deliveryMode != ProjectileDeliveryMode.ProjectilePrefab || projectile.projectilePrefab == null)
                return;

            GameObject projectilePrefab = projectile.projectilePrefab;
            AppendMissingScriptIssue(projectilePrefab, $"Projectile prefab `{projectilePrefab.name}`", requiredIssues);

            if (!HasComponentImplementing<IProjectileRuntimeBody>(projectilePrefab))
            {
                requiredIssues.Add($"Projectile prefab `{projectilePrefab.name}` needs Projectile or Projectile2D so ProjectileDefinition data reaches runtime shots.");
            }

            bool has3DPhysics = projectilePrefab.GetComponentInChildren<Rigidbody>(true) != null
                || projectilePrefab.GetComponentInChildren<Collider>(true) != null;
            bool has2DPhysics = projectilePrefab.GetComponentInChildren<Rigidbody2D>(true) != null
                || projectilePrefab.GetComponentInChildren<Collider2D>(true) != null;

            if (!has3DPhysics && !has2DPhysics)
                requiredIssues.Add($"Projectile prefab `{projectilePrefab.name}` needs 2D or 3D physics components for movement and hit detection.");

            if (has2DPhysics && has3DPhysics)
                recommendedIssues.Add($"Projectile prefab `{projectilePrefab.name}` mixes 2D and 3D physics. Keep one physics lane per projectile prefab.");
        }

        private static void AppendNetworkReadinessIssues(
            GameplaySessionBootstrap bootstrap,
            SessionDefinition session,
            List<string> requiredIssues,
            List<string> recommendedIssues)
        {
            if (session.networkMode == GameplayNetworkMode.LocalOnly)
                return;

            MonoBehaviour networkManager = FindSceneBehaviourByTypeName(bootstrap.gameObject.scene, NetworkManagerTypeName);
            if (networkManager == null)
            {
                requiredIssues.Add("Networked sessions require a scene NetworkManager.");
            }
            else if (!NetworkManagerUsesUnityTransport(networkManager))
            {
                requiredIssues.Add("Networked sessions require NetworkManager to use UnityTransport for the supported MVP lane.");
            }

            if (session.defaultParticipants == null)
                return;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                GameObject pawnPrefab = participant != null && participant.defaultPawn != null
                    ? participant.defaultPawn.pawnPrefab
                    : null;

                if (pawnPrefab == null)
                    continue;

                if (!HasComponentOfTypeName(pawnPrefab, NetworkObjectTypeName))
                {
                    requiredIssues.Add($"Networked participant slot {i} pawn prefab `{pawnPrefab.name}` needs a NetworkObject.");
                    continue;
                }

                if (networkManager != null && !NetworkManagerRegistersPrefab(networkManager, pawnPrefab))
                    recommendedIssues.Add($"Networked participant slot {i} pawn prefab `{pawnPrefab.name}` should be registered in NetworkManager Network Prefabs before scene playtesting.");
            }
        }

        private static void AppendMissingScriptIssue(GameObject root, string label, List<string> requiredIssues)
        {
            int missingScripts = GetMissingScriptCountInHierarchy(root);
            if (missingScripts > 0)
                requiredIssues.Add($"{label} has {missingScripts} missing script reference(s).");
        }

        private static int GetMissingScriptCountInHierarchy(GameObject root)
        {
            if (root == null)
                return 0;

            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].gameObject != root)
                    count += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(children[i].gameObject);
            }

            return count;
        }

        private static bool HasComponentImplementing<T>(GameObject root) where T : class
        {
            if (root == null)
                return false;

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is T)
                    return true;
            }

            return false;
        }

        private static bool HasComponentOfTypeName(GameObject root, string fullTypeName)
        {
            if (root == null || string.IsNullOrWhiteSpace(fullTypeName))
                return false;

            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && string.Equals(component.GetType().FullName, fullTypeName, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static MonoBehaviour FindSceneBehaviourByTypeName(UnityEngine.SceneManagement.Scene scene, string fullTypeName)
        {
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null
                    && behaviour.gameObject.scene == scene
                    && string.Equals(behaviour.GetType().FullName, fullTypeName, System.StringComparison.Ordinal))
                {
                    return behaviour;
                }
            }

            return null;
        }

        private static bool NetworkManagerUsesUnityTransport(MonoBehaviour networkManager)
        {
            object networkConfig = GetPropertyValue(networkManager, "NetworkConfig");
            object networkTransport = GetPropertyValue(networkConfig, "NetworkTransport");
            return networkTransport != null
                && string.Equals(networkTransport.GetType().FullName, UnityTransportTypeName, System.StringComparison.Ordinal);
        }

        private static bool NetworkManagerRegistersPrefab(MonoBehaviour networkManager, GameObject prefab)
        {
            object networkConfig = GetPropertyValue(networkManager, "NetworkConfig");
            object prefabs = GetPropertyValue(networkConfig, "Prefabs");
            object prefabList = GetPropertyValue(prefabs, "Prefabs");
            if (prefabList is not IEnumerable enumerable)
                return false;

            foreach (object networkPrefab in enumerable)
            {
                if (networkPrefab == null)
                    continue;

                if (ReferenceEquals(GetPropertyValue(networkPrefab, "Prefab"), prefab)
                    || ReferenceEquals(GetPropertyValue(networkPrefab, "SourcePrefabToOverride"), prefab)
                    || ReferenceEquals(GetPropertyValue(networkPrefab, "OverridingTargetPrefab"), prefab))
                {
                    return true;
                }
            }

            return false;
        }

        private static object GetPropertyValue(object target, string propertyName)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            return target.GetType().GetProperty(propertyName)?.GetValue(target);
        }

        private static T GetObjectReference<T>(SerializedObject serializedObject, string propertyName) where T : Object
        {
            return serializedObject.FindProperty(propertyName)?.objectReferenceValue as T;
        }
    }
}
