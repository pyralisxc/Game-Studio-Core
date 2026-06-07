using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
/// <summary>
/// A named hitbox slot. Add as many as needed in PawnCombatBehaviour.hitBoxZones.
///
/// Setup:
///   1. Add child GameObjects to your character Ã¢â‚¬â€ one per zone (e.g. HitBox_Punch, HitBox_Kick).
///   2. Add a BoxCollider (Is Trigger = true) and a HitBox component to each child.
///   3. Set the child's Layer to HitBox.
///   4. In PawnCombatBehaviour â†’ Hit Box Zones, add a slot for each child and match the zone name.
///   5. In WeaponData, set Hit Box Zone to the name of the zone you want that weapon to activate.
///
/// Example zone names: Punch, Kick, Aerial, Wide, High, Low
/// </summary>
[System.Serializable]
public class HitBoxSlot
{
    [Tooltip("Unique zone name. WeaponData.hitBoxZone must exactly match this to target this slot.\n" +
             "Examples: Punch, Kick, Aerial, Wide, High, Low")]
    public string zoneName = "Punch";

    [Tooltip("The HitBox component child for this zone.")]
    public HitBox hitBox;

    /// <summary>Cached absolute X offset from the character root. Set by PawnCombatBehaviour in Awake.</summary>
    [System.NonSerialized] public float absOffsetX = 0.5f;

    /// <summary>
    /// Mirror the hitbox to the correct world side of the owning character.
    /// Handles billboarded parents and flipped visual roots correctly.
    /// </summary>
    public void MirrorToSide(Transform root, bool faceRight)
    {
        if (hitBox == null) return;
        Vector3 worldPos = hitBox.transform.position;
        worldPos.x = root.position.x + (faceRight ? absOffsetX : -absOffsetX);
        if (hitBox.transform.parent != null)
            hitBox.transform.localPosition = hitBox.transform.parent.InverseTransformPoint(worldPos);
        else
            hitBox.transform.position = worldPos;
    }
}
}
