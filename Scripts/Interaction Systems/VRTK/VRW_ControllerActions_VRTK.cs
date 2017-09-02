using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace VRWeapons.InteractionSystems.VRTK
{

    public class VRW_ControllerActions_VRTK : MonoBehaviour
    {
        [HideInInspector]
        public Weapon CurrentHeldWeapon;

        [SerializeField]
        private bool dropMagazineWithTouchpad = true;

        private void Start()
        {
            if(dropMagazineWithTouchpad)
            { 
                GetComponent<VRTK_ControllerEvents>().TouchpadPressed += new ControllerInteractionEventHandler(DropMagazine);
            }
            GetComponent<VRTK_ControllerEvents>().TriggerAxisChanged += new ControllerInteractionEventHandler(TriggerAxisChanged);
        }

        public void DropMagazine(object sender, ControllerInteractionEventArgs e)
        {
            if (CurrentHeldWeapon != null && e.controllerReference.scriptAlias == CurrentHeldWeapon.holdingDevice)
            {
                CurrentHeldWeapon.DropMagazine();
            }
        }

        private void TriggerAxisChanged(object sender, ControllerInteractionEventArgs e)
        {
            if (CurrentHeldWeapon != null && e.controllerReference.scriptAlias == CurrentHeldWeapon.holdingDevice)
            {
                CurrentHeldWeapon.SetTriggerAngle(e.buttonPressure);
            }
        }
    }
}