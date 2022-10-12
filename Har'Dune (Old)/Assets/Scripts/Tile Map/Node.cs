using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    // Variables
    public List<Node> neighbors;
    public int x;
    public int y;

    //Out: Define Neighbors as a Node List
    public Node()
    {
        neighbors = new List<Node>();
    }
}
