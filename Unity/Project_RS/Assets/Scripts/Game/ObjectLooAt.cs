using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLookAt : MonoBehaviour
{

    Transform CameraTrans;
    // Start is called before the first frame update
    void Start()
    {
        CameraTrans = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(CameraTrans);
    }
}
