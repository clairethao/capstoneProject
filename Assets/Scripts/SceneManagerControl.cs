using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerControl : MonoBehaviour
{
    public void CreateNew()
    {
        SceneManager.LoadScene("SetUpScene");
    }

    public void Continue()
    {
        SceneManager.LoadScene("ContinueScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
