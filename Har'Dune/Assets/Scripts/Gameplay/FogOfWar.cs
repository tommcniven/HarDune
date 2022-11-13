using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Materials")]
    public Material fogMat;

    private void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    void UpdateFog()
    {
        var selectedUnit = scriptManager.scriptTileMap.selectedUnit;

        if (selectedUnit != null)
        {
            SetFogRange(scriptManager.scriptRangeFinder.GetVisionRange());
        }
    }

    public void SetFogRange(HashSet<Node> visibleNodes) //Note -- Move to FogOfWar Script
    {
        foreach (Node node in visibleNodes)
        {
            GameObject visibleNode = scriptManager.scriptTileMap.fogTiles[node.x, node.y];
            visibleNode.GetComponent<Renderer>().material = fogMat;
            visibleNode.GetComponent<Renderer>().enabled = false; //Note -- Need to start with all fogTiles On
        }
    }
}
