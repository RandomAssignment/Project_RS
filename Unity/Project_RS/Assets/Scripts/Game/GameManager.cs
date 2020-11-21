using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Unity Field
    [SerializeField]
    [Tooltip("방에 들어왔을 때 스폰되는 위치")]
    private Vector3 _characterEnteredPosition = new Vector3(0, 5, 0);

    [SerializeField]
    [Tooltip("게임을 시작했을 때 스폰되는 위치")]
    private Vector3[] _spawnPositions = new Vector3[8];

    [SerializeField]
    private GameObject _gameOverPanel;
    #endregion

    public static GameManager Instance { get; private set; }

    public Vector3[] SpawnPositions => _spawnPositions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Debug.Assert(_playerPrefab != null, "플레이어 프리팹이 설정되어있지 않음");
        var playerType = (string)PhotonNetwork.LocalPlayer.CustomProperties["type"];
        print($"Player {PhotonNetwork.NickName} : type : {playerType}");

        PhotonNetwork.Instantiate($"Prefabs/Character/{playerType}", _characterEnteredPosition, Quaternion.identity, 0);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        BattleManager.Instance.OnGameEnd?.AddListener(OpenGameOverPanel);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        BattleManager.Instance.OnGameEnd?.RemoveListener(OpenGameOverPanel);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainScene");
    }

    private void OpenGameOverPanel()
    {
        _gameOverPanel.SetActive(true);
    }
}
