using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

public sealed class Slime : BaseMonster
{
    public Slime()
    {
        Health = MaxHealth = 100;
        Speed = 7;
        ShieldGage = 10;

        Skill testSkill = new Skill(
            "테스트 스킬",
            "테스트 스킬이다.",
            cooldown: 5,
            (me, target, cancel) =>
            {
                target.Health -= 35;
                Debug.Log("펀치1");
                Task.Delay(5000, cancel).Wait();
                target.Health -= 50;
                Debug.Log("펀치2");
            });

        Skills = new Dictionary<string, Skill>
        {
            ["test-skill"] = testSkill
        };
    }

    protected override void InitializeOnStart()
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

}