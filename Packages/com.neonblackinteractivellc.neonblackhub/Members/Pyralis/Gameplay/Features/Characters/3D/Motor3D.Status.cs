using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public partial class Motor3D
    {
        public void ApplyReactionLock(float duration)
        {
            _reactionLockTimer = Mathf.Max(_reactionLockTimer, duration);
            Presentation.ResetMoveToIdle();
        }

        public void ClearReactionLock()
        {
            _reactionLockTimer = 0f;
        }

        public void SetStatusMoveSpeedMultiplier(float multiplier)
        {
            Movement?.SetExternalSpeedMultiplier(multiplier);
        }

        public void SetStatusActionLock(bool locked)
        {
            _statusActionLocked = locked;
            if (locked)
                Presentation?.ResetMoveToIdle();
        }
    }
}
