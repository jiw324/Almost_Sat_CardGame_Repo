using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Relic Node - automatically gives player a relic when they enter
/// </summary>
public class RelicNodeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button continueButton;
    
    [Header("Relic to Award")]
    [SerializeField] private RelicData relicToAward;
    
    private RelicBag playerRelicBag;

    private void Start()
    {
        // Get or create the player's relic bag
        playerRelicBag = FindPlayerRelicBag();
        
        if (playerRelicBag == null)
        {
            Debug.LogError("[RelicNodeManager] Could not find player's RelicBag!");
            return;
        }

        // Automatically award the relic
        AwardRelic();

        // Set up continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    private RelicBag FindPlayerRelicBag()
    {
        // Try to find existing relic bag
        RelicBag bag = FindObjectOfType<RelicBag>();
        
        if (bag == null)
        {
            // Try to find GameSession and attach relic bag to it
            var session = GameSession.Instance;
            if (session != null)
            {
                bag = session.GetComponent<RelicBag>();
                if (bag == null)
                {
                    bag = session.gameObject.AddComponent<RelicBag>();
                    Debug.Log("[RelicNodeManager] Created RelicBag on GameSession");
                }
            }
            else
            {
                // Fallback: create standalone persistent object
                GameObject relicBagObj = new GameObject("PlayerRelicBag");
                DontDestroyOnLoad(relicBagObj);
                bag = relicBagObj.AddComponent<RelicBag>();
                Debug.Log("[RelicNodeManager] Created new RelicBag for player");
            }
        }
        
        return bag;
    }

    private void AwardRelic()
    {
        if (relicToAward == null)
        {
            Debug.LogWarning("[RelicNodeManager] No relic assigned to award!");
            return;
        }

        bool success = playerRelicBag.AddRelic(relicToAward);
        
        if (success)
        {
            Debug.Log($"[RelicNodeManager] Awarded relic: {relicToAward.relicName}");
            // You can add UI feedback here (show relic card, play animation, etc.)
        }
        else
        {
            Debug.LogWarning($"[RelicNodeManager] Failed to add relic: {relicToAward.relicName}");
        }
    }

    private void OnContinueClicked()
    {
        // Mark node as completed and return to map
        var mapState = MapStateManager.Instance;
        if (mapState != null)
        {
            Node currentNode = mapState.GetCurrentNode();
            if (currentNode != null)
            {
                mapState.MarkCompleted(currentNode);
            }
        }

        // Return to map scene
        var sceneManagerObj = GameObject.Find("SceneManager");
        if (sceneManagerObj != null)
        {
            var sceneSwitch = sceneManagerObj.GetComponent<SceneSwitch>();
            if (sceneSwitch != null)
            {
                sceneSwitch.SceneChanger("Map");
            }
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}

