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

    public float tail_excitedness;

    [Header("Eyelids")]
    public float blink_timer;
    public float blink_interval;

    [Header("Wings")]
    public float wing_t;
    public List<GameObject> wing_bones;
    public float wing_speed = 0.01f;
    public float wing_mod_R = -30.0f;
    public float wing_mod_L = -30.0f;

    [Header("Contact")]
    public List<GameObject> horn_bones;
    public float[] part_lengths; //not something i wanna calc repeatedly. note that this is hardcoded which is annoying
    public List<GameObject> contact_targets;

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

        turnTarget = Camera.main.transform.position;
        eyesTarget = turnTarget;
    }

    // Update is called once per frame
    void Update()
    {
        WagTail();
        MoveEyelids();
        MoveWings();

        ContactFoldUpdate(); //must be after the other updates since its a post-effect on the other animations
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
            float wing_mod = (i == 0) ? wing_mod_R : wing_mod_L;
            float wing_rotation = (15.0f * Mathf.Sin(wing_t) + wing_mod);
            bone.transform.localEulerAngles = new Vector3(wing_rotation, bone.transform.localEulerAngles.y, bone.transform.localEulerAngles.z);
        }

        wing_t += wing_speed;

        //TODO: update wing_speed procedurally based on the velocity/acceleration of the dragon's movement
        // technically i need this to be two seperate wing_speeds for each wing but.... later problems
        wing_speed = 0.02f;
    }

    void WagTail() {


        // tail_excitedness = Mathf.Pow(Mathf.InverseLerp(tail_max_distance, tail_min_distance, target_distance), 2);


        if (tail_t > 1000.0f * (float) Math.PI) { tail_t -= 1000.0f * (float) Math.PI; }
    
        for(int i = 0; i < tail_bones.Count; i++){
            GameObject bone = tail_bones[i];

            // assume that all bones are at rotation z = 0 unless its the first one, which is z = 180. if this isnt the case we can fix it later
            float base_z = (i == 0) ? 180.0f : 0.0f ;
            float tail_rotation = (10.0f * tail_excitedness * Mathf.Sin(tail_t - (1.62f * i)));
            bone.transform.localEulerAngles = new Vector3(bone.transform.localEulerAngles.x, bone.transform.localEulerAngles.y, base_z + tail_rotation);
        }

        tail_t += tail_excitedness * 0.04f + 0.005f;

        // TODO: update tail_excitedness based on certain stimuli
        tail_excitedness = 0.75f;
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
        //TODO: fix broken animation updaters bc theyre currently not autoupdating in the other functions!! 
        //TODO: make it update to the "smallest" values supplied by each object in the contact targets list

        //tail: index 0
        float smallest_t = 1.0f;

        float smallest_w_R = 1.0f;
        float smallest_w_L = 1.0f;

        float smallest_h_R = 1.0f;
        float smallest_h_L = 1.0f;

        foreach(GameObject target in contact_targets) {
            
            //calculate tail size
            float t = 0.0f;
            foreach (GameObject bone in tail_bones) {
                float distance = Mathf.Min(Vector3.Distance(bone.transform.position, target.transform.position), part_lengths[0])/part_lengths[0];
                t += distance;
            }
            t /= 2.0f;
            smallest_t = Mathf.Min(smallest_t, t);

            
            //calculate wing fold R + L
            for (int i = 0; i < wing_bones.Count; i++) {
                //distance from base of bone
                float w = Mathf.Min(Vector3.Distance(wing_bones[i].transform.position, target.transform.position), part_lengths[1]) / part_lengths[1];
                //distance from tip of bone
                w += Mathf.Min(Vector3.Distance(wing_bones[i].transform.GetChild(0).transform.position, target.transform.position), part_lengths[1]) / part_lengths[1];
                //averaged
                w /= 2.0f;

                if (i == 0) {
                    //right wing
                    smallest_w_R = Mathf.Min(smallest_w_R, w);
                } else {
                    //left wing
                    smallest_w_L = Mathf.Min(smallest_w_L, w);
                }
            }

           //calculate horn squash
            for (int i = 0; i < horn_bones.Count; i++) {
                //distance from base of bone
                float h = Mathf.Min(Vector3.Distance(horn_bones[i].transform.position, target.transform.position), part_lengths[2]) / part_lengths[2];
                //distance from tip of bone
                h += Mathf.Min(Vector3.Distance(horn_bones[i].transform.GetChild(0).transform.position, target.transform.position), part_lengths[2]) / part_lengths[2];
                //averaged
                h /= 2.0f;
                //adjusted for scale
                h = Mathf.Lerp(-1.0f, 1.0f, h);

                if (i == 0) {
                    //right horn
                    smallest_h_R = Mathf.Min(smallest_h_R, h);
                } else {
                    //left horn
                    smallest_h_L = Mathf.Min(smallest_h_L, h);
                }
            }

        }

        //update body parts
        foreach (GameObject bone in tail_bones) {
            bone.transform.localScale = new Vector3(smallest_t, smallest_t, smallest_t);
            tail_excitedness = Mathf.Lerp(0, tail_excitedness, smallest_t);
        }
        //NOTE. these are not supposed to be hardcoded rn but im sick of adding variables
        wing_mod_R = Mathf.Lerp(30.0f, -30.0f, smallest_w_R);
        wing_mod_L = Mathf.Lerp(30.0f, -30.0f, smallest_w_L);
        if (smallest_w_R < 0.8f || smallest_w_L < 0.8f) {
            wing_speed /= 2.0f;
            wing_t = 0.0f;
        }

        for(int i = 0; i < horn_bones.Count; i++) {
            horn_bones[i].transform.localScale = (i == 0) ? new Vector3(1, smallest_h_R, 1) : new Vector3(1, smallest_h_L, 1);
        }

        
    }
}
