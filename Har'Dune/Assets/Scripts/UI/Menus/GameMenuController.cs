using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameMenuController : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;
    public CharacterMenu scriptCharacterMenu;
    public OptionsMenu scriptOptionsMenu;
    public AttacksMenu scriptAttacksMenu;
    public ActionsMenu scriptActionsMenu;

    [Header("Menu Variables")]
    public static bool menuOpen = false;

    [Header("Main Menus")]
    public GameObject gameMenuPanel;
    public GameObject characterMenu;
    public GameObject attacksMenu;
    public GameObject actionsMenu;
    public GameObject optionsMenu;

    //Note -- Old & Remove When Possible
    [Header("PC Spellbooks")]
    public GameObject druidSpellbookPanel;
    public GameObject druidCureWoundsButton;
    public GameObject druidCharmPersonButton;

    [Header("NPC Menus")]
    public GameObject banditActionMenuPanel;
    public GameObject banditArcherActionMenuPanel;
    public GameObject banditCaptainActionMenuPanel;

    //Note - Move Spell SLots Out of this Script
    [Header("Druid Max Spell Slots")]
    public TMP_Text maxLevelOneSpellSlots;
    public TMP_Text maxLevelTwoSpellSlots;
    public TMP_Text maxLevelThreeSpellSlots;
    public TMP_Text maxLevelFourSpellSlots;
    public TMP_Text maxLevelFiveSpellSlots;

    [Header("Druid Current Spell Slots")]
    public TMP_Text currentLevelOneSpellSlots;
    public TMP_Text currentLevelTwoSpellSlots;
    public TMP_Text currentLevelThreeSpellSlots;
    public TMP_Text currentLevelFourSpellSlots;
    public TMP_Text currentLevelFiveSpellSlots;

    public void Awake()
    {
        SetScriptManager();
    }

    void Update()
    {
        //Game Menu Controls
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuOpen)
            {
                CloseAllMenus();
            }
            else
            {
                OpenGameMenu();
            }
        }

        //Close Game Menu
        if (Input.GetMouseButtonDown(1) && menuOpen)
        {
            CloseAllMenus();
            FindObjectOfType<AudioManager>().Play("Close Menu");
        }
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void CloseAllMenus()
    {
        characterMenu.SetActive(false);
        gameMenuPanel.SetActive(false);
        banditActionMenuPanel.SetActive(false); //Old -- Remove When Possible
        banditArcherActionMenuPanel.SetActive(false); //Old -- Remove When Possible
        banditCaptainActionMenuPanel.SetActive(false); //Old -- Remove When Possible
        druidSpellbookPanel.SetActive(false); //Old -- Remove When Possible
        actionsMenu.SetActive(false);
        attacksMenu.SetActive(false);
        menuOpen = false;
    }

    public void CancelButton()
    {
        CloseAllMenus();

        if (scriptManager.scriptTileMap.selectedUnit != null)
        {
            if (scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().movementQueue.Count == 0 && scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().combatQueue.Count == 0)
            {
                if (scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().unitMovementStates != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().GetMovementState(3))
                {
                    //unselectedSound.Play();
                    scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetIdleAnimation();
                    scriptManager.scriptUnitSelection.DeselectUnit();
                }
            }

            //Return Unit to Start Position
            else if (scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().movementQueue.Count == 1)
            {
                scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitStats>().visualMovementSpeed = 0.5f;
            }
        }
    }

    public void WaitButton()
    {
        scriptManager.scriptMovementController.DisableMovementRangeHighlight();
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().Wait();
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetWaitAnimation();
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetMovementState(3);
        scriptManager.scriptUnitSelection.DeselectUnit();
        CloseAllMenus();
    }

    public void OpenGameMenu()
    {
        //Get Mouse Position
        Vector3 mousePosition = Input.mousePosition;
        float x = mousePosition.x;
        float y = mousePosition.y;

        //Set Menu at Mouse Position
        gameMenuPanel.transform.position = new Vector3(x, y - 50f);

        //Set Variables
        gameMenuPanel.SetActive(true);
        menuOpen = true;
        FindObjectOfType<AudioManager>().Play("Open Menu");
    }

    public void CloseGameMenu()
    {
        gameMenuPanel.SetActive(false);
        menuOpen = false;
    }

    public void ResetMenuOptions()
    {
        scriptCharacterMenu.ResetCharacterMenuOptions();
        scriptAttacksMenu.ResetAttackMenuOptions();
    }













    // Old
    // Old
    // Old



    //PC Spellbooks
    //Druid Spellbook
    public void OpenDruidSpellbook()
    {
        StartCoroutine(GetMaxSpellSlots());
        StartCoroutine(GetCurrentSpellSlots());
        CloseAllMenus();
        druidSpellbookPanel.SetActive(true);
        menuOpen = true;
    }

    public IEnumerator GetMaxSpellSlots()
    {
        var unitStats = scriptManager.scriptTileMap.selectedUnit.GetComponent<SpellSlots>();

        maxLevelOneSpellSlots.SetText(unitStats.maxLevelOneSpellSlots.ToString());
        maxLevelTwoSpellSlots.SetText(unitStats.maxLevelTwoSpellSlots.ToString());
        maxLevelThreeSpellSlots.SetText(unitStats.maxLevelThreeSpellSlots.ToString());
        maxLevelFourSpellSlots.SetText(unitStats.maxLevelFourSpellSlots.ToString());
        maxLevelFiveSpellSlots.SetText(unitStats.maxLevelFiveSpellSlots.ToString());

        yield return new WaitForEndOfFrame();
    }

    public IEnumerator GetCurrentSpellSlots()
    {
        var unitStats = scriptManager.scriptTileMap.selectedUnit.GetComponent<SpellSlots>();

        currentLevelOneSpellSlots.SetText(unitStats.currentLevelOneSpellSlots.ToString());
        currentLevelTwoSpellSlots.SetText(unitStats.currentLevelTwoSpellSlots.ToString());
        currentLevelThreeSpellSlots.SetText(unitStats.currentLevelThreeSpellSlots.ToString());
        currentLevelFourSpellSlots.SetText(unitStats.currentLevelFourSpellSlots.ToString());
        currentLevelFiveSpellSlots.SetText(unitStats.currentLevelFiveSpellSlots.ToString());

        yield return new WaitForEndOfFrame();
    }

    public void CloseLevelOneDruidSpellButtons()
    {
        druidCureWoundsButton.SetActive(false);
        druidCharmPersonButton.SetActive(false);
    }

    //NPCs Action Menus
    //Bandit
    public void OpenBanditActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = characterMenu.transform.position.x;
        float y = characterMenu.transform.position.y;
        banditActionMenuPanel.transform.position = new Vector3(x, y);
        banditActionMenuPanel.SetActive(true);
        menuOpen = true;
    }
    //Bandit Archer
    public void OpenBanditArcherActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = characterMenu.transform.position.x;
        float y = characterMenu.transform.position.y;
        banditArcherActionMenuPanel.transform.position = new Vector3(x, y);
        banditArcherActionMenuPanel.SetActive(true);
        menuOpen = true;
    }
    //Bandit Captain
    public void OpenBanditCaptainActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = characterMenu.transform.position.x;
        float y = characterMenu.transform.position.y;
        banditCaptainActionMenuPanel.transform.position = new Vector3(x, y);
        banditCaptainActionMenuPanel.SetActive(true);
        menuOpen = true;
    }

}
