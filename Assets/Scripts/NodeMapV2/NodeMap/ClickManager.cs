using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private GameObject lastHovered;

    private MapStateManager State => MapStateManager.Instance;

    private void Update()
    {
        HandleHover();
        HandleClick();
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
        else
        {
            EndHover(lastHovered);
            lastHovered = null;
        }
    }

    private void StartHover(GameObject obj)
    {
        if (obj == null) return;

        NodeView nv = obj.GetComponentInParent<NodeView>();
        if (nv == null) return;

        if (State.IsNodeInteractable(nv.NodeData))
            nv.ShowAvailable(true);
    }

    private void EndHover(GameObject obj)
    {
        if (obj == null) return;

        NodeView nv = obj.GetComponentInParent<NodeView>();
        if (nv == null) return;

        nv.ClearHoverGlow();
    }

    private void HandleClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
            OnClick(hit.collider.gameObject);
    }

    private void OnClick(GameObject clicked)
    {
        if (clicked == null) return;

        NodeView nv = clicked.GetComponentInParent<NodeView>();
        if (nv == null) return;

        if (State.TrySelectOrMoveToNode(nv.NodeData))
        {
            // ---- NEW: FIND THE SCENE MANAGER IN THIS SCENE ----
            var sceneManagerObj = GameObject.Find("SceneManager");

            if (sceneManagerObj == null)
            {
                Debug.LogError("ClickManager: Could not find a SceneManager object in this scene.");
                return;
            }

            var sceneSwitch = sceneManagerObj.GetComponent<SceneSwitch>();

            if (sceneSwitch == null)
            {
                Debug.LogError("ClickManager: SceneManager object does not contain a SceneSwitch component.");
                return;
            }

            // ---- DETERMINE WHICH SCENE TO LOAD ----
            // You can adjust this if NodeDefinitions carry their own scene name
            string route = nv.NodeData.Definition.nodeSceneName;

            if (string.IsNullOrEmpty(route))
            {
                Debug.LogError($"ClickManager: Node {nv.NodeData.Id} has no assigned sceneName in its NodeDefinition.");
                return;
            }

            // ---- CALL THE SCENE CHANGER ----
            sceneSwitch.SceneChanger(route);
        }
    }

}
