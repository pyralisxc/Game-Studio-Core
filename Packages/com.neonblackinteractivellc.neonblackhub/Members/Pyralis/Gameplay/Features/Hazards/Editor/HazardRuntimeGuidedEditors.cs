using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Zones;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards.Editor
{
    [CustomEditor(typeof(HazardFeedbackRuntime))]
    public sealed class HazardFeedbackRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Hazard Feedback Runtime",
                new PyralisGuideSection(
                    "What This Is",
                    "HazardFeedbackRuntime plays hazard flash feedback and world-space popup text from a HazardFeedbackProfile applied by a hazard runtime or setup route.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Hazard_Difficulty_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the hazard prefab or a child object that should own popup feedback.",
                        "Let the hazard setup call ApplyProfile with a HazardFeedbackProfile at runtime.",
                        "Assign Sprite Flasher directly, or keep Auto Find Sprite Flasher enabled when flash presets are used.",
                        "Assign Popup Camera when popup text should face the gameplay camera."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not expect feedback until a HazardFeedbackProfile is applied.",
                        "Do not disable Auto Find Sprite Flasher unless a Sprite Flasher is assigned manually.",
                        "Do not rely on a scene MainCamera tag for popup facing; wire Popup Camera explicitly.",
                        "Do not use this as the damage source; pair it with hazard impact or damage-zone logic."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetHazardFeedbackMessages(serializedObject), "HazardFeedbackRuntime is ready for hazard feedback profiles.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetHazardFeedbackMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty autoFind = serializedObject.FindProperty("autoFindSpriteFlasher");
            SerializedProperty spriteFlasher = serializedObject.FindProperty("spriteFlasher");
            SerializedProperty popupCamera = serializedObject.FindProperty("popupCamera");

            if (autoFind != null && !autoFind.boolValue && spriteFlasher != null && spriteFlasher.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Auto Find Sprite Flasher is disabled and Sprite Flasher is empty. Flash presets in the profile will have no visible effect."));

            if (popupCamera != null && popupCamera.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Popup Camera is empty. Assign the camera hazard popup text should face, or call SetPopupCamera when the hazard spawns."));

            return messages;
        }
    }

    [CustomEditor(typeof(DamageZone2D))]
    public sealed class DamageZone2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Damage Zone 2D",
                new PyralisGuideSection(
                    "What This Is",
                    "DamageZone2D applies repeated hazard impact or fallback damage to HealthComponent targets that stay inside a 2D trigger zone.",
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Hazard_Difficulty_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on a 2D trigger volume such as spikes, fire, acid, or a moving hazard hit zone.",
                        "Assign Hazard Impact Profile when damage, targeting, knockback, and feedback should come from authored hazard data.",
                        "Use fallback damage fields only for simple prototype zones without a profile.",
                        "Make sure intended targets have HealthComponent in their parent hierarchy."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not use a non-trigger collider; this zone uses OnTriggerEnter2D and OnTriggerExit2D.",
                        "Do not set Tick Interval to zero or very small values unless rapid damage is intended.",
                        "Do not leave targeting too broad in scenes with friendly NPCs or destructible props."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetDamageZoneMessages(serializedObject, (DamageZone2D)target), "DamageZone2D is ready for 2D hazard damage routing.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetDamageZoneMessages(SerializedObject serializedObject, DamageZone2D zone)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            Collider2D collider = zone != null ? zone.GetComponent<Collider2D>() : null;

            if (collider == null)
                messages.Add(PyralisGuideIssue.Required("Collider2D is required for trigger detection."));
            else if (!collider.isTrigger)
                messages.Add(PyralisGuideIssue.Required("Collider2D should be set to Is Trigger."));

            SerializedProperty impactProfile = serializedObject.FindProperty("impactProfile");
            SerializedProperty damage = serializedObject.FindProperty("damagePerTick");
            SerializedProperty tickInterval = serializedObject.FindProperty("tickInterval");
            SerializedProperty knockback = serializedObject.FindProperty("knockbackForce");

            if (impactProfile != null && impactProfile.objectReferenceValue == null)
            {
                if (damage != null && damage.floatValue <= 0f)
                    messages.Add(PyralisGuideIssue.Required("Fallback Damage Per Tick must be greater than zero when Impact Profile is empty."));
            }

            if (tickInterval != null && tickInterval.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Tick Interval must be greater than zero."));

            if (knockback != null && knockback.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Knockback Force cannot be negative."));

            return messages;
        }
    }
}
