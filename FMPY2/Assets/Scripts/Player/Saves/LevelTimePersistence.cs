using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

// Container for a single level's performance data
[Serializable]
public class LevelTimeEntry
{
    public string levelId;
    public float bestSeconds;
    public string bestFormatted; // Pre-formatted string for easy UI display
    public float latestSeconds;
    public string latestFormatted;
    public string updatedAtUtc; // Timestamp for when the record was achieved
}

// Wrapper class to make the list compatible with Unity's JsonUtility
[Serializable]
public class LevelTimeDatabase
{
    public List<LevelTimeEntry> levels = new List<LevelTimeEntry>();
}

public static class LevelTimePersistence
{
    // Event that UI scripts (like LevelTimeDisplay) listen to for real-time updates
    public static event Action<string, LevelTimeEntry> OnLevelTimeUpdated;

    private static LevelTimeDatabase cache;
    // Saves to a location that works on PC, Mac, and Mobile (persistentDataPath)
    private static string FilePath => Path.Combine(Application.persistentDataPath, "level_times.json");

    /// <summary>
    /// Records a new time for a level. Updates Personal Best if the time is faster.
    /// </summary>
    public static void SaveLevelTime(string levelId, float seconds)
    {
        if (string.IsNullOrWhiteSpace(levelId)) return;

        LevelTimeDatabase db = LoadDatabase();
        LevelTimeEntry entry = db.levels.Find(x => x.levelId == levelId);

        if (entry == null)
        {
            // First time this level has been completed: create a new entry
            entry = new LevelTimeEntry
            {
                levelId = levelId,
                bestSeconds = seconds,
                bestFormatted = FormatTime(seconds),
                latestSeconds = seconds,
                latestFormatted = FormatTime(seconds),
                updatedAtUtc = DateTime.UtcNow.ToString("o")
            };
            db.levels.Add(entry);
        }
        else
        {
            // Update the "Last Played" time
            entry.latestSeconds = seconds;
            entry.latestFormatted = FormatTime(seconds);

            // If this is the fastest time yet, update the Personal Best
            if (entry.bestSeconds <= 0f || seconds < entry.bestSeconds)
            {
                entry.bestSeconds = seconds;
                entry.bestFormatted = FormatTime(seconds);
            }

            entry.updatedAtUtc = DateTime.UtcNow.ToString("o");
        }

        SaveDatabase(db);

        // Notify any active UI elements that the data has changed
        OnLevelTimeUpdated?.Invoke(levelId, entry);
    }

    public static LevelTimeEntry GetLevelTime(string levelId)
    {
        LevelTimeDatabase db = LoadDatabase();
        return db.levels.Find(x => x.levelId == levelId);
    }

    public static LevelTimeDatabase GetAllTimes()
    {
        return LoadDatabase();
    }

    public static void ClearAll()
    {
        cache = new LevelTimeDatabase();
        SaveDatabase(cache);
    }

    /// <summary>
    /// Loads the JSON file from disk or returns the cached version.
    /// </summary>
    private static LevelTimeDatabase LoadDatabase()
    {
        if (cache != null) return cache; // Return memory cache if already loaded

        if (!File.Exists(FilePath))
        {
            cache = new LevelTimeDatabase();
            return cache;
        }

        // Read the text file and convert the JSON string back into a C# object
        string json = File.ReadAllText(FilePath);
        cache = string.IsNullOrWhiteSpace(json)
            ? new LevelTimeDatabase()
            : JsonUtility.FromJson<LevelTimeDatabase>(json);

        if (cache == null || cache.levels == null)
            cache = new LevelTimeDatabase();

        return cache;
    }

    /// <summary>
    /// Converts the data into JSON format and writes it to a file.
    /// </summary>
    private static void SaveDatabase(LevelTimeDatabase db)
    {
        cache = db ?? new LevelTimeDatabase();
        // 'true' makes the JSON readable for humans (adds indenting)
        string json = JsonUtility.ToJson(cache, true); 
        File.WriteAllText(FilePath, json);
    }

    private static string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return time.ToString(@"mm\:ss\.ff");
    }
}
