using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptManager : MonoBehaviour
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
    public MovementController scriptMovementController;
    public UnitSelection scriptUnitSelection;

    public void ConnectScripts()
    {
        //scriptUnitStats = GameObject.Find("Range Finder").GetComponent<RangeFinder>();
        //scriptUnitController = GameObject.Find("Range Finder").GetComponent<RangeFinder>();
        scriptBattleController = GameObject.Find("Game Controller").GetComponent<BattleController>();
        scriptGameController = GameObject.Find("Game Controller").GetComponent<GameController>();
        scriptTileMap = GameObject.Find("Game Controller").GetComponent<TileMap>();
        scriptGameMenuController = GameObject.Find("Menu Controller").GetComponent<GameMenuController>();
        scriptCameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        scriptRangeFinder = GameObject.Find("Range Finder").GetComponent<RangeFinder>();
        scriptMovementController = GameObject.Find("Movement Controller").GetComponent<MovementController>();
        scriptUnitSelection = GameObject.Find("Unit Selection").GetComponent<UnitSelection>();
    }
}
