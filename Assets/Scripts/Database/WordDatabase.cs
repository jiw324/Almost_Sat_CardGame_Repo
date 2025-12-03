using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class WordListWrapper
{
    public List<string> wordpool;
}

public static class WordDatabase
{
    private static bool isLoaded = false;
    private static List<string> words = new();

    // Ensure words are loaded before access.
    private static void EnsureLoaded()
    {
        if (isLoaded) return;

        string path = Path.Combine(Application.streamingAssetsPath, "wordpool.json");

        if (!File.Exists(path))
        {
            Debug.LogError($"[WordDatabase] File not found at path: {path}");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            WordListWrapper wrapper = JsonUtility.FromJson<WordListWrapper>(json);

            if (wrapper != null && wrapper.wordpool != null)
            {
                words = wrapper.wordpool;
                isLoaded = true;
                Debug.Log($"[WordDatabase] Loaded {words.Count} words");
            }
            else
            {
                Debug.LogWarning("[WordDatabase] JSON parsed but no words found.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WordDatabase] Failed to load words: {ex.Message}");
        }
    }

    public static List<string> GetAllWords()
    {
        EnsureLoaded();
        return words;
    }

    public static string GetRandomWord()
    {
        EnsureLoaded();
        if (words.Count == 0)
        {
            Debug.LogWarning("[WordDatabase] No words available.");
            return "";
        }
        return words[Random.Range(0, words.Count)];
    }

    public static List<string> GetRandomWords(int count)
    {
        EnsureLoaded();
        if (words.Count == 0) return new List<string>();

        List<string> result = new();
        for (int i = 0; i < count; i++)
        {
            result.Add(words[Random.Range(0, words.Count)]);
        }
        return result;
    }
}
