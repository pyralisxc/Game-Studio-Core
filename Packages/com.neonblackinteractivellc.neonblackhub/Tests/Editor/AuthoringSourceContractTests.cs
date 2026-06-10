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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public class AuthoringSourceContractTests : PyralisEditorTestSupport
    {
        [Test]
        public void PyralisEditor_Source_ExposesRuntimePatternAndGameSetupInspectors()
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

            string runtimePatternEditorPath = Path.Combine(editorRoot, "RuntimePatternDefinitionEditor.cs");
            string gameSetupEditorPath = Path.Combine(editorRoot, "GameSetupProfileEditor.cs");
            string authoringWindowPath = Path.Combine(editorRoot, "PyralisAuthoringWindow.cs");
            string routeReportPath = Path.Combine(editorRoot, "PyralisAuthoringRouteReport.cs");
            string setupRouteAnalysisPath = Path.Combine(editorRoot, "PyralisSetupRouteAnalysis.cs");
            string overviewPath = Path.Combine(editorRoot, "PyralisAuthoringOverviewSnapshot.cs");
            string sceneSurfaceGuidancePath = Path.Combine(editorRoot, "PyralisAuthoringSceneSurfaceGuidance.cs");
            string setupFlowMonitorPath = Path.Combine(editorRoot, "PyralisSetupFlowMonitor.cs");

            Assert.That(File.Exists(runtimePatternEditorPath), Is.True);
            Assert.That(File.Exists(gameSetupEditorPath), Is.True);
            Assert.That(File.Exists(routeReportPath), Is.True);
            Assert.That(File.Exists(setupRouteAnalysisPath), Is.True);
            Assert.That(File.Exists(overviewPath), Is.True);
            Assert.That(File.Exists(sceneSurfaceGuidancePath), Is.True);
            Assert.That(File.Exists(setupFlowMonitorPath), Is.True);

            string runtimePatternEditor = File.ReadAllText(runtimePatternEditorPath);
            string gameSetupEditor = File.ReadAllText(gameSetupEditorPath);
            string authoringWindow = File.ReadAllText(authoringWindowPath);
            string routeReportSource = File.ReadAllText(routeReportPath);
            string setupRouteAnalysisSource = File.ReadAllText(setupRouteAnalysisPath);
            string overviewSource = File.ReadAllText(overviewPath);
            string sceneSurfaceGuidanceSource = File.ReadAllText(sceneSurfaceGuidancePath);
            string setupFlowMonitorSource = File.ReadAllText(setupFlowMonitorPath);
            string routeProofSource = File.ReadAllText(Path.Combine(editorRoot, "PyralisAuthoringRouteProof.cs"));
            string factorySource = File.ReadAllText(Path.Combine(editorRoot, "GameplayStarterPackFactory.cs"));
            string runtimePatternAuthoringText = File.ReadAllText(Path.Combine(editorRoot, "RuntimePatternAuthoringText.cs"));
            string inputProfileEditorSource = File.ReadAllText(Path.Combine(editorRoot, "InputProfileEditor.cs"));
            string inputProfileSource = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Data",
                "Profiles",
                "InputProfile.cs"));
            string authoringExperienceVision = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Docs",
                "Setup",
                "AUTHORING_EXPERIENCE_VISION.md"));

            Assert.That(runtimePatternEditor.Contains("CustomEditor(typeof(RuntimePatternDefinition))"), Is.True);
            Assert.That(runtimePatternEditor.Contains("Supported Control Surfaces"), Is.True);
            Assert.That(gameSetupEditor.Contains("CustomEditor(typeof(GameSetupProfile))"), Is.True);
            Assert.That(gameSetupEditor.Contains("PyralisInspectorHandoff.DrawAuthoringButton"), Is.True);
            Assert.That(authoringWindow.Contains("DrawCreateButton<RuntimePatternDefinition>"), Is.False);
            Assert.That(authoringWindow.Contains("DrawCreateButton<GameSetupProfile>"), Is.False);
            Assert.That(authoringWindow.Contains("OnSelectionChange"), Is.True);
            Assert.That(authoringWindow.Contains("selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null"), Is.True);
            Assert.That(authoringWindow.Contains("_mode = AuthoringWindowMode.Guide"), Is.True);
            Assert.That(authoringWindow.Contains("ShouldShowSelectionFirstGuide"), Is.True);
            Assert.That(authoringWindow.Contains("Selected Object Next Step"), Is.True);
            Assert.That(authoringWindow.Contains("Inspector -> Add Component search for GameplaySessionBootstrap"), Is.True);
            Assert.That(authoringWindow.Contains("OnHierarchyChange"), Is.True);
            Assert.That(authoringWindow.Contains("OnProjectChange"), Is.True);
            Assert.That(authoringWindow.Contains("OnInspectorUpdate"), Is.True);
            Assert.That(authoringWindow.Contains("Project content pane/breadcrumb"), Is.True);
            Assert.That(authoringWindow.Contains("project-owned setup folder"), Is.True);
            Assert.That(authoringWindow.Contains("separate from imported art folders"), Is.True);
            Assert.That(setupFlowMonitorSource.Contains("keep imported art folders separate"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("PyralisAuthoringEvidenceState"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("LinkedToActiveSetup"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("ReadyToAttempt"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("PyralisAuthoringNativeAction"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("public static class PyralisAuthoringSurfaceBeacon"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("FocusSurface"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("Window/General/Project"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("Window/General/Hierarchy"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("Window/General/Inspector"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("Window/General/Game"), Is.True);
            Assert.That(authoringWindow.Contains("A found surface is evidence, not proof"), Is.True);
            Assert.That(authoringExperienceVision.Contains("Intent, Evidence, Proof"), Is.True);
            Assert.That(authoringExperienceVision.Contains("Start Guided Setup"), Is.True);
            Assert.That(routeReportSource.Contains("RuntimePatternDefinition pattern => pattern.GetValidationIssues()"), Is.True);
            Assert.That(routeReportSource.Contains("GameSetupProfile setup => setup.GetValidationIssues()"), Is.True);
            Assert.That(routeReportSource.Contains("CountAssignedSpawnPoints"), Is.True);
            Assert.That(routeReportSource.Contains("Enter Play Mode and confirm the first pawn spawns"), Is.True);
            Assert.That(routeReportSource.Contains("clean 1P proof"), Is.True);
            Assert.That(authoringWindow.Contains("CreateTabletopStarterPack"), Is.False);
            Assert.That(authoringWindow.Contains("CreatePawnStarterPack"), Is.False);
            Assert.That(authoringWindow.Contains("Selected Authoring Context"), Is.True);
            Assert.That(authoringWindow.Contains("DrawSelectedContext"), Is.True);
            Assert.That(authoringWindow.Contains("Active Setup"), Is.True);
            Assert.That(authoringWindow.Contains("Pin Selection As Active Setup"), Is.True);
            Assert.That(authoringWindow.Contains("Clear Pin"), Is.True);
            Assert.That(authoringWindow.Contains("Inspect Active Setup"), Is.True);
            Assert.That(authoringWindow.Contains("ResolveActiveSetup"), Is.True);
            Assert.That(authoringWindow.Contains("GetSetupContext"), Is.True);
            Assert.That(authoringWindow.Contains("GetSceneFallbackSetup"), Is.True);
            Assert.That(authoringWindow.Contains("Scene Gameplay Root"), Is.True);
            Assert.That(authoringWindow.Contains("Nothing is selected, so Authoring is using the single GameplaySessionBootstrap found in the open scene as the setup root."), Is.True);
            Assert.That(authoringWindow.Contains("GetBootstrapReferencingSelectedTransform"), Is.True);
            Assert.That(authoringWindow.Contains("GetOnlySceneBootstrap"), Is.True);
            Assert.That(authoringWindow.Contains("Steady Setup Context"), Is.True);
            Assert.That(authoringWindow.Contains("DrawGameObjectContext"), Is.True);
            Assert.That(authoringWindow.Contains("Fill Missing Guidance Text"), Is.True);
            Assert.That(authoringWindow.Contains("Open In Inspector"), Is.True);
            Assert.That(authoringWindow.Contains("Pyralis components on this GameObject"), Is.True);
            Assert.That(authoringWindow.Contains("RuntimePatternAuthoringText.GetSuggestedDescription"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(editorRoot, "GameplaySessionBootstrapEditor.cs")).Contains("defaultOpen: true"), Is.True);
            Assert.That(inputProfileSource.Contains("Gameplay Actions"), Is.True);
            Assert.That(inputProfileSource.Contains("actionBindings"), Is.True);
            Assert.That(inputProfileSource.Contains("GameplayInputActionRole"), Is.True);
            Assert.That(inputProfileSource.Contains("GameplayInputActionRole.Custom"), Is.True);
            Assert.That(inputProfileEditorSource.Contains("translation table"), Is.True);
            Assert.That(inputProfileEditorSource.Contains("Add Built-In Action"), Is.True);
            Assert.That(inputProfileEditorSource.Contains("Add Custom Action"), Is.True);
            Assert.That(authoringWindow.Contains("Setup Chain"), Is.True);
            Assert.That(authoringWindow.Contains("AuthoringWindowMode"), Is.True);
            Assert.That(authoringWindow.Contains("\"Overview\""), Is.True);
            Assert.That(authoringWindow.Contains("\"Guide\""), Is.True);
            Assert.That(authoringWindow.Contains("\"Map\""), Is.True);
            Assert.That(authoringWindow.Contains("\"Validate\""), Is.True);
            Assert.That(authoringWindow.Contains("\"Facts\""), Is.True);
            Assert.That(authoringWindow.Contains("\"Create\""), Is.False);
            Assert.That(authoringWindow.Contains("DrawContractBackedFeatureModuleSetup"), Is.True);
            Assert.That(authoringWindow.Contains("DrawFeatureContractSetupRecipes"), Is.True);
            Assert.That(authoringWindow.Contains("PyralisAuthoringContractRegistry.All"), Is.True);
            Assert.That(authoringWindow.Contains("Contract-Backed Feature Module Setup"), Is.True);
            Assert.That(authoringWindow.Contains("Native Setup Actions"), Is.True);
            Assert.That(authoringWindow.Contains("Project Window Create"), Is.True);
            Assert.That(authoringWindow.Contains("Inspector/Object Picker"), Is.True);
            Assert.That(authoringWindow.Contains("Prefab/Add Component"), Is.True);
            Assert.That(authoringWindow.Contains("Play Mode Proof"), Is.True);
            Assert.That(authoringWindow.Contains("DrawContractProofGuidance"), Is.True);
            Assert.That(authoringWindow.Contains("Contract Proof Targets"), Is.True);
            Assert.That(authoringWindow.Contains("Proof Target Exists"), Is.True);
            Assert.That(authoringWindow.Contains("Proof not run in Play Mode"), Is.True);
            Assert.That(authoringWindow.Contains("Unsupported Lane Cautions"), Is.True);
            Assert.That(authoringWindow.Contains("PyralisAuthoringContractProofGuidance.Build"), Is.True);
            Assert.That(routeProofSource.Contains("FindProofFact"), Is.True);
            Assert.That(routeProofSource.Contains("ProofTargetMissing"), Is.True);
            Assert.That(routeProofSource.Contains("ProofNotRunInPlayMode"), Is.True);
            Assert.That(routeProofSource.Contains("CollectActiveModuleContexts"), Is.True);
            Assert.That(authoringWindow.Contains("actor.pickups.2d"), Is.False);
            Assert.That(authoringWindow.Contains("actor.pickups.3d"), Is.False);
            Assert.That(authoringWindow.Contains("actor.combat.reaction"), Is.False);
            Assert.That(authoringWindow.Contains("actor.status"), Is.False);
            Assert.That(authoringWindow.Contains("actor.feedback"), Is.False);
            Assert.That(authoringWindow.Contains("enemy.reaction"), Is.False);
            Assert.That(authoringWindow.Contains("enemy.ambient"), Is.False);
            Assert.That(authoringWindow.Contains("Beginner Location Tags"), Is.True);
            Assert.That(authoringWindow.Contains("DrawBeginnerLocationLegend"), Is.True);
            Assert.That(authoringWindow.Contains("DrawSemanticTagStrip"), Is.True);
            Assert.That(authoringWindow.Contains("DrawSemanticTagBadge"), Is.True);
            Assert.That(authoringWindow.Contains("Click a matching surface beacon when a step names a Unity tab"), Is.True);
            Assert.That(authoringWindow.Contains("PyralisAuthoringSurfaceBeacon.DrawNativeAction"), Is.True);
            Assert.That(authoringWindow.Contains("GetWorkflowStepSurface"), Is.True);
            Assert.That(authoringWindow.Contains("SemanticTokenRules"), Is.True);
            Assert.That(authoringWindow.Contains("ColorizeSemanticTokens"), Is.True);
            Assert.That(authoringWindow.Contains("DrawSemanticMiniLabel"), Is.True);
            Assert.That(authoringWindow.Contains("GetSemanticMiniLabelStyle"), Is.True);
            Assert.That(authoringWindow.Contains("builder.Append(\"&gt;\")"), Is.False);
            Assert.That(authoringWindow.Contains("builder.Append(\"&lt;\")"), Is.False);
            Assert.That(authoringWindow.Contains("builder.Append(\"&amp;\")"), Is.False);
            Assert.That(authoringWindow.Contains("Project window, choose or create a project-owned setup folder"), Is.True);
            Assert.That(authoringWindow.Contains("Create -> NeonBlack -> Definitions -> Session Definition"), Is.True);
            Assert.That(authoringWindow.Contains("ShouldStartInIntent"), Is.True);
            Assert.That(authoringWindow.Contains("HasNoSetupContext"), Is.True);
            Assert.That(authoringWindow.Contains("_emptySceneIntentStartApplied"), Is.True);
            Assert.That(authoringWindow.Contains("GetModeAccentTag"), Is.True);
            Assert.That(authoringWindow.Contains("Facts is the advanced coverage map"), Is.True);
            Assert.That(authoringWindow.Contains("Project Intent And Capability Map"), Is.True);
            Assert.That(authoringWindow.Contains("World / Playfield"), Is.True);
            Assert.That(authoringWindow.Contains("Control Shape"), Is.True);
            Assert.That(authoringWindow.Contains("Choose only the goals you mean to explore"), Is.True);
            Assert.That(authoringWindow.Contains("does not fill a preset"), Is.True);
            Assert.That(authoringWindow.Contains("Use Suggested"), Is.False);
            Assert.That(authoringWindow.Contains("Mark Common"), Is.False);
            Assert.That(authoringWindow.Contains("ApplyIntentGoalDefaults"), Is.False);
            Assert.That(File.ReadAllText(Path.Combine(editorRoot, "PyralisAuthoringIntentAdvisor.cs")).Contains("GetDefaultGoals"), Is.False);
            Assert.That(authoringWindow.Contains("Matched Intent Families"), Is.True);
            Assert.That(authoringWindow.Contains("Active Authoring Focus"), Is.True);
            Assert.That(authoringWindow.Contains("DrawSemanticHelpBox(report.NextStep"), Is.True);
            Assert.That(authoringWindow.Contains("DrawSemanticHelpBox(GetPawnIssuePrimaryAction"), Is.True);
            Assert.That(authoringWindow.Contains("DrawSemanticMiniLabel($\"{status}: {message}\""), Is.True);
            Assert.That(authoringWindow.Contains("GetFactSemanticTags"), Is.True);
            Assert.That(authoringWindow.Contains("DrawOverviewMode"), Is.True);
            Assert.That(authoringWindow.Contains("Overview Dashboard"), Is.True);
            Assert.That(authoringWindow.Contains("DrawFirstProofCard"), Is.True);
            Assert.That(authoringWindow.Contains("First Playable Proof"), Is.True);
            Assert.That(authoringWindow.Contains("Setup Surface"), Is.True);
            Assert.That(authoringWindow.Contains("Proof Chain"), Is.True);
            Assert.That(authoringWindow.Contains("Defer Until After Proof"), Is.True);
            Assert.That(authoringWindow.Contains("Do Now"), Is.True);
            Assert.That(authoringWindow.Contains("Proof Enhancers"), Is.True);
            Assert.That(authoringWindow.Contains("Feature Cards"), Is.True);
            Assert.That(authoringWindow.Contains("Helpful native setup once Do Now is clear"), Is.True);
            Assert.That(authoringWindow.Contains("PyralisAuthoringOverviewModel.Build"), Is.True);
            Assert.That(authoringWindow.Contains("DrawOverviewGuidanceCard"), Is.True);
            Assert.That(authoringWindow.Contains("Guidance"), Is.True);
            Assert.That(authoringWindow.Contains("First Proof"), Is.True);
            Assert.That(authoringWindow.Contains("Proof Status"), Is.True);
            Assert.That(authoringWindow.Contains("Open Map"), Is.True);
            Assert.That(authoringWindow.Contains("Open Validate"), Is.True);
            Assert.That(authoringWindow.Contains("Inspect Best Target"), Is.True);
            Assert.That(authoringWindow.Contains("GetBestOverviewTarget"), Is.True);
            Assert.That(authoringWindow.Contains("Readiness Summary"), Is.True);
            Assert.That(authoringWindow.Contains("DrawReadinessSummary"), Is.True);
            Assert.That(authoringWindow.Contains("DrawModeToolbar"), Is.True);
            Assert.That(authoringWindow.Contains("DrawModeToolbarTab"), Is.True);
            Assert.That(authoringWindow.Contains("ColorizeModeTabLabel"), Is.True);
            Assert.That(authoringWindow.Contains("GetModeToolbarButtonStyle"), Is.True);
            Assert.That(authoringWindow.Contains("DrawGuideMode"), Is.True);
            Assert.That(authoringWindow.Contains("What This Selection Does"), Is.True);
            Assert.That(authoringWindow.Contains("Important Values"), Is.True);
            Assert.That(authoringWindow.Contains("What To Check First"), Is.True);
            Assert.That(authoringWindow.Contains("DrawCurrentStepPanel"), Is.True);
            Assert.That(authoringWindow.Contains("Current Step"), Is.True);
            Assert.That(authoringWindow.Contains("Primary Action"), Is.True);
            Assert.That(authoringWindow.Contains("currentStepSelection"), Is.True);
            Assert.That(authoringWindow.Contains("Runtime capabilities name the route intent before participant and pawn wiring becomes meaningful"), Is.True);
            Assert.That(gameSetupEditor.Contains("DrawCapabilityRow"), Is.True);
            Assert.That(gameSetupEditor.Contains("Pattern Recipe"), Is.True);
            Assert.That(gameSetupEditor.Contains("Select Pattern"), Is.True);
            Assert.That(gameSetupEditor.Contains("Choose From Intent Guidance"), Is.True);
            Assert.That(gameSetupEditor.Contains("FindFirstPattern"), Is.False);
            Assert.That(authoringWindow.Contains("GameplaySessionBootstrap>() == null"), Is.True);
            Assert.That(authoringWindow.Contains("A pattern slot is assigned, but Pyralis cannot trust it as the route source of truth until its metadata is real"), Is.True);
            Assert.That(authoringWindow.Contains("The setup root can still see pawn-required routes"), Is.True);
            Assert.That(authoringWindow.Contains("GetPawnIssuePrimaryAction"), Is.True);
            Assert.That(authoringWindow.Contains("assign that prefab to PawnDefinition > Pawn Prefab"), Is.True);
            Assert.That(authoringWindow.Contains("Motor2DInputAdapter for a 2D pawn"), Is.True);
            Assert.That(authoringWindow.Contains("drag the sprite or Aseprite asset from the Project window onto SpriteRenderer > Sprite"), Is.True);
            Assert.That(authoringWindow.Contains("object picker circle"), Is.True);
            Assert.That(authoringWindow.Contains("double-click the asset"), Is.True);
            Assert.That(routeReportSource.Contains("Incomplete capability pattern"), Is.True);
            Assert.That(setupRouteAnalysisSource.Contains("component that implements IPawnInputModule"), Is.True);
            Assert.That(setupRouteAnalysisSource.Contains("InputProfile actions can reach movement"), Is.True);
            Assert.That(routeReportSource.Contains("proof enhancer for this route"), Is.True);
            Assert.That(routeReportSource.Contains("should not stop a narrow Play Mode attempt"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("Treat these as feature cards after the first route works"), Is.True);
            Assert.That(authoringWindow.Contains("Why This Matters"), Is.True);
            Assert.That(authoringWindow.Contains("DrawMapMode"), Is.True);
            Assert.That(authoringWindow.Contains("Setup Map"), Is.True);
            Assert.That(authoringWindow.Contains("DrawValidateMode"), Is.True);
            Assert.That(authoringWindow.Contains("DrawFactExplorerMode"), Is.True);
            Assert.That(authoringWindow.Contains("Fact Explorer"), Is.True);
            Assert.That(authoringWindow.Contains("DrawFactCoverageSummary"), Is.True);
            Assert.That(authoringWindow.Contains("DrawFactGroup"), Is.True);
            Assert.That(authoringWindow.Contains("DrawFactCard"), Is.True);
            Assert.That(authoringWindow.Contains("Related Stable Ids"), Is.True);
            Assert.That(authoringWindow.Contains("No facts yet. This is a coverage gap"), Is.True);
            Assert.That(authoringWindow.Contains("PyralisAuthoringValidationModel.Build"), Is.True);
            Assert.That(authoringWindow.Contains("DrawValidationIssueGroup"), Is.True);
            Assert.That(authoringWindow.Contains("DrawValidationIssueCard"), Is.True);
            Assert.That(authoringWindow.Contains("DrawValidationIssueTypedMetadata"), Is.True);
            Assert.That(authoringWindow.Contains("Typed Issue"), Is.True);
            Assert.That(authoringWindow.Contains("Severity"), Is.True);
            Assert.That(authoringWindow.Contains("Work Intent"), Is.True);
            Assert.That(authoringWindow.Contains("Field / Component"), Is.True);
            Assert.That(authoringWindow.Contains("typedIssue.NativeAction.Value.ToGuidanceSentence()"), Is.True);
            Assert.That(authoringWindow.Contains("DrawValidationIssueEvidence"), Is.True);
            Assert.That(authoringWindow.Contains("Audit Evidence"), Is.True);
            Assert.That(authoringWindow.Contains("Success Looks Like"), Is.True);
            Assert.That(authoringWindow.Contains("Native Unity Action"), Is.True);
            Assert.That(authoringWindow.Contains("SelectAndPing"), Is.True);
            Assert.That(authoringWindow.Contains("EditorGUIUtility.PingObject"), Is.True);
            Assert.That(authoringWindow.Contains("Issue Code"), Is.True);
            Assert.That(authoringWindow.Contains("Affected Field"), Is.True);
            Assert.That(authoringWindow.Contains("TryRunSafeRepairAction"), Is.False);
            Assert.That(authoringWindow.Contains("TryRunGuidanceAction"), Is.True);
            Assert.That(authoringWindow.Contains("OpenGuideForTarget"), Is.True);
            Assert.That(authoringWindow.Contains("OpenMapForTarget"), Is.True);
            Assert.That(authoringWindow.Contains("AuthoringWindowMode.Map"), Is.True);
            Assert.That(authoringWindow.Contains("AuthoringWindowMode.Guide"), Is.True);
            Assert.That(authoringWindow.Contains("CreateAndAssignGameMode"), Is.False);
            Assert.That(authoringWindow.Contains("CreateAndAssignSetupProfile"), Is.False);
            Assert.That(authoringWindow.Contains("CreateAndAssignParticipant"), Is.False);
            Assert.That(authoringWindow.Contains("Why It Matters"), Is.True);
            Assert.That(authoringWindow.Contains("Inspect Next"), Is.True);
            Assert.That(routeReportSource.Contains("IssueCode"), Is.True);
            Assert.That(routeReportSource.Contains("TypedIssue"), Is.True);
            Assert.That(routeReportSource.Contains("TypedIssues"), Is.True);
            Assert.That(routeReportSource.Contains("PyralisAuthoringIssueAdapter"), Is.True);
            Assert.That(routeReportSource.Contains("PyralisAuthoringIssueSeverity.Required"), Is.True);
            Assert.That(routeReportSource.Contains("PyralisAuthoringIssueSeverity.Recommended"), Is.True);
            Assert.That(routeReportSource.Contains("PyralisAuthoringEvidenceState.CandidateDetected"), Is.True);
            Assert.That(routeReportSource.Contains("PyralisAuthoringNativeAction"), Is.True);
            Assert.That(routeReportSource.Contains("RepairActionLabel"), Is.False);
            Assert.That(routeReportSource.Contains("HasSafeRepairAction"), Is.False);
            Assert.That(routeReportSource.Contains("GetRepairActionLabel"), Is.False);
            Assert.That(routeReportSource.Contains("GuidanceActionLabel"), Is.True);
            Assert.That(routeReportSource.Contains("HasGuidanceAction"), Is.True);
            Assert.That(routeReportSource.Contains("GetGuidanceActionLabel"), Is.True);
            Assert.That(routeReportSource.Contains("BuildBootstrapIssues"), Is.True);
            Assert.That(routeReportSource.Contains("BuildSceneSurfaceIssues"), Is.True);
            Assert.That(routeReportSource.Contains("CreateSceneSurfaceIssue"), Is.True);
            Assert.That(routeReportSource.Contains("Expected"), Is.True);
            Assert.That(routeReportSource.Contains("Found"), Is.True);
            Assert.That(routeReportSource.Contains("SuccessLooksLike"), Is.True);
            Assert.That(routeReportSource.Contains("HasAuditEvidence"), Is.True);
            Assert.That(routeReportSource.Contains("GetSceneSurfaceExpected"), Is.True);
            Assert.That(routeReportSource.Contains("GetSceneSurfaceSuccess"), Is.True);
            Assert.That(routeReportSource.Contains("sceneSurface."), Is.True);
            Assert.That(routeReportSource.Contains("Open Map"), Is.True);
            Assert.That(routeReportSource.Contains("Open Setup Recipe Picker"), Is.True);
            Assert.That(routeReportSource.Contains("Open Session Guide"), Is.True);
            Assert.That(routeReportSource.Contains("Open Game Rules Guide"), Is.True);
            Assert.That(routeReportSource.Contains("Open Participant Guide"), Is.True);
            Assert.That(routeReportSource.Contains("Open Pawn Guide"), Is.True);
            Assert.That(routeReportSource.Contains("BuildStructuredIssues"), Is.True);
            Assert.That(routeReportSource.Contains("session.defaultGameMode.missing"), Is.True);
            Assert.That(routeReportSource.Contains("session.defaultParticipants.slot.empty"), Is.True);
            Assert.That(routeReportSource.Contains("gameMode.setupProfile.missing"), Is.True);
            Assert.That(routeReportSource.Contains("setupProfile.runtimePatterns.missing"), Is.True);
            Assert.That(routeReportSource.Contains("setupProfile.runtimePatterns.slot.empty"), Is.True);
            Assert.That(routeReportSource.Contains("setupProfile.runtimePatterns.duplicate"), Is.True);
            Assert.That(routeReportSource.Contains("pawn.pawnPrefab.missing"), Is.True);
            Assert.That(routeReportSource.Contains("AffectedMember"), Is.True);
            Assert.That(routeReportSource.Contains("GetAffectedMember"), Is.True);
            Assert.That(routeReportSource.Contains("PrimaryActionLabel"), Is.True);
            Assert.That(routeReportSource.Contains("CanInspectTarget"), Is.True);
            Assert.That(routeReportSource.Contains("Session Setup"), Is.True);
            Assert.That(routeReportSource.Contains("Game Rules"), Is.True);
            Assert.That(routeReportSource.Contains("Setup Recipe"), Is.True);
            Assert.That(routeReportSource.Contains("Players / Seats"), Is.True);
            Assert.That(routeReportSource.Contains("Pawns & Actors"), Is.True);
            Assert.That(routeReportSource.Contains("Scene Objects"), Is.True);
            Assert.That(overviewSource.Contains("FirstProofSetupSurface"), Is.True);
            Assert.That(overviewSource.Contains("FirstProofSuccessCriteria"), Is.True);
            Assert.That(overviewSource.Contains("FirstProofDeferUntilAfter"), Is.True);
            Assert.That(overviewSource.Contains("FirstProofChainSummary"), Is.True);
            Assert.That(overviewSource.Contains("GetFirstProofSetupSurface"), Is.True);
            Assert.That(overviewSource.Contains("GetFirstProofSuccessCriteria"), Is.True);
            Assert.That(overviewSource.Contains("GetFirstProofDeferUntilAfter"), Is.True);
            Assert.That(overviewSource.Contains("GetFirstProofChainSummary"), Is.True);
            Assert.That(overviewSource.Contains("NativeActionGuidance"), Is.True);
            Assert.That(overviewSource.Contains("right-click -> Create Empty"), Is.True);
            Assert.That(overviewSource.Contains("Inspector -> Add Component"), Is.True);
            Assert.That(setupFlowMonitorSource.Contains("Assign Camera Bounds Service"), Is.True);
            Assert.That(setupFlowMonitorSource.Contains("CinemachineCameraRigController"), Is.True);
            Assert.That(setupFlowMonitorSource.Contains("Camera Bounds Source"), Is.True);
            Assert.That(routeProofSource.Contains("PyralisAuthoringProofStep"), Is.True);
            Assert.That(routeProofSource.Contains("ProofChainSummary"), Is.True);
            Assert.That(routeProofSource.Contains("BuildProofChain"), Is.True);
            Assert.That(routeProofSource.Contains("TabletopBoardGridPresenter"), Is.True);
            Assert.That(routeProofSource.Contains("1P Pawn Movement Proof"), Is.True);
            Assert.That(routeProofSource.Contains("Network Ownership Proof"), Is.True);
            Assert.That(authoringWindow.Contains("DrawCreateMode"), Is.False);
            Assert.That(authoringWindow.Contains("Route Presets"), Is.False);
            Assert.That(authoringWindow.Contains("2D / 3D Pawn Action"), Is.False);
            Assert.That(authoringWindow.Contains("Tabletop Board"), Is.False);
            Assert.That(authoringWindow.Contains("DrawRoutePreset"), Is.False);
            Assert.That(authoringWindow.Contains("DrawSetupChain"), Is.True);
            Assert.That(authoringWindow.Contains("DrawServiceStep"), Is.True);
            Assert.That(authoringWindow.Contains("DrawExpandableServiceStep"), Is.True);
            Assert.That(authoringWindow.Contains("GetReadinessBadge"), Is.True);
            Assert.That(authoringWindow.Contains("[Ready]"), Is.True);
            Assert.That(authoringWindow.Contains("[Needs Setup]"), Is.True);
            Assert.That(authoringWindow.Contains("[Optional]"), Is.True);
            Assert.That(authoringWindow.Contains("[Blocked]"), Is.True);
            Assert.That(authoringWindow.Contains("Setup Recipe"), Is.True);
            Assert.That(authoringWindow.Contains("Capability Patterns"), Is.True);
            Assert.That(authoringWindow.Contains("Player / Seat"), Is.True);
            Assert.That(authoringWindow.Contains("Details"), Is.True);
            Assert.That(authoringWindow.Contains("Inspect Asset"), Is.True);
            Assert.That(authoringWindow.Contains("EditorGUILayout.ObjectField"), Is.False);
            Assert.That(authoringWindow.Contains("DrawNativeWorkflowStep"), Is.True);
            Assert.That(authoringWindow.Contains("Native Next Step"), Is.True);
            Assert.That(authoringWindow.Contains("Project window:"), Is.True);
            Assert.That(authoringWindow.Contains("IsSceneSupportObject"), Is.True);
            Assert.That(authoringWindow.Contains("keep `{selectedGameObject.name}` as scene support"), Is.True);
            Assert.That(authoringWindow.Contains("not by deleting Main Camera or turning it into the session root"), Is.True);
            Assert.That(authoringWindow.Contains("Create And Assign Game Mode"), Is.False);
            Assert.That(authoringWindow.Contains("Create And Assign Setup Profile"), Is.False);
            Assert.That(authoringWindow.Contains("Create And Assign Participant"), Is.False);
            Assert.That(authoringWindow.Contains("Create And Assign Pawn Definition"), Is.False);
            Assert.That(runtimePatternAuthoringText.Contains("GetSuggestedSetupNotes"), Is.True);
            Assert.That(factorySource.Contains("Assets/Create/NeonBlack/Pawn Starter Pack"), Is.False);
            Assert.That(factorySource.Contains("Assets/Create/NeonBlack/Tabletop Starter Pack"), Is.False);
            Assert.That(factorySource.Contains("MenuItem(\"Assets/Create/NeonBlack"), Is.False);
            Assert.That(factorySource.Contains("DefaultStarterPackFolder = \"Assets/NeonBlack/StarterPacks\""), Is.True);
            Assert.That(factorySource.Contains("PackageSamplesFolderSegment = \"/Samples~\""), Is.True);
            Assert.That(factorySource.Contains("NormalizeStarterPackFolder(GetSelectedFolder())"), Is.True);
            Assert.That(factorySource.Contains("BoardDefinition"), Is.True);
            Assert.That(factorySource.Contains("BoardMovePolicyDefinition"), Is.True);
            Assert.That(factorySource.Contains("BoardTerminalConditionDefinition"), Is.True);
            Assert.That(factorySource.Contains("TurnOrderDefinition"), Is.True);
            Assert.That(factorySource.Contains("ActionDefinition"), Is.True);
            Assert.That(factorySource.Contains("BoardMoveActionResolver"), Is.True);
            Assert.That(factorySource.Contains("TabletopStarterPack"), Is.True);
            Assert.That(factorySource.Contains("GameplayExamplePack"), Is.False);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringInspectorLayer()
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

            string guidePath = Path.Combine(editorRoot, "PyralisInspectorGuide.cs");

            Assert.That(File.Exists(guidePath), Is.True);

            string guideSource = File.ReadAllText(guidePath);
            Assert.That(guideSource.Contains("public readonly struct PyralisGuideContent"), Is.True);
            Assert.That(guideSource.Contains("public readonly struct PyralisGuideSection"), Is.True);
            Assert.That(guideSource.Contains("public enum PyralisGuideIssueSeverity"), Is.True);
            Assert.That(guideSource.Contains("DrawGuide"), Is.True);
            Assert.That(guideSource.Contains("DrawGuidedManual"), Is.False);
            Assert.That(guideSource.Contains("DrawFieldGuide"), Is.True);
            Assert.That(guideSource.Contains("Use this Inspector for field assignment, local customization, and field-local validation"), Is.True);
            Assert.That(guideSource.Contains("route setup, native workflow steps, first playable proof"), Is.True);
            Assert.That(guideSource.Contains("PyralisInspectorHandoff.DrawAuthoringButton"), Is.True);
            Assert.That(guideSource.Contains("PyralisAuthoringSurfaceBeacon.DrawBeaconRow"), Is.True);
            Assert.That(guideSource.Contains("PyralisAuthoringActionSurface.ProjectWindow"), Is.True);
            Assert.That(guideSource.Contains("PyralisAuthoringActionSurface.Hierarchy"), Is.True);
            Assert.That(guideSource.Contains("DrawGuidedManualSection"), Is.False);
            Assert.That(guideSource.Contains("NormalizeGuideTitle"), Is.False);
            Assert.That(guideSource.Contains("Copy Inspector Setup Steps"), Is.False);
            Assert.That(guideSource.Contains("Create Pawn Starter Pack"), Is.False);
            Assert.That(guideSource.Contains("Create Tabletop Starter Pack"), Is.False);
            Assert.That(guideSource.Contains("Open Manual"), Is.False);
            Assert.That(guideSource.Contains("Open First Manual Link"), Is.False);
            Assert.That(guideSource.Contains("DrawValidationIssues"), Is.True);
            Assert.That(guideSource.Contains("DrawValidationMessages"), Is.True);

            string[] guidedInspectorFiles =
            {
                "RuntimePatternDefinitionEditor.cs",
                "GameSetupProfileEditor.cs",
                "SessionDefinitionEditor.cs",
                "GameModeDefinitionEditor.cs",
                "ParticipantDefinitionEditor.cs",
                "PawnDefinitionEditor.cs",
                "FeatureModuleDefinitionEditor.cs"
            };

            for (int i = 0; i < guidedInspectorFiles.Length; i++)
            {
                string inspectorPath = Path.Combine(editorRoot, guidedInspectorFiles[i]);
                Assert.That(File.Exists(inspectorPath), Is.True, guidedInspectorFiles[i]);

                string inspectorSource = File.ReadAllText(inspectorPath);
                Assert.That(UsesSharedGuide(inspectorSource), Is.True, guidedInspectorFiles[i]);
            }
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForCoreProfiles()
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

            string[] guidedProfileInspectors =
            {
                "InputProfileEditor.cs",
                "SettingsProfileEditor.cs",
                "CameraRigProfileEditor.cs",
                "PlayfieldProfileEditor.cs",
                "PawnMovementProfileEditor.cs",
                "PawnTraversalProfileEditor.cs",
                "PawnPresentationProfileEditor.cs",
                "PawnAnimationProfileEditor.cs",
                "PawnCombatProfileEditor.cs"
            };

            for (int i = 0; i < guidedProfileInspectors.Length; i++)
            {
                string inspectorPath = Path.Combine(editorRoot, guidedProfileInspectors[i]);
                Assert.That(File.Exists(inspectorPath), Is.True, guidedProfileInspectors[i]);

                string inspectorSource = File.ReadAllText(inspectorPath);
                Assert.That(UsesSharedGuide(inspectorSource), Is.True, guidedProfileInspectors[i]);
            }
        }

        [Test]
        public void PyralisEditor_Source_CoreRouteHelpersAvoidPresetFraming()
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

            string[] routeHelperInspectors =
            {
                "PawnMovementProfileEditor.cs",
                "CameraRigProfileEditor.cs"
            };

            for (int i = 0; i < routeHelperInspectors.Length; i++)
            {
                string inspectorPath = Path.Combine(editorRoot, routeHelperInspectors[i]);
                Assert.That(File.Exists(inspectorPath), Is.True, routeHelperInspectors[i]);

                string inspectorSource = File.ReadAllText(inspectorPath);
                Assert.That(inspectorSource, Does.Not.Contain("Route Presets"), routeHelperInspectors[i]);
                Assert.That(inspectorSource, Does.Not.Contain("Use these presets"), routeHelperInspectors[i]);
                Assert.That(inspectorSource, Does.Not.Contain("This preset"), routeHelperInspectors[i]);
                bool usesStartingPointLanguage = inspectorSource.Contains("Starting Points") || inspectorSource.Contains("starting point");
                Assert.That(usesStartingPointLanguage, Is.True, routeHelperInspectors[i]);
            }
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForFeatureAndCombatAssets()
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

            string[] guidedFeatureAndCombatInspectors =
            {
                "ActionDefinitionEditor.cs",
                "ActorAnimationDefinitionEditor.cs",
                "ActorFeedbackProfileEditor.cs",
                "EnemyAmbientFeatureProfileEditor.cs",
                "EnemyFeatureProfileEditor.cs",
                "EnemyReactionProfileEditor.cs",
                "InteractionFeatureProfileEditor.cs",
                "PickupFeatureProfileEditor.cs",
                "HazardFeedbackProfileEditor.cs",
                "CombatActionDefinitionEditor.cs",
                "CombatSequenceDefinitionEditor.cs",
                "EnemyAttackEditor.cs",
                "FireModeDefinitionEditor.cs",
                "ProjectileDefinitionEditor.cs",
                "ProjectileImpactDefinitionEditor.cs",
                "StatusEffectDefinitionEditor.cs",
                "WeaponDataEditor.cs",
                "ActorCombatReactionProfileEditor.cs",
                "ActorStatusEffectProfileEditor.cs",
                "EnemyCombatProfileEditor.cs"
            };

            for (int i = 0; i < guidedFeatureAndCombatInspectors.Length; i++)
            {
                string inspectorPath = Path.Combine(editorRoot, guidedFeatureAndCombatInspectors[i]);
                Assert.That(File.Exists(inspectorPath), Is.True, guidedFeatureAndCombatInspectors[i]);

                string inspectorSource = File.ReadAllText(inspectorPath);
                Assert.That(UsesSharedGuide(inspectorSource), Is.True, guidedFeatureAndCombatInspectors[i]);
            }
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForSceneFlowHazardInputAndVisualAssets()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorRoot = Path.Combine(gameplayRoot, "Editor");

            string[] guidedMainEditorInspectors =
            {
                "GameConfigEditor.cs",
                "InputConfigEditor.cs",
                "LevelDataEditor.cs",
                "LevelRegistryEditor.cs",
                "FlashPresetSOEditor.cs",
                "HazardImpactProfileEditor.cs",
                "HazardPresetLibraryEditor.cs"
            };

            for (int i = 0; i < guidedMainEditorInspectors.Length; i++)
            {
                string inspectorPath = Path.Combine(editorRoot, guidedMainEditorInspectors[i]);
                Assert.That(File.Exists(inspectorPath), Is.True, guidedMainEditorInspectors[i]);

                string inspectorSource = File.ReadAllText(inspectorPath);
                Assert.That(UsesSharedGuide(inspectorSource), Is.True, guidedMainEditorInspectors[i]);
            }

            string[] guidedFeatureEditorInspectors =
            {
                Path.Combine(gameplayRoot, "Features", "Hazards", "Editor", "HazardDataEditor.cs"),
                Path.Combine(gameplayRoot, "Features", "Input", "2D", "Editor", "InputZoneSetEditor.cs")
            };

            for (int i = 0; i < guidedFeatureEditorInspectors.Length; i++)
            {
                Assert.That(File.Exists(guidedFeatureEditorInspectors[i]), Is.True, guidedFeatureEditorInspectors[i]);

                string inspectorSource = File.ReadAllText(guidedFeatureEditorInspectors[i]);
                Assert.That(UsesSharedGuide(inspectorSource), Is.True, guidedFeatureEditorInspectors[i]);
            }
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForGameplaySessionBootstrap()
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

            string inspectorPath = Path.Combine(editorRoot, "GameplaySessionBootstrapEditor.cs");
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");
            string inputRouterPath = Path.Combine(gameplayRoot, "Features", "Input", "ParticipantInputRouter.cs");
            string bootstrapPath = Path.Combine(gameplayRoot, "Features", "Characters", "GameplaySessionBootstrap.cs");

            Assert.That(File.Exists(inspectorPath), Is.True);
            Assert.That(File.Exists(inputRouterPath), Is.True);
            Assert.That(File.Exists(bootstrapPath), Is.True);

            string inspectorSource = File.ReadAllText(inspectorPath);
            string inputRouterSource = File.ReadAllText(inputRouterPath);
            string bootstrapSource = File.ReadAllText(bootstrapPath);

            Assert.That(inspectorSource.Contains("CustomEditor(typeof(GameplaySessionBootstrap))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(PawnRoot))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(SessionStateService))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(ParticipantRosterService))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(ParticipantSpawnService))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(ParticipantInputRouter))"), Is.True);
            Assert.That(inspectorSource.Contains("PyralisInspectorHandoff.DrawAuthoringButton"), Is.True);
            Assert.That(inspectorSource.Contains("DrawValidationMessages"), Is.True);
            Assert.That(inspectorSource.Contains("PyralisGuideIssue.Optional"), Is.True);
            Assert.That(inspectorSource.Contains("SessionDefinition"), Is.True);
            Assert.That(inspectorSource.Contains("Build The Scene In This Order"), Is.False);
            Assert.That(inspectorSource.Contains("BuildSceneCreationItems"), Is.False);
            Assert.That(inspectorSource.Contains("Use this Inspector for field assignment"), Is.False);
            Assert.That(inputRouterSource.Contains("private void Update()"), Is.False);
            Assert.That(inputRouterSource.Contains("onPlayerJoined += RegisterPlayerInput"), Is.True);
            Assert.That(inputRouterSource.Contains("onPlayerLeft += UnregisterPlayerInput"), Is.True);
            Assert.That(bootstrapSource.Contains("inputRouter.SetRosterService(rosterService)"), Is.True);
            Assert.That(bootstrapSource.Contains("cameraRigController.SetParticipantRoster(rosterService)"), Is.True);
            Assert.That(bootstrapSource.Contains("inputRouter.SetPlayerInputManager(playerInputManager)"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesSafeSetupFlowMonitorForGameplaySessionBootstrap()
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

            string typesPath = Path.Combine(editorRoot, "PyralisSetupFlowTypes.cs");
            string guidancePath = Path.Combine(editorRoot, "PyralisSetupFlowGuidance.cs");
            string validatorPath = Path.Combine(editorRoot, "PyralisSetupFlowValidator.cs");
            string resolverPath = Path.Combine(editorRoot, "PyralisRuntimeSystemClaimResolver.cs");
            string sceneReadinessPath = Path.Combine(editorRoot, "PyralisSceneReadinessValidator.cs");
            string bootstrapEditorPath = Path.Combine(editorRoot, "GameplaySessionBootstrapEditor.cs");
            string authoringWindowPath = Path.Combine(editorRoot, "PyralisAuthoringWindow.cs");
            string routeReportPath = Path.Combine(editorRoot, "PyralisAuthoringRouteReport.cs");

            Assert.That(File.Exists(typesPath), Is.True);
            Assert.That(File.Exists(guidancePath), Is.True);
            Assert.That(File.Exists(validatorPath), Is.True);
            Assert.That(File.Exists(resolverPath), Is.True);
            Assert.That(File.Exists(sceneReadinessPath), Is.True);

            string typesSource = File.ReadAllText(typesPath);
            string guidanceSource = File.ReadAllText(guidancePath);
            string validatorSource = File.ReadAllText(validatorPath);
            string resolverSource = File.ReadAllText(resolverPath);
            string sceneReadinessSource = File.ReadAllText(sceneReadinessPath);
            string bootstrapEditorSource = File.ReadAllText(bootstrapEditorPath);
            string authoringWindowSource = File.ReadAllText(authoringWindowPath);
            string routeReportSource = File.ReadAllText(routeReportPath);

            Assert.That(typesSource.Contains("public enum PyralisSetupFlowStepStatus"), Is.True);
            Assert.That(validatorSource.Contains("PyralisSetupFlowValidator"), Is.True);
            Assert.That(guidanceSource.Contains("Undo.AddComponent<PyralisGameplayLifetimeScope>"), Is.True);
            Assert.That(guidanceSource.Contains("RestoreFirstSceneDefaults"), Is.True);
            Assert.That(validatorSource.Contains("Assign Score Service"), Is.True);
            Assert.That(guidanceSource.Contains("ISessionScoreService"), Is.True);
            Assert.That(validatorSource.Contains("Assign Projectile Launcher"), Is.True);
            Assert.That(guidanceSource.Contains("ProjectileLauncherBase"), Is.True);
            Assert.That(validatorSource.Contains("Tabletop Runtime Contract"), Is.True);
            Assert.That(validatorSource.Contains("Assign Tabletop Selection Surface"), Is.True);
            Assert.That(guidanceSource.Contains("TabletopBoardGridPresenter"), Is.True);
            Assert.That(guidanceSource.Contains("TabletopBoardSelectionBridge"), Is.True);
            Assert.That(validatorSource.Contains("Resolve Runtime System Claims"), Is.True);
            Assert.That(validatorSource.Contains("Required Runtime Systems"), Is.True);
            Assert.That(validatorSource.Contains("PyralisRuntimeSystemClaimResolver.BuildReport"), Is.True);
            Assert.That(validatorSource.Contains("Scene And Prefab Readiness"), Is.True);
            Assert.That(validatorSource.Contains("PyralisSceneReadinessValidator.BuildReport"), Is.True);
            Assert.That(validatorSource.Contains("AssetDatabase.CreateAsset"), Is.False);
            Assert.That(validatorSource.Contains("PrefabUtility"), Is.False);
            Assert.That(resolverSource.Contains("public static class PyralisRuntimeSystemClaimResolver"), Is.True);
            Assert.That(resolverSource.Contains("ParticipantScoreService"), Is.True);
            Assert.That(resolverSource.Contains("ProjectileLauncher"), Is.True);
            Assert.That(resolverSource.Contains("PawnRoot"), Is.True);
            Assert.That(sceneReadinessSource.Contains("public static class PyralisSceneReadinessValidator"), Is.True);
            Assert.That(sceneReadinessSource.Contains("GameObjectUtility.GetMonoBehavioursWithMissingScriptCount"), Is.True);
            Assert.That(sceneReadinessSource.Contains("IProjectileRuntimeBody"), Is.True);
            Assert.That(sceneReadinessSource.Contains("NetworkObject"), Is.True);
            Assert.That(sceneReadinessSource.Contains("NetworkManager"), Is.True);
            Assert.That(sceneReadinessSource.Contains("cameraRigController"), Is.True);
            Assert.That(sceneReadinessSource.Contains("playerInputManager"), Is.True);
            Assert.That(sceneReadinessSource.Contains("spawnPoints"), Is.True);
            Assert.That(sceneReadinessSource.Contains("AppendSceneComponentIssues<ProjectileLauncherBase>"), Is.True);
            Assert.That(sceneReadinessSource.Contains("AppendSceneServiceIssues<ISessionScoreService>"), Is.True);
            Assert.That(sceneReadinessSource.Contains("GetValidationIssues"), Is.True);
            Assert.That(sceneReadinessSource.Contains("PrefabUtility"), Is.False);
            Assert.That(bootstrapEditorSource.Contains("PyralisSetupFlowValidator.BuildReport"), Is.False);
            Assert.That(bootstrapEditorSource.Contains("PyralisSetupFlowDrawer.Draw"), Is.False);
            Assert.That(bootstrapEditorSource.Contains("PyralisInspectorHandoff.DrawAuthoringButton"), Is.True);
            Assert.That(routeReportSource.Contains("session.GetValidationIssues()"), Is.True);
            Assert.That(routeReportSource.Contains("PyralisSceneReadinessValidator.BuildReport"), Is.True);
            Assert.That(routeReportSource.Contains("prefabReadiness."), Is.True);
            Assert.That(routeReportSource.Contains("ShouldSurfacePrefabReadinessIssue"), Is.True);
            Assert.That(authoringWindowSource.Contains("Default input profile is not assigned."), Is.False);
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
            string editorPath = Path.Combine(gameplayRoot, "Editor", "TabletopBoardSelectionBridgeEditor.cs");

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
            string editorPath = Path.Combine(gameplayRoot, "Editor", "TabletopBoardGridPresenterEditor.cs");
            string setupDocPath = Path.Combine(gameplayRoot, "Docs", "Setup", "Prefabs", "Board_Card_Tabletop_Setup.md");

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

            Assert.That(packageJson.Contains("\"version\": \"0.1.2\""), Is.True);
            Assert.That(packageJson.Contains("\"samples\""), Is.True);
            Assert.That(packageJson.Contains("\"Samples~/Example\""), Is.True);
            Assert.That(packageJson.Contains("\"com.unity.addressables\": \"2.9.1\""), Is.True);
            Assert.That(packageJson.Contains("\"com.unity.localization\": \"1.5.8\""), Is.True);
            Assert.That(packageJson.Contains("\"com.unity.netcode.gameobjects\": \"2.10.0\""), Is.True);
            Assert.That(packageJson.Contains("\"com.unity.transport\": \"2.6.0\""), Is.True);
            Assert.That(runtimeMarker.Contains("Version   = \"0.1.2\""), Is.True);
            Assert.That(editorMarker.Contains("Version   = \"0.1.2\""), Is.True);
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
        public void PyralisAuthoringWindow_Source_PrioritizesRouteAndStarterPacksOverRawRuntimePatternCreation()
        {
            string authoringWindowPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Editor",
                "PyralisAuthoringWindow.cs");

            string source = File.ReadAllText(authoringWindowPath);
            Assert.That(source.Contains("Guided Setup Route"), Is.True);
            Assert.That(source.Contains("Starter Packs"), Is.False);
            Assert.That(source.Contains("Manual Assets"), Is.False);
            Assert.That(source.Contains("Runtime Pattern Definition"), Is.True);
            Assert.That(source.Contains("PyralisAuthoringRouteReport.Build"), Is.True);
            Assert.That(source.Contains("Project window:"), Is.True);
            Assert.That(source.Contains("Create new runtime patterns only for advanced custom setup categories."), Is.False);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedRouteAnalysisForAuthoringSurfaces()
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

            string analysisPath = Path.Combine(editorRoot, "PyralisSetupRouteAnalysis.cs");
            string monitorPath = Path.Combine(editorRoot, "PyralisSetupFlowMonitor.cs");
            string authoringPath = Path.Combine(editorRoot, "PyralisAuthoringWindow.cs");
            string routeReportPath = Path.Combine(editorRoot, "PyralisAuthoringRouteReport.cs");
            string routeDescriptorPath = Path.Combine(editorRoot, "PyralisAuthoringRouteDescriptor.cs");
            string featureAdvisorPath = Path.Combine(editorRoot, "PyralisAuthoringFeatureAdvisor.cs");
            string sceneSurfacePath = Path.Combine(editorRoot, "PyralisAuthoringSceneSurfaceSnapshot.cs");

            Assert.That(File.Exists(analysisPath), Is.True);
            Assert.That(File.Exists(routeReportPath), Is.True);
            Assert.That(File.Exists(routeDescriptorPath), Is.True);

            string analysisSource = File.ReadAllText(analysisPath);
            string monitorSource = File.ReadAllText(monitorPath);
            string authoringSource = File.ReadAllText(authoringPath);
            string routeReportSource = File.ReadAllText(routeReportPath);
            string routeDescriptorSource = File.ReadAllText(routeDescriptorPath);
            string featureAdvisorSource = File.ReadAllText(featureAdvisorPath);
            string sceneSurfaceSource = File.ReadAllText(sceneSurfacePath);

            Assert.That(analysisSource.Contains("public sealed class PyralisSetupRouteAnalysis"), Is.True);
            Assert.That(routeDescriptorSource.Contains("public sealed class PyralisAuthoringRouteDescriptor"), Is.True);
            Assert.That(monitorSource.Contains("PyralisSetupRouteAnalysis.Build"), Is.True);
            Assert.That(routeReportSource.Contains("PyralisSetupRouteAnalysis.Build"), Is.True);
            Assert.That(featureAdvisorSource.Contains("PyralisAuthoringRouteDescriptor"), Is.True);
            Assert.That(sceneSurfaceSource.Contains("PyralisAuthoringRouteDescriptor"), Is.True);
            Assert.That(authoringSource.Contains("You Are Here"), Is.True);
            Assert.That(authoringSource.Contains("Blocking Setup Clear"), Is.True);
            Assert.That(authoringSource.Contains("GetEvidenceLabel"), Is.True);
            Assert.That(monitorSource.Contains("private static bool RequiresPawn"), Is.False);
            Assert.That(authoringSource.Contains("private static bool RequiresPawn"), Is.False);
            Assert.That(authoringSource.Contains("private static bool HasParticipants"), Is.False);
        }

        [Test]
        public void PyralisEditor_Source_UsesCollapsedGuidanceFor3DPawnStack()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string inspectorGuidePath = Path.Combine(gameplayRoot, "Editor", "PyralisInspectorGuide.cs");
            string pawnStackEditorPath = Path.Combine(gameplayRoot, "Editor", "Pawn3DStackEditors.cs");
            string traversalEditorPath = Path.Combine(gameplayRoot, "Features", "Traversal", "Editor", "Pawn3DTraversalComponentEditor.cs");
            string motorPath = Path.Combine(gameplayRoot, "Features", "Characters", "3D", "Motor3D.cs");
            string inputPath = Path.Combine(gameplayRoot, "Features", "Characters", "3D", "Pawn3DInputModule.cs");
            string movementPath = Path.Combine(gameplayRoot, "Features", "Characters", "Runtime", "Shared", "Components", "3D", "Pawn3DMovementComponent.cs");
            string presentationPath = Path.Combine(gameplayRoot, "Features", "Characters", "3D", "Pawn3DPresentationComponent.cs");
            string traversalPath = Path.Combine(gameplayRoot, "Features", "Traversal", "Runtime", "3D", "Pawn3DTraversalComponent.cs");
            string pawnSetupPath = Path.Combine(gameplayRoot, "Docs", "Setup", "Prefabs", "Pawn_Setup.md");

            Assert.That(File.Exists(inspectorGuidePath), Is.True);
            Assert.That(File.Exists(pawnStackEditorPath), Is.True);
            Assert.That(File.Exists(traversalEditorPath), Is.True);
            Assert.That(File.Exists(pawnSetupPath), Is.True);

            string inspectorGuideSource = File.ReadAllText(inspectorGuidePath);
            string pawnStackEditorSource = File.ReadAllText(pawnStackEditorPath);
            string traversalEditorSource = File.ReadAllText(traversalEditorPath);
            string pawnSetupSource = File.ReadAllText(pawnSetupPath);

            AssertNoMojibake(File.ReadAllText(motorPath), motorPath);
            AssertNoMojibake(File.ReadAllText(inputPath), inputPath);
            AssertNoMojibake(File.ReadAllText(movementPath), movementPath);
            AssertNoMojibake(File.ReadAllText(presentationPath), presentationPath);
            AssertNoMojibake(File.ReadAllText(traversalPath), traversalPath);
            AssertNoMojibake(pawnStackEditorSource, pawnStackEditorPath);
            AssertNoMojibake(traversalEditorSource, traversalEditorPath);

            Assert.That(inspectorGuideSource.Contains("bool defaultOpen = false"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("CustomEditor(typeof(Motor3D))"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("CustomEditor(typeof(Pawn3DInputModule))"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("CustomEditor(typeof(Pawn3DMovementComponent))"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("CustomEditor(typeof(Pawn3DPresentationComponent))"), Is.True);
            Assert.That(traversalEditorSource.Contains("CustomEditor(typeof(Pawn3DTraversalComponent))"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("defaultOpen: false"), Is.True);
            Assert.That(traversalEditorSource.Contains("defaultOpen: false"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("3D Pawn Stack Field Guide"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("PawnRoot"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("CharacterController"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("PawnDefinition"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("ActorAnimationDriver"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("Movement Camera is empty"), Is.True);
            Assert.That(pawnStackEditorSource.Contains("Non-combat pawns can still move"), Is.True);
            Assert.That(File.ReadAllText(movementPath).Contains("Camera.main"), Is.False);
            Assert.That(File.ReadAllText(movementPath).Contains("SetMovementCamera"), Is.True);
            Assert.That(traversalEditorSource.Contains("ClimbZone"), Is.True);
            Assert.That(pawnSetupSource.Contains("Inspector Field Guide"), Is.True);
            Assert.That(pawnSetupSource.Contains("assign `Movement Camera`"), Is.True);
        }

        [Test]
        public void ProjectileDefinitionEditor_Source_ValidatesPrefabRuntimeBodyCompatibility()
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

            string projectileEditorSource = File.ReadAllText(Path.Combine(editorRoot, "ProjectileDefinitionEditor.cs"));
            string fireModeEditorSource = File.ReadAllText(Path.Combine(editorRoot, "FireModeDefinitionEditor.cs"));

            Assert.That(projectileEditorSource.Contains("IProjectileRuntimeBody"), Is.True);
            Assert.That(projectileEditorSource.Contains("Projectile for 3D prefabs or Projectile2D"), Is.True);
            Assert.That(projectileEditorSource.Contains("mixes 2D and 3D physics components"), Is.True);
            Assert.That(fireModeEditorSource.Contains("ProjectileFirePlanner consumes burst"), Is.True);
            Assert.That(fireModeEditorSource.Contains("ProjectileMagazineState consumes clip size"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForCameraRuntimeComponents()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "CameraRuntimeGuidedEditors.cs");
            string cameraRigPath = Path.Combine(gameplayRoot, "Presentation", "Camera", "CinemachineCameraRigController.cs");
            string occlusionPath = Path.Combine(gameplayRoot, "Presentation", "Camera", "3D", "CameraOcclusionFader.cs");
            string shakePath = Path.Combine(gameplayRoot, "Presentation", "Visuals", "CameraShake.cs");
            string cameraZonePath = Path.Combine(gameplayRoot, "Features", "Zones", "3D", "CameraZone.cs");

            Assert.That(File.Exists(editorPath), Is.True);
            Assert.That(File.Exists(cameraRigPath), Is.True);
            Assert.That(File.Exists(occlusionPath), Is.True);
            Assert.That(File.Exists(shakePath), Is.True);
            Assert.That(File.Exists(cameraZonePath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            string cameraRigSource = File.ReadAllText(cameraRigPath);
            string occlusionSource = File.ReadAllText(occlusionPath);
            string shakeSource = File.ReadAllText(shakePath);
            string cameraZoneSource = File.ReadAllText(cameraZonePath);

            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(cameraRigSource, cameraRigPath);
            AssertNoMojibake(occlusionSource, occlusionPath);
            AssertNoMojibake(shakeSource, shakePath);
            AssertNoMojibake(cameraZoneSource, cameraZonePath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(CinemachineCameraRigController))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(CameraOcclusionFader))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(CameraShake))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(CameraZone))"), Is.True);
            Assert.That(editorSource.Contains("PyralisInspectorGuide.DrawFieldGuide"), Is.True);
            Assert.That(editorSource.Contains("Guided Setup Manual"), Is.False);
            Assert.That(editorSource.Contains("Inspector Field Guide: Cinemachine Camera Rig Controller"), Is.True);
            Assert.That(editorSource.Contains("Use this Inspector for assigned references and tuning values"), Is.True);
            Assert.That(editorSource.Contains("Camera/cursor path"), Is.False);
            Assert.That(editorSource.Contains("Realtime character path"), Is.False);
            Assert.That(editorSource.Contains("2D path"), Is.True);
            Assert.That(editorSource.Contains("Combat arena path"), Is.True);
            Assert.That(cameraRigSource.Contains("AddComponentMenu(\"NeonBlack/Gameplay/Camera/Cinemachine Camera Rig Controller\")"), Is.True);
            Assert.That(cameraRigSource.Contains("ICameraBoundsProvider"), Is.True);
            Assert.That(cameraRigSource.Contains("TryGetCameraBounds2D"), Is.True);
            Assert.That(cameraRigSource.Contains("enforceMinimumVisibleArea2D"), Is.True);
            Assert.That(cameraRigSource.Contains("ApplyMinimumVisibleArea2D(force: true);"), Is.True);
            Assert.That(occlusionSource.Contains("AddComponentMenu(\"NeonBlack/Gameplay/Camera/Camera Occlusion Fader 3D\")"), Is.True);
            Assert.That(shakeSource.Contains("AddComponentMenu(\"NeonBlack/Gameplay/Camera/Camera Shake\")"), Is.True);
            Assert.That(cameraZoneSource.Contains("AddComponentMenu(\"NeonBlack/Gameplay/Camera/Camera Zone 3D\")"), Is.True);
            Assert.That(cameraRigSource.Contains("Camera.main"), Is.False);
            Assert.That(shakeSource.Contains("Camera.main"), Is.False);
            Assert.That(occlusionSource.Contains("Physics.RaycastNonAlloc"), Is.True);
            Assert.That(occlusionSource.Contains("Physics.RaycastAll"), Is.False);
            Assert.That(occlusionSource.Contains("GetComponentsInChildren(false, _rendererScratch)"), Is.True);
            Assert.That(occlusionSource.Contains("new List<Renderer>()"), Is.False);
            Assert.That(editorSource.Contains("Runtime will fall back to Camera.main"), Is.False);
            Assert.That(editorSource.Contains("Target Camera is empty. Assign the physical Unity Camera that renders the view"), Is.True);
            Assert.That(editorSource.Contains("Target Camera is missing Cinemachine Brain"), Is.True);
            Assert.That(editorSource.Contains("physical Target Camera that has Cinemachine Brain"), Is.True);
            Assert.That(editorSource.Contains("normal Cinemachine route keeps or creates one physical Unity Camera"), Is.True);
            Assert.That(editorSource.Contains("Shared Camera Behaviour should be the Cinemachine Camera / virtual camera component"), Is.True);
            Assert.That(editorSource.Contains("Multiple enabled physical Camera components are present"), Is.True);
            Assert.That(editorSource.Contains("one render camera (usually Main Camera with Cinemachine Brain) plus one Cinemachine Camera"), Is.True);
            Assert.That(editorSource.Contains("Inspector > Add Component, add Cinemachine Brain"), Is.True);
        }

        [Test]
        public void PawnStarterPackCreatesOrthographicSharedCameraProfileFor2DProof()
        {
            string factoryPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Editor",
                "GameplayStarterPackFactory.cs");

            string factorySource = File.ReadAllText(factoryPath);
            Assert.That(factorySource.Contains("CameraRigProfile cameraRigProfile = CreateAsset<CameraRigProfile>(profilesFolder, \"SharedCameraRigProfile\")"), Is.True);
            Assert.That(factorySource.Contains("cameraRigProfile.orthographic = true"), Is.True);
            Assert.That(factorySource.Contains("cameraRigProfile.orthographicSize = 5f"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringFor2DPawnAndInputStack()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "Pawn2DStackGuidedEditors.cs");
            string motorPath = Path.Combine(gameplayRoot, "Features", "Characters", "2D", "Motor2D.cs");
            string movementPath = Path.Combine(gameplayRoot, "Features", "Characters", "2D", "Pawn2DMovementComponent.cs");
            string presentationPath = Path.Combine(gameplayRoot, "Features", "Characters", "2D", "Pawn2DPresentationComponent.cs");
            string combatPath = Path.Combine(gameplayRoot, "Features", "Characters", "2D", "PawnCombatBehaviour2D.cs");
            string sharedCombatPath = Path.Combine(gameplayRoot, "Features", "Characters", "PawnCombatBehaviour.cs");
            string inputAdapterPath = Path.Combine(gameplayRoot, "Features", "Input", "2D", "Motor2DInputAdapter.cs");
            string guardBridgePath = Path.Combine(gameplayRoot, "Features", "Characters", "2D", "ActorGuardInputBridge2D.cs");
            string interactionBridgePath = Path.Combine(gameplayRoot, "Features", "Characters", "2D", "ActorInteractionInputBridge2D.cs");
            string animationDriverPath = Path.Combine(gameplayRoot, "Presentation", "Animation", "ActorAnimationDriver.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(motorPath), motorPath);
            AssertNoMojibake(File.ReadAllText(movementPath), movementPath);
            AssertNoMojibake(File.ReadAllText(presentationPath), presentationPath);
            AssertNoMojibake(File.ReadAllText(combatPath), combatPath);
            AssertNoMojibake(File.ReadAllText(sharedCombatPath), sharedCombatPath);
            AssertNoMojibake(File.ReadAllText(inputAdapterPath), inputAdapterPath);
            AssertNoMojibake(File.ReadAllText(guardBridgePath), guardBridgePath);
            AssertNoMojibake(File.ReadAllText(interactionBridgePath), interactionBridgePath);
            AssertNoMojibake(File.ReadAllText(animationDriverPath), animationDriverPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(Motor2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(Pawn2DMovementComponent))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(Pawn2DPresentationComponent))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(PawnCombatBehaviour2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(PawnCombatBehaviour))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(Motor2DInputAdapter))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorGuardInputBridge2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorInteractionInputBridge2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorAnimationDriver))"), Is.True);
            Assert.That(editorSource.Contains("PyralisInspectorGuide.DrawFieldGuide"), Is.True);
            Assert.That(editorSource.Contains("2D Pawn Stack Field Guide"), Is.True);
            Assert.That(editorSource.Contains("ActorFeatureHost"), Is.True);
            Assert.That(editorSource.Contains("Motor2DInputAdapter or PlayerInputHandler"), Is.True);
            Assert.That(editorSource.Contains("Pawn2DMovementComponent is missing"), Is.True);
            Assert.That(editorSource.Contains("texture imported as Sprite (2D and UI) or a visible Sprite subasset"), Is.True);
            Assert.That(editorSource.Contains("export/select a static PNG frame or use the generated Aseprite prefab"), Is.True);
            Assert.That(editorSource.Contains("Animation Profile is empty"), Is.True);
            Assert.That(editorSource.Contains("Camera Override is empty for Billboard2_5D presentation"), Is.True);
            Assert.That(File.ReadAllText(animationDriverPath).Contains("Camera.main"), Is.False);
            Assert.That(File.ReadAllText(animationDriverPath).Contains("SetCameraOverride"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForActorFeatureRuntimes()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "ActorFeatureRuntimeGuidedEditors.cs");
            string contractGuideTextPath = Path.Combine(gameplayRoot, "Editor", "PyralisInspectorGuide.cs");
            string editorAsmdefPath = Path.Combine(gameplayRoot, "Editor", "NeonBlack.Gameplay.Editor.asmdef");
            string hostPath = Path.Combine(gameplayRoot, "Features", "Composition", "ActorFeatureHost.cs");
            string combatReactionPath = Path.Combine(gameplayRoot, "Features", "Combat", "ActorCombatReactionFeatureRuntime.cs");
            string statusPath = Path.Combine(gameplayRoot, "Features", "Combat", "ActorStatusEffectFeatureRuntime.cs");
            string interactionPath = Path.Combine(gameplayRoot, "Features", "Interaction", "Runtime", "Shared", "ActorInteractionFeatureRuntime.cs");
            string feedbackRuntimePath = Path.Combine(gameplayRoot, "Features", "Feedback", "Runtime", "Shared", "ActorFeedbackFeatureRuntime.cs");
            string floatingFeedbackPath = Path.Combine(gameplayRoot, "Features", "Feedback", "Runtime", "Shared", "ActorFloatingFeedbackReceiver.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            string contractGuideTextSource = File.ReadAllText(contractGuideTextPath);
            string editorAsmdefSource = File.ReadAllText(editorAsmdefPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(contractGuideTextSource, contractGuideTextPath);
            AssertNoMojibake(editorAsmdefSource, editorAsmdefPath);
            AssertNoMojibake(File.ReadAllText(hostPath), hostPath);
            AssertNoMojibake(File.ReadAllText(combatReactionPath), combatReactionPath);
            AssertNoMojibake(File.ReadAllText(statusPath), statusPath);
            AssertNoMojibake(File.ReadAllText(interactionPath), interactionPath);
            AssertNoMojibake(File.ReadAllText(feedbackRuntimePath), feedbackRuntimePath);
            AssertNoMojibake(File.ReadAllText(floatingFeedbackPath), floatingFeedbackPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorFeatureHost))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorCombatReactionFeatureRuntime))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorStatusEffectFeatureRuntime))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorInteractionFeatureRuntime))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorFeedbackFeatureRuntime))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorFloatingFeedbackReceiver))"), Is.True);
            Assert.That(editorSource.Contains("PyralisInspectorGuide.DrawFieldGuide"), Is.True);
            Assert.That(editorSource.Contains("Actor Feature Setup Guide"), Is.False);
            Assert.That(editorSource.Contains("Required Setup"), Is.False);
            Assert.That(editorSource.Contains("Common Mistakes"), Is.False);
            Assert.That(editorSource.Contains("FeatureModuleDefinition"), Is.True);
            Assert.That(editorSource.Contains("Runtime Prefab"), Is.True);
            Assert.That(editorSource.Contains("PyralisAuthoringContractGuideText.FeatureModuleSetup"), Is.True);
            Assert.That(editorSource.Contains("actor.combat.reaction"), Is.False);
            Assert.That(editorSource.Contains("actor.status"), Is.False);
            Assert.That(editorSource.Contains("actor.interaction"), Is.False);
            Assert.That(editorSource.Contains("actor.feedback"), Is.False);
            Assert.That(contractGuideTextSource.Contains("PyralisAuthoringContractRegistry.FindByModuleId"), Is.True);
            Assert.That(editorSource.Contains("NeonBlack.Gameplay.Features.Feedback.IActorFeedbackReceiver"), Is.True);
            Assert.That(editorSource.Contains("Every feedback category is disabled"), Is.True);
            Assert.That(editorSource.Contains("Damage Number Sink is empty"), Is.True);
            Assert.That(editorSource.Contains("Popup Camera is empty"), Is.True);
            Assert.That(File.ReadAllText(interactionPath).Contains("initializationContext != null"), Is.True);
            Assert.That(File.ReadAllText(interactionPath).Contains("StartCooldown();"), Is.True);
            Assert.That(File.ReadAllText(floatingFeedbackPath).Contains("DamageNumberSpawner.Instance"), Is.False);
            Assert.That(File.ReadAllText(floatingFeedbackPath).Contains("Camera.main"), Is.False);
            Assert.That(editorAsmdefSource.Contains("NeonBlack.Gameplay.Features.Composition"), Is.True);
            Assert.That(editorAsmdefSource.Contains("NeonBlack.Gameplay.Feature.Interaction"), Is.True);
            Assert.That(editorAsmdefSource.Contains("NeonBlack.Gameplay.Feature.Feedback"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForPickupAndScoringRuntimes()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "PickupScoringRuntimeGuidedEditors.cs");
            string collectible2DPath = Path.Combine(gameplayRoot, "Features", "Pickups", "2D", "Collectible2D.cs");
            string collectible3DPath = Path.Combine(gameplayRoot, "Features", "Pickups", "3D", "Collectible3D.cs");
            string feedbackPath = Path.Combine(gameplayRoot, "Features", "Pickups", "2D", "CollectibleFeedback2D.cs");
            string collector2DPath = Path.Combine(gameplayRoot, "Features", "Pickups", "Runtime", "2D", "ActorPickupCollectorFeature2D.cs");
            string collector3DPath = Path.Combine(gameplayRoot, "Features", "Pickups", "Runtime", "3D", "ActorPickupCollectorFeature3D.cs");
            string stillnessPath = Path.Combine(gameplayRoot, "Features", "Scoring", "2D", "StillnessBonus2D.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            string stillnessSource = File.ReadAllText(stillnessPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(collectible2DPath), collectible2DPath);
            AssertNoMojibake(File.ReadAllText(collectible3DPath), collectible3DPath);
            AssertNoMojibake(File.ReadAllText(feedbackPath), feedbackPath);
            AssertNoMojibake(File.ReadAllText(collector2DPath), collector2DPath);
            AssertNoMojibake(File.ReadAllText(collector3DPath), collector3DPath);
            AssertNoMojibake(stillnessSource, stillnessPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(Collectible2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(Collectible3D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(CollectibleFeedback2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorPickupCollectorFeature2D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorPickupCollectorFeature3D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(StillnessBonus2D))"), Is.True);
            Assert.That(editorSource.Contains("PyralisInspectorGuide.DrawFieldGuide"), Is.True);
            Assert.That(editorSource.Contains("IPickupAwardSink"), Is.True);
            Assert.That(editorSource.Contains("PyralisAuthoringContractGuideText.FeatureModuleSetup"), Is.True);
            Assert.That(editorSource.Contains("actor.pickups.2d"), Is.False);
            Assert.That(editorSource.Contains("actor.pickups.3d"), Is.False);
            Assert.That(editorSource.Contains("CircleCollider2D should be set to Is Trigger"), Is.True);
            Assert.That(editorSource.Contains("ParticipantScoreService"), Is.True);
            Assert.That(File.ReadAllText(collector2DPath).Contains("initializationContext != null"), Is.True);
            Assert.That(File.ReadAllText(collector3DPath).Contains("initializationContext != null"), Is.True);
            Assert.That(stillnessSource.Contains("Normalized 0-1 progress"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForFeedbackHudRuntimes()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "FeedbackHudGuidedEditors.cs");
            string relayPath = Path.Combine(gameplayRoot, "Features", "Feedback", "ParticipantFeedbackRelay.cs");
            string healthPanelPath = Path.Combine(gameplayRoot, "Features", "Feedback", "UI", "ParticipantHealthPanel.cs");
            string timedTextPanelPath = Path.Combine(gameplayRoot, "Features", "Feedback", "UI", "ParticipantTimedTextPanel.cs");
            string feedbackPresenterPath = Path.Combine(gameplayRoot, "Features", "Feedback", "UI", "ParticipantFeedbackHudPresenter.cs");
            string healthBinderPath = Path.Combine(gameplayRoot, "Features", "Feedback", "UI", "ParticipantHealthHudBinder.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(relayPath), relayPath);
            AssertNoMojibake(File.ReadAllText(healthPanelPath), healthPanelPath);
            AssertNoMojibake(File.ReadAllText(timedTextPanelPath), timedTextPanelPath);
            AssertNoMojibake(File.ReadAllText(feedbackPresenterPath), feedbackPresenterPath);
            AssertNoMojibake(File.ReadAllText(healthBinderPath), healthBinderPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(ParticipantFeedbackRelay))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ParticipantHealthPanel))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ParticipantTimedTextPanel))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ParticipantFeedbackHudPresenter))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ParticipantHealthHudBinder))"), Is.True);
            Assert.That(editorSource.Contains("PyralisInspectorGuide.DrawFieldGuide"), Is.True);
            Assert.That(editorSource.Contains("ParticipantFeedbackService"), Is.True);
            Assert.That(editorSource.Contains("IActorHealthState"), Is.True);
            Assert.That(editorSource.Contains("Participant Seat must be zero or greater"), Is.True);
            Assert.That(editorSource.Contains("Assign at least one feedback label or ParticipantTimedTextPanel array"), Is.True);
            Assert.That(editorSource.Contains("Assign a health label, fill image, or ParticipantHealthPanel array"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForPresentationAndLeaderboardRuntimes()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "PresentationScoringGuidedEditors.cs");
            string actorShadowPath = Path.Combine(gameplayRoot, "Presentation", "Visuals", "ActorShadowDriver.cs");
            string billboardPath = Path.Combine(gameplayRoot, "Presentation", "Visuals", "3D", "BillboardFacing3D.cs");
            string leaderboardManagerPath = Path.Combine(gameplayRoot, "Features", "Scoring", "LeaderboardManager.cs");
            string leaderboardScreenPath = Path.Combine(gameplayRoot, "Features", "Scoring", "UI", "LeaderboardScreen.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            string leaderboardScreenSource = File.ReadAllText(leaderboardScreenPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(actorShadowPath), actorShadowPath);
            AssertNoMojibake(File.ReadAllText(billboardPath), billboardPath);
            AssertNoMojibake(File.ReadAllText(leaderboardManagerPath), leaderboardManagerPath);
            AssertNoMojibake(leaderboardScreenSource, leaderboardScreenPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(ActorShadowDriver))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(BillboardFacing3D))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(LeaderboardManager))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(LeaderboardScreen))"), Is.True);
            Assert.That(editorSource.Contains("PyralisInspectorGuide.DrawFieldGuide"), Is.True);
            Assert.That(editorSource.Contains("ActorShadowDriver applies pawn presentation shadow settings"), Is.True);
            Assert.That(editorSource.Contains("BillboardFacing3D turns a visual target toward the active camera"), Is.True);
            Assert.That(editorSource.Contains("Camera Override is empty. Assign the camera this billboard should face"), Is.True);
            Assert.That(editorSource.Contains("Leaderboard Id should not be blank"), Is.True);
            Assert.That(editorSource.Contains("Row Prefab should contain at least three TextMeshProUGUI labels"), Is.True);
            Assert.That(File.ReadAllText(billboardPath).Contains("Camera.main"), Is.False);
            Assert.That(File.ReadAllText(billboardPath).Contains("SetCameraOverride"), Is.True);
            Assert.That(leaderboardScreenSource.Contains("Fetching scores..."), Is.True);
            Assert.That(leaderboardScreenSource.Contains("No scores yet - play and set a record!"), Is.True);
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

            string enemyEditorPath = Path.Combine(gameplayRoot, "Features", "Enemies", "3D", "Editor", "EnemyFeatureRuntimeGuidedEditors.cs");
            string hazardEditorPath = Path.Combine(gameplayRoot, "Features", "Hazards", "Editor", "HazardRuntimeGuidedEditors.cs");
            string traversalEditorPath = Path.Combine(gameplayRoot, "Features", "Traversal", "Editor", "PawnTraversalFeatureRuntime3DEditor.cs");
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
            Assert.That(enemyEditorSource.Contains("PyralisAuthoringContractGuideText.FeatureModuleSetup"), Is.True);
            Assert.That(enemyEditorSource.Contains("enemy.ambient"), Is.False);
            Assert.That(enemyEditorSource.Contains("enemy.reaction"), Is.False);
            Assert.That(hazardEditorSource.Contains("CustomEditor(typeof(HazardFeedbackRuntime))"), Is.True);
            Assert.That(hazardEditorSource.Contains("Popup Camera is empty"), Is.True);
            Assert.That(hazardEditorSource.Contains("CustomEditor(typeof(DamageZone2D))"), Is.True);
            Assert.That(hazardEditorSource.Contains("Collider2D should be set to Is Trigger"), Is.True);
            Assert.That(File.ReadAllText(hazardFeedbackRuntimePath).Contains("Camera.main"), Is.False);
            Assert.That(File.ReadAllText(hazardFeedbackRuntimePath).Contains("SetPopupCamera"), Is.True);
            Assert.That(traversalEditorSource.Contains("CustomEditor(typeof(PawnTraversalFeatureRuntime3D))"), Is.True);
            Assert.That(traversalEditorSource.Contains("PyralisAuthoringContractGuideText.FeatureModuleSetup"), Is.True);
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

            string editorPath = Path.Combine(gameplayRoot, "Features", "Combat", "Editor", "CombatDamageGuidedEditors.cs");
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
        public void PyralisEditor_Source_UsesGuidedAuthoringForSceneGameFlowComponents()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "SceneGameFlowGuidedEditors.cs");
            string gameManagerPath = Path.Combine(gameplayRoot, "Features", "GameFlow", "2D", "GameManager.cs");
            string playerSpawnerPath = Path.Combine(gameplayRoot, "Features", "Respawn", "3D", "PlayerSpawner.cs");
            string playerRegistryPath = Path.Combine(gameplayRoot, "Features", "Characters", "PlayerRegistry.cs");
            string difficultyManagerPath = Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "DifficultyManager.cs");
            string timeManagerPath = Path.Combine(gameplayRoot, "Core", "TimeManager.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(gameManagerPath), gameManagerPath);
            AssertNoMojibake(File.ReadAllText(playerSpawnerPath), playerSpawnerPath);
            AssertNoMojibake(File.ReadAllText(playerRegistryPath), playerRegistryPath);
            AssertNoMojibake(File.ReadAllText(difficultyManagerPath), difficultyManagerPath);
            AssertNoMojibake(File.ReadAllText(timeManagerPath), timeManagerPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(GameManager))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(PlayerSpawner))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(PlayerRegistry))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(DifficultyManager))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(TimeManager))"), Is.True);
            Assert.That(editorSource.Contains("PyralisInspectorGuide.DrawFieldGuide"), Is.True);
            Assert.That(editorSource.Contains("Guided Setup Manual"), Is.False);
            Assert.That(editorSource.Contains("Required Fields"), Is.True);
            Assert.That(editorSource.Contains("GameManager needs Score Manager, Hazard Spawner, Pickup Spawner, and Difficulty Manager"), Is.True);
            Assert.That(editorSource.Contains("PlayerSpawner needs Current Player, Player Prefab, or participant infrastructure"), Is.True);
            Assert.That(editorSource.Contains("PlayerRegistry should live on the active player root"), Is.True);
            Assert.That(editorSource.Contains("Wave mode needs at least one Wave Entry"), Is.True);
            Assert.That(editorSource.Contains("Only one TimeManager should be loaded"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesGuidedAuthoringForMenuNavigationComponents()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "MenuNavigationGuidedEditors.cs");
            string settingsMenuPath = Path.Combine(gameplayRoot, "Features", "Settings", "UI", "SettingsMenu.cs");
            string settingsScreenPath = Path.Combine(gameplayRoot, "Features", "Settings", "UI", "SettingsScreen.cs");
            string sceneFaderPath = Path.Combine(gameplayRoot, "Core", "Navigation", "UI", "SceneFader.cs");
            string sceneLoaderPath = Path.Combine(gameplayRoot, "Core", "SceneLoader.cs");
            string sceneGuardPath = Path.Combine(gameplayRoot, "Core", "Navigation", "UI", "SceneGuard.cs");
            string splashPath = Path.Combine(gameplayRoot, "Core", "Navigation", "UI", "SplashScreenController.cs");
            string loadingPath = Path.Combine(gameplayRoot, "Core", "Navigation", "UI", "LoadingScreenController.cs");
            string mainMenuPath = Path.Combine(gameplayRoot, "Core", "Navigation", "UI", "MainMenuManager.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(settingsMenuPath), settingsMenuPath);
            AssertNoMojibake(File.ReadAllText(settingsScreenPath), settingsScreenPath);
            AssertNoMojibake(File.ReadAllText(sceneFaderPath), sceneFaderPath);
            AssertNoMojibake(File.ReadAllText(sceneLoaderPath), sceneLoaderPath);
            AssertNoMojibake(File.ReadAllText(sceneGuardPath), sceneGuardPath);
            AssertNoMojibake(File.ReadAllText(splashPath), splashPath);
            AssertNoMojibake(File.ReadAllText(loadingPath), loadingPath);
            AssertNoMojibake(File.ReadAllText(mainMenuPath), mainMenuPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(SettingsMenu))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(SettingsScreen))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(SceneFader))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(SceneLoader))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(SceneGuard))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(SplashScreenController))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(LoadingScreenController))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(MainMenuManager))"), Is.True);
            Assert.That(editorSource.Contains("SettingsMenu needs at least one settings control assigned"), Is.True);
            Assert.That(editorSource.Contains("SettingsScreen needs Main Menu Page and Settings Page"), Is.True);
            Assert.That(editorSource.Contains("Settings Source must reference a component that implements IGameplaySettingsApplier"), Is.True);
            Assert.That(editorSource.Contains("Gameplay State Source must reference a component that implements IGameplayStateReader"), Is.True);
            Assert.That(File.ReadAllText(settingsMenuPath).Contains("SettingsManager.Instance"), Is.False);
            Assert.That(File.ReadAllText(settingsScreenPath).Contains("SettingsManager.Instance"), Is.False);
            Assert.That(File.ReadAllText(settingsScreenPath).Contains("_musicVolumeSlider"), Is.True);
            Assert.That(editorSource.Contains("SceneFader is ready as an explicit ISceneNavigator"), Is.True);
            Assert.That(editorSource.Contains("SceneLoader is ready as an explicit ISceneNavigator"), Is.True);
            Assert.That(editorSource.Contains("Scene Navigator Source must reference a component that implements ISceneNavigator"), Is.True);
            Assert.That(editorSource.Contains("SplashScreenController needs a Next Scene Name"), Is.True);
            Assert.That(editorSource.Contains("LoadingScreenController should be used only in the loading scene"), Is.True);
            Assert.That(editorSource.Contains("MainMenuManager needs a game scene name and button references"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesGuidedAuthoringForHazardSpawningComponents()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "HazardSpawningGuidedEditors.cs");
            string damageZonePath = Path.Combine(gameplayRoot, "Features", "Zones", "3D", "DamageZone.cs");
            string hazardPath = Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.cs");
            string hazardSpawnerPath = Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "HazardSpawner.cs");
            string spawnerPath = Path.Combine(gameplayRoot, "Features", "Spawning", "3D", "Spawner.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(damageZonePath), damageZonePath);
            AssertNoMojibake(File.ReadAllText(hazardPath), hazardPath);
            AssertNoMojibake(File.ReadAllText(hazardSpawnerPath), hazardSpawnerPath);
            AssertNoMojibake(File.ReadAllText(spawnerPath), spawnerPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(DamageZone))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(Hazard))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(HazardSpawner))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(Spawner))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(SpawnTracker))"), Is.True);
            Assert.That(editorSource.Contains("DamageZone needs a BoxCollider trigger and positive tick timing"), Is.True);
            Assert.That(editorSource.Contains("Hazard needs Hazard Data, Shadow Renderer, and hit colliders"), Is.True);
            Assert.That(editorSource.Contains("Camera Shake Sink is empty"), Is.True);
            Assert.That(editorSource.Contains("Settings Source must reference a component that implements IGameplaySettingsApplier"), Is.True);
            Assert.That(editorSource.Contains("HazardSpawner needs at least one valid Hazard Entry"), Is.True);
            Assert.That(editorSource.Contains("Spawner needs at least one prefab or sprite option"), Is.True);
            Assert.That(editorSource.Contains("SpawnTracker is runtime-added by Spawner"), Is.True);
            Assert.That(File.ReadAllText(hazardPath).Contains("CameraShake.Instance"), Is.False);
            Assert.That(File.ReadAllText(hazardPath).Contains("_settingsSource"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesGuidedAuthoringForVisualWorldHelperComponents()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "VisualWorldHelperGuidedEditors.cs");
            string spriteFlasherPath = Path.Combine(gameplayRoot, "Presentation", "Visuals", "SpriteFlasher.cs");
            string textFlasherPath = Path.Combine(gameplayRoot, "Presentation", "Visuals", "TextFlasher.cs");
            string depthSortingPath = Path.Combine(gameplayRoot, "Features", "Environment", "3D", "DepthSorting.cs");
            string arenaZonePath = Path.Combine(gameplayRoot, "Features", "Encounters", "3D", "ArenaZone.cs");
            string tilemapGroundPath = Path.Combine(gameplayRoot, "Features", "Environment", "3D", "TilemapGround.cs");
            string grabDetectorPath = Path.Combine(gameplayRoot, "Features", "Traversal", "Runtime", "3D", "GrabDetector.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(File.ReadAllText(spriteFlasherPath), spriteFlasherPath);
            AssertNoMojibake(File.ReadAllText(textFlasherPath), textFlasherPath);
            AssertNoMojibake(File.ReadAllText(depthSortingPath), depthSortingPath);
            AssertNoMojibake(File.ReadAllText(arenaZonePath), arenaZonePath);
            AssertNoMojibake(File.ReadAllText(tilemapGroundPath), tilemapGroundPath);
            AssertNoMojibake(File.ReadAllText(grabDetectorPath), grabDetectorPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(SpriteFlasher))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(TextFlasher))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(DepthSorting))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ArenaZone))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(TilemapGround))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(GrabDetector))"), Is.True);
            Assert.That(editorSource.Contains("SpriteFlasher needs renderers or Auto Find Renderers"), Is.True);
            Assert.That(editorSource.Contains("TextFlasher needs text targets or Auto Find Texts"), Is.True);
            Assert.That(editorSource.Contains("DepthSorting should live on a SpriteRenderer child"), Is.True);
            Assert.That(editorSource.Contains("ArenaZone needs a BoxCollider trigger and player tag"), Is.True);
            Assert.That(editorSource.Contains("TilemapGround needs a Source Tilemap"), Is.True);
            Assert.That(editorSource.Contains("GrabDetector needs a trigger Collider and climb traversal actor parent"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_ClassifiesFinalP2AuthoringCandidates()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorPath = Path.Combine(gameplayRoot, "Editor", "FinalP2GuidedEditors.cs");
            string projectilePoolHandlePath = Path.Combine(gameplayRoot, "Features", "Combat", "ProjectilePoolHandle.cs");
            string participantFeedbackServicePath = Path.Combine(gameplayRoot, "Features", "Feedback", "ParticipantFeedbackService.cs");
            string enemySpawnerPath = Path.Combine(gameplayRoot, "Features", "Spawning", "3D", "EnemySpawner.cs");

            Assert.That(File.Exists(editorPath), Is.True);

            string editorSource = File.ReadAllText(editorPath);
            string projectilePoolHandleSource = File.ReadAllText(projectilePoolHandlePath);
            string participantFeedbackServiceSource = File.ReadAllText(participantFeedbackServicePath);
            string enemySpawnerSource = File.ReadAllText(enemySpawnerPath);

            AssertNoMojibake(editorSource, editorPath);
            AssertNoMojibake(projectilePoolHandleSource, projectilePoolHandlePath);
            AssertNoMojibake(participantFeedbackServiceSource, participantFeedbackServicePath);
            AssertNoMojibake(enemySpawnerSource, enemySpawnerPath);

            Assert.That(editorSource.Contains("CustomEditor(typeof(ProjectilePoolHandle))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(ParticipantFeedbackService))"), Is.True);
            Assert.That(editorSource.Contains("CustomEditor(typeof(EnemySpawner))"), Is.True);
            Assert.That(editorSource.Contains("ProjectilePoolHandle is runtime-managed by projectile pooling"), Is.True);
            Assert.That(editorSource.Contains("ParticipantFeedbackService should be registered once"), Is.True);
            Assert.That(editorSource.Contains("EnemySpawner needs at least one enemy prefab with HealthComponent"), Is.True);

            Assert.That(projectilePoolHandleSource.Contains("[AddComponentMenu(\"\")]"), Is.True);
            Assert.That(participantFeedbackServiceSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Feedback/Participant Feedback Service\")]"), Is.True);
            Assert.That(enemySpawnerSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Spawning/Enemy Spawner\")]"), Is.True);
            Assert.That(enemySpawnerSource.Contains("GetValidEnemyPrefabs"), Is.True);
            Assert.That(enemySpawnerSource.Contains("TryPickSpawnOrigin"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_UsesSharedGuidedAuthoringForPrefabRuntimeComponents()
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

            string inspectorPath = Path.Combine(editorRoot, "PrefabRuntimeGuidedEditors.cs");
            string asmdefPath = Path.Combine(editorRoot, "NeonBlack.Gameplay.Editor.asmdef");
            string gameplayRoot = Directory.GetParent(editorRoot).FullName;
            string enemyEditorPath = Path.Combine(gameplayRoot, "Features", "Enemies", "3D", "Editor", "EnemyAIEditor.cs");
            string enemyEditorAsmdefPath = Path.Combine(gameplayRoot, "Features", "Enemies", "3D", "Editor", "NeonBlack.Gameplay.Feature.Enemies.Editor.asmdef");
            string hitBoxRuntimePath = Path.Combine(gameplayRoot, "Features", "Combat", "HitBox.cs");
            string hitBoxEditorPath = Path.Combine(gameplayRoot, "Features", "Combat", "Editor", "HitBoxEditor.cs");
            string hitBoxEditorAsmdefPath = Path.Combine(gameplayRoot, "Features", "Combat", "Editor", "NeonBlack.Gameplay.Feature.Combat.Editor.asmdef");
            string worldHealthBarEditorPath = Path.Combine(gameplayRoot, "Features", "Combat", "UI", "Editor", "WorldHealthBarEditor.cs");
            string worldHealthBarEditorAsmdefPath = Path.Combine(gameplayRoot, "Features", "Combat", "UI", "Editor", "NeonBlack.Gameplay.Feature.Combat.UI.Editor.asmdef");
            string climbZoneEditorPath = Path.Combine(gameplayRoot, "Features", "Traversal", "Editor", "ClimbZoneEditor.cs");
            string climbZoneEditorAsmdefPath = Path.Combine(gameplayRoot, "Features", "Traversal", "Editor", "NeonBlack.Gameplay.Feature.Traversal.Editor.asmdef");

            Assert.That(File.Exists(inspectorPath), Is.True);
            Assert.That(File.Exists(asmdefPath), Is.True);
            Assert.That(File.Exists(enemyEditorPath), Is.True);
            Assert.That(File.Exists(hitBoxRuntimePath), Is.True);
            Assert.That(File.Exists(hitBoxEditorPath), Is.True);
            Assert.That(File.Exists(worldHealthBarEditorPath), Is.True);
            Assert.That(File.Exists(climbZoneEditorPath), Is.True);

            string inspectorSource = File.ReadAllText(inspectorPath);
            string asmdefSource = File.ReadAllText(asmdefPath);
            string enemyEditorSource = File.ReadAllText(enemyEditorPath);
            string hitBoxRuntimeSource = File.ReadAllText(hitBoxRuntimePath);
            string hitBoxEditorSource = File.ReadAllText(hitBoxEditorPath);
            string worldHealthBarEditorSource = File.ReadAllText(worldHealthBarEditorPath);
            string climbZoneEditorSource = File.ReadAllText(climbZoneEditorPath);

            AssertNoMojibake(enemyEditorSource, enemyEditorPath);
            AssertNoMojibake(hitBoxRuntimeSource, hitBoxRuntimePath);
            AssertNoMojibake(hitBoxEditorSource, hitBoxEditorPath);
            AssertNoMojibake(worldHealthBarEditorSource, worldHealthBarEditorPath);
            AssertNoMojibake(climbZoneEditorSource, climbZoneEditorPath);

            Assert.That(inspectorSource.Contains("CustomEditor(typeof(HealthComponent))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(ProjectileLauncher2D))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(ProjectileLauncher3D))"), Is.True);
            Assert.That(inspectorSource.Contains("Hit Pause Sink and Camera Shake Sink"), Is.True);
            Assert.That(inspectorSource.Contains("Camera Shake Sink must reference a component that implements ICameraShakeSink"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(ParticipantScoreService))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(SettingsManager))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(PlayerInputHandler))"), Is.True);
            Assert.That(inspectorSource.Contains("Settings Registrar Source must reference a component that implements IInputSettingsRegistrar"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(VirtualJoystick))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(CollectibleSpawner2D))"), Is.True);
            Assert.That(inspectorSource.Contains("CustomEditor(typeof(UIManager))"), Is.True);
            Assert.That(inspectorSource.Contains("PyralisInspectorGuide.DrawFieldGuide"), Is.True);
            Assert.That(inspectorSource.Contains("Board/card path"), Is.True);
            Assert.That(inspectorSource.Contains("Projectile path"), Is.True);
            Assert.That(asmdefSource.Contains("\"NeonBlack.Gameplay.Feature.Combat\""), Is.True);
            Assert.That(asmdefSource.Contains("\"NeonBlack.Gameplay.Feature.Pickups\""), Is.True);
            Assert.That(asmdefSource.Contains("\"NeonBlack.Gameplay.Feature.Scoring\""), Is.True);
            Assert.That(enemyEditorSource.Contains("Inspector Field Guide: Enemy AI"), Is.True);
            Assert.That(hitBoxEditorSource.Contains("Inspector Field Guide: HitBox"), Is.True);
            Assert.That(hitBoxEditorSource.Contains("BoxCollider or SphereCollider as a sizing volume"), Is.True);
            Assert.That(hitBoxRuntimeSource.Contains("used as a sizing volume and gizmo only"), Is.True);
            Assert.That(worldHealthBarEditorSource.Contains("Inspector Field Guide: World Health Bar"), Is.True);
            Assert.That(worldHealthBarEditorSource.Contains("Damage Number Sink is empty"), Is.True);
            Assert.That(worldHealthBarEditorSource.Contains("Target Camera is empty"), Is.True);
            Assert.That(climbZoneEditorSource.Contains("Inspector Field Guide: Climb Zone"), Is.True);
            Assert.That(enemyEditorSource.Contains("Board/card path"), Is.True);
            Assert.That(hitBoxEditorSource.Contains("Turn-based/menu path"), Is.True);
            Assert.That(worldHealthBarEditorSource.Contains("Card/tabletop path"), Is.True);
            Assert.That(climbZoneEditorSource.Contains("Camera/board/card path"), Is.True);
            Assert.That(File.ReadAllText(enemyEditorAsmdefPath).Contains("\"NeonBlack.Gameplay.Editor\""), Is.True);
            Assert.That(File.ReadAllText(hitBoxEditorAsmdefPath).Contains("\"NeonBlack.Gameplay.Editor\""), Is.True);
            Assert.That(File.ReadAllText(worldHealthBarEditorAsmdefPath).Contains("\"NeonBlack.Gameplay.Editor\""), Is.True);
            Assert.That(File.ReadAllText(climbZoneEditorAsmdefPath).Contains("\"NeonBlack.Gameplay.Editor\""), Is.True);
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

            string sceneViewToolsPath = Path.Combine(gameplayRoot, "Editor", "SceneViewTools.cs");
            string inputZoneSetPath = Path.Combine(gameplayRoot, "Features", "Input", "2D", "InputZoneSet.cs");
            string inputZoneEditorPath = Path.Combine(gameplayRoot, "Features", "Input", "2D", "Editor", "InputZoneSetEditor.cs");
            string orientationHandlerPath = Path.Combine(gameplayRoot, "Features", "UI", "UIOrientationHandler.cs");
            string orientationEditorPath = Path.Combine(gameplayRoot, "Features", "UI", "Editor", "UIOrientationHandlerEditor.cs");

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
        public void PyralisEditor_Source_CoversConcreteInspectorVisibleRuntimeTypes()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorDirectorySegment = Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
            System.Text.RegularExpressions.Regex runtimeTypePattern = new System.Text.RegularExpressions.Regex(
                @"\b(?<abstract>abstract\s+)?(?:public|internal)?\s*(?:sealed\s+|partial\s+)*class\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*(?<bases>[^{\r\n]+)");
            System.Text.RegularExpressions.Regex customEditorPattern = new System.Text.RegularExpressions.Regex(
                @"CustomEditor\(typeof\((?<name>[A-Za-z_][A-Za-z0-9_]*)\)\)");

            System.Collections.Generic.HashSet<string> customEditorTypes = new System.Collections.Generic.HashSet<string>();
            foreach (string editorFile in Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories).Where(path => path.Contains(editorDirectorySegment)))
            {
                foreach (System.Text.RegularExpressions.Match match in customEditorPattern.Matches(File.ReadAllText(editorFile)))
                    customEditorTypes.Add(match.Groups["name"].Value);
            }

            System.Collections.Generic.HashSet<string> intentionalSkips = new System.Collections.Generic.HashSet<string>
            {
                "ParticipantHudTargetBinding",
                "ProjectileLauncherBase"
            };

            string[] offenders = Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(editorDirectorySegment))
                .SelectMany(path =>
                {
                    string source = File.ReadAllText(path);
                    return runtimeTypePattern.Matches(source)
                        .Cast<System.Text.RegularExpressions.Match>()
                        .Where(match => string.IsNullOrEmpty(match.Groups["abstract"].Value))
                        .Where(match => match.Groups["bases"].Value.Contains("MonoBehaviour") || match.Groups["bases"].Value.Contains("ScriptableObject"))
                        .Select(match => new { Path = path, Name = match.Groups["name"].Value });
                })
                .Where(candidate => !intentionalSkips.Contains(candidate.Name))
                .Where(candidate => !customEditorTypes.Contains(candidate.Name))
                .Select(candidate => $"{candidate.Name} in {candidate.Path}")
                .OrderBy(value => value)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Concrete Gameplay MonoBehaviour/ScriptableObject authoring surfaces should have guided CustomEditor coverage. Offenders: " + string.Join(", ", offenders));
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
            string sceneFlowEditorSource = File.ReadAllText(Path.Combine(gameplayRoot, "Editor", "SceneGameFlowGuidedEditors.cs"));

            Assert.That(bootstrapSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Setup/Gameplay Session Bootstrap\")]"), Is.True);
            Assert.That(gameManagerSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Game Flow/2D Game Manager\")]"), Is.True);
            Assert.That(healthSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Combat/Health Component\")]"), Is.True);
            Assert.That(hazardSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Hazards/2D Hazard\")]"), Is.True);
            Assert.That(enemySource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Enemies/Enemy AI\")]"), Is.True);
            Assert.That(inputSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Input/2D Player Input Handler\")]"), Is.True);
            Assert.That(scoreSource.Contains("[AddComponentMenu(\"NeonBlack/Gameplay/Scoring/Participant Score Service\")]"), Is.True);
            Assert.That(sceneFlowEditorSource.Contains("PlayerRegistry.Motor2D"), Is.False);
            Assert.That(sceneFlowEditorSource.Contains("PlayerRegistry.Player"), Is.False);
            Assert.That(sceneFlowEditorSource.Contains("Prefer assigning IPlayerProvider or ParticipantRosterService"), Is.True);
        }

        [Test]
        public void PyralisEditor_Source_ExposesGuideOnlyRuntimeCapabilityCatalog()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorRoot = Path.Combine(gameplayRoot, "Editor");
            string catalogPath = Path.Combine(editorRoot, "PyralisRuntimeCapabilityCatalog.cs");
            string factTypesPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisAuthoringFactTypes.cs");
            string factRegistryPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisAuthoringFactRegistry.cs");
            string conventionRegistryPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisAuthoringConventionFactRegistry.cs");
            string conventionFactsPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisConventionAuthoringFacts.cs");
            string routeCoverageFactsPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisRouteCoverageFacts.cs");
            string sceneSurfaceFactsPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisSceneSurfaceEvidenceFacts.cs");
            string inspectorHandoffFactsPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisInspectorHandoffFacts.cs");
            string setupFlowMonitorPath = Path.Combine(editorRoot, "PyralisSetupFlowMonitor.cs");
            string routeProofPath = Path.Combine(editorRoot, "PyralisAuthoringRouteProof.cs");
            string authoringWindowPath = Path.Combine(editorRoot, "PyralisAuthoringWindow.cs");
            string intentAdvisorPath = Path.Combine(editorRoot, "PyralisAuthoringIntentAdvisor.cs");
            string authoringBlueprintPath = Path.Combine(gameplayRoot, "Docs", "Setup", "AUTHORING_BLUEPRINT.md");
            string roadmapPath = Path.Combine(gameplayRoot, "Docs", "FEATURE_DEVELOPMENT_ROADMAP.md");

            Assert.That(File.Exists(catalogPath), Is.True);
            Assert.That(File.Exists(factTypesPath), Is.True);
            Assert.That(File.Exists(factRegistryPath), Is.True);
            Assert.That(File.Exists(conventionRegistryPath), Is.True);
            Assert.That(File.Exists(conventionFactsPath), Is.True);
            Assert.That(File.Exists(routeCoverageFactsPath), Is.True);
            Assert.That(File.Exists(sceneSurfaceFactsPath), Is.True);
            Assert.That(File.Exists(inspectorHandoffFactsPath), Is.True);

            string catalogSource = File.ReadAllText(catalogPath);
            string factTypesSource = File.ReadAllText(factTypesPath);
            string factRegistrySource = File.ReadAllText(factRegistryPath);
            string conventionRegistrySource = File.ReadAllText(conventionRegistryPath);
            string conventionFactsSource = File.ReadAllText(conventionFactsPath);
            string routeCoverageFactsSource = File.ReadAllText(routeCoverageFactsPath);
            string sceneSurfaceFactsSource = File.ReadAllText(sceneSurfaceFactsPath);
            string inspectorHandoffFactsSource = File.ReadAllText(inspectorHandoffFactsPath);
            string setupFlowMonitorSource = File.ReadAllText(setupFlowMonitorPath);
            string routeProofSource = File.ReadAllText(routeProofPath);
            string authoringWindowSource = File.ReadAllText(authoringWindowPath);
            string intentAdvisorSource = File.ReadAllText(intentAdvisorPath);
            string sceneSurfaceGuidanceSource = File.ReadAllText(Path.Combine(editorRoot, "PyralisAuthoringSceneSurfaceGuidance.cs"));
            string authoringBlueprint = File.ReadAllText(authoringBlueprintPath);
            string roadmap = File.ReadAllText(roadmapPath);

            Assert.That(catalogSource.Contains("RuntimeCapabilityCard"), Is.True);
            Assert.That(catalogSource.Contains("PyralisAuthoringFact"), Is.True);
            Assert.That(factTypesSource.Contains("public sealed class PyralisAuthoringFact"), Is.True);
            Assert.That(factTypesSource.Contains("public sealed class PyralisAuthoringIssue"), Is.True);
            Assert.That(catalogSource.Contains("PyralisAuthoringNativeAction"), Is.True);
            Assert.That(catalogSource.Contains("PyralisAuthoringFactSourceKind.HandAuthoredGuideCard"), Is.True);
            Assert.That(catalogSource.Contains("RuntimeCapabilityLaneTag"), Is.True);
Assert.That(catalogSource.Contains("GetByGoal"), Is.True);
            Assert.That(catalogSource.Contains("GetByLane"), Is.True);
            Assert.That(catalogSource.Contains("CustomizationMoments"), Is.True);
            Assert.That(catalogSource.Contains("FirstProof"), Is.True);
            Assert.That(catalogSource.Contains("CanWait"), Is.True);
            Assert.That(catalogSource.Contains("2D Pawn Movement"), Is.True);
            Assert.That(catalogSource.Contains("Camera Follow And Bounds"), Is.True);
            Assert.That(catalogSource.Contains("Interaction Or Action Selection"), Is.True);
            Assert.That(catalogSource.Contains("Combat Attack Proof"), Is.True);
            Assert.That(catalogSource.Contains("UI And Scoring Feedback"), Is.True);
            Assert.That(catalogSource.Contains("CreateAndAssign"), Is.False);
            Assert.That(catalogSource.Contains("AssetDatabase.CreateAsset"), Is.False);
            Assert.That(catalogSource.Contains("AddComponent<"), Is.False);

            Assert.That(catalogSource.Contains("public static class PyralisAuthoringFactRegistry"), Is.False);
            Assert.That(factRegistrySource.Contains("public static class PyralisAuthoringFactRegistry"), Is.True);
            Assert.That(catalogSource.Contains("PyralisAuthoringFactKind"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("PyralisAuthoringFactKind.RouteFamily"), Is.True);
            Assert.That(catalogSource.Contains("PyralisAuthoringConfidence"), Is.True);
            Assert.That(factTypesSource.Contains("PyralisAuthoringIssue"), Is.True);
            Assert.That(factTypesSource.Contains("PyralisAuthoringIssueSeverity"), Is.True);
            Assert.That(factRegistrySource.Contains("GetFacts(PyralisAuthoringFactKind kind)"), Is.True);
            Assert.That(factRegistrySource.Contains("HasDuplicateStableIds"), Is.True);
            Assert.That(factRegistrySource.Contains("PyralisSetupFlowGuidance.GetAuthoringFacts"), Is.True);
            Assert.That(factRegistrySource.Contains("PyralisAuthoringRouteProof.GetAuthoringFacts"), Is.True);
            Assert.That(factRegistrySource.Contains("PyralisRouteCoverageFacts.GetAuthoringFacts"), Is.True);
            Assert.That(factRegistrySource.Contains("PyralisInspectorHandoffFacts.GetAuthoringFacts"), Is.True);
            Assert.That(factRegistrySource.Contains("PyralisAuthoringConventionFactRegistry.AllFacts"), Is.True);
            Assert.That(factRegistrySource.Contains("PyralisConventionAuthoringFacts.GetAuthoringFacts"), Is.False);
            Assert.That(factRegistrySource.Contains("PyralisSceneSurfaceEvidenceFacts.GetAuthoringFacts"), Is.True);
            Assert.That(conventionRegistrySource.Contains("public interface IAuthoringConventionFactProvider"), Is.True);
            Assert.That(conventionRegistrySource.Contains("public static class PyralisAuthoringConventionFactRegistry"), Is.True);
            Assert.That(conventionRegistrySource.Contains("PyralisConventionAuthoringFactBridgeProvider"), Is.True);
            Assert.That(conventionRegistrySource.Contains("typeof(IAuthoringConventionFactProvider)"), Is.True);
            Assert.That(conventionRegistrySource.Contains("StartsWith(\"NeonBlack.Gameplay\""), Is.True);
            Assert.That(conventionRegistrySource.Contains("HasDuplicateStableIds"), Is.True);
            Assert.That(conventionRegistrySource.Contains("Debug.LogWarning"), Is.True);
            Assert.That(conventionRegistrySource.Contains("PyralisConventionAuthoringFacts.GetAuthoringFacts"), Is.True);
            Assert.That(conventionFactsSource.Contains("public static class PyralisConventionAuthoringFacts"), Is.True);
            Assert.That(conventionFactsSource.Contains("PyralisSprite2DConventionAuthoringFactProvider"), Is.True);
            Assert.That(conventionFactsSource.Contains("IAuthoringConventionFactProvider"), Is.True);
            Assert.That(conventionFactsSource.Contains("\"reflection.add-component-menu.motor-2d\""), Is.True);
            Assert.That(conventionFactsSource.Contains("\"convention.serialized-field.input-profile.gameplay-actions\""), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("public static class PyralisRouteCoverageFacts"), Is.True);
            Assert.That(sceneSurfaceFactsSource.Contains("public static class PyralisSceneSurfaceEvidenceFacts"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("public static class PyralisInspectorHandoffFacts"), Is.True);
            Assert.That(conventionFactsSource.Contains("PyralisAuthoringFactSourceKind.Reflection"), Is.True);
            Assert.That(conventionFactsSource.Contains("PyralisAuthoringFactSourceKind.Convention"), Is.True);
            Assert.That(conventionFactsSource.Contains("CreateAssetMenuAttribute"), Is.True);
            Assert.That(conventionFactsSource.Contains("AddComponentMenu"), Is.True);
            Assert.That(conventionFactsSource.Contains("RequireComponent"), Is.True);
            Assert.That(conventionFactsSource.Contains("BindingFlags"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "SessionDefinition.cs")).Contains("order = 0"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "GameModeDefinition.cs")).Contains("order = 10"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "ParticipantDefinition.cs")).Contains("order = 20"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "PawnDefinition.cs")).Contains("order = 30"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "GameSetupProfile.cs")).Contains("order = -100"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "InputProfile.cs")).Contains("order = -90"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "PlayfieldProfile.cs")).Contains("order = -80"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "CameraRigProfile.cs")).Contains("order = -70"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "PawnMovementProfile.cs")).Contains("order = -60"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "PawnTraversalProfile.cs")).Contains("order = -50"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "PawnPresentationProfile.cs")).Contains("order = -40"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "PawnAnimationProfile.cs")).Contains("order = -30"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "Combat", "PawnCombatProfile.cs")).Contains("order = -20"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "SettingsProfile.cs")).Contains("order = -10"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("PyralisAuthoringFactSourceKind.InspectorGuide"), Is.True);
            Assert.That(sceneSurfaceFactsSource.Contains("PyralisAuthoringFactSourceKind.SceneEvidence"), Is.True);
            Assert.That(conventionFactsSource.Contains("PyralisAuthoringConfidence.ConventionDerived"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.gameplay-session-bootstrap.session-definition"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.pawn-definition.pawn-prefab"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.input-profile.gameplay-action-names"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.game-mode-definition.board-and-turn-rules"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.game-mode-definition.camera-and-playfield"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.game-mode-definition.required-feature-modules"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.feature-module-definition.profile-runtime-network"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.cinemachine-camera-rig-controller.camera-fields"), Is.True);
            Assert.That(inspectorHandoffFactsSource.Contains("inspector.tabletop-board-grid-presenter.board-fields"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.create-asset-menu.session-definition"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.create-asset-menu.input-profile"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.create-asset-menu.board-definition"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.create-asset-menu.feature-module-definition"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.create-asset-menu.camera-rig-profile"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.create-asset-menu.action-definition"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.add-component-menu.gameplay-session-bootstrap"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.add-component-menu.cinemachine-camera-rig-controller"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.add-component-menu.tabletop-board-grid-presenter"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.add-component-menu.participant-feedback-hud-presenter"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.add-component-menu.enemy-ai"), Is.True);
            Assert.That(conventionFactsSource.Contains("reflection.require-component.pawn-2d-movement-component"), Is.True);
            Assert.That(conventionFactsSource.Contains("convention.serialized-field.pawn-definition.pawn-prefab"), Is.True);
            Assert.That(conventionFactsSource.Contains("convention.serialized-field.input-profile.gameplay-actions"), Is.True);
            Assert.That(conventionFactsSource.Contains("convention.serialized-field.game-mode-definition.board-definition"), Is.True);
            Assert.That(conventionFactsSource.Contains("convention.serialized-field.feature-module-definition.profile-asset"), Is.True);
            Assert.That(conventionFactsSource.Contains("convention.serialized-field.cinemachine-camera-rig-controller.camera-rig-profile"), Is.True);
            Assert.That(sceneSurfaceFactsSource.Contains("scene-evidence.ui-hud-menus"), Is.True);
            Assert.That(sceneSurfaceFactsSource.Contains("scene-evidence.board-action-selection"), Is.True);
            Assert.That(sceneSurfaceFactsSource.Contains("scene-evidence.pickups-hazards-enemies"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("route.pawn-actor"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("route.npc-enemy-actor"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("route.custom-object-feature"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("route.ui-hud-menu"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("route.world-camera"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("route.tabletop-card"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("route.networking"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("RouteCoverage"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("PyralisAuthoringActionSurface.AuthoringWindow"), Is.True);
            Assert.That(routeCoverageFactsSource.Contains("relatedStableIds"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("PyralisAuthoringSemanticTag"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("BeginnerLegendTags"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("GetSemanticTag(PyralisAuthoringActionSurface surface)"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("GetSemanticTagColor"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("PyralisAuthoringSemanticTag.Animation"), Is.True);
            Assert.That(sceneSurfaceGuidanceSource.Contains("PyralisAuthoringSemanticTag.Audio"), Is.True);
            Assert.That(setupFlowMonitorSource.Contains("GetAuthoringFacts"), Is.True);
            Assert.That(setupFlowMonitorSource.Contains("GetStableId(PyralisSetupFlowStepId stepId)"), Is.True);
            Assert.That(setupFlowMonitorSource.Contains("setup.assign-participant-pawn"), Is.True);
            Assert.That(setupFlowMonitorSource.Contains("capability.2d-pawn-movement"), Is.True);
            Assert.That(routeProofSource.Contains("proof.1p-pawn-movement"), Is.True);
            Assert.That(routeProofSource.Contains("proof.board-card-action"), Is.True);
            Assert.That(routeProofSource.Contains("proof.action-selection"), Is.True);
            Assert.That(routeProofSource.Contains("proof.npc-enemy-behavior"), Is.True);
            Assert.That(routeProofSource.Contains("proof.custom-object-effect"), Is.True);
            Assert.That(routeProofSource.Contains("proof.ui-hud-menu"), Is.True);
            Assert.That(routeProofSource.Contains("proof.camera-cursor-world"), Is.True);
            Assert.That(routeProofSource.Contains("proof.generated-content"), Is.True);
            Assert.That(routeProofSource.Contains("proof.network-ownership"), Is.True);
            Assert.That(routeProofSource.Contains("CreateProofFact"), Is.True);
            Assert.That(routeProofSource.Contains("FirstProof"), Is.True);
            Assert.That(routeProofSource.Contains("PyralisAuthoringFactKind.Proof"), Is.True);

            Assert.That(intentAdvisorSource.Contains("PyralisAuthoringIntentGuideTier"), Is.True);
            Assert.That(intentAdvisorSource.Contains("MatchingIntents"), Is.True);
            Assert.That(intentAdvisorSource.Contains("Recommendations"), Is.True);
            Assert.That(intentAdvisorSource.Contains("Cautions"), Is.True);
            Assert.That(intentAdvisorSource.Contains("FindMatchingIntentFacts"), Is.True);

            Assert.That(authoringWindowSource.Contains("Runtime Capability Catalog"), Is.True);
            Assert.That(authoringWindowSource.Contains("Browse By Game Goal"), Is.True);
            Assert.That(authoringWindowSource.Contains("Browse By Runtime Lane"), Is.True);
            Assert.That(authoringWindowSource.Contains("Project Intent And Capability Map"), Is.True);
            Assert.That(authoringWindowSource.Contains("World / Playfield"), Is.True);
            Assert.That(authoringWindowSource.Contains("Control Shape"), Is.True);
            Assert.That(authoringWindowSource.Contains("Current Intent Guide"), Is.True);
            Assert.That(authoringWindowSource.Contains("DrawCurrentIntentGuide"), Is.True);
            Assert.That(authoringWindowSource.Contains("Open Guide Cards"), Is.True);
            Assert.That(authoringWindowSource.Contains("Project Capabilities I'm Considering"), Is.True);
            Assert.That(authoringWindowSource.Contains("DrawRuntimeCapabilityCatalog"), Is.True);
            Assert.That(authoringWindowSource.Contains("DrawRuntimeCapabilityCard"), Is.True);
            Assert.That(authoringWindowSource.Contains("First Proof"), Is.True);
            Assert.That(authoringWindowSource.Contains("Customization Moments"), Is.True);
            Assert.That(authoringWindowSource.Contains("No asset or component creation happens here"), Is.True);

            Assert.That(authoringBlueprint.Contains("Runtime Capability Catalog"), Is.True);
            Assert.That(authoringBlueprint.Contains("guide-only"), Is.True);
            Assert.That(authoringBlueprint.Contains("PyralisAuthoringFactRegistry"), Is.True);
            Assert.That(roadmap.Contains("guide-card model"), Is.True);
            Assert.That(roadmap.Contains("typed authoring fact pipeline"), Is.True);
            Assert.That(roadmap.Contains("pawns, NPCs/enemies, custom objects, UI, world, and networking"), Is.True);
        }

        [Test]
        public void PyralisEditor_RuntimeCapabilityCards_AreBackedByStableAuthoringFacts()
        {
            Assert.That(PyralisAuthoringFactRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            IReadOnlyList<RuntimeCapabilityCard> cards = PyralisRuntimeCapabilityCatalog.All;
            Assert.That(cards.Count, Is.GreaterThanOrEqualTo(5));

            for (int i = 0; i < cards.Count; i++)
            {
                RuntimeCapabilityCard card = cards[i];
                Assert.That(card.StableId, Is.Not.Empty);
                Assert.That(card.Fact, Is.Not.Null);
                Assert.That(card.Fact.StableId, Is.EqualTo(card.StableId));
                Assert.That(card.Fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.RuntimeCapability));
                Assert.That(card.Fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.HandAuthoredGuideCard));
                Assert.That(card.Fact.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
                Assert.That(card.Fact.NativeActions.Length, Is.GreaterThanOrEqualTo(1));
                Assert.That(PyralisAuthoringFactRegistry.Find(card.StableId), Is.SameAs(card.Fact));
            }
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
                    Assert.That(source.Contains("using UnityEditor;"), Is.False, sourcePath);
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

            string authoringBlueprint = File.ReadAllText(Path.Combine(gameplayRoot, "Docs", "Setup", "AUTHORING_BLUEPRINT.md"));
            string authoringModel = File.ReadAllText(Path.Combine(gameplayRoot, "Docs", "Setup", "AUTHORING_MODEL.md"));
            string roadmap = File.ReadAllText(Path.Combine(gameplayRoot, "Docs", "FEATURE_DEVELOPMENT_ROADMAP.md"));
            string hardeningRoadmap = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "..",
                "docs",
                "superpowers",
                "plans",
                "2026-06-04-pyralis-reflective-authoring-hardening-roadmap.md"));

            Assert.That(authoringBlueprint.Contains("Export Footprint Boundary"), Is.True);
            Assert.That(authoringBlueprint.Contains("Unity build reports"), Is.True);
            Assert.That(authoringModel.Contains("Build Footprint Model"), Is.True);
            Assert.That(authoringModel.Contains("Future build-report checks"), Is.True);
            Assert.That(roadmap.Contains("BuildReport / Export Footprint"), Is.True);
            Assert.That(roadmap.Contains("editor-only authoring contracts/facts/providers/validators out of player builds"), Is.True);
            Assert.That(hardeningRoadmap.Contains("Later Export Footprint Gate"), Is.True);
            Assert.That(hardeningRoadmap.Contains("unexpected editor assemblies"), Is.True);
        }

        private static bool SourceContainsAuthoringExportSpine(string source)
        {
            return source.Contains("AuthoringContractAttribute")
                || source.Contains("IAuthoringConventionFactProvider")
|| source.Contains("PyralisAuthoringContractRegistry")
                || source.Contains("PyralisAuthoringConventionFactRegistry")
                || source.Contains("PyralisAuthoringFactRegistry")
                || source.Contains("PyralisAuthoringFactKind")
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
