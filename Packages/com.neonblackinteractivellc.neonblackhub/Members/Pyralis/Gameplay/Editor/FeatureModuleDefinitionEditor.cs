using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(FeatureModuleDefinition))]
    public class FeatureModuleDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            FeatureModuleDefinition definition = (FeatureModuleDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Feature Module Definition",
                "A feature module definition describes an optional actor or pawn capability that can be installed through a PawnDefinition or required by a GameModeDefinition.",
                whenToUse: new[]
                {
                    "Use this for modular capabilities such as pickups, interaction, feedback, status effects, combat reactions, traversal, or custom actor features.",
                    "Use a feature module when the capability should be reusable across multiple pawns or modes."
                },
                createBefore: new[]
                {
                    "Runtime prefab containing a component that implements IFeatureModuleRuntime.",
                    "Profile asset if this module needs authored tuning.",
                    "PawnDefinition or GameModeDefinition that will reference this module."
                },
                assignFirst: new[]
                {
                    "Set Module Id and Display Name.",
                    "Assign Runtime Prefab.",
                    "Assign Profile Asset when the module expects one.",
                    "Set Supported Presentation Modes when the feature only works for 2D, 2.5D, or rigged 3D."
                },
                safeToCustomize: new[]
                {
                    "Install Order controls feature setup order when multiple modules are installed.",
                    "Network Role can stay OfflineOnly for local-first features.",
                    "Notes should explain project-specific setup assumptions."
                },
                validation: new[]
                {
                    "Runtime prefab implements IFeatureModuleRuntime.",
                    "Profile Asset type matches the module id expectation.",
                    "Supported presentation modes match the pawn presentation profile."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Feature_Module_Framework_Setup.md")));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Platform Metadata", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Network Role", definition.networkRole.ToString());
            EditorGUILayout.LabelField("Authoring Category", string.IsNullOrWhiteSpace(definition.authoringCategory) ? "(Missing)" : definition.authoringCategory);
            EditorGUILayout.LabelField("Gizmo Mode", definition.gizmoMode.ToString());
            DrawMatchedContract(definition);

            if (GUILayout.Button("Sanitize Metadata"))
            {
                Undo.RecordObject(definition, "Sanitize Feature Module Metadata");
                definition.Sanitize();
                EditorUtility.SetDirty(definition);
            }

            List<string> issues = definition.GetValidationIssues();
            issues.AddRange(PyralisFeatureModuleContractValidator.GetValidationIssues(definition));
            PyralisInspectorGuide.DrawValidationIssues(issues, "Feature module definition is ready for pawn or mode assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawMatchedContract(FeatureModuleDefinition definition)
        {
            PyralisAuthoringContract contract = PyralisAuthoringContractRegistry.FindByModuleId(definition.moduleId);
            if (contract == null)
            {
                EditorGUILayout.LabelField("Authoring Contract", "No reflective contract matched this Module Id.");
                return;
            }

            EditorGUILayout.LabelField("Authoring Contract", contract.DisplayName);
            DrawMiniContractList("Profile", contract.RequiredProfileType != null ? new[] { contract.RequiredProfileType.Name } : null);
            DrawMiniContractList("Runtime Interfaces", contract.RequiredRuntimeInterfaceNames);
            DrawMiniContractList("Actions", contract.ConsumedActionRoles);
            DrawMiniContractList("Assignment Fields", contract.AssignmentFields);
            DrawMiniContractList("Customization", contract.CustomizationMoments);
            if (!string.IsNullOrWhiteSpace(contract.FirstProofTargetId))
                EditorGUILayout.LabelField("First Proof Target", contract.FirstProofTargetId);
        }

        private static void DrawMiniContractList(string label, string[] values)
        {
            if (values == null || values.Length == 0)
                return;

            EditorGUILayout.LabelField(label, string.Join(", ", values), EditorStyles.wordWrappedMiniLabel);
        }
    }

    public static class PyralisFeatureModuleContractValidator
    {
        private const string FeatureRuntimeInterfaceName = "NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime";

        public static List<string> GetValidationIssues(FeatureModuleDefinition definition)
        {
            List<string> issues = new List<string>();
            if (definition == null)
                return issues;

            PyralisAuthoringContract contract = PyralisAuthoringContractRegistry.FindByModuleId(definition.moduleId);
            if (contract == null)
                return issues;

            if (contract.RequiredProfileType != null && !contract.RequiredProfileType.IsInstanceOfType(definition.profileAsset))
            {
                issues.Add($"`{contract.StableId}` expects a {contract.RequiredProfileType.Name} profile asset.");
            }

            if (definition.supportedPresentationModes != null && definition.supportedPresentationModes.Length > 0)
            {
                for (int i = 0; i < definition.supportedPresentationModes.Length; i++)
                {
                    ActorPresentationMode mode = definition.supportedPresentationModes[i];
                    if (contract.IsExplicitlyUnsupported(mode))
                    {
                        if (!string.IsNullOrWhiteSpace(contract.UnsupportedLaneMessage))
                            issues.Add(contract.UnsupportedLaneMessage);
                    }
                    else if (!contract.SupportsPresentationMode(mode))
                    {
                        issues.Add($"`{contract.StableId}` does not declare support for {mode} presentation.");
                    }
                }
            }

            AppendRuntimeInterfaceIssues(definition, contract, issues);
            return issues;
        }

        private static void AppendRuntimeInterfaceIssues(FeatureModuleDefinition definition, PyralisAuthoringContract contract, List<string> issues)
        {
            if (definition.runtimePrefab == null)
                return;

            string[] requiredRuntimeInterfaceNames = contract.RequiredRuntimeInterfaceNames;
            if (requiredRuntimeInterfaceNames == null || requiredRuntimeInterfaceNames.Length == 0)
                return;

            MonoBehaviour[] behaviours = definition.runtimePrefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < requiredRuntimeInterfaceNames.Length; i++)
            {
                string requiredInterfaceName = requiredRuntimeInterfaceNames[i];
                if (string.IsNullOrWhiteSpace(requiredInterfaceName))
                    continue;

                if (string.Equals(requiredInterfaceName, FeatureRuntimeInterfaceName, System.StringComparison.Ordinal))
                    continue;

                if (HasComponentImplementing(behaviours, requiredInterfaceName))
                    continue;

                issues.Add(
                    $"`{definition.moduleId}` runtime prefab should expose {GetShortTypeName(requiredInterfaceName)}.");
            }
        }

        private static bool HasComponentImplementing(MonoBehaviour[] behaviours, string interfaceFullTypeName)
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                System.Type[] interfaces = behaviour.GetType().GetInterfaces();
                for (int j = 0; j < interfaces.Length; j++)
                {
                    if (string.Equals(interfaces[j].FullName, interfaceFullTypeName, System.StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        private static string GetShortTypeName(string fullTypeName)
        {
            int separatorIndex = fullTypeName.LastIndexOf('.');
            if (separatorIndex < 0 || separatorIndex + 1 >= fullTypeName.Length)
                return fullTypeName;

            return fullTypeName.Substring(separatorIndex + 1);
        }
    }
}
