using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralActions : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("UnitReferences")]
    public int actionModifier;
    public static event Action onDodge;

    public void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public int GetActionModifier(GameObject unit)
    {
        if (scriptManager.scriptBattleController.isGrappling == true)
        {
            actionModifier = unit.GetComponent<UnitStats>().strengthModifier;
        }

        return actionModifier;
    }



    // Actions //
    // Actions //
    // Actions //



    public void GrappleAction() //Called from Actions Menu Button
    {
        scriptManager.scriptBattleController.isGrappling = true;
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitStats>().attackRange = 1;
        SetVariables();
        HighlightActionRange();
    }

    public void HideAction() //Called from Actions Menu Button
    {
        //Set Variables
        scriptManager.scriptBattleController.isHiding = true;
        GameObject selectedUnit = scriptManager.scriptTileMap.selectedUnit;
        int dexModifier = selectedUnit.GetComponent<UnitStats>().dexterityModifier;
        SetWaitVariables();

        //Roll for Hide
        int rollResult = scriptManager.scriptDiceRoller.RollD20(dexModifier);
        Debug.Log(selectedUnit.GetComponent<UnitStats>().unitName + " Rolled an " + rollResult + " to Hide");
        //Note -- Need to Add Roll Pop Up
    }

    public void WaitAction() //Called from Actions Menu Button
    {
        SetWaitVariables();
    }

    public void DodgeAction()
    {
        scriptManager.scriptBattleController.isDodging = true;
        onDodge?.Invoke();
        SetWaitVariables();
    }



    // Template for Actions //
    // Template for Actions //
    // Template for Actions //



    public void SetVariables()
    {
        scriptManager.scriptBattleController.battleStatus = true;
        scriptManager.scriptBattleController.actionSelected = true;
        scriptManager.scriptGameMenuController.CloseAllMenus();
    }

    public void SetWaitVariables()
    {
        scriptManager.scriptMovementController.DisableMovementRangeHighlight();
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().Wait();
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetWaitAnimation();
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().SetMovementState(3);
        scriptManager.scriptUnitSelection.DeselectUnit();
        scriptManager.scriptGameMenuController.CloseAllMenus();
    }

    public void HighlightActionRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void StartAction()
    {
        //Set Variables
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = scriptManager.scriptRangeFinder.GetAttackableUnits();

        //Click
        if (Physics.Raycast(ray, out hit))
        {
            //Clicked a Tile
            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                //Unit on Clicked Tile
                if (hit.transform.GetComponent<ClickableTile>().unitOnTile != null)
                {
                    GameObject unitOnTile = hit.transform.GetComponent<ClickableTile>().unitOnTile;
                    int unitX = unitOnTile.GetComponent<UnitController>().x;
                    int unitY = unitOnTile.GetComponent<UnitController>().y;

                    //Opposing Team & Within Attackable Tiles
                    if (unitOnTile.GetComponent<UnitController>().teamNumber != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
                    {
                        //Unit is Alive
                        if (unitOnTile.GetComponent<UnitController>().currentHP > 0)
                        {
                            //Attack then Deselect
                            StartCoroutine(UseAction(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                            StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                        }
                    }
                }
            }
        }

        //Clicked a Unit
        else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
        {
            GameObject unitClicked = hit.transform.parent.gameObject;
            int unitX = unitClicked.GetComponent<UnitController>().x;
            int unitY = unitClicked.GetComponent<UnitController>().y;

            if (unitClicked.GetComponent<UnitController>().teamNumber != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
            {
                //Enmy Unit is Alive
                if (unitClicked.GetComponent<UnitController>().currentHP > 0)
                {
                    StartCoroutine(UseAction(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator UseAction(GameObject initiator, GameObject recipient)
    {
        //Set Variables
        float timeElapsed = 0;
        Vector3 initiatorPosition = initiator.transform.position;
        Vector3 recipientPosition = recipient.transform.position;
        initiator.GetComponent<UnitController>().SetRunAnimation();

        //Smooth Movement
        while (timeElapsed < .25f)
        {
            initiator.transform.position = Vector3.Lerp(initiatorPosition, initiatorPosition + ((((recipientPosition - initiatorPosition) / (recipientPosition - initiatorPosition).magnitude)).normalized * .5f), (timeElapsed / .25f));
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Action
        while (scriptManager.scriptBattleController.battleStatus)
        {
            CompareRolls(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void CompareRolls(GameObject initiator, GameObject recipient)
    {
        //Set Variables
        int initiatorActionModifier = GetActionModifier(initiator);
        int recipientActionModifier = GetActionModifier(recipient);

        //Rolls
        int initiatorRoll = scriptManager.scriptDiceRoller.RollD20(initiatorActionModifier);
        int recipientRoll = scriptManager.scriptDiceRoller.RollD20(recipientActionModifier);


        //Compare Rolls
        if (initiatorRoll >= recipientRoll)
        {
            ApplyConditions(initiator, recipient);
            Instantiate(recipient.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        else
        {
            scriptManager.scriptBattleController.battleStatus = false;
            Debug.Log("Action Roll was Unsuccessful");

        }

        scriptManager.scriptBattleController.ResetActionBools();
    }

    public void ApplyConditions(GameObject initiator, GameObject recipient)
    {
        //State Machine Variable References
        //int normal = 0;
        //int charmed = 1;
        //int deafend = 2;
        //int frightened = 3;
        int grappled = 4;
        //int paralyzed = 5;
        //int petrified = 6;
        //int restrained = 7;
        //int stunned = 8;
        //int hidden = 9;
        //int unconscious = 10;

        //Set Conditions
        if (scriptManager.scriptBattleController.isGrappling == true)
        {
            recipient.GetComponent<UnitController>().SetConditionState(grappled);
            recipient.GetComponent<UnitStats>().movementSpeed = 0;
            recipient.GetComponent<UnitStats>().maxAttackRange = 1;
            Debug.Log(initiator.GetComponent<UnitStats>().unitName + " Grapple Roll Was Success & " + recipient.GetComponent<UnitStats>().unitName + " is Grappled");
        }
    }
}