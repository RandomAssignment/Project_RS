using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Character : Mob
{
    #region Unity Field
    #endregion

    /// <summary>
    /// 획득한 순서대로 사용하는 스킬 목록. Dequeue() 메소드 사용하기
    /// </summary>
    public Queue<Skill> Skills { get; private set; }

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

    public override void OnLeftRoom()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.RaiseEvent(
                (byte)PhotonEventCodes.PlayerLeft,
                new object[] { PhotonNetwork.NickName },
                // 어차피 본인은 나갈꺼라 이벤트를 자신한테 또 보낼 필요가 없음
                new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                new SendOptions { Reliability = true });
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
        }

        // 모든 화면에서 해당 오브젝트가 죽음 처리 되야 하므로 DeadRPC 실행
        photonView.RPC(nameof(DeadRPC), RpcTarget.All);
    }

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

    public void Move(Vector3 stickpos)
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Assert(_objRigidbody);
            _objRigidbody.transform.Translate(new Vector3(stickpos.x, 0, stickpos.y) * Time.deltaTime * Speed);
            photonView.RPC(nameof(FlipSpriteRPC), RpcTarget.All, stickpos.x);
        }
    }

    private void SpawnCharacter()
    {
        _skillController.gameObject.SetActive(true);
        RandomSpawnPlayer();
    }

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
