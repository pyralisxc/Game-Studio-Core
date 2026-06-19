using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringGraphJsonExportControl
    {
        private const string TempGraphFolder = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/TempGraphs";

        public static void DrawMapSnapshot(PyralisAuthoringSetupGraph graph)
        {
            DrawSnapshot("Map", "Export Map JSON", graph);
        }

        public static void DrawRouteProofTrace(PyralisAuthoringSetupGraph graph)
        {
            DrawRouteProofTraceButton(graph, "Export Route Trace");
        }

        public static void DrawHygieneSnapshot(PyralisAuthoringSetupGraph graph)
        {
            DrawSnapshot("Hygiene", "Export Hygiene JSON", graph);
        }

        private static void DrawSnapshot(string viewName, string label, PyralisAuthoringSetupGraph graph)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSnapshotButton(viewName, label, graph);
                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawSnapshotButton(string viewName, string label, PyralisAuthoringSetupGraph graph)
        {
            bool canExport = graph != null || IsHygiene(viewName);
            using (new EditorGUI.DisabledScope(!canExport))
            {
                GUIContent content = new GUIContent(label, BuildTooltip(viewName));
                if (GUILayout.Button(content, GUILayout.Width(142f)))
                    Export(viewName, graph);
            }
        }

        private static void DrawRouteProofTraceButton(PyralisAuthoringSetupGraph graph, string label)
        {
            using (new EditorGUI.DisabledScope(graph == null))
            {
                GUIContent traceContent = BuildTraceContent(label);
                if (GUILayout.Button(traceContent, GUILayout.Width(142f)))
                    ExportRouteProofTrace(graph);
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
                return $"Write the Hygiene graph audit to {TempGraphFolder}. Includes graph health, dependency pressure, cleanup focus, watch-list pressure, and contract-source pressure.";
            }

            return $"Write the Map setup snapshot to {TempGraphFolder}. Map exports current setup reality only, not the Intent-projected desired route or Hygiene audit.";
        }

        private static GUIContent BuildTraceContent(string label)
        {
            return new GUIContent(
                label,
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
