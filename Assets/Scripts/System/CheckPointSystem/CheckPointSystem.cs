using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSystem : SingletonBaseWithMono<CheckPointSystem>
{
    //一个存储检查点数据的字典，键为检查点名称，值为对应的检查点数据
    public Dictionary<int,CheckPointData> CheckPoints = new Dictionary<int, CheckPointData>();
    public int currentStageIndex;//当前检查点的名称，可以根据需要进行设置和使用
    private bool _isLoadingCheckPoint = false;

    void Start()
    {
        EventCenter.Instance.AddListener("玩家死亡", LoadCheckPoint);
    }   

    public void SaveCheckPoint(CheckPointView checkPointData)
    {
        currentStageIndex = checkPointData.checkPointID;//每次保存检查点时，获得存档点的ID
        CheckPoints[checkPointData.checkPointID] = checkPointData.checkPointData;//将检查点数据添加到字典中，键为检查点ID，值为检查点数据
        CollectionSystem.Instance.stageCacheScanPoints.Clear();//清空当前关卡的扫描点缓存列表
    }
    public void LoadCheckPoint()
    {
        if (PlayerView.Instance.invincible)
        {
            return;
        }

        if (_isLoadingCheckPoint)
        {
            return;
        }

        if (CheckPoints.ContainsKey(currentStageIndex))
        {
            _isLoadingCheckPoint = true;
            try
            {
                CheckPointData checkPointData = CheckPoints[currentStageIndex];
                PlayerView.Instance.ResetPlayer(checkPointData);
                CollectionSystem.Instance.ResetCollection(checkPointData);

                // 重置遮罩
                CoverSystem.Instance.ResetCover(checkPointData);
            }
            finally
            {
                _isLoadingCheckPoint = false;
            }
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
