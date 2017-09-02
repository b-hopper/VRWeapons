using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

public class DamagePassthrough : MonoBehaviour, IAttackReceiver {
    HealthManager parentHM;

    [SerializeField]
    float damageMultiplier = 1;

    private void Start()
    {
        parentHM = GetComponentInParent<HealthManager>();
    }    

    public void ReceiveAttack(Weapon.Attack attack)
    {
        attack.damage *= damageMultiplier;
        parentHM.ReceiveAttack(attack);
    }

}
