using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitController : MonoBehaviour
{
    //Variables
    [Header("Scripts")]
    public UnitStats scriptUnitStats;
    public AudioManager scriptAudioManager;

    [Header("Statuses")]
    public AttackStates unitAttackState;
    public ActionStates unitActionState;
    public SpellSaveStates unitSpellSaveState;
    public SpellCastStates unitSpellCastState;
    public ConditionState unitConditionState;

    [Header("Info")]
    public int teamNumber;
    public int x;
    public int y;
    public Queue<int> movementQueue;
    public Queue<int> combatQueue;
    public GameObject occupiedTile;
    public GameObject damageParticles;
    public bool coroutineRunning;
    public Animator animator;

    //Update: Move to Custom Class Scripts for Multiple Class Types
    [Header("Stats")]
    public string unitName;
    public int moveSpeed = 2;
    public float moveSpeedTime = 1f;
    public int attackDamage = 1;
    public int currentHP;
    public int armorClass;
    public Sprite unitSprite;

    //UI Variables
    [Header("UI")]
    public Canvas healthBarCanvas;
    public Image healthBarFill;
    public Canvas damagePopupCanvas;
    public GameObject damageDisplayPanel;
    public TMP_Text damagePopupText;
    public Image damageBackdrop;

    //Map Location
    [Header("Map")]
    public TileMap map;
    public GameObject unitModel;
    public MovementStates unitMovementStates;

    //Movement Bool
    public bool unitInMovement;

    //Unit States Enum Variables
    public enum MovementStates
    {
        Unselected,
        Selected,
        Moved,
        ActionSelected,
        BonusActionSelected,
        Wait
    }
   
    //Pathfinding
    public List<Node> path = null;
    public List<Node> pathForMovement = null;
    public bool completedMovement = false;

    //Out: Initialize Varaiables
    private void Awake()
    {
        //Get Components
        animator = unitModel.GetComponent<Animator>();

        //Set Variables
        SetAwakeVariables();
        StartCoroutine(SetCurrentHP());
    }

    //Set Awake Variables
    public void SetAwakeVariables()
    {
        movementQueue = new Queue<int>();
        combatQueue = new Queue<int>();
        x = (int)transform.position.x;
        y = (int)transform.position.z;
        unitMovementStates = MovementStates.Unselected;
    }

    public IEnumerator SetCurrentHP()
    {
        //Must be Yielded for MaxHP to be set in UnitStats
        yield return new WaitForEndOfFrame();
        currentHP = scriptUnitStats.maxHP;
    }

    public void MoveNextTile()
    {
        //Path Count == 0
        if (path.Count == 0)
        {
            return;
        }

        //Path Count != 0
        else
        {
            StartCoroutine(SmoothMovement(transform.gameObject, path[path.Count - 1]));
        }
     }

    public void ResetSingleUnitMovement()
    {
        path = null;
        SetMovementState(0);
        completedMovement = false;
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        SetIdleAnimation();
    }

    //Out: Movement Sate Enum
    //Note: Define Enum Movement States
    public MovementStates GetMovementState(int i)
    {
        //Unselected
        if (i == 0)
        {
            return MovementStates.Unselected;
        }

        //Selected
        else if (i == 1)
        {
            return MovementStates.Selected;
        }

        //Moved
        else if (i == 2)
        {
            return MovementStates.Moved;
        }

        //Wait
        else if (i == 3)
        {
            return MovementStates.Wait;
        }

        //Unselected
        return MovementStates.Unselected;
    }

    //Out: Movement States Variables
    public void SetMovementState(int i)
    {
        //Unselected
        if (i == 0)
        {
            unitMovementStates =  MovementStates.Unselected;
        }

        //Selected
        else if (i == 1)
        {
            unitMovementStates = MovementStates.Selected;
        }

        //Moved
        else if (i == 2)
        {
            unitMovementStates = MovementStates.Moved;
        }

        //Wait
        else if (i == 3)
        {
            unitMovementStates = MovementStates.Wait;
        }
    }

    //Out: HealthBar Fill
    //Note: Set HealthBar Fill to Current Health / Max Health
    public void UpdateHealthBar()
    {
        healthBarFill.fillAmount = (float)currentHP / scriptUnitStats.maxHP;
    }

    //Out: Damage Dealt Calculation & Update
    public void DealDamage(int damage)
    {
        currentHP = currentHP - damage;
        UpdateHealthBar();
    }

    //Heal Damage
    public void HealDamage(int heal)
    {
        int tempCurrentHP = currentHP + heal;

        if (tempCurrentHP >= scriptUnitStats.maxHP)
        {
            currentHP = scriptUnitStats.maxHP;
        }
        else
        {
            currentHP = tempCurrentHP;
        }

        UpdateHealthBar();
    }

    //Out: Set Sprit Color to Gray
    //Use: Used to show Sprite Waiting Post Action
    public void Wait()
    {
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.gray;
    }

    //Out: Destroy Unit on Death
    //Note: Simple Fade Out
    //Use: Remove Dead Units
    public void UnitDie()
    {
        if (unitModel.activeSelf)
        {
            StartCoroutine(FadeOut());
            StartCoroutine(DestroyGameObject());
        }
    }

    //Out: Yield Return in Combat
    public IEnumerator DestroyGameObject()
    {
        //Combat in Combat Queue
        while (combatQueue.Count>0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        Destroy(gameObject);
    }

    //Out: Fade Out Effect
    //Use: Unit Death
    public IEnumerator FadeOut()
    {
        combatQueue.Enqueue(1);
        //setDieAnimation();
        //yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Renderer rend = GetComponentInChildren<SpriteRenderer>();
        
        //Fade Color Alpha Until 0.5
        for (float f = 1f; f >= .05; f -= 0.01f)
        {
            Color c = rend.material.color;
            c.a = f;
            rend.material.color = c;
            yield return new WaitForEndOfFrame();
        }
        combatQueue.Dequeue();
    }

    //Lerp Movement Enum
    //[Update] Add Flip Unit Code from Game Controller Script
    public IEnumerator SmoothMovement(GameObject movingObject, Node endNode)
    {
        movementQueue.Enqueue(1);
        FindObjectOfType<AudioManager>().PlayFootstepAudio();

        //Removes Tile the Unit is on
        path.RemoveAt(0);

        //Path Count != 0
        while (path.Count != 0)
        {
            Vector3 endPosition = map.NodePositionInScene(path[0].x, path[0].y);
            StartCoroutine(SetUnitDirection());
            movingObject.transform.position = Vector3.Lerp(transform.position, endPosition, scriptUnitStats.visualMovementSpeed);

            //At 0, Remove Tile Unit is On
            if ((transform.position - endPosition).sqrMagnitude < 0.001)
            {
                path.RemoveAt(0);
            }

            yield return new WaitForEndOfFrame();
        }

        scriptUnitStats.visualMovementSpeed = 0.15f;
        transform.position = map.NodePositionInScene(endNode.x, endNode.y);
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0);
        x = endNode.x;
        y = endNode.y;
        occupiedTile.GetComponent<ClickableTile>().unitOnTile = null; 
        occupiedTile = map.tilesOnMap[x, y];
        movementQueue.Dequeue();
        FindObjectOfType<AudioManager>().StopFootstepAudio();
    }

    public IEnumerator SetUnitDirection()
    {
        Vector3 endPosition = map.NodePositionInScene(path[0].x, path[0].y);

        //Move Left
        if (endPosition.x < transform.position.x)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 180);
        }

        //Move Right
        if (endPosition.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0);
        }

        yield return new WaitForEndOfFrame();
    }

    public IEnumerator DisplayDamage(int damageTaken)
    {
        combatQueue.Enqueue(1); 
        damagePopupText.SetText(damageTaken.ToString());
        damagePopupCanvas.enabled = true;

        //Display Damage Panel then Fade Out
        for (float f = 1f; f >=-0.01f; f -= 0.01f)
        {
            Color damagePanel = damageBackdrop.GetComponent<Image>().color;
            Color damageValue = damagePopupText.color;
            damagePanel.a = f;
            damageValue.a = f;
            damageBackdrop.GetComponent<Image>().color = damagePanel;
            damagePopupText.color = damageValue;
            yield return new WaitForEndOfFrame();
        }

        combatQueue.Dequeue();
    }

    //Out: Set Animation to Selected Animation
    public void SetSelectedAnimation()
    {
        animator.SetTrigger("Selected-Animation");
    }

    //Out: Set Animation to Idle Animation
    public void SetIdleAnimation()
    {        
        animator.SetTrigger("Idle-Animation");
    }

    //Out: Set Animation to Run Animation
    public void SetRunAnimation()
    {
        animator.SetTrigger("Run-Animation");
    }

    //Out: Set Animation to Attack Animation
    public void SetAttackAnimation()
    {
       animator.SetTrigger("Attack-Animation");
    }

    //Out: Set Animation to idle Wait Animation
    public void SetWaitAnimation()
    {
        
        animator.SetTrigger("Wait-Animation");
    }

    //Out: Set Animation to Die Animation
    public void SetDieAnimation()
    {
        animator.SetTrigger("Die-Animation");
    }

    public void SetSpellcastAnimation()
    {
        animator.SetTrigger("Spellcast-Animation");
    }

    //Attack States Enum
    public enum AttackStates
    {
        Normal,
        Advantage,
        Disadvantage
    }

    //Get Attack States
    public AttackStates GetAttackState(int i)
    {
        if (i == 0)
        {
            return AttackStates.Normal;
        }

        else if (i == 1)
        {
            return AttackStates.Advantage;
        }

        else if (i == 2)
        {
            return AttackStates.Disadvantage;
        }

        return AttackStates.Normal;
    }

    //Set Attack State
    public void SetAttackState(int i)
    {
        if (i == 0)
        {
            unitAttackState = AttackStates.Normal;
        }

        else if (i == 1)
        {
            unitAttackState = AttackStates.Advantage;
        }

        else if (i == 2)
        {
            unitAttackState = AttackStates.Disadvantage;
        }
    }

    //Action State Enum
    public enum ActionStates
    {
        Normal,
        Advantage,
        Disadvantage
    }

    //Get Action Sate
    public ActionStates GetActionState(int i)
    {
        if (i == 0)
        {
            return ActionStates.Normal;
        }

        else if (i == 1)
        {
            return ActionStates.Advantage;
        }

        else if (i == 2)
        {
            return ActionStates.Disadvantage;
        }

        return ActionStates.Normal;
    }

    //Set Action State
    public void SetActionState(int i)
    {
        if (i == 0)
        {
            unitActionState = ActionStates.Normal;
        }

        else if (i == 1)
        {
            unitActionState = ActionStates.Advantage;
        }

        else if (i == 2)
        {
            unitActionState = ActionStates.Disadvantage;
        }
    }

    //Action State Enum
    public enum SpellSaveStates
    {
        Normal,
        Advantage,
        Disadvantage
    }

    //Get Action Sate
    public SpellSaveStates GetSpellSaveState(int i)
    {
        if (i == 0)
        {
            return SpellSaveStates.Normal;
        }

        else if (i == 1)
        {
            return SpellSaveStates.Advantage;
        }

        else if (i == 2)
        {
            return SpellSaveStates.Disadvantage;
        }

        return SpellSaveStates.Normal;
    }

    //Set Action State
    public void SetSpellSaveState(int i)
    {
        if (i == 0)
        {
            unitSpellSaveState = SpellSaveStates.Normal;
        }

        else if (i == 1)
        {
            unitSpellSaveState = SpellSaveStates.Advantage;
        }

        else if (i == 2)
        {
            unitSpellSaveState = SpellSaveStates.Disadvantage;
        }
    }
    //Action State Enum
    public enum SpellCastStates
    {
        Normal,
        Advantage,
        Disadvantage
    }

    //Get Action Sate
    public SpellCastStates GetSpellCastState(int i)
    {
        if (i == 0)
        {
            return SpellCastStates.Normal;
        }

        else if (i == 1)
        {
            return SpellCastStates.Advantage;
        }

        else if (i == 2)
        {
            return SpellCastStates.Disadvantage;
        }

        return SpellCastStates.Normal;
    }

    //Set Action State
    public void SetSpellCastState(int i)
    {
        if (i == 0)
        {
            unitSpellCastState = SpellCastStates.Normal;
        }

        else if (i == 1)
        {
            unitSpellCastState = SpellCastStates.Advantage;
        }

        else if (i == 2)
        {
            unitSpellCastState = SpellCastStates.Disadvantage;
        }
    }

    public enum ConditionState
    {
        Normal,
        Blinded,
        Charmed,
        Deafened,
        Frightened,
        Grappled,
        Paralyzed,
        Petrified,
        Restrained,
        Stunned,
        Hidden,
        Unconscious
    }

    //Get Action Sate
    public ConditionState GetConditionState(int i)
    {
        if (i == 0)
        {
            return ConditionState.Normal;
        }

        else if (i == 1)
        {
            return ConditionState.Charmed;
        }

        else if (i == 2)
        {
            return ConditionState.Deafened;
        }

        else if (i == 3)
        {
            return ConditionState.Frightened;
        }

        else if (i == 4)
        {
            return ConditionState.Grappled;
        }

        else if (i == 5)
        {
            return ConditionState.Paralyzed;
        }

        else if (i == 6)
        {
            return ConditionState.Petrified;
        }

        else if (i == 7)
        {
            return ConditionState.Restrained;
        }

        else if (i == 8)
        {
            return ConditionState.Stunned;
        }

        else if (i == 9)
        {
            return ConditionState.Hidden;
        }

        else if (i == 10)
        {
            return ConditionState.Unconscious;
        }


        return ConditionState.Normal;
    }

    //Set Action State
    public void SetConditionState(int i)
    {
        if (i == 0)
        {
            unitConditionState = ConditionState.Normal;
        }

        else if (i == 1)
        {
            unitConditionState = ConditionState.Charmed;
        }

        else if (i == 2)
        {
            unitConditionState = ConditionState.Deafened;
        }

        else if (i == 3)
        {
            unitConditionState = ConditionState.Frightened;
        }

        else if (i == 4)
        {
            unitConditionState = ConditionState.Grappled;
        }

        else if (i == 5)
        {
            unitConditionState = ConditionState.Paralyzed;
        }

        else if (i == 6)
        {
            unitConditionState = ConditionState.Petrified;
        }

        else if (i == 7)
        {
            unitConditionState = ConditionState.Restrained;
        }

        else if (i == 8)
        {
            unitConditionState = ConditionState.Stunned;
        }

        else if (i == 9)
        {
            unitConditionState = ConditionState.Hidden;
        }

        else if (i == 9)
        {
            unitConditionState = ConditionState.Unconscious;
        }
    }
}
