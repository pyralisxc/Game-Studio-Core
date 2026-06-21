using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class RuntimeValidationIssueMetadataTests
    {
        [Test]
        public void PawnDefinition_PreservesFeatureModuleIssueCode()
        {
            FeatureModuleDefinition module = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            module.moduleId = "test.module";
            module.runtimePrefab = null;

            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            pawn.featureModules = new[] { module };

            PyralisRuntimeValidationIssue issue = pawn.GetRuntimeValidationIssues()
                .FirstOrDefault(candidate => candidate.IssueCode.Contains("RuntimePrefab.Missing"));

            Assert.That(issue, Is.Not.Null);
            Assert.That(issue.IssueCode, Is.EqualTo("PawnDefinition.FeatureModule.test_module.FeatureModuleDefinition.RuntimePrefab.Missing"));
            Assert.That(issue.FieldPath, Is.EqualTo("featureModules.runtimePrefab"));
            Assert.That(issue.NativeAction, Does.Contain("runtimePrefab"));

            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(module);
        }

        [Test]
        public void RuntimeValidationIssueUtility_WithParentContext_PreservesChildMetadata()
        {
            PyralisRuntimeValidationIssue child = PyralisRuntimeValidationIssue.Recommended(
                "Child semantic issue.",
                "childField",
                "ChildTarget",
                "Fix the child.",
                "Child is fixed.",
                "Child.Code");

            PyralisRuntimeValidationIssue parent = PyralisRuntimeValidationIssueUtility.WithParentContext(
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
            Assert.That(parent.Severity, Is.EqualTo(PyralisRuntimeValidationSeverity.Recommended));
        }
    }
}
