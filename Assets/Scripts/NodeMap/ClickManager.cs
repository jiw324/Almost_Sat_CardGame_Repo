using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SceneSwitch sceneSwitch;
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

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            NodeBehaviour nodeBehaviour = hit.collider.GetComponent<NodeBehaviour>();
            if (nodeBehaviour == null)
            {
                return;   
            }

            NodeType nodeType = nodeBehaviour.definition.nodeType;
            string sceneName = nodeBehaviour.definition.nodeSceneName;

            if (nodeType == NodeType.Combat || nodeType == NodeType.Rest || nodeType == NodeType.Shop)
            {
                // Initialize combat data if this is a combat node
                if (nodeType == NodeType.Combat)
                {
                    string enemyName = nodeBehaviour.GetEnemyDefinitionName();
                    if (!string.IsNullOrWhiteSpace(enemyName))
                    {
                        CombatNodeDataInitializer.InitializeCombatNode(enemyName);
                        Debug.Log($"[ClickManager] Initialized combat with enemy: {enemyName}");
                    }
                    else
                    {
                        Debug.LogWarning("[ClickManager] Combat node has no enemy assigned. Using default.");
                        // Optionally initialize with a default enemy or let BattleManager handle it
                    }
                }
                
                sceneSwitch.SceneChanger(sceneName);
            }

            Debug.Log($"Navigating to {sceneName} Scene");
        }
    }

    private void StartHover(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        // start hover logic here
    }

    private void EndHover(GameObject obj)
    {
        if (obj == null) {
            return;
        }
        // end hover logic here
    }
}