using UnityEngine;

public class ObjectGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "hello.png");
    }
}
