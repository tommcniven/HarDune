using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Unit Global Variable")]
    public GameObject tileDisplayed;

    [Header("Winner Canvas")] //Move to New Script (WinLossUIDisplay)
    public Canvas winnerCanvasUI;
    public bool isGameOver = false;

    [Header("Location Tracking")]
    private Ray ray;
    public RaycastHit hit;

    [Header("Movement Path")]
    public List<Node> unitPathToCursor = new List<Node>();
    public bool doesUnitPathExist = false;
    public int pathToX;
    public int pathToY;

    public void Awake()
    {
        SetScriptManager();
    }

    public void Update()
    {
        //Get Cursor Location
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            scriptManager.scriptCursorController.UpdateCursorUI();
            scriptManager.scriptUnitUIDisplay.UpdateUnitUI();

            //Highlight Current Path if Unit Selected
            if (scriptManager.scriptTileMap.selectedUnit != null && scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().GetMovementState(1) == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().unitMovementStates)
            {
                //Check if Cursor is Within Movement Range
                if (scriptManager.scriptTileMap.selectedUnitMovementRange.Contains(scriptManager.scriptTileMap.tileGraph[scriptManager.scriptCursorController.cursorX, scriptManager.scriptCursorController.cursorY]))
                {
                    //Generate New Path to Cursor
                    if (scriptManager.scriptCursorController.cursorX != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x || scriptManager.scriptCursorController.cursorY != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y)
                    {
                        //Check Movement Queue
                        if (!doesUnitPathExist && scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().movementQueue.Count == 0)
                        {
                            unitPathToCursor = scriptManager.scriptPathFinder.GenerateRouteToCursor(scriptManager.scriptCursorController.cursorX, scriptManager.scriptCursorController.cursorY);
                            pathToX = scriptManager.scriptCursorController.cursorX;
                            pathToY = scriptManager.scriptCursorController.cursorY;

                            //If Movement Available
                            if (unitPathToCursor.Count != 0)
                            {
                                //Set Node [x,y]
                                for(int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    //Set Cursor UI
                                    if (i == 0)
                                    {
                                        GameObject quadToUpdate = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
                                        quadToUpdate.GetComponent<Renderer>().material = scriptManager.scriptPathFinder.cursorUI;
                                    }

                                    //Set Tile Indicator Excluding First & Last Tiles
                                    else if (i!=0 && (i+1)!=unitPathToCursor.Count)
                                    {
                                        scriptManager.scriptPathFinder.SetRouteArrow(nodeX, nodeY,i);
                                    }

                                    //Set Tile Indiciator For Last Tile
                                    else if (i == unitPathToCursor.Count-1)
                                    {
                                        scriptManager.scriptPathFinder.SetRouteArrowTip(nodeX, nodeY, i);
                                    }

                                    scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY].GetComponent<Renderer>().enabled = true;
                                }
                            }

                            doesUnitPathExist = true;
                        }

                        //Check if Route & Cursor Are Same
                        else if (pathToX != scriptManager.scriptCursorController.cursorX || pathToY != scriptManager.scriptCursorController.cursorY)
                        {
                            //Path is Not Cursor
                            if (unitPathToCursor.Count != 0)
                            {
                                //Set Node[x,y]
                                for (int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY].GetComponent<Renderer>().enabled = false;
                                }
                            }
                            
                            doesUnitPathExist = false;
                        }
                    }

                    //Disable Route
                    else if(scriptManager.scriptCursorController.cursorX == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x && scriptManager.scriptCursorController.cursorY == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y)
                    {
                        scriptManager.scriptTileMap.DisableUnitRouteUI();
                        doesUnitPathExist = false;
                    }
                }               
            }
        }
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void CheckIfUnitsRemain(GameObject unit, GameObject enemy)
    {
        StartCoroutine(CheckIfUnitsRemainEnum(unit,enemy));
    }

    public IEnumerator CheckIfUnitsRemainEnum(GameObject unit, GameObject enemy)
    {
        //Player Combat in Queue
        while (unit.GetComponent<UnitController>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        //Enemy Combat in Queue
        while (enemy.GetComponent<UnitController>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }

        //All PC Units are Dead
        if (scriptManager.scriptTurnController.PCTeam.transform.childCount == 0)
        {
            winnerCanvasUI.enabled = true;
            isGameOver = true;
            winnerCanvasUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Defeat");
        }

        //All NPC Units are Dead
        else if (scriptManager.scriptTurnController.NPCTeam.transform.childCount == 0)
        {
            winnerCanvasUI.enabled = true;
            isGameOver = true;
            winnerCanvasUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Victory");
        }
    }
}
