using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneCameraMove : MonoBehaviour
{
    Vector3 _camerapos;
    // Start is called before the first frame update
    void Start()
    {
        _camerapos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = _camerapos + Vector3.right * Mathf.Sin(Time.time * 0.3f) * 0.1f;
    }
}
