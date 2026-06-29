using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Hazards
{
/// <summary>
/// A named catalogue of HazardData assets for quick designer access.
/// </summary>
[AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Hazard Data Catalog",
        Surface = AuthoringSurface.Goal,
        Summary = "A designer-facing catalogue of hazard data entries for quick assignment and lookup.",
        RequiredFields = new[] { nameof(entries) },
        SetupSteps = new[] { "Add HazardData entries.", "Set unique entry names." },
        SuccessChecks = new[] { "Verify that hazards can be correctly looked up by name from this library." },
        Tags = new[] { "capability:Combat", "runtime:Combat" }
    )]
[CreateAssetMenu(fileName = "HazardDataCatalog", menuName = "NeonBlack/Hazards/Hazard Data Catalog")]
public class HazardDataCatalog : ScriptableObject, IRuntimeValidationProvider
{
    public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
    {
        if (entries == null || entries.Length == 0)
        {
            yield return RuntimeValidationIssue.Required("Hazard data entries list is empty.");
            yield break;
        }

        HashSet<string> names = new HashSet<string>();
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] == null)
            {
                yield return RuntimeValidationIssue.Required($"Entries[{i}] is null.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(entries[i].entryName))
                yield return RuntimeValidationIssue.Required($"Entries[{i}] is missing a name.");
            else if (!names.Add(entries[i].entryName))
                yield return RuntimeValidationIssue.Required($"Duplicate hazard data entry name: {entries[i].entryName}");

            if (entries[i].data == null)
                yield return RuntimeValidationIssue.Required($"Entries[{i}] is missing HazardData.");
        }
    }

    [System.Serializable]
    public class Entry
    {
        [Tooltip("Descriptive name for this hazard data entry (e.g. 'Fast Bouncer', 'Wavy Diagonal').")]
        public string entryName;

        [Tooltip("The HazardData asset for this entry.")]
        public HazardData data;
    }

    [Tooltip("All named hazard data entries in this library.")]
    public Entry[] entries;

    /// <summary>
    /// Returns the HazardData whose entryName matches (case-insensitive), or null if not found.
    /// </summary>
    public HazardData GetData(string name)
    {
        if (entries == null) return null;
        foreach (var e in entries)
            if (e != null && string.Equals(e.entryName, name, System.StringComparison.OrdinalIgnoreCase))
                return e.data;
        return null;
    }
}
}
