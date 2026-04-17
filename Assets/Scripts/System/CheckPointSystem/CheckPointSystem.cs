using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSystem : SingletonBaseWithMono<CheckPointSystem>
{
    //存储检查点的栈，每次保存检查点时，将数据压入栈中，每次加载检查点时，从栈顶弹出数据
    public Stack<CheckPointData> checkPoints = new Stack<CheckPointData>();

    public void SaveCheckPoint()
    {
        // 获取玩家当前的位置和状态等信息，创建一个新的CheckPointData对象，并将其压入栈中
        CheckPointData checkPointData = new CheckPointData();

        //checkPointData.SetCheckPointData();
        checkPoints.Push(checkPointData);
    }
    public CheckPointData LoadCheckPoint()
    {
        return checkPoints.Pop();
    }
}
