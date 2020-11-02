
using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Unity Field
    [SerializeField]
    private BaseMob _playerPrefab;

    [SerializeField]
    private Vector3[] _spawnPositions = new Vector3[8];
    #endregion

    public static GameManager Instance;

    private void Start()
    {
        Instance = this;
        Debug.Assert(_playerPrefab != null, "플레이어 프리팹이 설정되어있지 않음");
        string playerType = (string)PhotonNetwork.LocalPlayer.CustomProperties["type"];
        Debug.Log($"Player {PhotonNetwork.NickName} : type : {playerType}");

        RandomSpawnPlayer(playerType);
    }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnLeftRoom() => SceneManager.LoadScene("MainScene");

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName}이 들어왔습니다.");
        Debug.Log($"{PhotonNetwork.MasterClient.NickName}가 이 방의 방장입니다.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public void AddKillLog(BaseMob killer, BaseMob target)
    {
        Debug.Log($"Kill: {killer.photonView.Owner.NickName} -> {target.photonView.Owner.NickName}");
        // 데이터 추가 작업 필요
    }

    private void RandomSpawnPlayer(string type)
    {
        byte max = PhotonNetwork.CurrentRoom.MaxPlayers;

        System.Random randomize = new System.Random();
        int n;
        // 아직 플레이어가 스폰되지 않은 지역 중 랜덤으로 찾기
        do
        {
            n = randomize.Next(max);
        } while ((bool)PhotonNetwork.CurrentRoom.CustomProperties[$"spawn{n}"]);

        if (n < 0 || n >= _spawnPositions.Length || _spawnPositions[n] == null)
        {
            Debug.Assert(false, $"_spawnPositions array 요소의 갯수가 부족하거나 비어있음");
        }

        // 해당 플레이어가 스폰할 지점의 값을 true로 바꿔서 다른 플레이어가 스폰하지 못하도록 하기
        PhotonNetwork.CurrentRoom.CustomProperties[$"spawn{n}"] = true;

        Vector3 pos = _spawnPositions[n];
        PhotonNetwork.Instantiate(type, pos, Quaternion.identity, 0);
        Debug.Log($"Spawned position: {n}");
    }
}
