using System.Collections.Generic;
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
        AssignmentFields = new[] { nameof(sceneName), nameof(displayName), nameof(previewImage) },
        FirstProof = "Verify the level is selectable in the menu and loads the correct scene.",
        NativeSetup = new[] { "Set SceneName to match Build Settings.", "Assign Preview Image." },
        ExpertAdvice = "LevelData assets are primarily used by the LevelRegistry to build the world-select UI. Ensure the SceneName exactly matches the entry in File -> Build Settings.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/navigation",
        CapabilityPath = "Core Setup/Navigation/Level Data",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.PlatformCore }
    )]
public class LevelData : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                yield return PyralisRuntimeValidationIssue.Required(
                    "Scene Name is required.",
                    nameof(sceneName),
                    nameof(LevelData));
            if (string.IsNullOrWhiteSpace(displayName))
                yield return PyralisRuntimeValidationIssue.Required(
                    "Display Name is required.",
                    nameof(displayName),
                    nameof(LevelData));
        }

        [Tooltip("Exact scene name as listed in File -> Build Settings. Must match perfectly.")]
        public string sceneName;

        [Tooltip("Friendly world name shown on the main menu selector (e.g. 'Kitchen', 'Bathroom').")]
        public string displayName;

        [Tooltip("Preview image shown on the main menu while this world is selected.")]
        public Sprite previewImage;
    }
}
