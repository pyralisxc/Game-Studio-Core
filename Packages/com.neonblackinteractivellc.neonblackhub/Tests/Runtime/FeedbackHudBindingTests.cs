using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Feedback;
using NeonBlack.Gameplay.Modules.Feedback.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class FeedbackHudBindingTests
    {
        [Test]
        public void ParticipantHealthHudBinder_WithChildHealthPanel_HasNoBinderSurfaceIssue()
        {
            GameObject root = new GameObject("Health Hud Root");
            GameObject panelObject = new GameObject("Health Panel");

            try
            {
                panelObject.transform.SetParent(root.transform);
                ParticipantHealthHudBinder binder = root.AddComponent<ParticipantHealthHudBinder>();
                panelObject.AddComponent<ParticipantHealthPanel>();

                RuntimeValidationIssue[] issues = ((IRuntimeValidationProvider)binder)
                    .GetRuntimeValidationIssues()
                    .ToArray();

                Assert.That(issues, Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ParticipantHealthHudBinder_WithoutHealthPanel_ReportsRecommendedPanelSurface()
        {
            GameObject root = new GameObject("Health Hud Root");

            try
            {
                ParticipantHealthHudBinder binder = root.AddComponent<ParticipantHealthHudBinder>();

                RuntimeValidationIssue[] issues = ((IRuntimeValidationProvider)binder)
                    .GetRuntimeValidationIssues()
                    .ToArray();

                Assert.That(issues, Has.Exactly(1).Matches<RuntimeValidationIssue>(issue =>
                    issue.Severity == RuntimeValidationSeverity.Recommended
                    && issue.Message.Contains("no ParticipantHealthPanel")));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FeedbackHudWidgets_WithMissingLocalSurfaces_ReportNoRequiredIssues()
        {
            GameObject root = new GameObject("Feedback Hud Root");

            try
            {
                IRuntimeValidationProvider[] providers =
                {
                    root.AddComponent<ParticipantTimedTextPanel>(),
                    root.AddComponent<ParticipantHealthPanel>(),
                    root.AddComponent<ParticipantFeedbackHudPresenter>()
                };

                RuntimeValidationIssue[] issues = providers
                    .SelectMany(provider => provider.GetRuntimeValidationIssues())
                    .ToArray();

                Assert.That(issues, Is.Not.Empty);
                Assert.That(issues, Has.None.Matches<RuntimeValidationIssue>(issue =>
                    issue.Severity == RuntimeValidationSeverity.Required));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void DamageNumberSpawner_SpawnDamage_CreatesRuntimeOutput()
        {
            GameObject root = new GameObject("Damage Number Spawner Root");

            try
            {
                DamageNumberSpawner spawner = root.AddComponent<DamageNumberSpawner>();

                spawner.Spawn(12f, Vector3.zero);

                Assert.That(root.transform.childCount, Is.GreaterThan(0));
                Assert.That(root.transform.GetChild(0).gameObject.name, Is.EqualTo("DamageNumber"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
