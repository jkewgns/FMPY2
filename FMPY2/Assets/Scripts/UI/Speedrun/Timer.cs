using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    public string FinalTime { get; private set; } = "00:00.00";
    public float ElapsedSeconds => elapsedTime;

    private float elapsedTime;
    private bool isStopped;

    void Update()
    {
        if (isStopped) return;

        elapsedTime += Time.deltaTime;
        FinalTime = FormatTime(elapsedTime);

        if (timerText != null)
            timerText.text = FinalTime;
    }

    public void StopTimer()
    {
        if (isStopped) return;
        isStopped = true;
        FinalTime = FormatTime(elapsedTime);
    }

    public static string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return time.ToString(@"mm\:ss\.ff");
    }
}