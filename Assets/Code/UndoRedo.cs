using UnityEngine;

public enum UndoRedoOperation { Invalid, Add, Remove };

public struct UndoRedoItem
{
    public Vector3Int          cell;
    public UndoRedoOperation   operation;

    public UndoRedoItem(Vector3Int c, UndoRedoOperation op)
    {
        cell = c;
        operation = op;
    }
}

public class UndoRedo
{
    private UndoRedoItem [] stack;
    private int capacity;

    // Note(Leo):
    //  - index is index to stackArray
    //  - current is current position between zero and top of stack
    //  - top is maximum position in stack array, that has been set properly
    private int index;
    private int current;
    private int top;


    public UndoRedo(int capacity)
    {
        stack   = new UndoRedoItem[capacity];
        index   = 0;
        current = 0;
        top     = 0;

        this.capacity = capacity;
    }

    private int IncrementOne(int i)
    {
        return (i + 1) % capacity;
    }

    private int DecrementOne(int i)
    {
        return (i + capacity - 1) % capacity;
    }

    public void Add(Vector3Int cell, UndoRedoOperation operation)
    {
        stack[index]    = new UndoRedoItem(cell, operation);
        index           = IncrementOne(index);
        current         = Mathf.Min(current + 1, capacity);
        top             = current;
    }

    public bool Undo(out UndoRedoItem item)
    {
        if (current > 0)
        {
            index = DecrementOne(index);
            current -= 1;
            item = stack[index];
            return true;
        }

        item = new UndoRedoItem();
        return false;
    }

    public bool Redo(out UndoRedoItem item)
    {
        if (current < top)
        {
            item = stack[index];
            index = IncrementOne(index);
            current += 1;
            return true;
        }

        item = new UndoRedoItem();
        return false;
    }
}