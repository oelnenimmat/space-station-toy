using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ModuleCreatorPrototype : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public int priority;
    public ModuleRotations rotations = ModuleRotations.All;
    public DirectionFlags insidesWithoutSocket;

    public void OnValidate()
    {
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = material;

        if (mesh != null)
        {
            name = mesh.name;
        }
        else
        {
            name = "MISSING MESH";
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.15f, 0.15f, 1.0f);

        void DrawIf(DirectionFlags flag)
        {
            if ((insidesWithoutSocket & flag) != 0)
            {
                Gizmos.DrawCube(transform.position + 0.5f * flag.ToVector3(), new Vector3(0.15f, 0.15f, 0.15f));
            }
        }

        DrawIf(DirectionFlags.Left);
        DrawIf(DirectionFlags.Right);
        DrawIf(DirectionFlags.Down);
        DrawIf(DirectionFlags.Up);
        DrawIf(DirectionFlags.Back);
        DrawIf(DirectionFlags.Forward);

    }
}
