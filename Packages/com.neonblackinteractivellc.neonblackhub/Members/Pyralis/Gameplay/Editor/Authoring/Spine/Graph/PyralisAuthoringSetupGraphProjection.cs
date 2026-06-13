using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringSetupGraphRow
    {
        public PyralisAuthoringSetupGraphRow(
            string label,
            PyralisAuthoringGraphNode node,
            bool isOptional = false,
            string fallbackMessage = null,
            Object fallbackTarget = null)
        {
            Label = label ?? string.Empty;
            Node = node;
            IsOptional = isOptional;
            FallbackMessage = fallbackMessage ?? string.Empty;
            FallbackTarget = fallbackTarget;
        }

        public string Label { get; }
        public PyralisAuthoringGraphNode Node { get; }
        public bool IsOptional { get; }
        public string FallbackMessage { get; }
        public Object FallbackTarget { get; }
        public Object Target => Node != null && Node.SourceObject != null ? Node.SourceObject : FallbackTarget;
        public string Message => Node != null && !string.IsNullOrWhiteSpace(Node.Guidance) ? Node.Guidance : FallbackMessage;
        public bool IsReady => Node != null && (Node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready || Node.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional);
        public bool IsMissing => Node != null && (Node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing || Node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked);
    }

    public sealed class PyralisAuthoringValidationGraphRow
    {
        public PyralisAuthoringValidationGraphRow(PyralisAuthoringGraphNode node)
        {
            Node = node;
        }

        public PyralisAuthoringGraphNode Node { get; }
        public string NodeId => Node != null ? Node.StableId : string.Empty;
        public string Label => Node != null ? Node.Label : string.Empty;
        public string Message => Node != null ? Node.Guidance : string.Empty;
        public string NativeAction => Node != null && Node.NativeSetup.Length > 0 ? Node.NativeSetup[0] : string.Empty;
        public Object Target => Node != null ? Node.SourceObject : null;
        public bool CanInspectTarget => Target != null;
        public string SourceLabel => Node != null ? FormatSourceKind(Node.SourceKind) : string.Empty;
        public string OriginLabel => Node != null ? FormatSourceOrigin(Node.SourceOrigin) : string.Empty;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown;

        private static string FormatSourceKind(PyralisAuthoringGraphSourceKind sourceKind)
        {
            return sourceKind switch
            {
                PyralisAuthoringGraphSourceKind.SetupProfile => "Setup Profile",
                PyralisAuthoringGraphSourceKind.RuntimeCapabilityCatalog => "Capability Catalog",
                PyralisAuthoringGraphSourceKind.RuntimePattern => "Runtime Pattern",
                PyralisAuthoringGraphSourceKind.AuthoringContract => "Authoring Contract",
                PyralisAuthoringGraphSourceKind.FactRegistry => "Fact Registry",
                PyralisAuthoringGraphSourceKind.SetupFlow => "Setup Flow",
                PyralisAuthoringGraphSourceKind.SceneReadiness => "Scene Readiness",
                PyralisAuthoringGraphSourceKind.RouteProof => "Route Proof",
                _ => "Graph"
            };
        }

        private static string FormatSourceOrigin(PyralisAuthoringGraphSourceOrigin sourceOrigin)
        {
            return sourceOrigin switch
            {
                PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup => "User Authored Setup",
                PyralisAuthoringGraphSourceOrigin.Reflection => "Reflection",
                PyralisAuthoringGraphSourceOrigin.Contract => "Contract",
                PyralisAuthoringGraphSourceOrigin.RuntimeEvidence => "Runtime Evidence",
                PyralisAuthoringGraphSourceOrigin.SpineGrammar => "Spine Grammar",
                PyralisAuthoringGraphSourceOrigin.SpineFallback => "Spine Fallback",
                PyralisAuthoringGraphSourceOrigin.LegacyFact => "Legacy Fact",
                _ => "Unknown"
            };
        }
    }

    public sealed class PyralisAuthoringGraphConnectionRow
    {
        public PyralisAuthoringGraphConnectionRow(
            PyralisAuthoringGraphNode from,
            PyralisAuthoringGraphNode to,
            PyralisAuthoringGraphEdge edge)
        {
            From = from;
            To = to;
            Edge = edge;
        }

        public PyralisAuthoringGraphNode From { get; }
        public PyralisAuthoringGraphNode To { get; }
        public PyralisAuthoringGraphEdge Edge { get; }
        public string FromLabel => From != null ? From.Label : Edge != null ? Edge.FromNodeId : string.Empty;
        public string ToLabel => To != null ? To.Label : Edge != null ? Edge.ToNodeId : string.Empty;
        public string Relationship => Edge != null ? FormatEdgeKind(Edge.Kind) : string.Empty;
        public string Detail => Edge != null ? Edge.Label : string.Empty;
        public string FromOrigin => From != null ? From.SourceOrigin.ToString() : string.Empty;
        public string ToOrigin => To != null ? To.SourceOrigin.ToString() : string.Empty;

        private static string FormatEdgeKind(PyralisAuthoringGraphEdgeKind kind)
        {
            return kind switch
            {
                PyralisAuthoringGraphEdgeKind.DependsOn => "depends on",
                PyralisAuthoringGraphEdgeKind.Satisfies => "satisfies",
                PyralisAuthoringGraphEdgeKind.Recommends => "recommends",
                PyralisAuthoringGraphEdgeKind.SupportsProof => "supports proof",
                PyralisAuthoringGraphEdgeKind.BlockedBy => "blocked by",
                _ => "relates to"
            };
        }
    }

    public sealed class PyralisAuthoringReflectiveContractGraphRow
    {
        public PyralisAuthoringReflectiveContractGraphRow(PyralisAuthoringGraphNode node)
        {
            Node = node;
        }

        public PyralisAuthoringGraphNode Node { get; }
        public string Label => Node != null ? Node.Label : string.Empty;
        public string Message => Node != null ? Node.Guidance : string.Empty;
        public Object Target => Node != null ? Node.SourceObject : null;
        public ResolvedAuthoringContract Contract => Node != null ? Node.SourceContract : null;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown;
    }

    public sealed class PyralisAuthoringSelectedContextGraphRow
    {
        public PyralisAuthoringSelectedContextGraphRow(
            Object selection,
            PyralisAuthoringGraphNode node,
            string role,
            string nextCheck)
        {
            Selection = selection;
            Node = node;
            Role = role ?? string.Empty;
            NextCheck = nextCheck ?? string.Empty;
        }

        public Object Selection { get; }
        public PyralisAuthoringGraphNode Node { get; }
        public string NodeId => Node != null ? Node.StableId : string.Empty;
        public string Label => Node != null ? Node.Label : Selection != null ? Selection.GetType().Name : "No Selection";
        public string Role { get; }
        public string NextCheck { get; }
        public string RuntimeMeaning => Node != null && !string.IsNullOrWhiteSpace(Node.Guidance) ? Node.Guidance : Role;
        public string NativeSetup => Node != null && Node.NativeSetup.Length > 0 ? string.Join("; ", Node.NativeSetup) : string.Empty;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown;
    }

    public sealed class PyralisAuthoringCurrentStepGraphRow
    {
        public PyralisAuthoringCurrentStepGraphRow(
            string routeName,
            PyralisAuthoringGraphNode node,
            string message,
            string detail,
            Object target)
        {
            RouteName = routeName ?? string.Empty;
            Node = node;
            Message = message ?? string.Empty;
            Detail = detail ?? string.Empty;
            Target = target;
        }

        public string RouteName { get; }
        public PyralisAuthoringGraphNode Node { get; }
        public string Label => Node != null ? Node.Label : "Create Setup Foundation";
        public string Message { get; }
        public string Detail { get; }
        public Object Target { get; }
        public PyralisAuthoringNativeAction? NativeAction => Node?.NativeAction;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Missing;
        public bool HasNode => Node != null;
    }

    public static class PyralisAuthoringSetupGraphProjection
    {
        public static IReadOnlyList<PyralisAuthoringSetupGraphRow> BuildSetupMapRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringSetupGraphRow>();

            List<PyralisAuthoringSetupGraphRow> rows = new List<PyralisAuthoringSetupGraphRow>
            {
                Row(graph, "Scene Root", "bootstrap.root", "Scene object that starts the session."),
                Row(graph, "Session", "session.definition", "Asset that names game rules and participants."),
                Row(graph, "Game Rules", "mode.definition", "Ruleset that chooses the setup profile."),
                Row(graph, "Setup Profile", "setup.profile", "Editable capability contract for this route."),
                BuildCapabilitiesRow(graph),
                Row(graph, "Participants", "participant.default", "Assign at least one default participant."),
                Row(graph, "Pawn / No Pawn", "pawn.definition", "Pawn-backed routes need a ParticipantDefinition.defaultPawn.", isOptional: graph.RouteAnalysis == null || !graph.RouteAnalysis.RequiresPawn),
                Row(graph, "Scene Surfaces", "scene.surfaces", "Route-recommended scene surface evidence is present or not needed yet.")
            };

            return rows;
        }

        public static IReadOnlyList<PyralisAuthoringSetupGraphRow> BuildReadinessRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringSetupGraphRow>();

            return new[]
            {
                Row(graph, "Scene Root", "bootstrap.root"),
                Row(graph, "Session", "session.definition"),
                Row(graph, "Game Rules", "mode.definition"),
                Row(graph, "Setup Profile", "setup.profile"),
                BuildCapabilitiesRow(graph),
                Row(graph, "Players / Seats", "participant.default"),
                Row(graph, "Pawn / No Pawn", "pawn.definition", isOptional: graph.RouteAnalysis == null || !graph.RouteAnalysis.RequiresPawn),
                Row(graph, "Scene Roots", "scene.surfaces", isOptional: true)
            };
        }

        public static IReadOnlyList<PyralisAuthoringGraphNode> FindSceneSurfaceNodes(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphNode>();

            return graph.FindNodes(PyralisAuthoringGraphNodeKind.SceneSurface)
                .Where(node => node != null && !string.Equals(node.StableId, "scene.surfaces", StringComparison.Ordinal))
                .ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringGraphConnectionRow> BuildMapConnectionRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphConnectionRow>();

            List<PyralisAuthoringGraphConnectionRow> rows = new List<PyralisAuthoringGraphConnectionRow>();
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                PyralisAuthoringGraphEdge edge = graph.Edges[i];
                if (edge == null)
                    continue;

                if (!graph.TryFindNode(edge.FromNodeId, out PyralisAuthoringGraphNode from)
                    || !graph.TryFindNode(edge.ToNodeId, out PyralisAuthoringGraphNode to))
                {
                    continue;
                }

                rows.Add(new PyralisAuthoringGraphConnectionRow(from, to, edge));
            }

            return rows.ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringGraphConnectionRow> BuildProofSupportRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphConnectionRow>();

            List<PyralisAuthoringGraphConnectionRow> rows = new List<PyralisAuthoringGraphConnectionRow>();
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> connections = BuildMapConnectionRows(graph);
            for (int i = 0; i < connections.Count; i++)
            {
                PyralisAuthoringGraphConnectionRow row = connections[i];
                if (row == null || row.To == null || row.To.Kind != PyralisAuthoringGraphNodeKind.Proof)
                    continue;

                if (row.Edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof
                    || row.Edge.Kind == PyralisAuthoringGraphEdgeKind.Recommends
                    || row.Edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy)
                {
                    rows.Add(row);
                }
            }

            return rows.ToArray();
        }

        public static PyralisAuthoringGraphNode FindCurrentProofNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            IReadOnlyList<PyralisAuthoringGraphNode> proofNodes = graph.FindNodes(PyralisAuthoringGraphNodeKind.Proof);
            return proofNodes.Count > 0 ? proofNodes[0] : null;
        }

        public static PyralisAuthoringGraphNode FindFirstUnresolvedNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            PyralisAuthoringGraphNode node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.SetupChain);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.ValidationEvidence);
            if (node != null)
                return node;

            return FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.SceneSurface);
        }

        public static PyralisAuthoringCurrentStepGraphRow BuildCurrentStepRow(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null || graph.Source == null)
                return BuildFallbackCurrentStepRow();

            PyralisAuthoringGraphNode node = FindFirstUnresolvedSetupFlowNode(graph)
                ?? FindFirstUnresolvedNode(graph);

            if (node != null)
            {
                string message = !string.IsNullOrWhiteSpace(node.Guidance)
                    ? node.Guidance
                    : !string.IsNullOrWhiteSpace(node.BlockingReason)
                        ? node.BlockingReason
                        : "Inspect this graph node and clear its missing setup evidence.";

                string detail = GetCurrentStepDetail(node);
                string routeName = GetCurrentRouteName(graph);
                return new PyralisAuthoringCurrentStepGraphRow(routeName, node, message, detail, node.SourceObject);
            }

            return BuildFallbackCurrentStepRow();
        }

        public static int CountNodes(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphEvidenceState evidenceState)
        {
            if (graph == null)
                return 0;

            return graph.Nodes.Count(node => node != null && node.EvidenceState == evidenceState);
        }

        public static IReadOnlyList<PyralisAuthoringValidationGraphRow> BuildValidationRows(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphEvidenceState evidenceState)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringValidationGraphRow>();

            return graph.Nodes
                .Where(node => node != null
                    && IsReadinessNode(node)
                    && node.EvidenceState == evidenceState)
                .Select(node => new PyralisAuthoringValidationGraphRow(node))
                .ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> BuildReflectiveContractRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringReflectiveContractGraphRow>();

            return graph.FindNodes(PyralisAuthoringGraphNodeKind.Contract)
                .Where(node => node != null
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.AuthoringContract
                    && node.EvidenceState != PyralisAuthoringGraphEvidenceState.Unknown)
                .Select(node => new PyralisAuthoringReflectiveContractGraphRow(node))
                .ToArray();
        }

        public static PyralisAuthoringSelectedContextGraphRow BuildSelectedContextRow(PyralisAuthoringSetupGraph graph, Object selection)
        {
            if (selection == null)
            {
                return new PyralisAuthoringSelectedContextGraphRow(
                    null,
                    null,
                    "Select a Pyralis setup asset, scene root, pawn prefab, or component to see its authoring meaning.",
                    "Select an object that participates in the setup route.");
            }

            PyralisAuthoringGraphNode node = FindSelectedNode(graph, selection);
            return new PyralisAuthoringSelectedContextGraphRow(
                selection,
                node,
                GetSelectionRole(selection, node),
                GetSelectionNextCheck(selection, node));
        }

        private static PyralisAuthoringSetupGraphRow BuildCapabilitiesRow(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode setupNode = FindNode(graph, "setup.profile");
            return new PyralisAuthoringSetupGraphRow(
                "Capabilities",
                FindNode(graph, "capability.selected"),
                fallbackMessage: "Choose capability ingredients before scene wiring.",
                fallbackTarget: setupNode?.SourceObject);
        }

        public static bool IsReadinessNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                || node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                || node.Kind == PyralisAuthoringGraphNodeKind.SceneSurface)
            {
                return true;
            }

            return node.Kind == PyralisAuthoringGraphNodeKind.Capability
                && string.Equals(node.StableId, "capability.selected", StringComparison.Ordinal);
        }

        private static PyralisAuthoringGraphNode FindFirstUnresolvedNode(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphNodeKind kind)
        {
            IReadOnlyList<PyralisAuthoringGraphNode> nodes = graph.FindNodes(kind);
            for (int i = 0; i < nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = nodes[i];
                if (node == null)
                    continue;

                if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
                    return node;
            }

            return null;
        }

        private static PyralisAuthoringGraphNode FindFirstUnresolvedSetupFlowNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = graph.Nodes[i];
                if (node == null || node.Kind != PyralisAuthoringGraphNodeKind.ValidationEvidence)
                    continue;

                if (node.SourceKind != PyralisAuthoringGraphSourceKind.SetupFlow)
                    continue;

                if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
                    return node;
            }

            return null;
        }

        private static PyralisAuthoringCurrentStepGraphRow BuildFallbackCurrentStepRow()
        {
            const string routeName = "No setup route selected";
            const string message = "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign the first SessionDefinition asset.";
            const string detail = "Start with the native Unity setup chain: Hierarchy object, bootstrap component, SessionDefinition, GameModeDefinition, GameSetupProfile, capability intent, then participants.";

            return new PyralisAuthoringCurrentStepGraphRow(routeName, null, message, detail, null);
        }

        private static string GetCurrentStepDetail(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(node.BlockingReason) && !string.Equals(node.BlockingReason, node.Guidance, StringComparison.Ordinal))
                return node.BlockingReason;

            if (node.AssignmentFields.Length > 0)
                return "Inspector field: " + string.Join(", ", node.AssignmentFields);

            if (node.NativeSetup.Length > 0)
                return node.NativeSetup[0];

            return "Use Map for topology, Validate for the full issue list, and the Inspector for field-level edits.";
        }

        private static string GetCurrentRouteName(PyralisAuthoringSetupGraph graph)
        {
            if (graph != null && graph.RouteAnalysis != null)
            {
                PyralisAuthoringRouteDescriptor descriptor = PyralisAuthoringRouteDescriptor.Build(graph.RouteAnalysis);
                if (descriptor != null && !string.IsNullOrWhiteSpace(descriptor.RouteName))
                    return descriptor.RouteName;
            }

            return "No setup route selected";
        }

        private static PyralisAuthoringSetupGraphRow Row(
            PyralisAuthoringSetupGraph graph,
            string label,
            string nodeId,
            string fallbackMessage = null,
            bool isOptional = false)
        {
            PyralisAuthoringGraphNode node = FindNode(graph, nodeId);
            return new PyralisAuthoringSetupGraphRow(label, node, isOptional, fallbackMessage);
        }

        private static PyralisAuthoringGraphNode FindNode(PyralisAuthoringSetupGraph graph, string nodeId)
        {
            return graph != null && graph.TryFindNode(nodeId, out PyralisAuthoringGraphNode node) ? node : null;
        }

        private static PyralisAuthoringGraphNode FindSelectedNode(PyralisAuthoringSetupGraph graph, Object selection)
        {
            if (graph == null || selection == null)
                return null;

            string stableId = GetKnownSelectionNodeId(selection);
            if (!string.IsNullOrWhiteSpace(stableId) && graph.TryFindNode(stableId, out PyralisAuthoringGraphNode knownNode))
                return knownNode;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = graph.Nodes[i];
                if (node != null && node.SourceObject == selection)
                    return node;
            }

            ResolvedAuthoringContract contract = ResolveSelectionContract(selection);
            if (contract != null)
            {
                if (!string.IsNullOrWhiteSpace(contract.SetupNodeId) && graph.TryFindNode(contract.SetupNodeId, out PyralisAuthoringGraphNode setupNode))
                    return setupNode;

                if (graph.TryFindNode("contract." + contract.StableId, out PyralisAuthoringGraphNode contractNode))
                    return contractNode;
            }

            return null;
        }

        private static string GetKnownSelectionNodeId(Object selection)
        {
            if (selection is GameplaySessionBootstrap)
                return "bootstrap.root";

            if (selection is GameObject gameObject)
            {
                if (gameObject.GetComponent<GameplaySessionBootstrap>() != null)
                    return "bootstrap.root";
                if (gameObject.GetComponent<PawnRoot>() != null)
                    return "pawn.definition";
            }

            if (selection is Component component)
            {
                if (component is GameplaySessionBootstrap)
                    return "bootstrap.root";
                if (component is PawnRoot)
                    return "pawn.definition";
            }

            return selection switch
            {
                SessionDefinition => "session.definition",
                GameModeDefinition => "mode.definition",
                GameSetupProfile => "setup.profile",
                ParticipantDefinition => "participant.default",
                PawnDefinition => "pawn.definition",
                _ => string.Empty
            };
        }

        private static ResolvedAuthoringContract ResolveSelectionContract(Object selection)
        {
            if (selection is FeatureModuleDefinition module && !string.IsNullOrWhiteSpace(module.moduleId))
                return ResolvedAuthoringContractRegistry.FindByModuleId(module.moduleId);

            if (selection is Component component)
                return ResolvedAuthoringContractRegistry.FindByType(component.GetType());

            return ResolvedAuthoringContractRegistry.FindByType(selection.GetType());
        }

        private static string GetSelectionRole(Object selection, PyralisAuthoringGraphNode node)
        {
            if (node != null && !string.IsNullOrWhiteSpace(node.Guidance))
                return node.Guidance;

            return selection switch
            {
                GameplaySessionBootstrap => "Scene startup and setup-flow root.",
                SessionDefinition => "Session contract for game rules, participants, local/network mode, and participant limits.",
                GameModeDefinition => "Rules contract that chooses the setup profile and rule-level defaults.",
                GameSetupProfile => "Capability ingredients that describe what this game route needs before scene wiring starts.",
                RuntimePatternDefinition => "Optional advanced route contract for reusable metadata beyond generic capability rows.",
                ParticipantDefinition => "Seat, player, NPC, hand, faction, or command owner in the session.",
                PawnDefinition => "Pawn prefab, profiles, feature modules, and presentation setup.",
                FeatureModuleDefinition => "Feature module contract selected by the route.",
                Component component => component is PawnRoot
                    ? "PawnRoot marks the prefab root that Pyralis treats as a pawn actor."
                    : "Runtime or authoring component participating in the selected GameObject.",
                GameObject => "Scene or prefab object. Pyralis components on it determine how it participates in the route.",
                _ => "Use this asset's Inspector for fields, and use Guide to understand how it fits into setup."
            };
        }

        private static string GetSelectionNextCheck(Object selection, PyralisAuthoringGraphNode node)
        {
            if (node != null)
            {
                if (!string.IsNullOrWhiteSpace(node.BlockingReason))
                    return node.BlockingReason;
                if (node.AssignmentFields.Length > 0)
                    return "Inspect: " + string.Join(", ", node.AssignmentFields);
                if (node.NativeSetup.Length > 0)
                    return "Native setup: " + string.Join("; ", node.NativeSetup);
            }

            return selection switch
            {
                GameplaySessionBootstrap => "Inspect session definition, spawn points, input manager, and camera rig references.",
                SessionDefinition => "Inspect default game mode and default participants.",
                GameModeDefinition => "Inspect setup profile and required feature modules.",
                GameSetupProfile => "Open Intent when changing capability ingredients; keep Guide read-only for this selection.",
                ParticipantDefinition => "Inspect default pawn, input profile, seat index, and auto-join ownership.",
                PawnDefinition => "Inspect pawn prefab, movement/input/presentation profiles, and feature modules.",
                Component => "Use the Inspector for field values and Validate for concrete readiness issues.",
                GameObject => "Select the most specific Pyralis component on this object when you need field-level meaning.",
                _ => "Inspect the selected asset fields in Unity."
            };
        }
    }
}
