using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class GunScope : MonoBehaviour {

    bool zoomStageFar;
    Camera scopeCam;
    public float closeZoom, farZoom;

    void Awake()
    {
        scopeCam = GetComponentInChildren<Camera>();
        scopeCam.fieldOfView = closeZoom;
    }

    public void ChangeZoom()
    {
        if (zoomStageFar)
        {
            scopeCam.fieldOfView = closeZoom;
            zoomStageFar = false;
        }
        else
        {
            scopeCam.fieldOfView = farZoom;
            zoomStageFar = true;
        }
    }
    

}
