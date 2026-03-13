using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UIHoverSfxCaller : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null && !selectable.IsInteractable()) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectable != null && !selectable.IsInteractable()) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick();
    }
}