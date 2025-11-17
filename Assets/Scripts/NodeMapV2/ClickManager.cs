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

        NodeView view = hit.collider.GetComponent<NodeView>();
        if (view == null) return;

        if (!mapPositionManager.TrySelectOrMoveToNode(view.NodeData))
            return;

        var def = view.Definition;
        if (def == null) return;

        // Store node type for gameplay scene
        var sm = GameSession.Instance.gameSessionData.sessionNodeMapData;
        sm.currentNodeType = def.nodeType;
        sm.currentNodeJson = "";
        sm.currentNodeData = null;

        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);

        string sceneName = def.nodeSceneName;
        sceneSwitch.SceneChanger(sceneName);
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
