﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    // effect를 줄여서 Fx라고 하기도 한다.
    public const int BulletDisappearFxIndex = 0;
    public const int ActorDeadFxIndex = 1;
    public const int BombExplodeFxIndex = 2;


    [SerializeField]
    PrefabCacheData[] effectFiles;

    Dictionary<string, GameObject> FileCache = new Dictionary<string, GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        Prepare();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject GenerateEffect(int index, Vector3 position)
    {
        if (index < 0 || index >= effectFiles.Length)
        {
            Debug.LogError("GenerateEffect error! out of range! index = " + index);
            return null;
        }


        // // Quaternion.identity : 회전하지 않은
        // GameObject go = Instantiate<GameObject>(effectPreFabs[index], position, Quaternion.identity);

        string filePath = effectFiles[index].filePath;
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.Archive(filePath, position);
        // go.transform.position = position;

        AutoCachableEffect effect = go.GetComponent<AutoCachableEffect>();
        effect.FilePath = filePath;

        return go;
    }

    public GameObject Load(string resourcePath)
    {
        GameObject go = null;
        if (FileCache.ContainsKey(resourcePath))
        {
            go = FileCache[resourcePath];
        }
        else
        {
            go = Resources.Load<GameObject>(resourcePath);
            if (!go)
            {
                Debug.LogError("Load error! path = " + resourcePath);
                return null;
            }

            FileCache.Add(resourcePath, go);
        }
        return go;
    }

    public void Prepare()
    {
        for (int i = 0; i < effectFiles.Length; i++)
        {
            GameObject go = Load(effectFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.GenerateCache(effectFiles[i].filePath, go, effectFiles[i].cacheCount, this.transform);
        }
    }

    public bool RemoveEffect(AutoCachableEffect effect)
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.Restore(effect.FilePath, effect.gameObject);
        return true;
    }
}
