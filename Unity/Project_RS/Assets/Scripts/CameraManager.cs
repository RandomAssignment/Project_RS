using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    Transform Target;
    Vector3 TargetPos;
    void Start()
    {
        Target = GameObject.FindGameObjectWithTag("Player").transform;
        TargetPos = Target.position + transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Target.position + TargetPos;
    }
}
