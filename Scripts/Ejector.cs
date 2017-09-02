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

        [SerializeField]
        private ForceMode forceMode = ForceMode.Impulse;

        [SerializeField]
        private bool enableCollider = false;

        public void Eject(Transform t, Rigidbody rb)
        {
            rb.isKinematic = false;
            Vector3 forward = (transform.forward + (Random.insideUnitSphere * ejectorRandomness)).normalized;
            rb.AddForce(forward * ejectForce, forceMode);
            rb.AddTorque(Random.insideUnitSphere * ejectorRotationalRandomness, forceMode);
            t.parent = null;

            if(enableCollider)
            {
                var collider = rb.GetComponent<Collider>();
                if(collider != null)
                {
                    collider.enabled = true;
                }
            }
        }
    }
}