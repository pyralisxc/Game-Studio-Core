using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
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

        [Test]
        public void TabletopSetupDocs_DefineNoPawnMvpQuickPath()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Board_Card_Tabletop_Setup.md"));

            StringAssert.Contains("Non-Pawn Tabletop MVP quick path", docs);
            StringAssert.Contains("leave `Default Pawn` empty", docs);
            StringAssert.Contains("leave `Spawn Points` empty", docs);
            StringAssert.Contains("Create -> NeonBlack", docs);
            StringAssert.Contains("Runtime Pattern Definition", docs);
            StringAssert.Contains("select one generic token, card, marker, or board piece", docs);
        }

        [Test]
        public void TabletopSetupDocs_NameRuntimeProofComponents()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Board_Card_Tabletop_Setup.md"));

            StringAssert.Contains("TabletopBoardGridPresenter", docs);
            StringAssert.Contains("TabletopBoardSelectionBridge", docs);
            StringAssert.Contains("ActionQueueService", docs);
            StringAssert.Contains("BoardMoveActionResolver", docs);
            StringAssert.Contains("TurnOrderDefinition", docs);
            StringAssert.Contains("BoardTerminalConditionDefinition", docs);
        }

        [Test]
        public void SetupFlowSource_PreservesNoPawnParticipantAndSpawnGuidance()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Editor", "PyralisSetupFlowMonitor.cs"));

            StringAssert.Contains("No participant pawn is required for this setup route.", source);
            StringAssert.Contains("Spawn points can stay empty for no-pawn board/card/menu/camera routes.", source);
            StringAssert.Contains("Tabletop Runtime Contract", source);
            StringAssert.Contains("Assign Tabletop Selection Surface", source);
        }
    }
}
