namespace Pys.Authoring.Editor.Vocabulary
{
    internal static class UnityActionVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionInspectObject, "Inspect Object", "Select a Unity object and review its Inspector state.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionAddComponent, "Add Component", "Use the Inspector to add a Unity component.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionAssignField, "Assign Field", "Assign a serialized field or object reference.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionCreateAsset, "Create Asset", "Create a Unity project asset.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionOpenAsset, "Open Asset", "Select or open a Unity project asset.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionOpenWindow, "Open Window", "Open a native Unity authoring window.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionCreateGameObject, "Create GameObject", "Create a GameObject in the current scene.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionCreateComponent, "Create Component", "Add or create a Unity component on a GameObject.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionCreatePrefab, "Create Prefab", "Create a reusable prefab asset from a scene object.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionCreateClip, "Create Clip", "Create an Animation, Timeline, audio, or VFX authoring asset.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionCreateController, "Create Controller", "Create a controller asset such as an Animator Controller or Input Actions asset.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionBindReference, "Bind Reference", "Connect a scene object, asset, or component reference in the Inspector or authoring window.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionAssignTrack, "Assign Track", "Bind a Timeline, Animation, Audio, or VFX track to the intended object.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionPreviewAnimation, "Preview Animation", "Preview authored motion, timing, or visual feedback before Play Mode.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionFrameSelected, "Frame Selected", "Focus the selected Unity object or asset in the relevant view.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionPingAsset, "Ping Asset", "Ping a project asset so it is visible in the Project window.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionSelectInHierarchy, "Select In Hierarchy", "Select a scene object in the Hierarchy.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionOpenGraphAsset, "Open Graph Asset", "Open a graph-style Unity asset such as Shader Graph, VFX Graph, Animator, or Timeline.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionReviewCode, "Review Code", "Inspect source code or contract metadata.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionRunPlayModeCheck, "Run Play Mode Check", "Enter Play Mode and verify the stated success check.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionResolveMissingScript, "Resolve Missing Script", "Remove or replace a missing script component.", "Unity Action"));
        }
    }
}
