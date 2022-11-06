using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    // Steps for Adding Weapon:
    // 1. Create New SO Weapon from Asset Menu
    // 2. Create New Empty Game Object as Child of Weapon Attacks
    // 3. Attach Weapon Script to Game Object
    // 4. Copy & Paste New Weapon Attack from GreatswordAttack()
    // 5. Add Weapon Attack Bool to Battle Controller Script
    // 6. Add Weapon Attack Bool to GetSelectedWeapon()
    // 7. Add Weapon to be Removed from Attack Options in ResetAttackMenuOptions()
    // 8. Done!


    [Header("Scripts")]
    public ScriptManager scriptManager;
    public UnitStats scriptUnitStats_SelectedUnit;
    public WeaponSO weaponSO;
    public WeaponReference weaponReference;

    [Header("Debug Variables")]
    string selectedWeapon;

    public void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void GetSelectedWeapon()
    {
        if (scriptManager.scriptBattleController.unarmedStrike == true)
        {
            weaponReference = GameObject.Find("Unarmed Strike").GetComponent<WeaponReference>();
            selectedWeapon = "Unarmed Strike";
        }

        if (scriptManager.scriptBattleController.greatswordAttack == true)
        {
            weaponReference = GameObject.Find("Greatsword").GetComponent<WeaponReference>();
            selectedWeapon = "Greatsword";
        }

        if (scriptManager.scriptBattleController.daggerAttack == true)
        {
            weaponReference = GameObject.Find("Dagger").GetComponent<WeaponReference>();
            selectedWeapon = "Dagger";
        }

        if (scriptManager.scriptBattleController.quarterstaffAttack == true)
        {
            weaponReference = GameObject.Find("Quarterstaff").GetComponent<WeaponReference>();
            selectedWeapon = "Quarterstaff";
        }

        if(scriptManager.scriptBattleController.lightCrossbowAttack == true)
        {
            weaponReference = GameObject.Find("Light Crossbow").GetComponent<WeaponReference>();
            selectedWeapon = "Light Crossbow";
        }

        if(scriptManager.scriptBattleController.scimitarAttack == true)
        {
            weaponReference = GameObject.Find("Scimitar").GetComponent<WeaponReference>();
            selectedWeapon = "Scimitar";
        }

        if(scriptManager.scriptBattleController.daggerThrowAttack == true)
        {
            weaponReference = GameObject.Find("Dagger Throw").GetComponent<WeaponReference>();
            selectedWeapon = "Dagger Throw";
        }
    }



    // Weapon Attacks //
    // Weapon Attacks //
    // Weapon Attacks //

    public void UnarmedStrike() //Called from Class Button
    {
        scriptManager.scriptBattleController.unarmedStrike = true;
        scriptManager.scriptBattleController.battleStatus = true;
        GetSelectedWeapon();
        SetVariables();
        HighlightAttackRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
        StartAttack();
    }

    public void GreatswordAttack() //Called from Class Button
    {
        scriptManager.scriptBattleController.greatswordAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        GetSelectedWeapon();
        SetVariables();
        HighlightAttackRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
        StartAttack();
    }

    public void DaggerAttack() //Called from Class Button
    {
        scriptManager.scriptBattleController.daggerAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        GetSelectedWeapon();
        SetVariables();
        HighlightAttackRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
        StartAttack();
    }

    public void QuarterstaffAttack() //Called from Class Button
    {
        scriptManager.scriptBattleController.quarterstaffAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        GetSelectedWeapon();
        SetVariables();
        HighlightAttackRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
        StartAttack();
    }

    public void LightCrossbowAttack() //Called from Class Button
    {
        scriptManager.scriptBattleController.lightCrossbowAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        GetSelectedWeapon();
        SetVariables();
        HighlightAttackRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
        StartAttack();
    }

    public void ScimitarAttack() //Called from Class Button
    {
        scriptManager.scriptBattleController.scimitarAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        GetSelectedWeapon();
        SetVariables();
        HighlightAttackRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
        StartAttack();
    }

    public void DaggerThrowAttack() //Called from Class Button
    {
        scriptManager.scriptBattleController.daggerThrowAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        GetSelectedWeapon();
        SetVariables();
        HighlightAttackRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
        StartAttack();
    }



    // Template For Weapon Attacks //
    // Template For Weapon Attacks //
    // Template For Weapon Attacks //



    public void SetVariables()
    {
        //Set Variables
        UnitStats selectedUnit = scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitStats>();
        selectedUnit.attackRange = weaponReference.weapon.attackRange;
        selectedUnit.damageType = weaponReference.weapon.weaponDamageType;
    }

    public void HighlightAttackRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void StartAttack()
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
                            StartCoroutine(AttackTarget(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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

            //Opposing Team & Within Attackable Tiles
            if (unitClicked.GetComponent<UnitController>().teamNumber != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
            {
                //Unit is Alive
                if (unitClicked.GetComponent<UnitController>().currentHP > 0)
                {
                    //Attack then Deselect
                    StartCoroutine(AttackTarget(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator AttackTarget(GameObject initiator, GameObject recipient)
    {
        //Set Variables
        float timeElapsed = 0;
        Vector3 initiatorPosition = initiator.transform.position;
        Vector3 recipientPosition = recipient.transform.position;
        initiator.GetComponent<UnitController>().SetRunAnimation();

        //Move
        while (timeElapsed < .25f)
        {
            initiator.transform.position = Vector3.Lerp(initiatorPosition, initiatorPosition + ((((recipientPosition - initiatorPosition) / (recipientPosition - initiatorPosition).magnitude)).normalized * .5f), (timeElapsed / .25f));
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Attack
        while (scriptManager.scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));

            //Note - Add --> GetWeaponSelected();
            //Change RollDice_DealDamage_Greatsword(initiator, recipient) --> DealDamage(initiator, recipient);
            DealDamage(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void DealDamage(GameObject initiator, GameObject recipient)
    {
        //Set Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();
        var damageRoll = 0;

        //Get Weapon Selected
        GetSelectedWeapon();

        //Set Weapon Variables
        var attackDice = weaponReference.weapon.attackDice;
        var attackDamage = weaponReference.weapon.attackDamage;

        //Roll Weapon Dice
        for (int dice = 0; dice < attackDice; dice++)
        {
            var randomRoll = Random.Range(1, attackDamage);
            damageRoll += randomRoll;
        }
       
        //Set Rolls to Variables
        int initiatorAttackRoll = scriptManager.scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
        int initiatorDamageRoll = damageRoll + initiatorStats.damageModifier;
        int initiatorCritDamageRoll = damageRoll + damageRoll + initiatorStats.damageModifier;
        int recipientArmorClass = recipientStats.armorClass;

        //Apply Rolls -- Deal Damage
        if (initiatorAttackRoll > recipientArmorClass) //Initiator Attack Roll Hits
        {
            if (initiatorAttackRoll - initiatorStats.attackModifier == 20) //Critical Hit
            {
                recipientUnit.DealDamage(initiatorCritDamageRoll);
                FindObjectOfType<AudioManager>().Play("Greatsword Attack"); //Note -- Need to Update to Sound Scriptable Object
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                Debug.Log(initiatorStats.unitName + " Rolled a Crit, so " + recipientStats.unitName + " took " + initiatorCritDamageRoll + " damage");
            }

            else //No Critical Hit
            {
                recipientUnit.DealDamage(initiatorDamageRoll);
                FindObjectOfType<AudioManager>().Play("Greatsword Attack");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                Debug.Log(initiatorStats.unitName + "'s " + selectedWeapon + " Attack Roll of " + initiatorAttackRoll + " was higher than " + recipientStats.unitName + "'s AC of " + recipientArmorClass + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
            }

            if (scriptManager.scriptBattleController.isUnitDead(recipient)) //Kill Dead Units & Check for Winner
            {
                recipient.transform.parent = null; //Required for UnitDie()
                recipientUnit.UnitDie();
                scriptManager.scriptBattleController.battleStatus = false;
                scriptManager.scriptGameController.CheckIfUnitsRemain(initiator, recipient);
                return;
            }

            scriptManager.scriptBattleController.battleStatus = false;
        }

        else //Initiator Attack Roll Does Not Hit
        {
            FindObjectOfType<AudioManager>().Play("Attack Missed");
            Debug.Log(initiatorStats.unitName + "'s " + selectedWeapon + " Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        initiatorUnit.GetComponent<UnitController>().SetAttackState(0); //Remove Disadvantage
    }
}
