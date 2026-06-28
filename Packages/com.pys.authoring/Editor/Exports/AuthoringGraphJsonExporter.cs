using System;
using System.Collections.Generic;
using System.IO;
using Pys.Authoring.Contracts;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Exports
{
    public static class AuthoringGraphJsonExporter
    {
        public const string DefaultExportFolder = "Temp/PysAuthoringExports";

        public static string Export(AuthoringGraph graph, string scriptsRoot)
        {
            string folder = Path.Combine(Path.GetFullPath("."), DefaultExportFolder);
            Directory.CreateDirectory(folder);

            string safeRoot = string.IsNullOrWhiteSpace(scriptsRoot)
                ? "Project"
                : scriptsRoot.Replace('/', '_').Replace('\\', '_').Replace(':', '_');

            string path = Path.Combine(folder, safeRoot + "_authoring_graph.json");
            AuthoringGraphExportDto dto = AuthoringGraphExportDto.FromGraph(graph, scriptsRoot);
            File.WriteAllText(path, JsonUtility.ToJson(dto, true));
            EditorUtility.RevealInFinder(path);
            return path;
        }

        public static void OpenExportFolder()
        {
            string folder = Path.Combine(Path.GetFullPath("."), DefaultExportFolder);
            Directory.CreateDirectory(folder);
            EditorUtility.RevealInFinder(folder);
        }

        [Serializable]
        private sealed class AuthoringGraphExportDto
        {
            public string scriptsRoot;
            public int nodeCount;
            public int edgeCount;
            public List<NodeDto> nodes = new List<NodeDto>();
            public List<EdgeDto> edges = new List<EdgeDto>();

            public static AuthoringGraphExportDto FromGraph(AuthoringGraph graph, string scriptsRoot)
            {
                AuthoringGraphExportDto dto = new AuthoringGraphExportDto
                {
                    scriptsRoot = scriptsRoot ?? string.Empty,
                    nodeCount = graph != null ? graph.Nodes.Count : 0,
                    edgeCount = graph != null ? graph.Edges.Count : 0
                };

                if (graph == null)
                    return dto;

                for (int i = 0; i < graph.Nodes.Count; i++)
                    dto.nodes.Add(NodeDto.FromNode(graph.Nodes[i]));

                for (int i = 0; i < graph.Edges.Count; i++)
                    dto.edges.Add(EdgeDto.FromEdge(graph.Edges[i]));

                return dto;
            }
        }

        [Serializable]
        private sealed class NodeDto
        {
            public string id;
            public string label;
            public string kind;
            public List<MetadataDto> metadata = new List<MetadataDto>();

            public static NodeDto FromNode(AuthoringGraphNode node)
            {
                NodeDto dto = new NodeDto
                {
                    id = node != null ? node.Id : string.Empty,
                    label = node != null ? node.Label : string.Empty,
                    kind = node != null ? node.Kind.ToString() : string.Empty
                };

                if (node != null)
                    AddMetadata(dto.metadata, node.Metadata);

                return dto;
            }
        }

        [Serializable]
        private sealed class EdgeDto
        {
            public string fromNodeId;
            public string toNodeId;
            public string kind;
            public List<MetadataDto> metadata = new List<MetadataDto>();

            public static EdgeDto FromEdge(AuthoringGraphEdge edge)
            {
                EdgeDto dto = new EdgeDto
                {
                    fromNodeId = edge != null ? edge.FromNodeId : string.Empty,
                    toNodeId = edge != null ? edge.ToNodeId : string.Empty,
                    kind = edge != null ? edge.Kind.ToString() : string.Empty
                };

                if (edge != null)
                    AddMetadata(dto.metadata, edge.Metadata);

                return dto;
            }
        }

        [Serializable]
        private sealed class MetadataDto
        {
            public string key;
            public string value;
        }

        private static void AddMetadata(List<MetadataDto> target, Dictionary<string, string> metadata)
        {
            if (target == null || metadata == null)
                return;

            foreach (KeyValuePair<string, string> pair in metadata)
            {
                target.Add(new MetadataDto
                {
                    key = pair.Key ?? string.Empty,
                    value = pair.Value ?? string.Empty
                });
            }
        }
    }
}
