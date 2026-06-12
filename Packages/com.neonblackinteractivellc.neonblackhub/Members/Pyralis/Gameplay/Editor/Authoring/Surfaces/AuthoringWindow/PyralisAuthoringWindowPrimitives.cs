using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringWindowPrimitives
    {
        public static void DrawMiniField(string label, string value)
        {
            DrawMiniField(label, value, string.Empty);
        }

        public static void DrawMiniField(string label, string value, string tooltip)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            EditorGUILayout.LabelField(new GUIContent(label, tooltip ?? string.Empty), EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            PyralisAuthoringWindowText.DrawSemanticMiniLabel(value);
            EditorGUI.indentLevel--;
        }

        public static void DrawMiniList(string label, IReadOnlyList<string> values)
        {
            DrawMiniList(label, values, string.Empty);
        }

        public static void DrawMiniList(string label, IReadOnlyList<string> values, string tooltip, int maxVisibleItems = int.MaxValue)
        {
            if (values == null || values.Count == 0)
            {
                DrawMiniField(label, "None for this first proof.", tooltip);
                return;
            }

            EditorGUILayout.LabelField(new GUIContent(label, tooltip ?? string.Empty), EditorStyles.miniBoldLabel);
            int visibleCount = Mathf.Min(values.Count, maxVisibleItems);
            for (int i = 0; i < visibleCount; i++)
                PyralisAuthoringWindowText.DrawSemanticMiniLabel("- " + values[i]);

            if (visibleCount < values.Count)
                PyralisAuthoringWindowText.DrawSemanticMiniLabel("+ " + (values.Count - visibleCount) + " more when expanded");
        }

        public static void SelectAndPing(Object target)
        {
            if (target == null)
                return;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        public static string GetReadinessBadge(bool isReady, Object target, bool isOptional = false)
        {
            if (isReady)
                return "[Ready]";

            if (isOptional)
                return "[Optional]";

            if (target != null)
                return "[Blocked]";

            return "[Needs Setup]";
        }

        public static void DrawSemanticTagStrip(IReadOnlyList<PyralisAuthoringSemanticTag> tags)
        {
            if (tags == null || tags.Count == 0)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < tags.Count; i++)
                    DrawSemanticTagBadge(tags[i]);
            }
        }

        public static void DrawSemanticTagBadge(PyralisAuthoringSemanticTag tag)
        {
            Color color = PyralisAuthoringLabelUtility.GetSemanticTagColor(tag);
            string label = PyralisAuthoringLabelUtility.GetSemanticTagLabel(tag);
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                padding = new RectOffset(5, 5, 1, 2)
            };

            Vector2 size = style.CalcSize(new GUIContent(label));
            Rect rect = GUILayoutUtility.GetRect(size.x + 12f, 18f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(rect, color);
            Rect inner = new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f);
            EditorGUI.DrawRect(inner, new Color(color.r * 0.74f, color.g * 0.74f, color.b * 0.74f, 1f));
            GUI.Label(rect, label, style);
        }
    }
}
