using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
/// <summary>
/// A named catalogue of HazardData assets for quick designer access.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Combat,
    Relevance = "A designer-facing catalogue of hazard presets for quick assignment and lookup.",
    NativeSetup = new[] { "Create Asset.", "Add HazardData entries.", "Set unique preset names." },
    AssignmentFields = new[] { nameof(presets) },
    FirstProof = "Verify that hazards can be correctly looked up by name from this library.",
    ExpertAdvice = "Use this to manage a large variety of hazards without cluttering scene references."
)]
[CreateAssetMenu(fileName = "HazardPresetLibrary", menuName = "NeonBlack/Hazards/Hazard Preset Library")]
public class HazardPresetLibrary : ScriptableObject, IRuntimeValidationProvider
{
    public IEnumerable<string> GetRuntimeValidationIssues()
    {
        if (presets == null || presets.Length == 0)
        {
            yield return "Presets list is empty.";
            yield break;
        }

        HashSet<string> names = new HashSet<string>();
        for (int i = 0; i < presets.Length; i++)
        {
            if (presets[i] == null)
            {
                yield return $"Presets[{i}] is null.";
                continue;
            }

            if (string.IsNullOrWhiteSpace(presets[i].presetName))
                yield return $"Presets[{i}] is missing a name.";
            else if (!names.Add(presets[i].presetName))
                yield return $"Duplicate preset name: {presets[i].presetName}";

            if (presets[i].data == null)
                yield return $"Presets[{i}] is missing HazardData.";
        }
    }

    [System.Serializable]
    public class Entry
    {
        [Tooltip("Descriptive name for this preset (e.g. 'Fast Bouncer', 'Wavy Diagonal').")]
        public string presetName;

        [Tooltip("The HazardData asset for this preset.")]
        public HazardData data;
    }

    [Tooltip("All named hazard presets in this library.")]
    public Entry[] presets;

    /// <summary>
    /// Returns the HazardData whose presetName matches (case-insensitive), or null if not found.
    /// </summary>
    public HazardData GetPreset(string name)
    {
        if (presets == null) return null;
        foreach (var e in presets)
            if (e != null && string.Equals(e.presetName, name, System.StringComparison.OrdinalIgnoreCase))
                return e.data;
        return null;
    }
}
}
