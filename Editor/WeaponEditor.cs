using UnityEngine;
using System.Collections;
using UnityEditor;
using Valve.VR.InteractionSystem;

[CustomEditor (typeof(VRTK.Weapon))]


public class WeaponEditor : Editor
{
	bool isCodeSlide, isCodeTrigger, showTransformAdjust, showSounds;

#region Layer Setup

	public static void CheckTags (string[] tagNames)
	{
		SerializedObject manager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset"));
		SerializedProperty tagsProp = manager.FindProperty ("tags");
        

		foreach (string name in tagNames) {
            
			bool found = false;
			for (int i = 0; i < tagsProp.arraySize; i++) {
				SerializedProperty t = tagsProp.GetArrayElementAtIndex (i);
				if (t.stringValue.Equals (name)) {
					found = true;
					break;
				}
			}
            
			if (!found) {
				tagsProp.InsertArrayElementAtIndex (0);
				SerializedProperty n = tagsProp.GetArrayElementAtIndex (0);
				n.stringValue = name;
			}
		}
        
		manager.ApplyModifiedProperties ();
	}


	public static void CheckLayers (string layerName)
	{
		SerializedObject manager = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset"));
#if !UNITY_4
		SerializedProperty layersProp = manager.FindProperty ("layers");
#endif
		bool found = false;
		for (int i = 0; i <= 31; i++) {
#if UNITY_4
            string nm = "User Layer " + i;
            SerializedProperty sp = manager.FindProperty(nm);
#else
			SerializedProperty sp = layersProp.GetArrayElementAtIndex (i);
#endif
			if (sp != null && sp.stringValue == layerName) {
				found = true;
				break;
			}
		}
		if (!found) {
			SerializedProperty slot = null;
			for (int i = 8; i <= 31; i++) {
#if UNITY_4
                string nm = "User Layer " + i;
                SerializedProperty sp = manager.FindProperty(nm);
#else
				SerializedProperty sp = layersProp.GetArrayElementAtIndex (i);
#endif
				if (sp != null && string.IsNullOrEmpty (sp.stringValue)) {
					slot = sp;
					break;
				}
			}
			if (slot != null) {
				slot.stringValue = layerName;
			} else {
				Debug.LogError ("Could not find an open slot for " + layerName + " layer");
			}

		}

		manager.ApplyModifiedProperties ();

		string[] tags = new string[4] { "ReloadPoint", "Magazine", "GripPoint", "AttachPoint" };

		CheckTags (tags);
            
	}

	public static void AssignLayers ()
	{
        VRTK.Weapon[] tmp = FindObjectsOfType<VRTK.Weapon> ();
		magazine[] mag = FindObjectsOfType<magazine> ();
        ReloadPoint[] rld = FindObjectsOfType<ReloadPoint>();
		foreach (VRTK.Weapon wpn in tmp) {
			if (wpn.gameObject.layer != LayerMask.NameToLayer ("Weapon")) {
				wpn.gameObject.layer = LayerMask.NameToLayer ("Weapon");
			}
            foreach (ReloadPoint point in rld)
            {
                point.tag = "ReloadPoint";
                point.gameObject.layer = LayerMask.NameToLayer("Default");
            }
			
		}
		foreach (magazine mg in mag) {
			mg.gameObject.layer = LayerMask.NameToLayer ("Weapon");
			mg.gameObject.tag = "Magazine";
		}
	}

    #endregion

#region Main inspector view
    public override void OnInspectorGUI ()
	{
        VRTK.Weapon weaponScript = (VRTK.Weapon)target;
		//base.OnInspectorGUI();
		Undo.RecordObject (weaponScript, "Weapon Modifications");
		if (GUILayout.Button ("Set up layers and tags")) {
			CheckLayers ("Weapon");
            CheckLayers("HeldWeapon");
            CheckLayers("VRWControllers");
            AssignLayers ();
            Debug.Log("Layers and tags set up.");
		}
        
        if (GUILayout.Button("Build Weapon"))
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(WeaponBuild), false, "WeaponBuilder");
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 1000);

        }
        if (GUILayout.Button("Manual Slide Manipulation"))
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(SlideSetup), false, "SlideSetup");
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 1000);

        }/*
        if (GUILayout.Button("Attachment configuration"))
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(AttachmentSetup), false, "AttachmentSetup");
            window.minSize = new Vector2(400, 400);
            window.maxSize = new Vector2(400, 1000);

        }*/
        EditorGUILayout.LabelField("Click this button if weapon should");
        EditorGUILayout.LabelField("be able to be held on it's own:");
        if (GUILayout.Button("Add VRWInteractableWeapon Script"))
        {
            if (weaponScript.GetComponent<VRWInteractableWeapon>() == null)
            {
                weaponScript.gameObject.AddComponent<VRWInteractableWeapon>();
            }
            if (weaponScript.GetComponent<Collider>() == null && weaponScript.GetComponentInChildren<MeshCollider>() == null)
            {
                weaponScript.gameObject.AddComponent<BoxCollider>();
            }
        }
        weaponScript.range = EditorGUILayout.FloatField ("Range", weaponScript.range);
		weaponScript.damage = EditorGUILayout.IntField ("Damage", weaponScript.damage);

        weaponScript.bulletSpreadRange = EditorGUILayout.Slider("Bullet spread range", weaponScript.bulletSpreadRange, 0, 0.2f);

        weaponScript.spreadOverTimeModifier = EditorGUILayout.Slider("Bullet spread over time modifier", weaponScript.spreadOverTimeModifier, 0, 0.2f);
        weaponScript.bulletSpreadMax = EditorGUILayout.Slider("Bullet spread max", weaponScript.bulletSpreadMax, 0, 0.2f);
		weaponScript.automaticChamber = EditorGUILayout.Toggle ("Automatically chamber next round?", weaponScript.automaticChamber);

		if (weaponScript.automaticChamber) {
			weaponScript.fireRate = EditorGUILayout.FloatField ("Fire rate", weaponScript.fireRate);
		}

		weaponScript.fireMode = (VRTK.Weapon.FireMode)EditorGUILayout.EnumPopup ("Fire mode", weaponScript.fireMode);

		if (weaponScript.fireMode == VRTK.Weapon.FireMode.BurstFire) {
			weaponScript.burst = EditorGUILayout.IntField ("Burst amount", weaponScript.burst);
		} else if (weaponScript.fireMode == VRTK.Weapon.FireMode.Projectile) {
			weaponScript.projectile = (GameObject)EditorGUILayout.ObjectField ("Projectile", weaponScript.projectile, typeof(GameObject), true);
			weaponScript.projForce = (float)EditorGUILayout.FloatField ("Projectile shot force", weaponScript.projForce);
            weaponScript.projectileRotationalOffset = EditorGUILayout.Vector3Field("Projectile rotational offset", weaponScript.projectileRotationalOffset);
		} else if (weaponScript.fireMode == VRTK.Weapon.FireMode.Shotgun)
        {
            weaponScript.shotgunPellets = (int)EditorGUILayout.IntField("Shotgun pellets per shot", weaponScript.shotgunPellets);
        } else if (weaponScript.fireMode == VRTK.Weapon.FireMode.AutofireProj)
        {
            weaponScript.projectile = (GameObject)EditorGUILayout.ObjectField("Projectile", weaponScript.projectile, typeof(GameObject), true);
            weaponScript.projForce = (float)EditorGUILayout.FloatField("Projectile shot force", weaponScript.projForce);
        }
        if (weaponScript.fireMode == VRTK.Weapon.FireMode.Bullet || weaponScript.fireMode == VRTK.Weapon.FireMode.AutoFire || weaponScript.fireMode == VRTK.Weapon.FireMode.BurstFire)
        {
            weaponScript.usesLineRenderer = EditorGUILayout.Toggle("Use line renderer for bullets?", weaponScript.usesLineRenderer);
            if (weaponScript.usesLineRenderer)
            {
                weaponScript.lineRendererLife = EditorGUILayout.FloatField("Line renderer lifetime: ", weaponScript.lineRendererLife);
            }
        }
        if (GUILayout.Button("Set up Impact Profile"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/VRWeapons"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Impact Profiles"))
                    AssetDatabase.CreateFolder("Assets", "Impact Profiles");
                if (weaponScript.impactProfile == null)
                {
                    AssetDatabase.CreateAsset(CreateInstance("ImpactProfile"), "Assets/Impact Profiles/" + weaponScript.gameObject.name + "_ImpactProfile.asset");
                    weaponScript.impactProfile = (ImpactProfile)AssetDatabase.LoadAssetAtPath("Assets/Impact Profiles/" + weaponScript.gameObject.name + "_ImpactProfile.asset", typeof(ImpactProfile));
                    Debug.Log("Impact profile created in \"Assets/Impact Profiles\"");
                }
                else
                    Debug.LogWarning("Impact profile already exists. No new Impact profile created.");
                
            }
            else
            {
                if (!AssetDatabase.IsValidFolder("Assets/VRWeapons/Impact Profiles"))
                {
                    AssetDatabase.CreateFolder("Assets/VRWeapons", "Impact Profiles");
                }

                if (weaponScript.impactProfile == null)
                {
                    AssetDatabase.CreateAsset(CreateInstance("ImpactProfile"), "Assets/VRWeapons/Impact Profiles/" + weaponScript.gameObject.name + "_ImpactProfile.asset");
                    weaponScript.impactProfile = (ImpactProfile)AssetDatabase.LoadAssetAtPath("Assets/VRWeapons/Impact Profiles/" + weaponScript.gameObject.name + "_ImpactProfile.asset", typeof(ImpactProfile));
                    Debug.Log("Impact profile created in \"Assets/VRWeapons/Impact Profiles\"");
                }
                else
                    Debug.LogWarning("Impact profile already exists. No new Impact profile created.");

            }

            //EditorWindow window = EditorWindow.GetWindow(typeof(ImpactProfileWindow), false, "Impact Profile");
        }

		if (weaponScript.fireMode != VRTK.Weapon.FireMode.Projectile) {
			weaponScript.impactProfile = (ImpactProfile)EditorGUILayout.ObjectField("Impact Profile: ", weaponScript.impactProfile, typeof(ImpactProfile), true);
		}
		weaponScript.force = EditorGUILayout.FloatField ("Impact force", weaponScript.force);

		isCodeTrigger = EditorGUILayout.Toggle ("Animate trigger?", isCodeTrigger);

		if (isCodeTrigger) {
			weaponScript.trigger = (Transform)EditorGUILayout.ObjectField ("Trigger", weaponScript.trigger, typeof(Transform), true);
			weaponScript.triggerMult = EditorGUILayout.IntField ("Trigger rotation multiplier", weaponScript.triggerMult);
            weaponScript.triggerDir = EditorGUILayout.Vector3Field("Trigger rotation axis", weaponScript.triggerDir);
		}

        weaponScript.infiniteAmmo = EditorGUILayout.ToggleLeft("Infinite Ammo", weaponScript.infiniteAmmo);
        
		weaponScript.feedbackTime = EditorGUILayout.IntField ("Haptic time per shot (in frames)", weaponScript.feedbackTime);
		weaponScript.hapticStrength = (ushort)EditorGUILayout.Slider ("Haptic strength", weaponScript.hapticStrength, 0, 3999);
		showSounds = EditorGUILayout.Toggle ("Show audio options", showSounds);

		if (showSounds) {
			weaponScript.shotSound = (AudioClip)EditorGUILayout.ObjectField ("Shot sound", weaponScript.shotSound, typeof(AudioClip), true);
			weaponScript.magIn = (AudioClip)EditorGUILayout.ObjectField ("Magazine inserted", weaponScript.magIn, typeof(AudioClip), true);
			weaponScript.magOut = (AudioClip)EditorGUILayout.ObjectField ("Magazine removed", weaponScript.magOut, typeof(AudioClip), true);
            weaponScript.slideBack = (AudioClip)EditorGUILayout.ObjectField("Slide back", weaponScript.slideBack, typeof(AudioClip), true);
            weaponScript.slideForward = (AudioClip)EditorGUILayout.ObjectField("Slide forward", weaponScript.slideForward, typeof(AudioClip), true);
            weaponScript.noShotSound = (AudioClip)EditorGUILayout.ObjectField("Empty chamber 'click'", weaponScript.noShotSound, typeof(AudioClip), true);
        }
        Undo.RecordObject(weaponScript, "Weapon Building");
        Undo.RecordObject(this, "Weapon Building");
    }
    
}
#endregion

#region Weapon Builder window
public class WeaponBuild : EditorWindow
{
    ReloadPoint rld;
    GameObject gripPt, mainHandGripPt;
    GameObject dir, orig, magAssign;
    int index = 0;
    int flashSize;
    bool muzzleModify, kickModify, gripModify, found = false;
    string prefabName;
    public Vector2 scrollPosition = Vector2.zero;

    void OnGUI()
    {
        int count = 0;
        VRTK.Weapon[] tmp = FindObjectsOfType<VRTK.Weapon>();
        foreach (VRTK.Weapon wpn in tmp)
        {
            if (Selection.activeGameObject != null)
            {
                if (wpn == Selection.activeGameObject.GetComponent<VRTK.Weapon>() && !found)
                {
                    index = count;
                    found = true;
                }
            }
            count++;
        }
        found = true;
        count = 0;
        foreach (VRTK.Weapon wpn in tmp)
            count++;
        string[] names = new string[count];
        int i = 0;
        foreach (VRTK.Weapon wpn in tmp) {
            names[i] = wpn.name;
            i++;
        }

        index = EditorGUILayout.Popup("Weapon: ", index, names);
        VRTK.Weapon weaponScript = tmp[index];

        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(400), GUILayout.Height(position.height - 50));
        Undo.RecordObject(weaponScript, "Weapon Building");

        if (weaponScript.muzzleDirectionSet == false) {
            if (GUILayout.Button("Muzzle direction")) {
                if (orig == null) {
                    orig = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    orig.name = "Muzzle origin";
                    orig.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    orig.transform.parent = weaponScript.transform;
                    orig.transform.localPosition = new Vector3(0, 0, 0);
                    var tempMaterial = new Material(orig.GetComponent<Renderer>().sharedMaterial);
                    tempMaterial.color = new Vector4(1, 0, 0, 0.5f);
                    orig.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                    dir = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    dir.name = "Muzzle direction";
                    dir.transform.parent = orig.transform;
                    dir.transform.localPosition = new Vector3(0, 0, 0);
                    dir.transform.localScale = new Vector3(1, 1, 1);
                    dir.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                }
                weaponScript.showMuzDirectionSetup = true;
                Selection.activeGameObject = orig.gameObject;

            }
        }
        if (weaponScript.showMuzDirectionSetup) {
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Move the RED CUBE to muzzle origin location.");
            EditorGUILayout.LabelField("Move the RED SPHERE to muzzle shot direction.");

            //weaponScript.muzzleFlash = (GameObject)EditorGUILayout.ObjectField ("Muzzle flash", weaponScript.muzzleFlash, typeof(GameObject), true);

            EditorGUI.BeginChangeCheck();
            if (weaponScript.muzzleFlash != null)
                flashSize = weaponScript.muzzleFlash.Length;

            weaponScript.flashLife = EditorGUILayout.FloatField("Muzzle flash life", weaponScript.flashLife);

            flashSize = EditorGUILayout.IntSlider("Flash size", flashSize, 0, 50);
            if (EditorGUI.EndChangeCheck())
            {
                GameObject[] flashTmp = weaponScript.muzzleFlash;
                weaponScript.muzzleFlash = new GameObject[flashSize];
                for (int k = 0; k < flashSize; k++)
                {
                    if (flashSize > k)
                        if (flashTmp.Length > k)
                            weaponScript.muzzleFlash[k] = flashTmp[k];
                }
            }
            for (int j = 0; j < flashSize; j++)
            {
                weaponScript.muzzleFlash[j] = (GameObject)EditorGUILayout.ObjectField("Muzzle flash", weaponScript.muzzleFlash[j], typeof(GameObject), true);
            }

            weaponScript.flashDirection = EditorGUILayout.Vector3Field("Muzzle flash direction", weaponScript.flashDirection);



            if (GUILayout.Button("Save muzzle direction")) {
                Undo.RecordObject(weaponScript, "Weapon Building");
                weaponScript.muzzleDirection = dir.transform;
                weaponScript.muzzle = orig.transform;
                orig.SetActive(false);
                dir.SetActive(false);
                weaponScript.showMuzDirectionSetup = false;
                weaponScript.muzzleDirectionSet = true;
                orig = null;
                dir = null;

                //////////// Set up layers and tags (Assuming everyone will use the muzzle direction, so safe to assume this will cover all weapons) /////////////

                weaponScript.gameObject.layer = LayerMask.NameToLayer("Weapon");
                foreach (Transform t in weaponScript.GetComponentsInChildren<Transform>()) {
                    t.gameObject.layer = LayerMask.NameToLayer("Weapon");
                }

            }
            if (GUILayout.Button("Cancel")) {
                if (orig != null)
                    DestroyImmediate(orig.gameObject);
                if (dir != null)
                    DestroyImmediate(dir.gameObject);
                weaponScript.showMuzDirectionSetup = false;
                orig = null;
                dir = null;
            }
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }

        if (weaponScript.muzzleDirectionSet == true) {
            if (GUILayout.Button("Modify muzzle options")) {
                muzzleModify = true;
            }
            if (muzzleModify) {

                //weaponScript.muzzleFlash = (GameObject)EditorGUILayout.ObjectField ("Muzzle flash", weaponScript.muzzleFlash, typeof(GameObject), true);
                EditorGUI.BeginChangeCheck();
                if (weaponScript.muzzleFlash != null)
                    flashSize = weaponScript.muzzleFlash.Length;

                weaponScript.flashLife = EditorGUILayout.FloatField("Muzzle flash life", weaponScript.flashLife);

                flashSize = EditorGUILayout.IntSlider("Flash size", flashSize, 0, 50);
                if (EditorGUI.EndChangeCheck())
                {
                    GameObject[] flashTmp = weaponScript.muzzleFlash;
                    weaponScript.muzzleFlash = new GameObject[flashSize];
                    for (int k = 0; k < flashSize; k++)
                    {
                        if (flashSize > k)
                            if (flashTmp.Length > k)
                                weaponScript.muzzleFlash[k] = flashTmp[k];
                    }
                }
                for (int j = 0; j < flashSize; j++)
                {
                    weaponScript.muzzleFlash[j] = (GameObject)EditorGUILayout.ObjectField("Muzzle flash", weaponScript.muzzleFlash[j], typeof(GameObject), true);
                }


                weaponScript.flashDirection = EditorGUILayout.Vector3Field("Muzzle flash direction", weaponScript.flashDirection);



                if (GUILayout.Button("Save")) {
                    Undo.RecordObject(weaponScript, "Weapon Building");
                    muzzleModify = false;
                }
            }
            if (GUILayout.Button("Reset muzzle direction")) {
                if (weaponScript.gameObject.transform.FindChild("Muzzle origin") != null) {
                    DestroyImmediate(weaponScript.gameObject.transform.FindChild("Muzzle origin").gameObject);
                }
                weaponScript.muzzleDirectionSet = false;
                weaponScript.muzzleDirection = null;
            }
        }
        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        if (!weaponScript.kickSet)
        {
            Undo.RecordObject(weaponScript, "Weapon Building");
            if (GUILayout.Button("Set up weapon kick")) {
                weaponScript.showKickSetup = true;
            }
        }
        if (weaponScript.showKickSetup)
        {
            weaponScript.kickStrength = EditorGUILayout.Slider("Rotational kick strength", weaponScript.kickStrength, 0, 50);
            weaponScript.jDriveSpringStrenth = EditorGUILayout.Slider("Rotational kick recovery speed", weaponScript.jDriveSpringStrenth, 0, 100);
            weaponScript.jDriveDamper = EditorGUILayout.Slider("Rotational Kick spring damper", weaponScript.jDriveDamper, 1, 10);
            weaponScript.zKickStrength = EditorGUILayout.Slider("Backward kick strength", weaponScript.zKickStrength, 0, 10);
            weaponScript.jZDriveStrength = EditorGUILayout.Slider("Backward kick recovery speed", weaponScript.jZDriveStrength, 0, 5000);
            weaponScript.jZDriveDamper = EditorGUILayout.Slider("Backward kick spring damper", weaponScript.jZDriveDamper, 1, 100);
            if (GUILayout.Button("Save kick options"))
            {
                Undo.RecordObject(weaponScript, "Weapon Building");
                weaponScript.showKickSetup = false;
                weaponScript.kickSet = true;
            }
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }
        if (weaponScript.kickSet) {
            if (GUILayout.Button("Modify kick options")) {
                weaponScript.showKickSetup = true;
            }

        }
        if (GUILayout.Button("Reset kick settings")) {
            weaponScript.kickSet = false;
            weaponScript.kickStrength = 0;
            weaponScript.jDriveDamper = 1;
            weaponScript.jDriveSpringStrenth = 0;
        }
        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

        if (weaponScript.rldPointSet == false) {
            if (weaponScript.showRldPointSetup == false) {
                if (GUILayout.Button("Add reload point")) {
                    if (weaponScript.gameObject.GetComponentInChildren<ReloadPoint>() == null) {
                        rld = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<ReloadPoint>();
                        rld.name = "Reload Point";
                        rld.tag = "ReloadPoint";
                        rld.gameObject.layer = LayerMask.NameToLayer("Default");
                        rld.gameObject.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                        rld.transform.parent = weaponScript.gameObject.transform;
                        var tempMaterial = new Material(rld.GetComponent<Renderer>().sharedMaterial);
                        tempMaterial.color = new Vector4(0, 1, 1, 0.3f);

                        rld.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                        rld.transform.localPosition = new Vector3(0, 0, 0);
                        rld.transform.localEulerAngles = new Vector3(0, 0, 0);
                        rld.gameObject.GetComponent<SphereCollider>().isTrigger = true;
                        rld.weap = weaponScript;
                    } else {
                        rld = weaponScript.gameObject.GetComponentInChildren<ReloadPoint>();
                    }
                    Selection.activeGameObject = rld.gameObject;
                    weaponScript.showRldPointSetup = true;
                }
            }
            if (weaponScript.showRldPointSetup == true) {
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Move and resize the BLUE SPHERE to the magazine reload point");
                if (GUILayout.Button("Save reload point")) {
                    Undo.RecordObject(weaponScript, "Weapon Building");
                    weaponScript.showRldPointSetup = false;
                    weaponScript.rldPointSet = true;
                    DestroyImmediate(rld.GetComponent<Renderer>());
                }
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            }
        }
        if (weaponScript.rldPointSet == true) {
            if (GUILayout.Button("Reset reload point")) {
                if (weaponScript.GetComponentInChildren<ReloadPoint>() != null) {
                    rld = weaponScript.GetComponentInChildren<ReloadPoint>();
                    DestroyImmediate(rld.gameObject);
                }

                weaponScript.rldPointSet = false;
                rld = null;
            }
        }

        if (weaponScript.holdPtSet == false)
        {
            if (weaponScript.showHoldPtAdj == false)
            {
                if (GUILayout.Button("Set up main hand grip point"))
                {
                    if (weaponScript.GetComponent<ConfigurableJoint>() == null)
                    {
                        weaponScript.gameObject.AddComponent<ConfigurableJoint>();
                    }
                    if (weaponScript.transform.FindChild("Main Hand Grip Point") != null)
                    {
                        mainHandGripPt = weaponScript.transform.FindChild("Main Hand Grip Point").gameObject;
                        weaponScript.showHoldPtAdj = true;
                    }
                    else
                    {
                        mainHandGripPt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        mainHandGripPt.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                        mainHandGripPt.transform.parent = weaponScript.gameObject.transform;
                        mainHandGripPt.name = "Main Hand Grip Point";
                        var tempMaterial = new Material(mainHandGripPt.GetComponent<Renderer>().sharedMaterial);
                        tempMaterial.color = Color.white;
                        mainHandGripPt.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                        mainHandGripPt.transform.localPosition = weaponScript.GetComponent<ConfigurableJoint>().anchor;
                        Selection.activeGameObject = mainHandGripPt.gameObject;
                        weaponScript.showHoldPtAdj = true;
                    }
                }
            }
            if (weaponScript.showHoldPtAdj)
            {
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Move and resize the WHITE SPHERE to the main hand grip point");
                EditorGUILayout.LabelField("Best results are right behind the weapon's grip, where the base");
                EditorGUILayout.LabelField("of the thumb would touch the grip.");

                weaponScript.adjustRotation = EditorGUILayout.Vector3Field("Rotational adjust", weaponScript.adjustRotation);

                if (GUILayout.Button("Save main hand grip point"))
                {
                    weaponScript.GetComponent<ConfigurableJoint>().anchor = mainHandGripPt.transform.localPosition;
                    weaponScript.showHoldPtAdj = false;
                    weaponScript.holdPtSet = true;
                    DestroyImmediate(mainHandGripPt.gameObject);
                    Undo.RecordObject(weaponScript, "Weapon Building");
                }
                if (GUILayout.Button("Cancel"))
                {
                    DestroyImmediate(mainHandGripPt.gameObject);
                    weaponScript.showHoldPtAdj = false;
                }
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            }
        }
        else
        {
            if (GUILayout.Button("Modify main hand grip options"))
            {
                if (mainHandGripPt == null)
                {
                    mainHandGripPt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    mainHandGripPt.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                    mainHandGripPt.transform.parent = weaponScript.gameObject.transform;
                    mainHandGripPt.name = "Main Hand Grip Point";
                    var tempMaterial = new Material(mainHandGripPt.GetComponent<Renderer>().sharedMaterial);
                    tempMaterial.color = Color.white;
                    mainHandGripPt.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                    mainHandGripPt.transform.localPosition = weaponScript.GetComponent<ConfigurableJoint>().anchor;
                    Selection.activeGameObject = mainHandGripPt.gameObject;

                    weaponScript.showHoldPtAdj = true;
                    weaponScript.holdPtSet = false;
                }
            }

            if (GUILayout.Button("Reset main hand grip point"))
            {
                weaponScript.mainHandGripPoint = Vector3.zero;
                weaponScript.GetComponent<ConfigurableJoint>().anchor = Vector3.zero;
                weaponScript.holdPtSet = false;
            }
            Undo.RecordObject(weaponScript, "Weapon Building");

        }




        if (weaponScript.gripPtSet == false) {
            if (weaponScript.showGripAdj == false) {
                if (GUILayout.Button("Set up second hand grip point")) {
                    if (weaponScript.transform.FindChild("Grip Point") == null) {
                        gripPt = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        if (gripPt.GetComponent<Collider>() == null)
                        {
                            gripPt.AddComponent<CapsuleCollider>();
                        }
                        gripPt.gameObject.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
                        gripPt.transform.parent = weaponScript.gameObject.transform;
                        gripPt.name = "Grip Point";
                        gripPt.gameObject.layer = LayerMask.NameToLayer("Weapon");
                        gripPt.transform.localEulerAngles = new Vector3(90, 0, 0);
                        var tempMaterial = new Material(gripPt.GetComponent<Renderer>().sharedMaterial);
                        tempMaterial.color = new Vector4(1, 0.5f, 0, 0.3f);

                        gripPt.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                        gripPt.transform.localPosition = new Vector3(0, 0, 0);
                        gripPt.gameObject.GetComponent<Collider>().isTrigger = true;
                    } else {
                        gripPt = weaponScript.transform.FindChild("Grip Point").gameObject;
                    }
                    Selection.activeGameObject = gripPt.gameObject;
                    weaponScript.showGripAdj = true;
                    Undo.RecordObject(weaponScript, "Weapon Building");
                }
            }
            if (weaponScript.showGripAdj == true) {
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Move and resize the ORANGE CAPSULE to the second grip point");


                if (GUILayout.Button("Save grip point")) {
                    Undo.RecordObject(weaponScript, "Weapon Building");
                    if (gripPt.GetComponent<VRWInteractableGrip>() == null)
                    {
                        gripPt.AddComponent<VRWInteractableGrip>();
                    }
                    weaponScript.showGripAdj = false;
                    weaponScript.gripPtSet = true;
                    DestroyImmediate(gripPt.GetComponent<Renderer>());
                }
                if (GUILayout.Button("Cancel")) {
                    DestroyImmediate(gripPt.gameObject);
                    weaponScript.showGripAdj = false;
                }
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                Undo.RecordObject(weaponScript, "Weapon Building");
            }
        }
        if (weaponScript.gripPtSet == true)
        {
            if (GUILayout.Button("Reset grip point")) {
                if (weaponScript.transform.FindChild("Grip Point") != null) {
                    gripPt = weaponScript.transform.FindChild("Grip Point").gameObject;
                    DestroyImmediate(gripPt.gameObject);
                }

                weaponScript.gripPtSet = false;
                gripPt = null;
            }
            Undo.RecordObject(weaponScript, "Weapon Building");
        }

        if (GUILayout.Button("Pair magazine type to weapon")) {
            weaponScript.showPairMag = true;
        }
        if (weaponScript.showPairMag) {
            weaponScript.internalMagazine = EditorGUILayout.ToggleLeft("Internal magazine?", weaponScript.internalMagazine);
            weaponScript.magNeedsHeldToReload = EditorGUILayout.ToggleLeft("Mag must be held to reload", weaponScript.magNeedsHeldToReload);
            if (!weaponScript.internalMagazine)
            {
                if (weaponScript.mag != null)
                {
                    EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                    EditorGUILayout.LabelField("Align magazine to desired transform values");
                    weaponScript.mag = (magazine)EditorGUILayout.ObjectField("Magazine: ", weaponScript.mag, typeof(magazine), true);
                    if (weaponScript.mag != null)
                    {
                        weaponScript.mag.ammo = EditorGUILayout.IntField("Ammo capacity: ", weaponScript.mag.ammo);
                        weaponScript.chamberMustBeOpenToReload = EditorGUILayout.ToggleLeft("Chamber must be open to reload", weaponScript.chamberMustBeOpenToReload);
                        weaponScript.mag.adjustRot = EditorGUILayout.Vector3Field("Held magazine rotation", weaponScript.mag.adjustRot);
                        weaponScript.mag.magEjectDir = EditorGUILayout.Vector3Field("Magazine eject direction", weaponScript.mag.magEjectDir);
                        weaponScript.mag.magEjectForce = EditorGUILayout.FloatField("Magazine eject force", weaponScript.mag.magEjectForce);
                    }
                    Undo.RecordObject(weaponScript.mag, "Magazine setup");
                    Undo.RecordObject(weaponScript, "Weapon Building");
                }
                else
                {
                    magAssign = (GameObject)EditorGUILayout.ObjectField("Magazine: ", magAssign, typeof(GameObject), true);
                }
                if (magAssign != null)
                {
                    if (weaponScript.mag != magAssign)
                    {
                        if (magAssign.GetComponent<magazine>() == null)
                            weaponScript.mag = magAssign.AddComponent<magazine>();
                        else
                            weaponScript.mag = magAssign.GetComponent<magazine>();
                    }
                }
                Undo.RecordObject(weaponScript, "Weapon Building");
            }
            else
            {
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                weaponScript.intMag.ammoObj = (GameObject)EditorGUILayout.ObjectField("Rounds: ", weaponScript.intMag.ammoObj, typeof(GameObject), true);
                if (weaponScript.intMag.ammoObj != null)
                {
                    if (weaponScript.intMag.ammoObj.GetComponent<VRWRound>() == null)
                    {
                        weaponScript.intMag.ammoObj.AddComponent<VRWRound>();
                    }
                }
                weaponScript.intMag.maxAmmo = EditorGUILayout.IntField("Maximum ammo:", weaponScript.intMag.maxAmmo);
                weaponScript.intMag.fullOnAwake = EditorGUILayout.ToggleLeft("Weapon loaded on start?", weaponScript.intMag.fullOnAwake);
                weaponScript.chamberMustBeOpenToReload = EditorGUILayout.ToggleLeft("Chamber must be open to reload", weaponScript.chamberMustBeOpenToReload);
                Undo.RecordObject(weaponScript, "Weapon Building");


            }
            if (GUILayout.Button("Pair")) {
                if (!weaponScript.internalMagazine)
                {
                    if (weaponScript.mag != null)
                    {
                        VRTK.Weapon[] tmpArray = FindObjectsOfType<VRTK.Weapon>();
                        foreach (VRTK.Weapon wpn in tmpArray)
                        {
                            if (wpn.weaponType == weaponScript.weaponType)
                            {
                                weaponScript.weaponType = (int)Random.Range(1055, 2055);    // If it matches another weapon's weapon type, give it a random big number that it wouldn't realistically give to reset it. Should fix duplicate mag assignments
                            }
                        }
                        if (weaponScript.weaponType != weaponScript.mag.weaponType)
                        {
                            i = 0;
                            bool solved = false;
                            while (!solved)
                            {
                                solved = true;
                                foreach (VRTK.Weapon wpn in tmpArray)
                                {
                                    if (wpn.weaponType == i)
                                    {

                                        solved = false;
                                    }
                                }
                                if (!solved)
                                {
                                    i++;
                                }
                            }
                            weaponScript.weaponType = i;
                            weaponScript.mag.weaponType = i;
                        }
                        Vector3 magOPos = weaponScript.mag.transform.localPosition;
                        Vector3 magORot = weaponScript.mag.transform.localEulerAngles;
                        Vector3 magOSca = weaponScript.mag.transform.localScale;

                        weaponScript.magOPos = magOPos;
                        weaponScript.magORot = magORot;
                        weaponScript.magOSca = magOSca;
                        weaponScript.showPairMag = false;
                        weaponScript.mag.tag = "Magazine";

                        ///////////////// Ensuring that magazines are able to be picked up /////////////////////

                        if (weaponScript.mag.GetComponent<Collider>() == null)
                        {
                            weaponScript.mag.gameObject.AddComponent<BoxCollider>();
                        }
                        if (weaponScript.mag.GetComponent<Rigidbody>() == null)
                        {
                            weaponScript.mag.gameObject.AddComponent<Rigidbody>();
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Add a GameObject as the weapon's magazine");

                    }
                }
                else
                {
                    VRTK.Weapon[] tmpArray = FindObjectsOfType<VRTK.Weapon>();
                    if (weaponScript.intMag.ammoObj != null)
                    {
                        foreach (VRTK.Weapon wpn in tmpArray)
                        {
                            if (wpn.weaponType == weaponScript.weaponType)
                            {
                                weaponScript.weaponType = (int)Random.Range(1055, 2055);    // If it matches another weapon's weapon type, give it a random big number that it wouldn't realistically give to reset it. Should fix duplicate mag assignments
                            }
                        }
                        i = 0;
                        bool solved = false;
                        while (!solved)
                        {
                            solved = true;
                            foreach (VRTK.Weapon wpn in tmpArray)
                            {
                                Debug.Log("Comparing weapon " + wpn + " of type " + wpn.weaponType + " with possible type " + i);
                                if (wpn.weaponType == i)
                                {

                                    solved = false;
                                }
                            }
                            if (!solved)
                            {
                                i++;
                            }
                        }
                        
                            weaponScript.weaponType = i;
                            weaponScript.intMag.ammoObj.GetComponent<VRWRound>().WeaponType = i;
                        weaponScript.showPairMag = false;
                        weaponScript.intMag.ammoObj.tag = "Magazine";

                        ///////////////// Ensuring that magazines are able to be picked up /////////////////////

                        if (weaponScript.intMag.ammoObj.GetComponent<Collider>() == null)
                        {
                            weaponScript.intMag.ammoObj.AddComponent<BoxCollider>();
                        }
                        if (weaponScript.intMag.ammoObj.GetComponent<Rigidbody>() == null)
                        {
                            weaponScript.intMag.ammoObj.gameObject.AddComponent<Rigidbody>();
                        }

                        weaponScript.intMag.ammoObj.gameObject.layer = LayerMask.NameToLayer("Weapon");
                        
                    }
                    else
                    {

                        weaponScript.showPairMag = false;

                    }
                }
                Undo.RecordObject(weaponScript, "Weapon Building");

                magAssign = null;



            }
            if (GUILayout.Button("Cancel")) {
                weaponScript.showPairMag = false;
                magAssign = null;
            }
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }

        if (weaponScript.mag != null)
        {
            if (GUILayout.Button("Remove paired magazine"))
            {
                DestroyImmediate(weaponScript.mag.GetComponent<magazine>());
                weaponScript.mag = null;
            }
        }

        if (GUILayout.Button("Save weapon as prefab")) {
            weaponScript.showPrefabSave = true;
        }
        if (weaponScript.showPrefabSave == true) {
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            prefabName = EditorGUILayout.TextField("Prefab name:", prefabName);

            if (GUILayout.Button("Save") && prefabName != "") {
                ///////////// Cleaning up extra Origin and Direction GameObjects ////////////

                for (int t = 0; t < weaponScript.transform.childCount; t++) {
                    if (weaponScript.transform.GetChild(t).name == "Muzzle origin" && weaponScript.transform.GetChild(t) != weaponScript.muzzle) {
                        DestroyImmediate(weaponScript.transform.GetChild(t).gameObject);
                    }

                }

                for (int t = 0; t < weaponScript.transform.childCount; t++) {
                    if (weaponScript.transform.GetChild(t).name == "Muzzle direction" && weaponScript.transform.GetChild(t) != weaponScript.muzzleDirection) {
                        DestroyImmediate(weaponScript.transform.GetChild(t).gameObject);
                    }

                }

                for (int t = 0; t < weaponScript.transform.childCount; t++) {
                    if (weaponScript.transform.GetChild(t).name == "Ejector origin" && weaponScript.transform.GetChild(t) != weaponScript.ejector) {
                        DestroyImmediate(weaponScript.transform.GetChild(t).gameObject);
                    }

                }

                for (int t = 0; t < weaponScript.transform.childCount; t++) {
                    if (weaponScript.transform.GetChild(t).name == "Ejector direction" && weaponScript.transform.GetChild(t) != weaponScript.ejectorDirection) {
                        DestroyImmediate(weaponScript.transform.GetChild(t).gameObject);
                    }

                }

                ////////////// Now the actual saving ///////////////
                if (!AssetDatabase.IsValidFolder("Assets/VRWeapons"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                        AssetDatabase.CreateFolder("Assets", "Prefabs");
                    PrefabUtility.CreatePrefab("Assets/Prefabs/" + prefabName + ".prefab", weaponScript.gameObject);
                    if (!weaponScript.internalMagazine && weaponScript.mag != null)
                    {
                        PrefabUtility.CreatePrefab("Assets/Prefabs/" + prefabName + "_magazine.prefab", weaponScript.mag.gameObject);
                    }
                    else if (weaponScript.intMag.ammoObj.gameObject != null)
                    {
                        PrefabUtility.CreatePrefab("Assets/Prefabs/" + prefabName + "_ammo.prefab", weaponScript.intMag.ammoObj.gameObject);
                    }
                    weaponScript.showPrefabSave = false;
                    prefabName = "";
                }
                else
                {
                    if (!AssetDatabase.IsValidFolder("Assets/VRWeapons/Prefabs"))
                        AssetDatabase.CreateFolder("Assets/VRWeapons", "Prefabs");
                    PrefabUtility.CreatePrefab("Assets/VRWeapons/Prefabs/" + prefabName + ".prefab", weaponScript.gameObject);
                    if (!weaponScript.internalMagazine && weaponScript.mag != null)
                    {
                        PrefabUtility.CreatePrefab("Assets/VRWeapons/Prefabs/" + prefabName + "_magazine.prefab", weaponScript.mag.gameObject);
                    }
                    else if (weaponScript.intMag.ammoObj.gameObject != null)
                    {
                        PrefabUtility.CreatePrefab("Assets/VRWeapons/Prefabs/" + prefabName + "_ammo.prefab", weaponScript.intMag.ammoObj.gameObject);
                    }
                    weaponScript.showPrefabSave = false;
                    prefabName = "";
                }




            }
            if (GUILayout.Button("Cancel")) {
                weaponScript.showPrefabSave = false;
                prefabName = "";
            }
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }
        GUILayout.EndScrollView();
    }
}
#endregion

#region Slide Setup window
public class SlideSetup : EditorWindow
{
    int index = 0;
    bool found = false, slideLimSetup = false, ejectorModify, startPreviouslyExisted, endPreviouslyExisted;
    public Vector2 scrollPosition = Vector2.zero;
    Vector3 slideStart, slideEnd, oldSlidePos;
    GameObject ejDir, ejOrig, start, end;
    VRWSlideManipulation slideManip;

    void OnGUI()
    {
        int count = 0;
        VRTK.Weapon[] tmp = FindObjectsOfType<VRTK.Weapon>();
        foreach (VRTK.Weapon wpn in tmp)
        {
            if (Selection.activeGameObject != null)
            {
                if (wpn == Selection.activeGameObject.GetComponent<VRTK.Weapon>() && !found)
                {
                    index = count;
                    found = true;
                }
            }
            count++;
        }
        found = true;
        count = 0;
        foreach (VRTK.Weapon wpn in tmp)
            count++;
        string[] names = new string[count];
        int i = 0;
        foreach (VRTK.Weapon wpn in tmp)
        {
            names[i] = wpn.name;
            i++;
        }

        index = EditorGUILayout.Popup("Weapon: ", index, names);
        VRTK.Weapon weaponScript = tmp[index];

        //scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(400), GUILayout.Height(position.height - 25));
        Undo.RecordObject(weaponScript, "Weapon Building");

        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

        weaponScript.slideObj = (GameObject)EditorGUILayout.ObjectField("Weapon slide", weaponScript.slideObj, typeof(GameObject), true);

        if (weaponScript.slideObj != null)
        {
            if (GUILayout.Button("Set slide max & min limits"))
            {
                if (weaponScript.transform.FindChild("Linear Drive Start") != null)
                {
                    start = weaponScript.transform.FindChild("Linear Drive Start").gameObject;
                    startPreviouslyExisted = true;
                }

                if (weaponScript.transform.FindChild("Linear Drive End") != null)
                {
                    end = weaponScript.transform.FindChild("Linear Drive End").gameObject;
                    endPreviouslyExisted = true;
                }
                if (weaponScript.slideObj.GetComponent<VRWSlideManipulation>() == null)
                {
                    slideManip = weaponScript.slideObj.AddComponent<VRWSlideManipulation>();
                }
                else
                {
                    slideManip = weaponScript.slideObj.GetComponent<VRWSlideManipulation>();
                }
                weaponScript.slideManipDriver = slideManip;
                slideManip.repositionGameObject = false;
                slideManip.maintainMomemntum = false;
                slideManip.slideObj = weaponScript.slideObj;
                slideLimSetup = true;
                oldSlidePos = weaponScript.slideObj.transform.localPosition;
            }

            weaponScript.isBoltSeparate = EditorGUILayout.ToggleLeft("Bolt moves independently from charging handle", weaponScript.isBoltSeparate);

            if (weaponScript.isBoltSeparate)
            {
                weaponScript.separateBolt = (GameObject)EditorGUILayout.ObjectField("Bolt object: ", weaponScript.separateBolt, typeof(GameObject), true);
                if (GUILayout.Button("Set chamber fully closed position"))
                {
                    weaponScript.separateBoltStart = weaponScript.separateBolt.transform.localPosition;
                }
                if (GUILayout.Button("Set chamber fully open position"))
                {
                    weaponScript.separateBoltEnd = weaponScript.separateBolt.transform.localPosition;
                }
                if (GUILayout.Button("Reset to closed position"))
                {
                    weaponScript.separateBolt.transform.localPosition = weaponScript.separateBoltStart;
                }

            }

            if (slideLimSetup)
            {
                Undo.RecordObject(weaponScript, "Weapon Building");
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                if (GUILayout.Button("Set chamber fully closed position"))
                {
                    slideStart = weaponScript.slideObj.transform.localPosition;
                }
                if (GUILayout.Button("Set chamber fully open position"))
                {
                    slideEnd = weaponScript.slideObj.transform.localPosition;
                }
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

                if (GUILayout.Button("Set up Linear Drive start and end positions"))
                {

                    if (slideManip.startPosition != null)
                    {
                        start = slideManip.startPosition.gameObject;
                        start.SetActive(true);
                    }
                    if (slideManip.endPosition != null)
                    {
                        end = slideManip.endPosition.gameObject;
                        end.SetActive(true);
                    }


                    if (start == null)
                    {
                        start = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        start.name = "Linear Drive Start";
                        start.transform.localScale = new Vector3(.03f, .03f, .03f);
                        start.transform.parent = weaponScript.transform;
                        start.transform.localPosition = new Vector3(0, 0, 0);
                        var tempMaterial = new Material(start.GetComponent<Renderer>().sharedMaterial);
                        tempMaterial.color = Color.green;
                        start.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                        startPreviouslyExisted = false;
                    }
                    if (end == null)
                    {
                        end = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        end.name = "Linear Drive End";
                        end.transform.localScale = new Vector3(.03f, .03f, .03f);
                        end.transform.parent = weaponScript.transform;
                        end.transform.localPosition = new Vector3(0, 0, 0);
                        var tempMaterial = new Material(end.GetComponent<Renderer>().sharedMaterial);
                        tempMaterial.color = Color.green;
                        end.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                        endPreviouslyExisted = false;

                    }
                    Selection.activeGameObject = start;
                }

                if (GUILayout.Button("Save"))
                {
                    if (weaponScript.slideObj.GetComponent<Collider>() == null)
                    {
                        weaponScript.slideObj.AddComponent<BoxCollider>();
                    }

                    slideManip.startPosition = start.transform;
                    slideManip.endPosition = end.transform;

                    start.SetActive(false);
                    end.SetActive(false);

                    weaponScript.slideStart = slideStart;
                    weaponScript.slideEnd = slideEnd;
                    weaponScript.slideObj.transform.localPosition = weaponScript.slideStart;
                    slideLimSetup = false;
                    startPreviouslyExisted = false;
                    endPreviouslyExisted = false;
                }
                if (GUILayout.Button("Cancel"))
                {
                    weaponScript.slideObj.transform.localPosition = oldSlidePos;
                    slideLimSetup = false;

                    if (!startPreviouslyExisted && weaponScript.transform.FindChild("Linear Drive Start") != null)
                    {
                        DestroyImmediate(weaponScript.transform.FindChild("Linear Drive Start").gameObject);
                    }
                    if (!endPreviouslyExisted && weaponScript.transform.FindChild("Linear Drive End") != null)
                    {
                        DestroyImmediate(weaponScript.transform.FindChild("Linear Drive End").gameObject);
                    }

                    startPreviouslyExisted = false;
                    endPreviouslyExisted = false;
                }
                Undo.RecordObject(weaponScript, "Weapon Building");
            }

            weaponScript.slideMovesOnFiring = EditorGUILayout.ToggleLeft("Slide moves on firing", weaponScript.slideMovesOnFiring);

            weaponScript.autoRackForward = EditorGUILayout.ToggleLeft("Automatic rack forward", weaponScript.autoRackForward);

            weaponScript.chamberOnReload = EditorGUILayout.ToggleLeft("Weapon automatically chambers on reload", weaponScript.chamberOnReload);

            weaponScript.slideTime = EditorGUILayout.IntField("Slide speed (in frames)", weaponScript.slideTime);

            weaponScript.slideLeeway = EditorGUILayout.Slider("Open/Closed leeway", weaponScript.slideLeeway, 0, 0.5f);

            weaponScript.slideRotTriggerValue = EditorGUILayout.Slider("Rotate until: ", weaponScript.slideRotTriggerValue, 0, 1);
            if (GUILayout.Button("Set beginning rotation"))
            {
                weaponScript.slideRotStart = weaponScript.slideObj.transform.localEulerAngles;
            }
            if (GUILayout.Button("Set end rotation"))
            {
                weaponScript.slideRotEnd = weaponScript.slideObj.transform.localEulerAngles;
            }
            weaponScript.ableToGripAndManip = EditorGUILayout.ToggleLeft("Can be gripped and manipulate slide at same time", weaponScript.ableToGripAndManip);

            weaponScript.bulletShell = (GameObject)EditorGUILayout.ObjectField("Bullet Shell: ", weaponScript.bulletShell, typeof(GameObject), true);
            weaponScript.bullet = (GameObject)EditorGUILayout.ObjectField("Unspent round: ", weaponScript.bullet, typeof(GameObject), true);
            weaponScript.ejectForce = EditorGUILayout.FloatField("Shell eject force", weaponScript.ejectForce);

            if (weaponScript.ejectorDirectionSet == false && weaponScript.slideObj != null && weaponScript.bullet != null && !weaponScript.showEjectDirectionSetup)
            {
                if (GUILayout.Button("Add spent shell ejector"))
                {
                    if (ejOrig == null)
                    {
                        ejOrig = Instantiate(weaponScript.bullet, weaponScript.slideObj.transform, false);

                        Transform scalingParent = ejOrig.transform.parent;
                        float scaleX = scalingParent.localScale.x;
                        float scaleY = scalingParent.localScale.y;
                        float scaleZ = scalingParent.localScale.z;

                        Vector3 newScale = new Vector3(ejOrig.transform.localScale.x / scaleX, ejOrig.transform.localScale.y / scaleY, ejOrig.transform.localScale.z / scaleZ);
                        ejOrig.transform.localScale = newScale;

                        ejOrig.name = "Unspent round";
                        ejOrig.transform.localPosition = new Vector3(0, 0, 0);
                        ejDir = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        ejDir.name = "Ejector direction";

                        ejDir.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

                        ejDir.transform.parent = ejOrig.transform;
                        
                        ejDir.transform.localPosition = new Vector3(0, 0, 0);
                        var tempMaterial = new Material(ejDir.GetComponent<Renderer>().sharedMaterial);
                        tempMaterial.color = new Vector4(1, 0.92f, 0.016f, 0.3f);
                        ejDir.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;
                    }
                    weaponScript.showEjectDirectionSetup = true;
                    Selection.activeGameObject = ejOrig.gameObject;
                }
            }

            if (weaponScript.showEjectDirectionSetup)
            {
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Move the Unspent Round to the face of the bolt.");
                EditorGUILayout.LabelField("Move the YELLOW SPHERE to spent shell eject direction.");
                if (GUILayout.Button("Save shell eject direction"))
                {
                    Undo.RecordObject(weaponScript, "Weapon Building");
                    weaponScript.chamberedRoundLocation = ejOrig.transform;
                    weaponScript.ejectorDirection = ejDir.transform;
                    ejOrig.SetActive(false);
                    weaponScript.showEjectDirectionSetup = false;
                    weaponScript.ejectorDirectionSet = true;
                    weaponScript.slideObj.transform.localPosition = weaponScript.slideStart;
                    ejOrig = null;
                    ejDir = null;
                }
                if (GUILayout.Button("Cancel"))
                {
                    if (ejOrig != null)
                    {
                        DestroyImmediate(ejOrig.gameObject);
                        DestroyImmediate(ejDir.gameObject);
                    }
                    weaponScript.showEjectDirectionSetup = false;
                    ejOrig = null;
                    ejDir = null;
                }
                EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            }

            weaponScript.ejectorRandomness = EditorGUILayout.Slider("Ejector randomness: ", weaponScript.ejectorRandomness, 0, 1);
            weaponScript.ejectorRotationalRandomness = EditorGUILayout.Slider("Ejector Rotational randomness: ", weaponScript.ejectorRotationalRandomness, 0, 100);

            if (weaponScript.ejectorDirectionSet == true)
            {
                if (GUILayout.Button("Reset shell eject direction"))
                {
                    weaponScript.ejectorDirectionSet = false;
                    weaponScript.chamberedRoundLocation = null;
                    weaponScript.ejector = null;
                    if (weaponScript.transform.FindChild("Ejector direction").gameObject != null)
                    {
                        DestroyImmediate(weaponScript.transform.FindChild("Ejector direction").gameObject);
                    }
                }
            }
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);



            Undo.RecordObject(weaponScript, "Weapon Building");
        }
    }
}
#endregion
/*
#region Attachment window
public class AttachmentSetup : EditorWindow
{
    int index = 0, attachmentSize, totalAtPts;
    bool found = false, slideLimSetup = false, attachmentSetup;
    public Vector2 scrollPosition = Vector2.zero;
    GameObject[] attachmentZone;
    GameObject attachCfgObj, attachPtObj, attachObj, atObj;

    void OnGUI()
    {
        int count = 0;
        Weapon[] tmp = FindObjectsOfType<Weapon>();
        foreach (Weapon wpn in tmp)
        {
            if (Selection.activeGameObject != null)
            {
                if (wpn == Selection.activeGameObject.GetComponent<Weapon>() && !found)
                {
                    index = count;
                    found = true;
                }
            }
            count++;
        }
        found = true;
        count = 0;
        foreach (Weapon wpn in tmp)
            count++;
        string[] names = new string[count];
        int i = 0;
        foreach (Weapon wpn in tmp)
        {
            names[i] = wpn.name;
            i++;
        }

        index = EditorGUILayout.Popup("Weapon: ", index, names);
        Weapon weaponScript = tmp[index];

        //scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(400), GUILayout.Height(position.height - 25));
        Undo.RecordObject(weaponScript, "Weapon Building");

        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

        ////////////////////////////////////////////////////////////////////////////////////////
        totalAtPts = 0;
        foreach (VRWAttachPoint tmpAtPts in weaponScript.GetComponentsInChildren<VRWAttachPoint>())
        {
            totalAtPts++;
        }

        EditorGUILayout.TextField("Attachments set: " + totalAtPts);

        if (weaponScript.attachSet == false)
        {
            if (GUILayout.Button("Add attachment location"))
            {
                attachCfgObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                attachCfgObj.name = "Attachment zone";
                attachCfgObj.transform.parent = weaponScript.transform;
                attachCfgObj.transform.localPosition = new Vector3(0, 0, 0);
                attachCfgObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                var tempMaterial = new Material(attachCfgObj.GetComponent<Renderer>().sharedMaterial);
                tempMaterial.color = Color.gray;
                attachCfgObj.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;

                attachPtObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                attachPtObj.name = "Attachment point";
                attachPtObj.transform.parent = weaponScript.transform;
                attachPtObj.transform.localPosition = new Vector3(0, 0, 0);
                attachPtObj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                tempMaterial = new Material(attachCfgObj.GetComponent<Renderer>().sharedMaterial);
                tempMaterial.color = Color.blue;
                attachPtObj.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;

                weaponScript.showAttachSetup = true;
                Selection.activeGameObject = attachCfgObj.gameObject;

            }
        }
        if (weaponScript.showAttachSetup)
        {
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Move and resize the GREY CUBE to attachment collider.");
            EditorGUILayout.LabelField("Move  and rotate the BLUE CUBE to attachment point.");

            if (GUILayout.Button("Save attach point"))
            {
                DestroyImmediate(attachCfgObj.GetComponent<MeshRenderer>());
                DestroyImmediate(attachPtObj.GetComponent<MeshRenderer>());
                attachCfgObj.AddComponent<VRWAttachPoint>().attachPoint = attachPtObj.transform;
                attachCfgObj.GetComponent<VRWAttachPoint>().parentWeap = weaponScript.transform;
                attachCfgObj.GetComponent<BoxCollider>().isTrigger = true;

                attachCfgObj.gameObject.layer = LayerMask.NameToLayer("Weapon");
                attachPtObj.gameObject.tag = "AttachPoint";


                weaponScript.showAttachSetup = false;
                Undo.RecordObject(attachCfgObj.GetComponent<VRWAttachPoint>(), "Attachment building");
                attachCfgObj = null;
                attachPtObj = null;
                weaponScript.attachSet = true;

            }
            if (GUILayout.Button("Cancel"))
            {
                if (attachCfgObj != null)
                    DestroyImmediate(attachCfgObj.gameObject);
                if (attachPtObj != null)
                    DestroyImmediate(attachPtObj.gameObject);
                weaponScript.showAttachSetup = false;
                attachPtObj = null;
                attachCfgObj = null;
            }
        }

        if (weaponScript.attachSet == true)
        {
            if (!weaponScript.showAttachSetup)
            {
                if (GUILayout.Button("Add attachment location"))
                {
                    attachCfgObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    attachCfgObj.name = "Attachment zone";
                    attachCfgObj.transform.parent = weaponScript.transform;
                    attachCfgObj.transform.localPosition = new Vector3(0, 0, 0);
                    attachCfgObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    var tempMaterial = new Material(attachCfgObj.GetComponent<Renderer>().sharedMaterial);
                    tempMaterial.color = Color.gray;
                    attachCfgObj.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;

                    attachPtObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    attachPtObj.name = "Attachment point";
                    attachPtObj.transform.parent = weaponScript.transform;
                    attachPtObj.transform.localPosition = new Vector3(0, 0, 0);
                    attachPtObj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    tempMaterial = new Material(attachCfgObj.GetComponent<Renderer>().sharedMaterial);
                    tempMaterial.color = Color.blue;
                    attachPtObj.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;

                    weaponScript.showAttachSetup = true;
                    Selection.activeGameObject = attachCfgObj.gameObject;

                }

                if (weaponScript.showAttachSetup)
                {
                    EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                    EditorGUILayout.LabelField("Move and resize the GREY CUBE to attachment collider.");
                    EditorGUILayout.LabelField("Move and rotate the BLUE CUBE to attachment point.");

                    if (GUILayout.Button("Save attach point"))
                    {
                        DestroyImmediate(attachCfgObj.GetComponent<MeshRenderer>());
                        DestroyImmediate(attachPtObj.GetComponent<MeshRenderer>());
                        DestroyImmediate(attachPtObj.GetComponent<Collider>());

                        attachCfgObj.AddComponent<VRWAttachPoint>();
                        attachCfgObj.GetComponent<VRWAttachPoint>().attachPoint = attachPtObj.transform;
                        attachCfgObj.GetComponent<VRWAttachPoint>().parentWeap = weaponScript.transform;

                        attachCfgObj.gameObject.layer = LayerMask.NameToLayer("Weapon");
                        attachPtObj.gameObject.tag = "AttachPoint";
                        attachCfgObj.gameObject.tag = "AttachPoint";


                        weaponScript.showAttachSetup = false;
                        Undo.RecordObject(attachCfgObj.GetComponent<VRWAttachPoint>(), "Attachment building");
                        attachCfgObj = null;
                        attachPtObj = null;
                        weaponScript.attachSet = true;

                    }
                    if (GUILayout.Button("Cancel"))
                    {
                        if (attachCfgObj != null)
                            DestroyImmediate(attachCfgObj.gameObject);
                        if (attachPtObj != null)
                            DestroyImmediate(attachPtObj.gameObject);
                        weaponScript.showAttachSetup = false;
                        attachPtObj = null;
                        attachCfgObj = null;
                    }
                }
            }

            if (GUILayout.Button("Clear all attachments"))
            {
                while (weaponScript.transform.FindChild("Attachment zone") != null)
                {
                    DestroyImmediate(weaponScript.transform.FindChild("Attachment zone").gameObject);
                }
                while (weaponScript.transform.FindChild("Attachment point") != null)
                {
                    DestroyImmediate(weaponScript.transform.FindChild("Attachment point").gameObject);
                }
                weaponScript.attachSet = false;
            }

        }

        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

        EditorGUILayout.LabelField("Configure an attachment:");

        attachObj = (GameObject)EditorGUILayout.ObjectField("Attachment", attachObj, typeof(GameObject), true);

        if (!attachmentSetup)
        {
            if (GUILayout.Button("Configure"))
            {
                attachmentSetup = true;
                atObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                atObj.name = "Attachment point";
                atObj.transform.parent = attachObj.transform;
                atObj.transform.localPosition = new Vector3(0, 0, 0);
                atObj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                var tempMaterial = new Material(atObj.GetComponent<Renderer>().sharedMaterial);
                tempMaterial.color = Color.blue;
                atObj.gameObject.GetComponent<Renderer>().sharedMaterial = tempMaterial;
            }
        }
        if (attachmentSetup)
        {
            EditorGUILayout.LabelField("Move and rotate the BLUE CUBE to the attachment anchor point");
            if (GUILayout.Button("Save"))
            {
                if (attachObj.GetComponent<VRWAttachment>() == null)
                {
                    attachObj.AddComponent<VRWAttachment>();
                }
                attachObj.GetComponent<VRWAttachment>().attachPoint = atObj.transform;
                attachmentSetup = false;

                Undo.RecordObject(attachObj.GetComponent<VRWAttachment>(), "Attachment building");
                DestroyImmediate(atObj.GetComponent<MeshRenderer>());
                DestroyImmediate(atObj.GetComponent<Collider>());
                atObj = null;
                attachObj = null;
            }
        }
    }
}
    
    #endregion
    */
