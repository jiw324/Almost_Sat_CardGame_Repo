using UnityEngine;

public class FogController : MonoBehaviour
{
    [SerializeField] private float verticalPadding = 2f;
    [SerializeField] private float forestPadding = 6f;
    [SerializeField] private float fogHeight = 0.6f;
    [SerializeField] private int revealRadius = 3;

    private NodeMap _map;
    private MapStateManager _state;
    private Bounds _bounds;

    public void Initialize(NodeMap map, Bounds mapBounds)
    {
        _map = map;
        _bounds = mapBounds;
        _state = MapStateManager.Instance;

        UpdateFogAndVisibility();
        _state.OnNodeChanged += UpdateFogAndVisibility;
    }

    private void OnDestroy()
    {
        if (_state != null)
            _state.OnNodeChanged -= UpdateFogAndVisibility;
    }

    private void UpdateFogAndVisibility()
    {
        if (_map == null || _state == null)
            return;

        UpdateFogPlane();
        UpdateNodeHiddenStates();
    }

    private void UpdateFogPlane()
    {
        Node current = _state.GetCurrentNode();
        int currentFloor = current != null ? current.GridPos.y : 0;

        int fogStartFloor = Mathf.Clamp(currentFloor + revealRadius, 0, _map.MapHeight - 1);

        Vector3 fogStartWorld = _map.Grid.GridToWorld(0, fogStartFloor);
        float fogStartZ = fogStartWorld.z + 0.5f;

        float topZ = _bounds.max.z;

        float fogLength = Mathf.Max(1f, (topZ - fogStartZ) + verticalPadding * 2f);
        float fogWidth = (_bounds.max.x - _bounds.min.x) + forestPadding * 2f;

        float centerX = (_bounds.min.x + _bounds.max.x) * 0.5f;
        float centerZ = fogStartZ + fogLength * 0.5f;

        transform.position = new Vector3(centerX, fogHeight, centerZ);

        transform.localScale = new Vector3(
            fogWidth / 10f,
            1f,
            fogLength / 10f
        );
    }

    private void UpdateNodeHiddenStates()
    {
        var views = Object.FindObjectsByType<NodeView>(FindObjectsSortMode.None);
        if (views == null || views.Length == 0)
            return;

        Node current = _state.GetCurrentNode();
        int currentFloor = current != null ? current.GridPos.y : 0;

        int fogStartFloor = Mathf.Clamp(currentFloor + revealRadius, 0, _map.MapHeight - 1);

        int hideStartFloor = fogStartFloor + 1;

        foreach (var view in views)
        {
            int nodeFloor = view.NodeData.GridPos.y;
            bool hidden = nodeFloor >= hideStartFloor;
            view.SetHidden(hidden);
        }
    }
}
