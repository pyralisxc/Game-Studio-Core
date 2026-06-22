using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringGraphJsonExportControl
    {
        private const string TempGraphFolder = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/TempGraphs";

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

        public static Button BuildRouteProofTraceButton(PyralisAuthoringSetupGraph graph)
        {
            var button = PyralisAuthoringUi.Button("Export Route Trace", () => ExportRouteProofTrace(graph), BuildTraceTooltip());
            button.SetEnabled(graph != null);
            return button;
        }

        public static Button BuildIntentSnapshotButton(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            return PyralisAuthoringUi.Button(
                "Export Intent JSON",
                () => ExportIntentSnapshot(selection, model, descriptors),
                "Write the Intent tab steering snapshot: DNA axioms, presentation lane, participant route, capability descriptors, metadata backlog, selected ingredients, and advisor rows. It does not export scene/setup reality.");
        }

        public static void ExportIntentSnapshot(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            Directory.CreateDirectory(TempGraphFolder);
            string safeRouteName = MakeFileSafe("Intent");
            string path = Path.Combine(TempGraphFolder, $"Pyralis_{safeRouteName}_IntentSnapshot.json");
            string json = PyralisAuthoringSetupGraphJsonExporter.ToIntentJson(selection, model, descriptors);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            RefreshAndReveal();
        }

        public static void ExportHygieneSnapshot(PyralisAuthoringHygieneProjection projection)
        {
            if (projection == null)
                return;

            Directory.CreateDirectory(TempGraphFolder);
            string safeRouteName = MakeFileSafe(projection.Graph != null ? projection.Graph.RouteName : "No setup route selected");
            string path = Path.Combine(TempGraphFolder, $"Pyralis_{safeRouteName}_Hygiene_GraphSnapshot.json");
            string json = PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(projection);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            RefreshAndReveal();
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

            Directory.CreateDirectory(TempGraphFolder);
            WriteSnapshot(graph, viewName);
            RefreshAndReveal();
        }

        private static void WriteSnapshot(PyralisAuthoringSetupGraph graph, string viewName)
        {
            string safeRouteName = MakeFileSafe(graph != null ? graph.RouteName : "No setup route selected");
            string safeViewName = MakeFileSafe(viewName);
            string fileName = $"Pyralis_{safeRouteName}_{safeViewName}_GraphSnapshot.json";
            string path = Path.Combine(TempGraphFolder, fileName);
            string json = BuildJson(viewName, graph);
            File.WriteAllText(path, json, new UTF8Encoding(false));
        }

        private static string BuildJson(string viewName, PyralisAuthoringSetupGraph graph)
        {
            if (IsFacts(viewName))
                return PyralisAuthoringSetupGraphJsonExporter.ToFactsJson(graph);

            return PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);
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
            RefreshAndReveal();
        }

        private static void RefreshAndReveal()
        {
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                EditorUtility.RevealInFinder(TempGraphFolder);
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

        private static string BuildTraceTooltip()
        {
            return "Write a Route Proof Trace JSON. This exports the ordered fresh-scene setup-card path toward the selected first proof, plus blockers, proof context, source owners, and route evidence for humans and agents.";
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
