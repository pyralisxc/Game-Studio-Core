namespace Pys.Authoring.Editor.Vocabulary
{
    internal static class HygieneVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:Overview", "Overview", "Aggregate Hygiene lens.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:Contracts", "Contract Hygiene", "Contract completeness, duplicate StableId, and readiness-hint pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:Dependencies", "Dependency Pressure", "Grouped assembly, namespace, and graph dependency pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:ValidationEvidence", "Validation Evidence", "Validation record structure and ownership pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:ProjectionIntegrity", "Projection Integrity", "Display/export evidence pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:Ownership", "Ownership & Honesty", "Responsibility, source ownership, and evidence-backed claim pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:RuntimeFlow", "Runtime Flow", "Runtime coordination pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:DocsAndClaims", "Docs & Claims", "Prose claims versus typed evidence.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:VisualDependencyGraph", "Dependency Graph", "Textual dependency graph edge groups for visual graph rendering.", "Hygiene"));
        }
    }
}
