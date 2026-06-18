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
            Draw(viewName, graph, null);
        }

        public static void DrawRouteProofTrace(PyralisAuthoringSetupGraph graph)
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(115f)))
            using (new EditorGUI.DisabledScope(graph == null))
            {
                GUIContent traceContent = BuildTraceContent();
                if (GUILayout.Button(traceContent, GUILayout.Width(105f)))
                    ExportRouteProofTrace(graph);
            }
        }

        public static void Draw(string viewName, PyralisAuthoringSetupGraph graph, PyralisAuthoringSetupGraph routeProofTraceGraph)
        {
            bool canExport = graph != null || IsHygiene(viewName);
            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(330f)))
            {
                using (new EditorGUI.DisabledScope(!canExport))
                {
                    GUIContent content = new GUIContent(
                        "Export JSON",
                        BuildTooltip(viewName));
                    if (GUILayout.Button(content, GUILayout.Width(105f)))
                        Export(viewName, graph);
                }

                using (new EditorGUI.DisabledScope(routeProofTraceGraph == null))
                {
                    GUIContent traceContent = BuildTraceContent();
                    if (GUILayout.Button(traceContent, GUILayout.Width(105f)))
                        ExportRouteProofTrace(routeProofTraceGraph);
                }
            }
        }

        private static void Export(string viewName, PyralisAuthoringSetupGraph graph)
        {
            if (graph == null && !IsHygiene(viewName))
                return;

            Directory.CreateDirectory(TempGraphFolder);
            WriteSnapshot(graph, viewName);
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                EditorUtility.RevealInFinder(TempGraphFolder);
            };
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

        private static void ExportRouteProofTrace(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return;

            Directory.CreateDirectory(TempGraphFolder);
            string safeRouteName = MakeFileSafe(graph.RouteName);
            string path = Path.Combine(TempGraphFolder, $"Pyralis_{safeRouteName}_RouteProofTrace.json");
            string json = PyralisAuthoringSetupGraphJsonExporter.ToRouteProofTraceJson(graph);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                EditorUtility.RevealInFinder(TempGraphFolder);
            };
        }

        private static string BuildTooltip(string viewName)
        {
            if (IsHygiene(viewName))
            {
                return $"Write this {viewName} snapshot to {TempGraphFolder}. Hygiene can export dependency pressure even when no setup route is active; graph-specific sections stay empty until a route exists.";
            }

            return $"Write this {viewName} graph snapshot to {TempGraphFolder}. Map exports current setup reality only, not the Intent-projected desired route.";
        }

        private static GUIContent BuildTraceContent()
        {
            return new GUIContent(
                "Export Trace",
                "Write a Route Proof Trace JSON. This exports the ordered fresh-scene setup-card path toward the selected first proof, plus blockers, proof context, source owners, and route evidence for humans and agents.");
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
