using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpellbookMenu : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;
    public SpellReference scriptSpellReference;

    [Header("Spellbook Menu")]
    public GameObject spellbookMenu;

    [Header("Spells")]
    public GameObject button_Frostbite;
    public GameObject button_Guidance;
    public GameObject button_IceKnife;
    public GameObject button_HealingWord;
    public GameObject button_CharmPerson;
    public GameObject button_Barkskin;
    public GameObject button_HoldPerson;

    [Header("Current Spell Slots")]
    public TMP_Text current_1stLevelSpellSlots;
    public TMP_Text current_2ndLevelSpellSlots;

    [Header("Max Spell Slots")]
    public TMP_Text max_1stLevelSpellSlots;
    public TMP_Text max_2ndLevelSpellSlots;

    public void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    
    public void OpenSpellbookMenu()
    {
        StartCoroutine(SetMaxSpellSlots());
        StartCoroutine(SetCurrentSpellSlots());
        scriptManager.scriptGameMenuController.CloseAllMenus();
        spellbookMenu.SetActive(true);
        GameMenuController.menuOpen = true;
    }
    
    public IEnumerator SetCurrentSpellSlots()
    {
        SpellSlots unitSpellSlots = scriptManager.scriptTileMap.selectedUnit.GetComponent<SpellSlots>();

        current_1stLevelSpellSlots.SetText(unitSpellSlots.current_Level1_SpellSlots.ToString());
        current_2ndLevelSpellSlots.SetText(unitSpellSlots.current_Level2_SpellSlots.ToString());

        yield return new WaitForEndOfFrame();
    }

    public IEnumerator SetMaxSpellSlots()
    {
        SpellSlots unitSpellSlots = scriptManager.scriptTileMap.selectedUnit.GetComponent<SpellSlots>();

        max_1stLevelSpellSlots.SetText(unitSpellSlots.max_Level1_SpellSlots.ToString());
        max_2ndLevelSpellSlots.SetText(unitSpellSlots.max_Level2_SpellSlots.ToString());

        yield return new WaitForEndOfFrame();
    }

}
