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
        PhotonNetwork.RaiseEvent(
            (byte)PhotonEventCodes.GameStart,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true });

        var property = PhotonNetwork.CurrentRoom.CustomProperties;
        property["game-start"] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
        PhotonNetwork.CurrentRoom.IsOpen = false;

        gameObject.SetActive(false);
    }
}