using UnityEngine;
using UnityEngine.EventSystems;

public class WindowUI : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private RectTransform canvasRect;
    private Vector2 mouseOffset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Вычисляем смещение курсора относительно центра объекта
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out mouseOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            // Учитываем смещение курсора и ограничиваем позицию в пределах Canvas
            rectTransform.localPosition = ClampToCanvas(localPoint - mouseOffset);
        }
    }

    private Vector2 ClampToCanvas(Vector2 position)
    {
        // Получаем размеры объекта и Canvas
        Vector2 size = rectTransform.sizeDelta * canvas.scaleFactor;
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Ограничиваем позицию, чтобы объект не выходил за границы Canvas
        float xMin = -canvasSize.x * 0.5f + size.x * 0.5f;
        float xMax = canvasSize.x * 0.5f - size.x * 0.5f;
        float yMin = -canvasSize.y * 0.5f + size.y * 0.5f;
        float yMax = canvasSize.y * 0.5f - size.y * 0.5f;

        position.x = Mathf.Clamp(position.x, xMin, xMax);
        position.y = Mathf.Clamp(position.y, yMin, yMax);

        return position;
    }
}