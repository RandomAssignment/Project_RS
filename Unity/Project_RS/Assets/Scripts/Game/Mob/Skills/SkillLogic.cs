using System;
using System.Collections;
using UnityEngine;

[Serializable]
public abstract class SkillLogic : MonoBehaviour
{
    public abstract Func<IEnumerator> Logic { get; }
}