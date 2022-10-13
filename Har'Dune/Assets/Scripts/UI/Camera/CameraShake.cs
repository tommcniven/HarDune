using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    //Out: Camera Shake
    public IEnumerator ShakeCamera(float duration,float cameraShakeStrength,Vector3 direction)
    {
        if (cameraShakeStrength > 10)
        {
            cameraShakeStrength = 10;
        }

        float timePassed = 0f;
        while (timePassed < duration)
        {
            float xPosition = Random.Range(-.1f, .1f)*cameraShakeStrength;
            float zPosition = Random.Range(-.1f, .1f)*cameraShakeStrength;
            Vector3 newPosition = new Vector3(transform.position.x + xPosition, transform.position.y, transform.position.z + zPosition);
            transform.position = Vector3.Lerp(transform.position,newPosition,0.15f);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }           
    }
}
