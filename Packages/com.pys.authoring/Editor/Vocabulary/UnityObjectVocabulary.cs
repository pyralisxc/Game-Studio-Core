namespace Pys.Authoring.Editor.Vocabulary
{
    internal static class UnityObjectVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeAssembly, "Assembly", "Compiled code boundary or assembly definition.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeNamespace, "Namespace", "C# namespace used by a source file.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeType, "Type", "Reflected C# type.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeScript, "Script", "C# source file.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeComponent, "Component", "Unity component type.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeScriptableObject, "ScriptableObject", "Unity asset-backed data type.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeContract, "Contract", "Machine-readable authoring declaration.", "Authoring"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeValidator, "Validator", "Component that reports local authoring issues.", "Authoring"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeSceneObject, "GameObject", "Object in a loaded Unity scene.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodePrefab, "Prefab Asset", "Reusable authored Unity object.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeAsset, "Asset", "Unity project asset.", "Unity"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.NodeIssue, "Issue", "Structured authoring concern.", "Authoring"));
        }
    }

    internal static class UnityPackageVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:Cinemachine", "Cinemachine", "Unity camera composition, follow, look-at, and shot authoring package.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:InputSystem", "Input System", "Unity input actions, devices, players, and control schemes package.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:Timeline", "Timeline", "Unity sequencing package for tracks, clips, activation, animation, audio, and signals.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:VFXGraph", "VFX Graph", "Unity node graph package for authored visual effects.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:ShaderGraph", "Shader Graph", "Unity node graph package for authored shaders and materials.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:AnimationRigging", "Animation Rigging", "Unity runtime rigging and constraint package.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:Addressables", "Addressables", "Unity asset address, loading, and content build package.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:Localization", "Localization", "Unity string, asset table, and locale package.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:Splines", "Splines", "Unity curve and path authoring package.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:UIToolkit", "UI Toolkit", "Unity retained-mode UI authoring system.", "Unity Package"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.package:UGUI", "UGUI", "Unity GameObject and Canvas based UI system.", "Unity Package"));
        }
    }

    internal static class UnityAuthoringWindowVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:Inspector", "Inspector", "Review and edit selected Unity object, component, and asset properties.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:Hierarchy", "Hierarchy", "Inspect and organize scene GameObjects.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:Project", "Project", "Find, create, and organize Unity assets.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:SceneView", "Scene View", "Place, frame, select, and edit scene objects.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:Animator", "Animator", "Author Animator Controller states, transitions, parameters, and layers.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:Animation", "Animation", "Record and edit Animation Clip curves and keyed properties.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:Timeline", "Timeline", "Author sequencing tracks, clips, bindings, and signals.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:VFXGraph", "VFX Graph", "Author node-based visual effects.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:ShaderGraph", "Shader Graph", "Author node-based shaders.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:AudioMixer", "Audio Mixer", "Route, mix, group, snapshot, and expose audio controls.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:Lighting", "Lighting", "Configure scene lighting, environment, and baking settings.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:PackageManager", "Package Manager", "Install and inspect Unity packages.", "Unity Window"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.window:InputActions", "Input Actions", "Author action maps, actions, bindings, and control schemes.", "Unity Window"));
        }
    }

    internal static class UnityRoleVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Camera", "Camera", "Scene view rendered by a Unity Camera component.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:CinemachineCamera", "Cinemachine Camera", "Cinemachine authored camera behavior such as follow, look-at, and composition.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Animator", "Animator", "Component that evaluates an Animator Controller or animation graph.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:AnimationClip", "Animation Clip", "Asset containing keyed animation curves.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:AnimatorController", "Animator Controller", "Asset containing animation states, transitions, parameters, and layers.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:PlayableDirector", "Playable Director", "Component that plays Timeline or Playables content.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:TimelineAsset", "Timeline Asset", "Asset containing sequencing tracks and clips.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:AudioSource", "Audio Source", "Component that plays an Audio Clip through the scene audio pipeline.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:AudioListener", "Audio Listener", "Component that receives scene audio for playback.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:AudioMixer", "Audio Mixer", "Asset that groups, mixes, snapshots, and exposes audio controls.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:VisualEffect", "Visual Effect", "Component that plays a VFX Graph asset.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:ParticleSystem", "Particle System", "Component for built-in particle effects.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Canvas", "Canvas", "Root component for UGUI screen, world, or camera-space UI.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:EventSystem", "Event System", "Scene object that routes UI input events.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Light", "Light", "Scene light source.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Volume", "Volume", "Post-processing and render pipeline override volume.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Rigidbody", "Rigidbody", "3D physics body.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Rigidbody2D", "Rigidbody 2D", "2D physics body.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Collider", "Collider", "3D physics shape.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.role:Collider2D", "Collider 2D", "2D physics shape.", "Unity Role"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:Scene", "Scene", "Unity scene asset.", "Unity Asset"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:Prefab", "Prefab", "Reusable GameObject asset.", "Unity Asset"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:Material", "Material", "Surface rendering settings asset.", "Unity Asset"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:Shader", "Shader", "Rendering program or graph asset.", "Unity Asset"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:Texture", "Texture", "Image asset used by materials, UI, VFX, or sprites.", "Unity Asset"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:Sprite", "Sprite", "2D image asset with sprite import data.", "Unity Asset"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:AudioClip", "Audio Clip", "Audio sample asset.", "Unity Asset"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:InputActions", "Input Actions", "Input System action map and binding asset.", "Unity Asset"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.asset:RenderTexture", "Render Texture", "Texture asset rendered into by a camera or graphics pipeline.", "Unity Asset"));
        }
    }
}
