using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabCacheData
{
    public string filePath;         // 파일의 위치
    public int cacheCount;          // 캐시의 개수
}

public class PrefabCacheSystem
{
    Dictionary<string, Queue<GameObject>> Caches = new Dictionary<string, Queue<GameObject>>();

    public void GenerateCache(string filepath, GameObject gameObject, int cacheCount, Transform parentTransform = null)
    {
        if (Caches.ContainsKey(filepath))
        {
            Debug.LogWarning("Already cache generated! filePath = " + filepath);
            return;
        }

        else
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < cacheCount; i++)
            {
                GameObject go = Object.Instantiate<GameObject>(gameObject, parentTransform);
                go.SetActive(false);
                // 얘는 UI오브젝트이기 때문에 생성과 동시에 캔버스 밑에 붙어있어야 한다. 그렇지 않으면 크기가 비정상적으로 출력됨
                queue.Enqueue(go);
            }

            Caches.Add(filepath, queue);
        }
    }

    public GameObject Archive(string filePath)
    {
        if (!Caches.ContainsKey(filePath))
        {
            Debug.LogError("Archive Error! no Cache generated! filePath = " + filePath);
            return null;
        }

        if (Caches[filePath].Count == 0)
        {
            Debug.LogWarning("Archive problem! not enough Count");
            return null;
        }

        GameObject go = Caches[filePath].Dequeue();
        go.SetActive(true);

        return go;
    }

    public bool Restore(string filePath, GameObject gameObject)
    {
        if (!Caches.ContainsKey(filePath))
        {
            Debug.LogError("Restore Error! no Cache generated! filePath = " + filePath);
            return false;
        }

        gameObject.SetActive(false);

        Caches[filePath].Enqueue(gameObject);
        return true;
    }
}
