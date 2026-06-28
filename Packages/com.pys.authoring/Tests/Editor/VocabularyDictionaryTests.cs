using NUnit.Framework;
using Pys.Authoring.Editor.Vocabulary;

namespace Pys.Authoring.Editor.Tests
{
    public sealed class VocabularyDictionaryTests
    {
        [Test]
        public void Label_UsesDictionaryEntryWhenPresent()
        {
            AuthoringVocabularyDictionary dictionary = new AuthoringVocabularyDictionary();
            dictionary.Add(new AuthoringVocabularyEntry("category:Example", "Example Label"));

            Assert.That(dictionary.Label("category:Example", "Fallback"), Is.EqualTo("Example Label"));
        }

        [Test]
        public void Label_FallsBackWhenEntryMissing()
        {
            AuthoringVocabularyDictionary dictionary = new AuthoringVocabularyDictionary();

            Assert.That(dictionary.Label("category:Missing", "Fallback"), Is.EqualTo("Fallback"));
        }

        [Test]
        public void DefaultVocabulary_UsesDiscoveredProviderEntries()
        {
            AuthoringVocabularyDictionary dictionary = AuthoringVocabulary.BuildDefault();

            Assert.That(dictionary.Label("provider:example", "Fallback"), Is.EqualTo("Provider Example"));
        }

        [Test]
        public void DefaultVocabulary_ContainsUnitySetupAndGraphKeys()
        {
            AuthoringVocabularyDictionary dictionary = AuthoringVocabulary.BuildDefault();

            Assert.That(dictionary.Label(AuthoringVocabularyKey.NodeSceneObject, string.Empty), Is.EqualTo("GameObject"));
            Assert.That(dictionary.Label(AuthoringVocabularyKey.NodePrefab, string.Empty), Is.EqualTo("Prefab Asset"));
            Assert.That(dictionary.Label(AuthoringVocabularyKey.NodeAsset, string.Empty), Is.EqualTo("Asset"));
            Assert.That(dictionary.Label(AuthoringVocabularyKey.EdgeSceneContains, string.Empty), Is.EqualTo("Scene Contains"));
            Assert.That(dictionary.Label(AuthoringVocabularyKey.EdgePrefabContains, string.Empty), Is.EqualTo("Prefab Contains"));
            Assert.That(dictionary.Label(AuthoringVocabularyKey.ActionAddComponent, string.Empty), Is.EqualTo("Add Component"));
            Assert.That(dictionary.Label(AuthoringVocabularyKey.ActionAssignField, string.Empty), Is.EqualTo("Assign Field"));
            Assert.That(dictionary.Label(AuthoringVocabularyKey.ProjectionIntent, string.Empty), Is.EqualTo("Intent"));
        }
    }

    public sealed class FixtureVocabularyProvider : IAuthoringVocabularyProvider
    {
        public void AddEntries(AuthoringVocabularyDictionary dictionary)
        {
            dictionary.Add(new AuthoringVocabularyEntry("provider:example", "Provider Example"));
        }
    }
}
