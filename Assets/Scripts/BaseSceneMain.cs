using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSceneMain : MonoBehaviour
{
    private void Awake()
    {
        OnAwake();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateScene();
    }

    void OnDestroy()
    {
        Terminate();
    }

    protected virtual void OnAwake()
    {

    }

    protected virtual void OnStart()
    {

    }

    /// <summary>
    /// 외부에서 초기화 호출
    /// </summary>
    public virtual void Initialize()
    {

    }

    public virtual void UpdateScene()
    {

    }

    public virtual void Terminate()
    {

    }
}
