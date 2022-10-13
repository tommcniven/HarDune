using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackActions : MonoBehaviour
{
    [Header("Scripts")]
    public UnitStats scriptUnitStats;
    public UnitController scriptUnitController;
    public ScriptManager scriptManager;

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
                if (scriptManager.scriptBattleController.greatswordAttack)
                {
                    MakeGreatswordAttack();
                    scriptManager.scriptBattleController.ResetActionBools();
                }
                else if (scriptManager.scriptBattleController.daggerAttack)
                {
                    MakeDaggerAttack();
                    scriptManager.scriptBattleController.ResetActionBools();
                }
                else if (scriptManager.scriptBattleController.quarterstaffAttack)
                {
                    MakeQuarterstaffAttack();
                    scriptManager.scriptBattleController.ResetActionBools();
                }
                else if (scriptManager.scriptBattleController.lightCrossbowAttack)
                {
                    MakeLightCrossbowAttack();
                    scriptManager.scriptBattleController.ResetActionBools();
                }
                else if (scriptManager.scriptBattleController.scimitarAttack)
                {
                    MakeScimitarAttack();
                    scriptManager.scriptBattleController.ResetActionBools();
                }
                else if (scriptManager.scriptBattleController.daggerThrowAttack)
                {
                    MakeDaggerThrowAttack();
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

    public void StartGreatswordAttack()
    {
        //Set Variables
        scriptManager.scriptBattleController.greatswordAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Slashing";
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightGreatswordAttackRange();
        scriptManager.scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightGreatswordAttackRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeGreatswordAttack()
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
                            StartCoroutine(GreatswordAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(GreatswordAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator GreatswordAttackEffects(GameObject initiator, GameObject recipient)
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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Greatsword(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void RollDice_DealDamage_Greatsword(GameObject initiator, GameObject recipient)
    {

        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        int initiatorAttackRoll = scriptManager.scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
        int initiatorDamageRoll = Random.Range(1, 8) + initiatorStats.damageModifier;
        int initiatorCritDamageRoll = Random.Range(1, 8) + Random.Range(1, 8) + initiatorStats.damageModifier;
        int recipientArmorClass = recipientStats.armorClass;

        //Initiator Attack Roll Hits
        if (initiatorAttackRoll >= recipientArmorClass)
        {
            //Critical Hit
            if (initiatorAttackRoll - initiatorStats.attackModifier == 20)
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorCritDamageRoll);
                //FindObjectOfType<AudioManager>().Play("Fighter Attack Grunt");
                FindObjectOfType<AudioManager>().Play("Greatsword Attack");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                Debug.Log(initiatorStats.unitName + " Rolled a Crit, so " + recipientStats.unitName + " took " + initiatorCritDamageRoll + " damage");
            }
            //No Critical Hit
            else
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorDamageRoll);
                //FindObjectOfType<AudioManager>().Play("Fighter Attack Grunt");
                FindObjectOfType<AudioManager>().Play("Greatsword Attack");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was higher than " + recipientStats.unitName + "'s AC of " + recipientArmorClass + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
            }

            //Particle Effect on Hit
            //GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            //Destroy(tempParticle, 2f);

            //Kill Dead Units & Check for Winner
            if (scriptManager.scriptBattleController.CheckIfDead(recipient))
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
            //FindObjectOfType<AudioManager>().Play("Fighter Attack Grunt");
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }

    public void StartDaggerAttack()
    {
        //Set Variables
        scriptManager.scriptBattleController.daggerAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Stabbing";
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightDaggerAttackRange();
        scriptManager.scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightDaggerAttackRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeDaggerAttack()
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
                            StartCoroutine(DaggerAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(DaggerAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator DaggerAttackEffects(GameObject initiator, GameObject recipient)
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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().dexterityModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Dagger(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void RollDice_DealDamage_Dagger(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        //Calculate Hit, AC, & Damage
        int initiatorAttackRoll = scriptManager.scriptBattleController.AttackRoll();
        int initiatorDamageRoll = Random.Range(1, 4) + initiatorStats.damageModifier;
        int initiatorCritDamageRoll = Random.Range(1, 4) + Random.Range(1, 4) + initiatorStats.damageModifier;
        int recipientArmorClass = recipientStats.armorClass;

        //Initiator Attack Roll Hits
        if (initiatorAttackRoll >= recipientArmorClass)
        {
            //Critical Hit
            if (initiatorAttackRoll - initiatorStats.attackModifier == 20)
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorCritDamageRoll);
                FindObjectOfType<AudioManager>().Play("Dagger Attack");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                Debug.Log(initiatorStats.unitName + " Rolled a Crit, so " + recipientStats.unitName + " took " + initiatorCritDamageRoll + " damage");
            }
            //No Critical Hit
            else
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorDamageRoll);
                FindObjectOfType<AudioManager>().Play("Dagger Attack");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was higher than " + recipientStats.unitName + "'s AC of " + recipientArmorClass + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
            }

            //Particle Effect on Hit
            //GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            //Destroy(tempParticle, 2f);


            //Kill Dead Units & Check for Winner
            if (scriptManager.scriptBattleController.CheckIfDead(recipient))
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
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }

    public void StartQuarterstaffAttack()
    {
        //Set Variables
        scriptManager.scriptBattleController.quarterstaffAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Bludgeoning";
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightQuarterstaffAttackRange();
        scriptManager.scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightQuarterstaffAttackRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeQuarterstaffAttack()
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
                            StartCoroutine(QuarterstaffAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(QuarterstaffAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator QuarterstaffAttackEffects(GameObject initiator, GameObject recipient)
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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Quarterstaff(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void RollDice_DealDamage_Quarterstaff(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        //Calculate Hit, AC, & Damage
        int initiatorAttackRoll = scriptManager.scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
        int initiatorDamageRoll = Random.Range(1, 6) + initiatorStats.damageModifier;
        int initiatorCritDamageRoll = Random.Range(1, 6) + Random.Range(1, 6) + initiatorStats.damageModifier;
        int recipientArmorClass = recipientStats.armorClass;

        //Initiator Attack Roll Hits
        if (initiatorAttackRoll >= recipientArmorClass)
        {
            //Critical Hit
            if (initiatorAttackRoll - initiatorStats.attackModifier == 20)
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorCritDamageRoll);
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                Debug.Log(initiatorStats.unitName + " Rolled a Crit, so " + recipientStats.unitName + " took " + initiatorCritDamageRoll + " damage");
            }
            //No Critical Hit
            else
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorDamageRoll);
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was higher than " + recipientStats.unitName + "'s AC of " + recipientArmorClass + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
            }

            //Particle Effect on Hit
            //GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            //Destroy(tempParticle, 2f);

            //Kill Dead Units & Check for Winner
            if (scriptManager.scriptBattleController.CheckIfDead(recipient))
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

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }


    public void StartLightCrossbowAttack()
    {
        //Set Variables
        scriptManager.scriptBattleController.lightCrossbowAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Piercing";
        scriptUnitStats.attackRange = 6;

        //Update UI
        HighlightLightCrossbowAttackRange();
        scriptManager.scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightLightCrossbowAttackRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeLightCrossbowAttack()
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
                            StartCoroutine(LightCrossbowAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(LightCrossbowAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator LightCrossbowAttackEffects(GameObject initiator, GameObject recipient)
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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_LightCrossbow(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void RollDice_DealDamage_LightCrossbow(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        //Calculate Hit, AC, & Damage
        int initiatorAttackRoll = scriptManager.scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
        int initiatorDamageRoll = Random.Range(1, 8) + initiatorStats.damageModifier;
        int initiatorCritDamageRoll = Random.Range(1, 8) + Random.Range(1, 8) + initiatorStats.damageModifier;
        int recipientArmorClass = recipientStats.armorClass;

        //Initiator Attack Roll Hits
        if (initiatorAttackRoll >= recipientArmorClass)
        {
            //Critical Hit
            if (initiatorAttackRoll - initiatorStats.attackModifier == 20)
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorCritDamageRoll);
                FindObjectOfType<AudioManager>().Play("Light Crossbow");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                Debug.Log(initiatorStats.unitName + " Rolled a Crit, so " + recipientStats.unitName + " took " + initiatorCritDamageRoll + " damage");
            }
            //No Critical Hit
            else
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorDamageRoll);
                FindObjectOfType<AudioManager>().Play("Light Crossbow");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                Debug.Log("Archer Attack - Light Crossbow");
                Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was higher than " + recipientStats.unitName + "'s AC of " + recipientArmorClass + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
            }

            //Particle Effect on Hit
            //GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            //Destroy(tempParticle, 2f);

            //Kill Dead Units & Check for Winner
            if (scriptManager.scriptBattleController.CheckIfDead(recipient))
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
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }

    public void StartScimitarAttack()
    {
        //Set Variables
        scriptManager.scriptBattleController.scimitarAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Slashing";
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightScimitarAttackRange();
        scriptManager.scriptGameMenuController.CloseAllActionMenus();
    }

    //Highlight Tiles on Quarterstaff Attack Action Menu Button
    public void HighlightScimitarAttackRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeScimitarAttack()
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
                            StartCoroutine(ScimitarAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(ScimitarAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator ScimitarAttackEffects(GameObject initiator, GameObject recipient)
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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Scimitar(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void RollDice_DealDamage_Scimitar(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        //Calculate Hit, AC, & Damage
        int initiatorAttackRoll = scriptManager.scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
        int initiatorDamageRoll = Random.Range(1, 6) + initiatorStats.damageModifier;
        int initiatorCritDamageRoll = Random.Range(1, 6) + Random.Range(1, 6) + initiatorStats.damageModifier;
        int recipientArmorClass = recipientStats.armorClass;

        //Initiator Attack Roll Hits
        if (initiatorAttackRoll >= recipientArmorClass)
        {
            //Critical Hit
            if (initiatorAttackRoll - initiatorStats.attackModifier == 20)
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorCritDamageRoll);
                FindObjectOfType<AudioManager>().Play("Scimitar Attack");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                Debug.Log(initiatorStats.unitName + " Rolled a Crit, so " + recipientStats.unitName + " took " + initiatorCritDamageRoll + " damage");
            }
            //No Critical Hit
            else
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorDamageRoll);
                FindObjectOfType<AudioManager>().Play("Scimitar Attack");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was higher than " + recipientStats.unitName + "'s AC of " + recipientArmorClass + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
            }

            //Particle Effect on Hit
            //GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            //Destroy(tempParticle, 2f);

            //Kill Dead Units & Check for Winner
            if (scriptManager.scriptBattleController.CheckIfDead(recipient))
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
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }

    public void StartDaggerThrowAttack()
    {
        //Set Variables
        scriptManager.scriptBattleController.daggerThrowAttack = true;
        scriptManager.scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Piercing";
        scriptUnitStats.attackRange = 4;

        //Update UI
        HighlightDaggerThrowAttackRange();
        scriptManager.scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightDaggerThrowAttackRange()
    {
        scriptManager.scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptManager.scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeDaggerThrowAttack()
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
                            StartCoroutine(DaggerThrowAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(DaggerThrowAttackEffects(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptManager.scriptUnitSelection.DeselectUnitAfterMovement(scriptManager.scriptTileMap.selectedUnit, unitClicked));
                }
            }
        }
    }

    public IEnumerator DaggerThrowAttackEffects(GameObject initiator, GameObject recipient)
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
            StartCoroutine(scriptManager.scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptManager.scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_DaggerThrow(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptManager.scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void RollDice_DealDamage_DaggerThrow(GameObject initiator, GameObject recipient)
    {
        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        //Calculate Hit, AC, & Damage
        int initiatorAttackRoll = scriptManager.scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
        int initiatorDamageRoll = Random.Range(1, 4) + initiatorStats.damageModifier;
        int initiatorCritDamageRoll = Random.Range(1, 4) + Random.Range(1, 4) + initiatorStats.damageModifier;
        int recipientArmorClass = recipientStats.armorClass;

        //Initiator Attack Roll Hits
        if (initiatorAttackRoll >= recipientArmorClass)
        {
            //Critical Hit
            if (initiatorAttackRoll - initiatorStats.attackModifier == 20)
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorCritDamageRoll);
                FindObjectOfType<AudioManager>().Play("Dagger Throw");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorCritDamageRoll));
                Debug.Log(initiatorStats.unitName + " Rolled a Crit, so " + recipientStats.unitName + " took " + initiatorCritDamageRoll + " damage");
            }
            //No Critical Hit
            else
            {
                //Deal Damage
                recipientUnit.DealDamage(initiatorDamageRoll);
                FindObjectOfType<AudioManager>().Play("Dagger Throw");
                StartCoroutine(recipient.GetComponent<UnitController>().DisplayDamage(initiatorDamageRoll));
                Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was higher than " + recipientStats.unitName + "'s AC of " + recipientArmorClass + ", so " + recipientStats.unitName + " took " + initiatorDamageRoll + " damage");
            }

            //Particle Effect on Hit
            //GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitController>().damageParticles, recipient.transform.position, recipient.transform.rotation);
            //Destroy(tempParticle, 2f);

            //Kill Dead Units & Check for Winner
            if (scriptManager.scriptBattleController.CheckIfDead(recipient))
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
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptManager.scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }
}
