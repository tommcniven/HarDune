using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMenu : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Action Selection Options")]
    public GameObject button_spellBook;

    public void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void OpenCharacterMenu()
    {
        //Set Menus
        scriptManager.scriptGameMenuController.ResetMenuOptions();
        GetCharacterMenuOptions();

        //Get Mouse Position
        Vector3 mousePosition = Input.mousePosition;
        float x = mousePosition.x;
        float y = mousePosition.y;

        //Set Menu at Mouse Position
        scriptManager.scriptGameMenuController.characterMenu.transform.position = new Vector3(x + 175f, y);

        //Set Variables
        scriptManager.scriptGameMenuController.characterMenu.SetActive(true);
        GameMenuController.menuOpen = true;
    }

    public void CloseCharacterMenu()
    {
        scriptManager.scriptGameMenuController.characterMenu.SetActive(false);
    }

    public void GetCharacterMenuOptions()
    {
        string unitClass = scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitStats>().unitClass;

        if (unitClass == "Druid")
        {
            button_spellBook.gameObject.SetActive(true);
        }
    }

    public void ResetCharacterMenuOptions()
    {
        button_spellBook.gameObject.SetActive(false);
    }
}
