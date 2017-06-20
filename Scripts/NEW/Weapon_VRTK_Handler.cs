using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRWeapons;

public class Weapon_VRTK_Handler : VRTK_InteractableObject
{
    VRWeapons.Weapon thisWeap;

    private void Start()
    {
        thisWeap = GetComponent<VRWeapons.Weapon>();
    }


    public override void StartUsing(GameObject usingObject)
    {
        base.StartUsing(usingObject);
        thisWeap.StartFiring(usingObject);        
    }

    public override void StopUsing(GameObject previousUsingObject)
    {
        base.StopUsing(previousUsingObject);
        thisWeap.StopFiring(previousUsingObject);
    }
}
