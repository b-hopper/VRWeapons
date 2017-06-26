using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    [System.Serializable]
    public class Magazine : MonoBehaviour, IMagazine
    {
        Rigidbody rb;

        [SerializeField]
        List<IBulletBehavior> RoundsInMag;

        int index;

        [SerializeField]
        public GameObject[] rounds;

        private void Start()
        {
            RoundsInMag = new List<IBulletBehavior>(rounds.Length);
            rb = GetComponent<Rigidbody>();

            PopulateAllSlotsInList();   // This method is pretty expensive depending on how many rounds there are,
        }                               // but happens on loading the scene so it should be fine.
        
        public bool PushBullet(IBulletBehavior newRound)
        {
            bool val = false;
            Debug.Log("RoundsInMag Count: " + RoundsInMag.Count + ", rounds.length: " + rounds.Length);
            if (RoundsInMag.Count < rounds.Length)
            {
                RoundsInMag.Insert(index, newRound);
                val = true;
                rounds[index].GetComponent<Collider>().enabled = false;
                index++;
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
                index--;
            }
            return tmp;
        }

        public void MagIn(Weapon weap)
        {
            weap.Magazine = this;
            weap.PlaySound(Weapon.AudioClips.MagIn);
            transform.parent = weap.transform;
            rb.isKinematic = true;
            Debug.Log("Mag In: " + weap);
        }

        public void MagOut(Weapon weap)
        {
            weap.Magazine = null;
            weap.PlaySound(Weapon.AudioClips.MagOut);
            transform.parent = null;
            rb.isKinematic = false;
            Debug.Log("Mag out: " + weap);
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
                if (list[i].GetComponent<Collider>() != null)
                {
                    list[i].GetComponent<Collider>().enabled = false;   // Colliders are causing problems with ejection. Disable them...
                }
            }
            list[list.Length - 1].GetComponent<Collider>().enabled = true;  // ...except for the last one, which is the top round in the magazine.
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
    }
}