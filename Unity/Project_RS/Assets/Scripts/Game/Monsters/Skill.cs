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