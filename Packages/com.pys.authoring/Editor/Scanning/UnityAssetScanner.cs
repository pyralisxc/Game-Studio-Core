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
            AddAssets(observations, request, "t:AnimationClip", "AnimationClip");
            AddAssets(observations, request, "t:AnimatorController", "AnimatorController");
            AddAssets(observations, request, "t:AudioClip", "AudioClip");
            AddAssets(observations, request, "t:AudioMixer", "AudioMixer");
            AddAssets(observations, request, "t:Material", "Material");
            AddAssets(observations, request, "t:TimelineAsset", "TimelineAsset");
            AddAssets(observations, request, "t:InputActionAsset", "InputActionAsset");
            AddAssets(observations, request, "t:VisualEffectAsset", "VisualEffectAsset");
            RemoveDuplicateAssets(observations);
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

        private static void RemoveDuplicateAssets(List<UnityAssetObservation> observations)
        {
            HashSet<string> seen = new HashSet<string>();
            for (int i = observations.Count - 1; i >= 0; i--)
            {
                UnityAssetObservation observation = observations[i];
                if (observation == null || seen.Contains(observation.ObjectId))
                {
                    observations.RemoveAt(i);
                    continue;
                }

                seen.Add(observation.ObjectId);
            }
        }
    }
}
