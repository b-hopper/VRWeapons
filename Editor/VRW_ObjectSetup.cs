namespace VRWeapons
{
    using UnityEngine;
    using UnityEditor;
    public class VRW_ObjectSetup : EditorWindow
    {

        enum InteractionSystems
        {
            None = 0,
            VRTK = 1,
            MoreComingSoon = 2
        }

        enum MagazineType
        {
            Simple = 0,
            Complex = 1
        }
                
        InteractionSystems interactionSystem;

        MagazineType magType;
        
        bool twoHanded;

        GameObject weaponToBuild, slide;

        [MenuItem("Window/VRWeapons/Set up new Weapon")]
        private static void Init()
        {
            VRW_ObjectSetup window = (VRW_ObjectSetup)GetWindow(typeof(VRW_ObjectSetup));
            window.minSize = new Vector2(300f, 300f);
            window.maxSize = new Vector2(300f, 400f);

            window.autoRepaintOnSceneChange = true;
            window.titleContent.text = "Weapon setup";
            window.Show();
        }

        private void OnGUI()
        {
            weaponToBuild = (GameObject)Selection.activeObject;
            interactionSystem = (InteractionSystems)EditorGUILayout.EnumPopup("Interaction System", interactionSystem);
            twoHanded = EditorGUILayout.Toggle("2-handed weapon", twoHanded);
            if (GUILayout.Button(new GUIContent("Add base weapon script to selected object", "")))
            {
                if (weaponToBuild != null)
                {
                    if (weaponToBuild.GetComponent<Weapon>() == null)
                    {
                        weaponToBuild.AddComponent<Weapon>();
                    }
                    else
                    {
                        Debug.LogWarning("Weapon already found on " + weaponToBuild + ". No Weapon added.");
                    }

                    //// Interaction systems, more to come ////
                    if (interactionSystem == InteractionSystems.VRTK)
                    {
                        if (weaponToBuild.GetComponent<Weapon_VRTK_InteractableObject>() == null)
                        {
                            Weapon_VRTK_InteractableObject tmp = weaponToBuild.AddComponent<Weapon_VRTK_InteractableObject>();
                            tmp.isUsable = true;
                            tmp.isGrabbable = true;
                            tmp.holdButtonToGrab = false;
                            tmp.holdButtonToUse = true;
                        }
                        else
                        {
                            Debug.LogWarning("Weapon_VRTK_InteractableObject already found on " + weaponToBuild + ". No Weapon_VRTK_InteractableObject added.");
                        }
                        if (weaponToBuild.GetComponent<VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach>() == null)
                        {
                            weaponToBuild.AddComponent<VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach>();
                        }
                        else
                        {
                            Debug.LogWarning("VRTK_ChildOfControllerGrabAttach already found on " + weaponToBuild + ". No VRTK_ChildOfControllerGrabAttach added.");
                        }
                        if (weaponToBuild.GetComponent<IKickActions>() == null)
                        {
                            weaponToBuild.AddComponent<KinematicKick>();
                        }
                        else
                        {
                            Debug.LogWarning("KinematicKick already found on " + weaponToBuild + ". No KinematicKick added.");
                        }
                        if (weaponToBuild.GetComponent<IObjectPool>() == null)
                        {
                            weaponToBuild.AddComponent<ObjectPool>();
                        }
                        else
                        {
                            Debug.LogWarning("ObjectPool already found on " + weaponToBuild + ". No ObjectPool added.");
                        }

                        if (twoHanded)
                        {
                            if (weaponToBuild.GetComponent<Weapon_VRTK_ControlDirectionGrabAction>() == null)
                            {
                                weaponToBuild.AddComponent<Weapon_VRTK_ControlDirectionGrabAction>();
                            }
                            else
                            {
                                Debug.LogWarning("Weapon_VRTK_ControlDirectionGrabAction already found on " + weaponToBuild + ". No Weapon_VRTK_ControlDirectionGrabAction added.");
                            }
                        }
                    }
                }
            }
            if (GUILayout.Button(new GUIContent("Create new Muzzle object", "Align muzzle with weapon as desired, with the muzzle object's forward direction pointing " +
                "where the gun should fire.")))
            {
                if (weaponToBuild.GetComponent<Weapon>() == null)
                {
                    Debug.LogError("No Weapon script found on " + weaponToBuild + ". Please select an object with a valid Weapon script.");
                }
                else if (weaponToBuild != null && weaponToBuild.GetComponentInChildren<IMuzzleActions>() == null)
                {
                    GameObject muzzle = new GameObject();
                    muzzle.transform.parent = weaponToBuild.transform;
                    muzzle.transform.localPosition = Vector3.zero;
                    muzzle.AddComponent<Muzzle>();
                    muzzle.name = "Muzzle";
                    Selection.activeGameObject = muzzle;
                }
                else
                {
                    Debug.LogWarning("Class implementing IMuzzleActions already found on " + weaponToBuild + ". No Muzzle added.");
                }
            }
            if (GUILayout.Button(new GUIContent("Assign bolt to selected object", "Bolt should be a separate object - the part that moves when the weapon fires, " +
                "that the player is able to pull back to charge the weapon. \n\nIf no bolt exists on your model, then assign to an empty GameObject and adjust slide time " +
                "as required for fire rate.")))
            {
                slide = (GameObject)Selection.activeObject;
                weaponToBuild = slide.GetComponentInParent<Weapon>().gameObject;
                if (slide == null)
                {
                    Debug.LogError("No GameObject selected. Please select a GameObject to add a bolt.");
                }
                else if (slide.GetComponentInParent<Weapon>() == null)
                {
                    Debug.LogError("No Weapon script found on " + slide + "'s parent objects. Please select an object that is a child of a valid Weapon script.");
                }
                else if (slide != null)
                {
                    if (slide.GetComponent<IBoltActions>() == null)
                    {
                        slide.AddComponent<Bolt>();
                    }
                    else
                    {
                        Debug.LogWarning("Class implementing IBoltActions already found on " + slide + ". No bolt added.");
                    }
                    if (interactionSystem == InteractionSystems.VRTK)
                    {
                        if (weaponToBuild.GetComponentInChildren<Bolt_TrackObjectGrabAttach>() == null) {
                            GameObject slideControl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            slideControl.name = "Bolt Grab Point";
                            slideControl.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                            slideControl.GetComponent<MeshRenderer>().enabled = false;
                            slideControl.transform.parent = weaponToBuild.transform;
                            slideControl.transform.localPosition = Vector3.zero;
                            Rigidbody rb = slideControl.AddComponent<Rigidbody>();
                            rb.isKinematic = true;
                            Bolt_TrackObjectGrabAttach tmp = slideControl.AddComponent<Bolt_TrackObjectGrabAttach>();
                            tmp.isGrabbable = true;
                            tmp.holdButtonToGrab = true;
                            tmp.isUsable = false;
                            Selection.activeGameObject = slideControl;
                        }
                    }
                }
            }
            if (GUILayout.Button(new GUIContent("Create new Ejector", "Location of the ejector does not matter, but face ejector so that forward is ejection direction.")))
            {
                MonoBehaviour slide = (MonoBehaviour)weaponToBuild.GetComponentInChildren<IBoltActions>();
                if (weaponToBuild.GetComponent<Weapon>() == null)
                {
                    Debug.LogError("No Weapon script found on " + weaponToBuild + ". Please select an object with a valid Weapon script.");
                }
                else if (slide == null)
                {
                    Debug.LogError("No IBoltActions script found on " + weaponToBuild + ". Please assign a bolt before adding an ejector.");                    
                }
                else if (weaponToBuild.GetComponentInChildren<IEjectorActions>() == null)
                {
                    GameObject ejector = new GameObject();
                    ejector.name = "Ejector";
                    ejector.transform.parent = weaponToBuild.transform;
                    ejector.transform.localPosition = Vector3.zero;
                    ejector.AddComponent<Ejector>();
                    ejector.transform.localEulerAngles = new Vector3(-15, 90, 0);
                    Selection.activeGameObject = ejector;
                }
                else
                {
                    Debug.LogWarning("Class implementing IEjectorActions already found on children of " + weaponToBuild + ". No ejector added.");
                }
            }
            magType = (MagazineType)EditorGUILayout.EnumPopup("Magazine Type", magType);
            if (GUILayout.Button(new GUIContent("Set up selected object as Magazine", "Simple magazines should have a single bullet type added as a component to the magazine. \n\nComplex " +
                "magazines are capable of individual bullet behavior, and require rounds to be set up in the magazine. \n\nIf weapon has an internal magazine, select the weapon and press this.")))
            {
                GameObject newMag = (GameObject)Selection.activeObject;
                if (magType == MagazineType.Simple)
                {
                    if (newMag.GetComponent<IMagazine>() == null)
                    {
                        newMag.AddComponent<SimpleMagazine>();
                        if (newMag.GetComponent<Weapon>() != null)
                        {
                            newMag.GetComponent<IMagazine>().CanMagBeDetached = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Class implementing IMagazine already found on " + newMag + ". No magazine added.");
                    }
                }
                else if (magType == MagazineType.Complex)
                {
                    if (newMag.GetComponent<IMagazine>() == null)
                    {
                        newMag.AddComponent<Magazine>();
                        if (newMag.GetComponent<Weapon>() != null)
                        {
                            newMag.GetComponent<IMagazine>().CanMagBeDetached = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Class implementing IMagazine already found on " + newMag + ". No magazine added.");
                    }
                }
            }

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Interaction System-specific setup:");

            if (interactionSystem == InteractionSystems.VRTK)
            {
                if (GUILayout.Button(new GUIContent("Set up Magazine DropZone", "For VRTK, use a dropzone for magazine insertion. Use BulletDropZone for loading individual rounds into " +
                    "magazines.")))
                {
                    if (weaponToBuild.GetComponent<Weapon>() == null)
                    {
                        Debug.LogError("No Weapon script found on " + weaponToBuild + ". Please select an object with a valid Weapon script.");
                    }
                    else if (weaponToBuild.GetComponentInChildren<MagDropZone>() != null)
                    {
                        Debug.LogWarning("Magazine drop zone already found on " + weaponToBuild + ". No drop zone created.");
                    }
                    else
                    {
                        GameObject dz = new GameObject();
                        Undo.RecordObject(dz, "Set up Magazine Drop Zone");

                        dz.name = "Mag Drop Zone";
                        dz.AddComponent<MagDropZone>();

                        dz.transform.parent = weaponToBuild.transform;
                        dz.transform.localPosition = Vector3.zero;

                        if (dz.GetComponent<VRTK.VRTK_PolicyList>() == null)
                        {
                            dz.GetComponent<MagDropZone>().validObjectListPolicy = dz.AddComponent<VRTK.VRTK_PolicyList>();
                        }
                        else
                        {
                            dz.GetComponent<MagDropZone>().validObjectListPolicy = dz.GetComponent<VRTK.VRTK_PolicyList>();
                        }

                        if (dz.GetComponent<SphereCollider>() == null)
                        {
                            dz.AddComponent<SphereCollider>();
                        }
                        dz.GetComponent<SphereCollider>().radius = 0.05f;
                    }
                }
                if (GUILayout.Button(new GUIContent("Set up Bullet DropZone", "For VRTK, this drop zone is to add individual rounds to a complex magazine.")))
                {
                    GameObject mag = (GameObject)Selection.activeObject;
                    if (mag.GetComponent<IMagazine>() == null)
                    {
                        Debug.LogError("No IMagazine script found on " + mag + ". Please set up magazine before setting up Bullet DropZone.");
                    }
                    else if (mag.GetComponentInChildren<BulletDropZone>() != null)
                    {
                        Debug.LogWarning("Bullet Drop Zone already found on " + mag + ". No drop zone added.");
                    }
                    else
                    {
                        GameObject dz = new GameObject();
                        Undo.RecordObject(dz, "Set up Bullet Drop Zone");
                        dz.name = "Bullet Drop Zone";
                        dz.AddComponent<BulletDropZone>();
                        dz.GetComponent<BulletDropZone>().validObjectListPolicy = dz.AddComponent<VRTK.VRTK_PolicyList>();
                        dz.AddComponent<BoxCollider>();
                        dz.GetComponent<BoxCollider>().size = new Vector3(0.025f, 0.035f, 0.065f);
                    }
                }
            }
        }
    }
}