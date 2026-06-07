using UnityEngine;
using UnityEngine.Tilemaps;

namespace NeonBlack.Gameplay.Features.Environment
{
/// <summary>
/// QUICK SETUP: Tilemap ground layer for 2.5D environment.
/// 
/// HOW TO USE:
/// 1. Create empty GameObject called "Ground"
/// 2. Add this script to it
/// 3. Drag your Tilemap into the "Source Tilemap" field
/// 4. Optionally create a child GameObject called "Visual" with:
///    - SpriteRenderer showing your ground sprite (for visual reference)
///    - Set it as the visual layer
/// 5. Hit Play or manually call BakeTilemapToGround()
/// 6. Paint tiles in your tilemap normally - they render on the 3D ground
/// </summary>
public class TilemapGround : MonoBehaviour
{
    [Header("Tilemap Setup")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Material groundMaterial;
    [SerializeField] private bool useCollider = true;

    [Header("Layer")]
    [Tooltip("Physics layer name assigned to the baked ground mesh.")]
    [SerializeField] private string _groundLayerName = "Ground";

    private const float RenderHeight = 0.01f; // Slightly above Y=0 to avoid z-fighting
    private Mesh groundMesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && groundTilemap != null)
            BakeTilemapToGround();
#endif
    }

    /// <summary>
    /// Bakes the tilemap to a 3D ground plane on XZ axis.
    /// Call this after painting tiles, or it auto-updates in editor.
    /// </summary>
    public void BakeTilemapToGround()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("GroundTilemap not assigned!");
            return;
        }

        // Get bounds
        BoundsInt bounds = groundTilemap.cellBounds;
        
        // Convert cell bounds to world bounds
        float worldWidth = bounds.size.x;
        float worldHeight = bounds.size.y; // Tilemap rows are in Y; we project them onto world Z

        // Create or update mesh
        if (groundMesh == null)
            groundMesh = new Mesh();

        CreateGroundMesh(worldWidth, worldHeight, bounds);

        // Ensure components exist
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        
        meshFilter.mesh = groundMesh;

        var meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Apply material
        if (groundMaterial == null)
            groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        meshRenderer.material = groundMaterial;

        // Add collision
        if (useCollider)
        {
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
            
            meshCollider.convex = false;
        }

        // Position ground at XZ origin
        gameObject.transform.position = new Vector3(
            bounds.xMin - worldWidth / 2f,
            RenderHeight,
            bounds.yMin - worldHeight / 2f
        );

        gameObject.layer = LayerMask.NameToLayer(_groundLayerName);

        Debug.Log($"[TilemapGround] Baked: {worldWidth} x {worldHeight} at Y={RenderHeight}");
    }

    private void CreateGroundMesh(float width, float height, BoundsInt bounds)
    {
        // Create 4 corner vertices for a flat plane on XZ
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),                    // Bottom-left (X-, Z-)
            new Vector3(width, 0, 0),                // Bottom-right (X+, Z-)
            new Vector3(width, 0, height),           // Top-right (X+, Z+)
            new Vector3(0, 0, height)                // Top-left (X-, Z+)
        };

        // UV mapping: map entire tilemap to the quad
        Vector2[] uv = new Vector2[4]
        {
            Vector2.zero,          // 0,0
            Vector2.right,         // 1,0
            Vector2.one,           // 1,1
            Vector2.up             // 0,1
        };

        // Two triangles forming a quad
        int[] triangles = new int[6]
        {
            0, 2, 1,  // First triangle (CCW from above)
            0, 3, 2   // Second triangle (CCW from above)
        };

        groundMesh.Clear();
        groundMesh.vertices = vertices;
        groundMesh.uv = uv;
        groundMesh.triangles = triangles;
        groundMesh.RecalculateNormals();
        groundMesh.RecalculateBounds();
    }

#if UNITY_EDITOR
    // Auto-update in editor when you paint tiles
    private void OnValidate()
    {
        if (Application.isEditor && !Application.isPlaying && groundTilemap != null)
        {
            BakeTilemapToGround();
        }
    }
#endif
}
}
