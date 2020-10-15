using Photon.Pun;

using UnityEngine;

public class Launcher : MonoBehaviour
{
    private const string GameVersion = "1";
    private string _serverName = "";
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRoom(_serverName);
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = GameVersion;
        }
    }

    public void SetServerName(string name)
    {
        _serverName = name;
    }
}
