using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionSystem : SingletonBaseWithoutMono<CollectionSystem>
{
    // 这里可以添加一些收集系统相关的属性和方法
    public int scanPoint;
    public int energyPoint;

    /// <summary>
    /// 一个重置方法，用于在玩家死亡或重新开始时重置收集系统的数据
    /// </summary>
    /// <param name="checkPointData"></param>
    public void Reset(CheckPointData checkPointData)
    {
        scanPoint = checkPointData.scanPoint;
        energyPoint = checkPointData.energyPoint;
    }
}