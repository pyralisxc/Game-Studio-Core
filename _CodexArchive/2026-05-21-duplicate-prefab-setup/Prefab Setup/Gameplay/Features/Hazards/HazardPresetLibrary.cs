using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
/// <summary>
/// A named catalogue of HazardData assets for quick designer access.
///
/// Create a library via right-click â†’ Create â†’ NeonBlack â†’ Gameplay â†’ Hazards â†’ Hazard Preset Library.
/// Populate it with your tuned HazardData ScriptableObjects and give each a clear preset name
/// (e.g. "Fast Bouncer", "Wavy Diagonal", "OnImpact Bomber").
///
/// This asset is purely organisational â€” it has no runtime behaviour.
/// Reference a preset name in the Inspector or runtime code to pull the matching HazardData asset
/// without having to drag individual ScriptableObjects everywhere.
/// </summary>
[CreateAssetMenu(fileName = "HazardPresetLibrary", menuName = "NeonBlack/Gameplay/Hazards/Hazard Preset Library")]
public class HazardPresetLibrary : ScriptableObject
{
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
