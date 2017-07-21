using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRWeapons.InteractionSystems.VRTK
{
    [CustomEditor(typeof(Bolt_InteractableObject)), System.Serializable]

    public class Bolt_InteractableObjectEditor : Editor
    {
        bool togglePosition;

        public override void OnInspectorGUI()
        {
            Bolt_InteractableObject boltIntObj = (Bolt_InteractableObject)target;

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

        }

    }
}