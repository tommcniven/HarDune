using UnityEngine.Audio;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    //Referenced in Game Controller Script
    [Header("Cursor Information")]
    public Texture2D cursorIcon;
    public int cursorX;
    public int cursorY;
    public int tileUnderCursorX;
    public int tileUnderCursorY;

    void Awake()
    {
        SetCursor(cursorIcon);
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    private void SetCursor (Texture2D mouseCursor)
    {
        Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.Auto);
    }

    public void UpdateCursorUI()
    {
        //Reference Variables
        RaycastHit hit = scriptManager.scriptGameController.hit;

        //Highlight Mouseover Tiles
        if (hit.transform.CompareTag("Tile"))
        {
            //If Not Displayed
            if (scriptManager.scriptGameController.tileDisplayed == null)
            {
                tileUnderCursorX = hit.transform.gameObject.GetComponent<ClickableTile>().tileX;
                tileUnderCursorY = hit.transform.gameObject.GetComponent<ClickableTile>().tileY;
                cursorX = tileUnderCursorX;
                cursorY = tileUnderCursorY;
                scriptManager.scriptTileMap.cursorTiles[tileUnderCursorX, tileUnderCursorY].GetComponent<MeshRenderer>().enabled = true;
                scriptManager.scriptGameController.tileDisplayed = hit.transform.gameObject;
            }

            //If Mousover is Not a gameObject
            else if (scriptManager.scriptGameController.tileDisplayed != hit.transform.gameObject)
            {
                tileUnderCursorX = scriptManager.scriptGameController.tileDisplayed.GetComponent<ClickableTile>().tileX;
                tileUnderCursorY = scriptManager.scriptGameController.tileDisplayed.GetComponent<ClickableTile>().tileY;
                scriptManager.scriptTileMap.cursorTiles[tileUnderCursorX, tileUnderCursorY].GetComponent<MeshRenderer>().enabled = false;
                tileUnderCursorX = hit.transform.gameObject.GetComponent<ClickableTile>().tileX;
                tileUnderCursorY = hit.transform.gameObject.GetComponent<ClickableTile>().tileY;
                cursorX = tileUnderCursorX;
                cursorY = tileUnderCursorY;
                scriptManager.scriptTileMap.cursorTiles[tileUnderCursorX, tileUnderCursorY].GetComponent<MeshRenderer>().enabled = true;
                scriptManager.scriptGameController.tileDisplayed = hit.transform.gameObject;
            }
        }

        //Highlight Tile Under Units
        else if (hit.transform.CompareTag("Unit"))
        {
            //If Not Displayed
            if (scriptManager.scriptGameController.tileDisplayed == null)
            {
                tileUnderCursorX = hit.transform.parent.gameObject.GetComponent<UnitController>().x;
                tileUnderCursorY = hit.transform.parent.gameObject.GetComponent<UnitController>().y;
                cursorX = tileUnderCursorX;
                cursorY = tileUnderCursorY;
                scriptManager.scriptTileMap.cursorTiles[tileUnderCursorX, tileUnderCursorY].GetComponent<MeshRenderer>().enabled = true;
                scriptManager.scriptGameController.tileDisplayed = hit.transform.parent.gameObject.GetComponent<UnitController>().occupiedTile;
            }

            //If Mouseover is Not a gameObject
            else if (scriptManager.scriptGameController.tileDisplayed != hit.transform.gameObject)
            {
                if (hit.transform.parent.gameObject.GetComponent<UnitController>().movementQueue.Count == 0)
                {
                    tileUnderCursorX = scriptManager.scriptGameController.tileDisplayed.GetComponent<ClickableTile>().tileX;
                    tileUnderCursorY = scriptManager.scriptGameController.tileDisplayed.GetComponent<ClickableTile>().tileY;
                    scriptManager.scriptTileMap.cursorTiles[tileUnderCursorX, tileUnderCursorY].GetComponent<MeshRenderer>().enabled = false;
                    tileUnderCursorX = hit.transform.parent.gameObject.GetComponent<UnitController>().x;
                    tileUnderCursorY = hit.transform.parent.gameObject.GetComponent<UnitController>().y;
                    cursorX = tileUnderCursorX;
                    cursorY = tileUnderCursorY;
                    scriptManager.scriptTileMap.cursorTiles[tileUnderCursorX, tileUnderCursorY].GetComponent<MeshRenderer>().enabled = true;
                    scriptManager.scriptGameController.tileDisplayed = hit.transform.parent.GetComponent<UnitController>().occupiedTile;
                }
            }
        }

        //No Tile Under Cursor
        else
        {
            scriptManager.scriptTileMap.cursorTiles[tileUnderCursorX, tileUnderCursorY].GetComponent<MeshRenderer>().enabled = false;
        }

        //Close Game Menu
        if (GameMenuController.menuOpen)
        {
            scriptManager.scriptTileMap.cursorTiles[tileUnderCursorX, tileUnderCursorY].GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
