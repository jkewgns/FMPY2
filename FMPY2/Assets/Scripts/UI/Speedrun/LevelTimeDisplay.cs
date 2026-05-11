using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTimeDisplay : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Leave empty to use current scene name, or enter a specific Level ID.")]
    [SerializeField] private string levelIdOverride;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private TextMeshProUGUI latestTimeText;

    void OnEnable()
    {
        // Subscribe to the update event so the UI refreshes automatically when a new time is saved
        LevelTimePersistence.OnLevelTimeUpdated += HandleLevelTimeUpdated;
        Refresh();
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks or errors when the object is destroyed
        LevelTimePersistence.OnLevelTimeUpdated -= HandleLevelTimeUpdated;
    }

    // Triggered whenever any level time is updated in the persistence system
    private void HandleLevelTimeUpdated(string updatedLevelId, LevelTimeEntry entry)
    {
        // Determine which level ID this specific script is responsible for
        string thisLevelId = string.IsNullOrWhiteSpace(levelIdOverride)
            ? SceneManager.GetActiveScene().name
            : levelIdOverride;

        // Only refresh the UI if the updated level matches this display
        if (updatedLevelId == thisLevelId)
            Refresh();
    }

    /// <summary>
    /// Fetches the latest data from persistence and updates the text fields.
    /// </summary>
    public void Refresh()
    {
        string levelId = string.IsNullOrWhiteSpace(levelIdOverride)
            ? SceneManager.GetActiveScene().name
            : levelIdOverride;

        // Try to retrieve the saved data for this level
        LevelTimeEntry entry = LevelTimePersistence.GetLevelTime(levelId);

        // If no data exists yet (first time playing), show placeholders
        if (entry == null)
        {
            if (bestTimeText != null) bestTimeText.text = "PB: --:--.--";
            if (latestTimeText != null) latestTimeText.text = "Last: --:--.--";
            return;
        }

        // Apply the formatted time strings to the UI
        if (bestTimeText != null) bestTimeText.text = $"PB: {entry.bestFormatted}";
        if (latestTimeText != null) latestTimeText.text = $"Last: {entry.latestFormatted}";
    }
}
