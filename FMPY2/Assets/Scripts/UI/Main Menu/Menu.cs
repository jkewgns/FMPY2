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

    [Range(0f, 1f)] public float noSfxVolume = 1f; // volume only for the "no" sound

    [Range(0.05f, 0.5f)] public float shakeDuration = 0.18f;
    [Range(2f, 30f)] public float shakeMagnitude = 8f;

    private Coroutine shakeRoutine;

    void Start()
    {
        OpenMainMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (levelMenu.activeSelf || optionsMenu.activeSelf || replayMenu.activeSelf)
                OpenMainMenu();
        }
    }

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
        PlayNoFeedback(); // don't open options
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

    private void PlayNoFeedback()
    {
        if (sfxSource != null && noSfx != null)
            sfxSource.PlayOneShot(noSfx, noSfxVolume); // <- uses your variable

        if (optionsButton != null)
        {
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeUI(optionsButton));
        }
    }

    private IEnumerator ShakeUI(RectTransform target)
    {
        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude * 0.4f, shakeMagnitude * 0.4f);
            target.anchoredPosition = originalPos + new Vector2(x, y);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        target.anchoredPosition = originalPos;
        shakeRoutine = null;
    }
}