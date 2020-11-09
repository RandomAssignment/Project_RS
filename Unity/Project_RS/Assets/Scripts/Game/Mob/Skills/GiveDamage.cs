using UnityEngine;

public class GiveDamage : MonoBehaviour
{
    [SerializeField]
    private int _damage = 0;

    private BaseMob _attacker;

    private void Awake()
    {
        _attacker = transform.root.gameObject.GetComponent<BaseMob>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject != transform.root.gameObject)
            other.gameObject.GetComponent<BaseMob>().HitPlayer(_damage, _attacker);
    }
}
