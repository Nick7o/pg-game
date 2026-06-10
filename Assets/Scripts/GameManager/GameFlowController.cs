using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    [Header("Ekrany Koñcowe")]
    public GameObject winPanel;
    public GameObject losePanel;

    private void Awake()
    {
        Instance = this;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void TriggerWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void TriggerLose()
    {
        if (losePanel != null) losePanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        if (GameManager.Instance != null) GameManager.Instance.InitializeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Zamykanie gry...");
        Application.Quit();
    }
}