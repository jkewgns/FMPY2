using UnityEngine;
using System.Collections;

public class TitleAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float startOffset = 600f; // How far up the title starts
    public float duration = 0.6f;    // Speed of the drop-in

    RectTransform rect;
    Vector2 targetPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        
        // Capture the desired final position from the Inspector
        targetPos = rect.anchoredPosition;
        
        // Move the title UP (Vector2.up) so it can drop down into place
        rect.anchoredPosition = targetPos + Vector2.up * startOffset;
    }

    void Start()
    {
        // Begin the drop-down animation
        StartCoroutine(MoveIn());
    }

    /// <summary>
    /// Animates the title from its high offset down to its target position.
    /// </summary>
    IEnumerator MoveIn()
    {
        float t = 0f;
        Vector2 startPos = rect.anchoredPosition;

        while (t < 1f)
        {
            // Normalize time (0 to 1) based on the duration
            t += Time.deltaTime / duration;

            // Interpolate position with a smooth ease-in/out curve
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            
            // Wait for the next frame
            yield return null;
        }

        // Snap to exact target position to prevent minor floating point offsets
        rect.anchoredPosition = targetPos;
    }
}
