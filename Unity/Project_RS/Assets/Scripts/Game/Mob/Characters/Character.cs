using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Character : Mob, IPunObservable
{
    #region Unity Field
    [SerializeField]
    [Tooltip("캐릭터 위에 표시되는 이름과 체력바가 포함된 UI Prefab")]
    private GameObject _playerFloatingUiPrefab = null;
    #endregion

    /// <summary>
    /// 획득한 순서대로 사용하는 스킬 목록. Dequeue() 메소드 사용하기
    /// </summary>
    public Queue<Skill> Skills { get; private set; }

    private Rigidbody _objRigidbody;

    private GameObject _playerFloatingUI;

    protected override void InitializeMob()
    {
        if (_playerFloatingUiPrefab != null)
        {
            _playerFloatingUI = Instantiate(_playerFloatingUiPrefab);
            _playerFloatingUI.GetComponent<PlayerUI>().SetTarget(this);
        }

        if (photonView.IsMine)
        {
            var playerController = GameObject.FindGameObjectWithTag("PlayerController");
            var skillController = GameObject.FindGameObjectWithTag("SkillController");

            Debug.Assert(playerController);
            Debug.Assert(skillController);

            playerController.GetComponent<PlayerController>().SetTarget(this);
            skillController.GetComponent<SkillController>().SetTarget(this);
        }

        _objRigidbody = gameObject.GetComponent<Rigidbody>();

        print($"user name: {photonView.Controller.NickName}\nuser id: {photonView.Controller.UserId}");
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
        _playerFloatingUI.SetActive(false);
    }
}
