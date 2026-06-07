using UnityEngine;

namespace NeonBlack.Gameplay.Core.Navigation
{
    /// <summary>
    /// Ordered list of all playable worlds. Referenced by menu and session flow.
    /// Create one of these in your project: Assets -> Create -> NeonBlack -> Gameplay -> Scene Flow -> Level Registry
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Scene Flow/Level Registry", fileName = "LevelRegistry")]
    public class LevelRegistry : ScriptableObject
    {
        [Tooltip("All playable worlds in display order.")]
        public LevelData[] levels;

        public LevelData GetRandom()
        {
            if (levels == null || levels.Length == 0)
            {
                return null;
            }

            return levels[Random.Range(0, levels.Length)];
        }

        public LevelData FindByScene(string sceneName)
        {
            if (levels == null)
            {
                return null;
            }

            foreach (LevelData level in levels)
            {
                if (level != null && level.sceneName == sceneName)
                {
                    return level;
                }
            }

            return null;
        }
    }
}
