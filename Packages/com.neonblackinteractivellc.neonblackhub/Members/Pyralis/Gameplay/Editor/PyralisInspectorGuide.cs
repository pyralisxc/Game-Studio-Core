using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using System.Text;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Features.Composition;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public readonly struct PyralisGuideContent
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string[] WhenToUse;
        public readonly string[] CreateBefore;
        public readonly string[] AssignFirst;
        public readonly string[] SafeToCustomize;
        public readonly string[] Validation;
        public readonly string ManualPath;

        public PyralisGuideContent(
            string title,
            string summary,
            string[] whenToUse = null,
            string[] createBefore = null,
            string[] assignFirst = null,
            string[] safeToCustomize = null,
            string[] validation = null,
            string manualPath = null)
        {
            Title = title;
            Summary = summary;
            WhenToUse = whenToUse;
            CreateBefore = createBefore;
            AssignFirst = assignFirst;
            SafeToCustomize = safeToCustomize;
            Validation = validation;
            ManualPath = manualPath;
        }
    }

    public readonly struct PyralisGuideSection
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string[] Items;
        public readonly string ManualPath;

        public PyralisGuideSection(string title, string summary = null, string[] items = null, string manualPath = null)
        {
            Title = title;
            Summary = summary;
            Items = items;
            ManualPath = manualPath;
        }
    }

    public enum PyralisGuideIssueSeverity
    {
        RequiredFix,
        Recommended,
        Optional
    }

    public readonly struct PyralisGuideIssue
    {
        public readonly string Message;
        public readonly PyralisGuideIssueSeverity Severity;

        public PyralisGuideIssue(string message, PyralisGuideIssueSeverity severity = PyralisGuideIssueSeverity.RequiredFix)
        {
            Message = message;
            Severity = severity;
        }

        public static PyralisGuideIssue Required(string message)
        {
            return new PyralisGuideIssue(message, PyralisGuideIssueSeverity.RequiredFix);
        }

        public static PyralisGuideIssue Recommended(string message)
        {
            return new PyralisGuideIssue(message, PyralisGuideIssueSeverity.Recommended);
        }

        public static PyralisGuideIssue Optional(string message)
        {
            return new PyralisGuideIssue(message, PyralisGuideIssueSeverity.Optional);
        }
    }

    public static class PyralisInspectorGuide
    {
        private const string ManualRoot = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/";
        private const string InspectorHandoffText = "Use this Inspector for field assignment, local customization, and field-local validation. Use the Pyralis Authoring Window for route setup, native workflow steps, first playable proof, required/recommended/later guidance, and whole-game validation.";

        public static void DrawGuide(PyralisGuideContent content)
        {
            DrawGuideContent(content, true);
            PyralisInspectorHandoff.DrawAuthoringButton();
        }

        public static void DrawFieldGuide(string title, params PyralisGuideSection[] sections)
        {
            DrawFieldGuide(title, false, sections);
        }

        public static void DrawFieldGuide(string title, bool defaultOpen = false, params PyralisGuideSection[] sections)
        {
            DrawFieldGuideContent(title, defaultOpen, sections);
            PyralisInspectorHandoff.DrawAuthoringButton();
        }

        public static void DrawValidationIssues(IReadOnlyList<string> issues, string readyMessage = "No setup issues found.")
        {
            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox(readyMessage, MessageType.Info);
                return;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < issues.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(issues[i]))
                    continue;

                builder.Append("- ");
                builder.AppendLine(issues[i]);
            }

            if (builder.Length > 0)
                EditorGUILayout.HelpBox(builder.ToString().Trim(), MessageType.Warning);
        }

        public static void DrawValidationMessages(IReadOnlyList<PyralisGuideIssue> issues, string readyMessage = "No setup issues found.")
        {
            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox(readyMessage, MessageType.Info);
                return;
            }

            StringBuilder required = new StringBuilder();
            StringBuilder recommended = new StringBuilder();
            StringBuilder optional = new StringBuilder();

            for (int i = 0; i < issues.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(issues[i].Message))
                    continue;

                StringBuilder target = issues[i].Severity switch
                {
                    PyralisGuideIssueSeverity.RequiredFix => required,
                    PyralisGuideIssueSeverity.Recommended => recommended,
                    _ => optional
                };

                target.Append("- ");
                target.AppendLine(issues[i].Message);
            }

            if (required.Length > 0)
                EditorGUILayout.HelpBox("Required fixes\n" + required.ToString().Trim(), MessageType.Error);
            if (recommended.Length > 0)
                EditorGUILayout.HelpBox("Recommended checks\n" + recommended.ToString().Trim(), MessageType.Warning);
            if (optional.Length > 0)
                EditorGUILayout.HelpBox("Optional context\n" + optional.ToString().Trim(), MessageType.Info);
        }

        public static string BuildChecklist(PyralisGuideContent content)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(content.Title))
                builder.AppendLine(content.Title);

            if (builder.Length > 0)
                builder.AppendLine();
            builder.AppendLine(InspectorHandoffText);

            return builder.ToString().Trim();
        }

        public static string SetupManualPath(string relativePath)
        {
            return string.IsNullOrWhiteSpace(relativePath) ? ManualRoot + "MANUAL.md" : ManualRoot + relativePath;
        }

        private static void DrawGuideContent(PyralisGuideContent content, bool defaultOpen)
        {
            string title = string.IsNullOrWhiteSpace(content.Title) ? "Guided Authoring" : content.Title;
            if (!SessionFoldout(title, defaultOpen))
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawHandoffReminder();
            DrawWrappedLabel(content.Summary);
            DrawList("When To Use", content.WhenToUse);
            DrawList("Create Before", content.CreateBefore);
            DrawList("Assign First", content.AssignFirst);
            DrawList("Safe To Customize", content.SafeToCustomize);
            DrawList("Validation", content.Validation);
            DrawManualPath(content.ManualPath);
            EditorGUILayout.EndVertical();
        }

        private static void DrawFieldGuideContent(string title, bool defaultOpen, IReadOnlyList<PyralisGuideSection> sections)
        {
            title = string.IsNullOrWhiteSpace(title) ? "Inspector Field Guide" : title;
            if (!SessionFoldout(title, defaultOpen))
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawHandoffReminder();
            if (sections != null)
            {
                for (int i = 0; i < sections.Count; i++)
                {
                    PyralisGuideSection section = sections[i];
                    if (!string.IsNullOrWhiteSpace(section.Title))
                        EditorGUILayout.LabelField(section.Title, EditorStyles.boldLabel);
                    DrawWrappedLabel(section.Summary);
                    DrawList(null, section.Items);
                    DrawManualPath(section.ManualPath);
                    if (i < sections.Count - 1)
                        EditorGUILayout.Space(4f);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static bool SessionFoldout(string title, bool defaultOpen)
        {
            string key = "PyralisInspectorGuide." + title;
            bool open = SessionState.GetBool(key, defaultOpen);
            open = EditorGUILayout.Foldout(open, title, true);
            SessionState.SetBool(key, open);
            return open;
        }

        private static void DrawList(string title, IReadOnlyList<string> items)
        {
            if (items == null || items.Count == 0)
                return;

            if (!string.IsNullOrWhiteSpace(title))
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(items[i]))
                    continue;

                DrawWrappedLabel("- " + items[i]);
            }
        }

        private static void DrawWrappedLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            EditorGUILayout.LabelField(value, EditorStyles.wordWrappedLabel);
        }

        private static void DrawHandoffReminder()
        {
            EditorGUILayout.LabelField("Inspector = local fields. Authoring Window = route map and next step.", EditorStyles.wordWrappedMiniLabel);
            PyralisAuthoringSurfaceBeacon.DrawBeaconRow(
                PyralisAuthoringActionSurface.Inspector,
                PyralisAuthoringActionSurface.AuthoringWindow,
                PyralisAuthoringActionSurface.ProjectWindow,
                PyralisAuthoringActionSurface.Hierarchy);
            EditorGUILayout.Space(3f);
        }

        private static void DrawManualPath(string manualPath)
        {
            if (string.IsNullOrWhiteSpace(manualPath))
                return;

            EditorGUILayout.LabelField("Manual", manualPath, EditorStyles.miniLabel);
        }
    }

    public static class PyralisAuthoringContractGuideText
    {
        public static string FeatureModuleSetup(IFeatureModuleRuntime runtime)
        {
            return FeatureModuleSetup(runtime != null ? runtime.ModuleId : null);
        }

        public static string FeatureModuleSetup(string moduleId)
        {
            NeonBlack.Gameplay.Core.Contracts.PyralisAuthoringContract contract = NeonBlack.Gameplay.Core.Contracts.PyralisAuthoringContractRegistry.FindByModuleId(moduleId);
if (contract == null)
                return string.IsNullOrWhiteSpace(moduleId)
                    ? "Use a FeatureModuleDefinition whose module id matches this feature runtime."
                    : "Use a FeatureModuleDefinition with module id `" + moduleId + "`.";

            string profileName = RequiredProfileName(contract, null);
            if (string.IsNullOrWhiteSpace(profileName))
                return "Use a FeatureModuleDefinition with module id `" + contract.StableId + "`.";

            return "Use a FeatureModuleDefinition with module id `" + contract.StableId + "` and a " + profileName + ".";
        }

        public static string RequiredProfileName(IFeatureModuleRuntime runtime, string fallback)
        {
            string moduleId = runtime != null ? runtime.ModuleId : null;
            NeonBlack.Gameplay.Core.Contracts.PyralisAuthoringContract contract = NeonBlack.Gameplay.Core.Contracts.PyralisAuthoringContractRegistry.FindByModuleId(moduleId);
return RequiredProfileName(contract, fallback);
        }

        public static string RequiredProfileName(string moduleId, string fallback)
        {
            NeonBlack.Gameplay.Core.Contracts.PyralisAuthoringContract contract = NeonBlack.Gameplay.Core.Contracts.PyralisAuthoringContractRegistry.FindByModuleId(moduleId);
return RequiredProfileName(contract, fallback);
        }

        private static string RequiredProfileName(NeonBlack.Gameplay.Core.Contracts.PyralisAuthoringContract contract, string fallback)
        {
            if (contract != null && contract.RequiredProfileType != null)
                return contract.RequiredProfileType.Name;

            return fallback ?? string.Empty;
        }
    }
}
