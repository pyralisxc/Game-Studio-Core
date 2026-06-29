namespace Pys.Authoring.Editor.Vocabulary
{
    internal static class UnitySetupVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            AddDomains(dictionary);
            AddFields(dictionary);
            AddReadiness(dictionary);
            AddBindings(dictionary);
        }

        private static void AddDomains(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:Camera", "Camera", "Unity camera rendering, composition, follow, and look-at setup.", "Unity Setup Domain"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:Animation", "Animation", "Unity Animation Clip and Animator Controller setup.", "Unity Setup Domain"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:Timeline", "Timeline", "Unity Playable Director and Timeline Asset sequencing setup.", "Unity Setup Domain"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:Audio", "Audio", "Unity Audio Source, Audio Listener, Audio Clip, and Audio Mixer setup.", "Unity Setup Domain"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:VFX", "VFX", "Unity Particle System and Visual Effect setup.", "Unity Setup Domain"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:UI", "UI", "Unity Canvas and Event System setup.", "Unity Setup Domain"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:Input", "Input", "Unity Input Actions and input debugging setup.", "Unity Setup Domain"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:Lighting", "Lighting", "Unity Light, Volume, environment, and scene look setup.", "Unity Setup Domain"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.setup.domain:Assets", "Assets", "Unity prefab, material, clip, controller, and reusable project asset setup.", "Unity Setup Domain"));
        }

        private static void AddFields(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:Camera.enabled", "Camera Enabled", "Camera component is enabled and can render.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:AudioListener.enabled", "Audio Listener Enabled", "Audio Listener is enabled and can receive scene audio.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:AudioSource.enabled", "Audio Source Enabled", "Audio Source is enabled and can play audio.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:AudioSource.clip", "Audio Clip Assigned", "Audio Source has an Audio Clip reference.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:AudioSource.outputAudioMixerGroup", "Audio Mixer Group Assigned", "Audio Source routes to an Audio Mixer Group.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:Animator.runtimeAnimatorController", "Animator Controller Assigned", "Animator has a Runtime Animator Controller.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:Animator.avatar", "Animator Avatar Assigned", "Animator has an Avatar when humanoid or avatar-based animation requires one.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:PlayableDirector.playableAsset", "Timeline Asset Assigned", "Playable Director has a Timeline or Playable asset.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:VisualEffect.visualEffectAsset", "VFX Graph Asset Assigned", "Visual Effect component has a VFX Graph asset.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:ParticleSystemRenderer.material", "Particle Material Assigned", "Particle System renderer has a material.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:Canvas.enabled", "Canvas Enabled", "Canvas is enabled and can render UI.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:EventSystem.enabled", "Event System Enabled", "Event System is enabled and can route UI input.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:Light.enabled", "Light Enabled", "Light component is enabled and can affect the scene.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:Cinemachine.Follow", "Cinemachine Follow Target", "Cinemachine camera has a Follow target.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:Cinemachine.LookAt", "Cinemachine Look At Target", "Cinemachine camera has a Look At target.", "Unity Field"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.field:Cinemachine.TrackingTarget", "Cinemachine Tracking Target", "Cinemachine camera has a tracking target.", "Unity Field"));
        }

        private static void AddReadiness(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("unity.readiness:Observed", "Observed", "All required native Unity evidence for this setup guide was observed.", "Unity Readiness"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.readiness:Partial", "Partial", "Some required native Unity evidence was observed and some is still missing.", "Unity Readiness"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.readiness:MissingEvidence", "Missing Evidence", "Required native Unity evidence was not observed.", "Unity Readiness"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.readiness:NoRequiredEvidence", "No Required Evidence", "This guide does not declare required native Unity evidence.", "Unity Readiness"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.state:Assigned", "Assigned", "A Unity object reference is assigned.", "Unity Readiness"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.state:Missing", "Missing", "A Unity object reference is not assigned.", "Unity Readiness"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.state:true", "Enabled", "A Unity boolean property is enabled.", "Unity Readiness"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.state:false", "Disabled", "A Unity boolean property is disabled.", "Unity Readiness"));
        }

        private static void AddBindings(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("unity.binding:Target", "Target Binding", "A scene object, component, or asset reference used by a native Unity system.", "Unity Binding"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.binding:Track", "Track Binding", "A Timeline, Animation, Audio, Control, Signal, or VFX track binding.", "Unity Binding"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.binding:PlayableAsset", "Playable Asset Binding", "Playable Director link to a Timeline or Playable asset.", "Unity Binding"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.binding:Controller", "Controller Binding", "Component link to a controller asset such as Animator Controller or Input Actions.", "Unity Binding"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.binding:Clip", "Clip Binding", "Component link to an Animation Clip, Audio Clip, or other clip asset.", "Unity Binding"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.binding:Material", "Material Binding", "Renderer or effect link to a material asset.", "Unity Binding"));
            dictionary.Add(new AuthoringVocabularyEntry("unity.binding:GraphAsset", "Graph Asset Binding", "Component link to a graph asset such as VFX Graph, Shader Graph, Animator, or Timeline.", "Unity Binding"));
        }
    }
}
