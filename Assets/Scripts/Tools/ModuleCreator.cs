using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Linq;

[System.Serializable]
public class ModulePrototype
{
    public string name;
    public string prefabName;
    public int priority;

    public bool isTemporaryVisual;
    public bool includeInStructures;
    public bool includeInSolarArrays;

    public int leftSocket;
    public int rightSocket;
    public int downSocket;
    public int upSocket;
    public int backSocket;
    public int forwardSocket;
}

public enum ModuleRotations
{
    None,

    XAxisToRightAngles,
    YAxisToRightAngles,
    ZAxisToRightAngles,
    
    XAxisToRightAnglesAndOpposites,
    YAxisToRightAnglesAndOpposites,
    ZAxisToRightAnglesAndOpposites,
    
    All
}

public static class ModuleRotationsExtensions
{
    public static Quaternion[] GetRotations(this ModuleRotations r)
    {
        switch(r)
        {
            case ModuleRotations.None:
            {
                return new Quaternion[] {Quaternion.identity};
            }

            case ModuleRotations.XAxisToRightAngles:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(0, 0, 90),
                };
            }

            case ModuleRotations.YAxisToRightAngles:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(90, 0, 0),
                    Quaternion.Euler(0, 0, 90),
                };
            }


            case ModuleRotations.ZAxisToRightAngles:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(90, 0, 0),
                };
            }

            case ModuleRotations.XAxisToRightAnglesAndOpposites:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(0, 180, 0),
                    Quaternion.Euler(0, 270, 0),
                    Quaternion.Euler(0, 0, 90),
                    Quaternion.Euler(0, 0, 270),
                };
            }

            case ModuleRotations.YAxisToRightAnglesAndOpposites:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(90, 0, 0),
                    Quaternion.Euler(180, 0, 0),
                    Quaternion.Euler(270, 0, 0),
                    Quaternion.Euler(0, 0, 90),
                    Quaternion.Euler(0, 0, 270),
                };
            }


            case ModuleRotations.ZAxisToRightAnglesAndOpposites:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(0, 180, 0),
                    Quaternion.Euler(0, 270, 0),
                    Quaternion.Euler(90, 0, 0),
                    Quaternion.Euler(270, 0, 0),
                };
            }

            case ModuleRotations.All:
            {
                return new Quaternion[24]
                {
                    // -X up
                    Quaternion.Euler(0, 0, 270),
                    Quaternion.Euler(0, 90, 270),
                    Quaternion.Euler(0, 180, 270),
                    Quaternion.Euler(0, 270, 270),

                    // +X up
                    Quaternion.Euler(0, 0, 90),
                    Quaternion.Euler(0, 90, 90),
                    Quaternion.Euler(0, 180, 90),
                    Quaternion.Euler(0, 270, 90),

                    // -Y up
                    Quaternion.Euler(0, 0, 180),
                    Quaternion.Euler(0, 90, 180),
                    Quaternion.Euler(0, 180, 180),
                    Quaternion.Euler(0, 270, 180),

                    // +Y up
                    Quaternion.Euler(0, 0, 0),
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(0, 180, 0),
                    Quaternion.Euler(0, 270, 0),

                    // -Z up
                    Quaternion.Euler(90, 0, 0),
                    Quaternion.Euler(90, 90, 0),
                    Quaternion.Euler(90, 180, 0),
                    Quaternion.Euler(90, 270, 0),

                    // +Z up
                    Quaternion.Euler(270, 0, 0),
                    Quaternion.Euler(270, 90, 0),
                    Quaternion.Euler(270, 180, 0),
                    Quaternion.Euler(270, 270, 0),
                };
            }

            default:
                return GetRotations(ModuleRotations.All);
        }
    }
}

#if UNITY_EDITOR

public class ModuleCreator : MonoBehaviour
{
    public ModuleCreatorSource source;
    public Transform children;
    public ModuleCollection collection;

    public float socketVertexDistanceThreshold = 0.001f;

    public GameObject fallbackModule;

    [System.Serializable]
    public class Socket
    {
        public Vector2[] vertices;

        public Socket(HashSet<Vector2> vertices)
        {
            this.vertices = new Vector2[vertices.Count];
            vertices.CopyTo(this.vertices);
        }

        public bool Matches(Socket other)
        {
            if (other.vertices.Length != this.vertices.Length)
            {
                return false;
            }

            int count = this.vertices.Length;
            bool [] usedVertex = new bool[count];

            for (int t = 0; t < count; ++t)
            {
                bool foundVertex = false;

                for (int o = 0; o < count; ++o)
                {
                    if (usedVertex[o])
                    {
                        continue;
                    }

                    float distance = Vector2.Distance(other.vertices[o], this.vertices[t]);
                    if (distance < 0.001f)
                    {
                        usedVertex[o] = true;
                        foundVertex = true;
                        break;
                    }
                }

                if (foundVertex == false)
                {
                    return false;
                }
            }

            return true;
        }
    }



    public void Create()
    {
        Debug.Log("Creating things :)");

        var pieces = source.GetPrototypes();

        if (children == null)
        {
            children = transform.Find("children");
        }
    
        if (children != null)
        {
            DestroyImmediate(children.gameObject);
        }


        children = new GameObject("children").transform;
        children.SetParent(transform);

        float gap           = 2f;

        List<ModulePrototype> prototypes = new List<ModulePrototype>();

        // Note(Leo): this is supposed to be first, we may depend on that.
        // That may not be a good thing, although it sounds like proper
        // data oriented design
        ModulePrototype emptyPrototype = new ModulePrototype();
        emptyPrototype.name = "empty";
        emptyPrototype.prefabName = null;
        emptyPrototype.priority = -100;
        emptyPrototype.leftSocket = -2;
        emptyPrototype.rightSocket = -2;
        emptyPrototype.downSocket = -2;
        emptyPrototype.upSocket = -2;
        emptyPrototype.backSocket = -2;
        emptyPrototype.forwardSocket = -2;

        prototypes.Add(emptyPrototype);

        ModulePrototype insideEmptyPrototype = new ModulePrototype();
        insideEmptyPrototype.name = "inside_empty";
        insideEmptyPrototype.prefabName = "inside_empty";
        insideEmptyPrototype.priority = -100;
        insideEmptyPrototype.leftSocket = -3;
        insideEmptyPrototype.rightSocket = -3;
        insideEmptyPrototype.downSocket = -3;
        insideEmptyPrototype.upSocket = -3;
        insideEmptyPrototype.backSocket = -3;
        insideEmptyPrototype.forwardSocket = -3;

        GameObject insideEmptyModuleInstance = Instantiate(fallbackModule, children);
        insideEmptyModuleInstance.transform.localPosition = new Vector3(insideEmptyModuleInstance.transform.localPosition.x, 0, 0);
        insideEmptyModuleInstance.name = insideEmptyPrototype.name;

        prototypes.Add(insideEmptyPrototype);

        // ModulePrototype fallbackPrototype = new ModulePrototype();
        // fallbackPrototype.name = "fallback";
        // fallbackPrototype.prefabName = "fallback";
        // fallbackPrototype.priority = int.MinValue;
        // fallbackPrototype.leftSocket = int.MinValue;
        // fallbackPrototype.rightSocket = int.MinValue;
        // fallbackPrototype.downSocket = int.MinValue;
        // fallbackPrototype.upSocket = int.MinValue;
        // fallbackPrototype.backSocket = int.MinValue;
        // fallbackPrototype.forwardSocket = int.MinValue;
        
        // GameObject fallbackModuleInstance = Instantiate(fallbackModule, children);
        // fallbackModuleInstance.transform.localPosition = new Vector3(fallbackModuleInstance.transform.localPosition.z, 0, 0);
        // fallbackModuleInstance.name = fallbackPrototype.name;

        // prototypes.Add(fallbackPrototype);


        List<Socket> xAxisSockets = new List<Socket>();
        List<Socket> yAxisSockets = new List<Socket>();
        List<Socket> zAxisSockets = new List<Socket>();

        for (int pieceIndex = 0; pieceIndex < pieces.Length; ++pieceIndex)
        {
            Mesh mesh = pieces[pieceIndex].mesh;
            Quaternion [] rotations = pieces[pieceIndex].rotations.GetRotations();

            for (int rotationIndex = 0; rotationIndex < rotations.Length; ++rotationIndex)
            {
                string name = $"{mesh.name}_{rotationIndex}_{prototypes.Count}";

                // ------------------------------------

                GameObject g = new GameObject(name);
                
                g.transform.SetParent(children);
                g.transform.position = new Vector3(pieceIndex * gap, 0, rotationIndex * gap);
                g.transform.rotation = rotations[rotationIndex];

                var filter = g.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;

                var renderer = g.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = pieces[pieceIndex].material;

                // ------------------------------------

                ModulePrototype p = new ModulePrototype();

                p.name                  = name;
                p.prefabName            = name;
                p.priority              = pieces[pieceIndex].priority;
                p.isTemporaryVisual     = pieces[pieceIndex].isTemporaryVisual;
                p.includeInStructures   = pieces[pieceIndex].includeInStructures;
                p.includeInSolarArrays  = pieces[pieceIndex].includeInSolarArrays;

                Vector3 [] vertexPositions  = mesh.vertices;
                Bounds bounds               = new Bounds(Vector3.zero, Vector3.one);

                // Detect sockets
                {
                    HashSet<Vector2> verticesOnLeft = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnRight = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnDown = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnUp = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnBack = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnForward = new HashSet<Vector2>();

                    bool hasVerticesOnLeftHalf = false;
                    bool hasVerticesOnRightHalf = false;
                    bool hasVerticesOnDownHalf = false;
                    bool hasVerticesOnUpHalf = false;
                    bool hasVerticesOnBackHalf = false;
                    bool hasVerticesOnForwardHalf = false;

                    foreach(Vector3 _v in vertexPositions)
                    {   
                        // Rotate vertices, since mesh itself is never rotated
                        Vector3 v = rotations[rotationIndex] * _v;

                        // --------------------------------------------------

                        hasVerticesOnLeftHalf = (v.x < 0) || hasVerticesOnLeftHalf;
                        hasVerticesOnRightHalf = (v.x > 0) || hasVerticesOnRightHalf;

                        hasVerticesOnDownHalf = (v.y < 0) || hasVerticesOnDownHalf;
                        hasVerticesOnUpHalf = (v.y > 0) || hasVerticesOnUpHalf;

                        hasVerticesOnBackHalf = (v.z < 0) || hasVerticesOnBackHalf;
                        hasVerticesOnForwardHalf = (v.z > 0) || hasVerticesOnForwardHalf;
                        
                        // --------------------------------------------------

                        if (Mathf.Abs(bounds.min.x - v.x) < socketVertexDistanceThreshold)
                        {
                            verticesOnLeft.Add(new Vector2(v.y, v.z));
                        }

                        if (Mathf.Abs(bounds.max.x - v.x) < socketVertexDistanceThreshold)
                        {
                            verticesOnRight.Add(new Vector2(v.y, v.z));
                        }

                        if (Mathf.Abs(bounds.min.y - v.y) < socketVertexDistanceThreshold)
                        {
                            verticesOnDown.Add(new Vector2(v.x, v.z));
                        }

                        if (Mathf.Abs(bounds.max.y - v.y) < socketVertexDistanceThreshold)
                        {
                            verticesOnUp.Add(new Vector2(v.x, v.z));
                        }

                        if (Mathf.Abs(bounds.min.z - v.z) < socketVertexDistanceThreshold)
                        {
                            verticesOnBack.Add(new Vector2(v.x, v.y));
                        }

                        if (Mathf.Abs(bounds.max.z - v.z) < socketVertexDistanceThreshold)
                        {
                            verticesOnForward.Add(new Vector2(v.x, v.y));
                        }
                    }

                    int AddUniqueSocket(HashSet<Vector2> socketVertices, List<Socket> socketList)
                    {
                        // Note(Leo): this means invalid, or empty
                        int uniqueIndex = -1;

                        if (socketVertices.Count > 0)
                        {
                            var newSocket = new Socket(socketVertices);
                            bool matches = false;

                            for(int socketIndex = 0; socketIndex < socketList.Count; ++socketIndex)
                            {
                                if (newSocket.Matches(socketList[socketIndex]))
                                {
                                    matches = true;
                                    uniqueIndex = socketIndex;
                                    // Debug.Log($"Match found {pieceIndex}/{rotationIndex}");
                                    break;
                                }
                            }

                            if (matches == false)
                            {
                                uniqueIndex = socketList.Count;
                                socketList.Add(newSocket);
                            }
                        }
                        return uniqueIndex;
                    }

                    p.leftSocket = AddUniqueSocket(verticesOnLeft, xAxisSockets);
                    p.rightSocket = AddUniqueSocket(verticesOnRight, xAxisSockets);

                    p.downSocket = AddUniqueSocket(verticesOnDown, yAxisSockets);
                    p.upSocket = AddUniqueSocket(verticesOnUp, yAxisSockets);

                    p.backSocket = AddUniqueSocket(verticesOnBack, zAxisSockets);
                    p.forwardSocket = AddUniqueSocket(verticesOnForward, zAxisSockets);


                    int SetIfDetectedInsideEmpty(int socket, bool isDetected)
                    {
                        if(socket == -1 && isDetected == false)
                        {
                            // todo(Leo): fix magic number
                            return -3;
                        }
                        return socket;
                    }

                    p.leftSocket = SetIfDetectedInsideEmpty(p.leftSocket, hasVerticesOnLeftHalf);
                    p.rightSocket = SetIfDetectedInsideEmpty(p.rightSocket, hasVerticesOnRightHalf);
                    p.downSocket = SetIfDetectedInsideEmpty(p.downSocket, hasVerticesOnDownHalf);
                    p.upSocket = SetIfDetectedInsideEmpty(p.upSocket, hasVerticesOnUpHalf);
                    p.backSocket = SetIfDetectedInsideEmpty(p.backSocket, hasVerticesOnBackHalf);
                    p.forwardSocket = SetIfDetectedInsideEmpty(p.forwardSocket, hasVerticesOnForwardHalf);
                }

                prototypes.Add(p);
            }
        }


        // ------------------------------------------------------------

        GameObject masterPrefab = PrefabUtility.SaveAsPrefabAsset(children.gameObject, "Assets/prefabs/generated/prototypes.prefab");

        // ------------------------------------------------------------



        {
            var prefabDictionary = new Dictionary <string, GameObject>();
            foreach(Transform child in masterPrefab.transform)
            {
                prefabDictionary.Add(child.name, child.gameObject);
            }

            var storedPrototypes = prototypes;
            Module [] processedPrototypes = new Module[storedPrototypes.Count];
            for (int i = 0; i < processedPrototypes.Length; ++i)
            {
                processedPrototypes[i]          = new Module();
                processedPrototypes[i].name     = storedPrototypes[i].name;
                processedPrototypes[i].priority = storedPrototypes[i].priority;

                if (string.IsNullOrEmpty(storedPrototypes[i].prefabName) == false)
                {
                    processedPrototypes[i].prefab = prefabDictionary[storedPrototypes[i].prefabName];
                }
            }


            // Note(Leo): create separate lists, that we can add to. They are converted to arrays later
            HashSet<int>[] compLeft = new HashSet<int>[processedPrototypes.Length];
            HashSet<int>[] compRight = new HashSet<int>[processedPrototypes.Length];
            HashSet<int>[] compDown = new HashSet<int>[processedPrototypes.Length];
            HashSet<int>[] compUp = new HashSet<int>[processedPrototypes.Length];
            HashSet<int>[] compBack = new HashSet<int>[processedPrototypes.Length];
            HashSet<int>[] compForward = new HashSet<int>[processedPrototypes.Length];

            for (int i = 0; i < processedPrototypes.Length; ++i)
            {
                compLeft[i] = new HashSet<int>();
                compRight[i] = new HashSet<int>();
                compDown[i] = new HashSet<int>();
                compUp[i] = new HashSet<int>();
                compBack[i] = new HashSet<int>();
                compForward[i] = new HashSet<int>();
            }

            bool TestCompatibility(int from, int to)
            { 
                // Todo(Leo): these are undocumented magic values. They are used only in this class, but pls make proper naming or something

                // Todo(Leo): maybe we want to add priority, and afteer that allow these, but with lowest priority
                // Empty OUTside to actual empty
                if ((from == -1 && to == -2) || (from == -2 && to == -1))
                {
                    return true;
                }

                // These are actual pieces with empty side
                if (from == -1 && to == -1)
                {
                    return false;
                }

                // This is fallback, it matches with everything
                if (from == int.MinValue || to == int.MinValue)
                {
                    return true;
                }

                // All else works with these
                return from == to;
            }

            // Note(Leo): Either do triangle 2d loop, and add two processedPrototypes in each if,
            // or only add one and do square loop
            for (int from = 0; from < processedPrototypes.Length; ++from)
            {
                for(int to = from; to < processedPrototypes.Length; ++to)
                {
                    // From Right To Left 
                    if (TestCompatibility(storedPrototypes[from].rightSocket, storedPrototypes[to].leftSocket))
                    {
                        compRight[from].Add(to);
                        compLeft[to].Add(from);
                    }

                    // From Left To Right 
                    if (TestCompatibility(storedPrototypes[from].leftSocket, storedPrototypes[to].rightSocket))
                    {
                        compLeft[from].Add(to);
                        compRight[to].Add(from);
                    }

                    // From Up To Down 
                    if (TestCompatibility(storedPrototypes[from].upSocket, storedPrototypes[to].downSocket))
                    {
                        compUp[from].Add(to);
                        compDown[to].Add(from);
                    }

                    // From Down To Up 
                    if (TestCompatibility(storedPrototypes[from].downSocket, storedPrototypes[to].upSocket))
                    {
                        compDown[from].Add(to);
                        compUp[to].Add(from);
                    }

                    // From Forward To Back 
                    if (TestCompatibility(storedPrototypes[from].forwardSocket, storedPrototypes[to].backSocket))
                    {
                        compForward[from].Add(to);
                        compBack[to].Add(from);
                    }

                    // From Back To Forward 
                    if (TestCompatibility(storedPrototypes[from].backSocket, storedPrototypes[to].forwardSocket))
                    {
                        compBack[from].Add(to);
                        compForward[to].Add(from);
                    }
                }
            }

            for (int i = 0; i < processedPrototypes.Length; ++i)
            {
                List<int> list;

                list = compLeft[i].ToList();
                list.Sort();
                processedPrototypes[i].compatibilityLeft = list.ToArray();

                list = compRight[i].ToList();
                list.Sort();
                processedPrototypes[i].compatibilityRight = list.ToArray();

                list = compDown[i].ToList();
                list.Sort();
                processedPrototypes[i].compatibilityDown = list.ToArray();

                list = compUp[i].ToList();
                list.Sort();
                processedPrototypes[i].compatibilityUp = list.ToArray();

                list = compBack[i].ToList();
                list.Sort();
                processedPrototypes[i].compatibilityBack = list.ToArray();

                list = compForward[i].ToList();
                list.Sort();
                processedPrototypes[i].compatibilityForward = list.ToArray();
            }

            collection.modules = processedPrototypes;

            List<int> structureModuleIndices = new List<int>();
            List<int> solarArrayModuleIndices = new List<int>();

            for(int i = 0; i < processedPrototypes.Length; ++i)
            {
                if (processedPrototypes[i].name.Equals("empty"))
                {
                    collection.emptyModuleIndex = i;
                }

                if (processedPrototypes[i].name.Equals("inside_empty"))
                {
                    structureModuleIndices.Add(i);
                }

                if (storedPrototypes[i].isTemporaryVisual)
                {
                    collection.temporaryVisualModuleIndex = i;
                }

                if (storedPrototypes[i].includeInStructures)
                {
                    structureModuleIndices.Add(i);
                }

                if (storedPrototypes[i].includeInSolarArrays)
                {
                    solarArrayModuleIndices.Add(i);
                }
            }

            collection.structureModuleIndices = structureModuleIndices.ToArray();
            collection.solarArrayModuleIndices = solarArrayModuleIndices.ToArray();
        }
    }
}

#endif