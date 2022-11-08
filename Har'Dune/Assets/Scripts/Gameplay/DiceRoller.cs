using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public int RollD20(int modifier)
    {
        int rollResult = Random.Range(1, 20) + modifier;

        return rollResult;
    }
}
