using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSlots : MonoBehaviour
{
    [Header("Scripts")]
    public UnitStats scriptUnitStats;
    public TileMap scriptTileMap;
    public GameMenuController scriptGameMenuController;

    [Header("Max Spell Slots")]
    public int maxLevelOneSpellSlots;
    public int maxLevelTwoSpellSlots;
    public int maxLevelThreeSpellSlots;
    public int maxLevelFourSpellSlots;
    public int maxLevelFiveSpellSlots;

    [Header("Current Spell Slots")]
    public int currentLevelOneSpellSlots;
    public int currentLevelTwoSpellSlots;
    public int currentLevelThreeSpellSlots;
    public int currentLevelFourSpellSlots;
    public int currentLevelFiveSpellSlots;

    public void Start()
    {
        SetMaxSpellSlots();
        SetCurrentSpellSlots();
    }

    public void SetMaxSpellSlots()
    {
        if (scriptUnitStats.unitClass == "Druid")
        {
            if (scriptUnitStats.unitLevel == 1)
            {
                maxLevelOneSpellSlots = 2;
                maxLevelTwoSpellSlots = 0;
                maxLevelThreeSpellSlots = 0;
                maxLevelFourSpellSlots = 0;
                maxLevelFiveSpellSlots = 0;
            }
            else if (scriptUnitStats.unitLevel == 2)
            {
                maxLevelOneSpellSlots = 3;
                maxLevelTwoSpellSlots = 0;
                maxLevelThreeSpellSlots = 0;
                maxLevelFourSpellSlots = 0;
                maxLevelFiveSpellSlots = 0;
            }
            else if (scriptUnitStats.unitLevel == 3)
            {
                maxLevelOneSpellSlots = 4;
                maxLevelTwoSpellSlots = 2;
                maxLevelThreeSpellSlots = 0;
                maxLevelFourSpellSlots = 0;
                maxLevelFiveSpellSlots = 0;
            }
            else if (scriptUnitStats.unitLevel == 4)
            {
                maxLevelOneSpellSlots = 4;
                maxLevelTwoSpellSlots = 3;
                maxLevelThreeSpellSlots = 0;
                maxLevelFourSpellSlots = 0;
                maxLevelFiveSpellSlots = 0;
            }
            else if (scriptUnitStats.unitLevel == 5)
            {
                maxLevelOneSpellSlots = 4;
                maxLevelTwoSpellSlots = 3;
                maxLevelThreeSpellSlots = 2;
                maxLevelFourSpellSlots = 0;
                maxLevelFiveSpellSlots = 0;
            }
            else if (scriptUnitStats.unitLevel == 6)
            {
                maxLevelOneSpellSlots = 4;
                maxLevelTwoSpellSlots = 3;
                maxLevelThreeSpellSlots = 3;
                maxLevelFourSpellSlots = 0;
                maxLevelFiveSpellSlots = 0;
            }
            else if (scriptUnitStats.unitLevel == 7)
            {
                maxLevelOneSpellSlots = 4;
                maxLevelTwoSpellSlots = 3;
                maxLevelThreeSpellSlots = 3;
                maxLevelFourSpellSlots = 1;
                maxLevelFiveSpellSlots = 0;
            }
            else if (scriptUnitStats.unitLevel == 8)
            {
                maxLevelOneSpellSlots = 4;
                maxLevelTwoSpellSlots = 3;
                maxLevelThreeSpellSlots = 3;
                maxLevelFourSpellSlots = 2;
                maxLevelFiveSpellSlots = 0;
            }
            else if (scriptUnitStats.unitLevel == 9)
            {
                maxLevelOneSpellSlots = 4;
                maxLevelTwoSpellSlots = 3;
                maxLevelThreeSpellSlots = 3;
                maxLevelFourSpellSlots = 3;
                maxLevelFiveSpellSlots = 1;
            }
            else if (scriptUnitStats.unitLevel == 10)
            {
                maxLevelOneSpellSlots = 4;
                maxLevelTwoSpellSlots = 3;
                maxLevelThreeSpellSlots = 3;
                maxLevelFourSpellSlots = 3;
                maxLevelFiveSpellSlots = 2;
            }
        }
    }

    //Set Slots for Start Method
    public void SetCurrentSpellSlots()
    {
        currentLevelOneSpellSlots = maxLevelOneSpellSlots;
        currentLevelTwoSpellSlots = maxLevelTwoSpellSlots;
        currentLevelThreeSpellSlots = maxLevelThreeSpellSlots;
        currentLevelFourSpellSlots = maxLevelFourSpellSlots;
        currentLevelFiveSpellSlots = maxLevelFiveSpellSlots;
    }

    //Update After Use
    public void UpdateLevelOneCurrentSpellSlots()
    {
        currentLevelOneSpellSlots -= 1;
        StartCoroutine(CheckSpellSlots());
    }

    //Update After Use
    public void UpdateLevelTwoCurrentSpellSlots()
    {
        currentLevelTwoSpellSlots -= 1;
        StartCoroutine(CheckSpellSlots());
    }

    //Update After Use
    public void UpdateLevelThreeCurrentSpellSlots()
    {
        currentLevelThreeSpellSlots -= 1;
        StartCoroutine(CheckSpellSlots());
    }

    //Update After Use
    public void UpdateLevelFourCurrentSpellSlots()
    {
        currentLevelFourSpellSlots -= 1;
        StartCoroutine(CheckSpellSlots());
    }

    //Update After Use
    public void UpdateLevelFiveCurrentSpellSlots()
    {
        currentLevelFiveSpellSlots -= 1;
        StartCoroutine(CheckSpellSlots());
    }

    public IEnumerator CheckSpellSlots()
    {
        if (scriptUnitStats.unitName == "Baedaldas Springstar")
        {
            if (currentLevelOneSpellSlots == 0)
            {
                scriptGameMenuController.CloseLevelOneDruidSpellButtons();
            }
            else if (currentLevelTwoSpellSlots == 0)
            {

            }
            else if (currentLevelThreeSpellSlots == 0)
            {

            }
            else if (currentLevelFourSpellSlots == 0)
            {

            }
            else if (currentLevelFiveSpellSlots == 0)
            {

            }
        }

        yield return new WaitForEndOfFrame();
    }
}