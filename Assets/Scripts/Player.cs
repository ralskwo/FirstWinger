using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    [SerializeField]
    Vector3 MoveVector = Vector3.zero;      // 플레이어의 움직임

    [SerializeField]
    float Speed;                            // 플레이어 속도

    [SerializeField]
    BoxCollider boxcollider;                // 플레이어 collider

    [SerializeField]
    Transform MainBGQuadTransform;

    [SerializeField]
    Transform FireTransform;                // 발사 위치

    // [SerializeField]
    // GameObject Bullet;                      // 총알 오브젝트

    [SerializeField]
    float BulletSpeed = 1;                  // 발사 속도



    protected override void Initialize()
    {
        base.Initialize();
        PlayerStatePanel playerStatePanel = PanelManager.GetPanel(typeof(PlayerStatePanel)) as PlayerStatePanel;
        playerStatePanel.SetHP(CurrentHP, MaxHP);
    }


    // Update is called once per frame 
    protected override void UpdateActor()
    {
        UpdateMove();
    }

    void UpdateMove()
    {
        // sqrMagnitude: 두 점간의 거리의 제곱에 루트를 한 값. 두 점 간의 거리의 차이를 2차원 함수값으로 계산해준다.
        if (MoveVector.sqrMagnitude == 0)
            return;

        // 화면 바깥으로 나가지 않게 하기 위한 부분
        MoveVector = AdjustMoveVector(MoveVector);


        transform.position += MoveVector;
    }


    public void ProcessInput(Vector3 moveDirection)
    {
        MoveVector = moveDirection * Speed * Time.deltaTime;
    }

    Vector3 AdjustMoveVector(Vector3 moveVector)
    {
        Vector3 result = Vector3.zero;

        // 현재 플레이어(boxcollider)의 위치
        result = boxcollider.transform.position + boxcollider.center + moveVector;

        // 맵의 왼쪽을 넘어가지 않는 코드
        if (result.x - boxcollider.size.x * 0.5f < -MainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;

        // 맵의 오른쪽을 넘어가지 않는 코드
        if (result.x + boxcollider.size.x * 0.5f > MainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;

        // 맵의 아래쪽을 넘어가지 않는 코드
        if (result.y - boxcollider.size.y * 0.5f < -MainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;

        // // 맵의 위쪽을 넘어가지 않는 코드
        if (result.y + boxcollider.size.y * 0.5f > MainBGQuadTransform.localScale.y * 0.5f)
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
