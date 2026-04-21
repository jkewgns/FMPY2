using UnityEngine;
using System.Collections;

public class UIAnimate : MonoBehaviour
{
    public float startOffset = 600f;
    public float duration = 0.6f;

    RectTransform rect;
    Vector2 targetPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        targetPos = rect.anchoredPosition;
        rect.anchoredPosition = targetPos + Vector2.right * startOffset;
    }

    void Start()
    {
        StartCoroutine(MoveIn());
    }

    IEnumerator MoveIn()
    {
        float t = 0f;
        Vector2 startPos = rect.anchoredPosition;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }
}