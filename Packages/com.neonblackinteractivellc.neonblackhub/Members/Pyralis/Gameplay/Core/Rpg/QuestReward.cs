using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct QuestReward
    {
        public QuestReward(int experience, int skillPoints, QuestItemReward[] itemRewards)
        {
            Experience = experience < 0 ? 0 : experience;
            SkillPoints = skillPoints < 0 ? 0 : skillPoints;
            ItemRewards = itemRewards ?? Array.Empty<QuestItemReward>();
        }

        public int Experience { get; }
        public int SkillPoints { get; }
        public QuestItemReward[] ItemRewards { get; }
        public bool IsEmpty => Experience <= 0 && SkillPoints <= 0 && ItemRewards.Length == 0;
    }
}
