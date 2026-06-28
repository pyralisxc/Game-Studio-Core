namespace Pys.Authoring.Editor.Vocabulary
{
    internal static class ProjectionVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ProjectionSettings, "Settings", "Observation scope and scan controls.", "Projection"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ProjectionIntent, "Intent", "Selectable target-project capabilities exposed by contracts.", "Projection"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ProjectionOverview, "Overview", "Compact next-inspection summary.", "Projection"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ProjectionGuide, "Guide", "Evidence-backed action rows.", "Projection"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ProjectionMap, "Map", "Current Unity scene and asset evidence.", "Projection"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ProjectionHygiene, "Hygiene", "Authoring evidence health audit.", "Projection"));
            dictionary.Add(new AuthoringVocabularyEntry(AuthoringVocabularyKey.ProjectionFacts, "Facts", "Evidence inventory counts.", "Projection"));
        }
    }
}
