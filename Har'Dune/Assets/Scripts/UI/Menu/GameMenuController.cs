using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GameMenuController : MonoBehaviour
{
    //Variables
    [Header("Scripts")]
    public TileMap scriptTileMap;
    public UnitStats scriptUnitStats;
    public MovementController scriptMovementController;
    public ScriptManager scriptManager;

    [Header("Main Menus")]
    public GameObject gameMenuPanel;
    public GameObject actionMenuPanel;

    [Header("PC Menus")]
    public GameObject fighterActionMenuPanel;
    public GameObject rogueActionMenuPanel;
    public GameObject druidActionMenuPanel;

    [Header("PC Spellbooks")]
    public GameObject druidSpellbookPanel;
    public GameObject druidCureWoundsButton;
    public GameObject druidCharmPersonButton;

    [Header("NPC Menus")]
    public GameObject banditActionMenuPanel;
    public GameObject banditArcherActionMenuPanel;
    public GameObject banditCaptainActionMenuPanel;

    [Header("Menu Variables")]
    public static bool menuOpen = false;

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
                CloseGameMenu();
                CloseAllActionMenus();
            }
            else
            {
                OpenGameMenu();
            }
        }

        //Close Game Menu
        if (Input.GetMouseButtonDown(1) && menuOpen)
        {
            CloseGameMenu();
            CloseAllActionMenus();
            FindObjectOfType<AudioManager>().Play("Close Menu");
        }
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void OpenGameMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = mousePosition.x;
        float y = mousePosition.y;
        gameMenuPanel.transform.position = new Vector3(x, y - 50f);
        gameMenuPanel.SetActive(true);
        menuOpen = true;
        FindObjectOfType<AudioManager>().Play("Open Menu");
    }

    //Close Game Menu
    public void CloseGameMenu()
    {
        gameMenuPanel.SetActive(false);
        menuOpen = false;
    }


    //Open Options Menu
    public void OpenOptionsMenu()
    {

    }

    //
    //Action Menu
    //

    //Open Action Menu
    public void OpenActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = mousePosition.x;
        float y = mousePosition.y;
        actionMenuPanel.transform.position = new Vector3(x + 175f, y);
        actionMenuPanel.SetActive(true);
        menuOpen = true;
    }

    public void CloseMainActionMenu()
    {
        actionMenuPanel.SetActive(false);
    }

    //Close Action Menu For Cancel Buttons
    public void CloseAllActionMenus()
    {
        actionMenuPanel.SetActive(false);
        fighterActionMenuPanel.SetActive(false);
        rogueActionMenuPanel.SetActive(false);
        druidActionMenuPanel.SetActive(false);
        banditActionMenuPanel.SetActive(false);
        banditArcherActionMenuPanel.SetActive(false);
        banditCaptainActionMenuPanel.SetActive(false);
        druidSpellbookPanel.SetActive(false);
        menuOpen = false;
    }

    //Cancel Main Action Menu Button
    public void CancelActionMenuButton()
    {
        CloseAllActionMenus();

        if (scriptTileMap.selectedUnit != null)
        {
            if (scriptTileMap.selectedUnit.GetComponent<UnitController>().movementQueue.Count == 0 && scriptTileMap.selectedUnit.GetComponent<UnitController>().combatQueue.Count == 0)
            {
                if (scriptTileMap.selectedUnit.GetComponent<UnitController>().unitMovementStates != scriptTileMap.selectedUnit.GetComponent<UnitController>().GetMovementState(3))
                {
                    //unselectedSound.Play();
                    scriptTileMap.selectedUnit.GetComponent<UnitController>().SetIdleAnimation();
                    scriptManager.scriptUnitSelection.DeselectUnit();
                }
            }

            //Return Unit to Start Position
            else if (scriptTileMap.selectedUnit.GetComponent<UnitController>().movementQueue.Count == 1)
            {
                scriptTileMap.selectedUnit.GetComponent<UnitStats>().visualMovementSpeed = 0.5f;
            }
        }
    }

    //Wait Action Menu Button
    public void Wait()
    {
        scriptMovementController.DisableMovementRangeHighlight();
        scriptTileMap.selectedUnit.GetComponent<UnitController>().Wait();
        scriptTileMap.selectedUnit.GetComponent<UnitController>().SetWaitAnimation();
        scriptTileMap.selectedUnit.GetComponent<UnitController>().SetMovementState(3);
        scriptManager.scriptUnitSelection.DeselectUnit();
        CloseAllActionMenus();
    }

    //Get Class Action Menu Based on Unit Class
    public void GetClassActionMenuOptions()
    {
        //PCs
        //Fighter
        string unitClass = scriptTileMap.selectedUnit.GetComponent<UnitStats>().unitClass;

        if (unitClass == "Fighter")
        {
            OpenFighterActionMenu();
        }
        //Rogue
        else if (unitClass == "Rogue")
        {
            OpenRogueActionMenu();
        }
        //Druid
        else if (unitClass == "Druid")
        {
            OpenDruidActionMenu();
        }

        //NPCs
        //Bandit
        else if (unitClass == "Bandit")
        {
            OpenBanditActionMenu();
        }
        //Bandit Archer
        else if (unitClass == "Bandit Archer")
        {
            OpenBanditArcherActionMenu();
        }
        //Bandit Captain
        else if (unitClass == "Bandit Captain")
        {
            OpenBanditCaptainActionMenu();
        }

    }

    //Set Class Action Menu on Action Menu Button
    public void SetClassActionMenu()
    {
        GetClassActionMenuOptions();
        CloseMainActionMenu();
    }

    //PCs Action Menus
    //Fighter
    public void OpenFighterActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = actionMenuPanel.transform.position.x;
        float y = actionMenuPanel.transform.position.y;
        fighterActionMenuPanel.transform.position = new Vector3(x, y);
        fighterActionMenuPanel.SetActive(true);
        menuOpen = true;
    }
    //Rogue
    public void OpenRogueActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = actionMenuPanel.transform.position.x;
        float y = actionMenuPanel.transform.position.y;
        rogueActionMenuPanel.transform.position = new Vector3(x, y);
        rogueActionMenuPanel.SetActive(true);
        menuOpen = true;
    }
    //Druid
    public void OpenDruidActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = actionMenuPanel.transform.position.x;
        float y = actionMenuPanel.transform.position.y;
        druidActionMenuPanel.transform.position = new Vector3(x, y);
        druidActionMenuPanel.SetActive(true);
        menuOpen = true;
    }

    //PC Spellbooks
    //Druid Spellbook
    public void OpenDruidSpellbook()
    {
        StartCoroutine(GetMaxSpellSlots());
        StartCoroutine(GetCurrentSpellSlots());
        CloseAllActionMenus();
        druidSpellbookPanel.SetActive(true);
        menuOpen = true;
    }

    public IEnumerator GetMaxSpellSlots()
    {
        var unitStats = scriptTileMap.selectedUnit.GetComponent<SpellSlots>();

        maxLevelOneSpellSlots.SetText(unitStats.maxLevelOneSpellSlots.ToString());
        maxLevelTwoSpellSlots.SetText(unitStats.maxLevelTwoSpellSlots.ToString());
        maxLevelThreeSpellSlots.SetText(unitStats.maxLevelThreeSpellSlots.ToString());
        maxLevelFourSpellSlots.SetText(unitStats.maxLevelFourSpellSlots.ToString());
        maxLevelFiveSpellSlots.SetText(unitStats.maxLevelFiveSpellSlots.ToString());

        yield return new WaitForEndOfFrame();
    }

    public IEnumerator GetCurrentSpellSlots()
    {
        var unitStats = scriptTileMap.selectedUnit.GetComponent<SpellSlots>();

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
        float x = actionMenuPanel.transform.position.x;
        float y = actionMenuPanel.transform.position.y;
        banditActionMenuPanel.transform.position = new Vector3(x, y);
        banditActionMenuPanel.SetActive(true);
        menuOpen = true;
    }
    //Bandit Archer
    public void OpenBanditArcherActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = actionMenuPanel.transform.position.x;
        float y = actionMenuPanel.transform.position.y;
        banditArcherActionMenuPanel.transform.position = new Vector3(x, y);
        banditArcherActionMenuPanel.SetActive(true);
        menuOpen = true;
    }
    //Bandit Captain
    public void OpenBanditCaptainActionMenu()
    {
        Vector3 mousePosition = Input.mousePosition;
        float x = actionMenuPanel.transform.position.x;
        float y = actionMenuPanel.transform.position.y;
        banditCaptainActionMenuPanel.transform.position = new Vector3(x, y);
        banditCaptainActionMenuPanel.SetActive(true);
        menuOpen = true;
    }

}
