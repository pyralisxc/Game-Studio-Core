using System.IO;
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
        public void MainMenuManager_Source_ExposesCreditsPanelFlow()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Core", "Navigation", "UI", "MainMenuManager.cs"));

            StringAssert.Contains("creditsPanel", source);
            StringAssert.Contains("creditsButton", source);
            StringAssert.Contains("creditsBackButton", source);
            StringAssert.Contains("OnCredits", source);
            StringAssert.Contains("creditsButton.onClick.AddListener(OnCredits)", source);
            StringAssert.Contains("creditsBackButton.onClick.AddListener(OnBackToMain)", source);
            StringAssert.Contains("creditsPanel.SetActive(creditsPanel == target)", source);
        }

        [Test]
        public void MainMenuManagerEditor_GuidesCreditsAndRequiredNavigation()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Core", "Navigation", "UI", "MainMenuManager.cs"));

            StringAssert.Contains("AuthoringContract", source);
            StringAssert.Contains("Credits", source);
            StringAssert.Contains("creditsPanel", source);
            StringAssert.Contains("creditsButton", source);
            StringAssert.Contains("creditsBackButton", source);
            StringAssert.Contains("sceneNavigator", source);
            StringAssert.Contains("AssignmentFields", source);
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
