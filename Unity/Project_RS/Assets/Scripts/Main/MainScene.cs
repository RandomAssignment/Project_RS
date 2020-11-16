
using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainScene : MonoBehaviourPunCallbacks
{
    #region Unity Field
    [SerializeField]
    private byte _maxPlayersPerRoom = 8;
    #endregion

    private const string GameVersion = "1";
    private bool _isConnecting;

    private void Awake()
    {
        // Screen.SetResolution(1920, 1080, true);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Connect()
    {
        _isConnecting = true;
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
            return;
        }

        // 메인화면에서 몬스터 선택 가능
        // type 값은 Resources에 있는 프리팹 이름 사용하기
        string[] testCharacters = { "Slime", "Dummy1", "Dummy2" };
        var t = testCharacters[Random.Range(0, 3)];
        PhotonNetwork.LocalPlayer.CustomProperties = new Hashtable { ["type"] = t };
        PhotonNetwork.GameVersion = GameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() 호출됨");
        if (_isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 방 참여 실패. 새로운 방을 만듭니다.");

        var option = new RoomOptions
        {
            MaxPlayers = _maxPlayersPerRoom,
            PublishUserId = true,
            CustomRoomProperties = new Hashtable()
        };
        // TODO: PropertyConstants 라는 이름으로 const string만 모아놓은 클래스 만들기
        option.CustomRoomProperties.Add("game-start", false);
        option.CustomRoomPropertiesForLobby = new string[] { "game-start" };
        for (var i = 0; i < option.MaxPlayers; i++)
        {
            option.CustomRoomProperties.Add($"spawn{i}", false);
        }

        PhotonNetwork.CreateRoom(null, option);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Prototype");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
    }
}
