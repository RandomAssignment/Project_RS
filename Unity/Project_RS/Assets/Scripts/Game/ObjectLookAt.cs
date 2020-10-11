using UnityEngine;

public class ObjectLookAt : MonoBehaviour
{
    private Transform CameraTrans;

    // Start is called before the first frame update
    private void Start()
    {
        CameraTrans = Camera.main.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.LookAt(CameraTrans);
    }
}
