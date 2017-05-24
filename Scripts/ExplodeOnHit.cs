using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ExplodeOnHit : MonoBehaviour {

    public GameObject explosion;
    Vector3 origin;
    VRTK.Weapon.Attack attack;

    void Awake()
    {
        origin = transform.position;
        attack = new VRTK.Weapon.Attack
        {
            damage = 1,
            headshotMultiplier = 1,
            origin = this.transform.position
        };

    }

    void OnCollisionEnter(Collision col)
    {
        if (Vector3.Distance(transform.position, origin) > 2)
        {
            GameObject explode = Instantiate(explosion, this.transform.position, Quaternion.Euler(Vector3.zero));
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                ExecuteEvents.Execute<IAttackReceiver>(hitColliders[i].gameObject, null, ((handler, eventData) => handler.ReceiveAttack(attack)));
            }
            Destroy(this.gameObject);
        }
    }
}
