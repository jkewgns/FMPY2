using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Ending : MonoBehaviour
{
    [Header("Pulse Settings")]
    public Color startColor = Color.green;
    public Color pulseColor = Color.white;
    public float speed = 2.0f;

    [Header("Transition Settings")]
    public Image whiteFadeImage;
    public TextMeshProUGUI timerText;
    public float transitionTime = 1.5f;
    public float scaleMultiplier = 3.0f;
    public float waitBeforeLoad = 2.5f;

    private SpriteRenderer spriteRenderer;
    private bool isTriggered = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (whiteFadeImage != null)
        {
            whiteFadeImage.gameObject.SetActive(false);
            Color c = whiteFadeImage.color;
            c.a = 0f;
            whiteFadeImage.color = c;
        }
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1.0f);
        spriteRenderer.color = Color.Lerp(startColor, pulseColor, t);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(EndSequence());
        }
    }

    IEnumerator EndSequence()
    {
        Timer timerScript = FindFirstObjectByType<Timer>();
        if (timerScript != null)
        {
            timerScript.StopTimer();

            string levelId = SceneManager.GetActiveScene().name;
            LevelTimePersistence.SaveLevelTime(levelId, timerScript.ElapsedSeconds);
        }

        if (whiteFadeImage != null)
            whiteFadeImage.gameObject.SetActive(true);

        RectTransform rect = timerText != null ? timerText.GetComponent<RectTransform>() : null;

        Vector2 startPos = rect != null ? rect.anchoredPosition : Vector2.zero;
        Vector3 startScale = rect != null ? rect.localScale : Vector3.one;

        Vector2 targetPos = new Vector2(-200f, 145f);
        Vector3 targetScale = startScale * scaleMultiplier;

        float elapsed = 0f;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);
            float curve = Mathf.SmoothStep(0f, 1f, t);

            if (rect != null)
            {
                rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curve);
                rect.localScale = Vector3.Lerp(startScale, targetScale, curve);
            }

            if (whiteFadeImage != null)
            {
                Color c = whiteFadeImage.color;
                c.a = curve;
                whiteFadeImage.color = c;
            }

            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeLoad);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}