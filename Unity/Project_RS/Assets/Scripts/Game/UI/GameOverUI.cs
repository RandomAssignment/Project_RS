using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    #region Unity field
    [SerializeField]
    private Text _gameOverTitle = null;

    [SerializeField]
    private Text _gameOverDescription = null;

    //[SerializeField]
    //private Button _exitButton = null;
    #endregion

    // GameOverPanel은 게임이 종료되었을 때만 활성화되므로
    // OnEnable에서 설정하기
    private void OnEnable()
    {
        SetGameOverUI();
    }

    private void SetGameOverUI()
    {
        _gameOverTitle.text = BattleManager.Instance.IsLocalPlayerDead ? "You loose..." : "You win!";
        _gameOverDescription.text = $"{BattleManager.Instance.LocalPlayerRank}등";
    }

    public void Exit()
    {
        PhotonNetwork.LeaveRoom();
    }
}