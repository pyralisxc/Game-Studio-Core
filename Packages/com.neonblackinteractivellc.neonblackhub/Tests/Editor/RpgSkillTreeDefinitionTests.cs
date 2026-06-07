using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class RpgSkillTreeDefinitionTests
    {
        [Test]
        public void SkillTreeDefinition_GetValidationIssues_FlagsDuplicateNodeIds()
        {
            SkillTreeDefinition tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            tree.treeId = "tree.test";
            tree.displayName = "Test";
            tree.nodes = new[]
            {
                new SkillNodeDefinition("skill.root", "Root", 1),
                new SkillNodeDefinition("skill.root", "Other", 1)
            };

            Assert.That(tree.GetValidationIssues().Any(issue => issue.Contains("assigned more than once")), Is.True);

            Object.DestroyImmediate(tree);
        }

        [Test]
        public void SkillTreeDefinition_GetValidationIssues_FlagsBrokenPrerequisite()
        {
            SkillTreeDefinition tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            tree.treeId = "tree.test";
            tree.displayName = "Test";
            tree.nodes = new[]
            {
                new SkillNodeDefinition("skill.advanced", "Advanced", 1, new[] { "skill.missing" })
            };

            Assert.That(tree.GetValidationIssues().Any(issue => issue.Contains("skill.missing")), Is.True);

            Object.DestroyImmediate(tree);
        }

        [Test]
        public void SkillTreeDefinition_GetValidationIssues_FlagsInvalidCost()
        {
            SkillTreeDefinition tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            tree.treeId = "tree.test";
            tree.displayName = "Test";
            tree.nodes = new[] { new SkillNodeDefinition("skill.free", "Free", -1) };

            Assert.That(tree.GetValidationIssues().Any(issue => issue.Contains("cost")), Is.True);

            Object.DestroyImmediate(tree);
        }

        [Test]
        public void SkillTreeDefinition_TryGetNode_FindsNodeById()
        {
            SkillTreeDefinition tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            tree.nodes = new[] { new SkillNodeDefinition("skill.root", "Root", 1) };

            Assert.That(tree.TryGetNode("skill.root", out SkillNodeDefinition node), Is.True);
            Assert.That(node.displayName, Is.EqualTo("Root"));

            Object.DestroyImmediate(tree);
        }
    }
}
