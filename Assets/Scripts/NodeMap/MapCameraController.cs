using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    private Vector2 xzMin;
    private Vector2 xzMax;
    private bool boundsSet = false;

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

        if (boundsSet)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, xzMin.x, xzMax.x);
            pos.z = Mathf.Clamp(pos.z, xzMin.y, xzMax.y);
            transform.position = pos;
        }
    }

    public void SetBounds(Vector3 mapOrigin, int mapWidth, int mapHeight, float gridXSpacing, float gridYSpacing)
    {
        // horizontal center across columns
        float centerX = mapOrigin.x + (mapWidth - 1) * 0.5f * gridXSpacing;

        // bottom anchor
        float baseZ = mapOrigin.z;

        float mapWidthWorld = mapWidth * gridXSpacing;
        float mapHeightWorld = mapHeight * gridYSpacing;

        // horizontal margins
        float visibleCols = 3f;
        float horizontalMargin = visibleCols * gridXSpacing * 0.5f;

        // vertical margins
        float extraBottomRows = 1.5f;
        float topViewRows = 3.0f;

        float bottomMargin = extraBottomRows * gridYSpacing;
        float topMargin = topViewRows * gridYSpacing;

        // compute bounds
        xzMin = new Vector2(
            centerX - (mapWidthWorld * 0.5f) + horizontalMargin,
            baseZ - bottomMargin
        );

        xzMax = new Vector2(
            centerX + (mapWidthWorld * 0.5f) - horizontalMargin,
            baseZ + mapHeightWorld - topMargin
        );

        boundsSet = true;
    }
}
