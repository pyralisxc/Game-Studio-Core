using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class PickupAwardRoutingTests
    {
        [Test]
        public void CollectibleFeedback2D_AwardsThroughScoreAwardSinkContract()
        {
            GameObject go = new GameObject("Collectible Feedback");
            CollectibleFeedback2D feedback = go.AddComponent<CollectibleFeedback2D>();
            ScoreAwardSinkStub scoreAwardSink = new ScoreAwardSinkStub();
            feedback.ConfigureRuntime(scoreAwardSink);

            feedback.ApplyAward(new PickupAwardPayload(null, Vector3.zero, 5, PickupAwardOutcome.Collected));

            Assert.AreEqual(5, scoreAwardSink.Points);
            Object.DestroyImmediate(go);
        }

        private sealed class ScoreAwardSinkStub : ISessionScoreAwardSink, IParticipantScoreAwardSink
        {
            public int Points { get; private set; }
            public int ParticipantPoints { get; private set; }

            public void AddPoints(int amount = 1)
            {
                Points += amount;
            }

            public void AddScore(int participantId, int amount)
            {
                ParticipantPoints += amount;
            }
        }
    }
}
