using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    [Explicit("RPG roadmap documentation contract audit; run intentionally outside the default Unity EditMode smoke gate.")]
    public sealed class RpgSystemsRoadmapContractTests
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
        public void RpgRoadmap_DocumentsReusableCapabilityFamilies()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "RPG_SYSTEMS_ROADMAP.md"));

            StringAssert.Contains("RPG Systems Platform", docs);
            StringAssert.Contains("RPG Identity, Stats, And Progression", docs);
            StringAssert.Contains("Inventory And Item Catalog", docs);
            StringAssert.Contains("Equipment And Effects", docs);
            StringAssert.Contains("Skill Trees", docs);
            StringAssert.Contains("Quests And Objectives", docs);
            StringAssert.Contains("NPC And Dialogue Hooks", docs);
            StringAssert.Contains("Hub Framework", docs);
            StringAssert.Contains("Persistence", docs);
            StringAssert.Contains("Open-Zone Readiness", docs);
        }

        [Test]
        public void RpgRoadmap_PreservesCrossModePlatformPromise()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "RPG_SYSTEMS_ROADMAP.md"));

            StringAssert.Contains("participant-owned and actor-agnostic", docs);
            StringAssert.Contains("side-scrolling brawlers", docs);
            StringAssert.Contains("tabletop tactics", docs);
            StringAssert.Contains("survival loops", docs);
            StringAssert.Contains("hub-launched minigames", docs);
            StringAssert.Contains("skill tree node", docs);
            StringAssert.Contains("dialogue flag", docs);
        }

        [Test]
        public void CoreReadinessDocs_LinkRpgProgramWithoutReplacingMvpGate()
        {
            string checkpoints = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "CORE_PACKAGE_READINESS_CHECKPOINTS.md"));
            string roadmap = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "FEATURE_DEVELOPMENT_ROADMAP.md"));
            string matrix = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "RUNTIME_PARITY_MATRIX.md"));

            StringAssert.Contains("RPG Systems Platform", checkpoints);
            StringAssert.Contains("RPG Systems Platform", roadmap);
            StringAssert.Contains("RPG Systems Platform", matrix);
            StringAssert.Contains("Beginner Prototype Ready", checkpoints);
        }
    }
}
