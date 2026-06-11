using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Combat;
using UnityEditor;
using UnityEngine;
using static NeonBlack.Gameplay.Features.Combat.Editor.CombatDamageEditorUtility;

namespace NeonBlack.Gameplay.Features.Combat.Editor
{
    [CustomEditor(typeof(HitBox2D))]
    public sealed class HitBox2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: HitBox 2D",
                new PyralisGuideSection(
                    "What This Is",
                    "HitBox2D is a short-lived melee/contact damage trigger for 2D attacks. Animation events or combat scripts enable it for an active swing, then disable it again.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Health_Combat_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on an attack child such as a fist, weapon, or slash volume.",
                        "Add a Collider2D on the same GameObject and keep Is Trigger enabled.",
                        "Assign Owner to the attacking pawn root, or make sure a parent HealthComponent can be found at runtime.",
                        "Call EnableHitBox and DisableHitBox from animation events or the combat state that owns the active frames."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave the collider active as a normal body collider; this component enables and disables it as attack timing.",
                        "Do not use this 2D hitbox for 3D CharacterController actors.",
                        "Do not rely on base damage when a WeaponData asset is assigned; WeaponData overrides damage and knockback."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetHitBox2DMessages(serializedObject, (HitBox2D)target), "HitBox2D is ready for timed 2D attack contact.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetHitBox2DMessages(SerializedObject serializedObject, HitBox2D hitBox)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            GameObject root = hitBox != null ? hitBox.gameObject : null;
            Collider2D collider = root != null ? root.GetComponent<Collider2D>() : null;

            if (collider == null)
                messages.Add(PyralisGuideIssue.Required("Collider2D is required for 2D hit detection."));
            else if (!collider.isTrigger)
                messages.Add(PyralisGuideIssue.Required("Collider2D should be set to Is Trigger."));

            SerializedProperty owner = serializedObject.FindProperty("owner");
            if (root != null
                && owner != null
                && owner.objectReferenceValue == null
                && root.GetComponentInParent<HealthComponent>() == null)
            {
                messages.Add(PyralisGuideIssue.Optional("Owner is empty and no parent HealthComponent was found. Assign Owner so self/friendly hits can be filtered."));
            }

            SerializedProperty weapon = serializedObject.FindProperty("weapon");
            SerializedProperty baseDamage = serializedObject.FindProperty("baseDamage");
            if (weapon != null
                && weapon.objectReferenceValue == null
                && baseDamage != null
                && baseDamage.floatValue <= 0f)
            {
                messages.Add(PyralisGuideIssue.Required("Base Damage must be greater than zero when Weapon is empty."));
            }

            RequireNonNegative(serializedObject, messages, "knockbackForce", "Knockback Force");
            RequireNonNegative(serializedObject, messages, "freezeFrameDuration", "Freeze Frame Duration");
            SerializedProperty freezeFrameDuration = serializedObject.FindProperty("freezeFrameDuration");
            SerializedProperty hitPauseSink = serializedObject.FindProperty("hitPauseSink");
            if (freezeFrameDuration != null && freezeFrameDuration.floatValue > 0f)
            {
                if (hitPauseSink == null || hitPauseSink.objectReferenceValue == null)
                    messages.Add(PyralisGuideIssue.Recommended("Hit Pause Sink is empty. Assign TimeManager or another IHitPauseSink when Freeze Frame Duration is greater than zero."));
                else if (!ImplementsInterface(hitPauseSink.objectReferenceValue, "NeonBlack.Gameplay.Core.Contracts.IHitPauseSink"))
                    messages.Add(PyralisGuideIssue.Required("Hit Pause Sink must reference a component that implements IHitPauseSink."));
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

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Projectile",
                new PyralisGuideSection(
                    "What This Is",
                    "Projectile is the runtime projectile body launched by ranged combat. Launch supplies the owner, faction, damage, knockback, and speed.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Combat_Definitions_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Use this on a projectile prefab referenced by ranged WeaponData or a projectile launcher.",
                        "Keep a Rigidbody on the prefab root; the script configures interpolation, continuous collision, and gravity handling at runtime.",
                        "Add at least one 3D Collider with Is Trigger enabled so OnTriggerEnter can apply damage.",
                        "Tune Lifetime to clean up shots that miss every target."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not place this directly in a scene and expect it to move; Launch must be called after spawn.",
                        "Do not use Collider2D or Rigidbody2D with this 3D projectile.",
                        "Do not assign same-faction targets unless friendly fire is intended elsewhere; Projectile filters by faction."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetProjectileMessages(serializedObject, (Projectile)target), "Projectile is ready for ranged combat launch.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetProjectileMessages(SerializedObject serializedObject, Projectile projectile)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            GameObject root = projectile != null ? projectile.gameObject : null;

            if (root != null && root.GetComponent<Rigidbody>() == null)
                messages.Add(PyralisGuideIssue.Required("Rigidbody is required on the projectile root."));

            if (root != null)
            {
                Collider[] colliders = root.GetComponents<Collider>();
                if (colliders.Length == 0)
                {
                    messages.Add(PyralisGuideIssue.Required("Projectile needs at least one trigger Collider on the prefab root."));
                }
                else
                {
                    bool hasTrigger = false;
                    for (int i = 0; i < colliders.Length; i++)
                        hasTrigger |= colliders[i] != null && colliders[i].isTrigger;

                    if (!hasTrigger)
                        messages.Add(PyralisGuideIssue.Required("Projectile needs at least one trigger Collider so OnTriggerEnter can fire."));
                }
            }

            SerializedProperty lifetime = serializedObject.FindProperty("lifetime");
            if (lifetime != null && lifetime.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Lifetime must be greater than zero."));

            SerializedProperty hitPauseSink = serializedObject.FindProperty("hitPauseSink");
            if (hitPauseSink != null
                && hitPauseSink.objectReferenceValue != null
                && !ImplementsInterface(hitPauseSink.objectReferenceValue, "NeonBlack.Gameplay.Core.Contracts.IHitPauseSink"))
            {
                messages.Add(PyralisGuideIssue.Required("Hit Pause Sink must reference a component that implements IHitPauseSink."));
            }

            SerializedProperty cameraShakeSink = serializedObject.FindProperty("cameraShakeSink");
            if (cameraShakeSink != null
                && cameraShakeSink.objectReferenceValue != null
                && !ImplementsInterface(cameraShakeSink.objectReferenceValue, "NeonBlack.Gameplay.Core.Contracts.ICameraShakeSink"))
            {
                messages.Add(PyralisGuideIssue.Required("Camera Shake Sink must reference a component that implements ICameraShakeSink."));
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

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Projectile 2D",
                new PyralisGuideSection(
                    "What This Is",
                    "Projectile2D is the runtime projectile body for 2D ProjectileDefinition prefab delivery. ProjectileLauncher2D supplies damage, faction, lifetime, range, and impact behavior through ProjectileSpawnCommand.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Combat_Definitions_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Use this on a 2D projectile prefab referenced by ProjectileDefinition.",
                        "Keep a Rigidbody2D on the prefab root with Gravity Scale set for the intended shot style.",
                        "Add a Collider2D on the same GameObject and keep Is Trigger enabled.",
                        "Author damage, lifetime, friendly fire, max distance, and impact effects on ProjectileDefinition and ProjectileImpactDefinition."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not use the 3D Projectile component for Rigidbody2D shots.",
                        "Do not expect prefab-local damage values; the launcher command owns combat data.",
                        "Do not leave Collider2D as a solid body collider; this component expects trigger contact."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetProjectile2DMessages((Projectile2D)target), "Projectile2D is ready for 2D projectile prefab delivery.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetProjectile2DMessages(Projectile2D projectile)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            GameObject root = projectile != null ? projectile.gameObject : null;

            if (root != null && root.GetComponent<Rigidbody2D>() == null)
                messages.Add(PyralisGuideIssue.Required("Rigidbody2D is required on the projectile root."));

            Collider2D collider = root != null ? root.GetComponent<Collider2D>() : null;
            if (collider == null)
                messages.Add(PyralisGuideIssue.Required("Collider2D is required for 2D projectile trigger detection."));
            else if (!collider.isTrigger)
                messages.Add(PyralisGuideIssue.Required("Collider2D should be set to Is Trigger."));

            return messages;
        }
    }

    [CustomEditor(typeof(KnockbackReceiver))]
    public sealed class KnockbackReceiverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Knockback Receiver",
                new PyralisGuideSection(
                    "What This Is",
                    "KnockbackReceiver accumulates 3D knockback velocity so Motor3D, enemy movement, or another controller can fold that velocity into its CharacterController move.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Health_Combat_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the same GameObject as the actor CharacterController.",
                        "Make sure the actor movement script calls Tick(deltaTime) before reading Velocity each frame.",
                        "Use ApplyKnockback from HitBox, Projectile, hazard impact, or combat reaction code.",
                        "Tune resistance per actor: 1 accepts full knockback, 0 makes the actor immune."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not add this to 2D Rigidbody actors; 2D hitboxes apply Rigidbody2D impulses directly.",
                        "Do not read Velocity without ticking decay or knockback will linger longer than expected.",
                        "Do not set decay to zero unless knockback should never fade on its own."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetKnockbackMessages(serializedObject, (KnockbackReceiver)target), "KnockbackReceiver is ready for 3D actor knockback.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetKnockbackMessages(SerializedObject serializedObject, KnockbackReceiver receiver)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            GameObject root = receiver != null ? receiver.gameObject : null;

            if (root != null && root.GetComponent<CharacterController>() == null)
                messages.Add(PyralisGuideIssue.Required("CharacterController is required for 3D knockback."));

            RequireNonNegative(serializedObject, messages, "knockbackResistance", "Knockback Resistance");
            SerializedProperty decayRate = serializedObject.FindProperty("decayRate");
            if (decayRate != null && decayRate.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Decay Rate must be greater than zero."));

            return messages;
        }
    }

    [CustomEditor(typeof(HitFlash))]
    public sealed class HitFlashEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Hit Flash",
                new PyralisGuideSection(
                    "What This Is",
                    "HitFlash listens to sibling HealthComponent damage events and briefly flashes the SpriteRenderer using unscaled time so it still reads during freeze frames.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on a 2D damageable actor with SpriteRenderer and HealthComponent on the same GameObject.",
                        "Tune Flash Duration to overlap with the hit pause duration used by combat.",
                        "Use bright flash colors for impact confirmation and lower-contrast colors for subtle damage states."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put this on a child sprite if HealthComponent is only on the parent; this script reads the sibling HealthComponent.",
                        "Do not use it for 3D mesh flashes; use a renderer/material flash route instead.",
                        "Do not set duration to zero or the flash will be invisible."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetHitFlashMessages(serializedObject, (HitFlash)target), "HitFlash is ready for 2D damage feedback.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetHitFlashMessages(SerializedObject serializedObject, HitFlash hitFlash)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            GameObject root = hitFlash != null ? hitFlash.gameObject : null;

            if (root != null && root.GetComponent<SpriteRenderer>() == null)
                messages.Add(PyralisGuideIssue.Required("SpriteRenderer is required on the same GameObject."));

            if (root != null && root.GetComponent<HealthComponent>() == null)
                messages.Add(PyralisGuideIssue.Required("HealthComponent is required on the same GameObject."));

            SerializedProperty duration = serializedObject.FindProperty("flashDuration");
            if (duration != null && duration.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Flash Duration must be greater than zero."));

            return messages;
        }
    }

    [CustomEditor(typeof(DamageNumber))]
    public sealed class DamageNumberEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Damage Number",
                new PyralisGuideSection(
                    "What This Is",
                    "DamageNumber is a pooled world-space TextMeshPro popup built by DamageNumberSpawner for damage, critical hits, and healing.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Usually let DamageNumberSpawner create these at runtime instead of hand-placing them.",
                        "Tune motion, text, color, and outline settings on a prefab only when a custom pool route creates DamageNumber objects from that prefab.",
                        "Use outline when numbers need to remain readable over bright sprites, VFX, or tilemaps."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not add a TextMeshPro component manually; DamageNumber creates and configures one during Awake.",
                        "Do not set Lifetime or Font Size to zero.",
                        "Do not use this as the event listener; actor feedback or combat code should call DamageNumberSpawner."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetDamageNumberMessages(serializedObject), "DamageNumber is ready for pooled floating feedback.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetDamageNumberMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequirePositive(serializedObject, messages, "riseSpeed", "Rise Speed");
            RequireNonNegative(serializedObject, messages, "horizontalScatter", "Horizontal Scatter");
            RequirePositive(serializedObject, messages, "lifetime", "Lifetime");
            RequirePositive(serializedObject, messages, "fontSize", "Font Size");
            RequirePositive(serializedObject, messages, "criticalSizeMultiplier", "Critical Size Multiplier");
            return messages;
        }
    }

    [CustomEditor(typeof(DamageNumberSpawner))]
    public sealed class DamageNumberSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Damage Number Spawner",
                new PyralisGuideSection(
                    "What This Is",
                    "DamageNumberSpawner is the scene-level damage-number pool and IDamageNumberSink that creates DamageNumber objects and reuses them for combat and healing popups.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place one instance in the bootstrap scene or a persistent gameplay systems object.",
                        "Assign Popup Camera when numbers should billboard toward a specific gameplay camera.",
                        "Set Initial Pool Size high enough for the busiest expected burst of simultaneous hits.",
                        "Assign this component as Damage Number Sink on WorldHealthBar or ActorFloatingFeedbackReceiver when those components should show damage/heal numbers."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not rely on hidden global lookup; assign the spawner to consumers through their Damage Number Sink fields.",
                        "Do not assume there can be only one spawner; split-screen and multi-camera scenes can use one pool per camera.",
                        "Do not hand-parent pooled DamageNumber children; the spawner creates and returns them automatically.",
                        "Do not set the pool to zero unless runtime allocation spikes are acceptable.",
                        "Do not leave Popup Camera empty in split-screen, replay, or custom-camera scenes."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSpawnerMessages(serializedObject), "DamageNumberSpawner is ready as an explicit damage-number sink.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSpawnerMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty initialPoolSize = serializedObject.FindProperty("initialPoolSize");
            SerializedProperty popupCamera = serializedObject.FindProperty("popupCamera");
            if (initialPoolSize != null && initialPoolSize.intValue < 1)
                messages.Add(PyralisGuideIssue.Required("Initial Pool Size should be at least 1."));

            if (popupCamera != null && popupCamera.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Popup Camera is empty. Numbers will not billboard until a camera is assigned at authoring time or runtime."));
            return messages;
        }
    }

    internal static class CombatDamageEditorUtility
    {
        public static void RequirePositive(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required(displayName + " must be greater than zero."));
        }

        public static void RequireNonNegative(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required(displayName + " cannot be negative."));
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
