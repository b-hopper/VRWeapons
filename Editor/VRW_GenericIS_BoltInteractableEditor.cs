using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRWeapons.InteractionSystems.Generic
{
    [CustomEditor(typeof(VRW_GenericIS_BoltInteractable)), System.Serializable]

    public class VRW_GenericIS_BoltInteractableEditor : Editor
    {
        bool togglePosition;

        public override void OnInspectorGUI()
        {
            VRW_GenericIS_BoltInteractable boltIntObj = (VRW_GenericIS_BoltInteractable)target;

            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set Start\nPosition", "Set the start position of the bolt grab point."), GUILayout.Width(90)))
            {
                boltIntObj.boltClosedPosition = boltIntObj.transform.localPosition;
            }
            if (GUILayout.Button(new GUIContent("Set End\nPosition", "Set the end position of the bolt grab point."), GUILayout.Width(90)))
            {
                boltIntObj.boltOpenPosition = boltIntObj.transform.localPosition;
            }
            if (GUILayout.Button(new GUIContent("Toggle\nPosition", "Toggle between start and end positions."), GUILayout.Width(70)))
            {
                if (togglePosition)
                {
                    boltIntObj.transform.localPosition = boltIntObj.boltClosedPosition;
                    togglePosition = !togglePosition;
                }
                else
                {
                    boltIntObj.transform.localPosition = boltIntObj.boltOpenPosition;
                    togglePosition = !togglePosition;
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button(new GUIContent("Save Bolt Controller")))
            {
                boltIntObj.GetComponent<MeshRenderer>().enabled = false;
                boltIntObj.transform.localPosition = boltIntObj.boltClosedPosition;
            }
        }
    }
}