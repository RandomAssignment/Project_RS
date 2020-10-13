using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

public sealed class Slime : BaseMonster
{
    protected override void InitializeMonster()
    {
        Health = MaxHealth = 100;
        Speed = 7;

        Skill testSkill = new Skill(
            "테스트 스킬",
            "테스트 스킬이다.",
            cooldown: 5,
            PunchLogic);

        Skills = new Dictionary<string, Skill>
        {
            ["test-skill"] = testSkill
        };
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            Punch(this, taskCancellation.Token);
        }
    }

    public void Punch(BaseMonster target, CancellationToken cancellation)
    {
        Task.Run(() => Skills["test-skill"].Use(this, target, cancellation));
    }

    private void PunchLogic(BaseMonster me, BaseMonster target, CancellationToken cancellation)
    {
        target.Health -= 35;
        Debug.Log("펀치1");
        Task.Delay(5000, cancellation).Wait();
        target.Health -= 70;
        Debug.Log("펀치2");
    }
}
