using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGizmo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "hello.png");
    }
}
