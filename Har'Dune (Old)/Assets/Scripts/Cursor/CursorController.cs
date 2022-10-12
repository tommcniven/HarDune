using UnityEngine.Audio;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    //Variables
    public Texture2D cursorIcon;

    //Start
    void Awake()
    {
        SetCursor(cursorIcon);
        //Cursor.lockState = CursorLockMode.Confined;
    }

    //Out: Sets Cursor
    private void SetCursor (Texture2D mouseCursor)
    {
        Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.Auto);
    }
}
