using UnityEngine;

public class Chair : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isSitting;
    [SerializeField] private Transform sitPos;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private float seatedLookSensitivity = 2f;
    [SerializeField] private float maxLookUpAngle = 30f; // Максимальный угол взгляда вверх
    [SerializeField] private float maxLookDownAngle = -15f; // Максимальный угол взгляда вниз
    [SerializeField] private float maxLookLeftAngle = 60f; // Максимальный угол поворота влево
    [SerializeField] private float maxLookRightAngle = -60f; // Максимальный угол поворота вправо

    private float originalMouseSensitivity;
    private bool originalCursorVisibility;

    public void Interact()
    {
        if (!isSitting)
        {
            // Начинаем сидение
            isSitting = true;

            // Сохраняем оригинальные настройки
            originalMouseSensitivity = firstPersonController.mouseSensitivity;
            originalCursorVisibility = Cursor.visible;

            // Настраиваем параметры для сидения
            firstPersonController.transform.position = sitPos.position;
            firstPersonController.SetMoveControl(false);
            firstPersonController.SetLookControl(true);
            firstPersonController.mouseSensitivity = seatedLookSensitivity;

            // Устанавливаем ограничения вращения камеры
            firstPersonController.SetCameraRotationLimits(
                maxLookRightAngle,
                maxLookLeftAngle,
                maxLookDownAngle,
                maxLookUpAngle);

            // Включаем курсор
            firstPersonController.SetCursorVisibility(true);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && isSitting)
        {
            // Заканчиваем сидение
            isSitting = false;

            // Восстанавливаем оригинальные настройки
            firstPersonController.mouseSensitivity = originalMouseSensitivity;
            firstPersonController.SetMoveControl(true);
            firstPersonController.ResetCameraRotationLimits();
            firstPersonController.SetCursorVisibility(originalCursorVisibility);

            // Перемещаем игрока за стул
            if (firstPersonController != null)
            {
                Vector3 newPosition = sitPos.position;
                newPosition.z += 2f;
                firstPersonController.transform.position = newPosition;
            }
        }
    }
}