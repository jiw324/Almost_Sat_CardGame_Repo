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

    private int EffectiveRevealRadius
    {
        get
        {
            int bonus = 0;
            var session = GameSession.Instance;
            if (session != null)
            {
                foreach (var relic in session.GetPlayerRelics())
                {
                    if (relic != null && relic.effectType == "IncreaseVision")
                        bonus += relic.effectValue;
                }
            }
            return Mathf.Max(0, revealRadius + bonus);
        }
    }

    public void Initialize(NodeMap map, Bounds mapBounds)
    {
        _map = map;
        _bounds = mapBounds;
        _state = MapStateManager.Instance;

        UpdateFogAndVisibility();
    }

    public void UpdateFogExternally()
    {
        UpdateFogAndVisibility();
    }

    private void UpdateFogAndVisibility()
    {
        if (_map == null || _state == null)
            return;

        int currentFloor = _state.GetCurrentNode() != null
            ? _state.GetCurrentNode().GridPos.y
            : 0;

        int fogStartFloor = currentFloor + EffectiveRevealRadius;
        int bossFloor = _map.BossNode != null ? _map.BossNode.GridPos.y : _map.MapHeight;

        if (fogStartFloor >= bossFloor)
        {
            RevealAllNodes();
            gameObject.SetActive(false);
            return;
        }

        UpdateFogPlane();
        UpdateNodeHiddenStates();
    }

    private void RevealAllNodes()
    {
        var views = Object.FindObjectsByType<NodeView>(FindObjectsSortMode.None);
        foreach (var view in views)
            view.SetHidden(false);
    }

    private void UpdateFogPlane()
    {
        Node current = _state.GetCurrentNode();
        int currentFloor = current != null ? current.GridPos.y : 0;

        int fogStartFloor = currentFloor + EffectiveRevealRadius;
        Vector3 fogStartWorld = _map.Grid.GridToWorld(0, fogStartFloor);
        float fogStartZ = fogStartWorld.z + 0.5f;

        float topZ = _bounds.max.z;

        float fogLength = Mathf.Max(1f, (topZ - fogStartZ) + verticalPadding * 2f);
        float fogWidth = (_bounds.max.x - _bounds.min.x) + forestPadding * 2f;

        float centerX = (_bounds.min.x + _bounds.max.x) * 0.5f;
        float centerZ = fogStartZ + fogLength * 0.5f;

        transform.position = new Vector3(centerX, fogHeight, centerZ);
        transform.localScale = new Vector3(fogWidth / 10f, 1f, fogLength / 10f);
    }

    private void UpdateNodeHiddenStates()
    {
        var views = Object.FindObjectsByType<NodeView>(FindObjectsSortMode.None);
        if (views == null || views.Length == 0)
            return;

        Node current = _state.GetCurrentNode();
        int currentFloor = current != null ? current.GridPos.y : 0;

        int fogStartFloor = currentFloor + EffectiveRevealRadius;
        int hideStartFloor = fogStartFloor + 1;

        foreach (var view in views)
        {
            Node node = view.NodeData;

            bool isBoss = _map.BossNode != null && node == _map.BossNode;
            if (isBoss)
            {
                view.SetHidden(false);
                continue;
            }

            int nodeFloor = node.GridPos.y;
            bool hidden = nodeFloor >= hideStartFloor;

            view.SetHidden(hidden);
        }
    }
}
