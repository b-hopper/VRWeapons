using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    public class Ejector : MonoBehaviour, IEjectorActions
    {
        [SerializeField]
        float ejectForce = 0.15f;

        [SerializeField]
        float ejectorRandomness = 0.2f;

        [SerializeField]
        float ejectorRotationalRandomness = 45;
        
        public void Eject(Transform t, Rigidbody rb)
        {
            rb.isKinematic = false;
            Vector3 forward = (transform.forward + (Random.insideUnitSphere * ejectorRandomness)).normalized;
            rb.AddForce(forward * ejectForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * ejectorRotationalRandomness, ForceMode.Impulse);
            t.parent = null;
        }
    }
}