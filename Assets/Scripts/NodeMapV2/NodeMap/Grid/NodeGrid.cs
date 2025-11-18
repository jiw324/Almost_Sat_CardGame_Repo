using System.Collections.Generic;
using UnityEngine;

public class NodeGrid
{
    public int Width { get; }
    public int Height { get; }

    public float XSpacing { get; }
    public float YSpacing { get; }

    public Vector3 Origin { get; } // world-space anchor point for (0,0)

    // Per-cell visual offsets (x,z plane)
    private readonly Dictionary<Vector2Int, Vector2> _offsets = new Dictionary<Vector2Int, Vector2>();

    public NodeGrid(int width, int height, float xSpacing = 1.5f, float ySpacing = 1.5f, Vector3? origin = null)
    {
        Width = width;
        Height = height;
        XSpacing = xSpacing;
        YSpacing = ySpacing;
        Origin = origin ?? Vector3.zero;
    }

    /// <summary>
    /// Assigns a visual offset for a specific grid coordinate (x,z in world).
    /// </summary>
    public void SetOffset(int x, int y, Vector2 offset)
    {
        var key = new Vector2Int(x, y);
        _offsets[key] = offset;
    }

    /// <summary>
    /// Gets the visual offset for a specific grid coordinate (x,z in world).
    /// Returns Vector2.zero if not set.
    /// </summary>
    public Vector2 GetOffset(int x, int y)
    {
        var key = new Vector2Int(x, y);
        if (_offsets.TryGetValue(key, out var offset))
            return offset;

        return Vector2.zero;
    }

    /// <summary>
    /// Converts grid coordinates (x, y) into a world-space position, including visual offset.
    /// </summary>
    public Vector3 GridToWorld(int x, int y)
    {
        Vector2 offset = GetOffset(x, y);

        float worldX = Origin.x + (x * XSpacing) + offset.x;
        float worldZ = Origin.z + (y * YSpacing) + offset.y;
        return new Vector3(worldX, Origin.y, worldZ);
    }

    /// <summary>
    /// Converts a world-space position back to nearest grid coordinates.
    /// Note: ignores offsets and assumes ideal grid spacing.
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int gridX = Mathf.RoundToInt((worldPos.x - Origin.x) / XSpacing);
        int gridY = Mathf.RoundToInt((worldPos.z - Origin.z) / YSpacing);
        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// Returns true if a coordinate is within the bounds of the grid.
    /// </summary>
    public bool IsValidCoord(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Offsets for potential next-row neighbors (forward paths).
    /// </summary>
    public static readonly Vector2Int[] ForwardNeighborOffsets =
    {
        new Vector2Int(-1, 1), // up-left
        new Vector2Int(0, 1),  // straight up
        new Vector2Int(1, 1)   // up-right
    };

    /// <summary>
    /// Offsets for potential previous-row neighbors (incoming paths).
    /// </summary>
    public static readonly Vector2Int[] BackwardNeighborOffsets =
    {
        new Vector2Int(-1, -1), // down-left
        new Vector2Int(0, -1),  // straight down
        new Vector2Int(1, -1)   // down-right
    };

    /// <summary>
    /// Gets valid forward neighbor coordinates for a given grid position.
    /// </summary>
    public List<Vector2Int> GetForwardNeighbors(Vector2Int gridPos)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (var offset in ForwardNeighborOffsets)
        {
            Vector2Int neighbor = gridPos + offset;
            if (IsValidCoord(neighbor.x, neighbor.y))
                result.Add(neighbor);
        }
        return result;
    }

    /// <summary>
    /// Gets valid backward neighbor coordinates for a given grid position.
    /// </summary>
    public List<Vector2Int> GetBackwardNeighbors(Vector2Int gridPos)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (var offset in BackwardNeighborOffsets)
        {
            Vector2Int neighbor = gridPos + offset;
            if (IsValidCoord(neighbor.x, neighbor.y))
                result.Add(neighbor);
        }
        return result;
    }
}
