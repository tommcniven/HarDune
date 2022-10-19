using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItems
{
    public InventorySO item { get; private set; }
    public int stackSize { get; private set; }

    public InventoryItems(InventorySO source)
    {
        item = source;
        AddToStack();
    }

    public void AddToStack()
    {
        stackSize++;
    }

    public void RemoveFromStack()
    {
        stackSize--;
    }
}
