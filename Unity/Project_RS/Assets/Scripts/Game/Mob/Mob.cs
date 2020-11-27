using System.Collections.Generic;
using Photon.Pun;

using UnityEngine;

public abstract class Mob : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Unity field
    [SerializeField]
    [Tooltip("해당 몹의 타입이름")]
    private string _typeName = string.Empty;

    [SerializeField]
    [Tooltip("몹 위에 표시되는 이름과 체력바가 포함된 UI Prefab")]
    private GameObject _infoUIPrefab = null;

    [SerializeField]
    [Tooltip("Mob Info UI Prefab의 스크린 오프셋")]
    private Vector2 _uiScreenOffset = new Vector2(0, 100);

    //[SerializeField]
    //[Tooltip("현재 체력")]
    //private int _health;

    [SerializeField]
    [Tooltip("최대 체력")]
    private int _maxHealth;

    [SerializeField]
    [Tooltip("현재 스피드")]
    private float _speed;

    // TODO: 나중에 스킬 로테이션, 삽입 기능을 담당하는 커스텀 배열 구현 필요
    [SerializeField]
    [Tooltip("몹의 기본 고유 스킬 목록")]
    private Skill[] _characterSkills = null;
    #endregion

    /// <summary>
    /// Mob Info UI GameObject
    /// </summary>
    public GameObject InfoUI => _infoUI;
    private GameObject _infoUI;

    /// <summary>
    /// 현재 체력. 입력값이 0보다 작으면 0으로 저장된다.
    /// </summary>
    public int Health
    {
        get => _health;
        set => _health = value < 0 ? 0 : value;
    }
    private int _health;

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
    /// 몹의 이름
    /// </summary>
    public string TypeName => _typeName;

    /// <summary>
    /// Mob 스프라이트
    /// </summary>
    protected SpriteRenderer MobSprite { get; private set; }

    /// <summary>
    /// 몹이 기본으로 가지는 고유 스킬목록
    /// </summary>
    /// <remarks>
    /// key: 스킬의 게임오브젝트 이름<br/>
    /// value: 스킬 게임오브젝트가 가지고 있는 Skill 인스턴스
    /// </remarks>
    protected List<Skill> _uniqueSkills;

    private Vector3 _currentPosition;
    private GameObject _sceneCamera;
    private Vector3 _sceneCameraPos;

    /// <summary>
    /// Awake 단에서 초기화해야할 코드들을 여기에 씁니다.
    /// </summary>
    protected abstract void InitializeMob();

    /// <summary>
    /// 몹이 죽었을 시 실행됩니다.
    /// </summary>
    /// <param name="attacker">해당 몹을 죽인 Mob개체</param>
    protected abstract void OnDead(Mob attacker);

    private void Awake()
    {
        MobSprite = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        _sceneCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _sceneCameraPos = _sceneCamera.transform.position;
        _uniqueSkills = new List<Skill>(10);
        foreach (var skill in _characterSkills)
        {
            _uniqueSkills.Add(skill);
        }

        if (_infoUIPrefab != null)
        {
            _infoUI = Instantiate(_infoUIPrefab);
            _infoUI.GetComponent<FloatingInfoUI>().SetTarget(this, _uiScreenOffset);
        }

        InitializeMob();
        Health = _maxHealth;
    }

    protected virtual void Update()
    {
        if(photonView.IsMine)
        {

        }
        else if ((transform.position - _currentPosition).sqrMagnitude >= 100)
        {
            transform.position = _currentPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _currentPosition, Time.deltaTime * 10);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            _sceneCamera.transform.position = Vector3.Lerp(_sceneCamera.transform.position, transform.position + _sceneCameraPos, Time.deltaTime * 8);
        }
    }

    /// <summary>
    /// Mob 스프라이트를 반전합니다.<para/>
    /// 조이스틱의 axis값을 사용합니다.
    /// </summary>
    /// <param name="flip"></param>
    [PunRPC]
    protected void FlipSpriteRPC(float axis)
    {
        if (axis == 0) // 조이스틱이 움직이지 않으면 바로 이전 상태 유지
        {
            return;
        }
        MobSprite.flipX = axis > 0;
    }

    /// <summary>
    /// Mob 스프라이트를 반전합니다.<para/>
    /// 스프라이트가 왼쪽을 바라보는 기준으로 true는 오른쪽을 바라봅니다.
    /// </summary>
    /// <param name="flip"></param>
    [PunRPC]
    protected void FlipSpriteRPC(bool flip)
    {
        MobSprite.flipX = flip;
    }

    /// <summary>
    /// 스킬을 사용했을 때 실행되는 RPC메소드입니다.
    /// </summary>
    /// <param name="skillName">스킬 이름</param>
    /// <param name="direction">스킬을 사용하는 방향</param>
    [PunRPC]
    protected virtual void UseSkillRPC(int skillcount, Vector3 direction)
    {
        Skill skill = _uniqueSkills[skillcount];
        skill.Use(direction);
    }

    /// <summary>
    /// 이름이 일치하는 스킬을 사용합니다.<para/>
    /// 만약 스킬을 찾지 못했거나 쿨타임 등으로 인해 사용하지 못하면 false를 반환합니다.
    /// </summary>
    /// <param name="skillName">스킬 이름</param>
    /// <param name="direction">스킬을 사용하는 방향</param>
    public void UseSkill(int skillcount, Vector3 direction)
    {
        photonView.RPC(nameof(UseSkillRPC), RpcTarget.All, skillcount, direction);
    }

    /// <summary>
    /// 해당 Mob에게 데미지를 줍니다.
    /// </summary>
    /// <param name="damage">데미지</param>
    /// <param name="attacker">데미지를 주는 Mob개체</param>
    public virtual void Hit(int damage, Mob attacker)
    {
        Debug.Log($"damage: {damage}, attacker: {attacker.name}");
        Health -= damage;
        if (Health == 0)
        {
            Debug.Log($"hp is 0.");
            OnDead(attacker);
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
        }
        else
        {
            _currentPosition = (Vector3)stream.ReceiveNext();
            _health = (int)stream.ReceiveNext();
            _maxHealth = (int)stream.ReceiveNext();
            _speed = (float)stream.ReceiveNext();
        }
    }
}
