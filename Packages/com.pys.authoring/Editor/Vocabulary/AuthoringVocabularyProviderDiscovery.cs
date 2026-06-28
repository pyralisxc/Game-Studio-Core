using System;
using UnityEditor;

namespace Pys.Authoring.Editor.Vocabulary
{
    internal static class AuthoringVocabularyProviderDiscovery
    {
        public static void AddDiscoveredProviders(AuthoringVocabularyDictionary dictionary)
        {
            if (dictionary == null)
                return;

            foreach (Type type in TypeCache.GetTypesDerivedFrom<IAuthoringVocabularyProvider>())
            {
                if (type == null || type.IsAbstract || type.IsInterface)
                    continue;

                if (Activator.CreateInstance(type) is IAuthoringVocabularyProvider provider)
                    provider.AddEntries(dictionary);
            }
        }
    }
}
