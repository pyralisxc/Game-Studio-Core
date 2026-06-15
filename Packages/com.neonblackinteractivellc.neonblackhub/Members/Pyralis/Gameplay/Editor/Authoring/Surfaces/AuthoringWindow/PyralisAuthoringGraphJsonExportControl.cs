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
            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(210f)))
            {
                using (new EditorGUI.DisabledScope(graph == null))
                {
                    GUIContent content = new GUIContent(
                        "Export JSON",
                        $"Write this {viewName} graph snapshot to {TempGraphFolder}. Generated JSON is ignored and exists only for diagnostics, issue reports, and agent handoff.");
                    if (GUILayout.Button(content, GUILayout.Width(105f)))
                        Export(viewName, graph);
                }
            }
        }

        private static void Export(string viewName, PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return;

            Directory.CreateDirectory(TempGraphFolder);
            WriteSnapshot(graph, viewName);
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(TempGraphFolder);
        }

        private static void WriteSnapshot(PyralisAuthoringSetupGraph graph, string viewName)
        {
            string safeRouteName = MakeFileSafe(graph.RouteName);
            string safeViewName = MakeFileSafe(viewName);
            string fileName = $"Pyralis_{safeRouteName}_{safeViewName}_GraphSnapshot.json";
            string path = Path.Combine(TempGraphFolder, fileName);
            string json = PyralisAuthoringSetupGraphJsonExporter.ToJson(graph, viewName);
            File.WriteAllText(path, json, new UTF8Encoding(false));
        }

        private static string MakeFileSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Authoring";

            string result = value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
                result = result.Replace(invalid, '_');

            return string.IsNullOrWhiteSpace(result) ? "Authoring" : result;
        }
    }
}
