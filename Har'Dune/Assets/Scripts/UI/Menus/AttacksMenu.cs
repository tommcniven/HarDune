using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttacksMenu : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Weapon Attack Options")]
    public GameObject button_DaggerThrow;
    public GameObject button_Dagger;
    public GameObject button_Greatsword;
    public GameObject button_LightCrossbow;
    public GameObject button_Quarterstaff;
    public GameObject button_Scimitar;

    public void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void OpenAttacksMenu()
    {
        //Set Menus
        scriptManager.scriptGameMenuController.scriptCharacterMenu.CloseCharacterMenu();
        GetAttacksMenuOptions();

        //Set Menu to Character Menu Position
        float x = scriptManager.scriptGameMenuController.characterMenu.transform.position.x;
        float y = scriptManager.scriptGameMenuController.characterMenu.transform.position.y;
        scriptManager.scriptGameMenuController.attacksMenu.transform.position = new Vector3(x, y);

        //Set Variables
        scriptManager.scriptGameMenuController.attacksMenu.SetActive(true);
        GameMenuController.menuOpen = true;
    }

    public void CloseAttacksMenu()
    {
        scriptManager.scriptGameMenuController.attacksMenu.SetActive(false);
    }

    //Wishlist -- Redesign to Check Selected Unit's Inventory (rather than class)
    public void GetAttacksMenuOptions()
    {
        string unitClass = scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitStats>().unitClass;

        if (unitClass == "Fighter")
        {
            button_Greatsword.gameObject.SetActive(true);
        }

        if (unitClass == "Rogue")
        {
            button_Dagger.gameObject.SetActive(true);
            button_DaggerThrow.gameObject.SetActive(true);
        }

        if (unitClass == "Druid")
        {
            button_Quarterstaff.gameObject.SetActive(true);
        }

        if (unitClass == "Bandit Archer")
        {
            button_Dagger.gameObject.SetActive(true);
            button_LightCrossbow.gameObject.SetActive(true);
        }

        if (unitClass == "Bandit Captain")
        {
            button_Scimitar.gameObject.SetActive(true);
            button_Dagger.gameObject.SetActive(true);
            button_DaggerThrow.gameObject.SetActive(true);
        }

        if (unitClass == "Bandit")
        {
            button_Dagger.gameObject.SetActive(true);
            button_DaggerThrow.gameObject.SetActive(true);
        }
    }

    public void ResetAttackMenuOptions()
    {
        button_DaggerThrow.gameObject.SetActive(false);
        button_Dagger.gameObject.SetActive(false);
        button_Greatsword.gameObject.SetActive(false);
        button_LightCrossbow.gameObject.SetActive(false);
        button_Quarterstaff.gameObject.SetActive(false);
        button_Scimitar.gameObject.SetActive(false);
    }
}
