namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IProgressionCurve
    {
        int ResolveLevel(int experience);
        int GetSkillPointGrantForLevel(int level);
    }
}
