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
