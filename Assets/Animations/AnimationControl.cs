using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class AnimationControl : MonoBehaviour
{

    public Animator _animator;

    [Header("Tail Components")]

    public float tail_t;
    public List<GameObject> tail_bones;

    public float tail_excitedness = 0.5f;

    [Header("Eyelids")]
    public float blink_timer;
    public float blink_interval;

    [Header("Wings")]
    public float wing_t;
    public List<GameObject> wing_bones;
    public float wing_speed = 0.01f;
    public float wing_mod = -30.0f;

    [Header("Contact")]
    public List<GameObject> horn_bones;
    public float[] part_lengths; //not something i wanna calc repeatedly. note that this is hardcoded which is annoying
    public GameObject contactTarget;

    [Header("Tracking")]
    public Transform headBone;
    public Transform leftEyeBone;
    public Transform rightEyeBone;
    public float turnSpeed;

    //[HideInInspector]
    public Vector3 turnTarget;
    //[HideInInspector]
    public Vector3 eyesTarget;


    // Start is called before the first frame update
    void Start()
    {

        _animator = GetComponent<Animator>();
        if (_animator == null) { Debug.LogError("_animator is not assigned."); }

        tail_t = 0.0f;
        tail_bones = new List<GameObject>();
        tail_bones.Add(GameObject.Find("Armature/Base/Tail.1"));
        tail_bones.Add(GameObject.Find("Armature/Base/Tail.1/Tail.2"));

        blink_timer = 0.0f;
        blink_interval = 2.0f;

        wing_t = 0.0f;
        wing_speed = 0.0f;
        wing_bones = new List<GameObject>();
        wing_bones.Add(GameObject.Find("Armature/Base/Ear.1.L"));
        wing_bones.Add(GameObject.Find("Armature/Base/Ear.1.R"));

        horn_bones = new List<GameObject>();
        horn_bones.Add(GameObject.Find("Armature/Base/Horn.R"));
        horn_bones.Add(GameObject.Find("Armature/Base/Horn.L"));

        //bone length order is always tail, wings, horns
        part_lengths = new float[3];
        part_lengths[0] = Vector3.Distance(GameObject.Find("Armature/Base/Tail.1/Tail.2").transform.position, GameObject.Find("Armature/Base/Tail.1/Tail.2/Tail.2_end").transform.position);
        part_lengths[1] = Vector3.Distance(GameObject.Find("Armature/Base/Ear.1.L").transform.position, GameObject.Find("Armature/Base/Ear.1.L/Ear.1.L_end").transform.position);
        part_lengths[2] = Vector3.Distance(GameObject.Find("Armature/Base/Horn.R").transform.position, GameObject.Find("Armature/Base/Horn.R/Horn.R_end").transform.position);
        
        // UNCOMMENT IF ON MAIN PROJECT ITS JUST BROKEN FOR ME
        //turnTarget = Camera.main.transform.position;
        //eyesTarget = turnTarget;
    }

    // Update is called once per frame
    void Update()
    {
        WagTail();
        MoveEyelids();
        MoveWings();

        ContactFoldUpdate(); //must be after the other updates since its a post-effect
    }

    void LateUpdate()
    {
        HeadTrackingUpdate();
        EyeTrackingUpdate();
    }

    void MoveWings()
    {

        if (wing_t > 1000.0f * (float) Math.PI) { wing_t -= 1000.0f * (float) Math.PI; }

        for(int i = 0; i < wing_bones.Count; i++) {
            GameObject bone = wing_bones[i];
            float wing_rotation = (15.0f * Mathf.Sin(wing_t) + wing_mod);
            bone.transform.localEulerAngles = new Vector3(wing_rotation, bone.transform.localEulerAngles.y, bone.transform.localEulerAngles.z);
        }

        wing_t += wing_speed;
    }

    void WagTail() {
        // assume that all bones are at rotation z = 0 unless its the first one, which is z = 180.
        // if this isnt the case we can fix it later

        // tail_excitedness = Mathf.Pow(Mathf.InverseLerp(tail_max_distance, tail_min_distance, target_distance), 2);


        if (tail_t > 1000.0f * (float) Math.PI) { tail_t -= 1000.0f * (float) Math.PI; }
    
        for(int i = 0; i < tail_bones.Count; i++){
            GameObject bone = tail_bones[i];
            float base_z = (i == 0) ? 180.0f : 0.0f ;
            float tail_rotation = (10.0f * tail_excitedness * Mathf.Sin(tail_t - (1.62f * i)));
            bone.transform.localEulerAngles = new Vector3(bone.transform.localEulerAngles.x, bone.transform.localEulerAngles.y, base_z + tail_rotation);
        }

        tail_t += tail_excitedness * 0.04f + 0.005f;
    }

    void MoveEyelids() {
        //TODO: include state for closed and open eyes, which will disable the blinking animation timer
        blink_timer += 0.01f;

        if (blink_timer > blink_interval) {
            blink_timer = 0.0f;
            blink_interval = UnityEngine.Random.Range(0.5f, 7.0f);
            _animator.SetTrigger("doBlink");
        }
    }

    // animation triggers for later

    //_animator.SetTrigger("doBite");
    //_animator.SetTrigger("doBlink");
    //_aninator.SetBool("isPanting");
    //_aninator.SetBool("eyesClosed");
    //_aninator.SetBool("mouthOpen");

    void HeadTrackingUpdate()
    {
        // Store the current head rotation since we will be resetting it
        Quaternion currentLocalRotation = headBone.localRotation;
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation.
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        headBone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = turnTarget - headBone.position;
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
            Vector3.forward,
            targetLocalLookDir,
            2.0f * Mathf.PI, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
            0 // We don't care about the length here, so we leave it at zero
        );

        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        // Apply smoothing
        headBone.localRotation = Quaternion.Slerp(
            currentLocalRotation,
            targetLocalRotation,
            0.2f * turnSpeed + 0.005f
        );
    }

    void EyeTrackingUpdate()
    {

        Vector3 targetLookDir = Vector3.RotateTowards(
            headBone.forward,
            eyesTarget - headBone.position,
            Mathf.Deg2Rad * 40, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
            0 // We don't care about the length here, so we leave it at zero
        );

        Quaternion targetRotation = Quaternion.LookRotation(targetLookDir, Vector3.up);

        targetRotation = Quaternion.Slerp(
            leftEyeBone.rotation,
            targetRotation,
            0.2f
        );

        leftEyeBone.rotation = targetRotation;
        rightEyeBone.rotation = targetRotation;
    }

    void ContactFoldUpdate() {
        //TODO: fix broken animation updaters bc theyre currently not autoupdating in the other functions

        //tail: index 0
        float t = 0.0f;
        foreach (GameObject bone in tail_bones) {
            float distance = Mathf.Min(Vector3.Distance(bone.transform.position, contactTarget.transform.position), part_lengths[0]) /part_lengths[0];
            t += distance;
        }
        t /= 2.0f;
        foreach (GameObject bone in tail_bones) {
            bone.transform.localScale = new Vector3(t, t, t);
        }

        //wings: 1
        //TODO: currently doesn't work on both wings because a) wing_mod affects both of them, and b) i forget the other reason but this is a monday problem methinks
        foreach (GameObject bone in wing_bones) {
            float w = Mathf.Min(Vector3.Distance(bone.transform.position, contactTarget.transform.position), part_lengths[1]) / part_lengths[1];
            w += Mathf.Min(Vector3.Distance(bone.transform.GetChild(0).transform.position, contactTarget.transform.position), part_lengths[1]) / part_lengths[1];
            w /= 2.0f;
            //bone.transform.localScale = new Vector3(w, w, w);
            wing_mod = Mathf.Lerp(30.0f, -30.0f, w);
        }

        //horns: 2
        foreach (GameObject bone in horn_bones) {
            float h = Mathf.Min(Vector3.Distance(bone.transform.position, contactTarget.transform.position), part_lengths[2]) / part_lengths[2];
            h += Mathf.Min(Vector3.Distance(bone.transform.GetChild(0).transform.position, contactTarget.transform.position), part_lengths[2]) / part_lengths[2];
            h /= 2.0f;

            h = Mathf.Lerp(-1.0f, 1.0f, h);
            bone.transform.localScale = new Vector3(1, h, 1);
            //bone.transform.localEulerAngles = new Vector3(300 + h * 90.0f, 300 + h * 90.0f, bone.transform.localEulerAngles.z);
        }
    }
}
