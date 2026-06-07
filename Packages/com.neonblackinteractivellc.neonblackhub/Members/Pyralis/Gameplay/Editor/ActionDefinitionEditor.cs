using NeonBlack.Gameplay.Data.Definitions;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ActionDefinition))]
    public class ActionDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            ActionDefinition definition = (ActionDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Action Definition",
                "An action definition describes a reusable runtime action: what it is, when it executes, what it costs, and how it selects targets.",
                whenToUse: new[]
                {
                    "Use this for menu actions, turn-based commands, card actions, abilities, interact commands, and projectile-triggering actions.",
                    "Use target rules to support no-target actions, single target selection, area targeting, board-space targeting, or card selection."
                },
                createBefore: new[]
                {
                    "Runtime system or feature that knows how to execute this action id.",
                    "ProjectileDefinition or combat definition if this action drives combat output."
                },
                assignFirst: new[]
                {
                    "Set Action Id to a stable id such as action.attack.primary or action.card.play.",
                    "Set Action Family to group related actions in tools and menus.",
                    "Configure Target Rule before wiring UI selection."
                },
                safeToCustomize: new[]
                {
                    "Display Name is user/editor-facing and can be renamed freely.",
                    "Cooldown and Resource Cost can stay zero for simple actions.",
                    "Notes should explain what runtime system consumes the id."
                },
                validation: new[]
                {
                    "Action Id is unique and stable.",
                    "No-target actions use zero min/max targets.",
                    "Target counts match the UI or gameplay selection flow."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("AUTHORING_MODEL.md")));

            List<string> issues = definition != null ? definition.GetValidationIssues() : new List<string>();
            PyralisInspectorGuide.DrawValidationIssues(issues, "Action definition is ready for runtime wiring.");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
