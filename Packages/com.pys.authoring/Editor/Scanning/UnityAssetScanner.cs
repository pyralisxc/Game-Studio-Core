using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Scanning
{
    internal static class UnityAssetScanner
    {
        public static IReadOnlyList<UnityAssetObservation> ScanAssets(UnityCodebaseScanRequest request)
        {
            List<UnityAssetObservation> observations = new List<UnityAssetObservation>();
            AddAssets(observations, request, "t:ScriptableObject", "ScriptableObject");
            AddAssets(observations, request, "t:Scene", "Scene");
            return observations;
        }

        private static void AddAssets(List<UnityAssetObservation> observations, UnityCodebaseScanRequest request, string filter, string typeName)
        {
            string[] guids = UnityScanPathUtility.FindAssetsInRoot(filter, request);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null)
                    continue;

                observations.Add(new UnityAssetObservation(
                    "asset:" + path,
                    asset.name,
                    path,
                    typeName));
            }
        }
    }
}
