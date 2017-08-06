using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    [System.Serializable]
    public class Magazine : MonoBehaviour, IMagazine
    {
        public event EventHandler BulletPushed;
        public event EventHandler BulletPopped;

        VRWControl control;

        public event EventHandler MagDropped;

        Rigidbody rb;

        [SerializeField]
        List<IBulletBehavior> RoundsInMag;

        Collider[] roundColliders;

        Collider thisCol;

        Weapon currentWeap;

        int index;

        [SerializeField]
        public GameObject[] rounds;

        [Tooltip("If toggled, magazine is able to be removed from the weapon. Turn off if weapon is using an internal magazine."), SerializeField]
        bool canBeDetached = true;
        
        public bool CanMagBeDetached { get { return canBeDetached; } set { canBeDetached = value; } }

        //Needs to run on Awake so RoundsInMag will be ready before FeedRound is called
        private void Awake()
        {
            control = FindObjectOfType<VRWControl>();
            roundColliders = new Collider[rounds.Length];
            RoundsInMag = new List<IBulletBehavior>(rounds.Length);
            rb = GetComponent<Rigidbody>();
            thisCol = GetComponent<Collider>();
            if (rb == null)
            {
                Debug.LogWarning("Rigidbody not found", this);
            }

            PopulateAllSlotsInList();   // This method is pretty expensive depending on how many rounds there are,
                                        // but happens on loading the scene so it should be fine.
        }
        
        public bool PushBullet(GameObject newRound)
        {
            IBulletBehavior newBulletBehavior = newRound.GetComponent<IBulletBehavior>();
            bool val = false;
            if (newBulletBehavior != null)
            {
                if (RoundsInMag.Count < rounds.Length)
                {
                    index++;
                    RoundsInMag.Insert(index, newBulletBehavior);
                    val = true;

                    if (index > 0 && rounds[index - 1] != null)
                    {
                        rounds[index - 1].GetComponent<Collider>().enabled = false;
                    }
                    rounds[index] = newRound;
                    OnBulletPushed();
                }
            }
            return val;
        }

        public bool PopBullet()
        {
            bool val = false;
            if (RoundsInMag.Remove(RoundsInMag[index]))
            {
                val = true;
                index--;
                rounds[index].GetComponent<Collider>().enabled = true;
                OnBulletPopped();
            }
            return val;
        }

        public Rigidbody GetRoundRigidBody()
        {
            if (index >= 0)
            {
                return rounds[index].GetComponent<Rigidbody>();
            }
            return null;
        }

        public Transform GetRoundTransform()
        {
            if (index >= 0)
            {
                return rounds[index].transform;
            }
            return null;
        }

        public IBulletBehavior FeedRound()
        {
            IBulletBehavior tmp = null;
            if (index >= 0)
            {
                tmp = RoundsInMag[index];
                RoundsInMag.Remove(tmp);
                if (roundColliders[index] != null && currentWeap != null)
                {
                    currentWeap.IgnoreCollision(roundColliders[index]);
                }
                rounds[index] = null;
                index--;
                //TODO: Why doesn't FeedRound call PopBullet?
                OnBulletPopped();
            }
            return tmp;
        }

        public int GetCurrentRoundCount()
        {
            return index + 1;
        }

        public void MagIn(Weapon weap)
        {
            weap.Magazine = this;
            weap.PlaySound(Weapon.AudioClips.MagIn);
            transform.parent = weap.transform;
            Collider col;
            if (rounds[index] != null && (col = rounds[index].GetComponent<Collider>()) != null)
            {
                weap.IgnoreCollision(col);
            }
            currentWeap = weap;
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }

        public void MagOut(Weapon weap)
        {
            if (canBeDetached)
            {
                currentWeap = null;
                weap.Magazine = null;
                weap.PlaySound(Weapon.AudioClips.MagOut);
                transform.parent = null;
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
                OnMagDropped();
            }
        }

        public GameObject FindRoundAtIndex(int idx)
        {
            return rounds[idx];
        }

        void PopulateAllSlotsInList()
        {
            int offset = 0;
            for (int j = 0; j < rounds.Length; j++)
            {

                if (rounds[j] == null)
                {
                    offset++;
                }
                else if (rounds[j].GetComponent<IBulletBehavior>() == null)
                {
                    Debug.LogError("No IBulletBehavior found on " + rounds[j] + ". No round inserted at position " + j + ".");
                    offset++;
                }
                else
                {
                    RoundsInMag.Insert(j - offset, rounds[j].GetComponent<IBulletBehavior>());
                    if (rounds[j].GetComponent<Collider>() != null)
                    {
                        roundColliders[j - offset] = rounds[j].GetComponent<Collider>();
                    }
                    
                    index = j - offset;
                }
            }
            FixRoundsList(rounds);  // Fix GameObjects list to match IBulletBehavior list. GameObjects list is used for ejector RigidBodies and Transforms.
        }

        void FixRoundsList(GameObject[] list)
        {
            int offset = 0;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == null)
                {
                    offset++;
                }
                if (i + offset > list.Length - 1)
                {
                    list[i] = null;
                }
                else
                {
                    list[i] = list[i + offset];
                }
                if (list[i] != null && list[i].GetComponent<Collider>() != null)
                {
                    list[i].GetComponent<Collider>().enabled = false;   // Colliders are causing problems with ejection. Disable them...
                }
            }
            list[list.Length - 1 - offset].GetComponent<Collider>().enabled = true;  // ...except for the last one, which is the top round in the magazine.
        }

        void ReportRoundsInMag()
        {
            int j = 0;
            foreach (IBulletBehavior i in RoundsInMag)
            {
                Debug.Log("Round in position " + j + ": " + RoundsInMag[j]);
                j++;
            }
        }

        private void OnMagDropped()
        {
            if(MagDropped != null)
            {
                MagDropped(this, EventArgs.Empty);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.isTrigger && collision.gameObject.GetComponentInParent<Weapon>() != null)
            {
                Physics.IgnoreCollision(collision.collider, thisCol);
            }
        }

        private void OnBulletPushed()
        {
            if(BulletPushed != null)
            {
                BulletPushed.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnBulletPopped()
        {
            if (BulletPopped != null)
            {
                BulletPopped.Invoke(this, EventArgs.Empty);
            }
        }
    }
}