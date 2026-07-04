using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
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
        public void ParticipantHealthHudBinder_WithoutHealthPanel_ReportsMissingPanelSurface()
        {
            GameObject root = new GameObject("Health Hud Root");

            try
            {
                ParticipantHealthHudBinder binder = root.AddComponent<ParticipantHealthHudBinder>();

                RuntimeValidationIssue[] issues = ((IRuntimeValidationProvider)binder)
                    .GetRuntimeValidationIssues()
                    .ToArray();

                Assert.That(issues.Select(issue => issue.Message), Does.Contain("`ParticipantHealthHudBinder` should reference at least one `ParticipantHealthPanel`."));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
