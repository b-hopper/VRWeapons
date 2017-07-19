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

        float timeTEMP;

        int roundIndex;

        Vector3 amountToRotate;
        
        Vector3[] rotationPositions;

        [Tooltip("Determines how much to rotate the chamber per shot automatically, based on this value."), SerializeField]
        int roundsInChamber = 6;

        [Tooltip("Specify which direction to rotate. Values should be either 0, 1, or -1."), SerializeField]
        Vector3 rotationDirection;

        [Tooltip("Transform of the cylinder object to rotate on trigger pull."), SerializeField]
        Transform cylinder;

        private void Start()
        {
            //Time.timeScale = 0.1f;
            thisWeap = GetComponentInParent<Weapon>();
            amountToRotate = new Vector3((360 / (float)roundsInChamber) * rotationDirection.x, (360 / (float)roundsInChamber) * rotationDirection.y, (360 / (float)roundsInChamber) * rotationDirection.z);
            //Debug.Log(amountToRotate);

            SetUpRotationPositions();
            
        }

        void SetUpRotationPositions()
        {
            rotationPositions = new Vector3[roundsInChamber];
            for (int i = 0; i < roundsInChamber; i++)
            {
                rotationPositions[i] = cylinder.localEulerAngles + new Vector3((cylinder.localEulerAngles.x + (amountToRotate.x * (i + 1))),
                    (cylinder.localEulerAngles.y + (amountToRotate.y * (i + 1))), (cylinder.localEulerAngles.z + (amountToRotate.z * (i + 1))));

                //Debug.Log(rotationPositions[i]);
            }
            
        }

        public void OnTriggerPullActions(float angle)
        {
            if (cylinder != null)
            {
                if (angle <= 0.1f)
                {
                    //Debug.Log("Back to zero");
                    cylinder.localEulerAngles = rotationPositions[roundIndex];
                }
                else if (angle >= 0.9f)
                {
                    //Debug.Log("Full trigger pull");
                    cylinder.localEulerAngles = rotationPositions[(roundIndex + 1) % roundsInChamber];
                }
                else
                {
                    //cylinder.Rotate(Vector3.Lerp(rotationPositions[roundIndex], rotationPositions[(roundIndex + 1) % roundsInChamber], angle));
                    cylinder.localEulerAngles = Vector3.Lerp(rotationPositions[roundIndex], rotationPositions[(roundIndex + 1) % roundsInChamber], angle);
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
            timeTEMP = Time.time % 1;

            //Debug.Log(timeTEMP);
            
            OnTriggerPullActions(timeTEMP);
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