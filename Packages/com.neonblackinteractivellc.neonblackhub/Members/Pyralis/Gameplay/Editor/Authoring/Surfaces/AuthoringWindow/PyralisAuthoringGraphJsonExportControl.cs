using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringGraphJsonExportControl
    {
        private const string TempGraphFolder = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/TempGraphs";

        public static void Draw(string viewName, PyralisAuthoringSetupGraph graph)
        {
            bool canExport = graph != null || IsHygiene(viewName);
            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(210f)))
            {
                using (new EditorGUI.DisabledScope(!canExport))
                {
                    GUIContent content = new GUIContent(
                        "Export JSON",
                        BuildTooltip(viewName));
                    if (GUILayout.Button(content, GUILayout.Width(105f)))
                        Export(viewName, graph);
                }
            }
        }

        private static void Export(string viewName, PyralisAuthoringSetupGraph graph)
        {
            if (graph == null && !IsHygiene(viewName))
                return;

            Directory.CreateDirectory(TempGraphFolder);
            WriteSnapshot(graph, viewName);
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(TempGraphFolder);
        }

        private static void WriteSnapshot(PyralisAuthoringSetupGraph graph, string viewName)
        {
            string safeRouteName = MakeFileSafe(graph != null ? graph.RouteName : "No setup route selected");
            string safeViewName = MakeFileSafe(viewName);
            string fileName = $"Pyralis_{safeRouteName}_{safeViewName}_GraphSnapshot.json";
            string path = Path.Combine(TempGraphFolder, fileName);
            string json = IsHygiene(viewName)
                ? PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(graph, PyralisSourceDependencyHygieneScanner.ScanPackage())
                : PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);
            File.WriteAllText(path, json, new UTF8Encoding(false));
        }

        private static string BuildTooltip(string viewName)
        {
            if (IsHygiene(viewName))
            {
                return $"Write this {viewName} snapshot to {TempGraphFolder}. Hygiene can export dependency pressure even when no setup route is active; graph-specific sections stay empty until a route exists.";
            }

            return $"Write this {viewName} graph snapshot to {TempGraphFolder}. Map exports current setup reality only, not the Intent-projected desired route.";
        }

        private static string MakeFileSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "NoSetupRouteSelected";

            string result = value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
                result = result.Replace(invalid, '_');

            return string.IsNullOrWhiteSpace(result) ? "NoSetupRouteSelected" : result.Replace(' ', '_');
        }

        private static bool IsHygiene(string viewName)
        {
            return string.Equals(viewName, "Hygiene", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
