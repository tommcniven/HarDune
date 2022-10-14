using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Movement Path")]
    List<Node> currentPath;

    [Header("Path Arrows")]
    public Material arrowBody;
    public Material arrowCurve;
    public Material arrowTip;
    public Material cursorUI;

    void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }


    public List<Node> GenerateRouteToCursor(int x, int y)
    {
        //Selected Unit is Standing on Tile Selected
        if (scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().x == x && scriptManager.scriptTileMap.selectedUnit.GetComponent<UnitController>().y == y)
        {
            currentPath = new List<Node>();
            return currentPath;
        }

        //Selected Tile != Accessible
        if (scriptManager.scriptTileMap.isNodeEnterable(x, y) == false)
        {
            return null;
        }

        currentPath = null;
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

        currentPath = new List<Node>();
        Node tempCurrent = target;

        //Move to tempCurrent Node Then Update Previous to Old Location
        while (tempCurrent != null)
        {
            currentPath.Add(tempCurrent);
            tempCurrent = previous[tempCurrent];
        }

        //Need to Reverse to Generate Path from Current to Target
        currentPath.Reverse();
        return currentPath;
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
        //Reference Variables
        List<Node> unitPathToCursor = scriptManager.scriptGameController.unitPathToCursor;

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
        //Reference Variables
        List<Node> unitPathToCursor = scriptManager.scriptGameController.unitPathToCursor;

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
}
