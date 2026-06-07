using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class RuntimeExampleTest
    {
        [Test]
        public void HealthComponent_Source_ExposesBooleanDamageResult()
        {
            string source = File.ReadAllText(GameplayPath(
                "Features", "Combat", "HealthComponent.cs"));

            Assert.That(source.Contains("public bool TryTakeDamage("), Is.True);
            Assert.That(source.Contains("public void TakeDamage("), Is.True);
            Assert.That(source.Contains("TryTakeDamage(amount, hitPoint, source);"), Is.True);
        }

        [Test]
        public void Hitboxes_Source_OnlyConfirmAppliedDamage()
        {
            string hitBoxSource = File.ReadAllText(GameplayPath(
                "Features", "Combat", "HitBox.cs"));
            string hitBox2DSource = File.ReadAllText(GameplayPath(
                "Features", "Combat", "2D", "HitBox2D.cs"));

            Assert.That(hitBoxSource.Contains("if (!health.TryTakeDamage("), Is.True);
            Assert.That(hitBox2DSource.Contains("if (!hp.TryTakeDamage("), Is.True);
        }

        private static string GameplayPath(params string[] segments)
        {
            string[] root =
            {
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay"
            };

            return Path.Combine(Path.Combine(root), Path.Combine(segments));
        }
    }
}
