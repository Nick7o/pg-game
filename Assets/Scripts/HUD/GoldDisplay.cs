using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class GoldDisplay : MonoBehaviour
{
    private TMP_Text goldText;

    void Awake()
    {
        goldText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            goldText.text = GameManager.Instance.playerGold.ToString();
        }
    }
}