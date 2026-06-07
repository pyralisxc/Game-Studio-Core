using System.Reflection;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Networking.Participants;
using NeonBlack.Gameplay.Networking.Runtime;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class NetworkingRuntimeTests
    {
        [TearDown]
        public void TearDown()
        {
            GameplayPlatformContext.ClearCurrent();
            if (NetworkManager.Singleton != null)
                Object.DestroyImmediate(NetworkManager.Singleton.gameObject);
        }

        [Test]
        public void SessionDefinition_Sanitize_NetworkModesDisableLocalFirst()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.localFirst = true;
            session.networkMode = GameplayNetworkMode.NetcodeHost;

            session.Sanitize();

            Assert.That(session.localFirst, Is.False);

            Object.DestroyImmediate(session);
        }

        [Test]
        public void GameplaySessionBootstrap_NetcodeHostCreatesNetworkedServiceLane()
        {
            GameObject root = new GameObject("Networked Bootstrap");
            root.SetActive(false);
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.networkMode = GameplayNetworkMode.NetcodeHost;
            session.autoStartHost = false;

            SetPrivateField(bootstrap, "sessionDefinition", session);
            SetPrivateField(bootstrap, "dontDestroyOnLoad", false);

            root.SetActive(true);

            Assert.That(root.transform.Find("SessionStateService")?.GetComponent<NetworkedSessionStateService>(), Is.Not.Null);
            Assert.That(root.transform.Find("ParticipantRosterService")?.GetComponent<NetworkedParticipantRosterService>(), Is.Not.Null);
            Assert.That(root.transform.Find("ParticipantSpawnService")?.GetComponent<NetworkedParticipantSpawnService>(), Is.Not.Null);

            Assert.That(GameplayPlatformContext.TryGetCurrent(out GameplayPlatformContext context), Is.True);
            Assert.That(context.Services.TryResolve(out ISessionOwnershipService ownership), Is.True);
            Assert.That(ownership, Is.TypeOf<NetworkedSessionOwnershipService>());
            Assert.That(context.Services.TryResolve(out IParticipantAuthorityService authority), Is.True);
            Assert.That(authority, Is.TypeOf<NetworkedParticipantAuthorityService>());

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisNetworkSetupValidator_FlagsMissingNetworkManagerForNetworkedSession()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.networkMode = GameplayNetworkMode.NetcodeHost;
            session.autoStartHost = false;

            var issues = PyralisNetworkSetupValidator.GetIssues(session, null);

            Assert.That(issues, Has.Exactly(1).Contains("Networked sessions require a scene NetworkManager."));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void PyralisNetworkSetupValidator_RequiresTransportNetworkObjectAndRegisteredPrefab()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.networkMode = GameplayNetworkMode.NetcodeHost;
            session.autoStartHost = false;

            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject pawnPrefab = new GameObject("Network Pawn");
            pawn.pawnPrefab = pawnPrefab;
            participant.defaultPawn = pawn;
            session.defaultParticipants = new[] { participant };

            GameObject managerObject = new GameObject("NetworkManager");
            NetworkManager networkManager = managerObject.AddComponent<NetworkManager>();
            networkManager.NetworkConfig = new NetworkConfig();
            networkManager.NetworkConfig.NetworkTransport = null;

            var missingTransportAndNetworkObject = PyralisNetworkSetupValidator.GetIssues(session, networkManager);
            Assert.That(missingTransportAndNetworkObject.Exists(issue => issue.Contains("NetworkTransport")), Is.True);
            Assert.That(missingTransportAndNetworkObject.Exists(issue => issue.Contains("needs a NetworkObject")), Is.True);

            UnityTransport transport = managerObject.AddComponent<UnityTransport>();
            networkManager.NetworkConfig.NetworkTransport = transport;
            pawnPrefab.AddComponent<NetworkObject>();

            var missingRegistration = PyralisNetworkSetupValidator.GetIssues(session, networkManager);
            Assert.That(missingRegistration.Exists(issue => issue.Contains("not registered in NetworkManager Network Prefabs")), Is.True);

            bool added = networkManager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = pawnPrefab });
            Assert.That(added, Is.True);

            var readyIssues = PyralisNetworkSetupValidator.GetIssues(session, networkManager);
            Assert.That(readyIssues, Is.Empty);

            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(pawnPrefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(session);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected field `{fieldName}` on `{target.GetType().Name}`.");
            field.SetValue(target, value);
        }
    }
}
