using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace VRWeapons
{
    [CustomEditor(typeof(Bolt)), System.Serializable]

    public class BoltEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Bolt bolt = (Bolt)target;

            base.OnInspectorGUI();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set Bolt Group\nStart Position", "Set bolt group's fully closed position, and press this button."), GUILayout.Width(125)))
            {
                if (bolt.boltGroup != null)
                {
                    bolt.GroupStartPosition = bolt.boltGroup.localPosition;
                }
                else
                {
                    Debug.LogError("No Bolt Group assigned. Please assign a Bolt Group before trying to assign Bolt Group positions.");
                }
            }
            if (GUILayout.Button(new GUIContent("Set Bolt Group\nEnd Position", "Set bolt group's fully open position, and press this button.\n\nWill assign end position, then snap back to start position."), GUILayout.Width(125)))
            {
                if (bolt.boltGroup != null)
                {
                    bolt.GroupEndPosition = bolt.boltGroup.localPosition;
                    bolt.boltGroup.localPosition = bolt.GroupStartPosition;                    
                }
                else
                {
                    Debug.LogError("No Bolt Group assigned. Please assign a Bolt Group before trying to assign Bolt Group positions.");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set bolt\nStart Position", "Set bolt's fully closed position, and press this button."), GUILayout.Width(125)))
            {
                if (bolt.bolt != null)
                {
                    bolt.BoltStartPosition = bolt.bolt.localPosition;
                }
                else
                {
                    Debug.LogError("No bolt assigned. Please assign a bolt before trying to assign bolt positions.");
                }
            }
            if (GUILayout.Button(new GUIContent("Set bolt\nEnd Position", "Set bolt's fully open position, and press this button.\n\nWill assign end position, then snap back to start position."), GUILayout.Width(125)))
            {
                if (bolt.bolt != null)
                {
                    bolt.BoltEndPosition = bolt.bolt.localPosition;
                    bolt.bolt.localPosition = bolt.BoltStartPosition;
                }
                else
                {
                    Debug.LogError("No bolt assigned. Please assign a bolt before trying to assign bolt positions.");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set bolt rotation\nStart Position", "Set bolt's beginning rotation (if bolt rotates) and press this button."), GUILayout.Width(125)))
            {
                if (bolt.bolt != null)
                {
                    bolt.BoltRotationStart = bolt.bolt.localEulerAngles;
                }
                else
                {
                    Debug.LogError("No bolt assigned. Please assign a bolt before trying to assign bolt rotation.");
                }
            }
            if (GUILayout.Button(new GUIContent("Set bolt rotation\nEnd Position", "Set bolt's end rotation (if bolt rotates) and press this button.\n\nWill assign end rotation, then snap back to start rotation."), GUILayout.Width(125)))
            {
                if (bolt.bolt != null)
                {
                    bolt.BoltRotationEnd = bolt.bolt.localEulerAngles;
                    bolt.bolt.localEulerAngles = bolt.BoltRotationStart;
                }
                else
                {
                    Debug.LogError("No bolt assigned. Please assign a bolt before trying to assign bolt rotation.");
                }
            }
            GUILayout.EndHorizontal();
        } 
    }
}