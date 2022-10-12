using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    //Variables
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
    public int attackModifier;
    public int damageModifier;
    public int maxAttackRange;
    public int attackRange;
    public string damageType;

    [Header("Spellcasting")]
    public int spellAttackModifier;
    public int spellSaveModifier;
    public string spellDamageType;

    [Header("Movement")]
    public int movementSpeed;
    public float visualMovementSpeed = .15f;

    [Header("Health")]
    public int maxHP;
    public int hitDie;

    public void Start()
    {
        SetVariables();
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
}