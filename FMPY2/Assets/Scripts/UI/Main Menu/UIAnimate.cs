using UnityEngine;
using System.Collections;

public class UIAnimate : MonoBehaviour
{
    [Header("Animation Settings")]
    public float startOffset = 600f; // How far to the right the element starts
    public float duration = 0.6f;    // How long the slide-in takes

    RectTransform rect;
    Vector2 targetPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        
        // Save the current position (set in the Editor) as the final destination
        targetPos = rect.anchoredPosition;
        
        // Offset the element immediately so it starts off-screen or away from center
        rect.anchoredPosition = targetPos + Vector2.right * startOffset;
    }

    void Start()
    {
        // Kick off the slide-in effect as soon as the object is initialized
        StartCoroutine(MoveIn());
    }

    /// <summary>
    /// Smoothly interpolates the UI element from its offset position back to targetPos.
    /// </summary>
    IEnumerator MoveIn()
    {
        float t = 0f;
        Vector2 startPos = rect.anchoredPosition;

        while (t < 1f)
        {
            // Increase t based on time passed relative to the desired duration
            t += Time.deltaTime / duration;

            // Mathf.SmoothStep adds an "ease-in-out" feel (starts slow, ends slow)
            // Vector2.Lerp moves the object between the two points
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            
            // Wait until the next frame before continuing the loop
            yield return null;
        }

        // Ensure the element snaps exactly to the target position at the end
        rect.anchoredPosition = targetPos;
    }
}
