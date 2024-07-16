using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class AnimationControl : MonoBehaviour
{

    public float target_distance;
    public GameObject tracked_object;
    public Animator _animator;

    [Header("Tail Components")]

    public float tail_t;
    public List<GameObject> tail_bones;

    float tail_min_distance = 2.0f;
    float tail_max_distance = 20.0f;

    float tail_fast = 0.15f;
    float tail_slow = 0.01f;

    float tail_segment_delay = 1.62f;


    //[Header("Eyelids")]
    float blink_timer;
    float blink_interval;


    [Header("Wings")]
    float wing_t;
    public List<GameObject> wing_bones;
    public float wing_speed;


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
        wing_speed = 0.01f;
        wing_bones = new List<GameObject>();
        wing_bones.Add(GameObject.Find("Armature/Base/Ear.1.L"));
        wing_bones.Add(GameObject.Find("Armature/Base/Ear.1.R"));
    }

    // Update is called once per frame
    void Update()
    {
        target_distance = Mathf.Clamp(Vector3.Distance(this.transform.position, tracked_object.transform.position), tail_min_distance, tail_max_distance);
        WagTail();
        MoveEyelids();
        MoveWings();
    }

    void MoveWings() {
        wing_t += wing_speed;

        if (wing_t > 1000.0f * (float) Math.PI) { wing_t -= 1000.0f * (float) Math.PI; }

        for(int i = 0; i < wing_bones.Count; i++){
            GameObject bone = wing_bones[i];
            float wing_rotation = (10.0f * Mathf.Sin(wing_t) - 30.0f);
            bone.transform.localEulerAngles = new Vector3(wing_rotation, bone.transform.localEulerAngles.y, bone.transform.localEulerAngles.z);
        }
    }

    void WagTail() {
        // assume that all bones are at rotation z = 0 unless its the first one, which is z = 180.
        // if this isnt the case we can fix it later

        float tail_speed = Mathf.Lerp(tail_fast, tail_slow, (target_distance - tail_min_distance) / (tail_max_distance - tail_min_distance));
        tail_t += tail_speed;

        if (tail_t > 1000.0f * (float) Math.PI) { tail_t -= 1000.0f * (float) Math.PI; }
    
        for(int i = 0; i < tail_bones.Count; i++){
            GameObject bone = tail_bones[i];
            float base_z = (i == 0) ? 180.0f : 0.0f ;
            float tail_rotation = (40.0f/target_distance * Mathf.Sin(tail_t - (tail_segment_delay * i)));
            bone.transform.localEulerAngles = new Vector3(bone.transform.localEulerAngles.x, bone.transform.localEulerAngles.y, base_z + tail_rotation);
        }
    }

    void MoveEyelids() {

        blink_timer += 0.01f;

        if (blink_timer > blink_interval) {
            blink_timer = 0.0f;
            blink_interval = UnityEngine.Random.Range(1.0f, 20.0f);
            _animator.SetTrigger("doBlink");
        }

        //_animator.SetTrigger("closeEyes");
        //_animator.SetTrigger("openEyes");
    }
}
