using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageManager : MonoBehaviour
{

    public const int EnemyDamageIndex = 0;
    public const int PlayerDamageIndex = 0;

    // 캔버스의 TrasForm을 가져올 변수
    [SerializeField]
    Transform canvasTransform;

    public Transform CanvasTransform
    {
        get
        {
            return canvasTransform;
        }
    }

    [SerializeField]
    Canvas canvas;

    // 캐싱을 위한 배열
    [SerializeField]
    PrefabCacheData[] Files;

    // File 로드를 위한 FileCache를 딕셔너리로 생성
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
            }

            FileCache.Add(resourcePath, go);
        }

        return go;
    }

    public void Prepare()
    {
        for (int i = 0; i < Files.Length; i++)
        {
            GameObject go = Load(Files[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.GenerateCache(Files[i].filePath, go, Files[i].cacheCount, canvasTransform);
        }
    }

    public GameObject Generate(int index, Vector3 position, int damageValue, Color textColor)
    {
        if (index < 0 || index >= Files.Length)
        {
            Debug.LogError("Generate error! out of range! index = " + index);
            return null;
        }

        string filePath = Files[index].filePath;
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.Archive(filePath, Camera.main.WorldToScreenPoint(position));
        // go.transform.position = Camera.main.WorldToScreenPoint(position);

        UIDamage damage = go.GetComponent<UIDamage>();
        damage.FilePath = filePath;
        damage.ShowDamage(damageValue, textColor);

        return go;
    }

    public bool Remove(UIDamage damage)
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.Restore(damage.FilePath, damage.gameObject);
        return true;
    }
}
