using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    public Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    
    }

    // Перетаскивание окна
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.transform.localScale;
    }

    // Делаем окно активным при клике
    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling(); // Перемещаем поверх других окон
    }
}