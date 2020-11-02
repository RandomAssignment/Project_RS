﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public sealed class Dummy2 : Character
{
    protected override void InitializeMob()
    {
        MobName = "Dummy2";
        Health = MaxHealth = 100;
        Speed = 7;

        Skill testSkill = new Skill(this, "펀치", "자기 자신을 때린다 ㅋㅋ", cooldown: 10, PunchLogic);

        Skills = new Dictionary<string, Skill>
        {
            ["punch"] = testSkill
        };
    }

    //private void Start()
    //{
    //    Skills["punch"].Use();
    //}

    private IEnumerator PunchLogic()
    {
        Debug.Log("펀치1");
        HitPlayer(10, attacker: this);
        yield return new WaitForSeconds(4f);
        Debug.Log("펀치2");
        HitPlayer(1, attacker: this);
    }
}
