using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

public class VRW_ControllerActions_VRTK : MonoBehaviour
{
    [HideInInspector]
    public Weapon CurrentHeldWeapon;

    private void Start()
    {
        GetComponent<VRTK.VRTK_ControllerEvents>().TouchpadPressed += new VRTK.ControllerInteractionEventHandler(DropMagazine);
        GetComponent<VRTK.VRTK_ControllerEvents>().TriggerAxisChanged += new VRTK.ControllerInteractionEventHandler(TriggerAxisChanged);
    }

    private void DropMagazine(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (CurrentHeldWeapon != null && e.controllerReference.scriptAlias == CurrentHeldWeapon.holdingDevice)
        {
            CurrentHeldWeapon.DropMagazine();
        }
    }

    private void TriggerAxisChanged(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (CurrentHeldWeapon != null && e.controllerReference.scriptAlias == CurrentHeldWeapon.holdingDevice)
        {
            CurrentHeldWeapon.SetTriggerAngle(e.buttonPressure);
        }
    }
}
