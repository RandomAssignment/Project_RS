using System.Collections.Generic;
using Photon.Pun;
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



        print($"user name: {photonView.Controller.NickName}\nuser id: {photonView.Controller.UserId}");
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (photonView.IsMine)
        {
            BattleManager.Instance.OnGameStart.AddListener(SpawnCharacter);
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
        print("방을 나갑니다.");
    }

    protected override void OnDead(Mob attacker)
    {
        // RpcTarget.AllBuffered로 해야 다른 플레이어가 접속했을 때 자동으로 Dead RPC를 보내서 상대방 화면에서 죽음처리됨
        if (attacker is Character character)
        {
            photonView.RPC(nameof(DeadByPlayerRPC), RpcTarget.All, character.photonView.Controller.UserId);
            return;
        }
        else if (attacker is Monster monster)
        {
            photonView.RPC(nameof(DeadByMonsterRPC), RpcTarget.All, monster.name);
            return;
        }
        else
        {
            photonView.RPC(nameof(DeadRPC), RpcTarget.All);
            return;
        }
    }

    /// <summary>
    /// 플레이어가 죽었을 때 실행
    /// </summary>
    /// <param name="attackerUserID">공격자의 PhotonPlayer.UserId</param>
    [PunRPC]
    public void DeadByPlayerRPC(string attackerUserID)
    {
        if (string.IsNullOrEmpty(attackerUserID))
        {
            throw new System.ArgumentException($"'{nameof(attackerUserID)}' cannot be null or empty", nameof(attackerUserID));
        }

        // 씬 상의 플레이어들 중 공격자 UserId와 동일한 오브젝트를 가져옴
        Mob attacker = null;
        foreach (var player in FindObjectsOfType<Character>())
        {
            if (player.photonView.Controller.UserId == attackerUserID)
            {
                attacker = player;
                break;
            }
        }

        if (attacker == null)
        {
            print("attacker의 UserId와 일치하는 플레이어를 찾지 못함");
            return;
        }

        DeadRPC();
        GameManager.Instance.AddKillLog(attacker, this);
    }

    [PunRPC]
    public void DeadByMonsterRPC(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName))
        {
            throw new System.ArgumentException($"'{nameof(monsterName)}' cannot be null or empty", nameof(monsterName));
        }

        DeadRPC();
    }

    [PunRPC]
    public void DeadRPC()
    {
        gameObject.SetActive(false);
        InfoUI.SetActive(false);
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
