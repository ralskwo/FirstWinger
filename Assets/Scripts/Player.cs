﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class Player : Actor
{
    const string PlayerHUDPath = "Prefabs/PlayerHUD";

    [SerializeField]
    [SyncVar]
    Vector3 MoveVector = Vector3.zero;      // 플레이어의 움직임

    [SerializeField]
    NetworkIdentity NetworkIdentity = null;

    [SerializeField]
    float Speed;                            // 플레이어 속도

    [SerializeField]
    BoxCollider boxCollider;                // 플레이어 collider

    [SerializeField]
    Transform FireTransform;                // 발사 위치

    [SerializeField]
    float BulletSpeed = 1;                  // 발사 속도

    InputController inputController = new InputController();

    [SerializeField]
    [SyncVar]
    bool Host = false;                      // Host 플레이어인지 여부

    [SerializeField]
    Material ClientPlayerMaterial;

    [SerializeField]
    [SyncVar]
    int UsableItemCount = 0;

    public int ItemCount
    {
        get
        {
            return UsableItemCount;
        }
    }

    protected override void Initialize()
    {
        base.Initialize();


        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();

        if (isLocalPlayer)
            inGameSceneMain.Hero = this;
        else
            inGameSceneMain.OtherPlayer = this;

        if (isServer && isLocalPlayer)
        {
            Host = true;
            RpcSetHost();
        }

        if (!Host)
        {
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = ClientPlayerMaterial;
        }

        if (actorInstanceID != 0)
            inGameSceneMain.ActorManager.Regist(actorInstanceID, this);

        InitializePlayerHUD();
    }

    void InitializePlayerHUD()
    {
        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        GameObject go = Resources.Load<GameObject>(PlayerHUDPath);
        GameObject goInstance = Instantiate<GameObject>(go, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity, inGameSceneMain.DamageManager.CanvasTransform);
        PlayerHUD playerHUD = goInstance.GetComponent<PlayerHUD>();
        playerHUD.Initialize(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("OnStartClient");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("OnStartLocalPlayer");
    }


    // Update is called once per frame 
    protected override void UpdateActor()
    {
        if (!isLocalPlayer)
            return;

        UpdateInput();
        UpdateMove();
    }

    [ClientCallback]
    public void UpdateInput()
    {
        inputController.UpdateInput();
    }

    void UpdateMove()
    {
        // sqrMagnitude: 두 점간의 거리의 제곱에 루트를 한 값. 두 점 간의 거리의 차이를 2차원 함수값으로 계산해준다.
        if (MoveVector.sqrMagnitude == 0)
            return;

        // // 화면 바깥으로 나가지 않게 하기 위한 부분
        // MoveVector = AdjustMoveVector(MoveVector);
        // // transform.position += MoveVector;
        // CmdMove(MoveVector);

        // MonoBehaviour 인스턴스의 Update로 호출되어 실행되고 있을 때의 꼼수
        // 이 경우 클라이언트로 접속하면 Command로 보내지지만 자기 자신은 CmdMㅐve를 실행하지 못함
        if (isServer)
        {
            // Host 플레이어인 경우 Rpc로 보내고
            RpcMove(MoveVector);
        }
        else
        {
            // Client 플레이어인 경우 Cmd로 호스트로 보낸 후 자신을 Self 동작
            CmdMove(MoveVector);
            if (isLocalPlayer)
                transform.position += AdjustMoveVector(MoveVector);
        }

    }

    [Command]
    public void CmdMove(Vector3 moveVector)
    {
        this.MoveVector = moveVector;
        transform.position += AdjustMoveVector(this.MoveVector);
        base.SetDirtyBit(1);
        this.MoveVector = Vector3.zero;         // 타 플레이어가 보낸 경우 update를 통해 초기화되지 않으므로 사용 후 바로 초기화
    }

    [ClientRpc]
    public void RpcMove(Vector3 moveVector)
    {
        this.MoveVector = moveVector;
        transform.position += AdjustMoveVector(this.MoveVector);
        base.SetDirtyBit(1);
        this.MoveVector = Vector3.zero;         // 타 플레이어가 보낸 경우 update를 통해 초기화되지 않으므로 사용 후 바로 초기화
    }


    public void ProcessInput(Vector3 moveDirection)
    {
        if (!isLocalPlayer)
            return;

        MoveVector = moveDirection * Speed * Time.deltaTime;
    }

    Vector3 AdjustMoveVector(Vector3 moveVector)
    {
        Transform mainBGQuadTransform = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().MainBGQuadTransform;
        Vector3 result = Vector3.zero;

        // 현재 플레이어(boxCollider)의 위치
        result = boxCollider.transform.position + boxCollider.center + moveVector;

        // 맵의 왼쪽을 넘어가지 않는 코드
        if (result.x - boxCollider.size.x * 0.5f < -mainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;

        // 맵의 오른쪽을 넘어가지 않는 코드
        if (result.x + boxCollider.size.x * 0.5f > mainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;

        // 맵의 아래쪽을 넘어가지 않는 코드
        if (result.y - boxCollider.size.y * 0.5f < -mainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;

        // // 맵의 위쪽을 넘어가지 않는 코드
        if (result.y + boxCollider.size.y * 0.5f > mainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;

        return moveVector;
    }


    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy)
        {
            if (!enemy.IsDead)
            {
                BoxCollider box = ((BoxCollider)other);
                Vector3 crashPos = enemy.transform.position + box.center;
                crashPos.x += box.size.x * 0.5f;

                enemy.OnCrash(CrashDamage, crashPos);
            }
        }
    }

    //발사 함수
    public void Fire()
    {
        if (Host)
        {
            Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBulletIndex, FireTransform.position);
            bullet.Fire(actorInstanceID, FireTransform.right, BulletSpeed, Damage);
        }
        else
        {
            CmdFire(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        }
    }

    [Command]
    public void CmdFire(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBulletIndex, firePosition);
        bullet.Fire(ownerInstanceID, direction, speed, damage);
        base.SetDirtyBit(1);
    }

    public void FireBomb()
    {
        if (UsableItemCount <= 0)
            return;

        if (Host)
        {
            Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBombIndex, FireTransform.position);
            bullet.Fire(actorInstanceID, FireTransform.right, BulletSpeed, Damage);
        }
        else
        {
            CmdFireBomb(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        }
        DecreaseUsableItemCount();
    }

    [Command]
    public void CmdFireBomb(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBombIndex, firePosition);
        bullet.Fire(ownerInstanceID, direction, speed, damage);
        base.SetDirtyBit(1);
    }

    void DecreaseUsableItemCount()
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdDecreaseUsableItemCount();

        // MonoBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때의 꼼수
        if (isServer)
        {
            RpcDecreaseUsableItemCount();        // Host 플레이어인경우 RPC로 보내고
        }
        else
        {
            CmdDecreaseUsableItemCount();        // Client 플레이어인경우 Cmd로 호스트로 보낸후 자신을 Self 동작
            if (isLocalPlayer)
                UsableItemCount--;
        }
    }

    [Command]
    public void CmdDecreaseUsableItemCount()
    {
        UsableItemCount--;
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcDecreaseUsableItemCount()
    {
        UsableItemCount--;
        base.SetDirtyBit(1);
    }

    protected override void DecreaseHP(int value, Vector3 damagePos)
    {
        base.DecreaseHP(value, damagePos);

        Vector3 damagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.Generate(DamageManager.PlayerDamageIndex, damagePoint, value, Color.red);
    }


    protected override void OnDead()
    {
        base.OnDead();
        gameObject.SetActive(false);
    }

    [ClientRpc]
    public void RpcSetHost()
    {
        Host = true;
        base.SetDirtyBit(1);
    }

    protected virtual void InternalIncreaseHP(int value)
    {
        if (isDead)
            return;

        CurrentHP += value;

        if (CurrentHP > MaxHP)
            CurrentHP = MaxHP;
    }

    public virtual void IncreaseHP(int value)
    {
        if (isDead)
            return;

        CmdIncreaseHP(value);
    }

    [Command]
    public void CmdIncreaseHP(int value)
    {
        InternalIncreaseHP(value);
        base.SetDirtyBit(1);
    }

    public virtual void IncreaseUsableItem(int value = 1)
    {
        if (isDead)
            return;
        //
        CmdIncreaseUsableItem(value);
    }

    [Command]
    public void CmdIncreaseUsableItem(int value)
    {
        UsableItemCount += value;
        base.SetDirtyBit(1);
    }

}
