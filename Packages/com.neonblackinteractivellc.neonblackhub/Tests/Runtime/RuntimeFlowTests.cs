using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Glue.RuntimeFlow;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RuntimeFlowTests
    {
        [Test]
        public void PublishDispatchesOnlyMatchingSubscribedEvent()
        {
            GameplayEventChannel channel = new GameplayEventChannel();
            int receivedValue = 0;
            int otherReceivedCount = 0;

            channel.Subscribe<TestGameplayEvent>(evt => receivedValue = evt.Value);
            channel.Subscribe<OtherGameplayEvent>(_ => otherReceivedCount++);
            channel.Publish(new TestGameplayEvent(7));

            Assert.AreEqual(7, receivedValue);
            Assert.AreEqual(0, otherReceivedCount);
        }

        [Test]
        public void UnsubscribeAndDisposedSubscriptionStopDispatch()
        {
            GameplayEventChannel channel = new GameplayEventChannel();
            int unsubscribedCount = 0;
            int disposedCount = 0;
            System.Action<TestGameplayEvent> handler = _ => unsubscribedCount++;

            channel.Subscribe(handler);
            channel.Unsubscribe(handler);
            System.IDisposable subscription = channel.Subscribe<TestGameplayEvent>(_ => disposedCount++);
            subscription.Dispose();
            channel.Publish(new TestGameplayEvent(1));

            Assert.AreEqual(0, unsubscribedCount);
            Assert.AreEqual(0, disposedCount);
        }

        private readonly struct TestGameplayEvent : IGameplayEvent
        {
            public TestGameplayEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct OtherGameplayEvent : IGameplayEvent
        {
            public OtherGameplayEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
}
