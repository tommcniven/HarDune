using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Pan Variables
    private float panSpeed = 10f;
    private float panBorder = 10f;
    private Vector2 panLimitPositive = new Vector2(14, 9);
    private Vector2 panLimitNegative = new Vector2(-4, -2);

    //Update Void
    void Update()
    {
        //Variables
        Vector3 cameraPosition = transform.position;
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        //Up
        if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorder)
        {
            cameraPosition.z += panSpeed * Time.deltaTime;
        }

        //Down
        if (Input.GetKey("s") || Input.mousePosition.y <= panBorder)
        {
            cameraPosition.z -= panSpeed * Time.deltaTime;
        }

        //Right
        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorder)
        {
            cameraPosition.x += panSpeed * Time.deltaTime;
        }

        //Left
        if (Input.GetKey("a") || Input.mousePosition.x <= panBorder)
        {
            cameraPosition.x -= panSpeed * Time.deltaTime;
        }

        //Pan Limits
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, -panLimitNegative.x, panLimitPositive.x);
        cameraPosition.z = Mathf.Clamp(cameraPosition.z, -panLimitNegative.y, panLimitPositive.y);

        //Return
        transform.position = cameraPosition;
    }
}
