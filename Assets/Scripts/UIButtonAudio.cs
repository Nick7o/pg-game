using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private AudioCue _clickSound;
    [SerializeField] private AudioCue _hoverSound;
    [SerializeField] private bool _playClickWhenButtonDisabled = false;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        _button.onClick.AddListener(PlayClickSound);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(PlayClickSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hoverSound != null && (_button == null || _button.interactable))
            _hoverSound.PlayAt(transform.position);
    }

    private void PlayClickSound()
    {
        if (_clickSound == null)
            return;

        if (_button != null && !_button.interactable && !_playClickWhenButtonDisabled)
            return;

        _clickSound.PlayAt(transform.position);
    }
}
