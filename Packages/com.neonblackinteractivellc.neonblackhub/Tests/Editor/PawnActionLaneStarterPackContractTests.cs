using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class PawnActionLaneStarterPackContractTests
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
        public void PawnStarterPackFactory_CreatesAssetsForAllPawnActionLanes()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Editor", "GameplayStarterPackFactory.cs"));

            StringAssert.Contains("Sprite2DPresentationProfile", source);
            StringAssert.Contains("Billboard25DPresentationProfile", source);
            StringAssert.Contains("Rigged3DPresentationProfile", source);
            StringAssert.Contains("Sprite2DPawnDefinition", source);
            StringAssert.Contains("Billboard25DPawnDefinition", source);
            StringAssert.Contains("Rigged3DPawnDefinition", source);
            StringAssert.Contains("Sprite2DPawnPrefab", source);
            StringAssert.Contains("Billboard25DPawnPrefab", source);
            StringAssert.Contains("Rigged3DPawnPrefab", source);
        }

        [Test]
        public void PawnStarterPackFactory_CreatesCorrectPrefabStacksFor2DAnd3D()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Editor", "GameplayStarterPackFactory.cs"));

            StringAssert.Contains("CreateStarterPawnPrefab2D", source);
            StringAssert.Contains("CreateStarterPawnPrefab3D", source);
            StringAssert.Contains("EnsureComponent<Motor2D>(root)", source);
            StringAssert.Contains("root.AddComponent<Motor3D>()", source);
            StringAssert.Contains("ActorPresentationMode.Sprite2D", source);
            StringAssert.Contains("ActorPresentationMode.Billboard2_5D", source);
            StringAssert.Contains("ActorPresentationMode.ThirdPerson3D", source);
        }

        [Test]
        public void PawnSetupDocs_NamePawnActionMvpLaneStarterAssets()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Pawn_Setup.md"));

            StringAssert.Contains("Pawn-Backed Action MVP lane choice", docs);
            StringAssert.Contains("Sprite2DPawnPrefab", docs);
            StringAssert.Contains("Billboard25DPawnPrefab", docs);
            StringAssert.Contains("Rigged3DPawnPrefab", docs);
            StringAssert.Contains("Sprite2D", docs);
            StringAssert.Contains("Billboard2_5D", docs);
            StringAssert.Contains("Rigged3D", docs);
        }
    }
}
