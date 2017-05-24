using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;

public class ReloadPoint : MonoBehaviour {

    magazine mag;
    VRWRound round;
    public VRTK.Weapon weap;

    void OnTriggerStay(Collider col)
    {
        if (mag == null && col.GetComponent<magazine>() != null)
        {
            mag = col.GetComponent<magazine>();
        } 
        else if (round == null && col.GetComponent<VRWRound>() != null)
        {
            round = col.GetComponent<VRWRound>();
        }
        if (mag != null && weap.weaponType == mag.weaponType && (mag.held || !weap.magNeedsHeldToReload))
        {
            if (col.gameObject.tag == "Magazine" && mag.ableToUse == true)
            {
                if (weap.mag == null && (!weap.chamberMustBeOpenToReload || weap.chamberOpen))
                {
                    weap.Reload(mag);
                }
            }
        }
        else if (round != null && weap.weaponType == round.WeaponType && (round.held || !weap.magNeedsHeldToReload))
        {
            if (weap.chamberOpen || !weap.chamberMustBeOpenToReload)
            {
                weap.Reload(round);
            }
        }
    }
    void OnTriggerExit(Collider col)
    {
        mag = null;
        round = null;
    }
}
