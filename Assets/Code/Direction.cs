using UnityEngine;

public enum Direction { Left, Right, Down, Up, Back, Forward }

[System.Flags]
public enum DirectionFlags
{ 
    Left        = 1,
    Right       = 2,
    Down        = 4,
    Up          = 8,
    Back        = 16,
    Forward     = 32
}

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

    public static Vector3 ToVector3(this DirectionFlags df)
    {
        Vector3 v = Vector3.zero;

        if ((df & DirectionFlags.Left) != 0)
        {
            v += Vector3.left;
        }

        if ((df & DirectionFlags.Right) != 0)
        {
            v += Vector3.right;
        }

        if ((df & DirectionFlags.Down) != 0)
        {
            v += Vector3.down;
        }

        if ((df & DirectionFlags.Up) != 0)
        {
            v += Vector3.up;
        }

        if ((df & DirectionFlags.Back) != 0)
        {
            v += Vector3.back;
        }

        if ((df & DirectionFlags.Forward) != 0)
        {
            v += Vector3.forward;
        }

        return v;
    }
}

