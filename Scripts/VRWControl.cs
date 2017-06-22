using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;


namespace VRWeapons
{
    public class VRWControl : MonoBehaviour
    {
        private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
        float VRWVersion = 2.0f;
        public bool VRTKMode;
        [Header("Show controller when weapon grabbed?")]
        public bool showDeviceMainHand;
        [Header("Show second-hand grip controller?")]
        public bool showDeviceOffHand;
        [Header("Second-hand grip button must be held down?")]
        public bool mustHoldDownGrip;
        bool allowSwappingHands;
        /*public Weapon GrabOnStart;
        [Header("Which hand starts with weapon?")]
        [Range(0,1)]
        public int WeaponHand;*/
        Weapon[] WeaponsInScene;
        //magazine[] MagsInScene;
        public float throwForce = 1;
        //public float debugTimeScale = 1;
        [Header("Gunshot layer mask")]
        public LayerMask shotMask;
        Hand[] hands;
        JointDrive jDrive, jZDrive;
        
        struct wpnInfo
        {
            public Weapon HeldWeapon;
            //public magazine HeldWeaponMagazine;
            public SteamVR_TrackedObject ActiveController;
        }

        wpnInfo[] heldWeapon;

        public Valve.VR.EVRButtonId grabButton = Valve.VR.EVRButtonId.k_EButton_Grip;
        public Valve.VR.EVRButtonId fireButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
        public Valve.VR.EVRButtonId dropMag = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;



        void Awake()                        // Sets up colliders, adds viveCharControl.cs, and sets controller layers if VRTK mode is on
        {
            hands = new Hand[2];

            jDrive.positionSpring = 5000;
            jDrive.positionDamper = 100;
            jDrive.maximumForce = Mathf.Infinity;

            int i = 0;
            heldWeapon = new wpnInfo[2];
            if (!VRTKMode)
            {
                foreach (Hand obj in FindObjectsOfType<Hand>())
                {
                    if (obj.gameObject.GetComponent<Rigidbody>() == null)
                    {
                        obj.gameObject.AddComponent<Rigidbody>();
                        obj.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    if (obj.gameObject.GetComponent<ConfigurableJoint>() == null)
                    {
                        ConfigurableJoint jt = obj.gameObject.AddComponent<ConfigurableJoint>();
                        jt.anchor = new Vector3(0, 0, -0.055f);
                        jt.autoConfigureConnectedAnchor = false;
                        jt.projectionMode = JointProjectionMode.PositionAndRotation;
                        jt.projectionDistance = 0.25f;

                        jt.xDrive = jDrive;
                        jt.yDrive = jDrive;
                        jt.zDrive = jDrive;
                    }

                    hands[i] = obj;

                    i++;

                    obj.gameObject.layer = LayerMask.NameToLayer("VRWControllers");
                }
            }
            else
            {
                //StartCoroutine(SetUpControllersForVRTK());
            }



            for (int j = 0; j <= 31; j++)
            {
                if (j != LayerMask.NameToLayer("VRWControllers"))
                {
                    Physics.IgnoreLayerCollision(LayerMask.NameToLayer("HeldWeapon"), j);
                }
            }

            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Weapon"), LayerMask.NameToLayer("Weapon"));

            /*if (GrabOnStart != null)
            {
                //StartCoroutine(StartWithWeapon());
            }*/

            Debug.Log("VRWeapons v" + VRWVersion);

        }

        void FixedUpdate()
        {

            //Time.timeScale = debugTimeScale;


        }
        /*
        IEnumerator SetUpControllersForVRTK()
        {
            yield return new WaitForSeconds(0.1f);
            int ctrlCount = 0;
            while (ctrlCount < 2)
            {
                ctrlCount = 0;
                foreach (SteamVR_TrackedObject obj in FindObjectsOfType<SteamVR_TrackedObject>())
                {
                    if (obj.gameObject.GetComponent<Rigidbody>() == null)
                    {
                        obj.gameObject.AddComponent<Rigidbody>();
                        obj.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    if (obj.gameObject.GetComponent<ConfigurableJoint>() == null)
                    {
                        ConfigurableJoint jt = obj.gameObject.AddComponent<ConfigurableJoint>();
                        jt.anchor = new Vector3(0, 0, -0.055f);
                        jt.autoConfigureConnectedAnchor = false;
                        jt.projectionMode = JointProjectionMode.PositionAndRotation;
                        jt.projectionDistance = 0.25f;
                        jt.xDrive = jDrive;
                        jt.yDrive = jDrive;
                        jt.zDrive = jDrive;
                    }
                    else
                    {
                        ConfigurableJoint jt = obj.GetComponent<ConfigurableJoint>();
                        jt.anchor = new Vector3(0, 0, -0.055f);
                        jt.autoConfigureConnectedAnchor = false;
                        jt.projectionMode = JointProjectionMode.PositionAndRotation;
                        jt.projectionDistance = 0.25f;
                        jt.xDrive = jDrive;
                        jt.yDrive = jDrive;
                        jt.zDrive = jDrive;
                    }
                    if (obj.gameObject.GetComponent<VRWFixedJointConnection>() == null)
                    {
                        obj.gameObject.AddComponent<VRWFixedJointConnection>();
                    }
                    obj.gameObject.layer = LayerMask.NameToLayer("VRWControllers");
                    if (obj.GetComponent<Collider>() == null)
                    {
                        //Debug.LogError("No collider found on controller " + obj);
                    }
                    if (obj.GetComponent<Collider>() != null && !obj.GetComponent<Collider>().isTrigger)
                    {
                        Debug.LogError("Collider on controller " + obj + " is not a trigger. VRWeapons requires triggers on controller colliders in VRTK mode.");
                    }
                    if (obj.gameObject.GetComponent<Rigidbody>() != null && obj.gameObject.GetComponent<ConfigurableJoint>() != null)
                    {
                        ctrlCount++;
                    }
                }
                yield return new WaitForSeconds(2);
            }
        }*/

        /*IEnumerator StartWithWeapon()
        {
            yield return new WaitForSeconds(1);
            GrabOnStart.GetComponent<VRWInteractableWeapon>().handJoint = hands[WeaponHand].GetComponent<ConfigurableJoint>();
            hands[WeaponHand].AttachObject(GrabOnStart.gameObject, attachmentFlags);

            GrabOnStart.gameObject.transform.parent = hands[WeaponHand].transform;
            GrabOnStart.gameObject.transform.localEulerAngles = GrabOnStart.adjustRotation;
            GrabOnStart.GetComponent<Rigidbody>().isKinematic = false;
            GrabOnStart.joint.connectedAnchor = hands[WeaponHand].GetComponent<ConfigurableJoint>().anchor;
            GrabOnStart.joint.connectedBody = hands[WeaponHand].GetComponent<Rigidbody>();
            SetAllJointAxesLocked(GrabOnStart.joint);
            SetLayerInObjectAndChildren(GrabOnStart.gameObject, LayerMask.NameToLayer("HeldWeapon"));
            GrabOnStart.transform.parent = null;
            GrabOnStart.held = true;
            GrabOnStart.holdingDevice = hands[WeaponHand].gameObject;
            GrabOnStart.firingController = hands[WeaponHand];
            hands[WeaponHand].HoverLock(GetComponent<Interactable>());
            if (!showDeviceMainHand)
            {
                GrabOnStart.GetComponent<VRWInteractableWeapon>().disappearingHand = hands[WeaponHand].GetComponentInChildren<SpawnRenderModel>().gameObject;
                GrabOnStart.GetComponent<VRWInteractableWeapon>().disappearingHandHighlight = hands[WeaponHand].GetComponentInChildren<ControllerHoverHighlight>().gameObject;
                GrabOnStart.GetComponent<VRWInteractableWeapon>().disappearingHand.SetActive(false);
                GrabOnStart.GetComponent<VRWInteractableWeapon>().disappearingHandHighlight.SetActive(false);
            }

            yield break;
        }*/

        /*public void SetHeldWeapon(Weapon wpn, magazine mag, SteamVR_TrackedObject cntrl)
        {
            for (int i = 0; i < 2; i++)
            {
                if (cntrl == heldWeapon[i].ActiveController)
                {
                    heldWeapon[i].HeldWeapon = wpn;
                    heldWeapon[i].HeldWeaponMagazine = mag;
                }
            }
        }

        public VRTK.Weapon GetHeldWeapon(SteamVR_TrackedObject cntrl)
        {
            for (int i = 0; i < 2; i++)
            {
                if (cntrl == heldWeapon[i].ActiveController)
                {
                    return heldWeapon[i].HeldWeapon;
                }
            }
            return null;
        }

        public int GetMagAmmo(SteamVR_TrackedObject cntrl)
        {
            for (int i = 0; i < 2; i++)
            {
                if (cntrl == heldWeapon[i].ActiveController)
                {
                    return heldWeapon[i].HeldWeaponMagazine.ammo;
                }
            }
            return 0;
        }

        public interface WeaponInfo
        {

        }*/


        #region Useful functions

        public void WeaponJointSetupOnGrip(ConfigurableJoint jt)
        {

            jt.xMotion = ConfigurableJointMotion.Locked;
            jt.yMotion = ConfigurableJointMotion.Locked;
            jt.zMotion = ConfigurableJointMotion.Locked;

            jt.angularXMotion = ConfigurableJointMotion.Free;
            jt.angularYMotion = ConfigurableJointMotion.Free;
            jt.angularZMotion = ConfigurableJointMotion.Locked;
        }

        public void DeviceJointSetupOnGrip(ConfigurableJoint jt)
        {
            jt.xMotion = ConfigurableJointMotion.Locked;
            jt.yMotion = ConfigurableJointMotion.Locked;
            jt.zMotion = ConfigurableJointMotion.Locked;

            jt.angularXMotion = ConfigurableJointMotion.Free;
            jt.angularYMotion = ConfigurableJointMotion.Free;
            jt.angularZMotion = ConfigurableJointMotion.Free;
        }

        public void SetAllJointAxesFree(ConfigurableJoint jt)
        {
            jt.xMotion = ConfigurableJointMotion.Free;
            jt.yMotion = ConfigurableJointMotion.Free;
            jt.zMotion = ConfigurableJointMotion.Free;

            jt.angularXMotion = ConfigurableJointMotion.Free;
            jt.angularYMotion = ConfigurableJointMotion.Free;
            jt.angularZMotion = ConfigurableJointMotion.Free;
        }

        public void SetAllJointAxesLocked(ConfigurableJoint jt)
        {
            jt.xMotion = ConfigurableJointMotion.Locked;
            jt.yMotion = ConfigurableJointMotion.Locked;
            jt.zMotion = ConfigurableJointMotion.Locked;

            jt.angularXMotion = ConfigurableJointMotion.Locked;
            jt.angularYMotion = ConfigurableJointMotion.Locked;
            jt.angularZMotion = ConfigurableJointMotion.Locked;
        }

        public void SetLayerInObjectAndChildren(GameObject item, int layer)
        {
            int children = item.transform.childCount;
            if (children != 0)
            {
                for (int i = 0; i < children; i++)
                {
                    SetLayerInObjectAndChildren(item.transform.GetChild(i).gameObject, layer);
                }
            }

            if (item.tag != "ReloadPoint")
            {
                item.layer = layer;
            }
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }

        #endregion
    }
}