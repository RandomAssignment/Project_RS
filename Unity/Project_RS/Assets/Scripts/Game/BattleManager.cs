using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 게임에 대한 전반적인 기록 및 컨트롤을 담당
/// </summary>
public class BattleManager : MonoBehaviour, IOnEventCallback
{
    /// <summary>
    /// 싱글톤
    /// </summary>
    public static BattleManager Instance { get; private set; }

    /// <summary>
    /// 게임이 시작할 때 발생합니다.
    /// </summary>
    public UnityEvent OnGameStart { get; } = new UnityEvent();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        var eventCode = photonEvent.Code;
        if (eventCode == (byte)PhotonEventCodes.GameStart)
        {
            OnGameStart?.Invoke();
            return;
        }
    }
}
