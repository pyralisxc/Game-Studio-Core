namespace NeonBlack.Gameplay.Data.Rpg
{
    public interface ISkillTree
    {
        bool TryGetNode(string nodeId, out SkillNode node);
    }
}
