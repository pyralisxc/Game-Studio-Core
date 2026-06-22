using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringUi
    {
        public static ScrollView Page(string title, string help = null)
        {
            var page = new ScrollView(ScrollViewMode.Vertical);
            page.AddToClassList("authoring-page");
            Header(page, title, help);
            return page;
        }

        public static void Header(VisualElement parent, string title, string help = null)
        {
            var label = new Label(title ?? string.Empty);
            label.AddToClassList("authoring-title");
            parent.Add(label);
            if (!string.IsNullOrWhiteSpace(help))
                Help(parent, help);
        }

        public static VisualElement Section(VisualElement parent, string title, string help = null)
        {
            var section = new VisualElement();
            section.AddToClassList("authoring-section");
            if (!string.IsNullOrWhiteSpace(title))
            {
                var titleLabel = new Label(title);
                titleLabel.AddToClassList("authoring-section-title");
                section.Add(titleLabel);
            }
            if (!string.IsNullOrWhiteSpace(help))
                Help(section, help);
            parent.Add(section);
            return section;
        }

        public static VisualElement Card(VisualElement parent, string title = null, string status = null)
        {
            var card = new VisualElement();
            card.AddToClassList("authoring-card");
            if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(status))
            {
                var header = new VisualElement();
                header.AddToClassList("authoring-card-header");
                var titleLabel = new Label(title ?? string.Empty);
                titleLabel.AddToClassList("authoring-card-title");
                header.Add(titleLabel);
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var statusLabel = new Label(status);
                    statusLabel.AddToClassList("authoring-card-status");
                    header.Add(statusLabel);
                }
                card.Add(header);
            }
            parent.Add(card);
            return card;
        }

        public static Foldout Foldout(VisualElement parent, string title, bool expanded = false)
        {
            var foldout = new Foldout { text = title ?? string.Empty, value = expanded };
            foldout.AddToClassList("authoring-foldout");
            parent.Add(foldout);
            return foldout;
        }

        public static void Help(VisualElement parent, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            var help = new Label(text);
            help.AddToClassList("authoring-help");
            parent.Add(help);
        }

        public static void Mini(VisualElement parent, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            var label = new Label(text);
            label.AddToClassList("authoring-mini");
            parent.Add(label);
        }

        public static void Field(VisualElement parent, string label, string value, string tooltip = null)
        {
            if (string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(value))
                return;
            var row = new VisualElement();
            row.AddToClassList("authoring-field-row");
            row.tooltip = tooltip ?? string.Empty;
            var key = new Label(label ?? string.Empty);
            key.AddToClassList("authoring-field-label");
            row.Add(key);
            var val = new Label(string.IsNullOrWhiteSpace(value) ? "None" : value);
            val.AddToClassList("authoring-field-value");
            row.Add(val);
            parent.Add(row);
        }

        public static void List(VisualElement parent, string label, IReadOnlyList<string> values, string tooltip = null, int visibleLimit = 6)
        {
            if (values == null || values.Count == 0)
                return;
            var group = new VisualElement();
            group.AddToClassList("authoring-list");
            group.tooltip = tooltip ?? string.Empty;
            var title = new Label(label ?? string.Empty);
            title.AddToClassList("authoring-list-title");
            group.Add(title);
            int count = System.Math.Min(values.Count, visibleLimit);
            for (int i = 0; i < count; i++)
                Mini(group, "- " + values[i]);
            if (values.Count > count)
                Mini(group, "+" + (values.Count - count) + " more");
            parent.Add(group);
        }

        public static Button Button(string text, System.Action action, string tooltip = null)
        {
            var button = new Button(() => action?.Invoke()) { text = text ?? string.Empty, tooltip = tooltip ?? string.Empty };
            button.AddToClassList("authoring-button");
            return button;
        }

        public static VisualElement ActionRow(params Button[] buttons)
        {
            var row = new VisualElement();
            row.AddToClassList("authoring-action-row");
            if (buttons != null)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] != null)
                        row.Add(buttons[i]);
                }
            }
            return row;
        }

        public static void InspectButton(VisualElement parent, Object target, string label = "Inspect")
        {
            Button button = Button(label, () => SelectAndPing(target), "Select and ping the referenced Unity object.");
            button.SetEnabled(target != null);
            parent.Add(button);
        }

        public static void SelectAndPing(Object target)
        {
            if (target == null)
                return;
            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        public static string Evidence(PyralisAuthoringGraphEvidenceState state)
        {
            return state switch
            {
                PyralisAuthoringGraphEvidenceState.Ready => "Ready",
                PyralisAuthoringGraphEvidenceState.Optional => "Optional",
                PyralisAuthoringGraphEvidenceState.Missing => "Missing",
                PyralisAuthoringGraphEvidenceState.CandidateDetected => "Suggested",
                PyralisAuthoringGraphEvidenceState.Blocked => "Blocked",
                _ => "Unknown"
            };
        }

        public static string ObjectLabel(Object value, string empty = "None")
        {
            return value != null ? $"{value.name} ({value.GetType().Name})" : empty;
        }
    }

}