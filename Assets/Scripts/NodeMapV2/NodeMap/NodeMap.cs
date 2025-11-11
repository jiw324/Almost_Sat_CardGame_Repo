using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMap : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int mapWidth = 5;
    [SerializeField] private int mapHeight = 15;
    [SerializeField] private float gridXSpacing = 1.5f;
    [SerializeField] private float gridYSpacing = 1.5f;

    [Header("Connectivity Settings")]
    [Tooltip("Probability (0–1) that a node will branch into multiple next-floor nodes.")]
    [Range(0f, 1f)][SerializeField] private float branchChance = 0.65f;
    [Tooltip("Probability that a node connects straight up instead of diagonally.")]
    [Range(0f, 1f)][SerializeField] private float straightBias = 0.2f;

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public NodeGrid Grid { get; private set; }
    public NodeFactory Factory { get; private set; }
    public Dictionary<int, List<Node>> Floors { get; private set; } = new();
    public IEnumerable<Node> AllNodes => Floors.Values.SelectMany(list => list);
    private bool _generated = false;

    public void Generate()
    {
        if (_generated)
        {
            Debug.LogWarning("NodeMap.Generate() called multiple times. Ignoring.");
            return;
        }

        InitializeGrid();
        GenerateStructure();

        _generated = true;
        Debug.Log($"Generated NodeMap structure: {mapWidth}x{mapHeight} with {AllNodes.Count()} nodes.");
    }

    private void InitializeGrid()
    {
        Grid = new NodeGrid(mapWidth, mapHeight, gridXSpacing, gridYSpacing, Vector3.zero);
        Factory = new NodeFactory();
        Floors.Clear();

        for (int y = 0; y < mapHeight; y++)
            Floors[y] = new List<Node>();
    }

    private void GenerateStructure()
    {
        // 1: Create starting floor
        int numStartNodes = Mathf.Max(2, Mathf.RoundToInt(mapWidth * 0.6f));
        var startCols = GetRandomUniqueColumns(numStartNodes);
        foreach (int x in startCols)
            Floors[0].Add(CreateNode(x, 0));

        // 2: Expand map upwards, one floor at a time
        for (int y = 0; y < mapHeight - 1; y++)
            ConnectFloors(y, y + 1);

        // 3: Ensure at least one node on final floor
        if (Floors[mapHeight - 1].Count == 0)
        {
            int x = mapWidth / 2;
            Floors[mapHeight - 1].Add(CreateNode(x, mapHeight - 1));
        }
    }

    private void ConnectFloors(int fromY, int toY)
    {
        var currentFloor = Floors[fromY];
        var nextFloor = Floors[toY];
        HashSet<(int, int)> usedConnections = new();

        foreach (Node parent in currentFloor)
        {
            List<int> candidateXs = new();

            // 1: Add straight connections based on straightBias
            if (Random.value < straightBias)
                candidateXs.Add(parent.GridPos.x);

            // 2: Add diagonal paths based on branchChance
            if (Random.value < branchChance)
            {
                foreach (int dx in new int[] { -1, 1 })
                {
                    int nx = parent.GridPos.x + dx;
                    if (Grid.IsValidCoord(nx, toY))
                        candidateXs.Add(nx);
                }
            }

            // 3: Always have at least one candidate to fallback on if all checks fail
            if (candidateXs.Count == 0)
                candidateXs.Add(parent.GridPos.x);

            bool connected = false;

            // 4: Try connecting to each candidate, reject cross paths
            foreach (int nx in candidateXs.Distinct())
            {
                if (!Grid.IsValidCoord(nx, toY))
                    continue;

                if (WouldCross(parent.GridPos.x, nx, fromY, usedConnections))
                    continue;

                Node child = GetOrCreateNode(nx, toY);
                parent.ConnectTo(child);
                usedConnections.Add((parent.GridPos.x, nx));

                if (!nextFloor.Contains(child))
                    nextFloor.Add(child);

                connected = true;
            }

            // 5: If every candidate was rejected due to crossing, default to safe straight path
            if (!connected)
            {
                int nx = parent.GridPos.x;
                if (Grid.IsValidCoord(nx, toY))
                {
                    Node child = GetOrCreateNode(nx, toY);
                    parent.ConnectTo(child);
                    usedConnections.Add((parent.GridPos.x, nx));

                    if (!nextFloor.Contains(child))
                        nextFloor.Add(child);
                }
            }
        }

        // 6: Guarantee no empty floors
        if (nextFloor.Count == 0)
        {
            Node parent = currentFloor[Random.Range(0, currentFloor.Count)];
            int x = Mathf.Clamp(parent.GridPos.x, 0, mapWidth - 1);
            Node forced = GetOrCreateNode(x, toY);
            parent.ConnectTo(forced);
            usedConnections.Add((parent.GridPos.x, x));
            nextFloor.Add(forced);
        }

        // 7: Ensure every next floor node has at least one incoming path from previous floor
        foreach (Node child in nextFloor)
        {
            bool hasIncoming = currentFloor.Any(p => p.NextNodes.Contains(child));
            if (!hasIncoming)
            {
                Node nearestParent = currentFloor
                    .OrderBy(p => Mathf.Abs(p.GridPos.x - child.GridPos.x))
                    .FirstOrDefault();

                if (nearestParent != null && !nearestParent.NextNodes.Contains(child))
                {
                    nearestParent.ConnectTo(child);
                    usedConnections.Add((nearestParent.GridPos.x, child.GridPos.x));
                }
            }
        }
    }

    /// <summary>
    /// Prevents two edges (a -> b) and (c -> d) on the same level from crossing visually.
    /// </summary>
    private bool WouldCross(int startX, int endX, int y, HashSet<(int, int)> existing)
    {
        if (startX == endX)
            return false;

        foreach (var (a, b) in existing)
        {
            if ((a < startX && b > endX) || (a > startX && b < endX))
                return true;
        }
        return false;
    }

    private Node GetOrCreateNode(int x, int y)
    {
        var existing = Floors[y].FirstOrDefault(n => n.GridPos.x == x);
        if (existing != null) return existing;

        Node node = CreateNode(x, y);
        Floors[y].Add(node);
        return node;
    }

    private Node CreateNode(int x, int y)
    {
        Node node = new Node(null, new Vector2Int(x, y));
        return node;
    }

    private List<int> GetRandomUniqueColumns(int count)
    {
        List<int> allCols = Enumerable.Range(0, mapWidth).ToList();
        List<int> chosen = new();

        while (chosen.Count < count && allCols.Count > 0)
        {
            int index = Random.Range(0, allCols.Count);
            chosen.Add(allCols[index]);
            allCols.RemoveAt(index);
        }
        return chosen;
    }
}
