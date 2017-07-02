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
            return Mathf.Clamp(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB), 0, 1);
        }

        public static Vector3 V3Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            Vector3 tmp = value;
            tmp = new Vector3(Mathf.Clamp(tmp.x, min.x, max.x), Mathf.Clamp(tmp.y, min.y, max.y), Mathf.Clamp(tmp.z, min.z, max.z));
            return tmp;
        }
    }
}