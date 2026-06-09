using System;
using Unity.VisualScripting;
using UnityEngine;

public class HUD : MonoBehaviour
{
    private static HUD _instance;
    public static HUD Instance => _instance;

    [SerializeField] private GameObject _interactionPrompt;
    [SerializeField] private TMPro.TextMeshProUGUI _interactionPromptText;

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
}
