using UnityEngine;

public class Chair : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isSitting;
    [SerializeField] private Transform sitPos;
    PlayerController playerController = PlayerController.Instance;
    [SerializeField] private float seatedLookSensitivity = 3f;
    [SerializeField] private float maxLookUpAngle = 20f; // Максимальный угол взгляда вверх
    [SerializeField] private float maxLookDownAngle = -10f; // Максимальный угол взгляда вниз
    [SerializeField] private float maxLookLeftAngle = 260f; // Максимальный угол поворота влево
    [SerializeField] private float maxLookRightAngle = -280f; // Максимальный угол поворота вправо

    private float originalMouseSensitivity;

    public void Interact()
    {
        if (!isSitting)
        {
            isSitting = true;

            originalMouseSensitivity = playerController.mouseSensitivity;

            playerController.transform.position = sitPos.position;
            playerController.SetMoveControl(false);
            playerController.SetLookControl(true);
            playerController.mouseSensitivity = seatedLookSensitivity;

            playerController.SetCameraRotationLimits(
                maxLookRightAngle,
                maxLookLeftAngle,
                maxLookDownAngle,
                maxLookUpAngle);

            playerController.SetCursorVisibility(true);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && isSitting)
        {
            isSitting = false;

            playerController.mouseSensitivity = originalMouseSensitivity;
            playerController.SetMoveControl(true);
            playerController.ResetCameraRotationLimits();
            playerController.SetCursorVisibility(false);

            if (playerController != null)
            {
                Vector3 newPosition = sitPos.position;
                newPosition.z += 2f;
                playerController.transform.position = newPosition;
            }
        }
    }
}