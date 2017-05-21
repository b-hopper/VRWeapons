using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Flashlight : MonoBehaviour {
    
    Weapon parentWeapon;
    public GameObject light;
    VRWControl control;
    Hand currentHand;

    void Awake()
    {
        control = FindObjectOfType<VRWControl>();
    }

    public void TurnOnLight()
    {
        if (light.activeSelf)
        {
            light.SetActive(false);
        }
        else
        {
            light.SetActive(true);
        }
    }
}
