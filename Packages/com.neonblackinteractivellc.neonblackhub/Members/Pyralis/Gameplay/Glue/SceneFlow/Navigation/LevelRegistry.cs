using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Navigation
{
    /// <summary>
    /// Ordered list of all playable worlds. Referenced by menu and session flow.
    /// Create one of these in your project: Assets -> Create -> NeonBlack -> Scene Flow -> Level Registry
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Scene Flow/Level Registry", fileName = "LevelRegistry")]
    [AuthoringContract(
        Category = "Setup",
        CapabilityPath = "Core Setup/Navigation/Level Registry",
        Surface = AuthoringSurface.RequiredSetup,
        Summary = "Ordered list of all playable worlds. Referenced by menu and session flow.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/navigation",
        RequiredFields = new[] { nameof(LevelRegistry.levels) },
        SetupSteps = new[] { "Populate the Levels array with LevelData assets." },
        SuccessChecks = new[] { "The Level Registry is correctly discovered by the Session and Menu services." },
        Tags = new[] { "capability:Setup", "runtime:PlatformCore" },
        Selectable = false
    )]
    public class LevelRegistry : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (levels == null || levels.Length == 0)
                yield return PyralisRuntimeValidationIssue.Required("Levels list is empty.");
            else
            {
                for (int i = 0; i < levels.Length; i++)
                    if (levels[i] == null)
                    {
                        yield return PyralisRuntimeValidationIssue.Required(
                            $"Levels[{i}] is unassigned.",
                            $"{nameof(levels)}[{i}]",
                            nameof(LevelRegistry));
                    }
            }
        }

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
