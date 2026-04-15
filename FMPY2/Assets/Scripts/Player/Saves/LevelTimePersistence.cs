using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

[Serializable]
public class LevelTimeEntry
{
    public string levelId;
    public float bestSeconds;
    public string bestFormatted;
    public float latestSeconds;
    public string latestFormatted;
    public string updatedAtUtc;
}

[Serializable]
public class LevelTimeDatabase
{
    public List<LevelTimeEntry> levels = new List<LevelTimeEntry>();
}

public static class LevelTimePersistence
{
    public static event Action<string, LevelTimeEntry> OnLevelTimeUpdated;

    private static LevelTimeDatabase cache;
    private static string FilePath => Path.Combine(Application.persistentDataPath, "level_times.json");

    public static void SaveLevelTime(string levelId, float seconds)
    {
        if (string.IsNullOrWhiteSpace(levelId)) return;

        LevelTimeDatabase db = LoadDatabase();
        LevelTimeEntry entry = db.levels.Find(x => x.levelId == levelId);

        if (entry == null)
        {
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
            entry.latestSeconds = seconds;
            entry.latestFormatted = FormatTime(seconds);

            if (entry.bestSeconds <= 0f || seconds < entry.bestSeconds)
            {
                entry.bestSeconds = seconds;
                entry.bestFormatted = FormatTime(seconds);
            }

            entry.updatedAtUtc = DateTime.UtcNow.ToString("o");
        }

        SaveDatabase(db);

        // Real-time UI refresh hook
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

    private static LevelTimeDatabase LoadDatabase()
    {
        if (cache != null) return cache;

        if (!File.Exists(FilePath))
        {
            cache = new LevelTimeDatabase();
            return cache;
        }

        string json = File.ReadAllText(FilePath);
        cache = string.IsNullOrWhiteSpace(json)
            ? new LevelTimeDatabase()
            : JsonUtility.FromJson<LevelTimeDatabase>(json);

        if (cache == null || cache.levels == null)
            cache = new LevelTimeDatabase();

        return cache;
    }

    private static void SaveDatabase(LevelTimeDatabase db)
    {
        cache = db ?? new LevelTimeDatabase();
        string json = JsonUtility.ToJson(cache, true);
        File.WriteAllText(FilePath, json);
    }

    private static string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return time.ToString(@"mm\:ss\.ff");
    }
}