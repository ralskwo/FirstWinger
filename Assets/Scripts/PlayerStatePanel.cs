using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatePanel : BasePanel
{
    [SerializeField]
    Text scoreVaule;

    [SerializeField]
    Gage HPGage;

    public void SetScore(int value)
    {
        Debug.Log("SetScore value = " + value);

        scoreVaule.text = value.ToString();
    }

    public void SetHP(float currentValue, float maxValue)
    {
        HPGage.SetHP(currentValue, maxValue);
    }

}
