using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [SerializeField]
    protected int MaxHP = 100;        // 최대 체력

    [SerializeField]
    protected int CurrentHP;           // 현재 체력

    [SerializeField]
    protected int Damage = 1;           // 데미지

    [SerializeField]
    protected int crashDamage = 100;    // 충돌 시 데미지

    [SerializeField]
    bool isDead = false;

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

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // 초기 설정
    protected virtual void Initialize()
    {
        CurrentHP = MaxHP;
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
    public virtual void OnBulletHited(Actor attacker, int damage, Vector3 hitPos)
    {
        Debug.Log("OnBulletHited damage = " + damage);
        DecreasedHP(attacker, damage, hitPos);
    }

    // 비행기끼리 부딛혔을 때 호출
    public virtual void OnCrash(Actor attacker, int damage, Vector3 crashPos)
    {
        Debug.Log("OnCrash attacker = " + attacker.name + ", damage = " + damage);
        DecreasedHP(attacker, damage, crashPos);
    }

    protected virtual void DecreasedHP(Actor attacker, int value, Vector3 damagePos)
    {
        if (isDead)
            return;

        CurrentHP -= value;

        if (CurrentHP < 0)
            CurrentHP = 0;

        if (CurrentHP == 0)
        {
            OnDead(attacker);
        }
    }

    protected virtual void OnDead(Actor Killer)
    {
        Debug.Log(name + " OnDead");
        isDead = true;

        SystemManager.Instance.EffectManager.GenerateEffect(EffectManager.ActorDeadFxIndex, transform.position);
    }
}
