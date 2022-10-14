using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitUIDisplay : MonoBehaviour
{
    [Header("Scripts")]
    public ScriptManager scriptManager;

    [Header("Unit UI")]
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

    public void Awake()
    {
        SetScriptManager();
    }

    public void SetScriptManager()
    {
        scriptManager = GameObject.Find("Script Manager").GetComponent<ScriptManager>();
        scriptManager.ConnectScripts();
    }

    public void UpdateUnitUI()
    {
        //Reference Variables
        RaycastHit hit = scriptManager.scriptGameController.hit;

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
}
