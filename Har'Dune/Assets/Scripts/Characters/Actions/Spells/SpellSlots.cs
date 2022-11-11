using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSlots : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;
    public SpellbookMenu scriptSpellBookMenu;

    [Header("Max Spell Slots")]
    public int max_Level1_SpellSlots;
    public int max_Level2_SpellSlots;
    public int max_Level3_SpellSlots;
    public int max_Level4_SpellSlots;
    public int max_Level5_SpellSlots;

    [Header("Current Spell Slots")]
    public int current_Level1_SpellSlots;
    public int current_Level2_SpellSlots;
    public int current_Level3_SpellSlots;
    public int current_Level4_SpellSlots;
    public int current_Level5_SpellSlots;

    public void Awake()
    {
        SetScriptManager();
        SetSpellBookMenu();
    }

    public void Start()
    {
        SetMaxSpellSlots();
        SetCurrentSpellSlots();
        StartCoroutine(CheckSpellSlots());
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void SetSpellBookMenu()
    {
        scriptSpellBookMenu = GameObject.Find("Game Menu Controller").GetComponent<SpellbookMenu>();
    }

    public void SetMaxSpellSlots()
    {
        if (transform.GetComponent<UnitStats>().unitClass == "Druid")
        {
            if (transform.GetComponent<UnitStats>().unitLevel == 1)
            {
                max_Level1_SpellSlots = 2;
                max_Level2_SpellSlots = 0;
                max_Level3_SpellSlots = 0;
                max_Level4_SpellSlots = 0;
                max_Level5_SpellSlots = 0;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 2)
            {
                max_Level1_SpellSlots = 3;
                max_Level2_SpellSlots = 0;
                max_Level3_SpellSlots = 0;
                max_Level4_SpellSlots = 0;
                max_Level5_SpellSlots = 0;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 3)
            {
                max_Level1_SpellSlots = 4;
                max_Level2_SpellSlots = 2;
                max_Level3_SpellSlots = 0;
                max_Level4_SpellSlots = 0;
                max_Level5_SpellSlots = 0;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 4)
            {
                max_Level1_SpellSlots = 4;
                max_Level2_SpellSlots = 3;
                max_Level3_SpellSlots = 0;
                max_Level4_SpellSlots = 0;
                max_Level5_SpellSlots = 0;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 5)
            {
                max_Level1_SpellSlots = 4;
                max_Level2_SpellSlots = 3;
                max_Level3_SpellSlots = 2;
                max_Level4_SpellSlots = 0;
                max_Level5_SpellSlots = 0;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 6)
            {
                max_Level1_SpellSlots = 4;
                max_Level2_SpellSlots = 3;
                max_Level3_SpellSlots = 3;
                max_Level4_SpellSlots = 0;
                max_Level5_SpellSlots = 0;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 7)
            {
                max_Level1_SpellSlots = 4;
                max_Level2_SpellSlots = 3;
                max_Level3_SpellSlots = 3;
                max_Level4_SpellSlots = 1;
                max_Level5_SpellSlots = 0;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 8)
            {
                max_Level1_SpellSlots = 4;
                max_Level2_SpellSlots = 3;
                max_Level3_SpellSlots = 3;
                max_Level4_SpellSlots = 2;
                max_Level5_SpellSlots = 0;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 9)
            {
                max_Level1_SpellSlots = 4;
                max_Level2_SpellSlots = 3;
                max_Level3_SpellSlots = 3;
                max_Level4_SpellSlots = 3;
                max_Level5_SpellSlots = 1;
            }

            else if (transform.GetComponent<UnitStats>().unitLevel == 10)
            {
                max_Level1_SpellSlots = 4;
                max_Level2_SpellSlots = 3;
                max_Level3_SpellSlots = 3;
                max_Level4_SpellSlots = 3;
                max_Level5_SpellSlots = 2;
            }
        }
    }

    //Set Slots for Start Method
    public void SetCurrentSpellSlots()
    {
        current_Level1_SpellSlots = max_Level1_SpellSlots;
        current_Level2_SpellSlots = max_Level2_SpellSlots;
        current_Level3_SpellSlots = max_Level3_SpellSlots;
        current_Level4_SpellSlots = max_Level4_SpellSlots;
        current_Level5_SpellSlots = max_Level5_SpellSlots;
    }

    public void UpdateSpellSlots()
    {
        if (scriptManager.scriptSpellcasting.spellReference.spell.spellLevel == 0)
        {
            return;
        }

        if (scriptManager.scriptSpellcasting.spellReference.spell.spellLevel == 1)
        {
            current_Level1_SpellSlots -= 1;
            StartCoroutine(CheckSpellSlots());
        }

        if (scriptManager.scriptSpellcasting.spellReference.spell.spellLevel == 2)
        {
            current_Level2_SpellSlots -= 1;
            StartCoroutine(CheckSpellSlots());
        }

        if (scriptManager.scriptSpellcasting.spellReference.spell.spellLevel == 3)
        {
            current_Level3_SpellSlots -= 1;
            StartCoroutine(CheckSpellSlots());
        }

        if (scriptManager.scriptSpellcasting.spellReference.spell.spellLevel == 4)
        {
            current_Level4_SpellSlots -= 1;
            StartCoroutine(CheckSpellSlots());
        }

        if (scriptManager.scriptSpellcasting.spellReference.spell.spellLevel == 5)
        {
            current_Level5_SpellSlots -= 1;
            StartCoroutine(CheckSpellSlots());
        }
    }

    public IEnumerator CheckSpellSlots()
    {
        if (current_Level1_SpellSlots == 0) //Note -- Update to Phase to Gray post MVP
        {
            scriptSpellBookMenu.button_IceKnife.SetActive(false);
            scriptSpellBookMenu.button_HealingWord.SetActive(false);
            scriptSpellBookMenu.button_CharmPerson.SetActive(false);
        }

        else if (current_Level2_SpellSlots == 0) //Note -- Update to Phase to Gray post MVP
        {
            scriptSpellBookMenu.button_Barkskin.SetActive(false);
            scriptSpellBookMenu.button_HoldPerson.SetActive(false);
        }

        else if (current_Level3_SpellSlots == 0)
        {

        }

        else if (current_Level4_SpellSlots == 0)
        {

        }

        else if (current_Level5_SpellSlots == 0)
        {

        }

        yield return new WaitForEndOfFrame();
    }
}