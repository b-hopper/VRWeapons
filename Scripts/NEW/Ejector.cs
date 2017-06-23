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
            t.parent = null;
            rb.isKinematic = false;
            rb.AddForce((Vector3.forward + (Random.insideUnitSphere * ejectorRandomness)) * ejectForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * ejectorRotationalRandomness, ForceMode.Impulse);
        }
    }
}