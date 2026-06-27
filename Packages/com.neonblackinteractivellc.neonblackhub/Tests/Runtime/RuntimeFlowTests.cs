using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Glue.RuntimeFlow;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RuntimeFlowTests
    {
        [Test]
        public void PublishDispatchesSubscribedEvent()
        {
            GameplayEventChannel channel = new GameplayEventChannel();
            int receivedValue = 0;

            channel.Subscribe<TestGameplayEvent>(evt => receivedValue = evt.Value);
            channel.Publish(new TestGameplayEvent(7));

            Assert.AreEqual(7, receivedValue);
        }

        [Test]
        public void PublishOnlyDispatchesMatchingEventType()
        {
            GameplayEventChannel channel = new GameplayEventChannel();
            int receivedCount = 0;

            channel.Subscribe<TestGameplayEvent>(_ => receivedCount++);
            channel.Publish(new OtherGameplayEvent(3));

            Assert.AreEqual(0, receivedCount);
        }

        [Test]
        public void UnsubscribeStopsDispatch()
        {
            GameplayEventChannel channel = new GameplayEventChannel();
            int receivedCount = 0;
            System.Action<TestGameplayEvent> handler = _ => receivedCount++;

            channel.Subscribe(handler);
            channel.Unsubscribe(handler);
            channel.Publish(new TestGameplayEvent(1));

            Assert.AreEqual(0, receivedCount);
        }

        [Test]
        public void DisposedSubscriptionStopsDispatch()
        {
            GameplayEventChannel channel = new GameplayEventChannel();
            int receivedCount = 0;

            System.IDisposable subscription = channel.Subscribe<TestGameplayEvent>(_ => receivedCount++);
            subscription.Dispose();
            channel.Publish(new TestGameplayEvent(1));

            Assert.AreEqual(0, receivedCount);
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
