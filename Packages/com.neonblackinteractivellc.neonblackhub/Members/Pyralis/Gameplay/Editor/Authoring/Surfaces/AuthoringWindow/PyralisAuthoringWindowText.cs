using NeonBlack.Gameplay.Editor.Authoring;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringWindowText
    {
        private static readonly SemanticTokenRule[] SemanticTokenRules =
        {
            new SemanticTokenRule(PyralisAuthoringSemanticTag.PlayMode, "Play Mode proof"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.PlayMode, "Play Mode"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.PlayMode, "proof"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Project, "Project window"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Hierarchy, "Hierarchy window"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Inspector, "Inspector Add Component"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Inspector, "Object Picker"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Input, "Input Action Asset"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Project, "CreateAssetMenu"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Component, "AddComponentMenu"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Component, "RequireComponent"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Definition, "Definition"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Profile, "Profile"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Prefab, "Prefab"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Component, "Component"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Project, "Project"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Hierarchy, "Hierarchy"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Inspector, "Inspector"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Input, "Input"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.UI, "Canvas"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.UI, "HUD"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.UI, "UI"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Animation, "Animator"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Animation, "Animation"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Audio, "Audio"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Audio, "Sound"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Authoring, "Authoring")
        };

        public static string ColorizeModeTabLabel(string label, PyralisAuthoringSemanticTag tag)
        {
            string color = ColorUtility.ToHtmlStringRGB(PyralisAuthoringLabelUtility.GetSemanticTagColor(tag));
            return $"<color=#{color}>{label}</color>";
        }

        public static void DrawSemanticHelpBox(string message, MessageType type)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (type != MessageType.None)
                    EditorGUILayout.LabelField(type.ToString(), EditorStyles.miniBoldLabel);

                DrawSemanticMiniLabel(message);
            }
        }

        public static void DrawSemanticMiniLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            EditorGUILayout.LabelField(ColorizeSemanticTokens(value), GetSemanticMiniLabelStyle());
        }

        public static string ColorizeSemanticTokens(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(value.Length + 32);
            int index = 0;
            while (index < value.Length)
            {
                if (TryGetSemanticTokenAt(value, index, out SemanticTokenRule rule, out int length))
                {
                    string color = ColorUtility.ToHtmlStringRGB(PyralisAuthoringLabelUtility.GetSemanticTagColor(rule.Tag));
                    builder.Append("<color=#");
                    builder.Append(color);
                    builder.Append(">");
                    AppendEscapedRichText(builder, value, index, length);
                    builder.Append("</color>");
                    index += length;
                    continue;
                }

                AppendEscapedRichText(builder, value[index]);
                index++;
            }

            return builder.ToString();
        }

        public static string GetStatusLabel(PyralisSetupFlowStepStatus status)
        {
            switch (status)
            {
                case PyralisSetupFlowStepStatus.Missing:
                    return "Needs setup";
                case PyralisSetupFlowStepStatus.Blocked:
                    return "Blocked";
                case PyralisSetupFlowStepStatus.Recommended:
                    return "Recommended";
                case PyralisSetupFlowStepStatus.Optional:
                    return "Optional";
                default:
                    return "Ready";
            }
        }

        public static string GetWorkIntentLabel(PyralisSetupFlowWorkIntent workIntent)
        {
            switch (workIntent)
            {
                case PyralisSetupFlowWorkIntent.Foundation:
                    return "Foundation setup";
                case PyralisSetupFlowWorkIntent.ProofEnhancer:
                    return "Proof enhancer";
                case PyralisSetupFlowWorkIntent.FeatureCard:
                    return "Feature card";
                default:
                    return "Required setup";
            }
        }

        private static GUIStyle GetSemanticMiniLabelStyle()
        {
            return new GUIStyle(EditorStyles.wordWrappedMiniLabel)
            {
                richText = true
            };
        }

        private static bool TryGetSemanticTokenAt(string value, int index, out SemanticTokenRule match, out int length)
        {
            match = default(SemanticTokenRule);
            length = 0;

            for (int i = 0; i < SemanticTokenRules.Length; i++)
            {
                SemanticTokenRule rule = SemanticTokenRules[i];
                if (string.IsNullOrEmpty(rule.Token) || index + rule.Token.Length > value.Length)
                    continue;

                if (!IsSemanticTokenBoundary(value, index - 1) || !IsSemanticTokenBoundary(value, index + rule.Token.Length))
                    continue;

                if (string.Compare(value, index, rule.Token, 0, rule.Token.Length, System.StringComparison.OrdinalIgnoreCase) != 0)
                    continue;

                match = rule;
                length = rule.Token.Length;
                return true;
            }

            return false;
        }

        private static bool IsSemanticTokenBoundary(string value, int index)
        {
            if (index < 0 || index >= value.Length)
                return true;

            char character = value[index];
            return !char.IsLetterOrDigit(character) && character != '_';
        }

        private static void AppendEscapedRichText(System.Text.StringBuilder builder, string value, int start, int length)
        {
            for (int i = 0; i < length; i++)
                AppendEscapedRichText(builder, value[start + i]);
        }

        private static void AppendEscapedRichText(System.Text.StringBuilder builder, char character)
        {
            builder.Append(character);
        }

        private readonly struct SemanticTokenRule
        {
            public SemanticTokenRule(PyralisAuthoringSemanticTag tag, string token)
            {
                Tag = tag;
                Token = token ?? string.Empty;
            }

            public PyralisAuthoringSemanticTag Tag { get; }
            public string Token { get; }
        }
    }
}
