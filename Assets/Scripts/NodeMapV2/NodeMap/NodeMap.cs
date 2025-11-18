using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMap : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int mapWidth = 5;
    [SerializeField] private int mapHeight = 15;
    [SerializeField] private float gridXSpacing = .75f;
    [SerializeField] private float gridYSpacing = .75f;

    [Header("Connectivity Settings")]
    [Range(0f, 1f)][SerializeField] private float branchChance = 0.9f;
    [Range(0f, 1f)][SerializeField] private float straightBias = 0.2f;

    [Header("Offset Settings")]
    [SerializeField] private float maxXOffset = 0.1f;
    [SerializeField] private float maxYOffset = 0.2f;
    [Range(0.01f, 1f)][SerializeField] private float offsetStdDevFactor = 0.5f;

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;

    public Node BossNode { get; private set; }

    public NodeGrid Grid { get; private set; }
    public NodeFactory Factory { get; private set; }
    public Dictionary<int, List<Node>> Floors { get; private set; } = new Dictionary<int, List<Node>>();
    public IEnumerable<Node> AllNodes => Floors.Values.SelectMany(list => list);
    private bool _generated = false;

    public void Generate()
    {
        if (_generated)
        {
            Debug.LogWarning("Failed to generate map. Map already generated");
            return;
        }

        InitializeGrid();
        GenerateStructure();
        CreateBossNode();
        ApplyGaussianOffsets();

        _generated = true;
        Debug.Log($"Generated NodeMap structure: {mapWidth}x{mapHeight} with {AllNodes.Count()} nodes (+ BossNode).");
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
        int numStartNodes = Mathf.Max(2, Mathf.RoundToInt(mapWidth * 0.6f));
        var startCols = GetRandomUniqueColumns(numStartNodes);
        foreach (int x in startCols)
            Floors[0].Add(CreateNode(x, 0));

        for (int y = 0; y < mapHeight - 1; y++)
            ConnectFloors(y, y + 1);

        if (Floors[mapHeight - 1].Count == 0)
        {
            int x = mapWidth / 2;
            Floors[mapHeight - 1].Add(CreateNode(x, mapHeight - 1));
        }
    }

    private void CreateBossNode()
    {
        int restFloor = mapHeight - 1;
        int bossY = mapHeight; // virtual floor

        int bossX = mapWidth / 2;
        BossNode = new Node(null, new Vector2Int(bossX, bossY));

        foreach (Node n in Floors[restFloor])
            n.ConnectTo(BossNode);
    }

    private void ConnectFloors(int fromY, int toY)
    {
        var currentFloor = Floors[fromY];
        var nextFloor = Floors[toY];
        HashSet<(int, int)> usedConnections = new HashSet<(int, int)>();

        foreach (Node parent in currentFloor)
        {
            List<int> candidateXs = new List<int>();

            if (Random.value < straightBias)
                candidateXs.Add(parent.GridPos.x);

            if (Random.value < branchChance)
            {
                foreach (int dx in new int[] { -1, 1 })
                {
                    int nx = parent.GridPos.x + dx;
                    if (Grid.IsValidCoord(nx, toY))
                        candidateXs.Add(nx);
                }
            }

            if (candidateXs.Count == 0)
                candidateXs.Add(parent.GridPos.x);

            bool connected = false;

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

        if (nextFloor.Count == 0)
        {
            Node parent = currentFloor[Random.Range(0, currentFloor.Count)];
            int x = Mathf.Clamp(parent.GridPos.x, 0, mapWidth - 1);
            Node forced = GetOrCreateNode(x, toY);
            parent.ConnectTo(forced);
            usedConnections.Add((parent.GridPos.x, x));
            nextFloor.Add(forced);
        }

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

    private bool WouldCross(int startX, int endX, int y, HashSet<(int, int)> existing)
    {
        if (startX == endX)
            return false;

        foreach (var pair in existing)
        {
            int a = pair.Item1;
            int b = pair.Item2;

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
        return new Node(null, new Vector2Int(x, y));
    }

    private List<int> GetRandomUniqueColumns(int count)
    {
        List<int> allCols = Enumerable.Range(0, mapWidth).ToList();
        List<int> chosen = new List<int>();

        while (chosen.Count < count && allCols.Count > 0)
        {
            int index = Random.Range(0, allCols.Count);
            chosen.Add(allCols[index]);
            allCols.RemoveAt(index);
        }
        return chosen;
    }

    private void ApplyGaussianOffsets()
    {
        if (Grid == null)
            return;

        float sigmaX = maxXOffset * offsetStdDevFactor;
        float sigmaY = maxYOffset * offsetStdDevFactor;

        foreach (Node node in AllNodes)
        {
            Vector2 offset = new Vector2(
                SampleClampedGaussian(0f, sigmaX, -maxXOffset, maxXOffset),
                SampleClampedGaussian(0f, sigmaY, -maxYOffset, maxYOffset)
            );

            Grid.SetOffset(node.GridPos.x, node.GridPos.y, offset);
        }

        // boss node also gets a clean offset
        Grid.SetOffset(BossNode.GridPos.x, BossNode.GridPos.y, Vector2.zero);
    }

    private float SampleClampedGaussian(float mean, float stdDev, float min, float max)
    {
        if (stdDev <= 0f)
            return Mathf.Clamp(mean, min, max);

        for (int i = 0; i < 6; i++)
        {
            float u1 = 1f - Random.value;
            float u2 = 1f - Random.value;
            float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
            float val = mean + stdDev * randStdNormal;

            if (val >= min && val <= max)
                return val;
        }

        return Mathf.Clamp(mean, min, max);
    }
}
