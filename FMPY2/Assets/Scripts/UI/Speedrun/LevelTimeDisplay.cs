using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTimeDisplay : MonoBehaviour
{
    [SerializeField] private string levelIdOverride;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private TextMeshProUGUI latestTimeText;

    void OnEnable()
    {
        LevelTimePersistence.OnLevelTimeUpdated += HandleLevelTimeUpdated;
        Refresh();
    }

    void OnDisable()
    {
        LevelTimePersistence.OnLevelTimeUpdated -= HandleLevelTimeUpdated;
    }

    private void HandleLevelTimeUpdated(string updatedLevelId, LevelTimeEntry entry)
    {
        string thisLevelId = string.IsNullOrWhiteSpace(levelIdOverride)
            ? SceneManager.GetActiveScene().name
            : levelIdOverride;

        if (updatedLevelId == thisLevelId)
            Refresh();
    }

    public void Refresh()
    {
        string levelId = string.IsNullOrWhiteSpace(levelIdOverride)
            ? SceneManager.GetActiveScene().name
            : levelIdOverride;

        LevelTimeEntry entry = LevelTimePersistence.GetLevelTime(levelId);

        if (entry == null)
        {
            if (bestTimeText != null) bestTimeText.text = "PB: --:--.--";
            if (latestTimeText != null) latestTimeText.text = "Last: --:--.--";
            return;
        }

        if (bestTimeText != null) bestTimeText.text = $"PB: {entry.bestFormatted}";
        if (latestTimeText != null) latestTimeText.text = $"Last: {entry.latestFormatted}";
    }
}