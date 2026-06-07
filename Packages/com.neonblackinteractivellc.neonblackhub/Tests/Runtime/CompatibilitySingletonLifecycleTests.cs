using NeonBlack.Gameplay.Core.Navigation;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Settings;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class CompatibilitySingletonLifecycleTests
    {
        [Test]
        public void CompatibilitySingletons_ClearStaticInstanceWhenOwnerIsDestroyed()
        {
            AssertClearsInstance<TimeManager>(
                () => TimeManager.Instance,
                "TimeManager");
            AssertClearsInstance<SceneLoader>(
                () => SceneLoader.Instance,
                "SceneLoader");
            AssertClearsInstance<SceneFader>(
                () => SceneFader.Instance,
                "SceneFader");
            AssertClearsInstance<SettingsManager>(
                () => SettingsManager.Instance,
                "SettingsManager");
            AssertClearsInstance<GameManager>(
                () => GameManager.Instance,
                "GameManager");
        }

        private static void AssertClearsInstance<T>(System.Func<T> getInstance, string objectName)
            where T : Component
        {
            GameObject owner = new GameObject(objectName);
            T component = owner.AddComponent<T>();

            Assert.That(getInstance(), Is.SameAs(component));

            Object.DestroyImmediate(owner);

            Assert.That(getInstance(), Is.Null);
        }
    }
}
