using System;
using System.Collections.Generic;

namespace Pys.Authoring.Contracts
{
    public enum AuthoringGraphNodeKind
    {
        Assembly,
        Namespace,
        Type,
        Script,
        Component,
        Field,
        ScriptableObject,
        Contract,
        Validator,
        SceneObject,
        Prefab,
        Asset,
        Issue
    }

    public enum AuthoringGraphEdgeKind
    {
        AssemblyReference,
        NamespaceUsing,
        Inherits,
        Implements,
        SerializedField,
        RequiredComponent,
        ContractDeclares,
        ValidatorReports,
        SceneContains,
        PrefabContains,
        Observes,
        Owns
    }

    public sealed class AuthoringGraphNode
    {
        public AuthoringGraphNode(string id, string label, AuthoringGraphNodeKind kind)
        {
            Id = id ?? string.Empty;
            Label = label ?? string.Empty;
            Kind = kind;
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public string Id { get; }

        public string Label { get; }

        public AuthoringGraphNodeKind Kind { get; }

        public Dictionary<string, string> Metadata { get; }
    }

    public sealed class AuthoringGraphEdge
    {
        public AuthoringGraphEdge(string fromNodeId, string toNodeId, AuthoringGraphEdgeKind kind)
        {
            FromNodeId = fromNodeId ?? string.Empty;
            ToNodeId = toNodeId ?? string.Empty;
            Kind = kind;
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public string FromNodeId { get; }

        public string ToNodeId { get; }

        public AuthoringGraphEdgeKind Kind { get; }

        public Dictionary<string, string> Metadata { get; }
    }

    public sealed class AuthoringGraph
    {
        public List<AuthoringGraphNode> Nodes { get; } = new List<AuthoringGraphNode>();

        public List<AuthoringGraphEdge> Edges { get; } = new List<AuthoringGraphEdge>();
    }
}
