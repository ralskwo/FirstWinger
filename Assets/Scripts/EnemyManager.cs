    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    EnemyFactory enemyFactory;

    List<Enemy> enemies = new List<Enemy>();

    public List<Enemy> Enemies
    {
        get
        {
            return enemies;
        }
    }

    [SerializeField]
    PrefabCacheData[] enemyFiles;


    // Start is called before the first frame update
    void Start()
    {
        Prepare();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool GenerateEnemy(SquadronMemberStuct data)
    {
        string FilePath = SystemManager.Instance.EnemyTable.GetEnemy(data.EnemyID).FilePath;
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyCacheSystem.Archive(FilePath);

        go.transform.position = new Vector3(data.GeneratePointX, data.GeneratePointY, 0);

        Enemy enemy = go.GetComponent<Enemy>();
        enemy.FilePath = FilePath;
        enemy.Reset(data);

        enemies.Add(enemy);
        return true;
    }

    public bool RemoveEnemy(Enemy enemy)
    {
        if (!enemies.Contains(enemy))
        {
            Debug.Log("No exist Enemy");
            return false;
        }

        enemies.Remove(enemy);
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyCacheSystem.Restore(enemy.FilePath, enemy.gameObject);

        return true;
    }

    public void Prepare()
    {
        for (int i = 0; i < enemyFiles.Length; i++)
        {
            GameObject go = enemyFactory.Load(enemyFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.GenerateCache(enemyFiles[i].filePath, go, enemyFiles[i].cacheCount);
        }
    }
}
