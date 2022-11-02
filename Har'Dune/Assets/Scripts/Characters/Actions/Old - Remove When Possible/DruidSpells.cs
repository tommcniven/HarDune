using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DruidSpells : MonoBehaviour
{
    [Header("Scripts")]
    public UnitStats scriptUnitStats;
    public UnitController scriptUnitController;
    public SpellSlots scriptSpellSlots;
    public ScriptManager scriptManager;

    public void Awake()
    {
        SetScriptManager();
    }

    void Update()
    {
        //Click to Use Respective Attack on Enemy Units
        if (Input.GetMouseButtonDown(0))
        {
            if (scriptManager.scriptBattleController.battleStatus)
            {
                if (scriptManager.scriptBattleController.frostbite)
                {
                    CastFrostbiteSpell();
                    scriptManager.scriptBattleController.ResetActionBools();
                }
                else if (scriptManager.scriptBattleController.charmPerson)
                {
                    CastCharmPersonSpell();
                    scriptManager.scriptBattleController.ResetActionBools();
                }
                else if (scriptManager.scriptBattleController.cureWounds)
                {
                    CastCureWoundsSpell();
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

    //Start Druidcraft Cast
    public void StartDruidcraftSpell()
    {
        scriptManager.scriptGameMenuController.WaitButton();
        scriptManager.scriptBattleController.ResetActionBools();
    }

    //Start Frostbite Cast Sequence
    public void StartFrostbiteSpell()
    {
        //Set Variables
        scriptManager.scriptBattleController.frostbite = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Cold";
        scriptUnitStats.attackRange = 6;

        //Update UI
        HighlightFrostbiteSpellRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
    }

    //Highlight Tiles on Action Menu Button
    public void HighlightFrostbiteSpellRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    //Cast Frostbite
    public void CastFrostbiteSpell()
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
                            StartCoroutine(FrostbiteSpellEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(FrostbiteSpellEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    //Frostbite Cast Animation & Movement
    public IEnumerator FrostbiteSpellEffects(GameObject initiator, GameObject recipient)
    {
        float timeElapsed = 0;
        Vector3 initiatorPosition = initiator.transform.position;
        Vector3 recipientPosition = recipient.transform.position;
        initiator.GetComponent<UnitController>().SetSpellcastAnimation();

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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().spellAttackModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Frostbite(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    //Attack & Damage Rolls
    public void RollDice_DealDamage_Frostbite(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        int recipientConSave = scriptManager.scriptBattleController.AttackRoll() + recipientStats.constitutionModifier;
        int initiatorDamageRoll = Random.Range(1, 6);
        int initiatorCritDamageRoll = Random.Range(1, 6) + Random.Range(1, 6);

        //Initiator Attack Roll Hits
        if (recipientConSave <= initiatorStats.spellSaveModifier)
        {
            //Critical Fail
            if (recipientConSave - recipientStats.constitutionModifier == 1)
            {
                //Deal Damage & Apply Disadvantage
                recipientUnit.DealDamage(initiatorCritDamageRoll);
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                recipientUnit.GetComponent<UnitController>().SetAttackState(2);
                Debug.Log(recipientStats.unitName + " Rolled a Nat 1, so they took " + initiatorCritDamageRoll + " crit damage");
            }
            //No Critical Hit
            else
            {
                //Deal Damage & Apply Disadvantage
                recipientUnit.DealDamage(initiatorDamageRoll);
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                recipientUnit.GetComponent<UnitController>().SetAttackState(2);
                Debug.Log(recipientStats.unitName + "'s Con Save of " + recipientConSave + " was lower than " + initiatorStats.unitName + "'s Spell Save DC of " + initiatorStats.spellSaveModifier + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
            }

            //Particle Effect on Hit
            //GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            //Destroy(tempParticle, 2f);

            //Kill Dead Units & Check for Winner
            if (scriptManager.scriptBattleController.isUnitDead(recipient))
            {
                //Null Parent Required for unitDie() Method to Function
                recipient.transform.parent = null;
                recipientUnit.UnitDie();
                scriptManager.scriptBattleController.battleStatus = false;
                scriptManager.scriptGameController.CheckIfUnitsRemain(initiator, recipient);
                return;
            }

            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Initiator Attack Roll Does Not Hit
        else
        {
            //Particle Effect on Miss
            GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            Destroy(tempParticle, 2f);

            Debug.Log(recipientStats.unitName + "'s Con Save Roll of " + recipientConSave + " was higher than " + initiatorStats.unitName + "'s Spell Save DC of " + initiatorStats.spellSaveModifier);
            scriptManager.scriptBattleController.battleStatus = false;
        }
    }


    //Start Charm Person Cast Sequence
    public void StartCharmPersonSpell()
    {
        //Set Variables
        scriptManager.scriptBattleController.charmPerson = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.attackRange = 6;

        //Update UI
        HighlightCharmPersonSpellRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
    }

    //Highlight Tiles on Action Menu Button
    public void HighlightCharmPersonSpellRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    //Cast Charm Person
    public void CastCharmPersonSpell()
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
                            StartCoroutine(CharmPersonSpellEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(CharmPersonSpellEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    //Charm Person Cast Animation & Movement
    public IEnumerator CharmPersonSpellEffects(GameObject initiator, GameObject recipient)
    {
        float timeElapsed = 0;
        Vector3 initiatorPosition = initiator.transform.position;
        Vector3 recipientPosition = recipient.transform.position;
        initiator.GetComponent<UnitController>().SetSpellcastAnimation();

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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().spellAttackModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_CharmPerson(initiator, recipient);
            scriptSpellSlots.UpdateLevelOneCurrentSpellSlots();
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    //Save Rolls
    public void RollDice_CharmPerson(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        int recipientWisdomSave = scriptManager.scriptBattleController.AttackRoll() + recipientStats.wisdomModifier;

        //Initiator Attack Roll Hits
        if (recipientWisdomSave <= initiatorStats.spellSaveModifier)
        {
            //Critical Fail
            if (recipientWisdomSave - recipientStats.constitutionModifier == 1)
            {
                //Deal Damage & Apply Disadvantage
                Debug.Log(recipientStats.unitName + " Rolled a Nat 1, so they are Charmed");
            }
            //No Critical Hit
            else
            {
                //Deal Damage & Apply Disadvantage
                Debug.Log(recipientStats.unitName + "'s Wisdom Save of " + recipientWisdomSave + " was lower than " + initiatorStats.unitName + "'s Spell Save DC of " + initiatorStats.spellSaveModifier + ", so " + recipientStats.unitName + " is charmed");
            }

            //Particle Effect on Hit
            GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            Destroy(tempParticle, 2f);

            //Update Status
            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Initiator Attack Roll Does Not Hit
        else
        {
            //Particle Effect on Miss
            GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            Destroy(tempParticle, 2f);

            Debug.Log(recipientUnit.unitName + "'s Wisdom Save Roll of " + recipientWisdomSave + " was higher than " + initiatorStats.unitName + "'s Spell Save DC of " + initiatorStats.spellSaveModifier);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(recipientWisdomSave));
    }

    //Start Cure Wounds Cast Sequence
    public void StartCureWoundsSpell()
    {
        //Set Variables
        scriptManager.scriptBattleController.cureWounds = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightCureWoundsSpellRange();
        scriptManager.scriptGameMenuController.CloseAllMenus();
    }

    //Highlight Tiles on Action Menu Button
    public void HighlightCureWoundsSpellRange()
    {
        scriptManager.scriptRangeFinder.HighlightFriendlyUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    //Cast Cure Wounds
    public void CastCureWoundsSpell()
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

                    if (unitOnTile.GetComponent<UnitController>().teamNumber == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
                    {
                        StartCoroutine(CureWoundsSpellEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
                        StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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

            if (unitClicked.GetComponent<UnitController>().teamNumber == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptManager.scriptTileMap.tileGraph[unitX, unitY]))
            {
                StartCoroutine(CureWoundsSpellEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
            }
        }
    }

    //Cure Wounds Cast Animation & Movement
    public IEnumerator CureWoundsSpellEffects(GameObject initiator, GameObject recipient)
    {
        float timeElapsed = 0;
        Vector3 initiatorPosition = initiator.transform.position;
        Vector3 recipientPosition = recipient.transform.position;
        initiator.GetComponent<UnitController>().SetSpellcastAnimation();

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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().spellAttackModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_HealDamage_CureWounds(initiator, recipient);
            scriptSpellSlots.UpdateLevelOneCurrentSpellSlots();
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    //Attack & Damage Rolls
    public void RollDice_HealDamage_CureWounds(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        int initiatorHealRoll = Random.Range(1, 8) + initiatorStats.spellAttackModifier;

        //Heal Damage & Apply Disadvantage
        recipientUnit.HealDamage(initiatorHealRoll);
        StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorHealRoll));
        recipientUnit.GetComponent<UnitController>().SetAttackState(2);
        Debug.Log(recipientStats.unitName + " was healed for " + initiatorHealRoll + " by " + initiatorStats.unitName);


        //Particle Effect
        GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
        Destroy(tempParticle, 2f);

        scriptManager.scriptBattleController.battleStatus = false;
    }
}
