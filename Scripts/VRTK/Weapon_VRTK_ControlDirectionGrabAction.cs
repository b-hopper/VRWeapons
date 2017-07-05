using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.SecondaryControllerGrabActions;

namespace VRWeapons
{
    public class Weapon_VRTK_ControlDirectionGrabAction : VRTK_ControlDirectionGrabAction
    {
        Weapon thisWeap;
        bool isGripped;
        Weapon_VRTK_InteractableObject intObj;

        private void Start()
        {
            thisWeap = GetComponent<Weapon>();
            intObj = thisWeap.GetComponent<Weapon_VRTK_InteractableObject>();
        }

        private void Update()
        {
            if (!isGripped && secondaryGrabbingObject != null)
            {
                thisWeap.secondHandGripped = true;
                isGripped = true;   
            }
            if (isGripped && secondaryGrabbingObject == null)
            {
                thisWeap.secondHandGripped = false;
                intObj.SetColliderEnabled(false);
                isGripped = false;
            }
        }
        
    }
}