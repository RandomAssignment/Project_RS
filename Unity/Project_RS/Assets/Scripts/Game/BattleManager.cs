using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;

/// <summary>
/// 게임에 대한 전반적인 기록 및 컨트롤을 담당
/// </summary>
public class BattleManager : MonoBehaviourPunCallbacks, IOnEventCallback
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

    /// <summary>
    /// 현재 살아남은 인원
    /// </summary>
    public int SurvivedCount
    {
        get => _survivedCount;
        private set
        {
            if (value == 1 && !IsLocalPlayerDead)
            {
                // 자신만 살아남았으므로 GameEnd 이벤트를 자신이 모두에게 보냄
                LocalPlayerRank = 1;

                PhotonNetwork.RaiseEvent(
                    (byte)PhotonEventCodes.GameEnd,
                    new object[]
                    {
                         PhotonNetwork.LocalPlayer.NickName,
                         PhotonNetwork.LocalPlayer.UserId
                    },
                    new RaiseEventOptions { Receivers = ReceiverGroup.All },
                    new SendOptions { DeliveryMode = DeliveryMode.Reliable });
            }

            _survivedCount = value;
        }
    }
    private int _survivedCount;

    /// <summary>
    /// 플레이어의 등수. 로컬 플레이어가 죽을 때 몇등인지 결정됨
    /// </summary>
    public int LocalPlayerRank { get; private set; }

    /// <summary>
    /// 로컬 플레이어의 생존 여부
    /// </summary>
    public bool IsLocalPlayerDead { get; private set; }

    /// <summary>
    /// 게임이 진행중인지 여부
    /// </summary>
    public bool IsGameInProgress { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (IsGameInProgress)
        {
            SurvivedCount--;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)PhotonEventCodes.GameStart:
                OnGameStartEvent(photonEvent);
                break;
            case (byte)PhotonEventCodes.GameEnd:
                OnGameEndEvent(photonEvent);
                break;
            case (byte)PhotonEventCodes.PlayerDead:
                OnPlayerDeadEvent(photonEvent);
                break;
            case (byte)PhotonEventCodes.PlayerLeft:
                OnPlayerLeftEvent(photonEvent);
                break;
            default:
                break;
        }
    }

    private void OnGameStartEvent(EventData _)
    {
        SurvivedCount = PhotonNetwork.CurrentRoom.PlayerCount;
        IsGameInProgress = true;
        OnGameStart?.Invoke();
    }

    private void OnGameEndEvent(EventData photonEvent)
    {
        OnGameEnd?.Invoke();

        var data = (object[])photonEvent.CustomData;
        var name = data[0];
        var id = data[1];

        print($"플레이어 {name}이 마지막으로 생존하였습니다!\nUserId: {id}");
        print($"{LocalPlayerRank}등");
        IsGameInProgress = false;
    }

    private void OnPlayerDeadEvent(EventData photonEvent)
    {
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

        // 이벤트를 보낸 플레이어가 자신이면 랭크 기록
        if (PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).IsLocal)
        {
            LocalPlayerRank = SurvivedCount;
            IsLocalPlayerDead = true;
        }
        SurvivedCount--;
    }

    private void OnPlayerLeftEvent(EventData photonEvent)
    {
        var data = (object[])photonEvent.CustomData;
        var playerName = (string)data[0];

        print($"플레이어 {playerName}가 나갔습니다.");
        SurvivedCount--;
    }
}
