using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor.Authoring
{
    public sealed class PyralisHygieneIssue
    {
        public Object Asset { get; }
        public string PropertyPath { get; }
        public string Message { get; }
        public PyralisAuthoringContract Contract { get; }

        public PyralisHygieneIssue(Object asset, string propertyPath, string message, PyralisAuthoringContract contract = null)
        {
            Asset = asset;
            PropertyPath = propertyPath;
            Message = message;
            Contract = contract;
        }
    }

    public static class PyralisAssetHygieneScanner
    {
        public static List<PyralisHygieneIssue> Scan(SessionDefinition session)
        {
            List<PyralisHygieneIssue> issues = new List<PyralisHygieneIssue>();
            if (session == null) return issues;

            HashSet<Object> visited = new HashSet<Object>();
            ScanRecursive(session, visited, issues);

            return issues;
        }

        private static void ScanRecursive(Object asset, HashSet<Object> visited, List<PyralisHygieneIssue> issues)
        {
            if (asset == null || !visited.Add(asset))
                return;

            // 1. Check contracts for this asset type
            var contract = PyralisAuthoringContractRegistry.FindByType(asset.GetType());
            if (contract != null && contract.AssignmentFields != null)
            {
                SerializedObject so = new SerializedObject(asset);
                foreach (var fieldPath in contract.AssignmentFields)
                {
                    SerializedProperty prop = so.FindProperty(fieldPath);
                    if (prop == null) continue;

                    if (IsPropertyUnassigned(prop))
                    {
                        issues.Add(new PyralisHygieneIssue(
                            asset,
                            fieldPath,
                            $"{prop.displayName} is unassigned in {asset.name} ({asset.GetType().Name}).",
                            contract));
                    }
                }
            }

            // 2. Traverse visible properties to find more assets (deep crawl)
            SerializedObject serializedAsset = new SerializedObject(asset);
            SerializedProperty iterator = serializedAsset.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Object reference = iterator.objectReferenceValue;
                    if (reference != null && reference is ScriptableObject soReference)
                    {
                        // Filter out non-Pyralis assets if needed, but for now we follow all ScriptableObjects
                        ScanRecursive(soReference, visited, issues);
                    }
                }
            }
        }

        private static bool IsPropertyUnassigned(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue == null;
                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(prop.stringValue);
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize == 0;
                default:
                    return false;
            }
        }
    }
}