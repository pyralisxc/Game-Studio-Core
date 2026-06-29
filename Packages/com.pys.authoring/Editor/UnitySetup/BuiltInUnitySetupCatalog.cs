using System;
using System.Collections.Generic;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Vocabulary;
using UnityEditor.PackageManager;

namespace Pys.Authoring.Editor.UnitySetup
{
    internal sealed class BuiltInUnitySetupGuide
    {
        public string Id;
        public string StableId;
        public string DisplayName;
        public string CapabilityPath;
        public string Summary;
        public string SetupSteps;
        public string SuccessDescription;
        public string ReadinessHint;
        public string ExpectedEvidence;
        public string CompletionSignals;
        public string ActionKinds;
        public string IntentToggles;
        public string IntentLanes;
        public string CompatibleStableIds;
        public string SupportingStableIds;
        public string HoverExplanations;
        public string SetupDomain;
        public string RouteStage;
        public string RequiredPackages;
        public string RequiredComponents;
        public string RequiredAssets;
        public string RequiredFields;
        public string RequiredWindows;
    }

    internal static class BuiltInUnitySetupCatalog
    {
        public const string SourceKind = "BuiltInUnitySetup";
        public const int Priority = 100;

        public static IReadOnlyList<BuiltInUnitySetupGuide> Guides()
        {
            return new[]
            {
                Guide(
                    "camera",
                    "Set Up Camera",
                    "Unity/Camera/Basic",
                    "Create a native Unity Camera and make it render the scene clearly.",
                    "Create or select the Camera GameObject.\nPlace the Camera so the subject is visible in Scene View.\nSet Clear Flags, Projection, Field of View or Size, Culling Mask, and Audio Listener intentionally.",
                    "Camera renders the intended scene area in Game view.",
                    "A Camera component exists.\nThe Camera is enabled.\nGame view shows the intended subject.",
                    "Game view is framed and visible.",
                    "CreateGameObject\nSelectInHierarchy\nAssignField",
                    "Scene Camera\nPerspective\nOrthographic",
                    "Rendering\nAudio Listener",
                    "Use this when the project needs a plain Unity Camera without a custom camera system.",
                    "Presentation",
                    "Camera",
                    string.Empty,
                    "Camera\nAudioListener",
                    string.Empty,
                    "Camera.enabled=true\nAudioListener.enabled=true",
                    "Inspector\nHierarchy\nScene View\nGame View"),
                Guide(
                    "cinemachine-follow",
                    "Set Up Cinemachine Follow Camera",
                    "Unity/Cinemachine/Follow Camera",
                    "Create a Cinemachine camera route for follow/look-at composition without gameplay code.",
                    "Confirm the Cinemachine package is installed in Package Manager.\nCreate a Cinemachine Camera or Virtual Camera from the GameObject menu.\nAssign Follow and Look At targets in the Inspector.\nAdjust lens, damping, dead zone, and composition while previewing the Game view.",
                    "A Cinemachine camera follows or looks at the intended target in Play Mode.",
                    "Cinemachine package is installed.\nA Cinemachine camera component exists.\nFollow or Look At references are assigned.",
                    "Camera follows the target and the Game view composition is stable.",
                    "OpenWindow\nCreateGameObject\nBindReference\nPreviewAnimation",
                    "Follow Camera\nLook At Camera\n2D Camera\n3D Camera",
                    "unity.package:Cinemachine\nunity.role:Camera",
                    "Use this for camera setup that should be owned by Unity Cinemachine instead of project code.",
                    "Presentation",
                    "Camera",
                    "com.unity.cinemachine",
                    "Camera\nCinemachineCamera",
                    string.Empty,
                    "Cinemachine.Follow=Assigned\nCinemachine.LookAt=Assigned",
                    "Package Manager\nInspector\nGame View"),
                Guide(
                    "animation-clip",
                    "Set Up Animation Clip",
                    "Unity/Animation/Clip",
                    "Create a basic Animation Clip and key properties through Unity's Animation window.",
                    "Select the animated GameObject.\nOpen Window > Animation > Animation.\nCreate an Animation Clip asset.\nRecord or add keyframes for position, rotation, scale, sprite, material, or component properties.\nPreview the clip and adjust timing.",
                    "An Animation Clip previews correctly on the selected object.",
                    "Animation window is open.\nAnimation Clip asset exists.\nClip contains keyed curves for the intended object.",
                    "Preview shows the intended motion or property change.",
                    "SelectInHierarchy\nOpenWindow\nCreateClip\nPreviewAnimation\nAssignField",
                    "Transform Animation\nSprite Animation\nProperty Animation",
                    "unity.window:Animation\nunity.role:AnimationClip",
                    "Use this for authored motion or property changes before custom animation code exists.",
                    "Animation",
                    "Animation",
                    string.Empty,
                    "Animator",
                    "AnimationClip",
                    string.Empty,
                    "Animation\nInspector\nProject"),
                Guide(
                    "animator-controller",
                    "Set Up Animator Controller",
                    "Unity/Animation/Animator Controller",
                    "Create an Animator Controller and connect clips, states, parameters, and transitions.",
                    "Create an Animator Controller asset in the Project window.\nAssign it to an Animator component on the GameObject.\nOpen the Animator window.\nAdd states for the required clips.\nAdd parameters and transitions only for the behavior you need now.",
                    "Animator evaluates the intended state or transition in Play Mode.",
                    "Animator component exists.\nAnimator Controller asset is assigned.\nAt least one state references an Animation Clip.",
                    "Animator enters the expected state and plays the expected clip.",
                    "CreateController\nAssignField\nOpenGraphAsset\nBindReference\nRunPlayModeCheck",
                    "State Machine\nParameters\nTransitions\nLayers",
                    "unity.window:Animator\nunity.role:AnimatorController",
                    "Use this when Unity's Animator should own animation state before gameplay code reacts to it.",
                    "Animation",
                    "Animation",
                    string.Empty,
                    "Animator",
                    "AnimatorController\nAnimationClip",
                    "Animator.runtimeAnimatorController=Assigned",
                    "Animator\nInspector\nProject"),
                Guide(
                    "timeline",
                    "Set Up Timeline Sequence",
                    "Unity/Timeline/Sequence",
                    "Create a Timeline sequence with native track bindings.",
                    "Create or select a GameObject with Playable Director.\nCreate a Timeline Asset.\nOpen the Timeline window.\nAdd Animation, Activation, Audio, Signal, or Control tracks as needed.\nBind each track to the correct scene object or asset.",
                    "Playable Director plays the Timeline and all required tracks are bound.",
                    "Playable Director component exists.\nTimeline Asset is assigned.\nTrack bindings are not missing.",
                    "Timeline plays from the Playable Director without missing binding warnings.",
                    "CreateComponent\nCreateAsset\nOpenWindow\nAssignTrack\nRunPlayModeCheck",
                    "Animation Track\nAudio Track\nActivation Track\nSignal Track\nControl Track",
                    "unity.package:Timeline\nunity.role:PlayableDirector",
                    "Use this for scene sequencing, cutscenes, activation timing, and audio timing without custom sequence code.",
                    "Timeline",
                    "Sequencing",
                    "com.unity.timeline",
                    "PlayableDirector",
                    "TimelineAsset",
                    "PlayableDirector.playableAsset=Assigned",
                    "Timeline\nInspector\nProject"),
                Guide(
                    "audio-source",
                    "Set Up Sound Playback",
                    "Unity/Audio/Audio Source",
                    "Create native Unity sound playback with an Audio Source and Audio Clip.",
                    "Add or select an Audio Source component.\nAssign an Audio Clip.\nSet Play On Awake, Loop, Spatial Blend, Volume, and Output intentionally.\nConfirm an Audio Listener exists in the scene.",
                    "The intended sound plays through the scene audio pipeline.",
                    "Audio Source exists.\nAudio Clip is assigned.\nAudio Listener exists.",
                    "Audio is audible with the intended volume, loop, and spatial settings.",
                    "AddComponent\nAssignField\nInspectObject\nRunPlayModeCheck",
                    "2D Sound\n3D Sound\nLooping Sound\nOne-shot Sound",
                    "unity.role:AudioSource\nunity.role:AudioListener",
                    "Use this when a project needs sound playback before custom audio systems are introduced.",
                    "Audio",
                    "Sound",
                    string.Empty,
                    "AudioSource\nAudioListener",
                    "AudioClip",
                    "AudioSource.clip=Assigned\nAudioSource.enabled=true\nAudioListener.enabled=true",
                    "Inspector\nProject"),
                Guide(
                    "audio-mixer",
                    "Set Up Audio Mixer Routing",
                    "Unity/Audio/Mixer",
                    "Route Audio Sources through a Unity Audio Mixer for grouped control.",
                    "Create an Audio Mixer asset.\nCreate groups for music, SFX, UI, ambience, or voice as needed.\nAssign Audio Source Output fields to the correct mixer groups.\nCreate snapshots or exposed parameters only when they are needed.",
                    "Audio Sources route through the intended mixer groups.",
                    "Audio Mixer asset exists.\nMixer groups are named clearly.\nAudio Source Output fields point to mixer groups.",
                    "Changing mixer group volume affects the intended Audio Sources.",
                    "CreateAsset\nOpenWindow\nAssignField\nRunPlayModeCheck",
                    "Music\nSFX\nUI\nAmbience\nVoice",
                    "unity.window:AudioMixer\nunity.role:AudioMixer",
                    "Use this for beginner-friendly audio organization that stays native to Unity.",
                    "Audio",
                    "Sound",
                    string.Empty,
                    "AudioSource",
                    "AudioMixer",
                    "AudioSource.outputAudioMixerGroup=Assigned",
                    "Audio Mixer\nInspector\nProject"),
                Guide(
                    "vfx-graph",
                    "Set Up VFX Graph Effect",
                    "Unity/VFX Graph/Effect",
                    "Create a VFX Graph asset and bind it to a Visual Effect component.",
                    "Confirm Visual Effect Graph is installed and supported by the render pipeline.\nCreate a VFX Graph asset.\nAdd a Visual Effect component to a GameObject.\nAssign the graph asset and tune exposed properties.\nPreview the effect in Scene or Play Mode.",
                    "Visual Effect component plays the assigned VFX Graph.",
                    "VFX Graph asset exists.\nVisual Effect component exists.\nGraph asset is assigned.",
                    "Effect is visible and updates in Scene or Play Mode.",
                    "OpenWindow\nCreateAsset\nAddComponent\nOpenGraphAsset\nRunPlayModeCheck",
                    "Spawn\nParticles\nExposed Properties\nBindings",
                    "unity.package:VFXGraph\nunity.role:VisualEffect",
                    "Use this when the effect should be authored in Unity's VFX Graph instead of custom effect code.",
                    "VFX",
                    "VFX",
                    "com.unity.visualeffectgraph",
                    "VisualEffect",
                    "VisualEffectAsset",
                    "VisualEffect.visualEffectAsset=Assigned",
                    "VFX Graph\nInspector\nProject"),
                Guide(
                    "particle-system",
                    "Set Up Particle System Effect",
                    "Unity/VFX/Particle System",
                    "Create a built-in Particle System effect using native Unity modules.",
                    "Create or select a Particle System GameObject.\nTune Main, Emission, Shape, Renderer, and Lifetime modules.\nAssign material and sorting/layer settings intentionally.\nPreview the effect in Scene View.",
                    "Particle System previews the intended effect.",
                    "Particle System component exists.\nRenderer module has usable material and visibility settings.\nThe effect is visible in Scene View.",
                    "Effect appears with the intended timing, shape, and material.",
                    "CreateGameObject\nInspectObject\nAssignField\nPreviewAnimation",
                    "Burst\nLooping\nOne-shot\nWorld Space\nLocal Space",
                    "unity.role:ParticleSystem\nunity.asset:Material",
                    "Use this for native Unity particle effects that do not need VFX Graph.",
                    "VFX",
                    "VFX",
                    string.Empty,
                    "ParticleSystem",
                    "Material",
                    "ParticleSystemRenderer.material=Assigned",
                    "Inspector\nScene View"),
                Guide(
                    "ui-canvas",
                    "Set Up UI Canvas",
                    "Unity/UI/Canvas",
                    "Create a native Unity UI Canvas with an Event System and basic screen structure.",
                    "Create a Canvas from the GameObject UI menu.\nConfirm an Event System exists.\nChoose Screen Space Overlay, Screen Space Camera, or World Space.\nAdd UI elements and set anchors, pivots, scale mode, and sorting intentionally.",
                    "UI appears in Game view and responds to pointer or input events when needed.",
                    "Canvas exists.\nEvent System exists.\nUI elements are anchored and visible.",
                    "Game view shows the UI at the intended size and interaction route.",
                    "CreateGameObject\nInspectObject\nAssignField\nRunPlayModeCheck",
                    "Screen Space Overlay\nScreen Space Camera\nWorld Space\nUGUI\nUI Toolkit",
                    "unity.role:Canvas\nunity.role:EventSystem",
                    "Use this for native Unity UI setup before project-specific UI presenters exist.",
                    "UI",
                    "UI",
                    string.Empty,
                    "Canvas\nEventSystem",
                    string.Empty,
                    "Canvas.enabled=true\nEventSystem.enabled=true",
                    "Inspector\nHierarchy\nGame View"),
                Guide(
                    "input-actions",
                    "Set Up Input Actions",
                    "Unity/Input System/Input Actions",
                    "Create an Input Actions asset with action maps, bindings, and control schemes.",
                    "Confirm the Input System package is installed.\nCreate an Input Actions asset.\nAdd action maps, actions, and bindings with clear names.\nGenerate or assign references only if the project needs them.\nTest bindings through the Input Actions editor or Play Mode.",
                    "Input Actions asset contains the bindings the scene or component expects.",
                    "Input System package is installed.\nInput Actions asset exists.\nAction maps and bindings are named and saved.",
                    "Expected controls trigger the intended actions in Play Mode or the Input Debugger.",
                    "OpenWindow\nCreateController\nOpenAsset\nBindReference\nRunPlayModeCheck",
                    "Keyboard\nMouse\nGamepad\nTouch\nLocal Multiplayer",
                    "unity.package:InputSystem\nunity.asset:InputActions",
                    "Use this when input can be authored in Unity before target code consumes it.",
                    "Input",
                    "Input",
                    "com.unity.inputsystem",
                    string.Empty,
                    "InputActionAsset",
                    string.Empty,
                    "Input Actions\nInput Debugger\nProject"),
                Guide(
                    "prefab",
                    "Set Up Prefab",
                    "Unity/Prefab/Create Prefab",
                    "Turn a scene object into a reusable Unity Prefab asset.",
                    "Select the scene GameObject.\nConfirm components and child objects are intentionally arranged.\nCreate a prefab asset through the Project window or prefab creation controls.\nOpen Prefab Mode to verify overrides and references.",
                    "Prefab asset exists and can be instantiated without missing references.",
                    "Scene object exists.\nPrefab asset exists.\nPrefab has expected components and child objects.",
                    "Prefab opens cleanly and instances preserve intended references.",
                    "SelectInHierarchy\nCreatePrefab\nOpenAsset\nInspectObject",
                    "Scene Object\nPrefab Asset\nPrefab Mode\nOverrides",
                    "unity.asset:Prefab\nunity.window:Project",
                    "Use this for reusable scene content before code-specific factories exist.",
                    "Assets",
                    "Prefab",
                    string.Empty,
                    string.Empty,
                    "Prefab",
                    string.Empty,
                    "Hierarchy\nProject\nInspector"),
                Guide(
                    "lighting-volume",
                    "Set Up Lighting And Volume",
                    "Unity/Lighting/Scene Look",
                    "Set up native Unity scene lighting, environment, and post-processing volume basics.",
                    "Open the Lighting window.\nSet skybox, environment lighting, and reflection settings intentionally.\nCreate or select Light components.\nCreate a Volume if the render pipeline supports it.\nAssign a Volume Profile and add only the overrides needed now.",
                    "Scene lighting and volume settings produce the intended Game view look.",
                    "Lighting settings are configured.\nLight components exist where needed.\nVolume and profile exist when post-processing is needed.",
                    "Game view shows the intended exposure, color, lighting, and post-processing.",
                    "OpenWindow\nCreateGameObject\nCreateAsset\nAssignField\nRunPlayModeCheck",
                    "Directional Light\nPoint Light\nSpot Light\nGlobal Volume\nLocal Volume",
                    "unity.window:Lighting\nunity.role:Light\nunity.role:Volume",
                    "Use this for native scene look setup before custom rendering or gameplay systems are involved.",
                    "Rendering",
                    "Lighting",
                    string.Empty,
                    "Light\nVolume",
                    "VolumeProfile",
                    "Light.enabled=true",
                    "Lighting\nInspector\nGame View")
            };
        }

        private static BuiltInUnitySetupGuide Guide(
            string id,
            string displayName,
            string capabilityPath,
            string summary,
            string setupSteps,
            string successDescription,
            string expectedEvidence,
            string completionSignals,
            string actionKinds,
            string intentLanes,
            string supportingStableIds,
            string hoverExplanations,
            string setupDomain,
            string routeStage,
            string requiredPackages,
            string requiredComponents,
            string requiredAssets,
            string requiredFields,
            string requiredWindows)
        {
            return new BuiltInUnitySetupGuide
            {
                Id = "contract:unity.setup." + id,
                StableId = "unity.setup." + id,
                DisplayName = displayName,
                CapabilityPath = capabilityPath,
                Summary = summary,
                SetupSteps = setupSteps,
                SuccessDescription = successDescription,
                ReadinessHint = "Use native Unity windows and Inspector fields. No target-project code is required unless the project later chooses to consume this setup.",
                ExpectedEvidence = expectedEvidence,
                CompletionSignals = completionSignals,
                ActionKinds = actionKinds,
                IntentToggles = "Use Unity native workflow\nNo target code required",
                IntentLanes = intentLanes,
                CompatibleStableIds = "Target contracts may consume this setup when present.",
                SupportingStableIds = supportingStableIds,
                HoverExplanations = hoverExplanations,
                SetupDomain = setupDomain,
                RouteStage = routeStage,
                RequiredPackages = requiredPackages,
                RequiredComponents = requiredComponents,
                RequiredAssets = requiredAssets,
                RequiredFields = requiredFields,
                RequiredWindows = requiredWindows
            };
        }
    }

    internal static class BuiltInUnitySetupGraphContributor
    {
        public static void AddTo(AuthoringGraph graph, AuthoringVocabularyDictionary vocabulary)
        {
            AddTo(graph, vocabulary, IsPackageInstalled);
        }

        internal static void AddTo(AuthoringGraph graph, AuthoringVocabularyDictionary vocabulary, Func<string, bool> isPackageInstalled)
        {
            if (graph == null)
                return;

            IReadOnlyList<BuiltInUnitySetupGuide> guides = BuiltInUnitySetupCatalog.Guides();
            for (int i = 0; i < guides.Count; i++)
                AddGuide(graph, vocabulary, guides[i], i + 1, isPackageInstalled);

            AddReadinessEvidence(graph, vocabulary);
        }

        private static void AddGuide(AuthoringGraph graph, AuthoringVocabularyDictionary vocabulary, BuiltInUnitySetupGuide guide, int routeOrder, Func<string, bool> isPackageInstalled)
        {
            if (guide == null)
                return;

            AuthoringGraphNode node = new AuthoringGraphNode(guide.Id, guide.DisplayName, AuthoringGraphNodeKind.Contract);
            node.Metadata["kindLabel"] = vocabulary != null ? vocabulary.Label(AuthoringVocabularyKey.Node(AuthoringGraphNodeKind.Contract), AuthoringGraphNodeKind.Contract.ToString()) : AuthoringGraphNodeKind.Contract.ToString();
            node.Metadata["stableId"] = guide.StableId;
            node.Metadata["sourceKind"] = BuiltInUnitySetupCatalog.SourceKind;
            node.Metadata["sourceType"] = "PYS Built-In Unity Setup";
            node.Metadata["sourcePath"] = "Packages/com.pys.authoring/Editor/UnitySetup/BuiltInUnitySetupCatalog.cs";
            node.Metadata["intentSource"] = BuiltInUnitySetupCatalog.SourceKind;
            node.Metadata["priority"] = BuiltInUnitySetupCatalog.Priority.ToString();
            node.Metadata["category"] = "Unity Setup";
            node.Metadata["capabilityPath"] = guide.CapabilityPath;
            node.Metadata["surface"] = "NativeSetup";
            node.Metadata["selectable"] = "true";
            node.Metadata["summary"] = guide.Summary;
            node.Metadata["setupGuideKind"] = "UnitySetupGuide";
            node.Metadata["routeStage"] = guide.RouteStage;
            node.Metadata["routeOrder"] = routeOrder.ToString();
            node.Metadata["setupDomain"] = guide.SetupDomain;
            node.Metadata["successDescription"] = guide.SuccessDescription;
            node.Metadata["readinessHint"] = guide.ReadinessHint;
            node.Metadata["expectedEvidence"] = guide.ExpectedEvidence;
            node.Metadata["completionSignals"] = guide.CompletionSignals;
            node.Metadata["intentToggles"] = guide.IntentToggles;
            node.Metadata["intentLanes"] = guide.IntentLanes;
            node.Metadata["compatibleStableIds"] = guide.CompatibleStableIds;
            node.Metadata["supportingStableIds"] = guide.SupportingStableIds;
            node.Metadata["hoverExplanations"] = guide.HoverExplanations;
            node.Metadata["setupSteps"] = guide.SetupSteps;
            node.Metadata["actionKinds"] = guide.ActionKinds;
            node.Metadata["requiredPackages"] = guide.RequiredPackages;
            node.Metadata["requiredComponents"] = guide.RequiredComponents;
            node.Metadata["requiredAssets"] = guide.RequiredAssets;
            node.Metadata["requiredFields"] = guide.RequiredFields;
            node.Metadata["requiredWindows"] = guide.RequiredWindows;
            string[] requiredPackages = SplitLines(guide.RequiredPackages);
            node.Metadata["packageAvailability"] = PackageAvailability(requiredPackages, isPackageInstalled);
            node.Metadata["availability"] = HasMissingRequiredPackage(requiredPackages, isPackageInstalled) ? "MissingPackage" : "Ready";
            graph.Nodes.Add(node);

            AddMissingPackageIssues(graph, guide, requiredPackages, isPackageInstalled);
        }

        private static void AddMissingPackageIssues(AuthoringGraph graph, BuiltInUnitySetupGuide guide, string[] requiredPackages, Func<string, bool> isPackageInstalled)
        {
            if (graph == null || guide == null || requiredPackages == null)
                return;

            for (int i = 0; i < requiredPackages.Length; i++)
            {
                string packageName = requiredPackages[i];
                if (string.IsNullOrWhiteSpace(packageName) || IsPackageAvailable(packageName, isPackageInstalled))
                    continue;

                string issueId = "issue:" + guide.StableId + ":missing-package:" + packageName;
                AuthoringGraphNode issue = new AuthoringGraphNode(issueId, "Install required Unity package", AuthoringGraphNodeKind.Issue);
                issue.Metadata["kindLabel"] = AuthoringGraphNodeKind.Issue.ToString();
                issue.Metadata["issueCode"] = "UnitySetup.Package.Missing";
                issue.Metadata["severity"] = AuthoringIssueSeverity.Required.ToString();
                issue.Metadata["ownerStableId"] = guide.StableId;
                issue.Metadata["relatedStableIds"] = guide.StableId;
                issue.Metadata["targetLabel"] = guide.DisplayName;
                issue.Metadata["fieldPath"] = packageName;
                issue.Metadata["nativeAction"] = "Open Package Manager and install " + packageName + ".";
                issue.Metadata["successCheck"] = packageName + " is installed.";
                issue.Metadata["actionKind"] = AuthoringActionKind.OpenWindow.ToString();
                issue.Metadata["sourceKind"] = BuiltInUnitySetupCatalog.SourceKind;
                issue.Metadata["sourcePath"] = "Packages/com.pys.authoring/Editor/UnitySetup/BuiltInUnitySetupCatalog.cs";
                graph.Nodes.Add(issue);
                graph.Edges.Add(new AuthoringGraphEdge(guide.Id, issueId, AuthoringGraphEdgeKind.ValidatorReports));
            }
        }

        private static bool HasMissingRequiredPackage(string[] requiredPackages, Func<string, bool> isPackageInstalled)
        {
            if (requiredPackages == null)
                return false;

            for (int i = 0; i < requiredPackages.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(requiredPackages[i]) && !IsPackageAvailable(requiredPackages[i], isPackageInstalled))
                    return true;
            }

            return false;
        }

        private static string PackageAvailability(string[] requiredPackages, Func<string, bool> isPackageInstalled)
        {
            if (requiredPackages == null || requiredPackages.Length == 0)
                return string.Empty;

            List<string> rows = new List<string>();
            for (int i = 0; i < requiredPackages.Length; i++)
            {
                string packageName = requiredPackages[i];
                if (string.IsNullOrWhiteSpace(packageName))
                    continue;

                rows.Add(packageName + ": " + (IsPackageAvailable(packageName, isPackageInstalled) ? "Installed" : "Missing"));
            }

            return string.Join("\n", rows.ToArray());
        }

        private static string[] SplitLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];

            string[] raw = value.Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> split = new List<string>();
            for (int i = 0; i < raw.Length; i++)
            {
                string trimmed = raw[i].Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    split.Add(trimmed);
            }

            return split.ToArray();
        }

        private static bool IsPackageAvailable(string packageName, Func<string, bool> isPackageInstalled)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return true;

            return isPackageInstalled == null || isPackageInstalled(packageName.Trim());
        }

        private static bool IsPackageInstalled(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return true;

            return PackageInfo.FindForPackageName(packageName.Trim()) != null;
        }

        private static void AddReadinessEvidence(AuthoringGraph graph, AuthoringVocabularyDictionary vocabulary)
        {
            if (graph == null)
                return;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode contract = graph.Nodes[i];
                if (contract == null
                    || contract.Kind != AuthoringGraphNodeKind.Contract
                    || Metadata(contract, "sourceKind") != BuiltInUnitySetupCatalog.SourceKind)
                {
                    continue;
                }

                string[] requiredComponents = SplitLines(Metadata(contract, "requiredComponents"));
                string[] requiredAssets = SplitLines(Metadata(contract, "requiredAssets"));
                string[] requiredFields = SplitLines(Metadata(contract, "requiredFields"));
                string[] observedComponents = ObservedComponents(graph, requiredComponents);
                string[] observedAssets = ObservedAssets(graph, requiredAssets);
                string[] observedFields = ObservedFields(graph, requiredFields);
                string[] missingComponents = MissingValues(requiredComponents, observedComponents);
                string[] missingAssets = MissingValues(requiredAssets, observedAssets);
                string[] missingFields = MissingValues(requiredFields, observedFields);

                contract.Metadata["observedComponents"] = string.Join("\n", observedComponents);
                contract.Metadata["missingComponents"] = string.Join("\n", missingComponents);
                contract.Metadata["observedAssets"] = string.Join("\n", observedAssets);
                contract.Metadata["missingAssets"] = string.Join("\n", missingAssets);
                contract.Metadata["observedFields"] = string.Join("\n", observedFields);
                contract.Metadata["missingFields"] = string.Join("\n", missingFields);
                contract.Metadata["readinessState"] = ReadinessState(requiredComponents, requiredAssets, requiredFields, missingComponents, missingAssets, missingFields);
                contract.Metadata["readinessEvidenceSummary"] = ReadinessSummary(vocabulary, requiredComponents, requiredAssets, requiredFields, observedComponents, observedAssets, observedFields, missingComponents, missingAssets, missingFields);
            }
        }

        private static string[] ObservedComponents(AuthoringGraph graph, string[] requiredComponents)
        {
            List<string> observed = new List<string>();
            if (requiredComponents == null)
                return observed.ToArray();

            for (int i = 0; i < requiredComponents.Length; i++)
            {
                string required = requiredComponents[i];
                if (string.IsNullOrWhiteSpace(required))
                    continue;

                if (HasObservedComponent(graph, required))
                    observed.Add(required);
            }

            return observed.ToArray();
        }

        private static bool HasObservedComponent(AuthoringGraph graph, string requiredComponent)
        {
            if (graph == null || string.IsNullOrWhiteSpace(requiredComponent))
                return false;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null || node.Kind != AuthoringGraphNodeKind.Component)
                    continue;

                if (MatchesUnityType(node.Label, requiredComponent) || MatchesUnityType(NodeIdWithoutPrefix(node.Id, "component:"), requiredComponent))
                    return true;
            }

            return false;
        }

        private static string[] ObservedAssets(AuthoringGraph graph, string[] requiredAssets)
        {
            List<string> observed = new List<string>();
            if (requiredAssets == null)
                return observed.ToArray();

            for (int i = 0; i < requiredAssets.Length; i++)
            {
                string required = requiredAssets[i];
                if (string.IsNullOrWhiteSpace(required))
                    continue;

                if (HasObservedAsset(graph, required))
                    observed.Add(required);
            }

            return observed.ToArray();
        }

        private static string[] ObservedFields(AuthoringGraph graph, string[] requiredFields)
        {
            List<string> observed = new List<string>();
            if (requiredFields == null)
                return observed.ToArray();

            for (int i = 0; i < requiredFields.Length; i++)
            {
                string required = requiredFields[i];
                if (string.IsNullOrWhiteSpace(required))
                    continue;

                if (HasObservedField(graph, required))
                    observed.Add(required);
            }

            return observed.ToArray();
        }

        private static bool HasObservedField(AuthoringGraph graph, string requiredField)
        {
            RequiredFieldRequirement requirement = RequiredFieldRequirement.Parse(requiredField);
            if (!requirement.IsValid || graph == null)
                return false;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null || (node.Kind != AuthoringGraphNodeKind.SceneObject && node.Kind != AuthoringGraphNodeKind.Prefab))
                    continue;

                string[] observedFields = SplitLines(Metadata(node, "componentFields"));
                for (int fieldIndex = 0; fieldIndex < observedFields.Length; fieldIndex++)
                {
                    if (FieldMatches(observedFields[fieldIndex], requirement))
                        return true;
                }
            }

            return false;
        }

        private static bool FieldMatches(string observedField, RequiredFieldRequirement requirement)
        {
            RequiredFieldRequirement observed = RequiredFieldRequirement.Parse(observedField);
            if (!observed.IsValid || !requirement.IsValid)
                return false;

            return ComponentNameMatches(observed.ComponentName, requirement.ComponentName)
                && string.Equals(observed.FieldName, requirement.FieldName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(observed.ExpectedValue, requirement.ExpectedValue, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ComponentNameMatches(string observedComponentName, string requiredComponentName)
        {
            if (string.IsNullOrWhiteSpace(observedComponentName) || string.IsNullOrWhiteSpace(requiredComponentName))
                return false;

            string observed = observedComponentName.Trim();
            string required = requiredComponentName.Trim();
            return string.Equals(observed, required, StringComparison.OrdinalIgnoreCase)
                || observed.EndsWith("." + required, StringComparison.OrdinalIgnoreCase)
                || observed.EndsWith("+" + required, StringComparison.OrdinalIgnoreCase)
                || observed.IndexOf(required, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool HasObservedAsset(AuthoringGraph graph, string requiredAsset)
        {
            if (graph == null || string.IsNullOrWhiteSpace(requiredAsset))
                return false;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null)
                    continue;

                if (requiredAsset == "Prefab" && node.Kind == AuthoringGraphNodeKind.Prefab)
                    return true;

                if (node.Kind != AuthoringGraphNodeKind.Asset)
                    continue;

                string type = Metadata(node, "type");
                string sourcePath = Metadata(node, "sourcePath");
                if (MatchesUnityType(type, requiredAsset)
                    || MatchesUnityType(node.Label, requiredAsset)
                    || (!string.IsNullOrWhiteSpace(sourcePath) && sourcePath.EndsWith("." + requiredAsset, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] MissingValues(string[] required, string[] observed)
        {
            List<string> missing = new List<string>();
            if (required == null)
                return missing.ToArray();

            HashSet<string> observedSet = new HashSet<string>(observed ?? new string[0]);
            for (int i = 0; i < required.Length; i++)
            {
                string value = required[i];
                if (!string.IsNullOrWhiteSpace(value) && !observedSet.Contains(value))
                    missing.Add(value);
            }

            return missing.ToArray();
        }

        private static string ReadinessState(string[] requiredComponents, string[] requiredAssets, string[] requiredFields, string[] missingComponents, string[] missingAssets, string[] missingFields)
        {
            int requiredCount = Count(requiredComponents) + Count(requiredAssets) + Count(requiredFields);
            int missingCount = Count(missingComponents) + Count(missingAssets) + Count(missingFields);
            if (requiredCount == 0)
                return "NoRequiredEvidence";

            if (missingCount == 0)
                return "Observed";

            if (missingCount < requiredCount)
                return "Partial";

            return "MissingEvidence";
        }

        private static string ReadinessSummary(AuthoringVocabularyDictionary vocabulary, string[] requiredComponents, string[] requiredAssets, string[] requiredFields, string[] observedComponents, string[] observedAssets, string[] observedFields, string[] missingComponents, string[] missingAssets, string[] missingFields)
        {
            return "Observed components: " + JoinOrNone(observedComponents)
                + "\nMissing components: " + JoinOrNone(missingComponents)
                + "\nObserved assets: " + JoinOrNone(observedAssets)
                + "\nMissing assets: " + JoinOrNone(missingAssets)
                + "\nObserved fields: " + JoinFieldEvidence(vocabulary, observedFields)
                + "\nMissing fields: " + JoinFieldEvidence(vocabulary, missingFields)
                + "\nRequired components: " + JoinOrNone(requiredComponents)
                + "\nRequired assets: " + JoinOrNone(requiredAssets)
                + "\nRequired fields: " + JoinFieldEvidence(vocabulary, requiredFields);
        }

        private static int Count(string[] values)
        {
            return values != null ? values.Length : 0;
        }

        private static string JoinOrNone(string[] values)
        {
            return values != null && values.Length > 0 ? string.Join(", ", values) : "none";
        }

        private static string JoinFieldEvidence(AuthoringVocabularyDictionary vocabulary, string[] values)
        {
            if (values == null || values.Length == 0)
                return "none";

            List<string> labels = new List<string>();
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                RequiredFieldRequirement requirement = RequiredFieldRequirement.Parse(value);
                if (!requirement.IsValid)
                {
                    labels.Add(value);
                    continue;
                }

                string fieldLabel = vocabulary != null
                    ? vocabulary.Label("unity.field:" + requirement.ComponentName + "." + requirement.FieldName, requirement.ComponentName + "." + requirement.FieldName)
                    : requirement.ComponentName + "." + requirement.FieldName;
                string stateLabel = vocabulary != null
                    ? vocabulary.Label("unity.state:" + requirement.ExpectedValue, requirement.ExpectedValue)
                    : requirement.ExpectedValue;
                labels.Add(fieldLabel + ": " + stateLabel + " (" + value + ")");
            }

            return string.Join(", ", labels.ToArray());
        }

        private static bool MatchesUnityType(string observed, string required)
        {
            if (string.IsNullOrWhiteSpace(observed) || string.IsNullOrWhiteSpace(required))
                return false;

            string trimmedObserved = observed.Trim();
            string trimmedRequired = required.Trim();
            return string.Equals(trimmedObserved, trimmedRequired, StringComparison.OrdinalIgnoreCase)
                || trimmedObserved.EndsWith("." + trimmedRequired, StringComparison.OrdinalIgnoreCase)
                || trimmedObserved.EndsWith("+" + trimmedRequired, StringComparison.OrdinalIgnoreCase);
        }

        private static string NodeIdWithoutPrefix(string nodeId, string prefix)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || string.IsNullOrWhiteSpace(prefix) || !nodeId.StartsWith(prefix, StringComparison.Ordinal))
                return nodeId ?? string.Empty;

            return nodeId.Substring(prefix.Length);
        }

        private static string Metadata(AuthoringGraphNode node, string key)
        {
            if (node == null || string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return node.Metadata.TryGetValue(key, out string value) ? value ?? string.Empty : string.Empty;
        }

        private struct RequiredFieldRequirement
        {
            public string ComponentName;
            public string FieldName;
            public string ExpectedValue;
            public bool IsValid;

            public static RequiredFieldRequirement Parse(string value)
            {
                RequiredFieldRequirement requirement = new RequiredFieldRequirement();
                if (string.IsNullOrWhiteSpace(value))
                    return requirement;

                int equalsIndex = value.IndexOf('=');
                if (equalsIndex <= 0 || equalsIndex >= value.Length - 1)
                    return requirement;

                string left = value.Substring(0, equalsIndex).Trim();
                string expectedValue = value.Substring(equalsIndex + 1).Trim();
                int dotIndex = left.LastIndexOf('.');
                if (dotIndex <= 0 || dotIndex >= left.Length - 1)
                    return requirement;

                requirement.ComponentName = left.Substring(0, dotIndex).Trim();
                requirement.FieldName = left.Substring(dotIndex + 1).Trim();
                requirement.ExpectedValue = expectedValue;
                requirement.IsValid = !string.IsNullOrWhiteSpace(requirement.ComponentName)
                    && !string.IsNullOrWhiteSpace(requirement.FieldName)
                    && !string.IsNullOrWhiteSpace(requirement.ExpectedValue);
                return requirement;
            }
        }
    }
}
