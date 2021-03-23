using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class Player : Actor
{
    [SerializeField]
    [SyncVar]
    Vector3 MoveVector = Vector3.zero;      // 플레이어의 움직임

    [SerializeField]
    NetworkIdentity NetworkIdentity = null;

    [SerializeField]
    float Speed;                            // 플레이어 속도

    [SerializeField]
    BoxCollider boxcollider;                // 플레이어 collider

    [SerializeField]
    Transform FireTransform;                // 발사 위치

    [SerializeField]
    float BulletSpeed = 1;                  // 발사 속도

    InputController inputController = new InputController();

    protected override void Initialize()
    {
        base.Initialize();
        PlayerStatePanel playerStatePanel = PanelManager.GetPanel(typeof(PlayerStatePanel)) as PlayerStatePanel;
        playerStatePanel.SetHP(CurrentHP, MaxHP);

        if (isLocalPlayer)
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().Hero = this;
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
        MoveVector = moveDirection * Speed * Time.deltaTime;
    }

    Vector3 AdjustMoveVector(Vector3 moveVector)
    {
        Transform mainBGQuadTransform = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().MainBGQuadTransform;
        Vector3 result = Vector3.zero;

        // 현재 플레이어(boxcollider)의 위치
        result = boxcollider.transform.position + boxcollider.center + moveVector;

        // 맵의 왼쪽을 넘어가지 않는 코드
        if (result.x - boxcollider.size.x * 0.5f < -mainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;

        // 맵의 오른쪽을 넘어가지 않는 코드
        if (result.x + boxcollider.size.x * 0.5f > mainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;

        // 맵의 아래쪽을 넘어가지 않는 코드
        if (result.y - boxcollider.size.y * 0.5f < -mainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;

        // // 맵의 위쪽을 넘어가지 않는 코드
        if (result.y + boxcollider.size.y * 0.5f > mainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;

        return moveVector;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("other = " + other);

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy)
        {
            if (!enemy.IsDead)
            {
                BoxCollider box = ((BoxCollider)other);
                Vector3 crashPos = enemy.transform.position + box.center;
                crashPos.x += box.size.x * 0.5f;

                enemy.OnCrash(this, CrashDamage, crashPos);
            }
        }
    }

    public override void OnCrash(Actor attacker, int damage, Vector3 crashPos)
    {
        base.OnCrash(attacker, damage, crashPos);
    }

    //발사 함수
    public void Fire()
    {
        // GameObject go = Instantiate(Bullet);

        // Bullet bullet = go.GetComponent<Bullet>();
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBulletIndex);
        bullet.Fire(this, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
    }

    protected override void DecreasedHP(Actor attacker, int value, Vector3 damagePos)
    {
        base.DecreasedHP(attacker, value, damagePos);
        PlayerStatePanel playerStatePanel = PanelManager.GetPanel(typeof(PlayerStatePanel)) as PlayerStatePanel;
        playerStatePanel.SetHP(CurrentHP, MaxHP);

        Vector3 damagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.Generate(DamageManager.PlayerDamageIndex, damagePoint, value, Color.red);
    }


    protected override void OnDead(Actor Killer)
    {
        base.OnDead(Killer);
        gameObject.SetActive(false);
    }

}
