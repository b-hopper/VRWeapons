using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


namespace VRWeapons.InteractionSystems.VRTK
{
    public class Bolt_InteractableObject : VRTK_InteractableObject, IBoltGrabber
    {
        [SerializeField, Tooltip("Optionally override bolt. Will be found automatically if not specified")]
        private GameObject boltGameObject;

        IBoltActions bolt;
        
        float boltLerpValue, lerpValue;
        
        Weapon thisWeap;

        Vector3 lastGoodPosition;

        bool hasMoved;

        Vector3 startPos, startRot;
        [SerializeField]
        public Vector3 boltClosedPosition, boltOpenPosition;

        [SerializeField]
        bool isSecondHandGrip;
        [SerializeField]
        bool allowShotgunPump = true;

        Collider thisCol;

        protected override void Awake()
        {
            //Otherwise Bolt Controller will not move back when bolt is locked
            disableWhenIdle = false;

            base.Awake();
        }

        private void Start()
        {
            bolt = boltGameObject != null ? boltGameObject.GetComponent<IBoltActions>() : transform.parent.GetComponentInChildren<IBoltActions>();
            if(bolt == null)
            {
                Debug.LogError("IBoltActions not found", this);
                enabled = false;
                return;
            }

            if (isSecondHandGrip && allowShotgunPump)
            {
                VRW_ShotgunPump pump = gameObject.AddComponent<VRW_ShotgunPump>();
                pump.boltClosedPosition = boltClosedPosition;
                pump.boltOpenPosition = boltOpenPosition;
                pump.SetBolt(bolt);
                Destroy(this);
            }

            startPos = transform.localPosition;
            startRot = transform.localEulerAngles;
            thisWeap = GetComponentInParent<Weapon>();

            thisCol = GetComponentInChildren<Collider>();

            thisWeap.IgnoreCollision(thisCol);

            if (GetComponent<Rigidbody>() != null)
            {
                GetComponent<Rigidbody>().isKinematic = true;                                                                   // Don't want the manipulator to fall down off the bolt!
            }
        }

        protected override void Update()
        {
            float oldLerpValue = lerpValue;

            if (IsGrabbed())
            {
                bolt.IsCurrentlyBeingManipulated(true);
                hasMoved = true;
                lerpValue = VRWControl.V3InverseLerp(boltClosedPosition, boltOpenPosition, transform.localPosition);
                ClampControllerToTrack();
                bolt.boltLerpVal = lerpValue;
            }

            else if (hasMoved)
            {
                bolt.IsCurrentlyBeingManipulated(false);
                hasMoved = false;
                transform.localPosition = lastGoodPosition;
            }

            else
            {
                lerpValue = bolt.boltLerpVal;
            }

            if (lerpValue != oldLerpValue)                                                                                      // Moves the manipulator to be in the correct position (with the bolt object). Only 
            {                                                                                                                   // moves it if the lerp value has changed - doesn't need adjustment otherwise.
                transform.localPosition = Vector3.Lerp(boltClosedPosition, boltOpenPosition, lerpValue);
            }

            base.Update();

            transform.localEulerAngles = startRot;                                                                              // Rotation isn't an issue, but to avoid making the collider feel like it's in a 
        }                                                                                                                       // weird position, make sure the rotation is set back to how it originally was.
                                                                                                                                // Before this, the bolt manipulator was rotated with the controller.
        public Collider GetInteractableCollider()
        {
            return thisCol;
        }

        void ClampControllerToTrack()
        {
            transform.localPosition = Vector3.Lerp(boltClosedPosition, boltOpenPosition, lerpValue);
            lastGoodPosition = transform.localPosition;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(boltGameObject != null && boltGameObject.GetComponent<IBoltActions>() == null)
            {
                boltGameObject = null;
                Debug.LogError("boltGameObject must contain IBoltActions component");
            }
        } 
#endif
    }
}