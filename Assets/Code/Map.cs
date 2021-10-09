using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public struct Module
{
    public string      name;
    public GameObject  prefab;
    public int         priority;
    public int []      compatibilityLeft;
    public int []      compatibilityRight;
    public int []      compatibilityDown;
    public int []      compatibilityUp;
    public int []      compatibilityBack;
    public int []      compatibilityForward;

    public int [] GetCompatibility (Direction direction)
    {
        switch (direction)
        {
            case Direction.Left: return compatibilityLeft;
            case Direction.Right: return compatibilityRight;
            case Direction.Down: return compatibilityDown;
            case Direction.Up: return compatibilityUp;
            case Direction.Back: return compatibilityBack;
            case Direction.Forward: return compatibilityForward;
        }

        Debug.LogError($"Invalid Direction({direction})");
        return null;
    }

    public override string ToString()
    {
        return $"Module({name})";
    }
}

public enum Direction { Left, Right, Down, Up, Back, Forward }

public static class DirectionExtensions
{
    public static Vector3Int ToVector3Int(this Direction d)
    {
        switch(d)
        {
            case Direction.Left: return Vector3Int.left;
            case Direction.Right: return Vector3Int.right;
            case Direction.Down: return Vector3Int.down;
            case Direction.Up: return Vector3Int.up;
            case Direction.Back: return Vector3Int.back;
            case Direction.Forward: return Vector3Int.forward;
        }

        Debug.LogError($"Invalid direction ({d}) is invalid!");
        return Vector3Int.zero;
    }
}

public struct SpawnInfo
{
    public Vector3Int coords;
    public int moduleIndex;

    public SpawnInfo(Vector3Int c, int i)
    {
        coords = c;
        moduleIndex = i;
    }
}

public class Map : MonoBehaviour
{
    public ModuleCollection collection;
    public Module [] modules;
    public int emptyModuleIndex;


    public GameObject [,,] mapCells;
    public GameObject [,,] visuals;
    public int activeCellCount;

    public Vector3Int size;
    public Vector3Int startCoord;

    private Transform children;
    private Transform mapCellParent;

    public bool doWaveFunctionCollapse;

    private void Awake()
    {
        mapCells = new GameObject[size.x, size.y, size.z];
        visuals = new GameObject[size.x, size.y, size.z];

        modules = collection.modules;
        emptyModuleIndex = collection.emptyModuleIndex;
    }

    private void Start ()
    {
        mapCellParent = new GameObject("mapCellParent").transform;
        mapCellParent.SetParent(transform);
        mapCellParent.localPosition = Vector3.zero;

        Add(startCoord);
    }

    private void WaveFunctionCollapse()
    {

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Initialize superpositions
        List<int>[,,]superPositions = new List<int>[size.x, size.y, size.z];
        for (int z = 0; z < size.z; ++z)
        {
            for (int y = 0; y < size.y; ++y)
            {
                for (int x = 0; x < size.x; ++x)
                {
                    if (mapCells[x,y,z] == null)
                    {
                        superPositions[x,y,z] = new List<int>(1){emptyModuleIndex};
                    }
                    else
                    {
                        // Note(Leo): assert this, and then we can omit check for it in the loop :)
                        Assert.AreEqual(emptyModuleIndex, 0);
                
                        // Todo(Leo): build a list of these beforehand and just copy here
                        superPositions[x,y,z] = new List<int>(modules.Length - 1);
                        for (int k = 1; k < modules.Length; ++k)
                        {
                            superPositions[x,y,z].Add(k);
                        }
                    }
                }
            }
        }

        {
            void RemoveIncompatibilities(List<int> superPosition2, Direction direction)
            {
                for (int i = 0; i < superPosition2.Count; ++i)
                {
                    int [] moduleIndices = modules[superPosition2[i]].GetCompatibility(direction);
                    
                    // Note(Leo): This may be bug prone, we expect certain specific data
                    // bool isCompatibleWithEmptyModule = moduleIndices[0] == emptyModuleIndex;

                    // Note(Leo): This does same if data is correct and works even if it is not
                    bool isCompatibleWithEmptyModule = false;
                    foreach (int moduleIndex in moduleIndices)
                    {
                        if (moduleIndex == emptyModuleIndex)
                        {
                            isCompatibleWithEmptyModule = true;
                            break;
                        }
                    }

                    if (isCompatibleWithEmptyModule == false)
                    {
                        superPosition2.SwapRemove(i);
                        i -= 1;
                    }
                }
            }

            // Note(Leo): We really only need to do this for any cell that is next to non-empty cell
            // Note on that note(Leo): that is only true while have content only in non-empty cells
            // Note on those notes(Leo): really, we probably want to just have a layer of empty cells around 
            // the area available to player
            for(int y = 0; y < size.y; ++y)
            {
                for (int x = 0; x < size.x; ++x)
                {
                    RemoveIncompatibilities(superPositions[x,y,0], Direction.Back);
                    RemoveIncompatibilities(superPositions[x,y,size.z -1], Direction.Forward);
                }
            }

            for (int z = 0; z < size.z; ++z)
            {
                for (int x = 0; x < size.x; ++x)
                {
                    RemoveIncompatibilities(superPositions[x, 0, z], Direction.Down);
                    RemoveIncompatibilities(superPositions[x, size.y - 1, z], Direction.Up);
                }
            }

            for (int z = 0; z < size.z; ++z)
            {
                for (int y = 0; y < size.y; ++y)
                {
                    RemoveIncompatibilities(superPositions[0, y, z], Direction.Left);
                    RemoveIncompatibilities(superPositions[size.x - 1, y, z], Direction.Right);
                }
            }
        }

        bool IsFullyCollapsed()
        {
            for(int z = 0; z < size.z; z++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; ++x)
                    {
                        if (superPositions[x,y,z].Count == 0)
                        {
                            // Todo(Leo): we should really check this when removing possibilities superpositions
                            Debug.LogError("Superposition have collapsed too much, to zero!");
                        }

                        if (superPositions[x,y,z].Count != 1)
                        {
                            return false;
                        }
                    }
                }
            }

            // TODO(Leo): Study, this didn't work, is there something I should know about foreach on multidimensional arrays
            // foreach (var sp in superPositions)
            // {
            //     if (sp.Count == 0)
            //     {
            //         Debug.LogError("Superposition have collapsed too much, to zero!");
            //     }

            //     if (sp.Count != 1)
            //     {
            //         Debug.Log(sp.Count);
            //         return false;
            //     }
            // }
            return true;
        }

        Vector3Int GetMinEntropyCoords()
        {
            Vector3Int minEntropyCoords = new Vector3Int(0,0,0);
            int minEntropy = int.MaxValue;

            for (int z = 0; z < size.z; ++z)
            {
                for (int y = 0; y < size.y; ++y)
                {
                    for (int x = 0; x < size.x; ++x)
                    {
                        int entropy = superPositions[x,y,z].Count;
                        if (entropy < 1)
                        {
                            Debug.LogError("Too little entropy!");
                        }

                        if (entropy > 1 && entropy < minEntropy)
                        {
                            minEntropy = entropy;
                            minEntropyCoords = new Vector3Int(x,y,z);
                        }
                    }
                }
            }
            return minEntropyCoords;
        }

        void CollapseAt(Vector3Int coords)
        {
            // Basically, pick one with greatest priority
            var sp = superPositions[coords.x, coords.y, coords.z];

            int maxPriorityModuleIndex = 0;

            for(int i = 1; i < sp.Count; ++i)
            {
                if (modules[sp[i]].priority > modules[maxPriorityModuleIndex].priority)
                {
                    maxPriorityModuleIndex = sp[i];
                }
            }
            superPositions[coords.x, coords.y, coords.z] = new List<int>() {maxPriorityModuleIndex};
        }


        /*
        We need to get available neighbours for certain cells in certain directions,
        and we need something to hold that data. This is done thousands of times during
        the algorithm, so we don't want to allocate anything.

        We could use bool[] and reuse it. And it works and is way faster than e.g. HashSet.
        I tested :). But then we still need to clear the array every time new info is set. It
        is not much, but we can do better.

        We use integer array, and instead of clearing, we use incrementing integer value that
        is different after each invokation to represent "true" aka valid available neighbour.

            - avoid allocations: CHECK
            - avoid clearing: CHECK

        We are only left with the minimal number of required operations: 
            - set proper values to "true"
            - interpret values as "true" or "false"
    
        Happy coding :D
        */
        int [] availableNeighbours = new int[modules.Length];
        int currentAvailableNeighbourId = 0;

        var getPossibleNeighboursSw = new System.Diagnostics.Stopwatch();

        void GetPossibleNeighbours(Vector3Int coords, Direction direction)
        {  
            getPossibleNeighboursSw.Start();

            currentAvailableNeighbourId += 1;

            foreach (int index in superPositions[coords.x, coords.y, coords.z])
            {
                int [] compatibilities = modules[index].GetCompatibility(direction);
                foreach(int n in compatibilities)
                {
                    availableNeighbours[n] = currentAvailableNeighbourId;
                }
            }

            getPossibleNeighboursSw.Stop();
        }


        void Propagate(Vector3Int coord)
        {
            // Note(Leo): this can be put outside the function to avoid allocation, it made a small difference.
            // its here for clarity for now
            Direction [] neighbourDirections = new Direction[6];

            Stack<Vector3Int> toVisit = new Stack<Vector3Int>();
            toVisit.Push(coord);

            while(toVisit.Count > 0)
            {
                Vector3Int currentCell = toVisit.Pop();

                int neighbourDirectionCount = 0;

                if (currentCell.x > 0)
                {
                    neighbourDirections[neighbourDirectionCount++] = Direction.Left;
                }

                if (currentCell.x < (size.x - 1))
                {
                    neighbourDirections[neighbourDirectionCount++] = Direction.Right;
                }

                if (currentCell.y > 0)
                {
                    neighbourDirections[neighbourDirectionCount++] = Direction.Down;
                }

                if (currentCell.y < (size.y -1))
                {
                    neighbourDirections[neighbourDirectionCount++] = Direction.Up;
                }

                if (currentCell.z > 0)
                {
                    neighbourDirections[neighbourDirectionCount++] = Direction.Back;
                }

                if (currentCell.z < (size.z - 1))
                {
                    neighbourDirections[neighbourDirectionCount++] = Direction.Forward;
                }

                for (int n = 0; n < neighbourDirectionCount; ++n)
                {
                    Direction direction = neighbourDirections[n];

                    Vector3Int cellInDirection            = currentCell + direction.ToVector3Int();
                    List<int> otherCoordSuperPositions    = superPositions[cellInDirection.x, cellInDirection.y, cellInDirection.z];

                    GetPossibleNeighbours(currentCell, direction);

                    for (int i = 0; i < otherCoordSuperPositions.Count; ++i)
                    {
                        if (availableNeighbours[otherCoordSuperPositions[i]] != currentAvailableNeighbourId)
                        {
                            otherCoordSuperPositions.SwapRemove(i);
                            i -= 1;

                            if(toVisit.Contains(cellInDirection) == false)
                            {
                                toVisit.Push(cellInDirection);
                            }
                        }
                    }
                }

            }
        }

        for (int z = 0; z < size.z; ++z)
        {
            for (int y = 0; y < size.y; ++y)
            {
                for (int x = 0; x < size.x; ++x)
                {
                    Propagate(new Vector3Int(x,y,z));
                }
            }
        }


        // this is sanity check
        int rounds = 1000;
        // int rounds = size.x * size.y * size.z + 1;

        while(IsFullyCollapsed() == false && rounds > 0)
        {
            Vector3Int minEntropyCoords = GetMinEntropyCoords();
            CollapseAt(minEntropyCoords);
            Propagate(minEntropyCoords);

            rounds -= 1;
        }

        if (rounds == 0)
        {
            Debug.LogWarning("Loop not working yet :)");
        }

        // Note(Leo): activeCellCount as allocation count is true only while empty cells
        // have no content. It is still a decent start anyway.
        List<SpawnInfo> spawnList = new List<SpawnInfo>(activeCellCount);

        for (int z = 0; z < size.z; ++z)
        {
            for (int y = 0; y < size.y; ++y)
            {
                for (int x = 0; x < size.x; ++x)
                {
                    Module m = modules[superPositions[x,y,z][0]];

                    if (m.prefab != null)
                    {
                        spawnList.Add(new SpawnInfo(new Vector3Int(x,y,z), superPositions[x,y,z][0]));
                    }
                }
            }
        }

        InstantiateVisuals(spawnList);

        Debug.Log($"get neighbours took {getPossibleNeighboursSw.ElapsedMilliseconds / 1000f} s");
        Debug.Log($"Wave Function Collapse took {stopwatch.ElapsedMilliseconds / 1000f} s");
    }

    public bool Add(Vector3Int coords)
    {       
        bool insideSize = coords.x >= 0 && coords.x < size.x 
                            && coords.y >= 0 && coords.y < size.y
                            && coords.z >= 0 && coords.z < size.z;
        if (insideSize == false)
        {
            Debug.Log($"{coords} is out of bounds");
            return false;
        }

        if (mapCells[coords.x, coords.y, coords.z] != null)
        {
            Debug.Log($"{coords} already contains a cube");
            return false;
        }

        // STATE VALIDITY IS OK FROM NOW ON :)

        GameObject mapCell = new GameObject($"Map Cell ({coords})");
        mapCell.transform.SetParent(mapCellParent);
        mapCell.transform.position = coords;

        mapCell.AddComponent<MapCube>().coords = coords;
        mapCell.name = $"map cell {coords}";

        BoxCollider collider = mapCell.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = Vector3.one;

        mapCells[coords.x, coords.y, coords.z] = mapCell;            
        activeCellCount += 1;

        if (doWaveFunctionCollapse)
        {
            WaveFunctionCollapse();
        }

        return true;
    }

    public bool Destroy(Vector3Int coords)
    {
        if (activeCellCount > 1)
        {
            Destroy(mapCells[coords.x, coords.y, coords.z]);
            Destroy(visuals[coords.x, coords.y, coords.z]);

            mapCells[coords.x, coords.y, coords.z] = null;
            visuals[coords.x, coords.y, coords.z] = null;

            activeCellCount -= 1;

            if (doWaveFunctionCollapse)
            {
                WaveFunctionCollapse();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private void InstantiateVisuals(List<SpawnInfo> spawnList)
    {
        // Note(Leo): this is way faster than destroying children individually
        if (children != null)
        {
            Destroy(children.gameObject);
        }
        children = new GameObject("children").transform;

        foreach(SpawnInfo s in spawnList)
        {
            GameObject g = Instantiate(modules[s.moduleIndex].prefab, children);
            g.transform.position = s.coords;
            visuals[s.coords.x, s.coords.y, s.coords.z] = g;
        }   
    }
}