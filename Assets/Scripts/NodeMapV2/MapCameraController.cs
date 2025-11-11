using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class MapCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float smoothTime = 0.1f;

    private InputSystem_Actions _input;
    private Vector2 _moveInput;
    private Vector3 _velocity;

    private bool _boundsSet = false;
    private Vector3 _mapCenter;
    private Vector2 _xzMin;
    private Vector2 _xzMax;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();

        _input = new InputSystem_Actions();
        _input.NodeMap.MoveCamera.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.NodeMap.MoveCamera.canceled += ctx => _moveInput = Vector2.zero;
    }

    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    private void Start()
    {
        // Optionally recenter at startup
        if (_boundsSet)
            CenterCamera();
    }

    private void Update()
    {
        HandleMovement();
        ApplyBounds();
    }

    private void HandleMovement()
    {
        if (_moveInput.sqrMagnitude < 0.01f)
            return;

        // Convert input to world-space movement
        Vector3 moveDir = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
        Vector3 target = transform.position + moveDir * moveSpeed * Time.deltaTime;

        // Smooth damp movement
        transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, smoothTime);
    }

    private void ApplyBounds()
    {
        if (!_boundsSet)
            return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, _xzMin.x, _xzMax.x);
        pos.z = Mathf.Clamp(pos.z, _xzMin.y, _xzMax.y);
        transform.position = pos;
    }

    /// <summary>
    /// Called by the MapGenerationTester or MapGenerationManager after NodeMapSpawner completes.
    /// </summary>
    public void SetBounds(Vector3 mapCenter, int mapWidth, int mapHeight, float gridXSpacing, float gridYSpacing)
    {
        _mapCenter = mapCenter;

        float mapWidthWorld = mapWidth * gridXSpacing;
        float mapHeightWorld = mapHeight * gridYSpacing;

        // Margins define how far you can scroll beyond map edges
        float marginX = gridXSpacing * 1.5f;
        float marginZ = gridYSpacing * 1.5f;

        // Define world-space clamp bounds
        _xzMin = new Vector2(
            mapCenter.x - (mapWidthWorld / 2f) - marginX,
            mapCenter.z - marginZ
        );
        _xzMax = new Vector2(
            mapCenter.x + (mapWidthWorld / 2f) + marginX,
            mapCenter.z + mapHeightWorld + marginZ
        );

        _boundsSet = true;
        CenterCamera();
    }

    /// <summary>
    /// Repositions camera horizontally at map center (used after map spawn).
    /// </summary>
    public void CenterCamera()
    {
        if (!_boundsSet)
            return;

        Vector3 pos = transform.position;
        pos.x = _mapCenter.x;
        pos.z = _xzMin.y + (_xzMax.y - _xzMin.y) * 0.25f; // Slightly toward bottom of map
        transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_boundsSet)
            return;

        Gizmos.color = Color.yellow;
        Vector3 min = new Vector3(_xzMin.x, transform.position.y, _xzMin.y);
        Vector3 max = new Vector3(_xzMax.x, transform.position.y, _xzMax.y);
        Vector3 size = max - min;
        Gizmos.DrawWireCube(_mapCenter + new Vector3(0, 0, size.z / 2f), new Vector3(size.x, 0, size.z));
    }
}
