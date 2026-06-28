using System.Collections.Generic;
using Pys.Authoring.Editor.Contracts;

namespace Pys.Authoring.Editor.Vocabulary
{
    public sealed class AuthoringVocabularyDictionary
    {
        private readonly Dictionary<string, AuthoringVocabularyEntry> entries = new Dictionary<string, AuthoringVocabularyEntry>();

        public void Add(AuthoringVocabularyEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                return;

            entries[entry.Key] = entry;
        }

        public string Label(string key, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(key) && entries.TryGetValue(key, out AuthoringVocabularyEntry entry))
                return !string.IsNullOrWhiteSpace(entry.Label) ? entry.Label : fallback ?? string.Empty;

            return !string.IsNullOrWhiteSpace(fallback)
                ? fallback
                : AuthoringContractResolver.Prettify(key);
        }

        public bool TryGet(string key, out AuthoringVocabularyEntry entry)
        {
            return entries.TryGetValue(key ?? string.Empty, out entry);
        }
    }
}
