using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Actor : NetworkBehaviour
{
    [SerializeField]
    [SyncVar]
    protected int MaxHP = 100;        // 최대 체력

    public int HPMax
    {
        get
        {
            return MaxHP;
        }
    }

    [SerializeField]
    [SyncVar]
    protected int CurrentHP;           // 현재 체력

    public int HPCurrent
    {
        get
        {
            return CurrentHP;
        }
    }

    [SerializeField]
    [SyncVar]
    protected int Damage = 1;           // 데미지

    [SerializeField]
    [SyncVar]
    protected int crashDamage = 100;    // 충돌 시 데미지

    [SerializeField]
    [SyncVar]
    protected bool isDead = false;

    public bool IsDead
    {
        get
        {
            return isDead;
        }
    }

    protected int CrashDamage
    {
        get
        {
            return crashDamage;
        }
    }

    [SyncVar]
    protected int actorInstanceID = 0;

    public int ActorInstanceID
    {
        get
        {
            return actorInstanceID;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // 초기 설정
    protected virtual void Initialize()
    {
        CurrentHP = MaxHP;

        if (isServer)
        {
            actorInstanceID = GetInstanceID();              // 객체의 고유 아이디 설정
            RpcSetActorInstanceID(actorInstanceID);         // 서버에 ID값을 전송
        }
    }


    // Update is called once per frame
    void Update()
    {
        UpdateActor();
    }

    protected virtual void UpdateActor()
    {

    }

    // 총알에 맞았을 때 호출
    public virtual void OnBulletHited(int damage, Vector3 hitPos)
    {
        Debug.Log("OnBulletHited damage = " + damage);
        DecreaseHP(damage, hitPos);
    }

    // 비행기끼리 부딛혔을 때 호출
    public virtual void OnCrash(int damage, Vector3 crashPos)
    {
        DecreaseHP(damage, crashPos);
    }

    protected virtual void DecreaseHP(int value, Vector3 damagePos)
    {
        if (isDead)
            return;

        if (isServer)     
            RpcDecreaseHP(value, damagePos);     
        // else
        // {
        //     CmdDecreaseHP(value, damagePos);
        //     if (isLocalPlayer)
        //         InternalDecreaseHP(value, damagePos);
        // }

    }

    protected virtual void InternalDecreaseHP(int value, Vector3 damagePos)
    {
        if (isDead)
            return;

        CurrentHP -= value;

        if (CurrentHP < 0)
            CurrentHP = 0;

        if (CurrentHP == 0)
        {
            OnDead();
        }
    }



    protected virtual void OnDead()
    {
        Debug.Log(name + " OnDead");
        isDead = true;

        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.GenerateEffect(EffectManager.ActorDeadFxIndex, transform.position);
    }

    public void SetPosition(Vector3 position)
    {
        if (isServer)
            RpcSetPosition(position);
        else
        {
            CmdSetPosition(position);
            if (isLocalPlayer)
                transform.position = position;
        }
    }

    [Command]
    public void CmdSetPosition(Vector3 position)
    {
        this.transform.position = position;
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcSetPosition(Vector3 position)
    {
        this.transform.position = position;
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcSetActive(bool value)
    {
        this.gameObject.SetActive(value);
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcSetActorInstanceID(int instID)
    {
        this.actorInstanceID = instID;

        if (this.actorInstanceID != 0)
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ActorManager.Regist(this.actorInstanceID, this);


        base.SetDirtyBit(1);
    }

    [Command]
    public void CmdDecreaseHP(int value, Vector3 damagePos)
    {
        InternalDecreaseHP(value, damagePos);
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcDecreaseHP(int value, Vector3 damagePos)
    {
        InternalDecreaseHP(value, damagePos);
        base.SetDirtyBit(1);
    }


    // [Command]
    // public void CmdUpdateNetworkActor()
    // {
    //     base.SetDirtyBit(1);
    // }

    // [ClientRpc]
    // public void RpcUpdateNetworkActor()
    // {
    //     base.SetDirtyBit(1);
    // }


}
