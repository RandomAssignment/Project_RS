﻿using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Unity Field
    // [SerializeField]
    // private Mob _playerPrefab = null;

    [SerializeField]
    private Vector3[] _spawnPositions = new Vector3[8];
    #endregion

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Debug.Assert(_playerPrefab != null, "플레이어 프리팹이 설정되어있지 않음");
        var playerType = (string)PhotonNetwork.LocalPlayer.CustomProperties["type"];
        Debug.Log($"Player {PhotonNetwork.NickName} : type : {playerType}");

        RandomSpawnPlayer("Prefabs/Character/" + playerType);
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

    private void RandomSpawnPlayer(string type)
    {
        var max = PhotonNetwork.CurrentRoom.MaxPlayers;

        var cp = PhotonNetwork.CurrentRoom.CustomProperties;
        // 아직 플레이어가 스폰되지 않은 지역 중 랜덤으로 찾기

        var n = -1;
        for (var i = 0; i < max; i++)
        {
            var tmp = UnityEngine.Random.Range(0, max);
            if (!(bool)cp[$"spawn{tmp}"])
            {
                n = tmp;
                break;
            }
        }

        #region 게임의 방향과 다른 코드지만 테스트용
        // NOTE: 이 코드는 나중에 인원관련해서 구현을 완료했을 때 제거해도 됨
        if (n == -1)
        {
            print("스폰할 수 있는 지점이 없어서 방을 나갑니다.");
            LeaveRoom();
            return;
        }
        #endregion

        // 해당 플레이어가 스폰할 지점의 값을 true로 바꿔서 다른 플레이어가 스폰하지 못하도록 하기
        cp[$"spawn{n}"] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(cp);

        var pos = _spawnPositions[n];
        PhotonNetwork.Instantiate(type, pos, Quaternion.identity, 0);

        Debug.Log($"Spawned position: {n}");
    }
}
