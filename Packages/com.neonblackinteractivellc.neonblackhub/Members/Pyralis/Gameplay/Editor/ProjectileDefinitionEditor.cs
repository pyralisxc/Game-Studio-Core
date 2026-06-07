using NeonBlack.Gameplay.Features.Combat;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ProjectileDefinition))]
    public class ProjectileDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            ProjectileDefinition definition = (ProjectileDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Projectile Definition",
                "A projectile definition describes a fired, thrown, or hitscan combat output and how it links to action targeting and impact feedback.",
                whenToUse: new[]
                {
                    "Use this for bullets, fireballs, thrown weapons, beams, rays, and ability projectiles.",
                    "Use Hitscan for instant line/ray attacks and Projectile Prefab for moving physical projectiles."
                },
                createBefore: new[]
                {
                    "Projectile prefab when Delivery Mode is Projectile Prefab.",
                    "ProjectileImpactDefinition for hit/miss feedback.",
                    "ActionDefinition if this projectile is selected by an action/menu/card system."
                },
                assignFirst: new[]
                {
                    "Set Projectile Id and Display Name.",
                    "Choose Delivery Mode.",
                    "Assign Projectile Prefab or configure Hitscan Max Distance.",
                    "Set damage, knockback, speed, lifetime, and friendly fire."
                },
                safeToCustomize: new[]
                {
                    "Action Definition can stay empty for direct weapon-fired projectiles.",
                    "Impact Definition can be shared across many projectiles.",
                    "Friendly fire should be explicit per game mode."
                },
                validation: new[]
                {
                    "Prefab delivery has a projectile prefab and positive speed.",
                    "Hitscan delivery has max distance greater than zero.",
                    "Damage/knockback values match the weapon or action economy."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")));

            PyralisInspectorGuide.DrawValidationMessages(
                GetValidationMessages(definition),
                "Projectile definition is ready for weapon or action assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetValidationMessages(ProjectileDefinition definition)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (definition == null)
                return messages;

            List<string> issues = definition.GetValidationIssues();
            for (int i = 0; i < issues.Count; i++)
                messages.Add(PyralisGuideIssue.Required(issues[i]));

            if (definition.deliveryMode != ProjectileDeliveryMode.ProjectilePrefab || definition.projectilePrefab == null)
                return messages;

            if (!HasRuntimeBody(definition.projectilePrefab))
            {
                messages.Add(PyralisGuideIssue.Required(
                    "Projectile prefab delivery requires a runtime body: add Projectile for 3D prefabs or Projectile2D for Rigidbody2D prefabs so damage, lifetime, max distance, friendly fire, and impact behavior come from ProjectileDefinition."));
            }

            bool has3DPhysics = definition.projectilePrefab.GetComponentInChildren<Rigidbody>(true) != null
                || definition.projectilePrefab.GetComponentInChildren<Collider>(true) != null;
            bool has2DPhysics = definition.projectilePrefab.GetComponentInChildren<Rigidbody2D>(true) != null
                || definition.projectilePrefab.GetComponentInChildren<Collider2D>(true) != null;

            if (!has3DPhysics && !has2DPhysics)
            {
                messages.Add(PyralisGuideIssue.Required(
                    "Projectile prefab needs 2D or 3D physics components so launcher movement and hit detection have a real Unity runtime surface."));
            }

            if (has2DPhysics && has3DPhysics)
            {
                messages.Add(PyralisGuideIssue.Recommended(
                    "Projectile prefab mixes 2D and 3D physics components. Keep one physics lane per prefab: Projectile2D with Rigidbody2D/Collider2D or Projectile with Rigidbody/Collider."));
            }

            return messages;
        }

        private static bool HasRuntimeBody(GameObject prefab)
        {
            if (prefab == null)
                return false;

            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IProjectileRuntimeBody)
                    return true;
            }

            return false;
        }
    }
}
