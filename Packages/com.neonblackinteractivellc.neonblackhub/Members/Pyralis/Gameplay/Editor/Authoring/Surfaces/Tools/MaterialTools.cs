using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates project-standard materials for sprite and mesh rendering in URP.
/// Setup:
/// 1. In Unity, click Tools -> NeonBlack -> Gameplay -> Create Default Materials.
/// 2. Assign Assets/Materials/SpriteLit2D.mat to SpriteRenderers that should react to 2D lights.
/// 3. Assign Assets/Materials/MeshLitCutout.mat to MeshRenderers using alpha-cut textures.
/// </summary>
public static class MaterialTools
{
    private const string MaterialsFolder = "Assets/Materials";

    [MenuItem("Tools/NeonBlack/Gameplay/Create Default Materials")]
    private static void CreateDefaultMaterials()
    {
        EnsureFolder(MaterialsFolder);

        CreateMaterialIfMissing(
            path: MaterialsFolder + "/SpriteLit2D.mat",
            preferredShader: "Universal Render Pipeline/2D/Sprite-Lit-Default",
            fallbackShader: "Sprites/Default");

        Material meshCutout = CreateMaterialIfMissing(
            path: MaterialsFolder + "/MeshLitCutout.mat",
            preferredShader: "Universal Render Pipeline/Lit",
            fallbackShader: "Standard");

        if (meshCutout != null)
        {
            // Enable alpha clipping so transparent pixels cut out cleanly on 3D meshes.
            if (meshCutout.HasProperty("_AlphaClip")) meshCutout.SetFloat("_AlphaClip", 1f);
            if (meshCutout.HasProperty("_Cutoff")) meshCutout.SetFloat("_Cutoff", 0.5f);
            EditorUtility.SetDirty(meshCutout);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[MaterialTools] Materials ready in Assets/Materials");
    }

    private static Material CreateMaterialIfMissing(string path, string preferredShader, string fallbackShader)
    {
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find(preferredShader);
        if (shader == null)
            shader = Shader.Find(fallbackShader);

        if (shader == null)
        {
            Debug.LogError("[MaterialTools] Could not find shader: " + preferredShader + " or fallback: " + fallbackShader);
            return null;
        }

        Material material = new Material(shader);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string child = Path.GetFileName(folderPath);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(child))
            AssetDatabase.CreateFolder(parent, child);
    }
}
