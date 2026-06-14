using System;
using System.Collections.Generic;
using System.Linq;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringSetupGraph
    {
        private readonly List<PyralisAuthoringGraphNode> _nodes;
        private readonly List<PyralisAuthoringGraphEdge> _edges;
        private readonly Dictionary<string, PyralisAuthoringGraphNode> _nodeById;

        public PyralisAuthoringSetupGraph(
            UnityEngine.Object source,
            PyralisSetupRouteAnalysis routeAnalysis,
            IEnumerable<PyralisAuthoringGraphNode> nodes,
            IEnumerable<PyralisAuthoringGraphEdge> edges)
        {
            Source = source;
            RouteAnalysis = routeAnalysis;
            RouteName = routeAnalysis != null && !string.IsNullOrWhiteSpace(routeAnalysis.RouteName)
                ? routeAnalysis.RouteName
                : "No setup route selected";
            _nodes = nodes != null
                ? nodes.Where(node => node != null).ToList()
                : new List<PyralisAuthoringGraphNode>();
            _edges = edges != null
                ? edges.Where(edge => edge != null).ToList()
                : new List<PyralisAuthoringGraphEdge>();
            _nodeById = new Dictionary<string, PyralisAuthoringGraphNode>(StringComparer.Ordinal);

            for (int i = 0; i < _nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = _nodes[i];
                if (!string.IsNullOrWhiteSpace(node.StableId) && !_nodeById.ContainsKey(node.StableId))
                    _nodeById.Add(node.StableId, node);
            }
        }

        public UnityEngine.Object Source { get; }
        internal PyralisSetupRouteAnalysis RouteAnalysis { get; }
        public string RouteName { get; }
        public IReadOnlyList<PyralisAuthoringGraphNode> Nodes => _nodes;
        public IReadOnlyList<PyralisAuthoringGraphEdge> Edges => _edges;

        public bool TryFindNode(string stableId, out PyralisAuthoringGraphNode node)
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                node = null;
                return false;
            }

            return _nodeById.TryGetValue(stableId, out node);
        }

        public IReadOnlyList<PyralisAuthoringGraphNode> FindNodes(PyralisAuthoringGraphNodeKind kind)
        {
            return _nodes.Where(node => node.Kind == kind).ToArray();
        }

        public IReadOnlyList<PyralisAuthoringGraphEdge> FindOutgoing(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                return Array.Empty<PyralisAuthoringGraphEdge>();

            return _edges.Where(edge => string.Equals(edge.FromNodeId, stableId, StringComparison.Ordinal)).ToArray();
        }
    }
}
