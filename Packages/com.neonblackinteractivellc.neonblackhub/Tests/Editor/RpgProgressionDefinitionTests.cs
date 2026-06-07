using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class RpgProgressionDefinitionTests
    {
        [Test]
        public void StatDefinition_GetValidationIssues_RequiresStableId()
        {
            StatDefinition stat = ScriptableObject.CreateInstance<StatDefinition>();
            stat.statId = string.Empty;
            stat.displayName = "Wisdom";

            Assert.That(stat.GetValidationIssues().Any(issue => issue.Contains("stable id")), Is.True);

            Object.DestroyImmediate(stat);
        }

        [Test]
        public void StatDefinition_Sanitize_TrimsIdentityFields()
        {
            StatDefinition stat = ScriptableObject.CreateInstance<StatDefinition>();
            stat.statId = " wisdom ";
            stat.displayName = " Wisdom ";
            stat.category = " Mind ";

            stat.Sanitize();

            Assert.That(stat.statId, Is.EqualTo("wisdom"));
            Assert.That(stat.displayName, Is.EqualTo("Wisdom"));
            Assert.That(stat.category, Is.EqualTo("Mind"));

            Object.DestroyImmediate(stat);
        }

        [Test]
        public void ProgressionCurveDefinition_GetValidationIssues_RequiresLevelOneZeroThreshold()
        {
            ProgressionCurveDefinition curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
            curve.SetTestThresholds(new[] { 50, 100 });

            Assert.That(curve.GetValidationIssues().Any(issue => issue.Contains("Level 1")), Is.True);

            Object.DestroyImmediate(curve);
        }

        [Test]
        public void ProgressionCurveDefinition_GetValidationIssues_RejectsDescendingThresholds()
        {
            ProgressionCurveDefinition curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
            curve.SetTestThresholds(new[] { 0, 200, 150 });

            Assert.That(curve.GetValidationIssues().Any(issue => issue.Contains("greater than or equal")), Is.True);

            Object.DestroyImmediate(curve);
        }

        [Test]
        public void ProgressionCurveDefinition_ResolveLevel_UsesHighestReachedThreshold()
        {
            ProgressionCurveDefinition curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
            curve.SetTestThresholds(new[] { 0, 100, 250 });

            Assert.That(curve.ResolveLevel(99), Is.EqualTo(1));
            Assert.That(curve.ResolveLevel(100), Is.EqualTo(2));
            Assert.That(curve.ResolveLevel(999), Is.EqualTo(3));

            Object.DestroyImmediate(curve);
        }
    }
}
