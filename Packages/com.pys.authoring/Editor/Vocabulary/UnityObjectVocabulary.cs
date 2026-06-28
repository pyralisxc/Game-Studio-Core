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
}
