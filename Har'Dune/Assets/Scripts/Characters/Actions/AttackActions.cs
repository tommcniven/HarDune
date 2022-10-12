using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackActions : MonoBehaviour
{
    [Header("Scripts")]
    public UnitStats scriptUnitStats;
    public UnitController scriptUnitController;
    public BattleController scriptBattleController;
    public GameController scriptGameController;
    public TileMap scriptTileMap;
    public GameMenuController scriptGameMenuController;
    public CameraShake scriptCameraShake;
    public RangeFinder scriptRangeFinder;

    public void Update()
    {
        //Click to Use Respective Attack on Enemy Units
        if (Input.GetMouseButtonDown(0))
        {
            if (scriptBattleController.battleStatus)
            {
                if (scriptBattleController.greatswordAttack)
                {
                    MakeGreatswordAttack();
                    scriptBattleController.ResetActionBools();
                }
                else if (scriptBattleController.daggerAttack)
                {
                    MakeDaggerAttack();
                    scriptBattleController.ResetActionBools();
                }
                else if (scriptBattleController.quarterstaffAttack)
                {
                    MakeQuarterstaffAttack();
                    scriptBattleController.ResetActionBools();
                }
                else if (scriptBattleController.lightCrossbowAttack)
                {
                    MakeLightCrossbowAttack();
                    scriptBattleController.ResetActionBools();
                }
                else if (scriptBattleController.scimitarAttack)
                {
                    MakeScimitarAttack();
                    scriptBattleController.ResetActionBools();
                }
                else if (scriptBattleController.daggerThrowAttack)
                {
                    MakeDaggerThrowAttack();
                    scriptBattleController.ResetActionBools();
                }
            }
        }
    }

    public void StartGreatswordAttack()
    {
        //Set Variables
        scriptBattleController.greatswordAttack = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Slashing";
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightGreatswordAttackRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightGreatswordAttackRange()
    {
        scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeGreatswordAttack()
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
                            StartCoroutine(GreatswordAttackEffects(scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(GreatswordAttackEffects(scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Greatsword(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
        }
    }

    public void RollDice_DealDamage_Greatsword(GameObject initiator, GameObject recipient)
    {

        //Initiator & Recipient
        var initiatorUnit = initiator.GetComponent<UnitController>();
        var initiatorStats = initiator.GetComponent<UnitStats>();
        var recipientUnit = recipient.GetComponent<UnitController>();
        var recipientStats = recipient.GetComponent<UnitStats>();

        int initiatorAttackRoll = scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
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
            //FindObjectOfType<AudioManager>().Play("Fighter Attack Grunt");
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }

    public void StartDaggerAttack()
    {
        //Set Variables
        scriptBattleController.daggerAttack = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Stabbing";
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightDaggerAttackRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightDaggerAttackRange()
    {
        scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeDaggerAttack()
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
                            StartCoroutine(DaggerAttackEffects(scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(DaggerAttackEffects(scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().dexterityModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Dagger(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
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
        int initiatorAttackRoll = scriptBattleController.AttackRoll();
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
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }

    public void StartQuarterstaffAttack()
    {
        //Set Variables
        scriptBattleController.quarterstaffAttack = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Bludgeoning";
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightQuarterstaffAttackRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightQuarterstaffAttackRange()
    {
        scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeQuarterstaffAttack()
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
                            StartCoroutine(QuarterstaffAttackEffects(scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(QuarterstaffAttackEffects(scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Quarterstaff(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
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
        int initiatorAttackRoll = scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
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

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }


    public void StartLightCrossbowAttack()
    {
        //Set Variables
        scriptBattleController.lightCrossbowAttack = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Piercing";
        scriptUnitStats.attackRange = 6;

        //Update UI
        HighlightLightCrossbowAttackRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightLightCrossbowAttackRange()
    {
        scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeLightCrossbowAttack()
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
                            StartCoroutine(LightCrossbowAttackEffects(scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(LightCrossbowAttackEffects(scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_LightCrossbow(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
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
        int initiatorAttackRoll = scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
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
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }

    public void StartScimitarAttack()
    {
        //Set Variables
        scriptBattleController.scimitarAttack = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Slashing";
        scriptUnitStats.attackRange = 1;

        //Update UI
        HighlightScimitarAttackRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    //Highlight Tiles on Quarterstaff Attack Action Menu Button
    public void HighlightScimitarAttackRange()
    {
        scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeScimitarAttack()
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
                            StartCoroutine(ScimitarAttackEffects(scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(ScimitarAttackEffects(scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_Scimitar(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
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
        int initiatorAttackRoll = scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
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
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }

    public void StartDaggerThrowAttack()
    {
        //Set Variables
        scriptBattleController.daggerThrowAttack = true;
        scriptBattleController.battleStatus = true;
        scriptUnitStats.damageType = "Piercing";
        scriptUnitStats.attackRange = 4;

        //Update UI
        HighlightDaggerThrowAttackRange();
        scriptGameMenuController.CloseAllActionMenus();
    }

    public void HighlightDaggerThrowAttackRange()
    {
        scriptRangeFinder.HighlightAttackableUnitsInRange();
        scriptTileMap.HighlightNodeUnitIsOccupying();
    }

    public void MakeDaggerThrowAttack()
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
                            StartCoroutine(DaggerThrowAttackEffects(scriptTileMap.selectedUnit, unitOnTile));
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
                    StartCoroutine(DaggerThrowAttackEffects(scriptTileMap.selectedUnit, unitClicked));
                    StartCoroutine(scriptTileMap.DeselectUnitAfterMovement(scriptTileMap.selectedUnit, unitClicked));
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
        while (scriptBattleController.battleStatus)
        {
            StartCoroutine(scriptCameraShake.ShakeCamera(.2f, initiator.GetComponent<UnitStats>().strengthModifier, scriptBattleController.GetDirection(initiator, recipient)));
            RollDice_DealDamage_DaggerThrow(initiator, recipient);
            yield return new WaitForEndOfFrame();
        }

        //Return to Start Position After Attack
        if (initiator != null)
        {
            StartCoroutine(scriptBattleController.ReturnAfterAttack(initiator, initiatorPosition));
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
        int initiatorAttackRoll = scriptBattleController.AttackRoll() + initiatorStats.attackModifier;
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
            FindObjectOfType<AudioManager>().Play("Attack Missed");

            Debug.Log(initiatorStats.unitName + "'s Attack Roll of " + initiatorAttackRoll + " was lower than " + recipientStats.unitName + "'s AC of " + recipientArmorClass);
            scriptBattleController.battleStatus = false;
        }

        //Remove Disadvantage
        initiatorUnit.GetComponent<UnitController>().SetAttackState(0);
    }
}
