using UnityEngine;

public class ObjectLookAt : MonoBehaviour
{
    private Transform _cameraTrans;

    // Start is called before the first frame update
    private void Awake()
    {
        _cameraTrans = Camera.main.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.LookAt(_cameraTrans);
    }
}
