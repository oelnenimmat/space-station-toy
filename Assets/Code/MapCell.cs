using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCell : MonoBehaviour
{
    public Vector3Int coords;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
