using System;
using System.Collections;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    #region Unity field
    [SerializeField]
    [Tooltip("스킬 이름")]
    private string _name = string.Empty;

    [SerializeField]
    [Tooltip("스킬 설명")]
    private string _description = string.Empty;

    [SerializeField]
    [Tooltip("스킬의 기본 쿨타임")]
    private int _defaultCooldown = 0;
    #endregion

    /// <summary>
    /// 스킬 이름
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// 스킬 설명
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// 스킬의 기본 쿨타임
    /// </summary>
    public int DefaultCooldown => _defaultCooldown;

    /// <summary>
    /// 스킬의 현재 쿨타임
    /// </summary>
    public int Cooldown { get; set; }

    /// <summary>
    /// 스킬의 방향
    /// </summary>
    public Vector3 Direction { get; private set; }

    /// <summary>
    /// 스킬이 현재 쿨타임인지를 반환
    /// </summary>
    public bool IsCooldown => Cooldown > 0;

    /// <summary>
    /// 스킬 오브젝트가 속한 Mob 오브젝트
    /// </summary>
    public Mob Attacker { get; private set; }

    public abstract Func<IEnumerator> Logic { get; }

    private Coroutine _executeCoroutine = null;

    protected virtual void Awake()
    {
        Attacker = transform.root.gameObject.GetComponent<Mob>();
        Debug.Assert(Attacker);
    }

    /// <summary>
    /// 스킬을 사용합니다.
    /// </summary>
    /// <param name="target">스킬을 맞는 대상</param>
    /// <param name="setCool">true일 경우 스킬의 쿨타임이 생깁니다.</param>
    /// <returns>스킬 사용에 성공하면 true</returns>
    public bool Use(Vector3 direction)
    {
        Direction = direction;
        Debug.Assert(Attacker, "Attacker is null");
        Debug.Assert(Attacker.photonView, "photonView is null");
        if (!Attacker.photonView.IsMine || IsCooldown)
        {
            return false;
        }

        // 스킬 중복 실행 방지
        if (_executeCoroutine != null)
        {
            return false;
        }

        // Use를 사용하자마자 바로 쿨타임 적용
        print("쿨타임 코루틴 시작");
        Attacker.StartCoroutine(DecreaseCooldown());

        print("스킬 로직 실행");
        Attacker.StartCoroutine(ExecuteLogic());
        return true;
    }

    private IEnumerator ExecuteLogic()
    {
        // 스킬이 중복으로 실행되는 경우를 막기 위해
        // _executeCoroutine를 StartCoroutine 반환값으로 설정하고
        // 스킬로직 코루틴 실행이 완료될 때 까지 기다린 후 스킬 코루틴을 다시 null로 변경
        _executeCoroutine = Attacker.StartCoroutine(Logic());
        yield return _executeCoroutine;
        _executeCoroutine = null;
    }

    /// <summary>
    /// 쿨타임을 다시 기본으로 맞추고 1초마다 줄인 후 0이 되면 실행을 종료한다.
    /// </summary>
    private IEnumerator DecreaseCooldown()
    {
        var wait1sec = new WaitForSeconds(1f);
        Cooldown = _defaultCooldown;
        while (IsCooldown)
        {
            Debug.Log($"cooldown: {Cooldown}");
            yield return wait1sec;
            Cooldown--;
        }
    }
}
