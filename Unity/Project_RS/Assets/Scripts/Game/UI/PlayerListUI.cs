using System.Text;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.UI;

public class PlayerListUI : MonoBehaviourPunCallbacks
{
    #region Unity Field
    [SerializeField]
    private Text _playerListText = null;
    #endregion

    private void Start()
    {
        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        var builder = new StringBuilder();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            builder.AppendLine(
                player.UserId.Equals(PhotonNetwork.LocalPlayer.UserId)
                ? $"<color=blue>{player.NickName}</color>"
                : $"{player.NickName}");
        }
        _playerListText.text = builder.ToString();
    }
}
