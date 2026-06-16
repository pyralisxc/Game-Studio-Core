using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DMovementComponent
    {
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showBoundsGizmo)
                return;

            Vector3 centre = transform.position + (Vector3)spriteRadiusOffset;
            UnityEditor.Handles.color = new Color(0f, 1f, 0.4f, 0.35f);
            UnityEditor.Handles.DrawSolidDisc(centre, Vector3.back, spriteRadius);
            UnityEditor.Handles.color = new Color(0f, 1f, 0.4f, 1f);
            UnityEditor.Handles.DrawWireDisc(centre, Vector3.back, spriteRadius);
            UnityEditor.Handles.DrawSolidDisc(centre, Vector3.back, 0.02f);

            float total = spriteRadius + edgePadding;
            UnityEditor.Handles.color = new Color(1f, 0.6f, 0f, 0.6f);
            UnityEditor.Handles.DrawWireDisc(centre, Vector3.back, total);

            if (jumpEnabled)
            {
                Vector3 groundCheck = transform.position + (Vector3)groundCheckOffset;
                UnityEditor.Handles.color = isGrounded
                    ? new Color(0.2f, 0.8f, 1f, 0.8f)
                    : new Color(1f, 0.35f, 0.15f, 0.8f);
                UnityEditor.Handles.DrawWireDisc(groundCheck, Vector3.back, groundCheckRadius);
            }

            UnityEditor.EditorGUI.BeginChangeCheck();
            Vector3 newCentre = UnityEditor.Handles.PositionHandle(centre, Quaternion.identity);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                UnityEditor.Undo.RecordObject(this, "Move Sprite Radius Offset");
                spriteRadiusOffset = newCentre - transform.position;
            }
        }
#endif
    }
}
