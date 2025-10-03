using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    private InputSystem_Actions controls;
    private GameObject lastHovered;
    [SerializeField] private SceneSwitch sceneSwitch;

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
        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
            if (hit.collider.gameObject != lastHovered)
            {
                if (lastHovered != null)
                    // Handle end hover logic

                lastHovered = hit.collider.gameObject;
                // Handle start hover logic
            }
        }
        else if (lastHovered != null)
        {
            // Handle end hover logic
            lastHovered = null;
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            NodeBehaviour nodeBehaviour = hit.collider.gameObject.GetComponent<NodeBehaviour>();
            NodeType nodeType = nodeBehaviour.definition.nodeType;
            string nodeString = "";
            switch (nodeType)
            {
                case NodeType.Combat:
                    nodeString = "Combat";
                    sceneSwitch.SceneChanger(nodeString);
                    break;
                case NodeType.Loot:
                    nodeString = "Loot";
                    break;
                case NodeType.Rest:
                    nodeString = "Rest";
                    sceneSwitch.SceneChanger(nodeString);
                    break;
                case NodeType.Event:
                    nodeString = "Event";
                    break;
                case NodeType.Shop:
                    nodeString = "Shop";
                    sceneSwitch.SceneChanger(nodeString);
                    break;
                default:
                    nodeString = "Unknown Route";
                    break;
            }
            Debug.Log("Navigating to " + nodeString + " Scene");
        }
    }
}
