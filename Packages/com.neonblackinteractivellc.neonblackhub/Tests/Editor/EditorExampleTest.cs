using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    [Explicit("Source cleanup regression audit; run intentionally outside the default Unity EditMode smoke gate.")]
    public class EditorExampleTest
    {
        [Test]
        public void Traversal_Source_PreservesHangZoneBeforeExitingHang()
        {
            string source = File.ReadAllText(GameplayPath(
                "Features", "Traversal", "Runtime", "3D", "Pawn3DTraversalComponent.cs"));

            Assert.That(source.Contains("IClimbZone zone = _hangZone;"), Is.True);
            Assert.That(source.Contains("PerformClimb(zone);"), Is.True);
            Assert.That(source.Contains("PerformClimb(_hangZone);"), Is.False);
        }

        [Test]
        public void Combat_Source_UsesDamageResultForImpactSideEffects()
        {
            string launcherSource = File.ReadAllText(GameplayPath(
                "Features", "Combat", "ProjectileLauncherBase.cs"));
            string hitBoxSource = File.ReadAllText(GameplayPath(
                "Features", "Combat", "HitBox.cs"));
            string hitBox2DSource = File.ReadAllText(GameplayPath(
                "Features", "Combat", "2D", "HitBox2D.cs"));

            Assert.That(launcherSource.Contains("return health.TryTakeDamage("), Is.True);
            Assert.That(hitBoxSource.Contains("if (!health.TryTakeDamage("), Is.True);
            Assert.That(hitBox2DSource.Contains("if (!hp.TryTakeDamage("), Is.True);
        }

        [Test]
        public void ScoreConsumers_Source_UseExplicitRuntimeServices()
        {
            string feedbackSource = File.ReadAllText(GameplayPath(
                "Features", "Pickups", "2D", "CollectibleFeedback2D.cs"));
            string stillnessSource = File.ReadAllText(GameplayPath(
                "Features", "Scoring", "2D", "StillnessBonus2D.cs"));
            string uiSource = File.ReadAllText(GameplayPath(
                "Features", "GameFlow", "2D", "UI", "UIManager.cs"));

            Assert.That(feedbackSource.Contains("GameManager.Instance"), Is.False);
            Assert.That(stillnessSource.Contains("GameManager.Instance"), Is.False);
            Assert.That(uiSource.Contains("GameManager.Instance"), Is.False);
            Assert.That(stillnessSource.Contains("ISessionScoreAwardSink"), Is.True);
            Assert.That(uiSource.Contains("IGameplaySessionFlow"), Is.True);
        }

        [Test]
        public void WarningCleanup_Source_AvoidsDeprecatedPickupOverlapAndDead2DBlockState()
        {
            string pickupCollectorSource = File.ReadAllText(GameplayPath(
                "Features", "Pickups", "Runtime", "2D", "ActorPickupCollectorFeature2D.cs"));
            string combat2DSource = File.ReadAllText(GameplayPath(
                "Features", "Characters", "2D", "PawnCombatBehaviour2D.cs"));

            Assert.That(pickupCollectorSource.Contains("OverlapCircleNonAlloc"), Is.False);
            Assert.That(combat2DSource.Contains("private bool _isBlocking"), Is.False);
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
