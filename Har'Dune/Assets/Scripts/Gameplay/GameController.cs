using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Unit Canvas")] //Move to New Script (UnitUIDisplay)
    public TMP_Text currentHealthUI;
    public TMP_Text attackDamageUI;
    public TMP_Text attackRangeUI;
    public TMP_Text moveSpeedUI;
    public TMP_Text unitNameUI;
    public UnityEngine.UI.Image uniteSpriteUI;
    public Canvas unitCanvasUI;
    public GameObject activeUnitUI;
    public GameObject tileDisplayed;
    public bool isUnitDisplayed = false;

    [Header("Winner Canvas")] //Move to New Script (WinLossUIDisplay)
    public Canvas winnerCanvasUI;
    public bool isGameOver = false;

    [Header("Location Tracking")]
    private Ray ray;
    private RaycastHit hit;

    [Header("Movement Path")]
    //Potential Movement Path
    List<Node> currentRoutePath;
    List<Node> unitPathToCursor = new List<Node>();
    public bool doesUnitPathExist = false;

    [Header("Arrows")]
    public Material arrowBody;
    public Material arrowCurve;
    public Material arrowTip;
    public Material cursorUI;
    public int pathToX;
    public int pathToY;

    public void Awake()
    {
        SetScriptManager();
    }

    public void Update()
    {
        //Get Cursor Location
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            UpdateCursorUI();
            UpdateUnitUI();

            //Highlight Current Path if Unit Selected
            if (scriptManager.scriptTileMap.selectedUnit != null && scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().GetMovementState(1) == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().unitMovementStates)
            {
                //Check if Cursor is Within Movement Range
                if (scriptManager.scriptTileMap.selectedUnitMovementRange.Contains(scriptManager.scriptTileMap.tileGraph[scriptManager.scriptCursorController.cursorX, scriptManager.scriptCursorController.cursorY]))
                {
                    //Generate New Path to Cursor
                    if (scriptManager.scriptCursorController.cursorX != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x || scriptManager.scriptCursorController.cursorY != scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y)
                    {
                        //Check Movement Queue
                        if (!doesUnitPathExist && scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().movementQueue.Count == 0)
                        {
                            unitPathToCursor = GenerateRouteToCursor(scriptManager.scriptCursorController.cursorX, scriptManager.scriptCursorController.cursorY);
                            pathToX = scriptManager.scriptCursorController.cursorX;
                            pathToY = scriptManager.scriptCursorController.cursorY;

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
                                        GameObject quadToUpdate = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
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

                                    scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY].GetComponent<Renderer>().enabled = true;
                                }
                            }

                            doesUnitPathExist = true;
                        }

                        //Check if Route & Cursor Are Same
                        else if (pathToX != scriptManager.scriptCursorController.cursorX || pathToY != scriptManager.scriptCursorController.cursorY)
                        {
                            //Path is Not Cursor
                            if (unitPathToCursor.Count != 0)
                            {
                                //Set Node[x,y]
                                for (int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY].GetComponent<Renderer>().enabled = false;
                                }
                            }
                            
                            doesUnitPathExist = false;
                        }
                    }

                    //Disable Route
                    else if(scriptManager.scriptCursorController.cursorX == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x && scriptManager.scriptCursorController.cursorY == scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y)
                    {
                        scriptManager.scriptTileMap.DisableUnitRouteUI();
                        doesUnitPathExist = false;
                    }
                }               
            }
        }
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
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
                scriptManager.scriptCursorController.tileUnderCursorX = hit.transform.gameObject.GetComponent<ClickableTile>().tileX;
                scriptManager.scriptCursorController.tileUnderCursorY = hit.transform.gameObject.GetComponent<ClickableTile>().tileY;
                scriptManager.scriptCursorController.cursorX = scriptManager.scriptCursorController.tileUnderCursorX;
                scriptManager.scriptCursorController.cursorY = scriptManager.scriptCursorController.tileUnderCursorY;
                scriptManager.scriptTileMap.cursorTiles[scriptManager.scriptCursorController.tileUnderCursorX, scriptManager.scriptCursorController.tileUnderCursorY].GetComponent<MeshRenderer>().enabled = true;
                tileDisplayed = hit.transform.gameObject;
            }

            //If Mousover is Not a gameObject
            else if (tileDisplayed != hit.transform.gameObject)
            {
                scriptManager.scriptCursorController.tileUnderCursorX = tileDisplayed.GetComponent<ClickableTile>().tileX;
                scriptManager.scriptCursorController.tileUnderCursorY = tileDisplayed.GetComponent<ClickableTile>().tileY;
                scriptManager.scriptTileMap.cursorTiles[scriptManager.scriptCursorController.tileUnderCursorX, scriptManager.scriptCursorController.tileUnderCursorY].GetComponent<MeshRenderer>().enabled = false;
                scriptManager.scriptCursorController.tileUnderCursorX = hit.transform.gameObject.GetComponent<ClickableTile>().tileX;
                scriptManager.scriptCursorController.tileUnderCursorY = hit.transform.gameObject.GetComponent<ClickableTile>().tileY;
                scriptManager.scriptCursorController.cursorX = scriptManager.scriptCursorController.tileUnderCursorX;
                scriptManager.scriptCursorController.cursorY = scriptManager.scriptCursorController.tileUnderCursorY;
                scriptManager.scriptTileMap.cursorTiles[scriptManager.scriptCursorController.tileUnderCursorX, scriptManager.scriptCursorController.tileUnderCursorY].GetComponent<MeshRenderer>().enabled = true;
                tileDisplayed = hit.transform.gameObject;
            }
        }

        //Highlight Tile Under Units
        else if (hit.transform.CompareTag("Unit"))
        {
            //If Not Displayed
            if (tileDisplayed == null)
            {
                scriptManager.scriptCursorController.tileUnderCursorX = hit.transform.parent.gameObject.GetComponent<UnitController>().x;
                scriptManager.scriptCursorController.tileUnderCursorY = hit.transform.parent.gameObject.GetComponent<UnitController>().y;
                scriptManager.scriptCursorController.cursorX = scriptManager.scriptCursorController.tileUnderCursorX;
                scriptManager.scriptCursorController.cursorY = scriptManager.scriptCursorController.tileUnderCursorY;
                scriptManager.scriptTileMap.cursorTiles[scriptManager.scriptCursorController.tileUnderCursorX, scriptManager.scriptCursorController.tileUnderCursorY].GetComponent<MeshRenderer>().enabled = true;
                tileDisplayed = hit.transform.parent.gameObject.GetComponent<UnitController>().occupiedTile;
            }

            //If Mouseover is Not a gameObject
            else if (tileDisplayed != hit.transform.gameObject)
            {
                if (hit.transform.parent.gameObject.GetComponent<UnitController>().movementQueue.Count == 0)
                {
                    scriptManager.scriptCursorController.tileUnderCursorX = tileDisplayed.GetComponent<ClickableTile>().tileX;
                    scriptManager.scriptCursorController.tileUnderCursorY = tileDisplayed.GetComponent<ClickableTile>().tileY;
                    scriptManager.scriptTileMap.cursorTiles[scriptManager.scriptCursorController.tileUnderCursorX, scriptManager.scriptCursorController.tileUnderCursorY].GetComponent<MeshRenderer>().enabled = false;
                    scriptManager.scriptCursorController.tileUnderCursorX = hit.transform.parent.gameObject.GetComponent<UnitController>().x;
                    scriptManager.scriptCursorController.tileUnderCursorY = hit.transform.parent.gameObject.GetComponent<UnitController>().y;
                    scriptManager.scriptCursorController.cursorX = scriptManager.scriptCursorController.tileUnderCursorX;
                    scriptManager.scriptCursorController.cursorY = scriptManager.scriptCursorController.tileUnderCursorY;
                    scriptManager.scriptTileMap.cursorTiles[scriptManager.scriptCursorController.tileUnderCursorX, scriptManager.scriptCursorController.tileUnderCursorY].GetComponent<MeshRenderer>().enabled = true;
                    tileDisplayed = hit.transform.parent.GetComponent<UnitController>().occupiedTile;
                }
            }
        }

        //No Tile Under Cursor
        else
        {
            scriptManager.scriptTileMap.cursorTiles[scriptManager.scriptCursorController.tileUnderCursorX, scriptManager.scriptCursorController.tileUnderCursorY].GetComponent<MeshRenderer>().enabled = false;
        }

        //Close Game Menu
        if (GameMenuController.menuOpen)
        {
            scriptManager.scriptTileMap.cursorTiles[scriptManager.scriptCursorController.tileUnderCursorX, scriptManager.scriptCursorController.tileUnderCursorY].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void UpdateUnitUI()
    {
        //No Unit Info Displayed
        if (!isUnitDisplayed)
        {
            //Mouseover Unit
            if (hit.transform.CompareTag("Unit"))
            {
                unitCanvasUI.enabled = true;
                isUnitDisplayed = true;
                activeUnitUI = hit.transform.parent.gameObject;
                UnitController unitController = hit.transform.parent.gameObject.GetComponent<UnitController>();
                UnitStats unitStats = hit.transform.parent.gameObject.GetComponent<UnitStats>();
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
                    isUnitDisplayed = true;
                    UnitController unitController = activeUnitUI.GetComponent<UnitController>();
                    UnitStats unitStats = activeUnitUI.GetComponent<UnitStats>();
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
                isUnitDisplayed = false;
            }
            //Unit on Tile != Unit Displayed
            else if (hit.transform.GetComponent<ClickableTile>().unitOnTile != activeUnitUI)
            {
                unitCanvasUI.enabled = false;
                isUnitDisplayed = false;
            }
        }

        //Mouseover Unit
        else if (hit.transform.gameObject.CompareTag("Unit"))
        {
            //Unit != Unit Displayed
            if (hit.transform.parent.gameObject != activeUnitUI)
            {
                unitCanvasUI.enabled = false;
                isUnitDisplayed = false;
            }
        }
    }

    public List<Node> GenerateRouteToCursor(int x, int y)
    {
        //Selected Unit is Standing on Tile Selected
        if (scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x == x && scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y == y)
        {
            currentRoutePath = new List<Node>();
            return currentRoutePath;
        }

        //Selected Tile != Accessible
        if (scriptManager.scriptTileMap.isNodeEnterable(x, y) == false)
        {
            return null;
        }

        currentRoutePath = null;
        Dictionary<Node, float> distance = new Dictionary<Node, float>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();
        Node source = scriptManager.scriptTileMap.tileGraph[scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x, scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y];
        Node target = scriptManager.scriptTileMap.tileGraph[x, y];
        distance[source] = 0;
        previous[source] = null;
        List<Node> unvisited = new List<Node>();

        //Initialize Unvisited Nodes
        foreach (Node n in scriptManager.scriptTileMap.tileGraph)
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
                float alt = distance[unvistedNodes] + scriptManager.scriptTileMap.CostToEnterTile(n.x, n.y);
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

    public Vector2 GetDirectionBetweenNodes(Vector2 currentNode, Vector2 nextNode)
    {
        Vector2 direction = (nextNode - currentNode).normalized;
        
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

    public void SetRouteArrow(int nodeX, int nodeY, int i)
    {
        Vector2 previousNode = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentNode = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 nextNode = new Vector2(unitPathToCursor[i + 1].x + 1, unitPathToCursor[i + 1].y + 1);
        Vector2 previousNodeToCurrentNode = GetDirectionBetweenNodes(previousNode, currentNode);
        Vector2 currentNodeToNextNode = GetDirectionBetweenNodes(currentNode, nextNode);

        //Right
        if (previousNodeToCurrentNode == Vector2.right && currentNodeToNextNode == Vector2.right)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            updateQuadrant.GetComponent<Renderer>().material = arrowBody;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Right & Up
        else if (previousNodeToCurrentNode == Vector2.right && currentNodeToNextNode == Vector2.up)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Right & Down
        else if (previousNodeToCurrentNode == Vector2.right && currentNodeToNextNode == Vector2.down)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Left
        else if (previousNodeToCurrentNode == Vector2.left && currentNodeToNextNode == Vector2.left)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            updateQuadrant.GetComponent<Renderer>().material = arrowBody;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Left & Up
        else if (previousNodeToCurrentNode == Vector2.left && currentNodeToNextNode == Vector2.up)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Left & Down
        else if (previousNodeToCurrentNode == Vector2.left && currentNodeToNextNode == Vector2.down)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Up
        else if (previousNodeToCurrentNode == Vector2.up && currentNodeToNextNode == Vector2.up)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            updateQuadrant.GetComponent<Renderer>().material = arrowBody;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Up & Right
        else if (previousNodeToCurrentNode == Vector2.up && currentNodeToNextNode == Vector2.right)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Up & Left
        else if (previousNodeToCurrentNode == Vector2.up && currentNodeToNextNode == Vector2.left)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Down
        else if (previousNodeToCurrentNode == Vector2.down && currentNodeToNextNode == Vector2.down)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            updateQuadrant.GetComponent<Renderer>().material = arrowBody;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Down & Right
        else if (previousNodeToCurrentNode == Vector2.down && currentNodeToNextNode == Vector2.right)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;

        }

        //Down & Left
        else if (previousNodeToCurrentNode == Vector2.down && currentNodeToNextNode == Vector2.left)
        {
            GameObject updateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            updateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            updateQuadrant.GetComponent<Renderer>().material = arrowCurve;
            updateQuadrant.GetComponent<Renderer>().enabled = true;
        }
    }

    public void SetRouteArrowTip(int nodeX, int nodeY, int i)
    {
        Vector2 previousNode = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentNode = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 previousNodetoCurrentNode = GetDirectionBetweenNodes(previousNode, currentNode);

        //Right
        if (previousNodetoCurrentNode == Vector2.right)
        {
            GameObject UpdateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            UpdateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            UpdateQuadrant.GetComponent<Renderer>().material = arrowTip;
            UpdateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Left
        else if (previousNodetoCurrentNode == Vector2.left)
        {
            GameObject UpdateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            UpdateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            UpdateQuadrant.GetComponent<Renderer>().material = arrowTip;
            UpdateQuadrant.GetComponent<Renderer>().enabled = true;

        }

        //Up
        else if (previousNodetoCurrentNode == Vector2.up)
        {
            GameObject UpdateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
            UpdateQuadrant.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            UpdateQuadrant.GetComponent<Renderer>().material = arrowTip;
            UpdateQuadrant.GetComponent<Renderer>().enabled = true;
        }

        //Down
        else if (previousNodetoCurrentNode == Vector2.down)
        {
            GameObject UpdateQuadrant = scriptManager.scriptTileMap.pathfindingTiles[nodeX, nodeY];
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
        if (scriptManager.scriptTurnController.PCTeam.transform.childCount == 0)
        {
            winnerCanvasUI.enabled = true;
            isGameOver = true;
            winnerCanvasUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Defeat");
        }

        //All NPC Units are Dead
        else if (scriptManager.scriptTurnController.NPCTeam.transform.childCount == 0)
        {
            winnerCanvasUI.enabled = true;
            isGameOver = true;
            winnerCanvasUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Victory");
        }
    }
}
