using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    float elapsedTime;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        TimeSpan time = TimeSpan.FromSeconds(elapsedTime);

        // Formatting: 
        // hh = hours, mm = minutes, ss = seconds
        // fff = 3 decimal places (milliseconds)
        // ff = 2 decimal places
        timerText.text = time.ToString(@"mm\:ss\.ff");

        //for hours: timerText.text = time.ToString(@"hh\:mm\:ss\.ff");
    }
}
