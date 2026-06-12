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

        private static string AuthoringDoc(params string[] segments)
        {
            string path = Path.Combine(GameplayRoot, "Docs", "Authoring");
            foreach (string segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return path;
        }

        private static string FindGameplayEditorFile(string fileName)
        {
            string editorRoot = Path.Combine(GameplayRoot, "Editor");
            string[] matches = Directory.GetFiles(editorRoot, fileName, SearchOption.AllDirectories);
            Assert.That(matches.Length, Is.EqualTo(1), $"Expected one Gameplay Editor file named {fileName}.");
            return matches[0];
        }

        [Test]
        public void TabletopSetupDocs_DefineNoPawnMvpQuickPath()
        {
            string docs = File.ReadAllText(AuthoringDoc("Prefabs", "Board_Card_Tabletop_Setup.md"));

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
            string docs = File.ReadAllText(AuthoringDoc("Prefabs", "Board_Card_Tabletop_Setup.md"));

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
            string validator = File.ReadAllText(FindGameplayEditorFile("PyralisSetupFlowValidator.cs"));
            string guidance = File.ReadAllText(FindGameplayEditorFile("PyralisSetupFlowGuidance.cs"));
            string routeAnalysis = File.ReadAllText(FindGameplayEditorFile("PyralisSetupRouteAnalysis.cs"));

            StringAssert.Contains("ParticipantEmbodimentRequirement.RequiredPawn", routeAnalysis);
            StringAssert.Contains("No participant pawn is required for this setup route.", validator);
            StringAssert.Contains("Spawn points can stay empty for no-pawn board/card/menu/camera routes.", validator);
            StringAssert.Contains("Tabletop Runtime Contract", guidance);
            StringAssert.Contains("Assign Tabletop Selection Surface", guidance);
        }
    }
}
