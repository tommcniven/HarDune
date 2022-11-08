using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnController : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Turn Change Canvas")]
    public GameObject turnChangeCanvas;
    public GameObject turnChangeBGPanel;
    public GameObject turnChangeLeftBarPanel;
    public GameObject turnChangeRightBarPanel;
    private Animator turnChangeLeftBarAnimator;
    private Animator turnChangeRightBarAnimator;
    private Animator turnChangeTextAnimator;
    private TMP_Text turnChangeText;

    [Header("Team Tracking")]
    public GameObject unitsOnBoard;
    public GameObject PCTeam;
    public GameObject NPCTeam;
    public TMP_Text currentTeamUI;
    public int numberOfTeams = 2;
    public int currentTeam = 0;
    public static event Action onTurnChange;

    public void Awake()
    {
        SetScriptManager();
        SetTurnChangeVariables();
        SetCurrentTeamPlayer();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void SetTurnChangeVariables()
    {
        turnChangeLeftBarAnimator = turnChangeLeftBarPanel.GetComponent<Animator>();
        turnChangeRightBarAnimator = turnChangeRightBarPanel.GetComponent<Animator>();
        turnChangeText = turnChangeCanvas.GetComponentInChildren<TextMeshProUGUI>();
        turnChangeTextAnimator = turnChangeText.GetComponent<Animator>();
    }

    public void SetCurrentTeamPlayer()
    {
        currentTeamUI.SetText("Players Turn");
    }

    public void SetCurrentTeamEnemy()
    {
        currentTeamUI.SetText("Bandits Turn");
    }

    public void SwitchCurrentPlayer()
    {
        ResetTeamUnitMovement(GetTeamNumber(currentTeam));
        currentTeam++;

        if (currentTeam == numberOfTeams)
        {
            currentTeam = 0;
        }

        //Dodge Timer
        if (scriptManager.scriptBattleController.isDodging)
        {
            onTurnChange?.Invoke();
        }
    }

    public GameObject GetTeamNumber(int teamNumber)
    {
        GameObject currentTeam = null;

        if (teamNumber == 0)
        {
            currentTeam = PCTeam;
        }

        else if (teamNumber == 1)
        {
            currentTeam = NPCTeam;
        }

        return currentTeam;
    }

    public void ResetTeamUnitMovement(GameObject teamToReset)
    {
        foreach (Transform unit in teamToReset.transform)
        {
            unit.GetComponent<UnitController>().ResetSingleUnitMovement();
        }
    }

    public void EndTurn()
    {
        if (scriptManager.scriptTileMap.selectedUnit == null)
        {
            SwitchCurrentPlayer();
            scriptManager.scriptGameMenuController.CloseAllMenus();

            if (currentTeam == 1)
            {
                turnChangeLeftBarAnimator.SetTrigger("Left-Bar-Animation");
                turnChangeRightBarAnimator.SetTrigger("Right-Bar-Animation");
                turnChangeTextAnimator.SetTrigger("Display-Text");
                turnChangeText.SetText("Bandits Turn");
                SetCurrentTeamEnemy();
            }

            else if (currentTeam == 0)
            {
                turnChangeLeftBarAnimator.SetTrigger("Left-Bar-Animation");
                turnChangeRightBarAnimator.SetTrigger("Right-Bar-Animation");
                turnChangeTextAnimator.SetTrigger("Display-Text");
                turnChangeText.SetText("Players Turn");
                SetCurrentTeamPlayer();
            }
        }
    }
}
