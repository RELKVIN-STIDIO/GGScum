using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sitting : FirstPersonLook
{
 
    void Sit()
    {
        velocity.x = Mathf.Clamp(velocity.x, -90, 90);
    }
   
}
