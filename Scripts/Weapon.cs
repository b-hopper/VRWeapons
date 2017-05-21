using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

using Valve.VR.InteractionSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(LineRenderer))]

public class Weapon : MonoBehaviour
{
    #region Defining Variables
    public enum FireMode
    {
        Bullet = 0,
        Projectile = 1,
        AutoFire = 2,
        BurstFire = 3,
        Shotgun = 4,
        AutofireProj = 5
    }

    [System.Serializable]
    public struct Attack
    {
        public float damage;
        public float headshotMultiplier;
        public Vector3 origin;
        public RaycastHit hitInfo;
    }

    [System.Serializable]
    public struct InternalMagazine
    {
        public int ammo;
        public int maxAmmo;
        public bool fullOnAwake;
        public GameObject ammoObj;
    }

    public ImpactProfile impactProfile;



    public float range, ejectForce, force, fireRate, flashLife, spreadOverTimeModifier, tmpRandRange, bulletSpreadMax;
    
    public float bulletSpreadRange, kickStrength, zKickStrength, recoverSpeed, projForce, adjustForward, adjustVert, slideLeeway, slideRotTriggerValue, dropTime, lineRendererLife, ejectorRandomness, ejectorRotationalRandomness;
    public Vector3 adjustRotation, flashDirection, triggerDir, slideStart, slideEnd, mainHandGripPoint, slideRotStart, slideRotEnd, separateBoltStart, separateBoltEnd, projectileRotationalOffset;
    private float nextFire, startFire, clickTime, kickTime;
    public ushort hapticStrength;
    public int slideTime, triggerMult, feedbackTime, burst, damage, headshotMultiplier, shotgunPellets;
    Coroutine autofire;
    public bool chambered, moving, rackedBack, ableToGripAndManip, isBoltSeparate, isAttachment, clicked;
    public bool holdPtSet, showHoldPtAdj, automaticChamber, held, isLoaded, muzzleDirectionSet, ejectorDirectionSet, rldPointSet, showMuzDirectionSetup,
        showEjectDirectionSetup, showRldPointSetup, showPrefabSave, showPairMag, kickSet, showKickSetup, firing, showGripAdj, gripPtSet, infiniteAmmo,
        isManip, chamberOpen, chamberClosed, autoRackForward, slideMovesOnFiring, internalMagazine, justFired, chamberOnReload, attachSet, showAttachSetup,
        canGrip, canManip, chamberMustBeOpenToReload, gripped, separateBoltOpen, usesLineRenderer, magNeedsHeldToReload;
    public magazine mag;
    bool justManip;
    public AudioSource soundSource, shotSoundSource;
    public SteamVR_Controller.Device firingController;
    public Hand firingHand;
    public LayerMask shotMask;
    LineRenderer shotLineRenderer;

    public VRWSlideManipulation slideManipDriver;

    public ConfigurableJoint joint, offhandJoint;
    JointDrive jDrive, jZDrive;
    SoftJointLimit jLim;
    public float jDriveSpringStrenth, jDriveDamper, jZDriveStrength, jZDriveDamper;

    public AudioClip shotSound, magOut, magIn, slideBack, slideForward, noShotSound;
    //public checkVr CheckVR;
    public Transform gripPoint;

    [SerializeField]
    public GameObject[] muzzleFlash;

    public InternalMagazine intMag;

    VRWControl control;

    public int weaponType;
    public FireMode fireMode;

    Rigidbody thisRBody;
    public GameObject projectile, slideObj, holdingDevice, chamberedRound, separateBolt;

    public Transform ejector, muzzle, trigger, slide, muzzleDirection, ejectorDirection, chamberedRoundLocation;
    private Vector3 triggerRotation, triggerOrigin;
    public Vector3 magOPos, magORot, magOSca;

    public GameObject bulletShell, bullet;

    #endregion

    void Awake()
    {              // Doing some initial defining of variables, figuring things out on start

        shotLineRenderer = GetComponent<LineRenderer>();
        shotLineRenderer.enabled = false;

        control = FindObjectOfType<VRWControl>();
        if (GetComponent<VRWAttachment>() != null)
        {
            isAttachment = true;
        }

        if (GetComponentInChildren<VRWSlideManipulation>() != null)
        {
            slideManipDriver = GetComponentInChildren<VRWSlideManipulation>();
        }
        if (separateBolt != null)
        {
            isBoltSeparate = true;
        }
        chamberClosed = true;

        if (GetComponent<ConfigurableJoint>() != null)
        {
            joint = GetComponent<ConfigurableJoint>();
            joint.autoConfigureConnectedAnchor = false;
        }
        else if (GetComponent<VRWInteractableWeapon>() != null)
        {
            Debug.LogError("No Configurable Joint found on " + this.gameObject + ", please open weapon builder and configure the \"Set up main hand grip\" settings.");
        }

        jDrive.positionSpring = jDriveSpringStrenth;
        jDrive.positionDamper = jDriveDamper;
        jDrive.maximumForce = Mathf.Infinity;
        jZDrive.positionSpring = jZDriveStrength;
        jZDrive.positionDamper = jZDriveDamper;
        jZDrive.maximumForce = Mathf.Infinity;
        
        jLim.limit = 60;
        joint.angularXDrive = jDrive;
        joint.highAngularXLimit = jLim;
        joint.zDrive = jZDrive;

        soundSource = GetComponent<AudioSource>();
        shotSoundSource = Instantiate(new GameObject(), this.transform).AddComponent<AudioSource>();
        shotSoundSource.gameObject.name = "DedicatedGunshotAudioSource";
        shotSoundSource.transform.localPosition = muzzle.localPosition;
        shotSoundSource.transform.localEulerAngles = Vector3.zero;
        shotSoundSource.gameObject.layer = LayerMask.NameToLayer("Weapon");

        thisRBody = GetComponent<Rigidbody>();

        nextFire = Time.time;

        if (trigger != null)
        {
            triggerOrigin = trigger.localRotation.eulerAngles;  // For trigger pull on Vive controls
        }

        if (intMag.fullOnAwake)
        {
            intMag.ammo = intMag.maxAmmo;
        }

        if (GetComponentInChildren<magazine>() != null)
        {
            mag = GetComponentInChildren<magazine>();
        }
        else
        {
            mag = null;
        }

        if (mag != null)
        {
            Reload(mag);                        // Set up magazine for initial firing
        }
        
        Chamber();
        

        if (fireRate <= 0)
        {
            fireRate = 0.01f;
        }

        if (this.gameObject.layer != LayerMask.NameToLayer("Weapon"))
        {
            Debug.LogError("Layers not correctly assigned. Press the \"Assign Layers and Tags\" button to assign layers.");
        }



    }

    void FixedUpdate()
    {
        if (!held)
        {
            firing = false;
        }
            
        if (firing && ((fireMode == FireMode.Bullet) || (fireMode == FireMode.Projectile)))
        {
            if (Time.time > (nextFire + (fireRate * 2)))
            {
                firing = false;
            }
        }
        if (firing && (fireMode == FireMode.AutoFire))
        {
            if (Time.time - nextFire >= fireRate)
            {
                fireAuto(firingController);
            }
        }
        if (firing && (fireMode == FireMode.AutofireProj))
        {
            if (Time.time - nextFire >= fireRate)
            {
                fireAutoProj(firingController);
            }
        }
    }
        
    public void Fire()
    {
        Fire(null);
    }

    public void Fire(SteamVR_Controller.Device device)
    {
        firingController = device;
        
        if (fireMode == FireMode.Bullet && chambered && (Time.time - clickTime > fireRate))
        {   // Checks to see what weapon type the weapon is, and fires appropriately
            if (!infiniteAmmo) { chambered = false; }
            fireBullet(device);
            if (automaticChamber && justFired)
            {
                if (!internalMagazine)
                {
                    if (mag != null)
                    {
                        if (mag.ammo > 0)
                        {
                            StartCoroutine(RackBack(true));
                        }
                        else
                        {
                            StartCoroutine(RackBack(false));
                        }
                    }
                }
                else
                {
                    if (intMag.ammo > 0)
                    {
                        StartCoroutine(RackBack(true));
                    }
                    else
                    {
                        StartCoroutine(RackBack(false));
                    }
                }
            }
            clickTime = Time.time;
            firing = true;
        }
        else if (fireMode == FireMode.Projectile && chambered)
        {
            clickTime = Time.time;
            FireProj(device);
            firing = true;
        }
        else if (fireMode == FireMode.AutoFire && chambered)
        {
            tmpRandRange = bulletSpreadRange;
            startFire = Time.time;
            clickTime = Time.time;
            firing = true;
        }
        else if (fireMode == FireMode.BurstFire && chambered && !firing)
        {
            firing = true;
            clickTime = Time.time;
            autofire = StartCoroutine(fireBurst(device));
        }
        else if (fireMode == FireMode.Shotgun && chambered && (Time.time - clickTime > fireRate))
        {
            clickTime = Time.time;
            FireShotgun(device);
            if (automaticChamber)
            {
                if (!internalMagazine)
                {
                    if (mag.ammo > 0)
                        StartCoroutine(RackBack(true));
                    else
                        StartCoroutine(RackBack(false));
                }
                else
                {
                    if (intMag.ammo > 0)
                    {
                        StartCoroutine(RackBack(true));
                    }
                    else
                    {
                        StartCoroutine(RackBack(false));
                    }
                }

            }
        }
        else if (fireMode == FireMode.AutofireProj && chambered)
        {
            tmpRandRange = bulletSpreadRange;
            startFire = Time.time;
            clickTime = Time.time;
            firing = true;
        }

        else if (!chambered && !clicked && (mag == null || (!automaticChamber || (mag.ammo < 1 && intMag.ammo < 1))))
        {
            PlaySound(soundSource, 5);
            clickTime = Time.time;
        }
        clicked = true;
    }

    public void StopFiring()
    {
        if (fireMode == FireMode.BurstFire || fireMode == FireMode.AutoFire)
        {
            bulletSpreadRange = tmpRandRange;
        }
        firing = false;
        clicked = false;
    }

    public void SetFireMode(FireMode mode)
    {
        fireMode = mode;
    }
    
    void fireBullet(SteamVR_Controller.Device device)
    {
        if (Time.time - nextFire >= fireRate)
        {
            PlaySound(shotSoundSource, 0);
            if (chamberedRound != null && bulletShell != null)
            {
                DestroyImmediate(chamberedRound.gameObject);
                if (!isBoltSeparate)
                {
                    chamberedRound = Instantiate(bulletShell, chamberedRoundLocation.position, chamberedRoundLocation.rotation);
                    chamberedRound.transform.parent = slideObj.transform;
                }
                else
                {
                    chamberedRound = Instantiate(bulletShell, chamberedRoundLocation.position, chamberedRoundLocation.rotation);
                    chamberedRound.transform.parent = separateBolt.transform;
                }
                chamberedRound.layer = gameObject.layer;
                if (chamberedRound.GetComponent<Rigidbody>() != null)
                {
                    chamberedRound.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
            RaycastHit hit;
            Vector3 shotLocation = (muzzleDirection.position - muzzle.position).normalized * range + (Random.insideUnitSphere * bulletSpreadRange);
            Debug.DrawRay(muzzle.position, shotLocation, Color.red, Mathf.Infinity);
            if (Physics.Raycast(muzzle.transform.position, shotLocation, out hit, range, shotMask))
            {
                if (impactProfile != null)
                {
                    ImpactInfo impact = impactProfile.GetImpactInfo(hit);
                    GameObject cloneImpact = Instantiate(impact.GetRandomPrefab(), hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                    cloneImpact.transform.parent = hit.transform;
                }
                if (hit.transform.GetComponent<Rigidbody>() != null)
                {
                    hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(force * (muzzleDirection.position - muzzle.position).normalized, hit.point);
                }

                var attack = new Attack
                {
                    damage = damage,
                    headshotMultiplier = headshotMultiplier,
                    origin = muzzle.position,
                    hitInfo = hit
                };
                if (usesLineRenderer)
                {
                    shotLineRenderer.SetPosition(0, muzzle.position);
                    shotLineRenderer.SetPosition(1, hit.point);
                    StartCoroutine(DrawLine());
                }
                ExecuteEvents.Execute<IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(attack)));
            }
            else
            {
                if (usesLineRenderer)
                {
                    shotLineRenderer.SetPosition(0, muzzle.position);
                    shotLineRenderer.SetPosition(1, shotLocation);
                    StartCoroutine(DrawLine());
                }
            }

            if (device != null)
                StartCoroutine(forceFeedback(device));
            if (!infiniteAmmo) { chambered = false; }
            StartCoroutine(MuzzleFlash());
            StartCoroutine(Kick());
            nextFire = Time.time;
            justFired = true;
        }

    }

    IEnumerator DrawLine()
    {
        shotLineRenderer.enabled = true;
        yield return new WaitForSeconds(lineRendererLife);
        if (!firing || fireMode == FireMode.Bullet)
        {
            shotLineRenderer.enabled = false;
        }
    }

    IEnumerator MuzzleFlash()
    {

        if (muzzleFlash.Length > 0)
        {
            if (muzzleFlash[0] != null)
            {
                GameObject flash = GetRandomFlash();
                if (flash != null)
                {
                    flash = Instantiate(GetRandomFlash(), muzzle.position, Quaternion.Euler(flashDirection));
                    flash.transform.parent = this.transform;
                    flash.transform.localEulerAngles = flashDirection;
                    yield return new WaitForSeconds(flashLife);
                    Destroy(flash);
                }
            }
        }
        yield break;
    }

    GameObject GetRandomFlash()
    {
        if (muzzleFlash.Length == 0)
            return null;
        else if (muzzleFlash.Length == 1)
            return muzzleFlash[0];

        return muzzleFlash[Random.Range(0, muzzleFlash.Length)];
    }

    void DebugTest()
    {
        Debug.Log("Event system check");
    }

    void FireProj(SteamVR_Controller.Device device)
    {                           
        if (Time.time - nextFire >= fireRate)
        {
            GameObject proj = Instantiate(projectile, muzzle.transform.position, transform.rotation * Quaternion.Euler(projectileRotationalOffset));
            if (proj.GetComponent<Rigidbody>() != null)
            {
                proj.GetComponent<Rigidbody>().AddForce((muzzleDirection.position - muzzle.position).normalized * projForce, ForceMode.Impulse);
            }
            if (device != null)
                StartCoroutine(forceFeedback(device));
            nextFire = Time.time;
            StartCoroutine(MuzzleFlash());
            StartCoroutine(Kick());
            PlaySound(shotSoundSource, 0);
            justFired = true;
            if (!infiniteAmmo) { chambered = false; }
            if (automaticChamber)
            {
                if (!internalMagazine)
                {
                    if (mag != null)
                    {
                        if (mag.ammo > 0)
                            StartCoroutine(RackBack(true));
                        else
                            StartCoroutine(RackBack(false));
                    }
                }
                else
                {
                    if (intMag.ammo > 0)
                    {
                        StartCoroutine(RackBack(true));
                    }
                    else
                    {
                        StartCoroutine(RackBack(false));
                    }
                }
            }
        }
    }

    void FireShotgun(SteamVR_Controller.Device device)
    {
        if (Time.time - nextFire >= fireRate)
        {
            for (int i = 0; i < shotgunPellets; i++)
            {
                RaycastHit hit;
                Debug.DrawRay(muzzle.position, (muzzleDirection.position - muzzle.position).normalized, Color.red, Mathf.Infinity);
                if (Physics.Raycast(muzzle.transform.position, (muzzleDirection.position - muzzle.position).normalized + (Random.insideUnitSphere * bulletSpreadRange), out hit, range, shotMask))
                {
                    if (impactProfile != null)
                    {
                        ImpactInfo impact = impactProfile.GetImpactInfo(hit);
                        GameObject cloneImpact = Instantiate(impact.GetRandomPrefab(), hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                        cloneImpact.transform.parent = hit.transform;
                    }
                    if (hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForceAtPosition(force * (muzzleDirection.position - muzzle.position).normalized, hit.point);
                    }

                    var attack = new Attack
                    {
                        damage = damage,
                        headshotMultiplier = headshotMultiplier,
                        origin = muzzle.position,
                        hitInfo = hit
                    };
                    ExecuteEvents.Execute<IAttackReceiver>(hit.collider.gameObject, null, ((handler, eventData) => handler.ReceiveAttack(attack)));
                }
            }
            if (device != null)
                StartCoroutine(forceFeedback(device));
            if (!infiniteAmmo) { chambered = false; }
            StartCoroutine(MuzzleFlash());
            StartCoroutine(Kick());
            nextFire = Time.time;
            PlaySound(shotSoundSource, 0);
            justFired = true;
        }
    }

    void fireAuto(SteamVR_Controller.Device device)
    {
        bulletSpreadRange += (Time.time - startFire) * 0.1f * spreadOverTimeModifier;
        if (bulletSpreadRange > bulletSpreadMax)
            bulletSpreadRange = bulletSpreadMax;
        if (chambered && firing)
        {
            fireBullet(device);
            if (!infiniteAmmo) { chambered = false; }
            if (!internalMagazine)
            {
                if (mag != null)
                {
                    if (mag.ammo > 0)
                    {
                        StartCoroutine(RackBack(true));
                    }
                    else
                    {
                        firing = false;
                        StartCoroutine(RackBack(false));
                    }
                }
            }
            else
            {
                if (intMag.ammo > 0)
                {
                    StartCoroutine(RackBack(true));
                }
                else
                {
                    firing = false;
                    StartCoroutine(RackBack(false));
                }
            }
        }
        if (!chambered || !firing)
        {
            bulletSpreadRange = tmpRandRange;
        }
        nextFire = Time.time;

    }

    void fireAutoProj(SteamVR_Controller.Device device)
    {
        bulletSpreadRange += (Time.time - startFire) * 0.1f * spreadOverTimeModifier;
        if (bulletSpreadRange > bulletSpreadMax)
            bulletSpreadRange = bulletSpreadMax;
        if (chambered && firing)
        {
            FireProj(device);
        }
        if (!chambered || !firing)
        {
            bulletSpreadRange = tmpRandRange;
        }

    }

    IEnumerator fireBurst(SteamVR_Controller.Device device)
    {
        tmpRandRange = bulletSpreadRange;
        float startFire = Time.time;
        for (int i = 0; i < burst; i++)
        {
            bulletSpreadRange += (Time.time - startFire) * 0.1f * spreadOverTimeModifier;
            if (bulletSpreadRange > bulletSpreadMax)
                bulletSpreadRange = bulletSpreadMax;
            if (chambered)
            {
                fireBullet(device);
                if (!infiniteAmmo) { chambered = false; }
                if (!internalMagazine)
                {
                    if (mag.ammo > 0)
                        StartCoroutine(RackBack(true));
                    else
                        StartCoroutine(RackBack(false));
                }
                else
                {
                    if (intMag.ammo > 0)
                    {
                        StartCoroutine(RackBack(true));
                    }
                    else
                    {
                        StartCoroutine(RackBack(false));
                    }
                }
                yield return new WaitForSeconds(fireRate);
            }
            if (!chambered)
            {
                firing = false;
                yield break;
            }
        }
        firing = false;
        bulletSpreadRange = tmpRandRange;
    }
    
    public void TriggerPull(float angle)    // Trigger pull for VR weapons, accurately reflects trigger position
    {
            
        if (trigger != null)
        {
            triggerRotation = new Vector3(triggerOrigin.x - (angle * triggerMult * triggerDir.x), triggerOrigin.y - (angle * triggerMult * triggerDir.y), triggerOrigin.z - (angle * triggerMult * triggerDir.z));
            trigger.transform.localEulerAngles = triggerRotation;
        }
    }
    
    public void Reload(magazine newMag)
    {
        if (newMag.weaponType.Equals(this.weaponType) && (chamberOpen || !chamberMustBeOpenToReload))
        {
            if (!control.VRTKMode && firingHand != null && firingHand.otherHand.currentAttachedObject == newMag.gameObject)
            {
                firingHand.otherHand.DetachObject(newMag.gameObject);
                firingHand.otherHand.HoverUnlock(newMag.GetComponent<Interactable>());
            }
            mag = newMag;           // Uses a new magazine script with a new ammo value, effectively reloading
            mag.transform.parent = this.transform;
            mag.transform.localPosition = magOPos;
            mag.transform.localEulerAngles = magORot;
            mag.transform.localScale = magOSca;
            if (mag.transform.GetComponent<Rigidbody>() != null)
                mag.transform.GetComponent<Rigidbody>().isKinematic = true;
            if (mag.transform.GetComponent<Collider>() != null)
                mag.transform.GetComponent<Collider>().enabled = false;
            if (rackedBack && autoRackForward && chamberOnReload)         // Automatically chambers new round on reload
                StartCoroutine(RackForward(slideManipDriver.linearMapping.value, false));
            else if (!chambered && automaticChamber && chamberOnReload)
                Chamber();
            PlaySound(soundSource, 1);
            isLoaded = true;
            mag.ableToUse = false;
        }
    }

    public void Reload(VRWRound round)
    {
        if (round.weaponType == this.weaponType && intMag.ammo < intMag.maxAmmo)
        {
            if (!control.VRTKMode && firingHand != null && firingHand.otherHand.currentAttachedObject == round.gameObject)
            {
                firingHand.otherHand.DetachObject(round.gameObject);
                firingHand.otherHand.HoverUnlock(round.GetComponent<Interactable>());
            }
            intMag.ammo += round.amountPerRound;
            PlaySound(soundSource, 1);
            isLoaded = true;
            Destroy(round.gameObject);
        }
    }

    public void DropMag()
    {
        if (mag != null)
        {
            mag.dropMag();
            mag = null;
            PlaySound(soundSource, 2);
        }
    }

    public void PlaySound(AudioSource source, int clip)
    {
        source.pitch = Time.timeScale;
        switch (clip)
        {            
            case 0:
                source.clip = shotSound;
                source.Play();
                break;
            case 1:
                source.clip = magIn;
                source.Play();
                break;
            case 2:
                source.clip = magOut;
                source.Play();
                break;
            case 3:
                source.clip = slideBack;
                source.Play();
                break;
            case 4:
                source.clip = slideForward;
                source.Play();
                break;
            case 5:
                source.clip = noShotSound;
                source.Play();
                break;
        }
    }

    public IEnumerator RackBack(bool ret)          // This and RackForward are used for procedurally animated guns
    {
        

        if (slideObj != null)
        {
            if (slideMovesOnFiring && !isBoltSeparate)
            {
                moving = true;
                float amountToMove = (1 / (float)slideTime) * Time.timeScale;
                for (float i = 0; i < (slideTime / Time.timeScale); i++)
                {

                    slideManipDriver.linearMapping.value = Mathf.Lerp(0, 1, amountToMove * i);

                    yield return new WaitForFixedUpdate();
                }
                slideManipDriver.linearMapping.value = 1;
                if (automaticChamber)
                {
                    Eject();
                }
            }
            else if (slideMovesOnFiring && isBoltSeparate)
            {
                moving = true;
                float amountToMove = (1 / (float)slideTime) * Time.timeScale;
                for (float i = 0; i < (slideTime / Time.timeScale); i++)
                {

                    //slideManipDriver.linearMapping.value = Mathf.Lerp(0, 1, amountToMove * i);
                    separateBolt.transform.localPosition = Vector3.Lerp(separateBoltStart, separateBoltEnd, amountToMove * i);

                    yield return new WaitForFixedUpdate();
                }
                //slideManipDriver.linearMapping.value = 1;
                if (automaticChamber)
                {
                    Eject();
                }

            }
            else if (!slideMovesOnFiring && isAttachment)
            {
                moving = true;
                float amountToMove = (1 / (float)slideTime) * Time.timeScale;
                for (float i = 0; i < (slideTime / Time.timeScale); i++)
                {
                    Debug.Log(slideManipDriver.linearMapping.value);

                    slideManipDriver.linearMapping.value = Mathf.Lerp(0, 1, amountToMove * i);

                    yield return new WaitForFixedUpdate();
                }
                slideManipDriver.linearMapping.value = 1;
                if (automaticChamber)
                {
                    Eject();
                }
            }
        }

        moving = false;
        if (isBoltSeparate)
        {
            separateBoltOpen = true;
        }
        if (ret && slideManipDriver != null)
        {
            StartCoroutine(RackForward(slideManipDriver.linearMapping.value, false));
        }
        else if (ret)
        {
            StartCoroutine(RackForward(0, false));
        }
        rackedBack = true;
    }

    public IEnumerator RackForward(float ratio, bool manipped)
    {
        if (slideObj != null)
        {
            if ((slideMovesOnFiring || autoRackForward) && (!isBoltSeparate || manipped))
            {
                moving = true;

                float tmpSlideTime = (ratio * slideTime) / Time.timeScale;

                float amountToMove = (1 / (float)slideTime) * Time.timeScale;

                for (float i = tmpSlideTime; i > 0; i--)
                {
                    slideManipDriver.linearMapping.value = Mathf.Lerp(0, 1, amountToMove * i);

                    yield return new WaitForFixedUpdate();
                    if (slideManipDriver.linearMapping.value == 0)
                    {
                        break;
                    }
                }
                slideManipDriver.linearMapping.value = 0;
            }
            else if ((slideMovesOnFiring || autoRackForward) && isBoltSeparate)
            {
                moving = true;
                float amountToMove = (1 / (float)slideTime) * Time.timeScale;
                for (float i = 0; i < slideTime / Time.timeScale; i++)
                {
                    if (separateBoltOpen)
                    {
                        separateBolt.transform.localPosition = Vector3.Lerp(separateBoltEnd, separateBoltStart, amountToMove * i);
                    }
                    else
                    {
                        slideObj.transform.localPosition = Vector3.Lerp(slideEnd, slideStart, amountToMove * i);
                    }

                    yield return new WaitForFixedUpdate();
                }
                separateBolt.transform.localPosition = separateBoltStart;
            }

            separateBoltOpen = false;
        }
        moving = false;
        if (!chambered || infiniteAmmo)
        {
            Chamber();
        }
        rackedBack = false;

    }

    IEnumerator Kick()
    {
        if (!gripped)
        {
            joint.angularXMotion = ConfigurableJointMotion.Limited;
        }
        if (zKickStrength > 0)
        {
            joint.zMotion = ConfigurableJointMotion.Free;
        }

        if (gripped)
        {
            offhandJoint.xMotion = ConfigurableJointMotion.Free;
            offhandJoint.yMotion= ConfigurableJointMotion.Free;
            offhandJoint.zMotion = ConfigurableJointMotion.Free;
        }

        if (zKickStrength > 0)
        {
            joint.zMotion = ConfigurableJointMotion.Free;
        }

        thisRBody.AddForceAtPosition(transform.TransformDirection(Vector3.up) * kickStrength, muzzle.localPosition, ForceMode.Impulse);
        thisRBody.AddForceAtPosition(transform.TransformDirection(Vector3.back) * zKickStrength, muzzle.localPosition, ForceMode.Impulse);

        kickTime = Time.time;

        yield return new WaitForSeconds(1);

        if (Time.time - kickTime >= 1 && !gripped)
        {
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            if (gripped)
            {
                offhandJoint.xMotion = ConfigurableJointMotion.Locked;
                offhandJoint.yMotion = ConfigurableJointMotion.Locked;
                offhandJoint.zMotion = ConfigurableJointMotion.Locked;
            }
        }
    }
    
    IEnumerator forceFeedback(SteamVR_Controller.Device device)
    {
        if (device != null)
        {
            for (int i = 0; i < feedbackTime; i++)
            {
                device.TriggerHapticPulse(hapticStrength);
                yield return new WaitForFixedUpdate();
            }
        }
    }

    public void Chamber()
    {
        if (!internalMagazine && !chambered)
        {
            if (mag != null && mag.ammo > 0)
            {
                chambered = true;
                mag.ammo--;
            }
        }
        else if (!chambered)
        {
            if (intMag.ammo > 0)
            {
                chambered = true;
                intMag.ammo--;
            }
        }
        if (chambered && chamberedRoundLocation != null && bullet != null)
        {
            if (!isBoltSeparate)
            {
                chamberedRound = Instantiate(bullet, chamberedRoundLocation.position, chamberedRoundLocation.rotation);
                chamberedRound.transform.parent = slideObj.transform;
            }
            else
            {
                chamberedRound = Instantiate(bullet, chamberedRoundLocation.position, chamberedRoundLocation.rotation);
                chamberedRound.transform.parent = separateBolt.transform;
            }
            chamberedRound.layer = gameObject.layer;
            if (chamberedRound.GetComponent<Rigidbody>() != null)
            {
                chamberedRound.GetComponent<Rigidbody>().isKinematic = true;
            }
            if (chamberedRound.GetComponent<Collider>() != null)
            {
                chamberedRound.GetComponent<Collider>().enabled = false;
            }
        }
    }

    Vector3 DivideVector3(Vector3 first, Vector3 second)
    {
        Vector3 tmp = first;
        tmp.x /= second.x;
        tmp.y /= second.y;
        tmp.z /= second.z;
        Debug.Log(tmp);
        return tmp;
    }

    public void eject()
    {
        Debug.LogWarning("Weapon#eject is deprecated and will be removed in a future release. Please use Weapon#Eject instead");
        Eject();
    }

    public void Eject()
    {
        Vector3 finalEjectDirection = (ejectorDirection.position - chamberedRoundLocation.position).normalized + (Random.insideUnitSphere * ejectorRandomness);
        Rigidbody chamberedRoundRBody = chamberedRound.GetComponent<Rigidbody>();
        if (chamberedRound != null)
        {
            if (chamberedRound.GetComponent<VRWRound>() != null)
            {
                chamberedRound.GetComponent<VRWRound>().SetAbleToUse(true);
            }
            if (chamberedRoundRBody != null)
            {
                chamberedRoundRBody.isKinematic = false;
                chamberedRoundRBody.AddForce(finalEjectDirection * ejectForce, ForceMode.Impulse);
                chamberedRoundRBody.AddTorque(Random.insideUnitSphere * ejectorRotationalRandomness, ForceMode.Impulse);
            }
            chamberedRound.transform.parent = null;
            chamberedRound = null;
        }
        else if (ejector != null)
        {
            GameObject cloneShell = Instantiate(bulletShell, ejector.position, ejector.rotation);
            cloneShell.transform.parent = transform;
            if (cloneShell.GetComponent<Rigidbody>() != null)
                cloneShell.GetComponent<Rigidbody>().AddForce((ejectorDirection.position - ejector.position).normalized * ejectForce, ForceMode.Impulse);
            cloneShell.transform.parent = null;
        }
    }
    
    public static float V3InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
}
