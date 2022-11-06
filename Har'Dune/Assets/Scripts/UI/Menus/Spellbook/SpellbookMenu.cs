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

        current_1stLevelSpellSlots.SetText(unitSpellSlots.currentLevelOneSpellSlots.ToString());
        current_2ndLevelSpellSlots.SetText(unitSpellSlots.currentLevelTwoSpellSlots.ToString());

        yield return new WaitForEndOfFrame();
    }

    public IEnumerator SetMaxSpellSlots()
    {
        SpellSlots unitSpellSlots = scriptManager.scriptTileMap.selectedUnit.GetComponent<SpellSlots>();

        max_1stLevelSpellSlots.SetText(unitSpellSlots.maxLevelOneSpellSlots.ToString());
        max_2ndLevelSpellSlots.SetText(unitSpellSlots.maxLevelTwoSpellSlots.ToString());

        yield return new WaitForEndOfFrame();
    }

}
