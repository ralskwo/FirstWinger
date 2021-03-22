using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum OwnerSide : int
{
    Player = 0,
    Enemy
}

public class Bullet : MonoBehaviour
{
    const float LiftTime = 15.0f;
    OwnerSide ownerSide = OwnerSide.Player;

    [SerializeField]
    Vector3 MoveDirection = Vector3.zero;

    [SerializeField]
    float Speed = 0.0f;

    bool NeedMove = false;

    float FiredTime;

    bool Hited = false;

    [SerializeField]
    int Damage = 1;

    Actor Owner;

    public string FilePath
    {
        get;
        set;
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

    public void Fire(OwnerSide FireOwner, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        ownerSide = FireOwner;
        transform.position = firePosition;
        MoveDirection = direction;
        Speed = speed;
        Damage = damage;

        NeedMove = true;
        FiredTime = Time.time;
    }

    Vector3 AdjustMove(Vector3 moveVector)
    {
        RaycastHit hitInfo;
        if (Physics.Linecast(transform.position, transform.position + moveVector, out hitInfo))
        {

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

        GameObject go = SystemManager.Instance.EffectManager.GenerateEffect(EffectManager.BulletDisappearFxIndex, transform.position);
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
        Destroy(gameObject);
    }
}