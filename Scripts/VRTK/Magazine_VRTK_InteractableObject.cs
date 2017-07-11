using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace VRWeapons
{
    public class Magazine_VRTK_InteractableObject : VRTK_InteractableObject
    {
        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {            
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            base.OnInteractableObjectUngrabbed(e);
        }
    }
}