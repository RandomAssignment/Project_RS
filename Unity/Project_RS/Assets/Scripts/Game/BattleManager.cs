using System.Collections.Generic;
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

    /// <summary>
    /// 플레이어가 한명만 남아 게임이 끝날 때 발생합니다.
    /// </summary>
    public UnityEvent OnGameEnd { get; } = new UnityEvent();

    private const string SurviveCountKey = "survive";

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
        var eventCode = (PhotonEventCodes)photonEvent.Code;
        switch (eventCode)
        {
            case PhotonEventCodes.GameStart:
                OnGameStartEvent(photonEvent);
                break;
            case PhotonEventCodes.GameEnd:
                OnGameEndEvent(photonEvent);
                break;
            case PhotonEventCodes.PlayerDead:
                OnPlayerDeadEvent(photonEvent);
                break;
            case PhotonEventCodes.PlayerLeft:
                OnPlayerLeftEvent(photonEvent);
                break;
            default:
                break;
        }
    }

    private void OnGameStartEvent(EventData _)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var property = PhotonNetwork.CurrentRoom.CustomProperties;
            property.Add(SurviveCountKey, (int)PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.CurrentRoom.SetCustomProperties(property);
        }
        OnGameStart?.Invoke();
    }

    private void OnGameEndEvent(EventData photonEvent)
    {
        OnGameEnd?.Invoke();

        var data = (object[])photonEvent.CustomData;
        var name = data[0];
        var id = data[1];

        print($"플레이어 {name}이 마지막으로 생존하였습니다!\nUserId: {id}");
    }

    private void OnPlayerDeadEvent(EventData photonEvent)
    {
        ReduceSurviveCount();

        var data = (object[])photonEvent.CustomData;
        var reason = (PlayerDeadReason)data[0];
        var playerName = (string)data[1];
        switch (reason)
        {
            case PlayerDeadReason.Suicide:
                print($"플레이어 {playerName}가 자살했습니다.");
                break;

            case PlayerDeadReason.DeadByPlayer:
                var attackerName = (string)data[2];
                var attackerId = (string)data[3];
                print($"플레이어 {playerName}가 플레이어 {attackerName}에 의해 죽었습니다.\nUserId: {attackerId}");
                break;

            case PlayerDeadReason.DeadByMonster:
                var monsterName = (string)data[2];
                print($"플레이어 {playerName}가 몬스터 {monsterName}에 의해 죽었습니다.");
                break;

            default:
                break;
        }

        CheckGameEnd();
    }

    private void OnPlayerLeftEvent(EventData photonEvent)
    {
        ReduceSurviveCount();

        var data = (object[])photonEvent.CustomData;
        var playerName = (string)data[0];

        print($"플레이어 {playerName}가 나갔습니다.");

        CheckGameEnd();
    }

    private void ReduceSurviveCount()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var property = PhotonNetwork.CurrentRoom.CustomProperties;
            var prevCount = (int)property[SurviveCountKey];
            property[SurviveCountKey] = prevCount - 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(property);
        }
    }

    private void CheckGameEnd()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var property = PhotonNetwork.CurrentRoom.CustomProperties;
            var exist = property.TryGetValue(SurviveCountKey, out var count);
            if (exist)
            {
                var surviveCount = (int)count;
                if (surviveCount == 1)
                {
                    var (isLast, name, id) = FindLastSurviedPlayer();

                    Debug.Assert(isLast); // 테스트

                    PhotonNetwork.RaiseEvent(
                        (byte)PhotonEventCodes.GameEnd,
                        new object[]
                        {
                            name,
                            id
                        },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All },
                        new SendOptions { DeliveryMode = DeliveryMode.Reliable });

                    return;
                }
            }
        }
    }

    private (bool isLast, string name, string id) FindLastSurviedPlayer()
    {
        var players = FindObjectsOfType<Character>();

        var find = new List<Character>();
        foreach (var player in players)
        {
            if (player.gameObject.activeSelf)
            {
                find.Add(player);
            }
        }

        if (find.Count == 1)
        {
            return (true, find[0].photonView.Controller.NickName, find[0].photonView.Controller.UserId);
        }
        return (false, null, null);
    }
}
