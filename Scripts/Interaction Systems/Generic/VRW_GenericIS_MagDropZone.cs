using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons.InteractionSystems.Generic
{
    [RequireComponent(typeof(Rigidbody))]
    public class VRW_GenericIS_MagDropZone : MonoBehaviour
    {
        Weapon thisWeap;
        Collider thisCol;
        IMagazine collidingMagazine;

        [SerializeField]
        Transform magPosition;

        [SerializeField]
        Transform startingMag;

        [Tooltip("How long in seconds it takes for magazine to reach mag well, once inserted."), SerializeField]
        float timeToMagInsert;

        private void Start()
        {
            thisWeap = GetComponentInParent<Weapon>();
            thisCol = GetComponent<Collider>();
            GetComponent<Rigidbody>().isKinematic = true;
            if (magPosition != null)
            {
                magPosition.gameObject.SetActive(false);
            }
            if (thisCol != null)
            {
                Physics.IgnoreCollision(thisCol, thisWeap.weaponBodyCollider);
                if (thisWeap.secondHandGripCollider != null)
                {
                    Physics.IgnoreCollision(thisCol, thisWeap.weaponBodyCollider);
                }
            }
            else
            {
                Debug.LogError("No collider found on mag drop zone (Child of " + thisWeap + "). Please add a collider.");
            }
            thisCol.isTrigger = true;

            if (startingMag != null)
            {
                InsertMag(startingMag.GetComponent<IMagazine>(), startingMag);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if ((collidingMagazine = collision.transform.GetComponent<IMagazine>()) == null)
            {
                Physics.IgnoreCollision(thisCol, collision.collider);
            }
            else
            {
                if (!thisWeap.IsLoaded())
                {
                    collision.transform.parent = thisWeap.transform;
                    collision.rigidbody.isKinematic = true;
                    if (magPosition != null)
                    {
                        if (timeToMagInsert > 0)
                        {
                            LerpMovement(collision.transform, collidingMagazine);
                        }
                        else
                        {
                            InsertMag(collidingMagazine, collision.transform);
                        }
                    }
                }
            }
        }

        IEnumerator LerpMovement(Transform t, IMagazine mag)
        {
            float startTime = Time.time;
            while (Time.time < startTime + timeToMagInsert)
            {
                float lerpVal = (Time.time - startTime) / timeToMagInsert;
                t.localPosition = Vector3.Lerp(t.localPosition, magPosition.localPosition, lerpVal);
                t.localEulerAngles = Vector3.Lerp(t.localEulerAngles, magPosition.localEulerAngles, lerpVal);
                yield return new WaitForFixedUpdate();
            }
            InsertMag(mag, t);
            mag.MagIn(thisWeap);
        }

        void InsertMag(IMagazine newMag, Transform t)
        {
            t.localPosition = magPosition.localPosition;
            t.localEulerAngles = magPosition.localEulerAngles;
            newMag.MagIn(thisWeap);
        }
    }
}