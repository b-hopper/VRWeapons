using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Valve.VR.InteractionSystem;


namespace VRWeapons
{
    public class VRWControl : MonoBehaviour
    {
        float VRWVersion = 2.0f;

        [HideInInspector]
        public List<Weapon> WeaponsInScene;

        [Header("For now, all it is capable of is providing the shot layer mask.")]
        [Header("VRWControl will be completed soon. Will handle events.")]

        [Tooltip("Check this to disable controller on weapon pickup.\n\nNOTE: If using VRTK, MAKE SURE you have the Controller Attach Point on the VRTK_InteractGrab " +
            "script assigned to something other than the default \"Tip\" object. Otherwise, disabling the model will also disable the weapon."), SerializeField]
        public bool disableControllersOnPickup;

        [Header("Gunshot layer mask")]
        public LayerMask shotMask;

        public delegate void TriggerHaptics();

        private void Start()
        {
            Debug.Log("VRWeapons Version " + VRWVersion.ToString("F2"));
        }

        int CountWeapons()
        {
            int i = 0;
            foreach(Weapon tmp in FindObjectsOfType<Weapon>())
            {
                WeaponsInScene.Add(tmp);
                tmp.OnWeaponFired += new Weapon.WeaponFiredEvent(WeaponFired);
                i++;
            }
            return i;
        }

        void WeaponFired(Weapon weapon, IBulletBehavior roundFired)
        {

        }

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