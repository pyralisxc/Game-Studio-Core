using System;
using System.IO;
using UnityEditor;

namespace Pys.Authoring.Editor.Scanning
{
    internal static class UnityScanPathUtility
    {
        public static string NormalizeRoot(UnityCodebaseScanRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ScriptsRoot))
                return "Assets";

            return request.ScriptsRoot.Replace('\\', '/').TrimEnd('/');
        }

        public static string ToAssetPath(string absolutePath)
        {
            string projectRoot = Path.GetFullPath(".");
            string fullPath = Path.GetFullPath(absolutePath);
            if (fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                return fullPath.Substring(projectRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');

            return absolutePath.Replace('\\', '/');
        }

        public static string[] FindAssetsInRoot(string filter, UnityCodebaseScanRequest request)
        {
            string root = NormalizeRoot(request);
            if (!AssetDatabase.IsValidFolder(root))
                return new string[0];

            return AssetDatabase.FindAssets(filter, new[] { root });
        }
    }
}
