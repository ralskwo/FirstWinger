using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Actor
{
    public enum State : int
    {
        None = -1,  // 사용 전
        Ready = 0,  // 준비 완료
        Appear,      // 등장
        Battle,     // 전투중
        Dead,       // 사망
        Disappear,  // 퇴장
    }

    [SerializeField]
    State CurrentState = State.None;
    const float MaxSpeed = 10.0f;       // 최대 속력
    const float MaxSpeedTime = 0.5f;    // 퇴장 시 자연스러운 가속을 위한 속도

    [SerializeField]
    Vector3 TargetPosition;

    [SerializeField]
    float CurrentSpeed;

    Vector3 CurrentVelocity;

    float MoveStartTime = 0.0f;

    [SerializeField]
    Transform FireTransform;                // 발사 위치

    // [SerializeField]
    // GameObject Bullet;                      // 총알 오브젝트

    [SerializeField]
    float BulletSpeed = 1;                  // 발사 속도

    float LastActionUpdateTime = 0.0f;

    [SerializeField]
    int FireRemainCount = 1;

    [SerializeField]
    int GamePoint = 10;

    public string FilePath
    {
        get;
        set;
    }

    Vector3 AppearPoint;            // 입장 시 도착 위치
    Vector3 DisappearPoint;         // 퇴장 시 목표 위치

    // Update is called once per frame
    protected override void UpdateActor()
    {

        switch (CurrentState)
        {
            case State.None:
                break;
            case State.Ready:
                UpdateReady();
                break;
            case State.Dead:
                break;
            case State.Appear:
            case State.Disappear:
                UpdateSpeed();
                UpdateMove();
                break;

            case State.Battle:
                UpdateBattle();
                break;
        }

        if (CurrentState == State.Appear || CurrentState == State.Disappear)
        {


        }

        if (CurrentState == State.Battle)
        {

        }

    }

    void UpdateSpeed()
    {
        // Mathf.Lerp : 두 값 사이의 임의의 점
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, MaxSpeed, (Time.time - MoveStartTime) / MaxSpeedTime);
    }

    void UpdateMove()
    {
        // 거리(두 점간의 거리) = 타겟의 위치, 내 위치
        float distance = Vector3.Distance(TargetPosition, transform.position);
        if (distance == 0)
        {
            Arrived();
            return;
        }

        // 목표점(TargetPosition)에서 내 위치를 빼주면 목표점으로 가는 벡터가 나옴
        // normalized를 사용하여 단위벡터로 변경한 후 현재 속도를 곱함
        // 현재의 이동량이 나온다
        CurrentVelocity = (TargetPosition - transform.position).normalized * CurrentSpeed;

        // 속도 = 시간/거리 이므로 시간 = 거리/속도
        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref CurrentVelocity, distance / CurrentSpeed, MaxSpeed);
    }

    void Arrived()
    {
        CurrentSpeed = 0.0f;

        if (CurrentState == State.Appear)
        {
            CurrentState = State.Battle;
            LastActionUpdateTime = Time.time;
        }
        else // if (CurrentState == State.Disappear)
        {
            CurrentState = State.None;
        }
    }

    public void Reset(SquadronMemberStuct data)
    {
        EnemyStruct enemyStruct = SystemManager.Instance.EnemyTable.GetEnemy(data.EnemyID);

        CurrentHP = MaxHP = enemyStruct.MaxHP;             // CurrentHP까지 다시 입력
        Damage = enemyStruct.Damage;                       // 총알 데미지
        crashDamage = enemyStruct.CrashDamage;             // 충돌 데미지
        BulletSpeed = enemyStruct.BulletSpeed;             // 총알 속도
        FireRemainCount = enemyStruct.FireRemainCount;     // 발사할 총알 개수
        GamePoint = enemyStruct.GamePoint;                 // 파괴 시 얻을 점수

        AppearPoint = new Vector3(data.AppearPointX, data.AppearPointY, 0);                 // 입장 시 도착 위치
        DisappearPoint = new Vector3(data.DisappearPointX, data.DisappearPointY, 0);        // 퇴장 시 목표 위치

        CurrentState = State.Ready;
        LastActionUpdateTime = Time.time;
    }


    public void Appear(Vector3 targetPos)
    {
        TargetPosition = targetPos;
        CurrentSpeed = MaxSpeed;

        CurrentState = State.Appear;
        MoveStartTime = Time.time;
    }
    void Disappear(Vector3 targetPos)
    {
        TargetPosition = targetPos;
        CurrentSpeed = 0;                   // 사라질때는 0부터 속도 증가

        CurrentState = State.Disappear;
        MoveStartTime = Time.time;
    }

    void UpdateReady()
    {
        if (Time.time - LastActionUpdateTime > 1.0f)
        {
            Appear(AppearPoint);
        }
    }

    void UpdateBattle()
    {
        if (Time.time - LastActionUpdateTime > 1.0f)
        {
            if (FireRemainCount > 0)
            {
                Fire();
                FireRemainCount--;
            }
            else
            {
                Disappear(DisappearPoint);
            }


            LastActionUpdateTime = Time.time;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("other = " + other);

        Player player = other.GetComponentInParent<Player>();
        if (player)
        {
            if (!player.IsDead)
            {
                BoxCollider box = ((BoxCollider)other);
                Vector3 crashPos = player.transform.position + box.center;
                crashPos.x += box.size.x * 0.5f;

                player.OnCrash(this, CrashDamage, crashPos);
            }
        }
    }

    public override void OnCrash(Actor attacker, int damage, Vector3 crashPos)
    {
        base.OnCrash(attacker, damage, crashPos);
    }

    public void Fire()
    {
        // GameObject go = Instantiate(Bullet);

        // Bullet bullet = go.GetComponent<Bullet>();
        Bullet bullet = SystemManager.Instance.BulletManager.Generate(BulletManager.EnemayBulletIndex);
        bullet.Fire(OwnerSide.Enemy, FireTransform.position, -FireTransform.right, BulletSpeed, Damage);
    }

    protected override void OnDead(Actor killer)
    {
        base.OnDead(killer);

        SystemManager.Instance.GamePointAccumulator.Accumulate(GamePoint);
        SystemManager.Instance.EnemyManager.RemoveEnemy(this);

        CurrentState = State.Dead;
        // Destroy(gameObject);
    }

    protected override void DecreasedHP(Actor attacker, int value, Vector3 damagePos)
    {
        base.DecreasedHP(attacker, value, damagePos);

        Vector3 damagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.DamageManager.Generate(DamageManager.EnemyDamageIndex, damagePoint, value, Color.magenta);
    }

}