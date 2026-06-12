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

        public static PyralisAuthoringGraphNode FindCurrentProofNode(PyralisAuthoringSetupGraph graph)
        {
            return FindNode(graph, "proof.current");
        }

        public static PyralisAuthoringGraphNode FindFirstUnresolvedNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            PyralisAuthoringGraphNode node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.SetupChain);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.PrefabRequirement);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.ValidationEvidence);
            if (node != null)
                return node;

            return FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.SceneSurface);
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

            return graph.FindNodes(PyralisAuthoringGraphNodeKind.ValidationEvidence)
                .Where(node => node != null
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.SceneReadiness
                    && node.EvidenceState == evidenceState)
                .Select(node => new PyralisAuthoringValidationGraphRow(node))
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
            IReadOnlyList<PyralisAuthoringGraphNode> capabilityNodes = graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability);
            PyralisAuthoringGraphNode setupNode = FindNode(graph, "setup.profile");
            bool hasCapabilities = capabilityNodes.Count > 0;
            string message = GetCapabilityMessage(graph, hasCapabilities);
            PyralisAuthoringGraphNode node = new PyralisAuthoringGraphNode(
                "capability.selected",
                "Capabilities",
                PyralisAuthoringGraphNodeKind.Capability,
                PyralisAuthoringGraphSourceKind.SetupProfile,
                hasCapabilities ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: message,
                sourceObject: setupNode?.SourceObject);

            return new PyralisAuthoringSetupGraphRow("Capabilities", node, fallbackMessage: message, fallbackTarget: setupNode?.SourceObject);
        }

        private static string GetCapabilityMessage(PyralisAuthoringSetupGraph graph, bool hasCapabilities)
        {
            if (graph == null || graph.RouteAnalysis == null || graph.RouteAnalysis.SetupProfile == null)
                return "Create or assign the setup profile before choosing capabilities.";

            if (!hasCapabilities || !graph.RouteAnalysis.HasAssignedPatterns)
                return "Choose capability ingredients before scene wiring.";

            if (!graph.RouteAnalysis.HasValidPatterns)
                return "Fix setup capability validation before trusting route guidance.";

            return graph.RouteAnalysis.RouteName;
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
