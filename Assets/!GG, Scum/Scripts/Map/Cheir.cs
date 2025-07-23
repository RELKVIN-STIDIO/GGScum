using UnityEngine;

public class Cheir : MonoBehaviour
{
    public bool IsSitting;
    public Transform SitPos;
    private FirstPersonController Fpc;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !IsSitting)
        {
            IsSitting = true;
            other.transform.position = SitPos.position;
            Fpc = other.GetComponent<FirstPersonController>();

            if (Fpc != null)
            {
                Fpc.CanMove = false;
             
                //Fpc.rigidbody.isKinematic = true;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && IsSitting)
        {
            IsSitting = false;

            if (Fpc != null)
            {
             
                Fpc.CanMove = true;

                // Calculate new position with offset
                Vector3 newPosition = SitPos.position;
                newPosition.z += 2f;
                Fpc.transform.position = newPosition;
            }
        }
    }
}