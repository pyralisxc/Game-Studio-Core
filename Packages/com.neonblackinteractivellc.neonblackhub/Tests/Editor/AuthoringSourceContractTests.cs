using NeonBlack.Gameplay.Core.Contracts;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    [Explicit("Deep authoring source audit; run intentionally outside the default Unity EditMode smoke gate.")]
    public class AuthoringSourceContractTests : PyralisEditorTestSupport
    {
        private static string GameplayEditorRoot => Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub",
            "Members",
            "Pyralis",
            "Gameplay",
            "Editor");

        private static string GameplayRoot => Directory.GetParent(GameplayEditorRoot).FullName;

        private static string FindGameplayEditorFile(string fileName)
        {
            string editorDirectorySegment = Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
            string[] matches = Directory.GetFiles(GameplayRoot, fileName, SearchOption.AllDirectories)
                .Where(path => path.Contains(editorDirectorySegment))
                .ToArray();
            Assert.That(matches.Length, Is.EqualTo(1), $"Expected one Gameplay Editor file named {fileName}.");
            return matches[0];
        }

        private static string FindGameplaySourceFile(string fileName)
        {
            string[] matches = Directory.GetFiles(GameplayRoot, fileName, SearchOption.AllDirectories);
            Assert.That(matches.Length, Is.EqualTo(1), $"Expected one Gameplay source file named {fileName}.");
            return matches[0];
        }

        private static string AuthoringDoc(params string[] segments)
        {
            string path = Path.Combine(GameplayRoot, "Docs", "Authoring");
            foreach (string segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return path;
        }

        private static string GameplayEditorLayer(params string[] segments)
        {
            string path = GameplayEditorRoot;
            foreach (string segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return path;
        }

        [Test]
        public void PyralisEditor_Source_OrganizesAuthoringBySpineAndSurface()
        {
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Grammar")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Grammar", "Registry")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Grammar", "CapabilityVocabulary")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Grammar", "IntentVocabulary")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Grammar", "ProofVocabulary")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Spine", "Facts")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Spine", "Routes")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Spine", "Graph")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Spine", "Validation")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Spine", "Evidence")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Surfaces", "AuthoringWindow")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Surfaces", "Inspectors")), Is.True);
            Assert.That(Directory.Exists(GameplayEditorLayer("Authoring", "Surfaces", "Tools")), Is.True);

            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "README.md")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Spine", "README.md")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Surfaces", "README.md")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Grammar", "Registry", "PyralisAuthoringGrammarRegistry.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Grammar", "CapabilityVocabulary", "PyralisCapabilityVocabulary.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Grammar", "IntentVocabulary", "PyralisIntentVocabulary.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Spine", "Graph", "PyralisAuthoringOverviewModel.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Spine", "Graph", "PyralisAuthoringCapabilityDescriptor.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Spine", "Graph", "PyralisAuthoringSetupGraphProjection.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Spine", "Validation", "PyralisSetupFlowValidator.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Surfaces", "AuthoringWindow", "PyralisAuthoringWindow.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Surfaces", "AuthoringWindow", "PyralisCurrentStepPrimaryActionGuidance.cs")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Surfaces", "AuthoringWindow", "UI", "PyralisAuthoringWindow.uxml")), Is.True);
            Assert.That(File.Exists(GameplayEditorLayer("Authoring", "Surfaces", "Inspectors", "PyralisInspectorGuide.cs")), Is.True);
        }

        [Test]
        public void PyralisFeatureEditor_Source_KeepsGuidedAuthoringInsideEditorAuthoringFolders()
        {
            string featuresRoot = Path.Combine(GameplayRoot, "Features");
            string[] editorScripts = Directory.GetFiles(featuresRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => path.Split(Path.DirectorySeparatorChar).Contains("Editor"))
                .ToArray();

            Assert.That(editorScripts.Length, Is.GreaterThan(0));

            foreach (string editorScript in editorScripts)
            {
                string relative = editorScript.Replace(GameplayRoot + Path.DirectorySeparatorChar, string.Empty);
                Assert.That(
                    relative.Split(Path.DirectorySeparatorChar).Contains("Authoring"),
                    Is.True,
                    $"Feature editor script should live under an Editor/Authoring folder: {relative}");
            }
        }

        [Test]
        public void PyralisEditor_Source_ReflectiveAuthoringLayerSupportsCurrentTruth()
        {
            AssertReflectiveAuthoringLayerSupportsCurrentTruth();
        }

        [Test]
        public void PyralisAuthoringWindow_MapAndValidateTabs_ReadGraphProjectionNotLegacyModels()
        {
            string windowPath = FindGameplayEditorFile("PyralisAuthoringWindow.cs");
            string mapRendererPath = FindGameplayEditorFile("PyralisAuthoringMapRenderer.cs");
            string validateRendererPath = FindGameplayEditorFile("PyralisAuthoringValidateRenderer.cs");
            string factRendererPath = FindGameplayEditorFile("PyralisAuthoringFactExplorerRenderer.cs");
            string guidePath = FindGameplayEditorFile("PyralisAuthoringWindow.Guide.cs");

            string windowSource = File.ReadAllText(windowPath);
            string mapSource = File.ReadAllText(mapRendererPath);
            string validateSource = File.ReadAllText(validateRendererPath);
            string factSource = File.ReadAllText(factRendererPath);
            string guideSource = File.ReadAllText(guidePath);

            Assert.That(windowSource.Contains("_cachedSetupGraph"), Is.True);
            Assert.That(windowSource.Contains("GetCachedSetupGraph"), Is.True);
            Assert.That(windowSource.Contains("PyralisAuthoringSetupGraphBuilder.Build(graphSource)"), Is.True);
            Assert.That(mapSource.Contains("PyralisAuthoringSetupGraphBuilder.Build"), Is.False);
            Assert.That(mapSource.Contains("PyralisAuthoringSetupGraph graph"), Is.True);
            Assert.That(mapSource.Contains("PyralisAuthoringSetupGraphProjection.BuildSetupMapRows"), Is.True);
            Assert.That(mapSource.Contains("PyralisAuthoringSetupGraphProjection.BuildMapConnectionRows"), Is.True);
            Assert.That(mapSource.Contains("PyralisAuthoringRouteReport"), Is.False);
            Assert.That(mapSource.Contains("PyralisAuthoringValidationModel"), Is.False);
            Assert.That(mapSource.Contains("PyralisSetupRouteAnalysis.Build"), Is.False);

            Assert.That(validateSource.Contains("PyralisAuthoringSetupGraphBuilder.Build"), Is.False);
            Assert.That(validateSource.Contains("PyralisAuthoringSetupGraph graph"), Is.True);
            Assert.That(validateSource.Contains("PyralisAuthoringSetupGraphProjection.BuildValidationSections"), Is.True);
            Assert.That(validateSource.Contains("PyralisAuthoringSetupGraphProjection.BuildValidationDetailRows"), Is.True);
            Assert.That(validateSource.Contains("PyralisAuthoringSetupGraphProjection.BuildValidationRows"), Is.False);
            Assert.That(validateSource.Contains("PyralisAuthoringRouteReport"), Is.False);
            Assert.That(validateSource.Contains("PyralisAuthoringValidationModel"), Is.False);
            Assert.That(validateSource.Contains("PyralisSetupFlowValidator.BuildReport"), Is.False);
            Assert.That(validateSource.Contains("PyralisSceneReadinessValidator.BuildReport"), Is.False);

            Assert.That(factSource.Contains("PyralisAuthoringSetupGraphBuilder.Build"), Is.False);
            Assert.That(factSource.Contains("PyralisAuthoringSetupGraph graph"), Is.True);
            Assert.That(guideSource.Contains("PyralisAuthoringSetupGraphBuilder.Build"), Is.False);
            Assert.That(guideSource.Contains("PyralisAuthoringSetupGraph contextGraph"), Is.True);
        }

        [Test]
        public void PyralisAuthoringWindow_OverviewModel_DelegatesReadinessAndProofProjectionToGraph()
        {
            string overviewModelPath = FindGameplayEditorFile("PyralisAuthoringOverviewModel.cs");
            string overviewSource = File.ReadAllText(overviewModelPath);

            Assert.That(overviewModelPath, Does.Contain(Path.Combine("Spine", "Graph")));
            Assert.That(overviewModelPath, Does.Not.Contain(Path.Combine("Spine", "Routes")));
            Assert.That(overviewSource.Contains("PyralisAuthoringSetupGraphProjection.BuildOverviewIssues"), Is.True);
            Assert.That(overviewSource.Contains("PyralisAuthoringSetupGraphProjection.BuildOverviewPlayModeChecklist"), Is.True);
            Assert.That(overviewSource.Contains("PyralisAuthoringSetupGraphProjection.GetOverviewFirstProofLabel"), Is.True);
            Assert.That(overviewSource.Contains("PyralisAuthoringRouteReport"), Is.False);
            Assert.That(overviewSource.Contains("PyralisAuthoringValidationModel"), Is.False);
            Assert.That(overviewSource.Contains("PyralisSetupFlowStepStatus"), Is.False);
            Assert.That(overviewSource.Contains("PyralisSetupFlowWorkIntent"), Is.False);
            Assert.That(overviewSource.Contains("PyralisSetupFlowValidator.BuildReport"), Is.False);
            Assert.That(overviewSource.Contains("PyralisSceneReadinessValidator.BuildReport"), Is.False);
            Assert.That(overviewSource.Contains("private static PyralisAuthoringOverviewIssue BuildIssue"), Is.False);
            Assert.That(overviewSource.Contains("private static List<PyralisAuthoringPlayModeChecklistItem> BuildPlayModeChecklist"), Is.False);
        }

        [Test]
        public void PyralisAuthoringWindow_SurfaceText_DoesNotExposeSetupFlowLabels()
        {
            string windowTextPath = FindGameplayEditorFile("PyralisAuthoringWindowText.cs");
            string windowTextSource = File.ReadAllText(windowTextPath);

            Assert.That(windowTextSource.Contains("GetStatusLabel"), Is.False);
            Assert.That(windowTextSource.Contains("GetWorkIntentLabel"), Is.False);
            Assert.That(windowTextSource.Contains("PyralisSetupFlowStepStatus"), Is.False);
            Assert.That(windowTextSource.Contains("PyralisSetupFlowWorkIntent"), Is.False);
        }

        [Test]
        public void PyralisAuthoringWindow_TabsKeepProofAndContractAuditInFactsOrGuide()
        {
            string overviewRendererPath = FindGameplayEditorFile("PyralisAuthoringOverviewRenderer.cs");
            string factRendererPath = FindGameplayEditorFile("PyralisAuthoringFactExplorerRenderer.cs");
            string guidePath = FindGameplayEditorFile("PyralisAuthoringWindow.Guide.cs");

            string overviewSource = File.ReadAllText(overviewRendererPath);
            string factSource = File.ReadAllText(factRendererPath);
            string guideSource = File.ReadAllText(guidePath);

            Assert.That(overviewSource.Contains("BuildProofSupportRows"), Is.False);
            Assert.That(factSource.Contains("BuildProofSupportRows"), Is.True);
            Assert.That(factSource.Contains("BuildReflectiveContractRows"), Is.True);
            Assert.That(guideSource.Contains("BuildCurrentIntentGuideRows"), Is.True);
            Assert.That(guideSource.Contains("BuildReflectiveContractRows"), Is.True);
        }

        [Test]
        public void PyralisAuthoringWindow_GuideUsesGraphProjectionWithoutIntentAdvisorFallback()
        {
            string guidePath = FindGameplayEditorFile("PyralisAuthoringWindow.Guide.cs");
            string guideSource = File.ReadAllText(guidePath);

            Assert.That(guideSource.Contains("PyralisAuthoringSetupGraphProjection.BuildCurrentIntentGuideRows"), Is.True);
            Assert.That(guideSource.Contains("DrawGuideGraphRows(graphRows)"), Is.True);
            Assert.That(guideSource.Contains("Guide renders the resolved setup graph"), Is.True);
            Assert.That(guideSource.Contains("Pre-setup intent guidance"), Is.False);
            Assert.That(guideSource.Contains("GetCachedIntentModel"), Is.False);
            Assert.That(guideSource.Contains("DrawIntentRows"), Is.False);
        }

        [Test]
        public void PyralisAuthoringWindow_SelectedContextDetailsComeFromGraphProjection()
        {
            string selectedContextPath = FindGameplayEditorFile("PyralisSelectedContextRenderer.cs");
            string selectedContextSource = File.ReadAllText(selectedContextPath);

            Assert.That(selectedContextSource.Contains("BuildSelectedContextRow"), Is.True);
            Assert.That(selectedContextSource.Contains(".description"), Is.False);
            Assert.That(selectedContextSource.Contains(".setupNotes"), Is.False);
            Assert.That(selectedContextSource.Contains(".presentationLanes"), Is.False);
            Assert.That(selectedContextSource.Contains(".firstProofRequirements"), Is.False);
            Assert.That(selectedContextSource.Contains(".runtimePatterns"), Is.False);

            string graphProjectionPath = FindGameplayEditorFile("PyralisAuthoringSetupGraphProjection.cs");
            string graphProjectionSource = File.ReadAllText(graphProjectionPath);
            Assert.That(graphProjectionSource.Contains("PyralisRuntimePatternVocabulary"), Is.True);
        }

        [Test]
        public void PyralisAuthoringWindow_SurfacesDoNotRecomputeGraphSourceTruth()
        {
            string authoringWindowRoot = GameplayEditorLayer("Authoring", "Surfaces", "AuthoringWindow");
            string[] surfaceFiles = Directory.GetFiles(authoringWindowRoot, "*.cs", SearchOption.AllDirectories);

            foreach (string file in surfaceFiles)
            {
                string fileName = Path.GetFileName(file);
                string source = File.ReadAllText(file);

                Assert.That(
                    source.Contains("PyralisSetupFlowValidator.BuildReport"),
                    Is.False,
                    $"{fileName} should read setup-flow evidence through the resolved setup graph.");
                Assert.That(
                    source.Contains("PyralisSceneReadinessValidator.BuildReport"),
                    Is.False,
                    $"{fileName} should read scene-readiness evidence through the resolved setup graph.");
                Assert.That(
                    source.Contains("PyralisSetupRouteAnalysis.Build"),
                    Is.False,
                    $"{fileName} should read route shape through graph projection or setup context, not direct route analysis.");

                Assert.That(
                    source.Contains("PyralisAuthoringIntentAdvisor"),
                    Is.False,
                    $"{fileName} should read intent summaries through graph projection instead of direct advisor fallback.");

                if (fileName != "PyralisAuthoringWindow.cs")
                {
                    Assert.That(
                        source.Contains("PyralisAuthoringSetupGraphBuilder.Build"),
                        Is.False,
                        $"{fileName} should receive the cached graph from the Authoring Window shell.");
                }
            }
        }

        [Test]
        public void PyralisAuthoringWindow_FactsAndCapabilityCatalogReadThroughGraphProjection()
        {
            string factRendererPath = FindGameplayEditorFile("PyralisAuthoringFactExplorerRenderer.cs");
            string catalogRendererPath = FindGameplayEditorFile("PyralisCapabilityVocabularyRenderer.cs");
            string graphProjectionPath = FindGameplayEditorFile("PyralisAuthoringSetupGraphProjection.cs");

            string factRendererSource = File.ReadAllText(factRendererPath);
            string catalogRendererSource = File.ReadAllText(catalogRendererPath);
            string graphProjectionSource = File.ReadAllText(graphProjectionPath);

            Assert.That(factRendererSource.Contains("PyralisAuthoringGrammarRegistry.AllFacts"), Is.False);
            Assert.That(factRendererSource.Contains("PyralisAuthoringSetupGraphProjection.BuildCookbookFacts"), Is.True);
            Assert.That(catalogRendererSource.Contains("PyralisAuthoringGrammarRegistry.AllFacts"), Is.False);
            Assert.That(catalogRendererSource.Contains("PyralisAuthoringSetupGraphProjection.BuildRuntimeCapabilityFactsForCapability"), Is.True);
            Assert.That(catalogRendererSource.Contains("PyralisAuthoringSetupGraphProjection.BuildRuntimeCapabilityFactsForLane"), Is.True);
            Assert.That(graphProjectionSource.Contains("BuildCookbookFacts"), Is.True);
            Assert.That(graphProjectionSource.Contains("BuildRuntimeCapabilityFactsForCapability"), Is.True);
            Assert.That(graphProjectionSource.Contains("BuildRuntimeCapabilityFactsForLane"), Is.True);
        }

        [Test]
        public void PyralisAuthoringWindow_IntentProjectionUsesReflectedCapabilityDescriptors()
        {
            string projectionPath = FindGameplayEditorFile("PyralisIntentCapabilityProjection.cs");
            string intentPath = FindGameplayEditorFile("PyralisAuthoringWindow.Intent.cs");
            string descriptorPath = FindGameplayEditorFile("PyralisAuthoringCapabilityDescriptor.cs");
            string projectionSource = File.ReadAllText(projectionPath);
            string intentSource = File.ReadAllText(intentPath);
            string descriptorSource = File.ReadAllText(descriptorPath);

            Assert.That(projectionSource.Contains("PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies"), Is.True);
            Assert.That(projectionSource.Contains("PyralisRuntimeCapabilityFamilyMap.GetFamilies"), Is.False);
            Assert.That(intentSource.Contains("BuildIntentCapabilityGroups"), Is.True);
            Assert.That(intentSource.Contains("new Dictionary<string, (AuthoringCapability[] caps"), Is.False);
            Assert.That(descriptorSource.Contains("ResolvedAuthoringContractRegistry.All"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_GuidesTabletopSelectionBridgeAuthoring()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");
            string bridgePath = Path.Combine(gameplayRoot, "Core", "Rules", "Board", "TabletopBoardSelectionBridge.cs");
            string editorPath = FindGameplayEditorFile("TabletopBoardSelectionBridgeEditor.cs");

            Assert.That(File.Exists(bridgePath), Is.True);
            Assert.That(File.Exists(editorPath), Is.True);

            string bridgeSource = File.ReadAllText(bridgePath);
            string editorSource = File.ReadAllText(editorPath);

            Assert.That(bridgeSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Tabletop/Tabletop Board Selection Bridge\")]"), Is.True);
            Assert.That(bridgeSource.Contains("TrySelectPieceAt"), Is.True);
            Assert.That(bridgeSource.Contains("TrySelectDestination"), Is.True);
            Assert.That(bridgeSource.Contains("BoardMoveActionPayload"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(TabletopBoardSelectionBridge))"), Is.True);
            Assert.That(editorSource.Contains("Guided Authoring: Tabletop Board Selection Bridge"), Is.True);
            Assert.That(editorSource.Contains("BoardDefinition"), Is.True);
            Assert.That(editorSource.Contains("ActionQueueService"), Is.True);
            Assert.That(editorSource.Contains("project-owned board presenter"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_GuidesTabletopGridPresenterAuthoring()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");
            string presenterPath = Path.Combine(gameplayRoot, "Features", "Tabletop", "TabletopBoardGridPresenter.cs");
            string editorPath = FindGameplayEditorFile("TabletopBoardGridPresenterEditor.cs");
            string setupDocPath = AuthoringDoc("Prefabs", "Board_Card_Tabletop_Setup.md");

            Assert.That(File.Exists(presenterPath), Is.True);
            Assert.That(File.Exists(editorPath), Is.True);

            string presenterSource = File.ReadAllText(presenterPath);
            string editorSource = File.ReadAllText(editorPath);
            string setupDoc = File.ReadAllText(setupDocPath);

            Assert.That(presenterSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Tabletop/Tabletop Board Grid Presenter\")]"), Is.True);
            Assert.That(presenterSource.Contains("namespace NeonBlack.Gameplay.Features.Tabletop"), Is.True);
            Assert.That(presenterSource.Contains("using NeonBlack.Gameplay.Core.Rules.Board;"), Is.True);
            Assert.That(presenterSource.Contains("TabletopBoardSelectionBridge"), Is.True);
            Assert.That(presenterSource.Contains("BoardDefinition"), Is.True);
            Assert.That(presenterSource.Contains("BoardMovePolicyDefinition"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(TabletopBoardGridPresenter))"), Is.True);
            Assert.That(editorSource.Contains("Guided Authoring: Tabletop Board Grid Presenter"), Is.True);
            Assert.That(setupDoc.Contains("TabletopBoardGridPresenter"), Is.True);
        }

        [Test]
        public void PyralisPackage_Source_ExposesPackageManagerSampleAndCurrentVersion()
        {
            string packageRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub");

            string packageJsonPath = Path.Combine(packageRoot, "package.json");
            string runtimeMarkerPath = Path.Combine(packageRoot, "Runtime", "RuntimeExample.cs");
            string editorMarkerPath = Path.Combine(packageRoot, "Editor", "EditorExample.cs");
            string samplesPath = Path.Combine(packageRoot, "Samples~", "Example");

            Assert.That(File.Exists(packageJsonPath), Is.True);
            Assert.That(Directory.Exists(samplesPath), Is.True);

            string packageJson = File.ReadAllText(packageJsonPath);
            string runtimeMarker = File.ReadAllText(runtimeMarkerPath);
            string editorMarker = File.ReadAllText(editorMarkerPath);

            Assert.That(packageJson.Contains("\"version\": \"0.2.0\""), Is.True);
            Assert.That(packageJson.Contains("\"samples\""), Is.True);
            Assert.That(packageJson.Contains("\"Samples~/Example\""), Is.True);
            Assert.That(packageJson.Contains("\"com.unity.addressables\": \"2.9.1\""), Is.True);
            Assert.That(packageJson.Contains("\"com.unity.localization\": \"1.5.8\""), Is.True);
            Assert.That(packageJson.Contains("\"com.unity.netcode.gameobjects\": \"2.10.0\""), Is.True);
            Assert.That(packageJson.Contains("\"com.unity.transport\": \"2.6.0\""), Is.True);
            Assert.That(runtimeMarker.Contains("Version   = \"0.2.0\""), Is.True);
            Assert.That(editorMarker.Contains("Version   = \"0.2.0\""), Is.True);
        }

        [Test]
        public void PyralisPackage_Source_DoesNotShipLegacyRuntimeMemberScripts()
        {
            string packageRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub");

            string legacyRuntimeMembers = Path.Combine(packageRoot, "Runtime", "Members");
            Assert.That(Directory.Exists(legacyRuntimeMembers), Is.False, "Package delivery must not include stale Runtime/Members copies. Use Members/Pyralis/Gameplay as the active source root.");

            string legacyForwardPath = string.Join("/", "Runtime", "Members", "Pyralis", "Neon Black", "Scripts");
            string legacyBackslashPath = string.Join("\\", "Runtime", "Members", "Pyralis", "Neon Black", "Scripts");

            foreach (string path in Directory.GetFiles(packageRoot, "*.*", SearchOption.AllDirectories))
            {
                string normalized = path.Replace(Path.DirectorySeparatorChar, '/');
                Assert.That(normalized.Contains(legacyForwardPath), Is.False, path);

                string extension = Path.GetExtension(path);
                if (extension != ".cs" && extension != ".md" && extension != ".json" && extension != ".asmdef")
                    continue;

                string source = File.ReadAllText(path);
                Assert.That(source.Contains(legacyForwardPath), Is.False, path);
                Assert.That(source.Contains(legacyBackslashPath), Is.False, path);
            }
        }

        [Test]
        public void PyralisNetworking_Source_UsesParticipantSpecificOwnership()
        {
            string networkingRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Networking");

            string spawnSource = File.ReadAllText(Path.Combine(networkingRoot, "Characters", "NetworkedParticipantSpawnService.cs"));
            string authoritySource = File.ReadAllText(Path.Combine(networkingRoot, "Runtime", "NetworkedParticipantAuthorityService.cs"));

            Assert.That(spawnSource.Contains("participant.OwnerClientId"), Is.True);
            Assert.That(spawnSource.Contains("SpawnWithOwnership(participant.OwnerClientId"), Is.True);
            Assert.That(authoritySource.Contains("ResolveOwnerClientId(playerInput, seatIndex)"), Is.True);
            Assert.That(authoritySource.Contains("ownerClientId == networkManager.LocalClientId"), Is.True);
            Assert.That(authoritySource.Contains("networkManager.IsHost || networkManager.IsClient"), Is.False);
        }

        [Test]
        public void PyralisEditor_Source_UsesGuidedAuthoringForRemainingMenuFacingRuntimes()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string enemyEditorPath = Path.Combine(gameplayRoot, "Features", "Enemies", "3D", "Editor", "Authoring", "EnemyFeatureRuntimeGuidedEditors.cs");
            string hazardEditorPath = Path.Combine(gameplayRoot, "Features", "Hazards", "Editor", "Authoring", "HazardRuntimeGuidedEditors.cs");
            string traversalEditorPath = Path.Combine(gameplayRoot, "Features", "Traversal", "Editor", "Authoring", "PawnTraversalFeatureRuntime3DEditor.cs");
            string ambientRuntimePath = Path.Combine(gameplayRoot, "Features", "Enemies", "EnemyAmbientFeatureRuntime.cs");
            string reactionRuntimePath = Path.Combine(gameplayRoot, "Features", "Enemies", "EnemyReactionFeatureRuntime.cs");
            string hazardFeedbackRuntimePath = Path.Combine(gameplayRoot, "Features", "Hazards", "HazardFeedbackRuntime.cs");
            string damageZoneRuntimePath = Path.Combine(gameplayRoot, "Features", "Zones", "2D", "DamageZone2D.cs");
            string traversalRuntimePath = Path.Combine(gameplayRoot, "Features", "Traversal", "Runtime", "3D", "PawnTraversalFeatureRuntime3D.cs");

            Assert.That(File.Exists(enemyEditorPath), Is.True);
            Assert.That(File.Exists(hazardEditorPath), Is.True);
            Assert.That(File.Exists(traversalEditorPath), Is.True);

            string enemyEditorSource = File.ReadAllText(enemyEditorPath);
            string hazardEditorSource = File.ReadAllText(hazardEditorPath);
            string traversalEditorSource = File.ReadAllText(traversalEditorPath);
            AssertNoMojibake(enemyEditorSource, enemyEditorPath);
            AssertNoMojibake(hazardEditorSource, hazardEditorPath);
            AssertNoMojibake(traversalEditorSource, traversalEditorPath);
            AssertNoMojibake(File.ReadAllText(ambientRuntimePath), ambientRuntimePath);
            AssertNoMojibake(File.ReadAllText(reactionRuntimePath), reactionRuntimePath);
            AssertNoMojibake(File.ReadAllText(hazardFeedbackRuntimePath), hazardFeedbackRuntimePath);
            AssertNoMojibake(File.ReadAllText(damageZoneRuntimePath), damageZoneRuntimePath);
            AssertNoMojibake(File.ReadAllText(traversalRuntimePath), traversalRuntimePath);

            Assert.That(enemyEditorSource.Contains("CustomEditor(typeof(EnemyAmbientFeatureRuntime))"), Is.True);
            Assert.That(enemyEditorSource.Contains("CustomEditor(typeof(EnemyReactionFeatureRuntime))"), Is.True);
            Assert.That(enemyEditorSource.Contains("ResolvedAuthoringContractGuideText.FeatureModuleSetup"), Is.True);
            Assert.That(enemyEditorSource.Contains("enemy.ambient"), Is.False);
            Assert.That(enemyEditorSource.Contains("enemy.reaction"), Is.False);
            Assert.That(hazardEditorSource.Contains("CustomEditor(typeof(HazardFeedbackRuntime))"), Is.True);
            Assert.That(hazardEditorSource.Contains("Popup Camera is empty"), Is.True);
            Assert.That(hazardEditorSource.Contains("CustomEditor(typeof(DamageZone2D))"), Is.True);
            Assert.That(hazardEditorSource.Contains("Collider2D should be set to Is Trigger"), Is.True);
            Assert.That(File.ReadAllText(hazardFeedbackRuntimePath).Contains("Camera.main"), Is.False);
            Assert.That(File.ReadAllText(hazardFeedbackRuntimePath).Contains("SetPopupCamera"), Is.True);
            Assert.That(traversalEditorSource.Contains("CustomEditor(typeof(PawnTraversalFeatureRuntime3D))"), Is.True);
            Assert.That(traversalEditorSource.Contains("ResolvedAuthoringContractGuideText.FeatureModuleSetup"), Is.True);
            Assert.That(traversalEditorSource.Contains("actor.traversal.3d"), Is.False);
            Assert.That(traversalEditorSource.Contains("Pawn3DTraversalComponent is required"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesGuidedAuthoringForCombatDamageSceneComponents()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Features", "Combat", "Editor", "Authoring", "CombatDamageGuidedEditors.cs");
            string hitBox2DPath = Path.Combine(gameplayRoot, "Features", "Combat", "2D", "HitBox2D.cs");
            string projectilePath = Path.Combine(gameplayRoot, "Features", "Combat", "Projectile.cs");
            string projectile2DPath = Path.Combine(gameplayRoot, "Features", "Combat", "2D", "Projectile2D.cs");
            string projectileLauncherPath = Path.Combine(gameplayRoot, "Features", "Combat", "ProjectileLauncherBase.cs");
            string projectileImpactEffectPath = Path.Combine(gameplayRoot, "Features", "Combat", "ProjectileImpactEffectPlayer.cs");
            string knockbackPath = Path.Combine(gameplayRoot, "Features", "Combat", "KnockbackReceiver.cs");
            string hitFlashPath = Path.Combine(gameplayRoot, "Features", "Combat", "HitFlash.cs");
            string damageNumberPath = Path.Combine(gameplayRoot, "Features", "Combat", "DamageNumber.cs");
            string damageNumberSpawnerPath = Path.Combine(gameplayRoot, "Features", "Combat", "DamageNumberSpawner.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(hitBox2DPath), hitBox2DPath);
            AssertNoMojibake(File.ReadAllText(projectilePath), projectilePath);
            AssertNoMojibake(File.ReadAllText(projectile2DPath), projectile2DPath);
            AssertNoMojibake(File.ReadAllText(projectileLauncherPath), projectileLauncherPath);
            AssertNoMojibake(File.ReadAllText(projectileImpactEffectPath), projectileImpactEffectPath);
            AssertNoMojibake(File.ReadAllText(knockbackPath), knockbackPath);
            AssertNoMojibake(File.ReadAllText(hitFlashPath), hitFlashPath);
            AssertNoMojibake(File.ReadAllText(damageNumberPath), damageNumberPath);
            AssertNoMojibake(File.ReadAllText(damageNumberSpawnerPath), damageNumberSpawnerPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(HitBox2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(Projectile))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(Projectile2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(KnockbackReceiver))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(HitFlash))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(DamageNumber))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(DamageNumberSpawner))"), Is.True);
            Assert.That(editorSource.Contains("Collider2D should be set to Is Trigger"), Is.True);
            Assert.That(editorSource.Contains("Projectile needs at least one trigger Collider"), Is.True);
            Assert.That(editorSource.Contains("Hit Pause Sink must reference a component that implements IHitPauseSink"), Is.True);
            Assert.That(editorSource.Contains("Camera Shake Sink must reference a component that implements ICameraShakeSink"), Is.True);
            Assert.That(editorSource.Contains("CharacterController is required for 3D knockback"), Is.True);
            Assert.That(editorSource.Contains("IDamageNumberSink"), Is.True);
            Assert.That(editorSource.Contains("Popup Camera is empty"), Is.True);
            Assert.That(File.ReadAllText(damageNumberPath).Contains("DamageNumberSpawner.Instance"), Is.False);
            Assert.That(File.ReadAllText(damageNumberPath).Contains("Camera.main"), Is.False);
            Assert.That(File.ReadAllText(damageNumberSpawnerPath).Contains("IDamageNumberSink"), Is.True);
            Assert.That(File.ReadAllText(damageNumberSpawnerPath).Contains("static DamageNumberSpawner Instance"), Is.False);
            Assert.That(File.ReadAllText(projectilePath).Contains("TimeManager.Instance"), Is.False);
            Assert.That(File.ReadAllText(projectilePath).Contains("CameraShake.Instance"), Is.False);
            Assert.That(File.ReadAllText(projectileLauncherPath).Contains("IHitPauseSink"), Is.True);
            Assert.That(File.ReadAllText(projectileLauncherPath).Contains("ICameraShakeSink"), Is.True);
            Assert.That(File.ReadAllText(projectileImpactEffectPath).Contains("TimeManager.Instance"), Is.False);
            Assert.That(File.ReadAllText(projectileImpactEffectPath).Contains("CameraShake.Instance"), Is.False);
        }

        [Test]
        public void PyralisEditor_Source_KeepsFlaggedEditorToolsSafeAndReadable()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string sceneViewToolsPath = FindGameplayEditorFile("SceneViewTools.cs");
            string inputZoneSetPath = Path.Combine(gameplayRoot, "Features", "Input", "2D", "InputZoneSet.cs");
            string inputZoneEditorPath = Path.Combine(gameplayRoot, "Features", "Input", "2D", "Editor", "Authoring", "InputZoneSetEditor.cs");
            string orientationHandlerPath = Path.Combine(gameplayRoot, "Features", "UI", "UIOrientationHandler.cs");
            string orientationEditorPath = Path.Combine(gameplayRoot, "Features", "UI", "Editor", "Authoring", "UIOrientationHandlerEditor.cs");

            string sceneViewTools = File.ReadAllText(sceneViewToolsPath);
            string inputZoneSet = File.ReadAllText(inputZoneSetPath);
            string inputZoneEditor = File.ReadAllText(inputZoneEditorPath);
            string orientationHandler = File.ReadAllText(orientationHandlerPath);
            string orientationEditor = File.ReadAllText(orientationEditorPath);

            AssertNoMojibake(sceneViewTools, sceneViewToolsPath);
            AssertNoMojibake(inputZoneSet, inputZoneSetPath);
            AssertNoMojibake(inputZoneEditor, inputZoneEditorPath);
            AssertNoMojibake(orientationHandler, orientationHandlerPath);
            AssertNoMojibake(orientationEditor, orientationEditorPath);

            Assert.That(sceneViewTools.Contains("EditorPrefs.GetBool(PREF_ROTATION, false)"), Is.True);
            Assert.That(sceneViewTools.Contains("EditorPrefs.GetBool(PREF_LAYER, false)"), Is.True);
            Assert.That(sceneViewTools.Contains("EventType.MouseDrag"), Is.True);
            Assert.That(inputZoneEditor.Contains("Undo.RecordObject(asset"), Is.True);
            Assert.That(inputZoneEditor.Contains("zone.InvalidateBounds()"), Is.True);
            Assert.That(inputZoneEditor.Contains("Camera.main"), Is.False);
            Assert.That(orientationHandler.Contains("UnityEditor"), Is.False);
            Assert.That(orientationEditor.Contains("PrefabUtility.RecordPrefabInstancePropertyModifications"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_ExposesCoreSceneComponentsInAddComponentMenu()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string bootstrapSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "GameplaySessionBootstrap.cs"));
            string gameManagerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "GameFlow", "2D", "GameManager.cs"));
            string healthSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "HealthComponent.cs"));
            string hazardSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.cs"));
            string enemySource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Enemies", "3D", "EnemyAI.cs"));
            string inputSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Input", "2D", "PlayerInputHandler.cs"));
            string scoreSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Scoring", "Runtime", "Shared", "ParticipantScoreService.cs"));

            Assert.That(bootstrapSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Setup/Gameplay Session Bootstrap\")]"), Is.True);
            Assert.That(gameManagerSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Game Flow/2D Game Manager\")]"), Is.True);
            Assert.That(healthSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Combat/Health Component\")]"), Is.True);
            Assert.That(hazardSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Hazards/2D Hazard\")]"), Is.True);
            Assert.That(enemySource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Enemies/Enemy AI\")]"), Is.True);
            Assert.That(inputSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Input/2D Player Input Handler\")]"), Is.True);
            Assert.That(scoreSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Scoring/Participant Score Service\")]"), Is.True);
            Assert.That(gameManagerSource.Contains("PlayerRegistry.Motor2D"), Is.False);
            Assert.That(gameManagerSource.Contains("PlayerRegistry.Player"), Is.False);
            Assert.That(gameManagerSource.Contains("ParticipantRosterService"), Is.True);
            Assert.That(gameManagerSource.Contains("AuthoringContract"), Is.True);
        }

        [Test]
        public void PyralisEditor_PyralisCapabilityVocabularyCards_AreBackedByStableAuthoringFacts()
        {
            Assert.That(PyralisAuthoringGrammarRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            IReadOnlyList<PyralisCapabilityVocabularyCard> cards = PyralisCapabilityVocabulary.All;
            Assert.That(cards.Count, Is.GreaterThanOrEqualTo(5));

            for (int i = 0; i < cards.Count; i++)
            {
                PyralisCapabilityVocabularyCard card = cards[i];
                Assert.That(card.StableId, Is.Not.Empty);
                Assert.That(card.Fact, Is.Not.Null);
                Assert.That(card.Fact.StableId, Is.EqualTo(card.StableId));
                Assert.That(card.Fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.RuntimeCapability));
                Assert.That(card.Fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.HandAuthoredGuideCard));
                Assert.That(card.Fact.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
                Assert.That(card.Fact.NativeActions.Length, Is.GreaterThanOrEqualTo(1));
                Assert.That(card.Fact.RequiredDefinitions, Is.Empty);
                Assert.That(card.Fact.RequiredProfiles, Is.Empty);
                Assert.That(card.Fact.RequiredSceneComponents, Is.Empty);
                Assert.That(card.Fact.RequiredUnitySurfaces, Is.Empty);
                Assert.That(card.Fact.AssignmentFields, Is.Empty);
                Assert.That(card.Fact.CustomizationMoments, Is.Empty);

                PyralisAuthoringFact registeredFact = PyralisAuthoringGrammarRegistry.Find(card.StableId);
                Assert.That(registeredFact, Is.Not.Null);
                Assert.That(registeredFact.StableId, Is.EqualTo(card.Fact.StableId));
                Assert.That(registeredFact.Kind, Is.EqualTo(PyralisAuthoringFactKind.RuntimeCapability));
                Assert.That(registeredFact.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
            }
        }

        [Test]
        public void PyralisEditor_CapabilityVocabulary_DoesNotOwnRetiredRecipePaths()
        {
            string catalogPath = FindGameplayEditorFile("PyralisCapabilityVocabulary.cs");
            string catalogSource = File.ReadAllText(catalogPath);

            Assert.That(catalogSource.Contains("GetByGoal("), Is.False);
            Assert.That(catalogSource.Contains("GetByLane("), Is.False);
            Assert.That(catalogSource.Contains("CommonNextCapabilities"), Is.False);
            Assert.That(catalogSource.Contains("ProofStepLabel"), Is.False);
            Assert.That(catalogSource.Contains("ProofStepSuccessCriteria"), Is.False);
            Assert.That(
                Directory.GetFiles(GameplayRoot, "PyralisAuthoring" + "Route" + "Proof.cs", SearchOption.AllDirectories)
                    .Where(path => path.Contains(Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar))
                    .ToArray(),
                Is.Empty);
        }

        [Test]
        public void PyralisAuthoring2_Source_KeepsAuthoringSpineOutOfPlayerBuildAssemblies()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string[] asmdefPaths = Directory.GetFiles(gameplayRoot, "*.asmdef", SearchOption.AllDirectories);
            foreach (string asmdefPath in asmdefPaths)
            {
                string source = File.ReadAllText(asmdefPath);
                string name = Path.GetFileNameWithoutExtension(asmdefPath);

                if (name.EndsWith(".Editor"))
                {
                    Assert.That(source.Contains("\"includePlatforms\""), Is.True, asmdefPath);
                    Assert.That(source.Contains("\"Editor\""), Is.True, asmdefPath);
                    continue;
                }

                Assert.That(source.Contains("\"NeonBlack.Gameplay.Editor\""), Is.False, asmdefPath);
            }

            string[] sourcePaths = Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories);
            foreach (string sourcePath in sourcePaths)
            {
                string normalized = sourcePath.Replace('\\', '/');
                string source = File.ReadAllText(sourcePath);
                bool isEditorSource = normalized.Contains("/Editor/");

                if (SourceContainsAuthoringExportSpine(source))
                {
                    Assert.That(isEditorSource, Is.True, sourcePath);
                }

                if (!isEditorSource)
                {
                    Assert.That(source.Contains("using NeonBlack.Gameplay.Editor"), Is.False, sourcePath);
                    Assert.That(source.Contains("NeonBlack.Gameplay.Editor."), Is.False, sourcePath);
                    Assert.That(source.Contains("using UnityEditor;") && !source.Contains("#if UNITY_EDITOR"), Is.False, sourcePath);
                }
            }
        }

        [Test]
        public void PyralisAuthoring2_Docs_NameExportFootprintAndBuildReportPromotionGate()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string authoringBlueprint = File.ReadAllText(AuthoringDoc("AUTHORING_BLUEPRINT.md"));
            string authoringModel = File.ReadAllText(AuthoringDoc("AUTHORING_MODEL.md"));
            string roadmap = File.ReadAllText(Path.Combine(gameplayRoot, "Docs", "FEATURE_DEVELOPMENT_ROADMAP.md"));

            Assert.That(authoringBlueprint.Contains("Export Footprint Boundary"), Is.True);
            Assert.That(authoringBlueprint.Contains("Unity build reports"), Is.True);
            Assert.That(authoringBlueprint.Contains("unexpected editor assemblies"), Is.True);
            Assert.That(authoringModel.Contains("Build Footprint Model"), Is.True);
            Assert.That(authoringModel.Contains("Future build-report checks"), Is.True);
            Assert.That(roadmap.Contains("BuildReport / Export Footprint"), Is.True);
            Assert.That(roadmap.Contains("editor-only authoring contracts/facts/providers/validators out of player builds"), Is.True);
        }

        private static void AssertReflectiveAuthoringLayerSupportsCurrentTruth()
        {
            string gameplayRoot = GameplayRoot;
            string overlayPath = FindGameplayEditorFile("PyralisReflectiveInspectorOverlay.cs");
            string guidePath = FindGameplayEditorFile("PyralisInspectorGuide.cs");
            string factRegistryPath = FindGameplayEditorFile("PyralisAuthoringGrammarRegistry.cs");
            string factScannerPath = FindGameplayEditorFile("PyralisReflectiveFactScanner.cs");
            string routeAnalysisPath = FindGameplayEditorFile("PyralisSetupRouteAnalysis.cs");
            string graphProjectionPath = FindGameplayEditorFile("PyralisAuthoringSetupGraphProjection.cs");
            string currentStepGuidancePath = FindGameplayEditorFile("PyralisCurrentStepPrimaryActionGuidance.cs");
            string authoringWindowPath = FindGameplayEditorFile("PyralisAuthoringWindow.cs");

            Assert.That(File.Exists(overlayPath), Is.True);
            Assert.That(File.Exists(guidePath), Is.True);
            Assert.That(File.Exists(factRegistryPath), Is.True);
            Assert.That(File.Exists(factScannerPath), Is.True);
            Assert.That(File.Exists(routeAnalysisPath), Is.True);
            Assert.That(File.Exists(graphProjectionPath), Is.True);
            Assert.That(File.Exists(currentStepGuidancePath), Is.True);
            Assert.That(File.Exists(authoringWindowPath), Is.True);

            string overlaySource = File.ReadAllText(overlayPath);
            string guideSource = File.ReadAllText(guidePath);
            string factRegistrySource = File.ReadAllText(factRegistryPath);
            string factScannerSource = File.ReadAllText(factScannerPath);
            string routeAnalysisSource = File.ReadAllText(routeAnalysisPath);
            string graphProjectionSource = File.ReadAllText(graphProjectionPath);
            string currentStepGuidanceSource = File.ReadAllText(currentStepGuidancePath);
            string authoringWindowSource = File.ReadAllText(authoringWindowPath);

            Assert.That(
                overlaySource.Contains("CustomEditor(typeof(UnityEngine.Object), true)") ||
                overlaySource.Contains("CustomEditor(typeof(Object), true)"),
                Is.True);
            Assert.That(overlaySource.Contains("PyralisInspectorGuide"), Is.True);
            Assert.That(guideSource.Contains("Use this Inspector for field assignment, local customization, and field-local validation"), Is.True);
            Assert.That(factRegistrySource.Contains("PyralisReflectiveFactScanner.ScanProject()"), Is.True);
            Assert.That(factScannerSource.Contains("CreateAssetMenuAttribute"), Is.True);
            Assert.That(factScannerSource.Contains("AddComponentMenu"), Is.True);
            Assert.That(factScannerSource.Contains("SerializedField"), Is.True);
            Assert.That(graphProjectionSource.Contains("PyralisAuthoring" + "Route" + "Proof"), Is.False);
            Assert.That(graphProjectionSource.Contains("BuildCurrentStepRow"), Is.True);
            Assert.That(currentStepGuidanceSource.Contains("Inspector -> Add Component"), Is.True);
            Assert.That(currentStepGuidanceSource.Contains("GameplaySessionBootstrap"), Is.True);
            Assert.That(currentStepGuidanceSource.Contains("IsSceneSupportObject"), Is.True);
            Assert.That(currentStepGuidanceSource.Contains("Camera Root"), Is.True);
            Assert.That(currentStepGuidanceSource.Contains("PyralisSetupFlowValidator.BuildReport"), Is.False);
            Assert.That(currentStepGuidanceSource.Contains("PyralisSetupRouteAnalysis.Build"), Is.False);
            Assert.That(authoringWindowSource.Contains("Starter Packs"), Is.False);
            Assert.That(authoringWindowSource.Contains("CreatePawnStarterPack"), Is.False);
            Assert.That(authoringWindowSource.Contains("CreateTabletopStarterPack"), Is.False);

            string editorDirectorySegment = Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
            foreach (string editorFile in Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories).Where(path => path.Contains(editorDirectorySegment)))
            {
                string normalized = editorFile.Replace('\\', '/');
                Assert.That(normalized.Contains("/Editor/Authoring/"), Is.True, editorFile);
            }
        }
        private static bool SourceContainsAuthoringExportSpine(string source)
        {
            return source.Contains("PyralisAuthoringGrammarRegistry")
                || source.Contains("PyralisAuthoringIssue")
                || source.Contains("PyralisSetupFlowValidator")
                || source.Contains("PyralisAuthoringWindow");
        }

        [Test]
        public void PyralisGameplay_Source_CleansHighNoiseReadabilityFiles()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            foreach (string path in Directory.GetFiles(gameplayRoot, "*.*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(path);
                if (extension != ".cs" && extension != ".md")
                    continue;

                AssertNoMojibake(File.ReadAllText(path), path);
            }
        }
    }
}
