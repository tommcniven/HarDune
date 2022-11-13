using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Unit Info")]
    public string unitName;
    public string unitClass;
    public string unitSubClass;
    public string unitRace;
    public int unitLevel = 1;
    public int proficiency;

    [Header("Ability Stats")]
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    public int strengthModifier;
    public int dexterityModifier;
    public int constitutionModifier;
    public int intelligenceModifier;
    public int wisdomModifier;
    public int charismaModifier;

    [Header("Combat")]
    public int armorClass;
    public int baseArmorClass;
    public int attackModifier;
    public int damageModifier;
    public int maxAttackRange;
    public int baseAttackRange;
    public int attackRange;
    public string damageType;

    [Header("Spellcasting")]
    public int spellAttackModifier;
    public int spellSaveModifier;
    public int spellSaveDC;
    public string spellDamageType;
    public int splashRange;

    [Header("Movement")]
    public int movementSpeed;
    public int baseMovementSpeed;
    public float visualMovementSpeed = .15f;

    [Header("Health")]
    public int maxHP;
    public int hitDie;

    [Header("Vision")]
    public int visionRange;

    [Header("Condition")]
    public bool isBarkskinApplied = false;
    public int dodgeTimer;

    public void Start()
    {
        SetScriptManager();
        SetVariables();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    private void OnEnable()
    {
        GeneralActions.onDodge += UpdateArmorClass;
        TurnController.onTurnChange += SetDodgeTimer;
        UnitController.onConditionChange += UpdateUnitCondition;
    }

    private void OnDisable()
    {
        GeneralActions.onDodge -= UpdateArmorClass;
        TurnController.onTurnChange -= SetDodgeTimer;
        UnitController.onConditionChange += UpdateUnitCondition;
    }

    private void SetVariables()
    {
        //Set Ability Modifiers
        strengthModifier = (int)Mathf.Floor((strength - 10) / 2);
        dexterityModifier = (int)Mathf.Floor((dexterity - 10) / 2);
        constitutionModifier = (int)Mathf.Floor((constitution - 10) / 2);
        intelligenceModifier = (int)Mathf.Floor((intelligence - 10) / 2);
        wisdomModifier = (int)Mathf.Floor((wisdom - 10) / 2);
        charismaModifier = (int)Mathf.Floor((charisma - 10) / 2);

        //Set Armor Class
        //[Update] Need to Update to include armor when applicable
        armorClass = (10 + dexterityModifier);
        spellSaveDC = 8 + spellAttackModifier;

        //Set Base Values
        baseArmorClass = armorClass;
        baseAttackRange = maxAttackRange;


        //Set Proficiency
        if (unitLevel <= 4)
        {
            proficiency = 2;
        }
        else if (unitLevel >= 5)
        {
            if (unitLevel <= 8)
            {
                proficiency = 3;
            }
        }
        else if (unitLevel >= 9)
        {
            if (unitLevel <= 12)
            {
                proficiency = 4;
            }
        }
        else if (unitLevel >= 13)
        {
            if (unitLevel <= 16)
            {
                proficiency = 5;
            }
        }
        else if (unitLevel >= 17)
        {
            proficiency = 6;
        }

        //Set Attack Modifier
        if (unitClass == "Rogue" || unitClass =="Bandit")
        {
            attackModifier = (proficiency + dexterityModifier);
        }
        else
        {
            attackModifier = (proficiency + strengthModifier);
        }

        //Set Damage Modifier
        if (unitClass == "Rogue" || unitClass == "Bandit")
        {
            damageModifier = dexterityModifier;
        }
        else
        {
            damageModifier = strengthModifier;
        }


        //Set HP for PCs
        if (unitClass == "Rogue" || unitClass == "Druid" || unitClass == "Fighter")
        maxHP = (hitDie + (constitutionModifier * unitLevel));


        //Set Spellcasting Modifiers
        if (unitClass == "Druid")
        {
            spellAttackModifier = (proficiency + wisdomModifier);
            spellSaveModifier = (8 + proficiency + wisdomModifier);
        }
        else if (unitClass == "Wizard")
        {
            spellAttackModifier = (proficiency + intelligenceModifier);
            spellSaveModifier = (8 + proficiency + intelligenceModifier);
        }
        else
        {
            spellAttackModifier = 0;
            spellSaveModifier = 0;
        }
    }

    public void UpdateArmorClass()
    {
        if (scriptManager.scriptBattleController.isDodging == true)
        {
            armorClass += 3;
        }

        else
        {
            armorClass = baseArmorClass;
        }
    }

    public void SetDodgeTimer()
    {
        dodgeTimer++;

        if (dodgeTimer == scriptManager.scriptTurnController.numberOfTeams)
        {
            UpdateArmorClass();
            scriptManager.scriptBattleController.ResetActionBools();
            dodgeTimer = 0;
        }
    }

    public void UpdateUnitCondition()
    {
        var unitConditionState = transform.GetComponent<UnitController>().unitConditionState;

        if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Normal)
        {
            transform.GetComponent<UnitController>().SetActionState(0); //Reverses Frightened
            maxAttackRange = baseAttackRange; //Reverse Paralyzed, Petrified, Stunned, & Unconscious
            movementSpeed = baseMovementSpeed; //Reverse Grappled
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Charmed)
        {
            //Note -- Add Icon
            //Note -- Add "A charmed creature can't attack the charmer or target the charmer with harmful abilities or magical effects"
            //Note -- Add "The charmer has advantage on any ability check to interact soially with the creature"
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Deafened)
        {
            //Note -- Add Icon
            //Note -- Add "The deafened creature can't hear and automatically fails any ability check that requires hearing"
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Frightened)
        {
            transform.GetComponent<UnitController>().SetActionState(2);
            //Note -- Add Icon
            //Note -- Add "The creature can't willingly move closer to the source of its fear"
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Grappled)
        {
            movementSpeed = 0;
            //Note -- Add Icon
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Paralyzed)
        {
            maxAttackRange = 0;
            movementSpeed = 0;
            //Note -- Add Icon
            //Note -- Turn off Attack, Action, and Spellbook Menu
            //Note -- Add "The creature automatically fails Strength & Dexterity Saving Throws
            //Note -- Add "Attack rolls gainst the creature have advantage"
            //Note -- Add "Any attack that hits the creature is a critical hit if the attacker is within 5 feet of the creature"
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Petrified)
        {
            maxAttackRange = 0;
            movementSpeed = 0;
            //Note -- Add Icon
            //Note -- Turn off Attack, Action, and Spellbook Menu
            //Note -- Add "The creature automatically fails Strength & Dexterity Saving Throws"
            //Note -- Add "Attack rolls gainst the creature have advantage"
            //Note -- Add "The creature has resistance to all damage"
            //Note -- Add "The creature is imune to poison and disease, although a poison or disease already in its system is usspended, not neutrailized"
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Restrained)
        {
            movementSpeed = 0;
            //Note -- Add Icon
            //Note -- Add "Attack rolls gainst the creature have advantage"
            //Note -- Add "The creature has disadvantage on Dexterity Saving THrows"
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Stunned)
        {
            maxAttackRange = 0;
            movementSpeed = 0;
            //Note -- Add Icon
            //Note -- Turn off Attack, Action, and Spellbook Menu
            //Note -- Add "The creature automatically fails Strength & Dexterity Saving Throws"
            //Note -- Add "Attack rolls gainst the creature have advantage"
        }

        else if (transform.GetComponent<UnitController>().unitConditionState == UnitController.ConditionState.Unconscious)
        {
            maxAttackRange = 0;
            movementSpeed = 0;
            //Note -- Add Animation
            //Note -- Add New Character Artwork
            //Note -- Turn off Attack, Action, and Spellbook Menu
            //Note -- Add "The creature falls prone"
            //Note -- Add "The creature automatically fails Strength & Dexterity Saving Throws"
            //Note -- Add "Attack rolls gainst the creature have advantage"
            //Note -- Add "Any attack that hits the creature is a critical hit if the attacker is within 5 feet of the creature"
        }
    }
}