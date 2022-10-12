using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DruidSpells : MonoBehaviour
{
    [Header("Scripts")]
    public UnitStats scriptUnitStats;
    public UnitController scriptUnitController;
    public BattleController scriptBattleController;
    public GameController scriptGameController;
    public TileMap scriptTileMap;
    public GameMenuController scriptGameMenuController;
    public CameraShake scriptCameraShake;
    public SpellSlots scriptSpellSlots;

    void Update()
    {
        //Click to Use Respective Attack on Enemy Units
        if (Input.GetMouseButtonDown(0))
        {
            if (scriptBattleController.battleStatus)
            {
                if (scriptBattleController.frostbite)
                {
                    CastFrostbiteSpell();
                    scriptBattleController.ResetActionBools();
                }
                else if (scriptBattleController.charmPerson)
                {
                    CastCharmPersonSpell();
                    scriptBattleController.ResetActionBools();
                }
                else if (scriptBattleController.cureWounds)
                {
                    CastCureWoundsSpell();
                    scriptBattleController.ResetActionBools();
                }
            }
        }
    }

    //Start Druidcraft Cast
    public void StartDruidcraftSpell()
    {
        scriptGameMenuController.Wait();
        scriptBattleController.ResetActionBools();
    }

    //Start Frostbite Cast Sequence
    public void StartFrostbiteSpell()
    {
        //Set Variables
        scriptBattleController.frostbite = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Cold";
        scriptUnitStats.attackRange = 6;

        //Update UI
        HighlightFrostbiteSpellRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    //Highlight Tiles on Action Menu Button
    public void HighlightFrostbiteSpellRange()
    {
        scriptTileMap.HighlightAttackableUnitsInRange();
        scriptTileMap.HighlightTileUnitIsOccupying();
    }

    //Cast Frostbite
    public void CastFrostbiteSpell()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = scriptTileMap.GetAttackableUnits();

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

                    if (unitOnTile.GetComponent<UnitController>().teamNumber != scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptTileMap.tileGraph[unitX, unitY]))
                    {
                        if (unitOnTile.GetComponent<UnitController>().currentHP > 0)
                        {
                            StartCoroutine(FrostbiteSpellEffects(scriptTileMap.selectedUnit, unitOnTile));
                            StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitOnTile));
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

            if (unitClicked.GetComponent<UnitController>().teamNumber != scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptTileMap.tileGraph[unitX, unitY]))
            {
                //Enmy Unit is Alive
                if (unitClicked.GetComponent<UnitController>().currentHP > 0)
                {
                    StartCoroutine(FrostbiteSpellEffects(scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().spellAttackModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Frostbite(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
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

        int recipientConSave = scriptBattleController.AttackRoll() + recipientStats.constitutionModifier;
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
            if (scriptBattleController.CheckIfDead(recipient))
            {
                //Null Parent Required for unitDie() Method to Function
                recipient.transform.parent = null;
                recipientUnit.UnitDie();
                scriptBattleController.battleStatus = false;
                scriptGameController.CheckIfUnitsRemain(initiator, recipient);
                return;
            }

            scriptBattleController.battleStatus = false;
        }

        //Initiator Attack Roll Does Not Hit
        else
        {
            //Particle Effect on Miss
            GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            Destroy(tempParticle, 2f);

            Debug.Log(recipientStats.unitName + "'s Con Save Roll of " + recipientConSave + " was higher than " + initiatorStats.unitName + "'s Spell Save DC of " + initiatorStats.spellSaveModifier);
            scriptBattleController.battleStatus = false;
        }
    }


    //Start Charm Person Cast Sequence
    public void StartCharmPersonSpell()
    {
        //Set Variables
        scriptBattleController.charmPerson = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.attackRange = 6;

        //Update UI
        HighlightCharmPersonSpellRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    //Highlight Tiles on Action Menu Button
    public void HighlightCharmPersonSpellRange()
    {
        scriptTileMap.HighlightAttackableUnitsInRange();
        scriptTileMap.HighlightTileUnitIsOccupying();
    }

    //Cast Charm Person
    public void CastCharmPersonSpell()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = scriptTileMap.GetAttackableUnits();

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

                    if (unitOnTile.GetComponent<UnitController>().teamNumber != scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptTileMap.tileGraph[unitX, unitY]))
                    {
                        if (unitOnTile.GetComponent<UnitController>().currentHP > 0)
                        {
                            StartCoroutine(CharmPersonSpellEffects(scriptTileMap.selectedUnit, unitOnTile));
                            StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitOnTile));
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

            if (unitClicked.GetComponent<UnitController>().teamNumber != scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptTileMap.tileGraph[unitX, unitY]))
            {
                //Enmy Unit is Alive
                if (unitClicked.GetComponent<UnitController>().currentHP > 0)
                {
                    StartCoroutine(CharmPersonSpellEffects(scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().spellAttackModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_CharmPerson(initiator, recipient);
            scriptSpellSlots.UpdateLevelOneCurrentSpellSlots();
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
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

        int recipientWisdomSave = scriptBattleController.AttackRoll() + recipientStats.wisdomModifier;

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
            scriptBattleController.battleStatus = false;
        }

        //Initiator Attack Roll Does Not Hit
        else
        {
            //Particle Effect on Miss
            GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            Destroy(tempParticle, 2f);

            Debug.Log(recipientUnit.unitName + "'s Wisdom Save Roll of " + recipientWisdomSave + " was higher than " + initiatorStats.unitName + "'s Spell Save DC of " + initiatorStats.spellSaveModifier);
            scriptBattleController.battleStatus = false;
        }

        StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(recipientWisdomSave));
    }

    //Start Cure Wounds Cast Sequence
    public void StartCureWoundsSpell()
    {
        //Set Variables
        scriptBattleController.cureWounds = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightCureWoundsSpellRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    //Highlight Tiles on Action Menu Button
    public void HighlightCureWoundsSpellRange()
    {
        scriptTileMap.HighlightFriendlyUnitsInRange();
        scriptTileMap.HighlightTileUnitIsOccupying();
    }

    //Cast Cure Wounds
    public void CastCureWoundsSpell()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = scriptTileMap.GetAttackableUnits();

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

                    if (unitOnTile.GetComponent<UnitController>().teamNumber == scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptTileMap.tileGraph[unitX, unitY]))
                    {
                        StartCoroutine(CureWoundsSpellEffects(scriptTileMap.selectedUnit, unitOnTile));
                        StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitOnTile));
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

            if (unitClicked.GetComponent<UnitController>().teamNumber == scriptTileMap.selectedUnit.GetComponent<UnitController>().teamNumber && attackableTiles.Contains(scriptTileMap.tileGraph[unitX, unitY]))
            {
                StartCoroutine(CureWoundsSpellEffects(scriptTileMap.selectedUnit, unitClicked));
                StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().spellAttackModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_HealDamage_CureWounds(initiator, recipient);
            scriptSpellSlots.UpdateLevelOneCurrentSpellSlots();
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
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

        scriptBattleController.battleStatus = false;
    }
}
