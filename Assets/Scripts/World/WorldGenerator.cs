using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Refs")]
    public GridManager grid;
    public Tilemap resourceTilemap;
    public GroundNodeDatabase groundDb;

    [Header("Seed")]
    public int seed = 12345;

    [Header("Borders")]
    public int borderThickness = 2;
    public bool spawnBorderPrefabs = true;
    public GameObject[] borderPrefabs;

    [Header("Rocks")]
    public float rockNoiseScale = 18f;
    [Range(0f, 1f)] public float rockThreshold = 0.62f;
    public GameObject[] rockPrefabs;
    public int rockSpawnSkip = 1; // 1 = check every cell, 2 = every 2nd cell

    [Header("Resource Patches")]
    public int patchesPerResourceBase = 4;
    public int patchMinSize = 25;
    public int patchMaxSize = 120;
    public int patchGrowAttempts = 6;

    // rarer resources = fewer patches, smaller sizes
    public float rarePatchMultiplier = 0.5f;

    private System.Random _rng;

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (grid == null) grid = GameServices.Grid;
        if (grid == null || resourceTilemap == null || groundDb == null) return;

        _rng = new System.Random(seed);

        resourceTilemap.ClearAllTiles();

        ClearGroundMeta();
        ApplyBorders();
        PaintResourcePatches();
        SpawnRocks();
    }

    private void ClearGroundMeta()
    {
        for (int x = 0; x < grid.width; x++)
        for (int y = 0; y < grid.height; y++)
        {
            var c = new Vector2Int(x, y);
            grid.SetBlocked(c, false);
            grid.SetGroundId(c, 0);
        }
    }

    private void ApplyBorders()
    {
        for (int x = 0; x < grid.width; x++)
        for (int y = 0; y < grid.height; y++)
        {
            bool isBorder =
                x < borderThickness ||
                y < borderThickness ||
                x >= grid.width - borderThickness ||
                y >= grid.height - borderThickness;

            if (!isBorder) continue;

            var c = new Vector2Int(x, y);
            grid.SetBlocked(c, true);

            if (spawnBorderPrefabs && borderPrefabs != null && borderPrefabs.Length > 0)
                SpawnPrefabAtCell(borderPrefabs, c);
        }
    }

    private void PaintResourcePatches()
    {
        // Create patches per resource based on rarityWeight
        foreach (var node in groundDb.nodes)
        {
            if (node == null || node.groundId == 0 || node.tile == null) continue;

            // Weight: lower rarityWeight => fewer patches
            float w = Mathf.Max(0.05f, node.rarityWeight);
            int patchCount = Mathf.Max(1, Mathf.RoundToInt(patchesPerResourceBase * w));

            int minSize = Mathf.RoundToInt(patchMinSize * Mathf.Lerp(rarePatchMultiplier, 1f, w));
            int maxSize = Mathf.RoundToInt(patchMaxSize * Mathf.Lerp(rarePatchMultiplier, 1f, w));

            for (int i = 0; i < patchCount; i++)
            {
                int size = _rng.Next(minSize, maxSize + 1);
                TryGrowPatch(node.groundId, node.tile, size);
            }
        }
    }

    private void TryGrowPatch(int groundId, TileBase tile, int targetSize)
    {
        // pick random start not blocked
        for (int attempt = 0; attempt < 40; attempt++)
        {
            var start = new Vector2Int(
                _rng.Next(borderThickness, grid.width - borderThickness),
                _rng.Next(borderThickness, grid.height - borderThickness)
            );

            if (grid.IsBlocked(start)) continue;
            if (grid.GetGroundId(start) != 0) continue;

            // blob growth using frontier
            var frontier = new List<Vector2Int> { start };
            var placed = 0;

            while (frontier.Count > 0 && placed < targetSize)
            {
                // take random frontier cell
                int idx = _rng.Next(0, frontier.Count);
                var c = frontier[idx];
                frontier.RemoveAt(idx);

                if (!grid.InBounds(c)) continue;
                if (grid.IsBlocked(c)) continue;
                if (grid.GetGroundId(c) != 0) continue;

                grid.SetGroundId(c, groundId);
                resourceTilemap.SetTile((Vector3Int)c, tile);
                placed++;

                // expand
                for (int g = 0; g < patchGrowAttempts; g++)
                {
                    var n = c + RandomCardinal();
                    if (grid.InBounds(n) && !grid.IsBlocked(n) && grid.GetGroundId(n) == 0)
                        frontier.Add(n);
                }
            }

            // success enough => stop
            if (placed >= Mathf.Max(8, targetSize / 3))
                return;
        }
    }

    private Vector2Int RandomCardinal()
    {
        int r = _rng.Next(0, 4);
        return r switch
        {
            0 => Vector2Int.right,
            1 => Vector2Int.left,
            2 => Vector2Int.up,
            _ => Vector2Int.down
        };
    }

    private void SpawnRocks()
    {
        if (rockPrefabs == null || rockPrefabs.Length == 0) return;

        for (int x = borderThickness; x < grid.width - borderThickness; x += rockSpawnSkip)
        for (int y = borderThickness; y < grid.height - borderThickness; y += rockSpawnSkip)
        {
            var c = new Vector2Int(x, y);

            if (grid.IsBlocked(c)) continue;
            if (grid.GetGroundId(c) != 0) continue; // never overwrite resource nodes

            float nx = (x + seed * 0.01f) / rockNoiseScale;
            float ny = (y + seed * 0.02f) / rockNoiseScale;
            float n = Mathf.PerlinNoise(nx, ny);

            if (n < rockThreshold) continue;

            grid.SetBlocked(c, true);
            SpawnPrefabAtCell(rockPrefabs, c);
        }
    }

    private void SpawnPrefabAtCell(GameObject[] prefabs, Vector2Int cell)
    {
        if (prefabs == null || prefabs.Length == 0) return;
        var prefab = prefabs[_rng.Next(0, prefabs.Length)];
        if (prefab == null) return;

        var worldPos = grid.GridToWorldCenter(cell);
        Instantiate(prefab, worldPos, Quaternion.identity);
    }
}
