using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionLightotator : MonoBehaviour
{
    public Transform mainLightTransform;

    void Update()
    {
        transform.forward = -mainLightTransform.forward;
    }

}
