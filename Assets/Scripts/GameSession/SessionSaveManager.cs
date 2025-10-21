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
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            //string saveFilePath = $"{saveDirectoryPath}/Save_{currentDateTime}.json";
            string saveFilePath = $"{saveDirectoryPath}/Save.json";

            if (!Directory.Exists(saveDirectoryPath))
            {
                Directory.CreateDirectory(saveDirectoryPath);
            }

            if (sessionData.sessionNodeMapData.currentNodeData != null)
            {
                sessionData.sessionNodeMapData.currentNodeJson = JsonUtility.ToJson(sessionData.sessionNodeMapData.currentNodeData, true);
            }

            string gameSessionJson = JsonUtility.ToJson(sessionData, true);

            if (File.Exists(saveFilePath))
            {
                Debug.Log("Overwriting save file");
            }
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
        GameSessionData loadedSessionData;
        try
        {
            string savePath = $"{saveDirectoryPath}/{saveName}.json";
            if (!File.Exists(savePath))
            {
                Debug.Log("Save Failed to Load: File Does Not Exist");
                return null;
            }
            string gameSessionJson = File.ReadAllText(savePath);
            loadedSessionData = JsonUtility.FromJson<GameSessionData>(gameSessionJson);

            switch (loadedSessionData.sessionNodeMapData.currentNodeType)
            {
                case NodeType.Combat:
                    loadedSessionData.sessionNodeMapData.currentNodeData =
                        JsonUtility.FromJson<CombatNodeData>(loadedSessionData.sessionNodeMapData.currentNodeJson);
                    break;

                case NodeType.Shop:
                    loadedSessionData.sessionNodeMapData.currentNodeData =
                        JsonUtility.FromJson<ShopNodeData>(loadedSessionData.sessionNodeMapData.currentNodeJson);
                    break;

                case NodeType.Event:
                    loadedSessionData.sessionNodeMapData.currentNodeData =
                        JsonUtility.FromJson<EventNodeData>(loadedSessionData.sessionNodeMapData.currentNodeJson);
                    break;
                case NodeType.Rest:
                    loadedSessionData.sessionNodeMapData.currentNodeData =
                        JsonUtility.FromJson<RestNodeData>(loadedSessionData.sessionNodeMapData.currentNodeJson);
                    break;
                case NodeType.Loot:
                    loadedSessionData.sessionNodeMapData.currentNodeData =
                        JsonUtility.FromJson<LootNodeData>(loadedSessionData.sessionNodeMapData.currentNodeJson);
                    break;
            }          
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to Load Save Data: " + e.Message);
            return null;
        }
        Debug.Log("Successfully Loaded Game Session!");
        return loadedSessionData;
    }

    public static bool DeleteSavedGameSession(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        Debug.LogError("Could not delete save. File not found");
        return false;
    }

    // function to update save name
}
