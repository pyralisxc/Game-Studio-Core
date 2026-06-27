using System.IO;
using System.Linq;
using System.Text;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEditor;
using UnityEngine.UIElements;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringGraphJsonExportControl
    {
        private const string TempGraphFolder = "Temp/PyralisAuthoringExports";
        private static readonly string TempGraphFolderFullPath = Path.GetFullPath(TempGraphFolder);

        public static Button BuildMapSnapshotButton(PyralisAuthoringSetupGraph graph)
        {
            return BuildSnapshotButton("Map", "Export Map JSON", graph);
        }

        public static Button BuildHygieneSnapshotButton(PyralisAuthoringHygieneProjection projection)
        {
            var button = PyralisAuthoringUi.Button(
                "Export Hygiene JSON",
                () => ExportHygieneSnapshot(projection),
                $"Write the Hygiene projection snapshot to {TempGraphFolder}. Saves the same audit projection rendered by the tab: graph health, dependency pressure, cleanup focus, watch-list pressure, and contract-source pressure.");
            button.SetEnabled(projection != null);
            return button;
        }

        public static Button BuildFactsSnapshotButton(PyralisAuthoringSetupGraph graph)
        {
            return BuildSnapshotButton("Facts", "Export Facts JSON", graph);
        }

        public static Button BuildGuideSnapshotButton(PyralisAuthoringGuideTraceProjection projection)
        {
            var button = PyralisAuthoringUi.Button("Export Guide JSON", () => ExportGuideSnapshot(projection), BuildGuideTooltip());
            button.SetEnabled(projection?.Graph != null);
            return button;
        }

        public static Button BuildIntentSnapshotButton(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            PyralisAuthoringIntentProjection projection,
            PyralisAuthoringSetupGraph graph = null)
        {
            return PyralisAuthoringUi.Button(
                "Export Intent JSON",
                () => ExportIntentSnapshot(selection, model, projection, graph),
                "Write the Intent projection snapshot: DNA axioms, presentation lane, participant route, capability descriptors, metadata backlog, selected ingredients, route-shape summary, and advisor rows. It does not export scene/setup reality or proof/setup status.");
        }

        public static void ExportIntentSnapshot(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            PyralisAuthoringIntentProjection projection,
            PyralisAuthoringSetupGraph graph = null)
        {
            Directory.CreateDirectory(TempGraphFolderFullPath);
            string safeRouteName = MakeFileSafe("Intent");
            string path = Path.Combine(TempGraphFolderFullPath, $"Pyralis_{safeRouteName}_IntentSnapshot.json");
            string json = PyralisAuthoringSetupGraphJsonExporter.ToIntentJson(selection, model, projection, graph);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            RevealExportFolder();
        }

        public static void ExportHygieneSnapshot(PyralisAuthoringHygieneProjection projection)
        {
            if (projection == null)
                return;

            Directory.CreateDirectory(TempGraphFolderFullPath);
            string safeRouteName = MakeFileSafe(projection.Graph != null ? projection.Graph.RouteName : "No setup route selected");
            string path = Path.Combine(TempGraphFolderFullPath, $"Pyralis_{safeRouteName}_Hygiene_GraphSnapshot.json");
            string json = PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(projection);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            RevealExportFolder();
        }

        private static Button BuildSnapshotButton(string viewName, string label, PyralisAuthoringSetupGraph graph)
        {
            var button = PyralisAuthoringUi.Button(label, () => Export(viewName, graph), BuildTooltip(viewName));
            button.SetEnabled(graph != null || IsFacts(viewName));
            return button;
        }

        private static void Export(string viewName, PyralisAuthoringSetupGraph graph)
        {
            if (graph == null && !IsFacts(viewName))
                return;

            Directory.CreateDirectory(TempGraphFolderFullPath);
            WriteSnapshot(graph, viewName);
            RevealExportFolder();
        }

        private static void WriteSnapshot(PyralisAuthoringSetupGraph graph, string viewName)
        {
            string safeRouteName = MakeFileSafe(graph != null ? graph.RouteName : "No setup route selected");
            string safeViewName = MakeFileSafe(viewName);
            string fileName = $"Pyralis_{safeRouteName}_{safeViewName}_GraphSnapshot.json";
            string path = Path.Combine(TempGraphFolderFullPath, fileName);
            string json = BuildJson(viewName, graph);
            File.WriteAllText(path, json, new UTF8Encoding(false));
        }

        private static string BuildJson(string viewName, PyralisAuthoringSetupGraph graph)
        {
            if (IsFacts(viewName))
                return PyralisAuthoringSetupGraphJsonExporter.ToFactsJson(graph);

            return PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);
        }

        private static void ExportGuideSnapshot(PyralisAuthoringGuideTraceProjection projection)
        {
            if (projection?.Graph == null)
                return;

            Directory.CreateDirectory(TempGraphFolderFullPath);
            string safeRouteName = MakeFileSafe(projection.Graph.RouteName);
            string path = Path.Combine(TempGraphFolderFullPath, $"Pyralis_{safeRouteName}_Guide.json");
            string json = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(projection);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            RevealExportFolder();
        }

        private static void RevealExportFolder()
        {
            EditorApplication.delayCall += () =>
            {
                EditorUtility.RevealInFinder(TempGraphFolderFullPath);
            };
        }

        private static string BuildTooltip(string viewName)
        {
            if (IsFacts(viewName))
            {
                return $"Write the Facts dictionary snapshot to {TempGraphFolder}. Facts export vocabulary, reflected contracts, proof templates, and source/provenance counts only.";
            }

            return $"Write the Map setup snapshot to {TempGraphFolder}. Map exports current setup reality only, not the Intent-projected desired route or Hygiene audit.";
        }

        private static string BuildGuideTooltip()
        {
            return "Write the Guide projection snapshot. This exports the same ordered setup path and Guide Trace rendered by the tab: current action, selected proof, blockers, proof support, source owners, and route evidence.";
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

        private static bool IsFacts(string viewName)
        {
            return string.Equals(viewName, "Facts", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
