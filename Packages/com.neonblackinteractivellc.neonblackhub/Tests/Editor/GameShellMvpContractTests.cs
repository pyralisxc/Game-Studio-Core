using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
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
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Editor", "MenuNavigationGuidedEditors.cs"));

            StringAssert.Contains("Credits", source);
            StringAssert.Contains("creditsPanel", source);
            StringAssert.Contains("creditsButton", source);
            StringAssert.Contains("creditsBackButton", source);
            StringAssert.Contains("Scene Navigator Source is required for play/load/exit buttons.", source);
        }

        [Test]
        public void SceneFlowSetup_DocumentsCompleteGameShellRoute()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Scene_Flow_Setup.md"));

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
