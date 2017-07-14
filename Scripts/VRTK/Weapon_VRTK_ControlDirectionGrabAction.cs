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
        
        private void Start()
        {
            thisWeap = GetComponent<Weapon>();
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