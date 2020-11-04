using System;
using System.Collections;

using UnityEngine;

public class Skill
{
    /// <summary>
    /// 스킬 이름
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// 스킬 설명
    /// </summary>
    public readonly string Description;

    /// <summary>
    /// 스킬의 기본 쿨타임
    /// </summary>
    public readonly int DefaultCooldown;

    /// <summary>
    /// 스킬의 현재 쿨타임
    /// </summary>
    public int Cooldown { get; set; }

    /// <summary>
    /// 스킬의 방향
    /// </summary>

    public Vector3 _skillDirection;

    /// <summary>
    /// 스킬이 현재 쿨타임인지를 반환
    /// </summary>
    public bool IsCooldown => Cooldown > 0;

    private readonly Func<IEnumerator> _logic;
    private readonly BaseMob _attacker;

    private Coroutine _executeCoroutine = null;

    /// <summary>
    /// 스킬 인스턴스를 초기화 합니다.
    /// </summary>
    /// <param name="attacker">스킬 시전자</param>
    /// <param name="name">스킬 이름</param>
    /// <param name="desc">스킬 설명</param>
    /// <param name="cooldown">스킬 쿨타임</param>
    /// <param name="logic">코루틴으로 실행되는 스킬의 로직을 설정합니다.</param>
    public Skill(BaseMob attacker, string name, string desc, int cooldown, Func<IEnumerator> logic)
    {
        Name = name;
        Description = desc;
        DefaultCooldown = cooldown;
        Cooldown = 0;
        _logic = logic;
        _attacker = attacker;
    }

    /// <summary>
    /// 스킬을 사용합니다.
    /// </summary>
    /// <param name="target">스킬을 맞는 대상</param>
    /// <param name="setCool">true일 경우 스킬의 쿨타임이 생깁니다.</param>
    public void Use(Vector3 direction)
    {
        _skillDirection = direction;
        if (!_attacker.photonView.IsMine || IsCooldown)
        {
            return;
        }

        // 스킬 중복 실행 방지
        if (_executeCoroutine != null)
        {
            return;
        }

        // Use를 사용하자마자 바로 쿨타임 적용
        Debug.Log("쿨타임 코루틴 시작");
        _attacker.StartCoroutine(DecreaseCooldown());

        Debug.Log("스킬 로직 실행");
        _attacker.StartCoroutine(ExecuteLogic());
    }

    private IEnumerator ExecuteLogic()
    {
        // 스킬이 중복으로 실행되는 경우를 막기 위해
        // _executeCoroutine를 StartCoroutine 반환값으로 설정하고
        // 스킬로직 코루틴 실행이 완료될 때 까지 기다린 후 스킬 코루틴을 다시 null로 변경
        _executeCoroutine = _attacker.StartCoroutine(_logic());
        yield return _executeCoroutine;
        _executeCoroutine = null;
    }

    /// <summary>
    /// 쿨타임을 다시 기본으로 맞추고 1초마다 줄인 후 0이 되면 실행을 종료한다.
    /// </summary>
    private IEnumerator DecreaseCooldown()
    {
        WaitForSeconds wait1sec = new WaitForSeconds(1f);
        Cooldown = DefaultCooldown;
        while (IsCooldown)
        {
            Debug.Log($"cooldown: {Cooldown}");
            yield return wait1sec;
            Cooldown--;
        }
    }
}
