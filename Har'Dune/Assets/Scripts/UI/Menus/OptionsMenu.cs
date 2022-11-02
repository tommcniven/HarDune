using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
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

    public void OpenOptionsMenu() //Note -- Not Set in Inspector
    {
        //Get Mouse Position
        Vector3 mousePosition = Input.mousePosition;
        float x = mousePosition.x;
        float y = mousePosition.y;

        //Set Menu at Mouse Position
        scriptManager.scriptGameMenuController.optionsMenu.transform.position = new Vector3(x, y - 50f);

        //Set Variables
        scriptManager.scriptGameMenuController.optionsMenu.SetActive(true);
        GameMenuController.menuOpen = true;
    }

    public void CloseOptionsMenu()
    {
        scriptManager.scriptGameMenuController.optionsMenu.SetActive(false);
        GameMenuController.menuOpen = false;
    }
}
