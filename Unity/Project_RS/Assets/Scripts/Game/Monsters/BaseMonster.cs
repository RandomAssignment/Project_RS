using Photon.Pun;

using System.Collections.Generic;
using System.Threading;

using UnityEngine;

public abstract class BaseMonster : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Unity Property

    [Tooltip("현재 체력")]
    public int Health;

    [Tooltip("최대 체력")]
    public int MaxHealth;

    [Tooltip("현재 스피드")]
    public float Speed;

    [Tooltip("방어막")]
    public int ShieldGage;

    #endregion

    /// <summary>
    /// 스킬 목록 [skill-id, skill-instance]
    /// </summary>
    public Dictionary<string, Skill> Skills { get; protected set; }

    protected CancellationTokenSource taskCancellation;

    private GameObject sceneCamera;
    private Vector3 sceneCameraPos;

    // private bool isDead = false;
    private Vector3 currentPos;

    private Rigidbody objRigidbody;
    private SpriteRenderer monsterSpriteRenderer;

    protected abstract void InitializeMonster();

    private void Awake()
    {
        InitializeMonster();

        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera");
        sceneCameraPos = transform.position + sceneCamera.transform.position;

        if(photonView.IsMine)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<PlayerController>().SetTarget(this);
        }
        objRigidbody = gameObject.GetComponent<Rigidbody>();
        monsterSpriteRenderer = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
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
            sceneCamera.transform.position = transform.position + sceneCameraPos;
        }
        else if ((transform.position - currentPos).sqrMagnitude >= 100)
        {
            transform.position = currentPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, currentPos, Time.deltaTime * 10);
        }
    }

    public void Move(Vector3 stickpos)
    {
        objRigidbody.velocity = 
            new Vector3(
                stickpos.x,
                0,
                stickpos.y) * Time.deltaTime * Speed * 50;
        photonView.RPC("FlipX", RpcTarget.AllBuffered, stickpos.x);
    }

    [PunRPC]
    public void FlipX(float axis)
    {
        monsterSpriteRenderer.flipX = axis < 0;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(Health);
            stream.SendNext(MaxHealth);
            stream.SendNext(Speed);
            stream.SendNext(ShieldGage);
        }
        else
        {
            currentPos = (Vector3)stream.ReceiveNext();
            Health = (int)stream.ReceiveNext();
            MaxHealth = (int)stream.ReceiveNext();
            Speed = (float)stream.ReceiveNext();
            ShieldGage = (int)stream.ReceiveNext();
        }
    }
}
