using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MonitorController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image fakeCursor; // Перетащите сюда FakeCursor из редактора
    [SerializeField] private RectTransform monitorRect; // Область монитора
    [SerializeField] private Camera monitorCamera; // Камера, если монитор в 3D-пространстве

    private bool isCursorOnMonitor = false;

    private void Update()
    {
        if (isCursorOnMonitor)
        {
            // Получаем позицию мыши относительно Canvas
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                monitorRect,
                Input.mousePosition,
                monitorCamera,
                out localPoint
            );

            // Обновляем позицию фейкового курсора
            fakeCursor.rectTransform.anchoredPosition = localPoint;

            // Блокируем настоящий курсор
            Cursor.visible = false;
            fakeCursor.gameObject.SetActive(true);
        }
        else
        {
            // Возвращаем настоящий курсор
            Cursor.visible = true;
            fakeCursor.gameObject.SetActive(false);
        }
    }

    // Когда курсор входит в зону монитора
    public void OnPointerEnter(PointerEventData eventData)
    {
        isCursorOnMonitor = true;
    }

    // Когда курсор выходит из зоны монитора
    public void OnPointerExit(PointerEventData eventData)
    {
        isCursorOnMonitor = false;
    }
}