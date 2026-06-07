using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgSkillTreeRuntimeTests
    {
        [Test]
        public void SkillTreeService_UnlockNode_SpendsSkillPointsAndRecordsUnlock()
        {
            ProgressionService progression = CreateProgressionWithSkillPoints();
            SkillTreeService service = new SkillTreeService(progression);
            SkillTreeDefinition tree = CreateTree(
                new SkillNodeDefinition("skill.root", "Root", 1));
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            bool unlocked = service.TryUnlock(owner, tree, "skill.root", out string issue);

            Assert.That(unlocked, Is.True, issue);
            Assert.That(service.IsUnlocked(owner, "skill.root"), Is.True);
            Assert.That(progression.GetState(owner).SkillPoints, Is.EqualTo(2));

            Object.DestroyImmediate(tree);
        }

        [Test]
        public void SkillTreeService_UnlockNode_RejectsMissingPrerequisite()
        {
            ProgressionService progression = CreateProgressionWithSkillPoints();
            SkillTreeService service = new SkillTreeService(progression);
            SkillTreeDefinition tree = CreateTree(
                new SkillNodeDefinition("skill.root", "Root", 1),
                new SkillNodeDefinition("skill.advanced", "Advanced", 1, new[] { "skill.root" }));
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            bool unlocked = service.TryUnlock(owner, tree, "skill.advanced", out string issue);

            Assert.That(unlocked, Is.False);
            Assert.That(issue, Does.Contain("skill.root"));
            Assert.That(progression.GetState(owner).SkillPoints, Is.EqualTo(3));

            Object.DestroyImmediate(tree);
        }

        [Test]
        public void SkillTreeService_UnlockNode_AllowsRepeatableNode()
        {
            ProgressionService progression = CreateProgressionWithSkillPoints();
            SkillTreeService service = new SkillTreeService(progression);
            SkillNodeDefinition repeatable = new SkillNodeDefinition("skill.training", "Training", 1)
            {
                repeatable = true
            };
            SkillTreeDefinition tree = CreateTree(repeatable);
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryUnlock(owner, tree, "skill.training", out string firstIssue), Is.True, firstIssue);
            Assert.That(service.TryUnlock(owner, tree, "skill.training", out string secondIssue), Is.True, secondIssue);

            Assert.That(service.GetUnlockCount(owner, "skill.training"), Is.EqualTo(2));
            Assert.That(progression.GetState(owner).SkillPoints, Is.EqualTo(1));

            Object.DestroyImmediate(tree);
        }

        [Test]
        public void SkillTreeService_ApplySkillEffects_AddsStatModifiers()
        {
            ProgressionService progression = CreateProgressionWithSkillPoints();
            SkillTreeService service = new SkillTreeService(progression);
            SkillNodeDefinition node = new SkillNodeDefinition("skill.wisdom", "Wisdom", 1)
            {
                statModifiers = new[] { new StatModifierDefinition("stat.wisdom", 4f) }
            };
            SkillTreeDefinition tree = CreateTree(node);
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            StatSheet sheet = new StatSheet();
            sheet.SetBaseValue("stat.wisdom", 5f);

            Assert.That(service.TryUnlock(owner, tree, "skill.wisdom", out string issue), Is.True, issue);
            service.ApplySkillEffects(owner, tree, sheet);

            Assert.That(sheet.GetValue("stat.wisdom"), Is.EqualTo(9f));

            Object.DestroyImmediate(tree);
        }

        private static ProgressionService CreateProgressionWithSkillPoints()
        {
            ProgressionCurveDefinition curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
            curve.SetTestThresholds(new[] { 0, 10, 20, 30 });
            curve.SetTestSkillPointGrants(new[] { 0, 1, 1, 1 });
            ProgressionService progression = new ProgressionService(curve);
            progression.AddExperience(new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1"), 30);
            Object.DestroyImmediate(curve);
            return progression;
        }

        private static SkillTreeDefinition CreateTree(params SkillNodeDefinition[] nodes)
        {
            SkillTreeDefinition tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            tree.treeId = "tree.test";
            tree.displayName = "Test Tree";
            tree.nodes = nodes;
            return tree;
        }
    }
}
