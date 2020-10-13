using System.Collections.Generic;
using System.Threading;

using Photon.Pun;

using UnityEngine;

public abstract class BaseMonster : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Unity Property

    public GameObject PlayerUiPrefab;

    [SerializeField]
    [Tooltip("현재 체력")]
    private int _health;

    [SerializeField]
    [Tooltip("최대 체력")]
    private int _maxHealth;

    [SerializeField]
    [Tooltip("현재 스피드")]
    private float _speed;

    #endregion

    /// <summary>
    /// 현재 체력. 입력값이 0보다 작으면 0으로 저장된다.
    /// </summary>
    public int Health
    {
        get => _health;
        set => _health = value < 0 ? 0 : value;
    }

    /// <summary>
    /// 최대 체력. 입력값이 1보다 작으면 1로 저장된다.
    /// </summary>
    public int MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = value < 1 ? 1 : value;
    }

    /// <summary>
    /// 현재 스피드. 입력값이 0보다 작으면 0으로 저장된다.
    /// </summary>
    public float Speed
    {
        get => _speed;
        set => _speed = value < 0 ? 0 : value;
    }

    /// <summary>
    /// 스킬 목록 [skill-id, skill-instance]
    /// </summary>
    public Dictionary<string, Skill> Skills { get; protected set; }

    protected CancellationTokenSource taskCancellation;

    private GameObject _sceneCamera;
    private Vector3 _sceneCameraPos;

    // private bool isDead = false;
    private Vector3 _currentPos;

    private Rigidbody _objRigidbody;
    private SpriteRenderer _monsterSpriteRenderer;

    private GameObject _playerUI;

    private bool _isDead;

    protected abstract void InitializeMonster();

    private void Awake()
    {
        InitializeMonster();

        _sceneCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _sceneCameraPos = transform.position + _sceneCamera.transform.position;

        if (PlayerUiPrefab != null)
        {
            _playerUI = Instantiate(PlayerUiPrefab);
            _playerUI.GetComponent<PlayerUI>().SetTarget(this);
        }

        if (photonView.IsMine)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<PlayerController>().SetTarget(this);
        }
        _objRigidbody = gameObject.GetComponent<Rigidbody>();
        _monsterSpriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        taskCancellation = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        Debug.Log("실행중인 태스크 종료");
        taskCancellation.Cancel();
        taskCancellation.Dispose();
        taskCancellation = null;
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방을 나갑니다.");
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            _sceneCamera.transform.position = transform.position + _sceneCameraPos;
        }
        else if ((transform.position - _currentPos).sqrMagnitude >= 100)
        {
            transform.position = _currentPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _currentPos, Time.deltaTime * 10);
        }
    }

    /// <summary>
    /// 플레이어에게 데미지를 준다. 만약 데미지를 줘서 체력이 0이 되면 플레이어는 죽는다.
    /// </summary>
    /// <param name="damage">주는 데미지</param>
    /// <param name="attacker">공격자</param>
    public void HitPlayer(int damage, BaseMonster attacker)
    {
        if (_isDead)
        {
            return;
        }

        Health -= damage;
        Debug.Log($"damage: {damage}, attacker: {attacker.photonView.Owner.NickName}");
        if (Health == 0)
        {
            Debug.Log($"hp is 0.");
            photonView.RPC(nameof(Dead), RpcTarget.All, attacker);
            Debug.Log("RPC 보냄");
        }
    }

    public void Move(Vector3 stickpos)
    {
        _objRigidbody.velocity =
            new Vector3(
                stickpos.x,
                0,
                stickpos.y) * Time.deltaTime * Speed * 50;

        photonView.RPC(nameof(FlipX), RpcTarget.AllBuffered, stickpos.x);
    }

    [PunRPC]
    public void FlipX(float axis)
    {
        if (axis == 0) // 조이스틱이 움직이지 않으면 바로 이전 상태 유지
        {
            return;
        }
        _monsterSpriteRenderer.flipX = axis < 0;
    }

    /// <summary>
    /// 플레이어가 죽었을 때 실행
    /// </summary>
    /// <param name="attacker">공격자</param>
    [PunRPC]
    public void Dead(BaseMonster attacker)
    {
        Debug.Log("Dead RPC executed");
        Debug.Log($"attacker: {attacker.photonView.Owner.NickName}");
        gameObject.SetActive(false);
        _playerUI.SetActive(false);
        _isDead = true;

        if (attacker != null)
        {
            GameManager.Instance.AddKillLog(attacker, this);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(_health);
            stream.SendNext(_maxHealth);
            stream.SendNext(_speed);
            stream.SendNext(_isDead);
        }
        else
        {
            _currentPos = (Vector3)stream.ReceiveNext();
            _health = (int)stream.ReceiveNext();
            _maxHealth = (int)stream.ReceiveNext();
            _speed = (float)stream.ReceiveNext();
            _isDead = (bool)stream.ReceiveNext();
        }
    }
}
