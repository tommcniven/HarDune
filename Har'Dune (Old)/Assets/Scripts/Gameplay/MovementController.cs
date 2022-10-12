using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Scripts")]
    public TileMap scriptTileMap;
    public GameMenuController scriptGameMenuController;

    public HashSet<Node> GetMovementRange()
    {
        //Reference Varaibles
        var mapSizeX = scriptTileMap.mapSizeX;
        var mapSizeY = scriptTileMap.mapSizeY;
        var selectedUnit = scriptTileMap.selectedUnit;
        var tileGraph = scriptTileMap.tileGraph;

        //Initialize Variables
        HashSet<Node> highlightUI = new HashSet<Node>();
        HashSet<Node> temphighlightUI = new HashSet<Node>();
        HashSet<Node> movementHighlight = new HashSet<Node>();

        //Initialize Variables
        float[,] movementCost = new float[mapSizeX, mapSizeY];
        int moveSpeed = selectedUnit.GetComponent<UnitStats>().movementSpeed;
        Node initialNode = tileGraph[selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y];
        movementHighlight.Add(initialNode);

        //Check Neighbor Nodes
        foreach (Node n in initialNode.neighbors)
        {
            movementCost[n.x, n.y] = scriptTileMap.CostToEnterTile(n.x, n.y);

            //If Neighbors of the Unit's Initial Tile is Accessible Add Neighboring Nodes[x,y] to highlightUI
            if (moveSpeed - movementCost[n.x, n.y] >= 0)
            {
                highlightUI.Add(n);
            }
        }

        movementHighlight.UnionWith(highlightUI);

        //While Highlight Count != 0, Check the Neighbor Tiles
        while (highlightUI.Count != 0)
        {
            foreach (Node n in highlightUI)
            {
                foreach (Node nn in n.neighbors)
                {
                    //No Neighbors to Unit's Initial Tile Neighbors
                    if (!movementHighlight.Contains(nn))
                    {
                        movementCost[nn.x, nn.y] = scriptTileMap.CostToEnterTile(nn.x, nn.y) + movementCost[n.x, n.y];
                        //If Neighbor Tiles to Unit's Intiail Tile Neighbors are Accessible Add new Neighbor Nodes[x,y] to temphighlightUI
                        if (moveSpeed - movementCost[nn.x, nn.y] >= 0)
                        {
                            temphighlightUI.Add(nn);
                        }
                    }
                }
            }
            //Combine Highlighted Areas, Set Final Movement Highlight UI, then Reset the Variable (tempHighlightUI)
            highlightUI = temphighlightUI;
            movementHighlight.UnionWith(highlightUI);
            temphighlightUI = new HashSet<Node>();
        }

        return movementHighlight;
    }

    public void EnableMovementRangeHighlight(HashSet<Node> movementToHighlight)
    {
        var mapTiles = scriptTileMap.mapTiles;
        var lightTileHighlight = scriptTileMap.lightTileHighlight;

        foreach (Node n in movementToHighlight)
        {
            mapTiles[n.x, n.y].GetComponent<Renderer>().material = lightTileHighlight;
            mapTiles[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void DisableMovementRangeHighlight()
    {
        var mapTiles = scriptTileMap.mapTiles;

        foreach (GameObject tile in mapTiles)
        {
            if (tile.GetComponent<Renderer>().enabled == true)
            {
                tile.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void MoveUnit()
    {
        //Reference Variables
        var selectedUnit = scriptTileMap.selectedUnit;

        if (selectedUnit != null)
        {
            selectedUnit.GetComponent<UnitController>().MoveNextTile();
        }
    }

    public IEnumerator MoveUnitEnum()
    {
        //Reference Varaiables
        var selectedUnit = scriptTileMap.selectedUnit;

        //Run Methods
        DisableMovementRangeHighlight();
        scriptTileMap.DisableUnitRouteUI();

        //Wait to Exit Movement Queue
        while (selectedUnit.GetComponent<UnitController>().movementQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }

        UnitMoved_UpdatedVariables();
    }

    public void UnitMoved_UpdatedVariables()
    {
        //Reference Varaiables
        var tilesOnMap = scriptTileMap.tilesOnMap;
        var selectedUnit = scriptTileMap.selectedUnit;

        //Set Variables
        selectedUnit.GetComponent<UnitController>().SetSelectedAnimation();
        tilesOnMap[selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y].GetComponent<ClickableTile>().unitOnTile = selectedUnit;
        selectedUnit.GetComponent<UnitController>().SetMovementState(2);
        scriptGameMenuController.OpenActionMenu();
    }


    //Determine if Clicked Tile is Accessible & Generate Path
    public bool isSelectedNodeAccessible()
    {
        //Reference Variables
        var tileGraph = scriptTileMap.tileGraph;
        var selectedUnitMovementRange = scriptTileMap.selectedUnitMovementRange;
        var selectedUnit = scriptTileMap.selectedUnit;

        //Set Variables
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Clicked a Game Object
        if (Physics.Raycast(ray, out hit))
        {
            //Clicked a Tile
            if (hit.transform.gameObject.CompareTag("Tile"))
            {

                int clickedTileX = hit.transform.GetComponent<ClickableTile>().tileX;
                int clickedTileY = hit.transform.GetComponent<ClickableTile>().tileY;
                Node nodeToCheck = tileGraph[clickedTileX, clickedTileY];

                //Movement Range Still Has a Node to Check
                if (selectedUnitMovementRange.Contains(nodeToCheck))
                {
                    //Generate a Path
                    if ((hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile == null || hit.transform.gameObject.GetComponent<ClickableTile>().unitOnTile == selectedUnit) && (selectedUnitMovementRange.Contains(nodeToCheck)))
                    {
                        scriptTileMap.GeneratePath(clickedTileX, clickedTileY);
                        return true;
                    }
                }
            }

            //Clicked a Unit
            else if (hit.transform.gameObject.CompareTag("Unit"))
            {
                //Clicked an Enemy Unit
                if (hit.transform.parent.GetComponent<UnitController>().teamNumber != selectedUnit.GetComponent<UnitController>().teamNumber)
                {
                    //[Update] Good Place to Generate Path on Enemy Click Prior to Movement
                }

                //Generate Path to Selected Unit
                else if (hit.transform.parent.gameObject == selectedUnit)
                {
                    scriptTileMap.GeneratePath(selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y);
                    return true;
                }
            }
        }

        return false;
    }
}
