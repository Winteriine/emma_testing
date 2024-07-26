using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactScript : MonoBehaviour
{
    public List<GameObject> bones;
    public List<Vector3> scale_transforms;
    // Start is called before the first frame update
    void Start()
    {
        bones = new List<GameObject>();
        bones.Add(GameObject.Find("Armature/Base/Tail.1"));
        bones.Add(GameObject.Find("Armature/Base/Tail.1/Tail.2"));
        bones.Add(GameObject.Find("Armature/Base/Ear.1.R"));
        bones.Add(GameObject.Find("Armature/Base/Ear.1.L"));
        bones.Add(GameObject.Find("Armature/Base/Horn.L"));
        bones.Add(GameObject.Find("Armature/Base/Horn.R"));
        scale_transforms = new List<Vector3>{
            new Vector3(1.0f, 0.5f, 1.0f),
            new Vector3(1.0f, 0.5f, 1.0f),
            new Vector3(1.0f, 0.1f, 1.0f),
            new Vector3(1.0f, 0.1f, 1.0f),
            new Vector3(1.0f, 0.01f, 1.0f),
            new Vector3(1.0f, 0.01f, 1.0f),  
        };
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < bones.Count; i++) {
            bones[i].transform.localScale = scale_transforms[i];
        }
    }
}
