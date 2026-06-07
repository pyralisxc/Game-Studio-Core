using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Validation
{
    [InitializeOnLoad]
    public static class CodexUnityValidationRefreshBridge
    {
        private const string RequestRelativePath = "Temp/CodexUnityValidation/refresh-request.json";
        private const string DefaultPackagePath = "Packages/com.neonblackinteractivellc.neonblackhub";
        private const double PollIntervalSeconds = 0.5d;

        private static double _nextPollTime;

        static CodexUnityValidationRefreshBridge()
        {
            EditorApplication.update -= PollForRequest;
            EditorApplication.update += PollForRequest;
        }

        private static void PollForRequest()
        {
            if (EditorApplication.timeSinceStartup < _nextPollTime)
            {
                return;
            }

            _nextPollTime = EditorApplication.timeSinceStartup + PollIntervalSeconds;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }

            var requestPath = Path.Combine(Directory.GetCurrentDirectory(), RequestRelativePath);
            if (!File.Exists(requestPath))
            {
                return;
            }

            RefreshRequest request;
            try
            {
                request = JsonUtility.FromJson<RefreshRequest>(File.ReadAllText(requestPath)) ?? new RefreshRequest();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CodexUnityValidation] Failed to read refresh request: {exception.Message}");
                TryDeleteRequest(requestPath);
                return;
            }

            TryDeleteRequest(requestPath);
            ExecuteRefresh(request);
        }

        private static void ExecuteRefresh(RefreshRequest request)
        {
            var requestId = string.IsNullOrWhiteSpace(request.requestId) ? Guid.NewGuid().ToString("N") : request.requestId;
            var imported = new List<string>();

            try
            {
                var paths = request.paths != null && request.paths.Length > 0
                    ? request.paths
                    : new[] { DefaultPackagePath };

                foreach (var path in paths)
                {
                    var assetPath = ToAssetPath(path);
                    if (string.IsNullOrWhiteSpace(assetPath))
                    {
                        continue;
                    }

                    var options = ImportAssetOptions.ForceUpdate;
                    if (request.recursive || Directory.Exists(ToAbsolutePath(assetPath)))
                    {
                        options |= ImportAssetOptions.ImportRecursive;
                    }

                    AssetDatabase.ImportAsset(assetPath, options);
                    imported.Add(assetPath);
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Debug.Log($"[CodexUnityValidation] Refresh complete requestId={requestId} imported={string.Join(",", imported)} utc={DateTime.UtcNow:O}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CodexUnityValidation] Refresh failed requestId={requestId}: {exception}");
            }
        }

        private static string ToAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var normalized = path.Replace('\\', '/');
            if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            var projectRoot = Directory.GetCurrentDirectory().Replace('\\', '/').TrimEnd('/') + "/";
            if (Path.IsPathRooted(path))
            {
                var absolute = Path.GetFullPath(path).Replace('\\', '/');
                if (absolute.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return absolute.Substring(projectRoot.Length);
                }
            }

            return normalized.TrimStart('/');
        }

        private static string ToAbsolutePath(string assetPath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static void TryDeleteRequest(string requestPath)
        {
            try
            {
                File.Delete(requestPath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[CodexUnityValidation] Could not delete refresh request: {exception.Message}");
            }
        }

        [Serializable]
        private sealed class RefreshRequest
        {
            public string requestId = string.Empty;
            public string[] paths = Array.Empty<string>();
            public bool recursive = true;
        }
    }
}
