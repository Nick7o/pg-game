using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private static HUD _instance;
    public static HUD Instance => _instance;

    [SerializeField] private GameObject _interactionPrompt;
    [SerializeField] private TMPro.TextMeshProUGUI _interactionPromptText;

    [SerializeField] private Slider _playerHealthBar;

    private void Awake()
    {
        _instance = this;

        ShowInteractionPrompt(false, string.Empty);
    }

    public void ShowInteractionPrompt(bool show, string prompt)
    {
        if (_interactionPrompt != null)
            _interactionPrompt.SetActive(show);

        if (_interactionPromptText != null)
            _interactionPromptText.text = prompt;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (_playerHealthBar != null)
        {
            _playerHealthBar.maxValue = maxHealth;
            _playerHealthBar.value = currentHealth;
        }
    }
}
