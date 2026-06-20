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

        public static Button BuildHygieneSnapshotButton(PyralisAuthoringSetupGraph graph)
        {
            return BuildSnapshotButton("Hygiene", "Export Hygiene JSON", graph);
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
                "Write the Intent tab steering snapshot: DNA axioms, presentation lane, participant route, capability descriptors, selected ingredients, and advisor rows. It does not export scene/setup reality.");
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

        private static Button BuildSnapshotButton(string viewName, string label, PyralisAuthoringSetupGraph graph)
        {
            var button = PyralisAuthoringUi.Button(label, () => Export(viewName, graph), BuildTooltip(viewName));
            button.SetEnabled(graph != null || IsHygiene(viewName) || IsFacts(viewName));
            return button;
        }

        private static void Export(string viewName, PyralisAuthoringSetupGraph graph)
        {
            if (graph == null && !IsHygiene(viewName) && !IsFacts(viewName))
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
            if (IsHygiene(viewName))
                return PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(graph, PyralisSourceDependencyHygieneScanner.ScanPackage());
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
            if (IsHygiene(viewName))
            {
                return $"Write the Hygiene graph audit to {TempGraphFolder}. Includes graph health, dependency pressure, cleanup focus, watch-list pressure, and contract-source pressure.";
            }

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

        private static bool IsHygiene(string viewName)
        {
            return string.Equals(viewName, "Hygiene", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFacts(string viewName)
        {
            return string.Equals(viewName, "Facts", System.StringComparison.OrdinalIgnoreCase);
        }
    }

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

    internal static class PyralisAuthoringTabRenderer
    {
        public static VisualElement BuildOverview(PyralisAuthoringOverviewProjection projection, System.Action openIntent, System.Action openGuide, System.Action openMap)
        {
            var page = PyralisAuthoringUi.Page("Overview");
            if (projection?.Model == null)
                return page;
            VisualElement next = PyralisAuthoringUi.Section(page, "Next Unity Action");
            string guidance = projection.CurrentStep != null && !string.IsNullOrWhiteSpace(projection.CurrentStep.Message) ? projection.CurrentStep.Message : projection.Model.FirstProofGuidance;
            PyralisAuthoringUi.Help(next, guidance);
            PyralisAuthoringUi.Field(next, "Intent Focus", PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(projection.Graph));
            PyralisAuthoringUi.Field(next, "Route", projection.CurrentStep != null ? projection.CurrentStep.RouteName : string.Empty);
            PyralisAuthoringUi.Field(next, "Next", projection.CurrentStep != null ? projection.CurrentStep.Label : projection.Model.BestNextAction);
            if (projection.CurrentStep?.NativeAction.HasValue == true)
                DrawNativeAction(next, projection.CurrentStep.NativeAction.Value);
            next.Add(PyralisAuthoringUi.ActionRow(
                PyralisAuthoringUi.Button("Open Intent", openIntent),
                PyralisAuthoringUi.Button("Open Guide", openGuide),
                PyralisAuthoringUi.Button("Open Map", openMap),
                BuildInspectBestTargetButton(projection.Model)));
            DrawOverviewLane(page, "Do Now", "Only route-required missing or blocked work appears here.", projection.Model.DoNow, 3);
            DrawOverviewLane(page, "Proof Enhancers", "Useful before Play Mode when they make the first proof clearer.", projection.Model.DoSoon, 3);
            DrawFirstProof(page, projection);
            return page;
        }

        public static VisualElement BuildGuide(PyralisAuthoringGuideProjection projection)
        {
            var page = PyralisAuthoringUi.Page("Guide", "Guide expands Overview. Intent chooses the route focus; the graph orders setup steps.");
            if (projection == null)
                return page;
            if (projection.SelectionFirst)
                DrawSelectedContext(page, projection);
            DrawRouteChecklist(page, projection);
            DrawReflectiveContracts(page, projection.Contracts);
            if (!projection.SelectionFirst)
                DrawSelectedContext(page, projection);
            return page;
        }

        public static VisualElement BuildMap(PyralisAuthoringMapProjection projection)
        {
            var page = PyralisAuthoringUi.Page("Setup Map", "Use Map for scene and setup reality. Intent does not change this view; Hygiene owns graph integrity and developer audits.");
            page.Add(PyralisAuthoringUi.ActionRow(PyralisAuthoringGraphJsonExportControl.BuildMapSnapshotButton(projection?.Graph)));
            if (projection == null)
                return page;
            VisualElement context = PyralisAuthoringUi.Section(page, "Selected Authoring Context");
            PyralisAuthoringUi.Field(context, "Active Setup", PyralisAuthoringUi.ObjectLabel(projection.ActiveSetup, "No setup context"));
            PyralisAuthoringUi.Field(context, "Current Selection", PyralisAuthoringUi.ObjectLabel(projection.Selection, "Nothing selected"));
            VisualElement chain = PyralisAuthoringUi.Section(page, "Setup Chain");
            foreach (PyralisAuthoringSetupGraphRow row in projection.SetupRows)
                DrawMapSetupRow(chain, row);
            VisualElement surfaces = PyralisAuthoringUi.Section(page, "Scene Surface Scan", "Found surfaces are evidence, not proof. Play Mode still owns the final route proof.");
            if (projection.SceneSurfaces.Count == 0)
                PyralisAuthoringUi.Mini(surfaces, "No scene surfaces were detected yet.");
            foreach (PyralisAuthoringGraphNode node in projection.SceneSurfaces)
                DrawSceneSurface(surfaces, node);
            VisualElement issues = PyralisAuthoringUi.Section(page, "Scene Setup Issues", "Concrete Unity setup items: missing scene surfaces, empty fields, component requirements, or selected-route wiring.");
            if (projection.SceneSetupIssues.Count == 0)
                PyralisAuthoringUi.Mini(issues, "No current scene/setup issues are exposed by the graph.");
            foreach (PyralisAuthoringGraphAuditRow row in projection.SceneSetupIssues)
                DrawGraphAuditRow(issues, row, true);
            DrawConnections(page, projection.Connections);
            return page;
        }

        public static VisualElement BuildHygiene(PyralisAuthoringHygieneProjection projection, System.Action refreshDependencyAudit)
        {
            var page = PyralisAuthoringUi.Page("Hygiene", "Use Hygiene as the programmer audit surface. Map owns concrete scene and Inspector setup issues.");
            page.Add(PyralisAuthoringUi.ActionRow(
                PyralisAuthoringGraphJsonExportControl.BuildHygieneSnapshotButton(projection?.Graph),
                PyralisAuthoringUi.Button("Refresh Dependency Audit", refreshDependencyAudit)));
            if (projection == null)
                return page;
            VisualElement summary = PyralisAuthoringUi.Section(page, "Audit Summary");
            PyralisAuthoringUi.Field(summary, "Active Setup", PyralisAuthoringUi.ObjectLabel(projection.ActiveSetup, "No active setup graph selected"));
            PyralisAuthoringUi.Field(summary, "Graph Context", projection.Graph != null ? projection.Graph.RouteName : "No graph");
            PyralisAuthoringUi.Field(summary, "Graph Size", projection.Graph != null ? $"{projection.Graph.Nodes.Count} nodes, {projection.Graph.Edges.Count} edges" : "No graph");
            PyralisAuthoringUi.Field(summary, "Scanned Scripts", projection.DependencyRecords.Count.ToString());
            PyralisAuthoringUi.Field(summary, "Watch / Heavy / Boundary Risk", $"{projection.WatchCount} / {projection.HeavyCount} / {projection.BoundaryRiskCount}");
            PyralisAuthoringUi.Field(summary, "Actionable / Expected Pressure", $"{projection.ActionablePressureCount} / {projection.ExpectedPressureCount}");
            PyralisAuthoringUi.List(summary, "Pressure Types", projection.BuildPressureKindSummary());
            DrawHygieneSections(page, projection);
            DrawDependencyPressure(page, "Cleanup Focus", projection.CleanupFocus, "Actionable cleanup");
            DrawDependencyPressure(page, "Watch List", projection.WatchList, "Expected pressure");
            return page;
        }

        public static VisualElement BuildFacts(PyralisAuthoringFactsProjection projection)
        {
            var page = PyralisAuthoringUi.Page("Fact Explorer", "Read-only cookbook view. Facts explain Pyralis vocabulary, reflected contracts, proof targets, Inspector handoffs, and validation language.");
            page.Add(PyralisAuthoringUi.ActionRow(PyralisAuthoringGraphJsonExportControl.BuildFactsSnapshotButton(projection?.Graph)));
            if (projection == null)
                return page;
            VisualElement summary = PyralisAuthoringUi.Section(page, "Coverage");
            PyralisAuthoringUi.Field(summary, "Active Setup", PyralisAuthoringUi.ObjectLabel(projection.ActiveSetup, "No active setup selected"));
            PyralisAuthoringUi.Field(summary, "Graph Nodes", (projection.Graph?.Nodes.Count ?? 0).ToString());
            PyralisAuthoringUi.Field(summary, "Graph Edges", (projection.Graph?.Edges.Count ?? 0).ToString());
            PyralisAuthoringUi.Field(summary, "Total Facts", projection.Facts.Count.ToString());
            DrawFactCoverage(summary, projection.Facts);
            DrawFactContracts(page, projection.Contracts);
            DrawConnections(page, projection.ProofCoverage, "Graph Proof Coverage");
            DrawFactGroups(page, projection.Facts);
            return page;
        }

        private static Button BuildInspectBestTargetButton(PyralisAuthoringOverviewModel model)
        {
            Object target = GetFirstTarget(model?.DoNow) ?? GetFirstTarget(model?.DoSoon) ?? GetFirstTarget(model?.Later);
            Button button = PyralisAuthoringUi.Button("Inspect Best Target", () => PyralisAuthoringUi.SelectAndPing(target));
            button.SetEnabled(target != null);
            return button;
        }

        private static Object GetFirstTarget(IReadOnlyList<PyralisAuthoringOverviewIssue> issues)
        {
            if (issues == null)
                return null;
            for (int i = 0; i < issues.Count; i++)
                if (issues[i]?.Target != null)
                    return issues[i].Target;
            return null;
        }

        private static void DrawOverviewLane(VisualElement parent, string title, string description, IReadOnlyList<PyralisAuthoringOverviewIssue> issues, int visibleLimit)
        {
            VisualElement section = PyralisAuthoringUi.Section(parent, title, description);
            int count = issues?.Count ?? 0;
            PyralisAuthoringUi.Field(section, "Items", count.ToString());
            if (count == 0)
            {
                PyralisAuthoringUi.Mini(section, title == "Do Now" ? "No required blockers for the current Intent route." : "No route-specific proof helpers are asking for attention right now.");
                return;
            }
            int visible = System.Math.Min(count, visibleLimit);
            for (int i = 0; i < visible; i++)
                DrawOverviewIssue(section, issues[i]);
            if (count > visible)
                PyralisAuthoringUi.Mini(section, $"{count - visible} more item(s) are in Guide.");
        }

        private static void DrawOverviewIssue(VisualElement parent, PyralisAuthoringOverviewIssue issue)
        {
            if (issue == null)
                return;
            VisualElement card = PyralisAuthoringUi.Card(parent, issue.Label, PyralisAuthoringUi.Evidence(issue.EvidenceState));
            PyralisAuthoringUi.Field(card, "Why It Matters", issue.WorkIntentLabel);
            PyralisAuthoringUi.Mini(card, issue.Message);
            PyralisAuthoringUi.Field(card, "Native Unity Action", issue.NativeActionGuidance);
            PyralisAuthoringUi.Mini(card, issue.Evidence);
            PyralisAuthoringUi.InspectButton(card, issue.Target, "Inspect Target");
        }

        private static void DrawFirstProof(VisualElement parent, PyralisAuthoringOverviewProjection projection)
        {
            VisualElement section = PyralisAuthoringUi.Section(parent, "First Proof After Do Now");
            PyralisAuthoringOverviewModel model = projection.Model;
            PyralisAuthoringGraphNode proofNode = projection.ProofNode;
            PyralisAuthoringUi.Field(section, "Proof", proofNode != null ? proofNode.Label : model.FirstProofLabel);
            PyralisAuthoringUi.Field(section, "When To Test", GetFlowTestStatus(model));
            PyralisAuthoringUi.Field(section, "Play Mode Action", GetFirstValue(proofNode?.NativeSetup, model.FirstProofSetupSurface));
            PyralisAuthoringUi.Field(section, "Success Looks Like", !string.IsNullOrWhiteSpace(proofNode?.BlockingReason) ? proofNode.BlockingReason : model.FirstProofSuccessCriteria);
            PyralisAuthoringUi.Field(section, "Do Later", model.FirstProofDeferUntilAfter);
        }

        private static string GetFlowTestStatus(PyralisAuthoringOverviewModel model)
        {
            if (model == null)
                return "Select an active setup before testing the flow.";
            if (model.DoNow.Count > 0)
                return "Not ready to test yet. Clear Do Now in Edit Mode first, then use Play Mode only as the first proof test.";
            if (model.DoSoon.Count > 0)
                return "Ready for a narrow Play Mode proof. Proof Enhancers can make the first test clearer, but setup edits still belong in Edit Mode.";
            return "Ready for first proof. Run the smallest route pass named below, verify one interaction in Play Mode, stop Play Mode, then add one feature at a time.";
        }

        private static string GetFirstValue(string[] values, string fallback)
        {
            return values != null && values.Length > 0 && !string.IsNullOrWhiteSpace(values[0]) ? values[0] : fallback;
        }

        private static void DrawRouteChecklist(VisualElement parent, PyralisAuthoringGuideProjection projection)
        {
            VisualElement section = PyralisAuthoringUi.Section(parent, "Intent Route Checklist");
            section.Add(PyralisAuthoringUi.ActionRow(PyralisAuthoringGraphJsonExportControl.BuildRouteProofTraceButton(projection.Graph)));
            if (projection.Route?.OrderedSteps == null || projection.Route.OrderedSteps.Count == 0)
            {
                PyralisAuthoringUi.Help(section, "Guide needs an Intent focus and enough authored setup to build a route checklist. Start in Intent, then express setup through Unity assets and scene objects.");
                PyralisAuthoringUi.Field(section, "Next Surface", "Intent");
                return;
            }
            foreach (PyralisAuthoringRouteStepRow row in projection.Route.OrderedSteps)
                DrawRouteStep(section, row);
        }

        private static void DrawRouteStep(VisualElement parent, PyralisAuthoringRouteStepRow row)
        {
            if (row?.Node == null)
                return;
            Foldout foldout = PyralisAuthoringUi.Foldout(parent, $"{row.Sequence}. {row.Label}    {row.RoleLabel} / {PyralisAuthoringUi.Evidence(row.EvidenceState)}", row.IsCurrentAction);
            PyralisAuthoringUi.Field(foldout, "Path", $"{row.PhaseLabel} / {row.RoleLabel}");
            PyralisAuthoringUi.Field(foldout, "Why", row.Reason);
            PyralisAuthoringUi.Field(foldout, "Unity Action", row.UnityActionLabel);
            PyralisAuthoringUi.Field(foldout, "What It Means", row.Message);
            PyralisAuthoringUi.List(foldout, "Assignment Fields", row.AssignmentFields);
            PyralisAuthoringUi.List(foldout, "Customization", row.CustomizationMoments);
            PyralisAuthoringUi.Field(foldout, "Source", row.SourceOrigin.ToString());
        }

        private static void DrawSelectedContext(VisualElement parent, PyralisAuthoringGuideProjection projection)
        {
            if (projection.Selection == null && projection.SelectedContext == null)
                return;
            VisualElement section = PyralisAuthoringUi.Section(parent, projection.SelectionFirst ? "Selected Object Next Step" : "What This Selection Does");
            PyralisAuthoringUi.Field(section, "Selection", PyralisAuthoringUi.ObjectLabel(projection.Selection, "No selection"));
            if (projection.CurrentStep != null)
            {
                PyralisAuthoringUi.Field(section, "Current Step", projection.CurrentStep.Label);
                PyralisAuthoringUi.Help(section, projection.CurrentStep.Message);
                if (projection.CurrentStep.NativeAction.HasValue)
                    DrawNativeAction(section, projection.CurrentStep.NativeAction.Value);
            }
            PyralisAuthoringSelectedContextGraphRow context = projection.SelectedContext;
            if (context == null)
                return;
            PyralisAuthoringUi.Field(section, "Reference", string.IsNullOrWhiteSpace(context.NodeId) ? "No matching setup reference yet" : context.NodeId);
            PyralisAuthoringUi.Field(section, "Evidence", PyralisAuthoringUi.Evidence(context.EvidenceState));
            PyralisAuthoringUi.Field(section, "Role", context.Role);
            PyralisAuthoringUi.Field(section, "Next Check", context.NextCheck);
            PyralisAuthoringUi.Field(section, "Native Setup", context.NativeSetup);
            foreach (PyralisAuthoringSelectedContextDetail detail in context.Details)
            {
                PyralisAuthoringUi.Field(section, detail.Label, detail.Value);
                if (detail.CanSelectTarget)
                    PyralisAuthoringUi.InspectButton(section, detail.Target, "Select " + detail.Label);
            }
        }

        private static void DrawReflectiveContracts(VisualElement parent, IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> contracts)
        {
            if (contracts == null || contracts.Count == 0)
                return;
            Foldout foldout = PyralisAuthoringUi.Foldout(parent, $"Reflective Design Contracts ({contracts.Count})", false);
            int visible = System.Math.Min(contracts.Count, 24);
            for (int i = 0; i < visible; i++)
            {
                PyralisAuthoringReflectiveContractGraphRow row = contracts[i];
                VisualElement card = PyralisAuthoringUi.Card(foldout, row.Label, PyralisAuthoringUi.Evidence(row.EvidenceState));
                PyralisAuthoringUi.Mini(card, row.Message);
                PyralisAuthoringUi.InspectButton(card, row.Target);
            }
            if (contracts.Count > visible)
                PyralisAuthoringUi.Mini(foldout, $"+{contracts.Count - visible} more in Facts/JSON export.");
        }

        private static void DrawMapSetupRow(VisualElement parent, PyralisAuthoringSetupGraphRow row)
        {
            if (row == null)
                return;
            VisualElement card = PyralisAuthoringUi.Card(parent, row.Label, row.IsReady ? "Ready" : row.IsOptional ? "Optional" : PyralisAuthoringUi.Evidence(row.EffectiveEvidenceState));
            PyralisAuthoringUi.Mini(card, row.Message);
            PyralisAuthoringUi.InspectButton(card, row.Target);
        }

        private static void DrawSceneSurface(VisualElement parent, PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return;
            VisualElement card = PyralisAuthoringUi.Card(parent, node.Label, PyralisAuthoringUi.Evidence(node.EvidenceState));
            PyralisAuthoringUi.Field(card, "Evidence", node.Guidance);
            PyralisAuthoringUi.List(card, "Next fix", node.NativeSetup);
        }

        private static void DrawGraphAuditRow(VisualElement parent, PyralisAuthoringGraphAuditRow row, bool includeInspect)
        {
            if (row?.Node == null)
                return;
            VisualElement card = PyralisAuthoringUi.Card(parent, row.Label, PyralisAuthoringUi.Evidence(row.EvidenceState));
            PyralisAuthoringUi.Mini(card, row.Message);
            if (row.Node.NativeAction.HasValue)
                DrawNativeAction(card, row.Node.NativeAction.Value);
            PyralisAuthoringUi.List(card, "Field or component", row.Node.AssignmentFields);
            PyralisAuthoringUi.List(card, "Unity setup", row.Node.NativeSetup);
            PyralisAuthoringUi.Field(card, "Why", row.Node.BlockingReason);
            if (includeInspect)
                PyralisAuthoringUi.InspectButton(card, row.Target, "Inspect Target");
        }

        private static void DrawConnections(VisualElement parent, IReadOnlyList<PyralisAuthoringGraphConnectionRow> connections, string title = "Developer Route Connections")
        {
            Foldout foldout = PyralisAuthoringUi.Foldout(parent, $"{title} ({connections?.Count ?? 0})", false);
            if (connections == null || connections.Count == 0)
            {
                PyralisAuthoringUi.Mini(foldout, "No route connections were resolved yet.");
                return;
            }
            int visible = System.Math.Min(connections.Count, 32);
            for (int i = 0; i < visible; i++)
            {
                PyralisAuthoringGraphConnectionRow row = connections[i];
                VisualElement card = PyralisAuthoringUi.Card(foldout, $"{row.FromLabel} -> {row.ToLabel}", row.Relationship);
                PyralisAuthoringUi.Field(card, "Meaning", row.Detail);
            }
            if (connections.Count > visible)
                PyralisAuthoringUi.Mini(foldout, $"+{connections.Count - visible} more reflected connections are in JSON export.");
        }

        private static void DrawHygieneSections(VisualElement parent, PyralisAuthoringHygieneProjection projection)
        {
            VisualElement section = PyralisAuthoringUi.Section(parent, "Graph Audit Buckets");
            bool drewRows = false;
            foreach (PyralisAuthoringGraphAuditSection bucket in projection.Sections)
            {
                if (bucket == null)
                    continue;
                PyralisAuthoringUi.Field(section, bucket.Label, $"{bucket.Rows.Count} row(s)");
                if (!ShouldSurfaceHygieneRows(bucket))
                    continue;
                drewRows = true;
                Foldout foldout = PyralisAuthoringUi.Foldout(section, $"{bucket.Label} Details ({bucket.Rows.Count})", true);
                int visible = System.Math.Min(bucket.Rows.Count, 8);
                for (int rowIndex = 0; rowIndex < visible; rowIndex++)
                    DrawGraphAuditRow(foldout, bucket.Rows[rowIndex], false);
                if (bucket.Rows.Count > visible)
                    PyralisAuthoringUi.Mini(foldout, $"+{bucket.Rows.Count - visible} more row(s) in JSON export.");
            }
            if (!drewRows)
                PyralisAuthoringUi.Mini(section, "No graph integrity blockers found. Scene setup findings are handled in Map.");
        }

        private static bool ShouldSurfaceHygieneRows(PyralisAuthoringGraphAuditSection section)
        {
            return section != null
                && (string.Equals(section.Label, "Unvalidated Graph Nodes", System.StringComparison.Ordinal)
                    || string.Equals(section.Label, "Proof Blocker Links", System.StringComparison.Ordinal));
        }

        private static void DrawDependencyPressure(VisualElement parent, string title, IReadOnlyList<PyralisSourceDependencyHygieneRecord> records, string posture)
        {
            VisualElement section = PyralisAuthoringUi.Section(parent, title);
            if (records == null || records.Count == 0)
            {
                PyralisAuthoringUi.Mini(section, title == "Cleanup Focus" ? "No urgent ownership or direct-scene query cleanup is currently surfaced." : "No expected watch-list pressure is currently surfaced.");
                return;
            }
            foreach (PyralisSourceDependencyHygieneRecord record in records)
            {
                VisualElement card = PyralisAuthoringUi.Card(section, record.FileName, $"{record.Risk} ({record.RiskScore})");
                PyralisAuthoringUi.Field(card, "Posture", posture);
                PyralisAuthoringUi.Field(card, "Owner Domain", record.OwnerDomain);
                PyralisAuthoringUi.Field(card, "Pressure Kind", record.PressureKind.ToString());
                PyralisAuthoringUi.Field(card, "Dependencies", $"{record.DependencyCount} total, {record.Domains.Count} domain(s), {record.ConcreteCrossDomainCount} concrete cross-domain");
                PyralisAuthoringUi.Mini(card, record.ReviewHint);
                PyralisAuthoringUi.List(card, "Domains", record.Domains, visibleLimit: 6);
                PyralisAuthoringUi.List(card, "Pressure Reasons", record.Reasons, visibleLimit: 4);
                PyralisAuthoringUi.Field(card, "Source", record.AssetPath);
            }
        }

        private static void DrawFactCoverage(VisualElement parent, IReadOnlyList<PyralisAuthoringFact> facts)
        {
            System.Array kinds = System.Enum.GetValues(typeof(PyralisAuthoringFactKind));
            for (int i = 0; i < kinds.Length; i++)
            {
                PyralisAuthoringFactKind kind = (PyralisAuthoringFactKind)kinds.GetValue(i);
                int count = facts.Count(fact => fact != null && fact.Kind == kind);
                if (count > 0)
                    PyralisAuthoringUi.Field(parent, kind.ToString(), count.ToString());
            }
        }

        private static void DrawFactContracts(VisualElement parent, IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> contracts)
        {
            if (contracts == null || contracts.Count == 0)
                return;
            Foldout foldout = PyralisAuthoringUi.Foldout(parent, $"Graph Contract Coverage ({contracts.Count})", false);
            foreach (IGrouping<string, PyralisAuthoringReflectiveContractGraphRow> group in contracts.Where(row => row?.Contract != null).GroupBy(row => string.IsNullOrWhiteSpace(row.Contract.AuthoringCategory) ? "Uncategorized" : row.Contract.AuthoringCategory).OrderBy(group => group.Key, System.StringComparer.Ordinal))
            {
                Foldout category = PyralisAuthoringUi.Foldout(foldout, $"{group.Key} Contracts ({group.Count()})", false);
                foreach (PyralisAuthoringReflectiveContractGraphRow row in group)
                    DrawContract(category, row.Contract);
            }
        }

        private static void DrawContract(VisualElement parent, ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return;
            VisualElement card = PyralisAuthoringUi.Card(parent, contract.DisplayName, contract.StableId);
            PyralisAuthoringUi.Field(card, "Required Profile", contract.RequiredProfileType != null ? contract.RequiredProfileType.Name : "None for this module.");
            PyralisAuthoringUi.List(card, "Runtime Interfaces", contract.RequiredRuntimeInterfaceNames);
            PyralisAuthoringUi.List(card, "Required Unity Components", contract.RequiredComponentNames);
            PyralisAuthoringUi.List(card, "Assignment Fields", contract.AssignmentFields);
            PyralisAuthoringUi.List(card, "Customization Moments", contract.CustomizationMoments);
            PyralisAuthoringUi.Field(card, "First Proof Target", string.IsNullOrWhiteSpace(contract.FirstProofTargetId) ? "None recorded yet." : contract.FirstProofTargetId);
        }

        private static void DrawFactGroups(VisualElement parent, IReadOnlyList<PyralisAuthoringFact> facts)
        {
            System.Array kinds = System.Enum.GetValues(typeof(PyralisAuthoringFactKind));
            for (int i = 0; i < kinds.Length; i++)
            {
                PyralisAuthoringFactKind kind = (PyralisAuthoringFactKind)kinds.GetValue(i);
                PyralisAuthoringFact[] group = facts.Where(fact => fact != null && fact.Kind == kind).ToArray();
                if (group.Length == 0)
                    continue;
                Foldout foldout = PyralisAuthoringUi.Foldout(parent, $"{kind} ({group.Length})", false);
                int visible = System.Math.Min(group.Length, 32);
                for (int factIndex = 0; factIndex < visible; factIndex++)
                    DrawFact(foldout, group[factIndex]);
                if (group.Length > visible)
                    PyralisAuthoringUi.Mini(foldout, $"+{group.Length - visible} more fact(s) in JSON export.");
            }
        }

        private static void DrawFact(VisualElement parent, PyralisAuthoringFact fact)
        {
            VisualElement card = PyralisAuthoringUi.Card(parent, fact.DisplayName, fact.StableId);
            PyralisAuthoringUi.Field(card, "Source", fact.SourceKind + " / " + fact.Confidence);
            PyralisAuthoringUi.Field(card, "Capability", fact.Capability.ToString());
            PyralisAuthoringUi.Field(card, "Axioms", fact.Axioms.ToString());
            PyralisAuthoringUi.Field(card, "Work Intent", fact.WorkIntent);
            PyralisAuthoringUi.Field(card, "Route Relevance", fact.RouteRelevance);
            PyralisAuthoringUi.Mini(card, fact.Summary);
            PyralisAuthoringUi.List(card, "Assignment Fields", fact.AssignmentFields, visibleLimit: 4);
            PyralisAuthoringUi.List(card, "Customization", fact.CustomizationMoments, visibleLimit: 4);
            PyralisAuthoringUi.Field(card, "First Proof", fact.FirstProof);
        }

        private static void DrawNativeAction(VisualElement parent, PyralisAuthoringNativeAction action)
        {
            VisualElement card = PyralisAuthoringUi.Card(parent, "Native Unity Action", action.Surface.ToString());
            PyralisAuthoringUi.Field(card, "Action", action.ToGuidanceSentence());
            PyralisAuthoringUi.Field(card, "Target", action.Target);
            PyralisAuthoringUi.Field(card, "Field / Component", action.FieldOrComponent);
            PyralisAuthoringUi.Field(card, "Success Check", action.SuccessCheck);
        }
    }
}
