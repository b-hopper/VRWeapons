using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace VRWeapons
{
    [CustomEditor(typeof(Magazine)), System.Serializable]

    public class MagazineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Magazine mag = (Magazine)target;

            if (GUILayout.Button("Fill open slots with round in slot 0"))
            {
                Undo.RecordObject(mag, "Fill open slots with round in slot 0");
                if (mag.rounds[0].GetComponent<IBulletBehavior>() != null)
                {
                    for (int i = 0; i < mag.rounds.Length; i++)
                    {
                        if (mag.rounds[i] == null)
                        {
                            mag.rounds[i] = Instantiate(mag.rounds[0]);
                            mag.rounds[i].transform.parent = mag.transform;
                        }
                    }
                }
                else
                {
                    Debug.LogError("No IBulletBehavior found on " + mag.rounds[0] + ". Try again with a valid bullet object.");
                }
            }
            EditorGUILayout.LabelField("Rounds are fed in reverse order - ");
            EditorGUILayout.LabelField("Element 0 is the last round in the mag.");
            base.OnInspectorGUI();            
        } 
    }
}