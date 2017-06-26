namespace VRWeapons
{
    using UnityEngine;
    using UnityEditor;
    public class VRW_ObjectSetup : EditorWindow
    {



        [MenuItem("Component/VRWeapons/Set up new Weapon")]
        private static void Init()
        {
            VRW_ObjectSetup window = (VRW_ObjectSetup)EditorWindow.GetWindow(typeof(VRW_ObjectSetup));
            window.minSize = new Vector2(300f, 300f);
            window.maxSize = new Vector2(300f, 400f);

            window.autoRepaintOnSceneChange = true;
            window.titleContent.text = "Weapon setup";
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Weapon builder goes here");
        }
    }
}