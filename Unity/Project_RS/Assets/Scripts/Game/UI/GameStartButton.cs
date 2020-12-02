using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameStartButton : MonoBehaviour
{
    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
        }
    }

    public void GameStart()
    {
        // 현재 플레이어가 1명 일 때 시작하지 못하도록 막기
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            print("플레이어가 2명 이상이어야 합니다.");
            return;
        }

        PhotonNetwork.RaiseEvent(
            (byte)PhotonEventCodes.GameStart,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true });

        var property = PhotonNetwork.CurrentRoom.CustomProperties;
        property["game-start"] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
        PhotonNetwork.CurrentRoom.IsOpen = false;

        GameManager.Instance.GameStart();

        gameObject.SetActive(false);
    }
}