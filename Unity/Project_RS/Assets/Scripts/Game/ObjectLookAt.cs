using UnityEngine;

public class ObjectLookAt : MonoBehaviour
{

    // Start is called before the first frame update
    private void Awake()
    {
        transform.rotation = Quaternion.Euler(-25, -180,0);
    }

}
