using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 64;
    public int height = 64;
    public float cellSize = 1f;
    public Vector2 originWorld = Vector2.zero;

    private GridCell[,] _cells;
    private bool[,] _blocked;

    public void Init()
    {
        _cells = new GridCell[width, height];
        _blocked = new bool[width, height];

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            _cells[x, y] = new GridCell(new Vector2Int(x, y));
    }

    public Vector2Int WorldToGrid(Vector3 world)
    {
        var local = (Vector2)world - originWorld;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorldCenter(Vector2Int cell)
    {
        float x = originWorld.x + (cell.x + 0.5f) * cellSize;
        float y = originWorld.y + (cell.y + 0.5f) * cellSize;
        return new Vector3(x, y, 0f);
    }

    public bool InBounds(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;

    public bool IsBlocked(Vector2Int cell) => !InBounds(cell) || _blocked[cell.x, cell.y];

    public void SetBlocked(Vector2Int cell, bool blocked)
    {
        if (!InBounds(cell)) return;
        _blocked[cell.x, cell.y] = blocked;
    }

    public bool CanPlace(Vector2Int origin, Vector2Int footprint, int requiredGroundId = 0)
    {
        for (int dx = 0; dx < footprint.x; dx++)
        for (int dy = 0; dy < footprint.y; dy++)
        {
            var c = new Vector2Int(origin.x + dx, origin.y + dy);
            if (!InBounds(c)) return false;
            if (IsBlocked(c)) return false;
            if (!_cells[c.x, c.y].IsFree) return false;

            if (requiredGroundId != 0)
            {
                if (_cells[c.x, c.y].GroundId != requiredGroundId)
                    return false;
            }
        }
        return true;
    }

    public void Place(BuildingRuntime b, Vector2Int origin, Vector2Int footprint)
    {
        for (int dx = 0; dx < footprint.x; dx++)
        for (int dy = 0; dy < footprint.y; dy++)
        {
            var c = new Vector2Int(origin.x + dx, origin.y + dy);
            _cells[c.x, c.y].Occupant = b;
        }
    }

    public void Remove(BuildingRuntime b, Vector2Int origin, Vector2Int footprint)
    {
        for (int dx = 0; dx < footprint.x; dx++)
        for (int dy = 0; dy < footprint.y; dy++)
        {
            var c = new Vector2Int(origin.x + dx, origin.y + dy);
            if (InBounds(c) && _cells[c.x, c.y].Occupant == b)
                _cells[c.x, c.y].Occupant = null;
        }
    }

    public BuildingRuntime GetOccupantAt(Vector2Int cell)
    {
        if (!InBounds(cell)) return null;
        return _cells[cell.x, cell.y].Occupant;
    }
    
    public int GetGroundId(Vector2Int c) => InBounds(c) ? _cells[c.x, c.y].GroundId : -1;
    public void SetGroundId(Vector2Int c, int id) { if (InBounds(c)) _cells[c.x, c.y].GroundId = id; }

    public Vector2Int GetCenterCell() => new Vector2Int(width / 2, height / 2);


}
