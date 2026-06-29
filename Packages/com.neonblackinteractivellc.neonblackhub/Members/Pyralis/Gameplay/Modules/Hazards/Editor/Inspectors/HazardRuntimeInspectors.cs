using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Modules.Hazards;
using NeonBlack.Gameplay.Modules.Hazards.Zones;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Hazards.Editor
{
    [CustomEditor(typeof(HazardFeedbackRuntime))]
    public sealed class HazardFeedbackRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            InspectorValidation.DrawValidationMessages(GetHazardFeedbackMessages(serializedObject), "HazardFeedbackRuntime is ready for hazard feedback profiles.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<InspectorValidationIssue> GetHazardFeedbackMessages(SerializedObject serializedObject)
        {
            List<InspectorValidationIssue> messages = new List<InspectorValidationIssue>();
            SerializedProperty autoFind = serializedObject.FindProperty("autoFindSpriteFlasher");
            SerializedProperty spriteFlasher = serializedObject.FindProperty("spriteFlasher");
            SerializedProperty popupCamera = serializedObject.FindProperty("popupCamera");

            if (autoFind != null && !autoFind.boolValue && spriteFlasher != null && spriteFlasher.objectReferenceValue == null)
                messages.Add(InspectorValidationIssue.Optional("Auto Find Sprite Flasher is disabled and Sprite Flasher is empty. Flash entries in the profile will have no visible effect."));

            if (popupCamera != null && popupCamera.objectReferenceValue == null)
                messages.Add(InspectorValidationIssue.Recommended("Popup Camera is empty. Assign the camera hazard popup text should face, or call SetPopupCamera when the hazard spawns."));

            return messages;
        }
    }

    [CustomEditor(typeof(DamageZone2D))]
    public sealed class DamageZone2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            InspectorValidation.DrawValidationMessages(GetDamageZoneMessages(serializedObject, (DamageZone2D)target), "DamageZone2D is ready for 2D hazard damage routing.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<InspectorValidationIssue> GetDamageZoneMessages(SerializedObject serializedObject, DamageZone2D zone)
        {
            List<InspectorValidationIssue> messages = new List<InspectorValidationIssue>();
            Collider2D collider = zone != null ? zone.GetComponent<Collider2D>() : null;

            if (collider == null)
                messages.Add(InspectorValidationIssue.Required("Collider2D is required for trigger detection."));
            else if (!collider.isTrigger)
                messages.Add(InspectorValidationIssue.Required("Collider2D should be set to Is Trigger."));

            SerializedProperty impactProfile = serializedObject.FindProperty("impactProfile");

            if (impactProfile != null && impactProfile.objectReferenceValue == null)
                messages.Add(InspectorValidationIssue.Required("Hazard Impact Profile is required. Damage zones use profile-owned impact payloads."));

            return messages;
        }
    }
}
