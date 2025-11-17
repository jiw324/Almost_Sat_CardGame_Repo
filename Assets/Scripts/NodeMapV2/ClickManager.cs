using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SceneSwitch sceneSwitch;
    [SerializeField] private MapPositionManager mapPositionManager;

    private InputSystem_Actions controls;
    private GameObject lastHovered;

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        controls.NodeMap.Enable();
        controls.NodeMap.Click.performed += OnClick;
    }

    private void OnDisable()
    {
        controls.NodeMap.Click.performed -= OnClick;
        controls.NodeMap.Disable();
    }

    private void Update()
    {
        HandleHover();
    }

    private void HandleHover()
    {
        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject != lastHovered)
            {
                EndHover(lastHovered);
                lastHovered = hit.collider.gameObject;
                StartHover(lastHovered);
            }
        }
        else if (lastHovered != null)
        {
            EndHover(lastHovered);
            lastHovered = null;
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        NodeView nodeView = hit.collider.GetComponent<NodeView>();
        if (nodeView == null || nodeView.NodeData == null)
            return;

        INode clickedNode = nodeView.NodeData;

        bool canInteract = mapPositionManager.TrySelectOrMoveToNode(clickedNode);
        if (!canInteract)
            return;

        NodeDefinition def = nodeView.Definition;
        if (def == null)
            return;

        string sceneName = def.nodeSceneName;

        if (def.nodeType == NodeType.Combat ||
            def.nodeType == NodeType.Rest ||
            def.nodeType == NodeType.Shop)
        {
            sceneSwitch.SceneChanger(sceneName);
        }

        Debug.Log($"Navigating to {sceneName} Scene");
    }

    private void StartHover(GameObject obj)
    {
        if (obj == null) return;

        NodeView nodeView = obj.GetComponentInParent<NodeView>();
        if (nodeView == null) return;

        bool interactable = mapPositionManager.IsNodeInteractable(nodeView.NodeData);
        // TODO: add hover visuals based on interactable
    }

    private void EndHover(GameObject obj)
    {
        if (obj == null) return;
        // TODO: clear hover visuals
    }
}
