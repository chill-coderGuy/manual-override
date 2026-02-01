using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [Header("Scene Names")]
    public string tutorialSceneName = "TutorialLevel";
    public string gameSceneName = "GameLevel";
    public string settingsscene="settings";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void PlayTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void ToggleSettings()
    {
        SceneManager.LoadScene(settingsscene);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }
}