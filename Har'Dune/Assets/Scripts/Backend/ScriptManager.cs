using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptManager : MonoBehaviour
{
    [Header("Scripts")]
    public BattleController scriptBattleController;
    public GameController scriptGameController;
    public TileMap scriptTileMap;
    public GameMenuController scriptGameMenuController;
    public CameraShake scriptCameraShake;
    public RangeFinder scriptRangeFinder;
    public MovementController scriptMovementController;
    public UnitSelection scriptUnitSelection;
    public CursorController scriptCursorController;
    public TurnController scriptTurnController;
    public UnitUIDisplay scriptUnitUIDisplay;
    public PathFinder scriptPathFinder;
    public WeaponAttack scriptWeaponAttack;

    public void ConnectScripts()
    {
        scriptBattleController = GameObject.Find("Game Controller").GetComponent<BattleController>();
        scriptGameController = GameObject.Find("Game Controller").GetComponent<GameController>();
        scriptTileMap = GameObject.Find("Game Controller").GetComponent<TileMap>();
        scriptGameMenuController = GameObject.Find("Game Menu Controller").GetComponent<GameMenuController>();
        scriptCameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        scriptRangeFinder = GameObject.Find("Range Finder").GetComponent<RangeFinder>();
        scriptMovementController = GameObject.Find("Movement Controller").GetComponent<MovementController>();
        scriptUnitSelection = GameObject.Find("Unit Selection").GetComponent<UnitSelection>();
        scriptCursorController = GameObject.Find("Cursor Controller").GetComponent<CursorController>();
        scriptTurnController = GameObject.Find("Turn Controller").GetComponent<TurnController>();
        scriptUnitUIDisplay = GameObject.Find("Unit UI Display").GetComponent<UnitUIDisplay>();
        scriptPathFinder = GameObject.Find("Path Finder").GetComponent<PathFinder>();
        scriptWeaponAttack = GameObject.Find("Weapon Attacks").GetComponent<WeaponAttack>();
    }
}
