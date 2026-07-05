using System.Linq;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Glue.Bootstrap;
using NeonBlack.Gameplay.Glue.Wiring.Reporting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class GameplayWiringReportBuilderTests
    {
        [Test]
        public void Build_WithMissingSessionDefinition_UsesCanonicalMissingProviderOnly()
        {
            GameObject root = new GameObject("Gameplay Wiring Report Test");
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();

                GameplayWiringReport report = GameplayWiringReportBuilder.Build(root);

                Assert.That(
                    report.Rows.Count(row =>
                        row.Kind == GameplayWiringRowKind.MissingProvider
                        && row.Contract == "SessionDefinition"
                        && row.Receiver == nameof(GameplaySessionBootstrap)
                        && row.Package == "sessionDefinition"),
                    Is.EqualTo(1));

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.ValidationIssue
                        && row.Contract == "GameplaySessionBootstrap.SessionDefinition.Missing"),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Build_WithMissingSessionDefinition_DefersRouteDependentCameraGuidance()
        {
            GameObject root = new GameObject("Gameplay Wiring Report Test");
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();

                GameplayWiringReport report = GameplayWiringReportBuilder.Build(root);

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.ValidationIssue
                        && row.Contract == "GameplaySessionBootstrap.CameraRig.Optional"),
                    Is.False);

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.TimingIssue
                        && row.Contract == "CoreServiceRoute"),
                    Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Build_WithIncompleteParticipantRoute_DefersParticipantServiceRequirements()
        {
            GameObject root = new GameObject("Gameplay Wiring Report Test");
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();

                GameplayWiringReport report = GameplayWiringReportBuilder.Build(root, session);

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.TimingIssue
                        && row.Contract == "ParticipantServiceRoute"),
                    Is.True);

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.MissingProvider
                        && row.Contract == "ParticipantRosterService"),
                    Is.False);

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.MissingProvider
                        && row.Contract == "ParticipantSpawnService"),
                    Is.False);

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.MissingProvider
                        && row.Contract == "ParticipantInputRouter"),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(session);
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Build_WithIncompleteParticipantRoute_DefersFeatureServiceActivation()
        {
            GameObject root = new GameObject("Gameplay Wiring Report Test");
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();

                GameplayWiringReport report = GameplayWiringReportBuilder.Build(root, session);

                Assert.That(
                    report.Rows.Any(row => row.Kind == GameplayWiringRowKind.ServiceActivation),
                    Is.False);

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.ValidationIssue
                        && row.Contract == "GameplaySessionBootstrap.CameraRig.Optional"),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(session);
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Build_WithCompleteParticipantRoute_ReportsMissingRequiredParticipantProviders()
        {
            GameObject root = new GameObject("Gameplay Wiring Report Test");
            ParticipantDefinition participant = CreateParticipant();
            SessionDefinition session = CreateCompleteRoute(out GameModeDefinition mode, participant);
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();

                GameplayWiringReport report = GameplayWiringReportBuilder.Build(root, session);

                Assert.That(
                    HasMissingProvider(report, "ParticipantRosterService"),
                    Is.True);

                Assert.That(
                    HasMissingProvider(report, "ParticipantSpawnService"),
                    Is.True);

                Assert.That(
                    HasMissingProvider(report, "ParticipantInputRouter"),
                    Is.True);

                Assert.That(
                    report.Rows.Any(row =>
                        row.Kind == GameplayWiringRowKind.TimingIssue
                        && row.Contract == "ParticipantServiceRoute"),
                    Is.False);
            }
            finally
            {
                DestroyObjects(participant, mode, session, root);
            }
        }

        [Test]
        public void Build_WithPlayerInputManagerAndMultipleAutoJoinParticipants_ReportsJoinTimingIssue()
        {
            GameObject root = new GameObject("Gameplay Wiring Report Test");
            ParticipantDefinition firstParticipant = CreateParticipant(autoJoin: true);
            ParticipantDefinition secondParticipant = CreateParticipant(autoJoin: true);
            SessionDefinition session = CreateCompleteRoute(out GameModeDefinition mode, firstParticipant, secondParticipant);
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();
                root.AddComponent<PlayerInputManager>();

                GameplayWiringReport report = GameplayWiringReportBuilder.Build(root, session);

                Assert.That(
                    report.Rows.Count(row =>
                        row.Kind == GameplayWiringRowKind.TimingIssue
                        && row.Contract == "ParticipantJoinRoute"),
                    Is.EqualTo(1));
            }
            finally
            {
                DestroyObjects(secondParticipant, firstParticipant, mode, session, root);
            }
        }

        [Test]
        public void Build_WithCompleteCombatRoute_ReportsCombatFeatureActivation()
        {
            GameObject root = new GameObject("Gameplay Wiring Report Test");
            ParticipantDefinition participant = CreateParticipant();
            SessionDefinition session = CreateCompleteRoute(out GameModeDefinition mode, participant);
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();
                mode.enableCombat = true;

                GameplayWiringReport report = GameplayWiringReportBuilder.Build(root, session);

                Assert.That(
                    HasServiceActivation(report, "CombatServices"),
                    Is.True);
            }
            finally
            {
                DestroyObjects(participant, mode, session, root);
            }
        }

        [Test]
        public void Build_WithCompleteScoringRoute_ReportsScoringGameFlowAndFeedbackActivation()
        {
            GameObject root = new GameObject("Gameplay Wiring Report Test");
            ParticipantDefinition participant = CreateParticipant();
            SessionDefinition session = CreateCompleteRoute(out GameModeDefinition mode, participant);
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();
                mode.enableScore = true;

                GameplayWiringReport report = GameplayWiringReportBuilder.Build(root, session);

                Assert.That(HasServiceActivation(report, "GameFlowServices"), Is.True);
                Assert.That(HasServiceActivation(report, "ScoringServices"), Is.True);
                Assert.That(HasServiceActivation(report, "FeedbackServices"), Is.True);
            }
            finally
            {
                DestroyObjects(participant, mode, session, root);
            }
        }

        private static SessionDefinition CreateCompleteRoute(
            out GameModeDefinition mode,
            params ParticipantDefinition[] participants)
        {
            mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = participants;
            return session;
        }

        private static ParticipantDefinition CreateParticipant(bool autoJoin = true)
        {
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.autoJoin = autoJoin;
            return participant;
        }

        private static void DestroyObjects(params Object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    Object.DestroyImmediate(objects[i]);
            }
        }

        private static bool HasMissingProvider(GameplayWiringReport report, string contract)
        {
            return report.Rows.Any(row =>
                row.Kind == GameplayWiringRowKind.MissingProvider
                && row.Contract == contract);
        }

        private static bool HasServiceActivation(GameplayWiringReport report, string contract)
        {
            return report.Rows.Any(row =>
                row.Kind == GameplayWiringRowKind.ServiceActivation
                && row.Contract == contract
                && row.Provider == "RuntimeFeatureServicePolicy"
                && row.Receiver == "FeatureServiceInstaller");
        }
    }
}
