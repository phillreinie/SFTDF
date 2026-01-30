using UnityEngine;

public class GridCell
{
    public Vector2Int Pos { get; }
    public BuildingRuntime Occupant { get; set; }

    public GridCell(Vector2Int pos) => Pos = pos;
    public bool IsFree => Occupant == null;
    
    public int GroundId = 0;
}