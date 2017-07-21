using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRWeapons
{
    [CustomEditor(typeof(Bolt)), System.Serializable]

    public class BoltEditor : Editor
    {
        bool toggleBoltPosition, toggleBoltGroupPosition, toggleBoltRotation;
        public override void OnInspectorGUI()
        {
            Bolt bolt = (Bolt)target;

            base.OnInspectorGUI();

            if (GUILayout.Button(new GUIContent("Assign Round Snap Location", "Press this button once you have a round positioned correctly on the bolt face.")))
            {
                if (bolt.chamberedRoundSnapT != null)
                {
                    if (bolt.bolt != null)
                    {
                        bolt.chamberedRoundSnapT.parent = bolt.bolt;
                        bolt.chamberedRoundSnapT.gameObject.SetActive(false);
                    }
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set Group\nStart Position", "Set bolt group's fully closed position, and press this button."), GUILayout.Width(90)))
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
            if (GUILayout.Button(new GUIContent("Set Group\nEnd Position", "Set bolt group's fully open position, and press this button.\n\nWill assign end position, then snap back to start position."), GUILayout.Width(90)))
            {
                if (bolt.boltGroup != null)
                {
                    bolt.GroupEndPosition = bolt.boltGroup.localPosition;               
                }
                else
                {
                    Debug.LogError("No Bolt Group assigned. Please assign a Bolt Group before trying to assign Bolt Group positions.");
                }
            }
            if (GUILayout.Button(new GUIContent("Toggle\nposition", "Toggle between start and end positions."), GUILayout.Width(70)))
            {
                if (toggleBoltGroupPosition)
                {
                    if (bolt.boltGroup != null)
                    {
                        bolt.boltGroup.localPosition = bolt.GroupStartPosition;
                        toggleBoltGroupPosition = !toggleBoltGroupPosition;
                    }
                }
                else
                {
                    if (bolt.boltGroup != null)
                    {
                        bolt.boltGroup.localPosition = bolt.GroupEndPosition;
                        toggleBoltGroupPosition = !toggleBoltGroupPosition;
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set bolt\nStart Position", "Set bolt's fully closed position, and press this button."), GUILayout.Width(90)))
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
            if (GUILayout.Button(new GUIContent("Set bolt\nEnd Position", "Set bolt's fully open position, and press this button."), GUILayout.Width(90)))
            {
                if (bolt.bolt != null)
                {
                    bolt.BoltEndPosition = bolt.bolt.localPosition;
                }
                else
                {
                    Debug.LogError("No bolt assigned. Please assign a bolt before trying to assign bolt positions.");
                }
            }
            if (GUILayout.Button(new GUIContent("Toggle\nposition", "Toggle between start and end positions."), GUILayout.Width(70)))
            {
                if (toggleBoltPosition)
                {
                    if (bolt.bolt != null)
                    {
                        bolt.bolt.localPosition = bolt.BoltStartPosition;
                        toggleBoltPosition = !toggleBoltPosition;
                    }
                }
                else
                {
                    if (bolt.bolt != null)
                    {
                        bolt.bolt.localPosition = bolt.BoltEndPosition;
                        toggleBoltPosition = !toggleBoltPosition;
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set rotation\nStart Position", "Set bolt's beginning rotation (if bolt rotates) and press this button."), GUILayout.Width(90)))
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
            if (GUILayout.Button(new GUIContent("Set rotation\nEnd Position", "Set bolt's end rotation (if bolt rotates) and press this button."), GUILayout.Width(90)))
            {
                if (bolt.bolt != null)
                {
                    bolt.BoltRotationEnd = bolt.bolt.localEulerAngles;
                }
                else
                {
                    Debug.LogError("No bolt assigned. Please assign a bolt before trying to assign bolt rotation.");
                }
            }
            if (GUILayout.Button(new GUIContent("Toggle\nrotation", "Toggle between start and end rotations."), GUILayout.Width(70)))
            {
                if (toggleBoltRotation)
                {
                    if (bolt.bolt != null)
                    {
                        bolt.bolt.localEulerAngles = bolt.BoltRotationStart;
                        toggleBoltRotation = !toggleBoltRotation;
                    }
                }
                else
                {
                    if (bolt.bolt != null)
                    {
                        bolt.bolt.localEulerAngles = bolt.BoltRotationEnd;
                        toggleBoltRotation = !toggleBoltRotation;
                    }
                }
            }
            GUILayout.EndHorizontal();
        } 
    }
}