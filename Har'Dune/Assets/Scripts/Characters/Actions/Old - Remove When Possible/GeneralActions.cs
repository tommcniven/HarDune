using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralActions : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("UnitReferences")]
    public int actionModifier;

    public void Awake()
    {
        SetScriptManager();
    }

    public void Update()
    {
        //Click to Use Respective Attack on Enemy Units
        if (Input.GetMouseButtonDown(0))
        {
            if (scriptManager.scriptBattleController.battleStatus)
            {
                StartAction();
                scriptManager.scriptBattleController.ResetActionBools();
            }
        }
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public int GetActionModifier(GameObject unit)
    {
        if (scriptManager.scriptBattleController.grappleAction == true)
        {
            actionModifier = unit.GetComponent<UnitStats>().strengthModifier;
        }

        if (scriptManager.scriptBattleController.hideAction == true)
        {
            actionModifier = unit.GetComponent<UnitStats>().dexterityModifier;
        }

        return actionModifier;
    }



    // Actions //
    // Actions //
    // Actions //



    public void GrappleAction() //Called from Class Button
    {
        scriptManager.scriptBattleController.grappleAction = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitStats>().attackRange = 1;
        HighlightActionRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
        StartAction();
    }



    public void StartHide()
    {
        scriptManager.scriptBattleController.hideAction = true;
        scriptManager.scriptBattleController.battleStatus = true;
        StartAction();
        //scriptManager.scriptGameMenuController.WaitButton();
        //scriptManager.scriptBattleController.ResetActionBools();
    }



    // Template for Actions //
    // Template for Actions //
    // Template for Actions //


    
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
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        int initiatorActionModifier = GetActionModifier(initiator);
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();
        int recipientActionModifier = GetActionModifier(recipient);

        //Rolls
        int initiatorRoll = scriptManager.scriptDiceRoller.RollD20(initiatorActionModifier);
        int recipientRoll = scriptManager.scriptDiceRoller.RollD20(recipientActionModifier);

        //Compare Rolls
        if (initiatorRoll >= recipientRoll)
        {
            scriptManager.scriptBattleController.battleStatus = false;
            ApplyConditions(initiatorUnit, recipientUnit);
            Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
        }

        else
        {
            scriptManager.scriptBattleController.battleStatus = false;
        }
    }

    public void ApplyConditions(UnitController initiator, UnitController recipient)
    {
        if (scriptManager.scriptBattleController.grappleAction == true)
        {
            recipient.SetConditionState(4); //Set Condition State as Grappled
        }

        if (scriptManager.scriptBattleController.hideAction == true)
        {
            initiator.SetConditionState(9); //Set Condition State as Hidden
        }
    }
}