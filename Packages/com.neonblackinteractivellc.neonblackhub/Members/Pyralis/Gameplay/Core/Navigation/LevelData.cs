using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Navigation
{
    /// <summary>
    /// Defines a single playable world/level.
    /// Create via: Assets -> Create -> NeonBlack -> Scene Flow -> Level Data
    ///
    /// Setup:
    ///   1. Create one LevelData asset per world.
    ///   2. Set SceneName to exactly match the scene name in Build Settings.
    ///   3. Set DisplayName to the friendly world name shown on the main menu.
    ///   4. Assign a PreviewImage sprite shown on the main menu world selector.
    ///   5. Drag all LevelData assets into the LevelRegistry asset's Levels array.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Scene Flow/Level Data", fileName = "LevelData_New")]
    [AuthoringContract(
        Capability = AuthoringCapability.Setup | AuthoringCapability.Environment,
        Relevance = "Data container for level configuration, including display names and scene references.",
        AssignmentFields = new[] { "sceneName", "displayName" }
    )]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Exact scene name as listed in File -> Build Settings. Must match perfectly.")]
        public string sceneName;

        [Tooltip("Friendly world name shown on the main menu selector (e.g. 'Kitchen', 'Bathroom').")]
        public string displayName;

        [Tooltip("Preview image shown on the main menu while this world is selected.")]
        public Sprite previewImage;
    }
}
