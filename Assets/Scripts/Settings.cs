using UnityEngine;
using UnityEngine.SceneManagement;


public class Settings : MonoBehaviour
{
        public string mainmenu="Main Menu";

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainmenu);
    }
}
