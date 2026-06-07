namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct ProgressionState
    {
        public ProgressionState(int experience, int level, int skillPoints)
        {
            Experience = experience < 0 ? 0 : experience;
            Level = level < 1 ? 1 : level;
            SkillPoints = skillPoints < 0 ? 0 : skillPoints;
        }

        public int Experience { get; }
        public int Level { get; }
        public int SkillPoints { get; }

        public ProgressionState WithExperienceLevelAndSkillPoints(int experience, int level, int skillPoints)
        {
            return new ProgressionState(experience, level, skillPoints);
        }
    }
}
