using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Navigation
{
    /// <summary>
    /// Defines a single playable world/level.
    /// Create via: Assets -> Create -> NeonBlack -> Scene Flow -> Level Data
    ///
    /// Setup:
    ///   1. Create one LevelData asset per world.
    ///   2. Set SceneName to exactly match the scene name in Build Settings.
    ///   3. Set DisplayName to the friendly world name shown on the main menu.
    ///   4. Optionally assign a PreviewImage sprite shown on the main menu world selector.
    ///   5. Drag all LevelData assets into the LevelRegistry asset's Levels array.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Scene Flow/Level Data", fileName = "LevelData_New")]
    [AuthoringContract(
        Category = "Setup, Environment",
        CapabilityPath = "Core Setup/Navigation/Level Data",
        Surface = AuthoringSurface.RequiredSetup,
        Summary = "Data container for level configuration, including display names and scene references.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/navigation",
        RequiredFields = new[] { nameof(sceneName), nameof(displayName) },
        SetupSteps = new[] { "Set SceneName to match Build Settings.", "Assign Preview Image only when the level selector should show artwork." },
        SuccessChecks = new[] { "Verify the level is selectable in the menu and loads the correct scene." },
        Tags = new[] { "capability:Setup", "capability:Environment", "runtime:PlatformCore" },
        Selectable = false
    )]
    public class LevelData : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                yield return RuntimeValidationIssue.Required(
                    "Scene Name is required.",
                    nameof(sceneName),
                    nameof(LevelData));
            if (string.IsNullOrWhiteSpace(displayName))
                yield return RuntimeValidationIssue.Required(
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
