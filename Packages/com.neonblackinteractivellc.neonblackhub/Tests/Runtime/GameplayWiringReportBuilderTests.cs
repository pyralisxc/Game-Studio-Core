using System.Linq;
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
    }
}
