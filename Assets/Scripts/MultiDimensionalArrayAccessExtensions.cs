using UnityEngine;

public static class MultiDimensionalArrayAccessExtensions
{
    public static T Get<T> (this T[,,] array, Vector3Int index)
    {
        return array[index.x, index.y, index.z];
    }

    public static void Set<T> (this T[,,] array, Vector3Int index, T value)
    {
        array[index.x, index.y, index.z] = value;
    }
}