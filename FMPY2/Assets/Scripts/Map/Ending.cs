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
    public Image whiteFadeImage;      // Fullscreen UI image for the fade-to-white
    public TextMeshProUGUI timerText; // The timer UI to animate/highlight
    public float transitionTime = 1.5f;
    public float scaleMultiplier = 3.0f; // How much the timer grows when you finish
    public float waitBeforeLoad = 2.5f;   // Pause at the white screen before next level

    private SpriteRenderer spriteRenderer;
    private bool isTriggered = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize the fade image to be invisible and disabled
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
        // Creates a constant "glowing" effect by oscillating the color
        float t = Mathf.PingPong(Time.time * speed, 1.0f);
        spriteRenderer.color = Color.Lerp(startColor, pulseColor, t);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Trigger the end sequence only once when the player touches the goal
        if (collision.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(EndSequence());
        }
    }

    /// <summary>
    /// Handles stopping the clock, saving data, and performing the UI transition.
    /// </summary>
    IEnumerator EndSequence()
    {
        // 1. Logic: Stop the timer and save the result
        Timer timerScript = FindFirstObjectByType<Timer>();
        if (timerScript != null)
        {
            timerScript.StopTimer();

            string levelId = SceneManager.GetActiveScene().name;
            LevelTimePersistence.SaveLevelTime(levelId, timerScript.ElapsedSeconds);
        }

        // 2. Setup UI transition
        if (whiteFadeImage != null)
            whiteFadeImage.gameObject.SetActive(true);

        RectTransform rect = timerText != null ? timerText.GetComponent<RectTransform>() : null;
        Vector2 startPos = rect != null ? rect.anchoredPosition : Vector2.zero;
        Vector3 startScale = rect != null ? rect.localScale : Vector3.one;

        // Position where the timer will move to (e.g., center of screen)
        Vector2 targetPos = new Vector2(-200f, 145f);
        Vector3 targetScale = startScale * scaleMultiplier;

        float elapsed = 0f;

        // 3. Animation Loop: Fade to white and move/scale the timer text
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);
            float curve = Mathf.SmoothStep(0f, 1f, t); // Smooth easing

            if (rect != null)
            {
                rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curve);
                rect.localScale = Vector3.Lerp(startScale, targetScale, curve);
            }

            if (whiteFadeImage != null)
            {
                Color c = whiteFadeImage.color;
                c.a = curve; // Increase alpha over time
                whiteFadeImage.color = c;
            }

            yield return null;
        }

        // 4. Wait at the final state before moving to the next scene
        yield return new WaitForSeconds(waitBeforeLoad);
        
        // Load the next level in the Build Settings index
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
