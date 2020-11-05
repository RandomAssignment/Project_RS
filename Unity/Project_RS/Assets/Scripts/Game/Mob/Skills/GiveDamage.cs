using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveDamage : MonoBehaviour
{
    [SerializeField] int Damage;
    BaseMob attacker;

    private void Awake()
    {
        attacker = transform.root.gameObject.GetComponent<BaseMob>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && other.gameObject != transform.root.gameObject)
            other.gameObject.GetComponent<BaseMob>().HitPlayer(Damage, attacker);
    }
}
