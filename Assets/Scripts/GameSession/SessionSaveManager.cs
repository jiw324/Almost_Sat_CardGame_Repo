using System;
using System.IO;
using UnityEngine;

public static class SessionSaveManager
{
    private static string saveDirectoryPath = $"{Application.persistentDataPath}/Saves/";

    public static bool SaveGameSession(GameSessionData sessionData)
    {
        try
        {
            string saveFilePath = $"{saveDirectoryPath}/Save.json";

            if (!Directory.Exists(saveDirectoryPath))
                Directory.CreateDirectory(saveDirectoryPath);

            if (sessionData.sessionNodeMapData != null &&
                sessionData.sessionNodeMapData.currentNodeData != null)
            {
                sessionData.sessionNodeMapData.currentNodeJson =
                    JsonUtility.ToJson(sessionData.sessionNodeMapData.currentNodeData, true);
            }

            if (sessionData.tutorialNodeMapData != null &&
                sessionData.tutorialNodeMapData.currentNodeData != null)
            {
                sessionData.tutorialNodeMapData.currentNodeJson =
                    JsonUtility.ToJson(sessionData.tutorialNodeMapData.currentNodeData, true);
            }

            string gameSessionJson = JsonUtility.ToJson(sessionData, true);
            File.WriteAllText(saveFilePath, gameSessionJson);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to Save Game Session: " + e.Message);
            return false;
        }

        Debug.Log("Successfully Saved Game Session!");
        return true;
    }

    public static GameSessionData LoadGameSession(string saveName)
    {
        try
        {
            string savePath = $"{saveDirectoryPath}/{saveName}.json";
            if (!File.Exists(savePath))
            {
                Debug.Log("Save Failed to Load: File Does Not Exist");
                return null;
            }

            string gameSessionJson = File.ReadAllText(savePath);
            GameSessionData loadedSessionData =
                JsonUtility.FromJson<GameSessionData>(gameSessionJson);

            // Main run node data reconstruction
            RebuildNodeData(loadedSessionData.sessionNodeMapData);

            // Tutorial node data reconstruction
            RebuildNodeData(loadedSessionData.tutorialNodeMapData);

            Debug.Log("Successfully Loaded Game Session!");
            return loadedSessionData;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to Load Save Data: " + e.Message);
            return null;
        }
    }

    private static void RebuildNodeData(SessionNodeMapData nodeMapData)
    {
        if (nodeMapData == null || string.IsNullOrEmpty(nodeMapData.currentNodeJson))
            return;

        switch (nodeMapData.currentNodeType)
        {
            case NodeType.Combat:
                nodeMapData.currentNodeData =
                    JsonUtility.FromJson<CombatNodeData>(nodeMapData.currentNodeJson);
                break;

            case NodeType.Rest:
                nodeMapData.currentNodeData =
                    JsonUtility.FromJson<RestNodeData>(nodeMapData.currentNodeJson);
                break;

            case NodeType.Shop:
                nodeMapData.currentNodeData =
                    JsonUtility.FromJson<ShopNodeData>(nodeMapData.currentNodeJson);
                break;

            case NodeType.Loot:
                nodeMapData.currentNodeData =
                    JsonUtility.FromJson<LootNodeData>(nodeMapData.currentNodeJson);
                break;
        }
    }
}
