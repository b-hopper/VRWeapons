using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;


namespace VRWeapons
{
    public class VRWControl : MonoBehaviour
    {
        [Header("Gunshot layer mask")]
        [Header("For now, all it is capable of is providing the shot layer mask.")]
        [Header("VRWControl will be completed soon. Will handle events.")]
        public LayerMask shotMask;

        public static float V3InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }
    }
}