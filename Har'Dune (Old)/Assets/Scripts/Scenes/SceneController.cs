using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Canvases")]
    public Canvas mainCanvas;
    public Canvas helpCanvas;
    public Canvas helpCanvas2;

    //Scenes
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadLevelOne()
    {
        SceneManager.LoadScene(1);
    }

    //Canvases
    public void LoadHelpCanvas()
    {
        mainCanvas.enabled = false;
        helpCanvas.enabled = true;
    }

    public void LoadHelpCanvasBack()
    {
        helpCanvas2.enabled = false;
        helpCanvas.enabled = true;
    }

    public void LoadHelpCanvas2()
    {
        helpCanvas.enabled = false;
        helpCanvas2.enabled = true;
    }

    public void LoadMainCanvas()
    {
        mainCanvas.enabled = true;
        helpCanvas.enabled = false;
        helpCanvas2.enabled = false;
    }
}