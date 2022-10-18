using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponReference : MonoBehaviour
{
    [Header("Reference Scriptable Object")]
    public WeaponSO weapon;

    [Header("UI")]
    public int placeholder;

    public void SetWeapon()
    {
        //Call this method to set the weapon based on the weapon name
    }
}
