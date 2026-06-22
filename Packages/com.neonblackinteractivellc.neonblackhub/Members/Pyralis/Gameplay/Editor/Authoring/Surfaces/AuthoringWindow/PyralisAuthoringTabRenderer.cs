using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
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
                PyralisAuthoringGraphJsonExportControl.BuildHygieneSnapshotButton(projection),
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
            PyralisAuthoringUi.List(summary, "Ownership Buckets", projection.BuildOwnershipBucketSummary());
            DrawHygieneSections(page, projection);
            DrawDependencyPressure(page, "Cleanup Focus", projection.CleanupFocus, "Actionable cleanup");
            DrawDependencyPressure(page, "Watch List", projection.WatchList, "Expected pressure");
            return page;
        }

        public static VisualElement BuildFacts(PyralisAuthoringFactsProjection projection)
        {
            var page = PyralisAuthoringUi.Page("Fact Explorer", "Read-only dictionary view. Facts explain Pyralis vocabulary, reflected contracts, provenance, and coverage without owning route, proof, customization, or setup guidance.");
            page.Add(PyralisAuthoringUi.ActionRow(PyralisAuthoringGraphJsonExportControl.BuildFactsSnapshotButton(projection?.Graph)));
            if (projection == null)
                return page;
            VisualElement summary = PyralisAuthoringUi.Section(page, "Coverage");
            PyralisAuthoringUi.Field(summary, "Active Setup", PyralisAuthoringUi.ObjectLabel(projection.ActiveSetup, "No active setup selected"));
            PyralisAuthoringUi.Field(summary, "Graph Nodes", (projection.Graph?.Nodes.Count ?? 0).ToString());
            PyralisAuthoringUi.Field(summary, "Graph Edges", (projection.Graph?.Edges.Count ?? 0).ToString());
            PyralisAuthoringUi.Field(summary, "Total Facts", projection.Facts.Count.ToString());
            DrawFactCoverage(summary, projection.FactKindRows);
            DrawFactContracts(page, projection.ContractGroups);
            DrawFactGroups(page, projection.FactGroups);
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

        private static void DrawFactCoverage(VisualElement parent, IReadOnlyList<PyralisAuthoringFactKindSummaryRow> factKindRows)
        {
            if (factKindRows == null)
                return;

            for (int i = 0; i < factKindRows.Count; i++)
            {
                PyralisAuthoringFactKindSummaryRow row = factKindRows[i];
                if (row != null && row.Count > 0)
                    PyralisAuthoringUi.Field(parent, row.Label, row.Count.ToString());
            }
        }

        private static void DrawFactContracts(VisualElement parent, IReadOnlyList<PyralisAuthoringContractGroupRow> contractGroups)
        {
            if (contractGroups == null || contractGroups.Count == 0)
                return;

            int contractCount = 0;
            for (int i = 0; i < contractGroups.Count; i++)
                contractCount += contractGroups[i]?.Contracts.Count ?? 0;

            Foldout foldout = PyralisAuthoringUi.Foldout(parent, $"Graph Contract Coverage ({contractCount})", false);
            for (int groupIndex = 0; groupIndex < contractGroups.Count; groupIndex++)
            {
                PyralisAuthoringContractGroupRow group = contractGroups[groupIndex];
                if (group == null || group.Contracts.Count == 0)
                    continue;

                Foldout category = PyralisAuthoringUi.Foldout(foldout, $"{group.Category} Contracts ({group.Contracts.Count})", false);
                for (int contractIndex = 0; contractIndex < group.Contracts.Count; contractIndex++)
                    DrawContract(category, group.Contracts[contractIndex]?.Contract);
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

        private static void DrawFactGroups(VisualElement parent, IReadOnlyList<PyralisAuthoringFactGroupRow> factGroups)
        {
            if (factGroups == null)
                return;

            for (int i = 0; i < factGroups.Count; i++)
            {
                PyralisAuthoringFactGroupRow group = factGroups[i];
                if (group == null || group.Facts.Count == 0)
                    continue;
                Foldout foldout = PyralisAuthoringUi.Foldout(parent, $"{group.Label} ({group.Facts.Count})", false);
                int visible = System.Math.Min(group.Facts.Count, 32);
                for (int factIndex = 0; factIndex < visible; factIndex++)
                    DrawFact(foldout, group.Facts[factIndex]);
                if (group.Facts.Count > visible)
                    PyralisAuthoringUi.Mini(foldout, $"+{group.Facts.Count - visible} more fact(s) in JSON export.");
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
