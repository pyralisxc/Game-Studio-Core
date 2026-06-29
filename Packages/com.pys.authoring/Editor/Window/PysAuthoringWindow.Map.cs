using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Projections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow
    {
        private void DrawMap()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Map", EditorStyles.boldLabel);
            if (lastMap == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            DrawMapFilters();
            MapProjection renderedMap = RenderedMapProjection();
            DrawCompactRow("Rows", renderedMap.Rows.Count.ToString());

            string currentKind = string.Empty;
            for (int i = 0; i < renderedMap.Rows.Count; i++)
            {
                MapRow row = renderedMap.Rows[i];
                if (row.Kind != currentKind)
                {
                    currentKind = row.Kind;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(currentKind) ? "Unknown" : currentKind, EditorStyles.boldLabel);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(row.Label, EditorStyles.boldLabel);
                DrawInlineCounts(
                    "Kind: " + row.Kind,
                    "Components: " + row.ComponentCount,
                    "Issues: " + row.IssueCount);

                DrawWrappedRow("Source", row.SourcePath);
                DrawMapNavigation(row);
            }

            if (GUILayout.Button("Export Map JSON"))
                ProjectionJsonExporter.ExportMap(renderedMap, scriptsRoot);
        }

        private void DrawMapFilters()
        {
            if (EditorGUIUtility.currentViewWidth < 520f)
            {
                mapShowSceneObjects = EditorGUILayout.ToggleLeft("Scene", mapShowSceneObjects);
                mapShowPrefabs = EditorGUILayout.ToggleLeft("Prefabs", mapShowPrefabs);
                mapShowAssets = EditorGUILayout.ToggleLeft("Assets", mapShowAssets);
                mapShowIssuesOnly = EditorGUILayout.ToggleLeft("Issues Only", mapShowIssuesOnly);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                mapShowSceneObjects = EditorGUILayout.ToggleLeft("Scene", mapShowSceneObjects, GUILayout.MinWidth(80));
                mapShowPrefabs = EditorGUILayout.ToggleLeft("Prefabs", mapShowPrefabs, GUILayout.MinWidth(90));
                mapShowAssets = EditorGUILayout.ToggleLeft("Assets", mapShowAssets, GUILayout.MinWidth(80));
                mapShowIssuesOnly = EditorGUILayout.ToggleLeft("Issues Only", mapShowIssuesOnly, GUILayout.MinWidth(100));
            }
        }

        private static void DrawMapNavigation(MapRow row)
        {
            if (row == null || (!row.CanSelect && !row.CanPing))
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (row.CanSelect && GUILayout.Button(string.IsNullOrWhiteSpace(row.NavigationLabel) ? "Select in Hierarchy" : row.NavigationLabel, GUILayout.Width(150)))
                    SelectSceneObject(row);

                if (row.CanPing && GUILayout.Button(string.IsNullOrWhiteSpace(row.NavigationLabel) ? "Ping Asset" : row.NavigationLabel, GUILayout.Width(110)))
                    PingAsset(row);
            }
        }

        private static void PingAsset(MapRow row)
        {
            if (row == null || string.IsNullOrWhiteSpace(row.SourcePath))
                return;

            Object asset = AssetDatabase.LoadMainAssetAtPath(row.SourcePath);
            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static void SelectSceneObject(MapRow row)
        {
            GameObject sceneObject = FindSceneObject(row != null ? row.Id : string.Empty);
            if (sceneObject == null)
                return;

            Selection.activeObject = sceneObject;
            EditorGUIUtility.PingObject(sceneObject);
        }

        private static GameObject FindSceneObject(string mapId)
        {
            const string Prefix = "scene:";
            if (string.IsNullOrWhiteSpace(mapId) || !mapId.StartsWith(Prefix, System.StringComparison.Ordinal))
                return null;

            string payload = mapId.Substring(Prefix.Length);
            int splitIndex = payload.LastIndexOf(':');
            if (splitIndex < 0)
                return null;

            string scenePath = payload.Substring(0, splitIndex);
            string hierarchyPath = payload.Substring(splitIndex + 1);
            if (string.IsNullOrWhiteSpace(hierarchyPath))
                return null;

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                if (!string.IsNullOrWhiteSpace(scenePath) && scene.path != scenePath)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    GameObject found = FindSceneObjectByHierarchyPath(roots[rootIndex].transform, hierarchyPath);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        private static GameObject FindSceneObjectByHierarchyPath(Transform transform, string hierarchyPath)
        {
            if (transform == null)
                return null;

            if (HierarchyPath(transform) == hierarchyPath)
                return transform.gameObject;

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject found = FindSceneObjectByHierarchyPath(transform.GetChild(i), hierarchyPath);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static string HierarchyPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private MapProjection RenderedMapProjection()
        {
            if (lastMap == null)
                return null;

            MapProjection projection = new MapProjection();
            for (int i = 0; i < lastMap.Rows.Count; i++)
            {
                MapRow row = lastMap.Rows[i];
                if (mapShowIssuesOnly && row.IssueCount == 0)
                    continue;

                if (row.Kind == AuthoringGraphNodeKind.SceneObject.ToString() && !mapShowSceneObjects)
                    continue;
                if (row.Kind == AuthoringGraphNodeKind.Prefab.ToString() && !mapShowPrefabs)
                    continue;
                if (row.Kind == AuthoringGraphNodeKind.Asset.ToString() && !mapShowAssets)
                    continue;

                projection.Rows.Add(row);
            }

            return projection;
        }
    }
}
