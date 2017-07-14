using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.SecondaryControllerGrabActions;

namespace VRWeapons
{
    public class Weapon_VRTK_ControlDirectionGrabAction : VRTK_ControlDirectionGrabAction
    {
        Weapon thisWeap;
        Weapon_VRTK_InteractableObject intObj;
        bool isGripped;
        
        private void Start()
        {
            thisWeap = GetComponent<Weapon>();
            intObj = GetComponent<Weapon_VRTK_InteractableObject>();
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
                thisWeap.SetColliderEnabled(thisWeap.secondHandGripCollider, true);
                isGripped = false;
            }
        }        
    }
}