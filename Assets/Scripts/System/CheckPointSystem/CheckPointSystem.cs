using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSystem : SingletonBaseWithMono<CheckPointSystem>
{
    //一个存储检查点数据的字典，键为检查点名称，值为对应的检查点数据
    public Dictionary<string, CheckPointData> checkPoints = new Dictionary<string, CheckPointData>();
    public string currentCheckPointName;//当前检查点的名称，可以根据需要进行设置和使用

    public void SaveCheckPoint(string CheckPointName)
    {
        currentCheckPointName = CheckPointName;
        // 获取玩家当前的位置和状态等信息，创建一个新的CheckPointData对象，并将其压入栈中
        CheckPointData checkPointData = new CheckPointData();
        checkPointData.playerPosition = PlayerView.Instance.transform.position;
        checkPointData.scanPoint = CollectionSystem.Instance.scanPoint;
        checkPointData.energyPoint = CollectionSystem.Instance.energyPoint;

        //checkPointData.SetCheckPointData();
        checkPoints.Add(currentCheckPointName,checkPointData);
    }
    public void LoadCheckPoint()
    {
        if (checkPoints.ContainsKey(currentCheckPointName))
        {
            CheckPointData checkPointData = checkPoints[currentCheckPointName];
            PlayerView.Instance.Reset(checkPointData);
            CollectionSystem.Instance.Reset(checkPointData);
        }
        else
        {
            Debug.LogWarning("没有可用的检查点数据！");
        }
    }
}
