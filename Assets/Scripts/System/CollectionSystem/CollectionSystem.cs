using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectionSystem : SingletonBaseWithMono<CollectionSystem>
{
    // 这里可以添加一些收集系统相关的属性和方法
    public int stageScanPoint=0;
    public List<ScanPointView> stageCacheScanPoints;//当前关卡的临时收集点数，玩家在当前关卡中获得的收集点数，在玩家死亡后会刷新并且丢失
    public int permanentCollectionPoints = 0;//永久收集点数，玩家在游戏过程中获得的总收集点数，即使在玩家死亡后也不会丢失

    public void ResetCollection(CheckPointData checkPointData)
    {
        stageScanPoint = checkPointData.scanPoint;
        GamePanel.Instance.UpdateCollectionNum(permanentCollectionPoints);
        //更新UI界面上能量点的显示、
        //TODO: 可以在这里添加一些其他的重置逻辑，例如重置临时收集点数等
        foreach (ScanPointView scanPoint in stageCacheScanPoints)
        {
            // 可以在这里添加对每个临时收集点的重置逻辑
            scanPoint.ResetScanPoint();
        }
        stageCacheScanPoints.Clear();//清空当前关卡的临时收集点数列表
    }

    public void CollectItem()
    {
        permanentCollectionPoints++;
        GamePanel.Instance.UpdateCollectionNum(permanentCollectionPoints);
    }
    public void CollectScanPoint(ScanPointView scanPoint)
    {
        stageScanPoint++;
        stageCacheScanPoints.Add(scanPoint);//将当前关卡的临时收集点数添加到列表中

    }
}
