using System;
using System.Collections;
using UnityEngine;

public sealed class Punch : Skill
{
    [SerializeField]
    private int _damage = 0;

    private Collider _hitBox;

    protected override void Awake()
    {
        base.Awake();
        _hitBox = GetComponent<Collider>();
    }

    public override Func<IEnumerator> Logic => PunchLogic;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject != transform.root.gameObject)
        {
            other.gameObject.GetComponent<Mob>().Hit(_damage, Attacker);
        }
    }

    private IEnumerator PunchLogic()
    {
        print("펀치로직실행");
        Debug.Assert(Attacker.photonView);
        // NOTE: RPC는 UseSkill에서 사용했으므로 스킬에서 RPC를 사용할 필요는 없습니다.
        // e.g. SkillController.cs의 23번째 줄
        PunchOn();
        yield return new WaitForSeconds(0.2f);
        PunchOff();
    }

    private void PunchOn()
    {
        _hitBox.enabled = true;
        transform.localPosition = Direction;
    }

    private void PunchOff()
    {
        _hitBox.enabled = false;
    }
}
