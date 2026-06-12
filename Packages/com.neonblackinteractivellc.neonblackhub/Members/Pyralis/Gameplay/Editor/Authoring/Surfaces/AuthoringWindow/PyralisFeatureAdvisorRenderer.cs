using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisFeatureAdvisorRenderer
    {
        private static readonly Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();

        public static void Draw(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Game Type And Feature Guide", EditorStyles.boldLabel);

            PyralisAuthoringFeatureAdvisor advisor = PyralisAuthoringFeatureAdvisor.Build(setupProfile);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Route Intent", advisor.RouteIntent, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("First Playable Proof", advisor.FirstProofLabel, EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(advisor.FirstProofGuidance);
                PyralisAuthoringWindowPrimitives.DrawMiniField("First Unity Focus", advisor.FirstUnityFocus);
            }

            if (advisor.DesignPrompts.Count > 0)
            {
                EditorGUILayout.LabelField("Design Before Setup", EditorStyles.miniBoldLabel);
                for (int i = 0; i < advisor.DesignPrompts.Count; i++)
                    DrawDesignPrompt(advisor.DesignPrompts[i], i);
            }

            if (advisor.EnvironmentGuidance.Count > 0)
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("World And Environment Contract", EditorStyles.miniBoldLabel);
                for (int i = 0; i < advisor.EnvironmentGuidance.Count; i++)
                    DrawFeatureAdvisorRow(advisor.EnvironmentGuidance[i], "Environment." + i);
            }

            if (advisor.SelectedFeatures.Count == 0)
            {
                EditorGUILayout.HelpBox("No capability route contracts are selected yet. Use Intent to shape the game first, then author the setup assets that describe the route before wiring camera, input, HUD, pawns, menus, combat, or board objects.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Selected Capabilities", EditorStyles.miniBoldLabel);
            for (int i = 0; i < advisor.SelectedFeatures.Count; i++)
                DrawFeatureAdvisorRow(advisor.SelectedFeatures[i], "Selected." + i);

            if (advisor.RecommendedFeatures.Count == 0)
                return;

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Recommended Next Options", EditorStyles.miniBoldLabel);
            DrawFeatureAdvisorRowsBySource(advisor.RecommendedFeatures);
        }

        private static void DrawFeatureAdvisorRowsBySource(IReadOnlyList<PyralisAuthoringFeatureRow> rows)
        {
            string currentSource = null;
            for (int i = 0; i < rows.Count; i++)
            {
                PyralisAuthoringFeatureRow row = rows[i];
                if (row == null)
                    continue;

                if (currentSource != row.Source)
                {
                    currentSource = row.Source;
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField(currentSource, EditorStyles.miniBoldLabel);
                }

                DrawFeatureAdvisorRow(row, "Recommended." + i);
            }
        }

        private static void DrawDesignPrompt(PyralisAuthoringDesignPrompt prompt, int index)
        {
            if (prompt == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(prompt.Question, EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(prompt.Options);

                string key = "Pyralis.AuthoringWindow.DesignPrompt." + index + "." + prompt.Question;
                bool isOpen = Foldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Why And Setup Impact", true);
                Foldouts[key] = isOpen;

                if (!isOpen)
                    return;

                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Why it matters", EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(prompt.WhyItMatters);
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField("Setup impact", EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(prompt.SetupImpact);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawFeatureAdvisorRow(PyralisAuthoringFeatureRow row, string keySuffix)
        {
            if (row == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(row.Feature, row.Source, EditorStyles.boldLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(row.GameplayEffect);

                string key = "Pyralis.AuthoringWindow.FeatureAdvisor." + keySuffix + "." + row.Feature;
                bool isOpen = Foldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Setup And Customization", true);
                Foldouts[key] = isOpen;

                if (!isOpen)
                    return;

                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Unity setup", EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(row.UnitySetup);
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField("Customize here", EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(row.Customization);
                EditorGUI.indentLevel--;
            }
        }

    }
}