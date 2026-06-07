using System;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgSkillTreePanelPresenterRuntimeTests
    {
        [Test]
        public void RpgSkillTreePanelPresenter_ShowInteractionResult_ListsSkillNodes()
        {
            GameObject root = new GameObject("Skill Tree Panel");
            SkillTreeDefinition tree = CreateTree(
                new SkillNodeDefinition("skill.root", "Root Training", 1),
                new SkillNodeDefinition("skill.advanced", "Advanced Training", 2, new[] { "skill.root" }));
            try
            {
                ProgressionService progression = CreateProgressionWithSkillPoints();
                RpgSkillTreePanelPresenter presenter = root.AddComponent<RpgSkillTreePanelPresenter>();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                presenter.ConfigureForTests(owner, progression, new SkillTreeService(progression), new[] { tree });

                Assert.That(presenter.ShowInteractionResult(Result(PlayerPanelRoute.SkillTree)), Is.True, presenter.LastIssue);

                Assert.That(presenter.Entries.Length, Is.EqualTo(2));
                Assert.That(presenter.Entries[0].NodeId, Is.EqualTo("skill.root"));
                Assert.That(presenter.Entries[0].Title, Is.EqualTo("Root Training"));
                Assert.That(presenter.Entries[0].Cost, Is.EqualTo(1));
                Assert.That(presenter.Entries[0].CanUnlock, Is.True);
                Assert.That(presenter.SkillPointText, Is.EqualTo("Skill Points: 3"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tree);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RpgSkillTreePanelPresenter_UnlockSelectedNode_UpdatesUnlockStateAndSkillPoints()
        {
            GameObject root = new GameObject("Skill Trainer Panel");
            SkillTreeDefinition tree = CreateTree(new SkillNodeDefinition("skill.root", "Root Training", 1));
            try
            {
                ProgressionService progression = CreateProgressionWithSkillPoints();
                SkillTreeService service = new SkillTreeService(progression);
                RpgSkillTreePanelPresenter presenter = root.AddComponent<RpgSkillTreePanelPresenter>();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                presenter.ConfigureForTests(owner, progression, service, new[] { tree });
                presenter.ShowInteractionResult(Result(PlayerPanelRoute.Trainer));

                Assert.That(presenter.UnlockSelectedNode(), Is.True, presenter.LastIssue);

                Assert.That(service.IsUnlocked(owner, "skill.root"), Is.True);
                Assert.That(progression.GetState(owner).SkillPoints, Is.EqualTo(2));
                Assert.That(presenter.Entries[0].IsUnlocked, Is.True);
                Assert.That(presenter.Entries[0].CanUnlock, Is.False);
                Assert.That(presenter.SkillPointText, Is.EqualTo("Skill Points: 2"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tree);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static HubInteractionResult Result(PlayerPanelRoute route)
        {
            return new HubInteractionResult(
                HubInteractionStatus.Selected,
                string.Empty,
                default,
                route,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<HubNotificationPayload>());
        }

        private static ProgressionService CreateProgressionWithSkillPoints()
        {
            ProgressionCurveDefinition curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
            curve.SetTestThresholds(new[] { 0, 10, 20, 30 });
            curve.SetTestSkillPointGrants(new[] { 0, 1, 1, 1 });
            ProgressionService progression = new ProgressionService(curve);
            progression.AddExperience(new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1"), 30);
            UnityEngine.Object.DestroyImmediate(curve);
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
