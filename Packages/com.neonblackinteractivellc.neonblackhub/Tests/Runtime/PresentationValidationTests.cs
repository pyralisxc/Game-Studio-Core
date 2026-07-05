using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class PresentationValidationTests
    {
        [Test]
        public void BillboardFacing3D_WithRuntimeResolvedTarget_HasNoRequiredValidationIssues()
        {
            GameObject go = new GameObject("Billboard Facing");

            try
            {
                BillboardFacing3D billboard = go.AddComponent<BillboardFacing3D>();

                RuntimeValidationIssue[] issues = billboard.GetRuntimeValidationIssues().ToArray();

                Assert.That(issues, Has.None.Matches<RuntimeValidationIssue>(issue =>
                    issue.Severity == RuntimeValidationSeverity.Required));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ActorShadowDriver_WithIncompleteBlobOutput_HasNoRequiredValidationIssues()
        {
            GameObject go = new GameObject("Actor Shadow");
            PawnPresentationProfile profile = ScriptableObject.CreateInstance<PawnPresentationProfile>();

            try
            {
                profile.shadowMode = ActorShadowMode.BlobSprite;
                ActorShadowDriver shadowDriver = go.AddComponent<ActorShadowDriver>();
                shadowDriver.ApplyProfile(profile);

                RuntimeValidationIssue[] issues = shadowDriver.GetRuntimeValidationIssues().ToArray();

                Assert.That(issues, Has.Some.Matches<RuntimeValidationIssue>(issue =>
                    issue.Severity == RuntimeValidationSeverity.Recommended
                    && issue.Message.Contains("blob shadow output")));
                Assert.That(issues, Has.None.Matches<RuntimeValidationIssue>(issue =>
                    issue.Severity == RuntimeValidationSeverity.Required));
            }
            finally
            {
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(go);
            }
        }
    }
}
