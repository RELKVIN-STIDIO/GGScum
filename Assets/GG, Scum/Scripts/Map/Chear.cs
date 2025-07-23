using UnityEngine;

public class Chair : MonoBehaviour
{
    public bool IsSitting;
    public Transform SitPos;
    private FirstPersonMovement Fpm;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !IsSitting)
        {
            IsSitting = true;
            other.transform.position = SitPos.position;
            Fpm = other.GetComponent<FirstPersonMovement>();

            if (Fpm != null)
            {
                Fpm.enabled = false;
                Fpm.rigidbody.isKinematic = true;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && IsSitting)
        {
            IsSitting = false;

            if (Fpm != null)
            {
                Fpm.enabled = true;
                Fpm.rigidbody.isKinematic = false;

                // Calculate new position with offset
                Vector3 newPosition = SitPos.position;
                newPosition.z += 2f;
                Fpm.transform.position = newPosition;
            }
        }
    }
}