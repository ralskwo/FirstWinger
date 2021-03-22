using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[System.Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

public struct SquadronScheduleDataSturct
{
    public int index;
    public float GenerateTime;
    public int SquadronID;
}

public class SquadronScheduleTable : TableLoader<SquadronScheduleDataSturct>
{
    List<SquadronScheduleDataSturct> tableDatas = new List<SquadronScheduleDataSturct>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void AddData(SquadronScheduleDataSturct data)
    {
        tableDatas.Add(data);
    }

    public SquadronScheduleDataSturct GetScheduleData(int index)
    {
        if (index < 0 || index >= tableDatas.Count)
        {
            Debug.LogError("SquadronScheduleDataStruct Error! index = " + index);
            return default(SquadronScheduleDataSturct);
        }

        return tableDatas[index];
    }
}
