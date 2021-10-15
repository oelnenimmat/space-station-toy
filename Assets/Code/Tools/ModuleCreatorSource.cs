using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleCreatorSource : MonoBehaviour
{
    public Mesh [] meshesToAdd;
    public Material defaultMaterial;

    public Transform children;

    public void AddMeshes()
    {
        if (children == null)
        {
            children = new GameObject("children").transform;
            children.SetParent(transform);
            children.localPosition = Vector3.zero;
        }

        if (meshesToAdd != null)
        {
            foreach (var mesh in meshesToAdd)
            {
                if (mesh == null)
                {
                    continue;
                }

                if (children.Find(mesh.name) == null)
                {
                    var p = new GameObject().AddComponent<ModuleCreatorPrototype>();
                    p.mesh = mesh;
                    p.material = defaultMaterial;

                    p.transform.SetParent(children);
                    p.transform.localPosition = new Vector3(2 * (children.childCount - 1), 0, 0);


                    p.OnValidate();
                }
            }

            meshesToAdd = new Mesh[0];
        }
    }

    public void SortChildren()
    {
        if (children == null)
        {
            return;
        }

        List<Transform> childList = new List<Transform>();
        foreach (Transform child in children)
        {
            childList.Add(child);
        }

        childList.Sort((a, b) => { return a.name.CompareTo(b.name); });
        for (int i = 0; i < childList.Count; ++i)
        {
            // Undo.SetTransformParent(childList[i], childList[i].parent, "Sort Children");
            childList[i].SetSiblingIndex(i);
            childList[i].localPosition = new Vector3(2 * i, 0, 0);
        }
    }

    public ModuleCreatorPrototype [] GetPrototypes()
    {
        ModuleCreatorPrototype [] prototypes = children.GetComponentsInChildren<ModuleCreatorPrototype>();
        return prototypes;
    }
}
