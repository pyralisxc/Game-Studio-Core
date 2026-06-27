using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Modules.Combat;
using UnityEditor;
using UnityEngine;
using static NeonBlack.Gameplay.Modules.Combat.Editor.CombatDamageEditorUtility;

namespace NeonBlack.Gameplay.Modules.Combat.Editor
{
    [CustomEditor(typeof(HitBox2D))]
    public sealed class HitBox2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("HitBox 2D", null);

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetHitBox2DMessages(serializedObject, (HitBox2D)target), "HitBox2D is ready for timed 2D attack contact.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetHitBox2DMessages(SerializedObject serializedObject, HitBox2D hitBox)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            GameObject root = hitBox != null ? hitBox.gameObject : null;
            Collider2D collider = root != null ? root.GetComponent<Collider2D>() : null;

            if (collider == null)
                messages.Add(PyralisInspectorValidationIssue.Required("Collider2D is required for 2D hit detection."));
            else if (!collider.isTrigger)
                messages.Add(PyralisInspectorValidationIssue.Required("Collider2D should be set to Is Trigger."));

            SerializedProperty owner = serializedObject.FindProperty("owner");
            if (root != null
                && owner != null
                && owner.objectReferenceValue == null
                && root.GetComponentInParent<HealthComponent>() == null)
            {
                messages.Add(PyralisInspectorValidationIssue.Optional("Owner is empty and no parent HealthComponent was found. Assign Owner so self/friendly hits can be filtered."));
            }

            SerializedProperty weapon = serializedObject.FindProperty("weapon");
            SerializedProperty baseDamage = serializedObject.FindProperty("baseDamage");
            if (weapon != null
                && weapon.objectReferenceValue == null
                && baseDamage != null
                && baseDamage.floatValue <= 0f)
            {
                messages.Add(PyralisInspectorValidationIssue.Required("Base Damage must be greater than zero when Weapon is empty."));
            }

            RequireNonNegative(serializedObject, messages, "knockbackForce", "Knockback Force");
            RequireNonNegative(serializedObject, messages, "freezeFrameDuration", "Freeze Frame Duration");
            SerializedProperty freezeFrameDuration = serializedObject.FindProperty("freezeFrameDuration");
            SerializedProperty hitPauseSink = serializedObject.FindProperty("hitPauseSink");
            if (freezeFrameDuration != null && freezeFrameDuration.floatValue > 0f)
            {
                if (hitPauseSink == null || hitPauseSink.objectReferenceValue == null)
                    messages.Add(PyralisInspectorValidationIssue.Recommended("Hit Pause Sink is empty. Assign TimeManager or another IHitPauseSink when Freeze Frame Duration is greater than zero."));
                else if (!ImplementsInterface(hitPauseSink.objectReferenceValue, "NeonBlack.Gameplay.Core.Contracts.IHitPauseSink"))
                    messages.Add(PyralisInspectorValidationIssue.Required("Hit Pause Sink must reference a component that implements IHitPauseSink."));
            }
            return messages;
        }
    }

    [CustomEditor(typeof(Projectile))]
    public sealed class ProjectileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("Projectile", null);

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetProjectileMessages(serializedObject, (Projectile)target), "Projectile is ready for ranged combat launch.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetProjectileMessages(SerializedObject serializedObject, Projectile projectile)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            GameObject root = projectile != null ? projectile.gameObject : null;

            if (root != null && root.GetComponent<Rigidbody>() == null)
                messages.Add(PyralisInspectorValidationIssue.Required("Rigidbody is required on the projectile root."));

            if (root != null)
            {
                Collider[] colliders = root.GetComponents<Collider>();
                if (colliders.Length == 0)
                {
                    messages.Add(PyralisInspectorValidationIssue.Required("Projectile needs at least one trigger Collider on the prefab root."));
                }
                else
                {
                    bool hasTrigger = false;
                    for (int i = 0; i < colliders.Length; i++)
                        hasTrigger |= colliders[i] != null && colliders[i].isTrigger;

                    if (!hasTrigger)
                        messages.Add(PyralisInspectorValidationIssue.Required("Projectile needs at least one trigger Collider so OnTriggerEnter can fire."));
                }
            }

            SerializedProperty lifetime = serializedObject.FindProperty("lifetime");
            if (lifetime != null && lifetime.floatValue <= 0f)
                messages.Add(PyralisInspectorValidationIssue.Required("Lifetime must be greater than zero."));

            SerializedProperty hitPauseSink = serializedObject.FindProperty("hitPauseSink");
            if (hitPauseSink != null
                && hitPauseSink.objectReferenceValue != null
                && !ImplementsInterface(hitPauseSink.objectReferenceValue, "NeonBlack.Gameplay.Core.Contracts.IHitPauseSink"))
            {
                messages.Add(PyralisInspectorValidationIssue.Required("Hit Pause Sink must reference a component that implements IHitPauseSink."));
            }

            SerializedProperty cameraShakeSink = serializedObject.FindProperty("cameraShakeSink");
            if (cameraShakeSink != null
                && cameraShakeSink.objectReferenceValue != null
                && !ImplementsInterface(cameraShakeSink.objectReferenceValue, "NeonBlack.Gameplay.Core.Contracts.ICameraShakeSink"))
            {
                messages.Add(PyralisInspectorValidationIssue.Required("Camera Shake Sink must reference a component that implements ICameraShakeSink."));
            }

            RequireNonNegative(serializedObject, messages, "gravityScale", "Gravity Scale");
            return messages;
        }
    }

    [CustomEditor(typeof(Projectile2D))]
    public sealed class Projectile2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("Projectile 2D", null);

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetProjectile2DMessages((Projectile2D)target), "Projectile2D is ready for 2D projectile prefab delivery.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetProjectile2DMessages(Projectile2D projectile)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            GameObject root = projectile != null ? projectile.gameObject : null;

            if (root != null && root.GetComponent<Rigidbody2D>() == null)
                messages.Add(PyralisInspectorValidationIssue.Required("Rigidbody2D is required on the projectile root."));

            Collider2D collider = root != null ? root.GetComponent<Collider2D>() : null;
            if (collider == null)
                messages.Add(PyralisInspectorValidationIssue.Required("Collider2D is required for 2D projectile trigger detection."));
            else if (!collider.isTrigger)
                messages.Add(PyralisInspectorValidationIssue.Required("Collider2D should be set to Is Trigger."));

            return messages;
        }
    }

    [CustomEditor(typeof(KnockbackReceiver))]
    public sealed class KnockbackReceiverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("Knockback Receiver", null);

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetKnockbackMessages(serializedObject, (KnockbackReceiver)target), "KnockbackReceiver is ready for 3D actor knockback.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetKnockbackMessages(SerializedObject serializedObject, KnockbackReceiver receiver)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            GameObject root = receiver != null ? receiver.gameObject : null;

            if (root != null && root.GetComponent<CharacterController>() == null)
                messages.Add(PyralisInspectorValidationIssue.Required("CharacterController is required for 3D knockback."));

            RequireNonNegative(serializedObject, messages, "knockbackResistance", "Knockback Resistance");
            SerializedProperty decayRate = serializedObject.FindProperty("decayRate");
            if (decayRate != null && decayRate.floatValue <= 0f)
                messages.Add(PyralisInspectorValidationIssue.Required("Decay Rate must be greater than zero."));

            return messages;
        }
    }

    [CustomEditor(typeof(HitFlash))]
    public sealed class HitFlashEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("Hit Flash", null);

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetHitFlashMessages(serializedObject, (HitFlash)target), "HitFlash is ready for 2D damage feedback.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetHitFlashMessages(SerializedObject serializedObject, HitFlash hitFlash)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            GameObject root = hitFlash != null ? hitFlash.gameObject : null;

            if (root != null && root.GetComponent<SpriteRenderer>() == null)
                messages.Add(PyralisInspectorValidationIssue.Required("SpriteRenderer is required on the same GameObject."));

            if (root != null && root.GetComponent<HealthComponent>() == null)
                messages.Add(PyralisInspectorValidationIssue.Required("HealthComponent is required on the same GameObject."));

            SerializedProperty duration = serializedObject.FindProperty("flashDuration");
            if (duration != null && duration.floatValue <= 0f)
                messages.Add(PyralisInspectorValidationIssue.Required("Flash Duration must be greater than zero."));

            return messages;
        }
    }

    internal static class CombatDamageEditorUtility
    {
        public static void RequirePositive(SerializedObject serializedObject, List<PyralisInspectorValidationIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.floatValue <= 0f)
                messages.Add(PyralisInspectorValidationIssue.Required(displayName + " must be greater than zero."));
        }

        public static void RequireNonNegative(SerializedObject serializedObject, List<PyralisInspectorValidationIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.floatValue < 0f)
                messages.Add(PyralisInspectorValidationIssue.Required(displayName + " cannot be negative."));
        }

        public static bool ImplementsInterface(Object target, string interfaceName)
        {
            if (target == null)
                return false;

            System.Type[] interfaces = target.GetType().GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].FullName == interfaceName)
                    return true;
            }

            return false;
        }
    }
}
