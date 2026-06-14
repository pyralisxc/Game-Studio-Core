using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Scoring;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Presentation.Camera;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Tests.Editor
{
    [Explicit("Deep setup-flow and graph projection matrix; default coverage lives in PyralisAuthoringSmokeTests.")]
    public class SetupFlowValidatorTests : PyralisEditorTestSupport
    {
        [Test]
        public void PyralisSetupFlowValidator_EmptyBootstrap_ReportsMissingSessionFirst()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.FirstBlockingStep.Label, Is.EqualTo("Assign Session Definition"));
            Assert.That(report.FirstBlockingStep.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(report.GetStep("Assign Session Definition").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(report.GetStep("Assign Default Game Mode").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Blocked));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowReport_GuidedDisplaySteps_StartsWithNextRequiredStep()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.GuidedDisplaySteps[0].Label, Is.EqualTo("Assign Session Definition"));
            Assert.That(report.GuidedDisplaySteps[0].IsRequiredIssue, Is.True);
            Assert.That(report.GuidedDisplaySteps.Select(step => step.Label), Does.Contain("Visible Lifetime Scope"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringWindow_NoSelection_UsesSingleSceneBootstrapAsFallback()
        {
            UnityEngine.SceneManagement.Scene previousScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            GameObject selectedSupportObject = new GameObject("Selected Support Object");

            try
            {
                MethodInfo fallbackMethod = typeof(PyralisAuthoringWindow).GetMethod(
                    "GetSceneFallbackSetup",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                Assert.That(fallbackMethod, Is.Not.Null);
                Assert.That(fallbackMethod.Invoke(null, new Object[] { null, null }), Is.SameAs(bootstrap));
                Assert.That(fallbackMethod.Invoke(null, new Object[] { selectedSupportObject, null }), Is.Null);
                Assert.That(fallbackMethod.Invoke(null, new Object[] { null, bootstrap }), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(selectedSupportObject);
                Object.DestroyImmediate(root);
                if (previousScene.IsValid())
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }


        [Test]
        public void PyralisSetupFlowValidator_RuntimeServiceOwnership_NamesSupportedCompositionPath()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep step = report.GetStep("Runtime Service Ownership");

            Assert.That(step.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Ready));
            Assert.That(step.Message, Does.Contain("GameplaySessionBootstrap"));
            Assert.That(step.Message, Does.Contain("PyralisGameplayLifetimeScope"));
            Assert.That(step.Message, Does.Contain("PyralisGameplayLifetimeScope"));
            Assert.That(step.Message, Does.Contain("not hidden global lookups"));

            Object.DestroyImmediate(root);
        }


        [Test]
        public void PyralisAuthoringNativeAction_FormatsUnitySurfaceAndSuccessCheck()
        {
            PyralisAuthoringNativeAction action = new PyralisAuthoringNativeAction(
                "Assign",
                PyralisAuthoringActionSurface.Inspector,
                "Gameplay Root",
                "Session Definition",
                "Authoring changes from missing session to participant setup");

            string guidance = action.ToGuidanceSentence();

            Assert.That(guidance, Does.Contain("Inspector"));
            Assert.That(guidance, Does.Contain("Gameplay Root"));
            Assert.That(guidance, Does.Contain("Session Definition"));
            Assert.That(guidance, Does.Contain("Authoring changes"));
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_SetupFlowFacts_CoverStableStepIds()
        {
            Assert.That(PyralisAuthoringGrammarRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            foreach (PyralisSetupFlowStepId stepId in System.Enum.GetValues(typeof(PyralisSetupFlowStepId)))
            {
                if (stepId == PyralisSetupFlowStepId.Unknown)
                    continue;

                string stableId = PyralisSetupFlowGuidance.GetStableId(stepId);
                Assert.That(stableId, Is.Not.Empty, stepId.ToString());

                PyralisAuthoringFact fact = PyralisAuthoringGrammarRegistry.Find(stableId);
                Assert.That(fact, Is.Not.Null, stableId);
                Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.SetupNode));
                Assert.That(fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.SetupFlow));
                Assert.That(fact.WorkIntent, Is.EqualTo(PyralisSetupFlowGuidance.GetDefaultWorkIntent(stepId).ToString()));
            }
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_TwoDPawnMovement_RelatesCapabilityToSetupNodes()
        {
            PyralisAuthoringFact capability = PyralisAuthoringGrammarRegistry.Find("capability.2d-pawn-movement");
            Assert.That(capability, Is.Not.Null);
            Assert.That(capability.Kind, Is.EqualTo(PyralisAuthoringFactKind.RuntimeCapability));
            Assert.That(capability.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));

            string[] requiredPawnSetupFacts =
            {
                "setup.assign-participant-pawn",
                "setup.assign-input-profile",
                "setup.assign-spawn-points",
                "setup.tune-pawn-visuals-and-collision",
                "setup.tune-movement-and-input-feel"
            };

            foreach (string stableId in requiredPawnSetupFacts)
            {
                PyralisAuthoringFact setupFact = PyralisAuthoringGrammarRegistry.Find(stableId);
                Assert.That(setupFact, Is.Not.Null, stableId);
                Assert.That(setupFact.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"), stableId);
                Assert.That(setupFact.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"), stableId);
                Assert.That(setupFact.Summary, Is.Not.Empty, stableId);
            }
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_OnePlayerPawnProof_ConnectsCapabilitySetupAndProof()
        {
            PyralisAuthoringFact proof = PyralisAuthoringGrammarRegistry.Find("proof.1p-pawn-movement");
            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.Kind, Is.EqualTo(PyralisAuthoringFactKind.Proof));
            Assert.That(proof.WorkIntent, Is.EqualTo("FirstProof"));
            Assert.That(proof.FirstProof, Does.Contain("One participant spawns one pawn"));
            Assert.That(proof.RequiredDefinitions, Is.Empty);
            Assert.That(proof.RequiredSceneComponents, Is.Empty);
            Assert.That(proof.NativeActions.Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(proof.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.PlayMode));
            Assert.That(proof.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));
            Assert.That(proof.RelatedStableIds, Does.Contain("setup.assign-participant-pawn"));
            Assert.That(proof.RelatedStableIds, Does.Contain("setup.assign-input-profile"));
            Assert.That(proof.RelatedStableIds, Does.Contain("setup.assign-spawn-points"));
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_InspectorHandoffFacts_LinkNativeInspectorFieldsToSetup()
        {
            PyralisAuthoringFact bootstrapSession = PyralisAuthoringGrammarRegistry.Find("inspector.gameplay-session-bootstrap.session-definition");
            Assert.That(bootstrapSession, Is.Not.Null);
            Assert.That(bootstrapSession.Kind, Is.EqualTo(PyralisAuthoringFactKind.AssignmentField));
            Assert.That(bootstrapSession.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.InspectorGuide));
            Assert.That(bootstrapSession.NativeActions.Length, Is.EqualTo(1));
            Assert.That(bootstrapSession.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));
            Assert.That(bootstrapSession.AssignmentFields, Does.Contain("GameplaySessionBootstrap.sessionDefinition -> SessionDefinition"));
            Assert.That(bootstrapSession.RelatedStableIds, Does.Contain("setup.assign-session-definition"));

            PyralisAuthoringFact pawnPrefab = PyralisAuthoringGrammarRegistry.Find("inspector.pawn-definition.pawn-prefab");
            Assert.That(pawnPrefab, Is.Not.Null);
            Assert.That(pawnPrefab.RelatedStableIds, Does.Contain("setup.assign-participant-pawn"));
            Assert.That(pawnPrefab.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));
            Assert.That(pawnPrefab.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));

            PyralisAuthoringFact inputNames = PyralisAuthoringGrammarRegistry.Find("inspector.input-profile.gameplay-action-names");
            Assert.That(inputNames, Is.Not.Null);
            Assert.That(inputNames.Kind, Is.EqualTo(PyralisAuthoringFactKind.CustomizationMoment));
            Assert.That(inputNames.WorkIntent, Is.EqualTo("ProofEnhancer"));
            Assert.That(inputNames.CustomizationMoments[0], Does.Contain("InputProfile Gameplay Action Names"));
            Assert.That(inputNames.NativeActions[0].Verb, Is.EqualTo("Customize"));

            PyralisAuthoringFact boardRules = PyralisAuthoringGrammarRegistry.Find("inspector.game-mode-definition.board-and-turn-rules");
            Assert.That(boardRules, Is.Not.Null);
            Assert.That(boardRules.AssignmentFields[0], Does.Contain("GameModeDefinition.boardDefinition"));
            Assert.That(boardRules.RelatedStableIds, Does.Contain("route.tabletop-card"));
            Assert.That(boardRules.RelatedStableIds, Does.Contain("proof.board-card-action"));

            PyralisAuthoringFact cameraFields = PyralisAuthoringGrammarRegistry.Find("inspector.cinemachine-camera-rig-controller.camera-fields");
            Assert.That(cameraFields, Is.Not.Null);
            Assert.That(cameraFields.AssignmentFields[0], Does.Contain("CinemachineCameraRigController.cameraRigProfile"));
            Assert.That(cameraFields.RelatedStableIds, Does.Contain("proof.camera-cursor-world"));

            PyralisAuthoringFact featureFields = PyralisAuthoringGrammarRegistry.Find("inspector.feature-module-definition.profile-runtime-network");
            Assert.That(featureFields, Is.Not.Null);
            Assert.That(featureFields.AssignmentFields[0], Does.Contain("FeatureModuleDefinition.profileAsset"));
            Assert.That(featureFields.RelatedStableIds, Does.Contain("route.custom-object-feature"));
            Assert.That(featureFields.RelatedStableIds, Does.Contain("proof.network-ownership"));

            PyralisAuthoringFact cameraTuning = PyralisAuthoringGrammarRegistry.Find("inspector.camera-rig-profile.framing-fields");
            Assert.That(cameraTuning, Is.Not.Null);
            Assert.That(cameraTuning.Kind, Is.EqualTo(PyralisAuthoringFactKind.CustomizationMoment));
            Assert.That(cameraTuning.NativeActions[0].Verb, Is.EqualTo("Customize"));
            Assert.That(cameraTuning.RelatedStableIds, Does.Contain("capability.camera-follow-bounds"));
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_ConventionFacts_ExposeUnityMetadataAndSerializedFields()
        {
            Assert.That(PyralisAuthoringGrammarRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            PyralisAuthoringFact sessionCreate = PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.session-definition");
            Assert.That(sessionCreate, Is.Not.Null);
            Assert.That(sessionCreate.Kind, Is.EqualTo(PyralisAuthoringFactKind.Definition));
            Assert.That(sessionCreate.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Reflection));
            Assert.That(sessionCreate.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
            Assert.That(sessionCreate.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.ProjectWindow));
            Assert.That(sessionCreate.NativeActions[0].Target, Does.Contain("Assets/Create/NeonBlack/Definitions/Session Definition"));
            Assert.That(sessionCreate.RelatedStableIds, Does.Contain("setup.assign-session-definition"));

            PyralisAuthoringFact inputCreate = PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.input-profile");
            Assert.That(inputCreate, Is.Not.Null);
            Assert.That(inputCreate.Kind, Is.EqualTo(PyralisAuthoringFactKind.Profile));
            Assert.That(inputCreate.RequiredProfiles, Does.Contain("InputProfile"));
            Assert.That(inputCreate.RelatedStableIds, Does.Contain("inspector.input-profile.gameplay-action-names"));

            PyralisAuthoringFact bootstrapComponent = PyralisAuthoringGrammarRegistry.Find("reflection.add-component-menu.gameplay-session-bootstrap");
            Assert.That(bootstrapComponent, Is.Not.Null);
            Assert.That(bootstrapComponent.Kind, Is.EqualTo(PyralisAuthoringFactKind.SceneComponent));
            Assert.That(bootstrapComponent.RequiredSceneComponents, Does.Contain("GameplaySessionBootstrap"));
            Assert.That(bootstrapComponent.NativeActions[0].FieldOrComponent, Does.Contain("NeonBlack/Gameplay/Setup/Gameplay Session Bootstrap"));

            PyralisAuthoringFact movementRequirements = PyralisAuthoringGrammarRegistry.Find("reflection.require-component.pawn-2d-movement-component");
            Assert.That(movementRequirements, Is.Not.Null);
            Assert.That(movementRequirements.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Reflection));
            Assert.That(movementRequirements.RequiredUnitySurfaces, Does.Contain("Rigidbody2D"));
            Assert.That(movementRequirements.RequiredUnitySurfaces, Does.Contain("PolygonCollider2D"));
            Assert.That(movementRequirements.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));

            PyralisAuthoringFact pawnPrefabField = PyralisAuthoringGrammarRegistry.Find("convention.serialized-field.pawn-definition.pawn-prefab");
            Assert.That(pawnPrefabField, Is.Not.Null);
            Assert.That(pawnPrefabField.Kind, Is.EqualTo(PyralisAuthoringFactKind.AssignmentField));
            Assert.That(pawnPrefabField.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Convention));
            Assert.That(pawnPrefabField.Confidence, Is.EqualTo(PyralisAuthoringConfidence.ConventionDerived));
            Assert.That(pawnPrefabField.AssignmentFields[0], Does.Contain("PawnDefinition.pawnPrefab"));
            Assert.That(pawnPrefabField.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));
            Assert.That(pawnPrefabField.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));

            PyralisAuthoringFact boardCreate = PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.board-definition");
            Assert.That(boardCreate, Is.Not.Null);
            Assert.That(boardCreate.Kind, Is.EqualTo(PyralisAuthoringFactKind.Definition));
            Assert.That(boardCreate.NativeActions[0].Target, Does.Contain("Assets/Create/NeonBlack/Rules/Board Definition"));
            Assert.That(boardCreate.RelatedStableIds, Does.Contain("proof.board-card-action"));

            PyralisAuthoringFact cameraProfileCreate = PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.camera-rig-profile");
            Assert.That(cameraProfileCreate, Is.Not.Null);
            Assert.That(cameraProfileCreate.Kind, Is.EqualTo(PyralisAuthoringFactKind.Profile));
            Assert.That(cameraProfileCreate.RequiredProfiles, Does.Contain("CameraRigProfile"));
            Assert.That(cameraProfileCreate.RelatedStableIds, Does.Contain("proof.camera-cursor-world"));

            PyralisAuthoringFact featureModuleCreate = PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.feature-module-definition");
            Assert.That(featureModuleCreate, Is.Not.Null);
            Assert.That(featureModuleCreate.RequiredDefinitions, Does.Contain("FeatureModuleDefinition"));
            Assert.That(featureModuleCreate.RelatedStableIds, Does.Contain("proof.custom-object-effect"));

            PyralisAuthoringFact tabletopPresenterComponent = PyralisAuthoringGrammarRegistry.Find("reflection.add-component-menu.tabletop-board-grid-presenter");
            Assert.That(tabletopPresenterComponent, Is.Not.Null);
            Assert.That(tabletopPresenterComponent.Kind, Is.EqualTo(PyralisAuthoringFactKind.SceneComponent));
            Assert.That(tabletopPresenterComponent.RequiredSceneComponents, Does.Contain("TabletopBoardGridPresenter"));
            Assert.That(tabletopPresenterComponent.RelatedStableIds, Does.Contain("proof.board-card-action"));

            PyralisAuthoringFact enemyAiComponent = PyralisAuthoringGrammarRegistry.Find("reflection.add-component-menu.enemy-ai");
            Assert.That(enemyAiComponent, Is.Not.Null);
            Assert.That(enemyAiComponent.Kind, Is.EqualTo(PyralisAuthoringFactKind.UnitySurface));
            Assert.That(enemyAiComponent.RelatedStableIds, Does.Contain("proof.npc-enemy-behavior"));

            PyralisAuthoringFact cameraRigField = PyralisAuthoringGrammarRegistry.Find("convention.serialized-field.cinemachine-camera-rig-controller.camera-rig-profile");
            Assert.That(cameraRigField, Is.Not.Null);
            Assert.That(cameraRigField.AssignmentFields[0], Does.Contain("CinemachineCameraRigController.cameraRigProfile"));
            Assert.That(cameraRigField.RelatedStableIds, Does.Contain("capability.camera-follow-bounds"));
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_ExplicitConventionFactsPreserveFactSurface()
        {
            IReadOnlyList<PyralisAuthoringFact> bridgeFacts = PyralisConventionAuthoringFacts.GetAuthoringFacts();
            IReadOnlyList<PyralisAuthoringFact> intentFacts = PyralisIntentVocabulary.GetAuthoringFacts();

            AssertFactsReachMainRegistry(bridgeFacts);
            AssertFactsReachMainRegistry(intentFacts);

            Assert.That(PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.participant-definition"), Is.Not.Null);
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_GameTypeIntentFacts_AreStudioWideConventionFacts()
        {
            Assert.That(PyralisAuthoringGrammarRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.RouteIntent).Count, Is.GreaterThanOrEqualTo(7));

            PyralisAuthoringFact sideView = PyralisAuthoringGrammarRegistry.Find("intent.2d-side-view-action");
            Assert.That(sideView, Is.Not.Null);
            Assert.That(sideView.Kind, Is.EqualTo(PyralisAuthoringFactKind.RouteIntent));
            Assert.That(sideView.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Convention));
            Assert.That(sideView.LaneTags, Does.Contain(RuntimeCapabilityLaneTag.Sprite2D.ToString()));
            Assert.That(sideView.GoalTags, Does.Contain("JumpTraversal"));
            Assert.That(sideView.GoalTags, Does.Contain("Input"));
            Assert.That(sideView.GoalTags, Does.Contain("AnimationPresentation"));
Assert.That(sideView.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));

            PyralisAuthoringFact brawler = PyralisAuthoringGrammarRegistry.Find("intent.pawn-brawler");
            Assert.That(brawler, Is.Not.Null);
            Assert.That(brawler.GoalTags, Does.Contain("Combat"));
            Assert.That(brawler.GoalTags, Does.Contain("JumpTraversal"));
            Assert.That(brawler.GoalTags, Does.Contain("Input"));
            Assert.That(brawler.GoalTags, Does.Contain("AnimationPresentation"));
Assert.That(brawler.RelatedStableIds, Does.Contain("capability.combat-projectile-proof"));

            PyralisAuthoringFact topDown = PyralisAuthoringGrammarRegistry.Find("intent.2d-top-down-plane");
            Assert.That(topDown, Is.Not.Null);
            Assert.That(topDown.RouteRelevance, Does.Contain("top-down"));
            Assert.That(topDown.CanWait, Does.Contain("side-view gravity ground"));
        }

        [Test]
        public void PyralisAuthoringIntentAdvisor_Sprite2DBrawlerIntent_RanksRouteCapabilitiesWithoutCreatingPreset()
        {
            PyralisAuthoringIntentSelection selection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Combat | AuthoringCapability.Input | AuthoringCapability.Animation | AuthoringCapability.Camera,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityVertical);

            PyralisAuthoringIntentModel model = PyralisAuthoringIntentAdvisor.Build(selection);

            Assert.That(model.Summary, Does.Contain("Active focus"));
            Assert.That(model.MatchingIntents.Select(fact => fact.StableId), Does.Contain("intent.2d-side-view-action"));
            Assert.That(model.MatchingIntents.Select(fact => fact.StableId), Does.Contain("intent.pawn-brawler"));

            Assert.That(FindIntentRow(model.Recommendations, "capability.2d-pawn-movement"), Is.Not.Null);
            Assert.That(FindIntentRow(model.Recommendations, "capability.combat-projectile-proof"), Is.Not.Null);
            Assert.That(FindIntentRow(model.Recommendations, "capability.camera-follow-bounds"), Is.Not.Null);
            Assert.That(
                FindIntentRowIndex(model.Recommendations, "capability.2d-pawn-movement"),
                Is.LessThan(FindIntentRowIndex(model.Recommendations, "capability.camera-follow-bounds")));
            Assert.That(
                FindIntentRowIndex(model.Recommendations, "capability.combat-projectile-proof"),
                Is.LessThan(FindIntentRowIndex(model.Recommendations, "capability.camera-follow-bounds")));

            PyralisAuthoringIntentRow brawler = FindIntentRow(model.Recommendations, "intent.pawn-brawler");
            Assert.That(brawler, Is.Not.Null);
            Assert.That(brawler.Fact.NativeActions, Is.Empty);
            Assert.That(brawler.Tier, Is.EqualTo(PyralisAuthoringIntentGuideTier.Primary));

            PyralisAuthoringIntentRow movement = FindIntentRow(model.Recommendations, "capability.2d-pawn-movement");
            Assert.That(movement, Is.Not.Null);
            Assert.That(movement.Tier, Is.Not.EqualTo(PyralisAuthoringIntentGuideTier.OptionalEnhancer));

            PyralisAuthoringIntentRow combat = FindIntentRow(model.Recommendations, "capability.combat-projectile-proof");
            Assert.That(combat, Is.Not.Null);
            Assert.That(combat.Tier, Is.Not.EqualTo(PyralisAuthoringIntentGuideTier.OptionalEnhancer));
        }

        [Test]
        public void PyralisAuthoringCapabilityDescriptorRegistry_CentralizesContractCapabilityProjection()
        {
            RuntimeCapabilityFamily[] brawlerFamilies = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                AuthoringCapability.Movement | AuthoringCapability.Combat | AuthoringCapability.Input | AuthoringCapability.Animation | AuthoringCapability.Camera,
                RuntimeCapabilityLaneTag.Mixed,
                AuthoringWorldAxiom.None);
            Assert.That(brawlerFamilies, Does.Contain(RuntimeCapabilityFamily.CharacterPawnGameplay));
            Assert.That(brawlerFamilies, Does.Contain(RuntimeCapabilityFamily.Combat));
            Assert.That(brawlerFamilies, Does.Contain(RuntimeCapabilityFamily.CameraInput));
            Assert.That(brawlerFamilies, Does.Contain(RuntimeCapabilityFamily.AnimationPresentation));

            RuntimeCapabilityFamily[] rangedFamilies = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                AuthoringCapability.RangedFlow,
                RuntimeCapabilityLaneTag.Mixed,
                AuthoringWorldAxiom.None);
            Assert.That(rangedFamilies, Does.Contain(RuntimeCapabilityFamily.GunsProjectiles));
            Assert.That(rangedFamilies, Does.Contain(RuntimeCapabilityFamily.Combat));

            RuntimeCapabilityFamily[] tabletopFamilies = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                AuthoringCapability.None,
                RuntimeCapabilityLaneTag.TabletopBoard,
                AuthoringWorldAxiom.None);
            Assert.That(tabletopFamilies, Does.Contain(RuntimeCapabilityFamily.BoardCardTabletop));

            RuntimeCapabilityFamily[] cursorFamilies = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                AuthoringCapability.None,
                RuntimeCapabilityLaneTag.CameraCursor,
                AuthoringWorldAxiom.None);
            Assert.That(cursorFamilies, Does.Contain(RuntimeCapabilityFamily.CameraInput));

            RuntimeCapabilityFamily[] proceduralFamilies = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                AuthoringCapability.Environment,
                RuntimeCapabilityLaneTag.Mixed,
                AuthoringWorldAxiom.InfiniteSpace);
            Assert.That(proceduralFamilies, Does.Contain(RuntimeCapabilityFamily.ProceduralGeneration));

            RuntimeCapabilityFamily[] networkFamilies = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                AuthoringCapability.None,
                RuntimeCapabilityLaneTag.Mixed,
                AuthoringWorldAxiom.Networked);
            Assert.That(networkFamilies, Does.Contain(RuntimeCapabilityFamily.Networking));
        }

        [Test]
        public void PyralisAuthoringIntentAdvisor_CurrentIntentCards_AreRankedFromCookbook()
        {
            PyralisAuthoringIntentModel tabletop = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(
                    RuntimeCapabilityLaneTag.TabletopBoard,
                    AuthoringCapability.Tabletop | AuthoringCapability.Camera,
                    AuthoringWorldAxiom.TurnBased));

            Assert.That(tabletop.Summary, Does.Contain("DNA Axioms"));
            Assert.That(tabletop.MatchingIntents.Select(fact => fact.StableId), Does.Contain("intent.tabletop-board-card"));
            Assert.That(
                tabletop.Recommendations.Any(row => row.Fact.StableId.Contains("board") || row.Fact.StableId.Contains("tabletop")),
                Is.True);

            PyralisAuthoringIntentModel networking = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(
                    RuntimeCapabilityLaneTag.Mixed,
                    AuthoringCapability.Networking,
                    AuthoringWorldAxiom.None));

            Assert.That(
                networking.Recommendations.Any(row => row.Fact.StableId.Contains("network")),
                Is.True);
        }

        [Test]
        public void PyralisAuthoringIntentAdvisor_LaneSelection_ShowsUnsupportedFactsAsCautions()
        {
            PyralisAuthoringFact spriteOnly = new PyralisAuthoringFact(
                "test.sprite-only-capability",
                "Sprite Only Capability",
                PyralisAuthoringFactKind.RuntimeCapability,
                PyralisAuthoringFactSourceKind.HandAuthoredGuideCard,
                PyralisAuthoringConfidence.Explicit,
                "Sprite-only test fact.",
                "Used to prove lane cautions.",
                "Proof",
                goalTags: new[] { "Movement" },
                laneTags: new[] { RuntimeCapabilityLaneTag.Sprite2D.ToString() },
                unsupportedLaneTags: new[] { RuntimeCapabilityLaneTag.ThirdPerson3D.ToString() },
                capability: AuthoringCapability.Movement);
            PyralisAuthoringFact rigged = new PyralisAuthoringFact(
                "test.rigged-capability",
                "Rigged Capability",
                PyralisAuthoringFactKind.RuntimeCapability,
                PyralisAuthoringFactSourceKind.HandAuthoredGuideCard,
                PyralisAuthoringConfidence.Explicit,
                "Rigged test fact.",
                "Used to prove lane ranking.",
                "Proof",
                goalTags: new[] { "Movement" },
                laneTags: new[] { RuntimeCapabilityLaneTag.ThirdPerson3D.ToString() },
                capability: AuthoringCapability.Movement);

            PyralisAuthoringIntentModel model = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(RuntimeCapabilityLaneTag.ThirdPerson3D, AuthoringCapability.Movement, AuthoringWorldAxiom.None),
                new[] { spriteOnly, rigged });

            Assert.That(FindIntentRow(model.Recommendations, "test.rigged-capability"), Is.Not.Null);
            PyralisAuthoringIntentRow caution = FindIntentRow(model.Cautions, "test.sprite-only-capability");
            Assert.That(caution, Is.Not.Null);
            Assert.That(caution.Tier, Is.EqualTo(PyralisAuthoringIntentGuideTier.Caution));
        }

        private static void AssertFactsReachMainRegistry(IReadOnlyList<PyralisAuthoringFact> facts)
        {
            for (int i = 0; i < facts.Count; i++)
            {
                PyralisAuthoringFact directFact = facts[i];
                PyralisAuthoringFact registryFact = PyralisAuthoringGrammarRegistry.Find(directFact.StableId);

                Assert.That(registryFact, Is.Not.Null, directFact.StableId);
                Assert.That(registryFact.Kind, Is.EqualTo(directFact.Kind), directFact.StableId);
                Assert.That(registryFact.SourceKind, Is.EqualTo(directFact.SourceKind), directFact.StableId);
                Assert.That(registryFact.Confidence, Is.EqualTo(directFact.Confidence), directFact.StableId);
                Assert.That(registryFact.RelatedStableIds, Is.EquivalentTo(directFact.RelatedStableIds), directFact.StableId);
            }
        }

        private static bool ContainsFact(IReadOnlyList<PyralisAuthoringFact> facts, string stableId)
        {
            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i].MatchesStableId(stableId))
                    return true;
            }

            return false;
        }

        private static PyralisAuthoringIntentRow FindIntentRow(IReadOnlyList<PyralisAuthoringIntentRow> rows, string stableId)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Fact.MatchesStableId(stableId))
                    return rows[i];
            }

            return null;
        }

        private static int FindIntentRowIndex(IReadOnlyList<PyralisAuthoringIntentRow> rows, string stableId)
        {
            if (rows == null)
                return int.MaxValue;

            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i]?.Fact != null && rows[i].Fact.MatchesStableId(stableId))
                    return i;
            }

            return int.MaxValue;
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_RouteCoverageFacts_NameBroadAuthoringSurfaces()
        {
            Assert.That(PyralisAuthoringGrammarRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            string[] routeFamilies =
            {
                "route.pawn-actor",
                "route.npc-enemy-actor",
                "route.custom-object-feature",
                "route.ui-hud-menu",
                "route.world-camera",
                "route.tabletop-card",
                "route.networking"
            };

            foreach (string stableId in routeFamilies)
            {
                PyralisAuthoringFact route = PyralisAuthoringGrammarRegistry.Find(stableId);
                Assert.That(route, Is.Not.Null, stableId);
                Assert.That(route.Kind, Is.EqualTo(PyralisAuthoringFactKind.RouteFamily), stableId);
                Assert.That(route.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.HandAuthoredGuideCard), stableId);
                Assert.That(route.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit), stableId);
                Assert.That(route.WorkIntent, Is.EqualTo("RouteCoverage"), stableId);
                Assert.That(route.FirstProof, Is.Not.Empty, stableId);
                Assert.That(route.NativeActions.Length, Is.GreaterThanOrEqualTo(2), stableId);
                Assert.That(route.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.AuthoringWindow), stableId);
                Assert.That(route.NativeActions[1].Surface, Is.EqualTo(PyralisAuthoringActionSurface.PlayMode), stableId);
            }

            PyralisAuthoringFact pawn = PyralisAuthoringGrammarRegistry.Find("route.pawn-actor");
            Assert.That(pawn.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));
            Assert.That(pawn.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));
            Assert.That(pawn.RequiredDefinitions, Does.Contain("PawnDefinition"));
            Assert.That(pawn.RequiredUnitySurfaces, Does.Contain("PawnRoot"));

            PyralisAuthoringFact tabletop = PyralisAuthoringGrammarRegistry.Find("route.tabletop-card");
            Assert.That(tabletop.LaneTags, Does.Contain("TabletopBoard"));
            Assert.That(tabletop.UnsupportedLaneTags, Does.Contain("Sprite2D"));
            Assert.That(tabletop.RequiredDefinitions, Does.Contain("BoardDefinition"));

            PyralisAuthoringFact networking = PyralisAuthoringGrammarRegistry.Find("route.networking");
            Assert.That(networking.LaneTags, Does.Contain("Networked"));
            Assert.That(networking.AssignmentFields, Does.Contain("SessionDefinition.networkMode"));
            Assert.That(networking.RelatedStableIds, Does.Contain("proof.network-ownership"));
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_ProofVocabularyFacts_NameBroadFirstProofs()
        {
            Assert.That(PyralisAuthoringGrammarRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            string[] proofIds =
            {
                "proof.1p-pawn-movement",
                "proof.board-card-action",
                "proof.action-selection",
                "proof.npc-enemy-behavior",
                "proof.custom-object-effect",
                "proof.ui-hud-menu",
                "proof.camera-cursor-world",
                "proof.generated-content",
                "proof.network-ownership"
            };

            foreach (string stableId in proofIds)
            {
                PyralisAuthoringFact proof = PyralisAuthoringGrammarRegistry.Find(stableId);
                Assert.That(proof, Is.Not.Null, stableId);
                Assert.That(proof.Kind, Is.EqualTo(PyralisAuthoringFactKind.Proof), stableId);
                Assert.That(proof.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.SetupFlow), stableId);
                Assert.That(proof.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit), stableId);
                Assert.That(proof.WorkIntent, Is.EqualTo("FirstProof"), stableId);
                Assert.That(proof.FirstProof, Is.Not.Empty, stableId);
                Assert.That(proof.CanWait.Length, Is.GreaterThanOrEqualTo(3), stableId);
                Assert.That(proof.NativeActions.Length, Is.GreaterThanOrEqualTo(1), stableId);
                Assert.That(proof.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.PlayMode), stableId);
            }

            PyralisAuthoringFact tabletopProof = PyralisAuthoringGrammarRegistry.Find("proof.board-card-action");
            Assert.That(tabletopProof.LaneTags, Does.Contain("TabletopBoard"));
            Assert.That(tabletopProof.UnsupportedLaneTags, Does.Contain("Sprite2D"));
            Assert.That(tabletopProof.RelatedStableIds, Does.Contain("route.tabletop-card"));
            Assert.That(tabletopProof.RelatedStableIds, Does.Contain("capability.interaction-action-selection"));

            PyralisAuthoringFact uiProof = PyralisAuthoringGrammarRegistry.Find("proof.ui-hud-menu");
            Assert.That(uiProof.RequiredSceneComponents, Is.Empty);
            Assert.That(uiProof.RelatedStableIds, Does.Contain("route.ui-hud-menu"));

            PyralisAuthoringFact cameraProof = PyralisAuthoringGrammarRegistry.Find("proof.camera-cursor-world");
            Assert.That(cameraProof.RelatedStableIds, Does.Contain("capability.camera-follow-bounds"));

            PyralisAuthoringFact networkProof = PyralisAuthoringGrammarRegistry.Find("proof.network-ownership");
            Assert.That(networkProof.LaneTags, Does.Contain("Networked"));
            Assert.That(networkProof.RequiredSceneComponents, Is.Empty);
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_SceneEvidenceFacts_LinkSurfaceGuidanceToProofTargets()
        {
            Assert.That(PyralisAuthoringGrammarRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            string[] sceneEvidenceIds =
            {
                "scene-evidence.environment-playfield",
                "scene-evidence.camera-bounds",
                "scene-evidence.ui-hud-menus",
                "scene-evidence.scoring-objectives",
                "scene-evidence.board-action-selection",
                "scene-evidence.pickups-hazards-enemies"
            };

            foreach (string stableId in sceneEvidenceIds)
            {
                PyralisAuthoringFact sceneEvidence = PyralisAuthoringGrammarRegistry.Find(stableId);
                Assert.That(sceneEvidence, Is.Not.Null, stableId);
                Assert.That(sceneEvidence.Kind, Is.EqualTo(PyralisAuthoringFactKind.SceneComponent), stableId);
                Assert.That(sceneEvidence.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.SceneEvidence), stableId);
                Assert.That(sceneEvidence.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Inferred), stableId);
                Assert.That(sceneEvidence.WorkIntent, Is.EqualTo("SceneEvidence"), stableId);
                Assert.That(sceneEvidence.FirstProof, Is.Not.Empty, stableId);
                Assert.That(sceneEvidence.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.Hierarchy), stableId);
            }

            PyralisAuthoringFact uiEvidence = PyralisAuthoringGrammarRegistry.Find("scene-evidence.ui-hud-menus");
            Assert.That(uiEvidence.RequiredSceneComponents, Does.Contain("Canvas"));
            Assert.That(uiEvidence.RequiredSceneComponents, Does.Contain("EventSystem"));
            Assert.That(uiEvidence.RelatedStableIds, Does.Contain("proof.ui-hud-menu"));

            PyralisAuthoringFact selectionEvidence = PyralisAuthoringGrammarRegistry.Find("scene-evidence.board-action-selection");
            Assert.That(selectionEvidence.RequiredSceneComponents, Does.Contain("TabletopBoardGridPresenter"));
            Assert.That(selectionEvidence.RelatedStableIds, Does.Contain("proof.board-card-action"));
            Assert.That(selectionEvidence.RelatedStableIds, Does.Contain("proof.action-selection"));

            PyralisAuthoringFact encounterEvidence = PyralisAuthoringGrammarRegistry.Find("scene-evidence.pickups-hazards-enemies");
            Assert.That(encounterEvidence.RequiredSceneComponents, Does.Contain("EnemySpawner"));
            Assert.That(encounterEvidence.RelatedStableIds, Does.Contain("proof.npc-enemy-behavior"));
            Assert.That(encounterEvidence.RelatedStableIds, Does.Contain("proof.custom-object-effect"));
        }

        [Test]
        public void PyralisAuthoringSemanticTags_HaveBeginnerLegendLabelsAndColors()
        {
            Assert.That(PyralisAuthoringLabelUtility.BeginnerLegendTags.Length, Is.GreaterThanOrEqualTo(10));

            foreach (PyralisAuthoringSemanticTag tag in System.Enum.GetValues(typeof(PyralisAuthoringSemanticTag)))
            {
                string label = PyralisAuthoringLabelUtility.GetSemanticTagLabel(tag);
                Color color = PyralisAuthoringLabelUtility.GetSemanticTagColor(tag);

                Assert.That(label, Is.Not.Empty, tag.ToString());
                Assert.That(color.a, Is.GreaterThan(0f), tag.ToString());
            }

            Assert.That(PyralisAuthoringLabelUtility.GetSemanticTag(PyralisAuthoringActionSurface.ProjectWindow), Is.EqualTo(PyralisAuthoringSemanticTag.Project));
            Assert.That(PyralisAuthoringLabelUtility.GetSemanticTag(PyralisAuthoringActionSurface.Hierarchy), Is.EqualTo(PyralisAuthoringSemanticTag.Hierarchy));
            Assert.That(PyralisAuthoringLabelUtility.GetSemanticTag(PyralisAuthoringActionSurface.Inspector), Is.EqualTo(PyralisAuthoringSemanticTag.Inspector));
            Assert.That(PyralisAuthoringLabelUtility.GetSemanticTag(PyralisAuthoringActionSurface.PlayMode), Is.EqualTo(PyralisAuthoringSemanticTag.PlayMode));
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_FactExplorerCoverage_IncludesCurrentAuthoring2Kinds()
        {
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.RouteFamily).Count, Is.GreaterThanOrEqualTo(7));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.RuntimeCapability).Count, Is.GreaterThanOrEqualTo(5));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.SetupNode).Count, Is.GreaterThanOrEqualTo(10));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.Definition).Count, Is.GreaterThanOrEqualTo(15));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.Profile).Count, Is.GreaterThanOrEqualTo(9));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.SceneComponent).Count, Is.GreaterThanOrEqualTo(14));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.UnitySurface).Count, Is.GreaterThanOrEqualTo(7));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.Proof).Count, Is.GreaterThanOrEqualTo(9));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.AssignmentField).Count, Is.GreaterThanOrEqualTo(20));
            Assert.That(PyralisAuthoringGrammarRegistry.GetFacts(PyralisAuthoringFactKind.CustomizationMoment).Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(PyralisAuthoringGrammarRegistry.Find("capability.2d-pawn-movement"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("proof.1p-pawn-movement"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("proof.board-card-action"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("proof.ui-hud-menu"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("proof.camera-cursor-world"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("proof.network-ownership"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("inspector.input-profile.gameplay-action-names"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.session-definition"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.board-definition"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("reflection.create-asset-menu.feature-module-definition"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("reflection.add-component-menu.tabletop-board-grid-presenter"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("reflection.add-component-menu.cinemachine-camera-rig-controller"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("reflection.add-component-menu.gameplay-session-bootstrap"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("scene-evidence.ui-hud-menus"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("scene-evidence.board-action-selection"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("convention.serialized-field.pawn-definition.pawn-prefab"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("convention.serialized-field.game-mode-definition.board-definition"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("inspector.game-mode-definition.board-and-turn-rules"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("inspector.feature-module-definition.profile-runtime-network"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("route.tabletop-card"), Is.Not.Null);
            Assert.That(PyralisAuthoringGrammarRegistry.Find("route.networking"), Is.Not.Null);
        }

        [Test]
        public void PyralisAuthoringOverviewModel_NoActiveSetup_GuidesBlankSceneFoundation()
        {
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(null, null);

            Assert.That(model.ReadyToPressPlay, Is.False);
            Assert.That(model.FirstProofLabel, Is.EqualTo("Create Setup Foundation"));
            Assert.That(model.FirstProofGuidance, Does.Contain("GameplaySessionBootstrap"));
            Assert.That(model.FirstProofGuidance, Does.Contain("SessionDefinition"));
            Assert.That(model.DoNow.Select(issue => issue.Label), Does.Contain("Create Gameplay Root"));
            Assert.That(model.BestNextAction, Does.Contain("Create Gameplay Root"));
            Assert.That(model.BestNextAction, Does.Contain("Hierarchy"));
            Assert.That(model.BestNextAction, Does.Contain("Add Component"));
            Assert.That(model.PlayModeChecklist.Select(item => item.Label), Does.Contain("Setup foundation"));
        }

        [Test]
        public void PyralisAuthoringOverviewModel_EmptyBootstrap_PutsMissingSessionInDoNow()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model.ReadyToPressPlay, Is.False);
            Assert.That(model.DoNow.Select(issue => issue.Label), Does.Contain("Assign Session Definition"));
            Assert.That(model.DoSoon.Select(issue => issue.Label), Does.Contain("Visible Lifetime Scope"));
            Assert.That(model.BestNextAction, Does.StartWith("Assign Session Definition"));
            Assert.That(model.BestNextAction, Does.Contain("Project"));
            Assert.That(model.BestNextAction, Does.Contain("right-click"));
            Assert.That(model.BestNextAction, Does.Contain("setup folder"));
            Assert.That(model.BestNextAction, Does.Contain("imported art folders separate"));
            Assert.That(model.BestNextAction, Does.Contain("GameplaySessionBootstrap > Session Definition"));
            Assert.That(model.FirstProofLabel, Is.EqualTo("Choose Capability Ingredients"));
            Assert.That(model.FirstProofGuidance, Does.Contain("Intent"));
            Assert.That(model.FirstProofDeferUntilAfter, Does.Contain("scene wiring"));
            Assert.That(model.DoNow.First(issue => issue.Label == "Assign Session Definition").NativeActionGuidance, Does.Contain("Session Definition"));
            Assert.That(model.DoNow.First(issue => issue.Label == "Assign Session Definition").NativeActionGuidance, Does.Contain("project-owned setup folder"));
            Assert.That(model.DoNow.First(issue => issue.Label == "Assign Session Definition").NativeActionGuidance, Does.Contain("then confirm"));

            Object.DestroyImmediate(root);
        }


        [Test]
        public void PyralisNative1PMovementChecklist_ComponentsExposeAddComponentMenus()
        {
            AssertAddComponentMenu<GameplaySessionBootstrap>("NeonBlack/Gameplay/Setup/Gameplay Session Bootstrap");
            AssertAddComponentMenu(
                "NeonBlack.Gameplay.Core.Runtime.PyralisGameplayLifetimeScope, NeonBlack.Gameplay",
                "NeonBlack/Gameplay/Setup/Pyralis Gameplay Lifetime Scope");
            AssertAddComponentMenu<PawnRoot>("NeonBlack/Gameplay/Characters/Pawn Root");
            AssertAddComponentMenu<Motor2D>("NeonBlack/Gameplay/Runtime 2D/Motor 2D");
            AssertAddComponentMenu<Motor2DInputAdapter>("NeonBlack/Gameplay/Input/2D Motor Input Adapter");
            AssertAddComponentMenu<Pawn2DMovementComponent>("NeonBlack/Gameplay/Characters/2D/Pawn 2D Movement Component");
            AssertAddComponentMenu<Pawn2DPresentationComponent>("NeonBlack/Gameplay/Characters/2D/Pawn 2D Presentation Component");
        }

        [Test]
        public void PyralisAuthoringGrammarRegistry_Native1PMovementChecklist_ExposesCreateAndAddComponentFacts()
        {
            string[] expectedFactIds =
            {
                "reflection.create-asset-menu.session-definition",
                "reflection.create-asset-menu.game-mode-definition",
                "reflection.create-asset-menu.game-setup-profile",
                "reflection.create-asset-menu.participant-definition",
                "reflection.create-asset-menu.pawn-definition",
                "reflection.create-asset-menu.input-profile",
                "reflection.create-asset-menu.pawn-movement-profile",
                "reflection.create-asset-menu.pawn-presentation-profile",
                "reflection.add-component-menu.gameplay-session-bootstrap",
                "reflection.add-component-menu.pyralis-gameplay-lifetime-scope",
                "reflection.add-component-menu.pawn-root",
                "reflection.add-component-menu.motor-2d",
                "reflection.add-component-menu.motor-2d-input-adapter",
                "reflection.add-component-menu.pawn-2d-movement-component",
                "reflection.add-component-menu.pawn-2d-presentation-component"
            };

            foreach (string factId in expectedFactIds)
            {
                PyralisAuthoringFact fact = PyralisAuthoringGrammarRegistry.Find(factId);
                Assert.That(fact, Is.Not.Null, $"Missing native authoring fact `{factId}`.");
                Assert.That(fact.NativeActions, Is.Not.Empty, $"Native authoring fact `{factId}` should expose a Unity action.");
            }

            PyralisAuthoringFact movement = PyralisAuthoringGrammarRegistry.Find("reflection.add-component-menu.motor-2d-input-adapter");
            Assert.That(movement.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));
            Assert.That(movement.NativeActions[0].Verb, Is.EqualTo("Add Component"));
            Assert.That(movement.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));
        }


        [Test]
        public void PyralisPawnPrefabReadinessAnalysis_Pawn2DPrefab_FlagsEnvironmentSizedSpriteAndPhysicsDefaults()
        {
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject root = new GameObject("Large Visual Pawn");
            Texture2D texture = new Texture2D(640, 640);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 640f, 640f), new Vector2(0.5f, 0.5f), 64f);

            try
            {
                Rigidbody2D body = root.AddComponent<Rigidbody2D>();
                body.gravityScale = 1f;
                body.constraints = RigidbodyConstraints2D.None;
                root.AddComponent<PolygonCollider2D>();
                root.AddComponent<Pawn2DMovementComponent>();
                SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                pawn.pawnPrefab = root;

                List<string> issues = PyralisPawnPrefabReadinessAnalysis.BuildIssues(pawn);

                Assert.That(issues.Any(issue => issue.Contains("Gravity Scale to 0")), Is.True);
                Assert.That(issues.Any(issue => issue.Contains("Freeze Rotation")), Is.True);
                Assert.That(issues.Any(issue => issue.Contains("environment-sized sprite")), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(pawn);
            }
        }


        [Test]
        public void PyralisAuthoringCurrentStep_EmptySession_AsksForDefaultGameModeBeforeCapabilities()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);
            PyralisAuthoringCurrentStepGraphRow currentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(graph);

            Assert.That(currentStep.RouteName, Is.EqualTo("No setup route selected"));
            Assert.That(currentStep.Message, Does.Contain("Default Game Mode"));

            Object.DestroyImmediate(session);
        }


        private sealed class TestPawnMotor : MonoBehaviour, IPawnMotor
        {
            public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile)
            {
            }
        }

        private static void AssertAddComponentMenu<T>(string expectedMenuPath)
            where T : Component
        {
            AddComponentMenu attribute = typeof(T).GetCustomAttribute<AddComponentMenu>();
            Assert.That(attribute, Is.Not.Null, $"{typeof(T).Name} should expose an explicit Add Component menu path for native authoring.");
            Assert.That(attribute.componentMenu, Is.EqualTo(expectedMenuPath));
        }

        private static void AssertAddComponentMenu(string assemblyQualifiedTypeName, string expectedMenuPath)
        {
            System.Type type = System.Type.GetType(assemblyQualifiedTypeName);
            Assert.That(type, Is.Not.Null, $"Expected to resolve `{assemblyQualifiedTypeName}`.");
            AddComponentMenu attribute = type.GetCustomAttribute<AddComponentMenu>();
            Assert.That(attribute, Is.Not.Null, $"{type.Name} should expose an explicit Add Component menu path for native authoring.");
            Assert.That(attribute.componentMenu, Is.EqualTo(expectedMenuPath));
        }

        private static CameraRigProfile AddOrthographicCameraRig(GameplaySessionBootstrap bootstrap, GameModeDefinition mode, out GameObject cameraRoot)
        {
            cameraRoot = new GameObject("Camera Root");
            CinemachineCameraRigController rig = cameraRoot.AddComponent<CinemachineCameraRigController>();
            Camera targetCamera = new GameObject("Target Camera").AddComponent<Camera>();
            targetCamera.transform.SetParent(cameraRoot.transform, false);
            targetCamera.orthographic = true;

            CameraRigProfile profile = ScriptableObject.CreateInstance<CameraRigProfile>();
            profile.orthographic = true;
            mode.cameraRigProfile = profile;
            SetObjectReference(rig, "cameraRigProfile", profile);
            SetObjectReference(rig, "targetCamera", targetCamera);
            SetObjectReference(bootstrap, "cameraRigController", rig);
            return profile;
        }

        private static Object CreateInputActionsWithMove()
        {
            System.Type inputActionAssetType = System.Type.GetType("UnityEngine.InputSystem.InputActionAsset, Unity.InputSystem");
            Assert.That(inputActionAssetType, Is.Not.Null);
            System.Reflection.MethodInfo fromJson = inputActionAssetType.GetMethod("FromJson", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.That(fromJson, Is.Not.Null);

            const string json = "{\"name\":\"Pyralis Test Actions\",\"maps\":[{\"name\":\"Player\",\"id\":\"11111111-1111-1111-1111-111111111111\",\"actions\":[{\"name\":\"Move\",\"type\":\"Value\",\"id\":\"22222222-2222-2222-2222-222222222222\",\"expectedControlType\":\"Vector2\"}],\"bindings\":[]}],\"controlSchemes\":[]}";
            return (Object)fromJson.Invoke(null, new object[] { json });
        }

        private static InputProfile CreateMoveInputProfile(Object actions)
        {
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            inputProfile.name = "Move Input Profile";
            typeof(InputProfile).GetField("actions").SetValue(inputProfile, actions);
            inputProfile.primaryActionMap = "Player";
            inputProfile.actionBindings = new[]
            {
                GameplayInputActionBinding.BuiltIn(GameplayInputActionRole.Move, "Move", GameplayInputValueType.Vector2, true)
            };
            inputProfile.supportsKeyboardMouse = true;
            return inputProfile;
        }

        private sealed class TestPawnPresentation : MonoBehaviour, IPawnPresentationModule
        {
            public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
            {
            }
        }

        private sealed class TestPawnInput : MonoBehaviour, IPawnInputModule
        {
            public void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile inputProfile)
            {
            }
        }

        private sealed class TestProjectileRuntimeBody : MonoBehaviour, IProjectileRuntimeBody
        {
            public void Launch(ProjectileSpawnCommand command, NeonBlack.Gameplay.Core.Contracts.IHitPauseSink hitPauseSink = null, NeonBlack.Gameplay.Core.Contracts.ICameraShakeSink cameraShakeSink = null)
            {
            }
        }
    }
}
