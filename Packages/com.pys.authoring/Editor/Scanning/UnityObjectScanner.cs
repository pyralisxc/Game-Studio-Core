using System.Collections.Generic;
using Pys.Authoring.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pys.Authoring.Editor.Scanning
{
    internal static class UnityObjectScanner
    {
        public static IReadOnlyList<UnityObjectObservation> ScanActiveSceneObjects()
        {
            List<UnityObjectObservation> observations = new List<UnityObjectObservation>();
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                    AddSceneObjectTree(scene.path, roots[rootIndex], observations);
            }

            return observations;
        }

        public static IReadOnlyList<UnityObjectObservation> ScanPrefabs(UnityCodebaseScanRequest request)
        {
            List<UnityObjectObservation> observations = new List<UnityObjectObservation>();
            string[] prefabGuids = UnityScanPathUtility.FindAssetsInRoot("t:Prefab", request);
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                UnityObjectObservation observation = new UnityObjectObservation("prefab:" + path, prefab.name, path, "Prefab");
                AddComponentEvidence(prefab, observation);
                observations.Add(observation);
            }

            return observations;
        }

        private static void AddSceneObjectTree(string scenePath, GameObject gameObject, List<UnityObjectObservation> observations)
        {
            if (gameObject == null)
                return;

            UnityObjectObservation observation = new UnityObjectObservation(
                "scene:" + scenePath + ":" + GetHierarchyPath(gameObject.transform),
                gameObject.name,
                scenePath,
                "GameObject");

            AddComponentEvidence(gameObject, observation);
            observations.Add(observation);

            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
                AddSceneObjectTree(scenePath, transform.GetChild(i).gameObject, observations);
        }

        private static void AddComponentEvidence(GameObject gameObject, UnityObjectObservation observation)
        {
            Component[] components = gameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    observation.Issues.Add(new AuthoringIssue(
                        "Unity.MissingScript",
                        "Missing script component.",
                        AuthoringIssueSeverity.Required,
                        targetLabel: gameObject.name,
                        actionKind: AuthoringActionKind.ResolveMissingScript));
                    continue;
                }

                observation.Components.Add(component.GetType().FullName);

                if (component is IAuthoringValidationProvider validationProvider)
                    AddValidationIssues(validationProvider, observation);
            }
        }

        private static void AddValidationIssues(IAuthoringValidationProvider validationProvider, UnityObjectObservation observation)
        {
            IEnumerable<AuthoringIssue> issues = validationProvider.GetAuthoringIssues();
            if (issues == null)
                return;

            foreach (AuthoringIssue issue in issues)
            {
                if (issue != null)
                    observation.Issues.Add(issue);
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            List<string> parts = new List<string>();
            Transform current = transform;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
