using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable _currentIntaractable;
    private Outline _currentOutline;

    void Update()
    {
        CheckForInteractable();

        if (_currentIntaractable != null && Input.GetKeyDown(KeyCode.E))
        {
            _currentIntaractable.Interact();
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(playerController.playerCamera.position, playerController.playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (_currentIntaractable != interactable)
                {
                    ResetOutline();
                    _currentIntaractable= interactable;
                    _currentOutline = hit.collider.GetComponent<Outline>();

                    if (_currentOutline != null)
                    {
                        _currentOutline.enabled = true;
                    }
                }
                return;
            }
        }
        ResetOutline();
    }

    private void ResetOutline()
    {
        if (_currentOutline != null)
        {
            _currentOutline.enabled = false;
        }
        _currentOutline = null;
        _currentIntaractable = null;
    }
}
