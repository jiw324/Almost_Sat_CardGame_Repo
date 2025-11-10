using System.Collections.Generic;
using UnityEngine;

public class NodeMap
{
    private readonly NodeGrid _grid;
    private readonly NodeFactory _factory;
    private readonly int _width;
    private readonly int _height;
    private readonly int _maxPaths;

    private readonly Dictionary<int, List<Node>> _rows = new();
    public IReadOnlyDictionary<int, List<Node>> Rows => _rows;

    public NodeMap(NodeGrid grid, NodeFactory factory, int width, int height, int maxPaths = 10)
    {
        _grid = grid;
        _factory = factory;
        _width = width;
        _height = height;
        _maxPaths = maxPaths;
    }

    public void Generate()
    {
        Debug.Log("Generating logical NodeMap...");

        // Initialize rows
        for (int y = 0; y < _height; y++)
            _rows[y] = new List<Node>();

        // Step 1: Decide how many start nodes to have
        int numStarts = Mathf.Clamp(Random.Range(2, _width), 1, _width);
        List<Node> startNodes = new();

        // Step 2: Create a guaranteed path for each start node
        for (int i = 0; i < numStarts; i++)
        {
            int startX = Random.Range(0, _width);
            Vector2Int startPos = new(startX, 0);

            Node startNode = FindOrCreateNode(startPos);
            startNodes.Add(startNode);

            GeneratePathFrom(startNode);
        }

        // Step 3: Generate extra random paths if allowed
        int remainingPaths = Mathf.Max(0, _maxPaths - numStarts);
        for (int i = 0; i < remainingPaths; i++)
        {
            Node randomStart = startNodes[Random.Range(0, startNodes.Count)];
            GeneratePathFrom(randomStart);
        }

        Debug.Log($"Generated NodeMap with {_rows.Count} floors, {startNodes.Count} start nodes, {_maxPaths} total paths.");
    }

    private void GeneratePathFrom(Node startNode)
    {
        Node current = startNode;

        for (int y = 0; y < _height - 1; y++)
        {
            Vector2Int curPos = current.GridPos;
            List<Vector2Int> nextCandidates = _grid.GetForwardNeighbors(curPos);

            if (nextCandidates.Count == 0)
                break;

            Vector2Int chosenNext = nextCandidates[Random.Range(0, nextCandidates.Count)];
            Node nextNode = FindOrCreateNode(chosenNext);

            current.ConnectTo(nextNode);
            current = nextNode;
        }
    }

    private Node FindOrCreateNode(Vector2Int gridPos)
    {
        if (_rows.TryGetValue(gridPos.y, out var row))
        {
            Node existing = row.Find(n => n.GridPos == gridPos);
            if (existing != null)
                return existing;
        }

        NodeType type = NodeType.Combat; // placeholder; validator will assign proper type
        Node newNode = _factory.CreateNode(type, gridPos);
        _rows[gridPos.y].Add(newNode);
        return newNode;
    }

    public IEnumerable<Node> AllNodes()
    {
        foreach (var row in _rows.Values)
            foreach (var node in row)
                yield return node;
    }

    public void DebugPrintPaths()
    {
        Debug.Log("----- GENERATED PATHS -----");

        int pathIndex = 0;
        foreach (var startNode in _rows[0])
        {
            string pathLog = $"[Path {pathIndex}]  ";
            HashSet<Node> visited = new(); // avoid infinite loops

            Node current = startNode;
            while (current != null && !visited.Contains(current))
            {
                visited.Add(current);
                pathLog += current.Id;

                if (current.NextNodes.Count > 0)
                {
                    // follow the first connection for visualization
                    current = current.NextNodes[0] as Node;
                    if (current != null)
                        pathLog += " ? ";
                }
                else current = null;
            }

            Debug.Log(pathLog);
            pathIndex++;
        }

        Debug.Log("---------------------------");
    }

}
