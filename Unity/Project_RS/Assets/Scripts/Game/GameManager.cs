using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Unity Field
    [SerializeField]
    [Tooltip("방에 들어왔을 때 스폰되는 위치")]
    private Vector3 _characterEnteredPosition = new Vector3(0, 5, 0);

    [SerializeField]
    [Tooltip("게임을 시작했을 때 스폰되는 위치")]
    private Vector3[] _spawnPositions = new Vector3[8];
    #endregion

    public static GameManager Instance { get; private set; }

    public Vector3[] SpawnPositions => _spawnPositions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Debug.Assert(_playerPrefab != null, "플레이어 프리팹이 설정되어있지 않음");
        var playerType = (string)PhotonNetwork.LocalPlayer.CustomProperties["type"];
        Debug.Log($"Player {PhotonNetwork.NickName} : type : {playerType}");

        PhotonNetwork.Instantiate($"Prefabs/Character/{playerType}", _characterEnteredPosition, Quaternion.identity, 0);
    }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainScene");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName}이 들어왔습니다.");
        Debug.Log($"{PhotonNetwork.MasterClient.NickName}가 이 방의 방장입니다.");
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        for (var i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            if (!propertiesThatChanged.TryGetValue($"spawn{i}", out var check))
            {
                continue;
            }
            print($"{i}: {(bool)check}");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public void AddKillLog(Mob killer, Mob target)
    {
        Debug.Log($"Kill: {killer.photonView.Owner.NickName} -> {target.photonView.Owner.NickName}");
        // 데이터 추가 작업 필요
    }
}
