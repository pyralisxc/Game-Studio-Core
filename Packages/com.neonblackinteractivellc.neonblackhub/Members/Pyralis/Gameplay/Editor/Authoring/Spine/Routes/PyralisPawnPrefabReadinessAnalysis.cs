using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Input;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisPawnPrefabReadinessAnalysis
    {
        public static List<string> BuildIssues(PawnDefinition pawn)
        {
            List<string> issues = pawn != null
                ? pawn.GetValidationIssues()
                : new List<string>();

            AddIfPresent(issues, GetPawnPrefabGravityIssue(pawn));
            AddIfPresent(issues, GetPawnPrefabRotationIssue(pawn));
            AddIfPresent(issues, GetPawnPrefabSpriteScaleIssue(pawn));
            AddIfPresent(issues, GetPawnPrefabInputAdapterIssue(pawn));
            return issues;
        }

        private static string GetPawnPrefabGravityIssue(PawnDefinition pawn)
        {
            if (!TryGet2DPawnPrefabParts(pawn, out _, out _, out Rigidbody2D body, out _))
                return null;

            return body != null && Mathf.Abs(body.gravityScale) > 0.001f
                ? "Rigidbody2D gravity is non-zero on a Pawn2DMovementComponent prefab. Set Rigidbody2D > Gravity Scale to 0 for this native 2D pawn movement stack."
                : null;
        }

        private static string GetPawnPrefabRotationIssue(PawnDefinition pawn)
        {
            if (!TryGet2DPawnPrefabParts(pawn, out _, out _, out Rigidbody2D body, out _))
                return null;

            return body != null && (body.constraints & RigidbodyConstraints2D.FreezeRotation) == 0
                ? "Rigidbody2D rotation is not frozen on a Pawn2DMovementComponent prefab. Set Rigidbody2D > Constraints > Freeze Rotation so collision nudges do not spin the pawn."
                : null;
        }

        private static string GetPawnPrefabSpriteScaleIssue(PawnDefinition pawn)
        {
            if (!TryGet2DPawnPrefabParts(pawn, out _, out Pawn2DMovementComponent movement2D, out _, out SpriteRenderer spriteRenderer))
                return null;

            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return null;

            float largestVisualExtent = Mathf.Max(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
            float spriteRadius = GetSerializedFloat(movement2D, "spriteRadius", 0.32f);
            float expectedPawnExtent = Mathf.Max(6f, spriteRadius * 8f);
            return largestVisualExtent > expectedPawnExtent
                ? "The pawn prefab SpriteRenderer uses an environment-sized sprite. Drag a character sprite from Project onto SpriteRenderer > Sprite, and keep floor/map art on separate scene objects."
                : null;
        }

        private static string GetPawnPrefabInputAdapterIssue(PawnDefinition pawn)
        {
            if (!TryGet2DPawnPrefabParts(pawn, out GameObject prefab, out _, out _, out _))
                return null;

            Motor2DInputAdapter inputAdapter = prefab.GetComponent<Motor2DInputAdapter>();
            PlayerInputHandler[] inputHandlers = prefab.GetComponents<PlayerInputHandler>();
            if (inputAdapter == null && inputHandlers.Length == 0)
                return "The player-owned 2D pawn prefab has no input module. Use Inspector > Add Component on the pawn prefab root and add Motor2DInputAdapter so InputProfile actions can reach movement.";

            if (inputAdapter != null && inputHandlers.Length > 1)
                return "The pawn prefab has both Motor2DInputAdapter and an extra PlayerInputHandler. Keep Motor2DInputAdapter for the supported 2D player-input bridge, and remove the duplicate 2D Player Input Handler before the movement proof.";

            return null;
        }

        private static bool TryGet2DPawnPrefabParts(
            PawnDefinition pawn,
            out GameObject prefab,
            out Pawn2DMovementComponent movement2D,
            out Rigidbody2D body,
            out SpriteRenderer spriteRenderer)
        {
            prefab = pawn != null ? pawn.pawnPrefab : null;
            movement2D = prefab != null ? prefab.GetComponent<Pawn2DMovementComponent>() : null;
            body = prefab != null ? prefab.GetComponent<Rigidbody2D>() : null;
            spriteRenderer = prefab != null ? prefab.GetComponentInChildren<SpriteRenderer>(true) : null;
            return prefab != null && movement2D != null;
        }

        private static void AddIfPresent(List<string> issues, string issue)
        {
            if (!string.IsNullOrWhiteSpace(issue))
                issues.Add(issue);
        }

        private static float GetSerializedFloat(Object target, string propertyName, float fallback)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null ? property.floatValue : fallback;
        }
    }
}
