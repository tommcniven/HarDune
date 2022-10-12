using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Scripts")]
    public GameMenuController scriptGameMenu;
    public TileMap scriptTileMap;

    [Header("Unit Canvas")]
    public TMP_Text currentHealthUI;
    public TMP_Text attackDamageUI;
    public TMP_Text attackRangeUI;
    public TMP_Text moveSpeedUI;
    public TMP_Text unitNameUI;
    public UnityEngine.UI.Image uniteSpriteUI;
    public Canvas unitCanvasUI;
    public GameObject activeUnitUI;
    public GameObject tileDisplayed;
    public bool displayingUnitInfo;

    [Header("Intro Canvas")]
    public GameObject turnChangeCanvas;
    public GameObject turnChangeBGPanel;
    public GameObject turnChangeLeftBarPanel;
    public GameObject turnChangeRightBarPanel;
    private Animator turnChangeLeftBarAnimator;
    private Animator turnChangeRightBarAnimator;
    private Animator turnChangeTextAnimator;
    private TMP_Text turnChangeText;

    [Header("Winner Canvas")]
    public Canvas winnerCanvasUI;
    public bool gameOver = false;

    [Header("Team Tracking")]
    public TMP_Text currentTeamUI;
    public int numberOfTeams = 2;
    public int currentTeam;
    public GameObject unitsOnBoard;
    public GameObject PCTeam;
    public GameObject NPCTeam;

    [Header("Location Tracking")]
    private Ray ray;
    private RaycastHit hit;

    //Cursor Info for TileMap
    public int cursorX;
    public int cursorY;

    //Curent Tile Mouseover
    public int selectedTileX;
    public int selectedTileY;

    //Potential Movement Route
    List<Node> currentRoutePath;
    List<Node> unitPathToCursor;

    public bool unitPathExists;

    public Material arrowBody;
    public Material arrowCurve;
    public Material arrowTip;
    public Material cursorUI;

    public int routeToX;
    public int routeToY;

    public GameObject quadrantOneAway;

   //Start
    public void Start()
    {
        currentTeam = 0;
        SetCurrentTeamPlayer();
        displayingUnitInfo = false;
        turnChangeLeftBarAnimator = turnChangeLeftBarPanel.GetComponent<Animator>();
        turnChangeRightBarAnimator = turnChangeRightBarPanel.GetComponent<Animator>();
        turnChangeText = turnChangeCanvas.GetComponentInChildren<TextMeshProUGUI>();
        turnChangeTextAnimator = turnChangeText.GetComponent<Animator>();
        unitPathToCursor = new List<Node>();
        unitPathExists = false;
        scriptTileMap = GetComponent<TileMap>();
    }

    //Update
    public void Update()
    {
        //Get Cursor Location
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            UpdateCursorUI();
            UpdateUnitUI();

            //Highlight Current Path if Unit Selected
            if (scriptTileMap.selectedUnit != null && scriptTileMap.selectedUnit.GetComponent<UnitController>().GetMovementState(1) == scriptTileMap.selectedUnit.GetComponent<UnitController>().unitMovementStates)
            {
                //Check if Cursor is Within Movement Range
                if (scriptTileMap.selectedUnitMovementRange.Contains(scriptTileMap.tileGraph[cursorX, cursorY]))
                {
                    //Generate New Path to Cursor
                    if (cursorX != scriptTileMap.selectedUnit.GetComponent<UnitController>().x || cursorY != scriptTileMap.selectedUnit.GetComponent<UnitController>().y)
                    {
                        //Check Movement Queue
                        if (!unitPathExists && scriptTileMap.selectedUnit.GetComponent<UnitController>().movementQueue.Count == 0)
                        {
                            unitPathToCursor = GenerateRouteToCursor(cursorX, cursorY);
                            routeToX = cursorX;
                            routeToY = cursorY;

                            //If Movement Available
                            if (unitPathToCursor.Count != 0)
                            {
                                //Set Node [x,y]
                                for(int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    //Set Cursor UI
                                    if (i == 0)
                                    {
                                        GameObject quadToUpdate = scriptTileMap.pathfindingTiles[nodeX, nodeY];
                                        quadToUpdate.GetComponent<Renderer>().material = cursorUI;
                                    }

                                    //Set Tile Indicator Excluding First & Last Tiles
                                    else if (i!=0 && (i+1)!=unitPathToCursor.Count)
                                    {
                                        SetRouteArrow(nodeX, nodeY,i);
                                    }

                                    //Set Tile Indiciator For Last Tile
                                    else if (i == unitPathToCursor.Count-1)
                                    {
                                        SetRouteArrowTip(nodeX, nodeY, i);
                                    }
                                    
                                    scriptTileMap.pathfindingTiles[nodeX, nodeY].GetComponent<Renderer>().enabled = true;
                                }
                            }

                            unitPathExists = true;
                        }

                        //Check if Route & Cursor Are Same
                        else if (routeToX != cursorX || routeToY != cursorY)
                        {
                            //Path is Not Cursor
                            if (unitPathToCursor.Count != 0)
                            {
                                //Set Node[x,y]
                                for (int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    scriptTileMap.pathfindingTiles[nodeX, nodeY].GetComponent<Renderer>().enabled = false;
                                }
                            }
                            
                            unitPathExists = false;
                        }
                    }

                    //Disable Route
                    else if(cursorX == scriptTileMap.selectedUnit.GetComponent<UnitController>().x && cursorY == scriptTileMap.selectedUnit.GetComponent<UnitController>().y)
                    {
                        scriptTileMap.DisableUnitRouteUI();
                        unitPathExists = false;
                    }
                }               
            }
        }
    }

    //UI Element
    public void SetCurrentTeamPlayer()
    {
        currentTeamUI.SetText("Players Turn");
    }

    public void SetCurrentTeamEnemy()
    {
        currentTeamUI.SetText("Bandits Turn");
    }

    //Reset Player Movements & Change Teams
    public void SwitchCurrentPlayer()
    {
        ResetTeamUnitMovement(GetTeamNumber(currentTeam));
        currentTeam++;

        if (currentTeam == numberOfTeams)
        {
            currentTeam = 0;
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
        if (scriptTileMap.selectedUnit == null)
        {
            SwitchCurrentPlayer();
            scriptGameMenu.CloseGameMenu();

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

    public void CheckIfUnitsRemain(GameObject unit, GameObject enemy)
    {
        StartCoroutine(CheckIfUnitsRemainEnum(unit,enemy));
    }

    //Used for Cursor Mouseover
    public void UpdateCursorUI()
    {
        //Highlight Mouseover Tiles
        if (hit.transform.CompareTag("Tile"))
        {
            //If Not Displayed
            if (tileDisplayed == null)
            {
                selectedTileX = hit.transform.gameObject.GetComponent<ClickableTile>().tileX;
                selectedTileY = hit.transform.gameObject.GetComponent<ClickableTile>().tileY;
                cursorX = selectedTileX;
                cursorY = selectedTileY;
                scriptTileMap.cursorTiles[selectedTileX, selectedTileY].GetComponent<MeshRenderer>().enabled = true;
                tileDisplayed = hit.transform.gameObject;
            }

            //If Mousover is Not a gameObject
            else if (tileDisplayed != hit.transform.gameObject)
            {
                selectedTileX = tileDisplayed.GetComponent<ClickableTile>().tileX;
                selectedTileY = tileDisplayed.GetComponent<ClickableTile>().tileY;
                scriptTileMap.cursorTiles[selectedTileX, selectedTileY].GetComponent<MeshRenderer>().enabled = false;
                selectedTileX = hit.transform.gameObject.GetComponent<ClickableTile>().tileX;
                selectedTileY = hit.transform.gameObject.GetComponent<ClickableTile>().tileY;
                cursorX = selectedTileX;
                cursorY = selectedTileY;
                scriptTileMap.cursorTiles[selectedTileX, selectedTileY].GetComponent<MeshRenderer>().enabled = true;
                tileDisplayed = hit.transform.gameObject;
            }
        }

        //Highlight Tile Under Units
        else if (hit.transform.CompareTag("Unit"))
        {
            //If Not Displayed
            if (tileDisplayed == null)
            {
                selectedTileX = hit.transform.parent.gameObject.GetComponent<UnitController>().x;
                selectedTileY = hit.transform.parent.gameObject.GetComponent<UnitController>().y;
                cursorX = selectedTileX;
                cursorY = selectedTileY;
                scriptTileMap.cursorTiles[selectedTileX, selectedTileY].GetComponent<MeshRenderer>().enabled = true;
                tileDisplayed = hit.transform.parent.gameObject.GetComponent<UnitController>().occupiedTile;
            }

            //If Mouseover is Not a gameObject
            else if (tileDisplayed != hit.transform.gameObject)
            {
                if (hit.transform.parent.gameObject.GetComponent<UnitController>().movementQueue.Count == 0)
                {
                    selectedTileX = tileDisplayed.GetComponent<ClickableTile>().tileX;
                    selectedTileY = tileDisplayed.GetComponent<ClickableTile>().tileY;
                    scriptTileMap.cursorTiles[selectedTileX, selectedTileY].GetComponent<MeshRenderer>().enabled = false;
                    selectedTileX = hit.transform.parent.gameObject.GetComponent<UnitController>().x;
                    selectedTileY = hit.transform.parent.gameObject.GetComponent<UnitController>().y;
                    cursorX = selectedTileX;
                    cursorY = selectedTileY;
                    scriptTileMap.cursorTiles[selectedTileX, selectedTileY].GetComponent<MeshRenderer>().enabled = true;
                    tileDisplayed = hit.transform.parent.GetComponent<UnitController>().occupiedTile;
                }
            }
        }

        //No Tile Under Cursor
        else
        {
            scriptTileMap.cursorTiles[selectedTileX, selectedTileY].GetComponent<MeshRenderer>().enabled = false;
        }

        //Close Game Menu
        if (GameMenuController.menuOpen)
        {
            scriptTileMap.cursorTiles[selectedTileX, selectedTileY].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    //UI Element
    public void UpdateUnitUI()
    {
        //No Unit Info Displayed
        if (!displayingUnitInfo)
        {
            //Mouseover Unit
            if (hit.transform.CompareTag("Unit"))
            {
                unitCanvasUI.enabled = true;
                displayingUnitInfo = true;
                activeUnitUI = hit.transform.parent.gameObject;
                var unitController = hit.transform.parent.gameObject.GetComponent<UnitController>();
                var unitStats = hit.transform.parent.gameObject.GetComponent<UnitStats>();
                currentHealthUI.SetText(unitStats.maxHP.ToString());
                moveSpeedUI.SetText(unitStats.movementSpeed.ToString());
                unitNameUI.SetText(unitStats.unitName);
                uniteSpriteUI.sprite = unitController.unitSprite;
            }

            //Mouseover Tile
            else if (hit.transform.CompareTag("Tile"))
            {
                //No Unit on Tile
                if (hit.transform.GetComponent<ClickableTile>().unitOnTile != null)
                {
                    activeUnitUI = hit.transform.GetComponent<ClickableTile>().unitOnTile;
                    unitCanvasUI.enabled = true;
                    displayingUnitInfo = true;
                    var unitController = activeUnitUI.GetComponent<UnitController>();
                    var unitStats = activeUnitUI.GetComponent<UnitStats>();
                    currentHealthUI.SetText(unitStats.maxHP.ToString());
                    moveSpeedUI.SetText(unitStats.movementSpeed.ToString());
                    unitNameUI.SetText(unitStats.unitName);
                    uniteSpriteUI.sprite = unitController.unitSprite;
                }
            }
        }

        //Mouseover Tile
        else if (hit.transform.gameObject.CompareTag("Tile"))
        {
            //No Unit on Tile
            if (hit.transform.GetComponent<ClickableTile>().unitOnTile == null)
            {
                unitCanvasUI.enabled = false;
                displayingUnitInfo = false;
            }
            //Unit on Tile != Unit Displayed
            else if (hit.transform.GetComponent<ClickableTile>().unitOnTile != activeUnitUI)
            {
                unitCanvasUI.enabled = false;
                displayingUnitInfo = false;
            }
        }

        //Mouseover Unit
        else if (hit.transform.gameObject.CompareTag("Unit"))
        {
            //Unit != Unit Displayed
            if (hit.transform.parent.gameObject != activeUnitUI)
            {
                unitCanvasUI.enabled = false;
                displayingUnitInfo = false;
            }
        }
    }

    public List<Node> GenerateRouteToCursor(int x, int y)
    {
        //Selected Unit is Standing on Tile Selected
        if (scriptTileMap.selectedUnit.GetComponent<UnitController>().x == x && scriptTileMap.selectedUnit.GetComponent<UnitController>().y == y)
        {
            currentRoutePath = new List<Node>();
            return currentRoutePath;
        }

        //Selected Tile != Accessible
        if (scriptTileMap.isTileEnterable(x, y) == false)
        {
            return null;
        }

        currentRoutePath = null;
        Dictionary<Node, float> distance = new Dictionary<Node, float>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();
        Node source = scriptTileMap.tileGraph[scriptTileMap.selectedUnit.GetComponent<UnitController>().x, scriptTileMap.selectedUnit.GetComponent<UnitController>().y];
        Node target = scriptTileMap.tileGraph[x, y];
        distance[source] = 0;
        previous[source] = null;
        List<Node> unvisited = new List<Node>();

        //Initialize Unvisited Nodes
        foreach (Node n in scriptTileMap.tileGraph)
        {
            //Nodes != Unit Current[x,y]
            if (n != source)
            {
                distance[n] = Mathf.Infinity;
                previous[n] = null;
            }
            unvisited.Add(n);
        }

        //Check Unvisited Nodes
        while (unvisited.Count > 0)
        {
            Node unvistedNodes = null;
            foreach (Node accessibleUnvisitedNodes in unvisited)
            {
                if (unvistedNodes == null || distance[accessibleUnvisitedNodes] < distance[unvistedNodes])
                {
                    unvistedNodes = accessibleUnvisitedNodes;
                }
            }

            //Unvisited Node != in Play
            if (unvistedNodes == target)
            {
                break;
            }

            unvisited.Remove(unvistedNodes);

            //Check Neighboring Nodes
            foreach (Node n in unvistedNodes.neighbors)
            {

                //float alt = dist[u] + u.DistanceTo(n);
                float alt = distance[unvistedNodes] + scriptTileMap.CostToEnterTile(n.x, n.y);
                if (alt < distance[n])
                {
                    distance[n] = alt;
                    previous[n] = unvistedNodes;
                }
            }
        }

        //No Path Accessible
        if (previous[target] == null)
        {
            return null;
        }

        currentRoutePath = new List<Node>();
        Node tempCurrent = target;

        //Move to tempCurrent Node Then Update Previous to Old Location
        while (tempCurrent != null)
        {
            currentRoutePath.Add(tempCurrent);
            tempCurrent = previous[tempCurrent];
        }

        //Need to Reverse to Generate Path from Current to Target
        currentRoutePath.Reverse();
        return currentRoutePath;
    }

    //Get Direction Between Two Vectors
    public Vector2 GetDirectionBetween(Vector2 currentPosition, Vector2 nextPosition)
    {
        Vector2 direction = (nextPosition - currentPosition).normalized;
        
        //Right
        if (direction == Vector2.right)
        {
            return Vector2.right;
        }

        //Left
        else if (direction == Vector2.left)
        {
            return Vector2.left;
        }

        //Up
        else if (direction == Vector2.up)
        {
            return Vector2.up;
        }

        //Down
        else if (direction == Vector2.down)
        {
            return Vector2.down;
        }

        //No Direction
        else
        {
            Vector2 directionsBetween = new Vector2();
            return directionsBetween;
        }
    }

    //Set Route Arrow Direction & Type on Respective Tiles (Straight & Curve)
    //[Update] Copy for Unit Flip on Left & Right in Unit Controller Script
    public void SetRouteArrow(int nodeX, int nodeY, int i)
    {
        Vector2 previousTile = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 nextTile = new Vector2(unitPathToCursor[i + 1].x + 1, unitPathToCursor[i + 1].y + 1);
        Vector2 previousTileToCurrentTile = GetDirectionBetween(previousTile, currentTile);
        Vector2 currentTileToNextTile = GetDirectionBetween(currentTile, nextTile);

        //Right
        if (previousTileToCurrentTile == Vector2.right && currentTileToNextTile == Vector2.right)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            updateQuadrant.GetComponent<Renderer>().material = arrowBody;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Right & Up
        else if (previousTileToCurrentTile == Vector2.right && currentTileToNextTile == Vector2.up)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Right & Down
        else if (previousTileToCurrentTile == Vector2.right && currentTileToNextTile == Vector2.down)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Left
        else if (previousTileToCurrentTile == Vector2.left && currentTileToNextTile == Vector2.left)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            updateQuadrant.GetComponent<Renderer>().material = arrowBody;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Left & Up
        else if (previousTileToCurrentTile == Vector2.left && currentTileToNextTile == Vector2.up)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Left & Down
        else if (previousTileToCurrentTile == Vector2.left && currentTileToNextTile == Vector2.down)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Up
        else if (previousTileToCurrentTile == Vector2.up && currentTileToNextTile == Vector2.up)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            updateQuadrant.GetComponent<Renderer>().material = arrowBody;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Up & Right
        else if (previousTileToCurrentTile == Vector2.up && currentTileToNextTile == Vector2.right)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Up & Left
        else if (previousTileToCurrentTile == Vector2.up && currentTileToNextTile == Vector2.left)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Down
        else if (previousTileToCurrentTile == Vector2.down && currentTileToNextTile == Vector2.down)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            updateQuadrant.GetComponent<Renderer>().material = arrowBody;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Down & Right
        else if (previousTileToCurrentTile == Vector2.down && currentTileToNextTile == Vector2.right)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;

        }

        //Down & Left
        else if (previousTileToCurrentTile == Vector2.down && currentTileToNextTile == Vector2.left)
        {
            GameObject updateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }
    }

    //Set Arrow Tip on Final Tile
    //[Update] Copy for Unit Flip on Left & Right in Unit Controller Script
    public void SetRouteArrowTip(int nodeX, int nodeY, int i)
    {
        Vector2 previousTile = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 previousTiletoCurrentTile = GetDirectionBetween(previousTile, currentTile);

        //Right
        if (previousTiletoCurrentTile == Vector2.right)
        {
            GameObject UpdateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            UpdateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            UpdateQuadrant.GetComponent<Renderer>().material = arrowTip;
            UpdateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Left
        else if (previousTiletoCurrentTile == Vector2.left)
        {
            GameObject UpdateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            UpdateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            UpdateQuadrant.GetComponent<Renderer>().material = arrowTip;
            UpdateQuadrant.GetComponent<Renderer>().enabled = true;

        }

        //Up
        else if (previousTiletoCurrentTile == Vector2.up)
        {
            GameObject UpdateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            UpdateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            UpdateQuadrant.GetComponent<Renderer>().material = arrowTip;
            UpdateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Down
        else if (previousTiletoCurrentTile == Vector2.down)
        {
            GameObject UpdateQuadrant = scriptTileMap.pathfindingTiles[nodeX, nodeY];
            UpdateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            UpdateQuadrant.GetComponent<Renderer>().material = arrowTip;
            UpdateQuadrant.GetComponent<Renderer>().enabled = true;
        }
    }

    public IEnumerator CheckIfUnitsRemainEnum(GameObject unit, GameObject enemy)
    {
        //Player Combat in Queue
        while (unit.GetComponent<UnitController>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        //Enemy Combat in Queue
        while (enemy.GetComponent<UnitController>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }

        //All PC Units are Dead
        if (PCTeam.transform.childCount == 0)
        {
            winnerCanvasUI.enabled = true;
            gameOver = true;
            winnerCanvasUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Defeat");
        }

        //All NPC Units are Dead
        else if (NPCTeam.transform.childCount == 0)
        {
            winnerCanvasUI.enabled = true;
            gameOver = true;
            winnerCanvasUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Victory");
        }
    }
}
