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
        public void PyralisAuthoringWindow_LooseCreatedAssetSelection_KeepsRememberedSetupWhenItNeedsThatAssignment()
        {
            UnityEngine.SceneManagement.Scene previousScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();

            try
            {
                MethodInfo resolveMethod = typeof(PyralisAuthoringWindow).GetMethod(
                    "ResolveActiveSetup",
                    BindingFlags.Static | BindingFlags.NonPublic);

                Assert.That(resolveMethod, Is.Not.Null);
                Assert.That(
                    resolveMethod.Invoke(null, new Object[] { session, session, null, null, bootstrap }),
                    Is.SameAs(bootstrap));
                Assert.That(
                    resolveMethod.Invoke(null, new Object[] { session, session, null, null, null }),
                    Is.SameAs(bootstrap));

                SetObjectReference(bootstrap, "sessionDefinition", session);
                Assert.That(
                    resolveMethod.Invoke(null, new Object[] { mode, mode, null, null, bootstrap }),
                    Is.SameAs(bootstrap));
                Assert.That(
                    resolveMethod.Invoke(null, new Object[] { mode, mode, null, null, null }),
                    Is.SameAs(bootstrap));

                SetObjectReference(session, "defaultGameMode", mode);
                Assert.That(
                    resolveMethod.Invoke(null, new Object[] { setupProfile, setupProfile, null, null, bootstrap }),
                    Is.SameAs(bootstrap));
                Assert.That(
                    resolveMethod.Invoke(null, new Object[] { setupProfile, setupProfile, null, null, null }),
                    Is.SameAs(bootstrap));
            }
            finally
            {
                Object.DestroyImmediate(setupProfile);
                Object.DestroyImmediate(mode);
                Object.DestroyImmediate(session);
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
        public void PyralisSetupFlowValidator_SessionWithoutMode_ReportsMissingMode()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.FirstBlockingStep.Label, Is.EqualTo("Assign Default Game Mode"));
            Assert.That(report.GetStep("Assign Default Game Mode").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(report.GetStep("Assign Setup Profile").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Blocked));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_ModeWithoutSetupProfile_ReportsMissingSetupProfile()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            session.defaultGameMode = mode;
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.FirstBlockingStep.Label, Is.EqualTo("Assign Setup Profile"));
            Assert.That(report.GetStep("Assign Setup Profile").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(report.GetStep(PyralisSetupFlowStepId.AddRuntimePatterns).Status, Is.EqualTo(PyralisSetupFlowStepStatus.Blocked));

            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_SetupProfileWithoutPatterns_ReportsMissingPatterns()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Empty Setup";
            setupProfile.runtimePatterns = System.Array.Empty<RuntimePatternDefinition>();
            mode.setupProfile = setupProfile;
            session.defaultGameMode = mode;
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.FirstBlockingStep.Label, Is.EqualTo("Choose Capabilities"));
            Assert.That(report.GetStep(PyralisSetupFlowStepId.AddRuntimePatterns).Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_PawnRequiredSetup_RequiresParticipantPawnAndSpawnPoints()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.GetStep("Assign Participant Pawn").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(report.GetStep("Assign Spawn Points").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_PawnRequiredSetup_RejectsPawnDefinitionWithoutPrefab()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep participantPawn = report.GetStep("Assign Participant Pawn");

            Assert.That(participantPawn.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(participantPawn.Message, Does.Contain("pawn prefab"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_PawnRequiredSetup_RejectsPawnPrefabWithoutPawnRoot()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            GameObject prefab = new GameObject("Pawn Prefab Missing Root");
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep participantPawn = report.GetStep("Assign Participant Pawn");

            Assert.That(participantPawn.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(participantPawn.Message, Does.Contain("PawnRoot"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_PawnRequiredSetup_BlocksMissingInputActionsBeforePlayMode()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            GameObject spawn = new GameObject("Spawn Point");
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<TestPawnMotor>();
            prefab.AddComponent<TestPawnPresentation>();
            prefab.AddComponent<TestPawnInput>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character-input",
                "Realtime Character Input",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            inputProfile.name = "Input Profile Without Actions";
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.displayName = "Player One";
            participant.defaultPawn = pawn;
            participant.inputProfile = inputProfile;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.maxParticipants = 1;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            CameraRigProfile cameraRigProfile = AddOrthographicCameraRig(bootstrap, mode, out GameObject cameraRoot);

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
            spawnPoints.arraySize = 1;
            spawnPoints.GetArrayElementAtIndex(0).objectReferenceValue = spawn.transform;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            PyralisSetupFlowReport setupFlow = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisAuthoringRouteReport routeReport = PyralisAuthoringRouteReport.Build(bootstrap);

            Assert.That(setupFlow.GetStep("Assign Input Profile").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(setupFlow.GetStep("Assign Input Profile").Message, Does.Contain("assign Actions"));
            Assert.That(routeReport.NextStep, Does.Contain("assign Actions"));
            Assert.That(routeReport.NextStep, Does.Not.Contain("Enter Play Mode"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(cameraRigProfile);
            Object.DestroyImmediate(cameraRoot);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(spawn);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_NoPawnSetup_DoesNotRequirePawnOrSpawnPoints()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Board Card Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Tabletop Setup";
            setupProfile.runtimePatterns = new[] { tabletop };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.GetStep("Assign Participant Pawn").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Optional));
            Assert.That(report.GetStep("Assign Spawn Points").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Optional));
            Assert.That(report.GetStep("Tabletop Runtime Contract").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(report.GetStep("Tabletop Runtime Contract").Message, Does.Contain("BoardDefinition"));
            Assert.That(report.GetStep("Tabletop Runtime Contract").Message, Does.Contain("TurnOrderDefinition"));
            Assert.That(report.GetStep("Assign Tabletop Selection Surface").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(report.GetStep("Assign Tabletop Selection Surface").Message, Does.Contain("TabletopBoardGridPresenter"));
            Assert.That(report.GetStep("Assign Tabletop Selection Surface").Message, Does.Contain("selection/input bridge"));
            Assert.That(report.RequiredIssueCount, Is.EqualTo(1));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(tabletop);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringOverviewModel_NoPawnSetup_IsReadyWithRecommendedTabletopNextSteps()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Board Card Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Tabletop Setup";
            setupProfile.runtimePatterns = new[] { tabletop };
            BoardDefinition boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
            TurnOrderDefinition turnOrderDefinition = ScriptableObject.CreateInstance<TurnOrderDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            mode.boardDefinition = boardDefinition;
            mode.turnOrderDefinition = turnOrderDefinition;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model.RouteName, Is.EqualTo("Tabletop route"));
            Assert.That(model.ReadyToPressPlay, Is.True);
            Assert.That(model.DoNow, Is.Empty);
            Assert.That(model.DoSoon.Count, Is.GreaterThan(0));
            Assert.That(model.DoSoon.Any(issue => issue.Message.Contains("board") || issue.Message.Contains("Tabletop")), Is.True);
            Assert.That(model.Later.Count, Is.GreaterThan(0));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(turnOrderDefinition);
            Object.DestroyImmediate(boardDefinition);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(tabletop);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringOverviewModel_PawnSetupMissingPawn_IsNotReadyAndNamesRequiredFix()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.displayName = "Player One";
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model.RouteName, Is.EqualTo("Pawn-backed route"));
            Assert.That(model.ReadyToPressPlay, Is.False);
            Assert.That(model.DoNow.Count, Is.GreaterThan(0));
            Assert.That(model.DoNow.Select(issue => issue.Message), Has.Some.Contains("PawnDefinition"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_InvalidRuntimePattern_ReportsMissingPatterns()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition invalidPattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Invalid Setup";
            setupProfile.runtimePatterns = new[] { invalidPattern };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.FirstBlockingStep.Label, Is.EqualTo("Choose Capabilities"));
            Assert.That(report.GetStep(PyralisSetupFlowStepId.AddRuntimePatterns).Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(report.GetStep(PyralisSetupFlowStepId.AddRuntimePatterns).Message, Does.Contain("validation issues"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(invalidPattern);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_PawnRequiredSetup_RejectsPawnPrefabWithoutRuntimeModules()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            GameObject prefab = new GameObject("Pawn Prefab Missing Runtime Modules");
            prefab.AddComponent<PawnRoot>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport missingMotorReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(missingMotorReport.GetStep("Assign Participant Pawn").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(missingMotorReport.GetStep("Assign Participant Pawn").Message, Does.Contain("IPawnMotor"));

            prefab.AddComponent<TestPawnMotor>();
            PyralisSetupFlowReport missingPresentationReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(missingPresentationReport.GetStep("Assign Participant Pawn").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(missingPresentationReport.GetStep("Assign Participant Pawn").Message, Does.Contain("IPawnPresentationModule"));

            prefab.AddComponent<TestPawnPresentation>();
            prefab.AddComponent<TestPawnInput>();
            PyralisSetupFlowReport readyReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel overview = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(readyReport.GetStep("Assign Participant Pawn").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Ready));
            Assert.That(readyReport.GetStep("Tune Pawn Visuals And Collision").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(readyReport.GetStep("Tune Pawn Visuals And Collision").Message, Does.Contain("Collider2D"));
            Assert.That(readyReport.GetStep("Tune Pawn Visuals And Collision").StepId, Is.EqualTo(PyralisSetupFlowStepId.TunePawnVisualsAndCollision));
            Assert.That(readyReport.GetStep("Tune Pawn Visuals And Collision").WorkIntent, Is.EqualTo(PyralisSetupFlowWorkIntent.ProofEnhancer));
            Assert.That(readyReport.GetStep("Tune Movement And Input Feel").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(readyReport.GetStep("Tune Movement And Input Feel").Message, Does.Contain("InputProfile"));
            Assert.That(readyReport.GetStep("Tune Movement And Input Feel").NativeAction.HasValue, Is.True);
            Assert.That(overview.DoSoon.First(issue => issue.Label == "Tune Pawn Visuals And Collision").NativeActionGuidance, Does.Contain("Collider2D shape/size"));
            Assert.That(overview.DoSoon.First(issue => issue.Label == "Tune Movement And Input Feel").NativeActionGuidance, Does.Contain("movement speed"));
            Assert.That(overview.DoSoon.First(issue => issue.Label == "Tune Movement And Input Feel").WorkIntent, Is.EqualTo(PyralisSetupFlowWorkIntent.ProofEnhancer));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_SceneReadiness_ReportsPawnPrefabRuntimeGaps()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            GameObject prefab = new GameObject("Pawn Prefab Missing Readiness");
            prefab.AddComponent<PawnRoot>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep readiness = report.GetStep("Scene And Prefab Readiness");

            Assert.That(readiness.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(readiness.Message, Does.Contain("IPawnMotor"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupGraphProjection_MapsSceneReadinessIssuesToValidationRows()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            GameObject prefab = new GameObject("Pawn Prefab Missing Graph Validation");
            prefab.AddComponent<PawnRoot>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            IReadOnlyList<PyralisAuthoringValidationGraphRow> requiredRows =
                PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Blocked);

            Assert.That(requiredRows.Count, Is.GreaterThan(0));
            Assert.That(requiredRows.Any(row => row.NodeId.StartsWith("scenereadiness.", System.StringComparison.Ordinal)), Is.True);
            Assert.That(requiredRows.Any(row => row.Message.Contains("IPawnMotor")), Is.True);
            Assert.That(requiredRows.Any(row => row.NativeAction.Contains("prefab root")), Is.True);

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSceneReadiness_PawnVisualWithoutCollider_IsProofEnhancerAndChecklistItem()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            GameObject prefab = new GameObject("Visible Pawn Without Collider");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<TestPawnMotor>();
            prefab.AddComponent<TestPawnPresentation>();
            prefab.AddComponent<TestPawnInput>();
            prefab.AddComponent<MeshRenderer>();
            Object inputActions = CreateInputActionsWithMove();
            InputProfile inputProfile = CreateMoveInputProfile(inputActions);
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            participant.inputProfile = inputProfile;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSceneReadinessReport readiness = PyralisSceneReadinessValidator.BuildReport(bootstrap);
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel overview = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(readiness.IsReady, Is.True);
            Assert.That(readiness.GetIssues(PyralisSceneReadinessSeverity.ProofEnhancer).Any(issue => issue.Category == PyralisSceneReadinessCategory.Physics), Is.True);
            Assert.That(readiness.ProofEnhancerSummary, Does.Contain("visible renderers but no Collider"));
            PyralisAuthoringPlayModeChecklistItem physics = overview.PlayModeChecklist.First(item => item.Label == "Physics feel");
            Assert.That(physics.Ready, Is.True);
            Assert.That(physics.Detail, Does.Contain("visible renderers but no Collider"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(inputActions);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_SceneReadiness_ReportsProjectilePrefabRuntimeBodyGaps()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-projectile",
                "Realtime Projectile",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Projectile Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            GameObject pawnPrefab = new GameObject("Ready Pawn");
            pawnPrefab.AddComponent<PawnRoot>();
            pawnPrefab.AddComponent<TestPawnMotor>();
            pawnPrefab.AddComponent<TestPawnPresentation>();
            pawnPrefab.AddComponent<TestPawnInput>();
            GameObject projectilePrefab = new GameObject("Projectile Missing Body");
            projectilePrefab.AddComponent<Rigidbody2D>();
            ProjectileDefinition projectile = ScriptableObject.CreateInstance<ProjectileDefinition>();
            projectile.deliveryMode = ProjectileDeliveryMode.ProjectilePrefab;
            projectile.projectilePrefab = projectilePrefab;
            WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
            weapon.weaponType = WeaponType.Ranged;
            weapon.projectileDefinition = projectile;
            PawnCombatProfile combatProfile = ScriptableObject.CreateInstance<PawnCombatProfile>();
            combatProfile.attackWeapon = weapon;
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = pawnPrefab;
            pawn.combatProfile = combatProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep readiness = report.GetStep("Scene And Prefab Readiness");

            Assert.That(readiness.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(readiness.Message, Does.Contain("Projectile"));
            Assert.That(readiness.Message, Does.Contain("Projectile2D"));

            projectilePrefab.AddComponent<TestProjectileRuntimeBody>();
            PyralisSetupFlowReport readyReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(readyReport.GetStep("Scene And Prefab Readiness").Message, Does.Not.Contain("Projectile2D"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(combatProfile);
            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(projectile);
            Object.DestroyImmediate(projectilePrefab);
            Object.DestroyImmediate(pawnPrefab);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_SceneReadiness_NetworkedSessionRequiresNetworkPawnSurface()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-networked",
                "Realtime Networked",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Networked Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            GameObject prefab = new GameObject("Network Pawn Missing NetworkObject");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<TestPawnMotor>();
            prefab.AddComponent<TestPawnPresentation>();
            prefab.AddComponent<TestPawnInput>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.networkMode = GameplayNetworkMode.NetcodeHost;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep readiness = report.GetStep("Scene And Prefab Readiness");

            Assert.That(readiness.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(readiness.Message, Does.Contain("NetworkManager"));
            Assert.That(readiness.Message, Does.Contain("NetworkObject"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_ScoringPattern_RequiresSceneScoreService()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition scoring = CreateRuntimePattern(
                "pattern.scoring-objectives",
                "Scoring Objectives",
                RuntimeCapabilityFamily.ScoringObjectives,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Scoring Setup";
            setupProfile.runtimePatterns = new[] { scoring };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            mode.enableScore = true;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport missingReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(missingReport.GetStep("Assign Score Service").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(missingReport.GetStep("Assign Score Service").Message, Does.Contain("ISessionScoreService"));

            ParticipantScoreService scoreService = root.AddComponent<ParticipantScoreService>();
            PyralisSetupFlowReport readyReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(readyReport.GetStep("Assign Score Service").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Ready));

            Object.DestroyImmediate(scoreService);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(scoring);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_ScoringPattern_RequiresModeScoringEnabledEvenWithScoreService()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            ParticipantScoreService scoreService = root.AddComponent<ParticipantScoreService>();
            RuntimePatternDefinition scoring = CreateRuntimePattern(
                "pattern.scoring-objectives",
                "Scoring Objectives",
                RuntimeCapabilityFamily.ScoringObjectives,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Scoring Setup";
            setupProfile.runtimePatterns = new[] { scoring };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            mode.enableScore = false;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.GetStep("Assign Score Service").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Ready));
            Assert.That(report.GetStep("Enable Scoring Route").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(report.FirstBlockingStep.Label, Is.EqualTo("Enable Scoring Route"));

            Object.DestroyImmediate(scoreService);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(scoring);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_ActionSelectionPattern_GuidesCanvasAndPresenterSetup()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition actionSelection = CreateRuntimePattern(
                "pattern.action-selection",
                "Action Selection",
                RuntimeCapabilityFamily.ActionTargeting,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Action Selection Setup";
            setupProfile.runtimePatterns = new[] { actionSelection };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport missingUiReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep missingUiStep = missingUiReport.GetStep("Assign HUD / UI Surface");

            Assert.That(missingUiStep.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(missingUiStep.Message, Does.Contain("action selection"));
            Assert.That(missingUiStep.Message, Does.Contain("one selectable action"));

            Canvas canvas = root.AddComponent<Canvas>();
            PyralisSetupFlowReport canvasOnlyReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(canvasOnlyReport.GetStep("Assign HUD / UI Surface").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(canvasOnlyReport.GetStep("Assign HUD / UI Surface").Message, Does.Contain("Canvas"));
            Assert.That(canvasOnlyReport.GetStep("Assign HUD / UI Surface").Message, Does.Contain("no known Pyralis HUD/menu presenter"));

            ParticipantFeedbackHudPresenter feedbackPresenter = root.AddComponent<ParticipantFeedbackHudPresenter>();
            PyralisSetupFlowReport readyUiReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(readyUiReport.GetStep("Assign HUD / UI Surface").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Ready));
            Assert.That(readyUiReport.GetStep("Assign HUD / UI Surface").Message, Does.Contain("labels"));
            Assert.That(canvas, Is.Not.Null);

            Object.DestroyImmediate(feedbackPresenter);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(actionSelection);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringCapabilityGuidance_PawnCombatRoute_ExplainsBrawlerSetupAndCustomization()
        {
            RuntimePatternDefinition pawn = CreateRuntimePattern(
                "pattern.character-pawn",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            RuntimePatternDefinition combat = CreateRuntimePattern(
                "pattern.combat",
                "Combat",
                RuntimeCapabilityFamily.Combat,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Brawler Setup";
            setupProfile.runtimePatterns = new[] { pawn, combat };

            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(setupProfile);
            PyralisAuthoringRouteProof proof = PyralisAuthoringRouteProof.Build(route);
            PyralisAuthoringFeatureRow pawnRow = PyralisAuthoringCapabilityGuidance.BuildSelectedRow(pawn);
            PyralisAuthoringFeatureRow combatRow = PyralisAuthoringCapabilityGuidance.BuildSelectedRow(combat);
            List<PyralisAuthoringFeatureRow> recommended = PyralisAuthoringCapabilityGuidance.BuildRecommendedRows(route);
            List<PyralisAuthoringFeatureRow> environmentGuidance = PyralisAuthoringCapabilityGuidance.BuildEnvironmentRows(route);

            Assert.That(PyralisAuthoringCapabilityGuidance.GetRouteIntent(route, 2), Does.Contain("brawler"));
            Assert.That(proof.Label, Is.EqualTo("1P Pawn Movement Proof"));
            Assert.That(proof.Guidance, Does.Contain("before adding combat"));
            Assert.That(proof.FirstUnityFocus, Does.Contain("PawnRoot"));
            Assert.That(pawnRow.GameplayEffect, Does.Contain("actor bodies"));
            Assert.That(pawnRow.Customization, Does.Contain("speed"));
            Assert.That(combatRow.UnitySetup, Does.Contain("CombatActionDefinition"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("Animation / Presentation"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("HUD / Menus / Feedback"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("Movement / Traversal / Respawn"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("Feature Modules / Pickups / Interaction"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("Health / Hitboxes / Feedback"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("Enemies / Hazards / Encounter Zones"));
            Assert.That(environmentGuidance.Select(row => row.Feature), Does.Contain("Walkable Ground And Collision"));
            PyralisAuthoringFeatureRow environmentRow = environmentGuidance[0];
            Assert.That(environmentRow.GameplayEffect, Does.Contain("plain Unity objects"));
            Assert.That(environmentRow.GameplayEffect, Does.Contain("backdrops"));
            Assert.That(environmentRow.UnitySetup, Does.Contain("flat sprite/PNG backdrops"));
            Assert.That(environmentRow.UnitySetup, Does.Contain("Canvas backgrounds"));
            Assert.That(environmentRow.Customization, Does.Contain("collision layers"));
            Assert.That(environmentRow.Customization, Does.Contain("procedural"));

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(combat);
            Object.DestroyImmediate(pawn);
        }

        [Test]
        public void PyralisAuthoringCapabilityGuidance_TabletopRoute_KeepsPawnEmptyAndRecommendsSelectionSurface()
        {
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Tabletop Setup";
            setupProfile.runtimePatterns = new[] { tabletop };

            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(setupProfile);
            PyralisAuthoringRouteProof proof = PyralisAuthoringRouteProof.Build(route);
            PyralisAuthoringFeatureRow tabletopRow = PyralisAuthoringCapabilityGuidance.BuildSelectedRow(tabletop);
            List<PyralisAuthoringFeatureRow> recommended = PyralisAuthoringCapabilityGuidance.BuildRecommendedRows(route);
            List<PyralisAuthoringFeatureRow> environmentGuidance = PyralisAuthoringCapabilityGuidance.BuildEnvironmentRows(route);

            Assert.That(PyralisAuthoringCapabilityGuidance.GetRouteIntent(route, 1), Does.Contain("tabletop"));
            Assert.That(proof.Label, Is.EqualTo("Board/Card Action Proof"));
            Assert.That(proof.Guidance, Does.Contain("rules-backed selection"));
            Assert.That(proof.FirstUnityFocus, Does.Contain("Keep pawn fields empty"));
            Assert.That(tabletopRow.UnitySetup, Does.Contain("Start with no PawnDefinition"));
            Assert.That(tabletopRow.Customization, Does.Contain("turn order"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("Action Selection / Menus"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("Camera / Cursor Control"));
            Assert.That(recommended.Select(row => row.Feature), Does.Contain("Menus / Settings / Scene Flow"));
            Assert.That(environmentGuidance.Select(row => row.Feature), Does.Contain("Selectable Spaces"));
            Assert.That(environmentGuidance[0].UnitySetup, Does.Contain("Playfield Root"));

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(tabletop);
        }

        [Test]
        public void PyralisAuthoringCapabilityGuidance_PlatformCore_NamesCanonicalBootstrapAndSessionChain()
        {
            RuntimePatternDefinition platform = CreateRuntimePattern(
                "pattern.platform-core",
                "Platform Core",
                RuntimeCapabilityFamily.PlatformCore,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Custom);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Core Setup";
            setupProfile.runtimePatterns = new[] { platform };

            PyralisAuthoringFeatureRow platformRow = PyralisAuthoringCapabilityGuidance.BuildSelectedRow(platform);

            Assert.That(platformRow.Feature, Is.EqualTo("Core Session Setup"));
            Assert.That(platformRow.UnitySetup, Does.Contain("GameplaySessionBootstrap"));
            Assert.That(platformRow.UnitySetup, Does.Contain("SessionDefinition"));
            Assert.That(platformRow.Customization, Does.Contain("settings profile"));

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(platform);
        }

        [Test]
        public void PyralisAuthoringRouteDescriptor_TabletopActionRoute_CentralizesRouteFacts()
        {
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat);
            RuntimePatternDefinition action = CreateRuntimePattern(
                "pattern.action",
                "Action Selection",
                RuntimeCapabilityFamily.ActionTargeting,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { tabletop, action };

            PyralisAuthoringRouteDescriptor descriptor = PyralisAuthoringRouteDescriptor.Build(setupProfile);

            Assert.That(descriptor.HasValidPatterns, Is.True);
            Assert.That(descriptor.HasTabletop, Is.True);
            Assert.That(descriptor.HasActions, Is.True);
            Assert.That(descriptor.RequiresPawn, Is.False);
            Assert.That(descriptor.UsesUi, Is.True);
            Assert.That(descriptor.UsesActionOrTabletop, Is.True);
            Assert.That(descriptor.RouteName, Is.EqualTo("Tabletop + 1 capability route"));
            Assert.That(descriptor.RouteFacts.Select(fact => fact.Label), Is.EqualTo(new[] { "Tabletop", "Action Selection" }));
            Assert.That(descriptor.PrimaryRouteFact.Capability, Is.EqualTo(PyralisAuthoringRouteCapability.Tabletop));

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(action);
            Object.DestroyImmediate(tabletop);
        }

        [TestCase(RuntimeCapabilityFamily.GunsProjectiles, "Projectile Proof", "one shot")]
        [TestCase(RuntimeCapabilityFamily.Combat, "Combat Proof", "one attack")]
        [TestCase(RuntimeCapabilityFamily.ActionTargeting, "Action Selection Proof", "one command")]
        [TestCase(RuntimeCapabilityFamily.CameraInput, "Camera/Cursor Proof", "cursor")]
        [TestCase(RuntimeCapabilityFamily.ScoringObjectives, "Scoring Proof", "score/objective")]
        [TestCase(RuntimeCapabilityFamily.ProceduralGeneration, "Generated Content Proof", "Generate one output")]
        [TestCase(RuntimeCapabilityFamily.Networking, "Network Ownership Proof", "local route first")]
        public void PyralisAuthoringRouteProof_NonPawnFamilies_NameRouteSpecificFirstProof(
            RuntimeCapabilityFamily family,
            string expectedLabel,
            string expectedGuidance)
        {
            RuntimePatternDefinition pattern = CreateRuntimePattern(
                "pattern." + family,
                family.ToString(),
                family,
                family == RuntimeCapabilityFamily.CameraInput
                    ? ParticipantEmbodimentRequirement.NonPawnSurfaceRequired
                    : ParticipantEmbodimentRequirement.OptionalPawn,
                family == RuntimeCapabilityFamily.CameraInput
                    ? RuntimeControlSurface.Camera
                    : RuntimeControlSurface.Custom);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { pattern };

            PyralisAuthoringRouteProof proof = PyralisAuthoringRouteProof.Build(PyralisAuthoringRouteDescriptor.Build(setupProfile));

            Assert.That(proof.Label, Is.EqualTo(expectedLabel));
            Assert.That(proof.Guidance, Does.Contain(expectedGuidance));
            Assert.That(proof.SetupSurface, Is.Not.Empty);
            Assert.That(proof.SuccessCriteria, Is.Not.Empty);
            Assert.That(proof.DeferUntilAfter, Does.Contain("Defer"));
            Assert.That(proof.FirstUnityFocus, Does.Contain("First Unity focus"));
            Assert.That(proof.ProofChain, Has.Length.EqualTo(1));
            Assert.That(proof.ProofChainSummary, Does.Contain(proof.ProofChain[0].Label));

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void PyralisAuthoringRouteProof_PawnWithLaterSystems_StillStartsWithMovement()
        {
            RuntimePatternDefinition pawn = CreateRuntimePattern(
                "pattern.character-pawn",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            RuntimePatternDefinition projectile = CreateRuntimePattern(
                "pattern.projectile",
                "Projectiles",
                RuntimeCapabilityFamily.GunsProjectiles,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn);
            RuntimePatternDefinition networking = CreateRuntimePattern(
                "pattern.network",
                "Networking",
                RuntimeCapabilityFamily.Networking,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { pawn, projectile, networking };

            PyralisAuthoringRouteProof proof = PyralisAuthoringRouteProof.Build(PyralisAuthoringRouteDescriptor.Build(setupProfile));

            Assert.That(proof.Label, Is.EqualTo("1P Pawn Movement Proof"));
            Assert.That(proof.Guidance, Does.Contain("before adding combat"));
            Assert.That(proof.DeferUntilAfter, Does.Contain("projectiles"));
            Assert.That(proof.DeferUntilAfter, Does.Contain("networking"));
            Assert.That(proof.ProofChain.Select(step => step.Label), Is.EqualTo(new[]
            {
                "Local pawn movement",
                "Projectile resolution",
                "Network ownership"
            }));
            Assert.That(proof.ProofChainSummary, Is.EqualTo("Local pawn movement -> Projectile resolution -> Network ownership"));

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(networking);
            Object.DestroyImmediate(projectile);
            Object.DestroyImmediate(pawn);
        }

        [Test]
        public void PyralisAuthoringSceneSurfaceSnapshot_TabletopActionRoute_DetectsUnitySceneSurfaces()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            GameObject uiRoot = new GameObject("UI Root");
            uiRoot.AddComponent<Canvas>();
            GameObject eventSystemRoot = new GameObject("EventSystem");
            eventSystemRoot.AddComponent<EventSystem>();
            GameObject ground = new GameObject("Board Surface");
            ground.AddComponent<BoxCollider2D>();
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat);
            RuntimePatternDefinition action = CreateRuntimePattern(
                "pattern.action",
                "Action Selection",
                RuntimeCapabilityFamily.ActionTargeting,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { tabletop, action };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap);

            PyralisAuthoringSceneSurfaceRow environment = snapshot.Rows.First(row => row.Surface == "Environment / Playfield");
            PyralisAuthoringSceneSurfaceRow ui = snapshot.Rows.First(row => row.Surface == "UI / HUD / Menus");
            PyralisAuthoringSceneSurfaceRow selection = snapshot.Rows.First(row => row.Surface == "Board / Action Selection");
            Assert.That(environment.Present, Is.True);
            Assert.That(environment.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.CandidateDetected));
            Assert.That(environment.Current, Does.Contain("2D collider"));
            Assert.That(ui.Present, Is.True);
            Assert.That(ui.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.CandidateDetected));
            Assert.That(ui.Current, Does.Contain("Canvas"));
            Assert.That(ui.Current, Does.Contain("EventSystem"));
            Assert.That(selection.Present, Is.True);
            Assert.That(selection.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.CandidateDetected));
            Assert.That(selection.NextFix, Does.Contain("TabletopBoardGridPresenter"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(action);
            Object.DestroyImmediate(tabletop);
            Object.DestroyImmediate(ground);
            Object.DestroyImmediate(eventSystemRoot);
            Object.DestroyImmediate(uiRoot);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringSceneSurfaceSnapshot_PlatformCore_DoesNotMarkIrrelevantRowsReady()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition platform = CreateRuntimePattern(
                "pattern.platform-core",
                "Platform Core",
                RuntimeCapabilityFamily.PlatformCore,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Custom);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { platform };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap);

            PyralisAuthoringSceneSurfaceRow camera = snapshot.Rows.First(row => row.Surface == "Camera / Bounds");
            PyralisAuthoringSceneSurfaceRow encounters = snapshot.Rows.First(row => row.Surface == "Pickups / Hazards / Enemies");
            Assert.That(camera.Recommended, Is.False);
            Assert.That(camera.SupportsFirstProofAttempt, Is.True);
            Assert.That(encounters.Recommended, Is.False);
            Assert.That(encounters.Present, Is.False);
            Assert.That(encounters.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.NotRelevant));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(platform);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringSceneSurfaceSnapshot_LinkedSpawnPoint_DoesNotSatisfyPlayableSurface()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            GameObject spawn = new GameObject("P1 Spawn");
            RuntimePatternDefinition pawn = CreateRuntimePattern(
                "pattern.pawn.linked-spawn",
                "Pawn",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { pawn };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            PawnMovementProfile movement = ScriptableObject.CreateInstance<PawnMovementProfile>();
            movement.movementMode = MovementMode.TwoD;
            movement.use2DPhysics = true;
            movement.allow2DJump = true;
            PawnDefinition pawnDefinition = ScriptableObject.CreateInstance<PawnDefinition>();
            pawnDefinition.movementProfile = movement;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawnDefinition;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
            spawnPoints.arraySize = 1;
            spawnPoints.GetArrayElementAtIndex(0).objectReferenceValue = spawn.transform;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap);

            PyralisAuthoringSceneSurfaceRow environment = snapshot.Rows.First(row => row.Surface == "Environment / Playfield");
            Assert.That(environment.Present, Is.False);
            Assert.That(environment.SupportsFirstProofAttempt, Is.False);
            Assert.That(environment.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.Missing));
            Assert.That(environment.Current, Does.Contain("spawn point"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawnDefinition);
            Object.DestroyImmediate(movement);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(spawn);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringSceneSurfaceSnapshot_TopDown2DSpawnPoint_DoesNotRequireSideViewGround()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            GameObject spawn = new GameObject("P1 Spawn");
            RuntimePatternDefinition pawn = CreateRuntimePattern(
                "pattern.pawn.topdown-spawn",
                "Pawn",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { pawn };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            PawnMovementProfile movement = ScriptableObject.CreateInstance<PawnMovementProfile>();
            movement.movementMode = MovementMode.TwoD;
            movement.use2DPhysics = true;
            movement.allow2DJump = false;
            PawnDefinition pawnDefinition = ScriptableObject.CreateInstance<PawnDefinition>();
            pawnDefinition.movementProfile = movement;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawnDefinition;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
            spawnPoints.arraySize = 1;
            spawnPoints.GetArrayElementAtIndex(0).objectReferenceValue = spawn.transform;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap);

            PyralisAuthoringSceneSurfaceRow environment = snapshot.Rows.First(row => row.Surface == "Environment / Playfield");
            Assert.That(environment.Present, Is.True);
            Assert.That(environment.SupportsFirstProofAttempt, Is.True);
            Assert.That(environment.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.LinkedToActiveSetup));
            Assert.That(environment.Current, Does.Contain("spawn point"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawnDefinition);
            Object.DestroyImmediate(movement);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(spawn);
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
        public void PyralisAuthoringFactRegistry_SetupFlowFacts_CoverStableStepIds()
        {
            Assert.That(PyralisAuthoringFactRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            foreach (PyralisSetupFlowStepId stepId in System.Enum.GetValues(typeof(PyralisSetupFlowStepId)))
            {
                if (stepId == PyralisSetupFlowStepId.Unknown)
                    continue;

                string stableId = PyralisSetupFlowGuidance.GetStableId(stepId);
                Assert.That(stableId, Is.Not.Empty, stepId.ToString());

                PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find(stableId);
                Assert.That(fact, Is.Not.Null, stableId);
                Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.SetupNode));
                Assert.That(fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.SetupFlow));
                Assert.That(fact.WorkIntent, Is.EqualTo(PyralisSetupFlowGuidance.GetDefaultWorkIntent(stepId).ToString()));
            }
        }

        [Test]
        public void PyralisAuthoringFactRegistry_TwoDPawnMovement_RelatesCapabilityToSetupNodes()
        {
            PyralisAuthoringFact capability = PyralisAuthoringFactRegistry.Find("capability.2d-pawn-movement");
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
                PyralisAuthoringFact setupFact = PyralisAuthoringFactRegistry.Find(stableId);
                Assert.That(setupFact, Is.Not.Null, stableId);
                Assert.That(setupFact.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"), stableId);
                Assert.That(setupFact.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"), stableId);
                Assert.That(setupFact.Summary, Is.Not.Empty, stableId);
            }
        }

        [Test]
        public void PyralisAuthoringFactRegistry_OnePlayerPawnProof_ConnectsCapabilitySetupAndProof()
        {
            PyralisAuthoringFact proof = PyralisAuthoringFactRegistry.Find("proof.1p-pawn-movement");
            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.Kind, Is.EqualTo(PyralisAuthoringFactKind.Proof));
            Assert.That(proof.WorkIntent, Is.EqualTo("FirstProof"));
            Assert.That(proof.FirstProof, Does.Contain("One participant spawns one pawn"));
            Assert.That(proof.RequiredDefinitions, Does.Contain("PawnDefinition"));
            Assert.That(proof.RequiredUnitySurfaces, Does.Contain("PawnRoot"));
            Assert.That(proof.NativeActions.Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(proof.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.PlayMode));
            Assert.That(proof.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));
            Assert.That(proof.RelatedStableIds, Does.Contain("setup.assign-participant-pawn"));
            Assert.That(proof.RelatedStableIds, Does.Contain("setup.assign-input-profile"));
            Assert.That(proof.RelatedStableIds, Does.Contain("setup.assign-spawn-points"));
        }

        [Test]
        public void PyralisAuthoringFactRegistry_InspectorHandoffFacts_LinkNativeInspectorFieldsToSetup()
        {
            PyralisAuthoringFact bootstrapSession = PyralisAuthoringFactRegistry.Find("inspector.gameplay-session-bootstrap.session-definition");
            Assert.That(bootstrapSession, Is.Not.Null);
            Assert.That(bootstrapSession.Kind, Is.EqualTo(PyralisAuthoringFactKind.AssignmentField));
            Assert.That(bootstrapSession.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.InspectorGuide));
            Assert.That(bootstrapSession.NativeActions.Length, Is.EqualTo(1));
            Assert.That(bootstrapSession.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));
            Assert.That(bootstrapSession.AssignmentFields, Does.Contain("GameplaySessionBootstrap.sessionDefinition -> SessionDefinition"));
            Assert.That(bootstrapSession.RelatedStableIds, Does.Contain("setup.assign-session-definition"));

            PyralisAuthoringFact pawnPrefab = PyralisAuthoringFactRegistry.Find("inspector.pawn-definition.pawn-prefab");
            Assert.That(pawnPrefab, Is.Not.Null);
            Assert.That(pawnPrefab.RelatedStableIds, Does.Contain("setup.assign-participant-pawn"));
            Assert.That(pawnPrefab.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));
            Assert.That(pawnPrefab.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));

            PyralisAuthoringFact inputNames = PyralisAuthoringFactRegistry.Find("inspector.input-profile.gameplay-action-names");
            Assert.That(inputNames, Is.Not.Null);
            Assert.That(inputNames.Kind, Is.EqualTo(PyralisAuthoringFactKind.CustomizationMoment));
            Assert.That(inputNames.WorkIntent, Is.EqualTo("ProofEnhancer"));
            Assert.That(inputNames.CustomizationMoments[0], Does.Contain("InputProfile Gameplay Action Names"));
            Assert.That(inputNames.NativeActions[0].Verb, Is.EqualTo("Customize"));

            PyralisAuthoringFact boardRules = PyralisAuthoringFactRegistry.Find("inspector.game-mode-definition.board-and-turn-rules");
            Assert.That(boardRules, Is.Not.Null);
            Assert.That(boardRules.AssignmentFields[0], Does.Contain("GameModeDefinition.boardDefinition"));
            Assert.That(boardRules.RelatedStableIds, Does.Contain("route.tabletop-card"));
            Assert.That(boardRules.RelatedStableIds, Does.Contain("proof.board-card-action"));

            PyralisAuthoringFact cameraFields = PyralisAuthoringFactRegistry.Find("inspector.cinemachine-camera-rig-controller.camera-fields");
            Assert.That(cameraFields, Is.Not.Null);
            Assert.That(cameraFields.AssignmentFields[0], Does.Contain("CinemachineCameraRigController.cameraRigProfile"));
            Assert.That(cameraFields.RelatedStableIds, Does.Contain("proof.camera-cursor-world"));

            PyralisAuthoringFact featureFields = PyralisAuthoringFactRegistry.Find("inspector.feature-module-definition.profile-runtime-network");
            Assert.That(featureFields, Is.Not.Null);
            Assert.That(featureFields.AssignmentFields[0], Does.Contain("FeatureModuleDefinition.profileAsset"));
            Assert.That(featureFields.RelatedStableIds, Does.Contain("route.custom-object-feature"));
            Assert.That(featureFields.RelatedStableIds, Does.Contain("proof.network-ownership"));

            PyralisAuthoringFact cameraTuning = PyralisAuthoringFactRegistry.Find("inspector.camera-rig-profile.framing-fields");
            Assert.That(cameraTuning, Is.Not.Null);
            Assert.That(cameraTuning.Kind, Is.EqualTo(PyralisAuthoringFactKind.CustomizationMoment));
            Assert.That(cameraTuning.NativeActions[0].Verb, Is.EqualTo("Customize"));
            Assert.That(cameraTuning.RelatedStableIds, Does.Contain("capability.camera-follow-bounds"));
        }

        [Test]
        public void PyralisAuthoringFactRegistry_ConventionFacts_ExposeUnityMetadataAndSerializedFields()
        {
            Assert.That(PyralisAuthoringFactRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            PyralisAuthoringFact sessionCreate = PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.session-definition");
            Assert.That(sessionCreate, Is.Not.Null);
            Assert.That(sessionCreate.Kind, Is.EqualTo(PyralisAuthoringFactKind.Definition));
            Assert.That(sessionCreate.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Reflection));
            Assert.That(sessionCreate.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
            Assert.That(sessionCreate.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.ProjectWindow));
            Assert.That(sessionCreate.NativeActions[0].Target, Does.Contain("Assets/Create/NeonBlack/Definitions/Session Definition"));
            Assert.That(sessionCreate.RelatedStableIds, Does.Contain("setup.assign-session-definition"));

            PyralisAuthoringFact inputCreate = PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.input-profile");
            Assert.That(inputCreate, Is.Not.Null);
            Assert.That(inputCreate.Kind, Is.EqualTo(PyralisAuthoringFactKind.Profile));
            Assert.That(inputCreate.RequiredProfiles, Does.Contain("InputProfile"));
            Assert.That(inputCreate.RelatedStableIds, Does.Contain("inspector.input-profile.gameplay-action-names"));

            PyralisAuthoringFact bootstrapComponent = PyralisAuthoringFactRegistry.Find("reflection.add-component-menu.gameplay-session-bootstrap");
            Assert.That(bootstrapComponent, Is.Not.Null);
            Assert.That(bootstrapComponent.Kind, Is.EqualTo(PyralisAuthoringFactKind.SceneComponent));
            Assert.That(bootstrapComponent.RequiredSceneComponents, Does.Contain("GameplaySessionBootstrap"));
            Assert.That(bootstrapComponent.NativeActions[0].FieldOrComponent, Does.Contain("NeonBlack/Gameplay/Setup/Gameplay Session Bootstrap"));

            PyralisAuthoringFact movementRequirements = PyralisAuthoringFactRegistry.Find("reflection.require-component.pawn-2d-movement-component");
            Assert.That(movementRequirements, Is.Not.Null);
            Assert.That(movementRequirements.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Reflection));
            Assert.That(movementRequirements.RequiredUnitySurfaces, Does.Contain("Rigidbody2D"));
            Assert.That(movementRequirements.RequiredUnitySurfaces, Does.Contain("PolygonCollider2D"));
            Assert.That(movementRequirements.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));

            PyralisAuthoringFact pawnPrefabField = PyralisAuthoringFactRegistry.Find("convention.serialized-field.pawn-definition.pawn-prefab");
            Assert.That(pawnPrefabField, Is.Not.Null);
            Assert.That(pawnPrefabField.Kind, Is.EqualTo(PyralisAuthoringFactKind.AssignmentField));
            Assert.That(pawnPrefabField.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Convention));
            Assert.That(pawnPrefabField.Confidence, Is.EqualTo(PyralisAuthoringConfidence.ConventionDerived));
            Assert.That(pawnPrefabField.AssignmentFields[0], Does.Contain("PawnDefinition.pawnPrefab"));
            Assert.That(pawnPrefabField.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));
            Assert.That(pawnPrefabField.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));

            PyralisAuthoringFact boardCreate = PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.board-definition");
            Assert.That(boardCreate, Is.Not.Null);
            Assert.That(boardCreate.Kind, Is.EqualTo(PyralisAuthoringFactKind.Definition));
            Assert.That(boardCreate.NativeActions[0].Target, Does.Contain("Assets/Create/NeonBlack/Rules/Board Definition"));
            Assert.That(boardCreate.RelatedStableIds, Does.Contain("proof.board-card-action"));

            PyralisAuthoringFact cameraProfileCreate = PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.camera-rig-profile");
            Assert.That(cameraProfileCreate, Is.Not.Null);
            Assert.That(cameraProfileCreate.Kind, Is.EqualTo(PyralisAuthoringFactKind.Profile));
            Assert.That(cameraProfileCreate.RequiredProfiles, Does.Contain("CameraRigProfile"));
            Assert.That(cameraProfileCreate.RelatedStableIds, Does.Contain("proof.camera-cursor-world"));

            PyralisAuthoringFact featureModuleCreate = PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.feature-module-definition");
            Assert.That(featureModuleCreate, Is.Not.Null);
            Assert.That(featureModuleCreate.RequiredDefinitions, Does.Contain("FeatureModuleDefinition"));
            Assert.That(featureModuleCreate.RelatedStableIds, Does.Contain("proof.custom-object-effect"));

            PyralisAuthoringFact tabletopPresenterComponent = PyralisAuthoringFactRegistry.Find("reflection.add-component-menu.tabletop-board-grid-presenter");
            Assert.That(tabletopPresenterComponent, Is.Not.Null);
            Assert.That(tabletopPresenterComponent.Kind, Is.EqualTo(PyralisAuthoringFactKind.SceneComponent));
            Assert.That(tabletopPresenterComponent.RequiredSceneComponents, Does.Contain("TabletopBoardGridPresenter"));
            Assert.That(tabletopPresenterComponent.RelatedStableIds, Does.Contain("proof.board-card-action"));

            PyralisAuthoringFact enemyAiComponent = PyralisAuthoringFactRegistry.Find("reflection.add-component-menu.enemy-ai");
            Assert.That(enemyAiComponent, Is.Not.Null);
            Assert.That(enemyAiComponent.Kind, Is.EqualTo(PyralisAuthoringFactKind.UnitySurface));
            Assert.That(enemyAiComponent.RelatedStableIds, Does.Contain("proof.npc-enemy-behavior"));

            PyralisAuthoringFact cameraRigField = PyralisAuthoringFactRegistry.Find("convention.serialized-field.cinemachine-camera-rig-controller.camera-rig-profile");
            Assert.That(cameraRigField, Is.Not.Null);
            Assert.That(cameraRigField.AssignmentFields[0], Does.Contain("CinemachineCameraRigController.cameraRigProfile"));
            Assert.That(cameraRigField.RelatedStableIds, Does.Contain("capability.camera-follow-bounds"));
        }

        [Test]
        public void PyralisAuthoringFactRegistry_ExplicitConventionFactsPreserveFactSurface()
        {
            IReadOnlyList<PyralisAuthoringFact> bridgeFacts = PyralisConventionAuthoringFacts.GetAuthoringFacts();
            IReadOnlyList<PyralisAuthoringFact> intentFacts = PyralisRouteIntentAuthoringFactProvider.GetAuthoringFacts();

            AssertFactsReachMainRegistry(bridgeFacts);
            AssertFactsReachMainRegistry(intentFacts);

            Assert.That(PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.participant-definition"), Is.Not.Null);
        }

        [Test]
        public void PyralisAuthoringFactRegistry_GameTypeIntentFacts_AreStudioWideConventionFacts()
        {
            Assert.That(PyralisAuthoringFactRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.RouteIntent).Count, Is.GreaterThanOrEqualTo(7));

            PyralisAuthoringFact sideView = PyralisAuthoringFactRegistry.Find("intent.2d-side-view-action");
            Assert.That(sideView, Is.Not.Null);
            Assert.That(sideView.Kind, Is.EqualTo(PyralisAuthoringFactKind.RouteIntent));
            Assert.That(sideView.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Convention));
            Assert.That(sideView.LaneTags, Does.Contain(RuntimeCapabilityLaneTag.Sprite2D.ToString()));
            Assert.That(sideView.GoalTags, Does.Contain("JumpTraversal"));
            Assert.That(sideView.GoalTags, Does.Contain("Input"));
            Assert.That(sideView.GoalTags, Does.Contain("AnimationPresentation"));
Assert.That(sideView.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));

            PyralisAuthoringFact brawler = PyralisAuthoringFactRegistry.Find("intent.pawn-brawler");
            Assert.That(brawler, Is.Not.Null);
            Assert.That(brawler.GoalTags, Does.Contain("Combat"));
            Assert.That(brawler.GoalTags, Does.Contain("JumpTraversal"));
            Assert.That(brawler.GoalTags, Does.Contain("Input"));
            Assert.That(brawler.GoalTags, Does.Contain("AnimationPresentation"));
Assert.That(brawler.RelatedStableIds, Does.Contain("capability.combat-projectile-proof"));

            PyralisAuthoringFact topDown = PyralisAuthoringFactRegistry.Find("intent.2d-top-down-plane");
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
        public void PyralisRuntimeCapabilityFamilyMap_CentralizesContractCapabilityProjection()
        {
            RuntimeCapabilityFamily[] brawlerFamilies = PyralisRuntimeCapabilityFamilyMap.GetFamilies(
                AuthoringCapability.Movement | AuthoringCapability.Combat | AuthoringCapability.Input | AuthoringCapability.Animation | AuthoringCapability.Camera);
            Assert.That(brawlerFamilies, Does.Contain(RuntimeCapabilityFamily.CharacterPawnGameplay));
            Assert.That(brawlerFamilies, Does.Contain(RuntimeCapabilityFamily.Combat));
            Assert.That(brawlerFamilies, Does.Contain(RuntimeCapabilityFamily.CameraInput));
            Assert.That(brawlerFamilies, Does.Contain(RuntimeCapabilityFamily.AnimationPresentation));

            RuntimeCapabilityFamily[] rangedFamilies = PyralisRuntimeCapabilityFamilyMap.GetFamilies(AuthoringCapability.RangedFlow);
            Assert.That(rangedFamilies, Does.Contain(RuntimeCapabilityFamily.GunsProjectiles));
            Assert.That(rangedFamilies, Does.Contain(RuntimeCapabilityFamily.Combat));

            RuntimeCapabilityFamily[] tabletopFamilies = PyralisRuntimeCapabilityFamilyMap.GetFamilies(
                AuthoringCapability.None,
                RuntimeCapabilityLaneTag.TabletopBoard);
            Assert.That(tabletopFamilies, Does.Contain(RuntimeCapabilityFamily.BoardCardTabletop));

            RuntimeCapabilityFamily[] cursorFamilies = PyralisRuntimeCapabilityFamilyMap.GetFamilies(
                AuthoringCapability.None,
                RuntimeCapabilityLaneTag.CameraCursor);
            Assert.That(cursorFamilies, Does.Contain(RuntimeCapabilityFamily.CameraInput));

            RuntimeCapabilityFamily[] proceduralFamilies = PyralisRuntimeCapabilityFamilyMap.GetFamilies(
                AuthoringCapability.Environment,
                RuntimeCapabilityLaneTag.Mixed,
                AuthoringWorldAxiom.InfiniteSpace);
            Assert.That(proceduralFamilies, Does.Contain(RuntimeCapabilityFamily.ProceduralGeneration));

            RuntimeCapabilityFamily[] networkFamilies = PyralisRuntimeCapabilityFamilyMap.GetFamilies(
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
                PyralisAuthoringFact registryFact = PyralisAuthoringFactRegistry.Find(directFact.StableId);

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
        public void PyralisAuthoringFactRegistry_RouteCoverageFacts_NameBroadAuthoringSurfaces()
        {
            Assert.That(PyralisAuthoringFactRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

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
                PyralisAuthoringFact route = PyralisAuthoringFactRegistry.Find(stableId);
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

            PyralisAuthoringFact pawn = PyralisAuthoringFactRegistry.Find("route.pawn-actor");
            Assert.That(pawn.RelatedStableIds, Does.Contain("capability.2d-pawn-movement"));
            Assert.That(pawn.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));
            Assert.That(pawn.RequiredDefinitions, Does.Contain("PawnDefinition"));
            Assert.That(pawn.RequiredUnitySurfaces, Does.Contain("PawnRoot"));

            PyralisAuthoringFact tabletop = PyralisAuthoringFactRegistry.Find("route.tabletop-card");
            Assert.That(tabletop.LaneTags, Does.Contain("TabletopBoard"));
            Assert.That(tabletop.UnsupportedLaneTags, Does.Contain("Sprite2D"));
            Assert.That(tabletop.RequiredDefinitions, Does.Contain("BoardDefinition"));

            PyralisAuthoringFact networking = PyralisAuthoringFactRegistry.Find("route.networking");
            Assert.That(networking.LaneTags, Does.Contain("Networked"));
            Assert.That(networking.AssignmentFields, Does.Contain("SessionDefinition.networkMode"));
            Assert.That(networking.RelatedStableIds, Does.Contain("proof.network-ownership"));
        }

        [Test]
        public void PyralisAuthoringFactRegistry_RouteProofFacts_NameBroadFirstProofs()
        {
            Assert.That(PyralisAuthoringFactRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

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
                PyralisAuthoringFact proof = PyralisAuthoringFactRegistry.Find(stableId);
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

            PyralisAuthoringFact tabletopProof = PyralisAuthoringFactRegistry.Find("proof.board-card-action");
            Assert.That(tabletopProof.LaneTags, Does.Contain("TabletopBoard"));
            Assert.That(tabletopProof.UnsupportedLaneTags, Does.Contain("Sprite2D"));
            Assert.That(tabletopProof.RelatedStableIds, Does.Contain("route.tabletop-card"));
            Assert.That(tabletopProof.RelatedStableIds, Does.Contain("capability.interaction-action-selection"));

            PyralisAuthoringFact uiProof = PyralisAuthoringFactRegistry.Find("proof.ui-hud-menu");
            Assert.That(uiProof.RequiredSceneComponents, Does.Contain("Canvas"));
            Assert.That(uiProof.RequiredSceneComponents, Does.Contain("EventSystem"));
            Assert.That(uiProof.RelatedStableIds, Does.Contain("route.ui-hud-menu"));

            PyralisAuthoringFact cameraProof = PyralisAuthoringFactRegistry.Find("proof.camera-cursor-world");
            Assert.That(cameraProof.RequiredProfiles, Does.Contain("CameraRigProfile"));
            Assert.That(cameraProof.RelatedStableIds, Does.Contain("capability.camera-follow-bounds"));

            PyralisAuthoringFact networkProof = PyralisAuthoringFactRegistry.Find("proof.network-ownership");
            Assert.That(networkProof.LaneTags, Does.Contain("Networked"));
            Assert.That(networkProof.RequiredSceneComponents, Does.Contain("NetworkManager"));
            Assert.That(networkProof.AssignmentFields, Does.Contain("NetworkManager.NetworkPrefabs"));
        }

        [Test]
        public void PyralisAuthoringFactRegistry_SceneEvidenceFacts_LinkSurfaceGuidanceToRouteProofs()
        {
            Assert.That(PyralisAuthoringFactRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

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
                PyralisAuthoringFact sceneEvidence = PyralisAuthoringFactRegistry.Find(stableId);
                Assert.That(sceneEvidence, Is.Not.Null, stableId);
                Assert.That(sceneEvidence.Kind, Is.EqualTo(PyralisAuthoringFactKind.SceneComponent), stableId);
                Assert.That(sceneEvidence.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.SceneEvidence), stableId);
                Assert.That(sceneEvidence.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Inferred), stableId);
                Assert.That(sceneEvidence.WorkIntent, Is.EqualTo("SceneEvidence"), stableId);
                Assert.That(sceneEvidence.FirstProof, Is.Not.Empty, stableId);
                Assert.That(sceneEvidence.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.Hierarchy), stableId);
            }

            PyralisAuthoringFact uiEvidence = PyralisAuthoringFactRegistry.Find("scene-evidence.ui-hud-menus");
            Assert.That(uiEvidence.RequiredSceneComponents, Does.Contain("Canvas"));
            Assert.That(uiEvidence.RequiredSceneComponents, Does.Contain("EventSystem"));
            Assert.That(uiEvidence.RelatedStableIds, Does.Contain("proof.ui-hud-menu"));

            PyralisAuthoringFact selectionEvidence = PyralisAuthoringFactRegistry.Find("scene-evidence.board-action-selection");
            Assert.That(selectionEvidence.RequiredSceneComponents, Does.Contain("TabletopBoardGridPresenter"));
            Assert.That(selectionEvidence.RelatedStableIds, Does.Contain("proof.board-card-action"));
            Assert.That(selectionEvidence.RelatedStableIds, Does.Contain("proof.action-selection"));

            PyralisAuthoringFact encounterEvidence = PyralisAuthoringFactRegistry.Find("scene-evidence.pickups-hazards-enemies");
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
        public void PyralisAuthoringFactRegistry_FactExplorerCoverage_IncludesCurrentAuthoring2Kinds()
        {
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.RouteFamily).Count, Is.GreaterThanOrEqualTo(7));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.RuntimeCapability).Count, Is.GreaterThanOrEqualTo(5));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.SetupNode).Count, Is.GreaterThanOrEqualTo(10));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.Definition).Count, Is.GreaterThanOrEqualTo(15));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.Profile).Count, Is.GreaterThanOrEqualTo(9));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.SceneComponent).Count, Is.GreaterThanOrEqualTo(14));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.UnitySurface).Count, Is.GreaterThanOrEqualTo(7));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.Proof).Count, Is.GreaterThanOrEqualTo(9));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.AssignmentField).Count, Is.GreaterThanOrEqualTo(20));
            Assert.That(PyralisAuthoringFactRegistry.GetFacts(PyralisAuthoringFactKind.CustomizationMoment).Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(PyralisAuthoringFactRegistry.Find("capability.2d-pawn-movement"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("proof.1p-pawn-movement"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("proof.board-card-action"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("proof.ui-hud-menu"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("proof.camera-cursor-world"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("proof.network-ownership"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("inspector.input-profile.gameplay-action-names"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.session-definition"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.board-definition"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("reflection.create-asset-menu.feature-module-definition"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("reflection.add-component-menu.tabletop-board-grid-presenter"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("reflection.add-component-menu.cinemachine-camera-rig-controller"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("reflection.add-component-menu.gameplay-session-bootstrap"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("scene-evidence.ui-hud-menus"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("scene-evidence.board-action-selection"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("convention.serialized-field.pawn-definition.pawn-prefab"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("convention.serialized-field.game-mode-definition.board-definition"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("inspector.game-mode-definition.board-and-turn-rules"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("inspector.feature-module-definition.profile-runtime-network"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("route.tabletop-card"), Is.Not.Null);
            Assert.That(PyralisAuthoringFactRegistry.Find("route.networking"), Is.Not.Null);
        }

        [Test]
        public void PyralisAuthoringValidationModel_BootstrapRoute_AddsSceneSurfaceAuditCards()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop.validation",
                "Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat);
            RuntimePatternDefinition action = CreateRuntimePattern(
                "pattern.action.validation",
                "Action Selection",
                RuntimeCapabilityFamily.ActionTargeting,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { tabletop, action };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(bootstrap);

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(bootstrap, report);

            PyralisAuthoringValidationIssue uiIssue = model.Issues.First(issue => issue.IssueCode == "sceneSurface.ui-hud-menus.missing");
            PyralisAuthoringValidationIssue selectionIssue = model.Issues.First(issue => issue.IssueCode == "sceneSurface.board-action-selection.missing");
            Assert.That(uiIssue.Category, Is.EqualTo(PyralisAuthoringValidationCategory.SceneObjects));
            Assert.That(uiIssue.Problem, Does.Contain("UI / HUD / Menus"));
            Assert.That(uiIssue.WhyItMatters, Does.Contain("Validate found"));
            Assert.That(uiIssue.InspectionHint, Does.Contain("Canvas"));
            Assert.That(uiIssue.AffectedMember, Is.EqualTo("Scene surface: UI / HUD / Menus"));
            Assert.That(uiIssue.Expected, Does.Contain("route-owned UI surface"));
            Assert.That(uiIssue.Found, Does.Contain("No Canvas"));
            Assert.That(uiIssue.SuccessLooksLike, Does.Contain("Pressing Play shows"));
            Assert.That(uiIssue.HasAuditEvidence, Is.True);
            Assert.That(uiIssue.Target, Is.EqualTo(bootstrap));
            Assert.That(uiIssue.GuidanceActionLabel, Is.EqualTo("Open Map"));
            Assert.That(uiIssue.HasGuidanceAction, Is.True);
            Assert.That(selectionIssue.InspectionHint, Does.Contain("selection surface"));
            Assert.That(selectionIssue.Expected, Does.Contain("selection surface"));
            Assert.That(selectionIssue.SuccessLooksLike, Does.Contain("choose one legal action"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(action);
            Object.DestroyImmediate(tabletop);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringValidationModel_BootstrapWithoutSession_SurfacesMissingSessionDefinition()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(bootstrap);

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(bootstrap, report);

            PyralisAuthoringValidationIssue issue = model.Issues.First(card => card.IssueCode == "bootstrap.sessionDefinition.missing");
            Assert.That(model.HasIssues, Is.True);
            Assert.That(issue.Category, Is.EqualTo(PyralisAuthoringValidationCategory.SessionSetup));
            Assert.That(issue.Problem, Does.Contain("Session Definition is not assigned"));
            Assert.That(issue.AffectedMember, Is.EqualTo("GameplaySessionBootstrap.sessionDefinition"));
            Assert.That(issue.Target, Is.EqualTo(bootstrap));
            Assert.That(issue.GuidanceActionLabel, Is.EqualTo("Open Bootstrap Guide"));
            Assert.That(model.TypedIssues.Select(typedIssue => typedIssue.IssueCode), Does.Contain("bootstrap.sessionDefinition.missing"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringValidationModel_SessionIssue_ExposesTypedIssueMetadata()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();

            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(session);
            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(session, report);

            PyralisAuthoringValidationIssue cardIssue = model.Issues.First(issue => issue.IssueCode == "session.defaultGameMode.missing");
            PyralisAuthoringIssue typedIssue = cardIssue.TypedIssue;

            Assert.That(model.TypedIssues.Select(issue => issue.IssueCode), Does.Contain("session.defaultGameMode.missing"));
            Assert.That(typedIssue, Is.Not.Null);
            Assert.That(typedIssue.IssueCode, Is.EqualTo("session.defaultGameMode.missing"));
            Assert.That(typedIssue.Severity, Is.EqualTo(PyralisAuthoringIssueSeverity.Required));
            Assert.That(typedIssue.WorkIntent, Is.EqualTo(PyralisSetupFlowWorkIntent.RequiredSetup.ToString()));
            Assert.That(typedIssue.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.Missing));
            Assert.That(typedIssue.FieldOrComponent, Is.EqualTo("SessionDefinition.defaultGameMode"));
            Assert.That(typedIssue.NativeAction, Is.Not.Null);
            Assert.That(typedIssue.NativeAction.Value.Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void PyralisAuthoringValidationModel_SceneSurfaceIssue_ExposesProofEnhancerTypedIssue()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop.typed.validation",
                "Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { tabletop };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            SetObjectReference(bootstrap, "sessionDefinition", session);
            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(bootstrap);

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(bootstrap, report);

            PyralisAuthoringValidationIssue sceneIssue = model.Issues.First(issue => issue.IssueCode == "sceneSurface.board-action-selection.missing");
            PyralisAuthoringIssue typedIssue = sceneIssue.TypedIssue;
            Assert.That(typedIssue.Severity, Is.EqualTo(PyralisAuthoringIssueSeverity.Recommended));
            Assert.That(typedIssue.WorkIntent, Is.EqualTo(PyralisSetupFlowWorkIntent.ProofEnhancer.ToString()));
            Assert.That(typedIssue.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.Missing));
            Assert.That(typedIssue.TargetObject, Is.EqualTo("scene Hierarchy"));
            Assert.That(typedIssue.NativeAction, Is.Not.Null);
            Assert.That(typedIssue.NativeAction.Value.Surface, Is.EqualTo(PyralisAuthoringActionSurface.Hierarchy));
            Assert.That(typedIssue.NativeAction.Value.Verb, Is.EqualTo("Create or link"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(tabletop);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringValidationModel_BootstrapRoute_AddsPrefabReadinessAuditCards()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            GameObject pawnPrefab = new GameObject("Bare Pawn");
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = pawnPrefab;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.defaultPawn = pawn;
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(bootstrap);

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(bootstrap, report);

            PyralisAuthoringValidationIssue pawnRootIssue = model.Issues.First(issue =>
                issue.IssueCode.StartsWith("prefabReadiness.required.")
                && issue.Found.Contains("PawnRoot"));
            Assert.That(pawnRootIssue.Category, Is.EqualTo(PyralisAuthoringValidationCategory.PawnsActors));
            Assert.That(pawnRootIssue.Problem, Does.Contain("Required prefab readiness issue"));
            Assert.That(pawnRootIssue.AffectedMember, Is.EqualTo("PawnDefinition.pawnPrefab"));
            Assert.That(pawnRootIssue.WhyItMatters, Does.Contain("audit"));
            Assert.That(pawnRootIssue.InspectionHint, Does.Contain("pawn prefab root"));
            Assert.That(pawnRootIssue.Expected, Does.Contain("PawnRoot"));
            Assert.That(pawnRootIssue.SuccessLooksLike, Does.Contain("spawn or control one pawn"));
            Assert.That(pawnRootIssue.Target, Is.EqualTo(pawnPrefab));
            Assert.That(pawnRootIssue.PrimaryActionLabel, Is.EqualTo("Inspect Pawn Setup"));
            Assert.That(pawnRootIssue.GuidanceActionLabel, Is.EqualTo("Open Map"));
            Assert.That(pawnRootIssue.HasGuidanceAction, Is.True);
            Assert.That(pawnRootIssue.HasAuditEvidence, Is.True);
            Assert.That(pawnRootIssue.TypedIssue.IssueCode, Does.StartWith("prefabReadiness.required."));
            Assert.That(pawnRootIssue.TypedIssue.Severity, Is.EqualTo(PyralisAuthoringIssueSeverity.Required));
            Assert.That(pawnRootIssue.TypedIssue.EvidenceState, Is.EqualTo(PyralisAuthoringEvidenceState.CandidateDetected));
            Assert.That(pawnRootIssue.TypedIssue.WorkIntent, Is.EqualTo(PyralisSetupFlowWorkIntent.RequiredSetup.ToString()));
            Assert.That(pawnRootIssue.TypedIssue.NativeAction, Is.Not.Null);
            Assert.That(pawnRootIssue.TypedIssue.NativeAction.Value.Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(pawnPrefab);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringCapabilitySelection_ReplacesOneCapabilityWithoutDroppingOthers()
        {
            RuntimePatternDefinition pawn = CreateRuntimePattern(
                "pattern.character-pawn",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            RuntimePatternDefinition oldCombat = CreateRuntimePattern(
                "pattern.combat.old",
                "Old Combat",
                RuntimeCapabilityFamily.Combat,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn);
            RuntimePatternDefinition newCombat = CreateRuntimePattern(
                "pattern.combat.new",
                "New Combat",
                RuntimeCapabilityFamily.Combat,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn);
            RuntimePatternDefinition scoring = CreateRuntimePattern(
                "pattern.scoring",
                "Scoring",
                RuntimeCapabilityFamily.ScoringObjectives,
                ParticipantEmbodimentRequirement.NoneRequired,
                RuntimeControlSurface.MenuSelection);

            RuntimePatternDefinition[] result = PyralisAuthoringCapabilitySelection.SetCapabilityPattern(
                new[] { pawn, oldCombat, scoring },
                newCombat);

            Assert.That(result, Has.Member(pawn));
            Assert.That(result, Has.Member(newCombat));
            Assert.That(result, Has.Member(scoring));
            Assert.That(result, Has.No.Member(oldCombat));
            Assert.That(PyralisAuthoringCapabilitySelection.GetSelectedPattern(result, RuntimeCapabilityFamily.Combat), Is.EqualTo(newCombat));

            RuntimePatternDefinition[] removed = PyralisAuthoringCapabilitySelection.RemoveCapabilityFamily(result, RuntimeCapabilityFamily.CharacterPawnGameplay);

            Assert.That(removed, Has.No.Member(pawn));
            Assert.That(removed, Has.Member(newCombat));
            Assert.That(removed, Has.Member(scoring));

            Object.DestroyImmediate(scoring);
            Object.DestroyImmediate(newCombat);
            Object.DestroyImmediate(oldCombat);
            Object.DestroyImmediate(pawn);
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
        public void PyralisAuthoringRouteReport_PlainSceneObject_GuidesAddComponentBeforeAssetWiring()
        {
            GameObject root = new GameObject("Gameplay Root");

            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(root);

            Assert.That(report.RouteName, Is.EqualTo("Scene object selected"));
            Assert.That(report.NextStep, Does.Contain("Inspector -> Add Component"));
            Assert.That(report.NextStep, Does.Contain("GameplaySessionBootstrap"));
            Assert.That(report.RouteGuidance, Does.Contain("Hierarchy"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringRouteReport_MainCamera_DoesNotSuggestBootstrapOnCamera()
        {
            GameObject cameraRoot = new GameObject("Main Camera");
            cameraRoot.AddComponent<Camera>();

            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(cameraRoot);

            Assert.That(report.RouteName, Is.EqualTo("Scene support object selected"));
            Assert.That(report.NextStep, Does.Contain("Create or select a Gameplay Root"));
            Assert.That(report.NextStep, Does.Contain("add GameplaySessionBootstrap there"));
            Assert.That(report.NextStep, Does.Not.Contain("on `Main Camera`"));
            Assert.That(report.RouteGuidance, Does.Contain("camera"));
            Assert.That(report.RouteGuidance, Does.Contain("active setup"));

            Object.DestroyImmediate(cameraRoot);
        }

        [Test]
        public void PyralisAuthoringOverviewModel_TabletopRoute_SeparatesRecommendationsFromOptionalPawnWork()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Board Card Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { tabletop };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model.DoNow.Select(issue => issue.Label), Does.Contain("Tabletop Runtime Contract"));
            Assert.That(model.DoSoon.Select(issue => issue.Label), Does.Contain("Assign Tabletop Selection Surface"));
            Assert.That(model.Later.Select(issue => issue.Label), Does.Contain("Assign Participant Pawn"));
            Assert.That(model.FirstProofLabel, Is.EqualTo("Board/Card Action Proof"));
            Assert.That(model.FirstProofSetupSurface, Does.Contain("TabletopBoardGridPresenter"));
            Assert.That(model.FirstProofSetupSurface, Does.Contain("project-owned equivalent"));
            Assert.That(model.FirstProofSuccessCriteria, Does.Contain("choose one legal board space"));
            Assert.That(model.FirstProofSuccessCriteria, Does.Contain("board/card/turn state"));
            Assert.That(model.FirstProofDeferUntilAfter, Does.Contain("Defer pawn actors"));
            Assert.That(model.FirstProofDeferUntilAfter, Does.Contain("networking"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(tabletop);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringOverviewModel_PawnRoute_RecommendsPawnMovementProof()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model.FirstProofLabel, Is.EqualTo("1P Pawn Movement Proof"));
            Assert.That(model.FirstProofGuidance, Does.Contain("spawn the pawn"));
            Assert.That(model.FirstProofGuidance, Does.Contain("visible movement"));
            Assert.That(model.FirstProofSetupSurface, Does.Contain("one PawnDefinition"));
            Assert.That(model.FirstProofSuccessCriteria, Does.Contain("spawns one pawn"));
            Assert.That(model.FirstProofDeferUntilAfter, Does.Contain("HUD"));
            Assert.That(model.BestNextAction, Does.Contain("Pawn Definition"));
            Assert.That(model.BestNextAction, Does.Contain("ParticipantDefinition > Default Pawn"));
            Assert.That(model.DoNow.First(issue => issue.Label == "Assign Participant Pawn").NativeActionGuidance, Does.Contain("right-click"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
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
        public void PyralisAuthoringFactRegistry_Native1PMovementChecklist_ExposesCreateAndAddComponentFacts()
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
                PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find(factId);
                Assert.That(fact, Is.Not.Null, $"Missing native authoring fact `{factId}`.");
                Assert.That(fact.NativeActions, Is.Not.Empty, $"Native authoring fact `{factId}` should expose a Unity action.");
            }

            PyralisAuthoringFact movement = PyralisAuthoringFactRegistry.Find("reflection.add-component-menu.motor-2d-input-adapter");
            Assert.That(movement.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));
            Assert.That(movement.NativeActions[0].Verb, Is.EqualTo("Add Component"));
            Assert.That(movement.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));
        }

        [Test]
        public void PyralisAuthoringOverviewModel_TabletopRoute_RecommendsBoardActionProof()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Board Card Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { tabletop };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model.FirstProofLabel, Is.EqualTo("Board/Card Action Proof"));
            Assert.That(model.FirstProofGuidance, Does.Contain("rules-backed selection"));
            Assert.That(model.FirstProofGuidance, Does.Contain("selection surface"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(tabletop);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringValidationModel_PawnIssue_BuildsInspectionCard()
        {
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(pawn);

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(pawn, report);

            Assert.That(model.HasIssues, Is.True);
            PyralisAuthoringValidationIssue issue = model.Issues.First();
            Assert.That(issue.IssueCode, Is.EqualTo("pawn.pawnPrefab.missing"));
            Assert.That(issue.Category, Is.EqualTo(PyralisAuthoringValidationCategory.PawnsActors));
            Assert.That(issue.Problem, Does.Contain("pawn prefab"));
            Assert.That(issue.AffectedMember, Is.EqualTo("PawnDefinition.pawnPrefab"));
            Assert.That(issue.WhyItMatters, Does.Contain("Pawn-backed routes"));
            Assert.That(issue.InspectionHint, Does.Contain("PawnDefinition"));
            Assert.That(issue.Target, Is.EqualTo(pawn));
            Assert.That(issue.CanInspectTarget, Is.True);
            Assert.That(issue.PrimaryActionLabel, Is.EqualTo("Inspect Pawn Setup"));
            Assert.That(issue.GuidanceActionLabel, Is.EqualTo("Open Pawn Guide"));
            Assert.That(issue.HasGuidanceAction, Is.True);

            Object.DestroyImmediate(pawn);
        }

        [Test]
        public void PyralisAuthoringRouteReport_Pawn2DPrefab_FlagsEnvironmentSizedSpriteAndPhysicsDefaults()
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

                PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(pawn);
                PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(pawn, report);

                Assert.That(report.ValidationIssues.Any(issue => issue.Contains("Gravity Scale to 0")), Is.True);
                Assert.That(report.ValidationIssues.Any(issue => issue.Contains("Freeze Rotation")), Is.True);
                Assert.That(report.ValidationIssues.Any(issue => issue.Contains("environment-sized sprite")), Is.True);
                Assert.That(model.Issues.Any(issue => issue.IssueCode == "pawn.prefab.sprite.environmentSized"), Is.True);
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
        public void PyralisAuthoringValidationModel_SetupPatternIssue_PreservesSetupProfileGuidance()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = System.Array.Empty<RuntimePatternDefinition>();
            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(setupProfile);

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(setupProfile, report);

            Assert.That(model.Issues.Select(issue => issue.Category), Does.Contain(PyralisAuthoringValidationCategory.SetupProfile));
            PyralisAuthoringValidationIssue setupIssue = model.Issues.First(issue => issue.Category == PyralisAuthoringValidationCategory.SetupProfile);
            Assert.That(setupIssue.IssueCode, Is.EqualTo("setupProfile.runtimeCapabilities.missing"));
            Assert.That(setupIssue.Problem, Does.Contain("runtime capability"));
            Assert.That(setupIssue.AffectedMember, Is.EqualTo("GameSetupProfile.runtimeCapabilities"));
            Assert.That(setupIssue.WhyItMatters, Does.Contain("setup profile"));
            Assert.That(setupIssue.InspectionHint, Does.Contain("GameSetupProfile"));
            Assert.That(setupIssue.Target, Is.EqualTo(setupProfile));
            Assert.That(setupIssue.PrimaryActionLabel, Is.EqualTo("Inspect Setup Profile"));
            Assert.That(setupIssue.GuidanceActionLabel, Is.EqualTo("Open Setup Profile"));
            Assert.That(setupIssue.HasGuidanceAction, Is.True);

            Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void PyralisAuthoringValidationModel_SessionIssue_UsesStructuredIssueCode()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = null;
            session.defaultParticipants = System.Array.Empty<ParticipantDefinition>();
            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(session);

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(session, report);

            Assert.That(model.Issues.Select(issue => issue.IssueCode), Does.Contain("session.defaultGameMode.missing"));
            Assert.That(model.Issues.Select(issue => issue.IssueCode), Does.Contain("session.defaultParticipants.missing"));
            PyralisAuthoringValidationIssue modeIssue = model.Issues.First(issue => issue.IssueCode == "session.defaultGameMode.missing");
            PyralisAuthoringValidationIssue participantIssue = model.Issues.First(issue => issue.IssueCode == "session.defaultParticipants.missing");
            Assert.That(modeIssue.AffectedMember, Is.EqualTo("SessionDefinition.defaultGameMode"));
            Assert.That(modeIssue.PrimaryActionLabel, Is.EqualTo("Inspect Session Chain"));
            Assert.That(modeIssue.GuidanceActionLabel, Is.EqualTo("Open Session Guide"));
            Assert.That(modeIssue.HasGuidanceAction, Is.True);
            Assert.That(participantIssue.PrimaryActionLabel, Is.EqualTo("Inspect Participant Setup"));
            Assert.That(participantIssue.GuidanceActionLabel, Is.EqualTo("Open Participant Guide"));
            Assert.That(participantIssue.HasGuidanceAction, Is.True);

            Object.DestroyImmediate(session);
        }

        [Test]
        public void PyralisAuthoringValidationModel_GameModeIssue_UsesStructuredIssueCode()
        {
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = null;
            mode.enableRespawn = false;
            mode.startingLives = 1;
            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(mode);

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(mode, report);

            Assert.That(model.Issues.Select(issue => issue.IssueCode), Does.Contain("gameMode.setupProfile.missing"));
            Assert.That(model.Issues.Select(issue => issue.IssueCode), Does.Contain("gameMode.startingLives.respawnDisabled"));
            PyralisAuthoringValidationIssue setupProfileIssue = model.Issues.First(issue => issue.IssueCode == "gameMode.setupProfile.missing");
            Assert.That(setupProfileIssue.AffectedMember, Is.EqualTo("GameModeDefinition.setupProfile"));
            Assert.That(setupProfileIssue.PrimaryActionLabel, Is.EqualTo("Inspect Setup Profile"));
            Assert.That(setupProfileIssue.GuidanceActionLabel, Is.EqualTo("Open Game Rules Guide"));
            Assert.That(setupProfileIssue.HasGuidanceAction, Is.True);

            Object.DestroyImmediate(mode);
        }

        [Test]
        public void PyralisAuthoringValidationModel_FixableSlots_OpenFocusedGuides()
        {
            RuntimePatternDefinition duplicatePattern = CreateRuntimePattern(
                "pattern.duplicate",
                "Duplicate Pattern",
                RuntimeCapabilityFamily.Combat,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.Combat },
                null,
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.Combat }
            };
            setupProfile.runtimePatterns = new[] { duplicatePattern };
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultParticipants = new ParticipantDefinition[] { null };

            PyralisAuthoringValidationModel setupModel = PyralisAuthoringValidationModel.Build(setupProfile, PyralisAuthoringRouteReport.Build(setupProfile));
            PyralisAuthoringValidationModel sessionModel = PyralisAuthoringValidationModel.Build(session, PyralisAuthoringRouteReport.Build(session));

            Assert.That(setupModel.Issues.First(issue => issue.IssueCode == "setupProfile.runtimeCapabilities.slot.empty").GuidanceActionLabel, Is.EqualTo("Open Setup Profile"));
            Assert.That(setupModel.Issues.First(issue => issue.IssueCode == "setupProfile.runtimeCapabilities.duplicate").GuidanceActionLabel, Is.EqualTo("Open Setup Profile"));
            Assert.That(sessionModel.Issues.First(issue => issue.IssueCode == "session.defaultParticipants.slot.empty").GuidanceActionLabel, Is.EqualTo("Open Participant Guide"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(duplicatePattern);
        }

        [Test]
        public void PyralisSetupFlowValidator_ProjectilePattern_RequiresSceneProjectileLauncher()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition projectiles = CreateRuntimePattern(
                "pattern.projectile-combat",
                "Projectile Combat",
                RuntimeCapabilityFamily.GunsProjectiles,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Projectile Setup";
            setupProfile.runtimePatterns = new[] { projectiles };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport missingReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(missingReport.GetStep("Assign Projectile Launcher").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(missingReport.GetStep("Assign Projectile Launcher").Message, Does.Contain("ProjectileLauncher2D"));

            ProjectileLauncher2D launcher = root.AddComponent<ProjectileLauncher2D>();
            PyralisSetupFlowReport readyReport = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(readyReport.GetStep("Assign Projectile Launcher").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Ready));

            Object.DestroyImmediate(launcher);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(projectiles);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_RuntimeSystemClaims_ReportUnverifiedRequiredSystems()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition combat = CreateRuntimePattern(
                "pattern.combat",
                "Combat",
                RuntimeCapabilityFamily.Combat,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            combat.requiredRuntimeSystems = new[] { "ParticipantRosterService", "CombatActionDefinition", "HealthComponent and HitBox as needed" };
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Combat Setup";
            setupProfile.runtimePatterns = new[] { combat };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep runtimeClaims = report.GetStep("Resolve Runtime System Claims");

            Assert.That(runtimeClaims.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(runtimeClaims.Message, Does.Contain("CombatActionDefinition"));
            Assert.That(runtimeClaims.Message, Does.Contain("HealthComponent"));
            Assert.That(runtimeClaims.Message, Does.Not.Contain("ParticipantRosterService"));
            Assert.That(report.RequiredIssueCount, Is.EqualTo(0));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(combat);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_RuntimeSystemClaims_RecognizesVerifiedProjectileLauncher()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            ProjectileLauncher2D launcher = root.AddComponent<ProjectileLauncher2D>();
            RuntimePatternDefinition projectiles = CreateRuntimePattern(
                "pattern.projectile-combat",
                "Projectile Combat",
                RuntimeCapabilityFamily.GunsProjectiles,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.MenuSelection);
            projectiles.requiredRuntimeSystems = new[] { "ProjectileFirePlanner", "ProjectileLauncher2D or ProjectileLauncher3D" };
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Projectile Setup";
            setupProfile.runtimePatterns = new[] { projectiles };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.GetStep("Assign Projectile Launcher").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Ready));
            Assert.That(report.GetStep("Resolve Runtime System Claims").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Ready));

            Object.DestroyImmediate(launcher);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(projectiles);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisRuntimeSystemClaimResolver_KnownBootstrapClaims_AreVerified()
        {
            PyralisRuntimeSystemClaimReport report = PyralisRuntimeSystemClaimResolver.BuildReport(
                new[] { "ParticipantRosterService", "SessionStateService", "ParticipantInputRouter", "ProjectileFirePlanner" },
                new PyralisRuntimeSystemClaimContext(null, false, false, false));

            Assert.That(report.HasDeclaredClaims, Is.True);
            Assert.That(report.HasUnverifiedClaims, Is.False);
            Assert.That(report.Unverified, Is.Empty);
        }

        [Test]
        public void PyralisRuntimeSystemClaimResolver_KnownTabletopClaims_AreVerified()
        {
            PyralisRuntimeSystemClaimReport report = PyralisRuntimeSystemClaimResolver.BuildReport(
                new[] { "BoardDefinition", "BoardMovePolicyDefinition", "TurnOrderDefinition", "ActionQueueService", "BoardMoveActionResolver" },
                new PyralisRuntimeSystemClaimContext(null, false, false, false));

            Assert.That(report.HasDeclaredClaims, Is.True);
            Assert.That(report.HasUnverifiedClaims, Is.False);
            Assert.That(report.Unverified, Is.Empty);
        }

        [Test]
        public void PyralisRuntimeSystemClaimResolver_ProjectOwnedClaims_RemainUnverified()
        {
            PyralisRuntimeSystemClaimReport report = PyralisRuntimeSystemClaimResolver.BuildReport(
                new[] { "CombatActionDefinition", "project-owned board/card rule system", "HealthComponent and HitBox as needed" },
                new PyralisRuntimeSystemClaimContext(null, true, true, true));

            Assert.That(report.HasUnverifiedClaims, Is.True);
            Assert.That(report.UnverifiedSummary, Does.Contain("CombatActionDefinition"));
            Assert.That(report.UnverifiedSummary, Does.Contain("board/card rule system"));
            Assert.That(report.UnverifiedSummary, Does.Contain("HealthComponent"));
        }

        [Test]
        public void PyralisRuntimeSystemClaimResolver_ProjectOwnedAlternative_RemainsUnverifiedEvenWhenNamedWithKnownService()
        {
            PyralisRuntimeSystemClaimReport report = PyralisRuntimeSystemClaimResolver.BuildReport(
                new[] { "ParticipantInputRouter or project-owned UI/input bridge" },
                new PyralisRuntimeSystemClaimContext(null, false, false, false));

            Assert.That(report.HasUnverifiedClaims, Is.True);
            Assert.That(report.UnverifiedSummary, Does.Contain("project-owned UI/input bridge"));
        }

        [Test]
        public void PyralisRuntimeSystemClaimResolver_ScoringClaim_RequiresScoreServiceAndEnabledMode()
        {
            string[] claims = { "ParticipantScoreService or project-owned ISessionScoreService" };

            PyralisRuntimeSystemClaimReport serviceOnlyReport = PyralisRuntimeSystemClaimResolver.BuildReport(
                claims,
                new PyralisRuntimeSystemClaimContext(null, false, true, false));
            PyralisRuntimeSystemClaimReport enabledOnlyReport = PyralisRuntimeSystemClaimResolver.BuildReport(
                claims,
                new PyralisRuntimeSystemClaimContext(null, false, false, true));
            PyralisRuntimeSystemClaimReport readyReport = PyralisRuntimeSystemClaimResolver.BuildReport(
                claims,
                new PyralisRuntimeSystemClaimContext(null, false, true, true));

            Assert.That(serviceOnlyReport.HasUnverifiedClaims, Is.True);
            Assert.That(enabledOnlyReport.HasUnverifiedClaims, Is.True);
            Assert.That(readyReport.HasUnverifiedClaims, Is.False);
        }

        [Test]
        public void PyralisRuntimeSystemClaimResolver_PawnRootClaim_FollowsPawnPrefabValidation()
        {
            string[] claims = { "PawnRoot with movement and presentation modules" };

            PyralisRuntimeSystemClaimReport missingPawnReport = PyralisRuntimeSystemClaimResolver.BuildReport(
                claims,
                new PyralisRuntimeSystemClaimContext("Default participant is missing PawnRoot.", false, false, false));
            PyralisRuntimeSystemClaimReport readyPawnReport = PyralisRuntimeSystemClaimResolver.BuildReport(
                claims,
                new PyralisRuntimeSystemClaimContext(null, false, false, false));

            Assert.That(missingPawnReport.HasUnverifiedClaims, Is.True);
            Assert.That(readyPawnReport.HasUnverifiedClaims, Is.False);
        }

        [Test]
        public void PyralisSetupFlowValidator_CameraPattern_RecommendsCameraRigWithoutRequiringIt()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition camera = CreateRuntimePattern(
                "pattern.camera-cursor",
                "Camera Cursor Control",
                RuntimeCapabilityFamily.CameraInput,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.Camera,
                RuntimeControlSurface.Cursor);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Camera Setup";
            setupProfile.runtimePatterns = new[] { camera };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisAuthoringRouteReport routeReport = PyralisAuthoringRouteReport.Build(bootstrap);
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel overview = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            PyralisSetupFlowStep cameraRigStep = report.GetStep("Assign Camera Rig");
            Assert.That(cameraRigStep.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(cameraRigStep.Message, Does.Contain("Camera Root"));
            Assert.That(cameraRigStep.Message, Does.Contain("CinemachineCameraRigController"));
            Assert.That(cameraRigStep.Message, Does.Contain("Create or choose a Camera Rig Profile"));
            Assert.That(cameraRigStep.Message, Does.Contain("create or choose a separate Cinemachine Camera"));
            Assert.That(cameraRigStep.Message, Does.Contain("Shared Camera Behaviour"));
            Assert.That(cameraRigStep.Message, Does.Contain("physical Main Camera keeps the MainCamera tag and Cinemachine Brain"));
            Assert.That(cameraRigStep.Message, Does.Contain("one physical Unity Camera"));
            Assert.That(cameraRigStep.Message, Does.Contain("Camera Rig Controller"));
            Assert.That(overview.DoSoon.First(issue => issue.Label == "Assign Camera Rig").NativeActionGuidance, Does.Contain("Camera Rig Controller"));
            Assert.That(overview.DoSoon.First(issue => issue.Label == "Assign Camera Rig").NativeActionGuidance, Does.Contain("CinemachineCameraRigController"));
            Assert.That(overview.DoSoon.First(issue => issue.Label == "Assign Camera Rig").NativeActionGuidance, Does.Contain("Cinemachine Camera under Camera Root"));
            Assert.That(overview.DoSoon.First(issue => issue.Label == "Assign Camera Rig").NativeActionGuidance, Does.Contain("physical Main Camera keeps the MainCamera tag and Cinemachine Brain"));
            Assert.That(overview.DoSoon.First(issue => issue.Label == "Assign Camera Rig").NativeActionGuidance, Does.Contain("disable or remove accidental extra physical Camera objects"));
            Assert.That(report.RequiredIssueCount, Is.EqualTo(0));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(camera);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_MultiParticipantLocalJoin_RequiresPlayerInputManager()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-local-join",
                "Realtime Local Join",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Local Join Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition firstParticipant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            ParticipantDefinition secondParticipant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.maxParticipants = 2;
            session.defaultParticipants = new[] { firstParticipant, secondParticipant };
            SetObjectReference(bootstrap, "sessionDefinition", session);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            PyralisSetupFlowStep inputManagerStep = report.GetStep("Assign Player Input Manager");
            Assert.That(inputManagerStep.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));
            Assert.That(inputManagerStep.Message, Does.Contain("local join"));
            Assert.That(inputManagerStep.Message, Does.Contain("PlayerInput prefab"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(firstParticipant);
            Object.DestroyImmediate(secondParticipant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringRouteReport_LocalJoin_WithPlayerInputManagerMissingPrefab_DoesNotTellUserToEnterPlayMode()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            System.Type playerInputManagerType = System.Type.GetType("UnityEngine.InputSystem.PlayerInputManager, Unity.InputSystem");
            Assert.That(playerInputManagerType, Is.Not.Null);
            Component playerInputManager = root.AddComponent(playerInputManagerType);
            GameObject secondSpawn = new GameObject("Spawn Point 2");
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<TestPawnMotor>();
            prefab.AddComponent<TestPawnPresentation>();
            prefab.AddComponent<TestPawnInput>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.local-join-missing-prefab",
                "Local Join Missing Prefab",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition firstParticipant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            ParticipantDefinition secondParticipant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            firstParticipant.defaultPawn = pawn;
            secondParticipant.defaultPawn = pawn;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.maxParticipants = 2;
            session.defaultParticipants = new[] { firstParticipant, secondParticipant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            SetObjectReference(bootstrap, "playerInputManager", playerInputManager);
            CameraRigProfile cameraRigProfile = AddOrthographicCameraRig(bootstrap, mode, out GameObject cameraRoot);
            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
            spawnPoints.arraySize = 2;
            spawnPoints.GetArrayElementAtIndex(0).objectReferenceValue = root.transform;
            spawnPoints.GetArrayElementAtIndex(1).objectReferenceValue = secondSpawn.transform;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(bootstrap);

            Assert.That(report.RouteName, Is.EqualTo("Pawn-backed local-join route"));
            Assert.That(report.NextStep, Does.Contain("PlayerInputManager > Player Prefab"));
            Assert.That(report.NextStep, Does.Not.Contain("Enter Play Mode"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(firstParticipant);
            Object.DestroyImmediate(secondParticipant);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(secondSpawn);
            Object.DestroyImmediate(cameraRigProfile);
            Object.DestroyImmediate(cameraRoot);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupFlowValidator_SingleParticipantInputRoute_DoesNotRequirePlayerInputManager()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-single-player",
                "Realtime Single Player",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Single Player Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.maxParticipants = 1;
            session.defaultParticipants = new[] { participant };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            CameraRigProfile cameraRigProfile = AddOrthographicCameraRig(bootstrap, mode, out GameObject cameraRoot);

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.GetStep("Assign Player Input Manager").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Optional));
            Assert.That(report.GetStep("Tune Camera Framing").Status, Is.EqualTo(PyralisSetupFlowStepStatus.Recommended));
            Assert.That(report.GetStep("Tune Camera Framing").Message, Does.Contain("orthographic size"));
            Assert.That(report.GetStep("Tune Camera Framing").WorkIntent, Is.EqualTo(PyralisSetupFlowWorkIntent.ProofEnhancer));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisAuthoringRouteReport_EmptySession_AsksForDefaultGameModeBeforeRuntimePatterns()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();

            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(session);

            Assert.That(report.RouteName, Is.EqualTo("No setup route selected"));
            Assert.That(report.NextStep, Does.Contain("Default Game Mode"));
            Assert.That(report.NextStep, Does.Not.Contain("RuntimePatternDefinition"));
            Assert.That(report.RouteGuidance, Does.Contain("native Unity creation and Inspector fields"));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void PyralisAuthoringRouteReport_NoPawnSetup_TellsRoutePawnPrefabCanStayEmpty()
        {
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Board Card Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Tabletop Setup";
            setupProfile.runtimePatterns = new[] { tabletop };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringRouteReport report = PyralisAuthoringRouteReport.Build(session);

            Assert.That(report.RouteName, Is.EqualTo("Tabletop route"));
            Assert.That(report.NextStep, Does.Contain("one selection surface"));
            Assert.That(report.RouteGuidance, Does.Contain("Pawn prefab can stay empty"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(tabletop);
        }

        [Test]
        public void PyralisAuthoringRouteReport_PawnBootstrap_ReactsToAssignedSpawnPoints()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            GameObject spawn = new GameObject("Spawn Point 1");
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<TestPawnMotor>();
            prefab.AddComponent<TestPawnPresentation>();
            prefab.AddComponent<TestPawnInput>();
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.pawnPrefab = prefab;
            PawnMovementProfile movement = ScriptableObject.CreateInstance<PawnMovementProfile>();
            movement.movementMode = MovementMode.TwoD;
            movement.use2DPhysics = true;
            movement.allow2DJump = true;
            pawn.movementProfile = movement;
            ParticipantDefinition playerOne = ScriptableObject.CreateInstance<ParticipantDefinition>();
            playerOne.displayName = "Player One";
            playerOne.defaultPawn = pawn;
            ParticipantDefinition playerTwo = ScriptableObject.CreateInstance<ParticipantDefinition>();
            playerTwo.displayName = "Player Two";
            playerTwo.defaultPawn = pawn;
            Object inputActions = CreateInputActionsWithMove();
            InputProfile inputProfile = CreateMoveInputProfile(inputActions);
            playerOne.inputProfile = inputProfile;
            playerTwo.inputProfile = inputProfile;
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.maxParticipants = 2;
            session.defaultParticipants = new[] { playerOne, playerTwo };
            SetObjectReference(bootstrap, "sessionDefinition", session);
            CameraRigProfile cameraRigProfile = AddOrthographicCameraRig(bootstrap, mode, out GameObject cameraRoot);

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
            spawnPoints.arraySize = 1;
            spawnPoints.GetArrayElementAtIndex(0).objectReferenceValue = spawn.transform;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            PyralisAuthoringRouteReport mismatchReport = PyralisAuthoringRouteReport.Build(bootstrap);

            Assert.That(mismatchReport.NextStep, Does.Contain("1 assigned spawn point(s) for 2 default participant(s)"));
            Assert.That(mismatchReport.NextStep, Does.Contain("clean 1P proof"));

            session.defaultParticipants = new[] { playerOne };
            session.maxParticipants = 1;
            PyralisAuthoringRouteReport missingPlayfieldReport = PyralisAuthoringRouteReport.Build(bootstrap);

            Assert.That(missingPlayfieldReport.NextStep, Does.Contain("Environment / Playfield"));
            Assert.That(missingPlayfieldReport.NextStep, Does.Not.Contain("Enter Play Mode"));
            Assert.That(missingPlayfieldReport.RouteGuidance, Does.Contain("spawn point only says where the pawn appears"));

            GameObject ground = new GameObject("Ground");
            ground.AddComponent<BoxCollider2D>();
            PyralisAuthoringRouteReport readyReport = PyralisAuthoringRouteReport.Build(bootstrap);

            Assert.That(readyReport.NextStep, Does.Contain("Enter Play Mode"));
            Assert.That(readyReport.NextStep, Does.Contain("moves"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(playerTwo);
            Object.DestroyImmediate(playerOne);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(inputActions);
            Object.DestroyImmediate(movement);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(cameraRigProfile);
            Object.DestroyImmediate(cameraRoot);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
            Object.DestroyImmediate(ground);
            Object.DestroyImmediate(spawn);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void PyralisSetupRouteAnalysis_PawnSetup_ReportsSharedRouteFacts()
        {
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Pawn Setup";
            setupProfile.runtimePatterns = new[] { realtime };
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisSetupRouteAnalysis analysis = PyralisSetupRouteAnalysis.Build(session);

            Assert.That(analysis.RouteName, Is.EqualTo("Pawn Action route"));
            Assert.That(analysis.RequiresPawn, Is.True);
            Assert.That(analysis.HasAssignedPatterns, Is.True);
            Assert.That(analysis.HasValidPatterns, Is.True);
            Assert.That(analysis.HasParticipants, Is.True);
            Assert.That(analysis.RouteFacts, Has.Length.EqualTo(1));
            Assert.That(analysis.PrimaryRouteFact.Capability, Is.EqualTo(PyralisAuthoringRouteCapability.PawnAction));
            Assert.That(analysis.PrimaryRouteFact.Label, Is.EqualTo("Pawn Action"));
            Assert.That(analysis.ParticipantPawnIssue, Does.Contain("PawnDefinition"));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(realtime);
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
