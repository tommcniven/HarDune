using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleController : MonoBehaviour
{
    //Variables
    [Header("Scripts")]
    public CameraShake scriptCameraShake;
    public GameController scriptGameController;
    public TileMap scriptTileMap;
    public AttackActions scriptAttackActions;

    [Header("Statuses")]
    public bool battleStatus = false;

    [Header("Actions")]
    public bool grappleAction = false;
    public bool hideAction = false;
    public bool stealthAction = false;

    [Header("Attacks")]
    public bool greatswordAttack = false;
    public bool daggerAttack = false;
    public bool quarterstaffAttack = false;
    public bool lightCrossbowAttack = false;
    public bool scimitarAttack = false;
    public bool daggerThrowAttack = false;

    [Header("Spells")]
    public bool druidcraft = false;
    public bool frostbite = false;
    public bool cureWounds = false;
    public bool charmPerson = false;

    public void ResetActionBools()
    {
        //Actions
        grappleAction = false;
        hideAction = false;
        stealthAction = false;

        //Attacks
        greatswordAttack = false;
        daggerAttack = false;
        quarterstaffAttack = false;
        lightCrossbowAttack = false;
        scimitarAttack = false;
        daggerThrowAttack = false;

        //Spells
        druidcraft = false;
        frostbite = false;
        cureWounds = false;
        charmPerson = false;
    }

    //Death Check
    public bool CheckIfDead(GameObject unitCurrentHeatlh)
    {
        //Unit HP <= 0
        if (unitCurrentHeatlh.GetComponent<UnitController>().currentHP <= 0)
        {
            return true;
        }

        return false;
    }

    //Destroy Unit
    public void DestroyObject(GameObject deadUnit)
    {
        Destroy(deadUnit);
    }

    // Return Unit to Pre-Battle Position & Set Idle
    public IEnumerator ReturnAfterAttack(GameObject initiator, Vector3 endPoint)
    {
        float elapsedTime = 0;

        //Smooth Movement
        while (elapsedTime < .30f)
        {
            initiator.transform.position = Vector3.Lerp(initiator.transform.position, endPoint, (elapsedTime / .25f));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        initiator.GetComponent<UnitController>().SetWaitAnimation();
        initiator.GetComponent<UnitController>().Wait();
    }

    //Movement Direction
    public Vector3 GetDirection(GameObject initiator, GameObject recipient)
    {
        Vector3 startingPosition = initiator.transform.position;
        Vector3 endingPosition = recipient.transform.position;
        return ((endingPosition - startingPosition) / (endingPosition - startingPosition).magnitude).normalized;
    }

    //Dice Rolls
    public int RollD20()
    {
        int rollD20 = Random.Range(1, 20);
        return rollD20;
    }
    public int RollD20Advantage()
    {
        int rollOne = RollD20();
        int rollTwo = RollD20();
        int rollD20s = Mathf.Max(rollOne, rollTwo);
        return rollD20s;
    }
    public int RollD20Disadvantage()
    {
        int rollOne = RollD20();
        int rollTwo = RollD20();
        int rollD20s = Mathf.Min(rollOne, rollTwo);
        return rollD20s;
    }

    //Attack Rolls
    public int AttackRoll()
    {
        var selectedUnitStats= scriptTileMap.selectedUnit.GetComponent<UnitStats>();
        var selectedUnitController = scriptTileMap.selectedUnit.GetComponent<UnitController>();

        //Attack Normal
        if (selectedUnitController.GetComponent<UnitController>().unitAttackState == selectedUnitController.GetComponent<UnitController>().GetAttackState(0))
        {
            int attackRoll = RollD20() + selectedUnitStats.attackModifier;
            return attackRoll;
        }
        //Attack Advantage
        else if (selectedUnitController.GetComponent<UnitController>().unitAttackState == selectedUnitController.GetComponent<UnitController>().GetAttackState(1))
        {
            int attackRoll = RollD20Advantage() + selectedUnitStats.attackModifier;
            return attackRoll;
        }
        //Attack Disadvantage
        else if (selectedUnitController.GetComponent<UnitController>().unitAttackState == selectedUnitController.GetComponent<UnitController>().GetAttackState(2))
        {
            int attackRoll = RollD20Disadvantage() + selectedUnitStats.attackModifier;
            return attackRoll;
        }
        return 100;
    }

    //Attack Rolls
    public int ActionRoll()
    {
        var selectedUnitStats = scriptTileMap.selectedUnit.GetComponent<UnitStats>();
        var selectedUnitController = scriptTileMap.selectedUnit.GetComponent<UnitController>();

        //Attack Normal
        if (selectedUnitController.GetComponent<UnitController>().unitActionState == selectedUnitController.GetComponent<UnitController>().GetActionState(0))
        {
            int attackRoll = RollD20() + selectedUnitStats.attackModifier;
            return attackRoll;
        }
        //Attack Advantage
        else if (selectedUnitController.GetComponent<UnitController>().unitActionState == selectedUnitController.GetComponent<UnitController>().GetActionState(1))
        {
            int attackRoll = RollD20Advantage() + selectedUnitStats.attackModifier;
            return attackRoll;
        }
        //Attack Disadvantage
        else if (selectedUnitController.GetComponent<UnitController>().unitActionState == selectedUnitController.GetComponent<UnitController>().GetActionState(2))
        {
            int attackRoll = RollD20Disadvantage() + selectedUnitStats.attackModifier;
            return attackRoll;
        }
        return 100;
    }

    //Spell Save
    public int SpellSaveRoll()
    {
        var selectedUnitController = scriptTileMap.selectedUnit.GetComponent<UnitController>();

        //Attack Normal
        if (selectedUnitController.GetComponent<UnitController>().unitSpellSaveState == selectedUnitController.GetComponent<UnitController>().GetSpellSaveState(0))
        {
            int attackRoll = RollD20();
            return attackRoll;
        }
        //Attack Advantage
        else if (selectedUnitController.GetComponent<UnitController>().unitSpellSaveState == selectedUnitController.GetComponent<UnitController>().GetSpellSaveState(1))
        {
            int attackRoll = RollD20Advantage();
            return attackRoll;
        }
        //Attack Disadvantage
        else if (selectedUnitController.GetComponent<UnitController>().unitSpellSaveState == selectedUnitController.GetComponent<UnitController>().GetSpellSaveState(2))
        {
            int attackRoll = RollD20Disadvantage();
            return attackRoll;
        }
        return 100;
    }

    //Spell Cast
    public int SpellCastRoll()
    {
        var selectedUnitController = scriptTileMap.selectedUnit.GetComponent<UnitController>();

        //Attack Normal
        if (selectedUnitController.GetComponent<UnitController>().unitSpellCastState == selectedUnitController.GetComponent<UnitController>().GetSpellCastState(0))
        {
            int attackRoll = RollD20();
            return attackRoll;
        }
        //Attack Advantage
        else if (selectedUnitController.GetComponent<UnitController>().unitSpellCastState == selectedUnitController.GetComponent<UnitController>().GetSpellCastState(1))
        {
            int attackRoll = RollD20Advantage();
            return attackRoll;
        }
        //Attack Disadvantage
        else if (selectedUnitController.GetComponent<UnitController>().unitSpellCastState == selectedUnitController.GetComponent<UnitController>().GetSpellCastState(2))
        {
            int attackRoll = RollD20Disadvantage();
            return attackRoll;
        }
        return 100;
    }
}