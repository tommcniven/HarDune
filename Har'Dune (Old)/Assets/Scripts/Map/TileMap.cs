using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour    
{
    [Header("Scripts")]
    public BattleController scriptBattleController;
    public GameController scriptGameController;
    public GameMenuController scriptGameMenuController;
    public AudioManager scriptAudioManager;
    public MovementController scriptMovementController;
    public RangeFinder scriptRangeFinder;

    [Header("Menus")]
    public GameObject actionMenuPanel;

    [Header("Tiles")]
    public Tiles[] tileTypes;
    public int[,] tiles;

    //Tile List
    // private int dirtTile = 0;
    private int forestTile = 1;
    private int mountainTile = 2;
    private int grassTile = 3;
    private int roadTile = 4;
    // private int cityTile = 5;
    private int bridgeTile = 6;
    private int waterTile = 7;

    [Header("Active Units")]
    public GameObject activeUnits;
    public GameObject[,] tilesOnMap;
    public GameObject mapUI;
    public GameObject mapCursorUI;
    public GameObject mapUnitMovementUI;
    public List<Node> currentPath = null;
    public Node[,] tileGraph;

    //Unit Tiles
    public GameObject[,] mapTiles;
    public GameObject[,] pathfindingTiles;
    public GameObject[,] cursorTiles;
    
    //Tile Containers
    [Header("Containers")]
    public GameObject tileTypesContainer;
    public GameObject mapContainer;
    public GameObject cursorContainer;
    public GameObject pathfindingContainer;

    [Header("Board Size")]
    public int mapSizeX = 19;
    public int mapSizeY = 12;

    [Header("Selected Unit Info")]
    public GameObject selectedUnit;
    public HashSet<Node> selectedUnitTotalRange;
    public HashSet<Node> selectedUnitMovementRange;
    public bool isUnitSelected = false;
    public int selectedUnitPreviousX;
    public int selectedUnitPreviousY;
    public GameObject previousOccupiedTile;

    //[Header("Audio")]
    //public AudioSource selectedSound;
    //public AudioSource unselectedSound;

    [Header("Materials")]
    public Material redBorderTileHighlight;
    public Material blueBorderTileHighlight;
    public Material lightTileHighlight;
    public Material lightBorderTileHighlight;

    private void Start()
    {
        GenerateMapInfo();
        GeneratePathfindingGraph();
        GenerateMapVisuals();
        SetNodeisOccupied();
    }

    private void Update()
    {
        //Left Click
        if (Input.GetMouseButtonDown(0))
        {
            //No Unit
            if (selectedUnit == null)
            {
                ClickToSelectUnit();
            }

            //Move Unit
            else if (selectedUnit.GetComponent<UnitController>().unitMovementStates == selectedUnit.GetComponent<UnitController>().GetMovementState(1) && selectedUnit.GetComponent<UnitController>().movementQueue.Count == 0)
            {
                if (scriptMovementController.isSelectedNodeAccessible())
                {
                    //scriptAudioManager.PlayFootstepAudio();
                    selectedUnitPreviousX = selectedUnit.GetComponent<UnitController>().x;
                    selectedUnitPreviousY = selectedUnit.GetComponent<UnitController>().y;
                    previousOccupiedTile = selectedUnit.GetComponent<UnitController>().occupiedTile;
                    selectedUnit.GetComponent<UnitController>().SetRunAnimation();
                    scriptMovementController.MoveUnit();
                    StartCoroutine(scriptMovementController.MoveUnitEnum());                   
                }
            }
        }

        //Unselect Unit & Set Game State to Idle on Right Click
        if (Input.GetMouseButtonDown(1))
        {
            scriptBattleController.battleStatus = false;

            if (selectedUnit != null)
            {
                if (selectedUnit.GetComponent<UnitController>().movementQueue.Count == 0 && selectedUnit.GetComponent<UnitController>().combatQueue.Count==0)
                {
                    if (selectedUnit.GetComponent<UnitController>().unitMovementStates != selectedUnit.GetComponent<UnitController>().GetMovementState(3))
                    {
                        //unselectedSound.Play();
                        selectedUnit.GetComponent<UnitController>().SetIdleAnimation();
                        DeselectUnit();
                    }
                }

                //Return Unit to Start Position
                else if (selectedUnit.GetComponent<UnitController>().movementQueue.Count == 1)
                {
                    selectedUnit.GetComponent<UnitStats>().visualMovementSpeed = 0.5f;
                }
            }
        }
    }

    public void GenerateMapInfo()
    {
        //Get Map Size [x,y]
        tiles = new int[mapSizeX, mapSizeY];

        //Grass Tiles
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = grassTile;
            }
        }

        //Forest Tiles
        tiles[1, 11] = forestTile;
        tiles[2, 11] = forestTile;

        tiles[1, 9] = forestTile;
        tiles[2, 9] = forestTile;
        tiles[3, 9] = forestTile;
        tiles[6, 9] = forestTile;
        tiles[6, 8] = forestTile;
        tiles[7, 9] = forestTile;

        tiles[6, 0] = forestTile;
        tiles[7, 0] = forestTile;
        tiles[8, 0] = forestTile;
        tiles[9, 0] = forestTile;
        tiles[10, 0] = forestTile;
        tiles[11, 0] = forestTile;
        tiles[12, 0] = forestTile;
        tiles[13, 0] = forestTile;
        tiles[14, 0] = forestTile;

        tiles[8, 4] = forestTile;
        tiles[9, 5] = forestTile;

        tiles[12, 3] = forestTile;
        tiles[13, 3] = forestTile;

        tiles[15, 5] = forestTile;
        tiles[15, 7] = forestTile;
        tiles[15, 8] = forestTile;

        tiles[16, 11] = forestTile;
        tiles[17, 11] = forestTile;

        tiles[17, 8] = forestTile;
        tiles[18, 7] = forestTile;

        tiles[18, 3] = forestTile;

        //Mountain Tiles
        tiles[0, 11] = mountainTile;
        tiles[0, 10] = mountainTile;
        tiles[1, 10] = mountainTile;
        tiles[0, 9] = mountainTile;
        tiles[0, 8] = mountainTile;
        tiles[1, 8] = mountainTile;
        tiles[1, 7] = mountainTile;
        tiles[2, 8] = mountainTile;
        tiles[3, 8] = mountainTile;

        tiles[6, 11] = mountainTile;
        tiles[6, 10] = mountainTile;
        tiles[7, 11] = mountainTile;

        tiles[6, 4] = mountainTile;

        tiles[7, 3] = mountainTile;
        tiles[8, 3] = mountainTile;
        tiles[9, 3] = mountainTile;

        tiles[8, 5] = mountainTile;

        tiles[12, 2] = mountainTile;
        tiles[13, 2] = mountainTile;
        tiles[14, 2] = mountainTile;
        tiles[15, 2] = mountainTile;

        tiles[16, 6] = mountainTile;
        tiles[17, 6] = mountainTile;
        tiles[18, 5] = mountainTile;

        tiles[18, 2] = mountainTile;

        //Road Tiles
        tiles[1, 1] = roadTile;
        tiles[2, 1] = roadTile;
        tiles[3, 1] = roadTile;

        tiles[5, 1] = roadTile;
        tiles[6, 1] = roadTile;
        tiles[7, 1] = roadTile;
        tiles[8, 1] = roadTile;

        tiles[11, 1] = roadTile;
        tiles[12, 1] = roadTile;
        tiles[13, 1] = roadTile;

        tiles[14, 1] = roadTile;
        tiles[15, 1] = roadTile;
        tiles[16, 1] = roadTile;

        tiles[18, 1] = roadTile;

        //Bridge Tiles
        tiles[4, 1] = bridgeTile;

        tiles[14, 10] = bridgeTile;

        //Water Tiles
        tiles[4, 0] = waterTile;
        tiles[4, 2] = waterTile;
        tiles[4, 3] = waterTile;
        tiles[4, 4] = waterTile;
        tiles[4, 5] = waterTile;
        tiles[5, 5] = waterTile;
        tiles[6, 5] = waterTile;
        tiles[6, 6] = waterTile;
        tiles[7, 6] = waterTile;
        tiles[8, 6] = waterTile;
        tiles[9, 6] = waterTile;
        tiles[10, 6] = waterTile;
        tiles[11, 6] = waterTile;
        tiles[12, 6] = waterTile;
        tiles[12, 7] = waterTile;
        tiles[12, 8] = waterTile;
        tiles[13, 8] = waterTile;
        tiles[14, 8] = waterTile;
        tiles[14, 9] = waterTile;
        tiles[14, 11] = waterTile;
    }

    public void GeneratePathfindingGraph()
    {
        tileGraph = new Node[mapSizeX, mapSizeY];

        //Initialize Graph 
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tileGraph[x, y] = new Node();
                tileGraph[x, y].x = x;
                tileGraph[x, y].y = y;
            }
        }

        //calculate Neighbors
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                //Right
                if (x < mapSizeX - 1)
                {
                    tileGraph[x, y].neighbors.Add(tileGraph[x + 1, y]);
                }

                //Left
                if (x > 0)
                {                   
                    tileGraph[x, y].neighbors.Add(tileGraph[x - 1, y]);
                }

                //Up
                if (y < mapSizeY - 1)
                {
                    tileGraph[x, y].neighbors.Add(tileGraph[x, y + 1]);
                }

                //Down
                if (y > 0)
                {
                    tileGraph[x, y].neighbors.Add(tileGraph[x, y - 1]);
                }
            }
        }
    }

    //Generate Grid, Tiles, Pathfinding, & Cursor
    public void GenerateMapVisuals()
    {
        //Initialize Variables
        tilesOnMap = new GameObject[mapSizeX, mapSizeY];
        mapTiles = new GameObject[mapSizeX, mapSizeY];
        pathfindingTiles = new GameObject[mapSizeX, mapSizeY];
        cursorTiles = new GameObject[mapSizeX, mapSizeY];
        int index;

        //Generate Map Visuals
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                //Tile Prefabs
                index = tiles[x, y];
                GameObject newTile = Instantiate(tileTypes[index].tileVisualPrefab, new Vector3(x, 0, y), Quaternion.identity);
                newTile.GetComponent<ClickableTile>().tileX = x;
                newTile.GetComponent<ClickableTile>().tileY = y;
                newTile.GetComponent<ClickableTile>().map = this;
                newTile.transform.SetParent(tileTypesContainer.transform);
                tilesOnMap[x, y] = newTile;

                //Save Space at Vector3 (x, 0.501f, y) for Tile Map Overlays

                //Map Grid
                GameObject mapGridUI = Instantiate(mapUI, new Vector3(x, 0.502f, y), Quaternion.Euler(90f, 0, 0));
                mapGridUI.transform.SetParent(mapContainer.transform);
                mapTiles[x, y] = mapGridUI;

                //Pathfinding Grid
                GameObject pathfindingGridUI = Instantiate(mapUnitMovementUI, new Vector3(x, 0.503f, y), Quaternion.Euler(90f, 0, 0));
                pathfindingGridUI.transform.SetParent(pathfindingContainer.transform);
                pathfindingTiles[x, y] = pathfindingGridUI;

                //Cursor Grid
                GameObject cursorGridUI = Instantiate(mapCursorUI, new Vector3(x, 0.504f, y), Quaternion.Euler(90f, 0, 0));
                cursorGridUI.transform.SetParent(cursorContainer.transform);              
                cursorTiles[x, y] = cursorGridUI;
            }
        }
    }

    //Tile Coordinates in Scene Space Vector Adjusted by 0.75f to Account for Tile Map
    public Vector3 NodePositionInScene(int x, int y)
    {
        return new Vector3(x, 1, y);
    }

    public void SetNodeisOccupied()
    {
        foreach (Transform team in activeUnits.transform)
        {
            foreach (Transform unitOnTeam in team) { 
                int unitX = unitOnTeam.GetComponent<UnitController>().x;
                int unitY = unitOnTeam.GetComponent<UnitController>().y;
                unitOnTeam.GetComponent<UnitController>().occupiedTile = tilesOnMap[unitX, unitY];
                tilesOnMap[unitX, unitY].GetComponent<ClickableTile>().unitOnTile = unitOnTeam.gameObject;
            }
            
        }
    }

    public void GeneratePath(int x, int y)
    {
        //Selected Tile == Selected Unit Location
        if (selectedUnit.GetComponent<UnitController>().x == x && selectedUnit.GetComponent<UnitController>().y == y){
            currentPath = new List<Node>();
            selectedUnit.GetComponent<UnitController>().path = currentPath;
            return;
        }

        //Path Not Accessible
        if (isNodeEnterable(x, y) == false)
        {
            return;
        }

        selectedUnit.GetComponent<UnitController>().path = null;
        currentPath = null;
        Dictionary<Node, float> distance = new Dictionary<Node, float>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();
        Node source = tileGraph[selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y];
        Node target = tileGraph[x, y];
        distance[source] = 0;
        previous[source] = null;
        List<Node> unvisited = new List<Node>();

        //Initialize to Infinity (Scale Down From There)
        foreach (Node n in tileGraph)
        {
            if (n != source)
            {
                distance[n] = Mathf.Infinity;
                previous[n] = null;
            }
            unvisited.Add(n);
        }

        //Check All Unvisited Nodes
        while (unvisited.Count > 0)
        {
            //Univisted Node is Accesible
            Node unvisitedNode = null;

            foreach (Node accessibleUnvisitedNode in unvisited)
            {
                if (unvisitedNode == null || distance[accessibleUnvisitedNode] < distance[unvisitedNode])
                {
                    unvisitedNode = accessibleUnvisitedNode;
                }
            }

            //Unvisisted Node is the Target
            if (unvisitedNode == target)
            {
                break;
            }

            unvisited.Remove(unvisitedNode);

            foreach (Node n in unvisitedNode.neighbors)
            {
                float alt = distance[unvisitedNode] + CostToEnterTile(n.x, n.y);
                if (alt < distance[n])
                {
                    distance[n] = alt;
                    previous[n] = unvisitedNode;
                }
            }
        }

        //No Path Accessible
        if (previous[target] == null)
        {
            return;
        }

        currentPath = new List<Node>();
        Node tempCurrent = target;

        //Path to tempCurrent Node, Then Update Previous Node to Old Location
        while (tempCurrent != null)
        {
            currentPath.Add(tempCurrent);
            tempCurrent = previous[tempCurrent];
        }

        //Set Variable (currentPath)
        currentPath.Reverse();
        selectedUnit.GetComponent<UnitController>().path = currentPath;

    }

    public float CostToEnterTile(int x, int y)
    {
        if (isNodeEnterable(x, y) == false)
        {
            return Mathf.Infinity;
        }

        Tiles allTileTypes = tileTypes[tiles[x, y]];
        float distance = allTileTypes.movementCost;

        return distance;
    }

    //Bool for can a Unit Enter a Tile
    public bool isNodeEnterable(int x, int y)
    {
        //Cannot Enter Tile Occupied by Other Team
        if (tilesOnMap[x, y].GetComponent<ClickableTile>().unitOnTile != null)
        {
            if (tilesOnMap[x, y].GetComponent<ClickableTile>().unitOnTile.GetComponent<UnitController>().teamNumber != selectedUnit.GetComponent<UnitController>().teamNumber)
            {
                return false;
            }
        }

        //Can Enter Unoccupied Tile
        return tileTypes[tiles[x, y]].isWalkable;
    }

    public void ClickToSelectUnit()
    {
        //No Unit Selected & Tile is Displayed
        if (isUnitSelected == false && scriptGameController.tileDisplayed != null)
        {
            //Unit on Tile
            if (scriptGameController.tileDisplayed.GetComponent<ClickableTile>().unitOnTile != null)
            {
                //Select Unit on Tile
                GameObject tempSelectedUnit = scriptGameController.tileDisplayed.GetComponent<ClickableTile>().unitOnTile;

                //Selected Unit Movement State is 0
                if (tempSelectedUnit.GetComponent<UnitController>().unitMovementStates == tempSelectedUnit.GetComponent<UnitController>().GetMovementState(0))
                {
                    if (tempSelectedUnit.GetComponent<UnitController>().teamNumber == scriptGameController.currentTeam)
                    {
                        scriptMovementController.DisableMovementRangeHighlight();
                        //selectedSound.Play();
                        //Set Selected Unit & Setup UI
                        selectedUnit = tempSelectedUnit;
                        selectedUnit.GetComponent<UnitController>().map = this;
                        selectedUnit.GetComponent<UnitController>().SetMovementState(1);
                        selectedUnit.GetComponent<UnitController>().SetSelectedAnimation();
                        isUnitSelected = true;
                        scriptRangeFinder.HighlightUnitRange();
                    }
                }
            }

            //No Unit on Tile
            else if (scriptGameController.tileDisplayed.GetComponent<ClickableTile>().unitOnTile == null)
            {
                if (GameMenuController.menuOpen == false && scriptGameController.isGameOver == false)
                {
                    scriptGameMenuController.OpenGameMenu();
                }
            }
        }  
    }

    public void DeselectUnit()
    {
        //A Unit is Selected
        if (selectedUnit != null)
        {
            //Unit Movment State is Selected
            if (selectedUnit.GetComponent<UnitController>().unitMovementStates == selectedUnit.GetComponent<UnitController>().GetMovementState(1))
            {
                scriptMovementController.DisableMovementRangeHighlight();
                DisableUnitRouteUI();
                selectedUnit.GetComponent<UnitController>().SetMovementState(0);
                selectedUnit = null;
                isUnitSelected = false;
            }

            //Unit Movement State is Wait
            else if (selectedUnit.GetComponent<UnitController>().unitMovementStates == selectedUnit.GetComponent<UnitController>().GetMovementState(3))
            {
                scriptMovementController.DisableMovementRangeHighlight();
                DisableUnitRouteUI();
                isUnitSelected = false;
                selectedUnit = null;
            }

            //Unit Movement State is Not Selected or Wait
            else
            {
                scriptMovementController.DisableMovementRangeHighlight();
                DisableUnitRouteUI();
                tilesOnMap[selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y].GetComponent<ClickableTile>().unitOnTile = null;
                tilesOnMap[selectedUnitPreviousX, selectedUnitPreviousY].GetComponent<ClickableTile>().unitOnTile = selectedUnit;
                selectedUnit.GetComponent<UnitController>().x = selectedUnitPreviousX;
                selectedUnit.GetComponent<UnitController>().y = selectedUnitPreviousY;
                selectedUnit.GetComponent<UnitController>().occupiedTile = previousOccupiedTile;
                selectedUnit.transform.position = NodePositionInScene(selectedUnitPreviousX, selectedUnitPreviousY);
                selectedUnit.GetComponent<UnitController>().SetMovementState(0);
                selectedUnit = null;
                isUnitSelected = false;
            }
        }
    }

    public void DisableUnitRouteUI()
    {
        foreach(GameObject tile in pathfindingTiles)
        {
            //Turn off Renderer
            if (tile.GetComponent<Renderer>().enabled == true)
            {
                tile.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    //Get Attackable Units (Neighbord Nodes + Neighbor Node's Neighbors within Attack Range) for Movement State (Selected)
    public HashSet<Node> GetAttackableUnits()
    {
        HashSet<Node> tempNeighborNodes = new HashSet<Node>();
        HashSet<Node> neighborNodes = new HashSet<Node>();
        HashSet<Node> checkedNodes = new HashSet<Node>();
        Node selectedUnitNode = tileGraph[selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y];
        int attackRange = selectedUnit.GetComponent<UnitStats>().attackRange;

        //Add Initial Node to Neighbor Nodes
        neighborNodes = new HashSet<Node>();
        neighborNodes.Add(selectedUnitNode);

        //Add Each Neighboring Node & Each Neighbor to those Nodes within the Attack Range to the Variable(tempNeighborNodes)
        for (int i = 0; i < attackRange; i++)
        {
            foreach (Node n in neighborNodes)
            {
                foreach (Node nn in n.neighbors)
                {
                    //Add Furthest Attackable Tiles
                    tempNeighborNodes.Add(nn);
                    tempNeighborNodes.Add(n);
                }
            }

            //Set Variable(neighborNodes) to Temp Neighbor Nodes, then reset Temp Neighbor Nodes
            neighborNodes = tempNeighborNodes;
            tempNeighborNodes = new HashSet<Node>();
        }

        return neighborNodes;
    }

    public HashSet<Node> GetNodeUnitIsOccupying()
    {
        int x = selectedUnit.GetComponent<UnitController>().x;
        int y = selectedUnit.GetComponent<UnitController>().y;
        HashSet<Node> unitOccupiedTile = new HashSet<Node>();
        unitOccupiedTile.Add(tileGraph[x, y]);
        return unitOccupiedTile;
    }

    public void HighlightNodeUnitIsOccupying()
    {
        if (selectedUnit != null)
        {
            scriptMovementController.EnableMovementRangeHighlight(GetNodeUnitIsOccupying());
        }
    }

    //Play Sound, Set Animation to Wait, Disable Highlight & Route, & Deselect Unit Enum
    public IEnumerator DeselectUnitAfterMovement(GameObject unit, GameObject enemy)
    {
        //selectedSound.Play();
        selectedUnit.GetComponent<UnitController>().SetMovementState(3);
        scriptMovementController.DisableMovementRangeHighlight();
        DisableUnitRouteUI();

        //Required Yield to Avoid Errors
        yield return new WaitForSeconds(.25f);

        //Wait for Unit Combat to Complete
        while (unit.GetComponent<UnitController>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        //Wait for Enemy Combat to Complete
        while (enemy.GetComponent<UnitController>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
          
        }

        DeselectUnit();
    }
}
