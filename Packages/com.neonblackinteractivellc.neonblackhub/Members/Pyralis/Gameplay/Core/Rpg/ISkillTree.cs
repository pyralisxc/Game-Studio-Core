namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface ISkillTree
    {
        bool TryGetNode(string nodeId, out SkillNode node);
    }
}
