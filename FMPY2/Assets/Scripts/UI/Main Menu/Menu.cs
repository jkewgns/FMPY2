using System.Collections;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject levelMenu;
    public GameObject optionsMenu;
    public GameObject replayMenu;

    [Header("Options Button Feedback")]
    public RectTransform optionsButton;
    public AudioSource sfxSource;
    public AudioClip noSfx;

    [Range(0f, 1f)] public float noSfxVolume = 1f; // Specific volume for the "denied" sound

    [Header("Shake Animation Settings")]
    [Range(0.05f, 0.5f)] public float shakeDuration = 0.18f;
    [Range(2f, 30f)] public float shakeMagnitude = 8f;

    private Coroutine shakeRoutine;

    void Start()
    {
        // Ensure the game starts on the Main Menu
        OpenMainMenu();
    }

    void Update()
    {
        // If Escape is pressed, go back to the Main Menu from any sub-menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (levelMenu.activeSelf || optionsMenu.activeSelf || replayMenu.activeSelf)
                OpenMainMenu();
        }
    }

    // --- Navigation Methods ---

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        levelMenu.SetActive(false);
        optionsMenu.SetActive(false);
        replayMenu.SetActive(false);
    }

    public void OpenLevels()
    {
        mainMenu.SetActive(false);
        levelMenu.SetActive(true);
        optionsMenu.SetActive(false);
        replayMenu.SetActive(false);
    }

    public void OpenOptions()
    {
        // Currently disabled: plays a "no" sound and shakes the button instead
        PlayNoFeedback(); 
    }

    public void OpenReplays()
    {
        mainMenu.SetActive(false);
        levelMenu.SetActive(false);
        optionsMenu.SetActive(false);
        replayMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Application quit.");
        Application.Quit();
    }

    // --- Juice/Feedback Logic ---

    private void PlayNoFeedback()
    {
        // Play the "denied" sound effect
        if (sfxSource != null && noSfx != null)
            sfxSource.PlayOneShot(noSfx, noSfxVolume);

        // Shake the button to show it's locked/unavailable
        if (optionsButton != null)
        {
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeUI(optionsButton));
        }
    }

    /// <summary>
    /// Coroutine that jitters the UI element randomly for a short duration.
    /// </summary>
    private IEnumerator ShakeUI(RectTransform target)
    {
        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            // Calculate a random offset within the magnitude range
            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            // Smaller vertical shake for a more natural "head shake" feel
            float y = Random.Range(-shakeMagnitude * 0.4f, shakeMagnitude * 0.4f);
            
            target.anchoredPosition = originalPos + new Vector2(x, y);

            // Use unscaledDeltaTime so the shake works even if the game is paused
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Return to the original position once the shake is done
        target.anchoredPosition = originalPos;
        shakeRoutine = null;
    }
}
