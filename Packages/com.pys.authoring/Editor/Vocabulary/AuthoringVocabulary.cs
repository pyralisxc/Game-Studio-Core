namespace Pys.Authoring.Editor.Vocabulary
{
    public static class AuthoringVocabulary
    {
        public static AuthoringVocabularyDictionary BuildDefault()
        {
            AuthoringVocabularyDictionary dictionary = new AuthoringVocabularyDictionary();
            UnityObjectVocabulary.AddTo(dictionary);
            UnityGraphVocabulary.AddTo(dictionary);
            UnityActionVocabulary.AddTo(dictionary);
            ProjectionVocabulary.AddTo(dictionary);
            HygieneVocabulary.AddTo(dictionary);
            AuthoringVocabularyProviderDiscovery.AddDiscoveredProviders(dictionary);
            return dictionary;
        }
    }
}
