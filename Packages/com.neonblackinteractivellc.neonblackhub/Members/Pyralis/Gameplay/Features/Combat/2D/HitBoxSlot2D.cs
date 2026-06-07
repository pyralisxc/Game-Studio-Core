using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    /// <summary>
    /// Named 2D hitbox slot used by the shared 2D combat controller.
    /// </summary>
    [System.Serializable]
    public class HitBoxSlot2D
    {
        public string zoneName = "Punch";
        public HitBox2D hitBox;
        [System.NonSerialized] public float absOffsetX = 0.5f;

        public void MirrorToSide(Transform root, bool facingRight)
        {
            if (hitBox == null)
                return;

            Vector3 worldPos = hitBox.transform.position;
            worldPos.x = root.position.x + (facingRight ? absOffsetX : -absOffsetX);
            if (hitBox.transform.parent != null)
                hitBox.transform.localPosition = hitBox.transform.parent.InverseTransformPoint(worldPos);
            else
                hitBox.transform.position = worldPos;
        }
    }
}
