using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 데미지를 띄우는 함수
public class UIDamage : MonoBehaviour
{
    // 상태 열거. 없음, 크기 커짐, 나타남, 사라짐
    enum DamageState : int
    {
        None = 0,
        SizeUp,
        Display,
        FadeOut,
    }

    // 상태 변수
    [SerializeField]
    DamageState damageState = DamageState.None;

    // 사이즈가 커질 때 걸리는 시간
    const float SizeUpDuration = 0.1f;

    // 보여지는 시간
    const float DisplayDuration = 0.5f;

    // 사라지는 시간
    const float FadeOutDuration = 0.2f;


    // 텍스트 Prefab을 받아올 변수
    [SerializeField]
    Text damageText;

    // 현재 위치
    Vector3 CurrentVelocity;

    // 나타나고 사라지기 시작하는 각 시작 시간
    float DisplayStartTime;
    float FadeOutStartTime;

    // 캐싱을 위한 FilePath
    public string FilePath
    {
        get;
        set;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateDamage();
    }

    // // GUI에서 테스트해보기 위한 버튼
    // private void OnGUI()
    // {
    //     if (GUILayout.Button("Show"))
    //     {
    //         ShowDamage(9999);
    //     }
    // }

    // 외부에서 데미지가 보이게 하기 위한 함수
    public void ShowDamage(int damage, Color textColor)
    {
        // int로 받아온 damage를 string으로 변환하여 넘겨준 뒤
        damageText.text = damage.ToString();
        // damage의 색상을 textColor로 바꾸고
        damageText.color = textColor;
        // 리셋한 후
        Reset();
        // 나타나기 시작하므로 상태를 SizeUp으로 변경
        damageState = DamageState.SizeUp;
    }

    // 리셋은
    void Reset()
    {
        // 우선 크기를 0으로 만든 후
        transform.localScale = Vector3.zero;
        // 색상을 정의한 후
        Color newColor = damageText.color;
        // 알파값(a)을 다 보이는 상태인 1.0f로 설정
        newColor.a = 1.0f;
        damageText.color = newColor;
    }

    void UpdateDamage()
    {
        // 데미지의 상태가 '없음'이면 return
        if (damageState == DamageState.None)
            return;

        // 현재 데미지의 상태가
        switch (damageState)
        {
            // SizeUp 이라면
            case DamageState.SizeUp:

                // Vector3.SmoothDamp(현재 크기, 목표 크기, 나타날 위치, 커지는 단위)
                // 데미지(transform)의 크기(localScale)을 부드럽게 키우기 위해(SmoothDamp)
                // SmoothDamp(데미지의 크기(transform.localScale)가 Vector3.one(1. 100%를 의미)가 될 때까지 해당 위치(ref CurrentVelocity)에
                //              설정된 시간(SizeUpDuration)만큼 증가한다)
                transform.localScale = Vector3.SmoothDamp(transform.localScale, Vector3.one, ref CurrentVelocity, SizeUpDuration);

                // 위의 구문을 통해 transform.localScale이 Vector3.one에 도달했다면
                if (transform.localScale == Vector3.one)
                {
                    // 상태를 Display로 바꾸고
                    damageState = DamageState.Display;
                    // 현재 시각을 Display가 시작하는 시간으로 설정한다.
                    DisplayStartTime = Time.time;
                }
                break;

            // Display 라면
            case DamageState.Display:

                // Time.time - DisplayStartTime : 실제 시작시간. deltaTime이다. 이게 미리 설정해둔 나타나는 시간보다 커진다면
                if (Time.time - DisplayStartTime > DisplayDuration)
                {
                    // 상태를 FadeOut으로 바꾸고
                    damageState = DamageState.FadeOut;
                    // 현재 시각을 FadeOut이 시작하는 시간으로 설정한다.
                    FadeOutStartTime = Time.time;
                }

                break;

            // FadeOut 이라면
            case DamageState.FadeOut:
                // 색상을 설정할 변수 생성
                Color newColor = damageText.color;

                // Mathf.Lerp(시작 값, 목표 값, 더해지는 단위)
                // 해당 색생의 alpha 값을 1부터 0까지 줄이는데, 이 시간을 시작 시점의 deltaTime에서 미리 설정해둔 사라지는 시간으로 나눈다.
                newColor.a = Mathf.Lerp(1, 0, (Time.time - FadeOutStartTime) / FadeOutDuration);
                damageText.color = newColor;

                // alpha값이 0이라면 (사라졌다면)
                if (newColor.a == 0)
                {
                    // 상태를 없음으로 바꾸고
                    damageState = DamageState.None;
                    // SystemManager를 통해 이 DamageManger를 삭제한다.
                    SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.Remove(this);
                }

                break;



        }


    }
}
