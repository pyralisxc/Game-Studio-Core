namespace Pys.Authoring.Editor.Vocabulary
{
    internal static class UnityGraphVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeAssemblyReference, "Assembly Reference", "One assembly definition references another.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeNamespaceUsing, "Namespace Using", "A source file imports a namespace.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeInherits, "Inherits", "A type inherits from another type.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeImplements, "Implements", "A type implements an interface.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeSerializedField, "Serialized Field", "A type or contract exposes a Unity-serialized field.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeRequiredComponent, "Required Component", "A type or contract requires a component.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeContractDeclares, "Contract Declaration", "A type declares authoring metadata.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeValidatorReports, "Validation Report", "A validator reports a structured issue.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeSceneContains, "Scene Contains", "A scene object contains a component.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgePrefabContains, "Prefab Contains", "A prefab contains a component.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeObserves, "Observes", "An observer records evidence about another node.", "Graph"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.EdgeOwns, "Owns", "A node owns or declares responsibility for another node.", "Graph"));
        }
    }
}
