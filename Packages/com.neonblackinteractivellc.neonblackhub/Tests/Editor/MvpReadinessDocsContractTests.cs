using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class MvpReadinessDocsContractTests
    {
        private static readonly string DocsRoot = Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub",
            "Members",
            "Pyralis",
            "Gameplay",
            "Docs");

        [Test]
        public void RuntimeParityMatrixNamesMvpRoutesAndRuntimeLanes()
        {
            string path = Path.Combine(DocsRoot, "RUNTIME_PARITY_MATRIX.md");
            string text = File.ReadAllText(path);

            StringAssert.Contains("Game Shell", text);
            StringAssert.Contains("Pawn-Backed Action / `Sprite2D`", text);
            StringAssert.Contains("Pawn-Backed Action / `Billboard2_5D`", text);
            StringAssert.Contains("Pawn-Backed Action / `Rigged3D`", text);
            StringAssert.Contains("Non-Pawn Tabletop", text);
        }

        [Test]
        public void RuntimeParityMatrixDefinesFivePartCompletionBar()
        {
            string path = Path.Combine(DocsRoot, "RUNTIME_PARITY_MATRIX.md");
            string text = File.ReadAllText(path);

            StringAssert.Contains("Runtime", text);
            StringAssert.Contains("Authoring", text);
            StringAssert.Contains("Guidance", text);
            StringAssert.Contains("Validation", text);
            StringAssert.Contains("Proof", text);
        }

        [Test]
        public void CoreReadinessCheckpointsPreserveBeginnerPrototypeReadyPromise()
        {
            string path = Path.Combine(DocsRoot, "CORE_PACKAGE_READINESS_CHECKPOINTS.md");
            string text = File.ReadAllText(path);

            StringAssert.Contains("Beginner Prototype Ready through guided Unity setup", text);
            StringAssert.Contains("Game Shell MVP", text);
            StringAssert.Contains("Pawn-Backed Action MVP", text);
            StringAssert.Contains("Non-Pawn Tabletop MVP", text);
            StringAssert.Contains("all three official runtime lanes", text);
        }
    }
}
