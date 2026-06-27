namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface ISkillTree
    {
        bool TryGetNode(string nodeId, out SkillNode node);
    }
}
