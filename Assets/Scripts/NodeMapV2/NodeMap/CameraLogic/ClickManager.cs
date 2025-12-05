using System.Linq;
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

        if (State == null) return;

        bool isAvailable = State
            .GetAvailableNodes()
            .Any(n => n.Id == nv.NodeData.Id);

        if (isAvailable)
            nv.ShowAvailable(hover: true);
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
            // Initialize node-specific data
            if (nv.NodeData.Definition.nodeType == NodeType.Combat)
            {
                string enemyName = CombatNodeEnemyAssigner.GetEnemyDefinitionName(nv.NodeData);
                if (string.IsNullOrWhiteSpace(enemyName))
                {
                    // Use default enemy if none is assigned
                    enemyName = "Gary Goblin";
                    Debug.LogWarning($"[ClickManager] Combat node has no enemy assigned. Using default enemy: {enemyName}");
                }
                else
                {
                    Debug.Log($"[ClickManager] Initialized combat with enemy: {enemyName}");
                }
                
                // Always initialize combat node data, even with default enemy
                CombatNodeDataInitializer.InitializeCombatNode(enemyName);
            }
            else if (nv.NodeData.Definition.nodeType == NodeType.Loot)
            {
                // When entering a loot node, immediately grant a relic from the JSON list
                LootNodeDataInitializer.InitializeLootNode();
            }

            var sceneManagerObj = GameObject.Find("SceneManager");
            if (sceneManagerObj == null) return;

            var sceneSwitch = sceneManagerObj.GetComponent<SceneSwitch>();
            if (sceneSwitch == null) return;

            string route = nv.NodeData.Definition.nodeSceneName;
            if (string.IsNullOrEmpty(route)) return;

            sceneSwitch.SceneChanger(route);
        }
    }
}
