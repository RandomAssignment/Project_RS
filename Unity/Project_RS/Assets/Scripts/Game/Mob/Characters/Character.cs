using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Character : Mob
{
    #region Unity Field
    #endregion

    private Rigidbody _objRigidbody;
    private PlayerController _playerController;
    private SkillController _skillController;

    protected override void InitializeMob()
    {
        if (photonView.IsMine)
        {
            var playerController = GameObject.FindGameObjectWithTag("PlayerController");
            var skillController = GameObject.FindGameObjectWithTag("SkillController");

            Debug.Assert(playerController);
            Debug.Assert(skillController);

            _playerController = playerController.GetComponent<PlayerController>();
            _playerController.SetTarget(this);

            _skillController = skillController.GetComponent<SkillController>();
            _skillController.SetTarget(this);
            _skillController.gameObject.SetActive(false);
        }

        _objRigidbody = gameObject.GetComponent<Rigidbody>();

        print(
            new StringBuilder()
            .AppendLine("\n[MOB_INFO]")
            .AppendLine($"user name: {photonView.Controller.NickName}")
            .AppendLine($"user id: {photonView.Controller.UserId}")
            .Append($"type: {TypeName}"));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (photonView.IsMine)
        {
            BattleManager.Instance.OnGameStart.AddListener(SpawnCharacter);
            // 게임이 끝난 후 뭔가 설정해야 될 것이 있으면 아래 코드 활용하기
            // BattleManager.Instance.OnGameEnd.AddListener();
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (photonView.IsMine)
        {
            BattleManager.Instance.OnGameStart.RemoveListener(SpawnCharacter);
        }
    }

    public override void Hit(int damage, Mob attacker)
    {
        // 플레이어가 Hit당할 때는 때린 사람과 맞는 자신이 함께 존재하므로
        // Hit당한 자신이 HP를 수정함

        // TODO: 이거 관련해서 의논 후 수정 필요
        // 자신이 상대방에게 Hit당했을 때 자신을 Hit한 상대방의 화면에 있는 자신 오브젝트가 이를 실행해야 하느냐
        // 아니면 자신의 화면에 있는 자신 오브젝트가 이를 실행해야 하느냐
        // 근데 이렇게 하면(현행 로직) 네트워크 렉으로 스킬 동작이 늦게 보이면 상대방을 나를 때렸지만 
        // 피가 안깎이고 내 화면에서 상대방이 때렸을 때만 피가 깎여서 이렇게 하면 안될거 같음
        // 일단 차후 수정방향으로는 상대방이 나를 때리면 일단 상대방화면에서 나를 때렸을 때 피를 깎고
        // 내 화면에서는 피는 깎지 않고 스킬동작만 보이게 해야 될거 같음
        // 그러면 이 구현대로 했을 때 아래 코드는 !photonView.IsMine 일때 실행해야 됨
        // 이렇게 수정할 때 분명 다른 코드에서도 수정해야 할 부분이 있으므로 수정 시 확인해서 수정하기
        if (photonView.IsMine)
        {
            base.Hit(damage, attacker);
        }
    }

    protected override void OnDead(Mob attacker)
    {
        // 자신이 죽었을 때만 이벤트 호출하기
        if (photonView.IsMine)
        {
            var eventCode = (byte)PhotonEventCodes.PlayerDead;
            object[] contents = null;
            var eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var sendOptions = new SendOptions { Reliability = true };

            if (attacker is Character character) // attacker: player
            {
                contents = new object[]
                {
                        (int)PlayerDeadReason.DeadByPlayer,
                        PhotonNetwork.NickName,
                        character.photonView.Controller.NickName,
                        character.photonView.Controller.UserId
                };
            }
            else if (attacker is Monster monster) // attacker: monster
            {
                contents = new object[]
                {
                        (int)PlayerDeadReason.DeadByPlayer,
                        PhotonNetwork.NickName,
                        monster.TypeName
                };
            }
            else if (ReferenceEquals(this, attacker)) // suicide
            {
                contents = new object[]
                {
                    (int)PlayerDeadReason.Suicide,
                    PhotonNetwork.NickName
                };
            }
            PhotonNetwork.RaiseEvent(eventCode, contents, eventOptions, sendOptions);
            print("RaiseEvent 실행됨"); //! 아마 여기서 버그가 발생하는거 같은데 확인 필요
        }

        // 모든 화면에서 해당 오브젝트가 죽음 처리 되야 하므로 DeadRPC 실행
        photonView.RPC(nameof(DeadRPC), RpcTarget.All);
    }

    /// <summary>
    /// 캐릭터의 죽음관련 행동을 처리하는 RPC메소드입니다.
    /// </summary>
    [PunRPC]
    public void DeadRPC()
    {
        gameObject.SetActive(false);
        InfoUI.SetActive(false);

        if (photonView.IsMine)
        {
            _playerController.gameObject.SetActive(false);
            _skillController.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// RPC를 사용하여 게임 내 캐릭터를 움직입니다.
    /// </summary>
    /// <param name="stickpos">캐릭터가 움직일 방향</param>
    public void Move(Vector3 stickpos)
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Assert(_objRigidbody);
            _objRigidbody.transform.Translate(new Vector3(stickpos.x, 0, stickpos.y) * Time.deltaTime * Speed);
            photonView.RPC(nameof(FlipSpriteRPC), RpcTarget.All, stickpos.x);
        }
    }

    /// <summary>
    /// 스킬 컨트롤러를 활성화하고 캐릭터를 랜덤스폰합니다.
    /// </summary>
    private void SpawnCharacter()
    {
        _skillController.gameObject.SetActive(true);
        RandomSpawnPlayer();
    }

    /// <summary>
    /// 랜덤으로 캐릭터를 스폰합니다.
    /// </summary>
    /// <remarks>
    /// 캐릭터의 위치는 OnPhotonSerializeView로 공유되고 있으므로 OnEnable, OnDisable에서 게임시작 이벤트를
    /// photonView.IsMine일 경우에만 등록하여 방장이 게임을 시작하면 자신의 캐릭터만 이 메소드를 실행함
    /// </remarks>
    private void RandomSpawnPlayer()
    {
        var max = PhotonNetwork.CurrentRoom.MaxPlayers;
        var property = PhotonNetwork.CurrentRoom.CustomProperties;
        // 아직 플레이어가 스폰되지 않은 지역 중 랜덤으로 찾기

        var n = -1;
        for (var i = 0; i < max; i++)
        {
            var tmp = Random.Range(0, max);
            if (!(bool)property[$"spawn{tmp}"])
            {
                n = tmp;
                break;
            }
        }

        // 해당 플레이어가 스폰할 지점의 값을 true로 바꿔서 다른 플레이어가 스폰하지 못하도록 하기
        property[$"spawn{n}"] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(property);

        var pos = GameManager.Instance.SpawnPositions[n];
        transform.position = pos;

        Debug.Log($"Spawned position: {n}");
    }
}
