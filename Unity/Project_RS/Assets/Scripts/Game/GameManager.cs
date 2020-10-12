using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public BaseMonster PlayerPrefab;

    private void Start()
    {
        Instance = this;
        if (PlayerPrefab == null)
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
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"{PhotonNetwork.NickName}은 이 방의 방장입니다.");
            //Debug.Log("Loading level...");
            //PhotonNetwork.LoadLevel("Prototype");
        }
    }
}
