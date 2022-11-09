using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spellcasting : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;
    public SpellReference spellReference;

    [Header("Debug Variables")]
    public string selectedSpell;

    [Header("Unit References")]
    public int spellcastingModifier;
    public int splashRange;
    public int spellSaveModifier;
    public int UnitX;
    public int unitY;

    public void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public int GetSpellcastingModifier(GameObject unit)
    {
        if (scriptManager.scriptBattleController.isAttackSpell)
        {
            spellcastingModifier = unit.GetComponent<UnitStats>().spellAttackModifier;
        }

        if (scriptManager.scriptBattleController.isAttackSpellSave)
        {
            spellcastingModifier = unit.GetComponent<UnitStats>().spellSaveModifier;
        }

        if (scriptManager.scriptBattleController.isHealingSpell)
        {
            spellcastingModifier = unit.GetComponent<UnitStats>().spellAttackModifier - unit.GetComponent<UnitStats>().proficiency;
            Debug.Log("isHealingSpellCheck");
        }

        return spellcastingModifier;
    }

    public int GetSpellSaveModifier(GameObject unit)
    {
        GetSelectedSpell();

        
        if (spellReference.spell.spellSaveType == "Strength")
        {
            spellSaveModifier = unit.GetComponent<UnitStats>().strengthModifier;
        }

        if (spellReference.spell.spellSaveType == "Dexterity")
        {
            spellSaveModifier = unit.GetComponent<UnitStats>().dexterityModifier;
        }

        if (spellReference.spell.spellSaveType == "Constitution")
        {
            spellSaveModifier = unit.GetComponent<UnitStats>().constitutionModifier;
        }

        if (spellReference.spell.spellSaveType == "Intelligence")
        {
            spellSaveModifier = unit.GetComponent<UnitStats>().intelligenceModifier;
        }

        if (spellReference.spell.spellSaveType == "Wisdom")
        {
            spellSaveModifier = unit.GetComponent<UnitStats>().wisdomModifier;
        }

        if (spellReference.spell.spellSaveType == "Charisma")
        {
            spellSaveModifier = unit.GetComponent<UnitStats>().charismaModifier;
        }

        return spellSaveModifier;
    }

    public void GetSelectedSpell()
    {
        if (scriptManager.scriptBattleController.frostbite)
        {
            spellReference = GameObject.Find("Frostbite").GetComponent<SpellReference>();
            selectedSpell = "Frostbite";
        }

        if (scriptManager.scriptBattleController.guidance)
        {
            spellReference = GameObject.Find("Guidance").GetComponent<SpellReference>();
            selectedSpell = "Guidance";
        }

        if (scriptManager.scriptBattleController.iceKnife)
        {
            spellReference = GameObject.Find("Ice Knife").GetComponent<SpellReference>();
            selectedSpell = "Ice Knife";
        }

        if (scriptManager.scriptBattleController.healingWord)
        {
            spellReference = GameObject.Find("Healing Word").GetComponent<SpellReference>();
            selectedSpell = "Healing Word";
        }

        if (scriptManager.scriptBattleController.charmPerson)
        {
            spellReference = GameObject.Find("Charm Person").GetComponent<SpellReference>();
            selectedSpell = "Charm Person";
        }

        if (scriptManager.scriptBattleController.barkskin)
        {
            spellReference = GameObject.Find("Barksin").GetComponent<SpellReference>();
            selectedSpell = "Barksin";
        }

        if (scriptManager.scriptBattleController.holdPerson)
        {
            spellReference = GameObject.Find("Hold Person").GetComponent<SpellReference>();
            selectedSpell = "Hold Person";
        }
    }


    // Spells //
    // Spells //
    // Spells //

    public void CastFrostbite() //Called from Action Menu Button
    {
        scriptManager.scriptBattleController.frostbite = true;
        scriptManager.scriptBattleController.isAttackSpell = true;
        SetVariables();
        HighlightSpellRange();
    }

    public void CastGuidance() //Called from Action Menu Button
    {
        scriptManager.scriptBattleController.guidance = true;
        scriptManager.scriptBattleController.isModifier = true;
        SetVariables();
        HighlightFriendlySpellRange();
    }

    public void CastIceKnife() //Called from Action Menu Button
    {
        scriptManager.scriptBattleController.iceKnife = true;
        scriptManager.scriptBattleController.isAttackSpell = true;
        SetVariables();
        HighlightSpellRange();
    }

    public void CastCharmPerson() //Called from Action Menu Button
    {
        scriptManager.scriptBattleController.charmPerson = true;
        scriptManager.scriptBattleController.isAttackSpellSave = true;
        SetVariables();
        HighlightSpellRange();
    }

    public void CastHealingWord() //Called from Action Menu Button
    {
        scriptManager.scriptBattleController.healingWord = true;
        scriptManager.scriptBattleController.isHealingSpell = true;
        SetVariables();
        HighlightFriendlySpellRange();
        Debug.Log("CastHealingWord()");
    }



    // Template For Weapon Attacks //
    // Template For Weapon Attacks //
    // Template For Weapon Attacks //



    public void SetVariables()
    {
        GetSelectedSpell();
        scriptManager.scriptBattleController.battleStatus = true;
        scriptManager.scriptBattleController.spellSelected = true;
        GameObject selectedUnit = scriptManager.scriptTileMap.selectedUnit;
        selectedUnit.GetComponent<UnitStats>().attackRange = spellReference.spell.spellRange;
        selectedUnit.GetComponent<UnitStats>().damageType = spellReference.spell.spellDamageType;
        splashRange = spellReference.spell.splashRange;
        scriptManager.scriptGameMenuController.CloseAllMenus();
    }

    public void HighlightSpellRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void HighlightFriendlySpellRange()
    {
        scriptManager.scriptRangeFinder.HighlightFriendlyUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void StartSpell()
    {
        //Set Variables
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = scriptManager.scriptRangeFinder.GetAttackableUnits();

        if (Physics.Raycast(ray, out hit)) //Click
        {
            if (hit.transform.gameObject.CompareTag("Tile")) //Clicked Tile
            {
                if (hit.transform.GetComponent<ClickableTile>().unitOnTile != null) //Unit Exists on Clicked Tile
                {
                    GameObject unitOnTile = hit.transform.GetComponent<ClickableTile>().unitOnTile;
                    int unitX = unitOnTile.GetComponent<UnitController>().x;
                    int unitY = unitOnTile.GetComponent<UnitController>().y;

                    if (scriptManager.scriptBattleController.isAttackSpell || scriptManager.scriptBattleController.isAttackSpellSave)
                    {
                        //Enemy Unit on Attackable Tile
                        if (unitOnTile.GetComponent<UnitController>().teamNumber != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
                        {
                            if (unitOnTile.GetComponent<UnitController>().currentHP > 0) //Unit is Alive
                            {
                                //Attack then Deselect
                                StartCoroutine(CastSpell(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                                StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                            }
                        }
                    }

                    else if (scriptManager.scriptBattleController.isHealingSpell)
                    {
                        //Teamate on Attackable Tile
                        if (unitOnTile.GetComponent<UnitController>().teamNumber == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
                        {
                            //Heal then Deselect
                            StartCoroutine(CastSpell(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                            StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                        }
                    }

                    else if (scriptManager.scriptBattleController.isModifier)
                    {
                        if (unitOnTile.GetComponent<UnitController>().teamNumber == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]) && unitOnTile != scriptManager.scriptTileMap.selectedUnit)
                        {
                            StartCoroutine(CastSpell(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                            StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                        }

                    }
                }
            }
        }

        else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit")) //Clicked Unit
        {
            GameObject unitClicked = hit.transform.parent.gameObject;
            int unitX = unitClicked.GetComponent<UnitController>().x;
            int unitY = unitClicked.GetComponent<UnitController>().y;

            //Opposing Team & Within Attackable Tiles
            if (unitClicked.GetComponent<UnitController>().teamNumber != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
            {
                if (unitClicked.GetComponent<UnitController>().currentHP > 0) //Unit is Alive
                {
                    //Attack then Deselect
                    StartCoroutine(CastSpell(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator CastSpell(GameObject initiator, GameObject recipient)
    {
        //Set Variables
        float timeElapsed = 0;
        Vector3 initiatorPosition = initiator.transform.position;
        Vector3 recipientPosition = recipient.transform.position;
        initiator.GetComponent<UnitController>().SetSpellcastAnimation();

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
            DealDamage(initiator, recipient);
            //scriptSpellSlots.UpdateSpellSlots();
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
        //Reference Variables
        GetSelectedSpell();
        GetSpellcastingModifier(initiator);
        int damageRoll = 0;
        UnitController initiatorUnit = initiator.GetComponent<UnitController>();
        UnitStats initiatorStats = initiator.GetComponent<UnitStats>();
        UnitController recipientUnit = recipient.GetComponent<UnitController>();
        UnitStats recipientStats = recipient.GetComponent<UnitStats>();
        int attackDice = spellReference.spell.spellAttackDice;
        int attackDamage = spellReference.spell.spellAttackDamage;
        int recipientArmorClass = recipientStats.armorClass;
        GameObject selectedUnit = scriptManager.scriptTileMap.selectedUnit;


        //Roll Dice
        for (int dice = 0; dice < attackDice; dice++)
        {
            int randomRoll = Random.Range(1, attackDamage);
            damageRoll += randomRoll;
        }

        //Set Rolls to Variables
        int initiatorAttackRoll = scriptManager.scriptBattleController.AttackRoll(spellcastingModifier);
        int initiatorDamageRoll = damageRoll + spellcastingModifier;
        int initiatorCritDamageRoll = damageRoll + damageRoll + spellcastingModifier;

        //Compare Rolls
        if (scriptManager.scriptBattleController.isAttackSpell)
        {
            if (initiatorAttackRoll > recipientArmorClass) //Initiator Attack Roll Hits
            {
                if (initiatorAttackRoll - spellcastingModifier == 20) //Critical Hit
                {
                    recipientUnit.DealDamage(initiatorCritDamageRoll);
                    FindObjectOfType<AudioManager>().Play("Greatsword Attack"); //Note -- Need to Update to Sound Scriptable Object
                    StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                    Debug.Log(initiatorStats.unitName + " Rolled a Crit, so " + recipientStats.unitName + " took " + initiatorCritDamageRoll + " damage");
                }

                else //Hit Without Critical
                {
                    recipientUnit.DealDamage(initiatorDamageRoll);
                    FindObjectOfType<AudioManager>().Play("Greatsword Attack"); //Note -- Need to Update to Sound Scriptable Object
                    StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                    Debug.Log(initiatorStats.unitName + "'s " + selectedSpell + " Attack Roll of " + initiatorAttackRoll + " was higher than " + recipientStats.unitName + "'s AC of " + recipientArmorClass + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
                }

                if (scriptManager.scriptBattleController.isUnitDead(recipient)) //Kill Dead Units & Check for Winner
                {
                    //Save Unit Location for Potential Splash Damage
                    UnitX = recipient.GetComponent<UnitController>().x;
                    unitY = recipient.GetComponent<UnitController>().y;

                    //Kill unit & Check for Winner
                    recipient.transform.parent = null; //Required for UnitDie()
                    recipientUnit.UnitDie();
                    scriptManager.scriptBattleController.battleStatus = false;
                    scriptManager.scriptGameController.CheckIfUnitsRemain(initiator, recipient);
                    return;
                }

                if (scriptManager.scriptBattleController.isConditionApplied == false)
                {
                    ApplyConditions(initiator, recipient);
                }
            }

            else //Initiator Attack Roll Does Not Hit
            {
                FindObjectOfType<AudioManager>().Play("Attack Missed"); //Note -- Need to Update to Sound Scriptable Object
                Debug.Log(initiatorStats.unitName + "'s " + selectedSpell + " Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
                scriptManager.scriptBattleController.battleStatus = false;
            }
        }

        else if (scriptManager.scriptBattleController.isHealingSpell)
        {
            recipientUnit.HealDamage(initiatorDamageRoll);
            StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));

            if (scriptManager.scriptBattleController.isConditionApplied == false)
            {
                ApplyConditions(initiator, recipient);
            }
        }

        else if (scriptManager.scriptBattleController.isModifier)
        {
            if (scriptManager.scriptBattleController.isConditionApplied == false)
            {
                ApplyConditions(initiator, recipient);
            }
        }

        //Reset Variables
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0); //Remove Disadvantage
        scriptManager.scriptBattleController.battleStatus = false;
        scriptManager.scriptBattleController.ResetActionBools();
    }

    public void DealSplashDamage(GameObject initiator, GameObject recipient)
    {
        //Set Variables
        GetSelectedSpell();
        initiator.GetComponent<UnitStats>().splashRange = spellReference.spell.splashRange;
        UnitX = recipient.GetComponent<UnitController>().x;
        unitY = recipient.GetComponent<UnitController>().y;
        HashSet<Node> splashUnitNodes = scriptManager.scriptRangeFinder.GetSplashUnits(initiator, UnitX, unitY);
        GameObject[] activeUnits = GameObject.FindGameObjectsWithTag("Unit");
        Vector2 unitPosition = transform.position;

        //Damage Variables
        int spellcastingModifier = GetSpellcastingModifier(initiator);
        int spellSaveModifier = GetSpellSaveModifier(recipient);
        int attackDice = spellReference.spell.spellAttackDice;
        int attackDamage = spellReference.spell.spellAttackDamage;
        int damageRoll = 0;

        for (int dice = 0; dice < attackDice; dice++) //Roll Dice
        {
            int randomRoll = Random.Range(1, attackDamage);
            damageRoll += randomRoll;
        }

        foreach (GameObject activeUnit in activeUnits) //Get Units
        {
            GameObject unit = activeUnit.transform.gameObject;
            int unitX = unit.GetComponent<UnitController>().x;
            int unitY = unit.GetComponent<UnitController>().y;

            if (splashUnitNodes.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY])) //Unit in Splash Range
            {
                if (unit.GetComponent<UnitController>().teamNumber != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber) //Enemy
                {
                    if (unit.GetComponent<UnitController>().currentHP > 0) //Alive
                    {
                        int spellSaveRoll = scriptManager.scriptBattleController.AttackRoll(spellSaveModifier);

                        if (initiator.GetComponent<UnitStats>().spellSaveDC >= spellSaveRoll) //Failed Roll
                        {
                            int spellDamage = damageRoll + spellcastingModifier;
                            unit.GetComponent<UnitController>().DealDamage(spellDamage);
                            FindObjectOfType<AudioManager>().Play("Greatsword Attack"); //Note -- Need to Update to Sound Scriptable Object
                            StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(spellDamage));
                        }

                        else if (initiator.GetComponent<UnitStats>().spellSaveDC < spellSaveRoll && spellReference.spell.isHalfDamageApplied)
                        {
                            int spellDamage = damageRoll / 2 + spellcastingModifier;
                            unit.GetComponent<UnitController>().DealDamage(spellDamage);
                            FindObjectOfType<AudioManager>().Play("Greatsword Attack"); //Note -- Need to Update to Sound Scriptable Object
                            StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(spellDamage));
                        }

                        if (scriptManager.scriptBattleController.isUnitDead(unit)) //Kill Dead Units & Check for Winner
                        {
                            //Save Unit Location for Potential Splash Damage
                            UnitX = recipient.GetComponent<UnitController>().x;
                            unitY = recipient.GetComponent<UnitController>().y;

                            //Kill unit & Check for Winner
                            unit.transform.parent = null; //Required for UnitDie()
                            unit.GetComponent<UnitController>().UnitDie();
                            scriptManager.scriptGameController.CheckIfUnitsRemain(initiator, unit);
                            return;
                        }
                    }
                }
            }
        }

        //Reset Variables
        initiator.GetComponent<UnitController>().SetAttackState(0); //Remove Disadvantage
        scriptManager.scriptBattleController.battleStatus = false;
        scriptManager.scriptBattleController.ResetActionBools();
    }

    public void ApplyConditions(GameObject initiator, GameObject recipient)
    {
        if (scriptManager.scriptBattleController.frostbite)
        {
            recipient.GetComponent<UnitController>().SetAttackState(2); //Set Disadvantage
            scriptManager.scriptBattleController.isConditionApplied = true;
        }

        if (scriptManager.scriptBattleController.guidance)
        {
            recipient.GetComponent<UnitController>().SetAttackState(1); //Set Advantage
            scriptManager.scriptBattleController.isConditionApplied = true;
            //Note -- Change to Use Guidance Button Beside Ability Check Menu (Menu not yet made)
        }

        if (scriptManager.scriptBattleController.iceKnife)
        {
            DealSplashDamage(initiator, recipient);
            scriptManager.scriptBattleController.isConditionApplied = true;
        }

        if (scriptManager.scriptBattleController.charmPerson)
        {
            scriptManager.scriptBattleController.isConditionApplied = true;
        }
    }
}
