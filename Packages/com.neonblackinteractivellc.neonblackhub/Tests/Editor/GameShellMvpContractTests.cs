using System.IO;
using System.Linq;
using System.Reflection;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Navigation;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    [Explicit("MVP documentation contract audit; run intentionally outside the default Unity EditMode smoke gate.")]
    public sealed class GameShellMvpContractTests
    {
        private static readonly string PackageRoot = Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub");

        private static readonly string GameplayRoot = Path.Combine(
            PackageRoot,
            "Members",
            "Pyralis",
            "Gameplay");

        private static string AuthoringDoc(params string[] segments)
        {
            string path = Path.Combine(GameplayRoot, "Docs", "Authoring");
            foreach (string segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return path;
        }

        [Test]
        public void MainMenuManager_RuntimeSurfaceExposesCreditsPanelFlow()
        {
            const BindingFlags members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Assert.That(typeof(MainMenuManager).GetField("creditsPanel", members), Is.Not.Null);
            Assert.That(typeof(MainMenuManager).GetField("creditsButton", members), Is.Not.Null);
            Assert.That(typeof(MainMenuManager).GetField("creditsBackButton", members), Is.Not.Null);
            Assert.That(typeof(MainMenuManager).GetMethod("OnCredits", members), Is.Not.Null);
            Assert.That(typeof(MainMenuManager).GetMethod("SetSceneNavigator", members), Is.Not.Null);
        }

        [Test]
        public void MainMenuManager_AuthoringContractGuidesCreditsAndRequiredNavigation()
        {
            AuthoringContractAttribute contract = typeof(MainMenuManager)
                .GetCustomAttributes(typeof(AuthoringContractAttribute), false)
                .Cast<AuthoringContractAttribute>()
                .SingleOrDefault();

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.Capability.HasFlag(AuthoringCapability.UI), Is.True);
            Assert.That(contract.AssignmentFields, Does.Contain("mainPanel"));
            Assert.That(contract.AssignmentFields, Does.Contain("newGameButton"));
            Assert.That(contract.AssignmentFields, Does.Contain("exitButton"));
            Assert.That(contract.AssignmentFields, Does.Contain("gameSceneName"));
            Assert.That(contract.AssignmentFields, Does.Contain("sceneNavigatorSource"));
            Assert.That(string.Join(" ", contract.NativeSetup), Does.Contain("Back button"));
            Assert.That(contract.ExpertAdvice, Does.Contain("Scene Navigator Source"));
        }

        [Test]
        public void SceneFlowSetup_DocumentsCompleteGameShellRoute()
        {
            string docs = File.ReadAllText(AuthoringDoc("Prefabs", "Scene_Flow_Setup.md"));

            StringAssert.Contains("Game Shell MVP route", docs);
            StringAssert.Contains("boot scene", docs);
            StringAssert.Contains("loading scene", docs);
            StringAssert.Contains("main menu", docs);
            StringAssert.Contains("settings", docs);
            StringAssert.Contains("credits", docs);
            StringAssert.Contains("gameplay scene transition", docs);
            StringAssert.Contains("FadeToSceneViaLoader", docs);
            Assert.That(docs.Contains("MainMenuManager` (your main menu scene) -> **Level Registry**"), Is.False);
        }
    }
}
