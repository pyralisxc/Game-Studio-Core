using NUnit.Framework;
using System.IO;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    [Explicit("Documentation contract audit; run intentionally outside the default Unity EditMode smoke gate.")]
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
            string sceneGuidePath = Path.Combine(authoringRoot, "SCENE_SETUP_GUIDE.md");

            string startHere = File.ReadAllText(startHerePath);
            string canonical = File.ReadAllText(canonicalPath);
            string sceneGuide = File.ReadAllText(sceneGuidePath);

            Assert.That(startHere.Contains("Setup Flow"), Is.True);
            Assert.That(startHere.Contains("route capability"), Is.True);
            Assert.That(canonical.Contains("`Docs/Authoring/START_HERE.md`"), Is.True);
            Assert.That(canonical.Contains("MANUAL.md"), Is.False);
            Assert.That(canonical.Contains("map Pyralis gameplay roles to your project's action names"), Is.True);
            Assert.That(canonical.Contains("The 2D input stack reads movement, dash/jump, attack"), Is.True);
            Assert.That(canonical.Contains("The 3D input stack also reads action names from the effective `InputProfile`"), Is.True);
            Assert.That(sceneGuide.Contains("Use native Unity creation and assignment while Authoring explains the route."), Is.True);
            Assert.That(sceneGuide.Contains("Future scaffold tooling should be treated as route scaffolding only"), Is.True);
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
            string sceneGuide = File.ReadAllText(Path.Combine(authoringRoot, "SCENE_SETUP_GUIDE.md"));

            Assert.That(readme.Contains("Setup Maintenance Contract"), Is.True);
            Assert.That(readme.Contains("PyralisSetupRouteAnalysis"), Is.True);
            Assert.That(readme.Contains("Prefabs/"), Is.False);
            Assert.That(migration.Contains("setup guidance is product code"), Is.True);
            Assert.That(migration.Contains("shared route analysis"), Is.True);
            Assert.That(architecture.Contains("Unity-facing entry point"), Is.True);
            Assert.That(architecture.Contains("PyralisGameplayLifetimeScope as the singular source of truth"), Is.True);
            Assert.That(architecture.Contains("Static `Instance` properties"), Is.True);
            AssertNoMojibake(architecture, "Architecture_Overview.md");
            Assert.That(sceneGuide.Contains("MainMenuManager"), Is.True);
            Assert.That(sceneGuide.Contains("ISceneNavigator"), Is.False);
        }

        [Test]
        public void PyralisAuthoringDocs_DefineResolvedGraphSourceOfTruth()
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
            string blueprint = File.ReadAllText(Path.Combine(authoringRoot, "AUTHORING_BLUEPRINT.md"));
            string model = File.ReadAllText(Path.Combine(authoringRoot, "AUTHORING_MODEL.md"));
            string sceneGuide = File.ReadAllText(Path.Combine(authoringRoot, "SCENE_SETUP_GUIDE.md"));

            Assert.That(readme.Contains("Source-Of-Truth Map"), Is.True);
            Assert.That(readme.Contains("Contracts own feature meaning."), Is.True);
            Assert.That(readme.Contains("The resolved setup graph synthesizes those inputs."), Is.True);
            Assert.That(blueprint.Contains("Authoring Information Flow"), Is.True);
            Assert.That(blueprint.Contains("Cleanup Closure Criteria"), Is.True);
            Assert.That(blueprint.Contains("If the answer is \"UI projection,\" the file should not be discovering route truth."), Is.True);
            Assert.That(model.Contains("Contracts + reflection + dependency tree + scene evidence + validators + grammar"), Is.True);
            Assert.That(sceneGuide.Contains("In Authoring Window: clear `Do Now`, open `Map`, check intent-required blockers in `Validate`, then Play."), Is.True);
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
            string sceneGuidePath = Path.Combine(authoringRoot, "SCENE_SETUP_GUIDE.md");

            Assert.That(File.Exists(canonicalPath), Is.True);
            Assert.That(File.Exists(sceneGuidePath), Is.True);

            string canonical = File.ReadAllText(canonicalPath);
            string sceneGuide = File.ReadAllText(sceneGuidePath);

            Assert.That(canonical.Contains("at least one `PawnDefinition`"), Is.False);
            Assert.That(canonical.Contains("Create pawn assets only when a participant needs an actor body"), Is.True);
            Assert.That(sceneGuide.Contains("Pawn route only"), Is.True);
            Assert.That(sceneGuide.Contains("pawn route"), Is.True);
        }

        [Test]
        public void AuthoringModel_ExplainsBringYourOwnAnimatorControllerFlow()
        {
            string modelPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Docs",
                "Authoring",
                "AUTHORING_MODEL.md");

            string model = File.ReadAllText(modelPath);

            Assert.That(model.Contains("Bring your own Animator Controller is the normal path."), Is.True);
            Assert.That(model.Contains("map Pyralis signals such as move, jump, dash, attack, hurt, and interact"), Is.True);
            Assert.That(model.Contains("The animation definition maps gameplay signals to Animator parameters."), Is.True);
            Assert.That(model.Contains("Animator Controller"), Is.True);
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
