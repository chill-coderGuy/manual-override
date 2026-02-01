using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu"; 

  
    public void ActivateGameOver()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}