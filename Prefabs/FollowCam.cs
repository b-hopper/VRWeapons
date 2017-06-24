using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour {

    [SerializeField]
    public Camera followCamera;

    [SerializeField]
    float lerpChange;
    float currentPos;

    [SerializeField]
    Vector3 offset;


    private void FixedUpdate()
    {

        if (currentPos > 1)
        {
            currentPos = 1;
        }
        Debug.Log("Check");
        followCamera.transform.parent = null;
        followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, transform.position + offset, currentPos);
        followCamera.transform.LookAt(transform);
        currentPos += lerpChange;

        Debug.Log(currentPos);

        Time.timeScale = Mathf.Lerp(1, 0.01f, currentPos);


    }
}
