using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    [Header("Ekrany Koñcowe")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("System Œmierci i Respawnu")]
    public CanvasGroup deathFadePanel;
    public Transform playerRespawnPoint;
    public Transform shipRespawnPoint;

    [Tooltip("Procent traconego z³ota. 1.0 to 100%, 0.5 to 50%")]
    [Range(0f, 1f)] public float goldLossPercentage = 1.0f;
    public float fadeDuration = 8.0f;

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

    public void TriggerPlayerDeath()
    {
        StartCoroutine(DeathSequence());
    }

    private System.Collections.IEnumerator DeathSequence()
    {
        if (Player.Instance != null) Player.Instance.Controller.enabled = false;

        deathFadePanel.blocksRaycasts = true; 
        float t = 0;
        
        deathFadePanel.alpha = 1;

        if (GameManager.Instance != null && GameManager.Instance.playerGold > 0)
        {
            int goldToLose = Mathf.RoundToInt(GameManager.Instance.playerGold * goldLossPercentage);
            GameManager.Instance.SpendGold(goldToLose);
            Debug.Log($"Œmieræ! Utracono {goldToLose} z³ota.");
        }

        if (Player.Instance != null)
        {
            Player.Instance.SetState(PlayerState.Player);

            Player.Instance.transform.position = playerRespawnPoint.position;

            if (Player.Instance.Ship != null && shipRespawnPoint != null)
            {
                Player.Instance.Ship.transform.position = shipRespawnPoint.position;
                Player.Instance.Ship.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Zerujemy pêd statku!
            }

            Player.Instance.Heal(100f);
            Player.Instance.ResetDeathState();
        }

        yield return new WaitForSecondsRealtime(3.0f);

        t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            deathFadePanel.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        deathFadePanel.alpha = 0;
        deathFadePanel.blocksRaycasts = false;

        if (Player.Instance != null) Player.Instance.Controller.enabled = true;
    }
}