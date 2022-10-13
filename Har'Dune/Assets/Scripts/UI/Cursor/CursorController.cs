using UnityEngine.Audio;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    //Referenced in Game Controller Script
    [Header("Cursor Information")]
    public Texture2D cursorIcon;
    public int cursorX;
    public int cursorY;
    public int tileUnderCursorX;
    public int tileUnderCursorY;

    void Awake()
    {
        SetCursor(cursorIcon);
    }

    private void SetCursor (Texture2D mouseCursor)
    {
        Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.Auto);
    }
}
