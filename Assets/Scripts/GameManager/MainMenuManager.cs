using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeGame();
        }

        SceneManager.LoadScene("MainScene");
    }

    public void QuitGame()
    {
        Debug.Log("Zamykanie gry...");
        Application.Quit(); 
    }
}