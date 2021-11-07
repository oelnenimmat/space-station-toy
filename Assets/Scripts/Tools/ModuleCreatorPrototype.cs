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

    public bool isTemporaryVisual;
    public bool includeInStructures;
    public bool includeInSolarArrays;

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
}
