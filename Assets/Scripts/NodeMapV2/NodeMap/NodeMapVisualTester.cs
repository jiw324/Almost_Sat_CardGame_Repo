using UnityEngine;

public class NodeMapVisualTester : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 5;
    [SerializeField] private int mapHeight = 10;
    [SerializeField] private int maxPaths = 10;

    [Header("Spawner Settings")]
    [SerializeField] private NodeMapSpawner spawner;   // assign in inspector
    [SerializeField] private Transform mapParent;      // optional
    [SerializeField] private bool logPaths = true;     // toggle debug logs

    private NodeMap _map;
    private NodeGrid _grid;

    private void Start()
    {
        Debug.Log("=== NodeMap Visual Test Starting ===");

        // --- Step 1: Create the core systems ---
        _grid = new NodeGrid(mapWidth, mapHeight);
        NodeFactory factory = new NodeFactory();

        // --- Step 2: Generate the logical map ---
        _map = new NodeMap(_grid, factory, mapWidth, mapHeight, maxPaths);
        _map.Generate();

        if (logPaths)
            _map.DebugPrintPaths();

        // --- Step 3: Spawn the visual map ---
        if (spawner == null)
        {
            spawner = FindObjectOfType<NodeMapSpawner>();
            if (spawner == null)
            {
                Debug.LogError("No NodeMapSpawner found in scene!");
                return;
            }
        }

        spawner.Spawn(_map, _grid);

        Debug.Log("=== NodeMap Visual Test Complete ===");
    }
}
