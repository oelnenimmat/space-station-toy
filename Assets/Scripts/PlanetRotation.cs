using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    public float speed;

    void Update()
    {
        transform.RotateAround(transform.position, transform.up, speed * Time.deltaTime);
    }
}
