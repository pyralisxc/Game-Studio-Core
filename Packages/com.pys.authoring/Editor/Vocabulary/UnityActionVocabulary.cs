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
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionReviewCode, "Review Code", "Inspect source code or contract metadata.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionRunPlayModeCheck, "Run Play Mode Check", "Enter Play Mode and verify the stated success check.", "Unity Action"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ActionResolveMissingScript, "Resolve Missing Script", "Remove or replace a missing script component.", "Unity Action"));
        }
    }
}
