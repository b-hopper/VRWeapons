using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRWeapons
{
    public class Bolt_Rotating : MonoBehaviour, IBoltActions
    {

        Weapon thisWeap;
        IEjectorActions Ejector;
        IObjectPool spentShellPool;

        public float boltLerpVal { set; get; }

        bool isFiring;

        int roundIndex;

        float amountToRotate, timeTEMP;
        
        Vector3[] rotationPositions;

        [Tooltip("Determines how much to rotate the chamber per shot automatically, based on this value."), SerializeField]
        int roundsInChamber = 6;

        [Tooltip("Specify which direction to rotate. Values should be either 0, 1, or -1."), SerializeField]
        Vector3 rotationDirection;

        [Tooltip("Transform of the cylinder object to rotate on trigger pull."), SerializeField]
        Transform cylinder;

        private void Start()
        {
            thisWeap = GetComponentInParent<Weapon>();
            amountToRotate = 360 / (float)roundsInChamber;

            SetUpRotationPositions();
            
        }

        void SetUpRotationPositions()
        {
            rotationPositions = new Vector3[roundsInChamber];
            for (int i = 0; i < roundsInChamber; i++)
            {
                rotationPositions[i] = cylinder.localEulerAngles + new Vector3((cylinder.localEulerAngles.x * (amountToRotate * (i + 1))),
                    (cylinder.localEulerAngles.y * (amountToRotate * (i + 1))), (cylinder.localEulerAngles.z * (amountToRotate * (i + 1))));
            }
        }

        public void OnTriggerPullActions(float angle)
        {
            if (cylinder != null)
            {
                if (angle <= 0.1f)
                {
                    Debug.Log("Back to zero");
                    cylinder.localEulerAngles = rotationPositions[roundIndex];
                }
                else if (angle >= 0.9f)
                {
                    Debug.Log("Full trigger pull");
                    cylinder.localEulerAngles = rotationPositions[(roundIndex + 1) % roundsInChamber];
                }
                else
                {
                    cylinder.Rotate(new Vector3(0,15,0));
                    //cylinder.Rotate(Vector3.Lerp(rotationPositions[roundIndex], rotationPositions[(roundIndex + 1) % roundsInChamber], angle));
                    //cylinder.localEulerAngles = Vector3.Lerp(rotationPositions[roundIndex], rotationPositions[(roundIndex + 1) % roundsInChamber], angle);
                }
            }
        }

        public void SetEjector(IEjectorActions newEjector)
        {
            Ejector = newEjector;
        }

        public void BoltBack()
        {
            isFiring = true;
        }

        public IBulletBehavior ChamberNewRound()
        {
            return null;
        }

        private void FixedUpdate()
        {
            if (Time.time - timeTEMP > 1)
            {
                roundIndex = (roundIndex + 1) % roundsInChamber;
                timeTEMP = Time.time;
            }
            OnTriggerPullActions(Time.time - timeTEMP);
        }

        public void ReplaceRoundWithEmptyShell(GameObject go)
        {

        }

        public void IsCurrentlyBeingManipulated(bool val)
        {

        }

        public Vector3 GetMinValue() { return Vector3.zero; }
        public Vector3 GetMaxValue() { return Vector3.zero; }
    }
}