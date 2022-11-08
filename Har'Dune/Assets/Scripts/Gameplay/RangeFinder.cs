using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeFinder : MonoBehaviour
{
    [Header("Scripts")]
    public TileMap scriptTileMap;
    public MovementController scriptMovementController;


    public void HighlightUnitRange()
    {
        //Reference Variables
        var selectedUnit = scriptTileMap.selectedUnit;
        var tileGraph = scriptTileMap.tileGraph;
        var tilesOnMap = scriptTileMap.tilesOnMap;
        var selectedUnitTotalRange = scriptTileMap.selectedUnitTotalRange;

        //Set Variables
        HashSet<Node> movementRange = new HashSet<Node>();
        HashSet<Node> attackableTiles = new HashSet<Node>();
        HashSet<Node> enemyUnitsInMovementRange = new HashSet<Node>();
        int attackRange = selectedUnit.GetComponent<UnitStats>().maxAttackRange;
        Node selectedUnitNode = tileGraph[selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y];
        movementRange = scriptMovementController.GetMovementRange();
        attackableTiles = GetAttackRange(movementRange, attackRange, selectedUnitNode);

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

    public HashSet<Node> GetTotalRange(HashSet<Node> totalMovementRange, HashSet<Node> totalAttackableTiles)
    {
        HashSet<Node> totalRange = new HashSet<Node>();
        totalRange.UnionWith(totalMovementRange);
        totalRange.UnionWith(totalAttackableTiles);
        return totalRange;
    }

    public HashSet<Node> GetAttackRange(HashSet<Node> movementHighlight, int attackRange, Node unitInitialNode)
    {
        HashSet<Node> tempNeighorNodes = new HashSet<Node>();
        HashSet<Node> neighborNodes = new HashSet<Node>();
        HashSet<Node> checkedNodes = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();

        foreach (Node n in movementHighlight)
        {
            neighborNodes = new HashSet<Node>();
            neighborNodes.Add(n);

            for (int i = 0; i < attackRange; i++)
            {
                foreach (Node t in neighborNodes)
                {
                    foreach (Node tn in t.neighbors)
                    {
                        tempNeighorNodes.Add(tn);
                    }
                }

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

    public HashSet<Node> GetAttackableUnits()
    {
        //Set Variables
        var tileGraph = scriptTileMap.tileGraph;
        var selectedUnit = scriptTileMap.selectedUnit;
        HashSet<Node> tempNeighborNodes = new HashSet<Node>();
        HashSet<Node> neighborNodes = new HashSet<Node>();
        HashSet<Node> checkedNodes = new HashSet<Node>();
        Node selectedUnitNode = tileGraph[selectedUnit.GetComponent<UnitController>().x, selectedUnit.GetComponent<UnitController>().y];
        int attackRange = selectedUnit.GetComponent<UnitStats>().attackRange;
        neighborNodes = new HashSet<Node>();
        neighborNodes.Add(selectedUnitNode);

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
