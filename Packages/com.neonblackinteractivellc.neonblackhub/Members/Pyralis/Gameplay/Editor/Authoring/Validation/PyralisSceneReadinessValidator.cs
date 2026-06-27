using System.Collections.Generic;
using NeonBlack.Gameplay.Glue.Bootstrap;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Glue.InputRouting;
using NeonBlack.Gameplay.Modules.Actor.Composition;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using NeonBlack.Gameplay.Presentation.HUD.GameFlow;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public enum PyralisSceneReadinessSeverity
    {
        RequiredBeforePlay,
        RecommendedBeforePlay
    }

    public enum PyralisSceneReadinessCategory
    {
        SceneRoot,
        CameraAudio,
        Input,
        UserInterface,
        Presentation,
        Physics,
        Networking,
        Other
    }

    public sealed class PyralisSceneReadinessIssue
    {
        public PyralisSceneReadinessIssue(
            string message,
            PyralisSceneReadinessSeverity severity,
            PyralisSceneReadinessCategory category,
            string nativeAction = "",
            string nativeActionTarget = "",
            string nativeActionFieldOrComponent = "")
        {
            Message = message ?? string.Empty;
            Severity = severity;
            Category = category;
            NativeAction = nativeAction ?? string.Empty;
            NativeActionTarget = nativeActionTarget ?? string.Empty;
            NativeActionFieldOrComponent = nativeActionFieldOrComponent ?? string.Empty;
        }

        public string Message { get; }
        public PyralisSceneReadinessSeverity Severity { get; }
        public PyralisSceneReadinessCategory Category { get; }
        public string NativeAction { get; }
        public string NativeActionTarget { get; }
        public string NativeActionFieldOrComponent { get; }
    }

    public sealed class PyralisSceneReadinessReport
    {
        private readonly List<string> _requiredIssues;
        private readonly List<string> _recommendedIssues;
        private readonly List<PyralisSceneReadinessIssue> _issues;

        public PyralisSceneReadinessReport(IEnumerable<PyralisSceneReadinessIssue> issues)
        {
            _issues = new List<PyralisSceneReadinessIssue>(issues ?? System.Array.Empty<PyralisSceneReadinessIssue>());
            _requiredIssues = new List<string>(GetMessages(PyralisSceneReadinessSeverity.RequiredBeforePlay));
            _recommendedIssues = new List<string>(GetMessages(PyralisSceneReadinessSeverity.RecommendedBeforePlay));
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

        internal static void AddRequired(
            List<PyralisSceneReadinessIssue> issues,
            string message,
            PyralisSceneReadinessCategory category,
            string nativeAction = "",
            string nativeActionTarget = "",
            string nativeActionFieldOrComponent = "")
        {
            AddIssue(
                issues,
                message,
                PyralisSceneReadinessSeverity.RequiredBeforePlay,
                category,
                nativeAction,
                nativeActionTarget,
                nativeActionFieldOrComponent);
        }

        internal static void AddRecommended(
            List<PyralisSceneReadinessIssue> issues,
            string message,
            PyralisSceneReadinessCategory category,
            string nativeAction = "",
            string nativeActionTarget = "",
            string nativeActionFieldOrComponent = "")
        {
            AddIssue(
                issues,
                message,
                PyralisSceneReadinessSeverity.RecommendedBeforePlay,
                category,
                nativeAction,
                nativeActionTarget,
                nativeActionFieldOrComponent);
        }

        private static void AddIssue(
            List<PyralisSceneReadinessIssue> issues,
            string message,
            PyralisSceneReadinessSeverity severity,
            PyralisSceneReadinessCategory category,
            string nativeAction,
            string nativeActionTarget,
            string nativeActionFieldOrComponent)
        {
            if (issues == null || string.IsNullOrWhiteSpace(message))
                return;

            issues.Add(new PyralisSceneReadinessIssue(
                message,
                severity,
                category,
                nativeAction,
                nativeActionTarget,
                nativeActionFieldOrComponent));
        }
    }

    public static class PyralisSceneReadinessValidator
    {
        private const string NetworkManagerTypeName = "Unity.Netcode.NetworkManager";
        private const string UnityTransportTypeName = "Unity.Netcode.Transports.UTP.UnityTransport";
        private const string NetworkedSessionStateServiceFullName = "NeonBlack.Gameplay.Networking.Participants.NetworkedSessionStateService";
        private const string NetworkedParticipantRosterServiceFullName = "NeonBlack.Gameplay.Networking.Participants.NetworkedParticipantRosterService";
        private const string NetworkedParticipantSpawnServiceFullName = "NeonBlack.Gameplay.Networking.Participants.NetworkedParticipantSpawnService";

        public static PyralisSceneReadinessReport BuildReport(GameplaySessionBootstrap bootstrap)
        {
            List<PyralisSceneReadinessIssue> issues = new List<PyralisSceneReadinessIssue>();

            if (bootstrap == null)
            {
                AddRequired(
                    issues,
                    "Select a GameplaySessionBootstrap before checking scene and prefab readiness.",
                    PyralisSceneReadinessCategory.SceneRoot,
                    "Select the Gameplay Root object with GameplaySessionBootstrap before checking local readiness.");
                return new PyralisSceneReadinessReport(issues);
            }

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SessionDefinition session = serializedBootstrap.FindProperty("sessionDefinition")?.objectReferenceValue as SessionDefinition;
            if (session == null)
                return new PyralisSceneReadinessReport(issues);

            AppendSceneRootIssues(bootstrap, serializedBootstrap, issues);
            AppendCoreRuntimeServiceIssues(bootstrap, serializedBootstrap, session, issues);
            AppendNetworkReadinessIssues(bootstrap, session, issues);

            return new PyralisSceneReadinessReport(issues);
        }

        private static void AddRequired(
            List<PyralisSceneReadinessIssue> issues,
            string message,
            PyralisSceneReadinessCategory category,
            string nativeAction = "",
            string nativeActionTarget = "",
            string nativeActionFieldOrComponent = "")
        {
            PyralisSceneReadinessReport.AddRequired(
                issues,
                message,
                category,
                nativeAction,
                nativeActionTarget,
                nativeActionFieldOrComponent);
        }

        private static void AddRecommended(
            List<PyralisSceneReadinessIssue> issues,
            string message,
            PyralisSceneReadinessCategory category,
            string nativeAction = "",
            string nativeActionTarget = "",
            string nativeActionFieldOrComponent = "")
        {
            PyralisSceneReadinessReport.AddRecommended(
                issues,
                message,
                category,
                nativeAction,
                nativeActionTarget,
                nativeActionFieldOrComponent);
        }

        private static void AppendSceneRootIssues(
            GameplaySessionBootstrap bootstrap,
            SerializedObject serializedBootstrap,
            List<PyralisSceneReadinessIssue> issues)
        {
            HashSet<GameObject> inspectedRoots = new HashSet<GameObject>();
            AppendReferencedHierarchyIssue(bootstrap.gameObject, "Gameplay root", inspectedRoots, issues, PyralisSceneReadinessCategory.SceneRoot);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "cameraRigController"), "Camera rig", inspectedRoots, issues, PyralisSceneReadinessCategory.CameraAudio);
            PlayerInputManager playerInputManager = GetObjectReference<PlayerInputManager>(serializedBootstrap, "playerInputManager");
            AppendReferencedHierarchyIssue(playerInputManager, "Player input manager", inspectedRoots, issues, PyralisSceneReadinessCategory.Input);
            AppendPlayerInputManagerIssues(playerInputManager, issues);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "sessionStateService"), "Session state service", inspectedRoots, issues, PyralisSceneReadinessCategory.SceneRoot);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "participantRosterService"), "Participant roster service", inspectedRoots, issues, PyralisSceneReadinessCategory.SceneRoot);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "participantSpawnService"), "Participant spawn service", inspectedRoots, issues, PyralisSceneReadinessCategory.SceneRoot);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "participantInputRouter"), "Participant input router", inspectedRoots, issues, PyralisSceneReadinessCategory.Input);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "sceneNavigatorSource"), "Scene navigator", inspectedRoots, issues, PyralisSceneReadinessCategory.SceneRoot);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "timeManager"), "Time manager", inspectedRoots, issues, PyralisSceneReadinessCategory.SceneRoot);
            AppendReferencedHierarchyIssue(GetObjectReference<Object>(serializedBootstrap, "cameraShake"), "Camera shake", inspectedRoots, issues, PyralisSceneReadinessCategory.CameraAudio);
            ParticipantSpawnService spawnService = GetParticipantSpawnService(bootstrap, serializedBootstrap);
            if (spawnService != null)
                AppendArrayReferenceIssues(new SerializedObject(spawnService), "spawnPoints", "Spawn point", inspectedRoots, issues, PyralisSceneReadinessCategory.SceneRoot);

            UnityEngine.SceneManagement.Scene scene = bootstrap.gameObject.scene;
            AppendCameraAndAudioIssues(scene, inspectedRoots, issues);
            AppendUiEventSystemIssues(scene, inspectedRoots, issues);
            AppendSceneSpriteRendererIssues(scene, inspectedRoots, issues);
        }

        private static void AppendCoreRuntimeServiceIssues(
            GameplaySessionBootstrap bootstrap,
            SerializedObject serializedBootstrap,
            SessionDefinition session,
            List<PyralisSceneReadinessIssue> issues)
        {
            if (bootstrap == null || session == null)
                return;

            bool usesNetworkedCoreServices = session.networkMode != GameplayNetworkMode.LocalOnly;
            AppendCoreRuntimeServiceIssue<SessionStateService>(
                bootstrap,
                serializedBootstrap,
                "sessionStateService",
                "SessionStateService",
                usesNetworkedCoreServices ? NetworkedSessionStateServiceFullName : string.Empty,
                issues);
            AppendCoreRuntimeServiceIssue<ParticipantRosterService>(
                bootstrap,
                serializedBootstrap,
                "participantRosterService",
                "ParticipantRosterService",
                usesNetworkedCoreServices ? NetworkedParticipantRosterServiceFullName : string.Empty,
                issues);
            AppendCoreRuntimeServiceIssue<ParticipantSpawnService>(
                bootstrap,
                serializedBootstrap,
                "participantSpawnService",
                "ParticipantSpawnService",
                usesNetworkedCoreServices ? NetworkedParticipantSpawnServiceFullName : string.Empty,
                issues);
            AppendCoreRuntimeServiceIssue<ParticipantInputRouter>(
                bootstrap,
                serializedBootstrap,
                "participantInputRouter",
                "ParticipantInputRouter",
                string.Empty,
                issues);
        }

        private static void AppendCoreRuntimeServiceIssue<T>(
            GameplaySessionBootstrap bootstrap,
            SerializedObject serializedBootstrap,
            string propertyName,
            string serviceName,
            string preferredFullTypeName,
            List<PyralisSceneReadinessIssue> issues) where T : Component
        {
            T service = GetObjectReference<T>(serializedBootstrap, propertyName);
            service ??= bootstrap.GetComponentInChildren<T>(true);
            if (service == null)
            {
                string componentName = !string.IsNullOrWhiteSpace(preferredFullTypeName)
                    ? GetTypeDisplayName(preferredFullTypeName)
                    : typeof(T).Name;
                AddRequired(
                    issues,
                    $"Gameplay Root is missing authored core runtime service `{serviceName}`. Add a child GameObject with {componentName}, or assign Bootstrap > {ObjectNames.NicifyVariableName(propertyName)} before Play Mode.",
                    PyralisSceneReadinessCategory.SceneRoot,
                    $"Select Gameplay Root in the Hierarchy, create a child GameObject, add {componentName}, or assign Bootstrap > {ObjectNames.NicifyVariableName(propertyName)}.",
                    "Gameplay Root",
                    componentName);
                return;
            }

            if (!string.IsNullOrWhiteSpace(preferredFullTypeName)
                && !string.Equals(service.GetType().FullName, preferredFullTypeName, System.StringComparison.Ordinal))
            {
                AddRequired(
                    issues,
                    $"Gameplay Root core runtime service `{serviceName}` uses `{service.GetType().Name}`, but this networked route expects `{GetTypeDisplayName(preferredFullTypeName)}`.",
                    PyralisSceneReadinessCategory.Networking,
                    "Inspect the authored core service on Gameplay Root and assign the networked service variant for this route.");
            }
        }

        private static string GetTypeDisplayName(string fullTypeName)
        {
            if (string.IsNullOrWhiteSpace(fullTypeName))
                return string.Empty;

            int lastDot = fullTypeName.LastIndexOf('.');
            return lastDot >= 0 && lastDot < fullTypeName.Length - 1
                ? fullTypeName.Substring(lastDot + 1)
                : fullTypeName;
        }

        private static void AppendPlayerInputManagerIssues(PlayerInputManager playerInputManager, List<PyralisSceneReadinessIssue> issues)
        {
            if (playerInputManager == null)
                return;

            if (playerInputManager.playerPrefab == null)
            {
                AddRequired(
                    issues,
                    "Bootstrap has a PlayerInputManager assigned, but PlayerInputManager > Player Prefab is empty. Clear Bootstrap > Player Input Manager for single-player auto-join, or assign a player prefab with PlayerInput and PawnRoot for local join.",
                    PyralisSceneReadinessCategory.Input,
                    "Inspect PlayerInputManager and assign a player prefab with PlayerInput and PawnRoot, or clear Bootstrap > Player Input Manager for solo auto-start.");
                return;
            }

            PlayerInput playerInput = playerInputManager.playerPrefab.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                AddRequired(
                    issues,
                    $"PlayerInputManager prefab `{playerInputManager.playerPrefab.name}` needs a Unity PlayerInput component for local join.",
                    PyralisSceneReadinessCategory.Input,
                    "Open the PlayerInputManager player prefab and add Unity PlayerInput.");
                return;
            }

            if (playerInput.actions == null)
                AddRequired(
                    issues,
                    $"PlayerInputManager prefab `{playerInputManager.playerPrefab.name}` has PlayerInput but no Actions asset. Assign the same Input Actions asset used by the controlling InputProfile.",
                    PyralisSceneReadinessCategory.Input,
                    "Open the PlayerInput prefab and assign PlayerInput > Actions.");

            if (!PrefabContainsPawnInitializer(playerInputManager.playerPrefab))
                AddRequired(
                    issues,
                    $"PlayerInputManager prefab `{playerInputManager.playerPrefab.name}` should contain PawnRoot/IPawnParticipantInitializer so the joined PlayerInput controls that participant's pawn instead of a shared action asset.",
                    PyralisSceneReadinessCategory.Input,
                    "Open the PlayerInput prefab and make the root or child pawn initializer visible to Unity PlayerInputManager.");
        }

        private static bool PrefabContainsPawnInitializer(GameObject prefab)
        {
            if (prefab == null)
                return false;

            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IPawnParticipantInitializer)
                    return true;
            }

            return false;
        }

        private static void AppendCameraAndAudioIssues(
            UnityEngine.SceneManagement.Scene scene,
            HashSet<GameObject> inspectedRoots,
            List<PyralisSceneReadinessIssue> issues)
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
                AppendReferencedHierarchyIssue(camera, "Camera", inspectedRoots, issues, PyralisSceneReadinessCategory.CameraAudio);

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
                AppendReferencedHierarchyIssue(listener, "Audio listener", inspectedRoots, issues, PyralisSceneReadinessCategory.CameraAudio);
            }

            if (!hasSceneCamera)
                AddRequired(
                    issues,
                    "Scene needs at least one enabled Camera before Play Mode can show gameplay.",
                    PyralisSceneReadinessCategory.CameraAudio,
                    "Create or select the physical Main Camera or Camera Root, then inspect framing and target camera fields.");

            if (!hasSceneAudioListener)
                AddRecommended(
                    issues,
                    "Scene should have one enabled AudioListener, usually on Main Camera, before Play Mode to avoid Unity audio errors.",
                    PyralisSceneReadinessCategory.CameraAudio,
                    "Select Main Camera in the Hierarchy and add or enable AudioListener in the Inspector.");
            else if (sceneListenerCount > 1)
                AddRequired(
                    issues,
                    $"Scene has {sceneListenerCount} enabled AudioListener components. Keep exactly one active listener before Play Mode.",
                    PyralisSceneReadinessCategory.CameraAudio,
                    "Disable duplicate AudioListener components so Unity has exactly one active listener.");
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
            List<PyralisSceneReadinessIssue> issues)
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
                AppendReferencedHierarchyIssue(eventSystem, "EventSystem", inspectedRoots, issues, PyralisSceneReadinessCategory.UserInterface);

                if (eventSystem.GetComponent<InputSystemUIInputModule>() != null)
                    hasInputSystemModule = true;

                if (eventSystem.GetComponent<StandaloneInputModule>() != null)
                    hasStandaloneModule = true;
            }

            if (hasSceneUi && sceneEventSystemCount == 0)
                AddRequired(
                    issues,
                    "Scene UI needs one EventSystem before Play Mode so buttons, menus, HUD selection, and pointer input can work.",
                    PyralisSceneReadinessCategory.UserInterface,
                    "Create or select one EventSystem in the Hierarchy, then inspect its input module in the Inspector.");

            if (sceneEventSystemCount > 1)
                AddRequired(
                    issues,
                    $"Scene has {sceneEventSystemCount} active EventSystem objects. Keep one active EventSystem before Play Mode.",
                    PyralisSceneReadinessCategory.UserInterface,
                    "Keep one active EventSystem in the scene and disable or remove duplicates.");

            if (sceneEventSystemCount > 0 && hasStandaloneModule && !hasInputSystemModule)
                AddRequired(
                    issues,
                    "EventSystem uses StandaloneInputModule. Replace it with InputSystemUIInputModule for this Input System project before Play Mode.",
                    PyralisSceneReadinessCategory.UserInterface,
                    "Select the EventSystem in the Hierarchy, then use the Inspector warning or Add Component path to replace StandaloneInputModule with InputSystemUIInputModule.");
        }

        private static void AppendArrayReferenceIssues(
            SerializedObject serializedObject,
            string propertyName,
            string label,
            HashSet<GameObject> inspectedRoots,
            List<PyralisSceneReadinessIssue> issues,
            PyralisSceneReadinessCategory category)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
                return;

            for (int i = 0; i < property.arraySize; i++)
            {
                Object value = property.GetArrayElementAtIndex(i).objectReferenceValue;
                AppendReferencedHierarchyIssue(value, $"{label} {i}", inspectedRoots, issues, category);
            }
        }

        private static void AppendReferencedHierarchyIssue(
            Object reference,
            string label,
            HashSet<GameObject> inspectedRoots,
            List<PyralisSceneReadinessIssue> issues,
            PyralisSceneReadinessCategory category)
        {
            GameObject root = GetReferenceGameObject(reference);
            if (root == null || !inspectedRoots.Add(root))
                return;

            int missingScripts = GetMissingScriptCountInHierarchy(root);
            if (missingScripts > 0)
                AddRequired(
                    issues,
                    $"{label} `{root.name}` has {missingScripts} missing script reference(s) in its hierarchy.",
                    category,
                    "Inspect the named scene object or prefab root and repair missing script references.");
        }

        private static GameObject GetReferenceGameObject(Object reference)
        {
            if (reference is GameObject gameObject)
                return gameObject;

            if (reference is Component component)
                return component.gameObject;

            return null;
        }

        private static void AppendSceneSpriteRendererIssues(
            UnityEngine.SceneManagement.Scene scene,
            HashSet<GameObject> inspectedRoots,
            List<PyralisSceneReadinessIssue> issues)
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

                AddRequired(
                    issues,
                    $"Scene SpriteRenderer `{renderer.gameObject.name}` has no Sprite assigned. Assign a sprite before using Play Mode to inspect scene visuals.",
                    PyralisSceneReadinessCategory.Presentation,
                    "Select the named scene object and assign a Sprite on its SpriteRenderer.");
            }
        }

        private static void AppendNetworkReadinessIssues(
            GameplaySessionBootstrap bootstrap,
            SessionDefinition session,
            List<PyralisSceneReadinessIssue> issues)
        {
            if (session.networkMode == GameplayNetworkMode.LocalOnly)
                return;

            MonoBehaviour networkManager = FindSceneBehaviourByTypeName(bootstrap.gameObject.scene, NetworkManagerTypeName);
            if (networkManager == null)
            {
                AddRequired(
                    issues,
                    "Networked sessions require a scene NetworkManager.",
                    PyralisSceneReadinessCategory.Networking,
                    "Create or assign a NetworkManager in the scene for the networked route.");
            }
            else if (!NetworkManagerUsesUnityTransport(networkManager))
            {
                AddRequired(
                    issues,
                    "Networked sessions require NetworkManager to use UnityTransport for the supported MVP lane.",
                    PyralisSceneReadinessCategory.Networking,
                    "Select NetworkManager and add or assign UnityTransport.");
            }
        }

        private static void AppendMissingScriptIssue(
            GameObject root,
            string label,
            List<PyralisSceneReadinessIssue> issues,
            PyralisSceneReadinessCategory category)
        {
            int missingScripts = GetMissingScriptCountInHierarchy(root);
            if (missingScripts > 0)
                AddRequired(
                    issues,
                    $"{label} has {missingScripts} missing script reference(s).",
                    category,
                    "Inspect the prefab root and repair missing script references.");
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

        private static ParticipantSpawnService GetParticipantSpawnService(GameplaySessionBootstrap bootstrap, SerializedObject serializedBootstrap)
        {
            ParticipantSpawnService service = GetObjectReference<ParticipantSpawnService>(serializedBootstrap, "participantSpawnService");
            if (service != null || bootstrap == null)
                return service;

            return bootstrap.GetComponentInChildren<ParticipantSpawnService>(true);
        }
    }
}
