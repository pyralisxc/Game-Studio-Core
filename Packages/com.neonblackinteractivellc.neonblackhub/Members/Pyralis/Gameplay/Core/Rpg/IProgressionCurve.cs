namespace NeonBlack.Gameplay.Core.Rpg
{
    /// <summary>
    /// Interface for defining experience-to-level progression curves.
    /// </summary>
    public interface IProgressionCurve
    {
        /// <summary>
        /// Resolves the character level for a given amount of experience points.
        /// </summary>
        int ResolveLevel(int experience);

        /// <summary>
        /// Gets the number of skill points granted for reaching a specific level.
        /// </summary>
        int GetSkillPointGrantForLevel(int level);
    }
}
