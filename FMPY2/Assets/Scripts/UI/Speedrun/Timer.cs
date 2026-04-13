using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    public string FinalTime { get; private set; } // Added this
    float elapsedTime;
    bool isStopped = false; // Added this

    void Update()
    {
        if (isStopped) return;

        elapsedTime += Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
        FinalTime = time.ToString(@"mm\:ss\.ff");
        timerText.text = FinalTime;
    }

    public void StopTimer() => isStopped = true;
}
