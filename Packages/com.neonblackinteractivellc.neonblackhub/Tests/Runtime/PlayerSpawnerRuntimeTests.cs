using System.Collections;
using System.Reflection;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Respawn;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class PlayerSpawnerRuntimeTests
    {
        [Test]
        public void PlayerSpawner_SpawnPlayerWithoutParticipantOrPrefab_ReturnsNullInsteadOfThrowing()
        {
            GameObject spawnerObject = new GameObject("PlayerSpawner");
            PlayerSpawner spawner = spawnerObject.AddComponent<PlayerSpawner>();

            LogAssert.Expect(LogType.Warning, "[PlayerSpawner] Cannot spawn player because no participant pawn was resolved and Player Prefab is empty.");

            GameObject spawned = InvokeSpawnPlayer(spawner, Vector3.zero, null);

            Assert.That(spawned, Is.Null);

            Object.DestroyImmediate(spawnerObject);
        }

        [Test]
        public void PlayerSpawner_ResolveParticipantServices_UsesPlatformContextRegistry()
        {
            GameplayPlatformContext context = GameplayPlatformContext.CreateOrReplace();
            GameObject root = new GameObject("Participant Services");
            ParticipantRosterService roster = root.AddComponent<ParticipantRosterService>();
            ParticipantSpawnService spawn = root.AddComponent<ParticipantSpawnService>();
            context.Services.Register(roster);
            context.Services.Register<IParticipantRoster>(roster);
            context.Services.Register<IPlayerProvider>(roster);
            context.Services.Register(spawn);

            GameObject spawnerObject = new GameObject("PlayerSpawner");
            PlayerSpawner spawner = spawnerObject.AddComponent<PlayerSpawner>();

            InvokePrivate(spawner, "ResolveParticipantServices");

            Assert.That(GetPrivateField<ParticipantRosterService>(spawner, "rosterService"), Is.SameAs(roster));
            Assert.That(GetPrivateField<ParticipantSpawnService>(spawner, "participantSpawnService"), Is.SameAs(spawn));
            Assert.That(GetPrivateField<IPlayerProvider>(spawner, "_playerProvider"), Is.SameAs(roster));

            Object.DestroyImmediate(spawnerObject);
            Object.DestroyImmediate(root);
            GameplayPlatformContext.ClearCurrent();
        }

        [Test]
        public void PlayerSpawner_ResolveTrackedParticipant_DoesNotFallbackWhenTargetSeatIsMissing()
        {
            GameObject spawnerObject = new GameObject("PlayerSpawner");
            PlayerSpawner spawner = spawnerObject.AddComponent<PlayerSpawner>();
            GameObject rosterObject = new GameObject("Roster");
            ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();
            SetPrivateField(spawner, "rosterService", roster);
            SetPrivateField(spawner, "targetSeatIndex", 0);

            ParticipantHandle participant = roster.RegisterParticipant(null, preferredSeatIndex: 1);
            Assert.That(participant.SeatIndex, Is.EqualTo(1));

            ParticipantHandle resolved = InvokeResolveTrackedParticipant(spawner);

            Assert.That(resolved, Is.Null);

            Object.DestroyImmediate(spawnerObject);
            Object.DestroyImmediate(rosterObject);
        }

        [Test]
        public void PlayerSpawner_ResolveTrackedParticipant_AllowsFirstParticipantFallbackWhenSeatIsNegative()
        {
            GameObject spawnerObject = new GameObject("PlayerSpawner");
            PlayerSpawner spawner = spawnerObject.AddComponent<PlayerSpawner>();
            GameObject rosterObject = new GameObject("Roster");
            ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();
            SetPrivateField(spawner, "rosterService", roster);
            SetPrivateField(spawner, "targetSeatIndex", -1);

            ParticipantHandle participant = roster.RegisterParticipant(null, preferredSeatIndex: 2);

            ParticipantHandle resolved = InvokeResolveTrackedParticipant(spawner);

            Assert.That(resolved, Is.SameAs(participant));

            Object.DestroyImmediate(spawnerObject);
            Object.DestroyImmediate(rosterObject);
        }

        [UnityTest]
        public IEnumerator PlayerSpawner_Destroy_RemovesRuntimeCountdownCanvas()
        {
            GameObject spawnerObject = new GameObject("PlayerSpawner");
            PlayerSpawner spawner = spawnerObject.AddComponent<PlayerSpawner>();

            InvokePrivate(spawner, "BuildCountdownUI");
            GameObject canvasObject = GetPrivateField<GameObject>(spawner, "_countdownCanvas");
            Assert.That(canvasObject, Is.Not.Null);

            Object.DestroyImmediate(spawnerObject);
            yield return null;

            Assert.That(canvasObject == null, Is.True);
        }

        private static GameObject InvokeSpawnPlayer(PlayerSpawner spawner, Vector3 position, ParticipantHandle participant)
        {
            MethodInfo method = typeof(PlayerSpawner).GetMethod("SpawnPlayer", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(spawner, new object[] { position, participant }) as GameObject;
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(target, null);
        }

        private static ParticipantHandle InvokeResolveTrackedParticipant(PlayerSpawner spawner)
        {
            MethodInfo method = typeof(PlayerSpawner).GetMethod("ResolveTrackedParticipant", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(spawner, null) as ParticipantHandle;
        }

        private static T GetPrivateField<T>(object target, string fieldName) where T : class
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return field.GetValue(target) as T;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }
    }
}
