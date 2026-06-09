using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HangmanManager : MonoBehaviour
{
    [Header("UI Kontenery")]
    public Transform wordContainer;
    public Transform row1, row2, row3;
    public TMP_Text timerText;
    public TMP_Text signatureText;

    [Header("Prefaby")]
    public GameObject wordLetterPrefab;
    public GameObject coinButtonPrefab;

    [Header("Ustawienia Gry")]
    public int letterCost = 50;
    public string playerName = "Cpt. Boris";

    private class WordSlot
    {
        public char targetChar;
        public TMP_Text letterText;
        public bool isSpace; 
    }
    private List<WordSlot> wordSlots = new List<WordSlot>();
    private List<GameObject> spawnedCoins = new List<GameObject>();

    private Color boughtLetterColor;
    private Color typedLetterColor;
    private Color coinBaseColor;
    private Color coinCorrectColor;
    private Color coinWrongColor;
    private Color wrongSignatureColor;

    private string currentGuess = "";
    private GameObject currentPlayer;
    private bool skipFirstFrame = false;

    void Awake()
    {
        ColorUtility.TryParseHtmlString("#A67B00", out boughtLetterColor);
        ColorUtility.TryParseHtmlString("#000000", out typedLetterColor);
        ColorUtility.TryParseHtmlString("#433A18", out coinBaseColor);
        ColorUtility.TryParseHtmlString("#2B4318", out coinCorrectColor);
        ColorUtility.TryParseHtmlString("#431818", out coinWrongColor);
        ColorUtility.TryParseHtmlString("#AE0000", out wrongSignatureColor);
    }


    void OnEnable()
    {
        Keyboard.current.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        Keyboard.current.onTextInput -= OnTextInput;
    }

    public void OpenScreen(GameObject player)
    {
        currentPlayer = player;

        if (currentPlayer != null)
        {
            PlayerController2D playerController = currentPlayer.GetComponentInParent<PlayerController2D>();
            if (playerController == null) playerController = currentPlayer.GetComponentInChildren<PlayerController2D>();

            if (playerController != null)
            {
                playerController.enabled = false;

                Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
            else
            {
                Debug.LogWarning("Mened¿er nie znalaz³ PlayerController2D na przekazanym obiekcie gracza!");
            }

            InteractionController interactionCtrl = currentPlayer.GetComponentInParent<InteractionController>();
            if (interactionCtrl == null) interactionCtrl = currentPlayer.GetComponentInChildren<InteractionController>();
            if (interactionCtrl != null) interactionCtrl.enabled = false;
        }

        skipFirstFrame = true;
        gameObject.SetActive(true);
        RefreshBoard();
    }

    void Update()
    {
        if (skipFirstFrame) { skipFirstFrame = false; return; }

        UpdateTimerUI();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseScreen();
            return;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            SubmitFullGuess();
            return;
        }
    }


    private void OnTextInput(char c)
    {
        if (!gameObject.activeInHierarchy || skipFirstFrame) return;

        if (c == '\b') // Backspace
        {
            if (currentGuess.Length > 0) currentGuess = currentGuess.Substring(0, currentGuess.Length - 1);
            signatureText.text = "";
        }
        else if (char.IsLetter(c))
        {
            int maxTypable = 0;
            string targetNoSpaces = GameManager.Instance.currentWord.Replace(" ", "");
            foreach (char t in targetNoSpaces)
            {
                if (!GameManager.Instance.unlockedLetters.Contains(t)) maxTypable++;
            }

            if (currentGuess.Length < maxTypable)
            {
                currentGuess += char.ToUpper(c);
                signatureText.text = "";
            }
        }
        UpdateBoardVisuals();
    }

    public void RefreshBoard()
    {
        currentGuess = "";
        signatureText.text = "";

        GenerateWordSlots();
        GenerateKeyboard();
        UpdateBoardVisuals();
    }

    private void GenerateWordSlots()
    {
        foreach (Transform child in wordContainer) Destroy(child.gameObject);
        wordSlots.Clear();

        string[] words = GameManager.Instance.currentWord.Split(' ');

        foreach (string word in words)
        {
            GameObject wordRow = new GameObject("WordRow");
            wordRow.transform.SetParent(wordContainer, false);

            HorizontalLayoutGroup hlg = wordRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;

            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 15; 

            ContentSizeFitter csf = wordRow.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (char c in word)
            {
                GameObject newSlotObj = Instantiate(wordLetterPrefab, wordRow.transform);
                TMP_Text parentUnderscore = newSlotObj.GetComponent<TMP_Text>();
                TMP_Text childLetter = newSlotObj.transform.GetChild(0).GetComponent<TMP_Text>();

                parentUnderscore.text = "_";
                childLetter.text = "";

                wordSlots.Add(new WordSlot { targetChar = c, letterText = childLetter, isSpace = false });
            }
        }
    }

    private void GenerateKeyboard()
    {
        foreach (GameObject coin in spawnedCoins) Destroy(coin);
        spawnedCoins.Clear();

        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        for (int i = 0; i < alphabet.Length; i++)
        {
            Transform targetRow = (i < 9) ? row1 : ((i < 18) ? row2 : row3);
            GameObject coin = Instantiate(coinButtonPrefab, targetRow);
            spawnedCoins.Add(coin);

            char letter = alphabet[i];
            TMP_Text letterText = coin.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text priceText = coin.transform.GetChild(1).GetComponent<TMP_Text>();

            letterText.text = letter.ToString();
            priceText.text = letterCost.ToString();

            if (GameManager.Instance.unlockedLetters.Contains(letter))
            {
                bool isCorrect = GameManager.Instance.currentWord.Contains(letter.ToString());
                letterText.color = isCorrect ? coinCorrectColor : coinWrongColor;
                priceText.color = isCorrect ? coinCorrectColor : coinWrongColor;
            }
            else
            {
                letterText.color = coinBaseColor;
                priceText.color = coinBaseColor;
            }

            Button btn = coin.GetComponent<Button>();
            btn.onClick.AddListener(() => OnCoinClicked(letter, letterText, priceText));
        }
    }

    private void OnCoinClicked(char letter, TMP_Text lText, TMP_Text pText)
    {
        if (GameManager.Instance.unlockedLetters.Contains(letter)) return;

        if (GameManager.Instance.SpendGold(letterCost))
        {
            GameManager.Instance.unlockedLetters.Add(letter);
            bool isCorrect = GameManager.Instance.currentWord.Contains(letter.ToString());

            lText.color = isCorrect ? coinCorrectColor : coinWrongColor;
            pText.color = isCorrect ? coinCorrectColor : coinWrongColor;

            UpdateBoardVisuals();
        }
    }

    private void UpdateBoardVisuals()
    {
        int typedIndex = 0; 

        for (int i = 0; i < wordSlots.Count; i++)
        {
            char target = wordSlots[i].targetChar;

            if (GameManager.Instance.unlockedLetters.Contains(target))
            {
                wordSlots[i].letterText.text = target.ToString();
                wordSlots[i].letterText.color = boughtLetterColor;
            }
            else
            {
                if (typedIndex < currentGuess.Length)
                {
                    wordSlots[i].letterText.text = currentGuess[typedIndex].ToString();
                    wordSlots[i].letterText.color = typedLetterColor;
                    typedIndex++;
                }
                else
                {
                    wordSlots[i].letterText.text = ""; 
                }
            }
        }
    }

    public void SubmitFullGuess()
    {
        signatureText.text = playerName;

        string finalAttempt = "";
        int typedIdx = 0;
        string targetNoSpaces = GameManager.Instance.currentWord.Replace(" ", "");

        foreach (char targetChar in targetNoSpaces)
        {
            if (GameManager.Instance.unlockedLetters.Contains(targetChar))
            {
                finalAttempt += targetChar;
            }
            else
            {
                if (typedIdx < currentGuess.Length)
                {
                    finalAttempt += currentGuess[typedIdx]; 
                    typedIdx++;
                }
                else
                {
                    break;
                }
            }
        }

        if (finalAttempt == targetNoSpaces)
        {
            Debug.Log("WYGRANA! Prawid³owe has³o!");
            signatureText.color = boughtLetterColor; 
        }
        else
        {
            Debug.Log("Z£E HAS£O! Kara czasowa!");
            signatureText.color = wrongSignatureColor; 
            GameManager.Instance.ApplyTimePenalty(120f);

            currentGuess = ""; 
            UpdateBoardVisuals();
        }
    }

    void UpdateTimerUI()
    {
        float time = GameManager.Instance.timeRemaining;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void CloseScreen()
    {
        if (currentPlayer != null)
        {
            PlayerController2D playerController = currentPlayer.GetComponentInParent<PlayerController2D>();
            if (playerController == null) playerController = currentPlayer.GetComponentInChildren<PlayerController2D>();
            if (playerController != null) playerController.enabled = true;

            InteractionController interactionCtrl = currentPlayer.GetComponentInParent<InteractionController>();
            if (interactionCtrl == null) interactionCtrl = currentPlayer.GetComponentInChildren<InteractionController>();
            if (interactionCtrl != null) interactionCtrl.enabled = true;
        }

        gameObject.SetActive(false);
    }
}