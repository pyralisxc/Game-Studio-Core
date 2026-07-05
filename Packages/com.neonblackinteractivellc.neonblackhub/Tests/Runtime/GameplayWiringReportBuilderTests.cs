using System.Linq;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Glue.Bootstrap;
using NeonBlack.Gameplay.Glue.Wiring.Reporting;
using NUnit.Framework;
using UnityEngine;

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
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();
                session.defaultGameMode = mode;
                session.defaultParticipants = new[] { participant };

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
                Object.DestroyImmediate(participant);
                Object.DestroyImmediate(mode);
                Object.DestroyImmediate(session);
                Object.DestroyImmediate(root);
            }
        }

        private static bool HasMissingProvider(GameplayWiringReport report, string contract)
        {
            return report.Rows.Any(row =>
                row.Kind == GameplayWiringRowKind.MissingProvider
                && row.Contract == contract);
        }
    }
}
