using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeFinder : MonoBehaviour
{
    [Header("Scripts")]
    public TileMap scriptTileMap;
    public MovementController scriptMovementController;


    //Highlight Total Range (Movement + Max Attack Range) for Movement State (Selected)
    public void HighlightUnitRange()
    {
        //Reference Variables
        var selectedUnit = scriptTileMap.selectedUnit;
        var tileGraph = scriptTileMap.tileGraph;
        var tilesOnMap = scriptTileMap.tilesOnMap;
        var selectedUnitTotalRange = scriptTileMap.selectedUnitTotalRange;

        HashSet<Node> movementRange = new HashSet<Node>();
        HashSet<Node> attackableTiles = new HashSet<Node>();
        HashSet<Node> enemyUnitsInMovementRange = new HashSet<Node>();
        int attackRange = selectedUnit.GetComponent<UnitStats>().maxAttackRange;
        Node selectedUnitNode = tileGraph[selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y];
        movementRange = scriptMovementController.GetMovementRange();
        attackableTiles = GetAttackRange(movementRange, attackRange, selectedUnitNode);
        // Remove scriptTileMap ^^^

        //Check Attackable Tiles for Units
        foreach (Node n in attackableTiles)
        {
            //Unit on Tile
            if (tilesOnMap[n.x, n.y].GetComponent<ClickableTile>().unitOnTile != null)
            {
                GameObject unitOnSelectedTile = tilesOnMap[n.x, n.y].GetComponent<ClickableTile>().unitOnTile;

                //Unit on Tile is Enemy Unit
                if (unitOnSelectedTile.GetComponent<UnitController>().teamNumber != selectedUnit.GetComponent<UnitController>().teamNumber)
                {
                    enemyUnitsInMovementRange.Add(n);
                }
            }
        }

        HighlightEnemiesInRange(attackableTiles);
        scriptMovementController.EnableMovementRangeHighlight(movementRange);
        scriptTileMap.selectedUnitMovementRange = movementRange;
        selectedUnitTotalRange = GetTotalRange(movementRange, attackableTiles);
    }

    //Get Total Range (Movemnet Range + Attack Range) for Movement State (Selected)
    public HashSet<Node> GetTotalRange(HashSet<Node> totalMovementRange, HashSet<Node> totalAttackableTiles)
    {
        HashSet<Node> totalRange = new HashSet<Node>();
        totalRange.UnionWith(totalMovementRange);
        totalRange.UnionWith(totalAttackableTiles);
        return totalRange;
    }


    //Get Total Attackable Tiles (Neighbord Nodes + Neighbor Node's Neighbors within Attack Range) for Movement State (Selected)
    public HashSet<Node> GetAttackRange(HashSet<Node> movementHighlight, int attackRange, Node unitInitialNode)
    {
        HashSet<Node> tempNeighorNodes = new HashSet<Node>();
        HashSet<Node> neighborNodes = new HashSet<Node>();
        HashSet<Node> checkedNodes = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();

        //Add All Nodes in Movement Highlight into Variable(neighborNodes)
        foreach (Node n in movementHighlight)
        {
            neighborNodes = new HashSet<Node>();
            neighborNodes.Add(n);

            //Add Each Neighboring Node & Each Neighbor to those Nodes within the Attack Range to the Variable(tempNeighborNodes)
            for (int i = 0; i < attackRange; i++)
            {
                foreach (Node t in neighborNodes)
                {
                    foreach (Node tn in t.neighbors)
                    {
                        tempNeighorNodes.Add(tn);
                    }
                }

                //Set Variable(neighborNodes) to Temp Neighbor Nodes, then reset Temp Neighbor Nodes
                neighborNodes = tempNeighorNodes;
                tempNeighorNodes = new HashSet<Node>();

                //Add Neighbor Nodes to Checked Nodes
                if (i < attackRange - 1)
                {
                    checkedNodes.UnionWith(neighborNodes);
                }
            }

            neighborNodes.ExceptWith(checkedNodes);
            checkedNodes = new HashSet<Node>();
            totalAttackableTiles.UnionWith(neighborNodes);
        }

        totalAttackableTiles.Remove(unitInitialNode);
        return totalAttackableTiles;
    }

    //Get Attackable Units (Neighbord Nodes + Neighbor Node's Neighbors within Attack Range) for Movement State (Selected)
    public HashSet<Node> GetAttackableUnits()
    {
        //Reference Variables
        var tileGraph = scriptTileMap.tileGraph;
        var selectedUnit = scriptTileMap.selectedUnit;

        //Set Variables
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

    public void HighlightAttackableUnitsInRange()
    {
        var selectedUnit = scriptTileMap.selectedUnit;

        if (selectedUnit != null)
        {
            HighlightEnemiesInRange(GetAttackableUnits());
        }
    }

    public void HighlightEnemiesInRange(HashSet<Node> enemiesToHighlight)
    {
        //Reference Variables
        var mapTiles = scriptTileMap.mapTiles;
        var lightBorderTileHighlight = scriptTileMap.lightBorderTileHighlight;

        foreach (Node n in enemiesToHighlight)
        {
            mapTiles[n.x, n.y].GetComponent<Renderer>().material = lightBorderTileHighlight;
            mapTiles[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void HighlightFriendlyUnitsInRange()
    {
        //Reference Variables
        var selectedUnit = scriptTileMap.selectedUnit;

        if (selectedUnit != null)
        {
            HighlightFriendlyUnitsInRange(GetAttackableUnits());
        }
    }

    public void HighlightFriendlyUnitsInRange(HashSet<Node> friendlyUnitsToHighlight)
    {
        //Reference Variables
        var mapTiles = scriptTileMap.mapTiles;
        var blueBorderTileHighlight = scriptTileMap.blueBorderTileHighlight;

        foreach (Node n in friendlyUnitsToHighlight)
        {
            mapTiles[n.x, n.y].GetComponent<Renderer>().material = blueBorderTileHighlight;
            mapTiles[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
