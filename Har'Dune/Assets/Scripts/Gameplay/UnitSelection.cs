using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
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

    public void ClickToSelectUnit()
    {
        //No Unit Selected & Tile is Displayed
        if (scriptManager.scriptTileMap.isUnitSelected == false && scriptManager.scriptGameController.tileDisplayed != null)
        {
            //Unit on Tile
            if (scriptManager.scriptGameController.tileDisplayed.GetComponent<ClickableTile>().unitOnTile != null)
            {
                //Select Unit on Tile
                GameObject tempSelectedUnit = scriptManager.scriptGameController.tileDisplayed.GetComponent<ClickableTile>().unitOnTile;

                //Selected Unit Movement State is 0
                if (tempSelectedUnit.GetComponent<UnitController>().unitMovementStates == tempSelectedUnit.GetComponent<UnitController>().GetMovementState(0))
                {
                    if (tempSelectedUnit.GetComponent<UnitController>().teamNumber == scriptManager.scriptGameController.currentTeam)
                    {
                        scriptManager.scriptMovementController.DisableMovementRangeHighlight();
                        //selectedSound.Play();
                        //Set Selected Unit & Setup UI
                        scriptManager.scriptTileMap.selectedUnit = tempSelectedUnit;
                        scriptManager.scriptTileMap.SetThisState();
                        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetMovementState(1);
                        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetSelectedAnimation();
                        scriptManager.scriptTileMap.isUnitSelected = true;
                        scriptManager.scriptRangeFinder.HighlightUnitRange();
                    }
                }
            }

            //No Unit on Tile
            else if (scriptManager.scriptGameController.tileDisplayed.GetComponent<ClickableTile>().unitOnTile == null)
            {
                if (GameMenuController.menuOpen == false && scriptManager.scriptGameController.isGameOver == false)
                {
                    scriptManager.scriptGameMenuController.OpenGameMenu();
                }
            }
        }
    }

    public void DeselectUnit()
    {
        //A Unit is Selected
        if (scriptManager.scriptTileMap.selectedUnit != null)
        {
            //Unit Movment State is Selected
            if (scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().unitMovementStates == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().GetMovementState(1))
            {
                scriptManager.scriptMovementController.DisableMovementRangeHighlight();
                scriptManager.scriptTileMap.DisableUnitRouteUI();
                scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetMovementState(0);
                scriptManager.scriptTileMap.selectedUnit = null;
                scriptManager.scriptTileMap.isUnitSelected = false;
            }

            //Unit Movement State is Wait
            else if (scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().unitMovementStates == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().GetMovementState(3))
            {
                scriptManager.scriptMovementController.DisableMovementRangeHighlight();
                scriptManager.scriptTileMap.DisableUnitRouteUI();
                scriptManager.scriptTileMap.isUnitSelected = false;
                scriptManager.scriptTileMap.selectedUnit = null;
            }

            //Unit Movement State is Not Selected or Wait
            else
            {
                scriptManager.scriptMovementController.DisableMovementRangeHighlight();
                scriptManager.scriptTileMap.DisableUnitRouteUI();
                scriptManager.scriptTileMap.tilesOnMap[scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x, scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y].GetComponent<ClickableTile>().unitOnTile = null;
                scriptManager.scriptTileMap.tilesOnMap[scriptManager.scriptTileMap.selectedUnitPreviousX, scriptManager.scriptTileMap.selectedUnitPreviousY].GetComponent<ClickableTile>().unitOnTile = scriptManager.scriptTileMap.selectedUnit;
                scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x = scriptManager.scriptTileMap.selectedUnitPreviousX;
                scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y = scriptManager.scriptTileMap.selectedUnitPreviousY;
                scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().occupiedTile = scriptManager.scriptTileMap.previousOccupiedTile;
                scriptManager.scriptTileMap.selectedUnit.transform.position = scriptManager.scriptTileMap.NodePositionInScene(scriptManager.scriptTileMap.selectedUnitPreviousX, scriptManager.scriptTileMap.selectedUnitPreviousY);
                scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetMovementState(0);
                scriptManager.scriptTileMap.selectedUnit = null;
                scriptManager.scriptTileMap.isUnitSelected = false;
            }
        }
    }


    //Play Sound, Set Animation to Wait, Disable Highlight & Route, & Deselect Unit Enum
    public IEnumerator DeselectUnitAfterMovement(GameObject unit, GameObject enemy)
    {
        //selectedSound.Play();
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetMovementState(3);
        scriptManager.scriptMovementController.DisableMovementRangeHighlight();
        scriptManager.scriptTileMap.DisableUnitRouteUI();

        //Required Yield to Avoid Errors
        yield return new WaitForSeconds(.25f);

        //Wait for Unit Combat to Complete
        while (unit.GetComponent<UnitController>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        //Wait for Enemy Combat to Complete
        while (enemy.GetComponent<UnitController>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();

        }

        scriptManager.scriptUnitSelection.DeselectUnit();
    }
}
