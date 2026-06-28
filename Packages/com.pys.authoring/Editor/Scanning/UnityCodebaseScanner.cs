namespace Pys.Authoring.Editor.Scanning
{
    public static class UnityCodebaseScanner
    {
        public static UnityCodebaseScanResult Scan(UnityCodebaseScanRequest request)
        {
            UnityCodebaseScanResult result = new UnityCodebaseScanResult
            {
                ScriptsRoot = UnityScanPathUtility.NormalizeRoot(request)
            };

            result.Types.AddRange(UnityTypeScanner.Scan(request));
            result.AssemblyDefinitions.AddRange(SourceDependencyScanner.ScanAssemblyDefinitions(request));
            result.SourceDependencies.AddRange(SourceDependencyScanner.ScanSourceDependencies(request));
            result.SceneObjects.AddRange(UnityObjectScanner.ScanActiveSceneObjects());
            result.Prefabs.AddRange(UnityObjectScanner.ScanPrefabs(request));
            result.Assets.AddRange(UnityAssetScanner.ScanAssets(request));
            return result;
        }
    }
}
