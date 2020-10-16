using System;
using System.Threading;
using System.Threading.Tasks;

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
    /// 스킬 로직
    /// </summary>
    private Action<BaseMonster, BaseMonster, CancellationToken> Logic { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="desc"></param>
    /// <param name="cooldown"></param>
    /// <param name="logic">스킬의 로직을 설정합니다. 로직은 동기 메서드여야 합니다. aync 사용 금지</param>
    public Skill(string name, string desc, int cooldown, Action<BaseMonster, BaseMonster, CancellationToken> logic)
    {
        Name = name;
        Description = desc;
        DefaultCooldown = cooldown;
        Cooldown = 0;
        Logic = logic;
    }

    /// <summary>
    /// 스킬을 사용합니다.
    /// </summary>
    /// <param name="attacker">스킬 시전자</param>
    /// <param name="target">스킬을 맞는 대상</param>
    /// <param name="setCool">true일 경우 스킬의 쿨타임이 생깁니다.</param>
    public void Use(BaseMonster attacker, BaseMonster target, CancellationToken cancellation, bool setCool = true)
    {
        if (!attacker.photonView.IsMine)
        {
            return;
        }
        if (Cooldown > 0)
        {
            return;
        }

        try
        {
            Logic(attacker, target, cancellation);
            Debug.Log($"스킬 종료");
        }
        catch (AggregateException e) when (e.InnerException is TaskCanceledException)
        {
            Debug.Log($"스킬 중단함");
            return;
        }
        finally
        {
            // NOTE: Cancellation 관련
            // 이렇게 코드를 작성할 경우 플레이어가 게임을 나가면 정상적으로 종료되지만
            // 상대방 플레이어에 의해 스킬이 중단되거나 죽어서 스킬이 중단되면 쿨타임이 새로적용되긴하는데
            // 쿨타임이 줄어드는 Task가 작동을 하지 않게 됨.
            // 죽고나서도 쿨타임 줄어드는 것을 유지하고 싶으면 다르게 로직을 짜야 함
            // 만약 죽고나서 리스폰 시 모든 쿨타임이 초기화되게 하려면 이렇게 코드를 작성하고 플레이어 오브젝트를 삭제 후
            // 다시 생성하는것도 방법이겠지만 쿨타임이 긴 스킬을 쓰고나서 죽고 다시 쓰는 등 악용의 여지가 있음
            // 이에 대한 해결책으로는 게임 종료시 Cancel되는 CancellationToken을 매개변수로 더 받는 방법 등이 있음
            // 제일 깔끔한 방법을 찾아야 할 듯
            if (setCool)
            {
                SetCooldownToDefault();
                Task.Run(() => DecreaseCooldownAsync(cancellation));
            }
            Debug.Log($"set cooldown: {Cooldown}");
        }
    }

    /// <summary>
    /// 스킬의 현재 쿨타임을 기본 쿨타임으로 맞춥니다.
    /// </summary>
    public void SetCooldownToDefault() => Cooldown = DefaultCooldown;

    /// <summary>
    /// 1초마다 쿨타임을 줄인다.
    /// </summary>
    private async void DecreaseCooldownAsync(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, cancellation);
            }
            catch (AggregateException e) when (e.InnerException is TaskCanceledException)
            {
                return;
            }

            if (--Cooldown <= 0)
            {
                return;
            }
            Debug.Log($"cooldown: {Cooldown}");
        }
    }
}
