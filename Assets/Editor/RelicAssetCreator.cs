using UnityEditor;
using UnityEngine;

/// <summary>
/// One-click helper to create a sample relic ScriptableObject that
/// uses the "RelicIcon" sprite as its icon (if found).
/// </summary>
public static class RelicAssetCreator
{
    private const string AssetFolderPath = "Assets/Resources/Relics";
    private const string AssetPath = AssetFolderPath + "/StarterRelic.asset";

    [MenuItem("Tools/Relics/Create Starter Relic")]
    public static void CreateStarterRelic()
    {
        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(AssetFolderPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Relics");
        }

        RelicData relic = ScriptableObject.CreateInstance<RelicData>();
        relic.relicId = "starter_relic";
        relic.relicName = "Starter Relic";
        relic.description = "A simple test relic used to verify the relic bag UI.";
        relic.rarity = "Common";
        relic.effectType = "None";

        // Try to find a sprite named 'RelicIcon' anywhere in the project
        string[] guids = AssetDatabase.FindAssets("RelicIcon t:Sprite");
        if (guids != null && guids.Length > 0)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(guids[0]);
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (icon != null)
            {
                relic.icon = icon;
            }
        }

        AssetDatabase.CreateAsset(relic, AssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = relic;

        Debug.Log("[RelicAssetCreator] Created StarterRelic at " + AssetPath);
    }
}


