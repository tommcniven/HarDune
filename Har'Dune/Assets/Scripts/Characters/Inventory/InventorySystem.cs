using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private Dictionary<InventorySO, InventoryItems> itemDictionary;
    public List<InventoryItems> inventory { get; private set; }

    private void Awake()
    {
        inventory = new List<InventoryItems>();
        itemDictionary = new Dictionary<InventorySO, InventoryItems>();
    }

    public InventoryItems GetItem(InventorySO referenceData)
    {
        if (itemDictionary.TryGetValue(referenceData, out InventoryItems value))
        {
            return value;
        }

        return null;
    }

    public void AddItem(InventorySO referenceData)
    {
        //Add to Stack
        if (itemDictionary.TryGetValue(referenceData, out InventoryItems value))
        {
            value.AddToStack();
        }

        //Create New Item for Invenotry
        else
        {
            InventoryItems newItem = new InventoryItems(referenceData);
            inventory.Add(newItem);
            itemDictionary.Add(referenceData, newItem);
        }
    }

    public void RemoveItem(InventorySO referenceData)
    {
        //Remove from Stack
        if (itemDictionary.TryGetValue(referenceData, out InventoryItems value))
        {
            value.RemoveFromStack();

            //Remove Item from Inventory
            if (value.stackSize == 0)
            {
                inventory.Remove(value);
                itemDictionary.Remove(referenceData);
            }
        }
    }
}
