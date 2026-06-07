using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class NetworkingChainMvpContractTests
    {
        private static readonly string GameplayRoot = Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub",
            "Members",
            "Pyralis",
            "Gameplay");

        [Test]
        public void NetworkingReadme_DefinesBuildOrBuyBoundary()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Networking", "README.md"));

            StringAssert.Contains("Build Or Buy Boundary", docs);
            StringAssert.Contains("should not write its own low-level transport", docs);
            StringAssert.Contains("Unity Netcode for GameObjects plus Unity Transport", docs);
            StringAssert.Contains("Pyralis does write the game-development layer", docs);
            StringAssert.Contains("participant ownership", docs);
            StringAssert.Contains("authority checks", docs);
        }

        [Test]
        public void MultiplayerSetupDocs_SeparateLocalAndNetworkRoutes()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Multiplayer_Setup.md"));

            StringAssert.Contains("Local multiplayer", docs);
            StringAssert.Contains("Networked MVP setup", docs);
            StringAssert.Contains("Do not mix those routes accidentally.", docs);
            StringAssert.Contains("PlayerInputManager", docs);
            StringAssert.Contains("SessionDefinition.networkMode", docs);
            StringAssert.Contains("Network Chain MVP Quick Path", docs);
            StringAssert.Contains("NetworkManager", docs);
            StringAssert.Contains("UnityTransport", docs);
            StringAssert.Contains("NetworkObject", docs);
            StringAssert.Contains("NetworkManager.NetworkConfig.Prefabs", docs);
        }

        [Test]
        public void MvpReadinessDocs_TrackNetworkChainCheckpoint()
        {
            string matrix = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "RUNTIME_PARITY_MATRIX.md"));
            string checkpoints = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "CORE_PACKAGE_READINESS_CHECKPOINTS.md"));

            StringAssert.Contains("Network Chain MVP", matrix);
            StringAssert.Contains("optional `NeonBlack.Gameplay.Networking` assembly", matrix);
            StringAssert.Contains("rollback, prediction", matrix);
            StringAssert.Contains("Checkpoint 4: Network Chain MVP", checkpoints);
            StringAssert.Contains("use Unity Netcode for GameObjects and Unity Transport", checkpoints);
            StringAssert.Contains("keep direct NGO dependencies out of core gameplay feature code", checkpoints);
        }
    }
}
