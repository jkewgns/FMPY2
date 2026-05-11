using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;

    // Public properties to access the time from other scripts (e.g., a results screen)
    public string FinalTime { get; private set; } = "00:00.00";
    public float ElapsedSeconds => elapsedTime;

    private float elapsedTime;
    private bool isStopped;

    void Update()
    {
        // Stop updating if the timer is paused or finished
        if (isStopped) return;

        // Increment time based on the interval between frames
        elapsedTime += Time.deltaTime;
        
        // Convert the float seconds into a readable string format
        FinalTime = FormatTime(elapsedTime);

        // Update the UI text if a reference is assigned
        if (timerText != null)
            timerText.text = FinalTime;
    }

    /// <summary>
    /// Halts the timer and captures the final elapsed time.
    /// </summary>
    public void StopTimer()
    {
        if (isStopped) return;
        isStopped = true;
        
        // Final timestamp ensures the string is accurate to the exact stop moment
        FinalTime = FormatTime(elapsedTime);
    }

    /// <summary>
    /// Formats a float of seconds into a standard MM:SS.FF string.
    /// </summary>
    public static string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        // Using @mm\:ss\.ff for Minutes:Seconds.Milliseconds(2 digits)
        return time.ToString(@"mm\:ss\.ff");
    }
}
