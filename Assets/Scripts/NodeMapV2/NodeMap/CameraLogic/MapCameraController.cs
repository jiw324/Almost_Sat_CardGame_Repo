using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MapCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    private float minX, maxX, minZ, maxZ;
    private bool boundsSet = false;

    [SerializeField] private float horizontalPadding = 1f;
    [SerializeField] private float verticalPadding = 1f;

    [SerializeField] private float floorLookDownOffset = 0.75f;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.NodeMap.MoveCamera.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.NodeMap.MoveCamera.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        if (moveDir.sqrMagnitude > 0.01f)
            transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (!boundsSet)
            return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    public void SetBoundsUsingWorldBounds(Bounds bounds)
    {
        minX = bounds.min.x - horizontalPadding;
        maxX = bounds.max.x + horizontalPadding;

        float startFloorZ = bounds.min.z - verticalPadding;
        float topFloorZ = bounds.max.z;

        maxZ = topFloorZ - 0.5f;
        minZ = startFloorZ - 0.5f;

        boundsSet = true;

        var state = MapStateManager.Instance;
        Node current = state?.GetCurrentNode();

        if (current != null && MapGenerationManager.Instance?.ActiveMap != null)
        {
            var grid = MapGenerationManager.Instance.ActiveMap.Grid;

            Vector3 currentWorld = grid.GridToWorld(current.GridPos.x, current.GridPos.y);
            float floorSpacing = grid.YSpacing;
            float targetZ = currentWorld.z - floorSpacing * floorLookDownOffset;

            var available = state.GetAvailableNodes().ToList();

            float targetX;
            if (available.Count > 0)
            {
                targetX = available
                    .Select(n => grid.GridToWorld(n.GridPos.x, n.GridPos.y).x)
                    .Average();
            }
            else
            {
                targetX = currentWorld.x;
            }

            transform.position = new Vector3(
                targetX,
                transform.position.y,
                targetZ
            );

            return;
        }

        Vector3 fallback = new Vector3(
            (bounds.min.x + bounds.max.x) * 0.5f,
            transform.position.y,
            startFloorZ - 0.5f
        );

        transform.position = fallback;
    }
}
