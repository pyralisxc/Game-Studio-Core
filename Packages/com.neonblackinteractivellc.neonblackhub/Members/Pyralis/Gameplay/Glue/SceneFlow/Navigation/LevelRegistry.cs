using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Navigation
{
    /// <summary>
    /// Ordered list of all playable worlds. Referenced by menu and session flow.
    /// Create one of these in your project: Assets -> Create -> NeonBlack -> Scene Flow -> Level Registry
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Scene Flow/Level Registry", fileName = "LevelRegistry")]
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Ordered list of all playable worlds. Referenced by menu and session flow.",
        AssignmentFields = new[] { nameof(LevelRegistry.levels) },
        Proof = "The Level Registry is correctly discovered by the Session and Menu services.",
        NativeSetup = new[] { "Populate the Levels array with LevelData assets." },
        ExpertAdvice = "The Registry is the source of truth for the level selector UI. Use it to centralize world definitions across the project.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/navigation",
        CapabilityPath = "Core Setup/Navigation/Level Registry",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.PlatformCore },
        Surface = AuthoringContractSurface.SetupOnly
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
