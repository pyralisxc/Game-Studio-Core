using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Glue.Bootstrap;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class RuntimeValidationIssueMetadataTests
    {
        [Test]
        public void RuntimeValidationIssueUtility_WithParentContext_PreservesChildMetadata()
        {
            RuntimeValidationIssue child = RuntimeValidationIssue.Recommended(
                "Child semantic issue.",
                "childField",
                "ChildTarget",
                "Fix the child.",
                "Child is fixed.",
                "Child.Code");

            RuntimeValidationIssue parent = RuntimeValidationIssueUtility.WithParentContext(
                child,
                "Parent: ",
                "Parent.Code",
                "parentField",
                "ParentTarget",
                "Fix the parent.",
                "Parent is fixed.");

            Assert.That(parent.Message, Is.EqualTo("Parent: Child semantic issue."));
            Assert.That(parent.IssueCode, Is.EqualTo("Parent.Code.Child.Code"));
            Assert.That(parent.FieldPath, Is.EqualTo("parentField.childField"));
            Assert.That(parent.TargetLabel, Is.EqualTo("ChildTarget"));
            Assert.That(parent.NativeAction, Is.EqualTo("Fix the child."));
            Assert.That(parent.SuccessCheck, Is.EqualTo("Child is fixed."));
            Assert.That(parent.Severity, Is.EqualTo(RuntimeValidationSeverity.Recommended));
        }

        [Test]
        public void GameplaySessionBootstrap_ReportsGameplayOwnedRuntimeValidationIssues()
        {
            GameObject gameObject = new GameObject("Runtime Validation Bootstrap Test");
            gameObject.SetActive(false);
            try
            {
                GameplaySessionBootstrap bootstrap = gameObject.AddComponent<GameplaySessionBootstrap>();
                IRuntimeValidationProvider provider = bootstrap;
                RuntimeValidationIssue[] issues = provider.GetRuntimeValidationIssues().ToArray();

                Assert.That(issues.Select(issue => issue.IssueCode), Does.Contain("GameplaySessionBootstrap.SessionDefinition.Missing"));
                Assert.That(issues.Any(issue => issue.Severity == RuntimeValidationSeverity.Required), Is.True);
                Assert.That(issues.All(issue => !string.IsNullOrWhiteSpace(issue.Message)), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
