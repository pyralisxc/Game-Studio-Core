using System;
using System.Collections.Generic;
using Pys.Authoring.Contracts;

namespace Pys.Authoring.Editor.Scanning
{
    public sealed class UnityCodebaseScanRequest
    {
        public UnityCodebaseScanRequest(string scriptsRoot)
        {
            ScriptsRoot = scriptsRoot ?? "Assets";
        }

        public string ScriptsRoot { get; }
    }

    public sealed class UnityTypeObservation
    {
        public UnityTypeObservation(Type type, string assetPath = null)
        {
            Type = type;
            AssemblyName = type != null ? type.Assembly.GetName().Name : string.Empty;
            FullName = type != null ? type.FullName : string.Empty;
            DisplayName = type != null ? type.Name : string.Empty;
            AssetPath = assetPath ?? string.Empty;
            ImplementedInterfaces = new List<string>();
            SerializedFields = new List<string>();
            RequiredComponents = new List<string>();
            Contracts = new List<ResolvedAuthoringContract>();
        }

        public Type Type { get; }

        public string AssemblyName { get; }

        public string FullName { get; }

        public string DisplayName { get; }

        public string AssetPath { get; }

        public List<string> ImplementedInterfaces { get; }

        public List<string> SerializedFields { get; }

        public List<string> RequiredComponents { get; }

        public List<ResolvedAuthoringContract> Contracts { get; }

        public bool ImplementsAuthoringValidationProvider { get; set; }
    }

    public sealed class AssemblyDefinitionObservation
    {
        public AssemblyDefinitionObservation(string assetPath, string name)
        {
            AssetPath = assetPath ?? string.Empty;
            Name = name ?? string.Empty;
            References = new List<string>();
        }

        public string AssetPath { get; }

        public string Name { get; }

        public List<string> References { get; }
    }

    public sealed class SourceDependencyObservation
    {
        public SourceDependencyObservation(string assetPath)
        {
            AssetPath = assetPath ?? string.Empty;
            Namespaces = new List<string>();
        }

        public string AssetPath { get; }

        public List<string> Namespaces { get; }
    }

    public sealed class UnityObjectObservation
    {
        public UnityObjectObservation(string objectId, string label, string sourcePath, string typeName)
        {
            ObjectId = objectId ?? string.Empty;
            Label = label ?? string.Empty;
            SourcePath = sourcePath ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            Components = new List<string>();
            Issues = new List<AuthoringIssue>();
        }

        public string ObjectId { get; }

        public string Label { get; }

        public string SourcePath { get; }

        public string TypeName { get; }

        public List<string> Components { get; }

        public List<AuthoringIssue> Issues { get; }
    }

    public sealed class UnityAssetObservation
    {
        public UnityAssetObservation(string objectId, string label, string sourcePath, string typeName)
        {
            ObjectId = objectId ?? string.Empty;
            Label = label ?? string.Empty;
            SourcePath = sourcePath ?? string.Empty;
            TypeName = typeName ?? string.Empty;
        }

        public string ObjectId { get; }

        public string Label { get; }

        public string SourcePath { get; }

        public string TypeName { get; }
    }

    public sealed class UnityCodebaseScanResult
    {
        public UnityCodebaseScanResult()
        {
            ScriptsRoot = string.Empty;
            Types = new List<UnityTypeObservation>();
            AssemblyDefinitions = new List<AssemblyDefinitionObservation>();
            SourceDependencies = new List<SourceDependencyObservation>();
            SceneObjects = new List<UnityObjectObservation>();
            Prefabs = new List<UnityObjectObservation>();
            Assets = new List<UnityAssetObservation>();
        }

        public string ScriptsRoot { get; set; }

        public List<UnityTypeObservation> Types { get; }

        public List<AssemblyDefinitionObservation> AssemblyDefinitions { get; }

        public List<SourceDependencyObservation> SourceDependencies { get; }

        public List<UnityObjectObservation> SceneObjects { get; }

        public List<UnityObjectObservation> Prefabs { get; }

        public List<UnityAssetObservation> Assets { get; }
    }
}
