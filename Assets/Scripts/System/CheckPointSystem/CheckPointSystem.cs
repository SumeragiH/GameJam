using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSystem : SingletonBaseWithMono<CheckPointSystem>
{
    //一个存储检查点数据的字典，键为检查点名称，值为对应的检查点数据
    public Dictionary<int,CheckPointData> CheckPoints = new Dictionary<int, CheckPointData>();
    public int currentStageIndex;//当前检查点的名称，可以根据需要进行设置和使用

    public void SaveCheckPoint(CheckPointView checkPointData)
    {
        currentStageIndex = checkPointData.checkPointID;//每次保存检查点时，获得存档点的ID
        CheckPoints.Add(checkPointData.checkPointID, checkPointData.checkPointData);//将检查点数据添加到字典中，键为检查点ID，值为检查点数据
        CollectionSystem.Instance.stageCacheScanPoints.Clear();//清空当前关卡的扫描点缓存列表
    }
    public void LoadCheckPoint()
    {
        if (CheckPoints.ContainsKey(currentStageIndex))
        {
            CheckPointData checkPointData = CheckPoints[currentStageIndex];
            PlayerView.Instance.Reset(checkPointData);
            CollectionSystem.Instance.Reset(checkPointData);
        }
        else
        {
            Debug.LogWarning("没有可用的检查点数据！");
        }
    }

    public void ClearCheckPoints()
    {
        CheckPoints.Clear();
        currentStageIndex = 0;
    }
}
