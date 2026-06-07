using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct QuestRewardDefinition
    {
        public int experience;
        public int skillPoints;
        public QuestItemRewardDefinition[] itemRewards;

        public QuestItemRewardDefinition[] ItemRewards => itemRewards ?? Array.Empty<QuestItemRewardDefinition>();
        public bool IsEmpty => experience <= 0 && skillPoints <= 0 && ItemRewards.Length == 0;

        public void Sanitize()
        {
            experience = experience < 0 ? 0 : experience;
            skillPoints = skillPoints < 0 ? 0 : skillPoints;
            itemRewards = ItemRewards.Where(reward => !string.IsNullOrWhiteSpace(reward.ItemId)).ToArray();
            for (int i = 0; i < itemRewards.Length; i++)
                itemRewards[i].Sanitize();
        }

        public QuestReward CreateRuntimeReward()
        {
            QuestItemReward[] rewards = ItemRewards
                .Where(reward => reward.IsValid)
                .Select(reward => reward.CreateRuntimeReward())
                .ToArray();
            return new QuestReward(experience, skillPoints, rewards);
        }
    }
}
