using UnityEngine;

/// <summary>
/// 게임에 대한 전반적인 기록 및 컨트롤을 담당
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
