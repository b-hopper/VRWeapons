using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace VRWeapons
{
    [RequireComponent(typeof(Magazine))]
    public class Magazine_UnityEvents : MonoBehaviour
    {
        public UnityEvent BulletPushed;
        public UnityEvent BulletPopped;

        private Magazine mag;

        private void Awake()
        {
            mag = GetComponent<Magazine>();
            if(mag == null)
            {
                Debug.LogError("Magazine not found");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            mag.BulletPushed += Mag_BulletPushed;
            mag.BulletPoped += Mag_BulletPoped;
        }

        private void OnDisable()
        {
            mag.BulletPushed -= Mag_BulletPushed;
            mag.BulletPoped -= Mag_BulletPoped;
        }

        private void Mag_BulletPushed(object sender, System.EventArgs e)
        {
            BulletPushed.Invoke();
        }

        private void Mag_BulletPoped(object sender, System.EventArgs e)
        {
            BulletPopped.Invoke();
        }
    }
}
