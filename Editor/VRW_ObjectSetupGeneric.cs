using UnityEngine;
using UnityEditor;

namespace VRWeapons.InteractionSystems.Generic
{
    public class VRW_ObjectSetupGeneric : EditorWindow
    {
        
        enum MagazineType
        {
            Simple = 0,
            Complex = 1
        }

        enum BoltType
        {
            StraightBack = 0,
            RevolverStyle = 1
        }                

        enum BulletType
        {
            RaycastBullet = 0,
            ShotgunShell = 1,
            Projectile = 2
        }

        MagazineType magType;
        BoltType boltType;
        BulletType bulletTypeForMag, bulletType;

        bool isInteractable;
        
        GameObject target, slide, projectile;

        [MenuItem("Window/VRWeapons/Set up new Weapon")]
        private static void Init()
        {
            VRW_ObjectSetupGeneric window = (VRW_ObjectSetupGeneric)GetWindow(typeof(VRW_ObjectSetupGeneric));
            window.minSize = new Vector2(300f, 325f);
            window.maxSize = new Vector2(300f, 375f);

            window.autoRepaintOnSceneChange = true;
            window.titleContent.text = "Weapon setup";
            window.Show();
        }

        private void OnGUI()
        {
            target = Selection.activeGameObject;
            if (GUILayout.Button(new GUIContent("Add base weapon script to selected object", "")))
            {
                if (target != null)
                {
                    if (target.GetComponent<Weapon>() == null)
                    {
                        target.AddComponent<Weapon>();
                    }
                    else
                    {
                        Debug.LogWarning("Weapon already found on " + target + ". No Weapon added.");
                    }
                    target.GetComponent<AudioSource>().playOnAwake = false;
                    if (target.GetComponent<IKickActions>() == null)
                    {
                        target.AddComponent<KinematicKick>();
                    }
                    else
                    {
                        Debug.LogWarning("KinematicKick already found on " + target + ". No KinematicKick added.");
                    }
                    if (target.GetComponent<IObjectPool>() == null)
                    {
                        target.AddComponent<ObjectPool>();
                    }
                    else
                    {
                        Debug.LogWarning("ObjectPool already found on " + target + ". No ObjectPool added.");
                    }
                    
                    GameObject grabPoint;
                    if (target.transform.Find("Grab Point") == null)
                    {
                        grabPoint = new GameObject("Grab Point");
                    }
                    else
                    {
                        grabPoint = target.transform.Find("Grab Point").gameObject;
                    }
                    target.GetComponent<Weapon>().grabPoint = grabPoint.transform;

                    if (target.GetComponent<VRW_GenericIS_InteractableWeapon>() == null)
                    {
                        target.AddComponent<VRW_GenericIS_InteractableWeapon>();
                    }

                    grabPoint.transform.parent = target.transform;
                    grabPoint.transform.localPosition = Vector3.zero;
                    
                }
                if (FindObjectOfType<VRWControl>() == null)
                {
                    Debug.Log("No VRWControl found. Adding new VRWControl object to scene.");
                    GameObject tmp = new GameObject("VRWControl");
                    tmp.AddComponent<VRWControl>();
                }
            }
            if (GUILayout.Button(new GUIContent("Create new Muzzle object", "Align muzzle with weapon as desired, with the muzzle object's forward direction pointing " +
                "where the gun should fire.")))
            {
                if (target.GetComponent<Weapon>() == null)
                {
                    Debug.LogError("No Weapon script found on " + target + ". Please select an object with a valid Weapon script.");
                }
                else if (target != null && target.GetComponentInChildren<IMuzzleActions>() == null)
                {
                    GameObject muzzle = new GameObject();
                    muzzle.transform.parent = target.transform;
                    muzzle.transform.localPosition = Vector3.zero;
                    muzzle.AddComponent<Muzzle>();
                    muzzle.name = "Muzzle";
                    if (muzzle.GetComponent<AudioSource>() == null)
                    {
                        muzzle.AddComponent<AudioSource>();
                    }
                    muzzle.GetComponent<AudioSource>().playOnAwake = false;
                    Selection.activeGameObject = muzzle;
                }
                else
                {
                    Debug.LogWarning("Class implementing IMuzzleActions already found on " + target + ". No Muzzle added.");
                }
            }

            boltType = (BoltType)EditorGUILayout.EnumPopup("Bolt Type", boltType);
            if (boltType == BoltType.RevolverStyle)
            {
                EditorGUILayout.LabelField("Revolver functionality not complete.");
                EditorGUILayout.LabelField("Check back later.");
            }

            if (GUILayout.Button(new GUIContent("Assign bolt to selected object", "Bolt should be a separate object - the part that moves when the weapon fires, " +
                "that the player is able to pull back to charge the weapon. \n\nIf no bolt exists on your model, then assign to an empty GameObject and adjust slide time " +
                "as required for fire rate.")))
            {
                slide = (GameObject)Selection.activeObject;
                target = slide.GetComponentInParent<Weapon>().gameObject;
                if (slide == null)
                {
                    Debug.LogError("No GameObject selected. Please select a GameObject to add a bolt.");
                }
                else if (slide.GetComponentInParent<Weapon>() == null)
                {
                    Debug.LogError("No Weapon script found on " + slide + "'s parent objects. Please select an object that is a child of a valid Weapon script.");
                }
                else if (slide == target)
                {
                    Debug.LogError("Attempted to set up the weapon object as the bolt object. Please select desired bolt, instead of the weapon itself.");
                }
                else 
                {
                    if (boltType == BoltType.StraightBack)
                    {
                        if (slide.GetComponent<IBoltActions>() == null)
                        {
                            Bolt tmp = slide.AddComponent<Bolt>();
                            tmp.BoltEndPosition = tmp.transform.localPosition;
                            tmp.BoltStartPosition = tmp.transform.localPosition;
                            tmp.BoltRotationStart = tmp.transform.localEulerAngles;
                            tmp.BoltRotationEnd = tmp.transform.localEulerAngles;
                            tmp.bolt = slide.transform;
                        }
                        else
                        {
                            Debug.LogWarning("Class implementing IBoltActions already found on " + slide + ". No bolt added.");
                        }                        
                    }
                    else if (boltType == BoltType.RevolverStyle)
                    {
                        if (slide.GetComponent<IBoltActions>() == null)
                        {
                            slide.AddComponent<Bolt_Rotating>();
                        }
                        else
                        {
                            Debug.LogWarning("Class implementing IBoltActions already found on " + slide + ". No bolt added.");
                        }
                    }
                    if (slide.GetComponentInChildren<VRW_GenericIS_BoltInteractable>() == null)
                    {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = "Bolt Controller";
                        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        go.GetComponent<MeshRenderer>().enabled = false;
                        go.AddComponent<VRW_GenericIS_BoltInteractable>();
                        go.transform.parent = slide.GetComponentInParent<Weapon>().transform;
                        go.transform.localPosition = Vector3.zero;                        
                    }
                }
            }
            if (GUILayout.Button(new GUIContent("Create new Ejector", "Location of the ejector does not matter, but face ejector so that forward is ejection direction.")))
            {
                MonoBehaviour slide = (MonoBehaviour)target.GetComponentInChildren<IBoltActions>();
                if (slide != null)
                {
                    if (target.GetComponent<Weapon>() == null)
                    {
                        Debug.LogError("No Weapon script found on " + target + ". Please select an object with a valid Weapon script.");
                    }
                    else if (slide == null)
                    {
                        Debug.LogError("No IBoltActions script found on " + target + ". Please assign a bolt before adding an ejector.");
                    }
                    else if (target.GetComponentInChildren<IEjectorActions>() == null)
                    {
                        GameObject ejector = new GameObject();
                        ejector.name = "Ejector";
                        ejector.transform.parent = target.transform;
                        ejector.transform.localPosition = Vector3.zero;
                        ejector.AddComponent<Ejector>();
                        ejector.transform.localEulerAngles = new Vector3(-15, 90, 0);
                        Selection.activeGameObject = ejector;
                    }
                    else
                    {
                        Debug.LogWarning("Class implementing IEjectorActions already found on children of " + target + ". No ejector added.");
                    }
                }
            }
            magType = (MagazineType)EditorGUILayout.EnumPopup("Magazine Type", magType);

            if (magType == MagazineType.Simple)
            {
                bulletTypeForMag = (BulletType)EditorGUILayout.EnumPopup("Bullet Type", bulletTypeForMag);
            }

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
                    if (newMag.GetComponent<IBulletBehavior>() == null && newMag.GetComponent<IMagazine>() != null)
                    {
                        switch (bulletTypeForMag)
                        {
                            case BulletType.Projectile:
                                newMag.AddComponent<BulletTypes.ProjectileBullet>();
                                break;
                            case BulletType.RaycastBullet:
                                newMag.AddComponent<BulletTypes.RaycastBullet>();
                                break;
                            case BulletType.ShotgunShell:
                                newMag.AddComponent<BulletTypes.ShotgunBullet>();
                                break;
                        }
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

                if (newMag.GetComponent<VRW_GenericIS_Interactable>() == null)
                {
                    newMag.AddComponent<VRW_GenericIS_Interactable>();
                }

                if (newMag.GetComponent<Collider>() == null && newMag.GetComponent<Weapon>() == null)
                {
                    newMag.AddComponent<BoxCollider>();
                }

                if (newMag.GetComponent<Rigidbody>() == null)
                {
                    newMag.AddComponent<Rigidbody>();
                }                
            }
                        
            if (GUILayout.Button(new GUIContent("Set up Magazine DropZone", "Set up a dropzone for magazine insertion. Use BulletDropZone for loading individual rounds into " +
                "magazines.")))
            {
                GameObject dz;
                if (target.GetComponentInParent<Weapon>() == null)
                {
                    Debug.LogError("No Weapon script found on " + target + ". Please select an object with a valid Weapon script.");
                    dz = null;
                }
                else if (target.GetComponentInChildren<VRW_GenericIS_MagDropZone>() != null)
                {
                    Debug.LogWarning("Magazine Drop Zone already found on " + target + ". No Drop Zone added.");
                    dz = target.GetComponentInChildren<VRW_GenericIS_MagDropZone>().gameObject;
                }
                else
                {
                    dz = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    dz.name = "Magazine Drop Zone";
                    dz.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    dz.transform.parent = target.GetComponentInParent<Weapon>().transform;
                    dz.AddComponent<VRW_GenericIS_MagDropZone>();
                    dz.transform.localPosition = Vector3.zero;
                }
                if (dz != null)
                {
                    dz.GetComponent<Collider>().isTrigger = true;
                    dz.GetComponent<MeshRenderer>().enabled = false;
                }
                Selection.activeGameObject = dz;

            }
            if (GUILayout.Button(new GUIContent("Set up Bullet DropZone", "This drop zone is to add individual rounds to a complex magazine.")))
            {
                GameObject mag = (GameObject)Selection.activeObject;
                if (mag.GetComponent<IMagazine>() == null)
                {
                    Debug.LogError("No IMagazine script found on " + mag + ". Please set up magazine before setting up Bullet DropZone.");
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////NEED BULLET DROP ZONE HERE///////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////
            }
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

            GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;

            EditorGUILayout.LabelField("Bullet Builder", centeredStyle);

            bulletType = (BulletType)EditorGUILayout.EnumPopup("Bullet Type", bulletType);

            if (bulletType == BulletType.Projectile)
            {
                projectile = (GameObject)EditorGUILayout.ObjectField("Projectile Object", projectile, typeof(Object), true);
            }

            isInteractable = EditorGUILayout.Toggle("Bullet can be picked up", isInteractable);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Set Up\nBullet", "Press this button to set up a bullet for use in complex magazines.")))
            {
                if (target.GetComponent<IBulletBehavior>() == null)
                {
                    switch (bulletType)
                    {
                        case BulletType.RaycastBullet:
                            target.AddComponent<BulletTypes.RaycastBullet>();
                            break;
                        case BulletType.ShotgunShell:
                            target.AddComponent<BulletTypes.ShotgunBullet>();
                            break;
                        case BulletType.Projectile:
                            {
                                if (projectile != null)
                                {
                                    if (projectile.GetComponent<Rigidbody>() == null)
                                    {
                                        projectile.AddComponent<Rigidbody>();
                                    }
                                    target.AddComponent<BulletTypes.ProjectileBullet>();
                                    target.GetComponent<BulletTypes.ProjectileBullet>().projectile = projectile;
                                    projectile.transform.parent = target.transform;
                                    projectile.SetActive(false);
                                }
                                else
                                {
                                    Debug.LogError("Projectile round requires a projectile object. Please assign a projectile object above.", target);
                                }
                                break;
                            }
                    }
                    if (target.GetComponent<Rigidbody>() == null)
                    {
                        target.AddComponent<Rigidbody>();
                    }
                    if (target.GetComponent<Collider>() == null)
                    {
                        target.AddComponent<BoxCollider>();
                    }
                    if (isInteractable && target.GetComponent<VRW_GenericIS_Interactable>() == null)
                    {
                        target.AddComponent<VRW_GenericIS_Interactable>();
                    }
                }
            }

            if (GUILayout.Button(new GUIContent("Set Up\nEmpty Shell", "Press this button to set up an empty bullet shell, for use in ejecting spent rounds.")))
            {
                if (target.GetComponent<Rigidbody>() == null)
                {
                    target.AddComponent<Rigidbody>();
                }
                if (target.GetComponent<Collider>() == null)
                {
                    target.AddComponent<BoxCollider>();
                }
            }
            EditorGUILayout.EndHorizontal();


        }
    }
}