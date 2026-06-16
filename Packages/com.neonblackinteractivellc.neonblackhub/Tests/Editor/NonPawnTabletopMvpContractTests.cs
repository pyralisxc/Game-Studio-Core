using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    [Explicit("MVP documentation contract audit; run intentionally outside the default Unity EditMode smoke gate.")]
    public sealed class NonPawnTabletopMvpContractTests
    {
        private static readonly string GameplayRoot = Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub",
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
        public void TabletopSetupDocs_DefineNoPawnMvpQuickPath()
        {
            string docs = File.ReadAllText(AuthoringDoc("SCENE_SETUP_GUIDE.md"));

            StringAssert.Contains("Pawn route only", docs);
            StringAssert.Contains("pawn route", docs);
            StringAssert.Contains("Use native Unity creation and assignment while Authoring explains the route.", docs);
            StringAssert.Contains("The Authoring Window", docs);
        }

        [Test]
        public void TabletopSetupDocs_NameRuntimeProofComponents()
        {
            string docs = File.ReadAllText(AuthoringDoc("AUTHORING_MODEL.md"));

            StringAssert.Contains("TabletopBoardGridPresenter", docs);
            StringAssert.Contains("TabletopBoardSelectionBridge", docs);
            StringAssert.Contains("ActionQueueService", docs);
            StringAssert.Contains("BoardMoveActionResolver", docs);
            StringAssert.Contains("TurnOrderDefinition", docs);
            StringAssert.Contains("BoardTerminalConditionDefinition", docs);
        }

    }
}
