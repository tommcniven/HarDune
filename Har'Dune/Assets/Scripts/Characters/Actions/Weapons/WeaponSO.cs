using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Weapon Information")]
    public string weaponName;
    public string weaponDescription;
    public string weaponCategory;
    public int attackDice;
    public int attackDamage;
    public int attackRange;
    public string weaponDamageType;
    public int weaponWeight;
    public string weaponActionType;
    public Sprite weaponIconArtwork;
}
