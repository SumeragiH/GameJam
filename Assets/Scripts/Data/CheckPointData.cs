using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointData
{
    public Vector2 playerPosition;//玩家的位置
    public int scanPoint;
    public void SetCheckPointData(Vector2 playerPosition, int scanPoint)
    {
        this.playerPosition = playerPosition;
        this.scanPoint = scanPoint;
    }

    public CheckPointData()
    {
        this.playerPosition = Vector2.zero;
        this.scanPoint = 0;
    }

}
