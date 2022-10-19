using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Items", menuName = "Adventuring Gear")]
public class InventorySO : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;
    public string itemDescription;
    public string itemType;
    public int itemWeight;
    public int itemCost;
    public Sprite itemIconArtwork;
}
