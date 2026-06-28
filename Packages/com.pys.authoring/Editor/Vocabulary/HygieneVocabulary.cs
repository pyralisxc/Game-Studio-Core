namespace Pys.Authoring.Editor.Vocabulary
{
    internal static class HygieneVocabulary
    {
        public static void AddTo(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:Overview", "Overview", "Aggregate Hygiene lens.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:Ownership", "Ownership", "Responsibility and ownership pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:Dependencies", "Dependencies", "Assembly and namespace pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:Contracts", "Contracts", "Contract completeness pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:RuntimeFlow", "Runtime Flow", "Runtime coordination pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:ProjectionIntegrity", "Projection Integrity", "Display/export evidence pressure.", "Hygiene"));
            dictionary.Add(new AuthoringVocabularyEntry("hygiene:DocsAndClaims", "Docs & Claims", "Prose claims versus typed evidence.", "Hygiene"));
        }
    }
}
