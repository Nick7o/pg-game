using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    //globalny dostêp z ka¿dego miejsca poprzez GameManager.Instance
    public static GameManager Instance { get; private set; }

    [Header("Zasoby Gracza")]
    public int playerGold = 0;
    public float timeRemaining = 900f; // 15 minut w sekundach 

    [Header("Stan Zagadki (Wisielec)")]
    public string currentWord;
    public List<char> unlockedLetters = new List<char>();

    // Pula hasel
    private string[] wordPool = {
        "DEAD MEN TELL NO TALES",
        "WALK THE PLANK",
        "PIECES OF EIGHT",
        "SHIVER ME TIMBERS",
        "DAVY JONES LOCKER",
        "BATTEN DOWN THE HATCHES",
        "LOOSE LIPS SINK SHIPS",
        "SAIL THE SEVEN SEAS",
        "X MARKS THE SPOT",
        "NO PREY NO PAY",
        "CALM BEFORE THE STORM",
        "JOLLY ROGER",
        "BURIED IN THE SAND",
        "A PIRATES LIFE FOR ME",
        "WHY IS THE RUM GONE",
        "HOIST THE COLORS",
        "BRING ME THAT HORIZON",
        "CURSE OF THE BLACK PEARL",
        "PIRATES OF THE CARIBBEAN",
        "NOT ALL TREASURE IS GOLD"
    };

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Globalne odliczanie czasu
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else if (timeRemaining < 0 || timeRemaining == 0)
        {
            timeRemaining = 0;
            Debug.Log("PRZEGRANA - koniec czasu");
            // Tutaj w przysz³osci mo¿na wywo³ac funkcjê przegranej (Koniec Czasu)
        }
    }

    public void InitializeGame()
    {
        playerGold = 100; // TEST potem zmienic na 0
        timeRemaining = 900f;

        // Losowanie hasla i zamiana na wielkie litery dla pewnoœci
        currentWord = wordPool[Random.Range(0, wordPool.Length)].ToUpper();
        unlockedLetters.Clear();
    }

    // --- Funkcje pomocnicze dla innych skryptów ---

    public void AddGold(int amount)
    {
        playerGold += amount;
    }

    // Funkcja zwraca 'true' jeœli gracza staæ i pobrano z³oto, w przeciwnym razie 'false'
    public bool SpendGold(int amount)
    {
        if (playerGold >= amount)
        {
            playerGold -= amount;
            return true;
        }
        return false;
    }

    // Nak³ada karê czasow¹ za b³êdne zgadniêcie ca³ego has³a
    public void ApplyTimePenalty(float secondsToSubtract)
    {
        timeRemaining -= secondsToSubtract;
        if (timeRemaining < 0) timeRemaining = 0;
    }
}