using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to create a Relic Node scene by copying the existing ForestLoot scene.
/// This keeps all the same background/effects/UI (including LootSceneController),
/// and your text logic already shows the correct messages for relics.
///
/// Use via menu: Tools → Relics → Create Relic Node Scene
/// </summary>
public static class RelicNodeSceneCreator
{
    // Source scene to copy (your existing loot/relic scene)
    private const string SourceScenePath = "Assets/Scenes/ForestLoot.unity";

    [MenuItem("Tools/Relics/Create Relic Node Scene")]
    public static void CreateRelicNodeScene()
    {
        if (!System.IO.File.Exists(SourceScenePath))
        {
            Debug.LogError("[RelicNodeSceneCreator] Source scene not found at " + SourceScenePath);
            return;
        }

        // Ask user where to save the new scene (default path pre-filled)
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Relic Node Scene (Copy of ForestLoot)",
            "RelicNode",
            "unity",
            "Choose a location for the new Relic Node scene",
            "Assets/Scenes");

        if (string.IsNullOrEmpty(path))
            return;

        // Open the source scene
        Scene sourceScene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);

        // Save a copy of the opened scene to the new path
        if (!EditorSceneManager.SaveScene(sourceScene, path))
        {
            Debug.LogError("[RelicNodeSceneCreator] Failed to save Relic Node scene at " + path);
            return;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[RelicNodeSceneCreator] Created Relic Node scene by copying '{SourceScenePath}' to '{path}'.");
    }
}


