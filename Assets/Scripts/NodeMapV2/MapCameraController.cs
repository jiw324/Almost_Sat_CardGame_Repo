using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    // World-space bounding box for the map
    private float minX, maxX, minZ, maxZ;
    private bool boundsSet = false;

    // Extra padding around map bounds
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
        // Movement
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        if (moveDir.sqrMagnitude > 0.01f)
            transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (!boundsSet)
            return;

        // Bounds clamping
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }
    public void SetBoundsUsingWorldBounds(Bounds bounds)
    {
        // Expand map bounds slightly for movement comfort
        minX = bounds.min.x - horizontalPadding;
        maxX = bounds.max.x + horizontalPadding;

        // Clamp Z bounds safely so the map is never scrolled out of view
        float startFloorZ = bounds.min.z - verticalPadding;
        float topFloorZ = bounds.max.z;

        // Prevent scrolling ABOVE the top floor by more than half a unit
        maxZ = topFloorZ - 0.5f;

        // Prevent scrolling BELOW the bottom floor by more than half a unit
        minZ = startFloorZ - 0.5f;

        boundsSet = true;

        // Initial camera start:
        // - X centered
        // - Slightly below the bottom floor
        Vector3 startPos = new Vector3(
            (bounds.min.x + bounds.max.x) * 0.5f,
            transform.position.y,
            startFloorZ - 0.5f
        );

        transform.position = startPos;
    }
}
