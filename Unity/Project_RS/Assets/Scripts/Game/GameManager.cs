using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Unity Field
    [SerializeField]
    private BaseMob _playerPrefab;
    #endregion

    public static GameManager Instance;

    private void Start()
    {
        Instance = this;
        if (_playerPrefab == null)
        {
            Debug.LogError("플레이어 프리팹이 설정되어있지 않음. GameManager에서 설정하셈");
            return;
        }
        string playerType = (string)PhotonNetwork.LocalPlayer.CustomProperties["type"];
        Debug.Log($"Player {PhotonNetwork.NickName} : type : {playerType}");
        PhotonNetwork.Instantiate(playerType, Vector3.zero, Quaternion.identity, 0);
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
}
