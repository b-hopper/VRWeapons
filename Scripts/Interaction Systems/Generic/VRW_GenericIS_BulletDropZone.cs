using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons.InteractionSystems.Generic
{
    [RequireComponent(typeof(Rigidbody))]
    public class VRW_GenericIS_BulletDropZone : MonoBehaviour
    {
        IMagazine thisMag;

        MonoBehaviour thisMagGO;

        [HideInInspector]
        public Collider thisCol;

        Rigidbody thisRB;

        IBulletBehavior newBullet;

        IEnumerator lerpMovement;

        Vector3 startRotation;

        bool isLerping;

        [SerializeField]
        float timeToInsert;

        [SerializeField]
        Transform bulletLocation;

        [SerializeField]
        string[] validTags;

        private void Start()
        {
            thisMag = GetComponentInParent<IMagazine>();
            thisMag.MagDropped += ThisMag_MagDropped;

            thisCol = GetComponent<Collider>();
            thisCol.isTrigger = true;

            thisMagGO = thisMag as MonoBehaviour;

            thisRB = GetComponent<Rigidbody>();
            thisRB.constraints = RigidbodyConstraints.FreezeAll;

            if (bulletLocation != null)
            {
                bulletLocation.gameObject.SetActive(false);
            }
            startRotation = transform.localEulerAngles;
        }

        private void Update()
        {
            transform.localEulerAngles = startRotation;             // Constraining via rigidbody does not work reliably, so just set manually here
        }                                                           // Don't like it, but can't find a better solution

        private void ThisMag_MagDropped(object sender, System.EventArgs e)
        {
            thisCol.enabled = true;
        }

        private void OnTriggerStay(Collider other)
        {
            Collider bulletCol = other.GetComponent<Collider>();
            if ((newBullet = other.gameObject.GetComponent<IBulletBehavior>()) == null)
            {
                Physics.IgnoreCollision(thisCol, bulletCol);
            }

            else if (!isLerping && IsValidDrop(other.gameObject.tag) && thisMag.TryPushBullet(other.gameObject))
            {
                other.transform.parent = thisMagGO.transform;
                if (timeToInsert > 0)
                {
                    lerpMovement = LerpMovement(other.transform);
                    StartCoroutine(lerpMovement);
                    isLerping = true;
                }
                else
                {
                    if (bulletLocation != null)
                    {
                        other.transform.localPosition = bulletLocation.localPosition;
                        other.transform.localEulerAngles = bulletLocation.localEulerAngles;
                    }
                    else
                    {
                        other.gameObject.SetActive(false);
                    }
                }
                Physics.IgnoreCollision(thisCol, bulletCol);
            }
        }

        IEnumerator LerpMovement(Transform t)
        {
            float startTime = Time.time;
            
            while (Time.time < startTime + timeToInsert)
            {
                float lerpVal = (Time.time - startTime) / timeToInsert;
                t.localPosition = Vector3.Lerp(t.localPosition, bulletLocation.localPosition, lerpVal);
                t.rotation = Quaternion.Lerp(t.rotation, bulletLocation.rotation, lerpVal);
                yield return new WaitForFixedUpdate();
            }
            isLerping = false;
        }

        bool IsValidDrop(string tag)
        {
            bool ret = false;

            if (validTags.Length == 0)
            {
                ret = true;                             // In case no list is set up, allow all bullets.
            }

            for (int i = 0; i < validTags.Length; i++)
            {
                if (tag == validTags[i])
                {
                    ret = true;
                }
            }
            return ret;
        }
    }
}