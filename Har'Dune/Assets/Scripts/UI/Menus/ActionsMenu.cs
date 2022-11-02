using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsMenu : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    public void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }


    public void OpenActionsMenu()
    {
        //Set Menus
        scriptManager.scriptGameMenuController.scriptCharacterMenu.CloseCharacterMenu();

        //Set Menu to Character Menu Position
        float x = scriptManager.scriptGameMenuController.characterMenu.transform.position.x;
        float y = scriptManager.scriptGameMenuController.characterMenu.transform.position.y;
        scriptManager.scriptGameMenuController.actionsMenu.transform.position = new Vector3(x, y);

        //Set Variables
        scriptManager.scriptGameMenuController.actionsMenu.SetActive(true);
        GameMenuController.menuOpen = true;
    }

    public void CloseActionsMenu()
    {
        scriptManager.scriptGameMenuController.actionsMenu.SetActive(false);
    }
}
