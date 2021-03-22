using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bullet : MonoBehaviour
{
    const float LiftTime = 15.0f;

    [SerializeField]
    Vector3 MoveDirection = Vector3.zero;

    [SerializeField]
    float Speed = 0.0f;

    bool NeedMove = false;      // 이동 플래그

    float FiredTime;

    bool Hited = false;         // 충돌 플래그

    [SerializeField]
    int Damage = 1;

    Actor Owner;

    public string FilePath
    {
        get;
        set;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ProcessDisappearCondition())
        {
            return;
        }

        UpdateMove();
    }

    void UpdateMove()
    {
        if (!NeedMove)
            return;


        Vector3 moveVector = MoveDirection.normalized * Speed * Time.deltaTime;
        moveVector += AdjustMove(moveVector);
        transform.position += moveVector;
    }

    public void Fire(Actor owner, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        Owner = owner;
        transform.position = firePosition;
        MoveDirection = direction;
        Speed = speed;
        Damage = damage;

        NeedMove = true;
        FiredTime = Time.time;
    }

    Vector3 AdjustMove(Vector3 moveVector)
    {
        // 레이캐스트 힛 초기화
        RaycastHit hitInfo;
        if (Physics.Linecast(transform.position, transform.position + moveVector, out hitInfo))
        {
            Actor actor = hitInfo.collider.GetComponentInParent<Actor>();
            if (actor && actor.IsDead)
                return moveVector;

            moveVector = hitInfo.point - transform.position;
            OnBulletCollision(hitInfo.collider);
        }

        return moveVector;
    }

    void OnBulletCollision(Collider collider)
    {
        if (Hited)
            return;

        // 총알끼리 부딪히는 경우를 피할 수 있도록 코드로 다시 한 번 안전장치 설정
        if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyBullet")
            || collider.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
        {
            return;
        }

        Actor actor = collider.GetComponentInParent<Actor>();
        if (actor && actor.IsDead || actor.gameObject.layer == Owner.gameObject.layer)
            return;

        actor.OnBulletHited(Owner, Damage, transform.position);

        Collider myCollider = GetComponentInChildren<Collider>();
        myCollider.enabled = false;

        Hited = true;
        NeedMove = false;

        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManger.GenerateEffect(EffectManager.BulletDisappearFxIndex, transform.position);
        go.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Disappear();

    }


    private void OnTriggerEnter(Collider other)
    {
        OnBulletCollision(other);
    }


    bool ProcessDisappearCondition()
    {
        // 총알이 일정 범위를 벗어나거나
        if (transform.position.x > 15.0f || transform.position.x < -15.0f
            || transform.position.y > 15.0f || transform.position.y < -15.0f)
        {
            Disappear();
            return true;
        }
        // 오브젝트의 생존 시간을 넘긴다면 Disapper한다.
        else if (Time.time - FiredTime > LiftTime)
        {
            Disappear();
            return true;
        }

        return false;
    }

    void Disappear()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Remove(this);
    }
}