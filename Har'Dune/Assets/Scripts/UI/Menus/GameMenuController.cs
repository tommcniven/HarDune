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
    public SpellbookMenu scriptSpellbookMenu;

    [Header("Menu Variables")]
    public static bool menuOpen = false;

    [Header("Main Menus")]
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject characterMenu;
    public GameObject attacksMenu;
    public GameObject actionsMenu;
    public GameObject spellbookMenu;

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
                OpenMainMenu();
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
        mainMenu.SetActive(false);
        //optionsMenu.SetActive(false);
        characterMenu.SetActive(false);
        attacksMenu.SetActive(false);
        actionsMenu.SetActive(false);
        spellbookMenu.SetActive(false);
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

    public void OpenMainMenu()
    {
        //Get Mouse Position
        Vector3 mousePosition = Input.mousePosition;
        float x = mousePosition.x;
        float y = mousePosition.y;

        //Set Menu at Mouse Position
        mainMenu.transform.position = new Vector3(x, y - 50f);

        //Set Variables
        mainMenu.SetActive(true);
        menuOpen = true;
        FindObjectOfType<AudioManager>().Play("Open Menu");
    }

    public void CloseMainMenu()
    {
        mainMenu.SetActive(false);
        menuOpen = false;
    }

    public void ResetMenuOptions()
    {
        scriptCharacterMenu.ResetCharacterMenuOptions();
        scriptAttacksMenu.ResetAttackMenuOptions();
    }    
}
