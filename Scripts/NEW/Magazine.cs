using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRWeapons
{
    [System.Serializable]
    public class Magazine : MonoBehaviour, IMagazine
    {

        [SerializeField]
        List<IBulletBehavior> RoundsInMag;
        int index;

        [SerializeField]
        public GameObject[] rounds;

        private void Start()
        {
            RoundsInMag = new List<IBulletBehavior>(rounds.Length);

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
            }
            return val;
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
        }

        public void MagOut(Weapon weap)
        {
            weap.Magazine = null;
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