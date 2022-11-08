using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spells", menuName = "Spell")]
public class SpellsSO : ScriptableObject
{
    [Header("Spell Information")]
    public string spellName;
    public string spellDescription;
    public string spellAtHigherLevels;
    public string spellSchool;
    public string spellDamageType;
    public int spellAttackDice;
    public int spellAttackDamage;
    public int spellLevel;
    public string spellCastingTime;
    public int spellRange;
    public string spellComponents;
    public string spellDuration;
    public bool spellIsConcentration;
    public Sprite sepllIconArtwork;
}