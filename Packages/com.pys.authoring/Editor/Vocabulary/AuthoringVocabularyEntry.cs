namespace Pys.Authoring.Editor.Vocabulary
{
    public sealed class AuthoringVocabularyEntry
    {
        public AuthoringVocabularyEntry(string key, string label, string summary = null, string group = null, string hint = null)
        {
            Key = key ?? string.Empty;
            Label = label ?? string.Empty;
            Summary = summary ?? string.Empty;
            Group = group ?? string.Empty;
            Hint = hint ?? string.Empty;
        }

        public string Key { get; }

        public string Label { get; }

        public string Summary { get; }

        public string Group { get; }

        public string Hint { get; }
    }
}
