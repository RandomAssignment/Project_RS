using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class GameStartButton : MonoBehaviour
{
    [SerializeField]
    private Button _button = null;

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

        Debug.Assert(GameManager.Instance.SpawnPositions.Length >= PhotonNetwork.CurrentRoom.MaxPlayers);

        _button.interactable = false;
        if (PhotonNetwork.IsMasterClient)
        {
            var players = PhotonNetwork.PlayerList;
            var check = new List<(bool use, Vector3 pos)>();
            foreach (var pos in GameManager.Instance.SpawnPositions)
            {
                check.Add((false, pos));
            }
            foreach (var player in players)
            {
                int idx;
                do
                {
                    idx = Random.Range(0, check.Count);
                } while (check[idx].use);
                var cp = player.CustomProperties;
                if (cp.TryGetValue("spawn", out var _))
                {
                    cp["spawn"] = check[idx].pos;
                }
                else
                {
                    cp.Add("spawn", check[idx].pos);
                }
                player.SetCustomProperties(cp);
            }
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

        gameObject.SetActive(false);
    }
}