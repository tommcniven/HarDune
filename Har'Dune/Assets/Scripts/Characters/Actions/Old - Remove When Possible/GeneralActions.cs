using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralActions : MonoBehaviour
{
    //Variables
    [Header("Scripts")]
    public UnitStats scriptUnitStats;
    public UnitController scriptUnitController;
    public ScriptManager scriptManager;

    public void Awake()
    {
        SetScriptManager();
    }

    //Update
    public void Update()
    {
        //Click to Use Respective Attack on Enemy Units
        if (Input.GetMouseButtonDown(0))
        {
            if (scriptManager.scriptBattleController.battleStatus)
            {
                if (scriptManager.scriptBattleController.grappleAction)
                {
                    AttemptGrappleAction();
                    scriptManager.scriptBattleController.ResetActionBools();
                }
            }
        }
    }


    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    //Start Grapple Seqeuence
    public void StartGrapple()
    {
        //Set Variables
        scriptManager.scriptBattleController.grappleAction = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightGrappleRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
    }

    //Highlight Tiles on Grapple Action Menu Button
    public void HighlightGrappleRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    //Attempt Grapple
    public void AttemptGrappleAction()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = scriptManager.scriptRangeFinder.GetAttackableUnits();

        //Clicked
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

                    if (unitOnTile.GetComponent<UnitController>().teamNumber != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
                    {
                        if (unitOnTile.GetComponent<UnitController>().currentHP > 0)
                        {
                            StartCoroutine(GrappleActionEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(GrappleActionEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    //Action Animation & Movement
    public IEnumerator GrappleActionEffects(GameObject initiator, GameObject recipient)
    {
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

        //Attack
        while (scriptManager.scriptBattleController.battleStatus)
        {
            RollDice_Grapple(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    //Grapple Rolls
    public void RollDice_Grapple(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        //Grapple Rolls
        int initiatorGrappleRoll = Random.Range(1, 20) + initiatorStats.strengthModifier;
        int recipientGrappleRoll = Random.Range(1, 20) + recipientStats.strengthModifier;

        //Initiator Grapple Roll >= Recipient Grapple Roll
        if (initiatorGrappleRoll >= recipientGrappleRoll)
        {
            Debug.Log(initiatorStats.unitName + "'s Grapple Roll of " + initiatorGrappleRoll + " was higher than " + recipientStats.unitName + "'s Roll of " + recipientGrappleRoll);

            //Particle Effect
            //[Update] to New Graphic
            GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Initiator Grapple Roll < Recipient Grapple Roll
        else
        {
            Debug.Log(initiatorStats.unitName + "'s Grapple Roll of " + initiatorGrappleRoll + " was lower than " + recipientStats.unitName + "'s Roll of " + recipientGrappleRoll);
            scriptManager.scriptBattleController.battleStatus = false;
        }
    }

    //Attempt Hide
    public void StartHide()
    {
        //Set Statuses
        scriptManager.scriptBattleController.hideAction = true;
        scriptManager.scriptBattleController.battleStatus = true;

        //Run Methods
        //[Update] Add Hide Animation (May Conflict with Wait Animaiton)
        RollDice_Hide();
        scriptManager.scriptGameMenuController.WaitButton();
        scriptManager.scriptBattleController.ResetActionBools();
    }

    //Roll Hide
    public void RollDice_Hide()
    {
        //Initiator & Recipient
        var initiatorUnit = scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>();
        var initiatorStats = scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitStats>();

        //Hide Roll
        int initiatorHideRoll = Random.Range(1, 20) + initiatorStats.dexterityModifier;
        Debug.Log(scriptUnitStats.unitName + " Rolled a Stealth Roll of " + initiatorHideRoll);

        scriptManager.scriptBattleController.battleStatus = false;
    }
}
