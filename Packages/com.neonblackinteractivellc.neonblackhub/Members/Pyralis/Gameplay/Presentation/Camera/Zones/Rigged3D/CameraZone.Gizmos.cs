using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Camera.Zones
{
    public partial class CameraZone
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                return;

            Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.1f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.5f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
#endif
    }
}
