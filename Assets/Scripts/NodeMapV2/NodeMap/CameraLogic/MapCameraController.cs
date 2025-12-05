using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    private float minX, maxX, minZ, maxZ;
    private bool boundsSet = false;

    [SerializeField] private float horizontalPadding = 1f;
    [SerializeField] private float verticalPadding = 1f;

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

        if (current != null)
        {
            Vector3 nodePos = MapGenerationManager.Instance.ActiveMap
                .Grid
                .GridToWorld(current.GridPos.x, current.GridPos.y);

            Vector3 newCamPos = new Vector3(
                nodePos.x,
                transform.position.y,
                nodePos.z
            );

            transform.position = newCamPos;
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
