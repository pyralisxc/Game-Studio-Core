using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Allows components to modify incoming damage before it is applied.
    /// Return true when a modification was applied; false when untouched.
    /// </summary>
    public interface IDamageModifier
    {
        bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage);
    }
}
