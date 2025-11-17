using UnityEngine;

public class FogController : MonoBehaviour
{
    [SerializeField] private float horizontalPadding = 2f;
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

        UpdateFog();
        _state.OnNodeChanged += UpdateFog;
    }

    private void OnDestroy()
    {
        if (_state != null)
            _state.OnNodeChanged -= UpdateFog;
    }

    public void UpdateFog()
    {
        if (_map == null || _state == null)
            return;

        Node current = _state.GetCurrentNode();
        int currentFloor = current != null ? current.GridPos.y : 0;

        // Fog begins ONE FLOOR EARLIER now
        int fogStartFloor = currentFloor + revealRadius;
        fogStartFloor = Mathf.Clamp(fogStartFloor, 0, _map.MapHeight - 1);

        Vector3 fogStartWorld = _map.Grid.GridToWorld(0, fogStartFloor);
        float fogStartZ = fogStartWorld.z;

        float topZ = _bounds.max.z;

        float fogLength = Mathf.Max(1f, (topZ - fogStartZ) + verticalPadding * 2f);

        // NEW – extend horizontally into the forest ring
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
}
