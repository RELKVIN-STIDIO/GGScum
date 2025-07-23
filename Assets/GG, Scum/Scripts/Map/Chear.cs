using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chear : MonoBehaviour
{
    public bool IsSitting;
    public Transform SitPos;
    FirstPersonMovement Fpm;
    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player" && !IsSitting)
        {
            IsSitting = true;
            other.transform.position = SitPos.position;
            FirstPersonMovement Fpm = other.GetComponent<FirstPersonMovement>();
            Fpm.enabled = false;
            Fpm.rigidbody.isKinematic = true;

        }

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && IsSitting)
        {
            IsSitting = false;
            Fpm.enabled = true;
            Fpm.rigidbody.isKinematic = false;
        }
    }
}
