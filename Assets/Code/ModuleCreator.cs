using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ModulePrototype
{
    public string name;
    public string prefabName;
    public int priority;
    public int negX;
    public int posX;
    public int negY;
    public int posY;
    public int negZ;
    public int posZ;
}

public enum ModuleRotations
{
    None,
    PositiveXToSixDirections,
    PositiveZToSixDirections,
    PositiveYToSixDirections,
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

            case ModuleRotations.PositiveXToSixDirections:
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

            case ModuleRotations.PositiveZToSixDirections:
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

public class ModuleCreator : MonoBehaviour
{
    public ModuleCreatorSource source;
    public Transform children;
    public ModuleCollection collection;

    public float socketVertexDistanceThreshold = 0.001f;

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
        emptyPrototype.negX = -2;
        emptyPrototype.posX = -2;
        emptyPrototype.negY = -2;
        emptyPrototype.posY = -2;
        emptyPrototype.negZ = -2;
        emptyPrototype.posZ = -2;

        prototypes.Add(emptyPrototype);
        
        List<Socket> xAxisSockets = new List<Socket>();
        List<Socket> yAxisSockets = new List<Socket>();
        List<Socket> zAxisSockets = new List<Socket>();

        for (int pieceIndex = 0; pieceIndex < pieces.Length; ++pieceIndex)
        {
            Mesh mesh = pieces[pieceIndex].mesh;
            Quaternion [] rotations = pieces[pieceIndex].rotations.GetRotations();

            for (int rotationIndex = 0; rotationIndex < rotations.Length; ++rotationIndex)
            {
                string name = $"{mesh.name}_{rotationIndex}";

                // ------------------------------------

                GameObject g = new GameObject(name);
                
                g.transform.SetParent(children);
                g.transform.position = new Vector3(pieceIndex * gap, 0, rotationIndex * gap);
                g.transform.rotation = rotations[rotationIndex];

                var filter = g.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;

                var renderer = g.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = pieces[pieceIndex].material;

                // var collider = g.AddComponent<BoxCollider>();
                // collider.center = Vector3.zero;
                // collider.size = Vector3.one;

                // ------------------------------------

                ModulePrototype p = new ModulePrototype();

                p.name = name;
                p.prefabName = name;
                p.priority = pieces[pieceIndex].priority;

                Vector3 [] vertexPositions  = mesh.vertices;
                Bounds bounds               = new Bounds(Vector3.zero, Vector3.one);

                // Todo(Leo): I had idea, that we could this way check if a side was not used and declare that as a back
                // side, but then we would not get some shapes accurately
                // Bounds leftBounds           = new Bounds(new Vector3(-0.25f, 0, 0), new Vector3(0.5f, 1, 1));
                // Bounds rightBounds          = new Bounds(new Vector3(0.25f, 0, 0), new Vector3(0.5f, 1, 1));
                // Bounds downBounds           = new Bounds(new Vector3(0, -0.25f, 0), new Vector3(1, 0.5f, 1));
                // Bounds upBounds             = new Bounds(new Vector3(0, 0.25f, 0), new Vector3(1, 0.5f, 1));
                // Bounds backBounds           = new Bounds(new Vector3(0, 0, -0.25f), new Vector3(1, 1, 0.5f));
                // Bounds forwardBounds        = new Bounds(new Vector3(0, 0, 0.25f), new Vector3(1, 1, 0.5f));

                // Detect sockets
                {
                    HashSet<Vector2> verticesOnLeft = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnRight = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnDown = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnUp = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnBack = new HashSet<Vector2>();
                    HashSet<Vector2> verticesOnForward = new HashSet<Vector2>();

                    foreach(Vector3 _v in vertexPositions)
                    {   
                        // Rotate vertices, since mesh itself is never rotated
                        Vector3 v = rotations[rotationIndex] * _v;

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
                                    Debug.Log($"Match found {pieceIndex}/{rotationIndex}");
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

                    p.negX = AddUniqueSocket(verticesOnLeft, xAxisSockets);
                    p.posX = AddUniqueSocket(verticesOnRight, xAxisSockets);

                    p.negY = AddUniqueSocket(verticesOnDown, yAxisSockets);
                    p.posY = AddUniqueSocket(verticesOnUp, yAxisSockets);

                    p.negZ = AddUniqueSocket(verticesOnBack, zAxisSockets);
                    p.posZ = AddUniqueSocket(verticesOnForward, zAxisSockets);
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
            List<int>[] compNegX = new List<int>[processedPrototypes.Length];
            List<int>[] compPosX = new List<int>[processedPrototypes.Length];
            List<int>[] compNegY = new List<int>[processedPrototypes.Length];
            List<int>[] compPosY = new List<int>[processedPrototypes.Length];
            List<int>[] compNegZ = new List<int>[processedPrototypes.Length];
            List<int>[] compPosZ = new List<int>[processedPrototypes.Length];

            for (int i = 0; i < processedPrototypes.Length; ++i)
            {
                compNegX[i] = new List<int>();
                compPosX[i] = new List<int>();
                compNegY[i] = new List<int>();
                compPosY[i] = new List<int>();
                compNegZ[i] = new List<int>();
                compPosZ[i] = new List<int>();
            }

            bool TestCompatibility(int from, int to)
            { 
                // Todo(Leo): maybe we want to add priority, and afteer that allow these, but with lowest priority
                // Empty side to actual empty
                if ((from == -1 && to == -2) || (from == -2 && to == -1))
                {
                    return true;
                }

                // These are actual pieces with empty side
                if (from == -1 && to == -1)
                {
                    return false;
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
                    if (TestCompatibility(storedPrototypes[from].posX, storedPrototypes[to].negX))
                    {
                        compPosX[from].Add(to);
                        compNegX[to].Add(from);
                    }

                    if (TestCompatibility(storedPrototypes[from].negX, storedPrototypes[to].posX))
                    {
                        compNegX[from].Add(to);
                        compPosX[to].Add(from);
                    }

                    if (TestCompatibility(storedPrototypes[from].posY, storedPrototypes[to].negY))
                    {
                        compPosY[from].Add(to);
                        compNegY[to].Add(from);
                    }

                    if (TestCompatibility(storedPrototypes[from].negY, storedPrototypes[to].posY))
                    {
                        compNegY[from].Add(to);
                        compPosY[to].Add(from);
                    }

                    if (TestCompatibility(storedPrototypes[from].posZ, storedPrototypes[to].negZ))
                    {
                        compPosZ[from].Add(to);
                        compNegZ[to].Add(from);
                    }

                    if (TestCompatibility(storedPrototypes[from].negZ, storedPrototypes[to].posZ))
                    {
                        compNegZ[from].Add(to);
                        compPosZ[to].Add(from);
                    }
                }
            }

            for (int i = 0; i < processedPrototypes.Length; ++i)
            {
                compNegX[i].Sort();
                processedPrototypes[i].compatibilityLeft = compNegX[i].ToArray();

                compPosX[i].Sort();
                processedPrototypes[i].compatibilityRight = compPosX[i].ToArray();

                compNegY[i].Sort();
                processedPrototypes[i].compatibilityDown = compNegY[i].ToArray();

                compPosX[i].Sort();
                processedPrototypes[i].compatibilityUp = compPosY[i].ToArray();

                compNegZ[i].Sort();
                processedPrototypes[i].compatibilityBack = compNegZ[i].ToArray();

                compPosZ[i].Sort();
                processedPrototypes[i].compatibilityForward = compPosZ[i].ToArray();
            }

            collection.modules = processedPrototypes;

            for(int i = 0; i < processedPrototypes.Length; ++i)
            {
                if (processedPrototypes[i].name.Equals("empty"))
                {
                    collection.emptyModuleIndex = i;
                    break;
                }
            }
        }
    }
}
