using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VRWeapons;

namespace VRWeapons
{
    [System.Serializable]
    public class CameraFollowBullet : MonoBehaviour, IBulletBehavior
    {
        [SerializeField]
        GameObject projectile;

        [SerializeField]
        Camera followCamera;

        [SerializeField]
        float projForce;

        [SerializeField]
        Vector3 offset;

        [SerializeField]
        float lerpChange;

        bool isFollowing;
        float currentPos;
        GameObject proj;

        public void DoBulletBehavior(Transform muzzleDir, float damage, float range, float bulletSpreadRange, Weapon thisWeapon, LayerMask shotMask)
        {
            proj = Instantiate(projectile, muzzleDir);
            proj.transform.localPosition = Vector3.zero;
            if (proj.GetComponent<Rigidbody>() != null)
            {
                proj.GetComponent<Rigidbody>().AddForce(muzzleDir.forward.normalized * projForce, ForceMode.Impulse);
                isFollowing = true;
            }
            proj.transform.parent = null;
            proj.GetComponent<FollowCam>().followCamera = followCamera;

        }

    }
}