using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Core.Runtime;
using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public class SetupDocsContractTests : PyralisEditorTestSupport
    {
        [Test]
        public void PyralisAuthoringDocs_TeachSetupFlowAndStartHereAsLivingPath()
        {
            string authoringRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Docs",
                "Authoring");

            string startHerePath = Path.Combine(authoringRoot, "START_HERE.md");
            string canonicalPath = Path.Combine(authoringRoot, "CANONICAL_SETUP.md");
            string prefabReadmePath = Path.Combine(authoringRoot, "Prefabs", "README.md");
            string bootstrapPath = Path.Combine(authoringRoot, "Prefabs", "Bootstrap_Example_Setup.md");

            string startHere = File.ReadAllText(startHerePath);
            string canonical = File.ReadAllText(canonicalPath);
            string prefabReadme = File.ReadAllText(prefabReadmePath);
            string bootstrap = File.ReadAllText(bootstrapPath);

            Assert.That(startHere.Contains("Setup Flow"), Is.True);
            Assert.That(startHere.Contains("Create new `RuntimePatternDefinition` assets only when"), Is.True);
            Assert.That(canonical.Contains("`Docs/Authoring/START_HERE.md`"), Is.True);
            Assert.That(canonical.Contains("MANUAL.md"), Is.False);
            Assert.That(canonical.Contains("map Pyralis gameplay roles to your project's action names"), Is.True);
            Assert.That(canonical.Contains("The 2D input stack reads movement, dash/jump, attack"), Is.True);
            Assert.That(canonical.Contains("The 3D input stack also reads action names from the effective `InputProfile`"), Is.True);
            Assert.That(prefabReadme.Contains("MANUAL.md"), Is.False);
            Assert.That(bootstrap.Contains("Use manual native authoring for validation passes"), Is.True);
            Assert.That(bootstrap.Contains("Future scaffold tooling can capture a proven route later"), Is.True);
            Assert.That(startHere.Contains("Template or scaffold tooling is not the current first-test path"), Is.True);
            Assert.That(canonical.Contains("Future scaffolds must be downstream of a manually proven route"), Is.True);
        }

        [Test]
        public void PyralisAuthoringDocs_DefineSetupMaintenanceContract()
        {
            string authoringRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Docs",
                "Authoring");

            string readme = File.ReadAllText(Path.Combine(authoringRoot, "README.md"));
            string migration = File.ReadAllText(Path.Combine(authoringRoot, "Systems", "Migration_and_Readability_Standard.md"));
            string architecture = File.ReadAllText(Path.Combine(authoringRoot, "Systems", "Architecture_Overview.md"));
            string brawlerMenu = File.ReadAllText(Path.Combine(authoringRoot, "Prefabs", "Brawler_Menu_Example_Setup.md"));

            Assert.That(readme.Contains("Setup Maintenance Contract"), Is.True);
            Assert.That(readme.Contains("PyralisSetupRouteAnalysis"), Is.True);
            Assert.That(migration.Contains("setup guidance is product code"), Is.True);
            Assert.That(migration.Contains("shared route analysis"), Is.True);
            Assert.That(architecture.Contains("Unity-facing entry point"), Is.True);
            Assert.That(architecture.Contains("PyralisGameplayLifetimeScope as the singular source of truth"), Is.True);
            Assert.That(architecture.Contains("Static `Instance` properties"), Is.True);
            AssertNoMojibake(architecture, "Architecture_Overview.md");
            Assert.That(brawlerMenu.Contains("SceneLoader.Instance.LoadScene"), Is.False);
            Assert.That(brawlerMenu.Contains("ISceneNavigator"), Is.True);
        }

        [Test]
        public void PackageReadme_PointsToLivePyralisSetupDocs()
        {
            string packageRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub");

            string readme = File.ReadAllText(Path.Combine(packageRoot, "README.md"));

            Assert.That(readme.Contains("Members/Pyralis/Gameplay/Docs/Authoring/START_HERE.md"), Is.True);
            Assert.That(readme.Contains("Setup Flow"), Is.True);
            Assert.That(readme.Contains("Project window"), Is.True);
            Assert.That(readme.Contains("right-click"), Is.True);
            Assert.That(readme.Contains("jp.hadashikick.vcontainer"), Is.True);
            Assert.That(readme.Contains("https://package.openupm.com"), Is.True);
            Assert.That(readme.Contains("PyralisGameplayLifetimeScope"), Is.True);
            Assert.That(readme.Contains("Documentation/Gameplay"), Is.False);
            Assert.That(readme.Contains("Documentation/"), Is.False);
            AssertNoMojibake(readme, "Package README.md");
        }

        [Test]
        public void PackageSampleMetadata_IsNotUnityTemplatePlaceholder()
        {
            string packageRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub");

            string sampleMetadataPath = Path.Combine(packageRoot, "Samples~", "Example", ".sample.json");
            string sampleMarkerPath = Path.Combine(packageRoot, "Samples~", "Example", "PyralisSampleMarker.cs");

            Assert.That(File.Exists(sampleMetadataPath), Is.True);
            Assert.That(File.Exists(sampleMarkerPath), Is.True);

            string metadata = File.ReadAllText(sampleMetadataPath);
            string marker = File.ReadAllText(sampleMarkerPath);

            Assert.That(metadata.Contains("Example Sample"), Is.False);
            Assert.That(metadata.Contains("Replace this string"), Is.False);
            Assert.That(metadata.Contains("START_HERE.md"), Is.True);
            Assert.That(marker.Contains("MyPublicSampleExampleClass"), Is.False);
            Assert.That(marker.Contains("MyPublicRuntimeExampleClass"), Is.False);
            Assert.That(marker.Contains("PyralisSampleMarker"), Is.True);
        }

        [Test]
        public void BeginnerFacingEditorGuidance_UsesCompatibilityLanguageInsteadOfLegacy()
        {
            string editorRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Editor");

            string[] beginnerFacingFiles =
            {
                "GameConfigEditor.cs",
                "InputConfigEditor.cs",
                "SceneGameFlowGuidedEditors.cs",
                "Pawn2DStackGuidedEditors.cs"
            };

            for (int i = 0; i < beginnerFacingFiles.Length; i++)
            {
                string source = File.ReadAllText(Path.Combine(editorRoot, beginnerFacingFiles[i]));
                Assert.That(source.ToLowerInvariant().Contains("legacy"), Is.False, beginnerFacingFiles[i] + " should use compatibility/fresh-start wording.");
            }
        }

        [Test]
        public void SetupDocs_IncludeNoPawnTabletopGuideAndAvoidPawnRequiredContradiction()
        {
            string authoringRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Docs",
                "Authoring");

            string canonicalPath = Path.Combine(authoringRoot, "CANONICAL_SETUP.md");
            string tabletopPath = Path.Combine(authoringRoot, "Prefabs", "Board_Card_Tabletop_Setup.md");

            Assert.That(File.Exists(canonicalPath), Is.True);
            Assert.That(File.Exists(tabletopPath), Is.True);

            string canonical = File.ReadAllText(canonicalPath);
            string tabletop = File.ReadAllText(tabletopPath);

            Assert.That(canonical.Contains("at least one `PawnDefinition`"), Is.False);
            Assert.That(canonical.Contains("Create pawn assets only when a participant needs an actor body"), Is.True);
            Assert.That(tabletop.Contains("a player does not require a pawn"), Is.True);
            Assert.That(tabletop.Contains("leave `Default Pawn` empty"), Is.True);
            Assert.That(tabletop.Contains("TabletopBoardSelectionBridge"), Is.True);
            Assert.That(tabletop.Contains("ActionQueueService"), Is.True);
        }

        [Test]
        public void PawnSetupDocs_ExplainBringYourOwnAnimatorControllerFlow()
        {
            string pawnSetupPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Docs",
                "Authoring",
                "Prefabs",
                "Pawn_Setup.md");

            string pawnSetup = File.ReadAllText(pawnSetupPath);

            Assert.That(pawnSetup.Contains("Bring Your Own Animator Controller"), Is.True);
            Assert.That(pawnSetup.Contains("Controller Mapping Wizard"), Is.True);
            Assert.That(pawnSetup.Contains("Append Suggestions"), Is.True);
            Assert.That(pawnSetup.Contains("Replace With Suggestions"), Is.True);
            Assert.That(pawnSetup.Contains("Parameter pickers are filtered by binding type"), Is.True);
            Assert.That(pawnSetup.Contains("Partial mappings are valid"), Is.True);
            Assert.That(pawnSetup.Contains("Current support is Animator parameter mapping"), Is.True);
            Assert.That(pawnSetup.Contains("Blend trees are supported through float parameters"), Is.True);
            Assert.That(pawnSetup.Contains("NormalizedSpeed"), Is.True);
        }

        [Test]
        public void RuntimeParityMatrix_TracksCoreRulesSpine()
        {
            string docsRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Docs");

            string matrixPath = Path.Combine(docsRoot, "RUNTIME_PARITY_MATRIX.md");

            Assert.That(File.Exists(matrixPath), Is.True);

            string matrix = File.ReadAllText(matrixPath);

            Assert.That(matrix.Contains("Core Rules Spine"), Is.True);
            Assert.That(matrix.Contains("BoardRuntimeState"), Is.True);
            Assert.That(matrix.Contains("TurnRuntimeState"), Is.True);
            Assert.That(matrix.Contains("BoardDefinition"), Is.True);
            Assert.That(matrix.Contains("TurnOrderDefinition"), Is.True);
            Assert.That(matrix.Contains("GameModeDefinition"), Is.True);
        }
    }
}
