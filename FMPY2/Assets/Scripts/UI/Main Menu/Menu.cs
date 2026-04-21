using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject levelMenu;
    public GameObject optionsMenu;
    public GameObject replayMenu;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OpenMainMenu();
    }

    void Update()
    {
        if(levelMenu == true && Input.GetKey(KeyCode.Escape))
        {
            OpenMainMenu();
        }

        if(optionsMenu == true && Input.GetKey(KeyCode.Escape))
        {
            OpenMainMenu();
        }

        if(replayMenu == true && Input.GetKey(KeyCode.Escape))
        {
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
        mainMenu.SetActive(false);
        levelMenu.SetActive(false);
        optionsMenu.SetActive(true);
        replayMenu.SetActive(false);
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
}
